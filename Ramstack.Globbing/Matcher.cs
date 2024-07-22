using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ramstack.Globbing;

/// <summary>
/// Provides functionality for shell-style glob matching using the glob pattern syntax.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item>
///     <description>
///       Supported meta-characters include <c>'*'</c>, <c>'?'</c>, <c>'\'</c> and <c>'['</c>, <c>']'</c>.
///       And inside character classes <c>'-'</c>, <c>'!'</c> and <c>']'</c>.
///     </description>
///   </item>
///   <item>
///     <description>
///       The <c>'.'</c> and <c>'..'</c> symbols do not have any special treatment and are processed
///       as regular characters for matching.
///     </description>
///   </item>
///   <item>
///     <description>
///       Character classes can be negated by prefixing them with  <c>'!'</c>, such as <c>[!0-9]</c>,
///       which matches all characters except digits.
///     </description>
///   </item>
///   <item>
///     <description>
///       Brace patterns are supported, including nested brace pattern:
///       <c>{file,dir,name}</c>, <c>{file-1.{c,cpp},file-2.{cs,f}}</c>
///     </description>
///   </item>
///   <item>
///     <description>
///       An empty pattern in brace expansion  <c>{}</c> is allowed, as well as variations
///       like <c>{.cs,}</c>, <c>{name,,file}</c>, or  <c>{,.cs}</c>.
///     </description>
///   </item>
///   <item>
///     <description>Leading and trailing separators are ignored.</description>
///   </item>
///   <item>
///     <description>Consecutive separators are counted as one.</description>
///   </item>
///   <item>
///     <description>
///       The <c>'**'</c> sequence in the glob pattern can be used to match zero or more directories and subdirectories.
///       It can be used at the beginning, middle, or end of a pattern, for example,
///       <c>"**/file.txt"</c>, <c>"dir/**/*.txt"</c>, <c>"dir/**"</c>.
///     </description>
///   </item>
/// </list>
/// </remarks>
public static unsafe class Matcher
{
    /// <summary>
    /// Represents a marker structure that is used to indicate that the '\' character
    /// should be treated as an escape character in the context of glob pattern processing.
    /// This enables the JIT compiler to generate optimized versions of functions
    /// for different options, enhancing performance.
    /// </summary>
    private readonly struct Unix;

    /// <summary>
    /// Represents a marker structure that is used to indicate that the escape character '\'
    /// should not be treated as an escape character, but as a path separator instead,
    /// in the context of glob pattern processing.
    /// This enables the JIT compiler to generate optimized versions of functions
    /// for different options, enhancing performance.
    /// </summary>
    private readonly struct Windows;

    /// <summary>
    /// Determines whether the glob pattern matches the specified path, using the specified matching options.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     <description>Leading and trailing path separators are ignored.</description>
    ///   </item>
    ///   <item>
    ///     <description>Consecutive path separators are counted as one.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The '**' sequence in the <paramref name="pattern"/> can be used to match zero or more directories and subdirectories.
    ///       It can be used at the beginning, middle, or end of a pattern, for example, "**/file.txt", "dir/**/*.txt", "dir/**".
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <param name="path">The path to test for a match.</param>
    /// <param name="pattern">The glob pattern to match against.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// <see langword="true" /> if the pattern matches the path; otherwise, <see langword="false" />.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMatch(string path, string pattern, MatchFlags flags = MatchFlags.Auto)
    {
        _ = path.Length;
        _ = pattern.Length;

        return IsMatch(path.AsSpan(), pattern.AsSpan(), flags);
    }

    /// <summary>
    /// Determines whether the glob pattern matches the specified path, using the specified matching options.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     <description>Leading and trailing path separators are ignored.</description>
    ///   </item>
    ///   <item>
    ///     <description>Consecutive path separators are counted as one.</description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The '**' sequence in the <paramref name="pattern"/> can be used to match zero or more directories and subdirectories.
    ///       It can be used at the beginning, middle, or end of a pattern, for example, "**/file.txt", "dir/**/*.txt", "dir/**".
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <param name="path">The path to test for a match.</param>
    /// <param name="pattern">The glob pattern to match against.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// <see langword="true" /> if the pattern matches the path; otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsMatch(ReadOnlySpan<char> path, ReadOnlySpan<char> pattern, MatchFlags flags = MatchFlags.Auto)
    {
        return IsMatchImpl(
            ref MemoryMarshal.GetReference(path),
            path.Length,
            ref MemoryMarshal.GetReference(pattern),
            pattern.Length,
            flags);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool IsMatchImpl(ref char rv, int vlen, ref char rp, int plen, MatchFlags flags)
        {
            fixed (char* v = &rv, p = &rp)
            {
                var vend = v + (uint)vlen;
                var pend = p + (uint)plen;

                if (flags == MatchFlags.Windows || flags == MatchFlags.Auto && Path.DirectorySeparatorChar == '\\')
                    return DoMatch<Windows>(p, pend, v, vend) == vend;

                return DoMatch<Unix>(p, pend, v, vend) == vend;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int Length(char* s, char* e)
        {
            Debug.Assert((nint)s <= (nint)e);

            // C# emits suboptimal code for the e - s operation in our case.
            // However, since the condition s <= e is always true in our case,
            // we can assist the JIT in generating efficient code.
            return (int)(((nint)e - (nint)s) >>> 1);
        }

        // Advances the pointer past any slash characters
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static char* SkipSlash<TFlags>(char* p, char* pend)
        {
            while (p < pend && (p[0] == '/' || typeof(TFlags) == typeof(Windows) && p[0] == '\\'))
                p++;

            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static char* FindNextSlash<TFlags>(char* p, char* pend)
        {
            if (p < pend)
            {
                var n = Length(p, pend);
                var s = MemoryMarshal.CreateSpan(ref *p, n);
                var r = typeof(TFlags) == typeof(Windows)
                    ? s.IndexOfAny('/', '\\')
                    : s.IndexOf('/');

                if (r >= 0)
                    return p + (uint)r;
            }

            return pend;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static char* DoMatch<TFlags>(char* p, char* pend, char* v, char* vend)
        {
            var level = 0;

            p = SkipSlash<TFlags>(p, pend);
            v = SkipSlash<TFlags>(v, vend);

            while (p < pend && v <= vend)
            {
                var pe = FindNextSlash<TFlags>(p, pend);

                // In both systems (little-endian and big-endian), *(int*)"**" is 0x002A002A.
                //
                // 1. In UTF-16, the character '*' (asterisk) has the code 0x002A.
                // 2. In memory, "**" will be represented as a sequence of two 16-bit values: 0x002A and 0x002A.
                //
                // 3. * In little-endian systems: 2A 00 2A 00
                //    * In big-endian systems:    00 2A 00 2A
                //
                // 4. When we interpret these 4 bytes as an `int`, we get:
                //    * In little-endian: 0x002A002A (bytes are read from right to left)
                //    * In big-endian:    0x002A002A (bytes are read from left to right)
                //
                // Thus, despite the different byte order in memory, the numeric value interpreted
                // as an `int` turns out to be the same in both cases.
                //
                // This occurs because:
                // - Each '*' character occupies exactly 2 bytes in UTF-16.
                // - The value of each character (0x002A) is symmetrical with respect to byte order.
                if (Length(p, pe) == 2 && Unsafe.Read<int>(p) == ('*' << 16 | '*'))
                {
                    p = SkipSlash<TFlags>(pe, pend);

                    // short-circuit
                    // match found if "**" is the last segment in the pattern
                    if (p == pend)
                        return vend;

                    do
                    {
                        var r = DoMatch<TFlags>(p, pend, v, vend);
                        if (r != null)
                            return r;

                        // try to match the next segment
                        v = FindNextSlash<TFlags>(v, vend);
                        v = SkipSlash<TFlags>(v, vend);
                    }
                    while (v < vend);

                    // abort recursion
                    return (char*)-1;
                }

                var ve = FindNextSlash<TFlags>(v, vend);

                // 1. At the root level (level == 0), an empty path segment is valid,
                //    which can be represented by patterns like "*".
                // 2. At any deeper level (level > 0), an empty segment indicates that a required directory or file is missing,
                //    making the path invalid for patterns expecting something at that level.
                //
                // For example:
                //   Pattern: "*/*"    and path: "a" - This pattern requires at least one directory level, so "a" is not a match.
                //   Pattern: "*/{,b}" and path: "a" - Similarly, this pattern requires a directory or a specific file ("b")
                //                                     at the next level, so "a" doesn't match.
                //
                // This means that the patterns "*/{}" and "*/{,}" cannot match any path due to the rule:
                // an empty segment is not allowed beyond the root level.
                //
                // So, the check (v == ve && level != 0) ensures that empty segments are only acceptable at the root level.
                // If an empty segment is found at a deeper level, the function returns null to indicate no match.
                if (v == ve && level != 0)
                    return null;

                if (DoMatchSegment(p, pe, v, ve, subpattern: 0) != ve)
                    return null;

                p = SkipSlash<TFlags>(pe, pend);
                v = SkipSlash<TFlags>(ve, vend);

                level++;
            }

            if (p == pend && v == vend)
                return vend;

            return null;
        }
    }

    private static char* DoMatchSegment(char* p, char* pend, char* v, char* vend, int subpattern)
    {
        while (p < pend && v < vend)
        {
            switch (p[0])
            {
                case '[':
                {
                    // We check an overrun slightly further below
                    p++;

                    var inverse = false;
                    var matched = false;

                    if (p[0] == '!')
                    {
                        inverse = true;
                        p++;
                    }

                    var a = p[0];

                    do
                    {
                        // Malformed
                        if (++p >= pend)
                            return null;

                        var b = a;

                        if (p[0] == '-' && p[1] != ']')
                        {
                            // Malformed
                            if (++p >= pend)
                                return null;

                            b = p[0];
                            p++;
                        }

                        matched |= a <= v[0] & v[0] <= b;

                        a = p[0];

                    } while (a != ']');

                    if (matched == inverse)
                        return null;

                    p++;
                    v++;
                    break;
                }

                case '{':
                {
                    var e = ExtractPattern(p, pend);
                    if (e == null)
                        return null;

                    v = MatchSubpattern(p, e, v, vend);
                    if (v == null)
                        return null;

                    p = e;
                    break;
                }

                case '*':
                {
                    // *** --> *
                    // Treats consecutive stars as one
                    while (++p < pend && p[0] == '*')
                        continue; // ReSharper disable once RedundantJumpStatement

                    // Trailing '*' matches everything
                    if (p == pend)
                        return vend;

                    while (true)
                    {
                        var r = DoMatchSegment(p, pend, v, vend, subpattern);
                        if (r != null)
                            return r;

                        if (++v >= vend)
                            break;
                    }

                    // Aborting recursion when failing
                    //
                    // To prevent quadratic behavior in scenarios like the pattern "a*a*a*a*c"
                    // matching against the text "aaaaaaaaaaaaaaa", we use <-1> as a return
                    // value instead of <null>.
                    //
                    // This optimization prevents each star-loop from running to the end of the text.
                    // Because only the last star-loop needs to iterate to the end of the text
                    // since advancing the previous star-loops wouldn't yield positive results
                    // upon the failure of the last star-loop.
                    if (p >= pend || p[0] != '{')
                        return (char*)-1;

                    break;
                }

                case '\\':
                {
                    // Invalid escape sequence
                    if (p == pend)
                        return null;

                    p++;
                    goto default;
                }

                default:
                {
                    if (p[0] != '?' && p[0] != v[0])
                        return null;

                    p++;
                    v++;
                    break;
                }
            }
        }

        if (v == vend)
        {
            // Check if the remaining pattern can match an empty string:
            // - Trailing stars (*) can match zero or more characters, including an empty string.
            // - Brace patterns ({},{a,}) can include alternatives that match an empty string.
            while (p < pend)
            {
                if (p[0] == '*')
                {
                    p++;
                }
                else if (p[0] == '{')
                {
                    var se = ExtractPattern(p, pend);
                    if (MatchSubpattern(p, se, v, v) != v)
                        return null;

                    p = se;
                }
                else
                {
                    return null;
                }
            }

            return v;
        }

        if (p == pend && subpattern != 0)
            return v;

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char* MatchSubpattern(char* p, char* pend, char* v, char* vend)
    {
        var last = (char*)0;

        while (p < pend && p[0] != '}')
        {
            var e = ExtractSubpattern(p, pend);
            var r = DoMatchSegment(p + 1, e, v, vend, subpattern: 1);

            if (r > last)
                last = r;

            p = e;
        }

        return last;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static char* ExtractSubpattern(char* p, char* pend)
        {
            Debug.Assert(p[0] is ',' or '{');

            var depth = 1;

            while (++p < pend)
            {
                var c = p[0];
                if (c == ',' && depth == 1 || c == '}' && --depth == 0)
                    break;

                if (c == '{')
                {
                    depth++;
                }
                else if (c == '\\')
                {
                    p++;
                }
            }

            return p;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char* ExtractPattern(char* p, char* pend)
    {
        Debug.Assert(p[0] == '{');

        var depth = 1;

        while (++p < pend)
        {
            var c = p[0];
            if (c == '}' && --depth == 0)
                return p + 1;

            if (c == '{')
            {
                depth++;
            }
            else if (c == '\\')
            {
                p++;
            }
        }

        // Malformed
        return null;
    }
}

using System.IO.Enumeration;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Provides methods for enumerating files and directories based on glob patterns and optional exclusions.
/// </summary>
public static partial class Files
{
    private static ReadOnlySpan<char> GetRelativePath(ref FileSystemEntry entry)
    {
        var path = entry.ToFullPath();
        var skip = entry.RootDirectory.Length;

        return MemoryMarshal.CreateReadOnlySpan(
            length: path.Length - skip,
            reference: ref Unsafe.Add(
                ref Unsafe.AsRef(in path.GetPinnableReference()),
                skip));
    }

    private static bool ShouldInclude(ref FileSystemEntry entry, string[] patterns, string[] excludes, MatchFlags flags, SearchTarget target)
    {
        var current = entry.IsDirectory
            ? SearchTarget.Directories
            : SearchTarget.Files;

        var relative = GetRelativePath(ref entry);
        return ((target & current) != 0
            && IsLeafMatch(relative, excludes, flags) == false
            && IsLeafMatch(relative, patterns, flags));
    }

    private static bool ShouldRecurse(ref FileSystemEntry entry, string[] patterns, string[] excludes, MatchFlags flags)
    {
        var relative = GetRelativePath(ref entry);
        return IsLeafMatch(relative, excludes, flags) == false
            && IsPartialMatch(relative, patterns, flags);
    }

    private static bool IsLeafMatch(ReadOnlySpan<char> fullName, string[] patterns, MatchFlags flags)
    {
        foreach (var pattern in patterns)
            if (Matcher.IsMatch(fullName, pattern, flags))
                return true;

        return false;
    }

    private static bool IsPartialMatch(ReadOnlySpan<char> path, string[] patterns, MatchFlags flags)
    {
        var depth = CountPathSegments(path, flags) - 1;
        foreach (var pattern in patterns)
            if (Matcher.IsMatch(path, GetPartialPattern(pattern, depth), flags))
                return true;

        return false;
    }

    private static int CountPathSegments(ReadOnlySpan<char> path, MatchFlags flags)
    {
        ref var s = ref Unsafe.AsRef(in path.GetPinnableReference());
        ref var e = ref Unsafe.Add(ref s, (uint)path.Length);

        while (Unsafe.IsAddressLessThan(ref s, ref e) && (s == '/' || (s == '\\' && flags == MatchFlags.Windows)))
            s = ref Unsafe.Add(ref s, 1);

        var count = 1;
        var separator = false;

        for (; Unsafe.IsAddressLessThan(ref s, ref e); s = ref Unsafe.Add(ref s, 1))
        {
            if (s == '/' || (s == '\\' && flags == MatchFlags.Windows))
            {
                separator = true;
            }
            else if (separator)
            {
                separator = false;
                count++;
            }
        }

        return count;
    }

    private static ReadOnlySpan<char> GetPartialPattern(string pattern, int depth)
    {
        ref var s = ref Unsafe.AsRef(in pattern.GetPinnableReference());
        ref var e = ref Unsafe.Add(ref s, pattern.Length);

        while (Unsafe.IsAddressLessThan(ref s, ref e) && s == '/')
            s = ref Unsafe.Add(ref s, 1);

        var separator = false;
        var i = (nint)0;

        for (; i < pattern.Length; i++)
        {
            if (Unsafe.Add(ref s, i) == '/')
            {
                separator = true;
                if (depth == 0)
                    break;
            }
            else if (separator)
            {
                separator = false;
                depth--;

                if (Unsafe.As<char, int>(ref Unsafe.Add(ref s, i)) == ('*' << 16 | '*'))
                {
                    if (Unsafe.Add(ref s, i + 2) == '/' || i + 2 >= pattern.Length)
                    {
                        i += 2;
                        break;
                    }
                }
            }
        }

        return MemoryMarshal.CreateReadOnlySpan(ref s, (int)i);
    }

    private static MatchFlags AdjustMatchFlags(MatchFlags flags)
    {
        if (flags == MatchFlags.Auto)
            return Path.DirectorySeparatorChar == '\\'
                ? MatchFlags.Windows
                : MatchFlags.Unix;

        return flags;
    }

    private static string[] ToExcludes(string? exclude) =>
        exclude is not null ? [exclude] : [];

    #region Inner type: SearchTarget

    /// <summary>
    /// Specifies the search targets for enumeration.
    /// </summary>
    [Flags]
    private enum SearchTarget
    {
        /// <summary>
        /// Search for files only.
        /// </summary>
        Files = 1,

        /// <summary>
        /// Search for directories only.
        /// </summary>
        Directories = 2,

        /// <summary>
        /// Search for both files and directories.
        /// </summary>
        Both = Files | Directories
    }

    #endregion
}

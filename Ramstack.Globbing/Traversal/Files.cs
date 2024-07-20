using System.Buffers;
using System.IO.Enumeration;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Provides methods for enumerating files and directories based on glob patterns and optional exclusions.
/// </summary>
public static partial class Files
{
    private const int StackallocThreshold = 128;

    private static bool ShouldInclude(ref FileSystemEntry entry, string[] patterns, string[] excludes, MatchFlags flags, SearchTarget target)
    {
        char[]? rented = null;

        var current = entry.IsDirectory
            ? SearchTarget.Directories
            : SearchTarget.Files;

        var length = entry.Directory.Length - entry.RootDirectory.Length + entry.FileName.Length + 1;
        var relativePath = (uint)length <= StackallocThreshold
            ? stackalloc char[StackallocThreshold]
            : (rented = ArrayPool<char>.Shared.Rent(length));

        relativePath = relativePath[..length];
        WriteRelativePath(ref entry, relativePath);

        var matched = (target & current) != 0
            && IsLeafMatch(relativePath, excludes, flags) == false
            && IsLeafMatch(relativePath, patterns, flags);

        if (rented is not null)
            ArrayPool<char>.Shared.Return(rented);

        return matched;
    }

    private static bool ShouldRecurse(ref FileSystemEntry entry, string[] patterns, string[] excludes, MatchFlags flags)
    {
        char[]? rented = null;

        var length = entry.Directory.Length - entry.RootDirectory.Length + entry.FileName.Length + 1;
        var relativePath = (uint)length <= StackallocThreshold
            ? stackalloc char[StackallocThreshold]
            : (rented = ArrayPool<char>.Shared.Rent(length));

        relativePath = relativePath[..length];
        WriteRelativePath(ref entry, relativePath);

        var matched = IsLeafMatch(relativePath, excludes, flags) == false
            && IsPartialMatch(relativePath, patterns, flags);

        if (rented is not null)
            ArrayPool<char>.Shared.Return(rented);

        return matched;
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
        var count = CountPathSegments(path, flags);

        foreach (var pattern in patterns)
            if (Matcher.IsMatch(path, GetPartialPattern(pattern, flags, count), flags))
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

    private static ReadOnlySpan<char> GetPartialPattern(string pattern, MatchFlags flags, int depth)
    {
        ref var s = ref Unsafe.AsRef(in pattern.GetPinnableReference());
        ref var e = ref Unsafe.Add(ref s, pattern.Length);

        while (Unsafe.IsAddressLessThan(ref s, ref e) && (s == '/' || (s == '\\' && flags == MatchFlags.Windows)))
            s = ref Unsafe.Add(ref s, 1);

        var separator = true;
        var i = (nint)0;

        for (; i < pattern.Length; i++)
        {
            var ch = Unsafe.Add(ref s, i);
            if (ch == '/' || (ch == '\\' && flags == MatchFlags.Windows))
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
                    var c = Unsafe.Add(ref s, i + 2);
                    if (c == '/' || (c == '\\' && flags == MatchFlags.Windows) || i + 2 >= pattern.Length)
                    {
                        i += 2;
                        break;
                    }
                }
            }
        }

        return MemoryMarshal.CreateReadOnlySpan(ref s, (int)i);
    }

    private static void WriteRelativePath(ref FileSystemEntry entry, scoped Span<char> buffer)
    {
        entry.Directory.Slice(entry.RootDirectory.Length).CopyTo(buffer);
        buffer = buffer.Slice(entry.Directory.Length - entry.RootDirectory.Length);

        buffer[0] = '/';

        buffer = buffer.Slice(1);
        entry.FileName.CopyTo(buffer);
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

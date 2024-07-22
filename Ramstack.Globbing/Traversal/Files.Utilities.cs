using System.Buffers;
using System.Diagnostics;
using System.IO.Enumeration;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Ramstack.Globbing.Utilities;

namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Provides methods for enumerating files and directories based on glob patterns and optional exclusions.
/// </summary>
public static partial class Files
{
    private const int StackallocThreshold = 128;

    internal static bool ShouldInclude(ref FileSystemEntry entry, string[] patterns, string[] excludes, MatchFlags flags, SearchTarget target)
    {
        char[]? rented = null;

        var current = entry.IsDirectory
            ? SearchTarget.Directories
            : SearchTarget.Files;

        var length = ComputeRelativePathLength(ref entry);
        var relativePath = (uint)length <= StackallocThreshold
            ? stackalloc char[StackallocThreshold]
            : (rented = ArrayPool<char>.Shared.Rent(length));

        relativePath = relativePath[..length];
        WriteRelativePath(ref entry, relativePath);
        UpdatePathSeparators(relativePath, flags);

        var matched = (target & current) != 0
            && IsLeafMatch(relativePath, excludes, flags) == false
            && IsLeafMatch(relativePath, patterns, flags);

        if (rented is not null)
            ArrayPool<char>.Shared.Return(rented);

        return matched;
    }

    internal static bool ShouldRecurse(ref FileSystemEntry entry, string[] patterns, string[] excludes, MatchFlags flags)
    {
        char[]? rented = null;

        var length = ComputeRelativePathLength(ref entry);
        var relativePath = (uint)length <= StackallocThreshold
            ? stackalloc char[StackallocThreshold]
            : (rented = ArrayPool<char>.Shared.Rent(length));

        relativePath = relativePath[..length];
        WriteRelativePath(ref entry, relativePath);
        UpdatePathSeparators(relativePath, flags);

        var matched = IsLeafMatch(relativePath, excludes, flags) == false
            && IsPartialMatch(relativePath, patterns, flags);

        if (rented is not null)
            ArrayPool<char>.Shared.Return(rented);

        return matched;
    }

    internal static MatchFlags AdjustMatchFlags(MatchFlags flags)
    {
        if (flags == MatchFlags.Auto)
            return Path.DirectorySeparatorChar == '\\'
                ? MatchFlags.Windows
                : MatchFlags.Unix;

        return flags;
    }

    internal static string[] ToExcludes(string? exclude) =>
        exclude is not null ? [exclude] : [];

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
        var directoryLength = entry.Directory.Length;
        var rootLength = entry.RootDirectory.Length;
        var relativeLength = directoryLength - rootLength;

        entry.Directory.Slice(rootLength).CopyTo(buffer);
        buffer[relativeLength ] = '/';

        buffer = buffer.Slice(relativeLength  + 1);

        Debug.Assert(buffer.Length == entry.FileName.Length);
        entry.FileName.CopyTo(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeRelativePathLength(ref FileSystemEntry entry) =>
        entry.Directory.Length - entry.RootDirectory.Length + entry.FileName.Length + 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdatePathSeparators(scoped Span<char> path, MatchFlags flags)
    {
        // To enable escaping in Windows systems, we convert backslashes (\) to forward slashes (/).
        // This is safe because in Windows, backslashes are only used as path separators.
        // Otherwise, the backslash (\) in the path will be treated as an escape character,
        // and as a result, the `Unix` flag will essentially not work on a Windows system.
        if (Path.DirectorySeparatorChar == '\\' && flags == MatchFlags.Unix)
            PathHelper.ConvertToForwardSlashes(path);
    }
}

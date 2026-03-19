using System.Buffers;
using System.Diagnostics;
using System.IO.Enumeration;
using System.Runtime.CompilerServices;

using Ramstack.Globbing.Internal;

namespace Ramstack.Globbing.Traversal;

partial class Files
{
    /// <summary>
    /// The threshold size in characters for using stack allocation.
    /// This value corresponds to the <c>MAX_PATH</c> constant for paths in Windows.
    /// </summary>
    private const int StackallocThreshold = 260;

    /// <summary>
    /// Determines whether the specified file system entry should be included in the results.
    /// </summary>
    /// <param name="entry">A file system entry reference.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of files.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude files.</param>
    /// <param name="flags">The matching options to use.
    /// This parameter must be explicitly set to either <see cref="MatchFlags.Windows"/> or <see cref="MatchFlags.Unix"/>.
    /// <see cref="MatchFlags.Auto"/> is not allowed and will result in an assertion failure.</param>
    /// <param name="target">The search target.</param>
    /// <returns>
    /// <see langword="true" /> if the specified file system entry should be included in the results;
    /// otherwise, <see langword="false" />.
    /// </returns>
    internal static bool ShouldInclude(ref FileSystemEntry entry, string[] patterns, string[] excludes, MatchFlags flags, SearchTarget target)
    {
        Debug.Assert(flags != MatchFlags.Auto);

        char[]? rented = null;

        var current = entry.IsDirectory
            ? SearchTarget.Directories
            : SearchTarget.Files;

        var length = GetRelativePathLength(ref entry);
        var relativePath = (uint)length <= StackallocThreshold
            ? stackalloc char[StackallocThreshold]
            : rented = ArrayPool<char>.Shared.Rent(length);

        relativePath = relativePath[..length];
        WriteRelativePath(ref entry, relativePath);
        UpdatePathSeparators(relativePath, flags);

        var matched = (target & current) != 0
            && PathHelper.IsMatch(relativePath, excludes, flags) == false
            && PathHelper.IsMatch(relativePath, patterns, flags);

        if (rented is not null)
            ArrayPool<char>.Shared.Return(rented);

        return matched;
    }

    /// <summary>
    /// Determines whether the specified file system entry should be recursed.
    /// </summary>
    /// <param name="entry">A file system entry reference.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of files.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude files.</param>
    /// <param name="flags">The matching options to use.
    /// This parameter must be explicitly set to either <see cref="MatchFlags.Windows"/> or <see cref="MatchFlags.Unix"/>.
    /// <see cref="MatchFlags.Auto"/> is not allowed and will result in an assertion failure.</param>
    /// <returns>
    /// <see langword="true" /> if the specified directory entry should be recursed into;
    /// otherwise, <see langword="false" />.
    /// </returns>
    internal static bool ShouldRecurse(ref FileSystemEntry entry, string[] patterns, string[] excludes, MatchFlags flags)
    {
        Debug.Assert(flags != MatchFlags.Auto);

        char[]? rented = null;

        var length = GetRelativePathLength(ref entry);
        var relativePath = (uint)length <= StackallocThreshold
            ? stackalloc char[StackallocThreshold]
            : rented = ArrayPool<char>.Shared.Rent(length);

        relativePath = relativePath[..length];
        WriteRelativePath(ref entry, relativePath);
        UpdatePathSeparators(relativePath, flags);

        var matched = PathHelper.IsMatch(relativePath, excludes, flags) == false
            && PathHelper.IsPartialMatch(relativePath, patterns, flags);

        if (rented is not null)
            ArrayPool<char>.Shared.Return(rented);

        return matched;
    }

    /// <summary>
    /// Resolves the provided match flags by converting <see cref="MatchFlags.Auto"/>
    /// to a concrete value based on the current operating system's directory separator convention.
    /// </summary>
    /// <param name="flags">The initial match flags to resolve.</param>
    /// <returns>
    /// The resolved match flags. If the input is <see cref="MatchFlags.Auto"/>, returns
    /// <see cref="MatchFlags.Windows"/> on Windows systems and <see cref="MatchFlags.Unix"/>
    /// on Unix-like systems. Otherwise, returns the original flags unchanged.
    /// </returns>
    internal static MatchFlags ResolveMatchFlags(MatchFlags flags)
    {
        if (flags == MatchFlags.Auto)
            return Path.DirectorySeparatorChar == '\\'
                ? MatchFlags.Windows
                : MatchFlags.Unix;

        return flags;
    }

    /// <summary>
    /// Converts a nullable exclude string to an array of exclude strings.
    /// </summary>
    /// <param name="exclude">The exclude string to convert.</param>
    /// <returns>
    /// An array of exclude strings.
    /// </returns>
    internal static string[] ToExcludes(string? exclude) =>
        exclude is not null ? [exclude] : [];

    private static void WriteRelativePath(ref FileSystemEntry entry, scoped Span<char> buffer)
    {
        var rootLength = entry.RootDirectory.Length;
        var directoryLength = entry.Directory.Length;
        var start = directoryLength - rootLength;

        entry.Directory.Slice(rootLength).CopyTo(buffer);
        buffer[start ] = '/';
        buffer = buffer.Slice(start  + 1);
        entry.FileName.CopyTo(buffer);

        Debug.Assert(buffer.Length == entry.FileName.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetRelativePathLength(ref FileSystemEntry entry)
    {
        // AggressiveInlining
        // ------------------
        // This method is 47 bytes of IL code consisting solely of calls (6 calls),
        // and JIT refuses to inline it, even though the x86-64 output results
        // in a small set of instructions.

        return entry.Directory.Length - entry.RootDirectory.Length + entry.FileName.Length + 1;
    }

    private static void UpdatePathSeparators(scoped Span<char> path, MatchFlags flags)
    {
        Debug.Assert(flags != MatchFlags.Auto);

        // To enable escaping in Windows systems, we convert backslashes (\) to forward slashes (/).
        // This is safe because in Windows, backslashes are only used as path separators.
        // Otherwise, the backslash (\) in the path will be treated as an escape character,
        // and as a result, the MatchFlags.Unix flag will essentially not work on a Windows system.
        if (Path.DirectorySeparatorChar == '\\' && flags == MatchFlags.Unix)
            PathHelper.ConvertPathToPosixStyle(path);
    }
}

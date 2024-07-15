using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Provides methods for enumerating files and directories based on glob patterns and optional exclusions.
/// </summary>
public static class Files
{
    /// <summary>
    /// Enumerates files in a directory that match the specified glob pattern.
    /// </summary>
    /// <param name="path">The relative or absolute path to the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of files.</param>
    /// <param name="exclude">Optional glob pattern to exclude files.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of the full names for the files in the directory specified by <paramref name="path"/>
    /// and that match the specified glob pattern.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
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
    public static IEnumerable<string> EnumerateFiles(string path, string pattern, string? exclude = null, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateEntries(path, [pattern], ToExcludes(exclude), flags, SearchTarget.Files, depth: 0);

    /// <summary>
    /// Enumerates files in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="path">The relative or absolute path to the directory to search.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of files.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude files.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of the full names for the files in the directory specified by <paramref name="path"/>
    /// and that match the specified glob patterns.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
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
    public static IEnumerable<string> EnumerateFiles(string path, string[] patterns, string[]? excludes = null, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateEntries(path, patterns, excludes ?? [], flags, SearchTarget.Files, depth: 0);

    /// <summary>
    /// Enumerates directories in a directory that match the specified glob pattern.
    /// </summary>
    /// <param name="path">The relative or absolute path to the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of directories.</param>
    /// <param name="exclude">Optional glob pattern to exclude directories.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of the full names for the directories in the directory specified by <paramref name="path"/>
    /// and that match the specified glob pattern.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
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
    public static IEnumerable<string> EnumerateDirectories(string path, string pattern, string? exclude = null, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateEntries(path, [pattern], ToExcludes(exclude), flags, SearchTarget.Directories, depth: 0);

    /// <summary>
    /// Enumerates directories in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="path">The relative or absolute path to the directory to search.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of directories.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude directories.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of the full names for the directories in the directory specified by <paramref name="path"/>
    /// and that match the specified glob patterns.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
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
    public static IEnumerable<string> EnumerateDirectories(string path, string[] patterns, string[]? excludes = null, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateEntries(path, patterns, excludes ?? [], flags, SearchTarget.Directories, depth: 0);

    /// <summary>
    /// Returns an enumerable collection of file names and directory names that match a search pattern in a specified path.
    /// </summary>
    /// <param name="path">The relative or absolute path to the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of file-system entries in path.</param>
    /// <param name="exclude">Optional glob pattern to exclude file-system entries.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of the full names for the file-system entries in the directory specified by <paramref name="path"/>
    /// and that match the specified glob pattern.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
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
    public static IEnumerable<string> EnumerateFileSystemEntries(string path, string pattern, string? exclude = null, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateEntries(path, [pattern], ToExcludes(exclude), flags, SearchTarget.Both, depth: 0);

    /// <summary>
    /// Enumerates file-system entries (files and directories) in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="path">The relative or absolute path to the directory to search.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of file-system entries.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude file-system entries.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of the full names for the file-system entries in the directory specified by <paramref name="path"/>
    /// and that match the specified glob patterns.
    /// </returns>
    /// <remarks>
    /// Glob pattern:
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
    public static IEnumerable<string> EnumerateFileSystemEntries(string path, string[] patterns, string[]? excludes = null, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateEntries(path, patterns, excludes ?? [], flags, SearchTarget.Both, depth: 0);

    private static IEnumerable<string> EnumerateEntries(string path, string[] patterns, string[] excludes, MatchFlags flags, SearchTarget target, int depth)
    {
        path = Path.GetFullPath(path);
        return EnumerateEntriesRecursive(path, path, patterns, excludes, flags, target, depth);
    }

    private static IEnumerable<string> EnumerateEntriesRecursive(string basePath, string directory, string[] patterns, string[] excludes, MatchFlags flags, SearchTarget target, int depth)
    {
        foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
        {
            var current = Directory.Exists(entry)
                ? SearchTarget.Directories
                : SearchTarget.Files;

            var normalizedPath = MemoryMarshal
                .CreateReadOnlySpan(
                    length: entry.Length - basePath.Length,
                    reference: ref Unsafe.Add(
                        ref Unsafe.AsRef(in entry.GetPinnableReference()),
                        basePath.Length))
                .ToString();

            if ((target & current) != 0
                && IsLeafMatch(normalizedPath, excludes, flags) == false
                && IsLeafMatch(normalizedPath, patterns, flags))
                yield return entry;

            if (current != SearchTarget.Directories)
                continue;

            if (IsLeafMatch(normalizedPath, excludes, flags))
                continue;

            if (!IsPartialMatch(normalizedPath, patterns, flags, depth))
                continue;

            foreach (var e in EnumerateEntriesRecursive(basePath, entry, patterns, excludes, flags, target, depth + 1))
                yield return e;
        }
    }

    private static bool IsLeafMatch(string fullName, string[] patterns, MatchFlags flags)
    {
        foreach (var pattern in patterns)
            if (Matcher.IsMatch(fullName, pattern, flags))
                return true;

        return false;
    }

    private static bool IsPartialMatch(string path, string[] patterns, MatchFlags flags, int depth)
    {
        foreach (var pattern in patterns)
            if (Matcher.IsMatch(path, GetPartialPattern(pattern, depth), flags))
                return true;

        return false;
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

                if (Unsafe.As<char, int>(ref Unsafe.Add(ref s, i)) == 0x2a002a)
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

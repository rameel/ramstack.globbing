using System.IO.Enumeration;

namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Provides extension methods for <see cref="DirectoryInfo"/> class.
/// </summary>
public static class DirectoryInfoExtensions
{
    /// <summary>
    /// Enumerates files in a directory that match the specified glob pattern.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of files.</param>
    /// <param name="exclude">Optional glob pattern to exclude files.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of files in the directory specified by <paramref name="directory"/>
    /// that match the specified glob pattern.
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
    public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo directory, string pattern, string? exclude, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateFiles(directory.FullName, [pattern], Files.ToExcludes(exclude), flags, SearchTarget.Files, TraversalOptions.DefaultEnumerationOptions);

    /// <summary>
    /// Enumerates files in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of files.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude files.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of files in the directory specified by <paramref name="directory"/>
    /// that match the specified glob patterns.
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
    public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo directory, string[] patterns, string[]? excludes = null, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateFiles(directory.FullName, patterns, excludes ?? [], flags, SearchTarget.Files, TraversalOptions.DefaultEnumerationOptions);

    /// <summary>
    /// Enumerates files in a directory that match the specified glob pattern.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of files.</param>
    /// <param name="exclude">The glob pattern to exclude files.</param>
    /// <param name="options">An object describing the traversal options.</param>
    /// <returns>
    /// An enumerable collection of files in the directory specified by <paramref name="directory"/>
    /// that match the specified glob pattern.
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
    public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo directory, string pattern, string? exclude, TraversalOptions? options) =>
        EnumerateFiles(directory.FullName, [pattern], Files.ToExcludes(exclude), options?.MatchFlags ?? MatchFlags.Auto, SearchTarget.Files, options.ToEnumerationOptions());

    /// <summary>
    /// Enumerates files in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of files.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude files.</param>
    /// <param name="options">An object describing the traversal options.</param>
    /// <returns>
    /// An enumerable collection of files in the directory specified by <paramref name="directory"/>
    /// that match the specified glob patterns.
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
    public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo directory, string[] patterns, string[]? excludes, TraversalOptions? options) =>
        EnumerateFiles(directory.FullName, patterns, excludes ?? [], options?.MatchFlags ?? MatchFlags.Auto, SearchTarget.Files, options.ToEnumerationOptions());

    /// <summary>
    /// Enumerates directories in a directory that match the specified glob pattern.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of directories.</param>
    /// <param name="exclude">Optional glob pattern to exclude directories.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of directories in the directory specified by <paramref name="directory"/>
    /// that match the specified glob pattern.
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
    public static IEnumerable<DirectoryInfo> EnumerateDirectories(this DirectoryInfo directory, string pattern, string? exclude, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateDirectories(directory.FullName, [pattern], Files.ToExcludes(exclude), flags, SearchTarget.Directories, TraversalOptions.DefaultEnumerationOptions);

    /// <summary>
    /// Enumerates directories in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of directories.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude directories.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of directories in the directory specified by <paramref name="directory"/>
    /// that match the specified glob patterns.
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
    public static IEnumerable<DirectoryInfo> EnumerateDirectories(this DirectoryInfo directory, string[] patterns, string[]? excludes = null, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateDirectories(directory.FullName, patterns, excludes ?? [], flags, SearchTarget.Directories, TraversalOptions.DefaultEnumerationOptions);

    /// <summary>
    /// Enumerates directories in a directory that match the specified glob pattern.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of directories.</param>
    /// <param name="exclude">Optional glob pattern to exclude directories.</param>
    /// <param name="options">An object describing the traversal options.</param>
    /// <returns>
    /// An enumerable collection of directories in the directory specified by <paramref name="directory"/>
    /// that match the specified glob pattern.
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
    public static IEnumerable<DirectoryInfo> EnumerateDirectories(this DirectoryInfo directory, string pattern, string? exclude, TraversalOptions? options) =>
        EnumerateDirectories(directory.FullName, [pattern], Files.ToExcludes(exclude), options?.MatchFlags ?? MatchFlags.Auto, SearchTarget.Directories, options.ToEnumerationOptions());

    /// <summary>
    /// Enumerates directories in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of directories.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude directories.</param>
    /// <param name="options">An object describing the traversal options.</param>
    /// <returns>
    /// An enumerable collection of directories in the directory specified by <paramref name="directory"/>
    /// that match the specified glob patterns.
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
    public static IEnumerable<DirectoryInfo> EnumerateDirectories(this DirectoryInfo directory, string[] patterns, string[]? excludes, TraversalOptions? options) =>
        EnumerateDirectories(directory.FullName, patterns, excludes ?? [], options?.MatchFlags ?? MatchFlags.Auto, SearchTarget.Directories, options.ToEnumerationOptions());

    /// <summary>
    /// Enumerates file system information objects in a directory that match any of the specified glob pattern.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of file-system entries in path.</param>
    /// <param name="exclude">Optional glob pattern to exclude file-system entries.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of file system information objects in the directory specified by <paramref name="directory"/>
    /// that match the specified glob pattern.
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
    public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(this DirectoryInfo directory, string pattern, string? exclude, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateInfos(directory.FullName, [pattern], Files.ToExcludes(exclude), flags, SearchTarget.Both, TraversalOptions.DefaultEnumerationOptions);

    /// <summary>
    /// Enumerates file system information objects in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of file-system entries.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude file-system entries.</param>
    /// <param name="flags">The matching options to use. Default is <see cref="MatchFlags.Auto"/>.</param>
    /// <returns>
    /// An enumerable collection of file system information objects in the directory specified by <paramref name="directory"/>
    /// that match the specified glob patterns.
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
    public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(this DirectoryInfo directory, string[] patterns, string[]? excludes = null, MatchFlags flags = MatchFlags.Auto) =>
        EnumerateInfos(directory.FullName, patterns, excludes ?? [], flags, SearchTarget.Both, TraversalOptions.DefaultEnumerationOptions);

    /// <summary>
    /// Enumerates file system information objects in a directory that match any of the specified glob pattern.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="pattern">The glob pattern to match against the names of file-system entries in path.</param>
    /// <param name="exclude">Optional glob pattern to exclude file-system entries.</param>
    /// <param name="options">An object describing the traversal options.</param>
    /// <returns>
    /// An enumerable collection of file system information objects in the directory specified by <paramref name="directory"/>
    /// that match the specified glob pattern.
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
    public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(this DirectoryInfo directory, string pattern, string? exclude, TraversalOptions? options) =>
        EnumerateInfos(directory.FullName, [pattern], Files.ToExcludes(exclude), options?.MatchFlags ?? MatchFlags.Auto, SearchTarget.Both, options.ToEnumerationOptions());

    /// <summary>
    /// Enumerates file system information objects in a directory that match any of the specified glob patterns.
    /// </summary>
    /// <param name="directory">The <see cref="DirectoryInfo"/> instance representing the directory to search.</param>
    /// <param name="patterns">An array of glob patterns to match against the names of file-system entries.</param>
    /// <param name="excludes">Optional array of glob patterns to exclude file-system entries.</param>
    /// <param name="options">An object describing the traversal options.</param>
    /// <returns>
    /// An enumerable collection of file system information objects in the directory specified by <paramref name="directory"/>
    /// that match the specified glob patterns.
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
    public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(this DirectoryInfo directory, string[] patterns, string[]? excludes, TraversalOptions? options) =>
        EnumerateInfos(directory.FullName, patterns, excludes ?? [], options?.MatchFlags ?? MatchFlags.Auto, SearchTarget.Both, options.ToEnumerationOptions());

    private static IEnumerable<FileInfo> EnumerateFiles(string path, string[] patterns, string[] excludes, MatchFlags flags, SearchTarget target, EnumerationOptions options)
    {
        flags = Files.AdjustMatchFlags(flags);

        return new FileSystemEnumerable<FileInfo>(path, (ref FileSystemEntry entry) => (FileInfo)entry.ToFileSystemInfo(), options)
        {
            ShouldIncludePredicate = (ref FileSystemEntry entry) => Files.ShouldInclude(ref entry, patterns, excludes, flags, target),
            ShouldRecursePredicate = (ref FileSystemEntry entry) => Files.ShouldRecurse(ref entry, patterns, excludes, flags)
        };
    }
    private static IEnumerable<DirectoryInfo> EnumerateDirectories(string path, string[] patterns, string[] excludes, MatchFlags flags, SearchTarget target, EnumerationOptions options)
    {
        flags = Files.AdjustMatchFlags(flags);

        return new FileSystemEnumerable<DirectoryInfo>(path, (ref FileSystemEntry entry) => (DirectoryInfo)entry.ToFileSystemInfo(), options)
        {
            ShouldIncludePredicate = (ref FileSystemEntry entry) => Files.ShouldInclude(ref entry, patterns, excludes, flags, target),
            ShouldRecursePredicate = (ref FileSystemEntry entry) => Files.ShouldRecurse(ref entry, patterns, excludes, flags)
        };
    }
    private static IEnumerable<FileSystemInfo> EnumerateInfos(string path, string[] patterns, string[] excludes, MatchFlags flags, SearchTarget target, EnumerationOptions options)
    {
        flags = Files.AdjustMatchFlags(flags);

        return new FileSystemEnumerable<FileSystemInfo>(path, (ref FileSystemEntry entry) => entry.ToFileSystemInfo(), options)
        {
            ShouldIncludePredicate = (ref FileSystemEntry entry) => Files.ShouldInclude(ref entry, patterns, excludes, flags, target),
            ShouldRecursePredicate = (ref FileSystemEntry entry) => Files.ShouldRecurse(ref entry, patterns, excludes, flags)
        };
    }
}

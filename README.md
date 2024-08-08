# Ramstack.Globbing

Fast and zero-allocation .NET globbing library for matching file paths using [glob patterns](https://en.wikipedia.org/wiki/Glob_(programming)).
No external dependencies.

## Getting Started

To install the `Ramstack.Globbing` [NuGet package](https://www.nuget.org/packages/Ramstack.Globbing) to your project, run the following command:
```console
dotnet add package Ramstack.Globbing
```

## Usage

```csharp
bool result = Matcher.IsMatch("wiki/section-1/start.md", "wiki/**/*.md");
```
The `IsMatch` method attempts to match the specified path against the provided wildcard pattern.

By default, the system's default path separators are used. You can override this behavior by specifying one of the following flags:

| Name    | Description                                                                                                                                                                                                                              |
|---------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Auto    | Automatically determines whether to treat backslashes (`\ `) as escape sequences or path separators based on the platform's separator convention.                                                                                        |
| Windows | Treats backslashes (`\ `) as path separators instead of escape sequences.<br>Provides behavior consistent with Windows-style paths.<br>Both backslashes (`\ `) and forward slashes (`/`) are considered as path separators in this mode. |
| Unix    | Treats backslashes (`\ `) as escape sequences, allowing for special character escaping.<br>Provides behavior consistent with Unix-style paths.                                                                                           |

Example with a specific flag:
```csharp
bool result = Matcher.IsMatch("wiki/section-1/start.md", @"wiki\**\*.md", MatchFlags.Windows);
```

## Patterns
From [Wikipedia](https://en.wikipedia.org/wiki/Glob_(programming)#Syntax)

| Pattern | Description                                                                  | Example      | Matches                                                  | Does not match                        |
|---------|------------------------------------------------------------------------------|--------------|----------------------------------------------------------|---------------------------------------|
| *       | matches any number of any characters including none                          | Law*         | Law, Laws, or Lawyer                                     | GrokLaw, La, Law/foo or aw            |
|         |                                                                              | \*Law\*      | Law, GrokLaw, or Lawyer.                                 | La, or aw                             |
| ?       | matches any single character                                                 | ?at          | Cat, cat, Bat or bat                                     | at                                    |
| [abc]   | matches one character given in the bracket                                   | [CB]at       | Cat or Bat                                               | cat, bat or CBat                      |
| [a-z]   | matches one character from the (locale-dependent) range given in the bracket | Letter[0-9]  | Letter0, Letter1, Letter2 up to Letter9                  | Letters, Letter or Letter10           |
| [!abc]  | matches one character that is not given in the bracket                       | [!C]at       | Bat, bat, or cat                                         | Cat                                   |
| [!a-z]  | matches one character that is not from the range given in the bracket        | Letter[!3-5] | Letter1, Letter2, Letter6 up to Letter9 and Letterx etc. | Letter3, Letter4, Letter5 or Letterxx |

### Pattern specific for directories

| Pattern | Description                                         | Example | Matches                        | Does not match |
|---------|-----------------------------------------------------|---------|--------------------------------|----------------|
| **      | matches any number of path segments including none  | **/Law  | dir1/dir2/Law, dir1/Law or Law | dir1/La        |

### Brace patterns

Brace patterns allow for matching multiple alternatives in a single pattern. Here are some key features:

| Pattern        | Description                              | Example             | Matches                                         | Does not match |
|----------------|------------------------------------------|---------------------|-------------------------------------------------|----------------|
| {a,b,c}        | matches any of the comma-separated terms | file.{jpg,png}      | file.jpg, file.png                              | file.gif       |
| {src,test{s,}} | supports nested brace patterns           | {src,test{s,}}/*.cs | src/main.cs, tests/unit.cs, test/integration.cs | doc/readme.cs  |
| {main,,test}   | supports empty alternatives              | {main,,test}1.txt   | main1.txt, test1.txt, 1.txt                     | file1.txt      |
| {[sS]rc,test*} | supports full glob pattern within braces | {[sS]rc,test*}/*.cs | src/app.cs, Src/main.cs, testing/script.cs      | lib/util.cs    |

* Empty alternatives are valid, e.g., `{src,test,}` will also match paths without the listed prefixes.
* Brace patterns can be nested, allowing for complex matching scenarios.
* Full glob patterns can be used within braces, providing powerful and flexible matching capabilities.

## Escaping characters

The meta characters `?`, `*`, `[`, `\ ` can be escaped by using the `[]`, which means *match one character listed in the bracket*.
* `[[]` matches the literal `[`
* `[*]` matches the literal `*`

This works when using any `MatchFlags` (`Windows` or `Unix`).
When using `MatchFlags.Unix`, an additional escape character (`\`) is available:
* `\[` matches the literal `[`
* `\*` matches the literal `*`

## Notes
* Leading and trailing path separators are ignored.
* Consecutive path separators are counted as one separator.

### Special cases
* At the root level, an empty path segment is valid, which can be represented by patterns like "*".
* At any deeper level, an empty segment indicates that a required directory or file is missing,
  making the path invalid for patterns expecting something at that level.

| Pattern  | Matches     | Does not match | Explanation                                                                         |
|----------|-------------|----------------|-------------------------------------------------------------------------------------|
| `*`      | `foo`, `""` |                | Matches everything, e.g. empty string                                               |
| `*/*`    | `a/b`,`b/c` | `a`,`b`,`foo`  | Requires at least one directory level, so `a` is not a match                        |
| `*/{,b}` | `a/b`       | `a`,`b`,`foo`  | Requires a directory or a specific file `b` at the next level, so `a` doesn't match |

:bulb: This means that the patterns `*/{}` and `*/{,}` cannot match any path due to the rule: an empty segment is not allowed beyond the root level.

## Optimizations
We use optimizations that prevent quadratic behavior in scenarios like the pattern `a*a*a*a*a*a*a*a*a*c`
matching against the text `aaaaaaaaaaaaaaa...aaaa...aaa`.
Similarly, for the `a/**/a/**/a/**/.../a/**/a/**/a/**/b` pattern matching against `a/a/a/a/.../a/.../a`.

## File traversal

The `Files` class provides functionality for traversing the file system and retrieving lists of files and directories
based on specified glob patterns. This allows for flexible and efficient file and directory enumeration.

```csharp
using Ramstack.Globbing.Traversal;

// List all *.cs files
var files = Files.EnumerateFiles(@"/path/to/directory", "**/*.cs");
foreach (var file in files)
    Console.WriteLine(file);

// List all *.cs files except in tests directory
var files = Files.EnumerateFiles(@"/path/to/directory", "**/*.cs", "tests");
foreach (var file in files)
    Console.WriteLine(file);
```
Support for multiple patterns is also included:

```csharp
using Ramstack.Globbing.Traversal;

// List all *.cs files
var files = Files.EnumerateFiles(@"/path/to/directory", ["src/**/*.cs", "lib/**/*.cs"], ["**/tests"]);
foreach (var file in files)
    Console.WriteLine(file);
```

There are overloads that take a `TraversalOptions` allowing you to set additional options when traversing the file system,
such as:
- Filtering out specific attributes
- Ignoring inaccessible files
- Maximum recursion depth

These methods are quite efficient in terms of speed, memory consumption, and GC pressure.
Here are the benchmarking results for the [dotnet/runtime](https://github.com/dotnet/runtime) repository folder, which contained
59194 files at the time of testing. The search was for `*.md` files:

```
BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3880/23H2/2023Update/SunValley3)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 9.0.100-preview.6.24328.19
  [Host]     : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2
  Job-GMSEBO : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2

Runtime=.NET 8.0

| Method                             | Mean     | Error   | StdDev  | Gen0      | Gen1     | Allocated |
|----------------------------------- |---------:|--------:|--------:|----------:|---------:|----------:|
| >> Ramstack_Files_EnumerateFiles   | 154.4 ms | 0.67 ms | 0.59 ms |         - |        - |   2.33 MB |
| Microsoft_Directory_EnumerateFiles | 149.1 ms | 0.74 ms | 0.66 ms |         - |        - |   2.33 MB |
| Microsoft_FileSystemGlobbing       | 176.6 ms | 1.16 ms | 1.08 ms | 2000.0000 | 333.3333 |  35.99 MB |
```

As you can see, the code is as fast as a direct search using `Directory.EnumerateFiles` and consumes the same amount of memory.
This makes sense, since the implementation of `Files.EnumerateFiles` uses the same `FileSystemEnumerable` class.

Also, corresponding extension methods added for `DirectoryInfo`, which also allow you to leverage
the full power of glob patterns when searching for files.

## Custom File System Support

The `FileTreeEnumerable` class provides support for custom file systems with glob pattern matching capabilities.
Here is an example of its usage:
```csharp
// --------------------------------------------------------------------
// As an example, we'll use the existing DirectoryInfo/FileInfo classes

var root = new DirectoryInfo(@"D:\Projects\dotnet.runtime");
var enumeration = new FileTreeEnumerable<FileSystemInfo, string>(root)
{
    Patterns = ["**/*.cs"],
    Excludes = ["**/{bin,obj}"],
    FileNameSelector = info => info.Name,
    ShouldRecursePredicate = info => info is DirectoryInfo,
    // The following predicate used to filter the files
    ShouldIncludePredicate = info => info is FileInfo,
    ChildrenSelector = info => ((DirectoryInfo)info).EnumerateFileSystemInfos(),
    // Returns the full path of the file
    ResultSelector = info => info.FullName
};

// Prints all csharp files
foreach (string filePath in enumeration)
    Console.WriteLine(filePath);
```

### Asynchronous Enumeration
The `FileTreeAsyncEnumerable` class provides similar functionality to `FileTreeEnumerable`,
but supports asynchronous enumeration for remote file systems as an example.

Here's an example of how to use `FileTreeAsyncEnumerable`:

```csharp
// ---------------------------------------------------
// Assuming we have an IAsyncFileSystem implementation

var root = cloudFS.GetDirectory(@"/projects/dotnet.runtime");
var enumeration = new FileTreeAsyncEnumerable<IAsyncFileSystemEntry, string>(root)
{
    Patterns = ["**/*.cs"],
    Excludes = ["**/{bin,obj}"],
    FileNameSelector = entry => entry.Name,
    ShouldRecursePredicate = entry => entry is IDirectory,
    ShouldIncludePredicate = entry => entry is IFile,
    ChildrenSelector = (entry, token) => ((IDirectory)entry).GetFileEntriesAsync(token),
    ResultSelector = entry => entry.FullPath
};

// Prints all csharp files asynchronously
await foreach (string filePath in enumeration)
    Console.WriteLine(filePath);
```


## Supported versions

|      | Version |
|------|---------|
| .NET | 6, 7, 8 |

## Contributions

Bug reports and contributions are welcome.

## License
This package is released as open source under the **MIT License**. See the [LICENSE](LICENSE) file for more details.

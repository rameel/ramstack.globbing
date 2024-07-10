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
bool result = Matcher.IsMatch("wiki/**/*.md", "wiki/section-1/start.md");
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
bool result = Matcher.IsMatch(@"wiki\**\*.md", "wiki/section-1/start.md", MatchFlags.Windows);
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

## Supported versions

|      | Version |
|------|---------|
| .NET | 6, 7, 8 |

## Contributions

Bug reports and contributions are welcome.

## License
This package is released as open source under the **MIT License**. See the [LICENSE](LICENSE) file for more details.

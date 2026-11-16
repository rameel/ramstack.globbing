# Contributing

Bug reports, fixes, and documentation improvements are welcome, including small ones.

## Reporting bugs

Tell us what happened and what you expected.
Include the Globbing and .NET versions, your operating system and CPU architecture (x64 or ARM64),
the glob pattern, the path being matched, and the `MatchFlags` used.
For traversal issues, include a small directory layout and the traversal options.

## Development setup

Use the .NET 10 SDK or later to build from source, including the benchmark project.
Install the .NET 6 runtime to run the tests.

Run the commands below from the repository root.

## Building and testing

Build the solution and run the tests in both configurations:

```console
dotnet test -c Debug
dotnet test -c Release
```

CI also runs tests with `DOTNET_EnableAVX2=0` and `DOTNET_EnableHWIntrinsic=0` in both configurations.
Repeat those checks when changing SIMD or other optimized matching code.

## Code style

Follow [.editorconfig](.editorconfig) and the style of the surrounding code.
Keep formatting changes limited to the code you're working on.

## Pull requests

- Keep each pull request focused on one goal. Its commits can use different scopes when they serve that goal.
  Do not mix unrelated changes into it.
- Several cleanups that do not change behavior, such as formatting, comment fixes, and readability improvements,
  can share one pull request when each is a separate commit. Send behavior changes found along the way
  as separate pull requests.
- Explain what changed and why, and link to related issues if there are any.
- When fixing a reproducible bug, add a regression test. Add tests for new behavior where appropriate.
- Update the documentation if your change affects how people use the library.
- Keep changes compatible with the target frameworks declared in the project files.

For major public API changes, open an issue first so we can agree on the approach before you spend time on it.

Use the [scoped commit style](#commit-messages) for pull request titles. Choose the scope with the
[same rules](#choosing-a-scope) as for a commit, treating the whole pull request as one change.
The title does not need to summarize every supporting commit.

## Commit messages

Use scoped commit messages:

```text
<scope>: <concise description>
```

The scope identifies the part of the project that changed. Write the description in the imperative mood,
starting with a lowercase letter and without a trailing period. Describe what the change does rather than
just naming its type.

```text
glob: handle trailing directory separators
traversal: respect the recursion depth limit
bench: compare recursive wildcard patterns
```

Do not prefix messages with conventional commit types such as `feat`, `fix`, `refactor`, or `chore`,
and do not use ticket numbers as scopes. The description should make the type of change clear when it matters.
Put ticket references and implementation details in the message body when needed.

### Scopes

Use the smallest stable area that describes the outcome. Start with these root scopes;
add a subscope only when the history of a root scope becomes difficult to navigate.

| Scope       | Area                                                                                      |
|-------------|-------------------------------------------------------------------------------------------|
| `glob`      | Glob pattern matching, match flags, and matching optimizations                            |
| `traversal` | File and directory traversal, enumeration, and traversal options                          |
| `bench`     | Measurement scenarios, library comparisons, and benchmark results                         |
| `docs`      | General project documentation                                                             |
| `tests`     | Shared test infrastructure                                                                |
| `build`     | Build, CI, dependencies, development tooling, and NuGet packaging                         |
| `repo`      | Repository structure and metadata, contribution and code style rules, cross-area cleanups |

Use the product area's scope for its tests, documentation, and examples, even when a commit changes only
those files. Reserve `tests` and `docs` for their own infrastructure or for related changes across multiple product areas.
Use `docs` for README content that does not belong to one area, such as getting started and supported versions.

Use `glob` or `traversal` for performance improvements to the library itself.
Reserve `bench` for measurement scenarios, comparisons, and reported results.
Use `glob` for matching helpers such as `MemoryHelper`, and `traversal` for traversal helpers
such as `PathHelper` and `FileTreeHelper`.

### Choosing a scope

Most commits change one area; use its scope from the table. When a commit touches several scopes,
use the first rule that applies:

1. **It changes behavior, public API, performance, or debugging experience.** Use the scope of the area
   where the change is visible, even if the implementation spans other areas. If several areas change,
   use the one whose documentation describes the change, or the one the change mainly serves
   when it is not documented, such as an optimization.
2. **It introduces or updates one thing that the rest of the commit adopts.** Use the scope of that thing:
   `build: enable analyzers and treat warnings as errors`, `repo: define formatting rules`.
3. **It only changes comments or documentation.** Use `docs`.
4. **Anything else**, such as formatting, renames, or file moves. Use `repo` for a general cleanup, even when
   only product code changes. Use the product area's scope when the work targets that area's code itself,
   such as simplifying the traversal helpers.

Make each commit one logical change. Split logically separate changes into separate commits, even when they
share a scope: if the description needs "and" to join unrelated changes, the commit should be split.
Do not split a single change only to give each part its own scope, and do not list several scopes.

## Issue titles

Use a clear, descriptive issue title. Scope prefixes are optional; when used, choose from the same scopes
as commits and pull requests. Describe the problem or proposed capability; the imperative mood is not required.

## Test names

Prefer `Method_Condition_ExpectedResult` when adding or renaming tests. This is a recommendation,
not a strict requirement: choose a clearer name when the pattern does not fit naturally.
Use PascalCase within each part and describe the expected observable behavior.

```text
IsMatch_EmptyPatternAndPath_ReturnsTrue
IsMatch_PatternSlice_IgnoresCharsBeyondSlice
```

For parameterized tests, describe the common scenario and let the arguments distinguish individual cases.
There is no need to rename existing tests solely to follow this convention.

using System.Buffers;
using System.Collections;

namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Represents an enumerable file tree structure with customizable filtering and selection options.
/// </summary>
/// <typeparam name="TEntry">The type of the entry in the file tree.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class FileTreeEnumerable<TEntry, TResult> : IEnumerable<TResult>
{
    private readonly TEntry _directory;

    /// <summary>
    /// Gets or sets the glob patterns to include in the enumeration.
    /// </summary>
    public required string[] Patterns { get; init; }

    /// <summary>
    /// Gets or sets the patterns to exclude from the enumeration.
    /// Defaults to an empty array.
    /// </summary>
    public string[] Excludes { get; init; } = [];

    /// <summary>
    /// Gets or sets the matching options to use. Defaults to <see cref="MatchFlags.Auto"/>.
    /// </summary>
    public MatchFlags Flags { get; init; } = MatchFlags.Auto;

    /// <summary>
    /// Gets or sets the predicate that determines whether to recurse into a directory.
    /// </summary>
    public Func<TEntry, bool>? ShouldRecursePredicate { get; init; }

    /// <summary>
    /// Gets or sets the predicate that determines whether to include a file entry in the result set.
    /// </summary>
    public Func<TEntry, bool>? ShouldIncludePredicate { get; init; }

    /// <summary>
    /// Gets or sets a function to extract the name for a file entry.
    /// </summary>
    public required Func<TEntry, string> FileNameSelector { get; init; }

    /// <summary>
    /// Gets or sets a function that retrieves the child entries of an entry.
    /// </summary>
    public required Func<TEntry, IEnumerable<TEntry>> ChildrenSelector { get; init; }

    /// <summary>
    /// Gets or sets a function that transforms a file entry into a result.
    /// </summary>
    public required Func<TEntry, TResult> ResultSelector { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTreeEnumerable{TEntry, TResult}"/> class.
    /// </summary>
    /// <param name="directory">The root directory to start the enumeration from.</param>
    public FileTreeEnumerable(TEntry directory) =>
        _directory = directory;

    /// <inheritdoc />
    IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator() =>
        Enumerate().GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() =>
        Enumerate().GetEnumerator();

    private IEnumerable<TResult> Enumerate()
    {
        var chars = ArrayPool<char>.Shared.Rent(512);
        var stack = new Stack<(TEntry Directory, string Path)>();
        stack.Push((_directory, ""));

        while (stack.TryPop(out var e))
        {
            foreach (var entry in ChildrenSelector(e.Directory))
            {
                var name = FileNameSelector(entry);
                var fullName = GetFullName(ref chars, e.Path, name);

                if (ShouldRecurseInto(entry, fullName))
                    stack.Push((entry, fullName.ToString()));

                if (ShouldIncludeEntry(entry, fullName))
                    yield return ResultSelector(entry);
            }
        }

        ArrayPool<char>.Shared.Return(chars);
    }

    private bool ShouldRecurseInto(TEntry entry, ReadOnlySpan<char> path) =>
        (ShouldRecursePredicate?.Invoke(entry) ?? true) && IsPartialMatch(path);

    private bool ShouldIncludeEntry(TEntry entry, ReadOnlySpan<char> path) =>
        (ShouldIncludePredicate?.Invoke(entry) ?? true) && IsIncluded(path);

    private bool IsIncluded(ReadOnlySpan<char> path) =>
        !IsExcluded(path) && PathHelper.IsMatch(path, Patterns, Flags);

    private bool IsPartialMatch(ReadOnlySpan<char> path) =>
        !IsExcluded(path) && PathHelper.IsPartialMatch(path, Patterns, Flags);

    private bool IsExcluded(ReadOnlySpan<char> path) =>
        PathHelper.IsMatch(path, Excludes, Flags);

    private ReadOnlySpan<char> GetFullName(ref char[] chars, string path, string name)
    {
        var length = path.Length + name.Length + 1;
        if (chars.Length <= length)
        {
            ArrayPool<char>.Shared.Return(chars);
            chars = ArrayPool<char>.Shared.Rent(length);
            // Force non-null assumption
            _ = chars.Length;
        }

        var buffer = chars.AsSpan(0, length);

        path.CopyTo(buffer);
        buffer[path.Length] = '/';
        name.CopyTo(buffer.Slice(path.Length + 1));

        return buffer;
    }
}

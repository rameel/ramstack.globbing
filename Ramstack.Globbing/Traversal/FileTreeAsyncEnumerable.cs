using System.Buffers;
using System.Runtime.CompilerServices;

using Ramstack.Globbing.Internal;

namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Represents an asynchronously enumerable file tree structure with customizable filtering and selection options.
/// </summary>
/// <typeparam name="TEntry">The type of the entry in the file tree.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public sealed class FileTreeAsyncEnumerable<TEntry, TResult> : IAsyncEnumerable<TResult>
{
    private readonly TEntry _directory;
    private readonly CancellationToken _cancellationToken;

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
    public required Func<TEntry, CancellationToken, IAsyncEnumerable<TEntry>> ChildrenSelector { get; init; }

    /// <summary>
    /// Gets or sets a function that transforms a file entry into a result.
    /// </summary>
    public required Func<TEntry, TResult> ResultSelector { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTreeAsyncEnumerable{TEntry, TResult}"/> class.
    /// </summary>
    /// <param name="directory">The root directory to start the enumeration from.</param>
    /// <param name="cancellationToken">An optional cancellation token that may be used to cancel the asynchronous iteration.</param>
    public FileTreeAsyncEnumerable(TEntry directory, CancellationToken cancellationToken = default)
    {
        _directory = directory;
        _cancellationToken = cancellationToken;
    }

    /// <inheritdoc />
    IAsyncEnumerator<TResult> IAsyncEnumerable<TResult>.GetAsyncEnumerator(CancellationToken cancellationToken)
    {
        CancellationTokenSource? source = null;

        if (_cancellationToken != default)
            cancellationToken = cancellationToken != default
                ? (source = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, cancellationToken)).Token
                : _cancellationToken;

        return EnumerateAsync(source, cancellationToken).GetAsyncEnumerator(cancellationToken);
    }

    private async IAsyncEnumerable<TResult> EnumerateAsync(CancellationTokenSource? source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var chars = ArrayPool<char>.Shared.Rent(512);

        try
        {
            var stack = new Stack<(TEntry Directory, string Path)>();
            stack.Push((_directory, ""));

            while (stack.TryPop(out var e))
            {
                await foreach (var entry in ChildrenSelector(e.Directory, cancellationToken).ConfigureAwait(false))
                {
                    var name = FileNameSelector(entry);
                    var fullName = FileTreeHelper.GetFullName(ref chars, e.Path, name);

                    if (PathHelper.IsMatch(fullName, Excludes, Flags))
                        continue;

                    if (ShouldRecursePredicate == null || ShouldRecursePredicate(entry))
                        if (PathHelper.IsPartialMatch(fullName, Patterns, Flags))
                            stack.Push((entry, fullName.ToString()));

                    if (ShouldIncludePredicate == null || ShouldIncludePredicate(entry))
                        if (PathHelper.IsMatch(fullName, Patterns, Flags))
                            yield return ResultSelector(entry);
                }
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(chars);
            source?.Dispose();
        }
    }
}

namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Provides extension methods for the <see cref="TraversalOptions"/> class.
/// </summary>
internal static class TraversalOptionsExtensions
{
    /// <summary>
    /// Converts traversal options to <see cref="EnumerationOptions"/>.
    /// </summary>
    /// <param name="options">The traversal options to convert.</param>
    /// <returns>
    /// An <see cref="EnumerationOptions"/> object based on <paramref name="options"/>.
    /// </returns>
    public static EnumerationOptions ToEnumerationOptions(this TraversalOptions? options)
    {
        if (options is null or {
            AttributesToSkip: TraversalOptions.DefaultAttributesToSkip,
            MaxRecursionDepth: TraversalOptions.DefaultMaxRecursionDepth,
            IgnoreInaccessible: true })
            return TraversalOptions.DefaultEnumerationOptions;

        return new EnumerationOptions
        {
            AttributesToSkip = options.AttributesToSkip,
            IgnoreInaccessible = options.IgnoreInaccessible,
            MaxRecursionDepth = options.MaxRecursionDepth,
            RecurseSubdirectories = true
        };
    }
}

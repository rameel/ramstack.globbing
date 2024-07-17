﻿namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Provides extension methods for the <see cref="TraversalOptions"/> class.
/// </summary>
internal static class TraversalOptionsExtensions
{
    /// <summary>
    /// Converts <paramref name="options"/> of type <see cref="TraversalOptions"/> to <see cref="EnumerationOptions"/>.
    /// </summary>
    /// <param name="options">The traversal options to convert.</param>
    /// <returns>
    /// An <see cref="EnumerationOptions"/> object based on <paramref name="options"/>.
    /// </returns>
    public static EnumerationOptions ToEnumerationOptions(this TraversalOptions? options)
    {
        if (options is null or { IgnoreInaccessible: true, AttributesToSkip: (FileAttributes.Hidden | FileAttributes.System), MaxRecursionDepth: int.MaxValue })
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

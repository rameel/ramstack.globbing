namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Provides file and directory traversal options.
/// </summary>
public sealed class TraversalOptions
{
    /// <summary>
    /// Gets or sets the match flags. The default is <see cref="MatchFlags.Auto"/>
    /// </summary>
    public MatchFlags MatchFlags { get; set; }

    /// <summary>
    /// Gets or sets the attributes to skip.
    /// The default is <c>FileAttributes.Hidden | FileAttributes.System</c>.
    /// </summary>
    public FileAttributes AttributesToSkip { get; set; } = FileAttributes.Hidden | FileAttributes.System;

    /// <summary>
    /// Gets or sets a value that indicates the maximum directory depth to recurse while traversing.
    /// The default is <see cref="int.MaxValue"/>.
    /// </summary>
    /// <remarks>
    /// If <see cref="MaxRecursionDepth"/> is set to zero, the contents of the initial directory will be returned.
    /// </remarks>
    public int MaxRecursionDepth { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets a value that indicates whether to skip files or directories when access is denied
    /// (for example, <see cref="UnauthorizedAccessException"/> or <see cref="System.Security.SecurityException"/>).
    /// The default is <c>true</c>.
    /// </summary>
    public bool IgnoreInaccessible { get; set; } = true;

    /// <summary>
    /// Gets the default traversal options.
    /// </summary>
    public static TraversalOptions Default { get; } = new();

    /// <summary>
    /// Default enumeration options.
    /// </summary>
    internal static readonly EnumerationOptions DefaultEnumerationOptions = new()
    {
        RecurseSubdirectories = true
    };
}

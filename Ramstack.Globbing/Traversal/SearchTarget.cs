namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Specifies the search targets for enumeration.
/// </summary>
[Flags]
internal enum SearchTarget
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

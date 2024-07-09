namespace Ramstack.Globbing;

/// <summary>
/// Defines flags that control the behavior of the glob matching process.
/// </summary>
public enum MatchFlags
{
    /// <summary>
    /// Automatically determines whether to treat backslashes (<c>\</c>) as escape sequences
    /// or path separators based on the platform's separator convention.
    /// </summary>
    Auto,

    /// <summary>
    /// Treats backslashes (<c>\</c>) as path separators instead of escape sequences.
    /// This flag provides behavior consistent with Windows-style paths.
    /// Both backslashes (<c>\</c>) and forward (<c>/</c>) slashes are considered as path separators in this mode.
    /// </summary>
    Windows,

    /// <summary>
    /// Treats backslashes <c>\</c> as escape sequences, allowing for special character escaping.
    /// This flag provides behavior consistent with Unix-style paths.
    /// </summary>
    Unix
}

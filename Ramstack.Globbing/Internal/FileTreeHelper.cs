using System.Buffers;

namespace Ramstack.Globbing.Internal;

/// <summary>
/// Provides helper methods for file tree operations.
/// </summary>
internal static class FileTreeHelper
{
    /// <summary>
    /// Constructs the full name of a file by combining the path and the name.
    /// </summary>
    /// <param name="buffer">A buffer used for constructing the full name.
    /// It should be obtained from an array pool and will be resized if necessary.</param>
    /// <param name="path">The path of the file.</param>
    /// <param name="name">The name of the file.</param>
    /// <returns>
    /// A <see cref="ReadOnlySpan{T}"/> representing the full name of the file.
    /// </returns>
    public static ReadOnlySpan<char> GetFullName(ref char[] buffer, string path, string name)
    {
        var length = path.Length + name.Length + 1;
        if (buffer.Length < length)
        {
            ArrayPool<char>.Shared.Return(buffer);
            buffer = ArrayPool<char>.Shared.Rent(length);
        }

        var fullName = buffer.AsSpan(0, length);

        path.TryCopyTo(fullName);
        fullName[path.Length] = '/';
        name.TryCopyTo(fullName.Slice(path.Length + 1));

        return fullName;
    }
}

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
    /// <param name="chars">A buffer used for constructing the full name.
    /// It should be obtained from an array pool and will be resized if necessary.</param>
    /// <param name="path">The path of the file.</param>
    /// <param name="name">The name of the file.</param>
    /// <returns>
    /// A <see cref="ReadOnlySpan{T}"/> representing the full name of the file.
    /// </returns>
    public static ReadOnlySpan<char> GetFullName(ref char[] chars, string path, string name)
    {
        var array = chars;
        var count = path.Length + name.Length + 1;

        if (array.Length < count)
        {
            ArrayPool<char>.Shared.Return(array);
            array = ArrayPool<char>.Shared.Rent(count);
            chars = array;

            // Force null check to assist JIT
            _ = array.Length;
        }

        var fullName = array.AsSpan();

        path.TryCopyTo(fullName);
        fullName[path.Length] = '/';
        name.TryCopyTo(fullName.Slice(path.Length + 1));

        return fullName.Slice(0, count);
    }
}

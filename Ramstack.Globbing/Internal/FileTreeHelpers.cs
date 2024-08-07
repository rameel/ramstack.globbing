using System.Buffers;

namespace Ramstack.Globbing.Internal;

internal static class FileTreeHelpers
{
    public static ReadOnlySpan<char> GetFullName(ref char[] chars, string path, string name)
    {
        var length = path.Length + name.Length + 1;
        if (chars.Length < length)
        {
            ArrayPool<char>.Shared.Return(chars);
            chars = ArrayPool<char>.Shared.Rent(length);
        }

        var fullName = chars.AsSpan(0, length);

        path.TryCopyTo(fullName);
        fullName[path.Length] = '/';
        name.TryCopyTo(fullName.Slice(path.Length + 1));

        return fullName;
    }
}

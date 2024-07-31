using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ramstack.Globbing.Traversal;

/// <summary>
/// Provides helper methods for path manipulations.
/// </summary>
[SuppressMessage("Usage", "IDE0004:Remove Unnecessary Cast")]
[SuppressMessage("ReSharper", "RedundantCast")]
internal static class PathHelper
{
    /// <summary>
    /// Searches for a path separator within a span of characters.
    /// </summary>
    /// <param name="path">The span of characters representing the path.</param>
    /// <param name="flags">The flags indicating the type of path separators to match.</param>
    /// <returns>
    /// The index of the path separator if found; otherwise, <c>-1</c>.
    /// </returns>
    public static int SearchPathSeparator(scoped ReadOnlySpan<char> path, MatchFlags flags)
    {
        var count = (nint)(uint)path.Length;
        ref var p = ref MemoryMarshal.GetReference(path);
        return SearchPathSeparator(ref p, count, flags);
    }

    /// <summary>
    /// Searches for a path separator within a range of characters.
    /// </summary>
    /// <param name="p">A reference to the first character of the range.</param>
    /// <param name="count">The number of characters to search through.</param>
    /// <param name="flags">The flags indicating the type of path separators to match.</param>
    /// <returns>
    /// The index of the path separator if found; otherwise, <c>-1</c>.
    /// </returns>
    public static int SearchPathSeparator(scoped ref char p, nint count, MatchFlags flags)
    {
        var index = (nint)0;

        if (!Sse41.IsSupported || count < Vector128<ushort>.Count)
        {
            for (; index < count; index++)
                if (Unsafe.Add(ref p, index) == '/'
                    || (Unsafe.Add(ref p, index) == '\\' && flags == MatchFlags.Windows))
                    return (int)index;

            return -1;
        }

        if (!Avx2.IsSupported || count < Vector256<ushort>.Count)
        {
            var slash = Vector128.Create((ushort)'/');
            var backslash = Vector128.Create((ushort)'\\');
            var allowEscaping = CreateAllowEscaping128Bitmask(flags);

            var tail = count - Vector128<ushort>.Count;

            Vector128<ushort> chunk;
            Vector128<ushort> comparison;

            do
            {
                chunk = LoadVector128(ref p, index);
                comparison = Sse2.Or(
                    Sse2.CompareEqual(chunk, slash),
                    Sse2.AndNot(
                        allowEscaping,
                        Sse2.CompareEqual(chunk, backslash)));

                if (!Sse41.TestZ(comparison, comparison))
                {
                    var position = BitOperations.TrailingZeroCount(Sse2.MoveMask(comparison.AsByte()));
                    return (int)index + (position >>> 1);
                }

                index += Vector128<ushort>.Count;
            }
            while (index + Vector128<ushort>.Count <= count);

            chunk = LoadVector128(ref p, tail);
            comparison = Sse2.Or(
                Sse2.CompareEqual(chunk, slash),
                Sse2.AndNot(
                    allowEscaping,
                    Sse2.CompareEqual(chunk, backslash)));

            if (!Sse41.TestZ(comparison, comparison))
            {
                var position = BitOperations.TrailingZeroCount(Sse2.MoveMask(comparison.AsByte()));
                return (int)tail + (position >>> 1);
            }

            return -1;
        }
        else
        {
            var slash = Vector256.Create((ushort)'/');
            var backslash = Vector256.Create((ushort)'\\');
            var allowEscaping = CreateAllowEscaping256Bitmask(flags);
            var tail = count - Vector256<ushort>.Count;

            Vector256<ushort> chunk;
            Vector256<ushort> comparison;

            do
            {
                chunk = LoadVector256(ref p, index);
                comparison = Avx2.Or(
                    Avx2.CompareEqual(chunk, slash),
                    Avx2.AndNot(
                        allowEscaping,
                        Avx2.CompareEqual(chunk, backslash)));

                if (!Avx.TestZ(comparison, comparison))
                {
                    var position = BitOperations.TrailingZeroCount(Avx2.MoveMask(comparison.AsByte()));
                    return (int)index + (position >>> 1);
                }

                index += Vector256<ushort>.Count;
            }
            while (index + Vector256<ushort>.Count <= count);

            chunk = LoadVector256(ref p, tail);
            comparison = Avx2.Or(
                Avx2.CompareEqual(chunk, slash),
                Avx2.AndNot(
                    allowEscaping,
                    Avx2.CompareEqual(chunk, backslash)));

            if (!Avx.TestZ(comparison, comparison))
            {
                var position = BitOperations.TrailingZeroCount(Avx2.MoveMask(comparison.AsByte()));
                return (int)tail + (position >>> 1);
            }

            return -1;
        }
    }

    /// <summary>
    /// Determines whether the specified path matches any of the specified patterns.
    /// </summary>
    /// <param name="path">The path to match for a match.</param>
    /// <param name="patterns">An array of patterns to match against the path.</param>
    /// <param name="flags">The matching options to use.</param>
    /// <returns>
    /// <see langword="true" /> if the path matches any of the patterns;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsMatch(ReadOnlySpan<char> path, string[] patterns, MatchFlags flags)
    {
        foreach (var pattern in patterns)
            if (Matcher.IsMatch(path, pattern, flags))
                return true;

        return false;
    }

    /// <summary>
    /// Determines whether the specified path partially matches any of the specified patterns.
    /// </summary>
    /// <param name="path">The path to be partially matched.</param>
    /// <param name="patterns">An array of patterns to match against the path.</param>
    /// <param name="flags">The matching options to use.</param>
    /// <returns>
    /// <see langword="true" /> if the path partially matches any of the patterns;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsPartialMatch(ReadOnlySpan<char> path, string[] patterns, MatchFlags flags)
    {
        var count = CountPathSegments(path, flags);

        foreach (var pattern in patterns)
            if (Matcher.IsMatch(path, GetPartialPattern(pattern, flags, count), flags))
                return true;

        return false;
    }

    /// <summary>
    /// Counts the number of segments in the specified path.
    /// </summary>
    /// <param name="path">The path to count segments for.</param>
    /// <param name="flags">The flags indicating the type of path separators to match.</param>
    /// <returns>
    /// The number of segments in the path.
    /// </returns>
    public static int CountPathSegments(scoped ReadOnlySpan<char> path, MatchFlags flags)
    {
        var count = 0;
        ref var s = ref Unsafe.AsRef(in MemoryMarshal.GetReference(path));
        var iterator = new PathSegmentIterator(path.Length);

        while (true)
        {
            var r = iterator.GetNext(ref s, flags);
            if (r.start != r.final)
                count++;

            if (r.final == path.Length)
                break;
        }

        if (count == 0)
            count = 1;

        return count;
    }

    /// <summary>
    /// Returns a partial pattern from the specified pattern string based on the specified depth.
    /// </summary>
    /// <param name="pattern">The pattern string to extract from.</param>
    /// <param name="flags">The flags indicating the type of path separators to match.</param>
    /// <param name="depth">The depth level to extract the partial pattern up to.</param>
    /// <returns>
    /// A <see cref="ReadOnlySpan{T}"/> representing the partial pattern.
    /// </returns>
    public static ReadOnlySpan<char> GetPartialPattern(string pattern, MatchFlags flags, int depth)
    {
        Debug.Assert(depth >= 1);

        if (depth < 1)
            depth = 1;

        ref var s = ref Unsafe.AsRef(in pattern.GetPinnableReference());
        var iterator = new PathSegmentIterator(pattern.Length);

        while (true)
        {
            var r = iterator.GetNext(ref s, flags);
            if (r.start != r.final)
                depth--;

            if (depth < 1
                || r.final == pattern.Length
                || IsGlobStar(ref s, r.start, r.final))
                return MemoryMarshal.CreateReadOnlySpan(ref s, r.final);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsGlobStar(ref char s, int index, int final) =>
            index + 2 == final && Unsafe.ReadUnaligned<int>(
                ref Unsafe.As<char, byte>(
                    ref Unsafe.Add(ref s, (nint)(uint)index))) == ('*' << 16 | '*');
    }

    /// <summary>
    /// Converts path separators in the specified span to the Unix style (/).
    /// </summary>
    /// <param name="path">The path to convert.</param>
    public static void ConvertPathToPosixStyle(Span<char> path)
    {
        var length = (nint)(uint)path.Length;
        ref var reference = ref MemoryMarshal.GetReference(path);
        ConvertPathToPosixStyleImpl(ref reference, length);

        static void ConvertPathToPosixStyleImpl(ref char p, nint length)
        {
            var i = (nint)0;

            // The main reason for using our own implementation is that the method
            // Replace(this Span<char> span, char oldValue, char newValue) is only available
            // starting from .NET 8. Since we need to support earlier versions of .NET,
            // we are using our own implementation.

            if (Sse41.IsSupported && length >= Vector128<ushort>.Count)
            {
                Vector128<ushort> value;
                Vector128<ushort> mask;
                Vector128<ushort> result;

                var slash = Vector128.Create((ushort)'/');
                var backslash = Vector128.Create((ushort)'\\');
                var tail = length - Vector128<ushort>.Count;

                // +------+------+------+------+------+------+---+ DATA
                //                                        +------+ TAIL
                //
                // After the main loop, only one final vector operation
                // is needed for the 'tail' block.

                do
                {
                    value = LoadVector128(ref p, i);
                    mask = Sse2.CompareEqual(value, backslash);
                    result = Sse41.BlendVariable(value, slash, mask);
                    WriteVector128(ref p, i, result);

                    i += Vector128<ushort>.Count;
                }
                while (i < tail);

                // Process remaining chars
                // NOTE: An extra one write for the 'length == Vector128<ushort>.Count'

                value = LoadVector128(ref p, tail);
                mask = Sse2.CompareEqual(value, backslash);
                result = Sse41.BlendVariable(value, slash, mask);
                WriteVector128(ref p, tail, result);
            }
            else
            {
                for (; i < length; i++)
                    if (Unsafe.Add(ref p, i) == '\\')
                        Unsafe.Add(ref p, i) = '/';
            }
        }
    }

    #region Vector helper methods

    /// <summary>
    /// Creates a 256-bit bitmask that allows escaping characters based on the specified flags.
    /// </summary>
    /// <param name="flags">The flags indicating the type of path separators to match.</param>
    /// <returns>
    /// A 256-bit bitmask for escaping characters.
    /// </returns>
    private static Vector256<ushort> CreateAllowEscaping256Bitmask(MatchFlags flags)
    {
        // Here is a small trick to avoid branching.
        // To reduce the number of required instructions, we convert the value `Windows`,
        // which equals 2, into a bitmask that allows escaping characters.
        // Windows (2) (No character escaping):
        //                0000 0010 >> 1        = 0000 0001
        //                0000 0001 & 0000 0001 = 0000 0001
        //                0000 0001 - 1         = 0000 0000
        // Any other value will simply convert to 0.
        // Unix    (4) (Allow escaping characters)
        //                0000 0100 >> 1        = 0000 0010
        //                0000 0010 & 0000 0001 = 0000 0000
        //                0000 0000 - 1         = 1111 1111
        // Next, during the check, we can simply use the Avx2.AndNot instruction instead of Avx2.And:
        //    Avx2.AndNot(
        //        allowEscaping,
        //        Avx2.CompareEqual(chunk, backslash)))
        Debug.Assert(MatchFlags.Windows == (MatchFlags)2);
        return Vector256.Create(((uint)flags >> 1 & 1) - 1).AsUInt16();
    }

    /// <summary>
    /// Creates a 128-bit bitmask that allows escaping characters based on the specified flags.
    /// </summary>
    /// <param name="flags">The flags indicating the type of path separators to match.</param>
    /// <returns>
    /// A 128-bit bitmask for escaping characters.
    /// </returns>
    private static Vector128<ushort> CreateAllowEscaping128Bitmask(MatchFlags flags)
    {
        Debug.Assert(MatchFlags.Windows == (MatchFlags)2);
        return Vector128.Create(((uint)flags >> 1 & 1) - 1).AsUInt16();
    }

    /// <summary>
    /// Loads a 256-bit vector from the specified source.
    /// </summary>
    /// <param name="source">The source from which the vector will be loaded.</param>
    /// <param name="offset">The offset from source from which the vector will be loaded.</param>
    /// <returns>
    /// The loaded 256-bit vector.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ushort> LoadVector256(ref char source, nint offset) =>
        Unsafe.ReadUnaligned<Vector256<ushort>>(
            ref Unsafe.As<char, byte>(ref Unsafe.Add(ref source, offset)));

    /// <summary>
    /// Loads a 128-bit vector from the specified source.
    /// </summary>
    /// <param name="source">The source from which the vector will be loaded.</param>
    /// <param name="offset">The offset from source from which the vector will be loaded.</param>
    /// <returns>
    /// The loaded 128-bit vector.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ushort> LoadVector128(ref char source, nint offset) =>
        Unsafe.ReadUnaligned<Vector128<ushort>>(
            ref Unsafe.As<char, byte>(
                ref Unsafe.Add(ref source, offset)));

    /// <summary>
    /// Stores a 128-bit vector at the specified destination.
    /// </summary>
    /// <param name="destination">The destination at which the vector will be stored.</param>
    /// <param name="offset">The element offset from destination from which the vector will be stored.</param>
    /// <param name="value">The vector that will be stored.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteVector128(ref char destination, nint offset, Vector128<ushort> value) =>
        Unsafe.WriteUnaligned(
            ref Unsafe.As<char, byte>(ref Unsafe.Add(ref destination, offset)),
            value);

    #endregion

    #region Inner type: PathSegmentIterator

    /// <summary>
    /// Provides functionality to iterate over segments of a path.
    /// </summary>
    private struct PathSegmentIterator
    {
        private nint _last;
        private nint _position;
        private uint _mask;
        private readonly nint _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathSegmentIterator"/> structure.
        /// </summary>
        /// <param name="length">The path length.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PathSegmentIterator(int length) =>
            (_last, _length) = (-1, (nint)(uint)length);

        /// <summary>
        /// Retrieves the next segment of the path.
        /// </summary>
        /// <param name="source">A reference to the starting character of the path.</param>
        /// <param name="flags">The flags indicating the type of path separators to match.</param>
        /// <returns>
        /// A tuple containing the start and end indices of the next path segment.
        /// <c>start</c> indicates the beginning of the segment, and <c>final</c> satisfies
        /// the condition that <c>final - start</c> equals the length of the segment.
        /// The end of the iteration is indicated by <c>final</c> being equal to the length of the path.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int start, int final) GetNext(ref char source, MatchFlags flags)
        {
            var start = _last + 1;

            while (_position < _length)
            {
                if (_mask != 0)
                {
                    var offset = BitOperations.TrailingZeroCount(_mask);
                    _last = _position + (nint)((uint)offset >> 1);
                    _mask &= ~(3u << offset);

                    if (_mask == 0)
                        _position += Vector256<ushort>.Count;

                    return ((int)start, (int)_last);
                }

                if (_position + Vector256<ushort>.Count <= _length)
                {
                    var chunk = LoadVector256(ref source, _position);
                    var allowEscapingMask = CreateAllowEscaping256Bitmask(flags);
                    var slash = Vector256.Create((ushort)'/');
                    var backslash = Vector256.Create((ushort)'\\');

                    var comparison = Avx2.Or(
                        Avx2.CompareEqual(chunk, slash),
                        Avx2.AndNot(
                            allowEscapingMask,
                            Avx2.CompareEqual(chunk, backslash)));

                    _mask = (uint)Avx2.MoveMask(comparison.AsByte());
                    if (_mask == 0)
                        _position += Vector256<ushort>.Count;
                }
                else
                {
                    for (; _position < _length; _position++)
                    {
                        var ch = Unsafe.Add(ref source, _position);
                        if (ch == '/' || (ch == '\\' && flags == MatchFlags.Windows))
                        {
                            _last = _position;
                            _position++;
                            return ((int)start, (int)_last);
                        }
                    }
                }
            }

            return ((int)start, (int)_length);
        }
    }

    #endregion
}

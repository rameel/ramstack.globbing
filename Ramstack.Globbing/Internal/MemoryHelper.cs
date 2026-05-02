using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace Ramstack.Globbing.Internal;

/// <summary>
/// Provides low-level memory scanning helpers for UTF-16 character buffers.
/// </summary>
internal static unsafe class MemoryHelper
{
    /// <summary>
    /// Searches for the first occurrence of a character in a UTF-16 buffer.
    /// </summary>
    /// <param name="s">Pointer to the start of the buffer.</param>
    /// <param name="e">Pointer to one-past-the-end of the buffer.</param>
    /// <param name="ch">The character to search for.</param>
    /// <returns>
    /// The zero-based index of the first occurrence of <paramref name="ch"/>, or <c>-1</c> if not found.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf(char* s, char* e, char ch)
    {
        var i = 0;

        if (Sse2.IsSupported && s + Vector128<short>.Count <= e)
        {
            for (;;)
            {
                var result = Sse2.CompareEqual(
                    Vector128.Create((short)ch),
                    LoadVector(s));

                var mask = Sse2.MoveMask(result.AsByte());
                if (mask != 0)
                {
                    var offset = BitOperations.TrailingZeroCount(mask) >>> 1;
                    return i + offset;
                }

                s += Vector128<short>.Count;
                i += Vector128<short>.Count;

                if (s + Vector128<short>.Count <= e)
                    continue;

                if (s == e)
                    return -1;

                //
                // Tail handling via the same SIMD path (no scalar fallback)
                //
                var remaining = (int)((nint)e - (nint)s) >>> 1;
                i = i + remaining - Vector128<short>.Count;
                s = e - Vector128<short>.Count;
            }
        }

        if (AdvSimd.Arm64.IsSupported && s + Vector128<short>.Count <= e)
        {
            for (;;)
            {
                var result = AdvSimd.CompareEqual(
                    Vector128.Create((short)ch),
                    LoadVector(s));

                var mask = AdvSimd_ExtractMostSignificantBits(result);
                if (mask != 0)
                {
                    var offset = BitOperations.TrailingZeroCount(mask);
                    return i + offset;
                }

                s += Vector128<short>.Count;
                i += Vector128<short>.Count;

                if (s + Vector128<short>.Count <= e)
                    continue;

                if (s == e)
                    return -1;

                //
                // Tail handling via the same SIMD path (no scalar fallback)
                //
                var remaining = (int)((nint)e - (nint)s) >>> 1;
                i = i + remaining - Vector128<short>.Count;
                s = e - Vector128<short>.Count;
            }
        }

        for (; s < e; s++, i++)
            if (*s == ch)
                return i;

        return -1;
    }

    /// <summary>
    /// Searches for the first occurrence of either of two characters in a UTF-16 buffer.
    /// </summary>
    /// <param name="s">Pointer to the start of the buffer.</param>
    /// <param name="e">Pointer to one-past-the-end of the buffer.</param>
    /// <param name="ch1">First character to search for.</param>
    /// <param name="ch2">Second character to search for.</param>
    /// <returns>
    /// The zero-based index of the first occurrence of either character, or <c>-1</c> if neither is found.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfAny(char* s, char* e, char ch1, char ch2)
    {
        var i = 0;

        if (Sse2.IsSupported && s + Vector128<short>.Count <= e)
        {
            for (;;)
            {
                var source = LoadVector(s);
                var result = Sse2.Or(
                    Sse2.CompareEqual(source, Vector128.Create((short)ch1)),
                    Sse2.CompareEqual(source, Vector128.Create((short)ch2))
                ).AsByte();

                var mask = Sse2.MoveMask(result);
                if (mask != 0)
                {
                    var offset = BitOperations.TrailingZeroCount(mask) >>> 1;
                    return i + offset;
                }

                s += Vector128<short>.Count;
                i += Vector128<short>.Count;

                if (s + Vector128<short>.Count <= e)
                    continue;

                if (s == e)
                    return -1;

                //
                // Tail handling via the same SIMD path (no scalar fallback)
                //
                var remaining = (int)((nint)e - (nint)s) >>> 1;
                i = i + remaining - Vector128<short>.Count;
                s = e - Vector128<short>.Count;
            }
        }

        if (AdvSimd.Arm64.IsSupported && s + Vector128<short>.Count <= e)
        {
            for (;;)
            {
                var source = LoadVector(s);
                var result = AdvSimd.Or(
                    AdvSimd.CompareEqual(source, Vector128.Create((short)ch1)),
                    AdvSimd.CompareEqual(source, Vector128.Create((short)ch2)));

                var mask = AdvSimd_ExtractMostSignificantBits(result);
                if (mask != 0)
                {
                    var offset = BitOperations.TrailingZeroCount(mask);
                    return i + offset;
                }

                s += Vector128<short>.Count;
                i += Vector128<short>.Count;

                if (s + Vector128<short>.Count <= e)
                    continue;

                if (s == e)
                    return -1;

                //
                // Tail handling via the same SIMD path (no scalar fallback)
                //
                var remaining = (int)((nint)e - (nint)s) >>> 1;
                i = i + remaining - Vector128<short>.Count;
                s = e - Vector128<short>.Count;
            }
        }

        for (; s < e; s++, i++)
            if (*s == ch1 || *s == ch2)
                return i;

        return -1;
    }

    /// <summary>
    /// Extracts a bitmask representing the most significant bits of each 16-bit lane in a SIMD vector.
    /// </summary>
    /// <param name="v">Input vector of 16-bit integers.</param>
    /// <returns>
    /// A bitmask encoding which lanes have their high bit set.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int AdvSimd_ExtractMostSignificantBits(Vector128<short> v)
    {
        var sum = AdvSimd.Arm64.AddAcross(
            AdvSimd.ShiftLogical(
                AdvSimd.And(v, Vector128.Create(unchecked((short)0x8000))),
                Vector128.Create(-15, -14, -13, -12, -11, -10, -9, -8)));
        return sum.ToScalar();
    }

    /// <summary>
    /// Loads a 128-bit vector of UTF-16 characters.
    /// </summary>
    /// <param name="source">Pointer to the source memory location.</param>
    /// <returns>
    /// A vector containing 8 UTF-16 characters.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<short> LoadVector(void* source) =>
        Unsafe.ReadUnaligned<Vector128<short>>(source);
}

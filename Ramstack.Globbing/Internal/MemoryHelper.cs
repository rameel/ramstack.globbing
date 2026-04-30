using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ramstack.Globbing.Internal;

internal static unsafe class MemoryHelper
{
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

        for (; s < e; s++, i++)
            if (*s == ch)
                return i;

        return -1;
    }

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

        for (; s < e; s++, i++)
            if (*s == ch1 || *s == ch2)
                return i;

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<short> LoadVector(void* source) =>
        Unsafe.ReadUnaligned<Vector128<short>>(source);
}

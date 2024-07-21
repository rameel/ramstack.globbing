using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ramstack.Globbing.Internal;

internal static class PathHelper
{
    public static void ToForwardSlashed(Span<char> value)
    {
        var length = (nint)(uint)value.Length;
        ref var reference = ref Unsafe.As<char, ushort>(
            ref MemoryMarshal.GetReference(value));

        ReplaceImpl(ref reference, length);
    }

    [SuppressMessage("ReSharper", "RedundantCast")]
    private static void ReplaceImpl(ref ushort p, nint length)
    {
        var i = (nint)0;

        if (Sse41.IsSupported && length >= Vector128<ushort>.Count)
        {
            var slash = Vector128.Create((ushort)'/');
            var backslash = Vector128.Create((ushort)'\\');
            var tail = length - Vector128<ushort>.Count;

            Vector128<ushort> value;
            Vector128<ushort> mask;
            Vector128<ushort> result;

            while (i < tail)
            {
                value = LoadVector(ref p, i);
                mask = Sse2.CompareEqual(value, backslash);
                result = Sse41.BlendVariable(value, slash, mask);
                WriteVector(ref p, i, result);

                i += Vector128<ushort>.Count;
            }

            value = LoadVector(ref p, tail);
            mask = Sse2.CompareEqual(value, backslash);
            result = Sse41.BlendVariable(value, slash, mask);
            WriteVector(ref p, tail, result);
        }
        //else if (AdvSimd.IsSupported && length >= Vector128<ushort>.Count)
        //{
        //    var slash = Vector128.Create((ushort)'/');
        //    var backslash = Vector128.Create((ushort)'\\');
        //    var tail = length - Vector128<ushort>.Count;

        //    Vector128<ushort> value;
        //    Vector128<ushort> mask;
        //    Vector128<ushort> result;

        //    while (i < tail)
        //    {
        //        value = LoadVector(ref p, i);
        //        mask = AdvSimd.CompareEqual(value, backslash);
        //        result = AdvSimd.BitwiseSelect(mask, slash, value);
        //        WriteVector(ref p, i, result);

        //        i += Vector128<ushort>.Count;
        //    }

        //    value = LoadVector(ref p, tail);
        //    mask = AdvSimd.CompareEqual(value, backslash);
        //    result = AdvSimd.BitwiseSelect(mask, slash, value);
        //    WriteVector(ref p, tail, result);
        //}
        else
        {
            for (; i < length; i++)
                if (Unsafe.Add(ref p, i) == '\\')
                    Unsafe.Add(ref p, i) = '/';
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ushort> LoadVector(ref ushort p, nint offset) =>
        Unsafe.ReadUnaligned<Vector128<ushort>>(
            ref Unsafe.As<ushort, byte>(ref Unsafe.Add(ref p, offset)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteVector(ref ushort p, nint offset, Vector128<ushort> value) =>
        Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref Unsafe.Add(ref p, offset)), value);
}

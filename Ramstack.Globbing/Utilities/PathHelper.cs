using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace Ramstack.Globbing.Utilities;

/// <summary>
/// Provides helper methods for path manipulations.
/// </summary>
internal static class PathHelper
{
    /// <summary>
    /// Counts the number of segments in the given path.
    /// </summary>
    /// <param name="path">The path to count segments for.</param>
    /// <param name="flags">The match flags that may influence the counting.</param>
    /// <returns>
    /// The number of segments in the path.
    /// </returns>
    public static int CountPathSegments(ReadOnlySpan<char> path, MatchFlags flags)
    {
        ref var s = ref Unsafe.AsRef(in path.GetPinnableReference());
        ref var e = ref Unsafe.Add(ref s, (uint)path.Length);

        while (Unsafe.IsAddressLessThan(ref s, ref e) && (s == '/' || (s == '\\' && flags == MatchFlags.Windows)))
            s = ref Unsafe.Add(ref s, 1);

        var count = 1;
        var separator = false;

        for (; Unsafe.IsAddressLessThan(ref s, ref e); s = ref Unsafe.Add(ref s, 1))
        {
            if (s == '/' || (s == '\\' && flags == MatchFlags.Windows))
            {
                separator = true;
            }
            else if (separator)
            {
                separator = false;
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Returns a partial pattern from the specified pattern string based on the specified depth.
    /// </summary>
    /// <param name="pattern">The pattern string to extract from.</param>
    /// <param name="flags">The match flags that may influence the extraction.</param>
    /// <param name="depth">The depth level to extract the partial pattern up to.</param>
    /// <returns>
    /// A <see cref="ReadOnlySpan{T}"/> representing the partial pattern.
    /// </returns>
    public static ReadOnlySpan<char> GetPartialPattern(string pattern, MatchFlags flags, int depth)
    {
        ref var s = ref Unsafe.AsRef(in pattern.GetPinnableReference());
        ref var e = ref Unsafe.Add(ref s, pattern.Length);

        while (Unsafe.IsAddressLessThan(ref s, ref e) && (s == '/' || (s == '\\' && flags == MatchFlags.Windows)))
            s = ref Unsafe.Add(ref s, 1);

        var separator = true;
        var i = (nint)0;

        for (; Unsafe.IsAddressLessThan(ref Unsafe.Add(ref s, i), ref e); i++)
        {
            var ch = Unsafe.Add(ref s, i);
            if (ch == '/' || (ch == '\\' && flags == MatchFlags.Windows))
            {
                separator = true;
                if (depth == 0)
                    break;
            }
            else if (separator)
            {
                separator = false;
                depth--;

                if (Unsafe.As<char, int>(ref Unsafe.Add(ref s, i)) == ('*' << 16 | '*'))
                {
                    var c = Unsafe.Add(ref s, i + 2);
                    if (c == '/' || (c == '\\' && flags == MatchFlags.Windows) || i + 2 >= pattern.Length)
                    {
                        i += 2;
                        break;
                    }
                }
            }
        }

        return MemoryMarshal.CreateReadOnlySpan(ref s, (int)i);
    }

    /// <summary>
    /// Converts all backslashes in the specified span of characters to forward slashes.
    /// </summary>
    /// <param name="value">The span of characters to modify.</param>
    public static void ConvertToForwardSlashes(scoped Span<char> value)
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

        // The main reason for using our own implementation is that the method
        // Replace(this Span<char> span, char oldValue, char newValue) is only available
        // starting from .NET 8. Since we need to support earlier versions of .NET,
        // we are using our own implementation.
        //
        // We are limiting ourselves to 128-bit vector registers in this implementation.
        // Path lengths are typically small, so there is no significant benefit
        // in using 256-bit or 512-bit vector instructions. In addition, including
        // support for larger vector sizes would complicate the code without providing
        // noticeable performance improvements.

        if (Sse41.IsSupported && length >= Vector128<ushort>.Count)
        {
            var slash = Vector128.Create((ushort)'/');
            var backslash = Vector128.Create((ushort)'\\');
            var tail = length - Vector128<ushort>.Count;

            Vector128<ushort> value;
            Vector128<ushort> mask;
            Vector128<ushort> result;

            for (; i < tail; i += Vector128<ushort>.Count)
            {
                value = LoadVector(ref p, i);
                mask = CompareEqual(value, backslash);
                result = ConditionalSelect(value, slash, mask);
                WriteVector(ref p, i, result);
            }

            value = LoadVector(ref p, tail);
            mask = CompareEqual(value, backslash);
            result = ConditionalSelect(value, slash, mask);
            WriteVector(ref p, tail, result);
        }
        else
        {
            for (; i < length; i++)
                if (Unsafe.Add(ref p, i) == '\\')
                    Unsafe.Add(ref p, i) = '/';
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ushort> CompareEqual(Vector128<ushort> left, Vector128<ushort> right)
    {
        if (Sse41.IsSupported)
            return Sse2.CompareEqual(left, right);

        // TODO Test
        return AdvSimd.CompareEqual(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ushort> ConditionalSelect(Vector128<ushort> left, Vector128<ushort> right, Vector128<ushort> mask)
    {
        if (Sse41.IsSupported)
            return Sse41.BlendVariable(left, right, mask);

        // TODO Test
        return AdvSimd.BitwiseSelect(mask, left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ushort> LoadVector(ref ushort p, nint offset) =>
        Unsafe.ReadUnaligned<Vector128<ushort>>(
            ref Unsafe.As<ushort, byte>(ref Unsafe.Add(ref p, offset)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteVector(ref ushort p, nint offset, Vector128<ushort> value) =>
        Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref Unsafe.Add(ref p, offset)), value);
}

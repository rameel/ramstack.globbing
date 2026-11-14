using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Ramstack.Globbing;

/// <summary>
/// Represents a text buffer located at the very end of a readable memory page
/// that is immediately followed by an inaccessible (guard) page, so any read past
/// the end of the text results in an access violation instead of silently reading
/// neighboring memory.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed unsafe class GuardedBuffer : IDisposable
{
    // libc: mmap/mprotect/munmap
    private const int PROT_NONE = 0;
    private const int PROT_READ = 1;
    private const int PROT_WRITE = 2;
    private const int MAP_PRIVATE = 0x02;
    private const int MAP_ANON_LINUX = 0x20;
    private const int MAP_ANON_MACOS = 0x1000;

    // winnt.h
    private const uint MEM_COMMIT = 0x1000;
    private const uint MEM_RESERVE = 0x2000;
    private const uint MEM_RELEASE = 0x8000;
    private const uint PAGE_NOACCESS = 0x01;
    private const uint PAGE_READWRITE = 0x04;

    private readonly byte* _memory;
    private readonly nuint _size;
    private readonly char* _text;
    private readonly int _length;

    public static bool IsSupported =>
        OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsWindows();

    public ReadOnlySpan<char> Span => new(_text, _length);

    public GuardedBuffer(string text)
    {
        var page = (nuint)Environment.SystemPageSize;
        var data = ((nuint)text.Length * sizeof(char) + page - 1) / page * page;

        if (data == 0)
            data = page;

        _size = data + page;
        _memory = Allocate(_size);

        Protect(_memory + data, page);

        _length = text.Length;
        _text = (char*)(_memory + data) - _length;

        text.AsSpan().CopyTo(new Span<char>(_text, _length));
    }

    public void Dispose()
    {
        var released = OperatingSystem.IsWindows()
            ? VirtualFree(_memory, 0, MEM_RELEASE)
            : munmap(_memory, _size) == 0;

        if (!released)
            throw new InvalidOperationException($"Failed to release memory: {Marshal.GetLastWin32Error()}");
    }

    private static byte* Allocate(nuint size)
    {
        if (OperatingSystem.IsWindows())
        {
            var p = VirtualAlloc(null, size, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (p == null)
                throw new InvalidOperationException($"VirtualAlloc failed: {Marshal.GetLastWin32Error()}");

            return (byte*)p;
        }
        else
        {
            var flags = MAP_PRIVATE | (OperatingSystem.IsMacOS() ? MAP_ANON_MACOS : MAP_ANON_LINUX);
            var p = mmap(null, size, PROT_READ | PROT_WRITE, flags, -1, 0);
            if (p == (void*)-1)
                throw new InvalidOperationException($"mmap failed: {Marshal.GetLastWin32Error()}");

            return (byte*)p;
        }
    }

    private static void Protect(byte* address, nuint size)
    {
        if (OperatingSystem.IsWindows())
        {
            if (!VirtualProtect(address, size, PAGE_NOACCESS, out _))
                throw new InvalidOperationException($"VirtualProtect failed: {Marshal.GetLastWin32Error()}");
        }
        else
        {
            if (mprotect(address, size, PROT_NONE) != 0)
                throw new InvalidOperationException($"mprotect failed: {Marshal.GetLastWin32Error()}");
        }
    }

    [DllImport("libc", SetLastError = true)]
    private static extern void* mmap(void* addr, nuint length, int prot, int flags, int fd, nint offset);

    [DllImport("libc", SetLastError = true)]
    private static extern int mprotect(void* addr, nuint length, int prot);

    [DllImport("libc", SetLastError = true)]
    private static extern int munmap(void* addr, nuint length);

    [DllImport("kernel32", SetLastError = true)]
    private static extern void* VirtualAlloc(void* address, nuint size, uint allocationType, uint protect);

    [DllImport("kernel32", SetLastError = true)]
    private static extern bool VirtualProtect(void* address, nuint size, uint newProtect, out uint oldProtect);

    [DllImport("kernel32", SetLastError = true)]
    private static extern bool VirtualFree(void* address, nuint size, uint freeType);
}

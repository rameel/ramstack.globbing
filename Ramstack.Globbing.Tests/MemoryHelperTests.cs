using Ramstack.Globbing.Internal;

namespace Ramstack.Globbing;

[TestFixture]
public unsafe class MemoryHelperTests
{
    [Test]
    public void IndexOf_Char()
    {
        Span<char> buffer = stackalloc char[64];

        while (buffer.Length != 0)
        {
            Fill(buffer);

            var s = (char*)Unsafe.AsPointer(ref buffer[0]);
            Assert.That(
                MemoryHelper.IndexOf(s, s + buffer.Length, '*'),
                Is.EqualTo(-1));

            for (var i = 0; i < buffer.Length; i++)
            {
                Fill(buffer);
                buffer[i] = '*';

                Assert.That(
                    MemoryHelper.IndexOf(s, s + buffer.Length, '*'),
                    Is.EqualTo(i));
            }

            buffer = buffer[1..];
        }
    }

    [Test]
    public void IndexOfAny_CharChar()
    {
        Span<char> buffer = stackalloc char[64];

        while (buffer.Length != 0)
        {
            Fill(buffer);
            var s = (char*)Unsafe.AsPointer(ref buffer[0]);

            Assert.That(
                MemoryHelper.IndexOfAny(s, s + buffer.Length, '*', '?'),
                Is.EqualTo(-1));

            Assert.That(
                MemoryHelper.IndexOfAny(s, s + buffer.Length, '?', '*'),
                Is.EqualTo(-1));

            foreach (var needle in "*?")
            {
                for (var i = 0; i < buffer.Length; i++)
                {
                    Fill(buffer);
                    buffer[i] = needle;

                    Assert.That(
                        MemoryHelper.IndexOfAny(s, s + buffer.Length, '*', '?'),
                        Is.EqualTo(i));

                    Assert.That(
                        MemoryHelper.IndexOfAny(s, s + buffer.Length, '?', '*'),
                        Is.EqualTo(i));
                }
            }

            if (buffer.Length > 2)
            {
                foreach (var needle in new[] { "*?", "?*" })
                {
                    for (var i = 0; i < buffer.Length - 1; i++)
                    {
                        Fill(buffer);
                        buffer[i + 0] = needle[0];
                        buffer[i + 1] = needle[1];

                        var r = MemoryHelper.IndexOfAny(s, s + buffer.Length, '*', '?');
                        Assert.That(r, Is.EqualTo(i));

                        var p = MemoryHelper.IndexOfAny(s, s + buffer.Length, '?', '*');
                        Assert.That(p, Is.EqualTo(i));
                    }
                }
            }

            buffer = buffer[1..];
        }
    }

    private static void Fill(Span<char> s)
    {
        for (var i = 0; i < s.Length; i++)
        {
            while (true)
            {
                var v = Random.Shared.Next(32, 127);
                if (v is '*' or '?' or '{' or '}' or '[' or '\\')
                    continue;

                s[i] = (char)v;
                break;
            }
        }
    }
}

namespace Ramstack.Globbing.Utilities;

[TestFixture]
public class PathHelperTests
{
    [Test]
    public void ToForwardSlashed_NothingChange()
    {
        for (var n = 0; n < 512; n++)
        {
            var original = new string('@', n + 16).ToCharArray().AsSpan();
            var span = original[8..^8];
            span.Fill('a');

            var expected = original.ToString();
            PathHelper.ToForwardSlashed(span);

            Assert.That(original.ToString(), Is.EqualTo(expected));
        }
    }

    [Test]
    public void ToForwardSlashed_ChangesAll()
    {
        for (var n = 0; n < 512; n++)
        {
            var original = new string('@', n + 16).ToCharArray().AsSpan();
            var span = original[8..^8];
            span.Fill('\\');

            var expected = original.ToString().Replace('\\', '/');
            PathHelper.ToForwardSlashed(span);

            Assert.That(original.ToString(), Is.EqualTo(expected));
        }
    }

    [Test]
    public void ToForwardSlashed_ForwardSlahes()
    {
        for (var n = 0; n < 512; n++)
        {
            var original = new string('@', n + 16).ToCharArray().AsSpan();
            var span = original[8..^8];
            span.Fill('/');

            var expected = original.ToString().Replace('\\', '/');
            PathHelper.ToForwardSlashed(span);

            Assert.That(original.ToString(), Is.EqualTo(expected));
        }
    }

    [Test]
    public void ToForwardSlashed_RareChanges()
    {
        for (var n = 0; n < 512; n++)
        {
            var original = new string('@', n + 16).ToCharArray().AsSpan();
            var span = original[8..^8];
            for (var i = 0; i < span.Length; i++)
                if (i % 7 == 0)
                    span[i] = '\\';

            var expected = original.ToString().Replace('\\', '/');
            PathHelper.ToForwardSlashed(span);

            Assert.That(original.ToString(), Is.EqualTo(expected));
        }
    }
}

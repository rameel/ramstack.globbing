namespace Ramstack.Globbing.Utilities;

[TestFixture]
public partial class PathHelperTests
{
    [TestCase("", 1)]
    [TestCase("//", 1)]
    [TestCase("/dir1", 1)]
    [TestCase("dir1", 1)]
    [TestCase("/dir1/dir2/", 2)]
    [TestCase("dir1/dir2", 2)]
    [TestCase("dir1/dir2/", 2)]
    [TestCase("///dir1/dir2////", 2)]
    public void CountPathSegmentsTests(string path, int expected)
    {
        Assert.That(
            PathHelper.CountPathSegments(path, MatchFlags.Windows),
            Is.EqualTo(expected));

        Assert.That(
            PathHelper.CountPathSegments(path, MatchFlags.Unix),
            Is.EqualTo(expected));

        Assert.That(
            PathHelper.CountPathSegments(path.Replace('/', '\\'), MatchFlags.Windows),
            Is.EqualTo(expected));
    }
}

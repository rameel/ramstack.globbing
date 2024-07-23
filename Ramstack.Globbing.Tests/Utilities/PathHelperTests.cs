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

    [TestCase("/dir1/dir2/", 1, "/dir1")]
    [TestCase("/dir1/dir2/", 2, "/dir1/dir2")]
    [TestCase("/dir1/dir2/", 3, "/dir1/dir2/")]
    [TestCase("/dir1/dir2/", 9, "/dir1/dir2/")]
    [TestCase("dir1/dir2/", 1, "dir1")]
    [TestCase("dir1/dir2/", 2, "dir1/dir2")]
    [TestCase("dir1/dir2/", 3, "dir1/dir2/")]
    [TestCase("dir1/dir2/", 9, "dir1/dir2/")]
    [TestCase("dir1/dir2", 1, "dir1")]
    [TestCase("dir1/dir2", 2, "dir1/dir2")]
    [TestCase("dir1/dir2", 3, "dir1/dir2")]
    [TestCase("dir1/dir2", 9, "dir1/dir2")]
    [TestCase("/1/2/3/4/5/6/7/8/", 1, "/1")]
    [TestCase("/1/2/3/4/5/6/7/8/", 2, "/1/2")]
    [TestCase("/1/2/3/4/5/6/7/8/", 3, "/1/2/3")]
    [TestCase("/1/2/3/4/5/6/7/8/", 4, "/1/2/3/4")]
    [TestCase("/1/2/3/4/5/6/7/8/", 5, "/1/2/3/4/5")]
    [TestCase("/1/2/3/4/5/6/7/8/", 6, "/1/2/3/4/5/6")]
    [TestCase("/1/2/3/4/5/6/7/8/", 7, "/1/2/3/4/5/6/7")]
    [TestCase("/1/2/3/4/5/6/7/8/", 8, "/1/2/3/4/5/6/7/8")]
    [TestCase("/1/2/3/4/5/6/7/8/", 9, "/1/2/3/4/5/6/7/8/")]
    [TestCase("/1/2/3/4/5/6/7/8/", 10, "/1/2/3/4/5/6/7/8/")]
    [TestCase("", 1, "")]
    [TestCase("", 2, "")]
    [TestCase("", 3, "")]
    [TestCase("/", 1, "/")]
    [TestCase("/", 2, "/")]
    [TestCase("/", 3, "/")]
    [TestCase("////", 2, "////")]
    [TestCase("////dir1/dir2////", 1, "////dir1")]
    [TestCase("////dir1/dir2////", 2, "////dir1/dir2")]
    [TestCase("////dir1/dir2////", 3, "////dir1/dir2////")]
    [TestCase("**", 1, "**")]
    [TestCase("**", 2, "**")]
    [TestCase("**", 3, "**")]
    [TestCase("/**", 1, "/**")]
    [TestCase("/**", 2, "/**")]
    [TestCase("/**", 3, "/**")]
    [TestCase("**/", 1, "**")]
    [TestCase("**/", 2, "**")]
    [TestCase("**/", 3, "**")]
    [TestCase("/**/", 1, "/**")]
    [TestCase("/**/", 2, "/**")]
    [TestCase("/**/", 3, "/**")]
    [TestCase("**/dir1/dir2", 1, "**")]
    [TestCase("**/dir1/dir2", 2, "**")]
    [TestCase("**/dir1/dir2", 3, "**")]
    [TestCase("**/dir1/dir2", 4, "**")]
    [TestCase("/**/dir1/dir2", 1, "/**")]
    [TestCase("/**/dir1/dir2", 2, "/**")]
    [TestCase("/**/dir1/dir2", 3, "/**")]
    [TestCase("/**/dir1/dir2", 4, "/**")]
    [TestCase("dir1/**/dir2/dir3", 1, "dir1")]
    [TestCase("dir1/**/dir2/dir3", 2, "dir1/**")]
    [TestCase("dir1/**/dir2/dir3", 3, "dir1/**")]
    [TestCase("dir1/**/dir2/dir3", 4, "dir1/**")]
    public void GetPartialPatternTests(string path, int depth, string expected)
    {
        Assert.That(
            PathHelper.GetPartialPattern(path, MatchFlags.Windows, depth).ToString(),
            Is.EqualTo(expected));

        Assert.That(
            PathHelper.GetPartialPattern(path, MatchFlags.Unix, depth).ToString(),
            Is.EqualTo(expected));

        path = path.Replace('/', '\\');
        expected = expected.Replace('/', '\\');

        Assert.That(
            PathHelper.GetPartialPattern(path, MatchFlags.Windows, depth).ToString(),
            Is.EqualTo(expected));
    }
}

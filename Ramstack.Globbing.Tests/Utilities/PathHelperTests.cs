namespace Ramstack.Globbing.Utilities;

[TestFixture]
public partial class PathHelperTests
{
    [TestCase("", 1)]
    [TestCase("//", 1)]
    [TestCase("/dir1", 1)]
    [TestCase("dir1", 1)]
    [TestCase("dir1/", 1)]
    [TestCase("/directory_1", 1)]
    [TestCase("directory_1", 1)]
    [TestCase("directory_1/", 1)]
    [TestCase("/dir1/dir2/", 2)]
    [TestCase("dir1/dir2", 2)]
    [TestCase("dir1/dir2/", 2)]
    [TestCase("///dir1/dir2////", 2)]
    [TestCase("/directory_1/directory_2/", 2)]
    [TestCase("directory_1/directory_2", 2)]
    [TestCase("directory_1/directory_2/", 2)]
    [TestCase("///directory_1/directory_2////", 2)]
    public void CountPathSegments(string path, int expected)
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
    [TestCase("**/long_directory_name_1/long_directory_name_2", 1, "**")]
    [TestCase("**/long_directory_name_1/long_directory_name_2", 2, "**")]
    [TestCase("**/long_directory_name_1/long_directory_name_2", 3, "**")]
    [TestCase("**/long_directory_name_1/long_directory_name_2", 4, "**")]
    [TestCase("/**/long_directory_name_1/long_directory_name_2", 1, "/**")]
    [TestCase("/**/long_directory_name_1/long_directory_name_2", 2, "/**")]
    [TestCase("/**/long_directory_name_1/long_directory_name_2", 3, "/**")]
    [TestCase("/**/long_directory_name_1/long_directory_name_2", 4, "/**")]
    [TestCase("dir1/**/dir2/dir3", 1, "dir1")]
    [TestCase("dir1/**/dir2/dir3", 2, "dir1/**")]
    [TestCase("dir1/**/dir2/dir3", 3, "dir1/**")]
    [TestCase("dir1/**/dir2/dir3", 4, "dir1/**")]
    [TestCase("long_directory_name_1/**/long_directory_name_2/long_directory_name_3", 1, "long_directory_name_1")]
    [TestCase("long_directory_name_1/**/long_directory_name_2/long_directory_name_3", 2, "long_directory_name_1/**")]
    [TestCase("long_directory_name_1/**/long_directory_name_2/long_directory_name_3", 3, "long_directory_name_1/**")]
    [TestCase("long_directory_name_1/**/long_directory_name_2/long_directory_name_3", 4, "long_directory_name_1/**")]
    public void GetPartialPattern(string path, int depth, string expected)
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

    [Test]
    public void CountPathSegments_Generated()
    {
        foreach (var (path, slash, f) in GeneratePaths())
        {
            var expected = Math.Max(1, path.Split(slash, StringSplitOptions.RemoveEmptyEntries).Length);
            var count = PathHelper.CountPathSegments(path, f);

            Assert.That(count, Is.EqualTo(expected));
        }
    }

    [Test]
    public void GetPartialPattern_Generated()
    {
        foreach (var (path, slash, f) in GeneratePaths().OrderBy(_ => Random.Shared.Next()).Take(500))
        {
            var count = PathHelper.CountPathSegments(path, f);
            for (var depth = 1; depth <= count * 2; depth++)
            {
                var result = PathHelper
                    .GetPartialPattern(path, f, depth)
                    .ToString()
                    .Split(slash, StringSplitOptions.RemoveEmptyEntries);

                var expected = path
                    .Split(slash, StringSplitOptions.RemoveEmptyEntries)
                    .Take(depth);

                Assert.That(result, Is.EquivalentTo(expected));
            }
        }
    }

    [Test]
    public void SearchPathSeparator()
    {
        for (var n = 0; n < 5000; n++)
        {
            var p0 = new string('a', n);

            var p1 = p0 + "\\";
            var index1 = PathHelper.SearchPathSeparator(p1, MatchFlags.Windows);
            var index2 = PathHelper.SearchPathSeparator(p1, MatchFlags.Unix);

            Assert.That(index1, Is.EqualTo(p1.IndexOf('\\')), $"length: {n}");
            Assert.That(index2, Is.EqualTo(-1),               $"length: {n}");

            var p2 = p0 + "/";
            var index3 = PathHelper.SearchPathSeparator(p2, MatchFlags.Windows);
            var index4 = PathHelper.SearchPathSeparator(p2, MatchFlags.Unix);

            Assert.That(index3, Is.EqualTo(p2.IndexOf('/')), $"length: {n}");
            Assert.That(index4, Is.EqualTo(index3),          $"length: {n}");
        }
    }

    [Test]
    public void SearchPathSeparator_Nothing()
    {
        var source = new string('a', 5000);

        for (var n = 0; n < 5000; n++)
        {
            var p = source.AsSpan(0, n);
            var index1 = PathHelper.SearchPathSeparator(p, MatchFlags.Windows);
            var index2 = PathHelper.SearchPathSeparator(p, MatchFlags.Unix);

            Assert.That(index1, Is.EqualTo(-1));
            Assert.That(index2, Is.EqualTo(-1));
        }
    }

    private static IEnumerable<(string path, char slash, MatchFlags flags)> GeneratePaths()
    {
        var flags = new[]
        {
            ('/',  MatchFlags.Unix),
            ('/',  MatchFlags.Windows),
            ('\\', MatchFlags.Windows)
        };

        foreach (var (s, f) in flags)
        {
            for (var r = 1; r < 5; r++)
            for (var n = 1; n < 37; n++)
            {
                var slash = new string(s, r);
                var segments = new string[n];

                for (var l = 0; l < 43; l++)
                {
                    segments.AsSpan().Fill(new string('a', l));

                    var path = string.Join(slash, segments);
                    for (var v = 0; v < 4; v++)
                    {
                        path = v switch
                        {
                            0 => path,
                            1 => slash + path,
                            2 => slash + path + slash,
                            _ => path + slash
                        };

                        yield return (path, s, f);
                    }
                }
            }
        }
    }
}

using System.Text.RegularExpressions;

using Ramstack.Globbing.Traversal.Helpers;

namespace Ramstack.Globbing.Traversal;

[TestFixture]
public class FilesTests
{
    private readonly TempFileStorage _storage = new();

    [OneTimeTearDown]
    public void Cleanup() =>
        _storage.Dispose();

    [Test]
    public void EnumerateFiles_Default_IncludesHidden()
    {
        var count = Files
            .EnumerateFiles(_storage.Root, "**")
            .Count();

        Assert.That(count, Is.EqualTo(_storage.FileList.Count));
    }

    [Test]
    public void EnumerateDirectories_Default_IncludesHidden()
    {
        var count = Files
            .EnumerateDirectories(_storage.Root, "**")
            .Count();

        var expected = Directory
            .EnumerateDirectories(_storage.Root, "*", SearchOption.AllDirectories)
            .Count();

        Assert.That(count, Is.EqualTo(expected));
    }

    [Test]
    public void EnumerateFiles_OneLevel()
    {
        var list = Files
            .EnumerateFiles(_storage.Root, "project/*")
            .OrderBy(p => p);

        var expected = Directory
            .EnumerateFiles(Path.Combine(_storage.Root, "project"))
            .OrderBy(p => p);

        Assert.That(list, Is.EquivalentTo(expected));
    }

    [Test]
    public void EnumerateDirectories_OneLevel()
    {
        var list = Files
            .EnumerateDirectories(_storage.Root, "project/*")
            .OrderBy(p => p);

        var expected = Directory
            .EnumerateDirectories(Path.Combine(_storage.Root, "project"))
            .OrderBy(p => p);

        Assert.That(list, Is.EquivalentTo(expected));
    }

    [TestCase("cs")]
    [TestCase("csproj")]
    [TestCase("json")]
    [TestCase("log")]
    [TestCase("dat")]
    [TestCase("tmp")]
    [TestCase("ps1")]
    [TestCase("sh")]
    [TestCase("bat")]
    [TestCase("md")]
    [TestCase("sln")]
    [TestCase("dll")]
    [TestCase("nupkg")]
    [TestCase("vb")]
    public void EnumerateFiles_ByExtension(string ext)
    {
        var list = Files
            .EnumerateFiles(_storage.Root, $"**/*.{ext}")
            .OrderBy(p => p);

        var expected = Directory
            .EnumerateFiles(_storage.Root, $"*.{ext}", SearchOption.AllDirectories)
            .OrderBy(p => p);

        Assert.That(list, Is.EquivalentTo(expected));
    }

    [Test]
    public void EnumerateFiles_BraceExtension()
    {
        var list = Files
            .EnumerateFiles(_storage.Root, "**/*.{log,dat}")
            .OrderBy(p => p);

        var expected = Directory
            .EnumerateFiles(_storage.Root, "*", SearchOption.AllDirectories)
            .Where(p => Path.GetExtension(p) is ".log" or ".dat")
            .OrderBy(p => p);

        Assert.That(list, Is.EquivalentTo(expected));
    }

    [Test]
    public void EnumerateFiles_Set()
    {
        var list = Files
            .EnumerateFiles(_storage.Root, "**/tests/[a-zA-Z]*/*.xml")
            .OrderBy(p => p);

        var expected = Directory
            .EnumerateFiles(Path.Combine(_storage.Root, "project", "tests"), "*.xml", SearchOption.AllDirectories)
            .OrderBy(p => p);

        Assert.That(list, Is.EquivalentTo(expected));
    }

    [Test]
    public void EnumerateFiles_Logs()
    {
        var list = Files
            .EnumerateFiles(_storage.Root, "**/logs/**/*.log")
            .OrderBy(p => p);

        var expected = Directory
            .EnumerateFiles(_storage.Root, "*.log", SearchOption.AllDirectories)
            .OrderBy(p => p);

        Assert.That(list, Is.EquivalentTo(expected));
    }

    [Test]
    public void EnumerateFiles_Temps()
    {
        var list = Files
            .EnumerateFiles(_storage.Root, "**/{hidden{,-folder}}/**/*.{tmp,dat}")
            .OrderBy(p => p);

        var expected = Directory
            .EnumerateFiles(_storage.Root, "*", SearchOption.AllDirectories)
            .Where(p => Regex.IsMatch(p, @"\b(hidden|hidden-folder)\b"))
            .OrderBy(p => p);

        Assert.That(list, Is.EquivalentTo(expected));
    }

    [TestCase(@"**/{hidden{,-folder}}/**/*.{tmp,dat}")]
    [TestCase(@"**\{hidden{,-folder}}\**\*.{tmp,dat}")]
    [TestCase(@"project/**/{hidden{,-folder}}/**/*.{tmp,dat}")]
    [TestCase(@"project\**\{hidden{,-folder}}\**\*.{tmp,dat}")]
    public void EnumerateFiles_NoEscaping(string pattern)
    {
        var list = Files
            .EnumerateFiles(_storage.Root, pattern, flags: MatchFlags.Windows)
            .OrderBy(p => p);

        var expected = Directory
            .EnumerateFiles(_storage.Root, "*", SearchOption.AllDirectories)
            .Where(p => Regex.IsMatch(p, @"\b(hidden|hidden-folder)\b"))
            .OrderBy(p => p);

        Assert.That(list, Is.EquivalentTo(expected));
    }

    [TestCase(@"project/**/dataset-\[data]-\{1}.csv")]
    [TestCase(@"project/**/dataset-\[data\]-\{1\}.csv")]
    [TestCase(@"project/**/dataset-\[*]-\{[0-9]}.csv")]
    [TestCase(@"project/**/dataset-*.csv")]
    public void EnumerateFiles_Escaping(string pattern)
    {
        var list = Files
            .EnumerateFiles(_storage.Root, pattern, flags: MatchFlags.Unix)
            .OrderBy(p => p)
            .ToList();

        var expected = Directory
            .EnumerateFiles(Path.Combine(_storage.Root, "project", "data", "raw"), "dataset-*", SearchOption.AllDirectories)
            .OrderBy(p => p);

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list, Is.EquivalentTo(expected));
    }
}

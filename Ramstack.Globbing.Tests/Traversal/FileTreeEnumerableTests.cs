using Ramstack.Globbing.Traversal.Helpers;

namespace Ramstack.Globbing.Traversal;

[TestFixture]
public class FileTreeEnumerableTests
{
    private readonly TempFileStorage _storage = new();

    [OneTimeTearDown]
    public void Cleanup() =>
        _storage.Dispose();

    [Test]
    public void Enumerate_Files()
    {
        var enumerable = new FileTreeEnumerable<FileSystemInfo, string>(new DirectoryInfo(_storage.Root))
        {
            Patterns = ["**"],
            Flags = MatchFlags.Auto,
            FileNameSelector = info => info.Name,
            ShouldRecursePredicate = info => info is DirectoryInfo,
            ShouldIncludePredicate = info => info is FileInfo,
            ResultSelector = info => info.FullName,
            ChildrenSelector = info => ((DirectoryInfo)info).EnumerateFileSystemInfos()
        }.OrderBy(p => p);

        var expected = Directory
            .EnumerateFiles(_storage.Root, "*", SearchOption.AllDirectories)
            .OrderBy(p => p);

        Assert.That(enumerable, Is.EquivalentTo(expected));
    }

    [Test]
    public void Enumerate_Directories()
    {
        var enumerable = new FileTreeEnumerable<FileSystemInfo, string>(new DirectoryInfo(_storage.Root))
        {
            Patterns = ["**"],
            Flags = MatchFlags.Auto,
            FileNameSelector = info => info.Name,
            ShouldRecursePredicate = info => info is DirectoryInfo,
            ShouldIncludePredicate = info => info is DirectoryInfo,
            ResultSelector = info => info.FullName,
            ChildrenSelector = info => ((DirectoryInfo)info).EnumerateFileSystemInfos()
        }.OrderBy(p => p);

        var expected = Directory
            .EnumerateDirectories(_storage.Root, "*", SearchOption.AllDirectories)
            .OrderBy(p => p);

        Assert.That(enumerable, Is.EquivalentTo(expected));
    }

    [Test]
    public void Enumerate_Both()
    {
        var enumerable = new FileTreeEnumerable<FileSystemInfo, string>(new DirectoryInfo(_storage.Root))
        {
            Patterns = ["**"],
            Flags = MatchFlags.Auto,
            FileNameSelector = info => info.Name,
            ShouldRecursePredicate = info => info is DirectoryInfo,
            ResultSelector = info => info.FullName,
            ChildrenSelector = info => ((DirectoryInfo)info).EnumerateFileSystemInfos()
        }.OrderBy(p => p);

        var expected = Directory
            .EnumerateFileSystemEntries(_storage.Root, "*", SearchOption.AllDirectories)
            .OrderBy(p => p);

        Assert.That(enumerable, Is.EquivalentTo(expected));
    }
}

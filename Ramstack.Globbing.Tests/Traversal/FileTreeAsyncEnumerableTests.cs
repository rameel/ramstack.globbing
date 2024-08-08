using Ramstack.Globbing.Traversal.Helpers;

namespace Ramstack.Globbing.Traversal;

[TestFixture]
public class FileTreeAsyncEnumerableTests
{
    private readonly TempFileStorage _storage = new();

    [OneTimeTearDown]
    public void Cleanup() =>
        _storage.Dispose();

    [Test]
    public async Task Enumerate_Files()
    {
        var enumerable = new FileTreeAsyncEnumerable<FileSystemInfo, string>(new DirectoryInfo(_storage.Root))
        {
            Patterns = ["**"],
            Flags = MatchFlags.Auto,
            FileNameSelector = info => info.Name,
            ShouldRecursePredicate = info => info is DirectoryInfo,
            ShouldIncludePredicate = info => info is FileInfo,
            ResultSelector = info => info.FullName,
            ChildrenSelector = (info, _) => ((DirectoryInfo)info).EnumerateFileSystemInfos().ToAsyncEnumerable()
        }.OrderBy(p => p);

        var expected = Directory
            .EnumerateFiles(_storage.Root, "*", SearchOption.AllDirectories)
            .OrderBy(p => p);

        Assert.That(
            await enumerable.ToArrayAsync(),
            Is.EquivalentTo(expected));
    }

    [Test]
    public async Task Enumerate_Directories()
    {
        var enumerable = new FileTreeAsyncEnumerable<FileSystemInfo, string>(new DirectoryInfo(_storage.Root))
        {
            Patterns = ["**"],
            Flags = MatchFlags.Auto,
            FileNameSelector = info => info.Name,
            ShouldRecursePredicate = info => info is DirectoryInfo,
            ShouldIncludePredicate = info => info is DirectoryInfo,
            ResultSelector = info => info.FullName,
            ChildrenSelector = (info, _) => ((DirectoryInfo)info).EnumerateFileSystemInfos().ToAsyncEnumerable()
        }.OrderBy(p => p);

        var expected = Directory
            .EnumerateDirectories(_storage.Root, "*", SearchOption.AllDirectories)
            .OrderBy(p => p);

        Assert.That(
            await enumerable.ToArrayAsync(),
            Is.EquivalentTo(expected));
    }

    [Test]
    public async Task Enumerate_Both()
    {
        var enumerable = new FileTreeAsyncEnumerable<FileSystemInfo, string>(new DirectoryInfo(_storage.Root))
        {
            Patterns = ["**"],
            Flags = MatchFlags.Auto,
            FileNameSelector = info => info.Name,
            ShouldRecursePredicate = info => info is DirectoryInfo,
            ResultSelector = info => info.FullName,
            ChildrenSelector = (info, _) => ((DirectoryInfo)info).EnumerateFileSystemInfos().ToAsyncEnumerable(),
        }.OrderBy(p => p);

        var expected = Directory
            .EnumerateFileSystemEntries(_storage.Root, "*", SearchOption.AllDirectories)
            .OrderBy(p => p);

        Assert.That(
            await enumerable.ToArrayAsync(),
            Is.EquivalentTo(expected));
    }
}

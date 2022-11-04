namespace MarkDownIndexGenerator.Models;

public class MarkDownTree : Tree<FileSystemInfo>
{
    private MarkDownTree()
    {
    }

    private MarkDownTree(FileSystemInfo info)
    {
        Value = info;
    }

    /// <summary>
    ///     children node of the tree
    /// </summary>
    public new List<MarkDownTree>? Childs { get; set; }

    /// <summary>
    ///     generate markdown file tree
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="isRoot">is root node</param>
    /// <returns>mark down file tree</returns>
    public static MarkDownTree GetMarkDownFileTree(string path, bool isRoot = true)
    {
        var checkInfo = new FileInfo(path);
        var dirPath = checkInfo.Attributes == FileAttributes.Directory ? path : checkInfo.DirectoryName!;
        var result = new MarkDownTree(new FileInfo(path));
        var dirInfo = new DirectoryInfo(dirPath) ?? throw new ArgumentException("path is not a valid directory");
        var dirs = dirInfo.GetDirectories().ToList();
        var fileInfos = dirInfo.GetFiles("*.md", SearchOption.TopDirectoryOnly).ToList();
        // if is root then , remove same file in childs
        if (isRoot && checkInfo.Attributes != FileAttributes.Directory)
        {
            var delNode = fileInfos.FirstOrDefault(x => x.FullName == path);
            if (delNode != null)
                fileInfos.Remove(delNode);
        }

        if (dirs.Count < 1 && fileInfos.Count < 1)
            return result;

        var childs = new List<MarkDownTree>();
        childs.AddRange(fileInfos.Select(fileInfo => new MarkDownTree
        {
            Value = fileInfo
        }));
        foreach (var child in childs)
        {
            var sameNameDir = dirs.FirstOrDefault(dir =>
                Path.GetFileName(dir.FullName) == Path.GetFileNameWithoutExtension(child.Value.FullName));
            if (sameNameDir == null)
                continue;
            child.Childs = GetMarkDownFileTree(sameNameDir.FullName, false).Childs;
            dirs.Remove(sameNameDir);
        }

        childs.AddRange(dirs.Select(dir => GetMarkDownFileTree(dir.FullName, false)));
        result.Childs = childs;
        return result;
    }
}
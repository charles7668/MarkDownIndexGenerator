namespace MarkDownIndexGenerator.Models;

public class MarkDownTree : Tree<FileSystemInfo>
{
    /// <summary>
    ///     children node of the tree
    /// </summary>
    public new List<MarkDownTree>? Childs { get; set; }

    /// <summary>
    ///     generate markdown file tree
    /// </summary>
    /// <param name="path">path</param>
    /// <returns>mark down file tree</returns>
    public static MarkDownTree GetMarkDownFileTree(string path)
    {
        var result = new MarkDownTree();
        var dirInfo = new DirectoryInfo(path);
        var dirs = dirInfo.GetDirectories().ToList();
        var fileInfos = dirInfo.GetFiles("*.md", SearchOption.TopDirectoryOnly);
        if (dirs.Count < 1 && fileInfos.Length < 1)
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
            child.Childs = GetMarkDownFileTree(sameNameDir.FullName).Childs;
            dirs.Remove(sameNameDir);
        }

        childs.AddRange(dirs.Select(dir => GetMarkDownFileTree(dir.FullName)));
        result.Childs = childs;
        return result;
    }
}
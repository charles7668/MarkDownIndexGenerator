using System.Text;

namespace MarkDownIndexGenerator.Models;

public static class MarkDownTreeExtensions
{
    /// <summary>
    ///     print tree into string builder
    /// </summary>
    /// <param name="tree">tree</param>
    /// <param name="stringBuilder">string builder</param>
    /// <param name="prefix">prefix</param>
    /// <param name="maxLevel">max print level</param>
    /// <param name="currentLevel">current level</param>
    /// <param name="rootPath">root path</param>
    public static void PrintTree(this MarkDownTree tree, StringBuilder stringBuilder, string prefix = "- ",
        int maxLevel = 1,
        int currentLevel = 0, string rootPath = "")
    {
        var info = tree.Value;

        void GenerateDirectoryIndex(StringBuilder sb, string prefixText, string fileName)
        {
            stringBuilder.AppendLine($"{prefixText}{fileName}");
        }

        void GenerateFileIndex(StringBuilder sb, string prefixText, string fileName)
        {
            var link = info.FullName.Replace(" ", "%20");
            if (!string.IsNullOrEmpty(rootPath))
            {
                FileSystemInfo rootInfo = new FileInfo(rootPath);
                rootPath = rootInfo.Attributes != FileAttributes.Directory
                    ? Path.GetDirectoryName(rootInfo.FullName)!
                    : rootInfo.FullName;
                link = link.Replace(rootPath, string.Empty).TrimStart('\\');
            }

            stringBuilder.AppendLine($"{prefixText}[{fileName}]({link})");
        }

        if (currentLevel > 0)
        {
            var fileName = Path.GetFileNameWithoutExtension(info.FullName);
            Action<StringBuilder, string, string> generateAction = info.Attributes == FileAttributes.Directory
                ? GenerateDirectoryIndex
                : GenerateFileIndex;
            generateAction(stringBuilder, prefix, fileName);

            prefix = "  " + prefix;
        }

        var childs = tree.Childs;
        if (childs == null || currentLevel > maxLevel)
            return;
        foreach (var child in childs)
            PrintTree(child, stringBuilder, prefix, maxLevel, currentLevel + 1, rootPath);
    }

    /// <summary>
    ///     insert index into markdown file in tree
    /// </summary>
    /// <param name="tree">tree</param>
    /// <param name="root">root node</param>
    public static void InsertIndexIntoTree(this MarkDownTree tree, MarkDownTree root)
    {
        var info = tree.Value;
        if (info.Attributes != FileAttributes.Directory)
        {
            var position = IndexInserter.GetInsertPosition(info);
            var sb = new StringBuilder();
            tree.PrintTree(sb, "- ", 2, 0, root.Value.FullName);
            IndexInserter.InsertIndexIntoFile(info.FullName, sb.ToString(), position);
        }

        var childs = tree.Childs;
        if (childs == null)
            return;
        Parallel.ForEach(childs, child => InsertIndexIntoTree(child, root));
    }
}
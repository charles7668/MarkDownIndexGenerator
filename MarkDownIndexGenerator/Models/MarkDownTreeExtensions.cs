using System.Text;
using MarkDownIndexGenerator.Services;

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
    public static void PrintTree(this MarkDownTree tree, StringBuilder stringBuilder, string prefix = "- ",
        int maxLevel = 1,
        int currentLevel = 0)
    {
        var info = tree.Value;
        if (currentLevel > 0)
        {
            stringBuilder.AppendLine(prefix + Path.GetFileNameWithoutExtension(info.FullName));
            prefix = "  " + prefix;
        }

        var childs = tree.Childs;
        if (childs == null || currentLevel > maxLevel)
            return;
        foreach (var child in childs)
            PrintTree(child, stringBuilder, prefix, maxLevel, currentLevel + 1);
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
            var position = FileService.GetInsertPosition(info);
            var sb = new StringBuilder();
            tree.PrintTree(sb, "- ", 2);
            FileService.InsertIndexIntoFile(info.FullName, sb.ToString(), position);
        }

        var childs = tree.Childs;
        if (childs == null)
            return;
        foreach (var child in childs)
            InsertIndexIntoTree(child, root);
    }
}
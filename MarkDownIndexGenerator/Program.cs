// See https://aka.ms/new-console-template for more information

using CommandLine;
using MarkDownIndexGenerator.Models;

Parser.Default.ParseArguments<InputOption>(args).WithParsed(Run);

static void Run(InputOption option)
{
    var tree = GetMarkDownFileTree(option.RootPath);
    PrintTree(tree);
}

static void PrintTree(Tree<FileSystemInfo> tree, string prefix = "")
{
    Console.WriteLine(prefix + tree.Value.FullName);
    if (tree.Childs == null)
        return;
    foreach (var child in tree.Childs)
        PrintTree(child, prefix + "  ");
}

static Tree<FileSystemInfo> GetMarkDownFileTree(string path)
{
    var result = new Tree<FileSystemInfo>
    {
        Value = new FileInfo(path)
    };
    var dirInfo = new DirectoryInfo(path);
    var dirs = dirInfo.GetDirectories().ToList();
    var fileInfos = dirInfo.GetFiles("*.md", SearchOption.TopDirectoryOnly);
    if (dirs.Count < 1 && fileInfos.Length < 1)
        return result;

    var childs = new List<Tree<FileSystemInfo>>();
    childs.AddRange(fileInfos.Select(fileInfo => new Tree<FileSystemInfo>
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
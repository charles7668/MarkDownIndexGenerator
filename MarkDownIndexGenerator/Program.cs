// See https://aka.ms/new-console-template for more information

using System.Net;
using CommandLine;
using MarkDownIndexGenerator.Models;

Parser.Default.ParseArguments<InputOption>(args).WithParsed(Run);

static void Run(InputOption option)
{
    if (string.IsNullOrEmpty(option.RootFile) || !File.Exists(option.RootFile))
    {
        Console.WriteLine("Root file not found.");
        return;
    }

    var tree = GetMarkDownFileTree(Path.GetDirectoryName(Path.GetFullPath(option.RootFile)) ??
                                   throw new InvalidOperationException());
    // set root file
    tree.Value = new FileInfo(option.RootFile);
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
    var result = new Tree<FileSystemInfo>();
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
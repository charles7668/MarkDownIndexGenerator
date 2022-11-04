// See https://aka.ms/new-console-template for more information

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

    var tree = MarkDownTree.GetMarkDownFileTree(Path.GetFullPath(option.RootFile));
    // set root file
    tree.Value = new FileInfo(option.RootFile);
    var childs = tree.Childs;
    if (childs != null)
        tree.Childs = childs.Where(x => x.Value.FullName != tree.Value.FullName).ToList();
    tree.InsertIndexIntoTree(tree, option.TitleText);
}
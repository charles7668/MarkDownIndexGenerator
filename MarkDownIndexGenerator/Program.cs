// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.RegularExpressions;
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
    var childs = tree.Childs;
    if (childs != null)
        tree.Childs = childs.Where(x => x.Value.FullName != tree.Value.FullName).ToList();
    InsertIndexIntoAllFile(tree, tree);
}

static void InsertIndexIntoAllFile(Tree<FileSystemInfo> tree, Tree<FileSystemInfo> root)
{
    var info = tree.Value;
    if (info.Attributes != FileAttributes.Directory)
    {
        var position = GetInsertPosition(tree.Value);
        var sb = new StringBuilder();
        PrintTree(sb, tree, "- ", 2);
        InsertIndexIntoFile(info.FullName, sb.ToString(), position);
    }

    var childs = tree.Childs;
    if (childs == null)
        return;
    foreach (var child in childs)
        InsertIndexIntoAllFile(child, root);
}

static void InsertIndexIntoFile(string inputFile, string insertText,
    (int startLine, int endLine) positionInfo)
{
    var tempFile = Guid.NewGuid().ToString();
    using (var sw = new StreamWriter(tempFile))
    {
        void Insert()
        {
            sw.WriteLine();
            sw.Write(insertText);
            sw.WriteLine();
        }

        if (positionInfo.startLine == -1)
        {
            sw.WriteLine("# Index");
            Insert();
        }
        else
        {
            using var sr = new StreamReader(inputFile);
            var lineNumber = 0;
            while (sr.ReadLine() is { } temp)
            {
                if (lineNumber < positionInfo.startLine || lineNumber >= positionInfo.endLine)
                {
                    sw.WriteLine(temp);
                }
                else if (lineNumber == positionInfo.startLine)
                {
                    sw.WriteLine(temp);
                    Insert();
                }

                lineNumber++;
            }
        }
    }

    try
    {
        File.Copy(tempFile, inputFile, true);
    }
    catch (Exception e)
    {
        Console.WriteLine($"{inputFile} can't replace\n{e}");
    }
    finally
    {
        File.Delete(tempFile);
    }
}

static void PrintTree(StringBuilder stringBuilder, Tree<FileSystemInfo> tree, string prefix = "- ", int maxLevel = 1,
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
        PrintTree(stringBuilder, child, prefix, maxLevel, currentLevel + 1);
}

static (int startLine, int endLine) GetInsertPosition(FileSystemInfo info)
{
    var startLine = -1;
    var endLine = -1;
    if (!File.Exists(info.FullName))
        return (startLine, endLine);
    using var sr = new StreamReader(info.FullName);
    var regexStart = new Regex(@"^#+ Index");
    var regexEnd = new Regex(@"^ *- ");
    var lineNumber = 0;
    while (sr.ReadLine() is { } temp)
    {
        if (startLine == -1)
        {
            if (regexStart.IsMatch(temp))
                startLine = lineNumber;
        }
        else if (!regexEnd.IsMatch(temp) && !string.IsNullOrEmpty(temp))
        {
            endLine = lineNumber;
            break;
        }

        lineNumber++;
    }

    if (endLine == -1)
        endLine = lineNumber;

    return (startLine, endLine);
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
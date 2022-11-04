using CommandLine;

namespace MarkDownIndexGenerator.Models;

/// <summary>
/// input command option
/// </summary>
public class InputOption
{
    [Option('i', "input", Required = true, HelpText = "Input file path to be processed.")]
    public string? RootFile { get; set; }

    [Option('t', "title", Required = false,
        HelpText = "specify title which will be search or insert,default is 'Index'.")]
    public string TitleText { get; set; } = "Index";
}
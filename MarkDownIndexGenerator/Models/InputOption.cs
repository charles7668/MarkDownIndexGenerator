using CommandLine;

namespace MarkDownIndexGenerator.Models;

/// <summary>
/// input command option
/// </summary>
public class InputOption
{
    [Option('i', "input", Required = true, HelpText = "Input file path to be processed.")]
    public string? RootFile { get; set; }
}
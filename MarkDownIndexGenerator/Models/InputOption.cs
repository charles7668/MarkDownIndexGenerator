using CommandLine;

namespace MarkDownIndexGenerator.Models;

/// <summary>
/// input command option
/// </summary>
public class InputOption
{
    [Option('i', "input", Required = true, HelpText = "Input path to be processed.")]
    public string RootPath { get; set; }
}
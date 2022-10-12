using CommandLine;

namespace TidyCSharp.Cli;

public class CliOptions
{
    [Option('p', "path", Required = true, HelpText = "Solution path.")]
    public string SolutionPath { get; set; }

    [Option('o', "output", Required = true, HelpText = "Output path.")]
    public string OutputPath { get; set; }

    [Option('m', "mode", Required = true, HelpText = "Readonly(r) or RunSafeRules(w) mode.")]
    public string Mode { get; set; }
}

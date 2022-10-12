using Microsoft.CodeAnalysis.MSBuild;

namespace TidyCSharp.Cli;

public class ProgressBarProjectLoadStatus : IProgress<ProjectLoadProgress>
{
    public void Report(ProjectLoadProgress value)
    {
        Console.Out.WriteLine($"{value.Operation} {value.FilePath}");
    }
}
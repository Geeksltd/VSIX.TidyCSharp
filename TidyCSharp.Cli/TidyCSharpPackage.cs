using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using TidyCSharp.Cli.Menus.Cleanup.Utils;

namespace TidyCSharp.Cli;

public sealed class TidyCSharpPackage
{
    public static TidyCSharpPackage Instance { get; private set; }

    public MSBuildWorkspace VsWorkspace { get; set; }

    public Solution Solution
    {
        get; private set;
    }

    public async Task InitializeAsync(string solutionPath)
    {
        // MSBuild should copy Microsoft.CodeAnalysis.CSharp.Workspaces.dll
        var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

        VsWorkspace = MSBuildWorkspace.Create();

        VsWorkspace.SkipUnrecognizedProjects = true;
        VsWorkspace.WorkspaceFailed += (sender, e) =>
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
            {
                Console.Error.WriteLine(e.Diagnostic.Message);
            }
        };

        Solution = await VsWorkspace.OpenSolutionAsync(solutionPath, new ProgressBarProjectLoadStatus());

        Instance = this;
    }
}
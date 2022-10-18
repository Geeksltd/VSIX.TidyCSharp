using Microsoft.CodeAnalysis;

namespace TidyCSharp.Cli.Utility;

public class SolutionActions
{
    public static List<Project> FindProjects()
    {
        return TidyCSharpPackage.Instance.Solution.Projects.ToList();
    }
}
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers;
using TidyCSharp.Cli.Utility;
using static TidyCSharp.Cli.Menus.ActionsOnCSharp.CSharpAction;

namespace TidyCSharp.Cli.Menus.ActionsOnCSharp;

public class ActionCSharpOnSolution
{
    public static async Task InvokeAsync(TargetAction action, CleanupOptions cleanupOptions)
    {
        try
        {
            var projects = SolutionActions.FindProjects();

            for (var i = 0; i < projects.Count; i++)
            {
                var currentProject = projects[i];
                if (currentProject.Documents == null) continue;
                if (currentProject.FilePath.EndsWith(".shproj", StringComparison.OrdinalIgnoreCase)) continue;

                foreach (var document in currentProject.Documents)
                   await ActionCSharpOnProjectItem.ActionAsync(document, action, cleanupOptions);
            }
        }
        catch (Exception e)
        {
            ErrorNotification.ErrorNotification.WriteErrorToFile(e);
            ErrorNotification.ErrorNotification.WriteErrorToOutputWindow(e);
            ProcessActions.GeeksProductivityToolsProcess();
        }
    }
}
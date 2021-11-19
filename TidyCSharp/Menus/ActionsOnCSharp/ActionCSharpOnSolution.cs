using Geeks.GeeksProductivityTools.Utils;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using System;
using static Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp.CSharpActionDelegate;

namespace Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp
{
    public class ActionCSharpOnSolution
    {
        public static void Invoke(TargetAction action, CleanupOptions cleanupOptions)
        {
            try
            {
                var projects = SolutionActions.FindProjects(DteServiceProvider.Instance);

                for (var i = 0; i < projects.Count; i++)
                {
                    var currentProject = projects[i];
                    if (currentProject.ProjectItems == null) continue;
                    if (currentProject.FullName.EndsWith(".shproj", StringComparison.OrdinalIgnoreCase)) continue;

                    for (var j = 1; j < currentProject.ProjectItems.Count; j++)
                        ActionCSharpOnProjectItem.Action(currentProject.ProjectItems.Item(j), action, cleanupOptions);
                }
            }
            catch (Exception e)
            {
                ErrorNotification.WriteErrorToFile(e);
                ErrorNotification.WriteErrorToOutputWindow(e);
                ProcessActions.GeeksProductivityToolsProcess();
            }
        }
    }
}
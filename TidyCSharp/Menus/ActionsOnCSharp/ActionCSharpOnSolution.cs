using System;
using Geeks.GeeksProductivityTools.Utils;
using static Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp.CSharpActionDelegate;

namespace Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp
{
    public class ActionCSharpOnSolution
    {
        public static void Invoke(TargetAction action, Definition.CodeCleanerType[] type)
        {
            try
            {
                var projects = SolutionActions.FindProjects(DteServiceProvider.Instance);

                for (var i = 0; i < projects.Count; i++)
                {
                    var currentProject = projects[i];
                    if (currentProject.ProjectItems == null) continue;
                    if (currentProject.FullName.ToLower().EndsWith(".shproj")) continue;

                    for (var j = 1; j < currentProject.ProjectItems.Count; j++)
                        ActionCSharpOnProjectItem.Action(currentProject.ProjectItems.Item(j), action, type);
                }
            }
            catch (Exception e)
            {
                ErrorNotification.EmailError(e);
                ProcessActions.GeeksProductivityToolsProcess();
            }
        }
    }
}

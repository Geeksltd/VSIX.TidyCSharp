using EnvDTE;
using Geeks.GeeksProductivityTools.Utils;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using System;
using System.Windows;
using System.Windows.Forms;
using static Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp.CSharpActionDelegate;

namespace Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp
{
    public class ActionCSharpOnProject
    {
        public static void Invoke(TargetAction action, CleanupOptions cleanupOptions)
        {
            try
            {
                var projects = DteServiceProvider.Instance.ActiveSolutionProjects as Array;
                var currentProject = projects.GetValue(0) as Project;

                if (currentProject.ProjectItems == null) return;
                if (currentProject.FullName.EndsWith(".shproj", StringComparison.OrdinalIgnoreCase))
                {
                    System.Windows.MessageBox.Show("Clean up can't be called direlctly on Shared Project", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                for (var i = 1; i <= currentProject.ProjectItems.Count; i++)
                    ActionCSharpOnProjectItem.Action(currentProject.ProjectItems.Item(i), action, cleanupOptions);
            }
            catch (Exception e)
            {
                ErrorNotification.WriteErrorToOutputWindow(e);
                ErrorNotification.WriteErrorToFile(e);
                ProcessActions.GeeksProductivityToolsProcess();
            }
        }
    }
}
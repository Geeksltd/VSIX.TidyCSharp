using EnvDTE;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using static Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp.CSharpActionDelegate;

namespace Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp
{
    public class ActionCSharpOnProjectItem
    {
        public static async void Action(ProjectItem item, TargetAction targetAction, CleanupOptions cleanupOptions)
        {
            await targetAction(item, cleanupOptions);

            if (item.ProjectItems == null) return;

            for (var i = 1; i <= item.ProjectItems.Count; i++)
                Action(item.ProjectItems.Item(i), targetAction, cleanupOptions);
        }
    }
}

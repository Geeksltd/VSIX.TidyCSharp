using EnvDTE;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;

namespace Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp
{
    public class CSharpAction
    {
        public delegate void TargetAction(ProjectItem item, CleanupOptions cleanupOptions, bool fileWindowMustBeOpend = false);
    }
}
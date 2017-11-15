using System.Threading.Tasks;
using EnvDTE;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;

namespace Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp
{
    public class CSharpActionDelegate
    {
        public delegate Task TargetAction(ProjectItem item, CleanupOptions cleanupOptions, bool fileWindowMustBeOpend = false);
    }
}

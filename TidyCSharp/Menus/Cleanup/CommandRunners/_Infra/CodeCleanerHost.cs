using EnvDTE;
using Geeks.VSIX.TidyCSharp;
using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class CodeCleanerHost
    {
        public static void Run(ProjectItem item, CodeCleanerType command, CleanupOptions cleanupOptions)
        {
            if (!ActiveDocument.IsValid(item))
                ErrorNotification.EmailError(Resources.PrivateModifierCleanUpFailed);

            else
            {
                var instance = CodeCleanerFactory.Create(command, cleanupOptions);
                new CodeCleaner(instance, item).Run();
            }
        }
    }
}

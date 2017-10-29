using System;
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
                var invoker = new CleanerActionInvoker(item);
                invoker.Invoke(command, cleanupOptions);
                //switch (command)
                //{
                //    case CodeCleanerType.All:
                //        invoker.InvokeAll();
                //        break;
                //    default:
                //        invoker.Invoke(command);
                //        break;
                //}
            }
        }
    }
}

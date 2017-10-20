using EnvDTE;
using Geeks.GeeksProductivityTools.Definition;
using Geeks.GeeksTidyCSharp;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class CodeCleanerHost
    {
        public static void Run(ProjectItem item, CodeCleanerType command)
        {
            if (!ActiveDocument.IsValid(item))
                ErrorNotification.EmailError(Resources.PrivateModifierCleanUpFailed);

            else
            {
                var invoker = new CleanerActionInvoker(item);
                switch (command)
                {
                    case CodeCleanerType.All:
                        invoker.InvokeAll();
                        break;
                    default:
                        invoker.Invoke(command);
                        break;
                }
            }
        }
    }
}

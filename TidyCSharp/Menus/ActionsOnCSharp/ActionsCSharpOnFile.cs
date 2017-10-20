using System;
using EnvDTE;
using Geeks.GeeksProductivityTools.Definition;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.GeeksProductivityTools.Utils;
using Geeks.GeeksProductivityTools.Extensions;

namespace Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp
{
    public class ActionsCSharpOnFile
    {
        public static void DoCleanup(ProjectItem item, CodeCleanerType[] actionType)
        {
            DoCleanup(item, actionType, false);
        }

        public static void DoCleanup(ProjectItem item, CodeCleanerType[] actionType, bool fileWindowMustBeOpend = false)
        {
            if (!item.IsCsharpFile() || item.IsCSharpDesignerFile()) return;

            try
            {
                var path = item.Properties.Item("FullPath").Value.ToString();
                if (path.EndsWithAny(new[] { "AssemblyInfo.cs", "TheApplication.cs" })) return;

                var window = item.Open(Constants.vsViewKindCode);

                window.Activate();
                foreach (var actionTypeItem in actionType)
                {
                    CodeCleanerHost.Run(item, actionTypeItem);
                }
                if (fileWindowMustBeOpend == false)
                {
                    window.Close(vsSaveChanges.vsSaveChangesYes);
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

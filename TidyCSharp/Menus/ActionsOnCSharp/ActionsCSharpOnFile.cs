using EnvDTE;
using Geeks.GeeksProductivityTools.Extensions;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.GeeksProductivityTools.Utils;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using System;

namespace Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp
{
    public class ActionsCSharpOnFile
    {
        public static void DoCleanup(ProjectItem item, CleanupOptions cleanupOptions)
        {
            DoCleanup(item, cleanupOptions, false);
        }

        public static void DoCleanup(ProjectItem item, CleanupOptions cleanupOptions, bool fileWindowMustBeOpend = false)
        {
            if (!item.IsCsharpFile() || item.IsCSharpDesignerFile()) return;

            try
            {
                var path = item.Properties.Item("FullPath").Value.ToString();
                if (path.EndsWithAny(new[] { "AssemblyInfo.cs", "TheApplication.cs" })) return;

                var window = item.Open(Constants.vsViewKindCode);

                window.Activate();

                foreach (var actionTypeItem in cleanupOptions.ActionTypes)
                {
                    if (actionTypeItem == VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces) continue;
                    if (actionTypeItem == VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives) continue;

                    CodeCleanerHost.Run(item, actionTypeItem, cleanupOptions);
                }

                if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces))
                {
                    CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces, cleanupOptions);
                }

                if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives))
                {
                    window.Document.Close(vsSaveChanges.vsSaveChangesYes);

                    CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives, cleanupOptions);

                    if (fileWindowMustBeOpend == false)
                    {
                        window = item.Open(Constants.vsViewKindCode);

                        window.Activate();
                    }
                }
                else
                {
                    window.Document.Save();
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
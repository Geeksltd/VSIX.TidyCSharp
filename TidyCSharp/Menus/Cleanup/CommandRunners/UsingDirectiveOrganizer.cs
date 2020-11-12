using EnvDTE;
using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Definition;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.GeeksProductivityTools.Utils;
using Microsoft.CodeAnalysis;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class UsingDirectiveOrganizer : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            try
            {
                var window = ProjectItemDetails.ProjectItem.Open(Constants.vsViewKindCode);

                window.Activate();
                ProjectItemDetails.ProjectItem.Document.DTE.ExecuteCommand(UsingsCommands.REMOVE_AND_SORT_COMMAND_NAME);
                window.Document.Save();
            }
            catch (Exception e)
            {
                ErrorNotification.EmailError(e);
                ProcessActions.GeeksProductivityToolsProcess();
            }

            return ProjectItemDetails.ProjectItem.ToSyntaxNode();
        }
    }
}
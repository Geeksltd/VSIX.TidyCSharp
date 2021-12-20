using EnvDTE;
using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Definition;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.GeeksProductivityTools.Utils;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class UsingDirectiveOrganizer : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
        {
            var item = ProjectItemDetails.ProjectItem;

            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                item.Open(Constants.vsViewKindCode);

                var document = item.Document;
                document.Activate();

                try { document.DTE.ExecuteCommand(UsingsCommands.RemoveAndSortCommandName); }
                catch (Exception ex)
                {
                    if (ex.Message != "Command \"Edit.RemoveAndSort\" is not available.") throw;

                    document.Activate();
                    document.DTE.ExecuteCommand(UsingsCommands.RemoveAndSortCommandName);
                }

                var doc = (EnvDTE.TextDocument)(document.Object("TextDocument"));
                var p = doc.StartPoint.CreateEditPoint();
                var s = p.GetText(doc.EndPoint);
                var modified = SyntaxFactory.ParseSyntaxTree(s);

                if (IsReportOnlyMode &&
                    !IsEquivalentToUNModified(await modified.GetRootAsync()))
                {
                    CollectMessages(new ChangesReport(initialSourceNode)
                    {
                        LineNumber = 1,
                        Column = 1,
                        Message = "Your Using usage is not good",
                        Generator = nameof(UsingDirectiveOrganizer)
                    });

                    document.Undo();
                    return initialSourceNode;
                }

                document.Save();
            }
            catch (Exception e)
            {
                ErrorNotification.WriteErrorToFile(e, initialSourceNode.GetFilePath());
                ErrorNotification.WriteErrorToOutputWindow(e, initialSourceNode.GetFilePath());
                ProcessActions.GeeksProductivityToolsProcess();
            }

            return item.ToSyntaxNode();
        }
    }
}
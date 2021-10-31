using EnvDTE;
using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Definition;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.GeeksProductivityTools.Utils;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
	public class UsingDirectiveOrganizer : CodeCleanerCommandRunnerBase, ICodeCleaner
	{
		public override async Task<SyntaxNode> CleanUp(SyntaxNode initialSourceNode)
		{

			try
			{
				Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

				var window = ProjectItemDetails.ProjectItem.Open(Constants.vsViewKindCode);
				//window = ProjectItemDetails.ProjectItem.Document.DTE
				//    .ItemOperations.OpenFile(ProjectItemDetails.ProjectItem.Document.Path
				//    , Constants.vsViewKindCode);
				//window.Activate();
				ProjectItemDetails.ProjectItem.Document.Activate();
				ProjectItemDetails.ProjectItem.Document.DTE.ExecuteCommand(UsingsCommands.REMOVE_AND_SORT_COMMAND_NAME);
				EnvDTE.TextDocument doc = (EnvDTE.TextDocument)(ProjectItemDetails.ProjectItem.Document.Object("TextDocument"));
				var p = doc.StartPoint.CreateEditPoint();
				string s = p.GetText(doc.EndPoint);
				var modified = SyntaxFactory.ParseSyntaxTree(s);

				if (IsReportOnlyMode &&
					!IsEquivalentToUnModified(modified.GetRoot()))
				{
					this.CollectMessages(new ChangesReport(initialSourceNode)
					{
						LineNumber = 1,
						Column = 1,
						Message = "Your Using usage is not good",
						Generator = nameof(UsingDirectiveOrganizer)
					});
					ProjectItemDetails.ProjectItem.Document.Undo();
					return initialSourceNode;
				}
				ProjectItemDetails.ProjectItem.Document.Save();
				//window.Document.Save();
			}
			catch (Exception e)
			{
				ErrorNotification.WriteErrorToFile(e, initialSourceNode.GetFilePath());
				ErrorNotification.WriteErrorToOutputWindow(e, initialSourceNode.GetFilePath());
				ProcessActions.GeeksProductivityToolsProcess();
			}

			return ProjectItemDetails.ProjectItem.ToSyntaxNode();
		}
	}
}
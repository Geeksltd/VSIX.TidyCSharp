using EnvDTE;
using Geeks.GeeksProductivityTools.Extensions;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.GeeksProductivityTools.Utils;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;

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

				TextSpan span = new TextSpan();



				var documentText = item.ToSyntaxNode().SyntaxTree.GetText().ToString();
				if (documentText.Contains("[EscapeGCop(\"Auto generated code.\")]"))
				{
					return;
				}

				if (item.ToSyntaxNode()
					.DescendantNodesOfType<AttributeSyntax>()
					.Any(x => x.Name.ToString() == "EscapeGCop" &&
					x.ArgumentList != null &&
					x.ArgumentList.Arguments.FirstOrDefault().ToString()
					   == "\"Auto generated code.\""))
				{
					return;
				}

				var window = item.Open(Constants.vsViewKindCode);
				window.Activate();

				foreach (var actionTypeItem in cleanupOptions.ActionTypes)
				{
					if (actionTypeItem == VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces) continue;
					if (actionTypeItem == VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives) continue;
					if (actionTypeItem == VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods) continue;

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
				if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods))
				{
					CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods, cleanupOptions);
				}
				if (fileWindowMustBeOpend == false)
				{
					window.Close(vsSaveChanges.vsSaveChangesYes);
				}
			}
			catch (Exception e)
			{
				ErrorNotification.WriteErrorToFile(e, item.Properties.Item("FullPath").Value.ToString());
				ErrorNotification.WriteErrorToOutputWindow(e, item.Properties.Item("FullPath").Value.ToString());
				ProcessActions.GeeksProductivityToolsProcess();
			}
		}


		public static void ReportOnlyDoNotCleanup(ProjectItem item, CleanupOptions cleanupOptions, bool fileWindowMustBeOpend = false)
		{
			if (!item.IsCsharpFile() || item.IsCSharpDesignerFile()) return;

			try
			{

				ThreadHelper.JoinableTaskFactory.Run(async delegate
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				});
				
				//Sometimes cannot find document's file
				try
				{
					var documentText = item.ToSyntaxNode().SyntaxTree.GetText().ToString();
					if (documentText.Contains("[EscapeGCop(\"Auto generated code.\")]"))
					{
						return;
					}
				}
				catch
				{
					return;
				}

				var path = item.Properties.Item("FullPath").Value.ToString();
				if (path.EndsWithAny(new[] { "AssemblyInfo.cs", "TheApplication.cs" })) return;

				using (var tidyruntimelog = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyCurrentfilelog.txt"), true))
				{
					tidyruntimelog.WriteLine(path);
				}


				foreach (var actionTypeItem in cleanupOptions.ActionTypes)
				{
					if (actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces
						&& actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives
						&& actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods)
					{
						var watch = System.Diagnostics.Stopwatch.StartNew();
						CodeCleanerHost.Run(item, actionTypeItem, cleanupOptions, true);
						watch.Stop();

						using(var tidyruntimelog = new StreamWriter( Path.Combine(Path.GetTempPath(), "TidyCurrentActionslog.txt"), true))
                        {
							tidyruntimelog.WriteLine("Phase1-" + actionTypeItem.ToString()+"-"+ watch.ElapsedMilliseconds+" ms");
						}
					}
				}


				if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces))
				{
					var watch = System.Diagnostics.Stopwatch.StartNew();
					CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces, cleanupOptions, true);
					watch.Stop();
					using (var tidyruntimelog = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyCurrentActionslog.txt"), true))
					{
						tidyruntimelog.WriteLine("Phase2-" + "NormalizeWhiteSpaces" + "-" + watch.ElapsedMilliseconds + " ms");
					}
				}

				if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives))
				{
					var watch = System.Diagnostics.Stopwatch.StartNew();
					CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives, cleanupOptions, true);
					watch.Stop();
					using (var tidyruntimelog = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyCurrentActionslog.txt"), true))
					{
						tidyruntimelog.WriteLine("Phase3-" + "OrganizeUsingDirectives" + "-" + watch.ElapsedMilliseconds + " ms");
					}
				}

				if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods))
				{
					var watch = System.Diagnostics.Stopwatch.StartNew();
					CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods, cleanupOptions, true);
					watch.Stop();
					using (var tidyruntimelog = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyCurrentActionslog.txt"), true))
					{
						tidyruntimelog.WriteLine("Phase4-" + "ConvertMsharpGeneralMethods" + "-" + watch.ElapsedMilliseconds + " ms");
					}
				}
			}
			catch (Exception e)
			{
				ErrorNotification.WriteErrorToFile(e, item.Properties.Item("FullPath").Value.ToString());
				ErrorNotification.WriteErrorToOutputWindow(e, item.Properties.Item("FullPath").Value.ToString());
				ProcessActions.GeeksProductivityToolsProcess();
			}
		}
	}
}
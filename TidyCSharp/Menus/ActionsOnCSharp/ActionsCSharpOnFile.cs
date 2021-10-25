using EnvDTE;
using Geeks.GeeksProductivityTools.Extensions;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.GeeksProductivityTools.Utils;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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

				StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), "Currentfile.txt"), true);

				sw.WriteLine(path);

				sw.Close();


				foreach (var actionTypeItem in cleanupOptions.ActionTypes)
				{
					sw = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyActions.txt"), true);
					sw.WriteLine("Phase1: " + actionTypeItem.ToString());
					sw.Close();

					if (actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces
						&& actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives
						&& actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods

						&& actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.CamelCasedMethodVariable     //Disable due to freezing TidyC#
						&& actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.CamelCasedFields)             //Disable due to freezing TidyC#
																												   //&& actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods  //Disable due to freezing TidyC#
																												   //&& actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpUIMethods       //Disable due to freezing TidyC#
																												   //&& actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpModelMethods)   //Disable due to freezing TidyC#

						CodeCleanerHost.Run(item, actionTypeItem, cleanupOptions, true);
				}


				if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces))
				{
					sw = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyActions.txt"), true);
					sw.WriteLine("Phase2: NormalizeWhiteSpaces");
					sw.Close();
					CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces, cleanupOptions, true);
				}

				if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives))
				{
					sw = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyActions.txt"), true);
					sw.WriteLine("Phase3: OrganizeUsingDirectives");
					sw.Close();

					CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives, cleanupOptions, true);

				}
				else
				{
					//window.Document.Save();
				}


				if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods))
				{
					sw = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyActions.txt"), true);
					sw.WriteLine("Phase4: ConvertMsharpGeneralMethods");
					sw.Close();
					CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods, cleanupOptions, true);
				}

				if (fileWindowMustBeOpend == false)
				{
					//window.Close(vsSaveChanges.vsSaveChangesYes);
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
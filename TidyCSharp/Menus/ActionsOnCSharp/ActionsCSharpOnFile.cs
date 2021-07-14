using EnvDTE;
using Geeks.GeeksProductivityTools.Extensions;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.GeeksProductivityTools.Utils;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using System;
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
				ErrorNotification.WriteErrorToFile(e);
				ErrorNotification.WriteErrorToOutputWindow(e);
				ProcessActions.GeeksProductivityToolsProcess();
			}
		}


		public static void ReportOnlyDoNotCleanup(ProjectItem item, CleanupOptions cleanupOptions, bool fileWindowMustBeOpend = false)
		{
			if (!item.IsCsharpFile() || item.IsCSharpDesignerFile()) return;

			try
			{
				var path = item.Properties.Item("FullPath").Value.ToString();
				if (path.EndsWithAny(new[] { "AssemblyInfo.cs", "TheApplication.cs" })) return;

				foreach (var actionTypeItem in cleanupOptions.ActionTypes)
				{
					if (actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces
						&& actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives
						&& actionTypeItem != VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods)
						CodeCleanerHost.Run(item, actionTypeItem, cleanupOptions, true);
				}

				if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces))
				{
					CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.NormalizeWhiteSpaces, cleanupOptions, true);
				}

				if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives))
				{
					//window.Document.Close(vsSaveChanges.vsSaveChangesYes);

					CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.OrganizeUsingDirectives, cleanupOptions, true);

					//if (fileWindowMustBeOpend == false)
					//{
					//	window = item.Open(Constants.vsViewKindCode);

					//	window.Activate();
					//}
				}
				else
				{
					//window.Document.Save();
				}
				if (cleanupOptions.ActionTypes.Contains(VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods))
				{
					CodeCleanerHost.Run(item, VSIX.TidyCSharp.Cleanup.CodeCleanerType.ConvertMsharpGeneralMethods, cleanupOptions, true);
				}
				if (fileWindowMustBeOpend == false)
				{
					//window.Close(vsSaveChanges.vsSaveChangesYes);
				}
			}
			catch (Exception e)
			{
				ErrorNotification.WriteErrorToFile(e);
				ErrorNotification.WriteErrorToOutputWindow(e);
				ProcessActions.GeeksProductivityToolsProcess();
			}
		}
	}
}
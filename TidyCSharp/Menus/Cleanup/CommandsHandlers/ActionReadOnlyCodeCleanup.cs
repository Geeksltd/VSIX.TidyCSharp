using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geeks.VSIX.TidyCSharp.Cleanup;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
	public class ActionReadOnlyCodeCleanup
	{
		public CleanupOptions CleanupOptions { get; private set; }
		public ActionReadOnlyCodeCleanup()
		{
			CleanupOptions = new CleanupOptions();
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.ConvertZebbleGeneralMethods));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.OrganizeUsingDirectives));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.SortClassMembers));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.SimplifyVariableDeclarations));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.RemoveAttributeKeywork));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.UpgradeCSharpSyntax));
		}

		public void RunReadOnlyCleanUp()
		{
			ActionsOnCSharp.CSharpActionDelegate.TargetAction desiredAction = ActionsOnCSharp.ActionsCSharpOnFile.ReportOnlyDoNotCleanup;
			ActionsOnCSharp.ActionCSharpOnAnyWhere.Invoke(desiredAction, CleanupOptions);
		}
	}
}

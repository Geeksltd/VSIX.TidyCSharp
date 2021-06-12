using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.MembersToExpressionBodied;

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
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.ConvertPropertiesToAutoProperties));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.ConvertFullNameTypesToBuiltInTypes));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.CompactSmallIfElseStatements));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.ConvertMsharpGeneralMethods));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.ConvertMsharpUIMethods));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.ConvertMsharpModelMethods));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.ConvertMembersToExpressionBodied,
					new CleanerItemUIInfo[] {
						new CleanerItemUIInfo
						{
							CleanerType = (int)CleanupTypes.Convert_Constructors,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(CleanupTypes), CleanupTypes.Convert_Constructors).ToString()
						},
						new CleanerItemUIInfo
						{
							CleanerType = (int)CleanupTypes.Convert_Methods,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(CleanupTypes), CleanupTypes.Convert_Methods).ToString()
						},
						new CleanerItemUIInfo
						{
							CleanerType = (int)CleanupTypes.Convert_ReadOnly_Property,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(CleanupTypes), CleanupTypes.Convert_ReadOnly_Property).ToString()
						}
					}
				));
		}

		public void RunReadOnlyCleanUp()
		{
			ActionsOnCSharp.CSharpActionDelegate.TargetAction desiredAction = ActionsOnCSharp.ActionsCSharpOnFile.ReportOnlyDoNotCleanup;
			ActionsOnCSharp.ActionCSharpOnAnyWhere.Invoke(desiredAction, CleanupOptions);
		}
	}
}

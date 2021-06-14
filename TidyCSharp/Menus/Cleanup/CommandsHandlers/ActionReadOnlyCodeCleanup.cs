using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geeks.VSIX.TidyCSharp.Cleanup;
using CamelCasedClassFieldsCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedClassFields.CleanupTypes;
using MembersToExpressionBodiedCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.MembersToExpressionBodied.CleanupTypes;

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
							CleanerType = (int)MembersToExpressionBodiedCleanupTypes.Convert_Constructors,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(MembersToExpressionBodiedCleanupTypes), MembersToExpressionBodiedCleanupTypes.Convert_Constructors).ToString()
						},
						new CleanerItemUIInfo
						{
							CleanerType = (int)MembersToExpressionBodiedCleanupTypes.Convert_Methods,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(MembersToExpressionBodiedCleanupTypes),
								MembersToExpressionBodiedCleanupTypes.Convert_Methods).ToString()
						},
						new CleanerItemUIInfo
						{
							CleanerType = (int)MembersToExpressionBodiedCleanupTypes.Convert_ReadOnly_Property,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(MembersToExpressionBodiedCleanupTypes), MembersToExpressionBodiedCleanupTypes.Convert_ReadOnly_Property).ToString()
						}
					}
				));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.CamelCasedFields, new CleanerItemUIInfo[]
				{
					new CleanerItemUIInfo
					{
						CleanerType = (int)CamelCasedClassFieldsCleanupTypes.Normal_Fields,
						ShouldBeSelectedByDefault = true,
						Name = Enum.GetName(typeof(CamelCasedClassFieldsCleanupTypes),
								CamelCasedClassFieldsCleanupTypes.Normal_Fields).ToString()
					},
					new CleanerItemUIInfo
					{
						CleanerType = (int)CamelCasedClassFieldsCleanupTypes.Const_Fields,
						ShouldBeSelectedByDefault = true,
						Name = Enum.GetName(typeof(CamelCasedClassFieldsCleanupTypes),
								CamelCasedClassFieldsCleanupTypes.Const_Fields).ToString()
					},
				}));
		}

		public void RunReadOnlyCleanUp()
		{
			ActionsOnCSharp.CSharpActionDelegate.TargetAction desiredAction = ActionsOnCSharp.ActionsCSharpOnFile.ReportOnlyDoNotCleanup;
			ActionsOnCSharp.ActionCSharpOnAnyWhere.Invoke(desiredAction, CleanupOptions);
		}
	}
}

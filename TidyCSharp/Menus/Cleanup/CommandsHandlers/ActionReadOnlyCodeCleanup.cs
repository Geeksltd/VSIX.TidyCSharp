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
using SimplyAsyncCallCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.SimplyAsyncCall.CleanupTypes;
using RemovePrivateModifierCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.RemovePrivateModifier.CleanupTypes;
using RemoveExtraThisCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.RemoveExtraThisKeyword.CleanupTypes;
using CamelCasedMethodCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedMethodVariable.CleanupTypes;

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
			//CleanupOptions.Accept(new
			//	ReadOnlyMainCleanup(CodeCleanerType.OrganizeUsingDirectives));
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
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.SimplyAsyncCalls, new CleanerItemUIInfo[] {
					new CleanerItemUIInfo
						{
							CleanerType = (int)SimplyAsyncCallCleanupTypes.Single_Expression,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(SimplyAsyncCallCleanupTypes), SimplyAsyncCallCleanupTypes.Single_Expression).ToString()
						},
				}));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.PrivateAccessModifier, new CleanerItemUIInfo[] {
					new CleanerItemUIInfo
						{
							CleanerType = (int)RemovePrivateModifierCleanupTypes.Remove_Class_Fields_Private_Modifier,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.Remove_Class_Methods_Private_Modifier).ToString()
						},
					new CleanerItemUIInfo
						{
							CleanerType = (int)RemovePrivateModifierCleanupTypes.Remove_Class_Methods_Private_Modifier,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.Remove_Class_Methods_Private_Modifier).ToString()
						},
					new CleanerItemUIInfo
						{
							CleanerType = (int)RemovePrivateModifierCleanupTypes.Remove_Class_Properties_Private_Modifier,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.Remove_Class_Properties_Private_Modifier).ToString()
						},
					new CleanerItemUIInfo
						{
							CleanerType = (int)RemovePrivateModifierCleanupTypes.Remove_Nested_Class_Private_Modifier,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.Remove_Nested_Class_Private_Modifier).ToString()
						},
				}));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.RemoveExtraThisQualification, new CleanerItemUIInfo[] {
					new CleanerItemUIInfo
						{
							CleanerType = (int)RemoveExtraThisCleanupTypes.Remove_From_Fields_Call,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(RemoveExtraThisCleanupTypes), RemoveExtraThisCleanupTypes.Remove_From_Fields_Call).ToString()
						},
					new CleanerItemUIInfo
						{
							CleanerType = (int)RemoveExtraThisCleanupTypes.Remove_From_Method_Call,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(RemoveExtraThisCleanupTypes), RemoveExtraThisCleanupTypes.Remove_From_Method_Call).ToString()
						},
					new CleanerItemUIInfo
						{
							CleanerType = (int)RemoveExtraThisCleanupTypes.Remove_From_Properties_Call,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(RemoveExtraThisCleanupTypes), RemoveExtraThisCleanupTypes.Remove_From_Properties_Call).ToString()
						},
				}));
			CleanupOptions.Accept(new
				ReadOnlyMainCleanup(CodeCleanerType.CamelCasedMethodVariable, new CleanerItemUIInfo[] {
					new CleanerItemUIInfo
						{
							CleanerType = (int)CamelCasedMethodCleanupTypes.Local_variable,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(CamelCasedMethodCleanupTypes), CamelCasedMethodCleanupTypes.Local_variable).ToString()
						},
					new CleanerItemUIInfo
						{
							CleanerType = (int)CamelCasedMethodCleanupTypes.Method_Parameter,
							ShouldBeSelectedByDefault = true,
							Name = Enum.GetName(typeof(CamelCasedMethodCleanupTypes), CamelCasedMethodCleanupTypes.Method_Parameter).ToString()
						}, }));
		}

		public void RunReadOnlyCleanUp()
		{
			ActionsOnCSharp.CSharpActionDelegate.TargetAction desiredAction = ActionsOnCSharp.ActionsCSharpOnFile.ReportOnlyDoNotCleanup;
			ActionsOnCSharp.ActionCSharpOnSolution.Invoke(desiredAction, CleanupOptions);
			System.IO.File.WriteAllText(@"c:\\done.txt", "1");
		}
	}
}

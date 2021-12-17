using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System;
using CamelCasedClassFieldsCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedClassFields.CleanupTypes;
using CamelCasedMethodCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedMethodVariable.CleanupTypes;
using MembersToExpressionBodiedCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.MembersToExpressionBodied.CleanupTypes;
using NormalizeWhitespaceCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace.CleanupTypes;
using RemoveExtraThisCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.RemoveExtraThisKeyword.CleanupTypes;
using RemovePrivateModifierCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.RemovePrivateModifier.CleanupTypes;
using SimplifyClassFieldDeclarationCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.SimplifyClassFieldDeclaration.CleanupTypes;
using SimplyAsyncCallCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.SimplyAsyncCall.CleanupTypes;

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
            // CleanupOptions.Accept(new
            // 	ReadOnlyMainCleanup(CodeCleanerType.OrganizeUsingDirectives));
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
                            CleanerType = (int)CamelCasedMethodCleanupTypes.Local_Variable,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(CamelCasedMethodCleanupTypes), CamelCasedMethodCleanupTypes.Local_Variable).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)CamelCasedMethodCleanupTypes.Method_Parameter,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(CamelCasedMethodCleanupTypes), CamelCasedMethodCleanupTypes.Method_Parameter).ToString()
                        }, }));

            CleanupOptions.Accept(new
                ReadOnlyMainCleanup(CodeCleanerType.SimplifyClassFieldDeclarations, new CleanerItemUIInfo[] {
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.Group_And_Merge_Class_Fields,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(SimplifyClassFieldDeclarationCleanupTypes), SimplifyClassFieldDeclarationCleanupTypes.Group_And_Merge_Class_Fields).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.Remove_Class_Fields_Initializer_Literal,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(SimplifyClassFieldDeclarationCleanupTypes), SimplifyClassFieldDeclarationCleanupTypes.Remove_Class_Fields_Initializer_Literal).ToString()
                        },new CleanerItemUIInfo
                        {
                            CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.Remove_Class_Fields_Initializer_Null,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(SimplifyClassFieldDeclarationCleanupTypes), SimplifyClassFieldDeclarationCleanupTypes.Remove_Class_Fields_Initializer_Null).ToString()
                        },
                    }));

            CleanupOptions.Accept(new
                ReadOnlyMainCleanup(CodeCleanerType.NormalizeWhiteSpaces, new CleanerItemUIInfo[] {
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Adding_Blank_After_Block_Close_Bracket,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Adding_Blank_After_Block_Close_Bracket).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Adding_Blank_After_Method_Close_Bracket,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Adding_Blank_After_Method_Close_Bracket).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Add_Blank_Line_Between_Statements_More_Than_One_Line,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Add_Blank_Line_Between_Statements_More_Than_One_Line).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Insert_Space_Before_Comment_Text,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Insert_Space_Before_Comment_Text).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Remove_Blank_After_Open_Bracket_And_Before_Close_Brackets,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Remove_Blank_After_Open_Bracket_And_Before_Close_Brackets).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Remove_Brackets_Of_Block_That_Has_Only_One_Statement_With_Length_Shorter_Than_80_Chars,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Remove_Brackets_Of_Block_That_Has_Only_One_Statement_With_Length_Shorter_Than_80_Chars).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Remove_Duplicate_Between_Class_Members,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Remove_Duplicate_Between_Class_Members).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Remove_Duplicate_Between_Methods_Statements,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Remove_Duplicate_Between_Methods_Statements).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Remove_Duplicate_Between_Namespace_Members,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Remove_Duplicate_Between_Namespace_Members).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Remove_Duplicate_Inside_Comments,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Remove_Duplicate_Inside_Comments).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Remove_Duplicate_Inside_Usings,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Remove_Duplicate_Inside_Usings).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Trim_The_File,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Trim_The_File).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Use_Slash_Instead_Of_Slash_Slash,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Use_Slash_Instead_Of_Slash_Slash).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Add_An_Empty_Line_After_Using_Statements,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Add_An_Empty_Line_After_Using_Statements).ToString()
                        },
                }));
        }

        public void RunReadOnlyCleanUp()
        {
            ActionsOnCSharp.CSharpActionDelegate.TargetAction desiredAction = ActionsOnCSharp.ActionsCSharpOnFile.ReportOnlyDoNotCleanup;
            ActionsOnCSharp.ActionCSharpOnSolution.Invoke(desiredAction, CleanupOptions);
        }
    }
}
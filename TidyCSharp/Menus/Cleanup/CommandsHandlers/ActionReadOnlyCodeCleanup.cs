using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System;
using CamelCasedClassFieldsCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedClassFields.CleanupTypes;
using CamelCasedMethodCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedMethodVariable.CleanupTypes;
using MembersToExpressionBodiedCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.MembersToExpressionBodied.CleanupTypes;
using NormalizeWhitespaceCleanupTypes = Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhiteSpace.CleanupTypes;
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
                            CleanerType = (int)MembersToExpressionBodiedCleanupTypes.ConvertConstructors,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(MembersToExpressionBodiedCleanupTypes), MembersToExpressionBodiedCleanupTypes.ConvertConstructors).ToString()
                        },
                        new CleanerItemUIInfo
                        {
                            CleanerType = (int)MembersToExpressionBodiedCleanupTypes.ConvertMethods,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(MembersToExpressionBodiedCleanupTypes),
                                MembersToExpressionBodiedCleanupTypes.ConvertMethods).ToString()
                        },
                        new CleanerItemUIInfo
                        {
                            CleanerType = (int)MembersToExpressionBodiedCleanupTypes.ConvertReadOnlyProperty,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(MembersToExpressionBodiedCleanupTypes), MembersToExpressionBodiedCleanupTypes.ConvertReadOnlyProperty).ToString()
                        }
                    }
                ));

            CleanupOptions.Accept(new
                ReadOnlyMainCleanup(CodeCleanerType.CamelCasedFields, new CleanerItemUIInfo[]
                {
                    new CleanerItemUIInfo
                    {
                        CleanerType = (int)CamelCasedClassFieldsCleanupTypes.NormalFields,
                        ShouldBeSelectedByDefault = true,
                        Name = Enum.GetName(typeof(CamelCasedClassFieldsCleanupTypes),
                                CamelCasedClassFieldsCleanupTypes.NormalFields).ToString()
                    },
                    new CleanerItemUIInfo
                    {
                        CleanerType = (int)CamelCasedClassFieldsCleanupTypes.ConstFields,
                        ShouldBeSelectedByDefault = true,
                        Name = Enum.GetName(typeof(CamelCasedClassFieldsCleanupTypes),
                                CamelCasedClassFieldsCleanupTypes.ConstFields).ToString()
                    },
                }));

            CleanupOptions.Accept(new
                ReadOnlyMainCleanup(CodeCleanerType.SimplyAsyncCalls, new CleanerItemUIInfo[] {
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)SimplyAsyncCallCleanupTypes.SingleExpression,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(SimplyAsyncCallCleanupTypes), SimplyAsyncCallCleanupTypes.SingleExpression).ToString()
                        },
                }));

            CleanupOptions.Accept(new
                ReadOnlyMainCleanup(CodeCleanerType.PrivateAccessModifier, new CleanerItemUIInfo[] {
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveClassFieldsPrivateModifier,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.RemoveClassMethodsPrivateModifier).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveClassMethodsPrivateModifier,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.RemoveClassMethodsPrivateModifier).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveClassPropertiesPrivateModifier,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.RemoveClassPropertiesPrivateModifier).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveNestedClassPrivateModifier,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.RemoveNestedClassPrivateModifier).ToString()
                        },
                }));

            CleanupOptions.Accept(new
                ReadOnlyMainCleanup(CodeCleanerType.RemoveExtraThisQualification, new CleanerItemUIInfo[] {
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)RemoveExtraThisCleanupTypes.RemoveFromFieldsCall,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(RemoveExtraThisCleanupTypes), RemoveExtraThisCleanupTypes.RemoveFromFieldsCall).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)RemoveExtraThisCleanupTypes.RemoveFromMethodCall,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(RemoveExtraThisCleanupTypes), RemoveExtraThisCleanupTypes.RemoveFromMethodCall).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)RemoveExtraThisCleanupTypes.RemoveFromPropertiesCall,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(RemoveExtraThisCleanupTypes), RemoveExtraThisCleanupTypes.RemoveFromPropertiesCall).ToString()
                        },
                }));

            CleanupOptions.Accept(new
                ReadOnlyMainCleanup(CodeCleanerType.CamelCasedMethodVariable, new CleanerItemUIInfo[] {
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)CamelCasedMethodCleanupTypes.LocalVariable,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(CamelCasedMethodCleanupTypes), CamelCasedMethodCleanupTypes.LocalVariable).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)CamelCasedMethodCleanupTypes.MethodParameter,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(CamelCasedMethodCleanupTypes), CamelCasedMethodCleanupTypes.MethodParameter).ToString()
                        }, }));

            CleanupOptions.Accept(new
                ReadOnlyMainCleanup(CodeCleanerType.SimplifyClassFieldDeclarations, new CleanerItemUIInfo[] {
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.GroupAndMergeClassFields,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(SimplifyClassFieldDeclarationCleanupTypes), SimplifyClassFieldDeclarationCleanupTypes.GroupAndMergeClassFields).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerLiteral,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(SimplifyClassFieldDeclarationCleanupTypes), SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerLiteral).ToString()
                        },new CleanerItemUIInfo
                        {
                            CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerNull,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(SimplifyClassFieldDeclarationCleanupTypes), SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerNull).ToString()
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
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveBlankAfterOpenBracketAndBeforeCloseBrackets,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveBlankAfterOpenBracketAndBeforeCloseBrackets).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.Remove_Brackets_Of_Block_That_Has_Only_One_Statement_With_Length_Shorter_Than_80_Chars,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.Remove_Brackets_Of_Block_That_Has_Only_One_Statement_With_Length_Shorter_Than_80_Chars).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenClassMembers,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenClassMembers).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenMethodsStatements,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenMethodsStatements).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenNamespaceMembers,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenNamespaceMembers).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideComments,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideComments).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideUsings,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideUsings).ToString()
                        },
                    new CleanerItemUIInfo
                        {
                            CleanerType = (int)NormalizeWhitespaceCleanupTypes.TrimTheFile,
                            ShouldBeSelectedByDefault = true,
                            Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.TrimTheFile).ToString()
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
            ActionsOnCSharp.CSharpAction.TargetAction desiredAction = ActionsOnCSharp.ActionsCSharpOnFile.ReportOnlyDoNotCleanup;
            ActionsOnCSharp.ActionCSharpOnSolution.Invoke(desiredAction, CleanupOptions);
        }
    }
}
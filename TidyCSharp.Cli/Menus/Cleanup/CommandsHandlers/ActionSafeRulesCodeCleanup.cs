using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;
using CamelCasedClassFieldsCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.CamelCasedClassFields.Option.CleanupTypes;
using CamelCasedMethodCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.CamelCasedMethodVariable.Option.CleanupTypes;
using NormalizeWhitespaceCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.Whitespace.Option.CleanupTypes;
using RemoveExtraThisCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.RemoveExtraThisQualification.Option.CleanupTypes;
using RemovePrivateModifierCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.PrivateModifierRemover.Option.CleanupTypes;
using SimplyAsyncCallCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.SimplyAsyncCall.Option.CleanupTypes;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers;

public class ActionSafeRulesCodeCleanup
{
    public CleanupOptions CleanupOptions { get; private set; }
    public ActionSafeRulesCodeCleanup()
    {
        CleanupOptions = new CleanupOptions();

        //CleanupOptions.Accept(new
        //    SafeRuleMainCleanup(CodeCleanerType.ConvertZebbleGeneralMethods));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.OrganizeUsingDirectives));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.SortClassMembers));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.SimplifyVariableDeclarations));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.RemoveAttributeKeywork));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.UpgradeCSharpSyntax));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.ConvertPropertiesToAutoProperties));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.ConvertFullNameTypesToBuiltInTypes));

        //CleanupOptions.Accept(new
        //    SafeRuleMainCleanup(CodeCleanerType.CompactSmallIfElseStatements));

        //CleanupOptions.Accept(new
        //    SafeRuleMainCleanup(CodeCleanerType.ConvertMsharpGeneralMethods));

        //CleanupOptions.Accept(new
        //    SafeRuleMainCleanup(CodeCleanerType.ConvertMsharpUIMethods));

        //CleanupOptions.Accept(new
        //    SafeRuleMainCleanup(CodeCleanerType.ConvertMsharpModelMethods));

        //CleanupOptions.Accept(new
        //    SafeRuleMainCleanup(CodeCleanerType.ConvertMembersToExpressionBodied,
        //        new[] {
        //            new CleanerItemUIInfo
        //            {
        //                CleanerType = (int)MembersToExpressionBodiedCleanupTypes.ConvertConstructors,
        //                ShouldBeSelectedByDefault = true,
        //                Name = nameof(MembersToExpressionBodiedCleanupTypes.ConvertConstructors)
        //            },
        //            new CleanerItemUIInfo
        //            {
        //                CleanerType = (int)MembersToExpressionBodiedCleanupTypes.ConvertMethods,
        //                ShouldBeSelectedByDefault = true,
        //                Name = nameof(MembersToExpressionBodiedCleanupTypes.ConvertMethods)
        //            },
        //            new CleanerItemUIInfo
        //            {
        //                CleanerType = (int)MembersToExpressionBodiedCleanupTypes.ConvertReadOnlyProperty,
        //                ShouldBeSelectedByDefault = true,
        //                Name = nameof(MembersToExpressionBodiedCleanupTypes.ConvertReadOnlyProperty)
        //            }
        //        }
        //    ));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.CamelCasedFields, new[]
            {
                new CleanerItemUiInfo
                {
                    CleanerType = (int)CamelCasedClassFieldsCleanupTypes.NormalFields,
                    ShouldBeSelectedByDefault = true,
                    Name =nameof( CamelCasedClassFieldsCleanupTypes.NormalFields)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)CamelCasedClassFieldsCleanupTypes.ConstFields,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof( CamelCasedClassFieldsCleanupTypes.ConstFields)
                },
            }));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.SimplyAsyncCalls, new[] {
                new CleanerItemUiInfo
                {
                    CleanerType = (int)SimplyAsyncCallCleanupTypes.SingleExpression,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(SimplyAsyncCallCleanupTypes.SingleExpression)
                },
            }));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.PrivateAccessModifier, new[] {
                new CleanerItemUiInfo
                {
                    CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveClassFieldsPrivateModifier,
                    ShouldBeSelectedByDefault = true,
                    Name =nameof(RemovePrivateModifierCleanupTypes.RemoveClassMethodsPrivateModifier)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveClassMethodsPrivateModifier,
                    ShouldBeSelectedByDefault = true,
                    Name =nameof(RemovePrivateModifierCleanupTypes.RemoveClassMethodsPrivateModifier)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveClassPropertiesPrivateModifier,
                    ShouldBeSelectedByDefault = true,
                    Name =nameof(RemovePrivateModifierCleanupTypes.RemoveClassPropertiesPrivateModifier)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveNestedClassPrivateModifier,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(RemovePrivateModifierCleanupTypes.RemoveNestedClassPrivateModifier)
                },
            }));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.RemoveExtraThisQualification, new[] {
                new CleanerItemUiInfo
                {
                    CleanerType = (int)RemoveExtraThisCleanupTypes.RemoveFromFieldsCall,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(RemoveExtraThisCleanupTypes.RemoveFromFieldsCall)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)RemoveExtraThisCleanupTypes.RemoveFromMethodCall,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(RemoveExtraThisCleanupTypes.RemoveFromMethodCall)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)RemoveExtraThisCleanupTypes.RemoveFromPropertiesCall,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(RemoveExtraThisCleanupTypes.RemoveFromPropertiesCall)
                },
            }));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.CamelCasedMethodVariable, new[] {
                new CleanerItemUiInfo
                {
                    CleanerType = (int)CamelCasedMethodCleanupTypes.LocalVariable,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(CamelCasedMethodCleanupTypes.LocalVariable)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)CamelCasedMethodCleanupTypes.MethodParameter,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(CamelCasedMethodCleanupTypes.MethodParameter)
                }, }));

        //CleanupOptions.Accept(new
        //    SafeRuleMainCleanup(CodeCleanerType.SimplifyClassFieldDeclarations, new[] {
        //        new CleanerItemUIInfo
        //            {
        //                CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.GroupAndMergeClassFields,
        //                ShouldBeSelectedByDefault = true,
        //                Name = nameof(SimplifyClassFieldDeclarationCleanupTypes.GroupAndMergeClassFields)
        //            },
        //        new CleanerItemUIInfo
        //            {
        //                CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerLiteral,
        //                ShouldBeSelectedByDefault = true,
        //                Name = nameof(SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerLiteral)
        //            },new CleanerItemUIInfo
        //            {
        //                CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerNull,
        //                ShouldBeSelectedByDefault = true,
        //                Name = nameof(SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerNull)
        //            },
        //        }));

        CleanupOptions.Accept(new
            SafeRuleMainCleanup(CodeCleanerType.NormalizeWhiteSpaces, new[] {
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.AddingBlankAfterBlockCloseBracket,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.AddingBlankAfterBlockCloseBracket)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.AddingBlankAfterMethodCloseBracket,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.AddingBlankAfterMethodCloseBracket)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.AddBlankLineBetweenStatementsMoreThanOneLine,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.AddBlankLineBetweenStatementsMoreThanOneLine)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.InsertSpaceBeforeCommentText,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.InsertSpaceBeforeCommentText)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveBlankAfterOpenBracketAndBeforeCloseBrackets,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.RemoveBlankAfterOpenBracketAndBeforeCloseBrackets)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveBracketsOfBlockThatHasOnlyOneStatementWithLengthShorterThan80Chars,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.RemoveBracketsOfBlockThatHasOnlyOneStatementWithLengthShorterThan80Chars)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenClassMembers,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenClassMembers)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenMethodsStatements,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenMethodsStatements)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenNamespaceMembers,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenNamespaceMembers)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideComments,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideComments)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideUsings,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideUsings)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.TrimTheFile,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.TrimTheFile)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.UseSlashInsteadOfSlashSlash,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.UseSlashInsteadOfSlashSlash)
                },
                new CleanerItemUiInfo
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.AddAnEmptyLineAfterUsingStatements,
                    ShouldBeSelectedByDefault = true,
                    Name = nameof(NormalizeWhitespaceCleanupTypes.AddAnEmptyLineAfterUsingStatements)
                },
            }));
    }

    public async Task RunSafeRulesCleanUpAsync()
    {
        ActionsOnCSharp.CSharpAction.TargetAction desiredAction = ActionsOnCSharp.ActionsCSharpOnFile.DoCleanupAsync;
       await ActionsOnCSharp.ActionCSharpOnSolution.InvokeAsync(desiredAction, CleanupOptions);
    }
}
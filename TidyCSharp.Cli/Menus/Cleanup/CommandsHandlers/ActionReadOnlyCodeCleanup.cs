using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra.ReadOnly;
using CamelCasedClassFieldsCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.CamelCasedClassFields.Option.CleanupTypes;
using CamelCasedMethodCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.CamelCasedMethodVariable.Option.CleanupTypes;
using MembersToExpressionBodiedCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.ConvertMembersToExpressionBodied.Option.CleanupTypes;
using NormalizeWhitespaceCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.Whitespace.Option.CleanupTypes;
using RemoveExtraThisCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.RemoveExtraThisQualification.Option.CleanupTypes;
using RemovePrivateModifierCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.PrivateModifierRemover.Option.CleanupTypes;
using SimplifyClassFieldDeclarationCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.SimplifyClassFieldDeclarations.Option.CleanupTypes;
using SimplyAsyncCallCleanupTypes = TidyCSharp.Cli.Menus.Cleanup.CommandRunners.SimplyAsyncCall.Option.CleanupTypes;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers;

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
            ReadOnlyMainCleanup(CodeCleanerType.ConvertMsharpUiMethods));

        CleanupOptions.Accept(new
            ReadOnlyMainCleanup(CodeCleanerType.ConvertMsharpModelMethods));

        CleanupOptions.Accept(new
            ReadOnlyMainCleanup(CodeCleanerType.ConvertMembersToExpressionBodied,
                new CleanerItemUiInfo[] {
                    new()
                    {
                        CleanerType = (int)MembersToExpressionBodiedCleanupTypes.ConvertConstructors,
                        ShouldBeSelectedByDefault = true,
                        Name = Enum.GetName(typeof(MembersToExpressionBodiedCleanupTypes), MembersToExpressionBodiedCleanupTypes.ConvertConstructors).ToString()
                    },
                    new()
                    {
                        CleanerType = (int)MembersToExpressionBodiedCleanupTypes.ConvertMethods,
                        ShouldBeSelectedByDefault = true,
                        Name = Enum.GetName(typeof(MembersToExpressionBodiedCleanupTypes),
                            MembersToExpressionBodiedCleanupTypes.ConvertMethods).ToString()
                    },
                    new()
                    {
                        CleanerType = (int)MembersToExpressionBodiedCleanupTypes.ConvertReadOnlyProperty,
                        ShouldBeSelectedByDefault = true,
                        Name = Enum.GetName(typeof(MembersToExpressionBodiedCleanupTypes), MembersToExpressionBodiedCleanupTypes.ConvertReadOnlyProperty).ToString()
                    }
                }
            ));

        CleanupOptions.Accept(new
            ReadOnlyMainCleanup(CodeCleanerType.CamelCasedFields, new CleanerItemUiInfo[]
            {
                new()
                {
                    CleanerType = (int)CamelCasedClassFieldsCleanupTypes.NormalFields,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(CamelCasedClassFieldsCleanupTypes),
                        CamelCasedClassFieldsCleanupTypes.NormalFields).ToString()
                },
                new()
                {
                    CleanerType = (int)CamelCasedClassFieldsCleanupTypes.ConstFields,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(CamelCasedClassFieldsCleanupTypes),
                        CamelCasedClassFieldsCleanupTypes.ConstFields).ToString()
                },
            }));

        CleanupOptions.Accept(new
            ReadOnlyMainCleanup(CodeCleanerType.SimplyAsyncCalls, new CleanerItemUiInfo[] {
                new()
                {
                    CleanerType = (int)SimplyAsyncCallCleanupTypes.SingleExpression,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(SimplyAsyncCallCleanupTypes), SimplyAsyncCallCleanupTypes.SingleExpression).ToString()
                },
            }));

        CleanupOptions.Accept(new
            ReadOnlyMainCleanup(CodeCleanerType.PrivateAccessModifier, new CleanerItemUiInfo[] {
                new()
                {
                    CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveClassFieldsPrivateModifier,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.RemoveClassMethodsPrivateModifier).ToString()
                },
                new()
                {
                    CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveClassMethodsPrivateModifier,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.RemoveClassMethodsPrivateModifier).ToString()
                },
                new()
                {
                    CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveClassPropertiesPrivateModifier,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.RemoveClassPropertiesPrivateModifier).ToString()
                },
                new()
                {
                    CleanerType = (int)RemovePrivateModifierCleanupTypes.RemoveNestedClassPrivateModifier,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(RemovePrivateModifierCleanupTypes), RemovePrivateModifierCleanupTypes.RemoveNestedClassPrivateModifier).ToString()
                },
            }));

        CleanupOptions.Accept(new
            ReadOnlyMainCleanup(CodeCleanerType.RemoveExtraThisQualification, new CleanerItemUiInfo[] {
                new()
                {
                    CleanerType = (int)RemoveExtraThisCleanupTypes.RemoveFromFieldsCall,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(RemoveExtraThisCleanupTypes), RemoveExtraThisCleanupTypes.RemoveFromFieldsCall).ToString()
                },
                new()
                {
                    CleanerType = (int)RemoveExtraThisCleanupTypes.RemoveFromMethodCall,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(RemoveExtraThisCleanupTypes), RemoveExtraThisCleanupTypes.RemoveFromMethodCall).ToString()
                },
                new()
                {
                    CleanerType = (int)RemoveExtraThisCleanupTypes.RemoveFromPropertiesCall,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(RemoveExtraThisCleanupTypes), RemoveExtraThisCleanupTypes.RemoveFromPropertiesCall).ToString()
                },
            }));

        CleanupOptions.Accept(new
            ReadOnlyMainCleanup(CodeCleanerType.CamelCasedMethodVariable, new CleanerItemUiInfo[] {
                new()
                {
                    CleanerType = (int)CamelCasedMethodCleanupTypes.LocalVariable,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(CamelCasedMethodCleanupTypes), CamelCasedMethodCleanupTypes.LocalVariable).ToString()
                },
                new()
                {
                    CleanerType = (int)CamelCasedMethodCleanupTypes.MethodParameter,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(CamelCasedMethodCleanupTypes), CamelCasedMethodCleanupTypes.MethodParameter).ToString()
                }, }));

        CleanupOptions.Accept(new
            ReadOnlyMainCleanup(CodeCleanerType.SimplifyClassFieldDeclarations, new CleanerItemUiInfo[] {
                new()
                {
                    CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.GroupAndMergeClassFields,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(SimplifyClassFieldDeclarationCleanupTypes), SimplifyClassFieldDeclarationCleanupTypes.GroupAndMergeClassFields).ToString()
                },
                new()
                {
                    CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerLiteral,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(SimplifyClassFieldDeclarationCleanupTypes), SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerLiteral).ToString()
                },new()
                {
                    CleanerType = (int)SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerNull,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(SimplifyClassFieldDeclarationCleanupTypes), SimplifyClassFieldDeclarationCleanupTypes.RemoveClassFieldsInitializerNull).ToString()
                },
            }));

        CleanupOptions.Accept(new
            ReadOnlyMainCleanup(CodeCleanerType.NormalizeWhiteSpaces, new CleanerItemUiInfo[] {
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.AddingBlankAfterBlockCloseBracket,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.AddingBlankAfterBlockCloseBracket).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.AddingBlankAfterMethodCloseBracket,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.AddingBlankAfterMethodCloseBracket).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.AddBlankLineBetweenStatementsMoreThanOneLine,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.AddBlankLineBetweenStatementsMoreThanOneLine).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.InsertSpaceBeforeCommentText,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.InsertSpaceBeforeCommentText).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveBlankAfterOpenBracketAndBeforeCloseBrackets,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveBlankAfterOpenBracketAndBeforeCloseBrackets).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveBracketsOfBlockThatHasOnlyOneStatementWithLengthShorterThan80Chars,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveBracketsOfBlockThatHasOnlyOneStatementWithLengthShorterThan80Chars).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenClassMembers,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenClassMembers).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenMethodsStatements,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenMethodsStatements).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenNamespaceMembers,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveDuplicateBetweenNamespaceMembers).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideComments,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideComments).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideUsings,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.RemoveDuplicateInsideUsings).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.TrimTheFile,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.TrimTheFile).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.UseSlashInsteadOfSlashSlash,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.UseSlashInsteadOfSlashSlash).ToString()
                },
                new()
                {
                    CleanerType = (int)NormalizeWhitespaceCleanupTypes.AddAnEmptyLineAfterUsingStatements,
                    ShouldBeSelectedByDefault = true,
                    Name = Enum.GetName(typeof(NormalizeWhitespaceCleanupTypes), NormalizeWhitespaceCleanupTypes.AddAnEmptyLineAfterUsingStatements).ToString()
                },
            }));
    }

    public void RunReadOnlyCleanUp()
    {
        ActionsOnCSharp.CSharpAction.TargetAction desiredAction = ActionsOnCSharp.ActionsCSharpOnFile.ReportOnlyDoNotCleanupAsync;
        ActionsOnCSharp.ActionCSharpOnSolution.InvokeAsync(desiredAction, CleanupOptions);
    }
}
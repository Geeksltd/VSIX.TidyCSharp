using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class CodeCleanerFactory
    {
        public static ICodeCleaner Create(CodeCleanerType type, CleanupOptions cleanupOptions, bool IsReportOnly = false)
        {
            switch (type)
            {
                case CodeCleanerType.NormalizeWhiteSpaces:
                    return new WhiteSpaceNormalizer() { Options = cleanupOptions.WhiteSpaceNormalizer, IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.ConvertMembersToExpressionBodied:
                    return new ConvertMembersToExpressionBodied() { Options = cleanupOptions.ConvertMembersToExpressionBodied, IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.ConvertFullNameTypesToBuiltInTypes:
                    return new ConvertFullNameTypesToBuiltInTypes() { IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.SortClassMembers:
                    return new SortClassMembers() { IsReportOnlyMode = IsReportOnly }; ;
                case CodeCleanerType.SimplyAsyncCalls:
                    return new SimplyAsyncCalls() { Options = cleanupOptions.SimplyAsyncCall, IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.SimplifyClassFieldDeclarations:
                    return new SimplifyClassFieldDeclarations() { Options = cleanupOptions.SimplifyClassFieldDeclarations, IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.RemoveAttributeKeywork:
                    return new RemoveAttributeKeywork() { IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.CompactSmallIfElseStatements:
                    return new CompactSmallIfElseStatements() { IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.RemoveExtraThisQualification:
                    return new RemoveExtraThisQualification() { Options = cleanupOptions.RemoveExtraThisQualification, IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.CamelCasedMethodVariable:
                    return new CamelCasedLocalVariable() { Options = cleanupOptions.CamelCasedLocalVariable, IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.CamelCasedFields:
                    return new CamelCasedFields() { Options = cleanupOptions.CamelCasedFields, IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.PrivateAccessModifier:
                    return new PrivateModifierRemover() { IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.OrganizeUsingDirectives:
                    return new UsingDirectiveOrganizer() { Options = cleanupOptions.PrivateModifierRemover, IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.SimplifyVariableDeclarations:
                    return new SimplifyVariableDeclarations() { IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.ConvertPropertiesToAutoProperties:
                    return new ConvertPropertiesToAutoProperties() { IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.ConvertMsharpUIMethods:
                    return new MSharpUICleaner() { IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.ConvertMsharpModelMethods:
                    return new MSharpModelCleaner() { IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.ConvertMsharpGeneralMethods:
                    return new MSharpGeneralCleaner() { IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.ConvertZebbleGeneralMethods:
                    return new ZebbleCleaner() { IsReportOnlyMode = IsReportOnly };
                case CodeCleanerType.UpgradeCSharpSyntax:
                    return new CSharpSyntaxUpgrade() { IsReportOnlyMode = IsReportOnly };
                default: return null; // TODO
            }
        }
    }
}
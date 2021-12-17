using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class CodeCleanerFactory
    {
        public static ICodeCleaner Create(CodeCleanerType type, CleanupOptions cleanupOptions, bool isReportOnly = false)
        {
            switch (type)
            {
                case CodeCleanerType.NormalizeWhiteSpaces:
                    return new WhiteSpaceNormalizer() { Options = cleanupOptions.WhiteSpaceNormalizer, IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.ConvertMembersToExpressionBodied:
                    return new ConvertMembersToExpressionBodied() { Options = cleanupOptions.ConvertMembersToExpressionBodied, IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.ConvertFullNameTypesToBuiltInTypes:
                    return new ConvertFullNameTypesToBuiltInTypes() { IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.SortClassMembers:
                    return new SortClassMembers() { IsReportOnlyMode = isReportOnly }; ;
                case CodeCleanerType.SimplyAsyncCalls:
                    return new SimplyAsyncCalls() { Options = cleanupOptions.SimplyAsyncCall, IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.SimplifyClassFieldDeclarations:
                    return new SimplifyClassFieldDeclarations() { Options = cleanupOptions.SimplifyClassFieldDeclarations, IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.RemoveAttributeKeywork:
                    return new RemoveAttributeKeywork() { IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.CompactSmallIfElseStatements:
                    return new CompactSmallIfElseStatements() { IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.RemoveExtraThisQualification:
                    return new RemoveExtraThisQualification() { Options = cleanupOptions.RemoveExtraThisQualification, IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.CamelCasedMethodVariable:
                    return new CamelCasedLocalVariable() { Options = cleanupOptions.CamelCasedLocalVariable, IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.CamelCasedFields:
                    return new CamelCasedFields() { Options = cleanupOptions.CamelCasedFields, IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.PrivateAccessModifier:
                    return new PrivateModifierRemover() { IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.OrganizeUsingDirectives:
                    return new UsingDirectiveOrganizer() { Options = cleanupOptions.PrivateModifierRemover, IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.SimplifyVariableDeclarations:
                    return new SimplifyVariableDeclarations() { IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.ConvertPropertiesToAutoProperties:
                    return new ConvertPropertiesToAutoProperties() { IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.ConvertMsharpUIMethods:
                    return new MSharpUICleaner() { IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.ConvertMsharpModelMethods:
                    return new MSharpModelCleaner() { IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.ConvertMsharpGeneralMethods:
                    return new MSharpGeneralCleaner() { IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.ConvertZebbleGeneralMethods:
                    return new ZebbleCleaner() { IsReportOnlyMode = isReportOnly };
                case CodeCleanerType.UpgradeCSharpSyntax:
                    return new CSharpSyntaxUpgrade() { IsReportOnlyMode = isReportOnly };
                default: return null; // TODO
            }
        }
    }
}
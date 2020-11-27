using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class CodeCleanerFactory
    {
        public static ICodeCleaner Create(CodeCleanerType type, CleanupOptions cleanupOptions)
        {
            switch (type)
            {
                case CodeCleanerType.NormalizeWhiteSpaces:
                    return new WhiteSpaceNormalizer() { Options = cleanupOptions.WhiteSpaceNormalizer };
                case CodeCleanerType.ConvertMembersToExpressionBodied:
                    return new ConvertMembersToExpressionBodied() { Options = cleanupOptions.ConvertMembersToExpressionBodied };
                case CodeCleanerType.ConvertFullNameTypesToBuiltInTypes:
                    return new ConvertFullNameTypesToBuiltInTypes();
                case CodeCleanerType.SortClassMembers:
                    return new SortClassMembers();
                case CodeCleanerType.SimplyAsyncCalls:
                    return new SimplyAsyncCalls() { Options = cleanupOptions.SimplyAsyncCall };
                case CodeCleanerType.SimplifyClassFieldDeclarations:
                    return new SimplifyClassFieldDeclarations() { Options = cleanupOptions.SimplifyClassFieldDeclarations };
                case CodeCleanerType.RemoveAttributeKeywork:
                    return new RemoveAttributeKeywork();
                case CodeCleanerType.CompactSmallIfElseStatements:
                    return new CompactSmallIfElseStatements();
                case CodeCleanerType.RemoveExtraThisQualification:
                    return new RemoveExtraThisQualification() { Options = cleanupOptions.RemoveExtraThisQualification };
                case CodeCleanerType.CamelCasedMethodVariable:
                    return new CamelCasedLocalVariable() { Options = cleanupOptions.CamelCasedLocalVariable };
                case CodeCleanerType.CamelCasedFields:
                    return new CamelCasedFields() { Options = cleanupOptions.CamelCasedFields };
                case CodeCleanerType.PrivateAccessModifier:
                    return new PrivateModifierRemover();
                case CodeCleanerType.OrganizeUsingDirectives:
                    return new UsingDirectiveOrganizer() { Options = cleanupOptions.PrivateModifierRemover };
                case CodeCleanerType.SimplifyVariableDeclarations:
                    return new SimplifyVariableDeclarations();
                case CodeCleanerType.ConvertPropertiesToAutoProperties:
                    return new ConvertPropertiesToAutoProperties();
                case CodeCleanerType.ConvertMsharpUIMethods:
                    return new MSharpUICleaner();
                case CodeCleanerType.ConvertMsharpModelMethods:
                    return new MSharpModelCleaner();

                default: return null; // TODO
            }
        }
    }
}
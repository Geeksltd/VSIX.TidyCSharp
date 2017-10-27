using Geeks.GeeksProductivityTools.Definition;
using Geeks.VSIX.TidyCSharp.Cleanup;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class CodeCleanerFactory
    {
        public static ICodeCleaner Create(CodeCleanerType type)
        {
            switch (type)
            {
                case CodeCleanerType.NormalizeWhiteSpaces:
                    return new WhiteSpaceNormalizer();
                case CodeCleanerType.ConvertMembersToExpressionBodied:
                    return new ConvertMembersToExpressionBodied();
                case CodeCleanerType.ConvertFullNameTypesToBuiltInTypes:
                    return new ConvertFullNameTypesToBuiltInTypes();
                case CodeCleanerType.SortClassMembersCommand:
                    return new SortClassMembers();
                case CodeCleanerType.SimplyAsyncCallsCommand:
                    return new SimplyAsyncCalls();
                case CodeCleanerType.SimplifyClassFieldDeclarationsCommand:
                    return new SimplifyClassFieldDeclarations();
                case CodeCleanerType.RemoveAttributeKeyworkCommand:
                    return new RemoveAttributeKeywork();
                case CodeCleanerType.CompactSmallIfElseStatementsCommand:
                    return new CompactSmallIfElseStatements();
                case CodeCleanerType.RemoveExtraThisQualification:
                    return new RemoveExtraThisQualification();
                case CodeCleanerType.CamelCasedLocalVariable:
                    return new CamelCasedLocalVariable();
                case CodeCleanerType.CamelCasedFields:
                    return new CamelCasedFields();
                case CodeCleanerType.CamelCasedConstFields:
                    return new CamelCasedConstFields();
                case CodeCleanerType.PrivateAccessModifier:
                    return new PrivateModifierRemover();
                case CodeCleanerType.OrganizeUsingDirectives:
                    return new UsingDirectiveOrganizer();
                default: return null; // TODO
            }
        }
    }
}

using System;
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
                    return new SimplyAsyncCalls();
                case CodeCleanerType.SimplifyClassFieldDeclarations:
                    return new SimplifyClassFieldDeclarations() { Options = cleanupOptions.SimplifyClassFieldDeclarations };
                case CodeCleanerType.RemoveAttributeKeywork:
                    return new RemoveAttributeKeywork();
                case CodeCleanerType.CompactSmallIfElseStatements:
                    return new CompactSmallIfElseStatements();
                case CodeCleanerType.RemoveExtraThisQualification:
                    return new RemoveExtraThisQualification() { Options = cleanupOptions.RemoveExtraThisQualification };
                case CodeCleanerType.CamelCasedLocalVariable:
                    return new CamelCasedLocalVariable();
                case CodeCleanerType.CamelCasedFields:
                    return new CamelCasedFields();
                case CodeCleanerType.CamelCasedConstFields:
                    return new CamelCasedConstFields();
                case CodeCleanerType.PrivateAccessModifier:
                    return new PrivateModifierRemover();
                case CodeCleanerType.OrganizeUsingDirectives:
                    return new UsingDirectiveOrganizer() { Options = cleanupOptions.PrivateModifierRemover };
                default: return null; // TODO
            }
        }
    }
}

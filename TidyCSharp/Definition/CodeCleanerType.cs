namespace Geeks.GeeksProductivityTools.Definition
{
    public enum CodeCleanerType
    {
        Unspecified = 0,
        NormalizeWhiteSpaces = 1,
        PrivateAccessModifier = 2,
        OrganizeUsingDirectives = 3,
        ConvertMembersToExpressionBodied = 4,
        ConvertFullNameTypesToBuiltInTypes = 5,
        SimplyAsyncCallsCommand = 6,
        SortClassMembersCommand = 7,
        SimplifyClassFieldDeclarationsCommand = 8,
        RemoveAttributeKeyworkCommand = 9,
        CompactSmallIfElseStatementsCommand = 10,
        RemoveExtraThisQualification = 11,
        CamelCasedLocalVariable = 12,
        CamelCasedFields = 13,
        CamelCasedConstFields = 14,
        All = 35
    }
}

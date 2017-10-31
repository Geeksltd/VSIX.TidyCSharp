using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    [Flags]
    public enum CodeCleanerType
    {
        [CleanupItem(Title = "Remove and Sort Using Directives", FirstOrder = 0)]
        OrganizeUsingDirectives = 0x04,

        [CleanupItem(Title = "Normalize white spaces", FirstOrder = 1, SubItemType = typeof(NormalizeWhitespace.CleanupTypes))]
        NormalizeWhiteSpaces = 0x01,

        [CleanupItem(Title = "Remove unnecessary \"private\";", FirstOrder = 2, SubItemType = typeof(RemovePrivateModifier.CleanupTypes))]
        PrivateAccessModifier = 0x02,

        [CleanupItem(Title = "Small methods properties -> Expression bodied", FirstOrder = 3, SubItemType = typeof(SimplifyClassFieldDeclaration.CleanupTypes))]
        ConvertMembersToExpressionBodied = 0x08,

        [CleanupItem(Title = "Simply async calls", FirstOrder = 4, SubItemType = typeof(SimplyAsyncCall.CleanupTypes))]
        SimplyAsyncCalls = 0x20,

        [CleanupItem(Title = "Compact class field declarations", FirstOrder = 5, SubItemType = typeof(SimplifyClassFieldDeclaration.CleanupTypes))]
        SimplifyClassFieldDeclarations = 0x80,

        [CleanupItem(Title = "Compact variables declarations", FirstOrder = 5)]
        SimplifyVariableDeclarations = 0x8000,

        [CleanupItem(Title = "Remove unnecessary 'this.'", FirstOrder = 6, SubItemType = typeof(RemoveExtraThisKeyword.CleanupTypes))]
        RemoveExtraThisQualification = 0x400,

        [CleanupItem(Title = "Local variables -> camelCased", FirstOrder = 7, SubItemType = typeof(CamelCasedMethodVariable.CleanupTypes))]
        CamelCasedMethodVariable = 0x800,

        [CleanupItem(Title = "\"_something\" -> \"Something\" or \"something\"", FirstOrder = 8, SubItemType = typeof(CamelCasedClassFields.CleanupTypes))]
        CamelCasedFields = 0x1000,

        [CleanupItem(Title = "Move constructors before methods", FirstOrder = 9)]
        SortClassMembers = 0x40,

        [CleanupItem(Title = "Remove unnecessary \"Attribute\" (e.g. [SomethingAttribute] -> [Something]", FirstOrder = 10)]
        RemoveAttributeKeywork = 0x100,

        [CleanupItem(Title = "Compact small if/else blocks", FirstOrder = 11)]
        CompactSmallIfElseStatements = 0x200,

        [CleanupItem(Title = "Use C# alias type names (e.g. \"System.Int32\" -> \"int\")", FirstOrder = 11)]
        ConvertFullNameTypesToBuiltInTypes = 0x10,
    }
}
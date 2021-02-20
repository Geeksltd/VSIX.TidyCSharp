using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    [Flags]
    public enum CodeCleanerType   //CleanerMainType
    {
        [CleanupItem(Title = "Remove and sort Usings", FirstOrder = 0)]
        OrganizeUsingDirectives = 0x04,

        [CleanupItem(Title = "Normalize white spaces", FirstOrder = 1, SubItemType = typeof(NormalizeWhitespace.CleanupTypes))]
        NormalizeWhiteSpaces = 0x01,

        [CleanupItem(Title = "Remove unnecessary explicit 'private' where it's the default", FirstOrder = 2, SubItemType = typeof(RemovePrivateModifier.CleanupTypes))]
        PrivateAccessModifier = 0x02,

        [CleanupItem(Title = "Small methods properties -> Expression bodied", FirstOrder = 3, SubItemType = typeof(MembersToExpressionBodied.CleanupTypes))]
        ConvertMembersToExpressionBodied = 0x08,

        [CleanupItem(Title = "Simplify async calls", FirstOrder = 4, SubItemType = typeof(SimplyAsyncCall.CleanupTypes))]
        SimplyAsyncCalls = 0x20,

        [CleanupItem(Title = "Compact multiple class field declarations into one line", FirstOrder = 5, SubItemType = typeof(SimplifyClassFieldDeclaration.CleanupTypes))]
        SimplifyClassFieldDeclarations = 0x80,

        [CleanupItem(Title = "Use 'var' for variable declarations", FirstOrder = 5)]
        SimplifyVariableDeclarations = 0x8000,

        [CleanupItem(Title = "Convert traditional properties to auto-properties", FirstOrder = 5)]
        ConvertPropertiesToAutoProperties = 0x10000,

        [CleanupItem(Title = "Remove unnecessary \"this.\"", FirstOrder = 6, SubItemType = typeof(RemoveExtraThisKeyword.CleanupTypes))]
        RemoveExtraThisQualification = 0x400,

        [CleanupItem(Title = "Use camelCase for...", FirstOrder = 7, SubItemType = typeof(CamelCasedMethodVariable.CleanupTypes))]
        CamelCasedMethodVariable = 0x800,

        [CleanupItem(Title = "Class field and const casing...", FirstOrder = 8, SubItemType = typeof(CamelCasedClassFields.CleanupTypes))]
        CamelCasedFields = 0x1000,

        [CleanupItem(Title = "Move constructors before methods", FirstOrder = 9)]
        SortClassMembers = 0x40,

        [CleanupItem(Title = "Remove unnecessary 'Attribute' (e.g. [SomethingAttribute] -> [Something])", FirstOrder = 10)]
        RemoveAttributeKeywork = 0x100,

        [CleanupItem(Title = "Compact small if/else blocks", FirstOrder = 11, SelectedByDefault = false)]
        CompactSmallIfElseStatements = 0x200,

        [CleanupItem(Title = "Use C# alias type names (e.g. System.Int32 -> int)", FirstOrder = 11)]
        ConvertFullNameTypesToBuiltInTypes = 0x10,

        [CleanupItem(Title = "Renew M# UI methods", FirstOrder = 12)]
        ConvertMsharpUIMethods = 0x25,

        [CleanupItem(Title = "Renew M# Model methods", FirstOrder = 13)]
        ConvertMsharpModelMethods = 0x26,

        [CleanupItem(Title = "Renew M# General Statements", FirstOrder = 14)]
        ConvertMsharpGeneralMethods = 0x27,

        [CleanupItem(Title = "Renew Zebble General Statements", FirstOrder = 15)]
        ConvertZebbleGeneralMethods = 0x28,

        [CleanupItem(Title = "Upgrade C# Syntax", FirstOrder = 16)]
        UpgradeCSharpSyntax = 0x29
    }
}
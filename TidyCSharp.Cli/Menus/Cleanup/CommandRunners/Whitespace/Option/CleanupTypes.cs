using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.Whitespace.Option;

[Flags]
public enum CleanupTypes
{
    [CleanupItem(FirstOrder = 1, Title = "Trim the file")]
    TrimTheFile = 0x80,

    [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between namespace members")]
    RemoveDuplicateBetweenNamespaceMembers = 0x04,

    [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between class members")]
    RemoveDuplicateBetweenClassMembers = 0x02,

    [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between method statement")]
    RemoveDuplicateBetweenMethodsStatements = 0x01,

    [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between comments")]
    RemoveDuplicateInsideComments = 0x08,

    [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between \"Usings ..\"")]
    RemoveDuplicateInsideUsings = 0x10,

    [CleanupItem(FirstOrder = 1, Title = "No blank lines immediately after { and immediately before }")]
    RemoveBlankAfterOpenBracketAndBeforeCloseBrackets = 0x20,

    /// <summary>
    /// Insert_one_space_between_the_comment_delimiter_and_the_comment_text,
    /// </summary>
    [CleanupItem(FirstOrder = 1, Title = "Space before comment text: //TODO: => // TODO:")]
    InsertSpaceBeforeCommentText = 0x40,

    [CleanupItem(FirstOrder = 1, Title = "Blank line between methods.")]
    AddingBlankAfterMethodCloseBracket = 0x100,

    [CleanupItem(FirstOrder = 1, Title = "Blank line between } and the next statement.")]
    AddingBlankAfterBlockCloseBracket = 0x200,

    [CleanupItem(FirstOrder = 1, Title = "No { and } for short blocks (single statement and < 80 chars)")]
    RemoveBracketsOfBlockThatHasOnlyOneStatementWithLengthShorterThan80Chars = 0x400,

    [CleanupItem(FirstOrder = 1, Title = "Add Blank line between Statements more that one line")]
    AddBlankLineBetweenStatementsMoreThanOneLine = 0x500,

    [CleanupItem(FirstOrder = 1, Title = "Use \\n instead of \\r\\n")]
    UseSlashInsteadOfSlashSlash = 0x600,

    [CleanupItem(FirstOrder = 1, Title = "Add an empty line after using statements")]
    AddAnEmptyLineAfterUsingStatements = 0x700,
}
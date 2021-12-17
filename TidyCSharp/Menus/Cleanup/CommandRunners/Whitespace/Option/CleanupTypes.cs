using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(FirstOrder = 1, Title = "Trim the file")]
        Trim_The_File = 0x80,

        [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between namespace members")]
        Remove_Duplicate_Between_Namespace_Members = 0x04,

        [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between class members")]
        Remove_Duplicate_Between_Class_Members = 0x02,

        [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between method statement")]
        Remove_Duplicate_Between_Methods_Statements = 0x01,

        [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between comments")]
        Remove_Duplicate_Inside_Comments = 0x08,

        [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between \"Usings ..\"")]
        Remove_Duplicate_Inside_Usings = 0x10,

        [CleanupItem(FirstOrder = 1, Title = "No blank lines immediately after { and immediately before }")]
        Remove_Blank_After_Open_Bracket_And_Before_Close_Brackets = 0x20,

        /// <summary>
        /// Insert_one_space_between_the_comment_delimiter_and_the_comment_text,
        /// </summary>
        [CleanupItem(FirstOrder = 1, Title = "Space before comment text: //TODO: => // TODO:")]
        Insert_Space_Before_Comment_Text = 0x40,

        [CleanupItem(FirstOrder = 1, Title = "Blank line between methods.")]
        Adding_Blank_After_Method_Close_Bracket = 0x100,

        [CleanupItem(FirstOrder = 1, Title = "Blank line between } and the next statement.")]
        Adding_Blank_After_Block_Close_Bracket = 0x200,

        [CleanupItem(FirstOrder = 1, Title = "No { and } for short blocks (single statement and < 80 chars)")]
        Remove_Brackets_Of_Block_That_Has_Only_One_Statement_With_Length_Shorter_Than_80_Chars = 0x400,

        [CleanupItem(FirstOrder = 1, Title = "Add Blank line between Statements more that one line")]
        Add_Blank_Line_Between_Statements_More_Than_One_Line = 0x500,

        [CleanupItem(FirstOrder = 1, Title = "Use \\n instead of \\r\\n")]
        Use_Slash_Instead_Of_Slash_Slash = 0x600,

        [CleanupItem(FirstOrder = 1, Title = "Add an empty line after using statements")]
        Add_An_Empty_Line_After_Using_Statements = 0x700,
    }
}
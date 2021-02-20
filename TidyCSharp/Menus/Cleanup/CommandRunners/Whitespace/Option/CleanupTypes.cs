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
        Remove_DBL_Between_Namespace_Members = 0x04,

        [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between class members")]
        Remove_DBL_Between_Class_Members = 0x02,

        [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between method statement")]
        Remove_DBL_Between_Methods_Statements = 0x01,

        [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between comments")]
        Remove_DBL_Inside_Comments = 0x08,

        [CleanupItem(FirstOrder = 1, Title = "No duplicate blank lines between \"Usings ..\"")]
        Remove_DBL_Inside_Usings = 0x10,

        [CleanupItem(FirstOrder = 1, Title = "No blank lines immediately after { and immediately before }")]
        Remove_BLs_after_Open_Bracket_and_Before_Close_Brackets = 0x20,

        /// <summary>
        /// Insert_one_space_between_the_comment_delimiter_and_the_comment_text,
        /// </summary>
        [CleanupItem(FirstOrder = 1, Title = "Space before comment text: //TODO: => // TODO:")]
        Insert_Space_Before_Comment_Text = 0x40,

        [CleanupItem(FirstOrder = 1, Title = "Blank line between methods.")]
        Adding_Blank_after_Method_Close_Bracket = 0x100,

        [CleanupItem(FirstOrder = 1, Title = "Blank line between } and the next statement.")]
        Adding_Blank_after_Block_Close_Bracket = 0x200,

        [CleanupItem(FirstOrder = 1, Title = "No { and } for short blocks (single statement and < 80 chars)")]
        Remove_Brackets_of_block_that_has_only_one_statement_with_length_shorter_than_80_chars = 0x400,

        [CleanupItem(FirstOrder = 1, Title = "Add Blank line between Statements more that one line")]
        Add_blank_line_between_statements_more_than_one_line = 0x500,

        [CleanupItem(FirstOrder = 1, Title = "Use \\n instead of \\r\\n")]
        Use_slash_n_instead_of_slash_sr_slash_n = 0x600,
    }
}
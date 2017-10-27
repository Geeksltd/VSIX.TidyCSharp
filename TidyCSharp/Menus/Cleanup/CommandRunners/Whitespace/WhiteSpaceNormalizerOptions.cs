﻿using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace
{
    [Flags]
    public enum WSN_CleanupTypes
    {
        Remove_DBL_Between_Methods_Statements = 0x01,
        Remove_DBL_Between_Class_Members = 0x02,
        Remove_DBL_Between_Namespace_Members = 0x04,
        Remove_DBL_Inside_Comments = 0x08,
        Remove_DBL_Inside_Usings = 0x10,
        Remove_BLs_after_Open_Bracket_and_Before_Close_Brackets = 0x20,
        /// <summary>
        /// Insert_one_space_between_the_comment_delimiter_and_the_comment_text,
        /// </summary>
        Insert_Space_Before_Comment_Text = 0x40,
        Trim_The_File = 0x80,
        Adding_Blank_after_Method_Close_Bracket = 0x100,
        Adding_Blank_after_Block_Close_Bracket = 0x200,
        Remove_Brackets_of_block_that_has_only_one_statement_with_length_shorter_than_70_chars = 0x400,
    }

    public class WhiteSpaceNormalizerOptions
    {
        public const int BLOCK_SINGLE_STATEMENT_MAX_LENGTH = 70;

        public WSN_CleanupTypes? CleanupItems { get; set; }
    }

}

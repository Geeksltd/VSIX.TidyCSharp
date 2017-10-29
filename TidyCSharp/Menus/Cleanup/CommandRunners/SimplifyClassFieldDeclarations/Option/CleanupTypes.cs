using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.SimplifyClassFieldDeclaration
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Remove Class Fields Initializer when it is default value(default literal value for value types); (Except const)")]
        Remove_Class_Fields_Initializer_Literal = 0x01,

        [CleanupItem(Title = "Remove Class Fields Initializer when it is default value(null for reference types); (Except const)")]
        Remove_Class_Fields_Initializer_Null = 0x02,

        [CleanupItem(Title = "Group and Merge class fields by their type and moved to the same line, If its total size is less than 70 or 80 chars")]
        Group_And_Merge_class_fields = 0x04,
    }
}
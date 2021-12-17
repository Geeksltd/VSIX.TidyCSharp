using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.SimplifyClassFieldDeclaration
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Remove unnecessary explicit \"=0\" or \"=false\" from class fields.")]
        Remove_Class_Fields_Initializer_Literal = 0x01,

        [CleanupItem(Title = "Remove unnecessary explicit \"=null\" from class fields.")]
        Remove_Class_Fields_Initializer_Null = 0x02,

        [CleanupItem(Title = "Declare multiple class fields [with the same type] on the same line (if total size < 80 chars)", SelectedByDefault = false)]
        Group_And_Merge_Class_Fields = 0x04,
    }
}
using System;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.RemovePrivateModifier
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Remove 'private' from nested classes")]
        Remove_Nested_Class_Private_Modifier = 0x02,

        [CleanupItem(Title = "Remove 'private' from fields")]
        Remove_Class_Fields_Private_Modifier = 0x04,

        [CleanupItem(Title = "Remove 'private' from methods")]
        Remove_Class_Methods_Private_Modifier = 0x08,

        [CleanupItem(Title = "Remove 'private' from properties")]
        Remove_Class_Properties_Private_Modifier = 0x10,
    }
}

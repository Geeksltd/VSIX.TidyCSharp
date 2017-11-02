using System;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.RemovePrivateModifier
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Remove Class Private Modifier")]
        Remove_Class_Private_Modifier = 0x01,

        [CleanupItem(Title = "Remove Nested Class Private Modifier")]
        Remove_Nested_Class_Private_Modifier = 0x02,

        [CleanupItem(Title = "Remove Class Fields Private Modifier")]
        Remove_Class_Fields_Private_Modifier = 0x04,

        [CleanupItem(Title = "Remove Class Methods Private Modifier")]
        Remove_Class_Methods_Private_Modifier = 0x08,

        [CleanupItem(Title = "Remove Class Properties Private Modifier")]
        Remove_Class_Properties_Private_Modifier = 0x10,
    }

}

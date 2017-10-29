using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.RemovePrivateModifier
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem()]
        Remove_Class_Private_Modifier = 0x01,

        [CleanupItem()]
        Remove_Nested_Class_Private_Modifier = 0x02,

        [CleanupItem()]
        Remove_Class_Fields_Private_Modifier = 0x04,

        [CleanupItem()]
        Remove_Class_Methods_Private_Modifier = 0x08,

        [CleanupItem()]
        Remove_Class_Properties_Private_Modifier = 0x10,
    }

}

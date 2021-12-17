using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedMethodVariable
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Method parameters")]
        Method_Parameter = 0x01,

        [CleanupItem(Title = "Local variable declarations")]
        Local_Variable = 0x02,
    }
}
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedMethodVariable
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Method parameters")]
        MethodParameter = 0x01,

        [CleanupItem(Title = "Local variable declarations")]
        LocalVariable = 0x02,
    }
}
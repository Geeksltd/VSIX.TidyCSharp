using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedMethodVariable
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Method Parameter")]
        Method_Parameter = 0x01,

        [CleanupItem(Title = "Local variable")]
        Local_variable = 0x02,
    }
}
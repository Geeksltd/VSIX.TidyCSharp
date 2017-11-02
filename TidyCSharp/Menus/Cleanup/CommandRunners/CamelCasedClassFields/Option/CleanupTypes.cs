using System;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedClassFields
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Normal Fields")]
        Normal_Fields = 0x01,

        [CleanupItem(Title = "Const Fields")]
        Const_Fields = 0x02,
    }
}
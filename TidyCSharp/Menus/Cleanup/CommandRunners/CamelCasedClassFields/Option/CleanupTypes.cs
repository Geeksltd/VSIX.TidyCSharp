using System;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedClassFields
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Class fields: Change _something -> Something or something")]
        Normal_Fields = 0x01,

        [CleanupItem(Title = "Const fields: USE_THIS_FORMAT")]
        Const_Fields = 0x02,
    }
}
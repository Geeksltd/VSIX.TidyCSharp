using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedClassFields
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Class fields: Change _something -> Something or something")]
        NormalFields = 0x01,

        [CleanupItem(Title = "Const fields: USE_THIS_FORMAT")]
        ConstFields = 0x02,
    }
}
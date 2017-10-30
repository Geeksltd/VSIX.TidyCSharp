using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.RemoveExtraThisKeyword
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Remove From Method Call")]
        Remove_From_Method_Call = 0x01,

        //[CleanupItem(Title = "Remove From virual Method Call")]
        //Remove_From_virual_Method_Call = 0x02,

        [CleanupItem(Title = "Remove From Fields Call")]
        Remove_From_Fields_Call = 0x04,

        [CleanupItem(Title = "Remove From Properties Call")]
        Remove_From_Properties_Call = 0x08,
    }
}
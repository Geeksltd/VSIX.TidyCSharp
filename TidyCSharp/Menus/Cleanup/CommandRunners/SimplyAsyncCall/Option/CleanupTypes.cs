using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.SimplyAsyncCall
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Remove unnecessary async / await pair (simply return the task).")]
        SingleExpression = 0x02,
    }
}
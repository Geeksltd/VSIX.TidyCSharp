using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.SimplyAsyncCall
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "return x;")]
        Single_Return_Statement = 0x01,

        [CleanupItem(Title = "DoSomething();")]
        Single_Expression = 0x02,
    }
}
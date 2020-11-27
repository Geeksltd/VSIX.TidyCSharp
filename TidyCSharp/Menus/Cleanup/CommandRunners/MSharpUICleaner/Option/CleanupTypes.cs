using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.CleanMSharpUI
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Create Shortcute for Save and Cancel ")]
        Save_Cancel_Expresion = 0x02,
    }
}
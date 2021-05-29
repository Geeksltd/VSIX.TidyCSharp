using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geeks.VSIX.TidyCSharp.Cleanup;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class ActionReadOnlyCodeCleanup
    {
        public CleanupOptions CleanupOptions { get; private set; }
        public ActionReadOnlyCodeCleanup()
        {
            CleanupOptions = new CleanupOptions();
            CleanupOptions.Accept(new
                ReadOnlyMainCleanup(CodeCleanerType.ConvertZebbleGeneralMethods));
        }

        public void RunReadOnlyCleanUp()
        {
            ActionsOnCSharp.CSharpActionDelegate.TargetAction desiredAction = ActionsOnCSharp.ActionsCSharpOnFile.ReportOnlyDoNotCleanup;
            ActionsOnCSharp.ActionCSharpOnAnyWhere.Invoke(desiredAction, CleanupOptions);
        }
    }
}

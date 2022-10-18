using Geeks.GeeksProductivityTools.Menus.Cleanup.CommandsHandlers.Infra;
using Geeks.GeeksProductivityTools.Utility;
using Geeks.VSIX.TidyCSharp;
using Microsoft.VisualStudio.Shell;
using System;
using System.Windows.Forms;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class ActionCustomCodeCleanup : BaseCodeCleanupCommand
    {
        public ActionCustomCodeCleanup(OleMenuCommandService menu)
            : base(menu, PkgCmdIDList.CmdCustomUpAllActions)
        { }

        protected override void Callback(object sender, EventArgs e)
        {
            var messageBoxResult = MessageBoxDisplay.Show(new MessageBoxDisplay.MessageBoxArgs
            {
                Message = Resources.WarnOnCodeCleanUp,
                Caption = Resources.WarningCaptionCleanup,
                Button = MessageBoxButtons.OKCancel,
                Icon = MessageBoxIcon.Warning
            });

            if (CleanupOptionForm.Instance.ShowDialog() == DialogResult.Cancel) return;

            ActionsOnCSharp.CSharpAction.TargetAction desiredAction = ActionsOnCSharp.ActionsCSharpOnFile.DoCleanup;

            if (CleanupOptionForm.Instance.CleanupOptions != null)
            {
                ActionsOnCSharp.ActionCSharpOnAnywhere.Invoke(desiredAction, CleanupOptionForm.Instance.CleanupOptions);
                TidyCSharpPackage.Instance.SaveSolutionChanges();
            }
        }
    }
}
using System;
using Microsoft.VisualStudio.Shell;
using Geeks.GeeksProductivityTools.Menus.Cleanup.CommandsHandlers.Infra;
using System.Windows.Forms;
using Geeks.GeeksProductivityTools.Utility;
using Geeks.VSIX.TidyCSharp;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class ActionCustomCodeCleanup : BaseCodeCleanupCommand
    {
        public ActionCustomCodeCleanup(OleMenuCommandService menu)
            : base(menu, PkgCmdIDList.CmdCustomUpAllActions)
        { }

        protected override void CallBack(object sender, EventArgs e)
        {
            var messageBoxResult = MessageBoxDisplay.Show(new MessageBoxDisplay.MessageBoxArgs
            {
                Message = Resources.WarnOnCodeCleanUp,
                Caption = Resources.WarningCaptionCleanup,
                Button = MessageBoxButtons.OKCancel,
                Icon = MessageBoxIcon.Warning
            });

            if (messageBoxResult != DialogResult.OK) return;

            var dialogResult = CleanupOptionForm.Instance.ShowDialog();

            if (dialogResult == DialogResult.Cancel) return;

            ActionsOnCSharp.CSharpActionDelegate.TargetAction desiredAction = ActionsOnCSharp.ActionsCSharpOnFile.DoCleanup;

            if (CleanupOptionForm.Instance.CleanupOptions != null)
            {
                ActionsOnCSharp.ActionCSharpOnAnyWhere.Invoke(desiredAction, CleanupOptionForm.Instance.CleanupOptions);
                TidyCSharpPackage.Instance.SaveSolutionChanges();
            }
        }
    }
}

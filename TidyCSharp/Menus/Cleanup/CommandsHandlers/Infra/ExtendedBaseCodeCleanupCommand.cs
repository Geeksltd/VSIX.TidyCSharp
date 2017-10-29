using System;
using System.Windows.Forms;
using Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp;
using Geeks.GeeksProductivityTools.Utility;
using Microsoft.VisualStudio.Shell;
using Geeks.VSIX.TidyCSharp;
using Geeks.VSIX.TidyCSharp.Cleanup;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public abstract class ExtendedBaseCodeCleanupCommand : BaseCodeCleanupCommand
    {
        protected CodeCleanerType CleanerType { get; private set; }

        protected ExtendedBaseCodeCleanupCommand(OleMenuCommandService menu, uint commandID, CodeCleanerType cleanerType)
            : base(menu, commandID)
        {
            CleanerType = cleanerType;
        }

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

            CSharpActionDelegate.TargetAction desiredAction = ActionsCSharpOnFile.DoCleanup;
            var commandGuid = (sender as OleMenuCommand).CommandID.Guid;

            if (commandGuid == GuidList.GuidCleanupCmdSet)
            {
                ActionCSharpOnAnyWhere.Invoke(desiredAction,
                    new VSIX.TidyCSharp.Cleanup.CommandsHandlers.CleanupOptions
                    {
                        //ActionTypes = new[] { CleanerType }
                    }
                );
                GeeksProductivityToolsPackage.Instance.SaveSolutionChanges();
            }
            else return;
        }
    }
}

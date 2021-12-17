using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public abstract class BaseCodeCleanupCommand
    {
        protected OleMenuCommandService Menu { get; private set; }
        protected uint CommandId { get; private set; }
        protected BaseCodeCleanupCommand(OleMenuCommandService menu, uint commandId)
        {
            Menu = menu;
            CommandId = commandId;
        }

        protected abstract void Callback(object sender, EventArgs e);

        protected void Item_BeforeQueryStatus(object sender, EventArgs e)
        {
            var cmd = sender as OleMenuCommand;
            var activeDoc = App.Dte.ActiveDocument;

            if (null != cmd && activeDoc != null)
            {
                var fileName = App.Dte.ActiveDocument.FullName.ToUpper();
                cmd.Visible = true;
            }
        }

        public virtual void SetupCommands()
        {
            var menuCommandID = new CommandID(GuidList.GuidCleanupCmdSet, (int)CommandId);
            var menuItem = new OleMenuCommand(Callback, menuCommandID);
            menuItem.BeforeQueryStatus += Item_BeforeQueryStatus;
            Menu.AddCommand(menuItem);
        }
    }
}
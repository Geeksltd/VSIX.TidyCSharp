using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public abstract class BaseCodeCleanupCommand
    {
        protected OleMenuCommandService Menu { get; private set; }
        protected uint CommandID { get; private set; }
        protected BaseCodeCleanupCommand(OleMenuCommandService menu, uint commandID)
        {
            Menu = menu;
            CommandID = commandID;
        }

        protected abstract void CallBack(object sender, EventArgs e);

        protected void Item_BeforeQueryStatus(object sender, EventArgs e)
        {
            var cmd = sender as OleMenuCommand;
            var activeDoc = App.DTE.ActiveDocument;

            if (null != cmd && activeDoc != null)
            {
                var fileName = App.DTE.ActiveDocument.FullName.ToUpper();
                cmd.Visible = true;
            }
        }

        public virtual void SetupCommands()
        {
            var menuCommandID = new CommandID(GuidList.GuidCleanupCmdSet, (int)CommandID);
            var menuItem = new OleMenuCommand(CallBack, menuCommandID);
            menuItem.BeforeQueryStatus += Item_BeforeQueryStatus;
            Menu.AddCommand(menuItem);
        }
    }
}
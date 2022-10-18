using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup.CommandsHandlers.Infra
{
    public partial class CleanupOptionForm : Form
    {
        public static CleanupOptionForm Instance { get; set; } = new CleanupOptionForm();
        public CleanupOptions CleanupOptions { get; private set; }
        public IList<Control> AllControls { get; private set; } = new List<Control>();

        CleanupOptionForm()
        {
            InitializeComponent();
            mainPanel.Padding = new Padding(5, 5, 5, 0);
            MaximumSize = new Size(1000, Screen.PrimaryScreen.WorkingArea.Height);
            base.ShowInTaskbar = false;
            base.WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(0xF0, 0xF0, 0xF0);

            CreateControls();
            LoadFromSetting();
        }

        void CreateControls()
        {
            CleanupItemUserControl.CreateControls(typeof(CodeCleanerType), cleanupTypeItem => CreateCleanupTypeItemControl(cleanupTypeItem), true);

            foreach (var itemControl in AllControls.OrderByDescending(x => x.Height))
            {
                if(itemControl.IsDisposed) continue;
                itemControl.TabIndex = TAB_INDEX_START++;

                if (rightTableLayoutPanel.Height + itemControl.Height >= leftTableLayoutPanel.Height)
                {
                    leftTableLayoutPanel.Controls.Add(itemControl);
                }
                else
                {
                    rightTableLayoutPanel.Controls.Add(itemControl);
                }
            }
        }

        int TAB_INDEX_START = 3;

        void CreateCleanupTypeItemControl(CleanerItemUIInfo cleanupTypeItem)
        {
            var newControl = new CleanupItemUserControl()
            {
                Dock = DockStyle.Top,
                AutoSize = false,
            };

            newControl.Init((CodeCleanerType)cleanupTypeItem.CleanerType);
            AllControls.Add(newControl);
        }

        void DeserializeValues(string strValue)
        {
            try
            {
                var choices = strValue.Split(new string[] { CleanupOptions.To_String_Seprator }, System.StringSplitOptions.RemoveEmptyEntries);

                var controls = rightTableLayoutPanel.Controls.OfType<CleanupItemUserControl>()
                    .Union(leftTableLayoutPanel.Controls.OfType<CleanupItemUserControl>());

                foreach (var item in choices)
                {
                    var choiceItem = item.Split(new string[] { CleanupOptions.To_String_Seprator2 }, System.StringSplitOptions.RemoveEmptyEntries);

                    var cleanUpType = (CodeCleanerType)int.Parse(choiceItem[0]);
                    var isSelected = bool.Parse(choiceItem[1]);

                    var selectedControls = controls.FirstOrDefault(c => c.MainCleanupItemType == cleanUpType);

                    selectedControls?.SetMainItemCheckState(isSelected);
                    selectedControls?.SetItemsCheckState(int.Parse(choiceItem[2]), true);
                }
            }
            catch
            {
            }
        }

        void LoadFromSetting()
        {
            if (string.IsNullOrEmpty(Settings.Default.CleanupChoices))
            {
                foreach (IMainCleanup control in rightTableLayoutPanel.Controls.OfType<IMainCleanup>()
                    .Union(leftTableLayoutPanel.Controls.OfType<IMainCleanup>()))
                    control.ResetItemsCheckState();

                return;
            }

            DeserializeValues(Settings.Default.CleanupChoices);
            return;
        }

        void ApplyCleanup()
        {
            CleanupOptions = new CleanupOptions();

            foreach (CleanupItemUserControl item in rightTableLayoutPanel.Controls)
                CleanupOptions.Accept(item);

            foreach (CleanupItemUserControl item in leftTableLayoutPanel.Controls)
                CleanupOptions.Accept(item);

            Settings.Default.CleanupChoices = CleanupOptions.SerializeValues();

            if (string.IsNullOrEmpty(Settings.Default.CleanupChoices))
            {
                Settings.Default.CleanupChoices = "null";
            }

            Settings.Default.Save();
        }

        void btnApply_Click(object sender, EventArgs e)
        {
            ApplyCleanup();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
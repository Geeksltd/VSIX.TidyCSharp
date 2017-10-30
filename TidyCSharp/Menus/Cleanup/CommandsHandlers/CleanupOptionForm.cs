using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using static Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.CleanupItemUserControl;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Properties;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup.CommandsHandlers.Infra
{
    public partial class CleanupOptionForm : Form
    {
        static CleanupOptionForm()
        {
            Instance = new CleanupOptionForm();
        }
        //public static CleanupOptionForm Instance
        //{
        //    get
        //    {
        //        return new CleanupOptionForm();
        //    }
        //}
        public static CleanupOptionForm Instance { get; set; }
        public CleanupOptions CleanupOptions { get; private set; }

        CleanupOptionForm()
        {
            InitializeComponent();
            base.ShowInTaskbar = false;
            base.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(0xF0, 0xF0, 0xF0);
            this.Load += CleanupOptionForm_Load;
            CreateControls();
            LoadFromSetting();
        }


        private void CleanupOptionForm_Load(object sender, EventArgs e)
        {
        }

        private void CreateControls()
        {
            CleanupItemUserControl.CreateControls(typeof(CodeCleanerType), checkBoxItem => NewCheckboxItem(checkBoxItem), true);
        }

        int TAB_INDEX_START = 3;

        private void NewCheckboxItem(CheckBoxItemInfo checkBoxItem)
        {
            var newSubControl = new CleanupItemUserControl()
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                TabIndex = TAB_INDEX_START++,
            };

            newSubControl.Init((CodeCleanerType)checkBoxItem.CleanerType);

            mainPanel.Controls.Add(newSubControl);
            this.Height += newSubControl.Height;
        }


        private void LoadFromSetting()
        {
            if (string.IsNullOrEmpty(Settings.Default.CleanupChoices))
            {
                return;
            }
            try
            {

                var choices = Settings.Default.CleanupChoices.Split(new string[] { CleanupOptions.TO_STRING_SEPRATOR }, StringSplitOptions.RemoveEmptyEntries);
                int value = 0;
                foreach (var item in choices)
                {
                    var choiceItem = item.Split(new string[] { CleanupOptions.TO_STRING_SEPRATOR2 }, StringSplitOptions.RemoveEmptyEntries);

                    var cleanUpType = (CodeCleanerType)int.Parse(choiceItem[0]);

                    foreach (CleanupItemUserControl control in mainPanel.Controls.OfType<CleanupItemUserControl>())
                    {
                        if (control.MainCleanupItemType != cleanUpType) continue;

                        control.SetSubItems(int.Parse(choiceItem[1]));
                    }
                }
            }
            catch
            {

            }
        }

        private void ApplyCleanup()
        {
            CleanupOptions = new CleanupOptions();

            foreach (CleanupItemUserControl item in mainPanel.Controls)
            {
                CleanupOptions.Accept(item);
            }


            Settings.Default.CleanupChoices = CleanupOptions.ToString();
            Settings.Default.Save();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            ApplyCleanup();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void checkboxWhitespaceNormalizer_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}

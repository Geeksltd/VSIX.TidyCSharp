using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using static Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.CleanupItemUserControl;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Cleanup;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup.CommandsHandlers.Infra
{
    public partial class CleanupOptionForm : Form
    {
        static CleanupOptionForm()
        {
            Instance = new CleanupOptionForm();
        }
        public static CleanupOptionForm Instance2
        {
            get
            {
                return new CleanupOptionForm();
            }
        }
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

        private void NewCheckboxItem(CheckBoxItem checkBoxItem)
        {
            var newSubControl = new CleanupItemUserControl()
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                TabIndex = TAB_INDEX_START++,
            };

            newSubControl.Init((CodeCleanerType)checkBoxItem.CleanerType);

            mainPanel.Controls.Add(newSubControl);
        }


        private void LoadFromSetting()
        {
            ////if (string.IsNullOrEmpty(Settings.Default.CleanupChoices))
            ////{
            ////    for (int i = 0; i < checkedListBoxWhitespaceNormalizer.Items.Count; i++)
            ////    {
            ////        checkedListBoxWhitespaceNormalizer.SetItemChecked(i, true);
            ////    }
            ////    return;
            ////}

            ////var choices = Settings.Default.CleanupChoices.Split(',');
            ////int value = 0;
            ////foreach (var item in choices)
            ////{
            ////    if (int.TryParse(item, out value))
            ////    {
            ////        if (Enum.IsDefined(typeof(CodeCleanerType), value))
            ////        {
            ////            CodeCleanerType enumValue = (CodeCleanerType)Enum.ToObject(typeof(CodeCleanerType), value);

            ////            var foundItem = checkedListBoxWhitespaceNormalizer.Items.OfType<CheckBoxItem>().FirstOrDefault(x => x.CleanerType == enumValue);

            ////            if (foundItem == null) continue;

            ////            checkedListBoxWhitespaceNormalizer.SetItemChecked(checkedListBoxWhitespaceNormalizer.Items.IndexOf(foundItem), true);
            ////        }
            ////    }
            ////}
        }

        private void ApplyCleanup()
        {
            CleanupOptions = new CleanupOptions();

            foreach (CleanupItemUserControl item in mainPanel.Controls)
            {
                CleanupOptions.Accept(item);
            }
            ////var SelectedTypes = checkedListBoxWhitespaceNormalizer.CheckedItems?.Cast<CheckBoxItem>().Select(x => x.CleanerType).ToArray();
            ////SelectedTypes = SortSelectedTypes(SelectedTypes);

            ////CleanupOptions = new CleanupOptions() { ActionTypes = SelectedTypes };

            ////Settings.Default.CleanupChoices = string.Join(",", SelectedTypes.Select(x => (int)x));
            ////Settings.Default.Save();
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

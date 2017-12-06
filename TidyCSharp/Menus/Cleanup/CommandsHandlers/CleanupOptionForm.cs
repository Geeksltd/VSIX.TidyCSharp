using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Geeks.VSIX.TidyCSharp.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Properties;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup.CommandsHandlers.Infra
{
    public partial class CleanupOptionForm : Form
    {
        static CleanupOptionForm _Instance = new CleanupOptionForm();
        public static CleanupOptionForm Instance
        {
            get
            {
                return _Instance;
            }
            set
            {
                _Instance = value;
            }
        }
        public CleanupOptions CleanupOptions { get; private set; }

        CleanupOptionForm()
        {
            InitializeComponent();
            mainPanel.Padding = new Padding(5, 5, 5, 0);
            base.ShowInTaskbar = false;
            base.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(0xF0, 0xF0, 0xF0);

            CreateControls();
            LoadFromSetting();
        }

        void CreateControls()
        {
            CleanupItemUserControl.CreateControls(typeof(CodeCleanerType), cleanupTypeItem => CreateCleanupTypeItemControl(cleanupTypeItem), true);
        }

        int TAB_INDEX_START = 3;

        void CreateCleanupTypeItemControl(CleanerItemUIInfo cleanupTypeItem)
        {
            var newControl = new CleanupItemUserControl()
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                TabIndex = TAB_INDEX_START++,
            };

            newControl.Init((CodeCleanerType)cleanupTypeItem.CleanerType);

            mainPanel.Controls.Add(newControl);
            //this.Height += newSubControl.Height;
        }



        void DeserializeValues(string strValue)
        {
            try
            {
                var choices = strValue.Split(new string[] { CleanupOptions.TO_STRING_SEPRATOR }, System.StringSplitOptions.RemoveEmptyEntries);

                var controls = mainPanel.Controls.OfType<CleanupItemUserControl>();

                foreach (var item in choices)
                {
                    var choiceItem = item.Split(new string[] { CleanupOptions.TO_STRING_SEPRATOR2 }, System.StringSplitOptions.RemoveEmptyEntries);

                    var cleanUpType = (CodeCleanerType)int.Parse(choiceItem[0]);
                    var isSelected = bool.Parse(choiceItem[1]);

                    var selectedControls = controls.FirstOrDefault(c => c.MainCleanupItemType == cleanUpType);

                    selectedControls.SetMainItemCheckState(isSelected);
                    selectedControls.SetItemsCheckState(int.Parse(choiceItem[2]), true);
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
                foreach (IMainCleanup control in mainPanel.Controls.OfType<IMainCleanup>())
                {
                    control.ResetItemsCheckState();
                }

                return;
            }
            DeserializeValues(Settings.Default.CleanupChoices);
            return;
        }

        private void ApplyCleanup()
        {
            CleanupOptions = new CleanupOptions();

            foreach (CleanupItemUserControl item in mainPanel.Controls)
            {
                CleanupOptions.Accept(item);
            }


            Settings.Default.CleanupChoices = CleanupOptions.SerializeValues();
            if (string.IsNullOrEmpty(Settings.Default.CleanupChoices))
            {
                Settings.Default.CleanupChoices = "null";
            }
            Settings.Default.Save();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            ApplyCleanup();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Geeks.GeeksProductivityTools.Definition;
using Geeks.GeeksTidyCSharp.Properties;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup.CommandsHandlers.Infra
{
    public partial class CleanupOptionForm : Form
    {
        static CleanupOptionForm()
        {
            Instance = new CleanupOptionForm();
        }
        public static CleanupOptionForm Instance { get; set; }
        public CodeCleanerType[] SelectedTypes { get; private set; }

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
            NewCheckbox(CodeCleanerType.NormalizeWhiteSpaces, "Normalize white spaces");
            NewCheckbox(CodeCleanerType.PrivateAccessModifier, "Remove unnecessary \"private\";");
            NewCheckbox(CodeCleanerType.ConvertMembersToExpressionBodied, "Small methods properties -> Expression bodied");
            NewCheckbox(CodeCleanerType.ConvertFullNameTypesToBuiltInTypes, "Use C# alias type names (e.g. \"System.Int32\" -> \"int\")");
            NewCheckbox(CodeCleanerType.SimplyAsyncCallsCommand, "Simply async calls");
            NewCheckbox(CodeCleanerType.SortClassMembersCommand, "Move constructors before methods");
            NewCheckbox(CodeCleanerType.SimplifyClassFieldDeclarationsCommand, "Compact class field declarations");
            NewCheckbox(CodeCleanerType.RemoveAttributeKeyworkCommand, "Remove unnecessary \"Attribute\" (e.g. [SomethingAttribute] -> [Something]");
            NewCheckbox(CodeCleanerType.CompactSmallIfElseStatementsCommand, "Compact small if/else blocks");
            NewCheckbox(CodeCleanerType.RemoveExtraThisQualification, "Remove unnecessary 'this.'");
            NewCheckbox(CodeCleanerType.CamelCasedLocalVariable, "Local variables -> camelCased");
            NewCheckbox(CodeCleanerType.CamelCasedFields, "\"_something\" -> \"Something\" or \"something\"");
            NewCheckbox(CodeCleanerType.CamelCasedConstFields, "Const names \"Something\" -> \"SOME_THING\"");
        }

        private void NewCheckbox(CodeCleanerType cleanerType, string title)
        {
            checkedListBox1.Items.Add(new CheckBoxItem { Name = title, CleanerType = cleanerType });
        }

        private void LoadFromSetting()
        {
            if (string.IsNullOrEmpty(Settings.Default.CleanupChoices))
            {
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    checkedListBox1.SetItemChecked(i, true);
                }
                return;
            }

            var choices = Settings.Default.CleanupChoices.Split(',');
            int value = 0;
            foreach (var item in choices)
            {
                if (int.TryParse(item, out value))
                {
                    if (Enum.IsDefined(typeof(CodeCleanerType), value))
                    {
                        CodeCleanerType enumValue = (CodeCleanerType)Enum.ToObject(typeof(CodeCleanerType), value);

                        var foundItem = checkedListBox1.Items.OfType<CheckBoxItem>().FirstOrDefault(x => x.CleanerType == enumValue);

                        if (foundItem == null) continue;

                        checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf(foundItem), true);
                    }
                }
            }
        }

        private void ApplyCleanup()
        {
            SelectedTypes = checkedListBox1.CheckedItems?.Cast<CheckBoxItem>().Select(x => x.CleanerType).ToArray();
            SelectedTypes = SortSelectedTypes(SelectedTypes);

            Settings.Default.CleanupChoices = string.Join(",", SelectedTypes.Select(x => (int)x));
            Settings.Default.Save();
        }
        private CodeCleanerType[] SortSelectedTypes(CodeCleanerType[] selectedTypes)
        {
            return selectedTypes.OrderByDescending(x => (int)x).ToArray();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            ApplyCleanup();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public class CheckBoxItem
        {
            public string Name { get; set; }
            public CodeCleanerType CleanerType { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

    }
}

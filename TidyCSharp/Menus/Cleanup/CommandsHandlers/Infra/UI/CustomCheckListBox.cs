using System.Linq;
using System.Windows.Forms;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.Infra
{
    public partial class CustomCheckListBox : UserControl
    {
        public CustomCheckListBox() => InitializeComponent();

        public void AddItem(CleanerItemUIInfo itemInfo)
        {
            var newControl =
                new CustomCheckBox()
                {
                    Text = itemInfo.Name,
                    Info = itemInfo,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    MaximumSize = new System.Drawing.Size(460, 0),
                    AutoSize = true,
                    Dock = DockStyle.Top,
                };
            Controls.Add(newControl);
        }

        public CleanerItemUIInfo[] GetCheckedItems()
        {
            var selectedTypes = Controls.OfType<CustomCheckBox>().Where(x => x.Checked).Select(x => x.Info);
            return selectedTypes.OrderBy(x => x.Order).ToArray();
        }

        public bool HasItems => Controls.OfType<CustomCheckBox>().Any();

        public CleanerItemUIInfo[] GetItems()
        {
            return Controls.OfType<CustomCheckBox>().Select(x => x.Info).ToArray();
        }

        public void SetItemsChecked(int value, bool checkedState)
        {
            foreach (var item in Controls.OfType<CustomCheckBox>())
            {
                if (item.Info != null && ((item.Info.CleanerType & value) == item.Info.CleanerType))
                {
                    item.Checked = checkedState;
                }
            }
        }

        public void ResetItemsCheckState()
        {
            foreach (var item in Controls.OfType<CustomCheckBox>())
                item.Checked = item.Info.ShouldBeSelectedByDefault;

        }
    }
}
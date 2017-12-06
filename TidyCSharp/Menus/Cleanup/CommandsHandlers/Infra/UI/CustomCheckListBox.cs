using System.Linq;
using System.Windows.Forms;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.Infra
{
    public partial class CustomCheckListBox : UserControl
    {
        public CustomCheckListBox()
        {
            InitializeComponent();
        }

        public void AddItem(CleanerItemUIInfo itemInfo)
        {
            var newControl =
                new CustomCheckBox()
                {
                    Text = itemInfo.Name,
                    Info = itemInfo,
                    Dock = DockStyle.Top,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                };
            this.Controls.Add(newControl);
        }

        public CleanerItemUIInfo[] GetCheckedItems()
        {
            var selectedTypes = this.Controls.OfType<CustomCheckBox>().Where(x => x.Checked).Select(x => x.Info);
            return selectedTypes.OrderBy(x => x.Order).ToArray();
        }
        public bool HasItems => this.Controls.OfType<CustomCheckBox>().Any();

        public CleanerItemUIInfo[] GetItems()
        {
            return this.Controls.OfType<CustomCheckBox>().Select(x => x.Info).ToArray();
        }

        public void SetItemsChecked(int value, bool checkedState)
        {
            foreach (var item in this.Controls.OfType<CustomCheckBox>())
            {
                if (item.Info != null && ((item.Info.CleanerType & value) == item.Info.CleanerType))
                {
                    item.Checked = checkedState;
                }
            }
        }
        public void ResetItemsCheckState()
        {
            foreach (var item in this.Controls.OfType<CustomCheckBox>())
            {
                item.Checked = item.Info.ShouldBeSelectedByDefault;
            }
        }
    }
}

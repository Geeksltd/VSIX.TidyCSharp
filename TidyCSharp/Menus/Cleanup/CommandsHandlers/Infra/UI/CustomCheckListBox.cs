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

        public void AddItem(CheckBoxItemInfo item)
        {
            var newControl =
                new CustomCheckBox()
                {
                    Text = item.Name,
                    Info = item,
                    Dock = DockStyle.Top,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                };
            this.Controls.Add(newControl);
        }

        public CheckBoxItemInfo[] GetCheckedItems()
        {
            var selectedTypes = this.Controls.OfType<CustomCheckBox>().Where(x => x.Checked).Select(x => x.Info);
            return selectedTypes.OrderBy(x => x.Order).ToArray();
        }
        public bool HasItems
        {
            get
            {
                return this.Controls.OfType<CustomCheckBox>().Any();
            }
        }
        public CheckBoxItemInfo[] GetItems()
        {
            return this.Controls.OfType<CustomCheckBox>().Select(x => x.Info).ToArray();
        }

        public void SetCheckedItems(int value)
        {
            foreach (var item in this.Controls.OfType<CustomCheckBox>())
            {
                if (item.Info != null && ((item.Info.CleanerType & value) == item.Info.CleanerType))
                {
                    item.Checked = true;
                }
            }
        }
        public void ReSetSubItems(bool selectAll = false)
        {
            foreach (var item in this.Controls.OfType<CustomCheckBox>())
            {
                item.Checked = selectAll;
            }
        }
    }
}

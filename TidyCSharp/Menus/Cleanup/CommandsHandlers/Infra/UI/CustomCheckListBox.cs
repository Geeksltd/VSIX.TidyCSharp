using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.CleanupItemUserControl;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.Infra
{
    public partial class CustomCheckListBox : UserControl
    {
        public const int HEIGHT_OF_CHECKBOX = 15;
        public CustomCheckListBox()
        {
            InitializeComponent();
        }

        public void AddItem(CheckBoxItemInfo item)
        {
            var newControl = new CustomCheckBox() { Text = item.Name, Info = item, Dock = DockStyle.Top, Height = HEIGHT_OF_CHECKBOX };
            this.Controls.Add(newControl);
        }

        public CheckBoxItemInfo[] GetCheckedItems()
        {
            var selectedTypes = this.Controls.OfType<CustomCheckBox>().Where(x=>x.Checked).Select(x => x.Info);
            return selectedTypes.OrderBy(x => x.Order).ToArray();
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
    }
}

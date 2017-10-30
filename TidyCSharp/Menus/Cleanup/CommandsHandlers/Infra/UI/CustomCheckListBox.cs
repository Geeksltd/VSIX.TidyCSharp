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
            //this.Height = 0;
            //this.AutoSize = false;
        }

        public void AddItem(CheckBoxItem item)
        {
            var newControl = new CustomCheckBox() { Text = item.Name, Tag = item, Dock = DockStyle.Top, Height = HEIGHT_OF_CHECKBOX };
            this.Controls.Add(newControl);
        }

        public CheckBoxItem[] GetCheckedItems()
        {
            var selectedTypes = this.Controls.OfType<CustomCheckBox>().Where(x=>x.Checked).Select(x => x.Tag as CheckBoxItem);
            return selectedTypes.OrderBy(x => x.Order).ToArray();
        }
    }
}

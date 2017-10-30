using System.Windows.Forms;
using static Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.CleanupItemUserControl;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    public partial class CustomCheckBox : CheckBox
    {

        public CheckBoxItemInfo Info { get; set; }
    }
}

using System.Windows.Forms;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    public partial class CustomCheckBox : CheckBox
    {
        public CleanerItemUIInfo Info { get; set; }
    }
}
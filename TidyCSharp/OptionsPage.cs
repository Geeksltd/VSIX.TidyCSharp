using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace Geeks.GeeksProductivityTools
{
    public class OptionsPage : DialogPage
    {
        [DisplayName("Disable Open M# module")]
        [Description("Disables open M#m odule by Ctrl click")]
        public bool DisableShiftClick { get; set; }
    }
}

using System.Windows.Forms;

namespace Geeks.GeeksProductivityTools.Utility
{
    public class MessageBoxDisplay
    {
        public struct MessageBoxArgs
        {
            public string Message { get; set; }
            public string Caption { get; set; }
            public MessageBoxButtons Button { get; set; }
            public MessageBoxIcon Icon { get; set; }
        }

        public static DialogResult Show(MessageBoxArgs args) => MessageBox.Show(args.Message, args.Caption, args.Button, args.Icon);
    }
}
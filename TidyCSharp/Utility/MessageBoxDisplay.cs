using System.Windows.Forms;

namespace Geeks.GeeksProductivityTools.Utility
{
    public class MessageBoxDisplay
    {
        public struct MessageBoxArgs
        {
            public string Message;
            public string Caption;
            public MessageBoxButtons Button;
            public MessageBoxIcon Icon;
        }

        public static DialogResult Show(MessageBoxArgs args) => MessageBox.Show(args.Message, args.Caption, args.Button, args.Icon);
    }
}

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    public class CheckBoxItemInfo
    {
        public string Name { get; set; }
        public int CleanerType { get; set; }
        public int Order { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

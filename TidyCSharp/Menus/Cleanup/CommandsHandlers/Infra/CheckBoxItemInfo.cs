namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    public class CleanerItemUIInfo
    {
        public string Name { get; set; }
        public int CleanerType { get; set; }
        public int Order { get; set; }

        public bool ShouldBeSelectedByDefault { get; set; }

        public override string ToString() => Name;
    }
}
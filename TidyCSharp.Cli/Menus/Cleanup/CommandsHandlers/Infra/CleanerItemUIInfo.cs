namespace TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

public class CleanerItemUiInfo
{
    public string Name { get; set; }
    public int CleanerType { get; set; }
    public int Order { get; set; }

    public bool ShouldBeSelectedByDefault { get; set; }

    public override string ToString() => Name;
}
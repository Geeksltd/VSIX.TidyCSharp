namespace TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class CleanupItemAttribute : Attribute
{
    public string Title { get; set; } = null;
    public int FirstOrder { get; set; } = int.MaxValue;
    public bool SelectedByDefault { get; set; } = true;

    public int? Order => FirstOrder == int.MaxValue ? (int?)null : FirstOrder;
    public Type SubitemType { get; set; }

    public CleanupItemAttribute()
    {
    }
}
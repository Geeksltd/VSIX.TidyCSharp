using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.PrivateModifierRemover.Option;

[Flags]
public enum CleanupTypes
{
    [CleanupItem(Title = "Remove 'private' from nested classes")]
    RemoveNestedClassPrivateModifier = 0x02,

    [CleanupItem(Title = "Remove 'private' from fields")]
    RemoveClassFieldsPrivateModifier = 0x04,

    [CleanupItem(Title = "Remove 'private' from methods")]
    RemoveClassMethodsPrivateModifier = 0x08,

    [CleanupItem(Title = "Remove 'private' from properties")]
    RemoveClassPropertiesPrivateModifier = 0x10,
}
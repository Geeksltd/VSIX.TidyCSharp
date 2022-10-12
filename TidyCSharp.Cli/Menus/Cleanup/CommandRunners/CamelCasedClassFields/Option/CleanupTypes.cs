using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.CamelCasedClassFields.Option;

[Flags]
public enum CleanupTypes
{
    [CleanupItem(Title = "Class fields: Change _something -> Something or something")]
    NormalFields = 0x01,

    [CleanupItem(Title = "Const fields: USE_THIS_FORMAT")]
    ConstFields = 0x02,
}
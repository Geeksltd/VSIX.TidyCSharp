using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.CamelCasedMethodVariable.Option;

[Flags]
public enum CleanupTypes
{
    [CleanupItem(Title = "Method parameters")]
    MethodParameter = 0x01,

    [CleanupItem(Title = "Local variable declarations")]
    LocalVariable = 0x02,
}
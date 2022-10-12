using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.SimplyAsyncCall.Option;

[Flags]
public enum CleanupTypes
{
    [CleanupItem(Title = "Remove unnecessary async / await pair (simply return the task).")]
    SingleExpression = 0x02,
}
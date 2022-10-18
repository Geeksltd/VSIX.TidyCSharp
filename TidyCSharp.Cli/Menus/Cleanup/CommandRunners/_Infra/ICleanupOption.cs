using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;

public interface ICleanupOption
{
    int? CleanupItemsInteger { get; }

    void Accept(IMainCleanup mainCleanup);

    CodeCleanerType GetCodeCleanerType();
}
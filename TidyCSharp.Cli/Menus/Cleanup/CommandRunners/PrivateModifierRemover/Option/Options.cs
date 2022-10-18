using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.PrivateModifierRemover.Option;

public class Options : OptionsBase, ICleanupOption
{
    public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

    public override CodeCleanerType GetCodeCleanerType() => CodeCleanerType.PrivateAccessModifier;
}
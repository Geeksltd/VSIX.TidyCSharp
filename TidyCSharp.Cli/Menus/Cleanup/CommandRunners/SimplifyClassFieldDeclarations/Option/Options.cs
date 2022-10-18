using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.SimplifyClassFieldDeclarations.Option;

public class Options : OptionsBase, ICleanupOption
{
    public const int MaxFieldDeclarationLength = 80;

    public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

    public override CodeCleanerType GetCodeCleanerType()
    {
        return CodeCleanerType.SimplifyClassFieldDeclarations;
    }
}
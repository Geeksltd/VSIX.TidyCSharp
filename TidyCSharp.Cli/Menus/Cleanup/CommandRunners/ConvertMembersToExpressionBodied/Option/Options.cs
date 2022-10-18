using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.ConvertMembersToExpressionBodied.Option;

public class Options : OptionsBase, ICleanupOption
{
    public const int MaxExpressionBodiedMemberLength = 90;

    public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

    public override CodeCleanerType GetCodeCleanerType()
    {
        return CodeCleanerType.ConvertMembersToExpressionBodied;
    }
}
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.RemoveExtraThisQualification.Option;

[Flags]
public enum CleanupTypes
{
    [CleanupItem(Title = "Remove from method calls")]
    RemoveFromMethodCall = 0x01,

    // [CleanupItem(Title = "Remove From virual Method Call")]
    // Remove_From_virual_Method_Call = 0x02,

    [CleanupItem(Title = "Remove from field calls")]
    RemoveFromFieldsCall = 0x04,

    [CleanupItem(Title = "Remove from property calls")]
    RemoveFromPropertiesCall = 0x08,
}
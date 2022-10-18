using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.ConvertMembersToExpressionBodied.Option;

[Flags]
public enum CleanupTypes
{
    [CleanupItem(Title = "Convert Methods => Method with only a single return statement and lenth less than 100 chars(length of its signature and its single statement)")]
    ConvertMethods = 0x01,

    [CleanupItem(Title = "Convert ReadOnly Property =>  ReadOnly Property with only a single return statement and lenth less than 100 chars(length of its Defenition and its single statement)")]
    ConvertReadOnlyProperty = 0x02,

    [CleanupItem(Title = "Convert Constructors =>  Method with only a single return statement and lenth less than 100 chars(length of its signature and its single statement)")]
    ConvertConstructors = 0x03,
}
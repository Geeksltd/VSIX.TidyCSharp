using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners.CamelCasedClassFields.Option;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers;

public class CleanupOptions
{
    private List<ICleanupOption> _optionItems = new();

    public const string ToStringSeprator2 = ":", ToStringSeprator = ";";
    public List<CodeCleanerType> ActionTypes { get; private set; } = new();

    public CleanupOptions()
    {
        _optionItems.Add(CamelCasedFields = new CommandRunners.CamelCasedClassFields.Option.Options());
        _optionItems.Add(CamelCasedLocalVariable = new CommandRunners.CamelCasedMethodVariable.Option.Options());
        _optionItems.Add(ConvertMembersToExpressionBodied = new CommandRunners.ConvertMembersToExpressionBodied.Option.Options());
        _optionItems.Add(PrivateModifierRemover = new CommandRunners.PrivateModifierRemover.Option.Options());
        _optionItems.Add(RemoveExtraThisQualification = new CommandRunners.RemoveExtraThisQualification.Option.Options());
        _optionItems.Add(SimplifyClassFieldDeclarations = new CommandRunners.SimplifyClassFieldDeclarations.Option.Options());
        _optionItems.Add(SimplyAsyncCall = new CommandRunners.SimplyAsyncCall.Option.Options());
        _optionItems.Add(WhiteSpaceNormalizer = new CommandRunners.Whitespace.Option.Options());
    }

    public void Accept(IMainCleanup mainCleanup)
    {
        foreach (var item in _optionItems)
            item.Accept(mainCleanup);

        if (mainCleanup.IsMainObjectSelected)
        {
            ActionTypes.Add(mainCleanup.MainCleanupItemType);
        }
    }

    public CommandRunners.CamelCasedClassFields.Option.Options CamelCasedFields { get; private set; }
    public CommandRunners.CamelCasedMethodVariable.Option.Options CamelCasedLocalVariable { get; private set; }
    public CommandRunners.ConvertMembersToExpressionBodied.Option.Options ConvertMembersToExpressionBodied { get; private set; }
    public CommandRunners.PrivateModifierRemover.Option.Options PrivateModifierRemover { get; private set; }
    public CommandRunners.RemoveExtraThisQualification.Option.Options RemoveExtraThisQualification { get; private set; }
    public CommandRunners.SimplifyClassFieldDeclarations.Option.Options SimplifyClassFieldDeclarations { get; private set; }
    public CommandRunners.SimplyAsyncCall.Option.Options SimplyAsyncCall { get; private set; }
    public CommandRunners.Whitespace.Option.Options WhiteSpaceNormalizer { get; private set; }

    public string SerializeValues()
    {
        var values = Enum.GetValues(typeof(CodeCleanerType)).Cast<int>();

        return string.Join(ToStringSeprator, values
            .Select(enumValueAsObject =>
            {
                var optionItem = _optionItems.FirstOrDefault(o => (int)o.GetCodeCleanerType() == enumValueAsObject);

                var isParentSelected = ActionTypes.Contains((CodeCleanerType)enumValueAsObject);

                if (optionItem == null) return $"{enumValueAsObject}{ToStringSeprator2}{isParentSelected}{ToStringSeprator2}{-1}";

                if (optionItem.CleanupItemsInteger.HasValue == false)
                    return $"{enumValueAsObject}{ToStringSeprator2}{isParentSelected}{ToStringSeprator2}{0}";

                return $"{enumValueAsObject}{ToStringSeprator2}{isParentSelected}{ToStringSeprator2}{optionItem.CleanupItemsInteger.Value}";
            })
        );
    }

    public override string ToString()
    {
        return string.Join(ToStringSeprator, ActionTypes
            .Select(x =>
            {
                var optionItem = _optionItems.FirstOrDefault(o => o.GetCodeCleanerType() == x);
                if (optionItem == null) return $"{(int)x}{ToStringSeprator2}{-1}";
                if (optionItem.CleanupItemsInteger.HasValue == false) return $"{(int)x}{ToStringSeprator2}{0}";
                return $"{(int)x}{ToStringSeprator2}{optionItem.CleanupItemsInteger.Value}";
            })
        );
    }
}
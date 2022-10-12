using Microsoft.CodeAnalysis;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners.PrivateModifierRemover.Option;
using TidyCSharp.Cli.Menus.Cleanup.TokenRemovers;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.PrivateModifierRemover;

public class PrivateModifierRemover : CodeCleanerCommandRunnerBase
{
    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        return RemoveExplicitPrivateModifiers(initialSourceNode);
    }

    private SyntaxNode RemoveExplicitPrivateModifiers(SyntaxNode actualSourceCode)
    {
        var modifiedSourceNode = actualSourceCode;

        if (CheckOption((int)CleanupTypes.RemoveClassMethodsPrivateModifier))
        {
            var remover = new MethodTokenRemover(IsReportOnlyMode);
            modifiedSourceNode = remover.Remove(modifiedSourceNode) ?? modifiedSourceNode;

            if (IsReportOnlyMode && !IsEquivalentToUnModified(modifiedSourceNode))
            {
                CollectMessages(remover.GetReport());
            }
        }

        if (CheckOption((int)CleanupTypes.RemoveClassFieldsPrivateModifier))
        {
            var remover = new FieldTokenRemover(IsReportOnlyMode);
            modifiedSourceNode = remover.Remove(modifiedSourceNode) ?? modifiedSourceNode;

            if (IsReportOnlyMode && !IsEquivalentToUnModified(modifiedSourceNode))
            {
                CollectMessages(remover.GetReport());
            }
        }

        if (CheckOption((int)CleanupTypes.RemoveClassPropertiesPrivateModifier))
        {
            var remover = new PropertyTokenRemover(IsReportOnlyMode);
            modifiedSourceNode = remover.Remove(modifiedSourceNode) ?? modifiedSourceNode;

            if (IsReportOnlyMode && !IsEquivalentToUnModified(modifiedSourceNode))
            {
                CollectMessages(remover.GetReport());
            }
        }

        if (CheckOption((int)CleanupTypes.RemoveNestedClassPrivateModifier))
        {
            var remover = new NestedClassTokenRemover(IsReportOnlyMode);
            modifiedSourceNode = remover.Remove(modifiedSourceNode) ?? modifiedSourceNode;

            if (IsReportOnlyMode && !IsEquivalentToUnModified(modifiedSourceNode))
            {
                CollectMessages(remover.GetReport());
            }
        }

        if (IsReportOnlyMode) return actualSourceCode;
        return modifiedSourceNode;
    }
}
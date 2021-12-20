using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.RemovePrivateModifier;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class PrivateModifierRemover : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
        {
            return RemoveExplicitPrivateModifiers(initialSourceNode);
        }

        SyntaxNode RemoveExplicitPrivateModifiers(SyntaxNode actualSourceCode)
        {
            var modifiedSourceNode = actualSourceCode;

            if (CheckOption((int)CleanupTypes.RemoveClassMethodsPrivateModifier))
            {
                var remover = new MethodTokenRemover(IsReportOnlyMode);
                modifiedSourceNode = remover.Remove(modifiedSourceNode) ?? modifiedSourceNode;

                if (IsReportOnlyMode && !IsEquivalentToUNModified(modifiedSourceNode))
                {
                    CollectMessages(remover.GetReport());
                }
            }

            if (CheckOption((int)CleanupTypes.RemoveClassFieldsPrivateModifier))
            {
                var remover = new FieldTokenRemover(IsReportOnlyMode);
                modifiedSourceNode = remover.Remove(modifiedSourceNode) ?? modifiedSourceNode;

                if (IsReportOnlyMode && !IsEquivalentToUNModified(modifiedSourceNode))
                {
                    CollectMessages(remover.GetReport());
                }
            }

            if (CheckOption((int)CleanupTypes.RemoveClassPropertiesPrivateModifier))
            {
                var remover = new PropertyTokenRemover(IsReportOnlyMode);
                modifiedSourceNode = remover.Remove(modifiedSourceNode) ?? modifiedSourceNode;

                if (IsReportOnlyMode && !IsEquivalentToUNModified(modifiedSourceNode))
                {
                    CollectMessages(remover.GetReport());
                }
            }

            if (CheckOption((int)CleanupTypes.RemoveNestedClassPrivateModifier))
            {
                var remover = new NestedClassTokenRemover(IsReportOnlyMode);
                modifiedSourceNode = remover.Remove(modifiedSourceNode) ?? modifiedSourceNode;

                if (IsReportOnlyMode && !IsEquivalentToUNModified(modifiedSourceNode))
                {
                    CollectMessages(remover.GetReport());
                }
            }

            if (IsReportOnlyMode) return actualSourceCode;
            return modifiedSourceNode;
        }
    }
}
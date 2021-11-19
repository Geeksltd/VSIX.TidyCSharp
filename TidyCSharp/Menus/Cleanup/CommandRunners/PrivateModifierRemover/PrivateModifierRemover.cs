using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.RemovePrivateModifier;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class PrivateModifierRemover : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override async Task<SyntaxNode> CleanUp(SyntaxNode initialSourceNode)
        {
            return RemoveExplicitPrivateModifiers(initialSourceNode);
        }

        SyntaxNode RemoveExplicitPrivateModifiers(SyntaxNode actualSourceCode)
        {
            var modifiedSourceNode = actualSourceCode;

            if (CheckOption((int)CleanupTypes.Remove_Class_Methods_Private_Modifier))
            {
                var remover = new MethodTokenRemover(IsReportOnlyMode);
                modifiedSourceNode = remover.Remove(modifiedSourceNode) ?? modifiedSourceNode;

                if (IsReportOnlyMode && !IsEquivalentToUnModified(modifiedSourceNode))
                {
                    CollectMessages(remover.GetReport());
                }
            }

            if (CheckOption((int)CleanupTypes.Remove_Class_Fields_Private_Modifier))
            {
                var remover = new FieldTokenRemover(IsReportOnlyMode);
                modifiedSourceNode = remover.Remove(modifiedSourceNode) ?? modifiedSourceNode;

                if (IsReportOnlyMode && !IsEquivalentToUnModified(modifiedSourceNode))
                {
                    CollectMessages(remover.GetReport());
                }
            }

            if (CheckOption((int)CleanupTypes.Remove_Class_Properties_Private_Modifier))
            {
                var remover = new PropertyTokenRemover(IsReportOnlyMode);
                modifiedSourceNode = remover.Remove(modifiedSourceNode) ?? modifiedSourceNode;

                if (IsReportOnlyMode && !IsEquivalentToUnModified(modifiedSourceNode))
                {
                    CollectMessages(remover.GetReport());
                }
            }

            if (CheckOption((int)CleanupTypes.Remove_Nested_Class_Private_Modifier))
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
}
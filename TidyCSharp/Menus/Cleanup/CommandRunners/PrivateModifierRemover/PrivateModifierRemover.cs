using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.RemovePrivateModifier;
using Microsoft.CodeAnalysis;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class PrivateModifierRemover : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return RemoveExplicitPrivateModifiers(initialSourceNode);
        }

        SyntaxNode RemoveExplicitPrivateModifiers(SyntaxNode actualSourceCode)
        {
            if (CheckOption((int)CleanupTypes.Remove_Class_Methods_Private_Modifier))
            {
                actualSourceCode = new MethodTokenRemover().Remove(actualSourceCode) ?? actualSourceCode;
            }

            if (CheckOption((int)CleanupTypes.Remove_Class_Fields_Private_Modifier))
            {
                actualSourceCode = new FieldTokenRemover().Remove(actualSourceCode) ?? actualSourceCode;
            }

            if (CheckOption((int)CleanupTypes.Remove_Class_Properties_Private_Modifier))
            {
                actualSourceCode = new PropertyTokenRemover().Remove(actualSourceCode) ?? actualSourceCode;
            }

            if (CheckOption((int)CleanupTypes.Remove_Nested_Class_Private_Modifier))
            {
                actualSourceCode = new NestedClassTokenRemover().Remove(actualSourceCode) ?? actualSourceCode;
            }

            return actualSourceCode;
        }
    }
}
using Microsoft.CodeAnalysis;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class PrivateModifierRemover : CodeCleanerCommandRunnerBase, ICodeCleaner
    {

        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return RemoveExplicitPrivateModifiers(initialSourceNode);
        }

        SyntaxNode RemoveExplicitPrivateModifiers(SyntaxNode actualSourceCode)
        {
            actualSourceCode = new MethodTokenRemover().Remove(actualSourceCode) ?? actualSourceCode;
            actualSourceCode = new FieldTokenRemover().Remove(actualSourceCode) ?? actualSourceCode;
            actualSourceCode = new PropertyTokenRemover().Remove(actualSourceCode) ?? actualSourceCode;
            actualSourceCode = new NestedClassTokenRemover().Remove(actualSourceCode) ?? actualSourceCode;

            return actualSourceCode;
        }

    }
}

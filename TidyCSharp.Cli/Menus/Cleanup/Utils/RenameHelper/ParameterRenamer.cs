using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper;

internal class ParameterRenamer : VariableRenamer
{
    public ParameterRenamer(Document document) : base(document)
    {
    }

    protected override IEnumerable<SyntaxToken> GetItemsToRename(SyntaxNode currentNode)
    {
        return
            (currentNode as MethodDeclarationSyntax)
            .ParameterList
            .Parameters
            .Select(x => x.Identifier);
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    class ParameterRenamer : VariableRenamer
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
}
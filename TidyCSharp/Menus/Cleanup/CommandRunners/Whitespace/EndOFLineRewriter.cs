using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace
{
    public class EndOFLineRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia) && trivia.ToFullString() == "\r\n")
                return base.VisitTrivia(SyntaxFactory.EndOfLine("\n"));
            return base.VisitTrivia(trivia);
        }

        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            var n = node.Update(node.AttributeLists, node.Modifiers, node.ReturnType,
                node.OperatorKeyword, node.OperatorToken, node.ParameterList, node.Body,
                node.SemicolonToken);
            return base.VisitOperatorDeclaration(node);
        }

    }
}

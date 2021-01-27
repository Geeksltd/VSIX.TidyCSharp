using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class ZebbleCleaner : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return ChangeMethodHelper(initialSourceNode);
        }

        SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode)
        {
            initialSourceNode = new ReadOnlyRewriter(ProjectItemDetails.SemanticModel).Visit(initialSourceNode);
            return initialSourceNode;
        }


        class ReadOnlyRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public ReadOnlyRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                if (node.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)) return node;
                if (semanticModel.GetTypeInfo(node.Declaration.Type).Type.Name != "Bindable") return node;
                if (!node.Declaration.Type.IsKind(SyntaxKind.GenericName)) return node;
                if (semanticModel.GetTypeInfo(node.Declaration.Type).Type.ContainingNamespace.Name != "Zebble" ||
                    semanticModel.GetTypeInfo(node.Declaration.Type).Type.ContainingNamespace.Name != "Olive")
                    return node;

                return node.AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
                    .WithTrailingTrivia(SyntaxFactory.ParseTrailingTrivia(" ")));
            }
        }
    }
}

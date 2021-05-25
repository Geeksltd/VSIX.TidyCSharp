using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class ZebbleCleaner : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            var syntaxRewriter = new ReadOnlyRewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode);
            var modifiedSourceNode = syntaxRewriter.Visit(initialSourceNode);

            if (IsReportOnlyMode)
            {
                this.CollectMessages(syntaxRewriter.GetReport());
                return initialSourceNode;
            }
            return modifiedSourceNode;
        }

        class ReadOnlyRewriter : SyntaxRewriterBase
        {
            SemanticModel semanticModel;

            public ReadOnlyRewriter(SemanticModel semanticModel, bool isReportOnlyMode)
                : base(isReportOnlyMode)
            {
                this.semanticModel = semanticModel;
                this.IsReportOnlyMode = isReportOnlyMode;
            }
            public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                if (node.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)) return node;
                if (semanticModel.GetTypeInfo(node.Declaration.Type).Type.Name != "Bindable") return node;
                if (!node.Declaration.Type.IsKind(SyntaxKind.GenericName)) return node;
                if (semanticModel.GetTypeInfo(node.Declaration.Type).Type.ContainingNamespace.Name != "Zebble" &&
                    semanticModel.GetTypeInfo(node.Declaration.Type).Type.ContainingNamespace.Name != "Olive")
                    return node;

                if (IsReportOnlyMode)
                {
                    var lineSpan = node.GetFileLinePosSpan();
                    ChangesReport = ChangesReport.Append(new ChangesReport
                    {
                        LineNumber = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                        FileName = node.GetFileName(),
                        Message = "Should Add Readonly Modifier",
                        Generator = GetType().Name
                    });
                    return node;
                }
                return node.AddModifiers(
                       SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
                       .WithTrailingTrivia(SyntaxFactory.ParseTrailingTrivia(" ")));
            }
        }
    }
}

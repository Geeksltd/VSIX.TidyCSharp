using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class RemoveAttributeKeywork : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override async Task<SyntaxNode> CleanUp(SyntaxNode initialSourceNode)
        {
            return RemoveAttributeKeyworkHelper(initialSourceNode, ProjectItemDetails.SemanticModel);
        }

        SyntaxNode RemoveAttributeKeyworkHelper(SyntaxNode initialSourceNode, SemanticModel semanticModel)
        {
            var syntaxRewriter = new Rewriter(semanticModel, IsReportOnlyMode, Options);
            var modifiedSyntaxNode = syntaxRewriter.Visit(initialSourceNode);

            if (IsReportOnlyMode)
            {
                CollectMessages(syntaxRewriter.GetReport());
                return initialSourceNode;
            }

            return modifiedSyntaxNode;
        }

        static string Attribute_Keywork = SyntaxKind.Attribute.ToString();
        class Rewriter : CleanupCSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public Rewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options)
                : base(isReportOnlyMode, options) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitAttribute(AttributeSyntax node)
            {
                if (node.Name is IdentifierNameSyntax newNameNode)
                {
                    if (newNameNode.Identifier.ValueText.EndsWith(Attribute_Keywork))
                    {
                        var orginalNodeTypeInfo = semanticModel.GetTypeInfo(node.Name);

                        if (orginalNodeTypeInfo.Type == null) base.VisitAttribute(node);

                        if (orginalNodeTypeInfo.Type.Name == newNameNode.Identifier.ValueText)
                        {
                            var newName = newNameNode.Identifier.ValueText.TrimEnd(Attribute_Keywork);

                            if (IsReportOnlyMode)
                            {
                                var lineSpan = node.GetFileLinePosSpan();

                                AddReport(new ChangesReport(node)
                                {
                                    LineNumber = lineSpan.StartLinePosition.Line,
                                    Column = lineSpan.StartLinePosition.Character,
                                    Message = "Attributes should not ended with \"Attribute\"",
                                    Generator = nameof(RemoveAttributeKeywork)
                                });
                            }

                            node = node.WithName(SyntaxFactory.IdentifierName(newName));
                        }
                    }
                }

                return base.VisitAttribute(node);
            }
        }
    }
}
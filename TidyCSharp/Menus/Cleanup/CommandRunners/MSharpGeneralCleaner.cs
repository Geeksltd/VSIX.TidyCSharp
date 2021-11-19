using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeTypeConverter;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class MSharpGeneralCleaner : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override async Task<SyntaxNode> CleanUp(SyntaxNode initialSourceNode)
        {
            return ChangeMethodHelper(initialSourceNode);
        }

        SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode)
        {
            var csSyntaxRewriter = new CsMethodStringRewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode, Options);
            var modifiedSourceNode = csSyntaxRewriter.Visit(initialSourceNode);
            modifiedSourceNode = RefreshResult(modifiedSourceNode);
            var multiLineSyntaxRewriter = new MultiLineExpressionRewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode, Options);
            modifiedSourceNode = multiLineSyntaxRewriter.Visit(modifiedSourceNode);

            if (IsReportOnlyMode)
            {
                CollectMessages(csSyntaxRewriter.GetReport());
                CollectMessages(multiLineSyntaxRewriter.GetReport());
                return initialSourceNode;
            }

            return modifiedSourceNode;
        }
        class MultiLineExpressionRewriter : CleanupCSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public MultiLineExpressionRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options) :
                base(isReportOnlyMode, options) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
            {
                if (node.ToString().Length < 110)
                    return base.VisitExpressionStatement(node);

                var m = node.Expression;
                var rewritableToken = new List<SyntaxToken>();
                var trivia = new SyntaxTriviaList(SyntaxFactory.EndOfLine("\n"));

                trivia = trivia.AddRange(node.GetLeadingTrivia().Reverse()
                    .TakeWhile(x => x.IsDirective ^ !x.IsKind(SyntaxKind.EndOfLineTrivia)));

                trivia = trivia.Add(SyntaxFactory.Whitespace("    "));
                var newExpression = SyntaxFactory.ParseExpression("");

                while (m != null && m.ChildNodes().Any())
                {
                    var m2 = m.ChildNodes();

                    if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                        m2.LastOrDefault() is ArgumentListSyntax)
                    {
                        var methodName = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>();
                        var arguments = m2.LastOrDefault().As<ArgumentListSyntax>();
                        m = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>()?.Expression;

                        if (newExpression.ToString() == "")
                            newExpression = SyntaxFactory.InvocationExpression(methodName.Name, arguments)
                                .WithoutTrailingTrivia();
                        else
                            newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.InvocationExpression(methodName.Name, arguments).WithoutTrailingTrivia(),
                                SyntaxFactory.Token(SyntaxKind.DotToken).WithLeadingTrivia(trivia), SyntaxFactory.IdentifierName(newExpression.ToString()));
                    }
                    else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                       m2.LastOrDefault() is ArgumentListSyntax)
                    {
                        var identifierName = m2.FirstOrDefault() as IdentifierNameSyntax;
                        var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                        m = null;

                        if (newExpression.ToString() == "")
                            newExpression = SyntaxFactory.InvocationExpression(identifierName, arguments).WithoutTrailingTrivia();
                        else
                            newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.InvocationExpression(identifierName.WithoutTrailingTrivia(), arguments.WithoutTrailingTrivia()),
                                    SyntaxFactory.Token(SyntaxKind.DotToken).WithLeadingTrivia(trivia), SyntaxFactory.IdentifierName(newExpression.ToString()));
                    }
                    else
                    {
                        if (newExpression.ToString() == "")
                            newExpression = m.WithoutTrailingTrivia();
                        else
                            newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                               m.WithoutTrailingTrivia(), SyntaxFactory.Token(SyntaxKind.DotToken).WithLeadingTrivia(trivia), SyntaxFactory.IdentifierName(newExpression.ToString()));

                        m = null;
                    }
                }

                if (m != null)
                    newExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        m, SyntaxFactory.IdentifierName(newExpression.ToString()));

                if (!newExpression.ToFullString().Equals(node.ToFullString()))
                {
                    var lineSpan = node.GetFileLinePosSpan();

                    AddReport(new ChangesReport(node)
                    {
                        LineNumber = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                        Message = "your expression should be multilined",
                        Generator = nameof(MultiLineExpressionRewriter)
                    });
                }

                return SyntaxFactory.ExpressionStatement(newExpression)
                    .WithLeadingTrivia(node.GetLeadingTrivia())
                    .WithTrailingTrivia(node.GetTrailingTrivia());
            }
        }
        class CsMethodStringRewriter : CleanupCSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public CsMethodStringRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options) :
                base(isReportOnlyMode, options) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
            {
                if (node.Token.ValueText.StartsWith("c#:"))
                {
                    var classDeclaration = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                    if (classDeclaration == null) return node;

                    if (classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                        return node;

                    var s = semanticModel.GetDeclaredSymbol(classDeclaration);

                    if (!s.AllInterfaces.Any(x => x.Name == "IMSharpConcept"))
                        return node;

                    var args = new SeparatedSyntaxList<ArgumentSyntax>();

                    args = args.Add(SyntaxFactory.Argument(
                        SyntaxFactory.ParseExpression(node.Token.Text.Remove(node.Token.Text.IndexOf("c#:"), 3))));

                    var lineSpan = node.GetFileLinePosSpan();

                    AddReport(new ChangesReport(node)
                    {
                        LineNumber = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                        Message = "\"C#:\" --> cs(\"\")",
                        Generator = nameof(CsMethodStringRewriter)
                    });

                    return SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("cs")
                        , SyntaxFactory.ArgumentList(args));
                }

                return base.VisitLiteralExpression(node);
            }

            public override SyntaxNode VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
            {
                if (node.Contents.ToString().StartsWith("c#:"))
                {
                    var classDeclaration = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                    if (classDeclaration == null) return node;

                    if (classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                        return node;

                    var s = semanticModel.GetDeclaredSymbol(classDeclaration);

                    if (!s.AllInterfaces.Any(x => x.Name == "IMSharpConcept"))
                        return node;

                    var args = new SeparatedSyntaxList<ArgumentSyntax>();

                    args = args.Add(SyntaxFactory.Argument(
                        SyntaxFactory.ParseExpression(node.ToString().Remove(node.ToString().IndexOf("c#:"), 3))));

                    var lineSpan = node.GetFileLinePosSpan();

                    AddReport(new ChangesReport(node)
                    {
                        LineNumber = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                        Message = "\"C#:\" --> cs(\"\")",
                        Generator = nameof(CsMethodStringRewriter)
                    });

                    return SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("cs")
                        , SyntaxFactory.ArgumentList(args));
                }

                return base.VisitInterpolatedStringExpression(node);
            }
        }
    }
}
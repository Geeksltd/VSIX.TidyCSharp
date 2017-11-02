using System.Linq;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Cleanup.MembersToExpressionBodied;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class ConvertMembersToExpressionBodied : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return ConvertMembersToExpressionBodiedHelper(initialSourceNode, Options);
        }

        class Rewriter : CleanupCSharpSyntaxRewriter
        {
            public Rewriter(ICleanupOption Options) : base(Options) { }
            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node == null) return base.Visit(node);

                if (node is MethodDeclarationSyntax && node.Parent is ClassDeclarationSyntax)
                {
                    if (CheckOption((int)CleanupTypes.Convert_Methods))
                    {
                        node = ConvertToExpressionBodiedHelper(node as MethodDeclarationSyntax);
                    }
                }
                else if (node is PropertyDeclarationSyntax)
                {
                    if (CheckOption((int)CleanupTypes.Convert_ReadOnly_Property))
                    {
                        node = ConvertToExpressionBodiedHelper(node as PropertyDeclarationSyntax);
                    }
                }

                return base.Visit(node);
            }
        }

        static SyntaxTrivia[] _spaceTrivia = { SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ") };
        public static SyntaxNode ConvertMembersToExpressionBodiedHelper(SyntaxNode initialSourceNode, ICleanupOption Options)
        {
            return new Rewriter(Options).Visit(initialSourceNode);
        }

        public static MethodDeclarationSyntax ConvertToExpressionBodiedHelper(MethodDeclarationSyntax methodDeclaration)
        {
            var expression = AnalyzeMethods(methodDeclaration);

            if (expression == null) return methodDeclaration;

            var closeParen = methodDeclaration.DescendantTokens()
                .FirstOrDefault(x => x.IsKind(SyntaxKind.CloseParenToken));
            if (closeParen != null)
            {
                methodDeclaration =
                    methodDeclaration.ReplaceToken(closeParen, closeParen.WithTrailingTrivia(_spaceTrivia));
            }

            var newMethod =
                methodDeclaration
                    .WithLeadingTrivia(methodDeclaration.GetLeadingTrivia())
                    .WithExpressionBody(
                        SyntaxFactory.ArrowExpressionClause(expression.WithLeadingTrivia(_spaceTrivia)))
                    .WithBody(null)
                    .WithSemicolonToken(GetSemicolon(methodDeclaration.Body))
                    .WithAdditionalAnnotations(Formatter.Annotation);


            return newMethod;
        }

        static ExpressionSyntax AnalyzeMethods(MethodDeclarationSyntax method)
        {
            if ((method.Parent is ClassDeclarationSyntax) == false) return null;
            if (method.Body == null) return null;
            if (method.Body.Statements.Count != 1) return null;
            if (method.Body.ContainsDirectives) return null;

            var singleStatement = method.Body.Statements.First();
            if (singleStatement is IfStatementSyntax) return null;
            if (singleStatement is ThrowStatementSyntax) return null;
            if (singleStatement is YieldStatementSyntax) return null;
            if (singleStatement is ReturnStatementSyntax == false && singleStatement is ExpressionStatementSyntax == false) return null;
            if (singleStatement.HasLeadingTrivia)
            {
                if (HasNoneWhitespaceTrivia(singleStatement.GetLeadingTrivia()) == false) return null;
            }
            if (singleStatement.HasTrailingTrivia)
            {
                if (HasNoneWhitespaceTrivia(singleStatement.GetTrailingTrivia()) == false) return null;
            }
            if (method.Body.CloseBraceToken.HasLeadingTrivia)
            {
                if (HasNoneWhitespaceTrivia(method.Body.CloseBraceToken.LeadingTrivia) == false) return null;
            }
            if (method.Body.OpenBraceToken.HasLeadingTrivia)
            {
                if (HasNoneWhitespaceTrivia(method.Body.OpenBraceToken.LeadingTrivia) == false) return null;
            }

            var expression =
                (
                    (singleStatement is ReturnStatementSyntax)
                        ? (singleStatement as ReturnStatementSyntax).Expression
                        : (singleStatement as ExpressionStatementSyntax).Expression
                )
                .WithoutLeadingTrivia();

            var length = expression.WithoutTrivia().Span.Length + method.Span.Length - method.Body.FullSpan.Length;
            if (length > MembersToExpressionBodied.Options.MAX_EXPRESSION_BODIED_MEMBER_LENGTH) return null;
            if (method.Body.ChildNodes().OfType<UsingStatementSyntax>().Any()) return null;

            return expression;
        }

        static bool HasNoneWhitespaceTrivia(SyntaxTriviaList getLeadingTrivia)
        {
            return getLeadingTrivia.All(t => t.IsKind(SyntaxKind.EndOfLineTrivia) || t.IsKind(SyntaxKind.WhitespaceTrivia));
        }

        public static PropertyDeclarationSyntax ConvertToExpressionBodiedHelper(PropertyDeclarationSyntax propertyDeclaration)
        {
            if (propertyDeclaration.AccessorList == null) return propertyDeclaration;
            var getNode =
                propertyDeclaration.AccessorList.Accessors.FirstOrDefault(
                    x => x.Keyword.IsKind(SyntaxKind.GetKeyword));
            var setNode =
                propertyDeclaration.AccessorList.Accessors.FirstOrDefault(
                    x => x.Keyword.IsKind(SyntaxKind.SetKeyword));
            if (setNode != null || getNode.Body == null) return propertyDeclaration;
            if (getNode.Body == null) return propertyDeclaration;
            if (getNode.Body.Statements.Count > 1) return propertyDeclaration;
            if (getNode.Body.ContainsDirectives) return propertyDeclaration;

            var returnStatements = getNode.Body.Statements.OfType<ReturnStatementSyntax>().ToList();
            if (returnStatements.Count() != 1) return propertyDeclaration;
            var expression = returnStatements.First().Expression.WithoutTrivia();

            var length =
                expression.Span.Length +
                propertyDeclaration.Span.Length -
                propertyDeclaration.AccessorList.FullSpan.Length;

            if (length >= 100) return propertyDeclaration;

            propertyDeclaration =
                propertyDeclaration
                    .WithIdentifier(propertyDeclaration.Identifier.WithTrailingTrivia(_spaceTrivia))
                    .WithLeadingTrivia(propertyDeclaration.GetLeadingTrivia())
                    .WithExpressionBody(
                        SyntaxFactory.ArrowExpressionClause(expression.WithLeadingTrivia(_spaceTrivia)))
                    .WithAccessorList(null)
                    .WithSemicolonToken(GetSemicolon(propertyDeclaration.AccessorList))
                    .WithAdditionalAnnotations(Formatter.Annotation);

            return propertyDeclaration;
        }

        static SyntaxToken GetSemicolon(BlockSyntax block)
        {
            var statement = block.Statements.First();

            var semicolon =
                (statement is ExpressionStatementSyntax)
                    ? (statement as ExpressionStatementSyntax).SemicolonToken
                    : (statement as ReturnStatementSyntax).SemicolonToken;

            var trivia = semicolon.TrailingTrivia.AsEnumerable();
            trivia = trivia.Where(t => !t.IsKind(SyntaxKind.EndOfLineTrivia));

            var closeBraceTrivia = block.CloseBraceToken.TrailingTrivia.AsEnumerable();
            trivia = trivia.Concat(closeBraceTrivia);

            return semicolon.WithTrailingTrivia(trivia);
        }

        static SyntaxToken GetSemicolon(AccessorListSyntax accessorList)
        {
            var semicolon = ((ReturnStatementSyntax)accessorList.Accessors[0].Body.Statements[0]).SemicolonToken;

            var trivia = semicolon.TrailingTrivia.AsEnumerable();
            trivia = trivia.Where(t => !t.IsKind(SyntaxKind.EndOfLineTrivia));

            var closeBraceTrivia = accessorList.CloseBraceToken.TrailingTrivia.AsEnumerable();
            trivia = trivia.Concat(closeBraceTrivia);

            return semicolon.WithTrailingTrivia(trivia);
        }
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.ConvertMembersToExpressionBodied;

public class ConvertMembersToExpressionBodied : CodeCleanerCommandRunnerBase
{
    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        return ConvertMembersToExpressionBodiedHelper(initialSourceNode, Options);
    }

    private class Rewriter : CleanupCSharpSyntaxRewriter
    {
        public Rewriter(bool isReportOnlyMode, ICleanupOption options) :
            base(isReportOnlyMode, options)
        { }
        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (node == null) return base.Visit(node);

            var message = "";

            if (node is MethodDeclarationSyntax && node.Parent is ClassDeclarationSyntax)
            {
                if (CheckOption((int)Option.CleanupTypes.ConvertMethods))
                {
                    node = ConvertToExpressionBodiedHelper(node as MethodDeclarationSyntax);
                    message = "Method Declaration should be Converted to expression bodied";
                }
            }
            else if (node is PropertyDeclarationSyntax)
            {
                if (CheckOption((int)Option.CleanupTypes.ConvertReadOnlyProperty))
                {
                    node = ConvertToExpressionBodiedHelper(node as PropertyDeclarationSyntax);
                    message = "Property Declaration should be Converted to expression bodied";
                }
            }
            else if (node is ConstructorDeclarationSyntax)
            {
                if (CheckOption((int)Option.CleanupTypes.ConvertConstructors))
                {
                    node = ConvertToExpressionBodiedHelper(node as ConstructorDeclarationSyntax);
                    message = "Constructor should be Converted to expression bodied";
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                var lineSpan = node.GetFileLinePosSpan();

                AddReport(new ChangesReport(node)
                {
                    LineNumber = lineSpan.StartLinePosition.Line,
                    Column = lineSpan.StartLinePosition.Character,
                    Message = message,
                    Generator = nameof(ConvertMembersToExpressionBodied)
                });
            }

            return base.Visit(node);
        }
    }

    private static SyntaxTrivia[] _spaceTrivia = { SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ") };
    public SyntaxNode ConvertMembersToExpressionBodiedHelper(SyntaxNode initialSourceNode, ICleanupOption options)
    {
        var rewriter = new Rewriter(IsReportOnlyMode, options);
        var modifiedSourceNode = rewriter.Visit(initialSourceNode);

        if (IsReportOnlyMode)
        {
            CollectMessages(rewriter.GetReport());
            return initialSourceNode;
        }

        return modifiedSourceNode;
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

    private static ExpressionSyntax AnalyzeMethods(MethodDeclarationSyntax method)
    {
        if ((method.Parent is ClassDeclarationSyntax) == false) return null;
        if (method.Body == null) return null;
        if (method.Body.Statements.Count != 1) return null;
        if (method.Body.ContainsDirectives) return null;

        var singleStatement = method.Body.Statements.FirstOrDefault();
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
        if (length > Option.Options.MaxExpressionBodiedMemberLength) return null;
        if (method.Body.ChildNodes().OfType<UsingStatementSyntax>().Any()) return null;

        return expression;
    }

    private static bool HasNoneWhitespaceTrivia(SyntaxTriviaList getLeadingTrivia)
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
        var expression = returnStatements.FirstOrDefault().Expression.WithoutTrivia();

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

    private static SyntaxToken GetSemicolon(BlockSyntax block)
    {
        var statement = block.Statements.FirstOrDefault();

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

    private static SyntaxToken GetSemicolon(AccessorListSyntax accessorList)
    {
        var semicolon = ((ReturnStatementSyntax)accessorList.Accessors[0].Body.Statements[0]).SemicolonToken;

        var trivia = semicolon.TrailingTrivia.AsEnumerable();
        trivia = trivia.Where(t => !t.IsKind(SyntaxKind.EndOfLineTrivia));

        var closeBraceTrivia = accessorList.CloseBraceToken.TrailingTrivia.AsEnumerable();
        trivia = trivia.Concat(closeBraceTrivia);

        return semicolon.WithTrailingTrivia(trivia);
    }

    public static ConstructorDeclarationSyntax ConvertToExpressionBodiedHelper(ConstructorDeclarationSyntax constructorDeclaration)
    {
        if (constructorDeclaration.Body == null) return constructorDeclaration;
        if (constructorDeclaration.Body.Statements.Count != 1) return constructorDeclaration;
        if (constructorDeclaration.Body.ContainsDirectives) return constructorDeclaration;

        var singleStatement = constructorDeclaration.Body.Statements.FirstOrDefault();
        if (singleStatement is IfStatementSyntax) return constructorDeclaration;
        if (singleStatement is ThrowStatementSyntax) return constructorDeclaration;
        if (singleStatement is YieldStatementSyntax) return constructorDeclaration;
        if (singleStatement is ExpressionStatementSyntax == false) return constructorDeclaration;

        if (singleStatement.HasLeadingTrivia)
        {
            if (HasNoneWhitespaceTrivia(singleStatement.GetLeadingTrivia()) == false) return constructorDeclaration;
        }

        if (singleStatement.HasTrailingTrivia)
        {
            if (HasNoneWhitespaceTrivia(singleStatement.GetTrailingTrivia()) == false) return constructorDeclaration;
        }

        if (constructorDeclaration.Body.CloseBraceToken.HasLeadingTrivia)
        {
            if (HasNoneWhitespaceTrivia(constructorDeclaration.Body.CloseBraceToken.LeadingTrivia) == false) return constructorDeclaration;
        }

        if (constructorDeclaration.Body.OpenBraceToken.HasLeadingTrivia)
        {
            if (HasNoneWhitespaceTrivia(constructorDeclaration.Body.OpenBraceToken.LeadingTrivia) == false) return constructorDeclaration;
        }

        var expression = (singleStatement as ExpressionStatementSyntax).Expression
            .WithoutLeadingTrivia();

        var length = expression.WithoutTrivia().Span.Length +
            constructorDeclaration.Span.Length - constructorDeclaration.Body.FullSpan.Length;

        if (length > Option.Options.MaxExpressionBodiedMemberLength) return constructorDeclaration;
        if (constructorDeclaration.Body.ChildNodes().OfType<UsingStatementSyntax>().Any()) return constructorDeclaration;

        var newconstructorDeclaration = constructorDeclaration
            .WithIdentifier(constructorDeclaration.Identifier.WithTrailingTrivia(_spaceTrivia))
            .WithBody(null)
            .WithTrailingTrivia(_spaceTrivia)
            .WithLeadingTrivia(constructorDeclaration.GetLeadingTrivia())
            .WithExpressionBody(
                SyntaxFactory.ArrowExpressionClause(expression.WithLeadingTrivia(_spaceTrivia)))
            .WithSemicolonToken(GetSemicolon(constructorDeclaration.Body))
            .WithAdditionalAnnotations(Formatter.Annotation);

        return newconstructorDeclaration;
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;
using TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners;

public class ConvertPropertiesToAutoProperties : CodeCleanerCommandRunnerBase
{
    private Document _workingDocument, _orginalDocument;
    private const string SelectedMethodAnnotationRename = "SELECTED_METHOD_ANNOTATION_RENAME";
    private const string SelectedMethodAnnotationRemove = "SELECTED_METHOD_ANNOTATION_REMOVE";

    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        _orginalDocument = ProjectItemDetails.Document;
        _workingDocument = ProjectItemDetails.Document;

        var walker = new MyWalker(ProjectItemDetails, Options);
        walker.Visit(initialSourceNode);

        if (walker.VariablesToRemove.Any())
        {
            var rewriter = new Rewriter(walker, ProjectItemDetails, Options);
            initialSourceNode = rewriter.Visit(initialSourceNode);
            _workingDocument = _workingDocument.WithSyntaxRoot(initialSourceNode);

            IEnumerable<SyntaxNode> annotations;

            do
            {
                annotations = initialSourceNode.GetAnnotatedNodes(SelectedMethodAnnotationRename);

                if (annotations.Any() == false) break;

                var firstAnnotatedItem = annotations.FirstOrDefault();
                var annotationOfFirstAnnotatedItem = firstAnnotatedItem.GetAnnotations(SelectedMethodAnnotationRename).FirstOrDefault();

                var renameResult = await Renamer.RenameSymbolAsync(_workingDocument, initialSourceNode, null, firstAnnotatedItem, annotationOfFirstAnnotatedItem.Data);

                _workingDocument = renameResult.Document;
                initialSourceNode = await _workingDocument.GetSyntaxRootAsync();

                firstAnnotatedItem = initialSourceNode.GetAnnotatedNodes(annotationOfFirstAnnotatedItem).FirstOrDefault();
                initialSourceNode = initialSourceNode.ReplaceNode(firstAnnotatedItem, firstAnnotatedItem.WithoutAnnotations(SelectedMethodAnnotationRename));

                _workingDocument = _workingDocument.WithSyntaxRoot(initialSourceNode);
                initialSourceNode = await _workingDocument.GetSyntaxRootAsync();
            }
            while (annotations.Any());

            var lineSpan = initialSourceNode.GetAnnotatedNodes(SelectedMethodAnnotationRemove)
                .FirstOrDefault().GetFileLinePosSpan();

            initialSourceNode = initialSourceNode.RemoveNodes(initialSourceNode.GetAnnotatedNodes(SelectedMethodAnnotationRemove), SyntaxRemoveOptions.KeepEndOfLine);
            var t = initialSourceNode.DescendantNodes().OfType<FieldDeclarationSyntax>().Where(x => x.Declaration.Variables.Count == 0);
            initialSourceNode = initialSourceNode.RemoveNodes(t, SyntaxRemoveOptions.KeepEndOfLine);

            _workingDocument = _workingDocument.WithSyntaxRoot(initialSourceNode);

            if (IsReportOnlyMode &&
                !IsEquivalentToUnModified(initialSourceNode))
            {
                CollectMessages(new ChangesReport(await _orginalDocument.GetSyntaxRootAsync())
                {
                    LineNumber = lineSpan.StartLinePosition.Line,
                    Column = lineSpan.StartLinePosition.Character,
                    Message = "ConvertPropertiesToAutoProperties",
                    Generator = nameof(ConvertPropertiesToAutoProperties)
                });

                return initialSourceNode;
            }

            return null;
        }

        return initialSourceNode;
    }

    protected override async Task SaveResultAsync(SyntaxNode initialSourceNode)
    {
        await base.SaveResultAsync((await _workingDocument.GetSyntaxTreeAsync()).GetRoot());
    }

    private class MyWalker : CSharpSyntaxWalker
    {
        private const string VarKeyword = "var";
        private readonly ProjectItemDetailsType _projectItemDetails;
        private readonly SemanticModel _semanticModel;

        public MyWalker(ProjectItemDetailsType projectItemDetails, ICleanupOption options)
        {
            _projectItemDetails = projectItemDetails;
            _semanticModel = projectItemDetails.SemanticModel;
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax propertyDeclaration)
        {
            Task.Run(async delegate
                {
                    await FindFullPropertyForConvertingAutoPropertyAsync(propertyDeclaration);
                }).GetAwaiter().GetResult();
        }

        public List<Tuple<VariableDeclaratorSyntax, PropertyDeclarationSyntax, bool>> VariablesToRemove = new();

        private async Task<PropertyDeclarationSyntax> FindFullPropertyForConvertingAutoPropertyAsync(PropertyDeclarationSyntax propertyDeclaration)
        {
            if (propertyDeclaration.AccessorList == null) return null;
            if (propertyDeclaration.AccessorList.Accessors.Count != 2) return null;

            var getNode = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(x => x.Keyword.IsKind(SyntaxKind.GetKeyword));
            var setNode = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(x => x.Keyword.IsKind(SyntaxKind.SetKeyword));

            if (setNode == null || getNode == null) return null;

            var getIdentifier = HasJustReturnValue(getNode);
            if (getIdentifier == null) return null;

            var setIdentifier = HasJustSetValue(setNode);
            if (setIdentifier == null) return null;

            var setIdentifierSymbol = CSharpExtensions.GetSymbolInfo(_projectItemDetails.SemanticModel, setIdentifier).Symbol;
            var getIdentifierSymbol = CSharpExtensions.GetSymbolInfo(_projectItemDetails.SemanticModel, getIdentifier).Symbol;

            if (SymbolEqualityComparer.Default.Equals(setIdentifierSymbol, getIdentifierSymbol) == false) return null;
            //if (setIdentifierSymbol != getIdentifierSymbol) return null;

            var baseFieldSymbol = CSharpExtensions.GetSymbolInfo(_projectItemDetails.SemanticModel, setIdentifier).Symbol;
            if (baseFieldSymbol is IFieldSymbol == false) return null;

            var propertyDelarationSymbol = CSharpExtensions.GetDeclaredSymbol(_projectItemDetails.SemanticModel, propertyDeclaration);

            if (SymbolEqualityComparer.Default.Equals(baseFieldSymbol.ContainingType, propertyDelarationSymbol.ContainingType) == false) return null;
            //if (baseFieldSymbol.ContainingType != propertyDelarationSymbol.ContainingType) return null;

            var baseFieldVariableDeclaration = await baseFieldSymbol.DeclaringSyntaxReferences[0].GetSyntaxAsync() as VariableDeclaratorSyntax;
            var baseFieldFieldDeclaration = baseFieldVariableDeclaration.Parent.Parent as FieldDeclarationSyntax;

            if (CheckVisibility(baseFieldFieldDeclaration, getNode, setNode, propertyDeclaration) == false) return null;

            var refrences = await SymbolFinder.FindReferencesAsync(baseFieldSymbol, TidyCSharpPackage.Instance.Solution);

            ReferencedSymbol references = null;
            references = refrences.FirstOrDefault();

            if (references == null) return null;

            if (baseFieldFieldDeclaration.HasNoneWhiteSpaceTrivia(new SyntaxKind[] { SyntaxKind.SingleLineCommentTrivia, SyntaxKind.MultiLineCommentTrivia })) return null;

            VariablesToRemove.Add(
                new Tuple<VariableDeclaratorSyntax, PropertyDeclarationSyntax, bool>
                (
                    baseFieldVariableDeclaration,
                    propertyDeclaration,
                    references.Locations.Count() > 2
                )
            );

            return propertyDeclaration;
        }

        private bool CheckVisibility(FieldDeclarationSyntax baseFieldFieldDeclaration, AccessorDeclarationSyntax getNode, AccessorDeclarationSyntax setNode, PropertyDeclarationSyntax propertyDeclaration)
        {
            var methodResult = true;
            if (baseFieldFieldDeclaration.IsPrivate())
            {
                return methodResult;
            }

            if (baseFieldFieldDeclaration.IsPublic())
            {
                if (propertyDeclaration.IsPublic() == false || getNode.Modifiers.Any() || setNode.Modifiers.Any()) return false;
            }
            else if (baseFieldFieldDeclaration.IsProtected())
            {
                if (propertyDeclaration.IsPrivate() == false) return false;

                if (propertyDeclaration.IsProtected() && (getNode.Modifiers.Any() || setNode.Modifiers.Any())) return false;

                else if (propertyDeclaration.IsPublic())
                {
                    if (baseFieldFieldDeclaration.IsInternal() == false)
                    {
                        if (getNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword) || x.IsKind(SyntaxKind.InternalKeyword))) return false;
                        if (setNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword) || x.IsKind(SyntaxKind.InternalKeyword))) return false;
                    }
                    else
                    {
                        if (getNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword))) return false;
                        if (setNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword))) return false;
                    }
                }
            }
            else if (baseFieldFieldDeclaration.IsInternal())
            {
                if (propertyDeclaration.IsPrivate() == false) return false;

                if (propertyDeclaration.IsInternal() && (getNode.Modifiers.Any() || setNode.Modifiers.Any())) return false;

                else if (propertyDeclaration.IsPublic())
                {
                    if (getNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword) || x.IsKind(SyntaxKind.ProtectedKeyword))) return false;
                    if (setNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword) || x.IsKind(SyntaxKind.ProtectedKeyword))) return false;
                }
            }

            return methodResult;
        }

        private IdentifierNameSyntax HasJustSetValue(AccessorDeclarationSyntax setNode)
        {
            if (setNode.Body != null)
            {
                if (setNode.Body.Statements.Count > 1) return null;
                var singleStatement = setNode.Body.Statements.Single() as ExpressionStatementSyntax;
                if (singleStatement == null) return null;

                if (singleStatement.Expression is AssignmentExpressionSyntax assignMent)
                {
                    if (assignMent.OperatorToken.ValueText != "=") return null;

                    if (assignMent.Right is IdentifierNameSyntax rightIdentifier)
                    {
                        if (rightIdentifier.Identifier.ValueText != "value") return null;

                        if (assignMent.Left is IdentifierNameSyntax leftIdentifier)
                        {
                            return leftIdentifier;
                        }
                    }

                    return null;
                }

                return null;
            }
            else // if(getNode)
            {
            }

            return null;
        }

        private IdentifierNameSyntax HasJustReturnValue(AccessorDeclarationSyntax getNode)
        {
            if (getNode.Body != null)
            {
                if (getNode.Body.Statements.Count > 1) return null;
                var singleStatement = getNode.Body.Statements.Single() as ReturnStatementSyntax;
                if (singleStatement == null) return null;

                if (singleStatement.Expression is IdentifierNameSyntax identifierExpression)
                {
                    return identifierExpression;
                }

                return null;
            }
            else // if(getNode)
            {
            }

            return null;
        }
    }

    private class Rewriter : CleanupCSharpSyntaxRewriter
    {
        private const string VarKeyword = "var";
        private readonly MyWalker _walker;
        private readonly ProjectItemDetailsType _projectItemDetails;
        private readonly SemanticModel _semanticModel;
        private Document _workingDocument;

        public Rewriter(MyWalker walker, ProjectItemDetailsType projectItemDetails, ICleanupOption options)
            : base(false, options)
        {
            _walker = walker;
            _projectItemDetails = projectItemDetails;
            _semanticModel = projectItemDetails.SemanticModel;
            _workingDocument = projectItemDetails.Document;
        }

        public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var foundedItem = _walker.VariablesToRemove.FirstOrDefault(x => node == x.Item1);

            if (foundedItem != null)
            {
                if (foundedItem.Item3)
                {
                    node = node.WithAdditionalAnnotations(new SyntaxAnnotation(SelectedMethodAnnotationRename, foundedItem.Item2.Identifier.ValueText));
                }

                node = node.WithAdditionalAnnotations(new SyntaxAnnotation(SelectedMethodAnnotationRemove));
            }

            return base.VisitVariableDeclarator(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax propertyDeclaration)
        {
            var foundedItem = _walker.VariablesToRemove.FirstOrDefault(x => x.Item2 == propertyDeclaration);

            if (foundedItem != null)
            {
                propertyDeclaration = ConvertProperty(foundedItem.Item2, foundedItem.Item1.Parent.Parent as FieldDeclarationSyntax);
            }

            return base.VisitPropertyDeclaration(propertyDeclaration);
        }

        private PropertyDeclarationSyntax ConvertProperty(PropertyDeclarationSyntax propertyDeclaration, FieldDeclarationSyntax relatedField)
        {
            var leadingList = new SyntaxTriviaList();

            if (relatedField.Declaration.Variables.Count == 1)
            {
                leadingList = leadingList.AddRange(relatedField.GetLeadingTrivia());
            }

            var propertyDeclarationGetLeadingTrivia = propertyDeclaration.GetLeadingTrivia();

            if (leadingList.Any() && propertyDeclarationGetLeadingTrivia.FirstOrDefault().IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                var endOfLine = relatedField.GetLeadingTrivia().FirstOrDefault(x => x.IsKind(SyntaxKind.EndOfLineTrivia));

                if (!endOfLine.IsKind(SyntaxKind.None))
                {
                    leadingList = leadingList.Add(endOfLine);
                }
            }

            leadingList = leadingList.AddRange(propertyDeclaration.GetLeadingTrivia());

            var getNode = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(x => x.Keyword.IsKind(SyntaxKind.GetKeyword));
            var setNode = propertyDeclaration.AccessorList.Accessors.FirstOrDefault(x => x.Keyword.IsKind(SyntaxKind.SetKeyword));

            propertyDeclaration =
                propertyDeclaration
                    .WithAccessorList
                    (
                        propertyDeclaration.AccessorList.WithAccessors(

                                new SyntaxList<AccessorDeclarationSyntax>()
                                    .Add(
                                        getNode
                                            .WithBody(null)
                                            .WithTrailingTrivia()
                                            .WithSemicolonToken(
                                                SyntaxFactory.ParseToken(";")
                                                    .WithTrailingTrivia(SyntaxFactory.Space)
                                            )
                                            .WithLeadingTrivia(SyntaxFactory.Space)

                                    )
                                    .Add(
                                        setNode
                                            .WithBody(null)
                                            .WithTrailingTrivia()
                                            .WithSemicolonToken(
                                                SyntaxFactory.ParseToken(";")
                                                    .WithTrailingTrivia(SyntaxFactory.Space)

                                            )
                                            .WithLeadingTrivia(SyntaxFactory.Space)
                                    )
                            )
                            .WithOpenBraceToken(propertyDeclaration.AccessorList.OpenBraceToken.WithLeadingTrivia().WithTrailingTrivia())
                            .WithCloseBraceToken(propertyDeclaration.AccessorList.CloseBraceToken.WithLeadingTrivia()).WithoutTrailingTrivia()
                    )
                    .WithLeadingTrivia(leadingList)
                    .WithIdentifier(propertyDeclaration.Identifier.WithTrailingTrivia(SyntaxFactory.Space));

            if (relatedField.Declaration.Variables.FirstOrDefault().Initializer != null)
            {
                propertyDeclaration =
                    propertyDeclaration.WithInitializer(
                            relatedField.Declaration.Variables.FirstOrDefault().Initializer)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }

            return propertyDeclaration;
        }
    }
}
using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class ConvertPropertiesToAutoProperties : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        Document WorkingDocument, orginalDocument;
        const string SELECTED_METHOD_ANNOTATION_RENAME = "SELECTED_METHOD_ANNOTATION_RENAME";
        const string SELECTED_METHOD_ANNOTATION_REMOVE = "SELECTED_METHOD_ANNOTATION_REMOVE";

        public override async Task<SyntaxNode> CleanUp(SyntaxNode initialSourceNode)
        {
            orginalDocument = ProjectItemDetails.ProjectItemDocument;
            WorkingDocument = ProjectItemDetails.ProjectItemDocument;

            var walker = new MyWalker(ProjectItemDetails, Options);
            walker.Visit(initialSourceNode);

            if (walker.VariablesToRemove.Any())
            {
                var rewriter = new Rewriter(walker, ProjectItemDetails, Options);
                initialSourceNode = rewriter.Visit(initialSourceNode);
                WorkingDocument = WorkingDocument.WithSyntaxRoot(initialSourceNode);

                IEnumerable<SyntaxNode> annotations;

                do
                {
                    annotations = initialSourceNode.GetAnnotatedNodes(SELECTED_METHOD_ANNOTATION_RENAME);

                    if (annotations.Any() == false) break;

                    var firstAnnotatedItem = annotations.FirstOrDefault();
                    var annotationOfFirstAnnotatedItem = firstAnnotatedItem.GetAnnotations(SELECTED_METHOD_ANNOTATION_RENAME).FirstOrDefault();

                    var renameResult = await Renamer.RenameSymbol(WorkingDocument, initialSourceNode, null, firstAnnotatedItem, annotationOfFirstAnnotatedItem.Data);

                    WorkingDocument = renameResult.Document;
                    initialSourceNode = await WorkingDocument.GetSyntaxRootAsync();

                    firstAnnotatedItem = initialSourceNode.GetAnnotatedNodes(annotationOfFirstAnnotatedItem).FirstOrDefault();
                    initialSourceNode = initialSourceNode.ReplaceNode(firstAnnotatedItem, firstAnnotatedItem.WithoutAnnotations(SELECTED_METHOD_ANNOTATION_RENAME));

                    WorkingDocument = WorkingDocument.WithSyntaxRoot(initialSourceNode);
                    initialSourceNode = await WorkingDocument.GetSyntaxRootAsync();
                }
                while (annotations.Any());

                var lineSpan = initialSourceNode.GetAnnotatedNodes(SELECTED_METHOD_ANNOTATION_REMOVE)
                    .FirstOrDefault().GetFileLinePosSpan();

                initialSourceNode = initialSourceNode.RemoveNodes(initialSourceNode.GetAnnotatedNodes(SELECTED_METHOD_ANNOTATION_REMOVE), SyntaxRemoveOptions.KeepEndOfLine);
                var t = initialSourceNode.DescendantNodes().OfType<FieldDeclarationSyntax>().Where(x => x.Declaration.Variables.Count == 0);
                initialSourceNode = initialSourceNode.RemoveNodes(t, SyntaxRemoveOptions.KeepEndOfLine);

                WorkingDocument = WorkingDocument.WithSyntaxRoot(initialSourceNode);

                if (IsReportOnlyMode &&
                    !IsEquivalentToUnModified(initialSourceNode))
                {
                    CollectMessages(new ChangesReport(await orginalDocument.GetSyntaxRootAsync())
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

        protected override async Task SaveResult(SyntaxNode initialSourceNode)
        {
            if (WorkingDocument is null) return;
            var text = await WorkingDocument.GetTextAsync();

            if (string.Compare(text?.ToString(), ProjectItemDetails.InitialSourceNode.GetText().ToString(), false) != 0)
            {
                TidyCSharpPackage.Instance.RefreshSolution(WorkingDocument.Project.Solution);
            }
        }

        class MyWalker : CSharpSyntaxWalker
        {
            const string VarKeyword = "var";
            readonly ProjectItemDetailsType projectItemDetails;
            readonly SemanticModel semanticModel;

            public MyWalker(ProjectItemDetailsType projectItemDetails, ICleanupOption options)
            {
                this.projectItemDetails = projectItemDetails;
                semanticModel = projectItemDetails.SemanticModel;
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax propertyDeclaration)
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory
                    .Run(async delegate
                {
                    await FindFullPropertyForConvertingAutoProperty(propertyDeclaration);
                });
            }

            public List<Tuple<VariableDeclaratorSyntax, PropertyDeclarationSyntax, bool>> VariablesToRemove = new List<Tuple<VariableDeclaratorSyntax, PropertyDeclarationSyntax, bool>>();

            async Task<PropertyDeclarationSyntax> FindFullPropertyForConvertingAutoProperty(PropertyDeclarationSyntax propertyDeclaration)
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

                var setIdentifierSymbol = projectItemDetails.SemanticModel.GetSymbolInfo(setIdentifier).Symbol;
                var getIdentifierSymbol = projectItemDetails.SemanticModel.GetSymbolInfo(getIdentifier).Symbol;

                if (setIdentifierSymbol != getIdentifierSymbol) return null;

                var baseFieldSymbol = projectItemDetails.SemanticModel.GetSymbolInfo(setIdentifier).Symbol;
                if (baseFieldSymbol is IFieldSymbol == false) return null;

                var propertyDelarationSymbol = projectItemDetails.SemanticModel.GetDeclaredSymbol(propertyDeclaration);

                if (baseFieldSymbol.ContainingType != propertyDelarationSymbol.ContainingType) return null;

                var baseFieldVariableDeclaration = baseFieldSymbol.DeclaringSyntaxReferences[0].GetSyntax() as VariableDeclaratorSyntax;
                var baseFieldFieldDeclaration = baseFieldVariableDeclaration.Parent.Parent as FieldDeclarationSyntax;

                if (CheckVisibility(baseFieldFieldDeclaration, getNode, setNode, propertyDeclaration) == false) return null;

                var refrences = await Microsoft.CodeAnalysis.FindSymbols.SymbolFinder.FindReferencesAsync(baseFieldSymbol, TidyCSharpPackage.Instance.CleanupWorkingSolution);

                ReferencedSymbol references = null;
                references = refrences.FirstOrDefault();

                if (references == null) return null;

                if (baseFieldFieldDeclaration.HasNoneWhitespaceTrivia(new SyntaxKind[] { SyntaxKind.SingleLineCommentTrivia, SyntaxKind.MultiLineCommentTrivia })) return null;

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

            bool CheckVisibility(FieldDeclarationSyntax baseFieldFieldDeclaration, AccessorDeclarationSyntax getNode, AccessorDeclarationSyntax setNode, PropertyDeclarationSyntax propertyDeclaration)
            {
                if (baseFieldFieldDeclaration.IsPrivate())
                {
                    return true;
                }
                else
                {
                    if (baseFieldFieldDeclaration.IsPublic())
                    {
                        if (propertyDeclaration.IsPublic() == false) return false;
                        if (getNode.Modifiers.Any()) return false;
                        if (setNode.Modifiers.Any()) return false;
                    }
                    else if (baseFieldFieldDeclaration.IsProtected())
                    {
                        if (propertyDeclaration.IsPrivate() == false) return false;

                        if (propertyDeclaration.IsProtected())
                        {
                            if (getNode.Modifiers.Any()) return false;
                            if (setNode.Modifiers.Any()) return false;
                        }
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

                        if (propertyDeclaration.IsInternal())
                        {
                            if (getNode.Modifiers.Any()) return false;
                            if (setNode.Modifiers.Any()) return false;
                        }
                        else if (propertyDeclaration.IsPublic())
                        {
                            if (getNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword) || x.IsKind(SyntaxKind.ProtectedKeyword))) return false;
                            if (setNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword) || x.IsKind(SyntaxKind.ProtectedKeyword))) return false;
                        }
                    }
                }

                return true;
            }

            IdentifierNameSyntax HasJustSetValue(AccessorDeclarationSyntax setNode)
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

            IdentifierNameSyntax HasJustReturnValue(AccessorDeclarationSyntax getNode)
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

        class Rewriter : CleanupCSharpSyntaxRewriter
        {
            const string VarKeyword = "var";
            readonly MyWalker walker;
            readonly ProjectItemDetailsType projectItemDetails;
            readonly SemanticModel semanticModel;
            Document WorkingDocument;

            public Rewriter(MyWalker walker, ProjectItemDetailsType projectItemDetails, ICleanupOption options)
                : base(false, options)
            {
                this.walker = walker;
                this.projectItemDetails = projectItemDetails;
                semanticModel = projectItemDetails.SemanticModel;
                WorkingDocument = projectItemDetails.ProjectItemDocument;
            }

            public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                var foundedItem = walker.VariablesToRemove.FirstOrDefault(x => node == x.Item1);

                if (foundedItem != null)
                {
                    if (foundedItem.Item3)
                    {
                        node = node.WithAdditionalAnnotations(new SyntaxAnnotation(SELECTED_METHOD_ANNOTATION_RENAME, foundedItem.Item2.Identifier.ValueText));
                    }

                    node = node.WithAdditionalAnnotations(new SyntaxAnnotation(SELECTED_METHOD_ANNOTATION_REMOVE));
                }

                return base.VisitVariableDeclarator(node);
            }

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax propertyDeclaration)
            {
                var foundedItem = walker.VariablesToRemove.FirstOrDefault(x => x.Item2 == propertyDeclaration);

                if (foundedItem != null)
                {
                    propertyDeclaration = ConvertProperty(foundedItem.Item2, foundedItem.Item1.Parent.Parent as FieldDeclarationSyntax);
                }

                return base.VisitPropertyDeclaration(propertyDeclaration);
            }

            PropertyDeclarationSyntax ConvertProperty(PropertyDeclarationSyntax propertyDeclaration, FieldDeclarationSyntax relatedField)
            {
                var leadingList = new SyntaxTriviaList();

                if (relatedField.Declaration.Variables.Count == 1)
                {
                    leadingList = leadingList.AddRange(relatedField.GetLeadingTrivia());
                }

                var propertyDeclaration_GetLeadingTrivia = propertyDeclaration.GetLeadingTrivia();

                if (leadingList.Any() && propertyDeclaration_GetLeadingTrivia.FirstOrDefault().IsKind(SyntaxKind.EndOfLineTrivia) == false)
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
}
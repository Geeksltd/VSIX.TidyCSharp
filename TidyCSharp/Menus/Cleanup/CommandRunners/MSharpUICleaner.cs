using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class MSharpUICleaner : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            if (App.DTE.ActiveDocument.ProjectItem.ProjectItems.ContainingProject.Name == "#UI")
                return ChangeMethodHelper(initialSourceNode);
            return initialSourceNode;
        }

        SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode)
        {
            initialSourceNode = new SendItemIdRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new IfWorkFlowRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new CustomColumnRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new ElementsNewSyntaxRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new FormModuleWorkFlowRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new GeneralWorkFlowRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new CombinedExpressionsFormRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new ListModuleWorkFlowRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new FullSearchRewriter(ProjectItemDetails.SemanticModel
                , ProjectItemDetails.ProjectItemDocument.Project.Solution
                , ProjectItemDetails.ProjectItemDocument)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new CustomFieldRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            return initialSourceNode;
        }

        //cancelsave,deletecancelsave
        class CombinedExpressionsFormRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public CombinedExpressionsFormRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            class StatementType
            {
                public int Index { get; set; }
                public ExpressionStatementSyntax Row { get; set; }
                public string MethodName { get; set; }
                public List<int> RemovedIndexPositions { get; set; }
            }
            public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                var t = node.DescendantNodes().OfType<ExpressionStatementSyntax>()
                    .Select((r, i) => new StatementType
                    {
                        Row = r,
                        Index = i,
                        MethodName = (semanticModel.GetSymbolInfo(r.Expression).Symbol as IMethodSymbol)?.Name
                    });
                var deleteMethods = t.Where(x => x.MethodName == "Delete")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Delete");
                var cancelMethods = t.Where(x => x.MethodName == "Cancel")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Cancel");
                var saveMethods = t.Where(x => x.MethodName == "Save")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Save");
                int deleteIndex = -1;
                if (deleteMethods.Any())
                {
                    deleteIndex = deleteMethods.FirstOrDefault().Index;
                }
                if (!cancelMethods.Any() || !saveMethods.Any())
                {
                    return base.VisitConstructorDeclaration(node);
                }

                List<int> statementsToRemove = new List<int>();
                cancelMethods.Union(saveMethods).Union(deleteMethods).OrderByDescending(x => x.Index)
                    .Aggregate((arg1, arg2) =>
                    {
                        if (arg1.MethodName == "Save" && arg2.MethodName == "Cancel")
                        {
                            statementsToRemove.Add(arg1.Index);
                            return new StatementType
                            {
                                Index = arg2.Index,
                                Row = SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ParseExpression("button"),
                                                SyntaxFactory.IdentifierName("CancelSave")),
                                            SyntaxFactory.ArgumentList())),
                                MethodName = "CancelSave",
                                RemovedIndexPositions = new List<int>() { arg1.Index }
                            };
                        }
                        if (arg1.MethodName == "CancelSave" && arg2.MethodName == "Delete")
                        {
                            statementsToRemove.AddRange(arg1.RemovedIndexPositions);
                            statementsToRemove.Add(arg1.Index);
                            return new StatementType
                            {
                                Index = arg2.Index,
                                Row = SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ParseExpression("button"),
                                                SyntaxFactory.IdentifierName("DeleteCancelSave")),
                                            SyntaxFactory.ArgumentList())),
                                MethodName = "DeleteCancelSave",
                                RemovedIndexPositions = new List<int>(arg1.RemovedIndexPositions) { arg1.Index }
                            };
                        }
                        return arg2;
                    });
                if (!statementsToRemove.Any())
                    return base.VisitConstructorDeclaration(node);
                node = node.RemoveNodes(cancelMethods.Union(saveMethods).Union(deleteMethods)
                    .Where(x => statementsToRemove.Contains(x.Index)).Select(x => x.Row),
                    SyntaxRemoveOptions.KeepEndOfLine);
                t = node.DescendantNodes().OfType<ExpressionStatementSyntax>()
                    .Select((r, i) => new StatementType
                    {
                        Row = r,
                        Index = i,
                        MethodName = r.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString()
                    });
                deleteMethods = t.Where(x => x.MethodName == "Delete")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Delete");
                cancelMethods = t.Where(x => x.MethodName == "Cancel")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Cancel");
                saveMethods = t.Where(x => x.MethodName == "Save")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Save");
                node = node.ReplaceNodes(cancelMethods.Union(saveMethods).Union(deleteMethods)
                    .Select(x => x.Row),
                        (arg1, arg2) =>
                        {
                            if (arg1.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Delete")
                            {
                                return SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParseExpression("button"),
                                                    SyntaxFactory.IdentifierName("DeleteCancelSave")),
                                                SyntaxFactory.ArgumentList()));
                            }
                            if (arg1.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Cancel")
                            {
                                return SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParseExpression("button"),
                                                    SyntaxFactory.IdentifierName("CancelSave")),
                                                SyntaxFactory.ArgumentList()));
                            }
                            return arg2;
                        });
                return node;
            }
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.BaseList != null &&
                    node.BaseList.Types.Any(x => x.Type.IsKind(SyntaxKind.GenericName) &&
                     ((GenericNameSyntax)x.Type).Identifier.Text == "FormModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }
        }
        class IfWorkFlowRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public IfWorkFlowRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
            {
                var ifNode = node.DescendantNodes().OfType<InvocationExpressionSyntax>()
                    .Where(x =>
                    x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                    ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "If" &&
                    !(((MemberAccessExpressionSyntax)x.Expression).Expression is IdentifierNameSyntax))
                    .FirstOrDefault();
                if (ifNode != null)
                {
                    node = node.ReplaceNodes(ifNode.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() == "If" &&
                                !(((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax))
                            {
                                return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                            }
                            else if (((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() == "If" &&
                               (((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax))
                            {
                                return nde2;
                            }
                            if (((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax)
                            {
                                return SyntaxFactory.InvocationExpression(
                                       SyntaxFactory.MemberAccessExpression(
                                           SyntaxKind.SimpleMemberAccessExpression,
                                           SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParseExpression(((MemberAccessExpressionSyntax)nde1.Expression).Expression.ToString()),
                                                    SyntaxFactory.IdentifierName("If"))
                                                , SyntaxFactory.ArgumentList(ifNode.ArgumentList.Arguments)), ((MemberAccessExpressionSyntax)nde1.Expression).Name),
                                    SyntaxFactory.ArgumentList(nde1.ArgumentList.Arguments)).WithLeadingTrivia(nde1.GetLeadingTrivia());
                            }
                            return nde2;
                        });
                    return VisitSimpleLambdaExpression(node);
                }
                return node;
            }
        }
        class CustomColumnRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public CustomColumnRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var customColumnNode = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                    .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "CustomColumn").FirstOrDefault();

                if (customColumnNode != null)
                {
                    var newNode = node.ReplaceNodes(node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.ToString() == "CustomColumn()")
                            {
                                SeparatedSyntaxList<ArgumentSyntax> args = new SeparatedSyntaxList<ArgumentSyntax>();
                                var argsHeaderText = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                                    .Where(x => ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() ==
                                    "HeaderText").FirstOrDefault().ArgumentList;
                                var argsDisplayExpr = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                                    .Where(x => ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "DisplayExpression").FirstOrDefault().ArgumentList;

                                if (argsHeaderText.Arguments.Count == 1)
                                    args = args.Add(argsHeaderText.Arguments.FirstOrDefault());

                                if (argsDisplayExpr.Arguments.Count == 1)
                                    args = args.Add(argsDisplayExpr.Arguments.FirstOrDefault());

                                return SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                         SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ParseExpression("column"),
                                    SyntaxFactory.IdentifierName("Custom")),
                                    SyntaxFactory.ArgumentList(args))
                                .WithLeadingTrivia(nde1.GetLeadingTrivia());
                            }
                            else if (((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() ==
                               "HeaderText" &&
                               nde1.ArgumentList.Arguments.Count == 1)
                            {
                                return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                            }
                            else if (((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() ==
                              "DisplayExpression" &&
                              nde1.ArgumentList.Arguments.Count == 1)
                            {
                                return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                            }

                            return nde2;
                        });
                    return newNode;
                }
                return base.VisitInvocationExpression(node);
            }
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.BaseList != null &&
                    node.BaseList.Types.Any(x => x.Type.IsKind(SyntaxKind.GenericName) &&
                     ((GenericNameSyntax)x.Type).Identifier.Text == "ListModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }
        }
        class SendItemIdRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public SendItemIdRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                var methodName = methodSymbol?.Name;
                if (methodName == "Send")
                {
                    if (node.ArgumentList.Arguments.Count == 2 &&
                        node.ArgumentList.Arguments.FirstOrDefault().ToString() == "\"item\"" &&
                        node.ArgumentList.Arguments.LastOrDefault().ToString() == "\"item.ID\"")
                    {
                        var member = node.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                        member = member
                            .WithTrailingTrivia(member.GetTrailingTrivia().Union(node.GetLeadingTrivia()));
                        return SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression, member, SyntaxFactory.IdentifierName("SendItemId")),
                            SyntaxFactory.ArgumentList());
                    }
                }
                return base.VisitInvocationExpression(node);
            }
        }
        //column.search.field.
        class ElementsNewSyntaxRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public ElementsNewSyntaxRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                var methodName = methodSymbol?.Name;
                var methodType = methodSymbol?.ReturnType.OriginalDefinition?.ToString();

                var methodDefTypes = new string[] {
                    "MSharp.PropertyFilterElement<TEntity>",
                    "MSharp.ViewElement<TEntity>",
                    "MSharp.ViewElement<TEntity, TProperty>",
                    "MSharp.NumberFormElement",
                    "MSharp.BooleanFormElement",
                    "MSharp.DateTimeFormElement",
                    "MSharp.StringFormElement",
                    "MSharp.AssociationFormElement",
                    "MSharp.BinaryFormElement",
                    "MSharp.CommonFilterElement<TEntity>"
                };
                var methodDefNames = new string[] { "Field", "Column", "Search" };

                if (methodDefTypes.Contains(methodType) && methodDefNames.Contains(methodName))
                {
                    if (node.ArgumentList.Arguments.Count == 1 && node.ArgumentList.Arguments[0].Expression is SimpleLambdaExpressionSyntax)
                    {
                        var arg = ((MemberAccessExpressionSyntax)((SimpleLambdaExpressionSyntax)node.ArgumentList.Arguments[0].Expression).Body).Name;
                        return SyntaxFactory.ParseExpression($"{methodName.ToLower()}.{arg}()")
                            .WithLeadingTrivia(node.GetLeadingTrivia());
                    }
                }

                if (methodType == "MSharp.ListButton<TEntity>" || methodType == "MSharp.ModuleButton")
                {
                    switch (methodName)
                    {
                        case "ButtonColumn":
                            return node.WithExpression(SyntaxFactory.ParseExpression("column.Button"))
                                .WithLeadingTrivia(node.GetLeadingTrivia()); ;
                        case "LinkColumn":
                            return node.WithExpression(SyntaxFactory.ParseExpression("column.Link"))
                                .WithLeadingTrivia(node.GetLeadingTrivia());
                        case "SearchButton":
                            return node.WithExpression(SyntaxFactory.ParseExpression("search.Button"))
                                .WithLeadingTrivia(node.GetLeadingTrivia());
                    }
                }
                return base.VisitInvocationExpression(node);
            }
        }
        //cancel/save/delete
        class FormModuleWorkFlowRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public FormModuleWorkFlowRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var s = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "OnClick").FirstOrDefault();
                if (s == null)
                    return base.VisitInvocationExpression(node);
                var methodSymbol = (semanticModel.GetSymbolInfo(s).Symbol as IMethodSymbol);

                if (s.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().Count() == 1 &&
                    methodSymbol?.ReturnType.OriginalDefinition?.ToString() == "MSharp.ModuleButton")
                {
                    var saveMethods = new string[] { "ReturnToPreviousPage", "SaveInDatabase" };
                    var saveModalMethods = new string[] { "CloseModal(Refresh.Full)", "SaveInDatabase" };
                    var cancelMethods = new string[] { "ReturnToPreviousPage" };
                    var cancelModalMethods = new string[] { "CloseModal" };
                    var deleteMethods = new string[] { "ReturnToPreviousPage", "DeleteItem" };
                    var deleteModalMethods = new string[] { "CloseModal(Refresh.Full)", "DeleteItem" };
                    var optionalMethods = new string[] { "GentleMessage(\"Deleted successfully.\")" };

                    var lambdaExpressionArgument = s.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().FirstOrDefault();
                    var invocations = lambdaExpressionArgument.DescendantNodes().OfType<InvocationExpressionSyntax>();
                    if (invocations.Count() == 1 &&
                        (invocations.All(x => cancelMethods.Any(y => x.ToString().Contains(y))) ||
                     (invocations.All(x => cancelModalMethods.Any(y => x.ToString().Contains(y))))))
                    {
                        var m = node;
                        var methodSelector = invocations.Any(x => x.ToString().Contains("ReturnToPreviousPage"));
                        var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("button"),
                                SyntaxFactory.IdentifierName(methodSelector ? "Cancel" : "ModalCancel")),
                            SyntaxFactory.ArgumentList());

                        while (m != null && m.ChildNodes().Any())
                        {
                            var m2 = m.ChildNodes();
                            if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                                m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as MemberAccessExpressionSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.Name.ToString() == "Icon" && (arguments.Arguments.Count != 1
                                    || arguments.Arguments.FirstOrDefault().Expression.ToString() != "FA.Backward"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "CausesValidation" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "false"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "OnClick" && (arguments.Arguments.Count != 1 ||
                                    ((!arguments.Arguments.FirstOrDefault().Expression.ToString().Contains("ReturnToPreviousPage")) &&
                                    (!arguments.Arguments.FirstOrDefault().Expression.ToString().Contains("CloseModal")))))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                else if (methodName.Name.ToString() != "OnClick" &&
                                   methodName.Name.ToString() != "CausesValidation" &&
                                   methodName.Name.ToString() != "Icon")
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                m = (m2.FirstOrDefault() as MemberAccessExpressionSyntax).Expression
                                    as InvocationExpressionSyntax;
                            }
                            else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                               m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as IdentifierNameSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.ToString() == "Button" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"Cancel\""))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                m = m2.FirstOrDefault() as InvocationExpressionSyntax;
                            }
                        }
                        return newExpression
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithTrailingTrivia(node.GetTrailingTrivia());
                    }
                    else if (((invocations.Count() == 2 || invocations.Count() == 3) &&
                        invocations.All(x => x.Expression is MemberAccessExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)(x.Expression)).Expression is IdentifierNameSyntax)) &&
                        (invocations.All(x => saveMethods.Any(y => x.ToString().Contains(y))) ||
                        invocations.All(x => saveModalMethods.Any(y => x.ToString().Contains(y)))))
                    {
                        var m = node;
                        var methodSelector = invocations.Any(x => x.ToString().Contains("ReturnToPreviousPage"));
                        var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("button"),
                                SyntaxFactory.IdentifierName(methodSelector ? "Save" : "ModalSave")),
                            SyntaxFactory.ArgumentList());

                        while (m != null && m.ChildNodes().Any())
                        {
                            var m2 = m.ChildNodes();
                            if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                                m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as MemberAccessExpressionSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.Name.ToString() == "Icon" && (arguments.Arguments.Count != 1
                                    || arguments.Arguments.FirstOrDefault().Expression.ToString() != "FA.Check"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "CausesValidation" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "true"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "OnClick" && ((arguments.Arguments.Count != 1) ||
                                    !((arguments.Arguments.FirstOrDefault().Expression as SimpleLambdaExpressionSyntax).Body as BlockSyntax)
                                        .Statements.Any(x => saveMethods.Any(y => x.ToString().Contains(y))) ||
                                    !((arguments.Arguments.FirstOrDefault().Expression as SimpleLambdaExpressionSyntax).Body as BlockSyntax)
                                        .Statements.Any(x => saveModalMethods.Any(y => x.ToString().Contains(y)))))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                else if (methodName.Name.ToString() != "OnClick" &&
                                   methodName.Name.ToString() != "CausesValidation" &&
                                   methodName.Name.ToString() != "Icon")
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                m = (m2.FirstOrDefault() as MemberAccessExpressionSyntax).Expression
                                     as InvocationExpressionSyntax;
                            }
                            else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                               m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as IdentifierNameSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.ToString() == "Button" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"Save\""))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                m = m2.FirstOrDefault() as InvocationExpressionSyntax;
                            }
                        }
                        return newExpression
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithTrailingTrivia(node.GetTrailingTrivia());
                    }
                    else if (((invocations.Count() == 2 || invocations.Count() == 3) &&
                        invocations.All(x => x.Expression is MemberAccessExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)(x.Expression)).Expression is IdentifierNameSyntax)) &&
                        (invocations.All(x => deleteMethods.Union(optionalMethods).Any(y => x.ToString().Contains(y))) ||
                        invocations.All(x => deleteModalMethods.Union(optionalMethods).Any(y => x.ToString().Contains(y)))))
                    {
                        var m = node;
                        var methodSelector = invocations.Any(x => x.ToString().Contains("ReturnToPreviousPage"));
                        var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("button"),
                                SyntaxFactory.IdentifierName(methodSelector ? "Delete" : "ModalDelete")),
                            SyntaxFactory.ArgumentList());

                        while (m != null && m.ChildNodes().Any())
                        {
                            var m2 = m.ChildNodes();
                            if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                                m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as MemberAccessExpressionSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.Name.ToString() == "VisibleIf" && (arguments.Arguments.Count != 1
                                    || arguments.Arguments.FirstOrDefault().Expression.ToString() != "CommonCriterion.IsEditMode_Item_IsNew"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "Icon" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "FA.Close"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "ConfirmQuestion" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"Are you sure you want to delete it?\""))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "OnClick" && ((arguments.Arguments.Count != 1) ||
                                    !((arguments.Arguments.FirstOrDefault().Expression as SimpleLambdaExpressionSyntax).Body as BlockSyntax)
                                        .Statements.Any(x => deleteMethods.Any(y => x.ToString().Contains(y))) ||
                                    !((arguments.Arguments.FirstOrDefault().Expression as SimpleLambdaExpressionSyntax).Body as BlockSyntax)
                                        .Statements.Any(x => deleteModalMethods.Any(y => x.ToString().Contains(y)))))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                else if (methodName.Name.ToString() != "OnClick" &&
                                   methodName.Name.ToString() != "VisibleIf" &&
                                   methodName.Name.ToString() != "Icon" &&
                                   methodName.Name.ToString() != "ConfirmQuestion")
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                m = (m2.FirstOrDefault() as MemberAccessExpressionSyntax).Expression
                                     as InvocationExpressionSyntax;
                            }
                            else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                               m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as IdentifierNameSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.ToString() == "Button" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"Delete\""))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                m = m2.FirstOrDefault() as InvocationExpressionSyntax;
                            }
                        }
                        return newExpression
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithTrailingTrivia(node.GetTrailingTrivia());
                    }


                }
                return base.VisitInvocationExpression(node);
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.BaseList != null &&
                    node.BaseList.Types.Any(x => x.Type.IsKind(SyntaxKind.GenericName) &&
                     ((GenericNameSyntax)x.Type).Identifier.Text == "FormModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }
        }
        //onlick-go -> go
        class GeneralWorkFlowRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public GeneralWorkFlowRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var s = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "OnClick").FirstOrDefault();
                if (s == null)
                    return base.VisitInvocationExpression(node);
                var methodSymbol = (semanticModel.GetSymbolInfo(s).Symbol as IMethodSymbol);

                if (s.ArgumentList.Arguments.Count() == 1 &&
                    (s.ArgumentList.Arguments.FirstOrDefault()
                    .DescendantNodes().OfType<ExpressionStatementSyntax>().Count() == 1 ||
                    s.ArgumentList.Arguments.FirstOrDefault()
                    .DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().Count() == 1) &&
                    s.ArgumentList.Arguments.FirstOrDefault()
                    .DescendantNodes().OfType<GenericNameSyntax>().Any(x => x.Identifier.ToString() == "Go"))
                {
                    GenericNameSyntax goIdentifier = s.ArgumentList.Arguments.FirstOrDefault()
                        .DescendantNodes().OfType<GenericNameSyntax>().FirstOrDefault(x => x.Identifier.ToString() == "Go");
                    var newNode = node.ReplaceNodes(s.ArgumentList.Arguments.FirstOrDefault()
                        .DescendantNodes().OfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (((MemberAccessExpressionSyntax)nde1.Expression).Name is GenericNameSyntax &&
                            ((MemberAccessExpressionSyntax)nde1.Expression).Name.Identifier.ToString() == "Go" &&
                                nde1.ArgumentList.Arguments.Count == 0)
                            {
                                if (!(((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax))
                                    return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                                else return nde2.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                            }
                            return nde2;
                        });

                    newNode = newNode.ReplaceNode(newNode.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>()
                            .FirstOrDefault(x => x.Identifier.ToString() == "OnClick"),
                                SyntaxFactory.GenericName(goIdentifier.Identifier, goIdentifier.TypeArgumentList));
                    return newNode;
                }
                return base.VisitInvocationExpression(node);
            }
        }
        class ListModuleWorkFlowRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public ListModuleWorkFlowRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var s = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "Go" ||
                        (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "OnClick" ||
                        (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "Link").FirstOrDefault();
                if (s == null)
                    return base.VisitInvocationExpression(node);
                var editRequiredArguments = new string[] { "SendItemId", "SendReturnUrl" };
                var newRequiredArguments = new string[] { "SendReturnUrl" };
                var methodSymbol = (semanticModel.GetSymbolInfo(s).Symbol as IMethodSymbol);

                if (s.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().Count() == 1 &&
                    s.ArgumentList.Arguments.Count() == 1 &&
                    editRequiredArguments.All(x => (s.ArgumentList.Arguments.FirstOrDefault().DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Select(y => ((MemberAccessExpressionSyntax)y.Expression).Name.ToString())).Any(y => y == x)) &&
                    s.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)x.Expression).Expression is IdentifierNameSyntax &&
                        ((IdentifierNameSyntax)(((MemberAccessExpressionSyntax)x.Expression).Expression)).Identifier.ToString() == "column").Count() == 1)
                {
                    var m = node;
                    var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("column"),
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("Edit"),
                                ((GenericNameSyntax)(((MemberAccessExpressionSyntax)s.Expression).Name)).TypeArgumentList)),
                            SyntaxFactory.ArgumentList());
                    while (m != null && m.ChildNodes().Any())
                    {
                        var m2 = m.ChildNodes();
                        if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                            m2.LastOrDefault() is ArgumentListSyntax)
                        {
                            var methodName = m2.FirstOrDefault() as MemberAccessExpressionSyntax;
                            var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                            if (methodName.Name.Identifier.ToString() == "Go" && ((arguments.Arguments.Count != 1) ||
                                   !(editRequiredArguments.All(x => arguments.Arguments.FirstOrDefault()
                                   .Expression.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                   .FirstOrDefault().ToString().Contains(x)))))
                            {
                                return base.VisitInvocationExpression(node);
                            }
                            else if (methodName.Name.ToString() == "Icon" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "FA.Edit"))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (methodName.Name.ToString() == "NoText" && (arguments.Arguments.Count != 0))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (methodName.Name.ToString() == "HeaderText" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"Actions\""))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (methodName.Name.ToString() == "GridColumnCssClass" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"actions\""))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (methodName.Name.ToString() == "Button" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"Edit\""))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (methodName.Name.Identifier.ToString() != "Go" &&
                               methodName.Name.ToString() != "GridColumnCssClass" &&
                               methodName.Name.ToString() != "HeaderText" &&
                               methodName.Name.ToString() != "NoText" &&
                               methodName.Name.ToString() != "Icon" &&
                               methodName.Name.ToString() != "Button")
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            m = (m2.FirstOrDefault() as MemberAccessExpressionSyntax).Expression
                                as InvocationExpressionSyntax;
                        }
                    }
                    return newExpression
                                .WithLeadingTrivia(node.GetLeadingTrivia())
                                .WithTrailingTrivia(node.GetTrailingTrivia());
                }
                else if (s.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().Count() == 1 &&
                            s.ArgumentList.Arguments.Count() == 1 &&
                            newRequiredArguments.All(x => (s.ArgumentList.Arguments.FirstOrDefault().DescendantNodes()
                            .OfType<InvocationExpressionSyntax>()
                            .Select(y => ((MemberAccessExpressionSyntax)y.Expression).Name.ToString())).Any(y => y == x)))
                {
                    var m = node;
                    var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("button"),
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("New"),
                                ((GenericNameSyntax)(((MemberAccessExpressionSyntax)s.Expression).Name)).TypeArgumentList)),
                            SyntaxFactory.ArgumentList());
                    while (m != null && m.ChildNodes().Any())
                    {
                        var m2 = m.ChildNodes();
                        if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                            m2.LastOrDefault() is ArgumentListSyntax)
                        {
                            var methodName = m2.FirstOrDefault() as MemberAccessExpressionSyntax;
                            var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                            if (methodName.Name.Identifier.ToString() == "Go" && ((arguments.Arguments.Count != 1) ||
                                   !(newRequiredArguments.All(x => arguments.Arguments.FirstOrDefault()
                                   .Expression.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                   .FirstOrDefault().ToString().Contains(x)))))
                            {
                                return base.VisitInvocationExpression(node);
                            }
                            else if (methodName.Name.ToString() == "Icon" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "FA.Plus"))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (methodName.Name.Identifier.ToString() != "Go" &&
                               methodName.Name.ToString() != "Icon")
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            m = (m2.FirstOrDefault() as MemberAccessExpressionSyntax).Expression as InvocationExpressionSyntax;
                        }
                        else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                             m2.LastOrDefault() is ArgumentListSyntax)
                        {
                            var methodName = m2.FirstOrDefault() as IdentifierNameSyntax;
                            var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                            if (methodName.ToString() == "Button" && (arguments.Arguments.Count != 1
                                    || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"New\""))
                            {
                                return base.VisitInvocationExpression(node);
                            }
                            m = m2.FirstOrDefault() as InvocationExpressionSyntax;
                        }
                    }
                    return newExpression
                                .WithLeadingTrivia(node.GetLeadingTrivia())
                                .WithTrailingTrivia(node.GetTrailingTrivia());
                }
                else if (s.ArgumentList.Arguments.Count() == 1 &&
                    (s.ArgumentList.Arguments.FirstOrDefault()
                    .DescendantNodes().OfType<ExpressionStatementSyntax>().Count() == 1 ||
                    s.ArgumentList.Arguments.FirstOrDefault()
                    .DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().Count() == 1) &&
                    s.ArgumentList.Arguments.FirstOrDefault()
                    .DescendantNodes().OfType<IdentifierNameSyntax>().Any(x => x.Identifier.ToString() == "Export") &&
                    s.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(x => x.Expression is IdentifierNameSyntax &&
                        ((IdentifierNameSyntax)x.Expression).Identifier.ToString() == "Button" &&
                        x.ArgumentList.Arguments.Count() == 1 &&
                        x.ArgumentList.Arguments.FirstOrDefault().ToString() == "\"Export\"").Count() == 1)
                {
                    InvocationExpressionSyntax exportInvocation = s.DescendantNodes().OfType<InvocationExpressionSyntax>()
                        .FirstOrDefault(x => x.Expression is MemberAccessExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)(x.Expression)).Name.ToString() == "Export");
                    var neededArguments = exportInvocation.ArgumentList;
                    var newNode = node.ReplaceNodes(s.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.Expression is MemberAccessExpressionSyntax &&
                            ((MemberAccessExpressionSyntax)nde1.Expression).Name is IdentifierNameSyntax &&
                            ((MemberAccessExpressionSyntax)nde1.Expression).Name.Identifier.ToString() == "OnClick" &&
                                nde1.ArgumentList.Arguments.Count == 1)
                            {
                                if (!(((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax))
                                    return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                                else return nde2.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                            }
                            else if (nde1.Expression is IdentifierNameSyntax &&
                               ((IdentifierNameSyntax)nde1.Expression).Identifier.ToString() == "Button" &&
                               nde1.ArgumentList.Arguments.Count() == 1 &&
                               nde1.ArgumentList.Arguments.FirstOrDefault().ToString() == "\"Export\"")
                            {
                                return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ParseExpression("button"),
                                    SyntaxFactory.IdentifierName("Export")), neededArguments)
                                        .WithLeadingTrivia(nde1.GetLeadingTrivia())
                                        .WithTrailingTrivia(nde1.GetTrailingTrivia());
                            }

                            return nde2;
                        });

                    return newNode;
                }
                else if (s.ArgumentList.Arguments.Count() == 1 &&
                    (s.ArgumentList.Arguments.FirstOrDefault()
                    .DescendantNodes().OfType<ExpressionStatementSyntax>().Count() == 1 ||
                    s.ArgumentList.Arguments.FirstOrDefault()
                    .DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().Count() == 1) &&
                    s.ArgumentList.Arguments.FirstOrDefault()
                    .DescendantNodes().OfType<IdentifierNameSyntax>().Any(x => x.Identifier.ToString() == "Reload") &&
                    s.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "Button" &&
                        x.ArgumentList.Arguments.Count() == 1 &&
                        x.ArgumentList.Arguments.FirstOrDefault().ToString() == "\"Search\"").Count() == 1)
                {
                    InvocationExpressionSyntax iconInvocation = s.DescendantNodes().OfType<InvocationExpressionSyntax>()
                            .FirstOrDefault(x => x.Expression is MemberAccessExpressionSyntax &&
                            ((MemberAccessExpressionSyntax)(x.Expression)).Name.ToString() == "Icon");
                    var neededArguments = iconInvocation.ArgumentList;
                    var newNode = node.ReplaceNodes(s.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.Expression is MemberAccessExpressionSyntax &&
                            ((MemberAccessExpressionSyntax)nde1.Expression).Name is IdentifierNameSyntax &&
                            ((MemberAccessExpressionSyntax)nde1.Expression).Name.Identifier.ToString() == "OnClick" &&
                                nde1.ArgumentList.Arguments.Count == 1)
                            {
                                if (!(((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax))
                                    return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                                else return nde2.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                            }
                            else if (nde1.Expression is MemberAccessExpressionSyntax &&
                            ((MemberAccessExpressionSyntax)nde1.Expression).Name is IdentifierNameSyntax &&
                            ((MemberAccessExpressionSyntax)nde1.Expression).Name.Identifier.ToString() == "Icon" &&
                                nde1.ArgumentList.Arguments.Count == 1)
                            {
                                if (!(((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax))
                                    return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                                else return nde2.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                            }
                            else if (nde1.Expression is MemberAccessExpressionSyntax &&
                            ((MemberAccessExpressionSyntax)nde1.Expression).Name is IdentifierNameSyntax &&
                            ((MemberAccessExpressionSyntax)nde1.Expression).Name.Identifier.ToString() == "NoText" &&
                                nde1.ArgumentList.Arguments.Count == 0)
                            {
                                if (!(((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax))
                                    return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                                else return nde2.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                            }
                            else if (nde1.Expression is MemberAccessExpressionSyntax &&
                               ((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() == "Button" &&
                               nde1.ArgumentList.Arguments.Count() == 1 &&
                               nde1.ArgumentList.Arguments.FirstOrDefault().ToString() == "\"Search\"")
                            {
                                SeparatedSyntaxList<ArgumentSyntax> args = new SeparatedSyntaxList<ArgumentSyntax>();
                                args = args.AddRange(neededArguments.Arguments.Where(x => x.Expression.ToString() != "FA.Search"));
                                return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ParseExpression("search"),
                                    SyntaxFactory.IdentifierName("Icon")), SyntaxFactory.ArgumentList(args))
                                            .WithLeadingTrivia(nde1.GetLeadingTrivia())
                                            .WithTrailingTrivia(nde1.GetTrailingTrivia());
                            }

                            return nde2;
                        });

                    return newNode;
                }
                else if (node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                         ((MemberAccessExpressionSyntax)x.Expression).Expression is IdentifierNameSyntax &&
                         ((IdentifierNameSyntax)(((MemberAccessExpressionSyntax)x.Expression).Expression)).Identifier.ToString() == "column").Count() == 1 &&
                         node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "HeaderText" &&
                        x.ArgumentList.Arguments.Count() == 1).Count() == 1 &&
                        node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "Link" &&
                        x.ArgumentList.Arguments.Count() == 1).Count() == 1)
                {
                    var linkMethod = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "Link" &&
                        x.ArgumentList.Arguments.Count() == 1).FirstOrDefault();
                    var headerTextMethod = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "HeaderText" &&
                        x.ArgumentList.Arguments.Count() == 1).FirstOrDefault();

                    if (linkMethod.ArgumentList.Arguments.FirstOrDefault().ToString()
                        .Contains("item." + headerTextMethod.ArgumentList.Arguments.FirstOrDefault()
                        .ToString().Trim('"')))
                    {
                        SeparatedSyntaxList<ArgumentSyntax> args = new SeparatedSyntaxList<ArgumentSyntax>();
                        args = args.Add(SyntaxFactory.Argument(
                                SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(
                                    SyntaxFactory.ParseToken("x")), null,
                                    SyntaxFactory.MemberAccessExpression
                                    (SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ParseExpression("x"),
                                    SyntaxFactory.IdentifierName(headerTextMethod.ArgumentList.Arguments.FirstOrDefault()
                        .ToString().Trim('"'))))));
                        var newNode = node.ReplaceNodes(node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>(),
                       (nde1, nde2) =>
                       {
                           if (nde1.Expression is MemberAccessExpressionSyntax &&
                            ((MemberAccessExpressionSyntax)nde1.Expression).Name is IdentifierNameSyntax &&
                            ((MemberAccessExpressionSyntax)nde1.Expression).Name.Identifier.ToString() == "HeaderText" &&
                                nde1.ArgumentList.Arguments.Count == 1)
                           {
                               if (!(((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax))
                                   return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                               else return nde2.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                           }
                           else if (nde1.Expression is MemberAccessExpressionSyntax &&
                             ((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() == "Link" &&
                             nde1.ArgumentList.Arguments.Count() == 1)
                           {
                               return SyntaxFactory.InvocationExpression(
                                     SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression
                                     , SyntaxFactory.ParseExpression("column"),
                                     SyntaxFactory.IdentifierName("Link")),
                                     SyntaxFactory.ArgumentList(args))
                                            .WithLeadingTrivia(nde1.GetLeadingTrivia())
                                            .WithTrailingTrivia(nde1.GetTrailingTrivia());
                           }

                           return nde2;
                       });
                        return newNode;
                    }
                }
                return base.VisitInvocationExpression(node);
            }
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.BaseList != null &&
                    node.BaseList.Types.Any(x => x.Type.IsKind(SyntaxKind.GenericName) &&
                     ((GenericNameSyntax)x.Type).Identifier.Text == "ListModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }
        }
        class FullSearchRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            Solution solution;
            RoslynDocument roslynDocument;

            bool shouldConvertToFullSearch = false;
            public FullSearchRewriter(SemanticModel semanticModel,
                Solution solution,
                RoslynDocument roslynDocument)
            {
                this.semanticModel = semanticModel;
                this.solution = solution;
                this.roslynDocument = roslynDocument;
            }

            public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
            {
                var declarationSymbol = semanticModel.GetSymbolInfo(node.Type).Symbol;
                var identifierName = semanticModel.GetDeclaredSymbol(node.Variables.FirstOrDefault());
                if (declarationSymbol.Name == "ModuleButton" &&
                    node.Variables.Count() == 1)
                {
                    if (node.Variables.FirstOrDefault().Initializer.Value is InvocationExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)((InvocationExpressionSyntax)node.Variables.FirstOrDefault()
                        .Initializer.Value).Expression).Name.ToString() == "Icon")
                    {
                        var invocation = node.Variables.FirstOrDefault().Initializer.Value
                            as InvocationExpressionSyntax;
                        var result = Task.Run(() => GetReferencedSymbolsAsync(identifierName)).Result;
                        if (result.Count() == 1 && invocation.ArgumentList.Arguments.Count() == 0)
                        {
                            SyntaxNode nextNode = node.SyntaxTree.GetRoot().FindNode(result.FirstOrDefault().Locations
                                .FirstOrDefault().Location.SourceSpan);
                            if (nextNode.Ancestors().OfType<InvocationExpressionSyntax>().Any())
                            {
                                var isNextInvocOk = CheckNextInvocationExpression(nextNode.Ancestors()
                                    .OfType<InvocationExpressionSyntax>().FirstOrDefault(),
                                    identifierName.ToString());
                                if (isNextInvocOk)
                                {
                                    shouldConvertToFullSearch = true;
                                    return SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ParseExpression("search"),
                                            SyntaxFactory.IdentifierName("FullWithIcon")),
                                        SyntaxFactory.ArgumentList())
                                        .WithLeadingTrivia(node.GetLeadingTrivia())
                                        .WithTrailingTrivia(node.GetTrailingTrivia());
                                }
                            }
                        }
                    }
                }
                return base.VisitVariableDeclaration(node);
            }
            public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
            {
                if (node.Declaration is VariableDeclarationSyntax)
                {
                    var t = VisitVariableDeclaration(node.Declaration);
                    if (t.IsKind(SyntaxKind.InvocationExpression))
                        return SyntaxFactory.ExpressionStatement(t as InvocationExpressionSyntax)
                            .WithTrailingTrivia(node.GetTrailingTrivia())
                            .WithLeadingTrivia(node.GetLeadingTrivia());
                }
                return base.VisitLocalDeclarationStatement(node);
            }

            public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                return base.VisitVariableDeclarator(node);
            }
            public bool CheckNextInvocationExpression(InvocationExpressionSyntax node
                , string identifierString)
            {
                var s = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "AfterControlAddon").FirstOrDefault();
                while (s.Expression != null)
                {
                    if (s.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        var memberExpression = s.Expression as MemberAccessExpressionSyntax;
                        if (memberExpression.Name.ToString() == "AfterControlAddon" &&
                            s.ArgumentList.Arguments.Count() == 1 &&
                            s.ArgumentList.Arguments.FirstOrDefault().Expression.ToString() ==
                            $"{identifierString}.Ref")
                        {
                            s = memberExpression.Expression as InvocationExpressionSyntax;
                            continue;
                        }
                    }
                    else if (s.Expression.IsKind(SyntaxKind.IdentifierName))
                    {
                        var identifierExpression = s.Expression as IdentifierNameSyntax;
                        if (identifierExpression.Identifier.ToString() == "Search" &&
                            s.ArgumentList.Arguments.Count() == 1 &&
                            s.ArgumentList.Arguments.FirstOrDefault().Expression.ToString() ==
                            "GeneralSearch.AllFields")
                        {
                            return true;
                        }
                    }
                    return false;
                }
                return false;
            }
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.BaseList != null &&
                    node.BaseList.Types.Any(x => x.Type.IsKind(SyntaxKind.GenericName) &&
                     ((GenericNameSyntax)x.Type).Identifier.Text == "ListModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }

            public async Task<IEnumerable<ReferencedSymbol>> GetReferencedSymbolsAsync(ISymbol symbol)
            {
                return await SymbolFinder.FindReferencesAsync(symbol,
                    solution, documents:
                    ImmutableHashSet.Create(this.roslynDocument));
            }
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var s = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "AfterControlAddon").FirstOrDefault();
                if (s != null && shouldConvertToFullSearch)
                {
                    shouldConvertToFullSearch = false;
                    return SyntaxFactory.ParseExpression("");
                }
                return base.VisitInvocationExpression(node);
            }

            public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
            {
                var result = base.VisitExpressionStatement(node);
                if (result.ToString() == ";")
                    return null;
                return result;
            }
        }
        class CustomFieldRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public CustomFieldRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var customColumnNode = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                    .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "CustomField").FirstOrDefault();

                if (customColumnNode != null)
                {
                    var newNode = node.ReplaceNodes(node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.ToString() == "CustomField()")
                            {
                                SeparatedSyntaxList<ArgumentSyntax> args = new SeparatedSyntaxList<ArgumentSyntax>();
                                var argsLabel =
                                node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                                    .Any(x =>
                                    x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                    ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "Label")
                                    ?
                                node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                                    .Where(x => x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                    x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                    ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() ==
                                    "Label").FirstOrDefault().ArgumentList : null;
                                var argsControlType =
                                node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                                    .Any(x =>
                                    x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                    ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "Control")
                                    ?
                                node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                                    .Where(x =>
                                    x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                    ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "Control").FirstOrDefault().ArgumentList
                                    : SyntaxFactory.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(
                                        SyntaxFactory.Argument(
                                        SyntaxFactory.ParseExpression("ControlType.Textbox"))));
                                var argsPropertyType =
                                     node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                                     .Any(x =>
                                     x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                     ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "PropertyType")
                                    ? node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()
                                        .Where(x => ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "PropertyType").FirstOrDefault().ArgumentList
                                        : null;

                                if (argsPropertyType != null && argsLabel == null)
                                {
                                    argsLabel = SyntaxFactory.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(
                                        SyntaxFactory.Argument(
                                        SyntaxFactory.ParseExpression("\"\""))));
                                }

                                if (argsLabel != null && argsLabel.Arguments.Count == 1)
                                    args = args.Add(argsLabel.Arguments.FirstOrDefault());

                                if (argsControlType.Arguments.Count == 1)
                                    args = args.Add(argsControlType.Arguments.FirstOrDefault());

                                return SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                         SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ParseExpression("field"),
                                    (argsPropertyType != null ?
                                    SyntaxFactory.GenericName("Custom")
                                        .WithTypeArgumentList(
                                        SyntaxFactory.TypeArgumentList(new SeparatedSyntaxList<TypeSyntax>()
                                        .Add(SyntaxFactory.ParseTypeName(argsPropertyType.Arguments.FirstOrDefault()
                                        .ToString().Trim('"'))))) as SimpleNameSyntax
                                    : SyntaxFactory.IdentifierName("Custom") as SimpleNameSyntax)),
                                    SyntaxFactory.ArgumentList(args))
                                .WithLeadingTrivia(nde1.GetLeadingTrivia());
                            }
                            else if (((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() ==
                               "Label" &&
                               nde1.ArgumentList.Arguments.Count == 1)
                            {
                                return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                            }
                            else if (((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() ==
                              "Control" &&
                              nde1.ArgumentList.Arguments.Count == 1)
                            {
                                return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                            }
                            else if (((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() ==
                               "PropertyType" &&
                               nde1.ArgumentList.Arguments.Count == 1)
                            {
                                return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                            }

                            return nde2;
                        });
                    return newNode;
                }
                return base.VisitInvocationExpression(node);
            }
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.BaseList != null &&
                    node.BaseList.Types.Any(x => x.Type.IsKind(SyntaxKind.GenericName) &&
                     ((GenericNameSyntax)x.Type).Identifier.Text == "FormModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }
        }
    }
}
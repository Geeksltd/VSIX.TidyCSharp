using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class SortClassMembers : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
        {
            var modifiedSourceNode = SortClassMembersHelper(initialSourceNode);

            if (IsReportOnlyMode &&
                   !IsEquivalentToUNModified(modifiedSourceNode))
            {
                CollectMessages(new ChangesReport(initialSourceNode)
                {
                    LineNumber = 1,
                    Column = 1,
                    Message = "Sort Class Members",
                    Generator = nameof(SortClassMembers)
                });

                return initialSourceNode;
            }

            return modifiedSourceNode;
        }

        public static SyntaxNode SortClassMembersHelper(SyntaxNode initialSource)
        {
            var classes =
                initialSource
                .DescendantNodes()
                .Where(x => x is ClassDeclarationSyntax)
                .OfType<ClassDeclarationSyntax>();

            var newClassesDic = new Dictionary<ClassDeclarationSyntax, ClassDeclarationSyntax>();

            foreach (var classNode in classes)
            {
                var newClassNode = SortClassMemebersHelper(classNode);
                newClassesDic.Add(classNode, newClassNode);
            }

            initialSource =
                initialSource
                    .ReplaceNodes
                    (
                        classes,
                        (oldNode1, oldNode2) =>
                        {
                            var newClass = newClassesDic[oldNode1];
                            if (oldNode1 != newClass) return newClass;
                            return oldNode1;
                        }
                    );

            return initialSource;
        }

        public static ClassDeclarationSyntax SortClassMemebersHelper(ClassDeclarationSyntax classNode)
        {
            var methods = classNode.Members.Where(x => x is MethodDeclarationSyntax).ToList();
            var firstMethod = methods.FirstOrDefault();
            if (firstMethod == null) return classNode;

            var methodAnnotation = new SyntaxAnnotation();
            var annotatedClassNode = classNode.ReplaceNode(firstMethod, firstMethod.WithAdditionalAnnotations(methodAnnotation));

            var constructors = annotatedClassNode.Members.Where(x => x is ConstructorDeclarationSyntax).ToList();
            if (constructors.Any() == false) return classNode;

            var constructorsToMoveList = new List<SyntaxNode>();

            foreach (var constructorItem in constructors)
            {
                if (firstMethod.SpanStart < constructorItem.SpanStart)
                {
                    constructorsToMoveList.Add(constructorItem);
                }
            }

            if (constructorsToMoveList.Any())
            {
                annotatedClassNode = annotatedClassNode.RemoveNodes(constructorsToMoveList, SyntaxRemoveOptions.KeepNoTrivia);
                var annotatedMethod = annotatedClassNode.GetAnnotatedNodes(methodAnnotation).FirstOrDefault();
                annotatedClassNode = annotatedClassNode.InsertNodesBefore(annotatedMethod, constructorsToMoveList);

                return annotatedClassNode;
            }

            return classNode;
        }
    }
}
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class SimplifyClassFieldDeclarations : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        const int MAX_FIELD_DECLARATION_LENGTH = 80;

        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return SimplifyClassFieldDeclarationsHelper(initialSourceNode);
        }

        public static SyntaxNode SimplifyClassFieldDeclarationsHelper(SyntaxNode initialSourceNode)
        {
            initialSourceNode = new Rewriter().Visit(initialSourceNode);
            return initialSourceNode;
        }

        class Rewriter : CSharpSyntaxRewriter
        {
            public Rewriter() : base()
            {
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                node = Apply(node) as ClassDeclarationSyntax;

                return node;
            }

            public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                if (node.Initializer == null) return base.VisitVariableDeclarator(node);
                if (node.Parent is VariableDeclarationSyntax == false) return base.VisitVariableDeclarator(node);
                if (node.Parent.Parent is FieldDeclarationSyntax == false) return base.VisitVariableDeclarator(node);
                if ((node.Parent.Parent as FieldDeclarationSyntax).Modifiers.Any(x => x.ValueText == "const")) return base.VisitVariableDeclarator(node);

                var value = node.Initializer.Value;

                if (value is LiteralExpressionSyntax)
                {
                    var variableTypeNode = GetSystemTypeOfTypeNode((node.Parent as VariableDeclarationSyntax));
                    var valueObj = (value as LiteralExpressionSyntax).Token.Value;

                    if (TypesMapItem.GetAllPredefinedTypesDic().ContainsKey(variableTypeNode))
                    {
                        var typeItem = TypesMapItem.GetAllPredefinedTypesDic()[variableTypeNode];
                        if ((typeItem.DefaultValue == null && valueObj != null) || (typeItem.DefaultValue != null && !typeItem.DefaultValue.Equals(valueObj))) return base.VisitVariableDeclarator(node);
                    }
                    else
                    {
                        if (valueObj != null) return base.VisitVariableDeclarator(node);
                    }

                    node = node.WithInitializer(null).WithoutTrailingTrivia();
                }
                //else if (value is DefaultExpressionSyntax)
                //{
                //    node = node.WithInitializer(null).WithoutTrailingTrivia();
                //}
                //else if (value is ObjectCreationExpressionSyntax)
                //{
                //    if (variableTypeNode.IsKind(SyntaxKind.PredefinedType))
                //    {
                //        node = node.WithInitializer(null).WithoutTrailingTrivia();
                //    }
                //}

                return base.VisitVariableDeclarator(node);
            }


            SyntaxTrivia spaceTrivia = SyntaxFactory.Whitespace(" ");
            SyntaxNode Apply(ClassDeclarationSyntax classDescriptionNode)
            {
                var newDeclarationDic = new Dictionary<NewFieldDeclarationDicKey, NewFieldDeclarationDicItem>();

                var fieldDeclarations =
                    classDescriptionNode
                        .Members
                        .OfType<FieldDeclarationSyntax>()
                        .Where(fd => fd.AttributeLists.Any() == false)
                        .Where(fd => fd.HasStructuredTrivia == false)
                        .Where(fd => fd.DescendantTrivia().Any(t=> t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia))== false)
                        .Where(fd => fd.Declaration.Variables.All(x => x.Initializer == null || x.Initializer.Value is LiteralExpressionSyntax))
                        .ToList();

                foreach (var fieldDeclarationItem in fieldDeclarations)
                {
                    var variableType = GetSystemTypeOfTypeNode(fieldDeclarationItem.Declaration);

                    var key = GetKey(fieldDeclarationItem);

                    if (newDeclarationDic.ContainsKey(key) == false)
                    {
                        newDeclarationDic
                            .Add
                            (
                                key,
                                new NewFieldDeclarationDicItem
                                {
                                    VariablesWithoutInitializer = new List<VariableDeclaratorSyntax>(),
                                    VariablesWithInitializer = new List<VariableDeclaratorSyntax>(),
                                    OldFieldDeclarations = new List<FieldDeclarationSyntax>()
                                }
                            );
                    }

                    var currentItem = newDeclarationDic[key];

                    currentItem.OldFieldDeclarations.Add(fieldDeclarationItem);

                    var newDeclaration = VisitFieldDeclaration(fieldDeclarationItem) as FieldDeclarationSyntax;

                    currentItem.VariablesWithoutInitializer.AddRange(newDeclaration.Declaration.Variables.Where(v => v.Initializer == null));
                    currentItem.VariablesWithInitializer.AddRange(newDeclaration.Declaration.Variables.Where(v => v.Initializer != null));
                }

                var newDeclarationDicAllItems = newDeclarationDic.ToList();

                newDeclarationDic.Clear();

                foreach (var newDelarationItem in newDeclarationDicAllItems)
                {
                    var finalList = newDelarationItem.Value.VariablesWithoutInitializer.Select(x => x.WithoutTrailingTrivia().WithLeadingTrivia(spaceTrivia)).ToList();
                    finalList.AddRange(newDelarationItem.Value.VariablesWithInitializer.Select(x => x.WithoutTrailingTrivia().WithLeadingTrivia(spaceTrivia)));

                    finalList[0] = finalList[0].WithoutLeadingTrivia();

                    newDelarationItem.Value.NewFieldDeclaration =
                        newDelarationItem.Value.FirstOldFieldDeclarations
                        .WithDeclaration(
                            newDelarationItem.Value.FirstOldFieldDeclarations
                                .Declaration
                                .WithVariables(SyntaxFactory.SeparatedList(finalList))
                        );

                    if (newDelarationItem.Value.NewFieldDeclaration.Span.Length <= MAX_FIELD_DECLARATION_LENGTH)
                    {
                        newDeclarationDic.Add(newDelarationItem.Key, newDelarationItem.Value);
                    }
                    else
                    {
                        foreach (var item in newDelarationItem.Value.OldFieldDeclarations)
                        {
                            fieldDeclarations.Remove(item);
                        }
                    }
                }

                var replaceList = newDeclarationDic.Select(x => x.Value.FirstOldFieldDeclarations).ToList();

                classDescriptionNode =
                    classDescriptionNode
                    .ReplaceNodes
                    (
                         fieldDeclarations,
                         (node1, node2) =>
                         {
                             if (replaceList.Contains(node1))
                             {
                                 var dicItem = newDeclarationDic[GetKey(node1 as FieldDeclarationSyntax)];

                                 return
                                    dicItem
                                    .NewFieldDeclaration
                                    .WithLeadingTrivia(dicItem.FirstOldFieldDeclarations.GetLeadingTrivia())
                                    .WithTrailingTrivia(dicItem.FirstOldFieldDeclarations.GetTrailingTrivia());
                             }
                             return null;
                         }
                    );

                return classDescriptionNode;
            }

            NewFieldDeclarationDicKey GetKey(FieldDeclarationSyntax fieldDeclarationItem)
            {
                var header = new NewFieldDeclarationDicKey
                {
                    TypeName = GetSystemTypeOfTypeNode(fieldDeclarationItem.Declaration),
                };

                if (fieldDeclarationItem.Modifiers.Any())
                {
                    header.Modifiers = fieldDeclarationItem.Modifiers.Select(x => x.ValueText).ToArray();
                }
                return header;
            }

            string GetSystemTypeOfTypeNode(VariableDeclarationSyntax d)
            {
                if (d.Type is PredefinedTypeSyntax)
                    return TypesMapItem.GetAllPredefinedTypesDic()[(d.Type as PredefinedTypeSyntax).Keyword.ValueText].BuiltInName.Trim();

                return (d.Type.ToFullString().Trim());
            }


            struct NewFieldDeclarationDicKey : IEquatable<NewFieldDeclarationDicKey>
            {
                public string TypeName { get; set; }
                public string[] Modifiers { get; set; }

                public bool Equals(NewFieldDeclarationDicKey other)
                {
                    return this == other;
                }

                public static bool operator ==(NewFieldDeclarationDicKey left, NewFieldDeclarationDicKey right)
                {
                    if (string.Compare(left.TypeName, right.TypeName) != 0) return false;
                    if (left.Modifiers == null && right.Modifiers == null) return true;
                    if (left.Modifiers == null || right.Modifiers == null) return false;
                    if (left.Modifiers.Length != right.Modifiers.Length) return false;
                    foreach (var item in left.Modifiers)
                    {
                        if (right.Modifiers.Any(m => string.Compare(m, item) == 0) == false) return false;
                    }

                    return true;
                }
                public static bool operator !=(NewFieldDeclarationDicKey left, NewFieldDeclarationDicKey right)
                {
                    return !(left == right);
                }

            }
            class NewFieldDeclarationDicItem
            {
                public List<VariableDeclaratorSyntax> VariablesWithoutInitializer { get; set; }
                public List<VariableDeclaratorSyntax> VariablesWithInitializer { get; set; }
                public FieldDeclarationSyntax FirstOldFieldDeclarations
                {
                    get
                    {
                        return OldFieldDeclarations.First();
                    }
                }
                public List<FieldDeclarationSyntax> OldFieldDeclarations { get; set; }
                public FieldDeclarationSyntax NewFieldDeclaration { get; set; }
            }
        }
    }
}
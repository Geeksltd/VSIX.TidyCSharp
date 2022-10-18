﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper;

internal class FieldRenamer : Renamer
{
    public FieldRenamer(Document document) : base(document)
    {
    }

    protected override IEnumerable<SyntaxToken> GetItemsToRename(SyntaxNode currentNode)
    {
        var output = new List<VariableDeclaratorSyntax>();

        var selectedFields =
            (currentNode as ClassDeclarationSyntax)
            .Members.OfType<FieldDeclarationSyntax>()
            .Where(
                x =>
                    IsPrivate(x) &&
                    x.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)) == false
            );

        foreach (var item in selectedFields)
            output.AddRange(item.Declaration.Variables);

        return output.Select(x => x.Identifier);
    }

    protected override string[] GetNewName(string currentName)
    {
        if (currentName.StartsWith("_"))
        {
            currentName = currentName.TrimStart('_');

            if (char.IsLetter(currentName[0]))
            {
                return new[] { GetCamelCased(currentName), GetPascalCased(currentName) };
            }
        }

        return null;
    }
}
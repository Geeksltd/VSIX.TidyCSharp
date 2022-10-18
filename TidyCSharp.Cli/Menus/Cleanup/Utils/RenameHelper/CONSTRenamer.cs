using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper;

internal class ConstRenamer : Renamer
{
    public ConstRenamer(Document document) : base(document)
    {
    }

    protected override IEnumerable<SyntaxToken> GetItemsToRename(SyntaxNode currentNode)
    {
        var output = new List<VariableDeclaratorSyntax>();

        {
            var selectedFields =
                (currentNode as ClassDeclarationSyntax)
                .Members.OfType<FieldDeclarationSyntax>()
                .Where(
                    x =>
                        x.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)) &&
                        IsPrivate(x)
                );

            foreach (var item in selectedFields)
                output.AddRange(item.Declaration.Variables);
        }

        {
            var selectedFields =
                (currentNode as ClassDeclarationSyntax)
                .DescendantNodes().OfType<LocalDeclarationStatementSyntax>()
                .Where(
                    x =>
                        x.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword))
                );

            foreach (var item in selectedFields)
                output.AddRange(item.Declaration.Variables);
        }

        return output.Select(x => x.Identifier);
    }

    protected override string[] GetNewName(string currentName)
    {
        const char underline = '_';

        var newNameBuilder = new StringBuilder();
        var lastCharIsLowwer = false;

        foreach (var c in currentName)
        {
            if (char.IsUpper(c))
            {
                if (lastCharIsLowwer)
                {
                    newNameBuilder.Append(underline);
                }

                lastCharIsLowwer = false;
            }
            else if (c != underline) lastCharIsLowwer = true;

            newNameBuilder.Append(c);
        }

        if (string.Compare(currentName, currentName.ToUpper(), false) == 0) return null;

        return new[] { newNameBuilder.ToString().ToUpper() };
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    class VariableRenamer : Renamer
    {
        public VariableRenamer(Document document) : base(document)
        {
        }

        protected override IEnumerable<SyntaxToken> GetItemsToRename(SyntaxNode currentNode)
        {
            return
                currentNode
                .DescendantNodes()
                .OfType<VariableDeclaratorSyntax>()
                .Where(v => (v?.Parent?.Parent as LocalDeclarationStatementSyntax)?.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ConstKeyword)) == false)
                .Select(x => x.Identifier);
        }

        protected override string[] GetNewName(string currentName)
        {
            if (currentName.StartsWith("_"))
            {
                currentName = currentName.TrimStart('_');
                if (currentName.Length == 0) return null;
                if (char.IsLetter(currentName[0]))
                {
                    return new[] { GetCamelCased(currentName) };
                }
            }
            else if (char.IsLetter(currentName[0]) && char.IsUpper(currentName[0]))
            {
                return new[] { GetCamelCased(currentName) };
            }

            return null;
        }
    }
}
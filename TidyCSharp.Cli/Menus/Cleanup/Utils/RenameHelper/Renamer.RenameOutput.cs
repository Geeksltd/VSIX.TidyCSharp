using Microsoft.CodeAnalysis;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper;

internal abstract partial class Renamer
{
    public class RenameResult
    {
        public SyntaxNode Node { get; set; }
        public Solution Solution { get; set; }
        public Document Document { get; set; }
    }
}
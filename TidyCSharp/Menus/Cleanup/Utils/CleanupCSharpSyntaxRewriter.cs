using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Microsoft.CodeAnalysis.CSharp;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils
{
    public abstract class CleanupCSharpSyntaxRewriter : CSharpSyntaxRewriter
    {
        protected ICleanupOption Options { get; private set; }

        protected bool CheckOption(int? o)
        {
            return Options.Should(o);
        }

        public CleanupCSharpSyntaxRewriter(ICleanupOption options)
        {
            Options = options;
        }
    }
}

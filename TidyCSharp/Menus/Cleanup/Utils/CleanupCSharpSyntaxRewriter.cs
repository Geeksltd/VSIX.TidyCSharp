using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Microsoft.CodeAnalysis.CSharp;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils
{
    public abstract class CleanupCSharpSyntaxRewriter : CSharpSyntaxRewriter
    {
        ICleanupOption OptionsObj { get; set; }

        protected bool CheckOption(int? o)
        {
            return OptionsObj.CheckOption(o);
        }

        public CleanupCSharpSyntaxRewriter(ICleanupOption options)
        {
            OptionsObj = options;
        }
    }
}

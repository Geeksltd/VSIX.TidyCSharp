using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.SimplifyClassFieldDeclaration
{
    public class Options : OptionsBase, ICleanupOption
    {
        public const int MaxFieldDeclarationLength = 80;

        public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

        public override CodeCleanerType GetCodeCleanerType()
        {
            return CodeCleanerType.SimplifyClassFieldDeclarations;
        }
    }
}
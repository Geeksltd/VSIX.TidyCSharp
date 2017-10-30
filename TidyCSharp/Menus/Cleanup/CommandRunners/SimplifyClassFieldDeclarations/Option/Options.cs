using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.SimplifyClassFieldDeclaration
{
    public class Options : OptionsBase, ICleanupOption
    {
        public const int MAX_FIELD_DECLARATION_LENGTH = 80;

        public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

        public override CodeCleanerType GetCodeCleanerType()
        {
            return CodeCleanerType.SimplifyClassFieldDeclarations;
        }
    }

}

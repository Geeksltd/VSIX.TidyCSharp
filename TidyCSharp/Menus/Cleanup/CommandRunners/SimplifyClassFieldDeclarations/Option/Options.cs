using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.SimplifyClassFieldDeclaration
{
    public class Options : OptionsBase, ICleanupOption
    {
        public const int Max_Field_Declaration_Length = 80;

        public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

        public override CodeCleanerType GetCodeCleanerType()
        {
            return CodeCleanerType.SimplifyClassFieldDeclarations;
        }
    }
}
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.RemoveExtraThisKeyword
{
    public class Options : OptionsBase, ICleanupOption
    {
        public const int MaxFieldDeclarationLength = 80;

        public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

        public override CodeCleanerType GetCodeCleanerType()
        {
            return CodeCleanerType.RemoveExtraThisQualification;
        }
    }
}
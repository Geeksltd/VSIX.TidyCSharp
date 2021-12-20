using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhiteSpace
{
    public class Options : OptionsBase, ICleanupOption
    {
        public const int Block_Single_Statement_Max_Length = 80;

        public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

        public override CodeCleanerType GetCodeCleanerType() => CodeCleanerType.NormalizeWhiteSpaces;
    }
}
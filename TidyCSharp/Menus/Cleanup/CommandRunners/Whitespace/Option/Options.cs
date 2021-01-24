using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace
{
    public class Options : OptionsBase, ICleanupOption
    {
        public const int BLOCK_SINGLE_STATEMENT_MAX_LENGTH = 80;

        public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

        public override CodeCleanerType GetCodeCleanerType() => CodeCleanerType.NormalizeWhiteSpaces;
    }
}
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.CamelCasedClassFields
{
    public class Options : OptionsBase, ICleanupOption
    {
        public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

        public override CodeCleanerType GetCodeCleanerType() => CodeCleanerType.CamelCasedFields;
    }
}
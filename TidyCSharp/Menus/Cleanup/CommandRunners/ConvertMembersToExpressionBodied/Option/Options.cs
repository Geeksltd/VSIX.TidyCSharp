using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.MembersToExpressionBodied
{
    public class Options : OptionsBase, ICleanupOption
    {
        public const int Max_Expression_Bodied_Member_Length = 90;

        public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

        public override CodeCleanerType GetCodeCleanerType()
        {
            return CodeCleanerType.ConvertMembersToExpressionBodied;
        }
    }
}
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.MembersToExpressionBodied
{
    public class Options : OptionsBase, ICleanupOption
    {
        public const int MAX_EXPRESSION_BODIED_MEMBER_LENGTH = 90;

        public CleanupTypes? CleanupItems => (CleanupTypes?)CleanupItemsInteger;

        protected override CodeCleanerType GetCodeCleanerType()
        {
            return CodeCleanerType.ConvertMembersToExpressionBodied;
        }
    }

}

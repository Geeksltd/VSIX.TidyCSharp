using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;

namespace Geeks.VSIX.TidyCSharp.Cleanup.Infra
{
    public interface ICleanupOption
    {
        void Accept(IMainCleanup mainCleanup);
    }

}

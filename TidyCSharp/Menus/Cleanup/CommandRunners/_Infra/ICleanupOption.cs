using Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.Infra
{
    public interface ICleanupOption
    {
        int? CleanupItemsInteger { get; }

        void Accept(IMainCleanup mainCleanup);
    }

}

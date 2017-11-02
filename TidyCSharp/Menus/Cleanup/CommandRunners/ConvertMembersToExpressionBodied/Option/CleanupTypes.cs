using System;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Cleanup.MembersToExpressionBodied
{
    [Flags]
    public enum CleanupTypes
    {
        [CleanupItem(Title = "Convert Methods => Method with only a single return statement and lenth less than 100 chars(length of its signature and its single statement)")]
        Convert_Methods = 0x01,

        [CleanupItem(Title = "Convert ReadOnly Property =>  ReadOnly Property with only a single return statement and lenth less than 100 chars(length of its Defenition and its single statement)")]
        Convert_ReadOnly_Property = 0x02,
    }
}

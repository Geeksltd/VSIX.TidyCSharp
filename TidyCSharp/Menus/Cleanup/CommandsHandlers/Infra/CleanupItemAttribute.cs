using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.Infra
{
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class CleanupItemAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public string Title { get; set; } = null;
        public int FirstOrder { get; set; } = int.MaxValue;
        public int? Order
        {
            get
            {
                return FirstOrder == int.MaxValue ? (int?)null : FirstOrder;
            }
        }
        public Type SubItemType { get; set; }

        // This is a positional argument
        public CleanupItemAttribute()
        {
        }
    }

}

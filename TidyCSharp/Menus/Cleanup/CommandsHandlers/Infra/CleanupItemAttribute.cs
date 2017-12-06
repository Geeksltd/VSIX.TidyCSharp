using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup.Infra
{
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class CleanupItemAttribute : Attribute
    {
        public string Title { get; set; } = null;
        public int FirstOrder { get; set; } = int.MaxValue;
        public bool SelectedByDefault { get; set; } = true;

        public int? Order
        {
            get
            {
                return FirstOrder == int.MaxValue ? (int?)null : FirstOrder;
            }
        }
        public Type SubItemType { get; set; }

        public CleanupItemAttribute()
        {
        }
    }

}

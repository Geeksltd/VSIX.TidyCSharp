using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{

    public partial class CleanupItemUserControl : UserControl, IMainCleanup
    {
        private const int HEIGHT_OF_CHECKBOX = 15;

        public CleanupItemUserControl()
        {
            InitializeComponent();
            checkedListBoxcheckboxCleanupSubItems.BorderStyle = BorderStyle.None;
            checkboxCleanupItem.CheckedChanged += CheckboxCleanupItem_CheckedChanged;
            checkedListBoxcheckboxCleanupSubItems.Enabled = checkboxCleanupItem.Checked;
        }

        private void CheckboxCleanupItem_CheckedChanged(object sender, EventArgs e)
        {
            checkedListBoxcheckboxCleanupSubItems.Enabled = checkboxCleanupItem.Checked;
        }

        public CodeCleanerType MainCleanupItemType { get; set; }

        public void Init(CodeCleanerType mainCleanupItemType)
        {
            MainCleanupItemType = mainCleanupItemType;

            var values = Get(typeof(CodeCleanerType), mainCleanupItemType);

            checkboxCleanupItem.Text =
                values.Value != null ?
                    values.Value.Title ?? mainCleanupItemType.ToString() :
                    mainCleanupItemType.ToString();

            if (values.Value.SubItemType != null)
            {
                CreateControls(values.Value.SubItemType, checkBoxItem => NewCheckboxItem(checkBoxItem));
                this.BorderStyle = BorderStyle.FixedSingle;

                var h = (checkedListBoxcheckboxCleanupSubItems.Items.Count * (HEIGHT_OF_CHECKBOX + 4.5));
                this.Height += (int)h;
                return;
            }

            this.Height = HEIGHT_OF_CHECKBOX + 10;// checkboxCleanupItem.Height;

            //CreateControls<TSubItemType>(checkBoxItem => NewCheckboxItem(checkBoxItem));
        }

        public CheckBoxItem[] GetSubItems()
        {
            if (checkboxCleanupItem.Checked == false) return new CheckBoxItem[0];

            var selectedTypes = checkedListBoxcheckboxCleanupSubItems.CheckedItems?.Cast<CheckBoxItem>();
            return selectedTypes.OrderBy(x => x.Order).ToArray();
        }

        public static void CreateControls(Type subItemType, Action<CheckBoxItem> action, bool sortDESC = false)
        {
            try
            {
                var items = Get(subItemType);

                if (!sortDESC) items = items.OrderBy(x => x.Value.Order.GetValueOrDefault(x.Key));
                else items = items.OrderByDescending(x => x.Value.Order.GetValueOrDefault(x.Key));

                foreach (var item in items)
                {
                    var tempCheckBoxItem = new CheckBoxItem() { CleanerType = item.Key };

                    tempCheckBoxItem.Order = item.Value.Order.GetValueOrDefault(item.Key);

                    if (!string.IsNullOrEmpty(item.Value.Title))
                    {
                        tempCheckBoxItem.Name = item.Value.Title;
                    }
                    else
                    {
                        tempCheckBoxItem.Name = Enum.GetName(subItemType, item.Key).ToString();
                    }
                    action(tempCheckBoxItem);
                }
            }
            catch (System.Exception exp)
            {

            }
        }

        public static IEnumerable<KeyValuePair<int, CleanupItemAttribute>> Get(Type subItemType)
        {
            foreach (var item in Enum.GetValues(subItemType))
            {
                yield return Get(subItemType, item);
                //var memInfo = type.GetMember(item.ToString());
                //var attributes = memInfo[0].GetCustomAttributes(typeof(CleanupItemAttribute), false);
                ////var description = ((CleanupItemAttribute)attributes[0]).Title;
                //yield return new KeyValuePair<int, CleanupItemAttribute>((int)item, (CleanupItemAttribute)attributes.FirstOrDefault());
            }
        }

        public static KeyValuePair<int, CleanupItemAttribute> Get(Type subItemType, object item)
        {
            var memInfo = subItemType.GetMember(item.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(CleanupItemAttribute), false);
            //var description = ((CleanupItemAttribute)attributes[0]).Title;
            return new KeyValuePair<int, CleanupItemAttribute>((int)item,
                (CleanupItemAttribute)attributes.FirstOrDefault()
                ??
                new CleanupItemAttribute()
                );
        }

        //public static void CreateControls<T>(Action<CheckBoxItem> action)
        //    where T : struct
        //{
        //    foreach (var item in Get<T>().OrderBy(x => x.Value.Order))
        //    {
        //        var tempCheckBoxItem = new CheckBoxItem() { CleanerType = item.Key };

        //        if (item.Value != null)
        //        {
        //            tempCheckBoxItem.Order = item.Value.Order;

        //            if (item.Value.Title != null)
        //            {
        //                tempCheckBoxItem.Name = item.Value.Title;
        //            }
        //            else
        //            {
        //                tempCheckBoxItem.Name = Enum.GetName(typeof(T), item.Key).ToString();
        //            }
        //        }
        //        else
        //        {
        //            tempCheckBoxItem.Name = null;
        //            tempCheckBoxItem.Order = int.MaxValue;
        //        }
        //    }
        //}

        //public static IEnumerable<KeyValuePair<int, CleanupItemAttribute>> Get<T>()
        //    where T : struct
        //{
        //    foreach (var item in Enum.GetValues(typeof(T)))
        //    {
        //        yield return Get<T>(item);
        //        //var memInfo = type.GetMember(item.ToString());
        //        //var attributes = memInfo[0].GetCustomAttributes(typeof(CleanupItemAttribute), false);
        //        ////var description = ((CleanupItemAttribute)attributes[0]).Title;
        //        //yield return new KeyValuePair<int, CleanupItemAttribute>((int)item, (CleanupItemAttribute)attributes.FirstOrDefault());
        //    }
        //}

        //public static KeyValuePair<int, CleanupItemAttribute> Get<T>(object item)
        //    where T : struct
        //{
        //    var memInfo = typeof(T).GetMember(item.ToString());
        //    var attributes = memInfo[0].GetCustomAttributes(typeof(CleanupItemAttribute), false);
        //    //var description = ((CleanupItemAttribute)attributes[0]).Title;
        //    return new KeyValuePair<int, CleanupItemAttribute>((int)item, (CleanupItemAttribute)attributes.FirstOrDefault());
        //}

        private void NewCheckboxItem(CheckBoxItem checkBoxItem)
        {
            checkedListBoxcheckboxCleanupSubItems.Items.Add(checkBoxItem);
            ////this.Height += HEIGHT_OF_CHECKBOX + 25;
        }

        public class CheckBoxItem
        {
            public string Name { get; set; }
            public int CleanerType { get; set; }
            public int Order { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}

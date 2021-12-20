using Geeks.VSIX.TidyCSharp.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    public partial class CleanupItemUserControl : UserControl, IMainCleanup
    {
        public CleanupItemUserControl()
        {
            InitializeComponent();
            customCheckListBox1.BorderStyle = BorderStyle.None;
            checkboxCleanupItem.CheckedChanged += CheckboxCleanupItem_CheckedChanged;
            customCheckListBox1.Enabled = checkboxCleanupItem.Checked;
        }

        CleanupItemAttribute MainCleannerItemAttribute { get; set; }

        public void Init(CodeCleanerType mainCleanupItemType)
        {
            MainCleanupItemType = mainCleanupItemType;

            MainCleannerItemAttribute = ExtractCleannerItemAttribute(typeof(CodeCleanerType), mainCleanupItemType).Value;

            SetUIProps();
        }

        void SetUIProps()
        {
            checkboxCleanupItem.Text =
                MainCleannerItemAttribute != null ?
                    MainCleannerItemAttribute.Title ?? MainCleanupItemType.ToString() :
                    MainCleanupItemType.ToString();

            if (MainCleannerItemAttribute.SubitemType != null)
            {
                HasSubitems = true;

                CreateControls(MainCleannerItemAttribute.SubitemType, checkBoxItem => AddNewCheckboxItem(checkBoxItem));
                BorderStyle = BorderStyle.FixedSingle;

                for (int i = 0; i < customCheckListBox1.Controls.Count; i++)
                    Height += customCheckListBox1.Controls[i].Height;

                return;
            }

            Margin = new Padding(0);
            Padding = new Padding(0);
        }

        #region IMainCleanup

        public CodeCleanerType MainCleanupItemType { get; private set; }
        public bool HasSubitems { get; private set; } = false;

        public void SetMainItemCheckState(bool isSelected) => checkboxCleanupItem.Checked = isSelected;

        public void SetItemsCheckState(int value, bool checkedState)
        {
            customCheckListBox1.SetItemsChecked(value, checkedState);
        }

        public void ResetItemsCheckState()
        {
            checkboxCleanupItem.Checked = MainCleannerItemAttribute.SelectedByDefault;

            customCheckListBox1.ResetItemsCheckState();
        }

        public bool IsMainObjectSelected
        {
            get
            {
                if (checkboxCleanupItem.Checked == false) return false;
                if (HasSubitems && GetSelectedSubitems().Any() == false) return false;
                return true;
            }
        }
        public CleanerItemUIInfo[] GetSelectedSubitems()
        {
            var selectedTypes = customCheckListBox1.GetCheckedItems();
            return selectedTypes.OrderBy(x => x.Order).ToArray();
        }

        #endregion

        public static void CreateControls(Type itemsEnumType, Action<CleanerItemUIInfo> action, bool sortDesc = false)
        {
            try
            {
                var cleannerItemsAttributes = ExtractCleannerItemsAttributes(itemsEnumType);

                if (!sortDesc) cleannerItemsAttributes = cleannerItemsAttributes.OrderBy(x => x.Value.Order.GetValueOrDefault(x.Key));
                else cleannerItemsAttributes = cleannerItemsAttributes.OrderByDescending(x => x.Value.Order.GetValueOrDefault(x.Key));

                foreach (var item in cleannerItemsAttributes)
                {
                    var tempCheckBoxItemUiInfo =
                        new CleanerItemUIInfo
                        {
                            CleanerType = item.Key,
                            ShouldBeSelectedByDefault = item.Value.SelectedByDefault,
                            Order = item.Value.Order.GetValueOrDefault(item.Key)
                        };

                    if (!string.IsNullOrEmpty(item.Value.Title))
                    {
                        tempCheckBoxItemUiInfo.Name = item.Value.Title;
                    }
                    else
                    {
                        tempCheckBoxItemUiInfo.Name = Enum.GetName(itemsEnumType, item.Key).ToString();
                    }

                    action(tempCheckBoxItemUiInfo);
                }
            }
            catch
            {
            }
        }

        static IEnumerable<KeyValuePair<int, CleanupItemAttribute>> ExtractCleannerItemsAttributes(Type subItemType)
        {
            foreach (var item in Enum.GetValues(subItemType))
                yield return ExtractCleannerItemAttribute(subItemType, item);
        }

        static KeyValuePair<int, CleanupItemAttribute> ExtractCleannerItemAttribute(Type subItemType, object item)
        {
            var memInfo = subItemType.GetMember(item.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(CleanupItemAttribute), false);

            return new KeyValuePair<int, CleanupItemAttribute>((int)item,
                (CleanupItemAttribute)attributes.FirstOrDefault()
                ??
                new CleanupItemAttribute()
                );
        }

        void AddNewCheckboxItem(CleanerItemUIInfo checkBoxItem)
        {
            customCheckListBox1.AddItem(checkBoxItem);
        }

        void CheckboxCleanupItem_CheckedChanged(object sender, EventArgs e)
        {
            customCheckListBox1.Enabled = checkboxCleanupItem.Checked;
        }
    }
}
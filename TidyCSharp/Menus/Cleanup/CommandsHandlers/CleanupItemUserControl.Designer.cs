namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers
{
    partial class CleanupItemUserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkboxCleanupItem = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkedListBoxcheckboxCleanupSubItems = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // checkboxCleanupItem
            // 
            this.checkboxCleanupItem.AutoSize = true;
            this.checkboxCleanupItem.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkboxCleanupItem.Location = new System.Drawing.Point(3, 0);
            this.checkboxCleanupItem.Name = "checkboxCleanupItem";
            this.checkboxCleanupItem.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.checkboxCleanupItem.Size = new System.Drawing.Size(814, 29);
            this.checkboxCleanupItem.TabIndex = 9;
            this.checkboxCleanupItem.Text = "checkboxCleanupItem";
            this.checkboxCleanupItem.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(3, 29);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(27, 21);
            this.panel1.TabIndex = 15;
            // 
            // checkedListBoxcheckboxCleanupSubItems
            // 
            this.checkedListBoxcheckboxCleanupSubItems.CausesValidation = false;
            this.checkedListBoxcheckboxCleanupSubItems.CheckOnClick = true;
            this.checkedListBoxcheckboxCleanupSubItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBoxcheckboxCleanupSubItems.FormatString = "Name";
            this.checkedListBoxcheckboxCleanupSubItems.FormattingEnabled = true;
            this.checkedListBoxcheckboxCleanupSubItems.Location = new System.Drawing.Point(30, 29);
            this.checkedListBoxcheckboxCleanupSubItems.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkedListBoxcheckboxCleanupSubItems.Name = "checkedListBoxcheckboxCleanupSubItems";
            this.checkedListBoxcheckboxCleanupSubItems.Size = new System.Drawing.Size(787, 21);
            this.checkedListBoxcheckboxCleanupSubItems.TabIndex = 16;
            // 
            // CleanupItemUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.checkedListBoxcheckboxCleanupSubItems);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.checkboxCleanupItem);
            this.Margin = new System.Windows.Forms.Padding(4, 0, 4, 5);
            this.Name = "CleanupItemUserControl";
            this.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.Size = new System.Drawing.Size(820, 53);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox checkboxCleanupItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckedListBox checkedListBoxcheckboxCleanupSubItems;
    }
}

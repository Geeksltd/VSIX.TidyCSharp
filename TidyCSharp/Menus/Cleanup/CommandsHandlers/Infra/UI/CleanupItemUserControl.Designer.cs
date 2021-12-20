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
            this.panel1 = new System.Windows.Forms.Panel();
            this.customCheckListBox1 = new Geeks.VSIX.TidyCSharp.Menus.Cleanup.CommandsHandlers.Infra.CustomChecklistBox();
            this.checkboxCleanupItem = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(2, 17);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(18, 3);
            this.panel1.TabIndex = 15;
            // 
            // customCheckListBox1
            // 
            this.customCheckListBox1.AutoSize = true;
            this.customCheckListBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.customCheckListBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customCheckListBox1.Location = new System.Drawing.Point(20, 17);
            this.customCheckListBox1.MaximumSize = new System.Drawing.Size(460, 0);
            this.customCheckListBox1.Name = "customCheckListBox1";
            this.customCheckListBox1.Size = new System.Drawing.Size(458, 3);
            this.customCheckListBox1.TabIndex = 16;
            // 
            // checkboxCleanupItem
            // 
            this.checkboxCleanupItem.AutoSize = true;
            this.checkboxCleanupItem.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkboxCleanupItem.Location = new System.Drawing.Point(2, 0);
            this.checkboxCleanupItem.Margin = new System.Windows.Forms.Padding(2);
            this.checkboxCleanupItem.MaximumSize = new System.Drawing.Size(476, 0);
            this.checkboxCleanupItem.Name = "checkboxCleanupItem";
            this.checkboxCleanupItem.Size = new System.Drawing.Size(476, 17);
            this.checkboxCleanupItem.TabIndex = 9;
            this.checkboxCleanupItem.Text = "checkboxCleanupItem";
            this.checkboxCleanupItem.UseVisualStyleBackColor = true;
            // 
            // CleanupItemUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.customCheckListBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.checkboxCleanupItem);
            this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.Name = "CleanupItemUserControl";
            this.Padding = new System.Windows.Forms.Padding(2, 0, 2, 2);
            this.Size = new System.Drawing.Size(480, 22);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private Infra.CustomChecklistBox customCheckListBox1;
        private System.Windows.Forms.CheckBox checkboxCleanupItem;
    }
}

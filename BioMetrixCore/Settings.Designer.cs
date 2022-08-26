namespace BioMetrixCore
{
    partial class Settings
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.checkBoxAutoStart = new System.Windows.Forms.CheckBox();
            this.checkBoxRepeatWhenFails = new System.Windows.Forms.CheckBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkBoxAutoStart
            // 
            this.checkBoxAutoStart.AutoSize = true;
            this.checkBoxAutoStart.Location = new System.Drawing.Point(57, 47);
            this.checkBoxAutoStart.Name = "checkBoxAutoStart";
            this.checkBoxAutoStart.Size = new System.Drawing.Size(73, 17);
            this.checkBoxAutoStart.TabIndex = 0;
            this.checkBoxAutoStart.Text = "Auto Start";
            this.checkBoxAutoStart.UseVisualStyleBackColor = true;
            // 
            // checkBoxRepeatWhenFails
            // 
            this.checkBoxRepeatWhenFails.AutoSize = true;
            this.checkBoxRepeatWhenFails.Location = new System.Drawing.Point(57, 70);
            this.checkBoxRepeatWhenFails.Name = "checkBoxRepeatWhenFails";
            this.checkBoxRepeatWhenFails.Size = new System.Drawing.Size(117, 17);
            this.checkBoxRepeatWhenFails.TabIndex = 1;
            this.checkBoxRepeatWhenFails.Text = "Repeat When Fails";
            this.checkBoxRepeatWhenFails.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(57, 110);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(107, 42);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = "&Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(227, 188);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.checkBoxRepeatWhenFails);
            this.Controls.Add(this.checkBoxAutoStart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Settings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxAutoStart;
        private System.Windows.Forms.CheckBox checkBoxRepeatWhenFails;
        private System.Windows.Forms.Button buttonSave;
    }
}
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
            this.numericUpDownTimer = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.tbApiLoginUrl = new System.Windows.Forms.TextBox();
            this.tbApiPostUrl = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBoxAutoStart
            // 
            this.checkBoxAutoStart.AutoSize = true;
            this.checkBoxAutoStart.Location = new System.Drawing.Point(31, 29);
            this.checkBoxAutoStart.Name = "checkBoxAutoStart";
            this.checkBoxAutoStart.Size = new System.Drawing.Size(73, 17);
            this.checkBoxAutoStart.TabIndex = 0;
            this.checkBoxAutoStart.Text = "Auto Start";
            this.checkBoxAutoStart.UseVisualStyleBackColor = true;
            // 
            // checkBoxRepeatWhenFails
            // 
            this.checkBoxRepeatWhenFails.AutoSize = true;
            this.checkBoxRepeatWhenFails.Location = new System.Drawing.Point(31, 52);
            this.checkBoxRepeatWhenFails.Name = "checkBoxRepeatWhenFails";
            this.checkBoxRepeatWhenFails.Size = new System.Drawing.Size(117, 17);
            this.checkBoxRepeatWhenFails.TabIndex = 1;
            this.checkBoxRepeatWhenFails.Text = "Repeat When Fails";
            this.checkBoxRepeatWhenFails.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(596, 240);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(107, 42);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = "&Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // numericUpDownTimer
            // 
            this.numericUpDownTimer.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownTimer.Location = new System.Drawing.Point(31, 116);
            this.numericUpDownTimer.Maximum = new decimal(new int[] {
            3600000,
            0,
            0,
            0});
            this.numericUpDownTimer.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericUpDownTimer.Name = "numericUpDownTimer";
            this.numericUpDownTimer.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownTimer.TabIndex = 3;
            this.numericUpDownTimer.Value = new decimal(new int[] {
            150000,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Repeat Timer (in seconds)";
            // 
            // tbApiLoginUrl
            // 
            this.tbApiLoginUrl.Location = new System.Drawing.Point(112, 169);
            this.tbApiLoginUrl.Name = "tbApiLoginUrl";
            this.tbApiLoginUrl.PasswordChar = '*';
            this.tbApiLoginUrl.Size = new System.Drawing.Size(591, 20);
            this.tbApiLoginUrl.TabIndex = 5;
            // 
            // tbApiPostUrl
            // 
            this.tbApiPostUrl.Location = new System.Drawing.Point(112, 195);
            this.tbApiPostUrl.Name = "tbApiPostUrl";
            this.tbApiPostUrl.PasswordChar = '*';
            this.tbApiPostUrl.Size = new System.Drawing.Size(591, 20);
            this.tbApiPostUrl.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 175);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "API Login URL";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 202);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "API Post URL";
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(715, 294);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbApiPostUrl);
            this.Controls.Add(this.tbApiLoginUrl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDownTimer);
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
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxAutoStart;
        private System.Windows.Forms.CheckBox checkBoxRepeatWhenFails;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.NumericUpDown numericUpDownTimer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbApiLoginUrl;
        private System.Windows.Forms.TextBox tbApiPostUrl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}
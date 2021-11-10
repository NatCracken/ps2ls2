namespace ps2ls.Forms
{
    partial class SoundExportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SoundExportForm));
            this.label1 = new System.Windows.Forms.Label();
            this.soundFormatComboBox = new System.Windows.Forms.ComboBox();
            this.exportButton = new System.Windows.Forms.Button();
            this.exportFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File Type";
            // 
            // soundFormatComboBox
            // 
            this.soundFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.soundFormatComboBox.FormattingEnabled = true;
            this.soundFormatComboBox.Items.AddRange(new object[] {
            "WAV"});
            this.soundFormatComboBox.Location = new System.Drawing.Point(70, 12);
            this.soundFormatComboBox.Name = "soundFormatComboBox";
            this.soundFormatComboBox.Size = new System.Drawing.Size(217, 21);
            this.soundFormatComboBox.TabIndex = 1;
            this.soundFormatComboBox.SelectedIndexChanged += new System.EventHandler(this.soundFormatComboBox_SelectedIndexChanged);
            // 
            // exportButton
            // 
            this.exportButton.Location = new System.Drawing.Point(212, 48);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 2;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // SoundExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 82);
            this.Controls.Add(this.soundFormatComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.exportButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SoundExportForm";
            this.Text = "Export Sound";
            this.Load += new System.EventHandler(this.SoundExportForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox soundFormatComboBox;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.FolderBrowserDialog exportFolderBrowserDialog;
    }
}
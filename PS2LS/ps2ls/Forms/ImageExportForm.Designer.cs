namespace ps2ls.Forms
{
    partial class ImageExportForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageExportForm));
            this.exportButton = new System.Windows.Forms.Button();
            this.exportFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.label7 = new System.Windows.Forms.Label();
            this.textureFormatComboBox = new System.Windows.Forms.ComboBox();
            this.packageToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // exportButton
            // 
            this.exportButton.Location = new System.Drawing.Point(212, 39);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 5;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(78, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Texture Format";
            // 
            // textureFormatComboBox
            // 
            this.textureFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.textureFormatComboBox.FormattingEnabled = true;
            this.textureFormatComboBox.Items.AddRange(new object[] {
            "DirectDraw Surface (*.dds)",
            "Portal Network Graphics (*.png)",
            "Truevision TGA (*.tga)"});
            this.textureFormatComboBox.Location = new System.Drawing.Point(95, 12);
            this.textureFormatComboBox.Name = "textureFormatComboBox";
            this.textureFormatComboBox.Size = new System.Drawing.Size(192, 21);
            this.textureFormatComboBox.TabIndex = 6;
            this.textureFormatComboBox.SelectedIndexChanged += new System.EventHandler(this.textureFormatComboBox_SelectedIndexChanged);
            // 
            // ImageExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 74);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textureFormatComboBox);
            this.Controls.Add(this.exportButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageExportForm";
            this.ShowInTaskbar = false;
            this.Text = "Texture Export";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ImageExportForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.FolderBrowserDialog exportFolderBrowserDialog;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox textureFormatComboBox;
        private System.Windows.Forms.ToolTip packageToolTip;
    }
}
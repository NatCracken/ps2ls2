namespace ps2ls.Forms
{
    partial class SoundBrowser
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.searchBox = new System.Windows.Forms.ToolStripTextBox();
            this.SearchBoxClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lastPageButton = new System.Windows.Forms.ToolStripButton();
            this.nextPageButton = new System.Windows.Forms.ToolStripButton();
            this.filesListedLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.SoundControlPanel = new System.Windows.Forms.Panel();
            this.TrackProgressBar = new System.Windows.Forms.ProgressBar();
            this.PlayPauseButton = new System.Windows.Forms.Button();
            this.VisualizationBox = new System.Windows.Forms.PictureBox();
            this.TrackNameLabel = new System.Windows.Forms.Label();
            this.StopButton = new System.Windows.Forms.Button();
            this.TrackProgressLabel = new System.Windows.Forms.Label();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.progressTimer = new System.Windows.Forms.Timer(this.components);
            this.soundListBox = new ps2ls.Forms.Controls.CustomListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SoundControlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VisualizationBox)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.soundListBox);
            this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
            this.splitContainer1.Panel1.Controls.Add(this.statusStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.SoundControlPanel);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(20);
            this.splitContainer1.Size = new System.Drawing.Size(800, 600);
            this.splitContainer1.SplitterDistance = 266;
            this.splitContainer1.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.searchBox,
            this.SearchBoxClear,
            this.toolStripSeparator1,
            this.toolStripButton2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(266, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::ps2ls.Properties.Resources.magnifier;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // searchBox
            // 
            this.searchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.searchBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(100, 25);
            this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
            // 
            // SearchBoxClear
            // 
            this.SearchBoxClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SearchBoxClear.Image = global::ps2ls.Properties.Resources.ui_text_field_clear_button;
            this.SearchBoxClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchBoxClear.Name = "SearchBoxClear";
            this.SearchBoxClear.Size = new System.Drawing.Size(23, 22);
            this.SearchBoxClear.Text = "toolStripButton2";
            this.SearchBoxClear.Click += new System.EventHandler(this.SearchBoxClear_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::ps2ls.Properties.Resources.drive_download;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "toolStripButton2";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lastPageButton,
            this.nextPageButton,
            this.filesListedLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 578);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(266, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lastPageButton
            // 
            this.lastPageButton.Image = global::ps2ls.Properties.Resources.arrow_left;
            this.lastPageButton.Name = "lastPageButton";
            this.lastPageButton.Size = new System.Drawing.Size(23, 20);
            this.lastPageButton.Click += new System.EventHandler(this.lastPageButton_Click);
            // 
            // nextPageButton
            // 
            this.nextPageButton.Image = global::ps2ls.Properties.Resources.arrow_right;
            this.nextPageButton.Name = "nextPageButton";
            this.nextPageButton.Size = new System.Drawing.Size(23, 20);
            this.nextPageButton.Click += new System.EventHandler(this.nextPageButton_Click);
            // 
            // filesListedLabel
            // 
            this.filesListedLabel.Image = global::ps2ls.Properties.Resources.document_search_result;
            this.filesListedLabel.Name = "filesListedLabel";
            this.filesListedLabel.Size = new System.Drawing.Size(46, 17);
            this.filesListedLabel.Text = "0 / 0";
            // 
            // SoundControlPanel
            // 
            this.SoundControlPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SoundControlPanel.Controls.Add(this.TrackProgressBar);
            this.SoundControlPanel.Controls.Add(this.PlayPauseButton);
            this.SoundControlPanel.Controls.Add(this.VisualizationBox);
            this.SoundControlPanel.Controls.Add(this.TrackNameLabel);
            this.SoundControlPanel.Controls.Add(this.StopButton);
            this.SoundControlPanel.Controls.Add(this.TrackProgressLabel);
            this.SoundControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SoundControlPanel.Location = new System.Drawing.Point(20, 20);
            this.SoundControlPanel.Name = "SoundControlPanel";
            this.SoundControlPanel.Padding = new System.Windows.Forms.Padding(20, 20, 20, 40);
            this.SoundControlPanel.Size = new System.Drawing.Size(490, 560);
            this.SoundControlPanel.TabIndex = 6;
            // 
            // TrackProgressBar
            // 
            this.TrackProgressBar.Location = new System.Drawing.Point(23, 179);
            this.TrackProgressBar.Name = "TrackProgressBar";
            this.TrackProgressBar.Size = new System.Drawing.Size(442, 10);
            this.TrackProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.TrackProgressBar.TabIndex = 4;
            // 
            // PlayPauseButton
            // 
            this.PlayPauseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PlayPauseButton.Location = new System.Drawing.Point(140, 218);
            this.PlayPauseButton.Name = "PlayPauseButton";
            this.PlayPauseButton.Size = new System.Drawing.Size(100, 32);
            this.PlayPauseButton.TabIndex = 1;
            this.PlayPauseButton.Text = "▶ / ❚❚";
            this.PlayPauseButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.PlayPauseButton.UseVisualStyleBackColor = true;
            this.PlayPauseButton.Click += new System.EventHandler(this.PlayPause_Click);
            // 
            // VisualizationBox
            // 
            this.VisualizationBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.VisualizationBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.VisualizationBox.Location = new System.Drawing.Point(23, 125);
            this.VisualizationBox.Name = "VisualizationBox";
            this.VisualizationBox.Size = new System.Drawing.Size(442, 60);
            this.VisualizationBox.TabIndex = 5;
            this.VisualizationBox.TabStop = false;
            // 
            // TrackNameLabel
            // 
            this.TrackNameLabel.AutoSize = true;
            this.TrackNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TrackNameLabel.Location = new System.Drawing.Point(-5, 97);
            this.TrackNameLabel.MinimumSize = new System.Drawing.Size(500, 25);
            this.TrackNameLabel.Name = "TrackNameLabel";
            this.TrackNameLabel.Size = new System.Drawing.Size(500, 25);
            this.TrackNameLabel.TabIndex = 0;
            this.TrackNameLabel.Text = "No Track";
            this.TrackNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StopButton
            // 
            this.StopButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.StopButton.Location = new System.Drawing.Point(246, 218);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(100, 32);
            this.StopButton.TabIndex = 2;
            this.StopButton.Text = "⬛";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // TrackProgressLabel
            // 
            this.TrackProgressLabel.Location = new System.Drawing.Point(140, 192);
            this.TrackProgressLabel.Name = "TrackProgressLabel";
            this.TrackProgressLabel.Size = new System.Drawing.Size(206, 23);
            this.TrackProgressLabel.TabIndex = 3;
            this.TrackProgressLabel.Text = "00:00:00 / 00:00:00";
            this.TrackProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // refreshTimer
            // 
            this.refreshTimer.Interval = 500;
            this.refreshTimer.Tick += new System.EventHandler(this.refreshTimer_Tick);
            // 
            // progressTimer
            // 
            this.progressTimer.Tick += new System.EventHandler(this.progressTimer_Tick);
            // 
            // soundListBox
            // 
            this.soundListBox.AssetType = new ps2ls.Assets.Asset.Types[] { ps2ls.Assets.Asset.Types.FSB };
            this.soundListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.soundListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.soundListBox.Image = global::ps2ls.Properties.Resources.music;
            this.soundListBox.Items.AddRange(new object[] {
            "Input a search term to continue"});
            this.soundListBox.Location = new System.Drawing.Point(0, 25);
            this.soundListBox.Name = "soundListBox";
            this.soundListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.soundListBox.Size = new System.Drawing.Size(266, 553);
            this.soundListBox.TabIndex = 0;
            this.soundListBox.SelectedIndexChanged += new System.EventHandler(this.soundListBox_SelectedIndexChanged);
            // 
            // SoundBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "SoundBrowser";
            this.Size = new System.Drawing.Size(800, 600);
            this.Load += new System.EventHandler(this.SoundBrowser_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.SoundControlPanel.ResumeLayout(false);
            this.SoundControlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VisualizationBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private Controls.CustomListBox soundListBox;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripButton lastPageButton;
        private System.Windows.Forms.ToolStripButton nextPageButton;
        private System.Windows.Forms.ToolStripStatusLabel filesListedLabel;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripTextBox searchBox;
        private System.Windows.Forms.ToolStripButton SearchBoxClear;
        private System.Windows.Forms.Label TrackNameLabel;
        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Button PlayPauseButton;
        private System.Windows.Forms.ProgressBar TrackProgressBar;
        private System.Windows.Forms.Label TrackProgressLabel;
        private System.Windows.Forms.Timer progressTimer;
        private System.Windows.Forms.Panel SoundControlPanel;
        private System.Windows.Forms.PictureBox VisualizationBox;
    }
}
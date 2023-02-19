namespace ps2ls.Forms
{
    partial class ActorBrowser
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("ActorDefinition");
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.searchText = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.lastPageButton = new System.Windows.Forms.ToolStripButton();
            this.nextPageButton = new System.Windows.Forms.ToolStripButton();
            this.filesListedLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.searchTextTimer = new System.Windows.Forms.Timer(this.components);
            this.actorListbox = new ps2ls.Forms.Controls.CustomListBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.actorTreeView = new System.Windows.Forms.TreeView();
            this.toolStrip1.SuspendLayout();
            this.statusStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.searchText,
            this.toolStripButton2,
            this.toolStripSeparator1,
            this.toolStripButton3,
            this.toolStripSeparator2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(250, 25);
            this.toolStrip1.TabIndex = 1;
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
            // searchText
            // 
            this.searchText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.searchText.Name = "searchText";
            this.searchText.Size = new System.Drawing.Size(100, 25);
            this.searchText.TextChanged += new System.EventHandler(this.searchText_TextChanged);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::ps2ls.Properties.Resources.ui_text_field_clear_button;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "clearSearchText";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = global::ps2ls.Properties.Resources.drive_download;
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "Extract Image";
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // statusStrip2
            // 
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lastPageButton,
            this.nextPageButton,
            this.filesListedLabel});
            this.statusStrip2.Location = new System.Drawing.Point(0, 578);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Size = new System.Drawing.Size(250, 22);
            this.statusStrip2.TabIndex = 4;
            this.statusStrip2.Text = "statusStrip2";
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
            this.filesListedLabel.Size = new System.Drawing.Size(104, 17);
            this.filesListedLabel.Text = "Page 1: 0 - 0 / 0";
            // 
            // searchTextTimer
            // 
            this.searchTextTimer.Enabled = true;
            this.searchTextTimer.Interval = 500;
            this.searchTextTimer.Tick += new System.EventHandler(this.searchTextTimer_Tick);
            // 
            // actorListbox
            // 
            this.actorListbox.AssetType = new Assets.Asset.Types[] { Assets.Asset.Types.ADR };
            this.actorListbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actorListbox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.actorListbox.FormattingEnabled = true;
            this.actorListbox.Image = global::ps2ls.Properties.Resources.robot;
            this.actorListbox.Items.AddRange(new object[] {
            "ActorListBox"});
            this.actorListbox.Location = new System.Drawing.Point(0, 25);
            this.actorListbox.Name = "actorListbox";
            this.actorListbox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.actorListbox.Size = new System.Drawing.Size(250, 553);
            this.actorListbox.TabIndex = 0;
            this.actorListbox.SelectedIndexChanged += new System.EventHandler(this.actorListbox_SelectedIndexChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.actorListbox);
            this.splitContainer1.Panel1.Controls.Add(this.statusStrip2);
            this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.actorTreeView);
            this.splitContainer1.Panel2.Controls.Add(this.toolStrip2);
            this.splitContainer1.Size = new System.Drawing.Size(800, 600);
            this.splitContainer1.SplitterDistance = 250;
            this.splitContainer1.TabIndex = 5;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(546, 25);
            this.toolStrip2.TabIndex = 3;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // actorTreeView
            // 
            this.actorTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actorTreeView.Location = new System.Drawing.Point(0, 25);
            this.actorTreeView.Name = "actorTreeView";
            treeNode1.Name = "ActorDefinition";
            treeNode1.Text = "ActorDefinition";
            this.actorTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.actorTreeView.Size = new System.Drawing.Size(546, 575);
            this.actorTreeView.TabIndex = 4;
            // 
            // ActorBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ActorBrowser";
            this.Size = new System.Drawing.Size(800, 600);
            this.Load += new System.EventHandler(this.ActorBrowser_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.CustomListBox actorListbox;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripTextBox searchText;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.ToolStripButton lastPageButton;
        private System.Windows.Forms.ToolStripButton nextPageButton;
        private System.Windows.Forms.ToolStripStatusLabel filesListedLabel;
        private System.Windows.Forms.Timer searchTextTimer;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.TreeView actorTreeView;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ps2ls.Assets;
using System.Xml;
using System.Xml.XPath;


namespace ps2ls.Forms
{
    public partial class ActorBrowser : UserControl
    {
        #region Singleton
        private static ActorBrowser instance = null;

        public static void CreateInstance()
        {
            instance = new ActorBrowser();
        }

        public static void DeleteInstance()
        {
            instance = null;
        }

        public static ActorBrowser Instance { get { return instance; } }
        #endregion

        public ActorBrowser()
        {
            InitializeComponent();

            actorListbox.Items.Clear();

            Dock = DockStyle.Fill;
        }

        public void onEnter(object sender, EventArgs e)
        {
            actorListbox.LoadAndSortAssets();
            refreshActorListBox();
        }

        private void actorListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Asset asset;

            try
            {
                asset = (Asset)actorListbox.SelectedItem;
            }
            catch (InvalidCastException) { return; }

            System.IO.MemoryStream memoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name);
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(memoryStream);
            }
            catch (Exception)
            {
                return;
            }

            actorTreeView.Nodes.Clear();
            ConvertXmlNodeToTreeNode(xmlDoc, actorTreeView.Nodes);
            actorTreeView.ExpandAll();
        }

        private void ConvertXmlNodeToTreeNode(XmlNode xmlNode, TreeNodeCollection treeNodes)
        {
            TreeNode newTreeNode = treeNodes.Add(xmlNode.Name);

            switch (xmlNode.NodeType)
            {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.XmlDeclaration:
                    newTreeNode.Text = "<?" + xmlNode.Name + " " + xmlNode.Value + "?>";
                    break;
                case XmlNodeType.Element:
                    newTreeNode.Text = "<" + xmlNode.Name + ">";
                    break;
                case XmlNodeType.Attribute:
                    newTreeNode.Text = xmlNode.Name + ": " + xmlNode.Value;
                    return;//in this schema, attributes are allways the end of branch
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    newTreeNode.Text = xmlNode.Value;
                    break;
                case XmlNodeType.Comment:
                    newTreeNode.Text = "<!--" + xmlNode.Value + "-->";
                    break;
            }

            if (xmlNode.Attributes != null)
            {
                foreach (XmlAttribute attribute in xmlNode.Attributes)
                {
                    ConvertXmlNodeToTreeNode(attribute, newTreeNode.Nodes);
                }
            }
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                ConvertXmlNodeToTreeNode(childNode, newTreeNode.Nodes);
            }
        }


        private void searchText_TextChanged(object sender, EventArgs e)
        {
            handleTextTimer();
        }

        private void handleTextTimer()
        {
            searchTextTimer.Stop();
            searchTextTimer.Start();
        }


        private void searchTextTimer_Tick(object sender, EventArgs e)
        {
            if (searchText.Text.Length > 0)
            {
                searchText.BackColor = Color.Yellow;
                toolStripButton2.Enabled = true;

            }
            else
            {
                searchText.BackColor = Color.White;
                toolStripButton2.Enabled = false;
            }

            searchTextTimer.Stop();
            refreshActorListBox();
        }

        private int pageNumber = 0;
        private int pageSize = 1000;
        private void refreshActorListBox()
        {
            actorListbox.FilterBySearch(searchText.Text ?? "");

            int filtered = actorListbox.MaxFilteredCount;

            int populateStart = pageNumber * pageSize;
            int populateEnd = populateStart + pageSize;
            if (populateEnd > filtered) populateEnd = filtered;
            actorListbox.PopulateBox(populateStart, populateEnd);

            filesListedLabel.Text = "Page " + (pageNumber + 1)
                + ": " + populateStart + " - " + populateEnd + " / " + filtered;
        }

        private void nextPageButton_Click(object sender, EventArgs e)
        {
            int maxPageIndex = actorListbox.MaxFilteredCount / pageSize;
            if (++pageNumber > maxPageIndex) pageNumber = maxPageIndex;
            refreshActorListBox();
        }

        private void lastPageButton_Click(object sender, EventArgs e)
        {
            if (--pageNumber < 0) pageNumber = 0;
            refreshActorListBox();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            searchText.Clear();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            List<string> fileNames = new List<string>();

            foreach (object selectedItem in actorListbox.SelectedItems)
            {
                Asset asset = null;

                try
                {
                    asset = (Asset)selectedItem;
                }
                catch (InvalidCastException) { continue; }

                fileNames.Add(asset.Name);
            }

            foreach (string s in fileNames)
            {
                Console.WriteLine(s);
            }


            ImageExportForm modelExportForm = new ImageExportForm();
            modelExportForm.FileNames = fileNames;
            modelExportForm.ShowDialog();
        }


        private void ActorBrowser_Load(object sender, EventArgs e)
        {
            handleTextTimer();
        }

    }
}

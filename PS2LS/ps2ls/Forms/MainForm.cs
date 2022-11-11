using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ps2ls.Properties;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using ps2ls.Assets;
using ps2ls.Assets.Pack;

namespace ps2ls.Forms
{
    public partial class MainForm : Form
    {
        #region Singleton
        private static MainForm instance = null;

        public static void CreateInstance()
        {
            instance = new MainForm();
        }

        public static void DeleteInstance()
        {
            instance = null;
        }

        public static MainForm Instance { get { return instance; } }
        #endregion

        private MainForm()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox.Instance.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AssetBrowser.CreateInstance();
            ModelBrowser.CreateInstance();
            MaterialBrowser.CreateInstance();
            ImageBrowser.CreateInstance();
            SoundBrowser.CreateInstance();
            ActorBrowser.CreateInstance();

            ImageList imageList = new ImageList();
            imageList.Images.Add(Resources.box_small);
            imageList.Images.Add(Resources.tree_small);
            imageList.Images.Add(Resources.image);
            imageList.Images.Add(Resources.music);
            imageList.Images.Add(Resources.robot);
            tabControl1.ImageList = imageList;

            TabPage assetBrowserTabPage = new TabPage("Asset Browser");
            assetBrowserTabPage.Controls.Add(AssetBrowser.Instance);
            assetBrowserTabPage.ImageIndex = 0;
            tabControl1.TabPages.Add(assetBrowserTabPage);

            TabPage modelBrowserTabPage = new TabPage("Model Browser");
            modelBrowserTabPage.Controls.Add(ModelBrowser.Instance);
            modelBrowserTabPage.ImageIndex = 1;
            modelBrowserTabPage.Enter += ModelBrowser.Instance.onEnter;
            tabControl1.TabPages.Add(modelBrowserTabPage);
         
            TabPage imageBrowser = new TabPage("Image Browser");
            imageBrowser.Controls.Add(ImageBrowser.Instance);
            imageBrowser.ImageIndex = 2;
            imageBrowser.Enter += ImageBrowser.Instance.onEnter;
            tabControl1.TabPages.Add(imageBrowser);

            TabPage soundBrowser = new TabPage("Sound Browser");
            soundBrowser.Controls.Add(SoundBrowser.Instance);
            soundBrowser.ImageIndex = 3;
            soundBrowser.Enter += SoundBrowser.Instance.onEnter;
            tabControl1.TabPages.Add(soundBrowser);

            TabPage actorBrowser = new TabPage("Actor Browser");
            actorBrowser.Controls.Add(ActorBrowser.Instance);
            actorBrowser.ImageIndex = 4;
            actorBrowser.Enter += ActorBrowser.Instance.onEnter;
            tabControl1.TabPages.Add(actorBrowser);

        }

        private void reportIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("no issue url found");
           // System.Diagnostics.Process.Start(Settings.Default.ProjectNewIssueURL);
        }

        private void compareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssetManager.Instance.WriteFileListingToFile("FileListing.txt");
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ps2ls.Assets.Pack;

namespace ps2ls.Forms
{
    public partial class ImageBrowser : UserControl
    {

        #region Singleton
        private static ImageBrowser instance = null;

        public static void CreateInstance()
        {
            instance = new ImageBrowser();
        }

        public static void DeleteInstance()
        {
            instance = null;
        }

        public static ImageBrowser Instance { get { return instance; } }
        #endregion

        public ImageBrowser()
        {
            InitializeComponent();

            imageListbox.Items.Clear();

            Dock = DockStyle.Fill;
        }

        public void onEnter(object sender, EventArgs e)
        {
            refreshImageListBox();
        }

        private void imageListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Asset asset;

            try
            {
                asset = (Asset)imageListbox.SelectedItem;
            }
            catch (InvalidCastException) { return; }

            System.IO.MemoryStream memoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name);

            Image i = TextureManager.LoadDrawingImageFromStream(memoryStream);

            pictureWindow.BackgroundImage = i;
            BackgroundImageLayout = ImageLayout.Stretch;
            pictureWindow.Show();
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
            refreshImageListBox();
        }

        private void refreshImageListBox()
        {
            imageListbox.PopulateBox(searchText.Text ?? "");

            int count = imageListbox.Items.Count;
            int max = imageListbox.MaxCount;

            imagesCountLabel.Text = count + "/" + max;

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            searchText.Clear();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            List<string> fileNames = new List<string>();

            foreach (object selectedItem in imageListbox.SelectedItems)
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

        private void pictureWindow_Click(object sender, EventArgs e)
        {

        }

        private void ImageBrowser_Load(object sender, EventArgs e)
        {
            handleTextTimer();
        }


    }
}

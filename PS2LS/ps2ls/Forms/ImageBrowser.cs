using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ps2ls.Assets;

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
            imageListbox.LoadAndSortAssets();
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
            Image i;
            switch (asset.Type)
            {
                case Asset.Types.PNG:
                case Asset.Types.JPG:
                    i = TextureManager.CommonStreamToBitmap(memoryStream);
                    break;
                default:
                    i = TextureManager.DDSStreamToBitmap(memoryStream);
                    break;
            }

            pictureWindow.BackgroundImage = i;
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

        public class AssetSearchParam
        {
            public int value;
            public Asset asset;
            public AssetSearchParam(int getV, Asset getA)
            {
                value = getV;
                asset = getA;
            }
        }

        private int pageNumber = 0;
        private int pageSize = 1000;
        private void refreshImageListBox()
        {
            imageListbox.FilterBySearch(searchText.Text ?? "");

            if (!showMultipleResolutionsButton.Checked)//for names that contain resolutions, keep only the largest
            {
                Dictionary<string, AssetSearchParam> nameToAsset = new Dictionary<string, AssetSearchParam>();
                for (int i = imageListbox.filteredAssets.Count - 1; i >= 0; i--)
                {
                    Asset a = imageListbox.filteredAssets[i];
                    int resolution = doesNameContainResolution(a.Name);
                    if (resolution == -1) continue;
                    string safeName = a.Name.Replace(resolution + "", "N");
                    if (nameToAsset.ContainsKey(safeName))//if a pair. remove the lower and save the higher
                    {
                        AssetSearchParam searchParam = nameToAsset[safeName];
                        if (resolution > searchParam.value)//if new is higher, keep it discard old
                        {
                            searchParam.value = resolution;

                            imageListbox.filteredAssets.Remove(searchParam.asset);
                            searchParam.asset = a;
                        }
                        else//if new is lower discard new
                        {
                            imageListbox.filteredAssets.RemoveAt(i);
                        }
                    }
                    else
                    {
                        nameToAsset.Add(safeName, new AssetSearchParam(resolution, a));
                    }
                }
                imageListbox.updateFilteredCount();
            }


            int filtered = imageListbox.MaxFilteredCount;

            int populateStart = pageNumber * pageSize;
            int populateEnd = populateStart + pageSize;
            if (populateEnd > filtered) populateEnd = filtered;
            imageListbox.PopulateBox(populateStart, populateEnd);

            filesListedLabel.Text = "Page " + (pageNumber + 1)
                + ": " + populateStart + " - " + populateEnd + " / " + filtered;
        }

        //returns -1 if no resolution, else the resolution
        private int doesNameContainResolution(string name)
        {
            for (int i = 16; i <= 1024; i *= 2)
            {
                if (name.Contains(i + "")) return i;
            }
            return -1;
        }

        private void nextPageButton_Click(object sender, EventArgs e)
        {
            int maxPageIndex = imageListbox.MaxFilteredCount / pageSize;
            if (++pageNumber > maxPageIndex) pageNumber = maxPageIndex;
            refreshImageListBox();
        }

        private void lastPageButton_Click(object sender, EventArgs e)
        {
            if (--pageNumber < 0) pageNumber = 0;
            refreshImageListBox();
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


        private void ImageBrowser_Load(object sender, EventArgs e)
        {
            handleTextTimer();
        }

        private void showMultipleResolutionsButton_CheckedChanged(object sender, EventArgs e)
        {
            refreshImageListBox();
        }

        private void ImageStrechButton_CheckedChanged(object sender, EventArgs e)
        {
            if (ImageStrechButton.Checked)
            {
                pictureWindow.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                pictureWindow.BackgroundImageLayout = ImageLayout.Zoom;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using ps2ls.Assets;

namespace ps2ls.Forms.Controls
{
    public class CustomListBox : ListBox
    {
        public Image Image { get; set; }

        public Asset.Types[] AssetType;

        List<Asset> assets = new List<Asset>();
        public int MaxCount { get; protected set; }
        public List<Asset> filteredAssets = new List<Asset>();
        public int MaxFilteredCount { get; protected set; }
        public CustomListBox()
        {
            this.DrawItem += new DrawItemEventHandler(this.CustomListBox_DrawItem);

            DrawMode = DrawMode.OwnerDrawFixed;
        }

        private void CustomListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            e.DrawBackground();

            String text = ((ListBox)sender).Items[e.Index].ToString();
            Point point = new Point(0, e.Bounds.Y);

            if (Image != null)
            {
                e.Graphics.DrawImage(Image, point);
                point.X += Image.Width;
            }

            e.Graphics.DrawString(text, e.Font, new SolidBrush(Color.Black), point);
            e.DrawFocusRectangle();
        }

        public void LoadAndSortAssets()
        {
            assets = new List<Asset>();

            foreach (Asset.Types type in AssetType)
            {
                AssetManager.Instance.AssetsByType.TryGetValue(type, out List<Asset> getAssets);
                if (getAssets != null) assets.AddRange(getAssets);
            }

            assets.Sort(new Asset.NameComparer());

            MaxCount = assets == null ? 0 : assets.Count;
        }

        public void FilterBySearch(string searchText)
        {
            filteredAssets = new List<Asset>();
            if (assets != null) foreach (Asset asset in assets)
                {
                    if (asset.Name.IndexOf(searchText, 0, StringComparison.OrdinalIgnoreCase) >= 0) filteredAssets.Add(asset);
                }
            updateFilteredCount();
        }

        public void updateFilteredCount()
        {
            MaxFilteredCount = filteredAssets == null ? 0 : filteredAssets.Count;
        }

        public void excludeFromFilter(string searchText)
        {
            for(int i = filteredAssets.Count - 1; i >= 0; i--)
            {
                if (filteredAssets[i].Name.IndexOf(searchText, 0, StringComparison.OrdinalIgnoreCase) >= 0) filteredAssets.RemoveAt(i);
            }
            updateFilteredCount();
        }

        public void PopulateBox(int startIndex, int endIndex)
        {
            this.Items.Clear();

            if (filteredAssets.Count < endIndex) endIndex = filteredAssets.Count;
            for (int i = startIndex; i < endIndex; i++) Items.Add(filteredAssets[i]);
        }
    }
}

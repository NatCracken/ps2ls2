using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using ps2ls.Forms;
using ps2ls.Graphics.Materials;

namespace ps2ls.Assets.Pack
{
    class AssetManager
    {
        #region Singleton
        private static AssetManager instance = null;

        public static void CreateInstance()
        {
            instance = new AssetManager();
        }

        public static void DeleteInstance()
        {
            instance = null;
        }

        public static AssetManager Instance { get { return instance; } }
        #endregion

        public List<Pack> Packs { get; private set; }
        public Dictionary<Asset.Types, List<Asset>> AssetsByType { get; private set; }

        // Internal cache to check whether a pack has already been loaded
        private Dictionary<Int32, Pack> packLookupCache = new Dictionary<Int32, Pack>();

        //break down a namelist file into this, hashxxxxxxxxxxxx:name_name_name.name format
        private Dictionary<ulong, string> nameDict = new Dictionary<ulong, string>();

        private GenericLoadingForm loadingForm;
        private BackgroundWorker loadBackgroundWorker;
        private BackgroundWorker extractAllBackgroundWorker;
        private BackgroundWorker extractSelectionBackgroundWorker;

        private AssetManager()
        {
            Packs = new List<Pack>();
            AssetsByType = new Dictionary<Asset.Types, List<Asset>>();

            loadBackgroundWorker = new BackgroundWorker();
            loadBackgroundWorker.WorkerReportsProgress = true;
            loadBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(loadProgressChanged);
            loadBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loadRunWorkerCompleted);
            loadBackgroundWorker.DoWork += new DoWorkEventHandler(loadDoWork);

            extractAllBackgroundWorker = new BackgroundWorker();
            extractAllBackgroundWorker.WorkerReportsProgress = true;
            extractAllBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(extractAllProgressChanged);
            extractAllBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(extractAllRunWorkerCompleted);
            extractAllBackgroundWorker.DoWork += new DoWorkEventHandler(extractAllDoWork);

            extractSelectionBackgroundWorker = new BackgroundWorker();
            extractSelectionBackgroundWorker.WorkerReportsProgress = true;
            extractSelectionBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(extractSelectionProgressChanged);
            extractSelectionBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(extractSelectionRunWorkerCompleted);
            extractSelectionBackgroundWorker.DoWork += new DoWorkEventHandler(extractSelectionDoWork);
        }

        public void LoadBinaryFromDirectory(string directory)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(Properties.Settings.Default.AssetDirectory, "*.pack", SearchOption.TopDirectoryOnly);

            LoadBinaryFromPaths(files);
        }

        public void LoadBinaryFromPaths(IEnumerable<string> paths)
        {
            loadingForm = new GenericLoadingForm();
            loadingForm.SetWindowTitle("Loading Assets...");
            loadingForm.Show();

            loadBackgroundWorker.RunWorkerAsync(paths);
        }

        private void loadRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            loadingForm.Close();

            if (AssetBrowser.Instance != null)
            {
                AssetBrowser.Instance.RefreshPacksListBox();
            }

            if (ModelBrowser.Instance != null)
            {
                ModelBrowser.Instance.Refresh();
            }

            if (MaterialBrowser.Instance != null)
            {
                MaterialBrowser.Instance.Refresh();
            }
        }

        private void loadProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            loadingForm.SetProgressBarPercent(args.ProgressPercentage);
            loadingForm.SetLabelText((string)args.UserState);
        }

        private void loadDoWork(object sender, DoWorkEventArgs args)
        {
            loadBinaryFromPaths(sender, args.Argument);
        }

        private void loadBinaryFromPaths(object sender, object arg)
        {
            BackgroundWorker backgroundWorker = (BackgroundWorker)sender;
            IEnumerable<string> paths = (IEnumerable<string>)arg;

            for (int i = 0; i < paths.Count(); ++i)
            {
                string path = paths.ElementAt(i);
                Pack pack = null;

                if (packLookupCache.TryGetValue(path.GetHashCode(), out pack) == false)
                {
                    pack = Pack.LoadBinary(path, nameDict);

                    if (pack != null)
                    {
                        packLookupCache.Add(path.GetHashCode(), pack);
                        Packs.Add(pack);

                        foreach (Asset asset in pack.Assets)
                        {
                            if (false == AssetsByType.ContainsKey(asset.Type))
                            {
                                AssetsByType.Add(asset.Type, new List<Asset>());
                            }

                            AssetsByType[asset.Type].Add(asset);
                        }
                    }
                }

                float percent = (i + 1) / (float)paths.Count();
                backgroundWorker.ReportProgress((int)(percent * 100.0f), System.IO.Path.GetFileName(path));
            }
        }

        public Asset GetAssetByName(Asset.Types type, string search)
        {
            List<Asset> toSearch = AssetsByType[type];
            foreach (Asset asset in toSearch) if (asset.Name.Equals(search)) return asset;
            return null;
        }

        public bool LoadNameListFromPath(string paths)
        {
            string[] lines = System.IO.File.ReadAllLines(paths);

            nameDict = new Dictionary<ulong, string>();
            foreach (string line in lines)
            {
                string[] temp = line.Split(':');
                nameDict.Add(ulong.Parse(temp[0]), temp[1]);
            }

            Console.WriteLine("NameList Loaded, " + nameDict.Count + " entries");
            return true;
        }

        public void ExtractAllToDirectory(string directory)
        {
            loadingForm = new GenericLoadingForm();
            loadingForm.Show();

            extractAllBackgroundWorker.RunWorkerAsync(directory);
        }

        private void extractAllToDirectory(object sender, object arg)
        {
            BackgroundWorker backgroundWorker = (BackgroundWorker)sender;
            String directory = String.Empty;

            try
            {
                directory = (String)arg;
            }
            catch (InvalidCastException) { return; }

            for (Int32 i = 0; i < Packs.Count; ++i)
            {
                Pack pack = Packs.ElementAt(i);

                pack.ExtractAllAssetsToDirectory(directory);

                Single percent = (Single)(i + 1) / (Single)Packs.Count;
                backgroundWorker.ReportProgress((Int32)(percent * 100.0f), System.IO.Path.GetFileName(pack.Path));
            }
        }

        private void extractAllRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            loadingForm.Close();
        }

        private void extractAllProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            loadingForm.SetProgressBarPercent(args.ProgressPercentage);
            loadingForm.SetLabelText((String)args.UserState);
        }

        private void extractAllDoWork(object sender, DoWorkEventArgs args)
        {
            extractAllToDirectory(sender, args.Argument);
        }

        public void ExtractAssetsByNamesToDirectory(IEnumerable<String> names, String directory)
        {
            foreach (Pack pack in Packs)
            {
                foreach (String name in names)
                {
                    pack.ExtractAssetsByNameToDirectory(names, directory);
                }
            }
        }

        public void ExtractByAssetsToDirectoryAsync(IEnumerable<Asset> assets, string directory)
        {
            loadingForm = new GenericLoadingForm();
            loadingForm.Show();

            object[] args = new object[] { assets, directory };

            extractSelectionBackgroundWorker.RunWorkerAsync(args);
        }

        private void extractByAssetsToDirectory(object sender, object arg)
        {
            BackgroundWorker backgroundWorker = (BackgroundWorker)sender;
            object[] args = (object[])arg;
            IEnumerable<Asset> assets = (IEnumerable<Asset>)args[0];
            string directory = (string)args[1];

            for (int i = 0; i < assets.Count(); ++i)
            {
                Asset file = assets.ElementAt(i);

                file.Pack.ExtractAssetByNameToDirectory(file.Name, directory);

                float percent = (i + 1) / (float)assets.Count();
                backgroundWorker.ReportProgress((int)(percent * 100.0f), System.IO.Path.GetFileName(file.Name));
            }
        }

        private void extractSelectionRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            loadingForm.Close();
        }

        private void extractSelectionProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            loadingForm.SetProgressBarPercent(args.ProgressPercentage);
            loadingForm.SetLabelText((string)args.UserState);
        }

        private void extractSelectionDoWork(object sender, DoWorkEventArgs args)
        {
            extractByAssetsToDirectory(sender, args.Argument);
        }

        public MemoryStream CreateAssetMemoryStreamByName(string name)
        {
            MemoryStream memoryStream = null;

            foreach (Pack pack in Packs)
            {
                memoryStream = pack.CreateAssetMemoryStreamByName(name);

                if (memoryStream != null)
                {
                    break;
                }
            }

            return memoryStream;
        }

        public void WriteFileListingToFile(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (Pack p in Packs)
                {
                    foreach (Asset asset in p.Assets)
                    {
                        writer.WriteLine(string.Format("{0}\t{1}\t{2}", asset.Name, asset.UnzippedLength, asset.Crc32));
                    }
                }
            }

        }

    }
}

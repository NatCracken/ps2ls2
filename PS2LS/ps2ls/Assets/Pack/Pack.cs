using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;
using ps2ls.IO;
using Ionic.Zlib;

namespace ps2ls.Assets.Pack
{
    public class Pack
    {
        [DescriptionAttribute("The path on disk to this pack file.")]
        [ReadOnlyAttribute(true)]
        public string Path { get; private set; }

        [DescriptionAttribute("The number of assets contained in this pack file.")]
        [ReadOnlyAttribute(true)]
        public UInt32 AssetCount { get; private set; }

        [DescriptionAttribute("The size of the pack2 file in bytes.")]
        [ReadOnlyAttribute(true)]
        public ulong Length { get; private set; }

        [DescriptionAttribute("Offset of the Map, where all the asset hashes are located")]
        [ReadOnlyAttribute(true)]
        public ulong MapOffset { get; private set; }

        [BrowsableAttribute(false)]
        public List<Asset> Assets { get; private set; }

        [BrowsableAttribute(false)]
        public List<Asset> Raw_Assets { get; private set; }

        [DescriptionAttribute("The total size in bytes of all assets contained in this pack file.")]
        [ReadOnlyAttribute(true)]
        public ulong AssetSize
        {
            get
            {
                ulong assetSize = 0;

                foreach (Asset asset in Assets)
                {
                    assetSize += asset.DataLength;
                }

                return assetSize;
            }
        }

        [BrowsableAttribute(false)]
        public String Name
        {
            get { return System.IO.Path.GetFileName(Path); }
        }

        public Dictionary<Int32, Asset> assetLookupCache = new Dictionary<Int32, Asset>();

        private Pack(String path)
        {
            Path = path;
            Assets = new List<Asset>();
        }

        public static Pack LoadBinary(string path, Dictionary<ulong, string> nameDict)
        {
            Pack pack = new Pack(path);
            FileStream fileStream = null;

            try
            {
                fileStream = File.OpenRead(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                return null;
            }

            //BinaryReaderBigEndian BinaryReaderBE = new BinaryReaderBigEndian(fileStream);
            BinaryReader BinaryReaderLE = new BinaryReader(fileStream);


            //---------------------------------read header--------------------------
            fileStream.Seek(0, SeekOrigin.Begin);
            uint magic = BinaryReaderLE.ReadUInt32();
            //TODO check magic matches pak header
            pack.AssetCount = BinaryReaderLE.ReadUInt32();
            pack.Length = BinaryReaderLE.ReadUInt64();
            pack.MapOffset = BinaryReaderLE.ReadUInt64();

            fileStream.Seek(Convert.ToInt64(pack.MapOffset), SeekOrigin.Begin);
            for (int i = 0; i < pack.AssetCount; i++)
            {
                Asset asset = Asset.LoadBinary(pack, fileStream, nameDict);
                pack.assetLookupCache.Add(asset.Name.GetHashCode(), asset);
                pack.Assets.Add(asset);
            }


            return pack;
        }

        static byte[] decompress(byte[] data)
        {
            using (var memStream = new MemoryStream(data))
            using (var zLibStream = new ZlibStream(memStream, CompressionMode.Decompress))
            using (var outStream = new MemoryStream())
            {
                zLibStream.CopyTo(outStream);
                return outStream.ToArray();
            }
        }

        public Boolean ExtractAllAssetsToDirectory(String directory)
        {
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                return false;
            }

            foreach (Asset asset in Assets)
            {
                byte[] buffer = new byte[(int)asset.DataLength];

                fileStream.Seek(Convert.ToInt64(asset.Offset) + 8, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, (int)asset.DataLength);

                FileStream file = new FileStream(directory + @"\" + asset.Name, FileMode.Create, FileAccess.Write, FileShare.Write);
                file.Write(decompress(buffer), 0, (int)asset.UnzippedLength);
                file.Close();
            }

            fileStream.Close();

            return true;
        }

        public Boolean ExtractAssetsByNameToDirectory(IEnumerable<String> names, String directory)
        {
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                return false;
            }

            foreach (String name in names)
            {
                Asset asset = null;

                if (false == assetLookupCache.TryGetValue(name.GetHashCode(), out asset))
                {
                    // could not find file, skip.
                    continue;
                }

                byte[] buffer = new byte[(int)asset.DataLength];

                fileStream.Seek(Convert.ToInt64(asset.Offset) + 8, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, (int)asset.DataLength);

                //TODO, unzip asset and save unzipped version
                FileStream file = new FileStream(directory + @"\" + asset.Name, FileMode.Create, FileAccess.Write, FileShare.Write);
                file.Write(decompress(buffer), 0, (int)asset.UnzippedLength);
                file.Close();
            }

            fileStream.Close();

            return true;
        }

        public Boolean ExtractAssetByNameToDirectory(String name, String directory)
        {
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                return false;
            }

            Asset asset = null;

            if (false == assetLookupCache.TryGetValue(name.GetHashCode(), out asset))
            {
                fileStream.Close();

                return false;
            }

            byte[] buffer = new byte[(int)asset.DataLength];

            fileStream.Seek(Convert.ToInt64(asset.Offset) + 8, SeekOrigin.Begin);
            fileStream.Read(buffer, 0, (int)asset.DataLength);

            FileStream file = new FileStream(directory + @"\" + asset.Name, FileMode.Create, FileAccess.Write, FileShare.Write);
            file.Write(decompress(buffer), 0, (int)asset.UnzippedLength);
            file.Close();

            fileStream.Close();

            return true;
        }

        public MemoryStream CreateAssetMemoryStreamByName(String name)
        {
            Asset asset = null;

            if (name == null) return null;

            if (false == assetLookupCache.TryGetValue(name.GetHashCode(), out asset))
            {
                return null;
            }

            //ExtractAssetByNameToDirectory(name, "E:\\Out"); //rip asset without showing it, for debugging assets in the alternate dmod format

            FileStream file = null;

            try
            {
                file = File.Open(asset.Pack.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                return null;
            }

            byte[] buffer = new byte[asset.DataLength];

            file.Seek(Convert.ToInt64(asset.Offset) + 8, SeekOrigin.Begin);
            file.Read(buffer, 0, (int)asset.DataLength);

            return new MemoryStream(decompress(buffer));
        }

        public Boolean CreateTemporaryFileAndOpen(String name)
        {
            String tempPath = System.IO.Path.GetTempPath();

            if (ExtractAssetByNameToDirectory(name, tempPath))
            {
                Process.Start(tempPath + @"\" + name);

                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
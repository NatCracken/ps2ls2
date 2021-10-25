using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using ps2ls.IO;

namespace ps2ls.Assets.Pack
{
    public class Asset
    {
        static readonly uint[] ZIPPED_FLAGS = new uint[] { 0x01, 0x11 };
        static readonly uint[] UNZIPPED_FLAGS = new uint[] { 0x10, 0x00 };
        public enum Types
        {
            ADR,
            AGR,
            CDT,
            CNK0,
            CNK1,
            CNK2,
            CNK3,
            CRC,
            DDS,
            DMA,
            DME,
            DMV,
            DSK,
            ECO,
            FSB,
            FXO,
            GFX,
            LST,
            NSA,
            TXT,
            XML,
            ZONE,
            Unknown
        };

        private Asset(Pack pack)
        {
            Pack = pack;
            Name = String.Empty;
            NameHash = 0;
            Offset = 0;
            DataLength = 0;
            isZipped = false;
            dataHash = 0;
            UnzippedLength = 0;
            Type = Types.Unknown;
        }

        static Asset()
        {
            createTypeImages();
        }

        public static Asset LoadBinary(Pack pack, Stream stream, Dictionary<ulong, string> nameDict)
        {
            BinaryReader BinaryReaderLE = new BinaryReader(stream);
            BinaryReaderBigEndian BinaryReaderBE = new BinaryReaderBigEndian(stream);

            Asset asset = new Asset(pack);

            //read map to find metadata
            asset.NameHash = BinaryReaderLE.ReadUInt64();
            asset.Offset = BinaryReaderLE.ReadUInt64();
            asset.DataLength = BinaryReaderLE.ReadUInt64();
            uint zippedflag = BinaryReaderLE.ReadUInt32();
            asset.isZipped = testZipped(zippedflag) && asset.DataLength > 0;
            asset.dataHash = BinaryReaderLE.ReadUInt32();
            asset.UnzippedLength = 0;
            if (asset.isZipped)
            {
                long pos = stream.Position;
                stream.Seek(Convert.ToInt64(asset.Offset), SeekOrigin.Begin);
                uint zipMagic = BinaryReaderLE.ReadUInt32();
                //TODO check magic matches a1b2c3d4 header
                asset.UnzippedLength = BinaryReaderBE.ReadUInt32();
                stream.Seek(pos, SeekOrigin.Begin);
            }

            //TODO: search lookup for my name

            if (nameDict.ContainsKey(asset.NameHash))
            {
                asset.Name = nameDict[asset.NameHash];
            } else
            {
                asset.Name = asset.NameHash + ".unknown";
            }

            // Set the type of the asset based on the extension
            {
                // First get the extension without the leading '.'
                string extension = System.IO.Path.GetExtension(asset.Name).Substring(1);
                try
                {
                    asset.Type = (Asset.Types)Enum.Parse(typeof(Types), extension, true);
                }
                catch (System.ArgumentException exception)
                {
                    // This extension isn't mapped in the enum
                    System.Diagnostics.Debug.Write(exception.ToString());
                    asset.Type = Types.Unknown;
                }
            }

            return asset;
        }

        private static bool testZipped(uint flag)
        {
            foreach (uint zipped in ZIPPED_FLAGS) if (flag == zipped) return true;
            return false;
        }

        public override string ToString()
        {
            return Name;
        }

        private static Dictionary<Types, System.Drawing.Image> typeImages;

        public static System.Drawing.Image GetImageFromType(Asset.Types type)
        {
            return typeImages[type];
        }

        private static void createTypeImages()
        {
            if (typeImages != null)
                return;

            typeImages = new Dictionary<Types, System.Drawing.Image>();

            foreach (Types type in Enum.GetValues(typeof(Types)))
            {
                switch (type)
                {
                    case Asset.Types.DME:
                        typeImages[type] = Properties.Resources.tree;
                        break;
                    case Asset.Types.DDS:
                        typeImages[type] = Properties.Resources.image;
                        break;
                    case Asset.Types.TXT:
                        typeImages[type] = Properties.Resources.document_tex;
                        break;
                    case Asset.Types.XML:
                        typeImages[type] = Properties.Resources.document_xaml;
                        break;
                    case Asset.Types.FSB:
                        typeImages[type] = Properties.Resources.music;
                        break;
                    default:
                        typeImages[type] = Properties.Resources.question;
                        break;
                }
            }
        }

        [BrowsableAttribute(false)]
        public Pack Pack { get; private set; }
        public String Name { get; private set; }
        public String Path { get; private set; }
        public ulong NameHash { get; private set; }
        public ulong Offset { get; private set; }
        public ulong DataLength { get; private set; }
        public uint UnzippedLength { get; private set; }
        public bool isZipped { get; private set; }
        public uint dataHash { get; private set; }
        public UInt32 Crc32 { get; private set; }


        public Asset.Types Type { get; private set; }

        public class NameComparer : Comparer<Asset>
        {
            public override int Compare(Asset x, Asset y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }
        public class SizeComparer : Comparer<Asset>
        {
            public override int Compare(Asset x, Asset y)
            {
                if (x.DataLength > y.DataLength)
                    return -1;
                if (x.DataLength < y.DataLength)
                    return 1;
                else
                    return 0;
            }
        }
    }
}

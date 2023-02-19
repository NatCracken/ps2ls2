using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using ps2ls.IO;

namespace ps2ls.Assets
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
            PNG,
            JPG,
            MRN,
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
            CreateTypeImages();
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
            asset.isZipped = TestZipped(zippedflag) && asset.DataLength > 0;
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

            if (nameDict.ContainsKey(asset.NameHash))
            {
                asset.Name = nameDict[asset.NameHash];
            } else
            {
                asset.Name = asset.NameHash + ".unknown";
            }

            // Set the type of the asset based on the extension

            // First, check for an extension. Some pack file names don't,
            // have one, such as {NAMELIST}
            string extension = System.IO.Path.GetExtension(asset.Name);
            if (string.IsNullOrEmpty(extension))
            {
                asset.Type = Types.Unknown;
            }
            else
            {
                // Remove the leading '.' and normalise any names that have
                // alternative spellings
                extension = extension.Substring(1);
                if (extension.Equals("jpeg"))
                    extension = "jpg";

                asset.Type = Enum.TryParse(extension, true, out Types parsedType)
                    ? parsedType
                    : Types.Unknown;
            }

            return asset;
        }

        public static bool TestZipped(uint flag)
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

        private static void CreateTypeImages()
        {
            if (typeImages != null)
                return;

            typeImages = new Dictionary<Types, System.Drawing.Image>();

            foreach (Types type in Enum.GetValues(typeof(Types)))
            {
                switch (type)
                {
                    case Types.MRN:
                    case Types.DME:
                        typeImages[type] = Properties.Resources.tree;
                        break;
                    case Types.DDS:
                    case Types.PNG:
                    case Types.JPG:
                        typeImages[type] = Properties.Resources.image;
                        break;
                    case Types.TXT:
                        typeImages[type] = Properties.Resources.document_tex;
                        break;
                    case Types.XML:
                        typeImages[type] = Properties.Resources.document_xaml;
                        break;
                    case Types.FSB:
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

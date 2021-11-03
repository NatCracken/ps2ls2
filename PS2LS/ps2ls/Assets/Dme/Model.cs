using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using OpenTK;
using ps2ls.Assets.Dma;
using ps2ls.Graphics.Materials;
using ps2ls.Cryptography;

namespace ps2ls.Assets.Dme
{
    public class Model
    {
        public uint Version { get; private set; }
        public string Name { get; private set; }
        public uint Unknown0 { get; private set; }
        public uint Unknown1 { get; private set; }
        public uint Unknown2 { get; private set; }
        private Vector3 min;
        public Vector3 Min { get { return min; } }
        private Vector3 max;
        public Vector3 Max { get { return max; } }
        public List<Material> Materials { get; private set; }
        public Mesh[] Meshes { get; private set; }
        public List<string> TextureStrings { get; private set; }
        public BoneMap[] BoneMaps { get; private set; }

        #region Attributes
        public UInt32 VertexCount
        {
            get
            {
                UInt32 vertexCount = 0;

                for (Int32 i = 0; i < Meshes.Length; ++i)
                {
                    vertexCount += Meshes[i].VertexCount;
                }

                return vertexCount;
            }
        }
        public UInt32 IndexCount
        {
            get
            {
                UInt32 indexCount = 0;

                for (Int32 i = 0; i < Meshes.Length; ++i)
                {
                    indexCount += Meshes[i].IndexCount;
                }

                return indexCount;
            }
        }
        #endregion

        public static Model LoadFromStream(String name, Stream stream)
        {

            BinaryReader binaryReader = new BinaryReader(stream);

            //header
            byte[] magic = binaryReader.ReadBytes(4);

            if (magic[0] != 'D' ||
                magic[1] != 'M' ||
                magic[2] != 'O' ||
                magic[3] != 'D')
            {
                return null;
            }
            Model model = new Model();
            model.Name = name;
            model.Version = binaryReader.ReadUInt32();

            if (model.Version != 4)
            {
                Console.WriteLine(name + " is an unsupported dmod file. v." + model.Version);
                return null;
            }

            //materials
            uint dmatLength = binaryReader.ReadUInt32();
            byte[] dmatData = binaryReader.ReadBytes(Convert.ToInt32(dmatLength));
            List<string> texStringList = new List<string>();
            List<Material> matList = new List<Material>();
            Dma.Dma.LoadFromStream(dmatData, ref texStringList, ref matList);
            model.TextureStrings = texStringList;
            model.Materials = matList;

            //bounding box
            model.min.X = binaryReader.ReadSingle();
            model.min.Y = binaryReader.ReadSingle();
            model.min.Z = binaryReader.ReadSingle();

            model.max.X = binaryReader.ReadSingle();
            model.max.Y = binaryReader.ReadSingle();
            model.max.Z = binaryReader.ReadSingle();

            //meshes
            uint meshCount = binaryReader.ReadUInt32();

            model.Meshes = new Mesh[meshCount];

            for (int i = 0; i < meshCount; ++i)
            {
                Mesh mesh = Mesh.LoadFromStream(binaryReader.BaseStream, model.Materials);

                if (mesh != null) model.Meshes[i] = mesh;
            }

            //bone maps
            uint boneMapCount = binaryReader.ReadUInt32();

            model.BoneMaps = new BoneMap[boneMapCount];

            for (int i = 0; i < boneMapCount; ++i)
            {
                BoneMap boneMap = BoneMap.LoadFromStream(binaryReader.BaseStream);

                if (boneMap != null)
                {
                    model.BoneMaps[i] = boneMap;
                }
            }

            //bone map entries
            uint boneMapEntryCount = binaryReader.ReadUInt32();

            BoneMapEntry[] boneMapEntries = new BoneMapEntry[boneMapEntryCount];

            for (int i = 0; i < boneMapEntryCount; ++i)
            {
                BoneMapEntry boneMapEntry = BoneMapEntry.LoadFromStream(binaryReader.BaseStream);

                boneMapEntries[i] = boneMapEntry;
            }

            return model;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using OpenTK;
using ps2ls.Graphics.Materials;
using ps2ls.Cryptography;

namespace ps2ls.Assets
{
    public class Model
    {
        public string name { get; private set; }
        public uint version { get; private set; }
        public Dma dma;
        public uint unknown0 { get; private set; }
        public uint unknown1 { get; private set; }
        public uint unknown2 { get; private set; }
        public Vector3 min { get; private set; }
        public Vector3 max { get; private set; }
        public Mesh[] meshes { get; private set; }
        public uint boneDrawCallCount { get; private set; }
        public BoneDrawCall[] boneDrawCalls { get; private set; }
        public uint boneMapEntryCount { get; private set; }
        public BoneMapEntry[] boneMapEntries { get; private set; }
        public Dictionary<int, int> boneMap1 = new Dictionary<int, int>();
        public Dictionary<int, int> boneMap2 = new Dictionary<int, int>();
        public uint boneCount { get; private set; }
        public Bone[] bones { get; private set; }
        public uint vertexCount;
        public uint indexCount;
        public readonly bool isValid;
        public Model(string getName, Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            //header
            byte[] magic = binaryReader.ReadBytes(4);

            if (magic[0] != 'D' ||
                magic[1] != 'M' ||
                magic[2] != 'O' ||
                magic[3] != 'D')
            {

                Console.WriteLine("Magic Missmatch:" + Encoding.UTF8.GetString(magic));
                return;
            }

            name = getName;

#if DEBUG
            Console.WriteLine("~~~~~~~~Model~~~~~~~");
            Console.WriteLine(getName);
#endif
            version = binaryReader.ReadUInt32();

            if (version != 4)
            {
                Console.WriteLine(name + " is an unsupported dmod file. v." + version);
                return;
            }

            //materials
            dma = new Dma(stream);
            if (!dma.isValid) return;

            //bounding box
            min = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            max = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());

            //meshes
            uint meshCount = binaryReader.ReadUInt32();

            meshes = new Mesh[meshCount];
            vertexCount = 0;
            indexCount = 0;
            for (int i = 0; i < meshCount; i++)
            {
                meshes[i] = new Mesh(binaryReader.BaseStream);
                vertexCount += meshes[i].vertexCount;
                indexCount += meshes[i].indexCount;
            }

            //bone maps
            boneDrawCallCount = binaryReader.ReadUInt32();
            boneDrawCalls = new BoneDrawCall[boneDrawCallCount];

            for (int i = 0; i < boneDrawCallCount; i++)
            {
                boneDrawCalls[i] = new BoneDrawCall(binaryReader.BaseStream);
            }


            //bone map entries
            boneMapEntryCount = binaryReader.ReadUInt32();
            boneMapEntries = new BoneMapEntry[boneMapEntryCount];

            for (int i = 0; i < boneMapEntryCount; ++i)
            {
                boneMapEntries[i] = new BoneMapEntry(binaryReader.BaseStream);
                if (boneMap1.ContainsKey(boneMapEntries[i].globalIndex))
                {
                    boneMap1.Add(boneMapEntries[i].globalIndex + 64, boneMapEntries[i].boneIndex);
                    boneMap2.Add(boneMapEntries[i].globalIndex, boneMapEntries[i].boneIndex);
                }
                else
                {
                    boneMap1.Add(boneMapEntries[i].globalIndex, boneMapEntries[i].boneIndex);
                }
            }

            //return;
            boneCount = binaryReader.ReadUInt32();
            bones = new Bone[boneCount];
            for (int i = 0; i < boneCount; i++)
            {
                float[] matrix = new float[12];
                for (int j = 0; j < 12; j++) matrix[j] = binaryReader.ReadSingle();

                bones[i] = new Bone()
                {
                    inverseBindPose = new Matrix4(
                        new Vector4(matrix[0], matrix[1], matrix[2], 0),
                        new Vector4(matrix[3], matrix[4], matrix[5], 0),
                        new Vector4(matrix[6], matrix[7], matrix[8], 0),
                        new Vector4(matrix[9], matrix[10], matrix[11], 1))
                };
            }

            for (int i = 0; i < boneCount; i++)
            {
                bones[i].min = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                bones[i].max = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            }

            for (int i = 0; i < boneCount; i++)
            {
                bones[i].nameHash = binaryReader.ReadUInt32();
            }

            isValid = true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;

namespace ps2ls.Assets
{
    public struct BoneMapEntry
    {
        public ushort boneIndex;
        public ushort globalIndex;

        public BoneMapEntry(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);
            boneIndex = binaryReader.ReadUInt16();
            globalIndex = binaryReader.ReadUInt16();
        }
    }

    public struct Bone
    {
        public string name;
        public Matrix4 inverseBindPose;
        public Vector3 min;
        public Vector3 max;
        public ulong nameHash;
    }

    public struct BoneDrawCall
    {
        public uint Unknown0 { get; private set; }
        public uint BoneStart { get; private set; }
        public uint BoneCount { get; private set; }
        public uint Delta { get; private set; }
        public uint Unknown1 { get; private set; }
        public uint VertexOffset { get; private set; }
        public uint VertexCount { get; private set; }
        public uint IndexOffset { get; private set; }
        public uint IndexCount { get; private set; }

        public BoneDrawCall(Stream stream)
        {

            BinaryReader binaryReader = new BinaryReader(stream);
            Unknown0 = binaryReader.ReadUInt32();//max value
            BoneStart = binaryReader.ReadUInt32();
            BoneCount = binaryReader.ReadUInt32();
            Delta = binaryReader.ReadUInt32();
            Unknown1 = binaryReader.ReadUInt32();//seems to match boneCount for first draw call?
            VertexOffset = binaryReader.ReadUInt32();
            VertexCount = binaryReader.ReadUInt32();
            IndexOffset = binaryReader.ReadUInt32();
            IndexCount = binaryReader.ReadUInt32();
        }
    }
}

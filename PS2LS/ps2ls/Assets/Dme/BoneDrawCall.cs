using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ps2ls.Assets.Dme
{
    public struct BoneMapEntry
    {
        public ushort BoneIndex;
        public ushort GlobalIndex;

        public static BoneMapEntry LoadFromStream(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            return new BoneMapEntry
            {
                BoneIndex = binaryReader.ReadUInt16(),
                GlobalIndex = binaryReader.ReadUInt16(),
            };
        }
    }

    public class BoneDrawCall
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

        private BoneDrawCall()
        {
        }

        public static BoneDrawCall LoadFromStream(Stream stream)
        {
            if (stream == null)
                return null;

            BinaryReader binaryReader = new BinaryReader(stream);

            return new BoneDrawCall
            {
                Unknown0 = binaryReader.ReadUInt32(),//max value
                BoneStart = binaryReader.ReadUInt32(),
                BoneCount = binaryReader.ReadUInt32(),
                Delta = binaryReader.ReadUInt32(),
                Unknown1 = binaryReader.ReadUInt32(),//seems to match boneCount for first draw call?
                VertexOffset = binaryReader.ReadUInt32(),
                VertexCount = binaryReader.ReadUInt32(),
                IndexOffset = binaryReader.ReadUInt32(),
                IndexCount = binaryReader.ReadUInt32(),
            };
        }
    }
}

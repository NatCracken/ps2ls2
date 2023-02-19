using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ps2ls.Cryptography;
using OpenTK;
using ps2ls.Graphics.Materials;

namespace ps2ls.Assets
{
    public class Mesh
    {
        public class VertexStream
        {
            public VertexStream(int getBytesPerVertex, byte[] getBytes)
            {
                bytesPerVertex = getBytesPerVertex;
                data = getBytes;
            }
            public int bytesPerVertex { get; private set; }//if 12, 3 floats. If 6, 3 halves. If 8, 2 floats. If 4, 2 halves (maybe)
            public byte[] data { get; private set; }

            public byte[] GetSlice(int offset, int legnth)
            {
                byte[] toReturn = new byte[legnth];
                Buffer.BlockCopy(data, offset, toReturn, 0, legnth);
                return toReturn;
            }
        }


        public uint drawCallOffset { get; private set; }
        public uint drawCallCount { get; private set; }
        public uint boneTransformCount { get; private set; }
        public uint unknown3 { get; private set; }
        public uint vertexStreamCount { get; private set; }
        public VertexStream[] vertexStreams { get; private set; }
        public uint indexSize { get; private set; }
        public uint indexCount { get; private set; }
        public uint vertexCount { get; private set; }
        public byte[] indexData { get; private set; }
        public int indexByteLength { get; private set; }
        public Mesh(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);
            drawCallOffset = binaryReader.ReadUInt32();
            drawCallCount = binaryReader.ReadUInt32();
            boneTransformCount = binaryReader.ReadUInt32();
            unknown3 = binaryReader.ReadUInt32();//is usually max value
            vertexStreamCount = binaryReader.ReadUInt32();
            indexSize = binaryReader.ReadUInt16();//byte length of each index (2 if ushort, 4 if uint)
            binaryReader.ReadUInt16();//On new models this is way to large. just read it as 2 ushorts to mask out the offending bytes
            indexCount = binaryReader.ReadUInt32();
            vertexCount = binaryReader.ReadUInt32();

            vertexStreams = new VertexStream[vertexStreamCount];

            // read vertex streams
            for (int j = 0; j < vertexStreamCount; ++j)
            {
                int bytesPerVertex = Convert.ToInt32(binaryReader.ReadUInt32());//is usually 12 for positions (3 floats - 4 bytes each), but doesn't have to be
                VertexStream vertexStream = new VertexStream(bytesPerVertex, binaryReader.ReadBytes(bytesPerVertex * Convert.ToInt32(vertexCount)));
                vertexStreams[j] = vertexStream;
            }

            // read indices
            indexByteLength = Convert.ToInt32(indexSize) * Convert.ToInt32(indexCount);
            indexData = binaryReader.ReadBytes(indexByteLength);
        }
    }
}

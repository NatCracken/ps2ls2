using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ps2ls.Cryptography;
using OpenTK;

namespace ps2ls.Assets.Dme
{
    public class Mesh
    {
        public class VertexStream
        {
            public VertexStream(int getBytesPerVertex, byte[] getBytes)
            {
                BytesPerVertex = getBytesPerVertex;
                Data = getBytes;
            }
            public int BytesPerVertex { get; private set; }//if 12, 3 floats. if 6, 3 halves
            public byte[] Data { get; private set; }
        }

        public VertexStream[] VertexStreams { get; private set; }
        public byte[] IndexData { get; private set; }

        public uint drawCallOffset { get; set; }
        public uint drawCallCount { get; set; }
        public uint boneTransformCount { get; set; }
        public uint Unknown3 { get; set; }
        public uint Unknown4 { get; set; }
        public uint VertexCount { get; set; }
        public uint IndexCount { get; private set; }
        public uint IndexSize { get; private set; }

        public string AssignedTexture
        {
            get;
            set;
        }

        private Mesh()
        {
            AssignedTexture = "grey.dds";
        }

        public static Mesh LoadFromStream(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            Mesh mesh = new Mesh();

            uint vertexStreamCount;

            //Console.WriteLine("~~~~~Mesh Properties~~~~");
            //Console.WriteLine("Pos: " + binaryReader.BaseStream.Position);

            mesh.drawCallOffset = binaryReader.ReadUInt32();
            mesh.drawCallCount = binaryReader.ReadUInt32();
            mesh.boneTransformCount = binaryReader.ReadUInt32();
            mesh.Unknown3 = binaryReader.ReadUInt32();//is usually max value
            vertexStreamCount = binaryReader.ReadUInt32();
            mesh.IndexSize = binaryReader.ReadUInt16();//byte length of each index (2 if ushort, 4 if uint)
            binaryReader.ReadUInt16();//On new models this is way to large. just read it as 2 ushorts to mask out the offending bytes
            mesh.IndexCount = binaryReader.ReadUInt32();
            mesh.VertexCount = binaryReader.ReadUInt32();

            /*Console.WriteLine("Offset: " + mesh.drawCallOffset);
            Console.WriteLine("DCount: " + mesh.drawCallCount);
            Console.WriteLine("BCount: " + mesh.boneTransformCount);
            Console.WriteLine("FFFF: " + mesh.Unknown3);
            Console.WriteLine("vStream: " + vertexStreamCount);
            Console.WriteLine("iSize: " + mesh.IndexSize);
            Console.WriteLine("iCount: " + mesh.IndexCount);
            Console.WriteLine("vCtount: " + mesh.VertexCount);*/

            mesh.VertexStreams = new VertexStream[vertexStreamCount];

            // read vertex streams
            for (int j = 0; j < vertexStreamCount; ++j)
            {
                int bytesPerVertex = Convert.ToInt32(binaryReader.ReadUInt32());//is usually 12, but doesn't have to be (3 floats - 4 bytes each)
                VertexStream vertexStream = new VertexStream(bytesPerVertex, binaryReader.ReadBytes(bytesPerVertex * Convert.ToInt32(mesh.VertexCount)));
                mesh.VertexStreams[j] = vertexStream;
            }

            // read indices
            mesh.IndexData = binaryReader.ReadBytes(Convert.ToInt32(mesh.IndexSize) * Convert.ToInt32(mesh.IndexCount));

            return mesh;
        }

    }
}

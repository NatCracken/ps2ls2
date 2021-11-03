using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ps2ls.Graphics.Materials;
using ps2ls.Cryptography;
using OpenTK;

namespace ps2ls.Assets.Dme
{
    public class Mesh
    {
        public class VertexStream
        {
            public static VertexStream LoadFromStream(Stream stream, Int32 bytesPerVertex, Int32 vertexCount)
            {
                VertexStream vertexStream = new VertexStream();

                vertexStream.BytesPerVertex = bytesPerVertex;

                BinaryReader binaryReader = new BinaryReader(stream);

                vertexStream.Data = binaryReader.ReadBytes(vertexCount * bytesPerVertex);

                return vertexStream;
            }

            public int BytesPerVertex { get; private set; }//if 12, 3 floats. if 6, 3 halves
            public Byte[] Data { get; private set; }
        }

        public VertexStream[] VertexStreams { get; private set; }
        public Byte[] IndexData { get; private set; }

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

        public static Mesh LoadFromStream(Stream stream, ICollection<Dma.Material> materials)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            Mesh mesh = new Mesh();

            uint bytesPerVertex;
            uint vertexStreamCount;

            Console.WriteLine("~~~~~Mesh Properties~~~~");
            Console.WriteLine("Pos: " + binaryReader.BaseStream.Position);

            mesh.drawCallOffset = binaryReader.ReadUInt32();
            mesh.drawCallCount = binaryReader.ReadUInt32();
            mesh.boneTransformCount = binaryReader.ReadUInt32();
            mesh.Unknown3 = binaryReader.ReadUInt32();//is usually max value
            vertexStreamCount = binaryReader.ReadUInt32();
            mesh.IndexSize = binaryReader.ReadUInt16();//byte length of each index (2 if half, 4 if float, usually 2)
            binaryReader.ReadUInt16();//On new models this is way to large. just read it as 2 uints to mask out the offending bytes
            mesh.IndexCount = binaryReader.ReadUInt32();
            mesh.VertexCount = binaryReader.ReadUInt32();

            Console.WriteLine("Offset: " + mesh.drawCallOffset);
            Console.WriteLine("DCount: " + mesh.drawCallCount);
            Console.WriteLine("BCount: " + mesh.boneTransformCount);
            Console.WriteLine("FFFF: " + mesh.Unknown3);
            Console.WriteLine("vStream: " + vertexStreamCount);
            Console.WriteLine("iSize: " + mesh.IndexSize);
            Console.WriteLine("iCount: " + mesh.IndexCount);
            Console.WriteLine("vCtount: " + mesh.VertexCount);

            mesh.VertexStreams = new VertexStream[vertexStreamCount];

            // read vertex streams
            for (Int32 j = 0; j < vertexStreamCount; ++j)
            {
                bytesPerVertex = binaryReader.ReadUInt32();//is usually 12 (3 floats - 4 bytes each)

                VertexStream vertexStream = VertexStream.LoadFromStream(binaryReader.BaseStream, Convert.ToInt32(bytesPerVertex), Convert.ToInt32(mesh.VertexCount));

                if (vertexStream != null)
                {
                    mesh.VertexStreams[j] = vertexStream;
                }
            }

            // read indices
            mesh.IndexData = binaryReader.ReadBytes(Convert.ToInt32(mesh.IndexSize) * Convert.ToInt32(mesh.IndexCount));

            return mesh;
        }

    }
}

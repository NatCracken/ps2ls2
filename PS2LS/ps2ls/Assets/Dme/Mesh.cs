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
            public static VertexStream LoadFromStream(Stream stream, Int32 vertexCount, Int32 bytesPerVertex)
            {
                VertexStream vertexStream = new VertexStream();

                vertexStream.BytesPerVertex = bytesPerVertex;

                BinaryReader binaryReader = new BinaryReader(stream);

                vertexStream.Data = binaryReader.ReadBytes(vertexCount * bytesPerVertex);

                return vertexStream;
            }

            public int BytesPerVertex { get; private set; }
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

            UInt32 bytesPerVertex = 0;
            UInt32 vertexStreamCount = 0;

            mesh.drawCallOffset = binaryReader.ReadUInt32();
            mesh.drawCallCount = binaryReader.ReadUInt32();
            mesh.boneTransformCount = binaryReader.ReadUInt32();
            mesh.Unknown3 = binaryReader.ReadUInt32();//is usually max value
            vertexStreamCount = binaryReader.ReadUInt32();
            mesh.IndexSize = binaryReader.ReadUInt32();//byte length of each index (should always be 2)
            mesh.IndexCount = binaryReader.ReadUInt32();
            mesh.VertexCount = binaryReader.ReadUInt32();

           /* Console.WriteLine("~~~~~Mesh Properties~~~~");
            Console.WriteLine("I: " + mesh.MaterialIndex);
            Console.WriteLine("u: " + mesh.Unknown1);
            Console.WriteLine("u: " + mesh.Unknown2);
            Console.WriteLine("u: " + mesh.Unknown3);
            Console.WriteLine("vStream: " + vertexStreamCount);
            Console.WriteLine("iSize: " + mesh.IndexSize);//on new models this is waaaay to large. perhaps its a ushort?
            Console.WriteLine("iCount: " + mesh.IndexCount);
            Console.WriteLine("vCtount: " + mesh.VertexCount);*/

            mesh.VertexStreams = new VertexStream[vertexStreamCount];

            // read vertex streams
            for (Int32 j = 0; j < vertexStreamCount; ++j)
            {
                bytesPerVertex = binaryReader.ReadUInt32();

                VertexStream vertexStream = VertexStream.LoadFromStream(binaryReader.BaseStream, Convert.ToInt32(mesh.VertexCount), Convert.ToInt32(bytesPerVertex));

                if (vertexStream != null)
                {
                    mesh.VertexStreams[j] = vertexStream;
                }
            }

            // read indices
            mesh.IndexData = binaryReader.ReadBytes(Convert.ToInt32(mesh.IndexCount) * Convert.ToInt32(mesh.IndexSize));

            return mesh;
        }

    }
}

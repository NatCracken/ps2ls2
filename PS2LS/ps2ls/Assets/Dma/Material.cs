using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ps2ls.Graphics.Materials;

namespace ps2ls.Assets.Dma
{
    public class Material
    {
        public uint NameHash { get; private set; }
        public uint DataLength { get; private set; }
        public uint MaterialDefinitionHash { get; private set; }
        public string DefinitionName { get; private set; }
        public List<Parameter> Parameters { get; private set; }

        public Material(uint getName, uint getDataLength)
        {
            NameHash = getName;
            DataLength = getDataLength;
        }

        public void ParseFromBlock(byte[] matBlock)
        {
            MemoryStream stream = new MemoryStream(matBlock);
            BinaryReader binaryReader = new BinaryReader(stream);

            MaterialDefinitionHash = binaryReader.ReadUInt32();

            if (MaterialDefinitionManager.Instance.hasMaterialHash(MaterialDefinitionHash))
            {
                DefinitionName = MaterialDefinitionManager.Instance.MaterialDefinitions[MaterialDefinitionHash].Name;
            }

            uint parameterCount = binaryReader.ReadUInt32();
            Parameters = new List<Parameter>(Convert.ToInt32(parameterCount));

            for (uint j = 0; j < parameterCount; ++j) Parameters.Add(Parameter.LoadFromStream(stream));

            binaryReader.Dispose();
            stream.Dispose();
        }

        public class Parameter
        {
            //http://msdn.microsoft.com/en-us/library/windows/desktop/bb205378(v=vs.85).aspx
            public enum D3DXParameterClass
            {
                Scalar = 0,
                Vector,
                MatrixRows,
                MatrixColumns,
                Object,
                Struct,
                ForceDword = 0x7fffffff
            }

            //http://msdn.microsoft.com/en-us/library/windows/desktop/bb205380(v=vs.85).aspx
            public enum D3DXParameterType
            {
                Void = 0,
                Bool,
                Int,
                Float,
                String,
                Texture,
                Texture1D,
                Texture2D,
                Texture3D,
                TextureCube,
                Sampler,
                Sampler1D,
                Sampler2D,
                Sampler3D,
                SamplerCube,
                PixelShader,
                VertexShader,
                PixelFragment,
                VertexFrament,
                Unsupported,
                ForceDword = 0x7fffffff
            }

            private Parameter()
            {
            }

            public object parseData(out Type type)
            {
                switch (Type)
                {
                    case D3DXParameterType.Void:
                        type = null;
                        return null;
                    case D3DXParameterType.Bool:
                        type = typeof(bool);
                        return BitConverter.ToBoolean(Data, 0);
                    case D3DXParameterType.Int:
                    case D3DXParameterType.Texture:
                        type = typeof(uint);
                        return BitConverter.ToUInt32(Data, 0);
                    case D3DXParameterType.Float:
                        type = typeof(float);
                        return BitConverter.ToSingle(Data, 0);
                    case D3DXParameterType.String:
                        type = typeof(string);
                        return BitConverter.ToString(Data, 0);
                    default:
                        //Console.WriteLine("Unhandled parameter type:" + Type);
                        type = null;
                        return null;
                }
            }

            public static Parameter LoadFromStream(Stream stream)
            {
                Parameter parameter = new Parameter();

                BinaryReader binaryReader = new BinaryReader(stream);

                parameter.NameHash = binaryReader.ReadUInt32();
                /*if (MaterialDefinitionManager.Instance.hasParameterHash(parameter.NameHash))
                {
                    parameter.DefinitionName = MaterialDefinitionManager.Instance.ParameterDefinitions[parameter.NameHash].Name;
                }*/
                parameter.Class = (D3DXParameterClass)binaryReader.ReadUInt32();
                parameter.Type = (D3DXParameterType)binaryReader.ReadUInt32();

                uint dataLength = binaryReader.ReadUInt32();

                parameter.Data = binaryReader.ReadBytes(Convert.ToInt32(dataLength));

                return parameter;
            }

            public uint NameHash { get; private set; }
            public string DefinitionName { get; private set; }
            public D3DXParameterClass Class { get; private set; }
            public D3DXParameterType Type { get; private set; }
            public byte[] Data { get; private set; }
        }
    }
}

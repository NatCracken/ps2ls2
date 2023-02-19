using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ps2ls.Graphics.Materials;

namespace ps2ls.Assets
{
    public class Material
    {
        public uint NameHash { get; private set; }
        public uint DataLength { get; private set; }
        public uint MaterialDefinitionHash { get; private set; }
        public string DefinitionName { get; private set; }
        public List<Parameter> Parameters { get; private set; }
        public Material(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            NameHash = binaryReader.ReadUInt32();
            DataLength = binaryReader.ReadUInt32();
            MaterialDefinitionHash = binaryReader.ReadUInt32();
            uint parameterCount = binaryReader.ReadUInt32();

            if (MaterialDefinitionManager.Instance.hasMaterialHash(MaterialDefinitionHash))
            {
                DefinitionName = MaterialDefinitionManager.Instance.MaterialDefinitions[MaterialDefinitionHash].Name;
            }

            Parameters = new List<Parameter>(Convert.ToInt32(parameterCount));
            for (uint j = 0; j < parameterCount; ++j) Parameters.Add(Parameter.LoadFromStream(stream));
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

            public static Parameter LoadFromStream(Stream stream)
            {
                Parameter parameter = new Parameter();
                BinaryReader binaryReader = new BinaryReader(stream);

                parameter.NameHash = binaryReader.ReadUInt32();
                parameter.Class = (D3DXParameterClass)binaryReader.ReadUInt32();
                parameter.Type = (D3DXParameterType)binaryReader.ReadUInt32();
                parameter.dataLength = binaryReader.ReadUInt32();
                parameter.Data = binaryReader.ReadBytes(Convert.ToInt32(parameter.dataLength));
                return parameter;
            }

            public uint NameHash { get; private set; }
            public D3DXParameterClass Class { get; private set; }
            public D3DXParameterType Type { get; private set; }
            public uint dataLength;
            public byte[] Data { get; private set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ps2ls.Assets.Dma
{
    public static class Dma
    {
        public static void LoadFromStream(byte[] dmatBlock, ref List<string> textures, ref List<Material> materials)
        {
            MemoryStream stream = new MemoryStream(dmatBlock);
            BinaryReader binaryReader = new BinaryReader(stream);

            //header
            char[] magic = binaryReader.ReadChars(4);

            if (magic[0] != 'D' ||
                magic[1] != 'M' ||
                magic[2] != 'A' ||
                magic[3] != 'T')
            {
                return;
            }

            uint version = binaryReader.ReadUInt32();

            //textures
            uint texturesLength = binaryReader.ReadUInt32();
            char[] buffer = binaryReader.ReadChars(Convert.ToInt32(texturesLength));
            int startIndex = 0;

            for (int i = 0; i < buffer.Count(); ++i)
            {
                if (buffer[i] == '\0')
                {
                    int length = i - startIndex;

                    string textureName = new string(buffer, startIndex, length);
                    startIndex = i + 1;

                    textures.Add(textureName);
                }
            }

            //materials
            uint materialCount = binaryReader.ReadUInt32();

            for (int i = 0; i < materialCount; ++i)
            {
                Material mat = new Material(binaryReader.ReadUInt32(), binaryReader.ReadUInt32());//name hash, data length
                byte[] matBlock = binaryReader.ReadBytes(Convert.ToInt32(mat.DataLength));
                mat.ParseFromBlock(matBlock);
                materials.Add(mat);
            }

            binaryReader.Dispose();
            stream.Dispose();
        }
    }
}

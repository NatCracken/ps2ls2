using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace ps2ls.Assets
{
    public class Dma
    {
        public uint length;
        public string[] textureStrings;
        public Material[] materials;
        public readonly bool isValid;
        public Dma(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);
            length = binaryReader.ReadUInt32();
            //header
            byte[] magic = binaryReader.ReadBytes(4);

            if (magic[0] != 'D' ||
                magic[1] != 'M' ||
                magic[2] != 'A' ||
                magic[3] != 'T')
            {

                Console.WriteLine("Magic Missmatch:" + Encoding.UTF8.GetString(magic));
                return;
            }

            uint version = binaryReader.ReadUInt32();

            //textures
            uint filenameLength = binaryReader.ReadUInt32();
            char[] buffer = binaryReader.ReadChars(Convert.ToInt32(filenameLength));
            int startIndex = 0;

            List<string> texList = new List<string>();
            for (int i = 0; i < buffer.Count(); i++)
            {
                if (buffer[i] == '\0')
                {
                    int length = i - startIndex;

                    string textureName = new string(buffer, startIndex, length);
                    startIndex = i + 1;

                    texList.Add(textureName);
                }
            }
            textureStrings = texList.ToArray();

            //materials
            uint materialCount = binaryReader.ReadUInt32();
            materials = new Material[materialCount];
            for (int i = 0; i < materialCount; i++) materials[i] = new Material(stream);

            isValid = true;
        }
    }
}

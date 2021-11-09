using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SDI = System.Drawing.Imaging;
using ps2ls.Assets.Dme;
using ps2ls.Assets.Pack;
using ps2ls.IO;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using SD = System.Drawing;


namespace ps2ls.IO
{
    public static class TextureExporterStatic
    {
        public class TextureFormatInfo
        {
            public string Name { get; internal set; }
            public string Extension { get; internal set; }
            public SDI.ImageFormat ImageFormat { get; internal set; }

            internal TextureFormatInfo()
            {
            }

            public override string ToString()
            {
                return Name + @" (*." + Extension + @")";
            }
        }

        public static TextureFormatInfo[] TextureFormats;

        static TextureExporterStatic()
        {
            createTextureFormats();
        }

        private static void createTextureFormats()
        {
            List<TextureFormatInfo> textureFormats = new List<TextureFormatInfo>();

            //Portable Network Graphics (*.png)
            TextureFormatInfo textureFormat = new TextureFormatInfo();
            textureFormat.Name = "Portable Network Graphics";
            textureFormat.Extension = "png";
            textureFormat.ImageFormat = SDI.ImageFormat.Png;
            textureFormats.Add(textureFormat);

            //Microsoft Windows Bitmap (*.bmp)
            textureFormat = new TextureFormatInfo();
            textureFormat.Name = "Windows Bitmap";
            textureFormat.Extension = "bmp";
            textureFormat.ImageFormat = SDI.ImageFormat.Bmp;
            textureFormats.Add(textureFormat);

            //Tagged Image File Format (*.tiff)
            textureFormat = new TextureFormatInfo();
            textureFormat.Name = "Tagged Image File Format";
            textureFormat.Extension = "tiff";
            textureFormat.ImageFormat = SDI.ImageFormat.Tiff;
            textureFormats.Add(textureFormat);

            //Joint Photographic Experts Group (*.jpeg)
            textureFormat = new TextureFormatInfo();
            textureFormat.Name = "Joint Photographic Experts Group";
            textureFormat.Extension = "jpeg";
            textureFormat.ImageFormat = SDI.ImageFormat.Jpeg;
            textureFormats.Add(textureFormat);

            TextureFormats = textureFormats.ToArray();
        }

        public static bool exportTexture(string textureString, string directory, TextureFormatInfo textureFormat)
        {
            MemoryStream textureMemoryStream = AssetManager.Instance.CreateAssetMemoryStreamByName(textureString);

            if (textureMemoryStream == null)
                return false;

            SD.Image image;
            switch (Path.GetExtension(textureString).ToLower())
            {
                case ".png":
                case ".jpeg":
                case ".jpg":
                    image = TextureManager.CommonStreamToBitmap(textureMemoryStream);
                    break;
                default:
                    image = TextureManager.DDSStreamToBitmap(textureMemoryStream);
                    break;
            }
            

            if (image == null)
                return false;

            image.Save(directory + @"\" + Path.GetFileNameWithoutExtension(textureString) + @"." + textureFormat.Extension, textureFormat.ImageFormat);

            return true;
        }
    }
}

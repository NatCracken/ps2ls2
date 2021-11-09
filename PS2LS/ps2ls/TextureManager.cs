using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using SD = System.Drawing;
using SDI = System.Drawing.Imaging;
using Pfim;

namespace ps2ls
{
    public class TextureManager
    {
        public static int LoadFromStream(Stream stream)
        {
            if (stream == null)
                return 0;

            IImage image = Pfim.Pfim.FromStream(stream);
            IntPtr dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);

            int glTextureHandle = GL.GenTexture();

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, glTextureHandle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, image.Width, image.Height, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, dataPtr);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);

            return glTextureHandle;
        }

        public static SD.Image CommonStreamToBitmap(Stream stream)
        {
            return SD.Image.FromStream(stream);
        }

        public static SD.Image DDSStreamToBitmap(Stream stream)
        {
            IImage image = Pfim.Pfim.FromStream(stream);

            SDI.PixelFormat format;

            // Convert from Pfim's backend agnostic image format into GDI+'s image format
            switch (image.Format)
            {
                case ImageFormat.Rgba32:
                    format = SDI.PixelFormat.Format32bppArgb;
                    break;
                default:
                    // see the sample for more details
                    throw new NotImplementedException();
            }

            IntPtr dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
            SD.Bitmap bitmap = new SD.Bitmap(image.Width, image.Height, image.Stride, format, dataPtr);

            SDI.BitmapData bdata = bitmap.LockBits(new SD.Rectangle(0, 0, image.Width, image.Height), 
                SDI.ImageLockMode.WriteOnly, SDI.PixelFormat.Format32bppArgb);
            bitmap.UnlockBits(bdata);

            image.Dispose();
            
            return bitmap;          

        }
    }
}

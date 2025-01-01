using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace BlackPackageImageTool
{
    public partial class BlackPackageImageTool
    {
        public class Pt1MetaData
        {
            public int Bpp;
            public int Width;
            public int Height;
            public int Type;
            public int PackedSize;
            public int UnpackedSize;
            public int OffsetX;
            public int OffsetY;
        }

        public void ProcessDirectory(string inputDir, string outputDir)
        {
            if (!Directory.Exists(inputDir))
                throw new DirectoryNotFoundException($"Input directory '{inputDir}' does not exist.");

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            foreach (string filePath in Directory.GetFiles(inputDir, "*.pt1")) // Assuming .pt1 as file extension
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string outputPath = Path.Combine(outputDir, fileName + ".png");

                try
                {
                    UnpackImage(filePath, outputPath);
                    Console.WriteLine($"Successfully processed: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {filePath}: {ex.Message}");
                }
            }
        }

        public void UnpackImage(string inputImage, string outputImage)
        {
            using (FileStream fs = new FileStream(inputImage, FileMode.Open))
            using (BinaryReader file = new BinaryReader(fs))
            {
                int type = file.ReadInt32();
                if (type < 0 || type > 3)
                    throw new InvalidDataException("Invalid image type.");

                if (-1 != file.ReadInt32())
                    throw new InvalidDataException("Invalid header format.");

                var pt1metadata = new Pt1MetaData
                {
                    Type = type,
                    OffsetX = file.ReadInt32(),
                    OffsetY = file.ReadInt32(),
                    Width = file.ReadInt32(),
                    Height = file.ReadInt32(),
                    PackedSize = file.ReadInt32(),
                    UnpackedSize = file.ReadInt32(),
                    Bpp = 3 == type ? 32 : 24
                };

                if (pt1metadata.UnpackedSize != pt1metadata.Width * pt1metadata.Height * (pt1metadata.Bpp / 8))
                    throw new InvalidDataException("Unpacked size does not match expected dimensions.");

                var reader = new Reader(file, pt1metadata);
                byte[] pixelData = reader.Unpack(); // Assuming Unpack() returns byte[]

                // Save the image
                SaveImage(pixelData, pt1metadata, outputImage);
            }
        }

        private void SaveImage(byte[] pixelData, Pt1MetaData metaData, string outputPath)
        {
            using (Bitmap bitmap = new Bitmap(metaData.Width, metaData.Height, metaData.Bpp == 32 ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb))
            {
                var rect = new Rectangle(0, 0, metaData.Width, metaData.Height);
                BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

                // Copy pixel data into the bitmap
                System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);

                bitmap.UnlockBits(bmpData);
                bitmap.Save(outputPath, ImageFormat.Png); // Save as PNG
            }
        }
    }
}

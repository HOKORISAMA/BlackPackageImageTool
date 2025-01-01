using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BlackPackageImageTool
{
    public partial class BlackPackageImageTool
    {
        public void ConvertPngToPt1(string inputImage, string outputPt1)
        {
            using (var bitmap = new Bitmap(inputImage))
            {
                // Determine type based on image format
                int type = 0;

                if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    type = 3;
                }
                else if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    type = 2;
                }
                else if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    type = 1;
                }
                else
                {
                    type = 0;
                }
                
                // Get image data
                var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
                var pixelBytes = new byte[Math.Abs(bmpData.Stride) * bitmap.Height];
                Marshal.Copy(bmpData.Scan0, pixelBytes, 0, pixelBytes.Length);
                bitmap.UnlockBits(bmpData);

                // It's set to zero for now as it's the only algorithm working for packing right now.
                type = 0;
                // Create packer and pack data
                var packer = new Packer(pixelBytes, bitmap.Width, bitmap.Height, type);
                byte[] packedData = packer.Pack();

                // Write PT1 file
                using (var writer = new BinaryWriter(File.Create(outputPt1)))
                {
                    // Write header
                    writer.Write(type); //Type
                    writer.Write(-1);
                    writer.Write(0); // OffsetX
                    writer.Write(0); // OffsetY
                    writer.Write(bitmap.Width);
                    writer.Write(bitmap.Height);
                    writer.Write(packedData.Length);
                    writer.Write(pixelBytes.Length);
                    
                    // Write packed data
                    writer.Write(packedData);
                }
            }
        }

        public void ProcessDirectoryToPt1(string inputDir, string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            foreach (string filePath in Directory.GetFiles(inputDir, "*.png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string outputPath = Path.Combine(outputDir, fileName + ".pt1");

                try
                {
                    ConvertPngToPt1(filePath, outputPath);
                    Console.WriteLine($"Successfully converted: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting {filePath}: {ex.Message}");
                }
            }
        }
    }
}

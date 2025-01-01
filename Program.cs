using System;

namespace BlackPackageImageTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: BlackPackageImageTool <mode> <input directory> <output directory>");
                Console.WriteLine("Modes: pt1topng, pngtopt1");
                return;
            }

            string mode = args[0].ToLower();
            string inputDir = args[1];
            string outputDir = args[2];

            try
            {
                var tool = new BlackPackageImageTool();
                switch (mode)
                {
                    case "pt1topng":
                        tool.ProcessDirectory(inputDir, outputDir);
                        break;
                    case "pngtopt1":
                        tool.ProcessDirectoryToPt1(inputDir, outputDir);
                        break;
                    default:
                        Console.WriteLine("Invalid mode. Use pt1topng or pngtopt1");
                        return;
                }
                Console.WriteLine("Processing completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
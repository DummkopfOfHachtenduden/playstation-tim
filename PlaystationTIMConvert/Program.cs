using PlaystationTIM;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PlaystationTIMConvert
{
    public class Program
    {
        private static void Main(string[] args)
        {
            FileInfo inputFile = null;
            DirectoryInfo outputDir = null;
            TimTransparency transparency = TimTransparency.No;

#if DEBUG
            args = new string[]
            {
                "-inputFile=D:\\!TMP\\SAMPLE.TIM",
                "-outputDir=D:\\!TMP",
                "-transparency=black",
            };
#endif
            Console.WriteLine("Usage: -inputFile=filePath.ext [-outputDir=folderPath] [-transparency=no|black|semi|full]");

            foreach (var arg in args)
            {
                var argParams = arg.Split('=');
                switch (argParams[0])
                {
                    case "-inputFile":
                        inputFile = new FileInfo(argParams[1]);
                        break;

                    case "-outputDir":
                        outputDir = new DirectoryInfo(argParams[1]);
                        break;

                    case "-transparency":
                        if (Enum.TryParse(argParams[1], true, out transparency) == false)
                            Console.WriteLine("Warning: Unknown transparency argument in {0}.", arg);
                        break;

                    default:
                        Console.WriteLine("Warning: Unknown argument in {0}", arg);
                        break;
                }
            }

            if (inputFile == null)
            {
                Console.WriteLine("Error: No input file provided.");
                return;
            }

            if (inputFile.Exists == false)
            {
                Console.WriteLine("Error: Input file does not exist.");
                return;
            }

            if (outputDir == null)
                outputDir = inputFile.Directory;

            if (outputDir.Exists == false)
                outputDir.Create();

            using (var stream = inputFile.OpenRead())
            using (var tim = new Tim(stream))
            {
                tim.Transparency = transparency;
                var bitmap = tim.ToBitmapUnsafe();

                var outputName = Regex.Replace(inputFile.Name, ".tim", ".png", RegexOptions.IgnoreCase);
                var outputPath = Path.Combine(outputDir.FullName, outputName);
                bitmap.Save(outputPath);

                Console.WriteLine("Successfully saved as {0}", outputPath);
            }
        }
    }
}
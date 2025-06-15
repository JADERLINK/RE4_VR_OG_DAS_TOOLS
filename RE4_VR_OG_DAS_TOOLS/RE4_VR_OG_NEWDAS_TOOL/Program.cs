using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE4_VR_OG_NEWDAS_TOOL
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("# RE4_VR_OG_NEWDAS_TOOL");
            Console.WriteLine("# By: JADERLINK");
            Console.WriteLine("# youtube.com/@JADERLINK");
            Console.WriteLine("# github.com/JADERLINK");
            Console.WriteLine("# VERSION 1.2.1 (2025-06-15)");

            bool usingBatFile = false;
            int start = 0;
            if (args.Length >= 1 && args[0].ToLowerInvariant() == "-bat")
            {
                usingBatFile = true;
                start = 1;
            }

            var formatsToShowOffsets = FormatsToShowOffsets.Load();

            for (int i = start; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                {
                    try
                    {
                        Continue(args[i], formatsToShowOffsets);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + args[i]);
                        Console.WriteLine(ex);
                    }
                }
                else
                {
                    Console.WriteLine("File specified does not exist: " + args[i]);
                }

            }


            if (args.Length == 0)
            {
                Console.WriteLine("How to use: drag the file to the executable.");
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/RE4_VR_OG_DAS_TOOLS");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Finished!!!");
                if (!usingBatFile)
                {
                    Console.WriteLine("Press any key to close the console.");
                    Console.ReadKey();
                }
            }

        }

        private static void Continue(string file, Dictionary<string, bool> formatsToShowOffsets)
        {
            var fileInfo = new FileInfo(file);
            Console.WriteLine();
            Console.WriteLine("File: " + fileInfo.Name);
            var Extension = fileInfo.Extension.ToUpperInvariant();

            if (Extension == ".DAS")
            {
                Console.WriteLine("Extract Mode!");

                _ = new RE4_VR_OG_NEWDAS_TOOL_EXTRACT.Extract(fileInfo, formatsToShowOffsets);
            }
            else if (Extension == ".IDXRE4VRDAS")
            {
                Console.WriteLine("Repack Mode!");

                _ = new RE4_VR_OG_NEWDAS_TOOL_REPACK.RepackJ(fileInfo);
            }
            else
            {
                Console.WriteLine("The extension is not valid: " + Extension);
            }
        }

    }
}

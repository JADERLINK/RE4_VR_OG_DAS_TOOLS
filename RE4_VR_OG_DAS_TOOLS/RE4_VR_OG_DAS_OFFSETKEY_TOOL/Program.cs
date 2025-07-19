using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE4_VR_OG_DAS_OFFSETKEY_TOOL
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("# RE4_VR_OG_DAS_OFFSETKEY_TOOL");
            Console.WriteLine("# By: JADERLINK");
            Console.WriteLine("# youtube.com/@JADERLINK");
            Console.WriteLine("# github.com/JADERLINK");
            Console.WriteLine("# VERSION 1.2.2 (2025-07-19)");


            bool usingBatFile = false;
            string[] SelectedFormats = null;

            int start = 0;
            for (int i = 0; i < 2 && i < args.Length; i++)
            {
                if (args[i].ToLowerInvariant() == "-bat")
                {
                    usingBatFile = true;
                    start++;
                }
                if (args[i].StartsWith("$"))
                {
                    start++;
                    SelectedFormats = args[i].TrimStart('$').ToUpperInvariant().Split(':');
                }
            }

            for (int i = start; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                {
                    try
                    {
                        Continue(args[i], SelectedFormats);
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

        private static void Continue(string file, string[] SelectedFormats)
        {
            var fileInfo = new FileInfo(file);
            Console.WriteLine();
            Console.WriteLine("File: " + fileInfo.Name);
            var Extension = fileInfo.Extension.ToUpperInvariant();

            if (Extension == ".DAT" || Extension == ".MAP" || Extension == ".UDAS" || Extension == ".DAS")
            {
                FileFormat fileFormat = FileFormat.Null;
                switch (Extension)
                {
                    case ".DAT": fileFormat = FileFormat.DAT; break;
                    case ".MAP": fileFormat = FileFormat.MAP; break;
                    case ".UDAS": fileFormat = FileFormat.UDAS; break;
                    case ".DAS": fileFormat = FileFormat.DAS; break;
                }

                if (fileFormat != FileFormat.Null)
                {
                    _ = new Extract(fileInfo, fileFormat, SelectedFormats);
                }
            }
            else
            {
                Console.WriteLine("The extension is not valid: " + Extension);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_VR_OG_NEWDAS_TOOL
{
    internal static class FormatsToShowOffsets
    {
        public static Dictionary<string, bool> Load() 
        {
            Dictionary<string, bool> res = new Dictionary<string, bool>();
            res.Add(".BIN", true);
            res.Add(".TPL", true);
            res.Add(".EFF", true);

            string filename = "FormatsToShowOffsets.txt";
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            if (File.Exists(filepath))
            {
                try
                {
                    var r = new FileInfo(filepath).OpenText();

                    while (!r.EndOfStream)
                    {
                        string line = r.ReadLine();

                        if (line != null)
                        {
                            line = line.ToUpperInvariant().Trim();

                            if (line.StartsWith("."))
                            {
                                var split = line.Split(new char[] { '=' });
                                if (split.Length >= 2)
                                {
                                    string key = split[0].Trim();
                                    bool value = split[1].Trim().Contains("1") || split[1].Trim().Contains("TRUE");

                                    if (res.ContainsKey(key))
                                    {
                                        res[key] = value;
                                    }
                                    else 
                                    {
                                        res.Add(key,value);
                                    }

                                }
                            }
                        }
                    }

                    r.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading {filename} file: " + ex);
                }
            }
            else 
            {
                Console.WriteLine($"The {filename} file does not exist, it was not loaded into the program!");
            }

            return res;
        }

    }
}

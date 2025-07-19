using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_VR_OG_INSERTDAS_TOOL.IINSERT
{
    internal class GetIdx
    {
        public bool has_UDAS_END = false;
        public string UDAS_END = null;
        public Dictionary<int, (string DatID, string FileName)> Arqs = new Dictionary<int, (string DatID, string FileName)>();
        public bool has_error_on_load = false;

        public GetIdx(FileInfo info)
        {
            StreamReader idxj;

            try
            {
                idxj = info.OpenText();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                has_error_on_load = true;
                return;
            }

            while (!idxj.EndOfStream)
            {
                string line = idxj.ReadLine()?.Trim();

                if (!(string.IsNullOrEmpty(line)
                  || line.StartsWith("#")
                  || line.StartsWith("\\")
                  || line.StartsWith("/")
                  || line.StartsWith(":")
                  || line.StartsWith("!")
                  || line.StartsWith("@")
                  || line.StartsWith("(")
                  ))
                {
                    var split = line.Split(new char[] { ':' });
                    if (split.Length >= 2)
                    {
                        string key = split[0].ToUpperInvariant().Trim();
                        string value = split[1].Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

                        if (key.StartsWith("UDAS_END"))
                        {
                            UDAS_END = value;
                            has_UDAS_END = true;
                        }
                        else if (key.StartsWith("DAT_"))
                        {
                            var datIdSplit = key.Split('_');
                            if (datIdSplit.Length >= 2)
                            {
                                int.TryParse(datIdSplit[1], out int Id);
                                string FileName = value;

                                if (Id > -1)
                                {
                                    Arqs.Add(Id, (key, FileName));
                                }
                            }

                        }

                    }

                }
            }

            idxj.Close();

        }

    }
}

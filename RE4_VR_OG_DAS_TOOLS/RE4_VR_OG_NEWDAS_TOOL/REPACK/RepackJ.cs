using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_VR_OG_NEWDAS_TOOL_REPACK
{
    internal class RepackJ
    {
        public RepackJ(FileInfo info) 
        {
            StreamReader idxj;
         
            try
            {
                idxj = info.OpenText();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                return;
            }

            string FILE_FORMAT = "";
            uint DAT_AMOUNT = 0;
            int UDAS_SOUNDFLAG = 4;
            string UDAS_END = null;
            Dictionary<string, (string DatID, string FileName, uint offsetKey)> Arqs = new Dictionary<string, (string DatID, string FileName, uint offsetKey)>();

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
                ))
                {
                    var split = line.Split(new char[] { ':' });
                    if (split.Length >= 2)
                    {
                        string key = split[0].ToUpperInvariant().Trim();
                        string value = split[1].Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

                        if (key.Contains("FILE_FORMAT"))
                        {
                            FILE_FORMAT = value;
                        }
                        else if (key.Contains("UDAS_END"))
                        {
                            UDAS_END = value;
                        }
                        else if (key.Contains("UDAS_SOUNDFLAG"))
                        {
                            int.TryParse(value, out UDAS_SOUNDFLAG);
                        }
                        else if (key.Contains("DAT_AMOUNT"))
                        {
                            uint.TryParse(value, out DAT_AMOUNT);
                        }
                        else if (key.StartsWith("DAT"))
                        {
                            string datId = key;
                            string fileName = value;
                            uint offsetKey = 0;

                            if (split.Length >= 3)
                            {
                                uint.TryParse(split[2].Trim(), out offsetKey);
                            }

                            Arqs.Add(datId, (datId, fileName, offsetKey));

                        }

                    }
                }

            }

            idxj.Close();

            // -- validações

            if (FILE_FORMAT.Length <= 0 || DAT_AMOUNT <= 0)
            {
                Console.WriteLine("Not found FILE_FORMAT or DAT_AMOUNT tag.");
                return;
            }

            if (FILE_FORMAT != "DAS")
            {
                Console.WriteLine("Invalid FILE_FORMAT: " + FILE_FORMAT);
                return;
            }

            foreach (var item in Arqs)
            {
                if (item.Value.offsetKey > 0x10_00_00_00)
                {
                    Console.WriteLine("OffsetKey is larger than allowed.");
                    return;
                }
            }

            //---------------

            Console.WriteLine("FILE_FORMAT: " + FILE_FORMAT);

            FileStream stream;

            try
            {
                string endFileName = Path.ChangeExtension(info.FullName, FILE_FORMAT.ToLowerInvariant());
                FileInfo endFileInfo = new FileInfo(endFileName);
                stream = endFileInfo.Create();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                return;
            }

            DatInfo[] datGroup = new DatInfo[DAT_AMOUNT];

            // get files
            for (int i = 0; i < DAT_AMOUNT; i++)
            {
                DatInfo dat = new DatInfo();
                string key = "DAT_" + i.ToString("D3");
                if (Arqs.ContainsKey(key))
                {
                    dat.Path = Arqs[key].FileName;
                    dat.Offset = Arqs[key].offsetKey;
                }
                else
                {
                    dat.Path = "null";
                }

                FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, dat.Path));
                dat.fileInfo = a;
                dat.Extension = a.Extension.ToUpperInvariant().Replace(".", "").PadRight(4, (char)0x0).Substring(0, 4);

                if (a.Exists)
                {
                    int aLength = (int)(((a.Length + 15) / 16) * 16);

                    dat.FileExits = true;
                    dat.Length = aLength;

                    Console.WriteLine("DAT_" + i.ToString("D3") + ": " + dat.Path);
                }
                else
                {
                    Console.WriteLine("DAT_" + i.ToString("D3") + ": " + dat.Path + "   (File does not exist!)");
                }

                datGroup[i] = dat;
            }

            // -------------------------------------------------------------------------

            OrganizedDat organizedDat = new OrganizedDat(ref datGroup);

            // -------------------------------------------------------------------------

            UdasInfo udasGroup = new UdasInfo();
            udasGroup.datFileBytesLength = organizedDat.datFileBytesLength;
            udasGroup.HasExtraData = organizedDat.HasExtraData;
            udasGroup.ExtraEmptyFileID = organizedDat.ExtraEmptyFileID;
            udasGroup.ExtraDatOffset = organizedDat.ExtraDatOffset;

            udasGroup.SoundFlag = UDAS_SOUNDFLAG;
            Console.WriteLine("UDAS_SOUNDFLAG: " + udasGroup.SoundFlag.ToString());

            if (UDAS_END != null && UDAS_END.Length > 0)
            {
                udasGroup.End.Path = UDAS_END;
                FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, udasGroup.End.Path));
                udasGroup.End.fileInfo = a;

                if (a.Exists)
                {
                    int aLength = (int)(((a.Length + 15) / 16) * 16);

                    udasGroup.End.FileExits = true;
                    udasGroup.End.Length = aLength;

                    Console.WriteLine("UDAS_END: " + udasGroup.End.Path);
                }
                else
                {
                    Console.WriteLine("UDAS_END: " + udasGroup.End.Path + "   (File does not exist!)");
                }

            }

            _ = new Udas(stream, datGroup, udasGroup);

            stream.Close();

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_VR_OG_INSERTDAS_TOOL.IINSERT
{
    internal class GetNewFilesInfo
    {
        public Dictionary<int, DatInfo> toInsert = new Dictionary<int, DatInfo>();

        public GetNewFilesInfo(FileInfo info, Dictionary<int, (string DatID, string FileName)> Arqs) 
        {
            foreach (var arq in Arqs)
            {
                DatInfo dat = new DatInfo();
                dat.Path = arq.Value.FileName;
                dat.Offset = 0;

                FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, arq.Value.FileName));
                dat.fileInfo = a;
                dat.Extension = a.Extension.ToUpperInvariant().Replace(".", "").PadRight(4, (char)0x0).Substring(0, 4);

                if (a.Exists)
                {
                    int aLength = (int)(((a.Length + 15) / 16) * 16);

                    dat.FileExits = true;
                    dat.Length = aLength;

                    Console.WriteLine("DAT_" + arq.Key.ToString("D3") + ": " + arq.Value.FileName);
                }
                else
                {
                    Console.WriteLine("DAT_" + arq.Key.ToString("D3") + ": " + arq.Value.FileName + "   (File does not exist!)");
                }

                toInsert.Add(arq.Key, dat);
            }

        }

    }
}

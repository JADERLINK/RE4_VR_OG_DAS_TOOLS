using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_VR_OG_NEWDAS_TOOL_EXTRACT
{
    internal class Extract
    {
        public Extract(FileInfo info, Dictionary<string, bool> formatsToShowOffsets) 
        {
            FileStream stream = null;
            StreamWriter idxj = null;

            try
            {
                stream = info.OpenRead();

                string idxjFileName = Path.ChangeExtension(info.FullName, ".IDXRE4VRDAS");
                FileInfo idxjInfo = new FileInfo(idxjFileName);
                idxj = idxjInfo.CreateText();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }

            if (stream != null && idxj != null)
            {
                idxj?.WriteLine("# github.com/JADERLINK/RE4_VR_OG_DAS_TOOLS");
                idxj?.WriteLine("# youtube.com/@JADERLINK");
                idxj?.WriteLine("# RE4_VR_OG_NEWDAS_TOOL By JADERLINK");
                idxj?.WriteLine("FILE_FORMAT:DAS");

                string directory = info.Directory.FullName;
                string baseName = Path.GetFileNameWithoutExtension(info.Name);
                if (baseName.Length == 0)
                {
                    baseName = "NULL";
                }

                try
                {
                    Udas a = new Udas(idxj, stream, directory, baseName, formatsToShowOffsets);

                    //Console
                    int Amount = a.DatAmount;
                    if (a.SndPath != null) { Amount += 1; }

                    Console.WriteLine("FileCount = " + Amount);
                    Console.WriteLine("SoundFlag = " + a.SoundFlag);
                    for (int i = 0; i < a.DatFiles.Length; i++)
                    {
                        Console.WriteLine("File_" + i + " = " + a.DatFiles[i]);
                    }
                    if (a.SndPath != null)
                    {
                        Console.WriteLine("File_" + (Amount - 1) + " = " + a.SndPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                }

                stream.Close();
                idxj?.Close();
            }
        }


    }
}

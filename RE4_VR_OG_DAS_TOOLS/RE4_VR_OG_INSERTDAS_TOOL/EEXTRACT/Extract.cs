using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_VR_OG_INSERTDAS_TOOL
{
    internal class Extract
    {
        public Extract(FileInfo info)
        {
            FileStream stream;
            StreamWriter idxj;

            try
            {
                stream = info.OpenRead();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                return;
            }

            try
            {
                FileInfo idxjInfo = new FileInfo(Path.ChangeExtension(info.FullName, ".INSERTDASRE4VR"));
                idxj = idxjInfo.CreateText();
            }
            catch (Exception ex)
            {
                stream.Close();
                Console.WriteLine("Error: " + ex);
                return;
            }

            idxj?.WriteLine("# github.com/JADERLINK/RE4_VR_OG_DAS_TOOLS");
            idxj?.WriteLine("# youtube.com/@JADERLINK");
            idxj?.WriteLine("# RE4_VR_OG_INSERTDAS_TOOL By JADERLINK");

            string baseName = Path.GetFileNameWithoutExtension(info.Name);
            if (baseName.Length == 0)
            {
                baseName = "NULL";
            }

            try
            {
                Udas a = new Udas(idxj, stream, baseName);

                // .idx
                int Amount = a.DatAmount;
                if (a.SndPath != null)
                {
                    Amount += 1;
                }

                //Console
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

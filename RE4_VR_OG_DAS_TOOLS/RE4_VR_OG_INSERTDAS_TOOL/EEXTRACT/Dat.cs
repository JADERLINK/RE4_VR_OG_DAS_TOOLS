using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

    
namespace RE4_VR_OG_INSERTDAS_TOOL
{
    internal class Dat
    {
        public int DatAmount = 0;
        public string[] DatFiles = null;

        public Dat(StreamWriter idxj , Stream readStream, uint offsetStart, string baseName) 
        {
            readStream.Position = offsetStart;
            BinaryReader br = new BinaryReader(readStream);
            int amount = br.ReadInt32();
            if (amount >= 0x010000)
            {
                Console.WriteLine("Invalid dat file!");
                return;
            }

            idxj?.WriteLine("# DAT_AMOUNT:" + amount);
            DatAmount = amount;

            int blocklength = amount * 4;

            byte[] offsetblock = new byte[blocklength];
            byte[] nameblock = new byte[blocklength];

            readStream.Position = offsetStart + 16;

            readStream.Read(offsetblock, 0, blocklength);
            readStream.Read(nameblock, 0, blocklength);

            (uint offset, string FileFullName, string format)[] fileList = new (uint offset, string FileFullName, string format)[amount];

            int Temp = 0;
            for (int i = 0; i < amount; i++)
            {
                uint offset = BitConverter.ToUInt32(offsetblock, Temp);
                string format = Encoding.ASCII.GetString(nameblock, Temp, 4);
                format = ValidateFormat(format).ToUpperInvariant();

                string FileFullName = Path.Combine(baseName, baseName + "_" + i.ToString("D3"));
                if (format.Length > 0)
                {
                    FileFullName += "." + format;
                }

                fileList[i] = (offset, FileFullName, format);

                Temp += 4;
            }

            DatFiles = new string[amount];

            idxj?.WriteLine("# Lines starting with # are just comments.");
            idxj?.WriteLine("# Remove the (.format) to insert the file into DAS.");
            idxj?.WriteLine("# File-ID : File-Name");
            idxj?.WriteLine();

            for (int i = 0; i < fileList.Length; i++)
            {
                DatFiles[i] = fileList[i].FileFullName;

                string Line = "(." + fileList[i].format + ") DAT_" + i.ToString("D3") + ":" + fileList[i].FileFullName;
                idxj?.WriteLine(Line);
                idxj?.WriteLine();
            }

        }

        private string ValidateFormat(string source) 
        {
            string res = "";
            for (int i = 0; i < source.Length; i++)
            {
                if ((source[i] >= 65 && source[i] <= 90)
                 || (source[i] >= 97 && source[i] <= 122)
                 || (source[i] >= 48 && source[i] <= 57))
                {
                    res += source[i];
                }
            }
            return res;
        }

    }
}

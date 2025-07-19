using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

    
namespace RE4_VR_OG_DAS_OFFSETKEY_TOOL
{
    internal class Dat
    {
        public int DatAmount = 0;
        public (string fullName, uint offset, int length, string format)[] DatFiles = null;

        public uint[] UordenedOffsets = null;

        public Dat(
            StreamWriter idxj,
            Stream readStream,
            uint offsetStart,
            string baseName,
            uint lengthDat,
            uint endUdasOffset,
            bool HasExtraData,
            uint[] ExtraDatOffset,
            ushort[] ExtraEmptyFileID,
            string[] SelectedFormats
            )
        {
            readStream.Position = offsetStart;
            BinaryReader br = new BinaryReader(readStream);
            int amount = br.ReadInt32();
            if (amount >= 0x010000)
            {
                Console.WriteLine("Invalid file!");
                return;
            }

            idxj.WriteLine("DAT_AMOUNT:" + amount);
            DatAmount = amount;

            int blocklength = amount * 4;

            byte[] offsetblock = new byte[blocklength];
            byte[] nameblock = new byte[blocklength];

            readStream.Position = offsetStart + 16;

            readStream.Read(offsetblock, 0, blocklength);
            readStream.Read(nameblock, 0, blocklength);

            (uint offset, string fullName, string format)[] fileList = new (uint offset, string fullName, string format)[amount];

            int Temp = 0;
            for (int i = 0; i < amount; i++)
            {
                uint offset = BitConverter.ToUInt32(offsetblock, Temp);
                string format = Encoding.ASCII.GetString(nameblock, Temp, 4);
                format = ValidateFormat(format).ToUpperInvariant();

                string fullName = Path.Combine(baseName, baseName + "_" + i.ToString("D3"));
                if (format.Length > 0)
                {
                    fullName += "." + format;
                }

                fileList[i] = (offset, fullName, format);

                Temp += 4;
            }

            DatFiles = new (string fullName, uint offset, int length, string format)[amount];

            List<uint> ordenedOffsets = new List<uint>();
            ordenedOffsets.AddRange(fileList.Select(x => x.offset));
            ordenedOffsets.Add(lengthDat);
            ordenedOffsets.Add(endUdasOffset);
            if (HasExtraData && ExtraDatOffset != null)
            {
                ordenedOffsets.AddRange(ExtraDatOffset);
            }
            ordenedOffsets = ordenedOffsets.OrderByDescending(x => x).ToList();

            idxj.WriteLine("# File-ID : File-Name : OffsetKey : Length");

            for (int i = 0; i < fileList.Length; i++)
            {

                uint myOffset = fileList[i].offset;
                uint nextOfset = myOffset; // Inicialmente define o mesmo offset. Daí o 'length' fica 0;

                if (!(HasExtraData && ExtraEmptyFileID.Contains((ushort)i))) // Entra no if se for uma negativa, pois se tem no ExtraEmptyFileID vai ficar com length 0;
                {
                    if (fileList[i].format.Length > 0) // Tem que ser maior que zero, pois se for 0, não tem formato, não tem arquivo;
                    {
                        foreach (var item in ordenedOffsets)
                        {
                            if (item > myOffset)
                            {
                                nextOfset = item;
                            }
                        }
                    }

                }

                // conteudo do arquivo
                int subFileLength = (int)(nextOfset - myOffset);

                DatFiles[i] = (fileList[i].fullName, myOffset, subFileLength, fileList[i].format);

                string Line = "DAT_" + i.ToString("D3") + " : " + fileList[i].fullName + " : " + myOffset.ToString("D") + " : " + subFileLength.ToString("D");

                if (SelectedFormats == null || SelectedFormats.Contains(fileList[i].format))
                {
                    idxj.WriteLine(Line);
                }
             
            }

            UordenedOffsets = ordenedOffsets.ToArray();
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE4_VR_OG_INSERTDAS_TOOL.IINSERT
{
    internal class GetInfoDAT
    {
        public uint DatAmount = 0;
        public (uint offsetToOffset, uint offsetToFormat, uint OriginalOffsetToFile, string OriginalFormat)[] DatFiles = null;

        public GetInfoDAT(Stream readStream, uint offsetStart) 
        {
            readStream.Position = offsetStart;
            BinaryReader br = new BinaryReader(readStream);
            uint amount = br.ReadUInt32();
            if (amount >= 0x010000)
            {
                Console.WriteLine("Invalid file!");
                return;
            }

            DatAmount = amount;

            int blocklength = (int)amount * 4;

            byte[] offsetblock = new byte[blocklength];
            byte[] nameblock = new byte[blocklength];

            readStream.Position = offsetStart + 16;

            readStream.Read(offsetblock, 0, blocklength);
            readStream.Read(nameblock, 0, blocklength);

            DatFiles = new (uint offsetToOffset, uint offsetToFormat, uint OriginalOffsetToFile, string OriginalFormat)[amount];

            uint Calc_offsetToOffset = offsetStart + 16;
            uint Calc_offsetToFormat = offsetStart + 16 + (uint)blocklength;

            int Temp = 0;
            for (int i = 0; i < amount; i++)
            {
                uint offset = BitConverter.ToUInt32(offsetblock, Temp);
                string format = Encoding.ASCII.GetString(nameblock, Temp, 4);
                format = ValidateFormat(format).ToUpperInvariant();

                DatFiles[i] = (Calc_offsetToOffset, Calc_offsetToFormat, offset, format);

                Temp += 4;
                Calc_offsetToOffset += 4;
                Calc_offsetToFormat += 4;
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

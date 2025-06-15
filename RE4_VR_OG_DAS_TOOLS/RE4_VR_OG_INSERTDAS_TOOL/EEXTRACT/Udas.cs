using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace RE4_VR_OG_INSERTDAS_TOOL
{
    internal class Udas
    {
        public int SoundFlag = -1;
        public int DatAmount = 0;
        public string[] DatFiles = null;
        public string SndPath = null;

        public Udas(StreamWriter idxj, Stream readStream, string baseName) 
        {
            BinaryReader br = new BinaryReader(readStream);

            List<(uint type, uint offset, uint length)> UdasList = new List<(uint type, uint offset, uint length)>();

            uint temp = 0x20;
            for (int i = 0; i < 2; i++)
            {
                br.BaseStream.Position = temp;

                uint u_Type = br.ReadUInt32();
                uint u_DataSize = br.ReadUInt32();
                _ = br.ReadUInt32(); // Unused
                uint u_DataOffset = br.ReadUInt32();

                if (u_Type == 0xFFFFFFFF)
                {
                    break;
                }

                var ulist = (u_Type, u_DataOffset, u_DataSize);
                UdasList.Add(ulist);

                temp += 32;
            }

            if (UdasList[0].offset > readStream.Length && UdasList[0].offset > 0x1000)
            {
                Console.WriteLine("Error extracting UDAS file, first offset is invalid!");
                return;
            }

            bool readedDat = false;
            bool readedSnd = false;

            for (int i = 0; i < UdasList.Count; i++)
            {
                uint type = UdasList[i].type;

                // type == 0xFFFFFFFF : none

                if (type == 0x0 && !readedDat)
                {
                    // DAT
                    //uint length = UdasList[i].length;
                    uint startOffset = UdasList[i].offset;

                    Dat a = new Dat(idxj, readStream, startOffset, baseName);
                    DatAmount = a.DatAmount;
                    DatFiles = a.DatFiles;

                    readedDat = true;
                }
                else if (type != 0x0 && type != 0xFFFFFFFF && !readedSnd)
                {
                    // SND

                    int startOffset = (int)UdasList[i].offset;
                    int length = (int)(readStream.Length - startOffset);

                    //end
                    if (length > 0)
                    {
                        string FileFullNameEnd = Path.Combine(baseName, baseName + "_END.SND");
                        idxj?.WriteLine("(.SND) UDAS_END:" + FileFullNameEnd);

                        SndPath = FileFullNameEnd;
                    }

                    SoundFlag = (int)type;
                    idxj?.WriteLine("# UDAS_SOUNDFLAG:" + ((int)type).ToString());

                    readedSnd = true;
                }
                else if (type != 0xFFFFFFFF)
                {
                    idxj?.WriteLine($"# ERROR_FLAG{i:D1}:" + ((int)type).ToString());

                    int startOffset = (int)UdasList[i].offset;
                    int length = (int)(readStream.Length - startOffset);

                    if (length > 0)
                    {
                        string FileFullName = Path.Combine(baseName, baseName + $"_ERROR{i:D1}.HEX");
                        idxj?.WriteLine($"# ERROR_FILE{i:D1}:" + FileFullName);
                    }
                }
            }


        }


    }
}

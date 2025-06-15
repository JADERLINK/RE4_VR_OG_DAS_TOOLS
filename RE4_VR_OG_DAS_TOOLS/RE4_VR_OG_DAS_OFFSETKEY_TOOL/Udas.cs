using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace RE4_VR_OG_DAS_OFFSETKEY_TOOL
{
    internal class Udas
    {
        public int SoundFlag = -1;
        public int DatAmount = 0;
        public (string name, uint offset, int length)[] DatFiles = null;
        public (string name, uint offset, uint length) SndPath;


        public Udas(StreamWriter idxj, Stream readStream, string baseName, string[] SelectedFormats) 
        {
            BinaryReader br = new BinaryReader(readStream);

            // header principal
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

            // Dados adicionais da tool
            br.BaseStream.Position = 0xF8;
            uint ExtraOffset = br.ReadUInt32();
            uint ExtraMagic = br.ReadUInt32();

            uint[] ExtraDatOffset = null; // Inicio das áreas que estão vazias;
            ushort[] ExtraEmptyFileID = null; // Informa quais arquivos são length zero;
            bool HasExtraData = false;

            if (ExtraOffset != 0 && ExtraMagic == 0x3E3D3D3C)
            {
                br.BaseStream.Position = ExtraOffset;

                uint count = br.ReadUInt32();
                ExtraDatOffset = new uint[count];

                for (int i = 0; i < count; i++)
                {
                    ExtraDatOffset[i] = br.ReadUInt32();
                }

                count = br.ReadUInt16();
                ExtraEmptyFileID = new ushort[count];

                for (int i = 0; i < count; i++)
                {
                    ExtraEmptyFileID[i] = br.ReadUInt16();
                }

                HasExtraData = true;
            }

            //-----------------------

            bool readedDat = false;
            bool readedSnd = false;

            uint[] UordenedOffsets = null;

            for (int i = 0; i < UdasList.Count; i++)
            {
                uint type = UdasList[i].type;

                // type == 0xFFFFFFFF : none

                if (type == 0x0 && !readedDat)
                {
                    // DAT
                    uint lengthDat = UdasList[i].length;
                    uint startOffset = UdasList[i].offset;
                    uint endUdasOffset = (uint)(readStream.Length - startOffset);

                    Dat a = new Dat(idxj, readStream, startOffset, baseName, lengthDat, endUdasOffset, HasExtraData, ExtraDatOffset, ExtraEmptyFileID, SelectedFormats);
                    DatAmount = a.DatAmount;
                    DatFiles = a.DatFiles;
                    UordenedOffsets = a.UordenedOffsets;

                    readedDat = true;
                }
                else if (type != 0x0 && type != 0xFFFFFFFF && !readedSnd)
                {
                    // SND  

                    SoundFlag = (int)type;
                    if (SelectedFormats == null || SelectedFormats.Contains("SND"))
                    {
                        idxj?.WriteLine("UDAS_SOUNDFLAG:" + ((int)type).ToString());
                    }
                   

                    uint startOffset = (uint)UdasList[i].offset;
                    uint length = (uint)(readStream.Length - startOffset);

                    //end
                    if (length > 0)
                    {
                        string FileFullNameEnd = Path.Combine(baseName, baseName + "_END.SND");

                        if (SelectedFormats == null || SelectedFormats.Contains("SND"))
                        {
                            idxj?.WriteLine("UDAS_END:" + FileFullNameEnd + " : " + startOffset.ToString("D") + " : " + length.ToString("D"));
                        }
                       

                        SndPath = (FileFullNameEnd, startOffset, length);

                    }

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

            if (HasExtraData && SelectedFormats == null)
            {
                if (ExtraDatOffset != null && ExtraDatOffset.Length > 0)
                {
                    idxj?.WriteLine("");
                    idxj?.WriteLine("# Spaces without content:");
                    idxj?.WriteLine("# Offset : Length");

                    foreach (var val in ExtraDatOffset.OrderBy(x => x))
                    {
                        uint myOffset = val;
                        uint nextOfset = myOffset;

                        foreach (var item in UordenedOffsets)
                        {
                            if (item > myOffset)
                            {
                                nextOfset = item;
                            }
                        }
                        int subLength = (int)(nextOfset - myOffset);

                        idxj?.WriteLine("# " + myOffset.ToString("D") + " : " + subLength.ToString("D"));
                    }
                }

                if (ExtraEmptyFileID != null && ExtraEmptyFileID.Length > 0)
                {
                    idxj?.WriteLine("");
                    idxj?.WriteLine("# File IDs without content:");
                    foreach (var item in ExtraEmptyFileID.OrderBy(x => x))
                    {
                        idxj?.WriteLine("# DAT_" + item.ToString("D3"));
                    }
                }
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_VR_OG_INSERTDAS_TOOL.IINSERT
{
    internal class GetOriginalHeader
    {
        public bool has_error_on_load = false;

        public uint Original_DAT_Offset = 0;
        public int Original_DAT_Length = 0;
        public uint Original_SND_Offset = 0;
        public int Original_SND_Length = 0;
        public uint Original_DAT_COUNT = 0; //quantidade de arquivos no dat

        public uint Original_Offset_To_DAT_Offset = 0;
        public uint Original_Offset_To_DAT_Length = 0;
        public uint Original_Offset_To_SND_Offset = 0;
        public uint Original_Offset_To_SND_Length = 0;

        public (uint offsetToOffset, uint offsetToFormat, uint OriginalOffsetToFile, string OriginalFormat)[] DatFiles = null;

        public uint[] ExtraDatOffset = null; // Inicio das áreas que estão vazias;
        public ushort[] ExtraEmptyFileID = null; // Informa quais arquivos são length zero;
        public bool HasExtraData = false;


        public GetOriginalHeader(Stream stream)
        {
            // obtem os dados do arquivo original.

            var br = new BinaryReader(stream);

            List<(uint type, uint offset, int length, uint OffsetToType)> UdasList = new List<(uint type, uint offset, int length, uint OffsetToType)>();

            uint temp = 0x20;
            for (int i = 0; i < 2; i++)
            {
                br.BaseStream.Position = temp;

                uint u_Type = br.ReadUInt32();
                int u_DataSize = br.ReadInt32();
                _ = br.ReadUInt32(); // Unused
                uint u_DataOffset = br.ReadUInt32();

                if (u_Type == 0xFFFFFFFF)
                {
                    break;
                }

                var ulist = (u_Type, u_DataOffset, u_DataSize, temp);
                UdasList.Add(ulist);

                temp += 32;
            }

            if (UdasList.Count == 0 || UdasList[0].offset >= stream.Length || UdasList[0].offset >= 0x01_00_00)
            {
                Console.WriteLine("Error in file, first offset is invalid!");
                has_error_on_load = true;
                return;
            }

            //-----------------------

            // Dados adicionais da tool
            br.BaseStream.Position = 0xF8;
            uint ExtraOffset = br.ReadUInt32();
            uint ExtraMagic = br.ReadUInt32();

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

            for (int i = 0; i < UdasList.Count; i++)
            {
                uint type = UdasList[i].type;

                // type == 0xFFFFFFFF : none

                if (type == 0x0 && !readedDat)
                {
                    // DAT
                    int length = UdasList[i].length;
                    uint startOffset = UdasList[i].offset;

                    Original_DAT_Offset = startOffset;
                    Original_DAT_Length = length;

                    GetInfoDAT a = new GetInfoDAT(stream, startOffset);
                    Original_DAT_COUNT = a.DatAmount;
                    DatFiles = a.DatFiles;

                    Original_Offset_To_DAT_Offset = UdasList[i].OffsetToType + 12;
                    Original_Offset_To_DAT_Length = UdasList[i].OffsetToType + 4;

                    readedDat = true;
                }
                else if (type != 0x0 && type != 0xFFFFFFFF && !readedSnd)
                {
                    // SND

                    uint startOffset = UdasList[i].offset;
                    int length = (int)(stream.Length - startOffset);

                    //end
                    if (length > 0)
                    {
                        Original_SND_Offset = startOffset;
                        Original_SND_Length = length;

                        Original_Offset_To_SND_Offset = UdasList[i].OffsetToType + 12;
                        Original_Offset_To_SND_Length = UdasList[i].OffsetToType + 4;
                    }

                    readedSnd = true;
                }
                else if (type != 0xFFFFFFFF)
                {
                    Console.WriteLine("Something wrong is not right.");
                    Console.WriteLine("Type: " + type.ToString("X8"));
                }
            }

            //-----------------------------

            if (Original_DAT_Offset == 0 || Original_DAT_Length == 0 || Original_DAT_COUNT == 0)
            {
                Console.WriteLine("Something wrong is not right.");
                has_error_on_load = true;
                return;
            }

        }

        public byte[] SND_CONTENT(Stream stream)
        {
            byte[] SND_CONTENT = new byte[Original_SND_Length];

            stream.Position = Original_SND_Offset;
            stream.Read(SND_CONTENT, 0, Original_SND_Length);

            return SND_CONTENT;
        }


    }
}

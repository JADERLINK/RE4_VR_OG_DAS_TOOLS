using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace RE4_VR_OG_NEWDAS_TOOL_REPACK
{
    internal class Udas
    {
        public Udas(FileStream stream, DatInfo[] dat, UdasInfo udasGroup) 
        {
            byte[] EndBytes = new byte[udasGroup.End.Length];
            bool hasEnd = false;

            if (udasGroup.End.FileExits)
            {
                try
                {
                    BinaryReader br = new BinaryReader(udasGroup.End.fileInfo.OpenRead());
                    br.BaseStream.Read(EndBytes, 0, (int)udasGroup.End.fileInfo.Length);
                    br.Close();
                    hasEnd = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error to read file: " + udasGroup.End.fileInfo.Name + Environment.NewLine + " ex: " + ex);
                }
            }

            byte[] MiddleBytes = new byte[0];
            if (udasGroup.HasExtraData)
            {
                int middleLegth = 4 + (udasGroup.ExtraDatOffset.Length * 4) + 2 + (udasGroup.ExtraEmptyFileID.Length * 2);
                middleLegth = (((middleLegth + 15) / 16) * 16); // alinhamento

                MiddleBytes = new byte[middleLegth];
                BinaryWriter br = new BinaryWriter(new MemoryStream(MiddleBytes));
                br.BaseStream.Position = 0;
                br.Write((int)udasGroup.ExtraDatOffset.Length);
                foreach (var item in udasGroup.ExtraDatOffset)
                {
                    br.Write((uint)item);
                }

                br.Write((ushort)udasGroup.ExtraEmptyFileID.Length);
                foreach (var item in udasGroup.ExtraEmptyFileID)
                {
                    br.Write((ushort)item);
                }
                br.Close();
            }

            byte[] TopBytes = MakeUdasTop(udasGroup, hasEnd, dat.Length > 0, udasGroup.HasExtraData, MiddleBytes.Length);

            stream.Position = 0;
            stream.Write(TopBytes, 0, TopBytes.Length);

            _ = new Dat(stream , dat, stream.Position);

            if (MiddleBytes.Length != 0)
            {
                stream.Position = udasGroup.MiddleOffset;
                stream.Write(MiddleBytes, 0, MiddleBytes.Length);
            }

            if (EndBytes.Length != 0)
            {
                stream.Position = udasGroup.End.Offset;
                stream.Write(EndBytes, 0, EndBytes.Length);
            }

        }

        public static byte[] MakeUdasTop(UdasInfo udasGroup, bool hasEnd, bool hasDat, bool hasExtra, int extraLegth) 
        {
            byte[] TopBytes = MakerNewTopBytes(hasEnd, hasDat, udasGroup.SoundFlag);

            //-----

            uint firtPosition = BitConverter.ToUInt32(TopBytes, 0x2C);
            if (firtPosition != TopBytes.Length)
            {
                var b = BitConverter.GetBytes((uint)TopBytes.Length);
                TopBytes[0x2c] = b[0];
                TopBytes[0x2d] = b[1];
                TopBytes[0x2e] = b[2];
                TopBytes[0x2f] = b[3];
                firtPosition = (uint)TopBytes.Length;
            }

            udasGroup.MiddleOffset = (uint)firtPosition + udasGroup.datFileBytesLength;
            udasGroup.End.Offset = (uint)(udasGroup.MiddleOffset + extraLegth);


            if (hasDat)
            {
                uint firstType = BitConverter.ToUInt32(TopBytes, 0x20);
                if (firstType != 0)
                {
                    TopBytes[0x20] = 0;
                    TopBytes[0x21] = 0;
                    TopBytes[0x22] = 0;
                    TopBytes[0x23] = 0;
                }

                byte[] datlength = BitConverter.GetBytes((uint)udasGroup.datFileBytesLength);
                TopBytes[0x24] = datlength[0];
                TopBytes[0x25] = datlength[1];
                TopBytes[0x26] = datlength[2];
                TopBytes[0x27] = datlength[3];

                if (hasEnd)
                {
                    byte[] endOffset = BitConverter.GetBytes((uint)udasGroup.End.Offset);

                    TopBytes[0x4C] = endOffset[0];
                    TopBytes[0x4D] = endOffset[1];
                    TopBytes[0x4E] = endOffset[2];
                    TopBytes[0x4F] = endOffset[3];

                    TopBytes[0x44] = 0;
                    TopBytes[0x45] = 0;
                    TopBytes[0x46] = 0;
                    TopBytes[0x47] = 0;

                    uint secondType = BitConverter.ToUInt32(TopBytes, 0x40);
                    if (secondType == 0xFFFFFFFF)
                    {
                        byte[] SoundFlag = BitConverter.GetBytes((uint)udasGroup.SoundFlag);

                        TopBytes[0x40] = SoundFlag[0];
                        TopBytes[0x41] = SoundFlag[1];
                        TopBytes[0x42] = SoundFlag[2];
                        TopBytes[0x43] = SoundFlag[3];
                    }

                    TopBytes[0x60] = 0xFF;
                    TopBytes[0x61] = 0xFF;
                    TopBytes[0x62] = 0xFF;
                    TopBytes[0x63] = 0xFF;
                }
                else
                {
                    TopBytes[0x40] = 0xFF;
                    TopBytes[0x41] = 0xFF;
                    TopBytes[0x42] = 0xFF;
                    TopBytes[0x43] = 0xFF;
                }

            }
            else
            {
                if (hasEnd)
                {
                    byte[] endOffset = BitConverter.GetBytes((uint)udasGroup.End.Offset);

                    TopBytes[0x2C] = endOffset[0];
                    TopBytes[0x2D] = endOffset[1];
                    TopBytes[0x2E] = endOffset[2];
                    TopBytes[0x2F] = endOffset[3];



                    TopBytes[0x24] = 0;
                    TopBytes[0x25] = 0;
                    TopBytes[0x26] = 0;
                    TopBytes[0x27] = 0;

                    uint secondType = BitConverter.ToUInt32(TopBytes, 0x20);
                    if (secondType == 0xFFFFFFFF || secondType == 0)
                    {
                        byte[] SoundFlag = BitConverter.GetBytes((uint)udasGroup.SoundFlag);

                        TopBytes[0x20] = SoundFlag[0];
                        TopBytes[0x21] = SoundFlag[1];
                        TopBytes[0x22] = SoundFlag[2];
                        TopBytes[0x23] = SoundFlag[3];
                    }

                    TopBytes[0x40] = 0xFF;
                    TopBytes[0x41] = 0xFF;
                    TopBytes[0x42] = 0xFF;
                    TopBytes[0x43] = 0xFF;
                }
                else
                {
                    TopBytes[0x20] = 0xFF;
                    TopBytes[0x21] = 0xFF;
                    TopBytes[0x22] = 0xFF;
                    TopBytes[0x23] = 0xFF;
                }

            }

            if (hasExtra)
            {
                byte[] extraOffset = BitConverter.GetBytes(udasGroup.MiddleOffset);

                TopBytes[0xF8] = extraOffset[0];
                TopBytes[0xF9] = extraOffset[1];
                TopBytes[0xFA] = extraOffset[2];
                TopBytes[0xFB] = extraOffset[3];

                //MAGIC
                TopBytes[0xFC] = 0x3C;
                TopBytes[0xFD] = 0x3D;
                TopBytes[0xFE] = 0x3D;
                TopBytes[0xFF] = 0x3E;
            }

            return TopBytes;
        }

        private static byte[] MakerNewTopBytes(bool hasEnd, bool hasDat, int SoundFlag) 
        {
            byte[] top = new byte[0x400];
            int temp = 0;
            for (int i = 0; i < 8; i++)
            {
                top[temp] = 0xCA;
                top[temp+1] = 0xB6;
                top[temp+2] = 0xBE;
                top[temp+3] = 0x20;
                temp += 4;
            }

            top[0x2D] = 0x04; // offset; little endian

            if (hasDat && hasEnd)
            {
                byte[] soundFlag = BitConverter.GetBytes((uint)SoundFlag);

                top[0x40] = soundFlag[0];
                top[0x41] = soundFlag[1];
                top[0x42] = soundFlag[2];
                top[0x43] = soundFlag[3];

                top[0x60] = 0xFF;
                top[0x61] = 0xFF;
                top[0x62] = 0xFF;
                top[0x63] = 0xFF;
            }
            else if (hasDat && !hasEnd)
            {
                top[0x40] = 0xFF;
                top[0x41] = 0xFF;
                top[0x42] = 0xFF;
                top[0x43] = 0xFF;
            }
            else if (!hasDat && hasEnd)
            {
                byte[] soundFlag = BitConverter.GetBytes((uint)SoundFlag);

                top[0x20] = soundFlag[0];
                top[0x21] = soundFlag[1];
                top[0x22] = soundFlag[2];
                top[0x23] = soundFlag[3];

                top[0x40] = 0xFF;
                top[0x41] = 0xFF;
                top[0x42] = 0xFF;
                top[0x43] = 0xFF;
            }
            else 
            {
                top[0x20] = 0xFF;
                top[0x21] = 0xFF;
                top[0x22] = 0xFF;
                top[0x23] = 0xFF;
            }

            return top;
        }

    }


    internal class UdasInfo 
    {
        public uint datFileBytesLength = 0;
        public int SoundFlag = 4; 
        public DatInfo End = new DatInfo();
        public uint[] ExtraDatOffset = null;
        public ushort[] ExtraEmptyFileID = null;
        public bool HasExtraData = false;
        public uint MiddleOffset = 0;
    }


}

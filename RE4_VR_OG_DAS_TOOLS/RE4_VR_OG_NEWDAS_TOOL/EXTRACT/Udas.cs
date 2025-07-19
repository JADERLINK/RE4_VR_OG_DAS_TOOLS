using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_VR_OG_NEWDAS_TOOL_EXTRACT
{
    internal class Udas
    {
        public int SoundFlag = -1;
        public int DatAmount = 0;
        public string[] DatFiles = null;
        public string SndPath = null;

        public Udas(StreamWriter idxj, Stream readStream, string directory, string baseName, Dictionary<string, bool> formatsToShowOffsets) 
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

            if (UdasList.Count == 0 || UdasList[0].offset >= readStream.Length || UdasList[0].offset >= 0x01_00_00)
            {
                Console.WriteLine("Error extracting file, first offset is invalid!");
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


            // UDAS_TOP
            if (UdasList.Count >= 1)
            {
                if (!Directory.Exists(Path.Combine(directory, baseName)))
                {
                    try
                    {
                        Directory.CreateDirectory(Path.Combine(directory, baseName));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to create directory: " + Path.Combine(directory, baseName));
                        Console.WriteLine(ex);
                        return;
                    }
                }

                int udasTopLength = (int)UdasList[0].offset;
                byte[] udasTop = new byte[udasTopLength];

                readStream.Position = 0;
                readStream.Read(udasTop, 0, udasTopLength);

                string fullName = Path.Combine(baseName, baseName + "_TOP.HEX");

                try
                {
                    File.WriteAllBytes(Path.Combine(directory, fullName), udasTop);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(fullName + ": " + ex);
                }         

            }

            // UDAS_MIDDLE
            if (UdasList.Count == 1)
            {
                int length = (int)UdasList[0].length;
                int startOffset = (int)UdasList[0].offset;
                int maxLength = (int)readStream.Length;
                int newOffset = startOffset + length;
                int newLength = maxLength - newOffset;

                if (newLength > 0 && newOffset < readStream.Length)
                {
                    byte[] udasMiddle = new byte[newLength];

                    readStream.Position = newOffset;
                    readStream.Read(udasMiddle, 0, newLength);

                    string FileFullName = Path.Combine(baseName, baseName + "_MIDDLE.HEX");

                    try
                    {
                        File.WriteAllBytes(Path.Combine(directory, FileFullName), udasMiddle);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(FileFullName + ": " + ex);
                    }
                    
                }
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
                    uint lengthDat = UdasList[i].length;
                    uint startOffset = UdasList[i].offset;
                    uint endUdasOffset = (uint)(readStream.Length - startOffset);

                    Dat a = new Dat(idxj, readStream, startOffset, directory, baseName, lengthDat, endUdasOffset, HasExtraData, ExtraDatOffset, ExtraEmptyFileID, formatsToShowOffsets);
                    DatAmount = a.DatAmount;
                    DatFiles = a.DatFiles;

                    readedDat = true;
                }
                else if (type != 0x0 && type != 0xFFFFFFFF && !readedSnd)
                {
                    // SND  

                    SoundFlag = (int)type;
                    idxj?.WriteLine("UDAS_SOUNDFLAG:" + ((int)type).ToString());

                    int startOffset = (int)UdasList[i].offset;
                    int lengthSND = (int)(readStream.Length - startOffset);

                    //middle
                    if (i >= 1)
                    {
                        int M_Length = (int)UdasList[i-1].length;
                        int M_startOffset = (int)UdasList[i-1].offset;
                        int subOffset = M_startOffset + M_Length;
                        int subLength = startOffset - subOffset;

                        if (subLength > 0)
                        {
                            byte[] udasMiddle = new byte[subLength];

                            readStream.Position = subOffset;
                            readStream.Read(udasMiddle, 0, subLength);

                            string fullName = Path.Combine(baseName,  baseName + "_MIDDLE.HEX");

                            try
                            {
                                File.WriteAllBytes(Path.Combine(directory, fullName), udasMiddle);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(fullName + ": " + ex);
                            }
                        }
                    }

                    //end
                    if (lengthSND > 0)
                    {
                        byte[] udasEnd = new byte[lengthSND];

                        readStream.Position = startOffset;
                        readStream.Read(udasEnd, 0, lengthSND);

                        string fullNameSND = Path.Combine(baseName, baseName + "_END.SND");
                        idxj?.WriteLine("UDAS_END:" + fullNameSND);

                        SndPath = fullNameSND;

                        try
                        {
                            File.WriteAllBytes(Path.Combine(directory, fullNameSND), udasEnd);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(fullNameSND + ": " + ex);
                        }
 
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
                        byte[] udasError = new byte[length];

                        readStream.Position = startOffset;
                        readStream.Read(udasError, 0, length);

                        string fullName = Path.Combine(baseName, baseName + $"_ERROR{i:D1}.HEX");
                        idxj?.WriteLine($"# ERROR_FILE{i:D1}:" + fullName);

                        try
                        {
                            File.WriteAllBytes(Path.Combine(directory, fullName), udasError);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(fullName + ": " + ex);
                        }

                    }
                }
            }


        }


    }
}

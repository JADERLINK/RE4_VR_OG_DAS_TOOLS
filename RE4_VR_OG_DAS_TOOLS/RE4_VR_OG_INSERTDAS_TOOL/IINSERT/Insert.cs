using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE4_VR_OG_INSERTDAS_TOOL.IINSERT
{
    internal static class Insert
    {
        public static void DoIt(FileInfo info)
        {
            GetIdx idx = new GetIdx(info);
            if (idx.has_error_on_load)
            {
                return;
            }

            //------------------------------------

            FileStream stream;
            try
            {
                FileInfo endFileInfo = new FileInfo(Path.ChangeExtension(info.FullName, "das"));
                stream = endFileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                return;
            }

            //------------------------------------

            GetOriginalHeader oHeader = new GetOriginalHeader(stream);
            if (oHeader.has_error_on_load)
            {
                return;
            }

            byte[] SND_CONTENT = oHeader.SND_CONTENT(stream);

            // verifica os arquivos a ser inseridos.

            GetNewFilesInfo filesInfo = new GetNewFilesInfo(info, idx.Arqs);

            //-----------------------

            MakeOrganizedDat organized = new MakeOrganizedDat(oHeader, filesInfo);

            //--------------------------------------------------------------------------------------
            var bw = new BinaryWriter(stream);

            for (int i = 0; i < organized.DatFiles.Length; i++)
            {
                // grava novo offset
                stream.Position = organized.DatFiles[i].offsetToOffset;
                bw.Write((uint)organized.DatFiles[i].FinalOffsetToFile);

                //  grava novo formato
                byte[] name = Encoding.ASCII.GetBytes(organized.DatFiles[i].FinalFormat);
                stream.Position = organized.DatFiles[i].offsetToFormat;
                stream.Write(name, 0, 4);

                if (filesInfo.toInsert.ContainsKey(i) && filesInfo.toInsert[i].FileExits && filesInfo.toInsert[i].Length > 0)
                {
                    byte[] archive = new byte[filesInfo.toInsert[i].Length];
                    try
                    {
                        BinaryReader brl = new BinaryReader(filesInfo.toInsert[i].fileInfo.OpenRead());
                        brl.BaseStream.Read(archive, 0, (int)filesInfo.toInsert[i].fileInfo.Length);
                        brl.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error to read file: " + filesInfo.toInsert[i].fileInfo.Name);
                        Console.WriteLine(ex);
                    }

                    // grava arquivo
                    stream.Position = organized.DatFiles[i].FinalOffsetToFile + oHeader.Original_DAT_Offset;
                    stream.Write(archive, 0, archive.Length);

                    Console.WriteLine("The file was inserted: " + filesInfo.toInsert[i].Path);
                }
            }

            //--------------------------

            // arruma o tamanho do arquivo dat

            uint newDatLength = organized.datFileBytesLength;
            stream.Position = oHeader.Original_Offset_To_DAT_Length;
            bw.Write((uint)newDatLength);

            //midle
            uint SND_Offset = organized.datFileBytesLength + oHeader.Original_DAT_Offset;
            if (organized.HasExtraData)
            {
                int middleLegth = 4 + (organized.ExtraDatOffset.Length * 4) + 2 + (organized.ExtraEmptyFileID.Length * 2);
                middleLegth = (((middleLegth + 15) / 16) * 16); // alinhamento

                byte[] MiddleBytes = new byte[middleLegth];
                BinaryWriter mid = new BinaryWriter(new MemoryStream(MiddleBytes));
                mid.BaseStream.Position = 0;
                mid.Write((int)organized.ExtraDatOffset.Length);
                foreach (var item in organized.ExtraDatOffset)
                {
                    mid.Write((uint)item);
                }

                mid.Write((ushort)organized.ExtraEmptyFileID.Length);
                foreach (var item in organized.ExtraEmptyFileID)
                {
                    mid.Write((ushort)item);
                }
                mid.Close();

                stream.Position = SND_Offset;
                bw.Write(MiddleBytes);

                stream.Position = 0xF8;
                bw.Write((uint)SND_Offset);
                bw.Write((uint)0x3E3D3D3C);

                SND_Offset = (uint)(SND_Offset + MiddleBytes.Length);

              
            }
            else 
            {
                stream.Position = 0xF8;
                bw.Write((uint)0);
                bw.Write((uint)0);

            }

            // arruma o arquivo snd
            stream.Position = oHeader.Original_Offset_To_SND_Offset;
            bw.Write((uint)SND_Offset);

            stream.Position = oHeader.Original_Offset_To_SND_Length;
            bw.Write((uint)0);

            stream.Position = SND_Offset;

            uint EndLength = SND_Offset + (uint)SND_CONTENT.Length;

            if (idx.has_UDAS_END)
            {
                FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, idx.UDAS_END));

                if (a.Exists)
                {
                    uint aLength = (uint)(((a.Length + 15) / 16) * 16);

                    try
                    {
                        SND_CONTENT = new byte[aLength];
                        BinaryReader brl = new BinaryReader(a.OpenRead());
                        brl.BaseStream.Read(SND_CONTENT, 0, (int)a.Length);
                        brl.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error to read file: " + a.Name);
                        Console.WriteLine(ex);
                    }

                    EndLength = SND_Offset + aLength;

                    Console.WriteLine("UDAS_END: " + idx.UDAS_END);
                }
                else
                {
                    Console.WriteLine("UDAS_END: " + idx.UDAS_END + "   (File does not exist!)");
                }

            }

            stream.Write(SND_CONTENT, 0, SND_CONTENT.Length);

            stream.SetLength(EndLength);

            stream.Close();
        }


     

    }


}
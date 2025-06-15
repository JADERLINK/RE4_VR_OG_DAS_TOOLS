using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_VR_OG_DAS_OFFSETKEY_TOOL
{
    internal class Extract
    {
        public Extract(FileInfo info, FileFormat fileFormat, string[] SelectedFormats) 
        {
            FileStream stream = null;
            StreamWriter idxj = null;

            try
            {
                stream = info.OpenRead();

                string idxjFileName = Path.ChangeExtension(info.FullName, ".txt2");
                FileInfo idxjInfo = new FileInfo(idxjFileName);
                idxj = idxjInfo.CreateText();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }

            if (stream == null || idxj == null)
            {
                return;
            }

            idxj?.WriteLine("# github.com/JADERLINK/RE4_VR_OG_DAS_TOOLS");
            idxj?.WriteLine("# youtube.com/@JADERLINK");
            idxj?.WriteLine("# RE4_VR_OG_DAS_OFFSETKEY_TOOL By JADERLINK");
            switch (fileFormat)
            {
                case FileFormat.DAT:
                    idxj?.WriteLine("FILE_FORMAT:DAT");
                    break;
                case FileFormat.MAP:
                    idxj?.WriteLine("FILE_FORMAT:MAP");
                    break;
                case FileFormat.UDAS:
                    idxj?.WriteLine("FILE_FORMAT:UDAS");
                    break;
                case FileFormat.DAS:
                    idxj?.WriteLine("FILE_FORMAT:DAS");
                    break;
                default:
                    idxj?.WriteLine("FILE_FORMAT:NULL");
                    break;
            }

            string baseName = Path.GetFileNameWithoutExtension(info.Name);
            if (baseName.Length == 0)
            {
                baseName = "NULL";
            }

            if (fileFormat == FileFormat.DAT || fileFormat == FileFormat.MAP)
            {
                try
                {
                    Dat a = new Dat(idxj, stream, 0, baseName, (uint)info.Length, (uint)info.Length, false, null, null, SelectedFormats);

                    //Console
                    Console.WriteLine("FileCount = " + a.DatAmount);

                    Console.WriteLine("# File-ID : File-Name : OffsetKey : Length");

                    for (int i = 0; i < a.DatFiles.Length; i++)
                    {
                        string format = string.Concat(a.DatFiles[i].name.ToUpperInvariant().Skip(a.DatFiles[i].name.Length - 3));
                        if (SelectedFormats == null || SelectedFormats.Contains(format))
                        {
                            Console.WriteLine("File_" + i + " = " + a.DatFiles[i].name + " : " + a.DatFiles[i].offset.ToString("D") + " : " + a.DatFiles[i].length.ToString("D"));
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                }

            }

            else if (fileFormat == FileFormat.UDAS || fileFormat == FileFormat.DAS)
            {
                try
                {
                    Udas a = new Udas(idxj, stream, baseName, SelectedFormats);

                    // .idx
                    int Amount = a.DatAmount;
                    if (a.SndPath.name != null)
                    {
                        Amount += 1;
                    }

                    //Console
                    Console.WriteLine("FileCount = " + Amount);
                    Console.WriteLine("SoundFlag = " + a.SoundFlag);

                    Console.WriteLine("# File-ID : File-Name : OffsetKey : Length");

                    for (int i = 0; i < a.DatFiles.Length; i++)
                    {
                        string format = string.Concat(a.DatFiles[i].name.ToUpperInvariant().Skip(a.DatFiles[i].name.Length - 3));
                        if (SelectedFormats == null || SelectedFormats.Contains(format))
                        {
                            Console.WriteLine("File_" + i + " = " + a.DatFiles[i].name + " : " + a.DatFiles[i].offset.ToString("D") + " : " + a.DatFiles[i].length.ToString("D"));
                        }
                    }
                    if (a.SndPath.name != null && (SelectedFormats == null || SelectedFormats.Contains("SND")))
                    {
                        Console.WriteLine("File_" + (Amount - 1) + " = " + a.SndPath.name + " : " + a.SndPath.offset.ToString("D") + " : " + a.SndPath.length.ToString("D"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                }

            }

            stream.Close();
            idxj?.Close();
        }

    }
}

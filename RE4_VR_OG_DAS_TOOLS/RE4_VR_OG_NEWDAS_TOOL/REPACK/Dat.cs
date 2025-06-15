using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace RE4_VR_OG_NEWDAS_TOOL_REPACK
{
    internal class Dat
    {

        public Dat(FileStream stream, DatInfo[] dat, long StartOffset)
        {
            stream.Position = StartOffset;

            byte[] headerCont = new byte[16];
            byte[] Amount = BitConverter.GetBytes(dat.Length);
            headerCont[0] = Amount[0];
            headerCont[1] = Amount[1];
            headerCont[2] = Amount[2];
            headerCont[3] = Amount[3];
            stream.Write(headerCont, 0, 16);

            for (int i = 0; i < dat.Length; i++)
            {
                byte[] offset = BitConverter.GetBytes(dat[i].Offset);
                stream.Write(offset, 0, 4);
            }

            for (int i = 0; i < dat.Length; i++)
            {
                byte[] name = Encoding.ASCII.GetBytes(dat[i].Extension);
                stream.Write(name, 0, 4);
            }

            for (int i = 0; i < dat.Length; i++)
            {
                stream.Position = StartOffset + dat[i].Offset;

                try
                {
                    if (dat[i].FileExits)
                    {
                        var reader = dat[i].fileInfo.OpenRead();
                        reader.CopyTo(stream);
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error to read file: " + dat[i].fileInfo.Name + Environment.NewLine + " ex: " + ex);
                }
            }

        }

    }


    internal class DatInfo
    {
        public string Path = "";
        public FileInfo fileInfo = null;
        public string Extension = "";
        public int Length = 0;
        public uint Offset = 0;
        public bool FileExits = false;
    }


}

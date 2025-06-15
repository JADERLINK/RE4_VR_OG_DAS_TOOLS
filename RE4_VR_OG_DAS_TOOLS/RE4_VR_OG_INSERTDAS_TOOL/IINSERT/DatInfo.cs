using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RE4_VR_OG_INSERTDAS_TOOL.IINSERT
{
    internal class DatInfo
    {
        public string Path = null;
        public FileInfo fileInfo = null;
        public string Extension = null;
        public int Length = 0;
        public uint Offset = 0;
        public bool FileExits = false;
    }
}

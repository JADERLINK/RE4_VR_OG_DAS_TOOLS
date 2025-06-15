using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE4_VR_OG_INSERTDAS_TOOL.IINSERT
{
    internal class MakeOrganizedDat
    {
        public (uint offsetToOffset, uint offsetToFormat, uint FinalOffsetToFile, string FinalFormat)[] DatFiles = null;

        public uint[] ExtraDatOffset = null; // Inicio das áreas que estão vazias;
        public ushort[] ExtraEmptyFileID = null; // Informa quais arquivos são length zero;
        public bool HasExtraData = false;

        public uint datFileBytesLength = 0;

        public MakeOrganizedDat(GetOriginalHeader oHeader, GetNewFilesInfo filesInfo)
        {
            List<uint> ordenedOffsets = new List<uint>();
            ordenedOffsets.AddRange(oHeader.DatFiles.Select(x => x.OriginalOffsetToFile));
            if (oHeader.HasExtraData)
            {
                ordenedOffsets.AddRange(oHeader.ExtraDatOffset);
            }
            ordenedOffsets.Add((uint)oHeader.Original_DAT_Length);
            ordenedOffsets.Add((uint)oHeader.Original_SND_Offset);
            ordenedOffsets = ordenedOffsets.OrderByDescending(x => x).ToList();

            //--------------------
            List<OrganizedBlock> Original_Dat_Region = new List<OrganizedBlock>();

            for (int i = 0; i < oHeader.DatFiles.Length; i++)
            {
                uint myOffset = oHeader.DatFiles[i].OriginalOffsetToFile;
                uint nextOfset = myOffset; // Inicialmente define o mesmo offset. Daí o 'length' fica 0;

                if (!(oHeader.HasExtraData && oHeader.ExtraEmptyFileID.Contains((ushort)i))) // Entra no if se for uma negativa, pois se tem no ExtraEmptyFileID vai ficar com length 0;
                {
                    if (oHeader.DatFiles[i].OriginalFormat.Length > 0) // Tem que ser maior que zero, pois se for 0, não tem formato, não tem arquivo;
                    {
                        foreach (var item in ordenedOffsets)
                        {
                            if (item > myOffset)
                            {
                                nextOfset = item;
                            }
                        }
                    }

                }

                int subLength = (int)(nextOfset - myOffset);

                OrganizedBlock ob = new OrganizedBlock();
                ob.StartOffset = myOffset;
                ob.Length = subLength;
                ob.EndOffset = nextOfset;
                ob.IsOccupied = true;
                ob.fileID = i;
                ob.Format = oHeader.DatFiles[i].OriginalFormat;
                Original_Dat_Region.Add(ob);
            }

            OrganizedBlock header = null;
            // espaço ocupado pelo header
            {
                uint uheader = (uint)((oHeader.DatFiles.Length * 4 * 2) + 16);
                uheader = ((uheader + 15) / 16) * 16;

                OrganizedBlock obh = new OrganizedBlock();
                obh.StartOffset = 0;
                obh.Length = (int)uheader;
                obh.EndOffset = uheader;
                obh.IsOccupied = true;
                obh.fileID = -1;
                obh.Format = "";
                Original_Dat_Region.Add(obh);
                header = obh;
            }

            Original_Dat_Region = Original_Dat_Region.OrderBy(x => x.StartOffset).ToList();

            //----------------------

            List<OrganizedBlock> Final_Occupied_Region = new List<OrganizedBlock>();
            List<OrganizedBlock> Free_Region = new List<OrganizedBlock>();

            //------------------------

            foreach (var item in Original_Dat_Region)
            {
                if (filesInfo.toInsert.ContainsKey(item.fileID) && filesInfo.toInsert[item.fileID].FileExits)
                {
                    var ins = filesInfo.toInsert[item.fileID];

                    if (ins.Length <= item.Length && ins.Length > 0)
                    {
                        var clone = item.Clone();
                        ins.Offset = clone.StartOffset;
                        clone.Length = ins.Length;
                        clone.EndOffset = (uint)(clone.StartOffset + ins.Length);
                        clone.Format = ins.Extension;
                        Final_Occupied_Region.Add(clone);
                    }
                }
                else if (item.Length > 0) 
                {
                    var clone = item.Clone();
                    clone.Format = clone.Format.PadRight(4, '\0');
                    Final_Occupied_Region.Add(clone);
                }
            }

            Final_Occupied_Region = Final_Occupied_Region.OrderBy(x => x.StartOffset).ToList();
            //-----------

            for (int i = 0; i < Final_Occupied_Region.Count - 1; i++)
            {
                uint startOffset = Final_Occupied_Region[i].EndOffset;
                uint endOffset = Final_Occupied_Region[i + 1].StartOffset;
                int length = (int)((long)endOffset - (long)startOffset);

                if (length > 0)
                {
                    OrganizedBlock freee = new OrganizedBlock();
                    freee.StartOffset = startOffset;
                    freee.Length = length;
                    freee.EndOffset = endOffset;
                    freee.IsOccupied = false;
                    freee.fileID = -2;
                    Free_Region.Add(freee);
                }
            }

            Free_Region = Free_Region.OrderByDescending(x => x.Length).ToList();

            // last
            OrganizedBlock lastBlock;
            {
                uint startOffset = Final_Occupied_Region[Final_Occupied_Region.Count - 1].EndOffset;
                uint endOffset = int.MaxValue;
                int length = (int)((long)endOffset - (long)startOffset);

                OrganizedBlock freee = new OrganizedBlock();
                freee.StartOffset = startOffset;
                freee.Length = length;
                freee.EndOffset = endOffset;
                freee.IsOccupied = false;
                freee.fileID = -3;
                Free_Region.Add(freee);
                lastBlock = freee;
            }

            // proxima etapa, tira do espaço free
            // preencher os arquivos faltantes

            var _toInsetMissing = filesInfo.toInsert.Where(x => x.Value.Length > 0 && x.Value.Offset == 0 && x.Value.FileExits).OrderByDescending(x => x.Value.Length);
            foreach (var item in _toInsetMissing)
            {
                if (item.Value.Length > 0 && item.Value.Offset == 0 && item.Value.FileExits)
                {
                    for (int i = 0; i < Free_Region.Count; i++)
                    {
                        if (item.Value.Length <= Free_Region[i].Length)
                        {
                            OrganizedBlock filee = new OrganizedBlock();
                            filee.StartOffset = Free_Region[i].StartOffset;
                            item.Value.Offset = filee.StartOffset;
                            filee.Length = item.Value.Length;
                            filee.EndOffset = (uint)(Free_Region[i].StartOffset + item.Value.Length);
                            filee.IsOccupied = true;
                            filee.fileID = item.Key;
                            filee.Format = item.Value.Extension;
                            Final_Occupied_Region.Add(filee);

                            Free_Region[i].Length -= item.Value.Length;
                            Free_Region[i].StartOffset = (uint)(Free_Region[i].StartOffset + item.Value.Length);

                            if (Free_Region[i].Length <= 0)
                            {
                                var toRemove = Free_Region[i];
                                Free_Region.RemoveAll(x => x.GetBlockID() == toRemove.GetBlockID());
                            }

                            break;
                        }

                    }

                }
            }

            HashSet<ushort> ExtraEmptyFileID_List = new HashSet<ushort>();
            List<int> BusyIDs = Final_Occupied_Region.Select(x => x.fileID).ToList();

            for (int i = 0; i < oHeader.DatFiles.Length; i++)
            {
                if ( ! BusyIDs.Contains(i))
                {
                    ExtraEmptyFileID_List.Add((ushort)i);

                    OrganizedBlock ff = new OrganizedBlock();
                    ff.StartOffset = 0;
                    ff.Length = 0;
                    ff.EndOffset = 0;
                    ff.fileID = i;
                    
                    Final_Occupied_Region.Add(ff);

                }
            }

            HashSet<uint> ExtraDatOffset_List = new HashSet<uint>();
            Free_Region.RemoveAll(x => x.GetBlockID() == lastBlock.GetBlockID());

            foreach (var item in Free_Region)
            {
                ExtraDatOffset_List.Add(item.StartOffset);
            }

            //------------

            DatFiles = new (uint offsetToOffset, uint offsetToFormat, uint FinalOffsetToFile, string FinalFormat)[oHeader.DatFiles.Length];

            // define os offsets
            for (int i = 0; i < DatFiles.Length; i++)
            {
                uint FileOffset = 0;
                string Format = "\0\0\0\0";
                DatFiles[i] = (oHeader.DatFiles[i].offsetToOffset, oHeader.DatFiles[i].offsetToFormat, FileOffset, Format);
            }
            foreach (var item in Final_Occupied_Region)
            {
                if (item.fileID >= 0 && item.fileID < DatFiles.Length)
                {
                    DatFiles[item.fileID].FinalOffsetToFile = item.StartOffset;
                    DatFiles[item.fileID].FinalFormat = item.Format;
                }
            }

            // preenche caso o ultimo seja vazio
            if (DatFiles[DatFiles.Length - 1].FinalOffsetToFile == 0)
            {
                DatFiles[DatFiles.Length - 1].FinalOffsetToFile = lastBlock.StartOffset;
            }

            // preenche os vazios faltantes
            for (int i = DatFiles.Length - 2; i >= 0; i--)
            {
                if (DatFiles[i].FinalOffsetToFile == 0)
                {
                    DatFiles[i].FinalOffsetToFile = DatFiles[i + 1].FinalOffsetToFile;
                }

            }

            // final
            if (ExtraEmptyFileID_List.Count != 0 || ExtraDatOffset_List.Count != 0)
            {
                ExtraEmptyFileID = ExtraEmptyFileID_List.OrderBy(x => x).ToArray();
                ExtraDatOffset = ExtraDatOffset_List.OrderBy(x => x).ToArray();
                HasExtraData = true;
            }

            datFileBytesLength = lastBlock.StartOffset;

        }

        internal class OrganizedBlock
        {
            public uint StartOffset = 0;
            public int Length = 0;
            public uint EndOffset = 0;
            public bool IsOccupied = false; // é true quando tem arquivo
            public int fileID = -1; // id do arquivo, usar -1 para outras coisas.
            public string Format = "\0\0\0\0";

            public int GetBlockID() { return blockID; }
            private int blockID = -1;
            private static int blockIdCount = 0;
            public OrganizedBlock() { blockID = blockIdCount++; }

            public OrganizedBlock Clone() 
            {
                OrganizedBlock c = new OrganizedBlock();
                c.StartOffset = this.StartOffset;
                c.Length = this.Length;
                c.EndOffset = this.EndOffset;
                c.IsOccupied = this.IsOccupied;
                c.fileID = this.fileID;
                c.Format = this.Format;
                return c;
            }
        }
    }
}

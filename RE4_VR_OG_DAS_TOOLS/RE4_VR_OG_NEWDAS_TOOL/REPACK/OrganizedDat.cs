using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RE4_VR_OG_NEWDAS_TOOL_REPACK
{
    internal class OrganizedDat
    {
        public uint datFileBytesLength = 0;
        public uint[] ExtraDatOffset = null;
        public ushort[] ExtraEmptyFileID = null;
        public bool HasExtraData = false;
         
        public OrganizedDat(ref DatInfo[] datGroup)
        {
            //calculo Header
            int datHeaderLength = 16 + (4 * datGroup.Length * 2);
            datHeaderLength = ((datHeaderLength + 31) / 32) * 32; // alinhamento

            List<OrganizedBlock> blocksOccupied = new List<OrganizedBlock>();
            List<OrganizedBlock> blocksFree = new List<OrganizedBlock>();

            OrganizedBlock header = new OrganizedBlock();
            header.StartOffset = 0;
            header.Length = datHeaderLength;
            header.EndOffset = (uint)datHeaderLength;
            header.IsOccupied = true;
            header.fileID = -1;
            blocksOccupied.Add(header);

            List<(int Id, int Length)> others = new List<(int Id, int Length)>();

            for (int id = 0; id < datGroup.Length; id++)
            {
                if (datGroup[id].Offset > 0)
                {
                    OrganizedBlock filee = new OrganizedBlock();
                    filee.StartOffset = datGroup[id].Offset;
                    filee.Length = datGroup[id].Length;
                    filee.EndOffset = (uint)(datGroup[id].Offset + datGroup[id].Length);
                    filee.IsOccupied = true;
                    filee.fileID = id;
                    blocksOccupied.Add(filee);
                }
                else
                {
                    others.Add((id, datGroup[id].Length));
                }
            }

            blocksOccupied = blocksOccupied.OrderBy(x => x.StartOffset).ToList();

            for (int i = 0; i < blocksOccupied.Count -1; i++)
            {
                uint startOffset = blocksOccupied[i].EndOffset;
                uint endOffset = blocksOccupied[i+1].StartOffset;
                int length = (int)((long)endOffset - (long)startOffset);

                if (length > 0)
                {
                    OrganizedBlock freee = new OrganizedBlock();
                    freee.StartOffset = startOffset;
                    freee.Length = length;
                    freee.EndOffset = endOffset;
                    freee.IsOccupied = false;
                    freee.fileID = -2;
                    blocksFree.Add(freee);
                }
            }

            blocksFree = blocksFree.OrderByDescending(x => x.Length).ToList();

            // last
            OrganizedBlock lastBlock;
            {
                uint startOffset = blocksOccupied[blocksOccupied.Count - 1].EndOffset;
                uint endOffset = int.MaxValue;
                int length = (int)((long)endOffset - (long)startOffset);

                OrganizedBlock freee = new OrganizedBlock();
                freee.StartOffset = startOffset;
                freee.Length = length;
                freee.EndOffset = endOffset;
                freee.IsOccupied = false;
                freee.fileID = -3;
                blocksFree.Add(freee);
                lastBlock = freee;
            }

            others = others.OrderByDescending(x => x.Length).ToList();

            // proxima etapa, tira do espaço free
            // preencher os arquivos faltantes

            HashSet<ushort> ExtraEmptyFileID_List = new HashSet<ushort>();

            foreach (var item in others)
            {
                if (item.Length > 0)
                {
                    for (int i = 0; i < blocksFree.Count; i++)
                    {
                        if (item.Length <= blocksFree[i].Length)
                        {
                            OrganizedBlock filee = new OrganizedBlock();
                            filee.StartOffset = blocksFree[i].StartOffset;
                            filee.Length = item.Length;
                            filee.EndOffset = (uint)(blocksFree[i].StartOffset + item.Length);
                            filee.IsOccupied = true;
                            filee.fileID = item.Id;
                            blocksOccupied.Add(filee);

                            blocksFree[i].Length -= item.Length;
                            blocksFree[i].StartOffset = (uint)(blocksFree[i].StartOffset + item.Length);

                            if (blocksFree[i].Length <= 0)
                            {
                                var toRemove = blocksFree[i];
                                blocksFree.RemoveAll(x => x.GetBlockID() == toRemove.GetBlockID());
                            }

                            break;
                        }

                    }

                }
                else
                {
                    ExtraEmptyFileID_List.Add((ushort)item.Id);
                }

            }

            // conteudo vazio
            HashSet<uint> ExtraDatOffset_List = new HashSet<uint>();
            blocksFree.RemoveAll(x => x.GetBlockID() == lastBlock.GetBlockID());
            
            foreach (var item in blocksFree)
            {
                ExtraDatOffset_List.Add(item.StartOffset);
            }

            // define os offsets
            foreach (var item in blocksOccupied)
            {
                if (item.fileID >= 0 && item.fileID < datGroup.Length)
                {
                    datGroup[item.fileID].Offset = item.StartOffset;
                }
            }

            // preenche caso o ultimo seja vazio
            if (datGroup[datGroup.Length -1].Offset == 0)
            {
                datGroup[datGroup.Length - 1].Offset = lastBlock.StartOffset;
            }

            // preenche os vazios faltantes
            for (int i = datGroup.Length - 2; i >= 0; i--)
            {
                if (datGroup[i].Offset == 0)
                {
                    datGroup[i].Offset = datGroup[i + 1].Offset;
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


            public int GetBlockID() { return blockID; }
            private int blockID = -1;
            private static int blockIdCount = 0;
            public OrganizedBlock() {blockID = blockIdCount++; }
        }

    }
}

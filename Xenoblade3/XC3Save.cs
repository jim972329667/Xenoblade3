using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenoblade3
{
    public class XC3Save
    {
        public Byte[] BaseData { get; private set; }
        private static readonly Dictionary<string, Loc> LOC = new Dictionary<string, Loc>()
        {
            { "Money", new Loc(0x20, sizeof(UInt32))},
            { "Complete", new Loc(0xA6B, 0x1)},
            { "Characters", new Loc(0xE3A0, Character.SIZE * Character.MaxCount)},
            { "Souls", new Loc(0x53AA0, Soul.SIZE * Soul.MaxCount)},//6* 0x3C
            { "GemsHeader", new Loc(0x53C08, 0x28)},
            { "ItemBox", new Loc(0x53C78, ItemBox.SIZE)},
        };
        public UInt32 Money { get; set; }
        public Byte[] Complete { get; set; }
        public Character[] Characters { get; set; }
        public Soul[] Souls { get; set; }
        public UInt16[] GemsHeader { get; set; }
        public ItemBox ItemBox { get; set; }
        public XC3Save(byte[] data)
        {
            BaseData = data;

            Money = BitConverter.ToUInt32(data.GetByteSubArray(LOC["Money"].StartLoc, LOC["Money"].Length), 0);

            Complete = data.GetByteSubArray(LOC["Complete"].StartLoc, LOC["Complete"].Length);

            Characters = new Character[Character.MaxCount];
            for (int i = 0; i < Characters.Length; i++)
                Characters[i] = new Character(data.GetByteSubArray(LOC["Characters"].StartLoc + (i * Character.SIZE), Character.SIZE));

            Souls = new Soul[Soul.MaxCount];
            for (int i = 0; i < Souls.Length; i++)
                Souls[i] = new Soul(data.GetByteSubArray(LOC["Souls"].StartLoc + (i * Soul.SIZE), Soul.SIZE));

            GemsHeader = new UInt16[20];
            for (int i = 0; i < GemsHeader.Length; i++)
                GemsHeader[i] = BitConverter.ToUInt16(data.GetByteSubArray(LOC["GemsHeader"].StartLoc + (i * 2), 2), 0);

            ItemBox = new ItemBox(data.GetByteSubArray(LOC["ItemBox"].StartLoc, LOC["ItemBox"].Length));
        }
        public virtual byte[] ToRawData()
        {
            List<Byte> tmp = new List<Byte>();
            FixGems();

            LOC["Money"].Data = BitConverter.GetBytes(Money);

            LOC["Complete"].Data = Complete;

            foreach (var character in Characters)
            {
                tmp.AddRange(character.ToRawData());
            }
            LOC["Characters"].Data = tmp.ToArray();
            tmp.Clear();

            foreach (var soul in Souls)
            {
                tmp.AddRange(soul.ToRawData());
            }
            LOC["Souls"].Data = tmp.ToArray();
            tmp.Clear();

            foreach (var gem in GemsHeader)
            {
                tmp.AddRange(BitConverter.GetBytes(gem));
            }
            LOC["GemsHeader"].Data = tmp.ToArray();
            tmp.Clear();

            LOC["ItemBox"].Data = ItemBox.ToRawData();

            List<Loc> list = new List<Loc>();
            foreach (var loc in LOC)
            {
                list.Add(loc.Value);
            }
            return Util.GetWholeData(list, BaseData);
        }
        private void FixGems()
        {
            var list = GetGemsList();
            for(int i = 0; i < GemsHeader.Length; i++)
            {
                GemsHeader[i] = GetMaxNum(12010 + i * 10, list);
            }
        }
        private List<uint> GetGemsList()
        {
            List<uint> list = new List<uint>();
            foreach (var gem in ItemBox.Item_Gems)
            {
                if(gem.ID != 0)
                {
                    list.Add((uint) gem.ID);
                }
            }
            list.Sort();
            return list;
        }
        private UInt16 GetMaxNum(int end, List<uint> list)
        {
            UInt16 value = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if(list[i] < end)
                {
                    value = (ushort)list[i];
                }
                else if(list[i] == end)
                {
                    value = (ushort)end;
                    break;
                }
            }
            return value;
        }
    }
    public class Loc : IComparable<Loc>
    {
        public int StartLoc { get; set; }
        public int Length { get; set; }
        public Byte[] Data { get; set; }
        public Loc(int start, int length)
        {
            StartLoc = start;
            Length = length;
        }

        public int CompareTo(Loc other)
        {
            if (this.StartLoc > other.StartLoc)
            {
                return 1;
            }
            else if (this.StartLoc < other.StartLoc)
            {
                return -1;
            }
            else
            {
                return 0;
            }

        }
    }

}

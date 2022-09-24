using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenoblade3
{
    public class Character
    {
        public const int SIZE = 0x115C;
        public const int MaxCount = 64;

        public string Name { get; set; }
        public UInt32 Level { get; set; }
        public UInt32 Exp { get; set; }
        public UInt32 BounsExp { get; set; }
        public byte[] Unknow1 { get; set; }//4
        public UInt32 ID { get; set; }
        public Career[] Careers { get; set; }
        public byte[] Unknow2 { get; set; }//2

        public Character(byte[] data)
        {
            Level = BitConverter.ToUInt32(data.GetByteSubArray(0, 4), 0);
            Exp = BitConverter.ToUInt32(data.GetByteSubArray(4, 4), 0);
            BounsExp = BitConverter.ToUInt32(data.GetByteSubArray(8, 4), 0);
            Unknow1 = data.GetByteSubArray(12, 4);
            ID = BitConverter.ToUInt32(data.GetByteSubArray(16, 4), 0);
            Careers = new Career[Career.MaxCount];
            for (int i = 0; i < Careers.Length; i++)
            {
                Careers[i] = new Career(data.GetByteSubArray(20 + (i * Career.SIZE), Career.SIZE));
                Careers[i].CareerID = i + 1;
            }
                
            Unknow2 = data.GetByteSubArray(0x1114, 0x48);
        }
        public virtual Byte[] ToRawData()
        {
            List<Byte> result = new List<Byte>();

            result.AddRange(BitConverter.GetBytes(Level));
            result.AddRange(BitConverter.GetBytes(Exp));
            result.AddRange(BitConverter.GetBytes(BounsExp));
            result.AddRange(Unknow1);
            result.AddRange(BitConverter.GetBytes(ID));
            for (int i = 0; i < Careers.Length; i++)
            {
                result.AddRange(Careers[i].ToRawData());
            }
            result.AddRange(Unknow2);

            return result.ToArray();
        }
    }
    public class Career
    {
        public const int SIZE = 0x44;
        public const int MaxCount = 64;
        public int CareerID { get; set; }
        public UInt32 RankExp { get; set; }
        public UInt16 Fetters { get; set; }
        public byte RankLevel { get; set; }
        public byte Unknow_0x1 { get; set; }
        private Byte[] Gems { get; set; }
        public Byte[] Unknow_0x7 { get; }//0xFF * 7
        public UInt16[] IDs { get; set; } //14   前四个战技     IDs[4] - IDs[6] 额外战技   IDs[7] - IDs[10] 固定技能  IDs[11] - IDs[13] 额外技能
        public UInt16 Arts_1
        {
            get { return IDs[4]; }
            set { IDs[4] = value; }
        }
        public UInt16 Arts_2
        {
            get { return IDs[5]; }
            set { IDs[5] = value; }
        }
        public UInt16 Arts_3
        {
            get { return IDs[6]; }
            set { IDs[6] = value; }
        }
        public UInt16 Skill_1
        {
            get { return IDs[11]; }
            set { IDs[11] = value; }
        }
        public UInt16 Skill_2
        {
            get { return IDs[12]; }
            set { IDs[12] = value; }
        }
        public UInt16 Skill_3
        {
            get { return IDs[13]; }
            set { IDs[13] = value; }
        }
        public UInt16 Unknow_Demarcate { get; }//0xFFFF
        public Equip[] Equips { get; set; }
        public UInt16 UnknowNum { get; set; }
        public Career(Byte[] data)
        {
            RankExp = BitConverter.ToUInt32(data.GetByteSubArray(0, 4), 0);
            Fetters = BitConverter.ToUInt16(data.GetByteSubArray(4, 2), 0);
            RankLevel = data.GetByte(6);
            Unknow_0x1 = data.GetByte(7);
            Gems = data.GetByteSubArray(8, 3);
            Unknow_0x7 = data.GetByteSubArray(11, 7);
            IDs = new UInt16[14];
            for (int i = 0; i < IDs.Length; i++)
                IDs[i] = BitConverter.ToUInt16(data.GetByteSubArray(18 + (i * sizeof(UInt16)), sizeof(UInt16)), 0);
            Unknow_Demarcate = BitConverter.ToUInt16(data.GetByteSubArray(0x2E, 2), 0);
            Equips = new Equip[Equip.MaxCount];
            for (int i = 0; i < Equips.Length; i++)
                Equips[i] = new Equip(data.GetByteSubArray(0x30 + (i * Equip.SIZE), Equip.SIZE));
            UnknowNum = BitConverter.ToUInt16(data.GetByteSubArray(0x42, 2), 0);
        }
        public virtual Byte[] ToRawData()
        {
            List<Byte> result = new List<Byte>();

            result.AddRange(BitConverter.GetBytes(RankExp));
            result.AddRange(BitConverter.GetBytes(Fetters));
            result.Add(RankLevel);
            result.Add(Unknow_0x1);
            result.AddRange(Gems);
            result.AddRange(Unknow_0x7);
            for (int i = 0; i < IDs.Length; i++)
            {
                result.AddRange(BitConverter.GetBytes(IDs[i]));
            }
            result.AddRange(BitConverter.GetBytes(Unknow_Demarcate));
            for (int i = 0; i < Equips.Length; i++)
            {
                result.AddRange(Equips[i].ToRawData());
            }
            result.AddRange(BitConverter.GetBytes(UnknowNum));

            return result.ToArray();
        }


    }
    public class Equip
    {
        public const int SIZE = 0x6;
        public const int MaxCount = 3;
        public UInt16 ID { get; set; }
        public UInt16 Serial { get; set; }
        public UInt16 Type { get; set; }
        public Equip(Byte[] data)
        {
            ID = BitConverter.ToUInt16(data.GetByteSubArray(0, 2), 0);
            Serial = BitConverter.ToUInt16(data.GetByteSubArray(2, 2), 0);
            Type = BitConverter.ToUInt16(data.GetByteSubArray(4, 2), 0);
        }
        public virtual Byte[] ToRawData()
        {
            List<Byte> result = new List<Byte>();

            result.AddRange(BitConverter.GetBytes(ID));
            result.AddRange(BitConverter.GetBytes(Serial));
            result.AddRange(BitConverter.GetBytes(Type));

            return result.ToArray();
        }
    }
}

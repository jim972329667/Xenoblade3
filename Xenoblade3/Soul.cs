using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenoblade3
{
    public class Soul
    {
        public const int SIZE = 0x3C;
        public const int MaxCount = 6;
        public byte[] Unknow1 { get; set; }
        public UInt32 SoulCount { get; set; }
        public byte[] Unknow2 { get; set; }
        public Soul(byte[] data)
        {
            Unknow1 = data.GetByteSubArray(0, 0xC);
            SoulCount = BitConverter.ToUInt32(data.GetByteSubArray(0xC, 4), 0);
            Unknow2 = data.GetByteSubArray(0x10, 0x2C);
        }
        public virtual Byte[] ToRawData()
        {
            List<Byte> result = new List<Byte>();
            result.AddRange(Unknow1);
            result.AddRange(BitConverter.GetBytes(SoulCount));
            result.AddRange(Unknow2);
            return result.ToArray();
        }
    }
}

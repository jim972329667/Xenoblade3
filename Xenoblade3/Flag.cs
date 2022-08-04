using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenoblade3
{
    public class Flag
    {
        public bool[] flags { get; set; }
        public Flag(Byte data)
        {
            flags = new bool[8];
            for (int i = 0; i < flags.Length; i++)
            {
                int x = data >> i & 1;
                if (x == 1)
                    flags[i] = true;
                else
                    flags[i] = false;
            }
        }
        public virtual Byte ToRawData()
        {
            int a = 0;
            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i])
                {
                    a |= (0x1 << i);
                }
                else
                {
                    a &= ~(0x1 << i);
                }
            }
            return BitConverter.GetBytes(a)[0];
        }
    }
}

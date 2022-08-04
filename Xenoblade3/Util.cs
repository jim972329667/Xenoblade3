using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenoblade3
{
    public static class Util
    {
        public static Byte[] GetByteSubArray(this Byte[] data, int startLoc, int length)
        {
            Byte[] value = new Byte[length];

            for (int i = 0; i < value.Length; i++)
                value[i] = data[startLoc + i];

            return value;
        }
        public static Byte GetByte(this Byte[] data, int startLoc)
        {
            return data[startLoc];
        }
        public static Byte[] GetWholeData(List<Loc> list, Byte[] basedata)
        {
            List<Loc> newlist = list;
            newlist.Sort();
            List<Byte> result = new List<Byte>();

            int listpoint = 0;
            for (int i = 0; i < basedata.Length; i++)
            {
                if (i < newlist[listpoint].StartLoc)
                {
                    result.Add(basedata[i]);
                }
                else if (i == newlist[listpoint].StartLoc)
                {
                    if (newlist[listpoint].Length == 1)
                    {
                        result.AddRange(newlist[listpoint].Data);
                        listpoint++;
                    }
                    else
                        result.AddRange(newlist[listpoint].Data);
                }
                else
                {
                    if (i == newlist[listpoint].StartLoc + newlist[listpoint].Length - 1)
                    {
                        if (listpoint + 1 < newlist.Count)
                            listpoint++;
                    }
                    else if (i > newlist[listpoint].StartLoc + newlist[listpoint].Length - 1)
                    {
                        result.Add(basedata[i]);
                    }
                }
            }
            return result.ToArray();
        }
    }
}

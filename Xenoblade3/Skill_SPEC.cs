using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenoblade3
{
    public class SPEC
    {
        public const int SIZE = 0x2C;
        public List<Single_SPEC> SPEC_List = new List<Single_SPEC>();
        public SPEC(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                var tmp = new Flag(data[i]);
                for(int j = 1; j <= 4; j++)
                {
                    SPEC_List.Add(new Single_SPEC(i * 4 + j, tmp.flags[j * 2 - 2]));
                }
            }
        }
        public virtual Byte[] ToRawData()
        {
            List<Byte> result = new List<Byte>();
            int count = 0;
            var tmp = new Flag(0);
            for (int i = 0;i<SPEC_List.Count;i++)
            {
                tmp.flags[count] = SPEC_List[i].IsGet;
                count += 2;
                if(count > 6)
                {
                    count = 0;
                    result.Add(tmp.ToRawData());
                    tmp = new Flag(0);
                }
            }
            return result.ToArray();
        }
    }
    public class Single_SPEC
    {
        public bool IsGet { get; set; }
        public int Index {  get; private set; }
        public string Name { get; set; }  
        public Single_SPEC(int num, bool isget)
        {
            Index = num;
            IsGet = isget;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenoblade3
{
    public class Item : IComparable<Item>
    {
        public const int SIZE = 0x10;
        public UInt16 ID { get; set; }
        public string[] Names { get; set; }
        public int Language { get; set; }
        public string Name
        {
            get
            {
                if (Names != null)
                {
                    if (Names.Length <= Language)
                        return Names[0];
                    else
                        return Names[Language];
                }
                else
                    return string.Empty;
            }
        }
        public UInt16 Serial { get; set; }
        public UInt32 Type { get; set; }
        public UInt32 GetSerial { get; set; }
        public UInt16 Count { get; set; }
        public UInt16 IsNew { get; set; }//1 = old, 5 = new
        public bool IsDeleted 
        { 
            set 
            { 
                this.Delete();
            }
        }

        public Item()
        {
            ID = 0;
            Names = null;
            Serial = 0;
            Type = 0;
            Count = 0;
            IsNew = 0;
            GetSerial = 0;
        }

        public Item(byte[] data)
        {
            if (data is null)
            {
                ID = 0;
                Serial = 0;
                Type = 0;
                Count = 0;
                IsNew = 0;
                GetSerial = 0;
            }

            ID = BitConverter.ToUInt16(data.GetByteSubArray(0, 2), 0);
            Serial = BitConverter.ToUInt16(data.GetByteSubArray(2, 2), 0);
            Type = BitConverter.ToUInt32(data.GetByteSubArray(4, 4), 0);
            GetSerial = BitConverter.ToUInt32(data.GetByteSubArray(8, 4), 0);
            Count = BitConverter.ToUInt16(data.GetByteSubArray(12, 2), 0);
            IsNew = BitConverter.ToUInt16(data.GetByteSubArray(14, 2), 0);
        }
        public virtual Byte[] ToRawData()
        {
            List<Byte> result = new List<Byte>();
            result.AddRange(BitConverter.GetBytes(ID));
            result.AddRange(BitConverter.GetBytes(Serial));
            result.AddRange(BitConverter.GetBytes(Type));
            result.AddRange(BitConverter.GetBytes(GetSerial));
            result.AddRange(BitConverter.GetBytes(Count));
            result.AddRange(BitConverter.GetBytes(IsNew));
            return result.ToArray();
        }
        public override string ToString()
        {
            return $"{ID};{Type}";
        }

        public int CompareTo(Item other)
        {
            if (this.ID > other.ID)
                return 1;
            else if (this.ID < other.ID)
                return -1;
            else
                return 0;
        }
        public void Delete()
        {
            ID = 0;
            Serial = 0;
            Type = 0;
            GetSerial = 0;
            Count = 0;
            IsNew = 0;
            Names = null;
        }
    }
    public class ItemBox
    {
        public const int SIZE = 0x10 * 0x112C + 0x28;
        private static readonly Dictionary<string, int> LOC = new Dictionary<string, int>()
        {
            { "Item_GetCount", 0},
            { "Unknow", 4},
            { "Item_Ether", 0x28},//type = 1    0x10
            { "Item_Gems", 0x128},//type = 2    0x12C
            { "Item_Collectibles", 0x13E8},//type = 3   0x5DC
            { "Item_Unknow", 0x71A8},//type = 4   0x320
            { "Item_Accessories", 0xA3A8},//type = 5   0x5DC
            { "Item_Key", 0x10168},//type = 7     0x118
        };
        public UInt32 Item_GetCount { get; set; }
        public Byte[] Unknow { get; set; }
        public Item[] Item_Ether { get; set; }
        public Item[] Item_Gems { get; set; }
        public Item[] Item_Collectibles { get; set; }
        public Item[] Item_Unknow { get; set; }
        public Item[] Item_Accessories { get; set; }
        public Item[] Item_Key { get; set; }
        public bool HasEther { get; set; } = false;
        private int _SelectLanguage = 0;
        public int SelectLanguage 
        {
            get
            {
                return _SelectLanguage;
            } 
            set 
            { 
                _SelectLanguage = value;
                ChangeSelectLanguage();
            } 
        }
        public Dictionary<UInt16,string[]> collectiblesNames { get; set; }
        public Dictionary<UInt16, string[]> keyItemsNames { get; set; }
        public Dictionary<UInt16, string[]> accessoriesNames { get; set; }
        public Dictionary<UInt16, string[]> gemNames { get; set; }

        public ItemBox(byte[] data)
        {
            //Fill item name dictionaries.
            this.FillCollectiblesDict();
            this.FillAccessoriesDict();
            this.FillKeyitemsDict();
            this.FillGemDict();

            Item_GetCount = BitConverter.ToUInt32(data.GetByteSubArray(LOC["Item_GetCount"], 4), 0);
            Unknow = data.GetByteSubArray(LOC["Unknow"], 0x24);
            Item_Ether = new Item[0x10];
            for (int i = 0; i < Item_Ether.Length; i++)
            {
                Item_Ether[i] = new Item(data.GetByteSubArray(LOC["Item_Ether"] + (i * Item.SIZE), Item.SIZE));
                if (Item_Ether[i].ID == 6001)
                {
                    HasEther = true;
                }
            }

            Item_Gems = new Item[0x12C];
            for (int i = 0; i < Item_Gems.Length; i++)
            {
                Item_Gems[i] = new Item(data.GetByteSubArray(LOC["Item_Gems"] + (i * Item.SIZE), Item.SIZE));
                Item_Gems[i].Names = this.GetNameByID(Item_Gems[i].ID, gemNames);
            }


            Item_Collectibles = new Item[0x5DC];
            for (int i = 0; i < Item_Collectibles.Length; i++)
            {
                Item_Collectibles[i] = new Item(data.GetByteSubArray(LOC["Item_Collectibles"] + (i * Item.SIZE), Item.SIZE));
                Item_Collectibles[i].Names = this.GetNameByID(Item_Collectibles[i].ID, collectiblesNames);
            }
                

            Item_Unknow = new Item[0x320];
            for (int i = 0; i < Item_Unknow.Length; i++)
                Item_Unknow[i] = new Item(data.GetByteSubArray(LOC["Item_Unknow"] + (i * Item.SIZE), Item.SIZE));

            Item_Accessories = new Item[0x5DC];
            for (int i = 0; i < Item_Accessories.Length; i++)
            {
                Item_Accessories[i] = new Item(data.GetByteSubArray(LOC["Item_Accessories"] + (i * Item.SIZE), Item.SIZE));
                Item_Accessories[i].Names = this.GetNameByID(Item_Accessories[i].ID, accessoriesNames);
            }

            Item_Key = new Item[0x118];
            for (int i = 0; i < Item_Key.Length; i++)
            {
                Item_Key[i] = new Item(data.GetByteSubArray(LOC["Item_Key"] + (i * Item.SIZE), Item.SIZE));
                Item_Key[i].Names = this.GetNameByID(Item_Key[i].ID, keyItemsNames);
            }
                
        }

        private void ChangeSelectLanguage()
        {
            foreach(var item in Item_Accessories)
            {
                item.Language = SelectLanguage;
            }
            foreach (var item in Item_Gems)
            {
                item.Language = SelectLanguage;
            }
            foreach (var item in Item_Collectibles)
            {
                item.Language = SelectLanguage;
            }
            foreach (var item in Item_Key)
            {
                item.Language = SelectLanguage;
            }
        }
        public void FillCollectiblesDict()
        {
            collectiblesNames = new Dictionary<UInt16, string[]>();

            foreach (var line in File.ReadLines(@"Resources/Items/collectibles.txt").Skip(1))
            {
                var tempLine = line.Split('\t');
                string[] tmp = tempLine.Skip(1).Take(tempLine.Length - 1).ToArray();
                collectiblesNames.Add(UInt16.Parse(tempLine[0]), tmp);
            }            
        }

        public void FillAccessoriesDict()
        {
            accessoriesNames = new Dictionary<UInt16, string[]>();

            foreach (var line in File.ReadLines(@"Resources/Items/accessories.txt").Skip(1))
            {
                var tempLine = line.Split('\t');
                string[] tmp = tempLine.Skip(1).Take(tempLine.Length - 1).ToArray();
                accessoriesNames.Add(UInt16.Parse(tempLine[0]), tmp);
            }
        }

        public void FillGemDict()
        {
            gemNames = new Dictionary<UInt16, string[]>();

            foreach (var line in File.ReadLines(@"Resources/Items/gems.txt").Skip(1))
            {
                var tempLine = line.Split('\t');
                string[] tmp = tempLine.Skip(1).Take(tempLine.Length - 1).ToArray();
                gemNames.Add(UInt16.Parse(tempLine[0]), tmp);
            }
        }

        public void FillKeyitemsDict()
        {
            keyItemsNames = new Dictionary<UInt16, string[]>();

            foreach (var line in File.ReadLines(@"Resources/Items/keyItems.txt").Skip(1))
            {
                var tempLine = line.Split('\t');
                string[] tmp = tempLine.Skip(1).Take(tempLine.Length - 1).ToArray();

                keyItemsNames.Add(UInt16.Parse(tempLine[0]), tmp);
            }
        }

        public string[] GetNameByID(UInt32 ID, Dictionary<UInt16, string[]> dict)
        {
            string[] name;
            if (dict.TryGetValue((ushort)ID, out name))
                return name;
            return null;
        }

        public void FixSerial()
        {
            UInt32 GetSerial = 0;


            List<Item> TempItems = new List<Item>();

            foreach (var item in Item_Ether)
            {
                if (item != null && item != null && item.ID != 0)
                {
                    TempItems.Add(item);
                }
            }
            Item_Ether = new Item[Item_Ether.Length];
            if (TempItems.Count > 0)
            {
                for (int i = 0; i < TempItems.Count; i++)
                {
                    TempItems[i].Type = 1;
                    TempItems[i].Serial = (UInt16)i;
                    GetSerial += TempItems[i].Count;
                    TempItems[i].GetSerial = GetSerial;
                    Item_GetCount = GetSerial;
                    Item_Ether[i] = TempItems[i];
                }
            }
            TempItems.Clear();


            foreach (var item in Item_Gems)
            {
                if (item != null && item.ID != 0)
                {
                    TempItems.Add(item);
                }
            }
            Item_Gems = new Item[Item_Gems.Length];
            if (TempItems.Count > 0)
            {
                for (int i = 0; i < TempItems.Count; i++)
                {
                    TempItems[i].Type = 2;
                    TempItems[i].Serial = (UInt16)i;
                    GetSerial += TempItems[i].Count;
                    TempItems[i].GetSerial = GetSerial;
                    Item_GetCount = GetSerial;
                    Item_Gems[i] = TempItems[i];
                }
            }
            TempItems.Clear();


            foreach (var item in Item_Collectibles)
            {
                if (item != null && item.ID != 0)
                {
                    TempItems.Add(item);
                }
            }
            Item_Collectibles = new Item[Item_Collectibles.Length];
            if (TempItems.Count > 0)
            {
                for (int i = 0; i < TempItems.Count; i++)
                {
                    TempItems[i].Type = 3;
                    TempItems[i].Serial = (UInt16)i;
                    GetSerial += TempItems[i].Count;
                    TempItems[i].GetSerial = GetSerial;
                    Item_GetCount = GetSerial;
                    Item_Collectibles[i] = TempItems[i];
                }
            }
            TempItems.Clear();


            foreach (var item in Item_Unknow)
            {
                if (item != null && item.ID != 0)
                {
                    TempItems.Add(item);
                }
            }
            Item_Unknow = new Item[Item_Unknow.Length];
            if (TempItems.Count > 0)
            {
                for (int i = 0; i < TempItems.Count; i++)
                {
                    TempItems[i].Type = 4;
                    TempItems[i].Serial = (UInt16)i;
                    GetSerial += TempItems[i].Count;
                    TempItems[i].GetSerial = GetSerial;
                    Item_GetCount = GetSerial;
                    Item_Unknow[i] = TempItems[i];
                }
            }
            TempItems.Clear();


            foreach (var item in Item_Accessories)
            {
                if (item != null && item.ID != 0)
                {
                    TempItems.Add(item);
                }
            }
            Item_Accessories = new Item[Item_Accessories.Length];
            if (TempItems.Count > 0)
            {
                for (int i = 0; i < TempItems.Count; i++)
                {
                    TempItems[i].Type = 5;
                    TempItems[i].Serial = (UInt16)i;
                    GetSerial += TempItems[i].Count;
                    TempItems[i].GetSerial = GetSerial;
                    Item_GetCount = GetSerial;
                    Item_Accessories[i] = TempItems[i];
                }
            }
            TempItems.Clear();


            foreach (var item in Item_Key)
            {
                if (item != null && item.ID != 0)
                {
                    TempItems.Add(item);
                }
            }
            Item_Key = new Item[Item_Key.Length];
            if (TempItems.Count > 0)
            {
                for (int i = 0; i < TempItems.Count; i++)
                {
                    TempItems[i].Type = 7;
                    TempItems[i].Serial = (UInt16)i;
                    GetSerial += TempItems[i].Count;
                    TempItems[i].GetSerial = GetSerial;
                    Item_GetCount = GetSerial;
                    Item_Key[i] = TempItems[i];
                }
            }
            TempItems.Clear();

        }
        public void AddItem(UInt16 ID, UInt32 type, UInt16 Count)
        {
            if (type == 1)
            {
                foreach (var item in Item_Ether)
                {
                    if (item.ID == ID)
                    {
                        item.Count = Count;
                        return;
                    }
                }
                foreach (var item in Item_Ether)
                {
                    if (item.ID == 0)
                    {
                        item.ID = ID;
                        item.Count = Count;
                        item.IsNew = 5;
                        return;
                    }
                }
            }
            else if (type == 2)
            {
                foreach (var item in Item_Gems)
                {
                    if (item.ID == ID)
                    {
                        item.Count = Count;
                        return;
                    }
                }
                foreach (var item in Item_Gems)
                {
                    if (item.ID == 0)
                    {
                        this.CreateItem(item, ID, Count, gemNames);
                        return;
                    }
                }
            }
            else if (type == 3)
            {
                foreach (var item in Item_Collectibles)
                {
                    if (item.ID == ID)
                    {
                        item.Count = Count;
                        return;
                    }
                }
                foreach (var item in Item_Collectibles)
                {
                    if (item.ID == 0)
                    {
                        this.CreateItem(item, ID, Count, collectiblesNames);
                        return;
                    }
                }
            }
            else if (type == 4)
            {
                foreach (var item in Item_Unknow)
                {
                    if (item.ID == ID)
                    {
                        item.Count = Count;
                        return;
                    }
                }
                foreach (var item in Item_Unknow)
                {
                    if (item.ID == 0)
                    {
                        item.ID = ID;
                        item.Count = Count;
                        item.IsNew = 5;
                        return;
                    }
                }
            }
            else if (type == 5)
            {
                foreach (var item in Item_Accessories)
                {
                    if (item.ID == ID)
                    {
                        item.Count = Count;
                        return;
                    }
                }
                foreach (var item in Item_Accessories)
                {
                    if (item.ID == 0)
                    {
                        this.CreateItem(item, ID, Count, accessoriesNames);
                        return;
                    }
                }
            }
            else if (type == 7)
            {
                foreach (var item in Item_Key)
                {
                    if (item.ID == ID)
                    {
                        item.Count = Count;
                        return;
                    }
                }
                foreach (var item in Item_Key)
                {
                    if (item.ID == 0)
                    {
                        this.CreateItem(item, ID, Count, keyItemsNames);
                        return;
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void CreateItem(Item item, ushort id, ushort count, Dictionary<ushort, string[]> dict)
        {
            item.Names = this.GetNameByID(id, dict);
            item.ID = id;
            item.Count = count;
            item.IsNew = 5;
        }
        public void RemoveItem(UInt16 ID)
        {
            foreach (var item in Item_Ether)
            {
                if (item.ID == ID)
                {
                    item.Delete();
                    return;
                }
            }
            foreach (var item in Item_Gems)
            {
                if (item.ID == ID)
                {
                    item.Delete();
                    return;
                }
            }
            foreach (var item in Item_Collectibles)
            {
                if (item.ID == ID)
                {
                    item.Delete();
                    return;
                }
            }
            foreach (var item in Item_Unknow)
            {
                if (item.ID == ID)
                {
                    item.Delete();
                    return;
                }
            }
            foreach (var item in Item_Accessories)
            {
                if (item.ID == ID)
                {
                    item.Delete();
                    return;
                }
            }
            foreach (var item in Item_Key)
            {
                if (item.ID == ID)
                {
                    item.Delete();
                    return;
                }
            }
        }
        public virtual Byte[] ToRawData()
        {
            FixSerial();
            List<Byte> result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(Item_GetCount));
            result.AddRange(Unknow);
            foreach (var item in Item_Ether)
            {
                if (item == null)
                    result.AddRange(new byte[0x10]);
                else
                    result.AddRange(item.ToRawData());
            }
            foreach (var item in Item_Gems)
            {
                if (item == null)
                    result.AddRange(new byte[0x10]);
                else
                    result.AddRange(item.ToRawData());
            }
            foreach (var item in Item_Collectibles)
            {
                if (item == null)
                    result.AddRange(new byte[0x10]);
                else
                    result.AddRange(item.ToRawData());
            }
            foreach (var item in Item_Unknow)
            {
                if (item == null)
                    result.AddRange(new byte[0x10]);
                else
                    result.AddRange(item.ToRawData());
            }
            foreach (var item in Item_Accessories)
            {
                if (item == null)
                    result.AddRange(new byte[0x10]);
                else
                    result.AddRange(item.ToRawData());
            }
            foreach (var item in Item_Key)
            {
                if (item == null)
                    result.AddRange(new byte[0x10]);
                else
                    result.AddRange(item.ToRawData());
            }
            return result.ToArray();
        }

        public void AddAllCollectibles(ushort number)
        {
            foreach(var item in collectiblesNames)
            {
                // Skip the following IDs because they break quests.
                if(item.Key == 2152)
                {
                    continue;
                }

                //Only add items that are not blank.
                if(!item.Value.Contains("[blank]"))
                    this.AddItem(item.Key, 3, number);
            }
        }

        public void AddAllAccessories(ushort number)
        {
            foreach(var item in accessoriesNames)
            {
                //Only add items that are not blank.
                if (!item.Value.Contains("[blank]"))
                    this.AddItem(item.Key, 5, number);
            }
        }
    }
}

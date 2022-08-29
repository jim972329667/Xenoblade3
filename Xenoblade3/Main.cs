using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.TabControl;

namespace Xenoblade3
{
    public partial class Main : Form
    {
        private Languages thislanguage = Languages.中文;
        private static readonly Dictionary<Languages, TranslationContext> Context = new Dictionary<Languages, TranslationContext>();
        private XC3Save Save = null;
        private string OpenFileName = string.Empty;
        private OpenFileDialog ofdSaveFile = new OpenFileDialog();
        public Main()
        {
            InitializeComponent();
            thislanguage = (Languages)Enum.Parse(typeof(Languages), Properties.Settings.Default.thisLanguage);
            ChangeLanguage(thislanguage);
        }

        #region Language
        public enum Languages
        {
            English,
            中文,
        }
        public static TranslationContext GetContext(Languages lang)
        {
            if (Context.TryGetValue(lang, out var context))
                return context;
            var lines = LanguageUtil.LoadStringList(lang.ToString());
            Context.Add(lang, context = new TranslationContext(lines));
            return context;
        }
        private void LanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
            thislanguage = (Languages)Enum.Parse(typeof(Languages), toolStripMenuItem.Text);
            ChangeLanguage(thislanguage);
        }
        private void ChangeLanguage(Languages language)
        {
            var context = GetContext(language);
            if(Save != null)
                Save.ItemBox.SelectLanguage = (int)thislanguage;
            Properties.Settings.Default.thisLanguage = language.ToString();
            Properties.Settings.Default.Save();
            LanguageUtil.TranslateInterface(this, context);
            foreach(DataGridViewTextBoxColumn column in ClassDataGridView.Columns)
            {
                column.HeaderText = context.GetTranslatedText($"ClassDataGridView.{column.DataPropertyName}", column.HeaderText);
            }
            foreach (var column in Item_EtherDataGridView.Columns)
            {
                if(column is DataGridViewTextBoxColumn)
                {
                    DataGridViewTextBoxColumn x = (DataGridViewTextBoxColumn)column;
                    x.HeaderText = context.GetTranslatedText($"ItemDataGridView.{x.DataPropertyName}", x.HeaderText);
                }
                else if(column is DataGridViewButtonColumn)
                {
                    DataGridViewButtonColumn x = (DataGridViewButtonColumn)column;
                    x.HeaderText = context.GetTranslatedText($"ItemDataGridView.{x.Name}", x.HeaderText);
                    x.Text = context.GetTranslatedText($"ItemDataGridView.{x.Name}", x.Text);
                }
                    
            }
            foreach (DataGridViewTextBoxColumn column in Item_GemsDataGridView.Columns)
            {
                column.HeaderText = context.GetTranslatedText($"ItemDataGridView.{column.DataPropertyName}", column.HeaderText);
            }
            foreach (DataGridViewTextBoxColumn column in Item_CollectiblesDataGridView.Columns)
            {
                column.HeaderText = context.GetTranslatedText($"ItemDataGridView.{column.DataPropertyName}", column.HeaderText);
            }
            foreach (DataGridViewTextBoxColumn column in Item_AccessoriesDataGridView.Columns)
            {
                column.HeaderText = context.GetTranslatedText($"ItemDataGridView.{column.DataPropertyName}", column.HeaderText);
            }
            foreach (DataGridViewTextBoxColumn column in Item_KeyDataGridView.Columns)
            {
                column.HeaderText = context.GetTranslatedText($"ItemDataGridView.{column.DataPropertyName}", column.HeaderText);
            }

            ItemTypeComboBox.Items.Clear();
            foreach (TabPage x in tabControl2.TabPages)
            {
                ItemTypeComboBox.Items.Add(x.Text);
            }
        }

        #endregion
        private void LoadSave()
        {
            var context = GetContext(thislanguage);

            TryGetNumericUpDownValue(GoldNum, Save.Money);
            IsCompletedCheckBox.Checked = new Flag(Save.Complete[0]).flags[7];

            CharacterList.Items.Clear();
            for(int i = 0; i < Save.Characters.Length; i++)
            {
                Save.Characters[i].Name = context.GetTranslatedText($"CharacterList.{i}", "Unknow");
                CharacterList.Items.Add(Save.Characters[i]);
            }
            Save.ItemBox.SelectLanguage = (int)thislanguage;
            CharacterList.DisplayMember = "Name";

            Item_EtherDataGridView.DataSource = Save.ItemBox.Item_Ether;
            Item_GemsDataGridView.DataSource = Save.ItemBox.Item_Gems;
            Item_CollectiblesDataGridView.DataSource = Save.ItemBox.Item_Collectibles;
            Item_AccessoriesDataGridView.DataSource = Save.ItemBox.Item_Accessories;
            Item_KeyDataGridView.DataSource = Save.ItemBox.Item_Key;
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ofdSaveFile.Filter = "Save file|*.sav";
            DialogResult = ofdSaveFile.ShowDialog();
            if (DialogResult == DialogResult.OK)
            {
                var data = File.ReadAllBytes(ofdSaveFile.FileName);
                Save = new XC3Save(data);
                OpenFileName = ofdSaveFile.FileName;
                LoadSave();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Save == null)
                return;
            var newdata = Save.ToRawData();
            if (Save.BaseData.Length == newdata.Length && OpenFileName != string.Empty)
            {
                File.WriteAllBytes(OpenFileName, newdata);
                MessageBox.Show("Success");
            }
        }

        private void GoldNum_ValueChanged(object sender, EventArgs e)
        {
            if (Save == null)
                return;
            Save.Money = (uint)GoldNum.Value;
        }

        private void IsCompletedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Save == null)
                return;
            Flag flag = new Flag(Save.Complete[0]);
            if(IsCompletedCheckBox.Checked)
                flag.flags[7] = true;
            else
                flag.flags[7] = false;
            Save.Complete[0] = flag.ToRawData(); 

        }

        private void CharacterList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = CharacterList.SelectedIndex;
            if (index != -1)
            {
                TryGetNumericUpDownValue(CharacterExpNum, Save.Characters[index].Exp);
                TryGetNumericUpDownValue(CharacterBounsExpNum, Save.Characters[index].BounsExp);
                TryGetNumericUpDownValue(CharacterLvNum, Save.Characters[index].Level);
                TryGetNumericUpDownValue(CharacterClassIDNum, Save.Characters[index].ID);
                TryGetNumericUpDownValue(SoulPointNum, Save.Souls[index].SoulCount);

                ClassDataGridView.DataSource = Save.Characters[index].Careers;
            }
        }
        private void TryGetNumericUpDownValue(NumericUpDown numericUpDown,uint value)
        {
            if(numericUpDown.Maximum < value)
                numericUpDown.Maximum = value;
            numericUpDown.Value = value;
        }
        private void CharacterExpNum_ValueChanged(object sender, EventArgs e)
        {
            Character character = CharacterList.SelectedItem as Character;
            if (character != null)
            {
                character.Exp = (uint)CharacterExpNum.Value;
            }
        }

        private void CharacterBounsExpNum_ValueChanged(object sender, EventArgs e)
        {
            Character character = CharacterList.SelectedItem as Character;
            if (character != null)
            {
                character.BounsExp = (uint)CharacterBounsExpNum.Value;
            }
        }

        private void CharacterLvNum_ValueChanged(object sender, EventArgs e)
        {
            Character character = CharacterList.SelectedItem as Character;
            if (character != null)
            {
                character.Level = (uint)CharacterLvNum.Value;
            }
        }

        private void CharacterClassIDNum_ValueChanged(object sender, EventArgs e)
        {
            Character character = CharacterList.SelectedItem as Character;
            if (character != null)
            {
                character.ID = (uint)CharacterClassIDNum.Value;
            }
        }

        private void ExportItemButton_Click(object sender, EventArgs e)
        {
            string strJson = string.Empty;
            var opt = new JsonSerializerOptions() { WriteIndented = true };
            Item[] items = new Item[0];
            string fileName = string.Empty;

            //Ether
            if (tabControl2.SelectedIndex == 0)
            {
                fileName = "ether";
                items = (Item[])Item_EtherDataGridView.DataSource;
            }
            //Gems
            else if (tabControl2.SelectedIndex == 1)
            {
                fileName = "gems";
                items = (Item[])Item_GemsDataGridView.DataSource;
            }
            //Collectibles
            else if (tabControl2.SelectedIndex == 2)
            {
                fileName = "collectibles";
                items = (Item[])Item_CollectiblesDataGridView.DataSource;
            }
            //Accessories
            else if (tabControl2.SelectedIndex == 3)
            {
                fileName = "accessories";
                items = (Item[])Item_AccessoriesDataGridView.DataSource;
            }
            //Keyitems
            else if (tabControl2.SelectedIndex == 4)
            {
                fileName = "keyItems";
                items = (Item[])Item_KeyDataGridView.DataSource;                
                
            }

            byte[] utf8bytesJson = JsonSerializer.SerializeToUtf8Bytes<Item[]>(items, opt);
            string strResult = System.Text.Encoding.UTF8.GetString(utf8bytesJson);
                        
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JSON file|*.json";
            saveFileDialog1.Title = "Save an Item File";
            saveFileDialog1.FileName = fileName;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = saveFileDialog1.FileName;
                File.WriteAllText(file, strResult);
            }
        }

        private void ImportItemButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON file|*.json";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string file = openFileDialog.FileName;
                string strResult = File.ReadAllText(file);

                var opt = new JsonSerializerOptions() { WriteIndented = true };
                Item[] items = JsonSerializer.Deserialize<Item[]>(strResult, opt);

                //Ether
                if (tabControl2.SelectedIndex == 0)
                {
                    Item_EtherDataGridView.DataSource = items;
                }
                //Gems
                else if (tabControl2.SelectedIndex == 1)
                {
                    Item_GemsDataGridView.DataSource = items;
                }
                //Collectibles
                else if (tabControl2.SelectedIndex == 2)
                {
                    Item_CollectiblesDataGridView.DataSource = items;
                }
                //Accessories
                else if (tabControl2.SelectedIndex == 3)
                {
                    Item_AccessoriesDataGridView.DataSource = items;
                }
                //Keyitems
                else if (tabControl2.SelectedIndex == 4)
                {
                    Item_KeyDataGridView.DataSource = items;
                }
            }
        }

        private void DeleteItemButton_Click(object sender, EventArgs e)
        {
            if(tabControl2.SelectedIndex == 0)
            {
                for (int i = 0;i< Item_EtherDataGridView.SelectedRows.Count; i++)
                {
                    Item item = (Item)Item_EtherDataGridView.SelectedRows[i].DataBoundItem;
                    item.Delete();
                }
            }
            else if(tabControl2.SelectedIndex == 1)
            {
                for (int i = 0; i < Item_GemsDataGridView.SelectedRows.Count; i++)
                {
                    Item item = (Item)Item_GemsDataGridView.SelectedRows[i].DataBoundItem;
                    item.Delete();
                }
            }
            else if(tabControl2.SelectedIndex == 2)
            {
                for (int i = 0; i < Item_CollectiblesDataGridView.SelectedRows.Count; i++)
                {
                    Item item = (Item)Item_CollectiblesDataGridView.SelectedRows[i].DataBoundItem;
                    item.Delete();
                }
            }
            else if (tabControl2.SelectedIndex == 3)
            {
                for (int i = 0; i < Item_AccessoriesDataGridView.SelectedRows.Count; i++)
                {
                    Item item = (Item)Item_AccessoriesDataGridView.SelectedRows[i].DataBoundItem;
                    item.Delete();
                }
            }
            else if (tabControl2.SelectedIndex == 4)
            {
                for (int i = 0; i < Item_KeyDataGridView.SelectedRows.Count; i++)
                {
                    Item item = (Item)Item_KeyDataGridView.SelectedRows[i].DataBoundItem;
                    item.Delete();
                }
            }
        }

        private void AddItemButton_Click(object sender, EventArgs e)
        {
            if (Save == null)
                return;
            
            UInt16 ID = (ushort)ItemIDNum.Value;
            UInt16 Count = (ushort)ItemCountNum.Value;
            uint Type = 0;
            switch (ItemTypeComboBox.SelectedIndex)
            {
                case 0:
                    Type = 1;
                    break;
                case 1:
                    Type = 2;
                    break;
                case 2:
                    Type = 3;
                    break;
                case 3:
                    Type = 5;
                    break;
                case 4:
                    Type = 7;
                    break;

            }
            Save.ItemBox.AddItem(ID, Type, Count);
        }

        private void SoulPointNum_ValueChanged(object sender, EventArgs e)
        {
            int index = CharacterList.SelectedIndex;
            if (index != -1)
            {
                Save.Souls[index].SoulCount = (uint)SoulPointNum.Value;
            }
        }

        private void AddCollectiblesButton_Click(object sender, EventArgs e)
        {
            if (Save == null)
                return;

            Save.ItemBox.AddAllCollectibles(99);
        }

        private void AddAccessoriesButton_Click(object sender, EventArgs e)
        {
            if (Save == null)
                return;

            Save.ItemBox.AddAllAccessories(20);
        }

        private void ItemDataGridView_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView data = (DataGridView)sender;
            Item[] items = data.DataSource as Item[];
            

            if(e.ColumnIndex == 0)
                items = items.OrderBy(x => x.ID == 0).ThenBy(x => x.Serial).ToArray();
            else if(e.ColumnIndex == 1)
                items = items.OrderBy(x => x.ID == 0).ThenBy(x => x.ID).ToArray();



            data.DataSource = items;
        }
    }
}

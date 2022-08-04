using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
            ChangeLanguage(thislanguage);
        }

        #region Language
        public enum Languages
        {
            中文,
            English,
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
            GoldNum.Value = Save.Money;
            IsCompletedCheckBox.Checked = new Flag(Save.Complete[0]).flags[7];

            CharacterList.Items.Clear();
            for(int i = 0; i < Save.Characters.Length; i++)
            {
                Save.Characters[i].Name = context.GetTranslatedText($"CharacterList.{i}", "Unknow");
                CharacterList.Items.Add(Save.Characters[i]);
            }
            CharacterList.DisplayMember = "Name";

            Item_EtherDataGridView.DataSource = Save.ItemBox.Item_Ether;
            Item_GemsDataGridView.DataSource = Save.ItemBox.Item_Gems;
            Item_CollectiblesDataGridView.DataSource = Save.ItemBox.Item_Collectibles;
            Item_AccessoriesDataGridView.DataSource = Save.ItemBox.Item_Accessories;
            Item_KeyDataGridView.DataSource = Save.ItemBox.Item_Key;
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                File.WriteAllBytes(OpenFileName, newdata);
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
            Character character = CharacterList.SelectedItem as Character;
            if (character != null)
            {
                CharacterExpNum.Value = character.Exp;
                CharacterBounsExpNum.Value = character.BounsExp;
                CharacterLvNum.Value = character.Level;
                CharacterClassIDNum.Value = character.ID;
                ClassDataGridView.DataSource = character.Careers;
            }
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
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Xenoblade3
{
    public sealed class TranslationContext
    {
        public bool AddNew { private get; set; }
        public bool RemoveUsedKeys { private get; set; }
        public const char Separator = '=';
        private readonly Dictionary<string, string> Translation = new Dictionary<string, string>();

        public TranslationContext(IEnumerable<string> content, char separator = Separator)
        {
            var entries = GetContent(content, separator);
            foreach (var kvp in entries.Where(z => !Translation.ContainsKey(z.Key)))
                Translation.Add(kvp.Key, kvp.Value);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetContent(IEnumerable<string> content, char separator)
        {
            foreach (var line in content)
            {
                var index = line.IndexOf(separator);
                if (index < 0)
                    continue;
                var key = line.Substring(0, index);
                var value = line.Substring(index + 1);
                yield return new KeyValuePair<string, string>(key, value);
            }
        }

        public string GetTranslatedText(string val, string fallback)
        {
            if (RemoveUsedKeys)
                Translation.Remove(val);

            if (Translation.TryGetValue(val, out var translated))
                return translated;

            if (AddNew)
                Translation.Add(val, fallback);
            return fallback;
        }

        public IEnumerable<string> Write(char separator = Separator)
        {
            return Translation.Select(z => $"{z.Key}{separator}{z.Value}").OrderBy(z => z.Contains(".")).ThenBy(z => z);
        }

        public void UpdateFrom(TranslationContext other)
        {
            bool oldAdd = AddNew;
            AddNew = true;
            foreach (var kvp in other.Translation)
                GetTranslatedText(kvp.Key, kvp.Value);
            AddNew = oldAdd;
        }

        public void RemoveKeys(TranslationContext other)
        {
            foreach (var kvp in other.Translation)
                Translation.Remove(kvp.Key);
        }
    }
    public static class LanguageUtil
    {
        internal static void TranslateInterface(this Control form, TranslationContext context) => TranslateForm(form, context);
        public static string[] LoadStringList(string language)
        {
            if (language == null)
                return Array.Empty<string>();
            string file = $@"Resources/Form/{language}.txt";
            //var txt = (string)Properties.Resources.ResourceManager.GetObject(language);
            if(!File.Exists(file))
                return Array.Empty<string>();
            var txt = File.ReadAllText(file, Encoding.Default);
            string[] rawlist = txt.TrimEnd('\r', '\n').Split('\n');
            for (int i = 0; i < rawlist.Length; i++)
                rawlist[i] = rawlist[i].TrimEnd('\r');

            return (string[])rawlist.Clone();
        }
        private static void TranslateForm(Control form, TranslationContext context)
        {
            form.SuspendLayout();
            var formname = form.Name;
            // Translate Title
            form.Text = context.GetTranslatedText(formname, form.Text);
            var translatable = GetTranslatableControls(form);
            foreach (var c in translatable)
            {
                if (c is Control r)
                {
                    var current = r.Text;
                    var updated = context.GetTranslatedText($"{formname}.{r.Name}", current);
                    if (!ReferenceEquals(current, updated))
                        r.Text = updated;
                }
                else if (c is ToolStripItem t)
                {
                    var current = t.Text;
                    var updated = context.GetTranslatedText($"{formname}.{t.Name}", current);
                    if (!ReferenceEquals(current, updated))
                        t.Text = updated;
                }
            }
            form.ResumeLayout();
        }
        private static IEnumerable<object> GetTranslatableControls(Control f)
        {
            foreach (var z in f.GetChildrenOfType<Control>())
            {
                switch (z)
                {
                    case ToolStrip menu:
                        foreach (var obj in GetToolStripMenuItems(menu))
                            yield return obj;

                        break;
                    default:
                        if (string.IsNullOrWhiteSpace(z.Name))
                            break;

                        if (z.ContextMenuStrip != null) // control has attached menustrip
                        {
                            foreach (var obj in GetToolStripMenuItems(z.ContextMenuStrip))
                                yield return obj;
                        }

                        if (z is ListControl || z is TextBoxBase || z is LinkLabel || z is NumericUpDown || z is ContainerControl)
                            break; // undesirable to modify, ignore

                        if (!string.IsNullOrWhiteSpace(z.Text))
                            yield return z;
                        break;
                }
            }
        }
        private static IEnumerable<T> GetChildrenOfType<T>(this Control control) where T : class
        {
            foreach (var child in control.Controls.OfType<Control>())
            {
                if (child is T childOfT)
                    yield return childOfT;

                if (!child.HasChildren)
                    continue;

                foreach (var descendant in GetChildrenOfType<T>(child))
                    yield return descendant;
            }
        }
        private static IEnumerable<object> GetToolStripMenuItems(ToolStrip menu)
        {
            foreach (var i in menu.Items.OfType<ToolStripMenuItem>())
            {
                if (!string.IsNullOrWhiteSpace(i.Text))
                    yield return i;
                foreach (var sub in GetToolsStripDropDownItems(i).Where(z => !string.IsNullOrWhiteSpace(z.Text)))
                    yield return sub;
            }
        }
        private static IEnumerable<ToolStripMenuItem> GetToolsStripDropDownItems(ToolStripDropDownItem item)
        {
            foreach (var dropDownItem in item.DropDownItems.OfType<ToolStripMenuItem>())
            {
                yield return dropDownItem;
                if (!dropDownItem.HasDropDownItems) continue;
                foreach (ToolStripMenuItem subItem in GetToolsStripDropDownItems(dropDownItem))
                    yield return subItem;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChooseFontName : Form
    {
        public string FontName { get; set; }
        private readonly List<string> _fontNames;

        public ChooseFontName()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = Configuration.Settings.Language.Main.Menu.ContextMenu.FontName;
            labelShortcutsSearch.Text = Configuration.Settings.Language.General.Search;
            buttonSearchClear.Text = Configuration.Settings.Language.DvdSubRip.Clear;
            textBoxSearch.Left = labelShortcutsSearch.Left + labelShortcutsSearch.Width + 5;
            buttonSearchClear.Left = textBoxSearch.Left + textBoxSearch.Width + 5;
            groupBoxPreview.Text = Configuration.Settings.Language.General.Preview;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            buttonOK.Enabled = false;
            _fontNames = new List<string>();
            listBox1.Items.Clear();
            foreach (var x in FontFamily.Families)
            {
                if (!string.IsNullOrEmpty(x.Name) && (x.IsStyleAvailable(FontStyle.Regular) || x.IsStyleAvailable(FontStyle.Bold)))
                {
                    listBox1.Items.Add(x.Name);
                    _fontNames.Add(x.Name);
                }
            }
            labelPreview1.Text = string.Empty;
            labelPreview2.Text = string.Empty;
            labelPreview3.Text = string.Empty;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 0)
            {
                return;
            }

            buttonOK.Enabled = true;
            FontName = listBox1.Items[listBox1.SelectedIndex].ToString();
            labelPreview1.Text = FontName;
            labelPreview2.Text = FontName;
            labelPreview3.Text = FontName;
            labelPreview1.Font = new Font(new FontFamily(FontName), labelPreview1.Font.Size);
            labelPreview2.Font = new Font(new FontFamily(FontName), labelPreview2.Font.Size);
            labelPreview3.Font = new Font(new FontFamily(FontName), labelPreview3.Font.Size);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (_fontNames == null)
            {
                return;
            }

            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            foreach (var fn in _fontNames)
            {
                if (textBoxSearch.Text.Length < 2 ||
                    fn.Contains(textBoxSearch.Text, StringComparison.OrdinalIgnoreCase))
                {
                    listBox1.Items.Add(fn);
                }
            }
            listBox1.EndUpdate();
            buttonSearchClear.Enabled = textBoxSearch.Text.Length > 0;
            listView1_SelectedIndexChanged(null, null);
        }

        private void ChooseFontName_Load(object sender, EventArgs e)
        {
        }

        private void buttonSearchClear_Click(object sender, EventArgs e)
        {
            textBoxSearch.Text = string.Empty;
        }

        private void ChooseFontName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}

using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChooseEncoding : Form
    {
        private Encoding _encoding;
        private byte[] _fileBuffer;

        public ChooseEncoding()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            LanguageStructure.ChooseEncoding language = LanguageSettings.Current.ChooseEncoding;
            Text = language.Title;
            labelShortcutsSearch.Text = LanguageSettings.Current.General.Search;
            buttonSearchClear.Text = LanguageSettings.Current.DvdSubRip.Clear;
            textBoxSearch.Left = labelShortcutsSearch.Left + labelShortcutsSearch.Width + 5;
            buttonSearchClear.Left = textBoxSearch.Left + textBoxSearch.Width + 5;
            LabelPreview.Text = LanguageSettings.Current.General.Preview;
            listView1.Columns[0].Text = language.CodePage;
            listView1.Columns[1].Text = LanguageSettings.Current.General.Name;
            listView1.Columns[2].Text = language.DisplayName;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public void Initialize(string fileName)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    int readCount = (int)Math.Min(100000, fs.Length);
                    _fileBuffer = new byte[readCount];
                    fs.Read(_fileBuffer, 0, readCount);
                }
            }
            catch
            {
                _fileBuffer = new byte[0];
            }

            Encoding encoding;
            if (_fileBuffer.Length > 10 && _fileBuffer[0] == 0xef && _fileBuffer[1] == 0xbb && _fileBuffer[2] == 0xbf)
            {
                encoding = Encoding.UTF8;
            }
            else
            {
                encoding = LanguageAutoDetect.DetectAnsiEncoding(_fileBuffer);
            }

            listView1.BeginUpdate();
            foreach (var enc in Configuration.AvailableEncodings)
            {
                var item = new ListViewItem(new[] { enc.CodePage.ToString(), enc.WebName, enc.EncodingName });
                listView1.Items.Add(item);
                if (enc.CodePage == encoding.CodePage)
                {
                    item.Selected = true;
                }
            }
            listView1.ListViewItemSorter = new ListViewSorter { ColumnNumber = 0, IsNumber = true };
            listView1.EndUpdate();
        }

        private void ChooseEncoding_Load(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count >= 1)
            {
                listView1.EnsureVisible(listView1.SelectedItems[0].Index);
            }
        }

        private void FormChooseEncoding_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ChooseEncoding_ResizeEnd(object sender, EventArgs e)
        {
            listView1.AutoSizeLastColumn();
        }

        private void ChooseEncoding_Shown(object sender, EventArgs e)
        {
            ChooseEncoding_ResizeEnd(sender, e);
        }

        internal Encoding GetEncoding()
        {
            return _encoding;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(LanguageSettings.Current.ChooseEncoding.PleaseSelectAnEncoding);
            }
            else
            {
                _encoding = Encoding.GetEncoding(int.Parse(listView1.SelectedItems[0].Text));
                DialogResult = DialogResult.OK;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Encoding encoding = Encoding.GetEncoding(int.Parse(listView1.SelectedItems[0].Text));
                textBoxPreview.Text = encoding.GetString(_fileBuffer).Replace("\0", " ");
                LabelPreview.Text = LanguageSettings.Current.General.Preview + " - " + encoding.EncodingName;
            }
            else
            {
                textBoxPreview.Text = string.Empty;
                LabelPreview.Text = string.Empty;
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewSorter sorter = (ListViewSorter)listView1.ListViewItemSorter;

            if (e.Column == sorter.ColumnNumber)
            {
                sorter.Descending = !sorter.Descending; // inverse sort direction
            }
            else
            {
                sorter.ColumnNumber = e.Column;
                sorter.Descending = false;
                sorter.IsNumber = e.Column == 0; // only index 1 is numeric
            }
            listView1.Sort();
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var enc in Configuration.AvailableEncodings)
            {
                if (textBoxSearch.Text.Length < 2 ||
                    enc.CodePage.ToString().Contains(textBoxSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                    enc.WebName.Contains(textBoxSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                    enc.EncodingName.Contains(textBoxSearch.Text, StringComparison.OrdinalIgnoreCase))
                {
                    var item = new ListViewItem(new[] { enc.CodePage.ToString(), enc.WebName, enc.EncodingName });
                    listView1.Items.Add(item);
                }
            }
            listView1.EndUpdate();
            buttonSearchClear.Enabled = textBoxSearch.Text.Length > 0;
            listView1_SelectedIndexChanged(null, null);
        }

        private void buttonSearchClear_Click(object sender, EventArgs e)
        {
            textBoxSearch.Text = string.Empty;
        }
    }
}

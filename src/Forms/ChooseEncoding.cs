using Nikse.SubtitleEdit.Core;
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
            InitializeComponent();

            LanguageStructure.ChooseEncoding language = Configuration.Settings.Language.ChooseEncoding;
            Text = language.Title;
            LabelPreview.Text = Configuration.Settings.Language.General.Preview;
            listView1.Columns[0].Text = language.CodePage;
            listView1.Columns[1].Text = Configuration.Settings.Language.General.Name;
            listView1.Columns[2].Text = language.DisplayName;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public void Initialize(string fileName)
        {
            try
            {
                var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                int length = (int)file.Length;
                if (length > 100000)
                    length = 100000;

                file.Position = 0;
                _fileBuffer = new byte[length];
                file.Read(_fileBuffer, 0, length);
                file.Close();
            }
            catch
            {
                _fileBuffer = new byte[0];
            }

            var encoding = LanguageAutoDetect.DetectAnsiEncoding(_fileBuffer);
            foreach (var enc in Configuration.AvailableEncodings)
            {
                var item = new ListViewItem(new[] { enc.CodePage.ToString(), enc.WebName, enc.EncodingName });
                listView1.Items.Add(item);
                if (enc.CodePage == encoding.CodePage)
                    item.Selected = true;
            }

            listView1.ListViewItemSorter = new ListViewSorter { ColumnNumber = 0, IsNumber = true };
        }

        private void FormChooseEncoding_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        internal Encoding GetEncoding()
        {
            return _encoding;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                MessageBox.Show(Configuration.Settings.Language.ChooseEncoding.PleaseSelectAnEncoding);
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
                LabelPreview.Text = Configuration.Settings.Language.General.Preview + " - " + encoding.EncodingName;
            }
        }

        private void ChooseEncoding_Load(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count >= 1)
                listView1.EnsureVisible(listView1.SelectedItems[0].Index);
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
    }
}

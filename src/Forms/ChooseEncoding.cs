using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChooseEncoding : Form
    {
        private class ListViewSorter : System.Collections.IComparer
        {
            public int Compare(object o1, object o2)
            {
                var lvi1 = o1 as ListViewItem;
                var lvi2 = o2 as ListViewItem;
                if (lvi1 == null || lvi2 == null)
                    return 0;

                if (Descending)
                {
                    ListViewItem temp = lvi1;
                    lvi1 = lvi2;
                    lvi2 = temp;
                }

                if (IsNumber)
                {
                    int i1 = int.Parse(lvi1.SubItems[ColumnNumber].Text);
                    int i2 = int.Parse(lvi2.SubItems[ColumnNumber].Text);

                    if (i1 > i2)
                        return 1;
                    if (i1 == i2)
                        return 0;
                    return -1;
                }
                return string.Compare(lvi2.SubItems[ColumnNumber].Text, lvi1.SubItems[ColumnNumber].Text, StringComparison.Ordinal);
            }
            public int ColumnNumber { get; set; }
            public bool IsNumber { get; set; }
            public bool Descending { get; set; }
        }

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
            Utilities.FixLargeFonts(this, buttonOK);
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

                for (int i = 0; i < length; i++)
                {
                    if (_fileBuffer[i] == 0)
                        _fileBuffer[i] = 32;
                }

                file.Close();
            }
            catch
            {
                _fileBuffer = new byte[0];
            }

            Encoding encoding = Utilities.DetectAnsiEncoding(_fileBuffer);
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                var item = new ListViewItem(new[] { ei.CodePage.ToString(), ei.Name, ei.DisplayName });
                listView1.Items.Add(item);
                if (ei.CodePage == encoding.CodePage)
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
                textBoxPreview.Text = encoding.GetString(_fileBuffer);
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class JoinSubtitles : Form
    {
        List<string> _fileNamesToJoin = new List<string>();
        public Subtitle JoinedSubtitle { get; set; }

        public JoinSubtitles()
        {
            InitializeComponent();
            JoinedSubtitle = new Subtitle();
            labelTotalLines.Text = string.Empty;

            Text = Configuration.Settings.Language.JoinSubtitles.Title;
            groupBoxPreview.Text = Configuration.Settings.Language.JoinSubtitles.Information;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonSplit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void JoinSubtitles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void listViewParts_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void listViewParts_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string fileName in files)
            {
                bool alreadyInList = false;
                foreach (string existingFileName in _fileNamesToJoin)
                {
                    if (existingFileName.ToLower() == fileName.ToLower())
                        alreadyInList = true;
                }
                if (!alreadyInList)
                    _fileNamesToJoin.Add(fileName);
            }
            SortAndLoad();
        }

        private void SortAndLoad()
        {
            List<Subtitle> subtitles = new List<Subtitle>();
            for (int k = 0; k < _fileNamesToJoin.Count; k++)
            {
                string fileName = _fileNamesToJoin[k];
                try
                {
                    Subtitle sub = new Subtitle();
                    Encoding encoding;
                    var format = sub.LoadSubtitle(fileName, out encoding, null);
                    if (format == null)
                    {
                        for (int j = k; j < _fileNamesToJoin.Count; j++)
                            _fileNamesToJoin.RemoveAt(j);
                        MessageBox.Show("Unkown subtitle format: " + fileName);
                        return;
                    }
                    subtitles.Add(sub);
                }
                catch (Exception exception)
                {
                    for (int j = k; j < _fileNamesToJoin.Count; j++)
                        _fileNamesToJoin.RemoveAt(j);
                    MessageBox.Show(exception.Message);
                    return;
                }
            }

            for (int outer = 0; outer < subtitles.Count; outer++)
            {
                for (int inner = 1; inner < subtitles.Count; inner++)
                {
                    var a = subtitles[inner - 1];
                    var b = subtitles[inner];
                    if (a.Paragraphs.Count > 0 && b.Paragraphs.Count > 0 && a.Paragraphs[0].StartTime.TotalMilliseconds > b.Paragraphs[0].StartTime.TotalMilliseconds)
                    {
                        string t1 = _fileNamesToJoin[inner - 1];
                        _fileNamesToJoin[inner - 1] = _fileNamesToJoin[inner];
                        _fileNamesToJoin[inner] = t1;

                        var t2 = subtitles[inner - 1];
                        subtitles[inner - 1] = subtitles[inner];
                        subtitles[inner] = t2;
                    }
                }
            }

            listViewParts.Items.Clear();
            int i = 0;
            foreach (string fileName in _fileNamesToJoin)
            {
                Subtitle sub = subtitles[i];
                ListViewItem lvi = new ListViewItem(string.Format("{0:#,###,###}", sub.Paragraphs.Count));
                if (sub.Paragraphs.Count > 0)
                {
                    lvi.SubItems.Add(sub.Paragraphs[0].StartTime.ToString());
                    lvi.SubItems.Add(sub.Paragraphs[sub.Paragraphs.Count-1].StartTime.ToString());
                }
                else
                {
                    lvi.SubItems.Add("-");
                    lvi.SubItems.Add("-");
                }
                lvi.SubItems.Add(fileName);
                listViewParts.Items.Add(lvi);
                i++;
            }

            JoinedSubtitle = new Subtitle();
            foreach (Subtitle sub in subtitles)
            {
                foreach (Paragraph p in sub.Paragraphs)
                {
                    JoinedSubtitle.Paragraphs.Add(p);
                }
            }
            JoinedSubtitle.Renumber(1);
            labelTotalLines.Text = string.Format("Total number of lines: {0:#,###,###}", JoinedSubtitle.Paragraphs.Count);
        }

        private void JoinSubtitles_Resize(object sender, EventArgs e)
        {
            columnHeaderFileName.Width = -2;
        }

        private void buttonAddVobFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                foreach (string fileName in openFileDialog1.FileNames)
                {
                    bool alreadyInList = false;
                    foreach (string existingFileName in _fileNamesToJoin)
                    {
                        if (existingFileName.ToLower() == fileName.ToLower())
                            alreadyInList = true;
                    }
                    if (!alreadyInList)
                        _fileNamesToJoin.Add(fileName);
                }
                SortAndLoad();
            }
        }

        private void ButtonRemoveVob_Click(object sender, EventArgs e)
        {
            List<int> indices = new List<int>();
            foreach (int index in listViewParts.SelectedIndices)
                indices.Add(index);
            indices.Reverse();
            foreach (int index in indices)
                _fileNamesToJoin.RemoveAt(index);

            if (_fileNamesToJoin.Count == 0)
                buttonClear_Click(null, null);
            else
                SortAndLoad();

        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            _fileNamesToJoin.Clear();
            listViewParts.Items.Clear();
            JoinedSubtitle = new Subtitle();
        }

        private void JoinSubtitles_Shown(object sender, EventArgs e)
        {
            columnHeaderFileName.Width = -2;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial  class SyncPointsSync : Form
    {
        public class ListBoxSyncPoint
        {
            public int Index { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        string _subtitleFileName;
        string _videoFileName;
        Subtitle _subtitle;
        Subtitle _originalSubtitle;
        System.Collections.Generic.SortedDictionary<int, TimeSpan> _syncronizationPoints = new SortedDictionary<int, TimeSpan>();

        public string VideoFileName 
        {
            get { return _videoFileName;  }
        }

        public Subtitle FixedSubtitle
        {
            get { return _subtitle; }
        }

        public SyncPointsSync()
        {
            InitializeComponent();

            this.Text = Configuration.Settings.Language.PointSync.Title;
            buttonSetSyncPoint.Text = Configuration.Settings.Language.PointSync.SetSyncPoint;
            buttonRemoveSyncPoint.Text = Configuration.Settings.Language.PointSync.RemoveSyncPoint;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonApplySync.Text = Configuration.Settings.Language.General.Apply;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            labelNoOfSyncPoints.Text = string.Format(Configuration.Settings.Language.PointSync.SyncPointsX, 0);
            labelSyncHelp.Text = Configuration.Settings.Language.PointSync.Description;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            SubtitleListview1.InitializeTimeStampColumWidths(this);
            Utilities.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public void Initialize(Subtitle subtitle, string subtitleFileName, string videoFileName)
        {
            _subtitle = new Subtitle(subtitle);
            _originalSubtitle = subtitle;
            _subtitleFileName = subtitleFileName;
            _videoFileName = videoFileName;
            SubtitleListview1.Fill(subtitle);
            if (SubtitleListview1.Items.Count > 0)
                SubtitleListview1.Items[0].Selected = true;
        }

        private void RefreshSyncronizationPointsUI()
        {
            buttonApplySync.Enabled = _syncronizationPoints.Count > 1;
            labelNoOfSyncPoints.Text = string.Format(Configuration.Settings.Language.PointSync.SyncPointsX, _syncronizationPoints.Count);


            listBoxSyncPoints.Items.Clear();

            for (int i = 0; i < SubtitleListview1.Items.Count; i++)
            {
                if (_syncronizationPoints.ContainsKey(i))
                {
                    Paragraph p = new Paragraph();
                    p.StartTime.TotalMilliseconds = _syncronizationPoints[i].TotalMilliseconds;
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + _subtitle.Paragraphs[i].Duration.TotalMilliseconds;
                    SubtitleListview1.SetStartTime(i, p);

                    ListBoxSyncPoint item = new ListBoxSyncPoint() { Index = i, Text = _subtitle.Paragraphs[i].Number.ToString() + " - " + p.StartTime.ToString() };
                    listBoxSyncPoints.Items.Add(item);
                    SubtitleListview1.SetBackgroundColor(i, Color.Green);
                    SubtitleListview1.SetNumber(i, "* * * *");
                }
                else
                {
                    SubtitleListview1.SetBackgroundColor(i, SubtitleListview1.BackColor);
                    SubtitleListview1.SetNumber(i, (i + 1).ToString());
                    SubtitleListview1.SetStartTime(i, _subtitle.Paragraphs[i]);
                }
            }
        }

        private void buttonSetSyncPoint_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1 && _subtitle != null)
            {
                SetSyncPoint getTime = new SetSyncPoint();
                int index = SubtitleListview1.SelectedItems[0].Index;
                getTime.Initialize(_subtitle, _subtitleFileName, index, _videoFileName);
                if (getTime.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (_syncronizationPoints.ContainsKey(index))
                        _syncronizationPoints[index] = getTime.SyncronizationPoint;
                    else
                        _syncronizationPoints.Add(index, getTime.SyncronizationPoint);
                    RefreshSyncronizationPointsUI();
                    _videoFileName = getTime.VideoFileName;
                }
                _videoFileName = getTime.VideoFileName;
            }
        }

        private void buttonRemoveSyncPoint_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1 && _subtitle != null)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                if (_syncronizationPoints.ContainsKey(index))
                    _syncronizationPoints.Remove(index);
                RefreshSyncronizationPointsUI();
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (buttonApplySync.Enabled)
                buttonSync_Click(null, null);
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SyncPointsSync_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
            else if (e.KeyCode == Keys.F1)
            {
                Utilities.ShowHelp("#sync");
                e.SuppressKeyPress = true;
            }
        }

        private void Sync(int startIndex, int endIndex, int minIndex, int maxIndex, double startPos, double endPos)
        {
            if (endPos > startPos)
            {
                double subStart = _originalSubtitle.Paragraphs[startIndex].StartTime.TotalMilliseconds / 1000.0;
                double subEnd = _originalSubtitle.Paragraphs[endIndex].StartTime.TotalMilliseconds / 1000.0;

                double subDiff = subEnd - subStart;
                double realDiff = endPos - startPos;

                // speed factor
                double factor = realDiff / subDiff;

                // adjust to starting position
                double adjust = startPos - subStart * factor;

                for (int i=minIndex; i<_subtitle.Paragraphs.Count; i++)
                {
                    if (i <= maxIndex)
                    {
                        Paragraph p = _subtitle.Paragraphs[i];
                        p.StartTime.TotalMilliseconds = _originalSubtitle.Paragraphs[i].StartTime.TotalMilliseconds;
                        p.EndTime.TotalMilliseconds = _originalSubtitle.Paragraphs[i].EndTime.TotalMilliseconds;
                        p.Adjust(factor, adjust);
                    }
                }
            }
        }

        private void buttonSync_Click(object sender, EventArgs e)
        {
            int startIndex = -1;
            int endIndex = -1;
            int minIndex = 0;
            int maxIndex;

            List<int> syncIndices = new List<int>();         
            foreach (KeyValuePair<int, TimeSpan> kvp in _syncronizationPoints)
                syncIndices.Add(kvp.Key);

            for (int i = 0; i < syncIndices.Count; i++)
            { 
                if (i == 0)
                {
                    endIndex = syncIndices[i];
                }
                else
                {
                    startIndex = endIndex;
                    endIndex = syncIndices[i];

                    if (i == syncIndices.Count -1)
                        maxIndex = _subtitle.Paragraphs.Count;
                    else
                        maxIndex = syncIndices[i]; // maxIndex = syncIndices[i + 1];

                    Sync(startIndex, endIndex, minIndex, maxIndex, _syncronizationPoints[startIndex].TotalMilliseconds / 1000.0, _syncronizationPoints[endIndex].TotalMilliseconds / 1000.0);

                    minIndex = endIndex;
                }
            }
            _syncronizationPoints = new SortedDictionary<int, TimeSpan>();
            SubtitleListview1.Fill(_subtitle);
            RefreshSyncronizationPointsUI();
        }

        private void listBoxSyncPoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxSyncPoints.SelectedIndex >= 0)
            { 
                ListBoxSyncPoint item = (ListBoxSyncPoint) listBoxSyncPoints.Items[listBoxSyncPoints.SelectedIndex];
                SubtitleListview1.SelectIndexAndEnsureVisible(item.Index);
            }
        }

        private void SubtitleListview1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            { 
                int index = SubtitleListview1.SelectedItems[0].Index;
                if (_syncronizationPoints.ContainsKey(index))
                    buttonRemoveSyncPoint_Click(null, null);
                else
                    buttonSetSyncPoint_Click(null, null);
            }
        }

    }
}

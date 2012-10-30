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
        int _audioTrackNumber;
        Subtitle _subtitle;
        Subtitle _originalSubtitle;
        Subtitle _otherSubtitle;
        SortedDictionary<int, TimeSpan> _syncronizationPoints = new SortedDictionary<int, TimeSpan>();
        Keys _mainGeneralGoToNextSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
        Keys _mainGeneralGoToPrevSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);

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

            buttonSetSyncPoint.Text = Configuration.Settings.Language.PointSync.SetSyncPoint;
            buttonRemoveSyncPoint.Text = Configuration.Settings.Language.PointSync.RemoveSyncPoint;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonApplySync.Text = Configuration.Settings.Language.PointSync.ApplySync;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            labelNoOfSyncPoints.Text = string.Format(Configuration.Settings.Language.PointSync.SyncPointsX, 0);
            labelSyncInfo.Text = Configuration.Settings.Language.PointSync.Info;
            buttonFindText.Text = Configuration.Settings.Language.VisualSync.FindText;
            buttonFindTextOther.Text = Configuration.Settings.Language.VisualSync.FindText;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            subtitleListView2.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            SubtitleListview1.InitializeTimeStampColumWidths(this);
            subtitleListView2.InitializeTimeStampColumWidths(this);
            Utilities.InitializeSubtitleFont(SubtitleListview1);
            Utilities.InitializeSubtitleFont(subtitleListView2);
            SubtitleListview1.AutoSizeAllColumns(this);
            subtitleListView2.AutoSizeAllColumns(this);
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public void Initialize(Subtitle subtitle, string subtitleFileName, string videoFileName, int audioTrackNumber)
        {
            Text = Configuration.Settings.Language.PointSync.Title;
            labelSubtitleFileName.Text = subtitleFileName;
            _subtitle = new Subtitle(subtitle);
            _originalSubtitle = subtitle;
            _subtitleFileName = subtitleFileName;
            _videoFileName = videoFileName;
            _audioTrackNumber = audioTrackNumber;
            SubtitleListview1.Fill(subtitle);
            if (SubtitleListview1.Items.Count > 0)
                SubtitleListview1.Items[0].Selected = true;

            SubtitleListview1.Anchor = AnchorStyles.Left;
            buttonSetSyncPoint.Anchor = AnchorStyles.Left;
            buttonRemoveSyncPoint.Anchor = AnchorStyles.Left;
            labelNoOfSyncPoints.Anchor = AnchorStyles.Left;
            listBoxSyncPoints.Anchor = AnchorStyles.Left;
            groupBoxImportResult.Anchor = AnchorStyles.Left;
            labelOtherSubtitleFileName.Visible = false;
            subtitleListView2.Visible = false;
            buttonFindTextOther.Visible = false;
            groupBoxImportResult.Width = listBoxSyncPoints.Left + listBoxSyncPoints.Width + 20;
            Width = groupBoxImportResult.Left + groupBoxImportResult.Width + 15;
            SubtitleListview1.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right;
            buttonSetSyncPoint.Anchor = AnchorStyles.Right;
            buttonRemoveSyncPoint.Anchor = AnchorStyles.Right;
            labelNoOfSyncPoints.Anchor =  AnchorStyles.Right;
            listBoxSyncPoints.Anchor = AnchorStyles.Right;
            groupBoxImportResult.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right;
            buttonFindText.Left = SubtitleListview1.Left + SubtitleListview1.Width - buttonFindText.Width;
            Width = 800;
            groupBoxImportResult.Width = Width - groupBoxImportResult.Left * 3;
            MinimumSize = new Size(Width - 50, MinimumSize.Height);
        }

        public void Initialize(Subtitle subtitle, string subtitleFileName, string videoFileName, int audioTrackNumber, string otherSubtitleFileName, Subtitle otherSubtitle)
        {
            Text = Configuration.Settings.Language.PointSync.TitleViaOtherSubtitle;
            labelSubtitleFileName.Text = subtitleFileName;
            _subtitle = new Subtitle(subtitle);
            _otherSubtitle = otherSubtitle;
            _originalSubtitle = subtitle;
            _subtitleFileName = subtitleFileName;
            _videoFileName = videoFileName;
            _audioTrackNumber = audioTrackNumber;
            SubtitleListview1.Fill(subtitle);
            if (SubtitleListview1.Items.Count > 0)
                SubtitleListview1.Items[0].Selected = true;
            labelOtherSubtitleFileName.Text = otherSubtitleFileName;
            subtitleListView2.Fill(otherSubtitle);

            SubtitleListview1.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            subtitleListView2.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            buttonSetSyncPoint.Anchor = AnchorStyles.Left;
            buttonRemoveSyncPoint.Anchor = AnchorStyles.Left;
            labelNoOfSyncPoints.Anchor = AnchorStyles.Left;
            listBoxSyncPoints.Anchor = AnchorStyles.Left;
            labelOtherSubtitleFileName.Visible = true;
            subtitleListView2.Visible = true;
            buttonFindTextOther.Visible = true;
            Width = subtitleListView2.Width * 2 + 250;
            MinimumSize = new Size(Width-50, MinimumSize.Height);
        }

        private void RefreshSyncronizationPointsUI()
        {
            buttonApplySync.Enabled = _syncronizationPoints.Count > 0;
            labelNoOfSyncPoints.Text = string.Format(Configuration.Settings.Language.PointSync.SyncPointsX, _syncronizationPoints.Count);

            listBoxSyncPoints.Items.Clear();

            for (int i = 0; i < SubtitleListview1.Items.Count; i++)
            {
                if (_syncronizationPoints.ContainsKey(i))
                {
                    var p = new Paragraph();
                    p.StartTime.TotalMilliseconds = _syncronizationPoints[i].TotalMilliseconds;
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + _subtitle.Paragraphs[i].Duration.TotalMilliseconds;
                    SubtitleListview1.SetStartTime(i, p);

                    var item = new ListBoxSyncPoint() { Index = i, Text = _subtitle.Paragraphs[i].Number.ToString() + " - " + p.StartTime };
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
            if (subtitleListView2.Visible)
            {
                SetSyncPointViaOthersubtitle();
            }
            else
            {
                if (SubtitleListview1.SelectedItems.Count == 1 && _subtitle != null)
                {
                    var getTime = new SetSyncPoint();
                    int index = SubtitleListview1.SelectedItems[0].Index;
                    getTime.Initialize(_subtitle, _subtitleFileName, index, _videoFileName, _audioTrackNumber);
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
        }

        private void SetSyncPointViaOthersubtitle()
        {
            if (_otherSubtitle != null && subtitleListView2.SelectedItems.Count == 1)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                int indexOther = subtitleListView2.SelectedItems[0].Index;

                if (_syncronizationPoints.ContainsKey(index))
                    _syncronizationPoints[index] = TimeSpan.FromMilliseconds(_otherSubtitle.Paragraphs[indexOther].StartTime.TotalMilliseconds);
                else
                    _syncronizationPoints.Add(index, TimeSpan.FromMilliseconds(_otherSubtitle.Paragraphs[indexOther].StartTime.TotalMilliseconds));
                RefreshSyncronizationPointsUI();
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
            else if (_mainGeneralGoToNextSubtitle == e.KeyData || (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt))
            {
                int selectedIndex = 0;
                if (SubtitleListview1.SelectedItems.Count > 0)
                {
                    selectedIndex = SubtitleListview1.SelectedItems[0].Index;
                    selectedIndex++;
                }
                SubtitleListview1.SelectIndexAndEnsureVisible(selectedIndex);
                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralGoToPrevSubtitle == e.KeyData || (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt))
            {
                int selectedIndex = 0;
                if (SubtitleListview1.SelectedItems.Count > 0)
                {
                    selectedIndex = SubtitleListview1.SelectedItems[0].Index;
                    selectedIndex--;
                }
                SubtitleListview1.SelectIndexAndEnsureVisible(selectedIndex);
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
            if (_syncronizationPoints.Count == 1)
            {
                foreach (KeyValuePair<int, TimeSpan> kvp in _syncronizationPoints)
                    AdjustViaShowEarlierLater(kvp.Key, kvp.Value.TotalMilliseconds);
                _syncronizationPoints = new SortedDictionary<int, TimeSpan>();
                SubtitleListview1.Fill(_subtitle);
                RefreshSyncronizationPointsUI();
                return;
            }

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

        private void AdjustViaShowEarlierLater(int index, double newTotalMilliseconds)
        {
            var oldTotalMilliseconds = _subtitle.Paragraphs[index].StartTime.TotalMilliseconds;
            var diff = newTotalMilliseconds - oldTotalMilliseconds;
            _subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(diff));
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

        private void SyncPointsSyncResize(object sender, EventArgs e)
        {
            if (subtitleListView2.Visible)
            {
                int widthInMiddle = listBoxSyncPoints.Width;
                SubtitleListview1.Width = (groupBoxImportResult.Width - widthInMiddle) / 2 - 12;
                subtitleListView2.Width = SubtitleListview1.Width;
                subtitleListView2.Left = SubtitleListview1.Left + SubtitleListview1.Width + widthInMiddle + 10;
                listBoxSyncPoints.Left = SubtitleListview1.Left + SubtitleListview1.Width + 5;
                buttonSetSyncPoint.Left = listBoxSyncPoints.Left;
                buttonRemoveSyncPoint.Left = listBoxSyncPoints.Left;
                labelNoOfSyncPoints.Left = listBoxSyncPoints.Left;
                labelOtherSubtitleFileName.Left = subtitleListView2.Left;
                buttonFindText.Left = SubtitleListview1.Left + SubtitleListview1.Width - buttonFindText.Width;
            }
        }

        private void SyncPointsSyncShown(object sender, EventArgs e)
        {
            SyncPointsSyncResize(null, null);
        }

        private void ButtonFindTextClick(object sender, EventArgs e)
        {
            var findSubtitle = new FindSubtitleLine();
            findSubtitle.Initialize(_subtitle.Paragraphs, string.Empty);
            findSubtitle.ShowDialog();
            if (findSubtitle.SelectedIndex >= 0)
                SubtitleListview1.SelectIndexAndEnsureVisible(findSubtitle.SelectedIndex);
        }

        private void ButtonFindTextOtherClick(object sender, EventArgs e)
        {
            var findSubtitle = new FindSubtitleLine();
            findSubtitle.Initialize(_otherSubtitle.Paragraphs, string.Empty);
            findSubtitle.ShowDialog();
            if (findSubtitle.SelectedIndex >= 0)
                subtitleListView2.SelectIndexAndEnsureVisible(findSubtitle.SelectedIndex);
        }

    }
}

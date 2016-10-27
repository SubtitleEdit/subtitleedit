﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SyncPointsSync : PositionAndSizeForm
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

        private string _subtitleFileName;
        private string _videoFileName;
        private int _audioTrackNumber;
        private Subtitle _subtitle;
        private Subtitle _originalSubtitle;
        private Subtitle _otherSubtitle;
        private SortedDictionary<int, TimeSpan> _synchronizationPoints = new SortedDictionary<int, TimeSpan>();
        private readonly Keys _mainGeneralGoToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
        private readonly Keys _mainGeneralGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);

        public string VideoFileName
        {
            get { return _videoFileName; }
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
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonApplySync.Text = Configuration.Settings.Language.PointSync.ApplySync;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            labelNoOfSyncPoints.Text = string.Format(Configuration.Settings.Language.PointSync.SyncPointsX, 0);
            labelSyncInfo.Text = Configuration.Settings.Language.PointSync.Info;
            buttonFindText.Text = Configuration.Settings.Language.VisualSync.FindText;
            buttonFindTextOther.Text = Configuration.Settings.Language.VisualSync.FindText;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            subtitleListView2.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            SubtitleListview1.InitializeTimestampColumnWidths(this);
            subtitleListView2.InitializeTimestampColumnWidths(this);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            UiUtil.InitializeSubtitleFont(subtitleListView2);
            SubtitleListview1.AutoSizeAllColumns(this);
            subtitleListView2.AutoSizeAllColumns(this);
            UiUtil.FixLargeFonts(this, buttonOK);
            labelAdjustFactor.Text = string.Empty;
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
            labelNoOfSyncPoints.Anchor = AnchorStyles.Right;
            listBoxSyncPoints.Anchor = AnchorStyles.Right;
            groupBoxImportResult.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right;
            buttonFindText.Left = SubtitleListview1.Left + SubtitleListview1.Width - buttonFindText.Width;
            Width = 800;
            groupBoxImportResult.Width = Width - groupBoxImportResult.Left * 3;
            labelAdjustFactor.Left = listBoxSyncPoints.Left;
            labelAdjustFactor.Anchor = listBoxSyncPoints.Anchor;
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
            labelAdjustFactor.Anchor = listBoxSyncPoints.Anchor;
            labelOtherSubtitleFileName.Visible = true;
            subtitleListView2.Visible = true;
            buttonFindTextOther.Visible = true;
            Width = subtitleListView2.Width * 2 + 250;
            MinimumSize = new Size(Width - 50, MinimumSize.Height);
        }

        private void RefreshSynchronizationPointsUI()
        {
            buttonApplySync.Enabled = _synchronizationPoints.Count > 0;
            labelNoOfSyncPoints.Text = string.Format(Configuration.Settings.Language.PointSync.SyncPointsX, _synchronizationPoints.Count);

            listBoxSyncPoints.Items.Clear();

            for (int i = 0; i < SubtitleListview1.Items.Count; i++)
            {
                if (_synchronizationPoints.ContainsKey(i))
                {
                    var p = new Paragraph();
                    p.StartTime.TotalMilliseconds = _synchronizationPoints[i].TotalMilliseconds;
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + _subtitle.Paragraphs[i].Duration.TotalMilliseconds;
                    SubtitleListview1.SetStartTimeAndDuration(i, p);

                    var item = new ListBoxSyncPoint { Index = i, Text = _subtitle.Paragraphs[i].Number + " - " + p.StartTime };
                    listBoxSyncPoints.Items.Add(item);
                    SubtitleListview1.SetBackgroundColor(i, Color.Green);
                    SubtitleListview1.SetNumber(i, "* * * *");
                }
                else
                {
                    SubtitleListview1.SetBackgroundColor(i, SubtitleListview1.BackColor);
                    SubtitleListview1.SetNumber(i, (i + 1).ToString(CultureInfo.InvariantCulture));
                    SubtitleListview1.SetStartTimeAndDuration(i, _subtitle.Paragraphs[i]);
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
                    using (var getTime = new SetSyncPoint())
                    {
                        int index = SubtitleListview1.SelectedItems[0].Index;
                        getTime.Initialize(_subtitle, _subtitleFileName, index, _videoFileName, _audioTrackNumber);
                        if (getTime.ShowDialog(this) == DialogResult.OK)
                        {
                            if (_synchronizationPoints.ContainsKey(index))
                                _synchronizationPoints[index] = getTime.SynchronizationPoint;
                            else
                                _synchronizationPoints.Add(index, getTime.SynchronizationPoint);
                            RefreshSynchronizationPointsUI();
                            _videoFileName = getTime.VideoFileName;
                        }
                        Activate();
                        _videoFileName = getTime.VideoFileName;
                    }
                }
            }
            SetSyncFactorLabel();
        }

        private void SetSyncPointViaOthersubtitle()
        {
            if (_otherSubtitle != null && subtitleListView2.SelectedItems.Count == 1)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                int indexOther = subtitleListView2.SelectedItems[0].Index;

                if (_synchronizationPoints.ContainsKey(index))
                    _synchronizationPoints[index] = TimeSpan.FromMilliseconds(_otherSubtitle.Paragraphs[indexOther].StartTime.TotalMilliseconds);
                else
                    _synchronizationPoints.Add(index, TimeSpan.FromMilliseconds(_otherSubtitle.Paragraphs[indexOther].StartTime.TotalMilliseconds));
                RefreshSynchronizationPointsUI();
            }
            SetSyncFactorLabel();
        }

        private void buttonRemoveSyncPoint_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1 && _subtitle != null)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                if (_synchronizationPoints.ContainsKey(index))
                    _synchronizationPoints.Remove(index);
                RefreshSynchronizationPointsUI();
            }
            SetSyncFactorLabel();
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
            else if (e.KeyCode == UiUtil.HelpKeys)
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
            else if (e.KeyData == (Keys.Control | Keys.G))
            {
                var subView = SubtitleListview1;
                if (subtitleListView2 != null && subtitleListView2.Visible && !SubtitleListview1.Focused)
                {
                    var x = PointToClient(MousePosition).X;
                    if (x >= subtitleListView2.Left && x <= subtitleListView2.Left + subtitleListView2.Width)
                    {
                        subView = subtitleListView2;
                    }
                }
                using (var gotoForm = new GoToLine())
                {
                    gotoForm.Initialize(1, subView.Items.Count);
                    if (gotoForm.ShowDialog() == DialogResult.OK)
                    {
                        subView.SelectIndexAndEnsureVisible(gotoForm.LineNumber - 1, true);
                    }
                }
            }
        }

        private void SetSyncFactorLabel()
        {
            labelAdjustFactor.Text = string.Empty;
            if (_synchronizationPoints.Count == 1)
            {
                double startPos = _synchronizationPoints.First().Value.TotalMilliseconds / TimeCode.BaseUnit;
                double subStart = _originalSubtitle.Paragraphs[_synchronizationPoints.First().Key].StartTime.TotalMilliseconds / TimeCode.BaseUnit;

                var adjustment = startPos - subStart;
                labelAdjustFactor.Text = string.Format("{0:+0.000;-0.000}", adjustment);
            }
            else if (_synchronizationPoints.Count == 2)
            {
                double startPos = _synchronizationPoints.First().Value.TotalMilliseconds / TimeCode.BaseUnit;
                double endPos = _synchronizationPoints.Last().Value.TotalMilliseconds / TimeCode.BaseUnit;

                double subStart = _originalSubtitle.Paragraphs[_synchronizationPoints.First().Key].StartTime.TotalMilliseconds / TimeCode.BaseUnit;
                double subEnd = _originalSubtitle.Paragraphs[_synchronizationPoints.Last().Key].StartTime.TotalMilliseconds / TimeCode.BaseUnit;

                double subDiff = subEnd - subStart;
                double realDiff = endPos - startPos;

                // speed factor
                double factor = realDiff / subDiff;

                // adjust to starting position
                double adjust = startPos - subStart * factor;

                labelAdjustFactor.Text = string.Format("*{0:0.000}, {1:+0.000;-0.000}", factor, adjust);
            }
        }

        private void Sync(int startIndex, int endIndex, int minIndex, int maxIndex, double startPos, double endPos)
        {
            if (endPos > startPos)
            {
                double subStart = _originalSubtitle.Paragraphs[startIndex].StartTime.TotalMilliseconds / TimeCode.BaseUnit;
                double subEnd = _originalSubtitle.Paragraphs[endIndex].StartTime.TotalMilliseconds / TimeCode.BaseUnit;

                double subDiff = subEnd - subStart;
                double realDiff = endPos - startPos;

                // speed factor
                double factor = realDiff / subDiff;

                // adjust to starting position
                double adjust = startPos - subStart * factor;

                for (int i = minIndex; i < _subtitle.Paragraphs.Count; i++)
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
            if (_synchronizationPoints.Count == 1)
            {
                foreach (KeyValuePair<int, TimeSpan> kvp in _synchronizationPoints)
                    AdjustViaShowEarlierLater(kvp.Key, kvp.Value.TotalMilliseconds);
                _synchronizationPoints = new SortedDictionary<int, TimeSpan>();
                SubtitleListview1.Fill(_subtitle);
                RefreshSynchronizationPointsUI();
                return;
            }

            int endIndex = -1;
            int minIndex = 0;
            var syncIndices = new List<int>();
            foreach (var kvp in _synchronizationPoints)
                syncIndices.Add(kvp.Key);
            for (int i = 0; i < syncIndices.Count; i++)
            {
                if (i == 0)
                {
                    endIndex = syncIndices[i];
                }
                else
                {
                    var startIndex = endIndex;
                    endIndex = syncIndices[i];

                    int maxIndex;
                    if (i == syncIndices.Count - 1)
                        maxIndex = _subtitle.Paragraphs.Count;
                    else
                        maxIndex = syncIndices[i]; // maxIndex = syncIndices[i + 1];

                    Sync(startIndex, endIndex, minIndex, maxIndex, _synchronizationPoints[startIndex].TotalMilliseconds / TimeCode.BaseUnit, _synchronizationPoints[endIndex].TotalMilliseconds / TimeCode.BaseUnit);

                    minIndex = endIndex;
                }
            }
            SubtitleListview1.Fill(_subtitle);
            RefreshSynchronizationPointsUI();
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
                var item = (ListBoxSyncPoint)listBoxSyncPoints.Items[listBoxSyncPoints.SelectedIndex];
                SubtitleListview1.SelectIndexAndEnsureVisible(item.Index);
            }
        }

        private void SubtitleListview1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                if (_synchronizationPoints.ContainsKey(index))
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
                labelAdjustFactor.Left = listBoxSyncPoints.Left;
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
            using (var findSubtitle = new FindSubtitleLine())
            {
                findSubtitle.Initialize(_subtitle.Paragraphs, string.Empty);
                findSubtitle.ShowDialog();
                if (findSubtitle.SelectedIndex >= 0)
                    SubtitleListview1.SelectIndexAndEnsureVisible(findSubtitle.SelectedIndex);
            }
        }

        private void ButtonFindTextOtherClick(object sender, EventArgs e)
        {
            using (var findSubtitle = new FindSubtitleLine())
            {
                findSubtitle.Initialize(_otherSubtitle.Paragraphs, string.Empty);
                findSubtitle.ShowDialog();
                if (findSubtitle.SelectedIndex >= 0)
                    subtitleListView2.SelectIndexAndEnsureVisible(findSubtitle.SelectedIndex);
            }
        }

    }
}

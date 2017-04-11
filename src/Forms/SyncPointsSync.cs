using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Nikse.SubtitleEdit.Forms.GoogleTranslate;

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

            GoogleTranslate gt = new GoogleTranslate();
            gt.FillComboWithGoogleLanguages(comboBoxOtherSubLanguage);
            gt.FillComboWithGoogleLanguages(comboBoxSubToSyncLanguage);
            comboBoxOtherSubLanguage.SelectedItem = comboBoxOtherSubLanguage.Items[19];
            comboBoxSubToSyncLanguage.SelectedItem = comboBoxSubToSyncLanguage.Items[18];
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

        private void buttonAutoSync_Click(object sender, EventArgs e)
        {
            /*It should be possible to change these variables in a autosync settings menu*/
            int TIME_MARGIN = 5;
            int MINIMUM_WORDS_IN_PARAGRAPH = 1;
            int TARGET_PERCENTAGE = 60;

            labelAutoSyncing.Visible = true;
            progressAutoSync.Visible = true;
            AutoSync(_subtitle, _otherSubtitle, TIME_MARGIN, MINIMUM_WORDS_IN_PARAGRAPH, TARGET_PERCENTAGE, (comboBoxSubToSyncLanguage.SelectedItem as ComboBoxItem).Value, (comboBoxOtherSubLanguage.SelectedItem as ComboBoxItem).Value);
            labelAutoSyncing.Visible = false;
            progressAutoSync.Visible = false;
        }

        /*
            This function will try to sync subtitles automatically by comparing the one subtitle with another google translated one.
        */
        private void AutoSync(Subtitle subtitleToSync, Subtitle refrence, int timeMargin, int minimumWordsInParagraph, int targetPercentage, string languageSubtileToSync, string languageRefrence)
        {
            //Tasks are used beceause translation process takes a long time
            Task<Subtitle> taskTranslateOther = Task<Subtitle>.Factory.StartNew(() => TranslateSub(refrence, languageRefrence + "|" + languageSubtileToSync));
            Task<Subtitle> taskTranslateSubtileToSync = Task<Subtitle>.Factory.StartNew(() => TranslateSub(subtitleToSync, languageSubtileToSync + "|" + languageRefrence));

            int numberOfEqualWords;
            int numberOfWrongWords;
            int nextSyncPoint = 0;
            string[] subtitleWords;
            string[] translatedSubs;

            SortedList<int, int> syncPoints = new SortedList<int, int>();

            Subtitle subtitleWrong = new Subtitle(subtitleToSync, true);

            var otherTranslated = taskTranslateOther.Result;


            for (int z = 0; z < 2; z++)
            {
                //for each paragraph subtitle to sync
                for (int i = 0; i < subtitleWrong.Paragraphs.Count; i++)
                {
                    subtitleWords = Regex.Matches(subtitleWrong.Paragraphs[i].Text.ToLower(), @"\w+").Cast<Match>().Select(x => x.Value).ToArray(); //_subtitle.Paragraphs[i].Text.Split(' ');
                    //for each translated paragraph in range
                    for (int j = nextSyncPoint; j < otherTranslated.Paragraphs.Count && (subtitleWrong.Paragraphs[i].StartTime.Minutes < otherTranslated.Paragraphs[j].StartTime.Minutes + timeMargin && subtitleWrong.Paragraphs[i].StartTime.Minutes > otherTranslated.Paragraphs[j].StartTime.Minutes - timeMargin); j++)
                    {
                        numberOfEqualWords = 0;
                        numberOfWrongWords = 0;
                        translatedSubs = Regex.Matches(otherTranslated.Paragraphs[j].Text.ToLower(), @"\w+").Cast<Match>().Select(x => x.Value).ToArray(); //otherTranslated.Paragraphs[j].Text.Split(' ');
                        //for each word in paragrah
                        foreach (var word in subtitleWords)
                        {
                            //for each word in translated paragraph
                            foreach (var translatedWord in translatedSubs)
                            {
                                if (word.Equals(translatedWord))
                                {
                                    numberOfEqualWords++;
                                }
                                else
                                {
                                    numberOfWrongWords++;
                                }
                            }
                        }
                        int averageRightOriginal = (int)((numberOfEqualWords / (float)subtitleWords.Length) * 100);
                        int averageRightTranslated = (int)((numberOfEqualWords / (float)translatedSubs.Length) * 100);
                        //if targetpercentage right is met based on averageOriginal & translated. Also if the words in a paragraph are less than 4 the average should be 90%
                        if ((((averageRightTranslated >= targetPercentage) && translatedSubs.Length > 4) || averageRightTranslated >= 90)
                        && (((averageRightOriginal >= targetPercentage) && translatedSubs.Length > 4) || averageRightOriginal >= 90)
                        && subtitleWords.Length >= minimumWordsInParagraph && translatedSubs.Length >= minimumWordsInParagraph && !syncPoints.ContainsKey(i))
                        {
                            syncPoints.Add(i, j);
                            nextSyncPoint = j + 1;
                            j = otherTranslated.Paragraphs.Count;
                        }
                    }
                }
                //if true the subtiles will be compared again but translated the other way around
                if (z == 0)
                {
                    otherTranslated = refrence;
                    subtitleWrong = taskTranslateSubtileToSync.Result;
                    nextSyncPoint = 0;
                }
            }
            //This should be made multithreaded
            foreach (var point in syncPoints)
            {
                //This removes syncpoints with starttime bigger than the next syncpoint
                if(syncPoints.IndexOfKey(point.Key) == syncPoints.Count - 1 || (syncPoints.IndexOfKey(point.Key) < syncPoints.Count && otherTranslated.Paragraphs[syncPoints.Values[syncPoints.IndexOfKey(point.Key) + 1]].StartTime.TotalMilliseconds > otherTranslated.Paragraphs[syncPoints.Values[syncPoints.IndexOfKey(point.Key)]].StartTime.TotalMilliseconds))
                {
                    SetSyncPointAuto(point.Key, point.Value);
                }
            }
        }
        private void SetSyncPointAuto(int listVieuw1Index, int listVieuw2Index)
        {
            int index = listVieuw1Index;
            int indexOther = listVieuw2Index;

            if (_synchronizationPoints.ContainsKey(index))
                _synchronizationPoints[index] = TimeSpan.FromMilliseconds(_otherSubtitle.Paragraphs[indexOther].StartTime.TotalMilliseconds);
            else
                _synchronizationPoints.Add(index, TimeSpan.FromMilliseconds(_otherSubtitle.Paragraphs[indexOther].StartTime.TotalMilliseconds));
            RefreshSynchronizationPointsUI();
            SetSyncFactorLabel();
        }

        private Subtitle TranslateSub(Subtitle subtitleToTranslate, string languagePair)
        {
            Subtitle otherTranslated = new Subtitle(subtitleToTranslate, true);
            Parallel.For(0, otherTranslated.Paragraphs.Count, i =>
            {
                otherTranslated.Paragraphs[i].Text = GoogleTranslate.TranslateTextViaScreenScraping(subtitleToTranslate.Paragraphs[i].Text, languagePair, Encoding.Default, false);
            });
            return otherTranslated;
        }
    }
}

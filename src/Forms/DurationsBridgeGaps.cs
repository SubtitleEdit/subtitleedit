using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class DurationsBridgeGaps : PositionAndSizeForm
    {
        private readonly bool _shouldClearExtraColumn;
        private readonly Subtitle _subtitle;
        private Subtitle _fixedSubtitle;
        private Dictionary<string, string> _dic;
        private readonly Timer _refreshTimer = new Timer();
        public Subtitle FixedSubtitle => _fixedSubtitle;
        public int FixedCount { get; private set; }
        public int MinMsBetweenLines
        {
            get => (int)numericUpDownMinMsBetweenLines.Value;
            set => numericUpDownMinMsBetweenLines.Value = value;
        }

        public bool PreviousSubtitleTakesAllTime => radioButtonProlongEndTime.Checked;

        public DurationsBridgeGaps(Subtitle subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonOK);

            SubtitleListview1.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            SubtitleListview1.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            SubtitleListview1.SetCustomResize(SubtitleListView1_Resize);

            Text = Configuration.Settings.Language.DurationsBridgeGaps.Title;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.ShowExtraColumn(Configuration.Settings.Language.DurationsBridgeGaps.GapToNext);
            SubtitleListview1.AutoSizeAllColumns(this);

            labelBridgePart1.Text = Configuration.Settings.Language.DurationsBridgeGaps.BridgeGapsSmallerThanXPart1;
            numericUpDownMaxMs.Left = labelBridgePart1.Left + labelBridgePart1.Width + 4;
            labelMilliseconds.Text = Configuration.Settings.Language.DurationsBridgeGaps.BridgeGapsSmallerThanXPart2;
            labelMilliseconds.Left = numericUpDownMaxMs.Left + numericUpDownMaxMs.Width + 4;
            labelMinMsBetweenLines.Text = Configuration.Settings.Language.DurationsBridgeGaps.MinMillisecondsBetweenLines;
            numericUpDownMinMsBetweenLines.Left = labelMinMsBetweenLines.Left + labelMinMsBetweenLines.Width + 4;
            radioButtonProlongEndTime.Text = Configuration.Settings.Language.DurationsBridgeGaps.ProlongEndTime;
            radioButtonDivideEven.Text = Configuration.Settings.Language.DurationsBridgeGaps.DivideEven;
            groupBoxLinesFound.Text = string.Empty;

            _subtitle = subtitle;
            try
            {
                numericUpDownMaxMs.Value = Configuration.Settings.Tools.BridgeGapMilliseconds;
            }
            catch
            {
                numericUpDownMaxMs.Value = 100;
            }
            if (Configuration.Settings.General.MinimumMillisecondsBetweenLines >= 1 && Configuration.Settings.General.MinimumMillisecondsBetweenLines <= numericUpDownMinMsBetweenLines.Maximum)
            {
                numericUpDownMinMsBetweenLines.Value = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            }

            if (subtitle != null)
            {
                _refreshTimer.Interval = 400;
                _refreshTimer.Tick += RefreshTimerTick;
                GeneratePreview();
            }

            // if subtitle format stores information in Paragraph.Extra, the extra column
            // will contains Paragraph.Extra content in ExtraColumn
            _shouldClearExtraColumn = ShouldClearExtra();
        }

        private void RefreshTimerTick(object sender, EventArgs e)
        {
            _refreshTimer.Stop();
            GeneratePreviewReal();
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.BridgeGapMilliseconds = (int)numericUpDownMaxMs.Value;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void GeneratePreview()
        {
            if (_refreshTimer.Enabled)
            {
                _refreshTimer.Stop();
            }
            _refreshTimer.Start();
        }

        private bool ShouldClearExtra()
        {
            var originalType = _subtitle.OriginalFormat.GetType();

            // these are the formats that uses Paragraph.Extra property to store information
            return originalType == typeof(TimedText) ||
            originalType == typeof(AdvancedSubStationAlpha) ||
            originalType == typeof(TimedText10) ||
            originalType == typeof(WebVTT) ||
            originalType == typeof(WebVTTFileWithLineNumber) ||
            originalType == typeof(SubStationAlpha) ||
            originalType == typeof(StructuredTitles) ||
            originalType == typeof(SmpteTt2052) ||
            originalType == typeof(ItunesTimedText) ||
            originalType == typeof(NetflixTimedText) ||
            originalType == typeof(VocapiaSplit) ||
            originalType == typeof(SubUrbia) ||
            originalType == typeof(FinalCutProImage) ||
            originalType == typeof(Sami);
        }

        private void GeneratePreviewReal()
        {
            groupBoxLinesFound.Text = string.Empty;
            if (_subtitle == null)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            SubtitleListview1.BeginUpdate();
            SubtitleListview1.Items.Clear();
            _fixedSubtitle = new Subtitle(_subtitle);
            _dic = new Dictionary<string, string>();
            var fixedIndexes = new List<int>(_fixedSubtitle.Paragraphs.Count);

            var minMsBetweenLines = (int)numericUpDownMinMsBetweenLines.Value;
            FixedCount = Core.Forms.DurationsBridgeGaps.BridgeGaps(_fixedSubtitle, minMsBetweenLines, radioButtonDivideEven.Checked, (double)numericUpDownMaxMs.Value, fixedIndexes, _dic);

            // Note: By default SubtitleListView will update the extra-column if current subtitle format uses Paragraph.Extra
            // property e.g: NetflixTimedText
            SubtitleListview1.Fill(_fixedSubtitle);

            for (int i = 0; i < _fixedSubtitle.Paragraphs.Count - 1; i++)
            {
                Paragraph cur = _fixedSubtitle.Paragraphs[i];
                if (_dic.ContainsKey(cur.ID))
                {
                    SubtitleListview1.SetExtraText(i, _dic[cur.ID], SubtitleListview1.ForeColor);
                }
                else
                {
                    if (_shouldClearExtraColumn)
                    {
                        // don't display paragraph.text information
                        SubtitleListview1.SetExtraText(i, string.Empty, SubtitleListview1.BackColor);
                    }
                }

                SubtitleListview1.SetBackgroundColor(i, SubtitleListview1.BackColor);
            }

            foreach (var index in fixedIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, Color.LightGreen);
            }

            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.DurationsBridgeGaps.GapsBridgedX, FixedCount);

            Cursor = Cursors.Default;
        }

        private void numericUpDownMaxMs_ValueChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void numericUpDownMinMsBetweenLines_ValueChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void DurationsBridgeGaps_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void DurationsBridgeGaps_Shown(object sender, EventArgs e)
        {
            SubtitleListview1.Focus();
        }

        private void numericUpDownMaxMs_Validated(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void numericUpDownMaxMs_KeyUp(object sender, KeyEventArgs e)
        {
            GeneratePreview();
        }

        private void numericUpDownMinMsBetweenLines_Validated(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void numericUpDownMinMsBetweenLines_KeyUp(object sender, KeyEventArgs e)
        {
            GeneratePreview();
        }

        private void SubtitleListView1_Resize(object sender, EventArgs e)
        {
            const int lastColumnWidth = 150;
            var columnsCount = SubtitleListview1.Columns.Count - 1;
            var width = 0;
            for (int i = 0; i < columnsCount - 1; i++)
            {
                width += SubtitleListview1.Columns[i].Width;
            }
            SubtitleListview1.Columns[columnsCount - 1].Width = SubtitleListview1.Width - (width + lastColumnWidth);
            SubtitleListview1.Columns[columnsCount].Width = lastColumnWidth;
        }

        public void InitializeSettingsOnly()
        {
            groupBoxLinesFound.Enabled = false;
            Height = MinimumSize.Height;
            MaximizeBox = false;
            MinimizeBox = false;
        }
    }
}

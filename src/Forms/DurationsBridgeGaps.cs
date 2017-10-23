using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class DurationsBridgeGaps : PositionAndSizeForm
    {
        private readonly Subtitle _subtitle;
        private Subtitle _fixedSubtitle;
        private Dictionary<string, string> _dic;
        private readonly Timer _refreshTimer = new Timer();
        public Subtitle FixedSubtitle => _fixedSubtitle;
        public int FixedCount { get; private set; }
        public int MinMsBetweenLines
        {
            get { return (int)numericUpDownMinMsBetweenLines.Value; }
            set { numericUpDownMinMsBetweenLines.Value = value; }
        }

        public bool PreviousSubtitleTakesAllTime
        {
            get { return radioButtonProlongEndTime.Checked; }
            set { radioButtonProlongEndTime.Checked = value; }
        }

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
                numericUpDownMinMsBetweenLines.Value = Configuration.Settings.General.MinimumMillisecondsBetweenLines;

            if (subtitle != null)
            {
                _refreshTimer.Interval = 400;
                _refreshTimer.Tick += RefreshTimerTick;
                GeneratePreview();
            }
        }

        private void RefreshTimerTick(object sender, EventArgs e)
        {
            _refreshTimer.Stop();
            GeneratePreviewReal();
        }

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
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

        private void GeneratePreviewReal()
        {
            groupBoxLinesFound.Text = string.Empty;
            if (_subtitle == null)
                return;

            Cursor = Cursors.WaitCursor;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            _fixedSubtitle = new Subtitle(_subtitle);
            _dic = new Dictionary<string, string>();
            var fixedIndexes = new List<int>(_fixedSubtitle.Paragraphs.Count);

            var minMsBetweenLines = (int)numericUpDownMinMsBetweenLines.Value;
            FixedCount = Core.Forms.DurationsBridgeGaps.BridgeGaps(_fixedSubtitle, minMsBetweenLines, radioButtonDivideEven.Checked, (double)numericUpDownMaxMs.Value, fixedIndexes, _dic);

            SubtitleListview1.Fill(_fixedSubtitle);
            for (int i = 0; i < _fixedSubtitle.Paragraphs.Count - 1; i++)
            {
                Paragraph cur = _fixedSubtitle.Paragraphs[i];
                if (_dic.ContainsKey(cur.ID))
                    SubtitleListview1.SetExtraText(i, _dic[cur.ID], SubtitleListview1.ForeColor);
                SubtitleListview1.SetBackgroundColor(i, SubtitleListview1.BackColor);
            }

            foreach (var index in fixedIndexes)
                SubtitleListview1.SetBackgroundColor(index, Color.LightGreen);
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
                DialogResult = DialogResult.Cancel;
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

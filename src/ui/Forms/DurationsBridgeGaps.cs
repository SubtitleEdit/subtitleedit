using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class DurationsBridgeGaps : PositionAndSizeForm
    {
        private readonly Subtitle _subtitle;
        private Dictionary<string, string> _dic;
        private readonly Timer _refreshTimer = new Timer();
        public Subtitle FixedSubtitle { get; private set; }
        public int FixedCount { get; private set; }
        public int MinMsBetweenLines
        {
            get => (int)numericUpDownMinMsBetweenLines.Value;
            set => numericUpDownMinMsBetweenLines.Value = value;
        }

        private static readonly Color _listViewGreen = Configuration.Settings.General.UseDarkTheme ? Color.Green : Color.LightGreen;

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

            Text = LanguageSettings.Current.DurationsBridgeGaps.Title;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            SubtitleListview1.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.ShowExtraColumn(Configuration.Settings.General.UseTimeFormatHHMMSSFF ? LanguageSettings.Current.DurationsBridgeGaps.GapToNextFrames : LanguageSettings.Current.DurationsBridgeGaps.GapToNext);
            SubtitleListview1.AutoSizeAllColumns(this);

            labelBridgePart1.Text = LanguageSettings.Current.DurationsBridgeGaps.BridgeGapsSmallerThanXPart1;
            labelMilliseconds.Text = LanguageSettings.Current.DurationsBridgeGaps.BridgeGapsSmallerThanXPart2;
            labelMinMsBetweenLines.Text = LanguageSettings.Current.DurationsBridgeGaps.MinMillisecondsBetweenLines;
            radioButtonProlongEndTime.Text = LanguageSettings.Current.DurationsBridgeGaps.ProlongEndTime;
            radioButtonDivideEven.Text = LanguageSettings.Current.DurationsBridgeGaps.DivideEven;
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

            if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
            {
                labelBridgePart1.Text = LanguageSettings.Current.DurationsBridgeGaps.BridgeGapsSmallerThanXPart1Frames;
                labelMilliseconds.Text = LanguageSettings.Current.DurationsBridgeGaps.BridgeGapsSmallerThanXPart2Frames;

                labelMinMsBetweenLines.Text = LanguageSettings.Current.DurationsBridgeGaps.MinFramesBetweenLines;

                numericUpDownMaxMs.Value = SubtitleFormat.MillisecondsToFrames((double)numericUpDownMaxMs.Value);
                numericUpDownMaxMs.Increment = 1;
                numericUpDownMinMsBetweenLines.Value = SubtitleFormat.MillisecondsToFrames((double)numericUpDownMinMsBetweenLines.Value);
                numericUpDownMinMsBetweenLines.Increment = 1;
            }

            numericUpDownMinMsBetweenLines.Left = labelMinMsBetweenLines.Left + labelMinMsBetweenLines.Width + 4;
            numericUpDownMaxMs.Left = labelBridgePart1.Left + labelBridgePart1.Width + 4;
            labelMilliseconds.Left = numericUpDownMaxMs.Left + numericUpDownMaxMs.Width + 4;

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
            get => base.Text;
            set => base.Text = value;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.BridgeGapMilliseconds = (int)numericUpDownMaxMs.Value;
            if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
            {
                Configuration.Settings.Tools.BridgeGapMilliseconds = SubtitleFormat.FramesToMilliseconds((double)numericUpDownMaxMs.Value);
            }
                
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
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            FixedSubtitle = new Subtitle(_subtitle);
            _dic = new Dictionary<string, string>();
            var fixedIndexes = new List<int>(FixedSubtitle.Paragraphs.Count);
            var minMsBetweenLines = (int)numericUpDownMinMsBetweenLines.Value;
            var maxMs = (double)numericUpDownMaxMs.Value;
            if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
            {
                minMsBetweenLines = SubtitleFormat.FramesToMilliseconds(minMsBetweenLines);
                maxMs = SubtitleFormat.FramesToMilliseconds(maxMs);
            }

            FixedCount = Core.Forms.DurationsBridgeGaps.BridgeGaps(FixedSubtitle, minMsBetweenLines, radioButtonDivideEven.Checked, maxMs, fixedIndexes, _dic, Configuration.Settings.General.UseTimeFormatHHMMSSFF);
            SubtitleListview1.Fill(FixedSubtitle);
            for (int i = 0; i < FixedSubtitle.Paragraphs.Count; i++)
            {
                var cur = FixedSubtitle.Paragraphs[i];
                if (_dic.ContainsKey(cur.Id))
                {
                    SubtitleListview1.SetExtraText(i, _dic[cur.Id], SubtitleListview1.ForeColor);
                }
                else
                {
                    var info = string.Empty;
                    var next = FixedSubtitle.GetParagraphOrDefault(i + 1);
                    if (next != null)
                    {
                        var gap = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;
                        info = $"{ gap / TimeCode.BaseUnit:0.000}";
                        if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
                        {
                            info = $"{ SubtitleFormat.MillisecondsToFrames(gap)}";
                        }
                    }
                    SubtitleListview1.SetExtraText(i, info, SubtitleListview1.ForeColor);
                }

                SubtitleListview1.SetBackgroundColor(i, SubtitleListview1.BackColor);
            }

            foreach (var index in fixedIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, _listViewGreen);
            }

            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.DurationsBridgeGaps.GapsBridgedX, FixedCount);

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
            const int lastColumnWidth = 165;
            var columnsCount = SubtitleListview1.Columns.Count - 1;
            var width = 0;
            for (int i = 0; i < columnsCount - 1; i++)
            {
                width += SubtitleListview1.Columns[i].Width;
            }
            SubtitleListview1.Columns[columnsCount - 1].Width = SubtitleListview1.Width - (width + lastColumnWidth);
            SubtitleListview1.Columns[columnsCount].Width = -2;
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

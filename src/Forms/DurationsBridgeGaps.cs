using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class DurationsBridgeGaps : PositionAndSizeForm
    {
        private Subtitle _subtitle;
        private Subtitle _fixedSubtitle;
        private Dictionary<string, string> _dic;

        public Subtitle FixedSubtitle { get { return _fixedSubtitle; } }

        public DurationsBridgeGaps(Subtitle subtitle)
        {
            InitializeComponent();
            Utilities.FixLargeFonts(this, buttonOK);

            Text = Configuration.Settings.Language.DurationsBridgeGaps.Title;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            Utilities.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.ShowExtraColumn(Configuration.Settings.Language.DurationsBridgeGaps.GapToNext);
            SubtitleListview1.DisplayExtraFromExtra = true;
            SubtitleListview1.AutoSizeAllColumns(this);

            labelBridgePart1.Text = Configuration.Settings.Language.DurationsBridgeGaps.BridgeGapsSmallerThanXPart1;
            numericUpDownMaxMs.Left = labelBridgePart1.Left + labelBridgePart1.Width + 4;
            labelMilliseconds.Text = Configuration.Settings.Language.DurationsBridgeGaps.BridgeGapsSmallerThanXPart2;
            labelMilliseconds.Left = numericUpDownMaxMs.Left + numericUpDownMaxMs.Width + 4;
            labelMinMsBetweenLines.Text = Configuration.Settings.Language.DurationsBridgeGaps.MinMillisecondsBetweenLines;
            numericUpDownMinMsBetweenLines.Left = labelMinMsBetweenLines.Left + labelMinMsBetweenLines.Width + 4;
            radioButtonProlongEndTime.Text = Configuration.Settings.Language.DurationsBridgeGaps.ProlongEndTime;
            radioButtonDivideEven.Text = Configuration.Settings.Language.DurationsBridgeGaps.DivideEven;

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

            GeneratePreview();
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
            if (_subtitle == null)
                return;

            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            int count = 0;
            _fixedSubtitle = new Subtitle(_subtitle);
            _dic = new Dictionary<string, string>();
            var fixedIndexes = new List<int>(_fixedSubtitle.Paragraphs.Count);

            int minMsBetweenLines = (int)numericUpDownMinMsBetweenLines.Value;
            for (int i = 0; i < _fixedSubtitle.Paragraphs.Count - 1; i++)
            {
                Paragraph cur = _fixedSubtitle.Paragraphs[i];
                Paragraph next = _fixedSubtitle.Paragraphs[i + 1];
                string before = null;
                var difMs = Math.Abs(cur.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds);
                if (difMs < (double)numericUpDownMaxMs.Value && difMs > minMsBetweenLines && numericUpDownMaxMs.Value > minMsBetweenLines)
                {
                    before = string.Format("{0:0.000}", (next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds) / TimeCode.BaseUnit);
                    if (radioButtonDivideEven.Checked && next.StartTime.TotalMilliseconds > cur.EndTime.TotalMilliseconds)
                    {
                        double half = (next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds) / 2.0;
                        next.StartTime.TotalMilliseconds -= half;
                    }
                    cur.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - minMsBetweenLines;
                    fixedIndexes.Add(i);
                    fixedIndexes.Add(i + 1);
                    count++;
                }
                var msToNext = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds;
                if (msToNext < 2000)
                {
                    string info = string.Empty;
                    if (!string.IsNullOrEmpty(before))
                        info = string.Format("{0} => {1:0.000}", before, msToNext / TimeCode.BaseUnit);
                    else
                        info = string.Format("{0:0.000}", msToNext / TimeCode.BaseUnit);
                    _dic.Add(cur.ID, info);
                }
            }

            SubtitleListview1.Fill(_fixedSubtitle);
            for (int i = 0; i < _fixedSubtitle.Paragraphs.Count - 1; i++)
            {
                Paragraph cur = _fixedSubtitle.Paragraphs[i];
                if (_dic != null && _dic.ContainsKey(cur.ID))
                    SubtitleListview1.SetExtraText(i, _dic[cur.ID], SubtitleListview1.ForeColor);
                SubtitleListview1.SetBackgroundColor(i, SubtitleListview1.BackColor);
            }

            foreach (var index in fixedIndexes)
                SubtitleListview1.SetBackgroundColor(index, Color.Green);
            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.DurationsBridgeGaps.GapsBridgedX, count);
        }

        private void numericUpDownMaxMs_ValueChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void DurationsBridgeGaps_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

    }
}

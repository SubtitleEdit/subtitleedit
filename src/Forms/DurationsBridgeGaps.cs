using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class DurationsBridgeGaps : Form
    {
        private Subtitle _subtitle;
        private Subtitle _fixedSubtitle;

        public Subtitle FixedSubtitle { get { return _fixedSubtitle; } }

        public DurationsBridgeGaps(Subtitle subtitle)
        {
            InitializeComponent();
            FixLargeFonts();

            Text = Configuration.Settings.Language.DurationsBridgeGaps.Title;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
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
            GeneratePreview();
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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.BridgeGapMilliseconds = (int)numericUpDownMaxMs.Value;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
                return;

            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            int count = 0;
            _fixedSubtitle = new Subtitle(_subtitle);
            var fixedIndexes = new List<int>();

            for (int i = 0; i < _fixedSubtitle.Paragraphs.Count - 1; i++)
            {
                
                Paragraph cur = _fixedSubtitle.Paragraphs[i];
                Paragraph next = _fixedSubtitle.Paragraphs[i + 1];
                string before = null;
                if (Math.Abs(cur.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds) < (double)numericUpDownMaxMs.Value)
                {
                    before = string.Format("{0:0.000}", (next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds - 1) / 1000.0);
                    if (radioButtonDivideEven.Checked && next.StartTime.TotalMilliseconds > cur.EndTime.TotalMilliseconds)
                    {
                        double half = (next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds) / 2.0;
                        next.StartTime.TotalMilliseconds -= half;
                    }
                    cur.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                    fixedIndexes.Add(i);
                    fixedIndexes.Add(i+1);
                    count++;
                }
                var msToNext = next.StartTime.TotalMilliseconds - cur.EndTime.TotalMilliseconds -1;
                if (msToNext < 2000)
                {
                    if (!string.IsNullOrEmpty(before))
                        cur.Extra = string.Format("{0} => {1:0.000}", before, msToNext / 1000.0);
                    else
                        cur.Extra = string.Format("{0:0.000}", msToNext / 1000.0);
                }
            }

            SubtitleListview1.Fill(_fixedSubtitle);
            for (int i = 0; i < _fixedSubtitle.Paragraphs.Count - 1; i++)
            {
                Paragraph cur = _fixedSubtitle.Paragraphs[i];
                SubtitleListview1.SetExtraText(i, cur.Extra, SubtitleListview1.ForeColor);
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

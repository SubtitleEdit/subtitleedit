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
            SubtitleListview1.AutoSizeAllColumns(this);

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
                if (Math.Abs(cur.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds) < (double)numericUpDownMaxMs.Value)
                {
                    cur.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds-1;
                    fixedIndexes.Add(i);
                    fixedIndexes.Add(i+1);
                    count++;
                }
            }

            SubtitleListview1.Fill(_fixedSubtitle);
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

    }
}

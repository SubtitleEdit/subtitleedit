using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class SetMinimumDisplayTimeBetweenParagraphs : PositionAndSizeForm
    {

        private Subtitle _subtitle;
        public int FixCount { get; private set; }

        public SetMinimumDisplayTimeBetweenParagraphs()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = Configuration.Settings.Language.SetMinimumDisplayTimeBetweenParagraphs.Title;
            labelMaxMillisecondsBetweenLines.Text = Configuration.Settings.Language.SetMinimumDisplayTimeBetweenParagraphs.MinimumMillisecondsBetweenParagraphs;
            checkBoxShowOnlyChangedLines.Text = Configuration.Settings.Language.SetMinimumDisplayTimeBetweenParagraphs.ShowOnlyModifiedLines;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);
            UiUtil.FixLargeFonts(this, buttonOK);

            groupBoxFrameInfo.Text = Configuration.Settings.Language.SetMinimumDisplayTimeBetweenParagraphs.FrameInfo;
            comboBoxFrameRate.Items.Add(23.976.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(24.0.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(25.0.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(29.97.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(30.0.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(59.94.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(60.ToString(CultureInfo.CurrentCulture));
            if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 23.976) < 0.1)
            {
                comboBoxFrameRate.SelectedIndex = 0;
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 24) < 0.1)
            {
                comboBoxFrameRate.SelectedIndex = 1;
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 25) < 0.1)
            {
                comboBoxFrameRate.SelectedIndex = 2;
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 29.97) < 0.01)
            {
                comboBoxFrameRate.SelectedIndex = 3;
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 30) < 0.1)
            {
                comboBoxFrameRate.SelectedIndex = 4;
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 59.94) < 0.1)
            {
                comboBoxFrameRate.SelectedIndex = 5;
            }
            else if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 60) < 0.1)
            {
                comboBoxFrameRate.SelectedIndex = 6;
            }
            else
            {
                comboBoxFrameRate.SelectedIndex = 3;
            }
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        public Subtitle FixedSubtitle { get; private set; }

        public void Initialize(Subtitle subtitle)
        {
            _subtitle = subtitle;
            numericUpDownMinMillisecondsBetweenLines.Value = Configuration.Settings.General.MinimumMillisecondsBetweenLines != 0
                                                           ? Configuration.Settings.General.MinimumMillisecondsBetweenLines
                                                           : 1;
        }

        private void GeneratePreview()
        {
            var fixes = new List<int>();
            if (_subtitle == null)
            {
                return;
            }

            FixedSubtitle = new Subtitle(_subtitle);
            var onlyFixedSubtitle = new Subtitle();

            double minumumMillisecondsBetweenLines = (double)numericUpDownMinMillisecondsBetweenLines.Value;
            for (int i = 0; i < FixedSubtitle.Paragraphs.Count - 1; i++)
            {
                Paragraph p = FixedSubtitle.GetParagraphOrDefault(i);
                Paragraph next = FixedSubtitle.GetParagraphOrDefault(i + 1);
                if (next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds < minumumMillisecondsBetweenLines)
                {
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - minumumMillisecondsBetweenLines;
                    fixes.Add(i);
                    onlyFixedSubtitle.Paragraphs.Add(new Paragraph(p));
                }
            }

            SubtitleListview1.BeginUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.SetMinimumDisplayTimeBetweenParagraphs.PreviewLinesModifiedX, fixes.Count);
            if (checkBoxShowOnlyChangedLines.Checked)
            {
                SubtitleListview1.Fill(onlyFixedSubtitle);
            }
            else
            {
                SubtitleListview1.Fill(FixedSubtitle);
                foreach (int index in fixes)
                {
                    SubtitleListview1.SetBackgroundColor(index, Color.Silver);
                }
            }
            SubtitleListview1.EndUpdate();
            FixCount = fixes.Count;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SetMinimalDisplayTimeDifference_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                e.SuppressKeyPress = true;
            }
        }

        private void numericUpDownMinMillisecondsBetweenLines_ValueChanged(object sender, EventArgs e)
        {
            GeneratePreview();
            Configuration.Settings.General.MinimumMillisecondsBetweenLines = (int)numericUpDownMinMillisecondsBetweenLines.Value;
        }

        private void checkBoxShowOnlyChangedLines_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void numericUpDownMinMillisecondsBetweenLines_KeyUp(object sender, KeyEventArgs e)
        {
            numericUpDownMinMillisecondsBetweenLines.ValueChanged -= numericUpDownMinMillisecondsBetweenLines_ValueChanged;
            GeneratePreview();
            numericUpDownMinMillisecondsBetweenLines.ValueChanged += numericUpDownMinMillisecondsBetweenLines_ValueChanged;
            Configuration.Settings.General.MinimumMillisecondsBetweenLines = (int)numericUpDownMinMillisecondsBetweenLines.Value;
        }

        private void comboBoxFrameRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!double.TryParse(comboBoxFrameRate.Text.Trim().Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var frameRate))
            {
                frameRate = 25.0;
            }

            long ms = (long)Math.Round(TimeCode.BaseUnit / frameRate);
            labelOneFrameIsXMS.Text = string.Format(Configuration.Settings.Language.SetMinimumDisplayTimeBetweenParagraphs.OneFrameXisYMilliseconds, frameRate, ms);
        }

        private void comboBoxFrameRate_KeyUp(object sender, KeyEventArgs e)
        {
            comboBoxFrameRate_SelectedIndexChanged(sender, e);
        }

        private void SetMinimumDisplayTimeBetweenParagraphs_Shown(object sender, EventArgs e)
        {
            SubtitleListview1.Focus();
        }

    }

}

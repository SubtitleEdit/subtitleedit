using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChangeSpeedInPercent : PositionAndSizeForm
    {

        public double AdjustFactor { get; private set; }
        public bool AdjustAllLines { get; private set; }

        public ChangeSpeedInPercent(int numberOfSelectedLines)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = Configuration.Settings.Language.ChangeSpeedInPercent.Title;
            groupBoxInfo.Text = Configuration.Settings.Language.ChangeSpeedInPercent.Info;
            radioButtonAllLines.Text = Configuration.Settings.Language.ShowEarlierLater.AllLines;
            radioButtonSelectedLinesOnly.Text = Configuration.Settings.Language.ShowEarlierLater.SelectedLinesOnly;
            radioButtonSpeedCustom.Text = Configuration.Settings.Language.ChangeSpeedInPercent.Custom;
            radioButtonSpeedFromDropFrame.Text = Configuration.Settings.Language.ChangeSpeedInPercent.FromDropFrame;
            radioButtonToDropFrame.Text = Configuration.Settings.Language.ChangeSpeedInPercent.ToDropFrame;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            if (string.IsNullOrEmpty(Configuration.Settings.Language.ChangeSpeedInPercent.ToDropFrame))
            {
                radioButtonSpeedCustom.Visible = false;
                radioButtonSpeedFromDropFrame.Visible = false;
                radioButtonToDropFrame.Visible = false;
            }

            UiUtil.FixLargeFonts(this, buttonOK);

            if (numberOfSelectedLines > 1)
            {
                radioButtonSelectedLinesOnly.Checked = true;
            }
            else
            {
                radioButtonAllLines.Checked = true;
            }
        }

        private void ChangeSpeedInPercent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#sync");
                e.SuppressKeyPress = true;
            }
        }

        public Subtitle AdjustAllParagraphs(Subtitle subtitle)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                AdjustParagraph(p);
            }

            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (next != null)
                {
                    if (p.EndTime.TotalMilliseconds >= next.StartTime.TotalMilliseconds)
                    {
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                    }
                }
            }
            return subtitle;
        }

        public void AdjustParagraph(Paragraph p)
        {
            p.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds * AdjustFactor;
            p.EndTime.TotalMilliseconds = p.EndTime.TotalMilliseconds * AdjustFactor;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            AdjustFactor = Convert.ToDouble(numericUpDownPercent.Value) / 100.0;
            AdjustAllLines = radioButtonAllLines.Checked;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void radioButtonSpeedCustom_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownPercent.Enabled = true;
        }

        private void radioButtonSpeedFromDropFrame_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownPercent.Value = Convert.ToDecimal(099.98887);
            numericUpDownPercent.Enabled = false;
        }

        private void radioButtonToDropFrame_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownPercent.Value = Convert.ToDecimal(100.1001001);
            numericUpDownPercent.Enabled = false;
        }

    }
}

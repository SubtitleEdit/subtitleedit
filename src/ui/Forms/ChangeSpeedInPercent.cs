using Nikse.SubtitleEdit.Core.Common;
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
            Text = LanguageSettings.Current.ChangeSpeedInPercent.Title;
            groupBoxInfo.Text = LanguageSettings.Current.ChangeSpeedInPercent.Info;
            radioButtonAllLines.Text = LanguageSettings.Current.ShowEarlierLater.AllLines;
            radioButtonSelectedLinesOnly.Text = LanguageSettings.Current.ShowEarlierLater.SelectedLinesOnly;
            radioButtonSpeedCustom.Text = LanguageSettings.Current.ChangeSpeedInPercent.Custom;
            radioButtonSpeedFromDropFrame.Text = LanguageSettings.Current.ChangeSpeedInPercent.FromDropFrame;
            radioButtonToDropFrame.Text = LanguageSettings.Current.ChangeSpeedInPercent.ToDropFrame;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            if (string.IsNullOrEmpty(LanguageSettings.Current.ChangeSpeedInPercent.ToDropFrame))
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
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#sync");
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

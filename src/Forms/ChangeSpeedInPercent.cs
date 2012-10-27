using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChangeSpeedInPercent : Form
    {

        public double AdjustFactor { get; private set; }
        public bool AdjustAllLines { get; private set; }

        public ChangeSpeedInPercent(int numberOfSelectedLines)
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.ChangeSpeedInPercent.Title;
            labelInfo.Text = Configuration.Settings.Language.ChangeSpeedInPercent.Info;
            radioButtonAllLines.Text = Configuration.Settings.Language.ShowEarlierLater.AllLines;
            radioButtonSelectedLinesOnly.Text = Configuration.Settings.Language.ShowEarlierLater.SelectedLinesOnly;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();

            if (numberOfSelectedLines > 1)
                radioButtonSelectedLinesOnly.Checked = true;
            else
                radioButtonAllLines.Checked = true;
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

        private void ChangeSpeedInPercent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.F1)
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
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
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


    }
}

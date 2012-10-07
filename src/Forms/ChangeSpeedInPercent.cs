using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChangeSpeedInPercent : Form
    {

        private double _adjustFactor = 0;

        public ChangeSpeedInPercent()
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.ChangeSpeedInPercent.Title;
            labelInfo.Text = Configuration.Settings.Language.ChangeSpeedInPercent.Info;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
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
                DialogResult = DialogResult.Cancel;
        }

        public Subtitle AdjustSubtitle(Subtitle subtitle)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                p.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds * _adjustFactor;
                p.EndTime.TotalMilliseconds = p.EndTime.TotalMilliseconds * _adjustFactor;
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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _adjustFactor = Convert.ToDouble(numericUpDownPercent.Value) / 100.0;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }


    }
}

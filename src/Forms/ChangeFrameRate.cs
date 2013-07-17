using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChangeFrameRate : Form
    {
        public ChangeFrameRate()
        {
            InitializeComponent();

            comboBoxFrameRateFrom.Items.Add((23.976).ToString());
            comboBoxFrameRateFrom.Items.Add((24.0).ToString());
            comboBoxFrameRateFrom.Items.Add((25.0).ToString());
            comboBoxFrameRateFrom.Items.Add((29.97).ToString());

            comboBoxFrameRateTo.Items.Add((23.976).ToString());
            comboBoxFrameRateTo.Items.Add((24.0).ToString());
            comboBoxFrameRateTo.Items.Add((25.0).ToString());
            comboBoxFrameRateTo.Items.Add((29.97).ToString());

            LanguageStructure.ChangeFrameRate language = Configuration.Settings.Language.ChangeFrameRate;
            Text = language.Title;
            labelInfo.Text = language.ConvertFrameRateOfSubtitle;
            labelFromFrameRate.Text = language.FromFrameRate;
            labelToFrameRate.Text = language.ToFrameRate;
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

        private void FormChangeFrameRate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        public void Initialize(string fromFrameRate)
        {
            comboBoxFrameRateFrom.Text = fromFrameRate;
        }

        private string GetFrameRateFromVideoFile(string oldFrameRate)
        {
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetVideoFileFilter(false);
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                VideoInfo info = Utilities.GetVideoInfo(openFileDialog1.FileName, delegate { Application.DoEvents(); });
                if (info != null && info.Success)
                {
                    return info.FramesPerSecond.ToString();
                }
            }
            return oldFrameRate;
        }

        private void ButtonGetFrameRateFromClick(object sender, EventArgs e)
        {
            comboBoxFrameRateFrom.Text = GetFrameRateFromVideoFile(comboBoxFrameRateFrom.Text);
        }

        private void ButtonGetFrameRateToClick(object sender, EventArgs e)
        {
            comboBoxFrameRateTo.Text = GetFrameRateFromVideoFile(comboBoxFrameRateTo.Text);
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            double d;
            if (double.TryParse(comboBoxFrameRateFrom.Text, out d) &&
                double.TryParse(comboBoxFrameRateTo.Text, out d))
            {
                DialogResult = DialogResult.OK;
            }
            else if (comboBoxFrameRateFrom.Text.Trim() == comboBoxFrameRateTo.Text.Trim())
            {
                MessageBox.Show(Configuration.Settings.Language.ChangeFrameRate.FrameRateNotChanged);
            }
            else
            {
                MessageBox.Show(Configuration.Settings.Language.ChangeFrameRate.FrameRateNotCorrect);
            }
        }

        public double OldFrameRate
        {
            get
            {
                return double.Parse(comboBoxFrameRateFrom.Text);
            }
        }

        public double NewFrameRate
        {
            get
            {
                return double.Parse(comboBoxFrameRateTo.Text);
            }
        }

    }
}

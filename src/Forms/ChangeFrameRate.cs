using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChangeFrameRate : PositionAndSizeForm
    {
        public ChangeFrameRate()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            comboBoxFrameRateFrom.Items.Add(23.976);
            comboBoxFrameRateFrom.Items.Add(24.0);
            comboBoxFrameRateFrom.Items.Add(25.0);
            comboBoxFrameRateFrom.Items.Add(29.97);

            comboBoxFrameRateTo.Items.Add(23.976);
            comboBoxFrameRateTo.Items.Add(24.0);
            comboBoxFrameRateTo.Items.Add(25.0);
            comboBoxFrameRateTo.Items.Add(29.97);

            LanguageStructure.ChangeFrameRate language = Configuration.Settings.Language.ChangeFrameRate;
            Text = language.Title;
            labelInfo.Text = language.ConvertFrameRateOfSubtitle;
            labelFromFrameRate.Text = language.FromFrameRate;
            labelToFrameRate.Text = language.ToFrameRate;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
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
                VideoInfo info = UiUtil.GetVideoInfo(openFileDialog1.FileName);
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
            if (comboBoxFrameRateFrom.Text.Trim() == comboBoxFrameRateTo.Text.Trim())
            {
                MessageBox.Show(Configuration.Settings.Language.ChangeFrameRate.FrameRateNotChanged);
                return;
            }

            double d;
            if (double.TryParse(comboBoxFrameRateFrom.Text, out d) && double.TryParse(comboBoxFrameRateTo.Text, out d))
            {
                DialogResult = DialogResult.OK;
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

using Nikse.SubtitleEdit.Core.Common;
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

            InitializeCombobox(comboBoxFrameRateFrom);
            InitializeCombobox(comboBoxFrameRateTo);

            var language = LanguageSettings.Current.ChangeFrameRate;
            Text = language.Title;
            labelInfo.Text = language.ConvertFrameRateOfSubtitle;
            labelFromFrameRate.Text = language.FromFrameRate;
            labelToFrameRate.Text = language.ToFrameRate;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
            if (Configuration.IsRunningOnWindows)
            {
                buttonSwap.Text = "🡙";
            }
            else
            {
                buttonSwap.Text = "<->";
                buttonSwap.Width = 35;
                buttonSwap.Font = new Font(Font.FontFamily, Font.Size);
            }
        }

        private void InitializeCombobox(ComboBox comboBox)
        {
            comboBox.BeginUpdate();
            comboBox.Items.Clear();
            comboBox.Items.Add(23.976);
            comboBox.Items.Add(24.0);
            comboBox.Items.Add(25.0);
            comboBox.Items.Add(29.97);
            comboBox.Items.Add(30);
            comboBox.EndUpdate();
        }

        private void FormChangeFrameRate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        public void Initialize(string fromFrameRate)
        {
            comboBoxFrameRateFrom.Text = fromFrameRate;
        }

        private string GetFrameRateFromVideoFile(string oldFrameRate)
        {
            openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.GetVideoFileFilter(false);
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var info = UiUtil.GetVideoInfo(openFileDialog1.FileName);
                if (info?.Success == true)
                {
                    return Math.Round(info.FramesPerSecond, 3).ToString();
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
                MessageBox.Show(LanguageSettings.Current.ChangeFrameRate.FrameRateNotChanged);
                return;
            }

            if (double.TryParse(comboBoxFrameRateFrom.Text, out _) && double.TryParse(comboBoxFrameRateTo.Text, out _))
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(LanguageSettings.Current.ChangeFrameRate.FrameRateNotCorrect);
            }
        }

        private void buttonSwap_Click(object sender, EventArgs e)
        {
            string oldFrameRate = comboBoxFrameRateFrom.Text;
            string newFrameRate = comboBoxFrameRateTo.Text;

            comboBoxFrameRateFrom.Text = newFrameRate;
            comboBoxFrameRateTo.Text = oldFrameRate;
        }

        public double OldFrameRate => double.Parse(comboBoxFrameRateFrom.Text);

        public double NewFrameRate => double.Parse(comboBoxFrameRateTo.Text);
    }
}

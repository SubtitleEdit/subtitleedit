using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public partial class SettingsGapChoose : Form
    {
        public int MinGapMs { get; internal set; }

        public SettingsGapChoose()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            comboBoxFrameRate.Items.Clear();
            comboBoxFrameRate.Items.Add(23.976.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(24.0.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(25.0.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(29.97.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(30.00.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(50.00.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(59.94.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(60.00.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.SelectedIndex = 0;

            numericUpDownFrames.Value = 2;

            Text = Configuration.Settings.Language.Settings.MinimumGapMilliseconds;
            labelFrameRate.Text = Configuration.Settings.Language.General.FrameRate;
            labelFrames.Text = Configuration.Settings.Language.Settings.MinFrameGap;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            labelUseCalc.Font = new System.Drawing.Font(labelCalcInfo.Font.Name, labelCalcInfo.Font.Size, System.Drawing.FontStyle.Bold);
            CalcMilliseconds();
        }

        private void CalcMilliseconds()
        {
            var frameRate = GetFrameRate();
            MinGapMs = (int)Math.Round(1000.0 / frameRate * (double)numericUpDownFrames.Value);
            labelCalcInfo.Text = string.Format(Configuration.Settings.Language.Settings.XFramesAtYFrameRateGivesZMs, numericUpDownFrames.Value, frameRate, MinGapMs);
            labelUseCalc.Text = string.Format(Configuration.Settings.Language.Settings.UseXAsNewGap, MinGapMs);
        }

        private double GetFrameRate()
        {
            if (double.TryParse(comboBoxFrameRate.Text, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out var frameRate))
            {
                return frameRate;
            }

            return Configuration.Settings.General.CurrentFrameRate;
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SettingsGapChoose_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void comboBoxFrameRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalcMilliseconds();
        }

        private void numericUpDownFrames_ValueChanged(object sender, EventArgs e)
        {
            CalcMilliseconds();
        }

        private void comboBoxFrameRate_TextChanged(object sender, EventArgs e)
        {
            CalcMilliseconds();
        }
    }
}

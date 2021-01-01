using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public partial class SettingsLineWidth : Form
    {
        public SettingsLineWidth()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.Settings;
            var settings = Configuration.Settings.General;
            Text = language.LineWidthSettings;
            labelMaximumLineWidth.Text = language.MaximumLineWidth;
            labelPixelsSuffix.Text = language.Pixels;
            labelMeasureFont.Text = language.MeasureFont;
            checkBoxMeasureFontBold.Text = language.SubtitleBold;

            comboBoxMeasureFontName.BeginUpdate();
            comboBoxMeasureFontName.Items.Clear();
            var comboBoxFontNameList = new List<string>();
            var comboBoxSubtitleFontList = new List<string>();
            var comboBoxSubtitleFontIndex = 0;
            foreach (var x in FontFamily.Families.OrderBy(p => p.Name))
            {
                comboBoxSubtitleFontList.Add(x.Name);
                if (x.Name.Equals(settings.MeasureFontName, StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxSubtitleFontIndex = comboBoxSubtitleFontList.Count - 1;
                }
            }
            comboBoxMeasureFontName.Items.AddRange(comboBoxSubtitleFontList.ToArray<object>());
            comboBoxMeasureFontName.SelectedIndex = comboBoxSubtitleFontIndex;
            comboBoxMeasureFontName.EndUpdate();

            numericUpDownMaxLineWidth.Value = settings.SubtitleLineMaximumPixelWidth;
            numericUpDownMeasureFontSize.Value = settings.MeasureFontSize;
            checkBoxMeasureFontBold.Checked = settings.MeasureFontBold;

            numericUpDownMaxLineWidth.Left = labelMaximumLineWidth.Left + labelMaximumLineWidth.Width + 6;
            labelPixelsSuffix.Left = numericUpDownMaxLineWidth.Left + numericUpDownMaxLineWidth.Width + 6;
            checkBoxMeasureFontBold.Left = numericUpDownMeasureFontSize.Left + numericUpDownMeasureFontSize.Width + 6;
            comboBoxMeasureFontName.Left = numericUpDownMaxLineWidth.Left;
            comboBoxMeasureFontName.Width = numericUpDownMeasureFontSize.Left - numericUpDownMaxLineWidth.Left - 6;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.General.SubtitleLineMaximumPixelWidth = Convert.ToInt32(numericUpDownMaxLineWidth.Value);
            Configuration.Settings.General.MeasureFontName = comboBoxMeasureFontName.Text;
            Configuration.Settings.General.MeasureFontSize = Convert.ToInt32(numericUpDownMeasureFontSize.Value);
            Configuration.Settings.General.MeasureFontBold = checkBoxMeasureFontBold.Checked;

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SettingsLineWidth_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}

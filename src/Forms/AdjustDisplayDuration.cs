using System;
using System.Globalization;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AdjustDisplayDuration : PositionAndSizeForm
    {
        public string AdjustValue
        {
            get
            {
                if (radioButtonPercent.Checked)
                    return comboBoxPercent.Text;
                if (radioButtonAutoRecalculate.Checked)
                    return radioButtonAutoRecalculate.Text + ", " + labelMaxCharsPerSecond.Text + ": " + numericUpDownMaxCharsSec.Value; // TODO: Make language string with string.Format
                return comboBoxSeconds.Text;
            }
        }

        public bool AdjustUsingPercent
        {
            get
            {
                return radioButtonPercent.Checked;
            }
        }

        public bool AdjustUsingSeconds
        {
            get
            {
                return radioButtonSeconds.Checked;
            }
        }

        public decimal MaxCharactersPerSecond
        {
            get
            {
                return numericUpDownMaxCharsSec.Value;
            }
        }

        public AdjustDisplayDuration()
        {
            InitializeComponent();
            Icon = Nikse.SubtitleEdit.Properties.Resources.SubtitleEditFormIcon;

            comboBoxPercent.SelectedIndex = 0;
            comboBoxSeconds.SelectedIndex = 0;

            for (int i = 0; i < comboBoxSeconds.Items.Count; i++)
            {
                string s = comboBoxSeconds.Items[i].ToString();
                s = s.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                comboBoxSeconds.Items[i] = s;
            }
            numericUpDownMaxCharsSec.Value = (decimal)Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds;

            LanguageStructure.AdjustDisplayDuration language = Configuration.Settings.Language.AdjustDisplayDuration;
            Text = language.Title;
            groupBoxAdjustVia.Text = language.AdjustVia;
            radioButtonSeconds.Text = language.Seconds;
            radioButtonPercent.Text = language.Percent;
            radioButtonAutoRecalculate.Text = language.Recalculate;
            labelMaxCharsPerSecond.Text = Configuration.Settings.Language.Settings.MaximumCharactersPerSecond;
            labelAddSeconds.Text = language.AddSeconds;
            labelAddInPercent.Text = language.SetAsPercent;
            labelNote.Text = language.Note;
            comboBoxSeconds.Items[0] = language.PleaseChoose;
            comboBoxPercent.Items[0] = language.PleaseChoose;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            if (labelNote.Left + labelNote.Width + 5 > Width)
                Width = labelNote.Left + labelNote.Width + 5;
            Utilities.FixLargeFonts(this, buttonOK);
        }

        private void FormAdjustDisplayTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void RadioButtonPercentCheckedChanged(object sender, EventArgs e)
        {
            FixEnabled();
        }

        private void FixEnabled()
        {
            if (radioButtonPercent.Checked)
            {
                comboBoxPercent.Enabled = true;
                comboBoxSeconds.Enabled = false;
                numericUpDownMaxCharsSec.Enabled = false;
            }
            else if (radioButtonSeconds.Checked)
            {
                comboBoxPercent.Enabled = false;
                comboBoxSeconds.Enabled = true;
                numericUpDownMaxCharsSec.Enabled = false;
            }
            else
            {
                comboBoxPercent.Enabled = false;
                comboBoxSeconds.Enabled = false;
                numericUpDownMaxCharsSec.Enabled = true;
            }
        }

        private void RadioButtonSecondsCheckedChanged(object sender, EventArgs e)
        {
            FixEnabled();
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (radioButtonSeconds.Checked && comboBoxSeconds.SelectedIndex < 1)
            {
                MessageBox.Show(Configuration.Settings.Language.AdjustDisplayDuration.PleaseSelectAValueFromTheDropDownList);
                comboBoxSeconds.Focus();
            }
            else if (radioButtonPercent.Checked && comboBoxPercent.SelectedIndex < 1)
            {
                MessageBox.Show(Configuration.Settings.Language.AdjustDisplayDuration.PleaseSelectAValueFromTheDropDownList);
                comboBoxPercent.Focus();
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void radioButtonAutoRecalculate_CheckedChanged(object sender, EventArgs e)
        {
            FixEnabled();
        }
    }
}

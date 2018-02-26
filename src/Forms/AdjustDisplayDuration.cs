using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AdjustDisplayDuration : PositionAndSizeForm
    {
        public string AdjustValue
        {
            get
            {
                if (radioButtonPercent.Checked)
                    return numericUpDownPercent.Value.ToString(CultureInfo.InvariantCulture);
                if (radioButtonAutoRecalculate.Checked)
                    return radioButtonAutoRecalculate.Text + ", " + labelMaxCharsPerSecond.Text + ": " + numericUpDownMaxCharsSec.Value; // TODO: Make language string with string.Format
                return numericUpDownSeconds.Value.ToString(CultureInfo.InvariantCulture);
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
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Icon = Properties.Resources.SubtitleEditFormIcon;

            if (Configuration.Settings.Tools.AdjustDurationSeconds >= numericUpDownSeconds.Minimum &&
                Configuration.Settings.Tools.AdjustDurationSeconds <= numericUpDownSeconds.Maximum)
                numericUpDownSeconds.Value = Configuration.Settings.Tools.AdjustDurationSeconds;

            if (Configuration.Settings.Tools.AdjustDurationPercent >= numericUpDownPercent.Minimum &&
                Configuration.Settings.Tools.AdjustDurationPercent <= numericUpDownPercent.Maximum)
                numericUpDownPercent.Value = Configuration.Settings.Tools.AdjustDurationPercent;

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
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();

            if (Configuration.Settings.Tools.AdjustDurationLast == "seconds")
            {
                radioButtonSeconds.Checked = true;
            }
            else if (Configuration.Settings.Tools.AdjustDurationLast == "percent")
            {
                radioButtonPercent.Checked = true;
            }
            else if (Configuration.Settings.Tools.AdjustDurationLast == "recalc")
            {
                radioButtonAutoRecalculate.Checked = true;
            }
        }

        private void FixLargeFonts()
        {
            if (labelNote.Left + labelNote.Width + 5 > Width)
                Width = labelNote.Left + labelNote.Width + 5;
            UiUtil.FixLargeFonts(this, buttonOK);
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
                numericUpDownPercent.Enabled = true;
                numericUpDownSeconds.Enabled = false;
                numericUpDownMaxCharsSec.Enabled = false;
            }
            else if (radioButtonSeconds.Checked)
            {
                numericUpDownPercent.Enabled = false;
                numericUpDownSeconds.Enabled = true;
                numericUpDownMaxCharsSec.Enabled = false;
            }
            else
            {
                numericUpDownPercent.Enabled = false;
                numericUpDownSeconds.Enabled = false;
                numericUpDownMaxCharsSec.Enabled = true;
            }
        }

        private void RadioButtonSecondsCheckedChanged(object sender, EventArgs e)
        {
            FixEnabled();
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.AdjustDurationSeconds = numericUpDownSeconds.Value;
            Configuration.Settings.Tools.AdjustDurationPercent = (int)numericUpDownPercent.Value;

            if (radioButtonSeconds.Checked)
            {
                Configuration.Settings.Tools.AdjustDurationLast = "seconds";
            }
            else if (radioButtonPercent.Checked)
            {
                Configuration.Settings.Tools.AdjustDurationLast = "percent";
            }
            else if (radioButtonAutoRecalculate.Checked)
            {
                Configuration.Settings.Tools.AdjustDurationLast = "recalc";
            }

            DialogResult = DialogResult.OK;
        }

        private void radioButtonAutoRecalculate_CheckedChanged(object sender, EventArgs e)
        {
            FixEnabled();
        }

        public void HideRecalculate()
        {
            if (radioButtonAutoRecalculate.Checked)
                radioButtonSeconds.Checked = true;
            radioButtonAutoRecalculate.Visible = false;
            labelMaxCharsPerSecond.Visible = false;
            numericUpDownMaxCharsSec.Visible = false;
        }

    }
}

using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AdjustDisplayDuration : PositionAndSizeForm
    {
        internal const string Sec = "seconds";
        internal const string Per = "percent";
        internal const string Recal = "recalc";
        internal const string Fixed = "fixed";

        public string AdjustValue
        {
            get
            {
                if (radioButtonPercent.Checked)
                {
                    return numericUpDownPercent.Value.ToString(CultureInfo.InvariantCulture);
                }

                if (radioButtonAutoRecalculate.Checked)
                {
                    return $"{radioButtonAutoRecalculate.Text}, {labelMaxCharsPerSecond.Text}: {numericUpDownMaxCharsSec.Value}";
                }

                return numericUpDownSeconds.Value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public bool AdjustUsingPercent => radioButtonPercent.Checked;

        public bool AdjustUsingSeconds => radioButtonSeconds.Checked;

        public bool AdjustUsingRecalc => radioButtonAutoRecalculate.Checked;

        public decimal MaxCharactersPerSecond => numericUpDownMaxCharsSec.Value;

        public decimal OptimalCharactersPerSecond => numericUpDownOptimalCharsSec.Value;

        public int FixedMilliseconds => (int)numericUpDownFixedMilliseconds.Value;

        public bool ExtendOnly => checkBoxExtendOnly.Checked;

        public AdjustDisplayDuration(bool recalcActive = true)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Icon = Properties.Resources.SEIcon;

            numericUpDownSeconds.Enabled = false;
            numericUpDownPercent.Enabled = false;

            decimal adjustSeconds = Configuration.Settings.Tools.AdjustDurationSeconds;
            if (adjustSeconds >= numericUpDownSeconds.Minimum && adjustSeconds <= numericUpDownSeconds.Maximum)
            {
                numericUpDownSeconds.Value = adjustSeconds;
            }

            int adjustPercent = Configuration.Settings.Tools.AdjustDurationPercent;
            if (adjustPercent >= numericUpDownPercent.Minimum && adjustPercent <= numericUpDownPercent.Maximum)
            {
                numericUpDownPercent.Value = adjustPercent;
            }

            numericUpDownOptimalCharsSec.Value = (decimal)Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds;
            numericUpDownMaxCharsSec.Value = (decimal)Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds;

            checkBoxExtendOnly.Checked = Configuration.Settings.Tools.AdjustDurationExtendOnly;

            LanguageStructure.AdjustDisplayDuration language = LanguageSettings.Current.AdjustDisplayDuration;
            Text = language.Title;
            groupBoxAdjustVia.Text = language.AdjustVia;
            radioButtonSeconds.Text = language.Seconds;
            radioButtonPercent.Text = language.Percent;
            radioButtonAutoRecalculate.Text = language.Recalculate;
            labelOptimalCharsSec.Text = LanguageSettings.Current.Settings.OptimalCharactersPerSecond;
            labelMaxCharsPerSecond.Text = LanguageSettings.Current.Settings.MaximumCharactersPerSecond;
            labelAddSeconds.Text = language.AddSeconds;
            labelAddInPercent.Text = language.SetAsPercent;
            labelNote.Text = language.Note;
            radioButtonFixed.Text = language.Fixed;
            labelMillisecondsFixed.Text = language.Milliseconds;
            checkBoxExtendOnly.Text = language.ExtendOnly;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            FixLargeFonts();

            switch (Configuration.Settings.Tools.AdjustDurationLast)
            {
                case Sec:
                    radioButtonSeconds.Checked = true;
                    break;
                case Per:
                    radioButtonPercent.Checked = true;
                    break;
                case Recal:
                    radioButtonAutoRecalculate.Checked = true;
                    break;
                case Fixed:
                    radioButtonFixed.Checked = true;
                    break;
            }

            if (!recalcActive)
            {
                radioButtonAutoRecalculate.Enabled = false;
                if (radioButtonAutoRecalculate.Checked)
                {
                    radioButtonSeconds.Checked = true;
                }
            }

            FixEnabled();
        }

        private void FixLargeFonts()
        {
            if (labelNote.Left + labelNote.Width + 5 > Width)
            {
                Width = labelNote.Left + labelNote.Width + 5;
            }

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void FormAdjustDisplayTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void RadioButtonPercentCheckedChanged(object sender, EventArgs e)
        {
            FixEnabled();
        }

        private void FixEnabled()
        {
            numericUpDownPercent.Enabled = radioButtonPercent.Checked;
            numericUpDownSeconds.Enabled = radioButtonSeconds.Checked;
            numericUpDownMaxCharsSec.Enabled = radioButtonAutoRecalculate.Checked;
            numericUpDownOptimalCharsSec.Enabled = radioButtonAutoRecalculate.Checked;
            checkBoxExtendOnly.Enabled = radioButtonAutoRecalculate.Checked;
            numericUpDownFixedMilliseconds.Enabled = radioButtonFixed.Checked;
        }

        private void RadioButtonSecondsCheckedChanged(object sender, EventArgs e)
        {
            FixEnabled();
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.AdjustDurationSeconds = numericUpDownSeconds.Value;
            Configuration.Settings.Tools.AdjustDurationPercent = (int)numericUpDownPercent.Value;
            Configuration.Settings.Tools.AdjustDurationExtendOnly = checkBoxExtendOnly.Checked;

            if (radioButtonSeconds.Checked)
            {
                Configuration.Settings.Tools.AdjustDurationLast = Sec;
            }
            else if (radioButtonPercent.Checked)
            {
                Configuration.Settings.Tools.AdjustDurationLast = Per;
            }
            else if (radioButtonAutoRecalculate.Checked)
            {
                Configuration.Settings.Tools.AdjustDurationLast = Recal;
            }
            else if (radioButtonFixed.Checked)
            {
                Configuration.Settings.Tools.AdjustDurationLast = Fixed;
            }

            DialogResult = DialogResult.OK;
        }

        public void HideRecalculate()
        {
            if (radioButtonAutoRecalculate.Checked)
            {
                radioButtonSeconds.Checked = true;
            }

            radioButtonAutoRecalculate.Visible = false;
            labelMaxCharsPerSecond.Visible = false;
            numericUpDownMaxCharsSec.Visible = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            FixEnabled();
        }

        private void radioButtonAutoRecalculate_CheckedChanged(object sender, EventArgs e)
        {
            FixEnabled();
        }
    }
}

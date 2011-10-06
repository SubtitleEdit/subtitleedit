using System;
using System.Globalization;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AdjustDisplayDuration : Form
    {
        public string AdjustValue
        {
            get
            {
                if (radioButtonPercent.Checked)
                    return comboBoxPercent.Text;
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

        public AdjustDisplayDuration()
        {
            InitializeComponent();

            comboBoxPercent.SelectedIndex = 0;
            comboBoxSeconds.SelectedIndex = 0;

            for (int i=0; i<comboBoxSeconds.Items.Count; i++)
            {
                string s = comboBoxSeconds.Items[i].ToString();
                s = s.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                comboBoxSeconds.Items[i] = s;
            }

            LanguageStructure.AdjustDisplayDuration language = Configuration.Settings.Language.AdjustDisplayDuration;
            Text = language.Title;
            groupBoxAdjustVia.Text = language.AdjustVia;
            radioButtonSeconds.Text = language.Seconds;
            radioButtonPercent.Text = language.Percent;
            labelAddSeconds.Text = language.AddSeconds;
            labelAddInPercent.Text = language.SetAsPercent;
            labelNote.Text = language.Note;
            comboBoxSeconds.Items[0] = language.PleaseChoose;
            comboBoxPercent.Items[0] = language.PleaseChoose;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            if (labelNote.Left + labelNote.Width + 5 > Width)
                Width = labelNote.Left + labelNote.Width + 5;

            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void FormAdjustDisplayTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void RadioButtonPercentCheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPercent.Checked)
            {
                comboBoxPercent.Enabled = true;
            }
            else
            {
                comboBoxPercent.Enabled = false;
            }
        }

        private void RadioButtonSecondsCheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSeconds.Checked)
            {
                comboBoxSeconds.Enabled = true;
            }
            else
            {
                comboBoxSeconds.Enabled = false;
            }
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
    }
}

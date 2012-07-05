using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class DCinemaPropertiesInterop : Form
    {

        private Logic.Subtitle _subtitle;
        private string _videoFileName;

        public DCinemaPropertiesInterop(Subtitle _subtitle, string _videoFileName)
        {
            InitializeComponent();

            var l = Configuration.Settings.Language.DCinemaProperties;
            Text = l.Title;
            labelSubtitleID.Text = l.SubtitleId;
            labelMovieTitle.Text = l.MovieTitle;
            labelReelNumber.Text = l.ReelNumber;
            labelLanguage.Text = l.Language;
            groupBoxFont.Text = l.Font;
            labelFontId.Text = l.FontId;
            labelFontUri.Text = l.FontUri;
            labelFontColor.Text = l.FontColor;
            buttonFontColor.Text = l.ChooseColor;
            labelEffect.Text = l.FontEffect;
            labelEffectColor.Text = l.FontEffectColor;
            buttonFontEffectColor.Text = l.ChooseColor;
            labelFontSize.Text = l.FontSize;

            this._subtitle = _subtitle;
            this._videoFileName = _videoFileName;

            foreach (CultureInfo x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                comboBoxLanguage.Items.Add(x.EnglishName);
            }
            comboBoxLanguage.Sorted = true;

            var ss = Configuration.Settings.SubtitleSettings;
            if (!string.IsNullOrEmpty(ss.CurrentDCinemaSubtitleId))
            {
                textBoxSubtitleID.Text = ss.CurrentDCinemaSubtitleId;
                textBoxMovieTitle.Text = ss.CurrentDCinemaMovieTitle;
                int number;
                if (int.TryParse(ss.CurrentDCinemaReelNumber, out number))
                {
                    if (numericUpDownReelNumber.Minimum <= number && numericUpDownReelNumber.Maximum >= number)
                        numericUpDownReelNumber.Value = number;
                }
                comboBoxLanguage.Text = ss.CurrentDCinemaLanguage;
                textBoxFontID.Text = ss.CurrentDCinemaFontId;
                textBoxFontUri.Text = ss.CurrentDCinemaFontUri;
                panelFontColor.BackColor = ss.CurrentDCinemaFontColor;
                if (ss.CurrentDCinemaFontEffect == "border")
                    comboBoxFontEffect.SelectedIndex = 1;
                else if (ss.CurrentDCinemaFontEffect == "shadow")
                    comboBoxFontEffect.SelectedIndex = 2;
                else
                    comboBoxFontEffect.SelectedIndex = 0;
                panelFontEffectColor.BackColor = ss.CurrentDCinemaFontEffectColor;
                numericUpDownFontSize.Value = ss.CurrentDCinemaFontSize;
                if (numericUpDownTopBottomMargin.Minimum <= Configuration.Settings.SubtitleSettings.DCinemaBottomMargin &&
                   numericUpDownTopBottomMargin.Maximum >= Configuration.Settings.SubtitleSettings.DCinemaBottomMargin)
                    numericUpDownTopBottomMargin.Value = Configuration.Settings.SubtitleSettings.DCinemaBottomMargin;
                else
                    numericUpDownTopBottomMargin.Value = 8;
            }
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void buttonGenerateID_Click(object sender, EventArgs e)
        {
            string hex = Guid.NewGuid().ToString().Replace("-", string.Empty);
            textBoxSubtitleID.Text = hex.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
        }

        private void buttonFontColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelFontColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelFontColor.BackColor = colorDialog1.Color;
            }
        }

        private void buttonFontEffectColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelFontEffectColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelFontEffectColor.BackColor = colorDialog1.Color;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var ss = Configuration.Settings.SubtitleSettings;
            ss.CurrentDCinemaSubtitleId = textBoxSubtitleID.Text;
            ss.CurrentDCinemaMovieTitle = textBoxMovieTitle.Text;
            ss.CurrentDCinemaReelNumber = numericUpDownReelNumber.Value.ToString();
            if (comboBoxLanguage.SelectedItem != null)
                ss.CurrentDCinemaLanguage = comboBoxLanguage.SelectedItem.ToString();
            else
                ss.CurrentDCinemaLanguage = string.Empty;
            ss.CurrentDCinemaFontId = textBoxFontID.Text;
            ss.CurrentDCinemaFontUri = textBoxFontUri.Text;
            ss.CurrentDCinemaFontColor = panelFontColor.BackColor;
            if (comboBoxFontEffect.SelectedIndex == 1)
                ss.CurrentDCinemaFontEffect = "border";
            else if (comboBoxFontEffect.SelectedIndex == 2)
                ss.CurrentDCinemaFontEffect = "shadow";
            else
                ss.CurrentDCinemaFontEffect = string.Empty;
            ss.CurrentDCinemaFontEffectColor = panelFontEffectColor.BackColor;
            ss.CurrentDCinemaFontSize = (int)numericUpDownFontSize.Value;
            Configuration.Settings.SubtitleSettings.DCinemaBottomMargin = (int)numericUpDownTopBottomMargin.Value;

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}

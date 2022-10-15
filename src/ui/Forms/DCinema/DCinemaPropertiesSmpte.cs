using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Forms;

namespace Nikse.SubtitleEdit.Forms.DCinema
{
    public sealed partial class DCinemaPropertiesSmpte : PositionAndSizeForm
    {
        private const string ProfileExtension = ".DCinema-interop-profile";

        public DCinemaPropertiesSmpte()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var l = LanguageSettings.Current.DCinemaProperties;
            Text = l.TitleSmpte;
            labelSubtitleID.Text = l.SubtitleId;
            labelMovieTitle.Text = l.MovieTitle;
            labelReelNumber.Text = l.ReelNumber;
            labelLanguage.Text = l.Language;
            labelIssueDate.Text = l.IssueDate;
            labelEditRate.Text = l.EditRate;
            labelTimeCodeRate.Text = l.TimeCodeRate;
            labelStartTime.Text = l.StartTime;
            groupBoxFont.Text = l.Font;
            labelFontId.Text = l.FontId;
            labelFontUri.Text = l.FontUri;
            labelFontColor.Text = l.FontColor;
            buttonFontColor.Text = l.ChooseColor;
            labelEffect.Text = l.FontEffect;
            labelEffectColor.Text = l.FontEffectColor;
            buttonFontEffectColor.Text = l.ChooseColor;
            labelFontSize.Text = l.FontSize;
            buttonGenerateID.Text = l.GenerateId;
            buttonGenFontUri.Text = l.Generate;

            profilesToolStripMenuItem.Text = LanguageSettings.Current.Settings.Profiles;
            exportToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Export;
            importToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Import;
            ContextMenuStrip = contextMenuStripProfile;

            foreach (var x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                comboBoxLanguage.Items.Add(x.TwoLetterISOLanguageName);
            }
            comboBoxLanguage.Sorted = true;

            var ss = Configuration.Settings.SubtitleSettings;
            checkBoxGenerateIdAuto.Checked = ss.DCinemaAutoGenerateSubtitleId;

            if (!string.IsNullOrEmpty(ss.CurrentDCinemaSubtitleId))
            {
                textBoxSubtitleID.Text = ss.CurrentDCinemaSubtitleId;
                if (int.TryParse(ss.CurrentDCinemaReelNumber, out int number) &&
                    numericUpDownReelNumber.Minimum <= number && numericUpDownReelNumber.Maximum >= number)
                {
                    numericUpDownReelNumber.Value = number;
                }
                textBoxMovieTitle.Text = ss.CurrentDCinemaMovieTitle;
                comboBoxLanguage.Text = ss.CurrentDCinemaLanguage;
                textBoxFontID.Text = ss.CurrentDCinemaFontId;
                textBoxEditRate.Text = ss.CurrentDCinemaEditRate;
                comboBoxTimeCodeRate.Text = ss.CurrentDCinemaTimeCodeRate;

                timeUpDownStartTime.ForceHHMMSSFF();
                if (string.IsNullOrEmpty(ss.CurrentDCinemaStartTime))
                {
                    ss.CurrentDCinemaStartTime = "00:00:00:00";
                }

                timeUpDownStartTime.MaskedTextBox.Text = ss.CurrentDCinemaStartTime;

                textBoxFontUri.Text = ss.CurrentDCinemaFontUri;
                textBoxIssueDate.Text = ss.CurrentDCinemaIssueDate;
                panelFontColor.BackColor = ss.CurrentDCinemaFontColor;
                if (ss.CurrentDCinemaFontEffect == "border")
                {
                    comboBoxFontEffect.SelectedIndex = 1;
                }
                else if (ss.CurrentDCinemaFontEffect == "shadow")
                {
                    comboBoxFontEffect.SelectedIndex = 2;
                }
                else
                {
                    comboBoxFontEffect.SelectedIndex = 0;
                }

                panelFontEffectColor.BackColor = ss.CurrentDCinemaFontEffectColor;
                numericUpDownFontSize.Value = ss.CurrentDCinemaFontSize;
                if (numericUpDownTopBottomMargin.Minimum <= ss.DCinemaBottomMargin &&
                    numericUpDownTopBottomMargin.Maximum >= ss.DCinemaBottomMargin)
                {
                    numericUpDownTopBottomMargin.Value = ss.DCinemaBottomMargin;
                }
                else
                {
                    numericUpDownTopBottomMargin.Value = 8;
                }

                if (numericUpDownFadeUp.Minimum <= ss.DCinemaFadeUpTime &&
                    numericUpDownFadeUp.Maximum >= ss.DCinemaFadeUpTime)
                {
                    numericUpDownFadeUp.Value = ss.DCinemaFadeUpTime;
                }
                else
                {
                    numericUpDownFadeUp.Value = 0;
                }

                if (numericUpDownFadeDown.Minimum <= ss.DCinemaFadeDownTime &&
                    numericUpDownFadeDown.Maximum >= ss.DCinemaFadeDownTime)
                {
                    numericUpDownFadeDown.Value = ss.DCinemaFadeDownTime;
                }
                else
                {
                    numericUpDownFadeDown.Value = 0;
                }
            }
            UiUtil.FixLargeFonts(this, buttonCancel);
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

        private void buttonGenerateID_Click(object sender, EventArgs e)
        {
            textBoxSubtitleID.Text = DCinemaSmpte2007.GenerateId();
        }

        private void buttonToday_Click(object sender, EventArgs e)
        {
            textBoxIssueDate.Text = DateTime.Now.ToString("s") + ".000-00:00";
        }

        private void buttonOK_Click_1(object sender, EventArgs e)
        {
            var ss = Configuration.Settings.SubtitleSettings;
            ss.DCinemaAutoGenerateSubtitleId = checkBoxGenerateIdAuto.Checked;
            ss.CurrentDCinemaSubtitleId = textBoxSubtitleID.Text;
            ss.CurrentDCinemaMovieTitle = textBoxMovieTitle.Text;
            ss.CurrentDCinemaReelNumber = numericUpDownReelNumber.Value.ToString();
            ss.CurrentDCinemaEditRate = textBoxEditRate.Text;
            ss.CurrentDCinemaTimeCodeRate = comboBoxTimeCodeRate.Text;
            ss.CurrentDCinemaStartTime = timeUpDownStartTime.TimeCode.ToHHMMSSFF();
            if (comboBoxLanguage.SelectedItem != null)
            {
                ss.CurrentDCinemaLanguage = comboBoxLanguage.SelectedItem.ToString();
            }
            else
            {
                ss.CurrentDCinemaLanguage = string.Empty;
            }

            ss.CurrentDCinemaIssueDate = textBoxIssueDate.Text;
            ss.CurrentDCinemaFontId = textBoxFontID.Text;
            ss.CurrentDCinemaFontUri = textBoxFontUri.Text;
            ss.CurrentDCinemaFontColor = panelFontColor.BackColor;
            if (comboBoxFontEffect.SelectedIndex == 1)
            {
                ss.CurrentDCinemaFontEffect = "border";
            }
            else if (comboBoxFontEffect.SelectedIndex == 2)
            {
                ss.CurrentDCinemaFontEffect = "shadow";
            }
            else
            {
                ss.CurrentDCinemaFontEffect = string.Empty;
            }

            ss.CurrentDCinemaFontEffectColor = panelFontEffectColor.BackColor;
            ss.CurrentDCinemaFontSize = (int)numericUpDownFontSize.Value;
            ss.DCinemaBottomMargin = (int)numericUpDownTopBottomMargin.Value;

            ss.DCinemaFadeUpTime = (int)numericUpDownFadeUp.Value;
            ss.DCinemaFadeDownTime = (int)numericUpDownFadeDown.Value;

            DialogResult = DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBoxFontUri.Text = DCinemaSmpte2007.GenerateId();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog { FileName = string.Empty, Filter = "Export profile|*" + ProfileExtension })
            {
                if (openDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var importer = new DcPropertiesSmpte();
                if (importer.Load(openDialog.FileName) && importer.Load(openDialog.FileName))
                {
                    checkBoxGenerateIdAuto.Checked = Convert.ToBoolean(importer.GenerateIdAuto, CultureInfo.InvariantCulture);
                    if (decimal.TryParse(importer.ReelNumber, NumberStyles.Any, CultureInfo.InvariantCulture, out var reelNumber))
                    {
                        numericUpDownReelNumber.Value = (int)reelNumber;
                    }

                    comboBoxLanguage.Text = importer.Language;

                    textBoxEditRate.Text = importer.EditRate;
                    comboBoxTimeCodeRate.Text = importer.TimeCodeRate;
                    timeUpDownStartTime.TimeCode = new TimeCode(double.Parse(importer.StartTime));
                    textBoxFontID.Text = importer.FontId;
                    textBoxFontUri.Text = importer.FontUri;
                    panelFontColor.BackColor = Settings.FromHtml(importer.FontColor);
                    comboBoxFontEffect.Text = importer.Effect;
                    panelFontEffectColor.BackColor = Settings.FromHtml(importer.EffectColor);

                    if (decimal.TryParse(importer.FontSize, NumberStyles.Any, CultureInfo.InvariantCulture, out var fontSize))
                    {
                        numericUpDownFontSize.Value = fontSize;
                    }

                    if (decimal.TryParse(importer.TopBottomMargin, NumberStyles.Any, CultureInfo.InvariantCulture, out var margin))
                    {
                        numericUpDownTopBottomMargin.Value = margin;
                    }

                    if (decimal.TryParse(importer.FadeUpTime, NumberStyles.Any, CultureInfo.InvariantCulture, out var fadeUp))
                    {
                        numericUpDownFadeUp.Value = fadeUp;
                    }

                    if (decimal.TryParse(importer.FadeDownTime, NumberStyles.Any, CultureInfo.InvariantCulture, out var fadeDown))
                    {
                        numericUpDownFadeDown.Value = fadeDown;
                    }
                }
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog { FileName = string.Empty, Filter = "Export profile|*" + ProfileExtension })
            {
                if (saveDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var exporter = new DcPropertiesSmpte
                {
                    GenerateIdAuto = checkBoxGenerateIdAuto.Checked.ToString(CultureInfo.InvariantCulture),
                    ReelNumber = numericUpDownReelNumber.Value.ToString(CultureInfo.InvariantCulture),
                    Language = comboBoxLanguage.Text,
                    EditRate = textBoxEditRate.Text,
                    TimeCodeRate = comboBoxTimeCodeRate.Text,
                    StartTime = timeUpDownStartTime.TimeCode.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                    FontId = textBoxFontID.Text,
                    FontUri = textBoxFontUri.Text,
                    FontColor = Settings.ToHtml(panelFontColor.BackColor),
                    Effect = comboBoxFontEffect.Text,
                    EffectColor = Settings.ToHtml(panelFontEffectColor.BackColor),
                    FontSize = numericUpDownFontSize.Value.ToString(CultureInfo.InvariantCulture),
                    TopBottomMargin = numericUpDownTopBottomMargin.Value.ToString(CultureInfo.InvariantCulture),
                    FadeUpTime = numericUpDownFadeUp.Value.ToString(CultureInfo.InvariantCulture),
                    FadeDownTime = numericUpDownFadeDown.Value.ToString(CultureInfo.InvariantCulture),
                };

                exporter.Save(saveDialog.FileName);
            }
        }
    }
}

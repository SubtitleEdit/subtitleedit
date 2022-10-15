using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.DCinema
{
    public sealed partial class DCinemaPropertiesInterop : PositionAndSizeForm
    {
        private const string ProfileExtension = ".DCinema-interop-profile";

        public DCinemaPropertiesInterop()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var l = LanguageSettings.Current.DCinemaProperties;
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
            buttonGenerateID.Text = l.GenerateId;
            labelEffect.Text = l.FontEffect;
            labelEffectColor.Text = l.FontEffectColor;
            buttonFontEffectColor.Text = l.ChooseColor;
            labelFontSize.Text = l.FontSize;
            labelTopBottomMargin.Text = l.TopBottomMargin;
            labelZPosition.Text = l.ZPosition;
            labelZPositionHelp.Text = l.ZPositionHelp;
            labelFadeUpTime.Text = l.FadeUpTime;
            labelFadeDownTime.Text = l.FadeDownTime;

            profilesToolStripMenuItem.Text = LanguageSettings.Current.Settings.Profiles;
            exportToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Export;
            importToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Import;
            ContextMenuStrip = contextMenuStripProfile;

            foreach (var x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                comboBoxLanguage.Items.Add(x.EnglishName);
            }
            comboBoxLanguage.Sorted = true;

            var ss = Configuration.Settings.SubtitleSettings;
            checkBoxGenerateIdAuto.Checked = ss.DCinemaAutoGenerateSubtitleId;

            if (!string.IsNullOrEmpty(ss.CurrentDCinemaSubtitleId))
            {
                textBoxSubtitleID.Text = ss.CurrentDCinemaSubtitleId;
                textBoxMovieTitle.Text = ss.CurrentDCinemaMovieTitle;
                if (int.TryParse(ss.CurrentDCinemaReelNumber, out var number) && numericUpDownReelNumber.Minimum <= number && numericUpDownReelNumber.Maximum >= number)
                {
                    numericUpDownReelNumber.Value = number;
                }
                comboBoxLanguage.Text = ss.CurrentDCinemaLanguage;
                textBoxFontID.Text = ss.CurrentDCinemaFontId;
                textBoxFontUri.Text = ss.CurrentDCinemaFontUri;
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

                decimal zPosition = (decimal)ss.DCinemaZPosition;
                if (numericUpDownZPosition.Minimum <= zPosition &&
                   numericUpDownZPosition.Maximum >= zPosition)
                {
                    numericUpDownZPosition.Value = zPosition;
                }
                else
                {
                    numericUpDownZPosition.Value = 0;
                }
            }
            UiUtil.FixLargeFonts(this, buttonCancel);
        }

        private void buttonGenerateID_Click(object sender, EventArgs e)
        {
            Core.SubtitleFormats.DCinemaInterop.GenerateId();
            var hex = Guid.NewGuid().ToString().RemoveChar('-');
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
            ss.DCinemaAutoGenerateSubtitleId = checkBoxGenerateIdAuto.Checked;
            ss.CurrentDCinemaSubtitleId = textBoxSubtitleID.Text;
            ss.CurrentDCinemaMovieTitle = textBoxMovieTitle.Text;
            ss.CurrentDCinemaReelNumber = Convert.ToInt32(numericUpDownReelNumber.Value).ToString(CultureInfo.InvariantCulture);
            ss.CurrentDCinemaLanguage = comboBoxLanguage.Text;
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
            ss.DCinemaZPosition = (double)numericUpDownZPosition.Value;

            DialogResult = DialogResult.OK;
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

                var importer = new DcPropertiesInterop();
                if (importer.Load(openDialog.FileName) && importer.Load(openDialog.FileName))
                {
                    checkBoxGenerateIdAuto.Checked = Convert.ToBoolean(importer.GenerateIdAuto, CultureInfo.InvariantCulture);
                    if (decimal.TryParse(importer.ReelNumber, NumberStyles.Any, CultureInfo.InvariantCulture, out var reelNumber))
                    {
                        numericUpDownReelNumber.Value = (int)reelNumber;
                    }

                    comboBoxLanguage.Text = importer.Language;
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

                    if (decimal.TryParse(importer.ZPosition, NumberStyles.Any, CultureInfo.InvariantCulture, out var zPosition))
                    {
                        numericUpDownZPosition.Value = zPosition;
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

                var exporter = new DcPropertiesInterop
                {
                    GenerateIdAuto = checkBoxGenerateIdAuto.Checked.ToString(CultureInfo.InvariantCulture),
                    ReelNumber = numericUpDownReelNumber.Value.ToString(CultureInfo.InvariantCulture),
                    Language = comboBoxLanguage.Text,
                    FontId = textBoxFontID.Text,
                    FontUri = textBoxFontUri.Text,
                    FontColor = Settings.ToHtml(panelFontColor.BackColor),
                    Effect = comboBoxFontEffect.Text,
                    EffectColor = Settings.ToHtml(panelFontEffectColor.BackColor),
                    FontSize = numericUpDownFontSize.Value.ToString(CultureInfo.InvariantCulture),
                    TopBottomMargin = numericUpDownTopBottomMargin.Value.ToString(CultureInfo.InvariantCulture),
                    FadeUpTime = numericUpDownFadeUp.Value.ToString(CultureInfo.InvariantCulture),
                    FadeDownTime = numericUpDownFadeDown.Value.ToString(CultureInfo.InvariantCulture),
                    ZPosition = numericUpDownZPosition.Value.ToString(CultureInfo.InvariantCulture),
                };

                exporter.Save(saveDialog.FileName);
            }
        }
    }
}

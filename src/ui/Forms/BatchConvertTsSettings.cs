using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class BatchConvertTsSettings : Form
    {
        public BatchConvertTsSettings()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.BatchConvert.TransportStreamSettings;
            checkBoxOverrideOriginalXPosition.Text = LanguageSettings.Current.BatchConvert.TransportStreamOverrideXPosition;
            checkBoxOverrideOriginalYPosition.Text = LanguageSettings.Current.BatchConvert.TransportStreamOverrideYPosition;
            checkBoxOverrideVideoSize.Text = LanguageSettings.Current.BatchConvert.TransportStreamOverrideVideoSize;
            labelBottomMargin.Text = LanguageSettings.Current.ExportPngXml.BottomMargin;
            labelXMargin.Text = LanguageSettings.Current.ExportPngXml.LeftRightMargin;
            labelXAlignment.Text = LanguageSettings.Current.ExportPngXml.Align;
            labelWidth.Text = LanguageSettings.Current.General.Width;
            labelHeight.Text = LanguageSettings.Current.General.Height;
            labelFileNameEnding.Text = LanguageSettings.Current.BatchConvert.TransportStreamFileNameEnding;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);

            comboBoxHAlign.Items.Clear();
            comboBoxHAlign.Items.Add(LanguageSettings.Current.ExportPngXml.Left);
            comboBoxHAlign.Items.Add(LanguageSettings.Current.ExportPngXml.Center);
            comboBoxHAlign.Items.Add(LanguageSettings.Current.ExportPngXml.Right);

            numericUpDownXMargin.Left = labelXMargin.Left + labelXMargin.Width + 5;
            labelXMarginPercent.Left = numericUpDownXMargin.Left + numericUpDownXMargin.Width + 1;
            comboBoxHAlign.Left = labelXAlignment.Left + labelXAlignment.Width + 5;
            numericUpDownBottomMargin.Left = labelBottomMargin.Left + labelBottomMargin.Width + 5;
            labelBottomMarginPercent.Left = numericUpDownBottomMargin.Left + numericUpDownBottomMargin.Width + 1;
            var widthAndHeightLeft = Math.Max(labelWidth.Left + labelWidth.Width, labelHeight.Left + labelHeight.Width) + 5;
            numericUpDownWidth.Left = widthAndHeightLeft;
            numericUpDownHeight.Left = widthAndHeightLeft;

            checkBoxOverrideOriginalXPosition.Checked = Configuration.Settings.Tools.BatchConvertTsOverrideXPosition;
            checkBoxOverrideOriginalYPosition.Checked = Configuration.Settings.Tools.BatchConvertTsOverrideYPosition;
            checkBoxOnlyTeletext.Checked = Configuration.Settings.Tools.BatchConvertTsOnlyTeletext;
            numericUpDownXMargin.Value = Configuration.Settings.Tools.BatchConvertTsOverrideHMargin;
            numericUpDownBottomMargin.Value = Configuration.Settings.Tools.BatchConvertTsOverrideBottomMargin;
            checkBoxOverrideVideoSize.Checked = Configuration.Settings.Tools.BatchConvertTsOverrideScreenSize;
            numericUpDownWidth.Value = Configuration.Settings.Tools.BatchConvertTsScreenWidth;
            numericUpDownHeight.Value = Configuration.Settings.Tools.BatchConvertTsScreenHeight;
            textBoxFileNameAppend.Text = Configuration.Settings.Tools.BatchConvertTsFileNameAppend;
            if (Configuration.Settings.Tools.BatchConvertTsOverrideHAlign.Equals("left", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxHAlign.SelectedIndex = 0;
            }
            else if (Configuration.Settings.Tools.BatchConvertTsOverrideHAlign.Equals("right", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxHAlign.SelectedIndex = 2;
            }
            else
            {
                comboBoxHAlign.SelectedIndex = 1;
            }
            checkBoxOverrideOriginalXPosition_CheckedChanged(null, null);
            CheckBoxOverrideOriginalYPosition_CheckedChanged(null, null);
            CheckBoxOverrideVideoSize_CheckedChanged(null, null);
            TextBoxFileNameAppendTextChanged(null, null);
        }

        private void CheckBoxOverrideOriginalYPosition_CheckedChanged(object sender, EventArgs e)
        {
            labelBottomMargin.Enabled = checkBoxOverrideOriginalYPosition.Checked;
            numericUpDownBottomMargin.Enabled = checkBoxOverrideOriginalYPosition.Checked;
        }

        private void CheckBoxOverrideVideoSize_CheckedChanged(object sender, EventArgs e)
        {
            labelWidth.Enabled = checkBoxOverrideVideoSize.Checked;
            numericUpDownWidth.Enabled = checkBoxOverrideVideoSize.Checked;
            labelHeight.Enabled = checkBoxOverrideVideoSize.Checked;
            numericUpDownHeight.Enabled = checkBoxOverrideVideoSize.Checked;
            buttonChooseResolution.Enabled = checkBoxOverrideVideoSize.Checked;
        }

        private void BatchConvertTsSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.BatchConvertTsOverrideXPosition = checkBoxOverrideOriginalXPosition.Checked;
            Configuration.Settings.Tools.BatchConvertTsOverrideYPosition = checkBoxOverrideOriginalYPosition.Checked;
            Configuration.Settings.Tools.BatchConvertTsOnlyTeletext = checkBoxOnlyTeletext.Checked;
            Configuration.Settings.Tools.BatchConvertTsOverrideBottomMargin = (int)numericUpDownBottomMargin.Value;
            Configuration.Settings.Tools.BatchConvertTsOverrideScreenSize = checkBoxOverrideVideoSize.Checked;
            Configuration.Settings.Tools.BatchConvertTsScreenWidth = (int)numericUpDownWidth.Value;
            Configuration.Settings.Tools.BatchConvertTsScreenHeight = (int)numericUpDownHeight.Value;
            Configuration.Settings.Tools.BatchConvertTsOverrideHMargin = (int)numericUpDownXMargin.Value;
            Configuration.Settings.Tools.BatchConvertTsFileNameAppend = textBoxFileNameAppend.Text;
            if (comboBoxHAlign.SelectedIndex == 0)
            {
                Configuration.Settings.Tools.BatchConvertTsOverrideHAlign = "left";
            }
            else if (comboBoxHAlign.SelectedIndex == 2)
            {
                Configuration.Settings.Tools.BatchConvertTsOverrideHAlign = "right";
            }
            else
            {
                Configuration.Settings.Tools.BatchConvertTsOverrideHAlign = "center";
            }
            DialogResult = DialogResult.OK;
        }

        private void checkBoxOverrideOriginalXPosition_CheckedChanged(object sender, EventArgs e)
        {
            labelXMargin.Enabled = checkBoxOverrideOriginalXPosition.Checked;
            numericUpDownXMargin.Enabled = checkBoxOverrideOriginalXPosition.Checked;
            labelXAlignment.Enabled = checkBoxOverrideOriginalXPosition.Checked;
            comboBoxHAlign.Enabled = checkBoxOverrideOriginalXPosition.Checked;
        }

        private void ButtonChooseResolution_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.GetVideoFileFilter(false);
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var info = UiUtil.GetVideoInfo(openFileDialog1.FileName);
                if (info != null && info.Success)
                {
                    numericUpDownWidth.Value = info.Width;
                    numericUpDownHeight.Value = info.Height;
                }
            }
        }

        private void TwoLetterCountryCodeToolStripMenuItemClick(object sender, EventArgs e)
        {
            textBoxFileNameAppend.Text = textBoxFileNameAppend.Text.Insert(textBoxFileNameAppend.SelectionStart, "{two-letter-country-code}");
        }

        private void ThreeLetterCountryCodeToolStripMenuItemClick(object sender, EventArgs e)
        {
            textBoxFileNameAppend.Text = textBoxFileNameAppend.Text.Insert(textBoxFileNameAppend.SelectionStart, "{three-letter-country-code}");
        }

        private void TwoLetterCountryCodeUppercaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            textBoxFileNameAppend.Text = textBoxFileNameAppend.Text.Insert(textBoxFileNameAppend.SelectionStart, "{two-letter-country-code-uppercase}");
        }

        private void ThreeLetterCountryCodeUppercaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            textBoxFileNameAppend.Text = textBoxFileNameAppend.Text.Insert(textBoxFileNameAppend.SelectionStart, "{three-letter-country-code-uppercase}");
        }

        private void TextBoxFileNameAppendTextChanged(object sender, EventArgs e)
        {
            labelFileEndingSample.Text = ("MyVideoFile" + textBoxFileNameAppend.Text + ".sup")
                .Replace("{two-letter-country-code}", "en")
                .Replace("{two-letter-country-code-uppercase}", "EN")
                .Replace("{three-letter-country-code}", "eng")
                .Replace("{three-letter-country-code-uppercase}", "ENG");
        }
    }
}

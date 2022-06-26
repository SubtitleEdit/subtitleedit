using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class SettingsCustomContinuationStyle : Form
    {
        private bool _isUpdating = true;

        public SettingsCustomContinuationStyle()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.Settings;
            var settings = Configuration.Settings.General;
            Text = language.CustomContinuationStyle;

            toolStripMenuItemLoadStyle.Text = language.LoadStyle;
            labelSuffix.Text = language.Suffix;
            checkBoxSuffixAddForComma.Text = language.AddSuffixForComma;
            checkBoxSuffixAddSpace.Text = language.AddSpace;
            checkBoxSuffixRemoveComma.Text = language.RemoveComma;
            labelPrefix.Text = language.Prefix;
            checkBoxPrefixAddSpace.Text = language.AddSpace;
            checkBoxDifferentStyleGap.Text = language.DifferentStyleGap;
            labelMs.Text = language.Milliseconds;
            labelGapSuffix.Text = language.Suffix;
            checkBoxGapSuffixAddForComma.Text = language.AddSuffixForComma;
            checkBoxGapSuffixAddSpace.Text = language.AddSpace;
            checkBoxGapSuffixRemoveComma.Text = language.RemoveComma;
            labelGapPrefix.Text = language.Prefix;
            checkBoxGapPrefixAddSpace.Text = language.AddSpace;
            groupBoxPreview.Text = language.Preview;
            labelPreviewPause.Text = language.PreviewPause;
            labelNote.Text = language.CustomContinuationStyleNote;

            comboBoxSuffix.Left = labelSuffix.Left + labelSuffix.Width + 6;
            checkBoxSuffixAddForComma.Left = comboBoxSuffix.Left;
            checkBoxSuffixAddSpace.Left = comboBoxSuffix.Left;
            checkBoxSuffixRemoveComma.Left = comboBoxSuffix.Left;

            comboBoxPrefix.Left = labelPrefix.Left + labelPrefix.Width + 6;
            checkBoxPrefixAddSpace.Left = comboBoxPrefix.Left;

            numericUpDownDifferentStyleGapMs.Left = checkBoxDifferentStyleGap.Left + checkBoxDifferentStyleGap.Width + 6;
            labelMs.Left = numericUpDownDifferentStyleGapMs.Left + numericUpDownDifferentStyleGapMs.Width + 6;

            comboBoxGapSuffix.Left = labelGapSuffix.Left + labelGapSuffix.Width + 6;
            checkBoxGapSuffixAddForComma.Left = comboBoxGapSuffix.Left;
            checkBoxGapSuffixAddSpace.Left = comboBoxGapSuffix.Left;
            checkBoxGapSuffixRemoveComma.Left = comboBoxGapSuffix.Left;

            comboBoxGapPrefix.Left = labelGapPrefix.Left + labelGapPrefix.Width + 6;
            checkBoxGapPrefixAddSpace.Left = comboBoxGapPrefix.Left;

            // Populate styles menu
            toolStripMenuItemLoadStyle.DropDownItems.Clear();
            foreach (var continuationStyle in ContinuationUtilities.ContinuationStyles)
            {
                if (continuationStyle != ContinuationStyle.Custom)
                {
                    toolStripMenuItemLoadStyle.DropDownItems.Add(new ToolStripMenuItem(UiUtil.GetContinuationStyleName(continuationStyle), null, (sender, args) =>
                    {
                        ResetSettings(continuationStyle);
                    }));
                }
            }

            // Load config
            LoadSettings(ContinuationUtilities.GetContinuationProfile(ContinuationStyle.Custom));

            numericUpDownDifferentStyleGapMs.Value = settings.ContinuationPause;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void LoadSettings(ContinuationUtilities.ContinuationProfile profile)
        {
            // Update controls
            _isUpdating = true;
            comboBoxSuffix.Text = profile.Suffix;
            checkBoxSuffixAddForComma.Checked = profile.SuffixApplyIfComma;
            checkBoxSuffixAddSpace.Checked = profile.SuffixAddSpace;
            checkBoxSuffixRemoveComma.Checked = profile.SuffixReplaceComma;
            comboBoxPrefix.Text = profile.Prefix;
            checkBoxPrefixAddSpace.Checked = profile.PrefixAddSpace;
            checkBoxDifferentStyleGap.Checked = profile.UseDifferentStyleGap;
            comboBoxGapSuffix.Text = profile.GapSuffix;
            checkBoxGapSuffixAddForComma.Checked = profile.GapSuffixApplyIfComma;
            checkBoxGapSuffixAddSpace.Checked = profile.GapSuffixAddSpace;
            checkBoxGapSuffixRemoveComma.Checked = profile.GapSuffixReplaceComma;
            comboBoxGapPrefix.Text = profile.GapPrefix;
            checkBoxGapPrefixAddSpace.Checked = profile.GapPrefixAddSpace;
            _isUpdating = false;

            // Update preview
            RefreshControls(null, EventArgs.Empty);
        }
        
        private void RefreshControls(object sender, EventArgs e)
        {
            if (_isUpdating)
            {
                return;
            }

            // Update preview
            var profile = CreateContinuationProfile();
            var preview = ContinuationUtilities.GetContinuationStylePreview(profile);
            var previewSplit = preview.Split(new[] { "\n\n" }, StringSplitOptions.None);

            labelPreviewLine1.Text = previewSplit[0];
            labelPreviewLine2.Text = previewSplit[1];
            labelPreviewLine3.Text = previewSplit[2];
            labelPreviewLine4.Text = previewSplit[4];

            // Toggle gap controls
            labelGapSuffix.Enabled = checkBoxDifferentStyleGap.Checked;
            comboBoxGapSuffix.Enabled = checkBoxDifferentStyleGap.Checked;
            checkBoxGapSuffixAddForComma.Enabled = checkBoxDifferentStyleGap.Checked;
            checkBoxGapSuffixAddSpace.Enabled = checkBoxDifferentStyleGap.Checked;
            checkBoxGapSuffixRemoveComma.Enabled = checkBoxDifferentStyleGap.Checked;
            labelGapPrefix.Enabled = checkBoxDifferentStyleGap.Checked;
            comboBoxGapPrefix.Enabled = checkBoxDifferentStyleGap.Checked;
            checkBoxGapPrefixAddSpace.Enabled = checkBoxDifferentStyleGap.Checked;
        }

        private void checkBoxDifferentStyleGap_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdating)
            {
                return;
            }

            if (checkBoxDifferentStyleGap.Checked)
            {
                // Duplicate settings if empty
                if (comboBoxGapSuffix.Text == string.Empty && checkBoxGapSuffixAddForComma.Checked == false && checkBoxGapSuffixAddSpace.Checked == false
                    && checkBoxGapSuffixRemoveComma.Checked == false && comboBoxGapPrefix.Text == string.Empty && checkBoxGapPrefixAddSpace.Checked == false)
                {
                    _isUpdating = true;
                    comboBoxGapSuffix.Text = comboBoxSuffix.Text;
                    checkBoxGapSuffixAddForComma.Checked = checkBoxSuffixAddForComma.Checked;
                    checkBoxGapSuffixAddSpace.Checked = checkBoxSuffixAddSpace.Checked;
                    checkBoxGapSuffixRemoveComma.Checked = checkBoxSuffixRemoveComma.Checked;
                    comboBoxGapPrefix.Text = comboBoxPrefix.Text;
                    checkBoxGapPrefixAddSpace.Checked = checkBoxPrefixAddSpace.Checked;
                    _isUpdating = false;
                }
            }
            
            RefreshControls(sender, e);
        }

        private ContinuationUtilities.ContinuationProfile CreateContinuationProfile()
        {
            return new ContinuationUtilities.ContinuationProfile
            {
                Suffix = comboBoxSuffix.Text,
                SuffixApplyIfComma = checkBoxSuffixAddForComma.Checked,
                SuffixAddSpace = checkBoxSuffixAddSpace.Checked,
                SuffixReplaceComma = checkBoxSuffixRemoveComma.Checked,
                Prefix = comboBoxPrefix.Text,
                PrefixAddSpace = checkBoxPrefixAddSpace.Checked,
                UseDifferentStyleGap = checkBoxDifferentStyleGap.Checked,
                GapSuffix = comboBoxGapSuffix.Text,
                GapSuffixApplyIfComma = checkBoxGapSuffixAddForComma.Checked,
                GapSuffixAddSpace = checkBoxGapSuffixAddSpace.Checked,
                GapSuffixReplaceComma = checkBoxGapSuffixRemoveComma.Checked,
                GapPrefix = comboBoxGapPrefix.Text,
                GapPrefixAddSpace = checkBoxGapPrefixAddSpace.Checked
            };
        }

        private void ResetSettings(ContinuationStyle continuationStyle)
        {
            if (MessageBox.Show(this, LanguageSettings.Current.Settings.ResetCustomContinuationStyleWarning, LanguageSettings.Current.General.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                LoadSettings(ContinuationUtilities.GetContinuationProfile(continuationStyle));
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var profile = CreateContinuationProfile();

            // Save config
            Configuration.Settings.General.CustomContinuationStyleSuffix = profile.Suffix;
            Configuration.Settings.General.CustomContinuationStyleSuffixApplyIfComma = profile.SuffixApplyIfComma;
            Configuration.Settings.General.CustomContinuationStyleSuffixAddSpace = profile.SuffixAddSpace;
            Configuration.Settings.General.CustomContinuationStyleSuffixReplaceComma = profile.SuffixReplaceComma;
            Configuration.Settings.General.CustomContinuationStylePrefix = profile.Prefix;
            Configuration.Settings.General.CustomContinuationStylePrefixAddSpace = profile.PrefixAddSpace;
            Configuration.Settings.General.CustomContinuationStyleUseDifferentStyleGap = profile.UseDifferentStyleGap;
            Configuration.Settings.General.CustomContinuationStyleGapSuffix = profile.GapSuffix;
            Configuration.Settings.General.CustomContinuationStyleGapSuffixApplyIfComma = profile.GapSuffixApplyIfComma;
            Configuration.Settings.General.CustomContinuationStyleGapSuffixAddSpace = profile.GapSuffixAddSpace;
            Configuration.Settings.General.CustomContinuationStyleGapSuffixReplaceComma = profile.GapSuffixReplaceComma;
            Configuration.Settings.General.CustomContinuationStyleGapPrefix = profile.GapPrefix;
            Configuration.Settings.General.CustomContinuationStyleGapPrefixAddSpace = profile.GapPrefixAddSpace;

            Configuration.Settings.General.ContinuationPause = Convert.ToInt32(numericUpDownDifferentStyleGapMs.Value);

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}

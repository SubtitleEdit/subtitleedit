using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class BeautifyTimeCodesProfile : Form
    {
        public BeautifyTimeCodesProfile(double frameRate)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            if (frameRate > 0)
            {
                cuesPreviewViewInCues.FrameRate = (float)frameRate;
                cuesPreviewViewOutCues.FrameRate = (float)frameRate;
                cuesPreviewViewConnectedSubtitlesInCueClosest.FrameRate = (float)frameRate;
                cuesPreviewViewConnectedSubtitlesOutCueClosest.FrameRate = (float)frameRate;
                cuesPreviewViewChainingGeneral.FrameRate = (float)frameRate;
                cuesPreviewViewChainingInCueOnShot.FrameRate = (float)frameRate;
                cuesPreviewViewChainingOutCueOnShot.FrameRate = (float)frameRate;
            }

            var language = LanguageSettings.Current.BeautifyTimeCodesProfile;
            Text = language.Title;
            // TODO i18n

            LoadSettings();

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void LoadSettings()
        {
            var settings = Configuration.Settings.BeautifyTimeCodes.Profile;

            numericUpDownGap.Value = settings.Gap;

            numericUpDownInCuesGap.Value = settings.InCuesGap;
            numericUpDownInCuesLeftRedZone.Value = settings.InCuesLeftRedZone;
            numericUpDownInCuesRightRedZone.Value = settings.InCuesRightRedZone;
            numericUpDownInCuesLeftGreenZone.Value = settings.InCuesLeftGreenZone;
            numericUpDownInCuesRightGreenZone.Value = settings.InCuesRightGreenZone;

            numericUpDownOutCuesGap.Value = settings.OutCuesGap;
            numericUpDownOutCuesLeftRedZone.Value = settings.OutCuesLeftRedZone;
            numericUpDownOutCuesRightRedZone.Value = settings.OutCuesRightRedZone;
            numericUpDownOutCuesLeftGreenZone.Value = settings.OutCuesLeftGreenZone;
            numericUpDownOutCuesRightGreenZone.Value = settings.OutCuesRightGreenZone;

            numericUpDownConnectedSubtitlesInCueClosestLeftGap.Value = settings.ConnectedSubtitlesInCueClosestLeftGap;
            numericUpDownConnectedSubtitlesInCueClosestRightGap.Value = settings.ConnectedSubtitlesInCueClosestRightGap;
            numericUpDownConnectedSubtitlesOutCueClosestLeftGap.Value = settings.ConnectedSubtitlesOutCueClosestLeftGap;
            numericUpDownConnectedSubtitlesOutCueClosestRightGap.Value = settings.ConnectedSubtitlesOutCueClosestRightGap;
            numericUpDownConnectedSubtitlesLeftRedZone.Value = settings.ConnectedSubtitlesLeftRedZone;
            numericUpDownConnectedSubtitlesRightRedZone.Value = settings.ConnectedSubtitlesRightRedZone;
            numericUpDownConnectedSubtitlesLeftGreenZone.Value = settings.ConnectedSubtitlesLeftGreenZone;
            numericUpDownConnectedSubtitlesRightGreenZone.Value = settings.ConnectedSubtitlesRightGreenZone;
            numericUpDownConnectedSubtitlesTreatConnected.Value = settings.ConnectedSubtitlesTreatConnected;

            radioButtonChainingGeneralZones.Checked = settings.ChainingGeneralUseZones;
            radioButtonChainingGeneralMaxGap.Checked = !settings.ChainingGeneralUseZones;
            numericUpDownChainingGeneralMaxGap.Value = settings.ChainingGeneralMaxGap;
            numericUpDownChainingGeneralLeftRedZone.Value = settings.ChainingGeneralLeftRedZone;
            numericUpDownChainingGeneralLeftGreenZone.Value = settings.ChainingGeneralLeftGreenZone;
            comboBoxChainingGeneralShotChangeBehavior.SelectedIndex = (int)settings.ChainingGeneralShotChangeBehavior;

            radioButtonChainingInCueOnShotZones.Checked = settings.ChainingInCueOnShotUseZones;
            radioButtonChainingInCueOnShotMaxGap.Checked = !settings.ChainingInCueOnShotUseZones;
            numericUpDownChainingInCueOnShotMaxGap.Value = settings.ChainingInCueOnShotMaxGap;
            numericUpDownChainingInCueOnShotLeftRedZone.Value = settings.ChainingInCueOnShotLeftRedZone;
            numericUpDownChainingInCueOnShotLeftGreenZone.Value = settings.ChainingInCueOnShotLeftGreenZone;

            radioButtonChainingOutCueOnShotZones.Checked = settings.ChainingOutCueOnShotUseZones;
            radioButtonChainingOutCueOnShotMaxGap.Checked = !settings.ChainingOutCueOnShotUseZones;
            numericUpDownChainingOutCueOnShotMaxGap.Value = settings.ChainingOutCueOnShotMaxGap;
            numericUpDownChainingOutCueOnShotRightRedZone.Value = settings.ChainingOutCueOnShotRightRedZone;
            numericUpDownChainingOutCueOnShotRightGreenZone.Value = settings.ChainingOutCueOnShotRightGreenZone;

            RefreshControls();
        }

        private void RefreshControls()
        {
            // Radio button toggling
            numericUpDownChainingGeneralMaxGap.Enabled = radioButtonChainingGeneralMaxGap.Checked;
            labelChainingGeneralMaxGapSuffix.Enabled = radioButtonChainingGeneralMaxGap.Checked;
            numericUpDownChainingGeneralLeftGreenZone.Enabled = radioButtonChainingGeneralZones.Checked;
            numericUpDownChainingGeneralLeftRedZone.Enabled = radioButtonChainingGeneralZones.Checked;

            numericUpDownChainingInCueOnShotMaxGap.Enabled = radioButtonChainingInCueOnShotMaxGap.Checked;
            labelChainingInCueOnShotMaxGapSuffix.Enabled = radioButtonChainingInCueOnShotMaxGap.Checked;
            numericUpDownChainingInCueOnShotLeftGreenZone.Enabled = radioButtonChainingInCueOnShotZones.Checked;
            numericUpDownChainingInCueOnShotLeftRedZone.Enabled = radioButtonChainingInCueOnShotZones.Checked;

            numericUpDownChainingOutCueOnShotMaxGap.Enabled = radioButtonChainingOutCueOnShotMaxGap.Checked;
            labelChainingOutCueOnShotMaxGapSuffix.Enabled = radioButtonChainingOutCueOnShotMaxGap.Checked;
            numericUpDownChainingOutCueOnShotRightRedZone.Enabled = radioButtonChainingOutCueOnShotZones.Checked;
            numericUpDownChainingOutCueOnShotRightGreenZone.Enabled = radioButtonChainingOutCueOnShotZones.Checked;
            
            // Connected subtitles page toggling
            cuesPreviewViewConnectedSubtitlesInCueClosest.Visible = tabControlConnectedSubtitles.SelectedIndex == 0;
            cuesPreviewViewConnectedSubtitlesOutCueClosest.Visible = tabControlConnectedSubtitles.SelectedIndex == 1;
            
            // Chaining page toggling
            cuesPreviewViewChainingGeneral.Visible = tabControlChaining.SelectedIndex == 0;
            cuesPreviewViewChainingInCueOnShot.Visible = tabControlChaining.SelectedIndex == 1;
            cuesPreviewViewChainingOutCueOnShot.Visible = tabControlChaining.SelectedIndex == 2;

            // Update cue previews
            cuesPreviewViewInCues.RightGap = Convert.ToInt32(numericUpDownInCuesGap.Value);
            cuesPreviewViewInCues.LeftGreenZone = Convert.ToInt32(numericUpDownInCuesLeftGreenZone.Value);
            cuesPreviewViewInCues.LeftRedZone = Convert.ToInt32(numericUpDownInCuesLeftRedZone.Value);
            cuesPreviewViewInCues.RightRedZone = Convert.ToInt32(numericUpDownInCuesRightRedZone.Value);
            cuesPreviewViewInCues.RightGreenZone = Convert.ToInt32(numericUpDownInCuesRightGreenZone.Value);

            cuesPreviewViewOutCues.LeftGap = Convert.ToInt32(numericUpDownOutCuesGap.Value);
            cuesPreviewViewOutCues.LeftGreenZone = Convert.ToInt32(numericUpDownOutCuesLeftGreenZone.Value);
            cuesPreviewViewOutCues.LeftRedZone = Convert.ToInt32(numericUpDownOutCuesLeftRedZone.Value);
            cuesPreviewViewOutCues.RightRedZone = Convert.ToInt32(numericUpDownOutCuesRightRedZone.Value);
            cuesPreviewViewOutCues.RightGreenZone = Convert.ToInt32(numericUpDownOutCuesRightGreenZone.Value);

            cuesPreviewViewConnectedSubtitlesInCueClosest.LeftGap = Convert.ToInt32(numericUpDownConnectedSubtitlesInCueClosestLeftGap.Value);
            cuesPreviewViewConnectedSubtitlesInCueClosest.RightGap = Convert.ToInt32(numericUpDownConnectedSubtitlesInCueClosestRightGap.Value);
            cuesPreviewViewConnectedSubtitlesInCueClosest.LeftGreenZone = Convert.ToInt32(numericUpDownConnectedSubtitlesLeftGreenZone.Value);
            cuesPreviewViewConnectedSubtitlesInCueClosest.LeftRedZone = Convert.ToInt32(numericUpDownConnectedSubtitlesLeftRedZone.Value);
            cuesPreviewViewConnectedSubtitlesInCueClosest.RightRedZone = Convert.ToInt32(numericUpDownConnectedSubtitlesRightRedZone.Value);
            cuesPreviewViewConnectedSubtitlesInCueClosest.RightGreenZone = Convert.ToInt32(numericUpDownConnectedSubtitlesRightGreenZone.Value);

            cuesPreviewViewConnectedSubtitlesOutCueClosest.LeftGap = Convert.ToInt32(numericUpDownConnectedSubtitlesOutCueClosestLeftGap.Value);
            cuesPreviewViewConnectedSubtitlesOutCueClosest.RightGap = Convert.ToInt32(numericUpDownConnectedSubtitlesOutCueClosestRightGap.Value);
            cuesPreviewViewConnectedSubtitlesOutCueClosest.LeftGreenZone = Convert.ToInt32(numericUpDownConnectedSubtitlesLeftGreenZone.Value);
            cuesPreviewViewConnectedSubtitlesOutCueClosest.LeftRedZone = Convert.ToInt32(numericUpDownConnectedSubtitlesLeftRedZone.Value);
            cuesPreviewViewConnectedSubtitlesOutCueClosest.RightRedZone = Convert.ToInt32(numericUpDownConnectedSubtitlesRightRedZone.Value);
            cuesPreviewViewConnectedSubtitlesOutCueClosest.RightGreenZone = Convert.ToInt32(numericUpDownConnectedSubtitlesRightGreenZone.Value);

            cuesPreviewViewChainingGeneral.LeftGreenZone = Convert.ToInt32(numericUpDownChainingGeneralLeftGreenZone.Value);
            cuesPreviewViewChainingGeneral.LeftRedZone = Convert.ToInt32(numericUpDownChainingGeneralLeftRedZone.Value);

            cuesPreviewViewChainingInCueOnShot.RightGap = Convert.ToInt32(numericUpDownInCuesGap.Value);
            cuesPreviewViewChainingInCueOnShot.LeftGreenZone = Convert.ToInt32(numericUpDownChainingInCueOnShotLeftGreenZone.Value);
            cuesPreviewViewChainingInCueOnShot.LeftRedZone = Convert.ToInt32(numericUpDownChainingInCueOnShotLeftRedZone.Value);

            cuesPreviewViewChainingOutCueOnShot.LeftGap = Convert.ToInt32(numericUpDownOutCuesGap.Value);
            cuesPreviewViewChainingOutCueOnShot.RightRedZone = Convert.ToInt32(numericUpDownChainingOutCueOnShotRightRedZone.Value);
            cuesPreviewViewChainingOutCueOnShot.RightGreenZone = Convert.ToInt32(numericUpDownChainingOutCueOnShotRightGreenZone.Value);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Save settings
            Configuration.Settings.BeautifyTimeCodes.Profile.Gap = Convert.ToInt32(numericUpDownGap.Value);

            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap = Convert.ToInt32(numericUpDownInCuesGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesLeftGreenZone = Convert.ToInt32(numericUpDownInCuesLeftGreenZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesLeftRedZone = Convert.ToInt32(numericUpDownInCuesLeftRedZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesRightRedZone = Convert.ToInt32(numericUpDownInCuesRightRedZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesRightGreenZone = Convert.ToInt32(numericUpDownInCuesRightGreenZone.Value);

            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap = Convert.ToInt32(numericUpDownOutCuesGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesLeftGreenZone = Convert.ToInt32(numericUpDownOutCuesLeftGreenZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesLeftRedZone = Convert.ToInt32(numericUpDownOutCuesLeftRedZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesRightRedZone = Convert.ToInt32(numericUpDownOutCuesRightRedZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesRightGreenZone = Convert.ToInt32(numericUpDownOutCuesRightGreenZone.Value);

            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestLeftGap = Convert.ToInt32(numericUpDownConnectedSubtitlesInCueClosestLeftGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestRightGap = Convert.ToInt32(numericUpDownConnectedSubtitlesInCueClosestRightGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestLeftGap = Convert.ToInt32(numericUpDownConnectedSubtitlesOutCueClosestLeftGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestRightGap = Convert.ToInt32(numericUpDownConnectedSubtitlesOutCueClosestRightGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesLeftGreenZone = Convert.ToInt32(numericUpDownConnectedSubtitlesLeftGreenZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesLeftRedZone = Convert.ToInt32(numericUpDownConnectedSubtitlesLeftRedZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesRightRedZone = Convert.ToInt32(numericUpDownConnectedSubtitlesRightRedZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesRightGreenZone = Convert.ToInt32(numericUpDownConnectedSubtitlesRightGreenZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesTreatConnected = Convert.ToInt32(numericUpDownConnectedSubtitlesTreatConnected.Value);

            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralUseZones = radioButtonChainingGeneralZones.Checked;
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralMaxGap = Convert.ToInt32(numericUpDownChainingGeneralMaxGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftGreenZone = Convert.ToInt32(numericUpDownChainingGeneralLeftGreenZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftRedZone = Convert.ToInt32(numericUpDownChainingGeneralLeftRedZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralShotChangeBehavior = (BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingGeneralShotChangeBehaviorEnum)comboBoxChainingGeneralShotChangeBehavior.SelectedIndex;

            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotUseZones = radioButtonChainingInCueOnShotZones.Checked;
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotMaxGap = Convert.ToInt32(numericUpDownChainingInCueOnShotMaxGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotLeftGreenZone = Convert.ToInt32(numericUpDownChainingInCueOnShotLeftGreenZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotLeftRedZone = Convert.ToInt32(numericUpDownChainingInCueOnShotLeftRedZone.Value);

            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotUseZones = radioButtonChainingOutCueOnShotZones.Checked;
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotMaxGap = Convert.ToInt32(numericUpDownChainingOutCueOnShotMaxGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotRightRedZone = Convert.ToInt32(numericUpDownChainingOutCueOnShotRightRedZone.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotRightGreenZone = Convert.ToInt32(numericUpDownChainingOutCueOnShotRightGreenZone.Value);

            DialogResult = DialogResult.OK;
        }

        private void toolStripMenuItemLoadDefault_Click(object sender, EventArgs e)
        {
            ResetSettings(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset.Default);
        }

        private void toolStripMenuItemLoadNetflix_Click(object sender, EventArgs e)
        {
            ResetSettings(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset.Netflix);
        }

        private void toolStripMenuItemLoadSDI_Click(object sender, EventArgs e)
        {
            ResetSettings(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset.SDI);
        }

        private void ResetSettings(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset preset)
        {
            if (MessageBox.Show(this, "This will reset your current profile and replace all values with those of the selected preset. This cannot be undone.\n\nDo you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Configuration.Settings.BeautifyTimeCodes.Profile = new BeautifyTimeCodesSettings.BeautifyTimeCodesProfile(preset);
                LoadSettings();
            }
        }

        private void radioButtonChaining_CheckedChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void tabControlChaining_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            ValidateZones();
            RefreshControls();
        }

        private void tabControlConnectedSubtitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void numericUpDownGap_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownOutCuesGap.Value > 0)
            {
                numericUpDownOutCuesGap.Value = numericUpDownGap.Value;
            }

            if (numericUpDownConnectedSubtitlesInCueClosestLeftGap.Value > 0 && numericUpDownConnectedSubtitlesInCueClosestRightGap.Value == 0)
            {
                numericUpDownConnectedSubtitlesInCueClosestLeftGap.Value = numericUpDownGap.Value;
            }

            if (numericUpDownConnectedSubtitlesInCueClosestLeftGap.Value == 0 && numericUpDownConnectedSubtitlesInCueClosestRightGap.Value > 0)
            {
                numericUpDownConnectedSubtitlesInCueClosestRightGap.Value = numericUpDownGap.Value;
            }

            if (numericUpDownConnectedSubtitlesOutCueClosestRightGap.Value > 0 && numericUpDownConnectedSubtitlesOutCueClosestLeftGap.Value == 0)
            {
                numericUpDownConnectedSubtitlesOutCueClosestRightGap.Value = numericUpDownGap.Value;
            }

            if (numericUpDownConnectedSubtitlesOutCueClosestRightGap.Value == 0 && numericUpDownConnectedSubtitlesOutCueClosestLeftGap.Value > 0)
            {
                numericUpDownConnectedSubtitlesOutCueClosestLeftGap.Value = numericUpDownGap.Value;
            }
        }

        private void ValidateZones()
        {
            numericUpDownInCuesLeftGreenZone.Minimum = numericUpDownInCuesLeftRedZone.Value;
            numericUpDownInCuesRightGreenZone.Minimum = numericUpDownInCuesRightRedZone.Value;
            numericUpDownOutCuesLeftGreenZone.Minimum = numericUpDownOutCuesLeftRedZone.Value;
            numericUpDownOutCuesRightGreenZone.Minimum = numericUpDownOutCuesRightRedZone.Value;

            numericUpDownConnectedSubtitlesLeftGreenZone.Minimum = numericUpDownConnectedSubtitlesLeftRedZone.Value;
            numericUpDownConnectedSubtitlesRightGreenZone.Minimum = numericUpDownConnectedSubtitlesRightRedZone.Value;

            numericUpDownChainingGeneralLeftGreenZone.Minimum = numericUpDownChainingGeneralLeftRedZone.Value;
            numericUpDownChainingInCueOnShotLeftGreenZone.Minimum = numericUpDownChainingInCueOnShotLeftRedZone.Value;
            numericUpDownChainingOutCueOnShotRightGreenZone.Minimum = numericUpDownChainingOutCueOnShotRightRedZone.Value;
        }
    }
}
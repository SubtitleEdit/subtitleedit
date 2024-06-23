using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Settings;

namespace Nikse.SubtitleEdit.Forms.BeautifyTimeCodes
{
    public sealed partial class BeautifyTimeCodesProfileSimple : Form
    {
        private readonly double _frameRate;

        public BeautifyTimeCodesProfileSimple(double frameRate)
        {
            _frameRate = frameRate;

            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.BeautifyTimeCodesProfile;
            Text = language.CreateSimpleTitle;
            labelInstructions.Text = language.CreateSimpleInstruction;
            labelGap.Text = language.Gap;
            labelGapSuffix.Text = language.Frames;
            labelGapInstruction.Text = language.CreateSimpleGapInstruction;
            labelInCues.Text = language.CreateSimpleInCues;
            comboBoxInCues.Items.Clear();
            comboBoxInCues.Items.Add(language.CreateSimpleInCues0Frames);
            comboBoxInCues.Items.Add(language.CreateSimpleInCues1Frames);
            comboBoxInCues.Items.Add(language.CreateSimpleInCues2Frames);
            comboBoxInCues.Items.Add(language.CreateSimpleInCues3Frames);
            comboBoxInCues.SelectedIndex = 0;
            labelOutCues.Text = language.CreateSimpleOutCues;
            comboBoxOutCues.Items.Clear();
            comboBoxOutCues.Items.Add(language.CreateSimpleOutCues0Frames);
            comboBoxOutCues.Items.Add(language.CreateSimpleOutCues1Frames);
            comboBoxOutCues.Items.Add(language.CreateSimpleOutCues2Frames);
            comboBoxOutCues.Items.Add(language.CreateSimpleOutCues3Frames);
            comboBoxOutCues.Items.Add(language.CreateSimpleOutCuesGap);
            comboBoxOutCues.SelectedIndex = 0;
            checkBoxSnapClosestCue.Text = language.CreateSimpleSnapClosestCue;
            checkBoxSnapClosestCue.Checked = true;
            labelOffset.Text = language.CreateSimpleMaxOffset;
            labelOffsetSuffix.Text = language.Frames;
            labelOffsetInstruction.Text = language.CreateSimpleMaxOffsetInstruction;
            labelSafeZone.Text = language.CreateSimpleSafeZone;
            labelSafeZoneSuffix.Text = language.Frames;
            labelSafeZoneInstruction.Text = language.CreateSimpleSafeZoneInstruction;
            labelChainingGap.Text = language.CreateSimpleChainingGap;
            labelChainingGapSuffix.Text = language.Milliseconds;
            labelChainingGapInstruction.Text = language.CreateSimpleChainingGapInstruction;
            checkBoxChainingGapAfterShotChanges.Text = language.CreateSimpleChainingGapAfterShotChanges;
            labelChainingGapAfterShotChangesPrefix.Text = language.Maximum;
            labelChainingGapAfterShotChangesSuffix.Text = language.Milliseconds;
            toolTipChaining.SetToolTip(pictureBoxChainingInfo, language.CreateSimpleChainingToolTip);
            buttonLoadNetflixRules.Text = language.CreateSimpleLoadNetflixRules;

            labelGapHint.Left = labelGapSuffix.Left + labelGapSuffix.Width + 15;
            numericUpDownChainingGapAfterShotChanges.Left = labelChainingGapAfterShotChangesPrefix.Left + labelChainingGapAfterShotChangesPrefix.Width + 10;
            labelChainingGapAfterShotChangesSuffix.Left = numericUpDownChainingGapAfterShotChanges.Left + numericUpDownChainingGapAfterShotChanges.Width + 6;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            RefreshControls();
        }

        private void RefreshControls()
        {
            numericUpDownGap_ValueChanged(numericUpDownGap, EventArgs.Empty);
            checkBoxChainingGapAfterShotChanges_CheckedChanged(checkBoxChainingGapAfterShotChanges, EventArgs.Empty);
            comboBoxInCues_SelectedIndexChanged(comboBoxInCues, EventArgs.Empty);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Validation
            if (numericUpDownSafeZone.Value < numericUpDownOffset.Value)
            {
                MessageBox.Show(this, LanguageSettings.Current.BeautifyTimeCodesProfile.OffsetSafeZoneError, LanguageSettings.Current.General.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }

            // Save settings
            int gap;
            try
            {
                gap = Convert.ToInt32(numericUpDownGap.Value);
            }
            catch (Exception exception)
            {
                throw new Exception($"Unable to set gap from numericUpDownGap.Value to Int32: numericUpDownGap.Value = '{numericUpDownGap.Value}'", exception);
            }

            var inCuesGap = comboBoxInCues.SelectedIndex;
            var outCuesGap = comboBoxOutCues.SelectedIndex;
            if (comboBoxOutCues.SelectedIndex == comboBoxOutCues.Items.Count - 1)
            {
                outCuesGap = gap;
            }

            int redZone;
            try
            {
                redZone = Convert.ToInt32(numericUpDownOffset.Value);
            }
            catch (Exception exception)
            {
                throw new Exception($"Unable to set redZone from numericUpDownOffset.Value to Int32: numericUpDownOffset.Value = '{numericUpDownOffset.Value}'", exception);
            }

            int greenZone;
            try
            {
                greenZone = Convert.ToInt32(numericUpDownSafeZone.Value);
            }
            catch (Exception exception)
            {
                throw new Exception($"Unable to set greenZone from numericUpDownSafeZone.Value to Int32: numericUpDownSafeZone.Value = '{numericUpDownSafeZone.Value}'", exception);
            }


            Configuration.Settings.BeautifyTimeCodes.Profile.Gap = gap;

            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap = inCuesGap;
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesLeftGreenZone = greenZone;
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesLeftRedZone = redZone;
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesRightRedZone = redZone;
            Configuration.Settings.BeautifyTimeCodes.Profile.InCuesRightGreenZone = greenZone;

            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap = outCuesGap;
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesLeftGreenZone = greenZone;
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesLeftRedZone = redZone;
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesRightRedZone = redZone;
            Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesRightGreenZone = greenZone;

            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestLeftGap = Math.Max(gap, outCuesGap);
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestRightGap = inCuesGap;
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestLeftGap = checkBoxSnapClosestCue.Checked ? inCuesGap : Math.Max(gap, outCuesGap);
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestRightGap = checkBoxSnapClosestCue.Checked ? Math.Max(gap, outCuesGap) : inCuesGap;
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesLeftGreenZone = greenZone;
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesLeftRedZone = redZone;
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesRightRedZone = redZone;
            Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesRightGreenZone = greenZone;


            double treadConnectedMs = 0;
            try
            {
                treadConnectedMs = Math.Round(SubtitleFormat.FramesToMilliseconds(gap, _frameRate) * 1.5);
            }
            catch (Exception exception)
            {
                SeLogger.Error(exception, "Error when executing: treadConnectedMs = Math.Round(SubtitleFormat.FramesToMilliseconds(gap, _frameRate) * 1.5); );");
                SeLogger.Error("BeautifyTimeCodesProfileSimple.buttonOK_Click: Gap is " + gap);
                SeLogger.Error("BeautifyTimeCodesProfileSimple.buttonOK_Click: FrameRate is " + _frameRate);
                SeLogger.Error("BeautifyTimeCodesProfileSimple.buttonOK_Click: treadConnectedMs is " + treadConnectedMs);
                throw new Exception($"Unable to calculate treadConnectedMs from gap '{gap}' and frame rate {_frameRate}", exception);
            }

            try
            {
                Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesTreatConnected = Convert.ToInt32(treadConnectedMs);
            }
            catch (Exception exception)
            {
                SeLogger.Error(exception, "Error when executing: Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesTreatConnected = Convert.ToInt32(treadConnectedMs);");
                SeLogger.Error("BeautifyTimeCodesProfileSimple.buttonOK_Click: Gap is " + gap);
                SeLogger.Error("BeautifyTimeCodesProfileSimple.buttonOK_Click: FrameRate is " + _frameRate);
                SeLogger.Error("BeautifyTimeCodesProfileSimple.buttonOK_Click: treadConnectedMs is " + treadConnectedMs);
                throw new Exception($"Unable to set ConnectedSubtitlesTreatConnected from treadConnectedMs '{treadConnectedMs}'", exception);
            }

            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralUseZones = false;
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralMaxGap = Convert.ToInt32(numericUpDownChainingGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftGreenZone = GetChainingZoneFrames(Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralMaxGap);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftRedZone = GetChainingZoneFrames(Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralMaxGap) - 1;
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingGeneralShotChangeBehavior = BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;

            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotUseZones = false;
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotMaxGap = Convert.ToInt32(numericUpDownChainingGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotLeftGreenZone = GetChainingZoneFrames(Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotMaxGap);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotLeftRedZone = GetChainingZoneFrames(Configuration.Settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotMaxGap) - 1;

            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotUseZones = false;
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotMaxGap = checkBoxChainingGapAfterShotChanges.Checked ? Convert.ToInt32(numericUpDownChainingGapAfterShotChanges.Value) : Convert.ToInt32(numericUpDownChainingGap.Value);
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotRightRedZone = GetChainingZoneFrames(Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotMaxGap) - 1;
            Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotRightGreenZone = GetChainingZoneFrames(Configuration.Settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotMaxGap);

            DialogResult = DialogResult.OK;
        }

        private int GetChainingZoneFrames(double valueMs)
        {
            var frameDurationMs = TimeCodesBeautifierUtils.GetFrameDurationMs(_frameRate);
            return Convert.ToInt32(Math.Floor(valueMs / frameDurationMs));
        }

        private void numericUpDownGap_ValueChanged(object sender, EventArgs e)
        {
            var gapFrames = Convert.ToInt32(numericUpDownGap.Value);
            double gapMs = SubtitleFormat.FramesToMilliseconds(gapFrames, _frameRate);

            labelGapHint.Text = string.Format(LanguageSettings.Current.BeautifyTimeCodesProfile.GapInMsFormat, Math.Round(gapMs), Math.Round(_frameRate, 3));
        }

        private void checkBoxChainingGapAfterShotChanges_CheckedChanged(object sender, EventArgs e)
        {
            labelChainingGapAfterShotChangesPrefix.Enabled = checkBoxChainingGapAfterShotChanges.Checked;
            numericUpDownChainingGapAfterShotChanges.Enabled = checkBoxChainingGapAfterShotChanges.Checked;
            labelChainingGapAfterShotChangesSuffix.Enabled = checkBoxChainingGapAfterShotChanges.Checked;
        }

        private void comboBoxInCues_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxInCues.SelectedIndex != 0 || comboBoxOutCues.SelectedIndex != 0)
            {
                checkBoxSnapClosestCue.Checked = false;
            }
        }

        private void buttonLoadNetflixRules_Click(object sender, EventArgs e)
        {
            numericUpDownGap.Value = 2;
            comboBoxInCues.SelectedIndex = 0;
            comboBoxOutCues.SelectedIndex = 2;
            checkBoxSnapClosestCue.Checked = false;
            numericUpDownOffset.Value = 7;
            numericUpDownSafeZone.Value = 12;
            numericUpDownChainingGap.Value = 500;
            checkBoxChainingGapAfterShotChanges.Checked = false;

            RefreshControls();
        }

        private void BeautifyTimeCodesProfileSimple_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#beautify_time_codes");
            }
        }
    }
}

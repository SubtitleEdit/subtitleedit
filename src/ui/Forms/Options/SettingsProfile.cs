using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class SettingsProfile : Form
    {
        public List<RulesProfile> RulesProfiles { get; set; }
        private bool _editOn;

        public SettingsProfile(List<RulesProfile> rulesProfiles, string name)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.Settings;
            Text = language.Profiles;
            groupBoxGeneralRules.Text = language.Rules;
            labelName.Text = LanguageSettings.Current.General.Name;
            labelSubMaxLen.Text = language.SubtitleLineMaximumLength;
            labelOptimalCharsPerSecond.Text = language.OptimalCharactersPerSecond;
            labelMaxCharsPerSecond.Text = language.MaximumCharactersPerSecond;
            labelMaxWordsPerMin.Text = language.MaximumWordsPerMinute;
            labelMinDuration.Text = language.DurationMinimumMilliseconds;
            labelMaxDuration.Text = language.DurationMaximumMilliseconds;
            labelMinGapMs.Text = language.MinimumGapMilliseconds;
            labelMaxLines.Text = language.MaximumLines;
            labelMergeShortLines.Text = language.MergeLinesShorterThan;
            labelDialogStyle.Text = language.DialogStyle;
            labelContinuationStyle.Text = language.ContinuationStyle;
            checkBoxCpsIncludeWhiteSpace.Text = language.CpsIncludesSpace;
            listViewProfiles.Columns[0].Text = LanguageSettings.Current.General.Name;
            listViewProfiles.Columns[1].Text = language.SubtitleLineMaximumLength;
            listViewProfiles.Columns[2].Text = language.OptimalCharactersPerSecond;
            listViewProfiles.Columns[3].Text = language.MaximumCharactersPerSecond;
            listViewProfiles.Columns[4].Text = language.MinimumGapMilliseconds;

            var l = LanguageSettings.Current.SubStationAlphaStyles;
            buttonImport.Text = l.Import;
            buttonExport.Text = l.Export;
            buttonCopy.Text = l.Copy;
            buttonAdd.Text = l.New;
            buttonRemove.Text = l.Remove;
            buttonRemoveAll.Text = l.RemoveAll;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            comboBoxDialogStyle.Left = labelDialogStyle.Left + labelDialogStyle.Width + 5;
            comboBoxDialogStyle.Width = comboBoxMergeShortLineLength.Width + (comboBoxMergeShortLineLength.Left - comboBoxDialogStyle.Left);

            comboBoxContinuationStyle.Left = labelContinuationStyle.Left + labelContinuationStyle.Width + 5;
            comboBoxContinuationStyle.Width = comboBoxMergeShortLineLength.Width + (comboBoxMergeShortLineLength.Left - comboBoxContinuationStyle.Left);

            comboBoxMergeShortLineLength.BeginUpdate();
            comboBoxMergeShortLineLength.Items.Clear();
            for (int i = 1; i < 100; i++)
            {
                comboBoxMergeShortLineLength.Items.Add(i.ToString(CultureInfo.InvariantCulture));
            }
            comboBoxMergeShortLineLength.EndUpdate();
            RulesProfiles = rulesProfiles;
            ShowRulesProfiles(rulesProfiles.FirstOrDefault(p => p.Name == name), true);
        }

        private void ShowRulesProfiles(RulesProfile itemToFocus, bool sort)
        {
            _editOn = false;
            var idx = listViewProfiles.SelectedItems.Count > 0 ? listViewProfiles.SelectedItems[0].Index : -1;
            if (sort)
            {
                RulesProfiles = RulesProfiles.OrderBy(p => p.Name).ToList();
            }
            listViewProfiles.BeginUpdate();
            listViewProfiles.Items.Clear();
            foreach (var profile in RulesProfiles)
            {
                var item = new ListViewItem { Text = profile.Name };
                item.SubItems.Add(profile.SubtitleLineMaximumLength.ToString(CultureInfo.InvariantCulture));
                item.SubItems.Add(profile.SubtitleOptimalCharactersPerSeconds.ToString(CultureInfo.CurrentCulture));
                item.SubItems.Add(profile.SubtitleMaximumCharactersPerSeconds.ToString(CultureInfo.CurrentCulture));
                item.SubItems.Add(profile.MinimumMillisecondsBetweenLines.ToString(CultureInfo.CurrentCulture));
                listViewProfiles.Items.Add(item);
                if (itemToFocus == profile)
                {
                    listViewProfiles.Items[listViewProfiles.Items.Count - 1].Selected = true;
                }
            }
            listViewProfiles.EndUpdate();
            if (itemToFocus == null && listViewProfiles.Items.Count > 0)
            {
                if (idx < 0)
                {
                    idx = 0;
                }
                else if (idx >= listViewProfiles.Items.Count)
                {
                    idx = listViewProfiles.Items.Count - 1;
                }
                listViewProfiles.Items[idx].Selected = true;
            }

            _editOn = true;
        }

        private void SettingsProfile_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void SettingsProfile_ResizeEnd(object sender, EventArgs e)
        {
            listViewProfiles.AutoSizeLastColumn();
        }

        private void SettingsProfile_Shown(object sender, EventArgs e)
        {
            SettingsProfile_ResizeEnd(sender, e);
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            RulesProfiles.Clear();
            ShowRulesProfiles(null, true);
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            var idx = listViewProfiles.SelectedItems.Count > 0 ? listViewProfiles.SelectedItems[0].Index : -1;
            if (idx < 0)
            {
                return;
            }

            RulesProfiles.RemoveAt(idx);
            ShowRulesProfiles(null, true);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var gs = new GeneralSettings();
            var profile = new RulesProfile(gs.Profiles.First()) { Name = "New", Id = Guid.NewGuid() };
            RulesProfiles.Add(profile);
            ShowRulesProfiles(profile, false);
            textBoxName.Focus();
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            var idx = listViewProfiles.SelectedItems.Count > 0 ? listViewProfiles.SelectedItems[0].Index : -1;
            if (idx < 0)
            {
                return;
            }

            var source = RulesProfiles[idx];
            var profile = new RulesProfile(source) { Name = "Copy of " + source.Name, Id = Guid.NewGuid() };
            RulesProfiles.Add(profile);
            ShowRulesProfiles(profile, false);
        }

        private void UpdateRulesProfilesLine(int idx)
        {
            if (idx < 0 || idx >= RulesProfiles.Count)
            {
                return;
            }

            var p = RulesProfiles[idx];
            var lvi = listViewProfiles.Items[idx];
            lvi.Text = p.Name;
            lvi.SubItems[1].Text = p.SubtitleLineMaximumLength.ToString(CultureInfo.InvariantCulture);
            lvi.SubItems[2].Text = p.SubtitleOptimalCharactersPerSeconds.ToString(CultureInfo.CurrentCulture);
            lvi.SubItems[3].Text = p.SubtitleMaximumCharactersPerSeconds.ToString(CultureInfo.CurrentCulture);
            lvi.SubItems[4].Text = p.MinimumMillisecondsBetweenLines.ToString(CultureInfo.CurrentCulture);
        }

        private void UiElementChanged(object sender, EventArgs e)
        {
            var idx = listViewProfiles.SelectedItems.Count > 0 ? listViewProfiles.SelectedItems[0].Index : -1;
            if (idx < 0 || !_editOn)
            {
                return;
            }

            RulesProfiles[idx].Name = textBoxName.Text.Trim();
            RulesProfiles[idx].SubtitleLineMaximumLength = (int)numericUpDownSubtitleLineMaximumLength.Value;
            RulesProfiles[idx].SubtitleOptimalCharactersPerSeconds = numericUpDownOptimalCharsSec.Value;
            RulesProfiles[idx].SubtitleMaximumCharactersPerSeconds = numericUpDownMaxCharsSec.Value;
            RulesProfiles[idx].SubtitleMinimumDisplayMilliseconds = (int)numericUpDownDurationMin.Value;
            RulesProfiles[idx].SubtitleMaximumDisplayMilliseconds = (int)numericUpDownDurationMax.Value;
            RulesProfiles[idx].MinimumMillisecondsBetweenLines = (int)numericUpDownMinGapMs.Value;
            RulesProfiles[idx].MaxNumberOfLines = (int)numericUpDownMaxNumberOfLines.Value;
            RulesProfiles[idx].SubtitleMaximumWordsPerMinute = (int)numericUpDownMaxWordsMin.Value;
            RulesProfiles[idx].CpsIncludesSpace = checkBoxCpsIncludeWhiteSpace.Checked;
            RulesProfiles[idx].MergeLinesShorterThan = comboBoxMergeShortLineLength.SelectedIndex + 1;
            RulesProfiles[idx].DialogStyle = DialogSplitMerge.GetDialogStyleFromIndex(comboBoxDialogStyle.SelectedIndex);
            RulesProfiles[idx].ContinuationStyle = ContinuationUtilities.GetContinuationStyleFromIndex(comboBoxContinuationStyle.SelectedIndex);
            UpdateRulesProfilesLine(idx);

            toolTipContinuationPreview.RemoveAll();
            toolTipContinuationPreview.SetToolTip(comboBoxContinuationStyle, ContinuationUtilities.GetContinuationStylePreview(RulesProfiles[idx].ContinuationStyle));
        }

        private void listViewProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idx = listViewProfiles.SelectedItems.Count > 0 ? listViewProfiles.SelectedItems[0].Index : -1;
            if (idx < 0 || idx >= RulesProfiles.Count)
            {
                return;
            }

            var oldEditOn = _editOn;
            _editOn = false;
            var p = RulesProfiles[idx];
            textBoxName.Text = p.Name;
            if (p.SubtitleLineMaximumLength >= numericUpDownSubtitleLineMaximumLength.Minimum && p.SubtitleLineMaximumLength <= numericUpDownSubtitleLineMaximumLength.Maximum)
            {
                numericUpDownSubtitleLineMaximumLength.Value = p.SubtitleLineMaximumLength;
            }
            if (p.SubtitleOptimalCharactersPerSeconds >= numericUpDownOptimalCharsSec.Minimum && p.SubtitleOptimalCharactersPerSeconds <= numericUpDownOptimalCharsSec.Maximum)
            {
                numericUpDownOptimalCharsSec.Value = p.SubtitleOptimalCharactersPerSeconds;
            }
            if (p.SubtitleMaximumCharactersPerSeconds >= numericUpDownMaxCharsSec.Minimum && p.SubtitleMaximumCharactersPerSeconds <= numericUpDownMaxCharsSec.Maximum)
            {
                numericUpDownMaxCharsSec.Value = p.SubtitleMaximumCharactersPerSeconds;
            }
            if (p.SubtitleMinimumDisplayMilliseconds >= numericUpDownDurationMin.Minimum && p.SubtitleMinimumDisplayMilliseconds <= numericUpDownDurationMin.Maximum)
            {
                numericUpDownDurationMin.Value = p.SubtitleMinimumDisplayMilliseconds;
            }
            if (p.SubtitleMaximumDisplayMilliseconds >= numericUpDownDurationMax.Minimum && p.SubtitleMaximumDisplayMilliseconds <= numericUpDownDurationMax.Maximum)
            {
                numericUpDownDurationMax.Value = p.SubtitleMaximumDisplayMilliseconds;
            }
            if (p.MinimumMillisecondsBetweenLines >= numericUpDownMinGapMs.Minimum && p.MinimumMillisecondsBetweenLines <= numericUpDownMinGapMs.Maximum)
            {
                numericUpDownMinGapMs.Value = p.MinimumMillisecondsBetweenLines;
            }
            if (p.MaxNumberOfLines >= numericUpDownMaxNumberOfLines.Minimum && p.MaxNumberOfLines <= numericUpDownMaxNumberOfLines.Maximum)
            {
                numericUpDownMaxNumberOfLines.Value = p.MaxNumberOfLines;
            }
            else
            {
                numericUpDownMaxNumberOfLines.Value = numericUpDownMaxNumberOfLines.Minimum;
            }
            if (p.SubtitleMaximumWordsPerMinute >= numericUpDownMaxWordsMin.Minimum && p.SubtitleMaximumWordsPerMinute <= numericUpDownMaxWordsMin.Maximum)
            {
                numericUpDownMaxWordsMin.Value = p.SubtitleMaximumWordsPerMinute;
            }
            checkBoxCpsIncludeWhiteSpace.Checked = RulesProfiles[idx].CpsIncludesSpace;
            var comboIdx = RulesProfiles[idx].MergeLinesShorterThan - 1;
            if (comboIdx >= 0 && comboIdx < comboBoxMergeShortLineLength.Items.Count)
            {
                try
                {
                    comboBoxMergeShortLineLength.SelectedIndex = comboIdx;
                }
                catch
                {
                    comboBoxMergeShortLineLength.SelectedIndex = 0;
                }
            }
            else
            {
                comboBoxMergeShortLineLength.SelectedIndex = 0;
            }

            comboBoxDialogStyle.Items.Clear();
            comboBoxDialogStyle.Items.Add(LanguageSettings.Current.Settings.DialogStyleDashBothLinesWithSpace);
            comboBoxDialogStyle.Items.Add(LanguageSettings.Current.Settings.DialogStyleDashBothLinesWithoutSpace);
            comboBoxDialogStyle.Items.Add(LanguageSettings.Current.Settings.DialogStyleDashSecondLineWithSpace);
            comboBoxDialogStyle.Items.Add(LanguageSettings.Current.Settings.DialogStyleDashSecondLineWithoutSpace);
            switch (RulesProfiles[idx].DialogStyle)
            {
                case DialogType.DashBothLinesWithSpace:
                    comboBoxDialogStyle.SelectedIndex = 0;
                    break;
                case DialogType.DashBothLinesWithoutSpace:
                    comboBoxDialogStyle.SelectedIndex = 1;
                    break;
                case DialogType.DashSecondLineWithSpace:
                    comboBoxDialogStyle.SelectedIndex = 2;
                    break;
                case DialogType.DashSecondLineWithoutSpace:
                    comboBoxDialogStyle.SelectedIndex = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            comboBoxContinuationStyle.Items.Clear();
            comboBoxContinuationStyle.Items.Add(LanguageSettings.Current.Settings.ContinuationStyleNone);
            comboBoxContinuationStyle.Items.Add(LanguageSettings.Current.Settings.ContinuationStyleNoneTrailingDots);
            comboBoxContinuationStyle.Items.Add(LanguageSettings.Current.Settings.ContinuationStyleNoneLeadingTrailingDots);
            comboBoxContinuationStyle.Items.Add(LanguageSettings.Current.Settings.ContinuationStyleOnlyTrailingDots);
            comboBoxContinuationStyle.Items.Add(LanguageSettings.Current.Settings.ContinuationStyleLeadingTrailingDots);
            comboBoxContinuationStyle.Items.Add(LanguageSettings.Current.Settings.ContinuationStyleLeadingTrailingDash);
            comboBoxContinuationStyle.Items.Add(LanguageSettings.Current.Settings.ContinuationStyleLeadingTrailingDashDots);
            comboBoxContinuationStyle.SelectedIndex = 0;
            toolTipContinuationPreview.RemoveAll();
            toolTipContinuationPreview.SetToolTip(comboBoxContinuationStyle, ContinuationUtilities.GetContinuationStylePreview(RulesProfiles[idx].ContinuationStyle));
            try
            {
                comboBoxContinuationStyle.SelectedIndex = ContinuationUtilities.GetIndexFromContinuationStyle(RulesProfiles[idx].ContinuationStyle);
            }
            catch
            { 
                // ignore
            }

            _editOn = oldEditOn;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (RulesProfiles.Count == 0)
            {
                RulesProfiles.AddRange(new GeneralSettings().Profiles);
            }
            DialogResult = DialogResult.OK;
        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            UiElementChanged(sender, e);
            var name = textBoxName.Text.Trim();
            buttonOK.Enabled = name != string.Empty && RulesProfiles.Count(p => p.Name.Trim() == name) < 2;
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            openFileDialogImport.Title = LanguageSettings.Current.Settings.ImportProfiles;
            openFileDialogImport.InitialDirectory = Configuration.DataDirectory;
            openFileDialogImport.Filter = LanguageSettings.Current.Settings.Profiles + "|*.profile";
            if (openFileDialogImport.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                var importProfiles = RulesProfile.Deserialize(FileUtil.ReadAllTextShared(openFileDialogImport.FileName, Encoding.UTF8));
                foreach (var profile in importProfiles)
                {
                    var name = profile.Name;
                    if (RulesProfiles.Any(p => p.Name == profile.Name))
                    {
                        profile.Name = name + "_2";
                    }
                    if (RulesProfiles.Any(p => p.Name == profile.Name))
                    {
                        profile.Name = name + "_" + Guid.NewGuid();
                    }
                    RulesProfiles.Add(profile);
                    ShowRulesProfiles(profile, false);
                }
            }
            catch
            {
                MessageBox.Show("Unable to import profiles");
            }
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            if (listViewProfiles.Items.Count == 0)
            {
                return;
            }

            using (var form = new SettingsProfileExport(RulesProfiles))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    //TODO: display something?
                }
            }
        }
    }
}

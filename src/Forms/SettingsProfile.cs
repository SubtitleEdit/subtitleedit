using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
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

            var language = Configuration.Settings.Language.Settings;
            Text = language.Profiles;
            groupBoxGeneralRules.Text = language.Rules;
            labelName.Text = Configuration.Settings.Language.General.Name;
            labelSubMaxLen.Text = language.SubtitleLineMaximumLength;
            labelOptimalCharsPerSecond.Text = language.OptimalCharactersPerSecond;
            labelMaxCharsPerSecond.Text = language.MaximumCharactersPerSecond;
            labelMaxWordsPerMin.Text = language.MaximumWordssPerMinute;
            labelMinDuration.Text = language.DurationMinimumMilliseconds;
            labelMaxDuration.Text = language.DurationMaximumMilliseconds;
            labelMinGapMs.Text = language.MinimumGapMilliseconds;
            labelMaxLines.Text = language.MaximumLines;
            labelMergeShortLines.Text = language.MergeLinesShorterThan;
            checkBoxCpsIncludeWhiteSpace.Text = language.CpsIncludesSpace;
            listViewProfiles.Columns[0].Text = Configuration.Settings.Language.General.Name;
            listViewProfiles.Columns[1].Text = language.SubtitleLineMaximumLength;
            listViewProfiles.Columns[2].Text = language.OptimalCharactersPerSecond;
            listViewProfiles.Columns[3].Text = language.MaximumCharactersPerSecond;
            listViewProfiles.Columns[4].Text = language.MinimumGapMilliseconds;

            var l = Configuration.Settings.Language.SubStationAlphaStyles;
            buttonImport.Text = l.Import;
            buttonExport.Text = l.Export;
            buttonCopy.Text = l.Copy;
            buttonAdd.Text = l.New;
            buttonRemove.Text = l.Remove;
            buttonRemoveAll.Text = l.RemoveAll;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);


            comboBoxMergeShortLineLength.BeginUpdate();
            comboBoxMergeShortLineLength.Items.Clear();
            for (int i = 10; i < 100; i++)
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
            RulesProfiles[idx].MergeLinesShorterThan = comboBoxMergeShortLineLength.SelectedIndex + 10;
            UpdateRulesProfilesLine(idx);
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
            checkBoxCpsIncludeWhiteSpace.Checked = RulesProfiles[idx].CpsIncludesSpace;
            if (RulesProfiles[idx].MergeLinesShorterThan >= 10 && RulesProfiles[idx].MergeLinesShorterThan - 10 < comboBoxMergeShortLineLength.Items.Count)
            {
                comboBoxMergeShortLineLength.SelectedIndex = RulesProfiles[idx].MergeLinesShorterThan - 10;
            }
            else
            {
                comboBoxMergeShortLineLength.SelectedIndex = 0;
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
            openFileDialogImport.Title = Configuration.Settings.Language.Settings.ImportProfiles;
            openFileDialogImport.InitialDirectory = Configuration.DataDirectory;
            openFileDialogImport.Filter = Configuration.Settings.Language.Settings.Profiles + "|*.profile";
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

using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ProfileChoose : Form
    {
        private List<RulesProfile> RulesProfiles { get; set; }

        public ProfileChoose(List<RulesProfile> rulesProfiles, string name)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.Settings;
            Text = language.Profiles;
            listViewProfiles.Columns[0].Text = LanguageSettings.Current.General.Name;
            listViewProfiles.Columns[1].Text = language.SubtitleLineMaximumLength;
            listViewProfiles.Columns[2].Text = language.OptimalCharactersPerSecond;
            listViewProfiles.Columns[3].Text = language.MaximumCharactersPerSecond;
            listViewProfiles.Columns[4].Text = language.MinimumGapMilliseconds;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            RulesProfiles = rulesProfiles;
            if (RulesProfiles.Count == 0)
            {
                RulesProfiles.AddRange(new GeneralSettings().Profiles);
            }
            ShowRulesProfiles(rulesProfiles.FirstOrDefault(p => p.Name == name));
        }

        private void ShowRulesProfiles(RulesProfile itemToFocus)
        {
            var idx = listViewProfiles.SelectedItems.Count > 0 ? listViewProfiles.SelectedItems[0].Index : -1;
            RulesProfiles = RulesProfiles.OrderBy(p => p.Name).ToList();
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
                listViewProfiles.Items[idx].Focused = true;
            }
        }

        private void ProfileChoose_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var idx = listViewProfiles.SelectedIndices[0];
            Configuration.Settings.General.CurrentProfile = RulesProfiles[idx].Name;
            Configuration.Settings.General.SubtitleLineMaximumLength = RulesProfiles[idx].SubtitleLineMaximumLength;
            Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds = (double)RulesProfiles[idx].SubtitleOptimalCharactersPerSeconds;
            Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds = (double)RulesProfiles[idx].SubtitleMaximumCharactersPerSeconds;
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = RulesProfiles[idx].SubtitleMinimumDisplayMilliseconds;
            Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds = RulesProfiles[idx].SubtitleMaximumDisplayMilliseconds;
            Configuration.Settings.General.MinimumMillisecondsBetweenLines = RulesProfiles[idx].MinimumMillisecondsBetweenLines;
            Configuration.Settings.General.MaxNumberOfLines = RulesProfiles[idx].MaxNumberOfLines;
            Configuration.Settings.General.SubtitleMaximumWordsPerMinute = (double)RulesProfiles[idx].SubtitleMaximumWordsPerMinute;
            Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace = !RulesProfiles[idx].CpsIncludesSpace;
            Configuration.Settings.General.MergeLinesShorterThan = RulesProfiles[idx].MergeLinesShorterThan;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void listViewProfiles_DoubleClick(object sender, EventArgs e)
        {
            if (listViewProfiles.SelectedItems.Count > 0)
            {
                buttonOK_Click(sender, e);
            }
        }

        private void listViewProfiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(sender, e);
            }
        }

        private void ProfileChoose_ResizeEnd(object sender, EventArgs e)
        {
            listViewProfiles.AutoSizeLastColumn();
        }

        private void ProfileChoose_Shown(object sender, EventArgs e)
        {
            ProfileChoose_ResizeEnd(sender, e);

            if (listViewProfiles.SelectedIndices.Count == 0)
            {
                return;
            }

            listViewProfiles.Focus();
            var idx = listViewProfiles.SelectedIndices[0];
            listViewProfiles.Items[idx].Selected = true;
            listViewProfiles.Items[idx].Focused = true;
        }
    }
}

﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.NetflixQualityCheck;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.Options;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Nikse.SubtitleEdit.Forms.FixCommonErrors;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class NetflixFixErrors : Form
    {
        private readonly Subtitle _subtitle;
        private readonly SubtitleFormat _subtitleFormat;
        private readonly string _subtitleFileName;
        private readonly string _videoFileName;
        private readonly double _frameRate;
        private bool _loading;
        private string _language;
        private NetflixQualityController _netflixQualityController;

        public NetflixFixErrors(Subtitle subtitle, SubtitleFormat subtitleFormat, string subtitleFileName, string videoFileName, double frameRate)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            if (Configuration.Settings.General.UseDarkTheme)
            {
                listViewFixes.GridLines = Configuration.Settings.General.DarkThemeShowListViewGridLines;
            }

            _subtitle = subtitle;
            _subtitleFormat = subtitleFormat;
            _subtitleFileName = subtitleFileName;
            _videoFileName = videoFileName;
            _frameRate = frameRate;

            labelTotal.Text = string.Empty;
            linkLabelOpenReportFolder.Text = string.Empty;
            Text = LanguageSettings.Current.Main.Menu.ToolBar.NetflixQualityCheck;
            labelLanguage.Text = LanguageSettings.Current.ChooseLanguage.Language;
            groupBoxRules.Text = LanguageSettings.Current.Settings.Rules;
            checkBoxMinDuration.Text = LanguageSettings.Current.NetflixQualityCheck.MinimumDuration;
            buttonFixesSelectAll.Text = LanguageSettings.Current.FixCommonErrors.SelectAll;
            buttonFixesInverse.Text = LanguageSettings.Current.FixCommonErrors.InverseSelection;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            _loading = true;
            _language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
            comboBoxLanguage.SelectedIndexChanged += RuleCheckedChanged;
            InitializeLanguages(_language);
            RefreshCheckBoxes(_language);
            _loading = false;
            RuleCheckedChanged(null, null);
            UiUtil.FixLargeFonts(this, buttonOK);
            listViewFixes.ListViewItemSorter = new ListViewSorter { ColumnNumber = 0, IsNumber = true };
        }

        private void RefreshCheckBoxes(string language)
        {
            _netflixQualityController = new NetflixQualityController { Language = language, VideoFileName = _videoFileName, FrameRate = _frameRate };

            checkBoxNoItalics.Checked = !_netflixQualityController.AllowItalics;
            checkBoxNoItalics.Enabled = !_netflixQualityController.AllowItalics;

            int halfSecGapInFrames = (int)Math.Round(_frameRate / 2, MidpointRounding.AwayFromZero);
            checkBoxGapBridge.Text = $"Frame gap: 3 to {halfSecGapInFrames - 1} frames => 2 frames";

            var shotChangesExist = false;
            if (_netflixQualityController.VideoExists)
            {
                if (ShotChangeHelper.FromDisk(_videoFileName).Count > 0)
                {
                    shotChangesExist = true;
                }
            }
            checkBoxShotChange.Checked = shotChangesExist;
            checkBoxShotChange.Enabled = shotChangesExist;

            var checkFrameRate = _subtitleFormat.GetType() == new NetflixTimedText().GetType();
            checkBoxTtmlFrameRate.Checked = checkFrameRate;
            checkBoxTtmlFrameRate.Enabled = checkFrameRate;

            var speakerStyle = _netflixQualityController.SpeakerStyle;
            var checkBoxSpeakerStyleText = "Dual Speakers: Use a hyphen without a space";
            if (speakerStyle == DialogType.DashBothLinesWithSpace)
            {
                checkBoxSpeakerStyleText = "Dual Speakers: Use a hyphen with a space";
            }
            else if (speakerStyle == DialogType.DashSecondLineWithSpace)
            {
                checkBoxSpeakerStyleText = "Dual Speakers: Use a hyphen with a space to denote the second speaker only";
            }
            else if (speakerStyle == DialogType.DashSecondLineWithoutSpace)
            {
                checkBoxSpeakerStyleText = "Dual Speakers: Use a hyphen without a space to denote the second speaker only";
            }

            checkBoxSpeakerStyle.Text = checkBoxSpeakerStyleText;

            checkBox17CharsPerSecond.Text = string.Format(LanguageSettings.Current.NetflixQualityCheck.MaximumXCharsPerSecond, _netflixQualityController.CharactersPerSecond);
            checkBoxMaxLineLength.Text = string.Format(LanguageSettings.Current.NetflixQualityCheck.MaximumLineLength, _netflixQualityController.SingleLineMaxLength);
        }

        private void InitializeLanguages(string language)
        {
            comboBoxLanguage.SelectedIndexChanged -= RuleCheckedChanged;

            comboBoxLanguage.BeginUpdate();
            comboBoxLanguage.Items.Clear();
            foreach (var ci in Utilities.GetSubtitleLanguageCultures(true))
            {
                comboBoxLanguage.Items.Add(new LanguageItem(ci, ci.EnglishName));
            }
            comboBoxLanguage.Sorted = true;
            var languageCulture = CultureInfo.GetCultureInfo(language);
            var languageIndex = 0;
            for (var i = 0; i < comboBoxLanguage.Items.Count; i++)
            {
                var li = comboBoxLanguage.Items[i] as LanguageItem;
                if (li.Code.TwoLetterISOLanguageName == languageCulture.TwoLetterISOLanguageName)
                {
                    languageIndex = i;
                    break;
                }
                if (li.Code.TwoLetterISOLanguageName == "en")
                {
                    languageIndex = i;
                }
            }
            comboBoxLanguage.SelectedIndex = languageIndex;

            comboBoxLanguage.EndUpdate();

            comboBoxLanguage.Sorted = false;
            comboBoxLanguage.Items.Add(LanguageSettings.Current.General.ChangeLanguageFilter);

            if (comboBoxLanguage.SelectedIndex < 0 && comboBoxLanguage.Items.Count > 0)
            {
                comboBoxLanguage.SelectedIndex = 0;
            }

            comboBoxLanguage.SelectedIndexChanged += RuleCheckedChanged;
        }

        private async void RuleCheckedChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            await Task.Run(() => _netflixQualityController.RunChecks(_subtitle, GetAllSelectedChecks())).ConfigureAwait(true);
            labelTotal.Text = string.Format(LanguageSettings.Current.NetflixQualityCheck.FoundXIssues, _netflixQualityController.Records.Count);
            linkLabelOpenReportFolder.Left = labelTotal.Left + labelTotal.Width + 15;
            linkLabelOpenReportFolder.Text = LanguageSettings.Current.NetflixQualityCheck.OpenReportInFolder;
            checkBox17CharsPerSecond.Text = string.Format(LanguageSettings.Current.NetflixQualityCheck.MaximumXCharsPerSecond, _netflixQualityController.CharactersPerSecond);

            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            var listViewItems = new List<ListViewItem>();
            foreach (var record in _netflixQualityController.Records)
            {
                listViewItems.Add(MakeListViewItem(
                    record.OriginalParagraph,
                    record.Comment,
                    record.OriginalParagraph != null ? record.OriginalParagraph.ToString() : string.Empty,
                    record.FixedParagraph != null ? record.FixedParagraph.ToString() : string.Empty));
            }

            listViewFixes.Items.AddRange(listViewItems.ToArray());

            listViewFixes.EndUpdate();
        }

        private static ListViewItem MakeListViewItem(Paragraph p, string action, string before, string after)
        {
            var item = new ListViewItem(p.Number.ToString());
            item.SubItems.Add(action);
            item.SubItems.Add(before.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(after.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            return item;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
            }

            return base.ProcessDialogKey(keyData);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private List<INetflixQualityChecker> GetAllSelectedChecks()
        {
            var list = new List<INetflixQualityChecker>();
            if (checkBoxMinDuration.Checked)
            {
                list.Add(new NetflixCheckMinDuration());
            }

            if (checkBoxMaxDuration.Checked)
            {
                list.Add(new NetflixCheckMaxDuration());
            }

            if (checkBox17CharsPerSecond.Checked)
            {
                list.Add(new NetflixCheckMaxCps());
            }

            if (checkBoxGapMin.Checked)
            {
                list.Add(new NetflixCheckTwoFramesGap());
            }

            if (checkBoxGapBridge.Checked)
            {
                list.Add(new NetflixCheckBridgeGaps());
            }

            if (checkBoxTwoLinesMax.Checked)
            {
                list.Add(new NetflixCheckNumberOfLines());
            }

            if (checkBoxSpeakerStyle.Checked)
            {
                list.Add(new NetflixCheckDialogHyphenSpace());
            }

            if (checkBoxEllipsesNotThreeDots.Checked)
            {
                list.Add(new NetflixCheckEllipsesNotThreeDots());
            }

            if (checkBoxSquareBracketForHi.Checked)
            {
                list.Add(new NetflixCheckTextForHiUseBrackets());
            }

            if (checkBoxSpellOutStartNumbers.Checked)
            {
                list.Add(new NetflixCheckStartNumberSpellOut());
            }

            if (checkBoxWriteOutOneToTen.Checked)
            {
                list.Add(new NetflixCheckNumbersOneToTenSpellOut());
            }

            if (checkBoxCheckValidGlyphs.Checked)
            {
                list.Add(new NetflixCheckGlyph());
            }

            if (checkBoxNoItalics.Checked)
            {
                list.Add(new NetflixCheckItalics());
            }

            if (checkBoxTtmlFrameRate.Checked)
            {
                list.Add(new NetflixCheckTimedTextFrameRate());
            }

            if (checkBoxMaxLineLength.Checked)
            {
                list.Add(new NetflixCheckMaxLineLength());
            }

            if (checkBoxWhiteSpace.Checked)
            {
                list.Add(new NetflixCheckWhiteSpace());
            }

            if (checkBoxShotChange.Checked)
            {
                list.Add(new NetflixCheckShotChange());
            }

            return list;
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            if (comboBoxLanguage.SelectedIndex > 0 && comboBoxLanguage.Text == LanguageSettings.Current.General.ChangeLanguageFilter)
            {
                using (var form = new DefaultLanguagesChooser(Configuration.Settings.General.DefaultLanguages))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        Configuration.Settings.General.DefaultLanguages = form.DefaultLanguages;
                    }
                }

                InitializeLanguages(_language);
                return;
            }

            var languageItem = (LanguageItem)comboBoxLanguage.Items[comboBoxLanguage.SelectedIndex];
            RefreshCheckBoxes(languageItem.Code.TwoLetterISOLanguageName);
            _loading = false;
            RuleCheckedChanged(null, null);
        }

        private void ReadingSpeedChanged(object sender, EventArgs e)
        {
            _netflixQualityController.IsChildrenProgram = checkBoxChildrenProgram.Checked;
            _netflixQualityController.IsSDH = checkBoxSDH.Checked;
            RuleCheckedChanged(null, null);
        }

        private void linkLabelOpenReportFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Configuration.IsRunningOnWindows)
            {
                Process.Start("explorer.exe", $@"/select,""{MakeReport()}"" ");
            }
            else
            {
                UiUtil.OpenFolder(Path.GetDirectoryName(MakeReport()));
            }
        }

        private string MakeReport()
        {
            var fileName = string.IsNullOrEmpty(_subtitleFileName) ? "UntitledSubtitle" : Path.GetFileNameWithoutExtension(_subtitleFileName);
            var reportPath = Path.GetTempPath() + fileName + "_NetflixQualityCheck.csv";
            _netflixQualityController.SaveCsv(reportPath);
            return reportPath;
        }

        private void NetflixFixErrors_ResizeEnd(object sender, EventArgs e)
        {
            listViewFixes.AutoSizeLastColumn();
        }

        private void NetflixFixErrors_Shown(object sender, EventArgs e)
        {
            NetflixFixErrors_ResizeEnd(sender, e);
        }

        private void buttonFixesSelectAll_Click(object sender, EventArgs e)
        {
            _loading = true;
            foreach (var checkBox in groupBoxRules.Controls.OfType<CheckBox>())
            {
                if (checkBox.Enabled)
                {
                    checkBox.Checked = true;
                }
            }

            _loading = false;
            RuleCheckedChanged(null, null);
        }

        private void buttonFixesInverse_Click(object sender, EventArgs e)
        {
            _loading = true;
            foreach (var checkBox in groupBoxRules.Controls.OfType<CheckBox>())
            {
                if (checkBox.Enabled)
                {
                    checkBox.Checked = !checkBox.Checked;
                }
            }

            _loading = false;
            RuleCheckedChanged(null, null);
        }

        private void listViewFixes_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var sorter = (ListViewSorter)listViewFixes.ListViewItemSorter;
            if (e.Column == sorter.ColumnNumber)
            {
                sorter.Descending = !sorter.Descending; // inverse sort direction
            }
            else
            {
                sorter.ColumnNumber = e.Column;
                sorter.Descending = false;
                sorter.IsNumber = e.Column == 0; // only index 0 is numeric
            }

            listViewFixes.Sort();
        }
    }
}

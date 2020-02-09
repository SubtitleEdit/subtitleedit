using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.NetflixQualityCheck;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using static Nikse.SubtitleEdit.Forms.FixCommonErrors;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class NetflixFixErrors : Form
    {
        private readonly Subtitle _subtitle;
        private readonly SubtitleFormat _subtitleFormat;
        private readonly string _subtitleFileName;
        private bool _loading;
        private NetflixQualityController _netflixQualityController;

        public NetflixFixErrors(Subtitle subtitle, SubtitleFormat subtitleFormat, string subtitleFileName)
        {
            InitializeComponent();

            _subtitle = subtitle;
            _subtitleFormat = subtitleFormat;
            _subtitleFileName = subtitleFileName;

            labelTotal.Text = string.Empty;
            linkLabelOpenReportFolder.Text = string.Empty;
            Text = Configuration.Settings.Language.Main.Menu.ToolBar.NetflixQualityCheck;
            labelLanguage.Text = Configuration.Settings.Language.ChooseLanguage.Language;
            groupBoxRules.Text = Configuration.Settings.Language.Settings.Rules;
            checkBoxMinDuration.Text = Configuration.Settings.Language.NetflixQualityCheck.MinimumDuration;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            _loading = true;
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
            InitializeLanguages(language);
            RefreshCheckBoxes(language);
            _loading = false;
            RuleCheckedChanged(null, null);
        }

        private void RefreshCheckBoxes(string language)
        {
            _netflixQualityController = new NetflixQualityController { Language = language };

            checkBoxNoItalics.Checked = !_netflixQualityController.AllowItalics;
            checkBoxNoItalics.Enabled = !_netflixQualityController.AllowItalics;

            var checkFrameRate = _subtitleFormat.GetType() == new NetflixTimedText().GetType();
            checkBoxTtmlFrameRate.Checked = checkFrameRate;
            checkBoxTtmlFrameRate.Enabled = checkFrameRate;

            checkBoxDialogHypenNoSpace.Checked = _netflixQualityController.DualSpeakersHasHyphenAndNoSpace;
            checkBoxDialogHypenNoSpace.Enabled = _netflixQualityController.DualSpeakersHasHyphenAndNoSpace;

            checkBox17CharsPerSecond.Text = string.Format(Configuration.Settings.Language.NetflixQualityCheck.MaximumXCharsPerSecond, _netflixQualityController.CharactersPerSecond);
            checkBoxMaxLineLength.Text = string.Format(Configuration.Settings.Language.NetflixQualityCheck.MaximumLineLength, _netflixQualityController.SingleLineMaxLength);
        }

        private void InitializeLanguages(string language)
        {
            comboBoxLanguage.BeginUpdate();
            comboBoxLanguage.Items.Clear();
            foreach (var ci in Utilities.GetSubtitleLanguageCultures())
            {
                comboBoxLanguage.Items.Add(new LanguageItem(ci, ci.EnglishName));
            }
            comboBoxLanguage.Sorted = true;
            var languageCulture = CultureInfo.GetCultureInfo(language);
            int languageIndex = 0;
            for (int i = 0; i < comboBoxLanguage.Items.Count; i++)
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
            comboBoxLanguage.SelectedIndexChanged += RuleCheckedChanged;
            comboBoxLanguage.EndUpdate();
        }

        private void RuleCheckedChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            _netflixQualityController.RunChecks(_subtitle, GetAllSelectedChecks());
            labelTotal.Text = string.Format(Configuration.Settings.Language.NetflixQualityCheck.FoundXIssues, _netflixQualityController.Records.Count);
            linkLabelOpenReportFolder.Left = labelTotal.Left + labelTotal.Width + 15;
            linkLabelOpenReportFolder.Text = Configuration.Settings.Language.NetflixQualityCheck.OpenReportInFolder;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (var record in _netflixQualityController.Records)
            {
                AddFixToListView(
                    record.OriginalParagraph,
                    record.Comment,
                    record.OriginalParagraph != null ? record.OriginalParagraph.ToString() : string.Empty,
                    record.FixedParagraph != null ? record.FixedParagraph.ToString() : string.Empty);
            }
            listViewFixes.EndUpdate();
        }

        private void AddFixToListView(Paragraph p, string action, string before, string after)
        {
            var item = new ListViewItem(string.Empty) { Checked = true, Tag = p };
            item.SubItems.Add(p.Number.ToString());
            item.SubItems.Add(action);
            item.SubItems.Add(before.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(after.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            listViewFixes.Items.Add(item);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
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

            if (checkBoxGapMinTwoFrames.Checked)
            {
                list.Add(new NetflixCheckTwoFramesGap());
            }

            if (checkBoxTwoLinesMax.Checked)
            {
                list.Add(new NetflixCheckNumberOfLines());
            }

            if (checkBoxDialogHypenNoSpace.Checked)
            {
                list.Add(new NetflixCheckDialogHyphenSpace());
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

            return list;
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            _loading = true;
            var languageItem = (LanguageItem)comboBoxLanguage.Items[comboBoxLanguage.SelectedIndex];
            RefreshCheckBoxes(languageItem.Code.TwoLetterISOLanguageName);
            _loading = false;
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
                Logic.UiUtil.OpenFolder(Path.GetDirectoryName(MakeReport()));
            }
        }

        private string MakeReport()
        {
            var fileName = string.IsNullOrEmpty(_subtitleFileName) ? "UntitledSubtitle" : Path.GetFileNameWithoutExtension(_subtitleFileName);
            var reportPath = Path.GetTempPath() + fileName + "_NetflixQualityCheck.csv";
            _netflixQualityController.SaveCsv(reportPath);
            return reportPath;
        }
    }
}

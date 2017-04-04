﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class EbuSaveOptions : PositionAndSizeForm
    {
        private Ebu.EbuGeneralSubtitleInformation _header;
        private Subtitle _subtitle;

        public byte JustificationCode { get; private set; }

        public EbuSaveOptions()
        {
            InitializeComponent();

            var language = Configuration.Settings.Language.EbuSaveOptions;
            Text = language.Title;
            tabPageHeader.Text = language.GeneralSubtitleInformation;
            tabPageTextAndTiming.Text = language.TextAndTimingInformation;
            tabPageErrors.Text = language.Errors;

            labelCodePageNumber.Text = language.CodePageNumber;
            labelDiskFormatCode.Text = language.DiskFormatCode;
            labelDisplayStandardCode.Text = language.DisplayStandardCode;
            labelColorRequiresTeletext.Text = language.ColorRequiresTeletext;
            labelCharacterCodeTable.Text = language.CharacterCodeTable;
            labelLanguageCode.Text = language.LanguageCode;
            labelOriginalProgramTitle.Text = language.OriginalProgramTitle;
            labelOriginalEpisodeTitle.Text = language.OriginalEpisodeTitle;
            labelTranslatedProgramTitle.Text = language.TranslatedProgramTitle;
            labelTranslatedEpisodeTitle.Text = language.TranslatedEpisodeTitle;
            labelTranslatorsName.Text = language.TranslatorsName;
            labelSubtitleListReferenceCode.Text = language.SubtitleListReferenceCode;
            labelCountryOfOrigin.Text = language.CountryOfOrigin;
            labelTimeCodeStatus.Text = language.TimeCodeStatus;
            labelTimeCodeStartOfProgramme.Text = language.TimeCodeStartOfProgramme;
            labelFrameRate.Text = Configuration.Settings.Language.General.FrameRate;

            labelRevisionNumber.Text = language.RevisionNumber;
            labelMaxNoOfDisplayableChars.Text = language.MaxNoOfDisplayableChars;
            labelMaxNumberOfDisplayableRows.Text = language.MaxNumberOfDisplayableRows;
            labelDiskSequenceNumber.Text = language.DiskSequenceNumber;
            labelTotalNumberOfDisks.Text = language.TotalNumberOfDisks;

            buttonImport.Text = language.Import;

            labelJustificationCode.Text = language.JustificationCode;
            comboBoxJustificationCode.Items.Clear();
            comboBoxJustificationCode.Items.Add(language.TextUnchangedPresentation);
            comboBoxJustificationCode.Items.Add(language.TextLeftJustifiedText);
            comboBoxJustificationCode.Items.Add(language.TextCenteredText);
            comboBoxJustificationCode.Items.Add(language.TextRightJustifiedText);
            groupBoxTeletext.Text = language.Teletext;
            groupBoxVerticalPosition.Text = language.VerticalPosition;
            labelMarginTop.Text = language.MarginTop;
            labelMarginBottom.Text = language.MarginBottom;
            labelNewLineRows.Text = language.NewLineRows;
            int tempW = labelMarginTop.Left + 9 +  Math.Max(Math.Max(labelMarginTop.Width, labelMarginBottom.Width), labelNewLineRows.Width);
            numericUpDownMarginTop.Left = tempW;
            numericUpDownMarginBottom.Left = tempW;
            numericUpDownNewLineRows.Left = tempW;
            checkBoxTeletextBox.Text = language.UseBox;
            checkBoxTeletextDoubleHeight.Text = language.DoubleHeight;

            labelErrors.Text = language.Errors;

            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;

            timeUpDownStartTime.ForceHHMMSSFF();
        }

        internal void Initialize(Ebu.EbuGeneralSubtitleInformation header, byte justificationCode, string fileName, Subtitle subtitle)
        {
            _header = header;
            _subtitle = subtitle;

            if (_subtitle == null)
                tabControl1.TabPages.Remove(tabPageErrors);

            FillFromHeader(header);
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    FillHeaderFromFile(fileName);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("EbuOptions unable to read existing file: " + fileName + "  - " + ex.Message);
                }
                string title = Path.GetFileNameWithoutExtension(fileName);
                if (title.Length > 32)
                    title = title.Substring(0, 32).Trim();
                textBoxOriginalProgramTitle.Text = title;
            }

            comboBoxJustificationCode.SelectedIndex = justificationCode;
            numericUpDownMarginTop.Value = Configuration.Settings.SubtitleSettings.EbuStlMarginTop;
            numericUpDownMarginBottom.Value = Configuration.Settings.SubtitleSettings.EbuStlMarginBottom;
            numericUpDownNewLineRows.Value = Configuration.Settings.SubtitleSettings.EbuStlNewLineRows;
            checkBoxTeletextBox.Checked = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox;
            checkBoxTeletextDoubleHeight.Checked = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight;

            Text = Configuration.Settings.Language.EbuSaveOptions.Title;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
        }

        private void CheckErrors(Subtitle subtitle)
        {
            if (subtitle == null)
                return;

            textBoxErrors.Text = string.Empty;
            var sb = new StringBuilder();
            int errorCount = 0;
            int i = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var arr = p.Text.SplitToLines();
                foreach (string line in arr)
                {
                    string s = HtmlUtil.RemoveHtmlTags(line);
                    if (s.Length > numericUpDownMaxCharacters.Value)
                    {
                        sb.AppendLine(string.Format(Configuration.Settings.Language.EbuSaveOptions.MaxLengthError, i, numericUpDownMaxCharacters.Value, s.Length - numericUpDownMaxCharacters.Value, s));
                        errorCount++;
                    }
                }
                i++;
            }
            textBoxErrors.Text = sb.ToString();
            tabPageErrors.Text = string.Format(Configuration.Settings.Language.EbuSaveOptions.ErrorsX, errorCount);
        }

        private void FillFromHeader(Ebu.EbuGeneralSubtitleInformation header)
        {
            textBoxCodePageNumber.Text = header.CodePageNumber;

            comboBoxFrameRate.Items.Clear();
            comboBoxFrameRate.Items.Add(23.976);
            comboBoxFrameRate.Items.Add(24.0);
            comboBoxFrameRate.Items.Add(25.0);
            comboBoxFrameRate.Items.Add(29.97);
            comboBoxFrameRate.Items.Add(30.0);

            if (header.DiskFormatCode == "STL30.01")
            {
                comboBoxDiscFormatCode.SelectedIndex = 4;
                comboBoxFrameRate.Text = (30).ToString(CultureInfo.CurrentCulture);
            }
            else if (header.DiskFormatCode == "STL23.01")
            {
                comboBoxDiscFormatCode.SelectedIndex = 0;
                comboBoxFrameRate.Text = (23.976).ToString(CultureInfo.CurrentCulture);
            }
            else if (header.DiskFormatCode == "STL24.01")
            {
                comboBoxDiscFormatCode.SelectedIndex = 1;
                comboBoxFrameRate.Text = (24).ToString(CultureInfo.CurrentCulture);
            }
            else if (header.DiskFormatCode == "STL29.01")
            {
                comboBoxDiscFormatCode.SelectedIndex = 3;
                comboBoxFrameRate.Text = (25).ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                comboBoxDiscFormatCode.SelectedIndex = 2;
                comboBoxFrameRate.Text = (25).ToString(CultureInfo.CurrentCulture);
            }

            if (header.FrameRateFromSaveDialog > 20 && header.FrameRateFromSaveDialog < 200)
            {
                comboBoxFrameRate.Text = header.FrameRateFromSaveDialog.ToString(CultureInfo.CurrentCulture);
            }

            if (header.DisplayStandardCode == "0")
                comboBoxDisplayStandardCode.SelectedIndex = 0;
            else if (header.DisplayStandardCode == "1")
                comboBoxDisplayStandardCode.SelectedIndex = 1;
            else if (header.DisplayStandardCode == "2")
                comboBoxDisplayStandardCode.SelectedIndex = 2;
            else
                comboBoxDisplayStandardCode.SelectedIndex = 3;

            comboBoxCharacterCodeTable.SelectedIndex = int.Parse(header.CharacterCodeTableNumber);
            textBoxLanguageCode.Text = header.LanguageCode;
            textBoxOriginalProgramTitle.Text = header.OriginalProgrammeTitle.TrimEnd();
            textBoxOriginalEpisodeTitle.Text = header.OriginalEpisodeTitle.TrimEnd();
            textBoxTranslatedProgramTitle.Text = header.TranslatedProgrammeTitle.TrimEnd();
            textBoxTranslatedEpisodeTitle.Text = header.TranslatedEpisodeTitle.TrimEnd();
            textBoxTranslatorsName.Text = header.TranslatorsName.TrimEnd();
            textBoxSubtitleListReferenceCode.Text = header.SubtitleListReferenceCode.TrimEnd();
            textBoxCountryOfOrigin.Text = header.CountryOfOrigin;

            comboBoxTimeCodeStatus.SelectedIndex = 0;
            if (header.TimeCodeStatus == "1")
                comboBoxTimeCodeStatus.SelectedIndex = 1;
            try
            {
                // HHMMSSFF
                int hh = int.Parse(header.TimeCodeStartOfProgramme.Substring(0, 2));
                int mm = int.Parse(header.TimeCodeStartOfProgramme.Substring(2, 2));
                int ss = int.Parse(header.TimeCodeStartOfProgramme.Substring(4, 2));
                int ff = int.Parse(header.TimeCodeStartOfProgramme.Substring(6, 2));
                timeUpDownStartTime.TimeCode = new TimeCode(hh, mm, ss, SubtitleFormat.FramesToMillisecondsMax999(ff));
            }
            catch (Exception)
            {
                timeUpDownStartTime.TimeCode = new TimeCode();
            }

            int number;
            if (int.TryParse(header.RevisionNumber, out number))
                numericUpDownRevisionNumber.Value = number;
            else
                numericUpDownRevisionNumber.Value = 1;

            if (int.TryParse(header.MaximumNumberOfDisplayableCharactersInAnyTextRow, out number))
                numericUpDownMaxCharacters.Value = number;

            numericUpDownMaxRows.Value = 23;
            if (int.TryParse(header.MaximumNumberOfDisplayableRows, out number))
                numericUpDownMaxRows.Value = number;

            if (int.TryParse(header.DiskSequenceNumber, out number))
                numericUpDownDiskSequenceNumber.Value = number;
            else
                numericUpDownDiskSequenceNumber.Value = 1;
            if (int.TryParse(header.TotalNumberOfDisks, out number))
                numericUpDownTotalNumberOfDiscs.Value = number;
            else
                numericUpDownTotalNumberOfDiscs.Value = 1;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _header.CodePageNumber = textBoxCodePageNumber.Text;
            if (_header.CodePageNumber.Length < 3)
            {
                _header.CodePageNumber = "865";
            }

            if (comboBoxDiscFormatCode.SelectedIndex == 0)
                _header.DiskFormatCode = "STL23.01";
            else if (comboBoxDiscFormatCode.SelectedIndex == 1)
                _header.DiskFormatCode = "STL24.01";
            else if (comboBoxDiscFormatCode.SelectedIndex == 2)
                _header.DiskFormatCode = "STL25.01";
            else if (comboBoxDiscFormatCode.SelectedIndex == 3)
                _header.DiskFormatCode = "STL29.01";
            else
                _header.DiskFormatCode = "STL30.01";

            double d;
            if (double.TryParse(comboBoxFrameRate.Text.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), out d) && d > 20 && d < 200)
            {
                _header.FrameRateFromSaveDialog = d;
            }

            if (comboBoxDisplayStandardCode.SelectedIndex == 0)
                _header.DisplayStandardCode = "0";
            else if (comboBoxDisplayStandardCode.SelectedIndex == 1)
                _header.DisplayStandardCode = "1";
            else if (comboBoxDisplayStandardCode.SelectedIndex == 2)
                _header.DisplayStandardCode = "2";
            else
                _header.DisplayStandardCode = " ";

            _header.CharacterCodeTableNumber = "0" + comboBoxCharacterCodeTable.SelectedIndex;
            _header.LanguageCode = textBoxLanguageCode.Text;
            if (_header.LanguageCode.Length != 2)
                _header.LanguageCode = "0A";
            _header.OriginalProgrammeTitle = textBoxOriginalProgramTitle.Text.PadRight(32, ' ');
            _header.OriginalEpisodeTitle = textBoxOriginalEpisodeTitle.Text.PadRight(32, ' ');
            _header.TranslatedProgrammeTitle = textBoxTranslatedProgramTitle.Text.PadRight(32, ' ');
            _header.TranslatedEpisodeTitle = textBoxTranslatedEpisodeTitle.Text.PadRight(32, ' ');
            _header.TranslatorsName = textBoxTranslatorsName.Text.PadRight(32, ' ');
            _header.SubtitleListReferenceCode = textBoxSubtitleListReferenceCode.Text.PadRight(16, ' ');
            _header.CountryOfOrigin = textBoxCountryOfOrigin.Text;
            if (_header.CountryOfOrigin.Length != 3)
                _header.CountryOfOrigin = "USA";
            _header.TimeCodeStatus = comboBoxTimeCodeStatus.SelectedIndex.ToString(CultureInfo.InvariantCulture);
            _header.TimeCodeStartOfProgramme = timeUpDownStartTime.TimeCode.ToHHMMSSFF().Replace(":", string.Empty);

            _header.RevisionNumber = numericUpDownRevisionNumber.Value.ToString("00");
            _header.MaximumNumberOfDisplayableCharactersInAnyTextRow = numericUpDownMaxCharacters.Value.ToString("00");
            _header.MaximumNumberOfDisplayableRows = numericUpDownMaxRows.Value.ToString("00");
            _header.DiskSequenceNumber = numericUpDownDiskSequenceNumber.Value.ToString(CultureInfo.InvariantCulture);
            _header.TotalNumberOfDisks = numericUpDownTotalNumberOfDiscs.Value.ToString(CultureInfo.InvariantCulture);

            JustificationCode = (byte)comboBoxJustificationCode.SelectedIndex;
            Configuration.Settings.SubtitleSettings.EbuStlMarginTop = (int)Math.Round(numericUpDownMarginTop.Value);
            Configuration.Settings.SubtitleSettings.EbuStlMarginBottom = (int)Math.Round(numericUpDownMarginBottom.Value);
            Configuration.Settings.SubtitleSettings.EbuStlNewLineRows = (int)Math.Round(numericUpDownNewLineRows.Value);
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = checkBoxTeletextBox.Checked;
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = checkBoxTeletextDoubleHeight.Checked;

            if (_subtitle != null)
            {
                _subtitle.Header = _header.ToString();
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "EBU STL files (*.stl)|*.stl";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FillHeaderFromFile(openFileDialog1.FileName);
            }
        }

        private void FillHeaderFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                var ebu = new Ebu();
                var temp = new Subtitle();
                ebu.LoadSubtitle(temp, null, fileName);
                FillFromHeader(ebu.Header);
                if (ebu.JustificationCodes.Count > 2 && ebu.JustificationCodes[1] == ebu.JustificationCodes[2])
                {
                    if (ebu.JustificationCodes[1] >= 0 && ebu.JustificationCodes[1] < 4)
                        comboBoxJustificationCode.SelectedIndex = ebu.JustificationCodes[1];
                }
            }
        }

        private void numericUpDownMaxCharacters_ValueChanged(object sender, EventArgs e)
        {
            CheckErrors(_subtitle);
        }

        private void unitedStatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxCodePageNumber.Text = "437";
        }

        private void multilingualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxCodePageNumber.Text = "850";
        }

        private void portugalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxCodePageNumber.Text = "860";
        }

        private void canadaFrenchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxCodePageNumber.Text = "863";
        }

        private void nordicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxCodePageNumber.Text = "865";
        }

        private void comboBoxDiscFormatCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDiscFormatCode.SelectedIndex == 2)
            {
                comboBoxFrameRate.Text = (25).ToString(CultureInfo.CurrentCulture);
            }
            else if (comboBoxDiscFormatCode.SelectedIndex == 4)
            {
                comboBoxFrameRate.Text = (30).ToString(CultureInfo.CurrentCulture);
            }
        }

        private void comboBoxDisplayStandardCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_subtitle != null && comboBoxDisplayStandardCode.SelectedIndex == 0)
            {
                foreach (var paragraph in _subtitle.Paragraphs)
                {
                    if (paragraph.Text.Contains("<font color", StringComparison.OrdinalIgnoreCase))
                    {
                        labelColorRequiresTeletext.Visible = true;
                        return;
                    }
                }
            }
            labelColorRequiresTeletext.Visible = false;
        }

    }
}

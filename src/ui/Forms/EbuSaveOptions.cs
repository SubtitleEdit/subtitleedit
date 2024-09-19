using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
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
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.EbuSaveOptions;
            Text = language.Title;
            tabPageHeader.Text = language.GeneralSubtitleInformation;
            tabPageTextAndTiming.Text = language.TextAndTimingInformation;
            tabPageErrors.Text = language.Errors;

            labelCodePageNumber.Text = language.CodePageNumber;
            labelDiskFormatCode.Text = language.DiskFormatCode;
            labelDisplayStandardCode.Text = language.DisplayStandardCode;
            labelDisplayStandardCodeWarning.Text = string.Empty;
            labelMaxCharsPerRow38ForTeletext.Text = string.Empty;
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
            labelFrameRate.Text = LanguageSettings.Current.General.FrameRate;

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
            int tempW = labelMarginTop.Left + 9 + Math.Max(Math.Max(labelMarginTop.Width, labelMarginBottom.Width), labelNewLineRows.Width);
            numericUpDownMarginTop.Left = tempW;
            numericUpDownMarginBottom.Left = tempW;
            numericUpDownNewLineRows.Left = tempW;
            labelUseBox.Left = numericUpDownNewLineRows.Left + numericUpDownNewLineRows.Width + 9;
            checkBoxTeletextBox.Text = language.UseBox;
            checkBoxTeletextDoubleHeight.Text = language.DoubleHeight;

            labelErrors.Text = language.Errors;
            labelUseBox.Text = language.UseBoxForOneNewLine;

            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;

            labelLanguageCodeFriendlyName.Text = string.Empty;
            timeUpDownStartTime.ForceHHMMSSFF();

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        internal void Initialize(Ebu.EbuGeneralSubtitleInformation header, byte justificationCode, string fileName, Subtitle subtitle)
        {
            _header = header;
            _subtitle = subtitle;

            if (_subtitle == null)
            {
                tabControl1.TabPages.Remove(tabPageErrors);
            }

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

                var title = Path.GetFileNameWithoutExtension(fileName);
                if (title.Length > 32)
                {
                    title = title.Substring(0, 32).Trim();
                }

                textBoxOriginalProgramTitle.Text = title;
            }

            comboBoxJustificationCode.SelectedIndex = justificationCode;
            numericUpDownMarginTop.Value = Configuration.Settings.SubtitleSettings.EbuStlMarginTop;
            numericUpDownMarginBottom.Value = Configuration.Settings.SubtitleSettings.EbuStlMarginBottom;
            numericUpDownNewLineRows.Value = Configuration.Settings.SubtitleSettings.EbuStlNewLineRows;
            checkBoxTeletextBox.Checked = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox;
            checkBoxTeletextDoubleHeight.Checked = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight;

            Text = LanguageSettings.Current.EbuSaveOptions.Title;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private void CheckErrors(Subtitle subtitle)
        {
            if (subtitle == null)
            {
                return;
            }

            textBoxErrors.Text = string.Empty;
            var sb = new StringBuilder();
            var errorCount = 0;
            var i = 1;
            var isTeletext = comboBoxDisplayStandardCode.Text.Contains("teletext", StringComparison.OrdinalIgnoreCase);
            foreach (var p in subtitle.Paragraphs)
            {
                var arr = p.Text.SplitToLines();
                for (var index = 0; index < arr.Count; index++)
                {
                    var line = arr[index];
                    var s = HtmlUtil.RemoveHtmlTags(line, true);
                    if (s.Length > numericUpDownMaxCharacters.Value)
                    {
                        sb.AppendLine(string.Format(LanguageSettings.Current.EbuSaveOptions.MaxLengthError, i, numericUpDownMaxCharacters.Value, s.Length - numericUpDownMaxCharacters.Value, s));
                        errorCount++;
                    }

                    if (isTeletext)
                    {
                        // See https://kb.fab-online.com/0040-fabsubtitler-editor/00010-linelengthineditor/

                        // 36 characters for double height colored tex
                        if (arr.Count == 2 && s.Length > 36 && arr[index].Contains("<font ", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.AppendLine($"Line {i}-{index + 1}: 36 (not {s.Length}) should be maximum characters for double height colored text");
                            errorCount++;
                        }

                        // 37 characters for double height white text
                        else if (arr.Count == 2 && s.Length > 37 && !p.Text.Contains("<font ", StringComparison.OrdinalIgnoreCase))
                        {
                            sb.AppendLine($"Line {i}-{index + 1}: 37 (not {s.Length}) should be maximum characters for double height white text");
                            errorCount++;
                        }

                        // 38 characters for single height white text
                        else if (arr.Count == 1 && s.Length > 38)
                        {
                            sb.AppendLine($"Line {i}: 38 (not {s.Length}) should be maximum characters for single height white text");
                            errorCount++;
                        }
                    }
                }

                i++;
            }

            textBoxErrors.Text = sb.ToString();
            tabPageErrors.Text = string.Format(LanguageSettings.Current.EbuSaveOptions.ErrorsX, errorCount);
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
            {
                comboBoxDisplayStandardCode.SelectedIndex = 0;
            }
            else if (header.DisplayStandardCode == "1")
            {
                comboBoxDisplayStandardCode.SelectedIndex = 1;
            }
            else if (header.DisplayStandardCode == "2")
            {
                comboBoxDisplayStandardCode.SelectedIndex = 2;
            }
            else
            {
                comboBoxDisplayStandardCode.SelectedIndex = 3;
            }

            comboBoxCharacterCodeTable.SelectedIndex = int.Parse(header.CharacterCodeTableNumber);
            textBoxLanguageCode.Text = header.LanguageCode;
            textBoxOriginalProgramTitle.Text = header.OriginalProgrammeTitle.TrimEnd();
            textBoxOriginalEpisodeTitle.Text = header.OriginalEpisodeTitle.TrimEnd();
            textBoxTranslatedProgramTitle.Text = header.TranslatedProgrammeTitle.TrimEnd();
            textBoxTranslatedEpisodeTitle.Text = header.TranslatedEpisodeTitle.TrimEnd();
            textBoxTranslatorsName.Text = header.TranslatorsName.TrimEnd();
            textBoxSubtitleListReferenceCode.Text = header.SubtitleListReferenceCode.TrimEnd();
            textBoxCountryOfOrigin.Text = header.CountryOfOrigin;

            comboBoxTimeCodeStatus.SelectedIndex = 1;
            if (header.TimeCodeStatus == "0")
            {
                comboBoxTimeCodeStatus.SelectedIndex = 0; // 1 == intended for use, 0 == not intended for use
            }

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

            if (int.TryParse(header.RevisionNumber, out int number))
            {
                numericUpDownRevisionNumber.Value = number;
            }
            else
            {
                numericUpDownRevisionNumber.Value = 1;
            }

            if (int.TryParse(header.MaximumNumberOfDisplayableCharactersInAnyTextRow, out number))
            {
                numericUpDownMaxCharacters.Value = number;
            }

            numericUpDownMaxRows.Value = 23;
            if (int.TryParse(header.MaximumNumberOfDisplayableRows, out number))
            {
                numericUpDownMaxRows.Value = number;
            }

            if (int.TryParse(header.DiskSequenceNumber, out number))
            {
                numericUpDownDiskSequenceNumber.Value = number;
            }
            else
            {
                numericUpDownDiskSequenceNumber.Value = 1;
            }

            if (int.TryParse(header.TotalNumberOfDisks, out number))
            {
                numericUpDownTotalNumberOfDiscs.Value = number;
            }
            else
            {
                numericUpDownTotalNumberOfDiscs.Value = 1;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _header.CodePageNumber = textBoxCodePageNumber.Text;
            if (_header.CodePageNumber.Length < 3)
            {
                _header.CodePageNumber = "865";
            }

            if (comboBoxDiscFormatCode.SelectedIndex == 0)
            {
                _header.DiskFormatCode = "STL23.01";
            }
            else if (comboBoxDiscFormatCode.SelectedIndex == 1)
            {
                _header.DiskFormatCode = "STL24.01";
            }
            else if (comboBoxDiscFormatCode.SelectedIndex == 2)
            {
                _header.DiskFormatCode = "STL25.01";
            }
            else if (comboBoxDiscFormatCode.SelectedIndex == 3)
            {
                _header.DiskFormatCode = "STL29.01";
            }
            else
            {
                _header.DiskFormatCode = "STL30.01";
            }

            double d;
            if (double.TryParse(comboBoxFrameRate.Text.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), out d) && d > 20 && d < 200)
            {
                _header.FrameRateFromSaveDialog = d;
            }

            if (comboBoxDisplayStandardCode.SelectedIndex == 0)
            {
                _header.DisplayStandardCode = "0";
            }
            else if (comboBoxDisplayStandardCode.SelectedIndex == 1)
            {
                _header.DisplayStandardCode = "1";
            }
            else if (comboBoxDisplayStandardCode.SelectedIndex == 2)
            {
                _header.DisplayStandardCode = "2";
            }
            else
            {
                _header.DisplayStandardCode = " ";
            }

            _header.CharacterCodeTableNumber = "0" + comboBoxCharacterCodeTable.SelectedIndex;
            _header.LanguageCode = textBoxLanguageCode.Text;
            if (_header.LanguageCode.Length != 2)
            {
                _header.LanguageCode = "0A";
            }

            _header.OriginalProgrammeTitle = textBoxOriginalProgramTitle.Text.PadRight(32, ' ');
            _header.OriginalEpisodeTitle = textBoxOriginalEpisodeTitle.Text.PadRight(32, ' ');
            _header.TranslatedProgrammeTitle = textBoxTranslatedProgramTitle.Text.PadRight(32, ' ');
            _header.TranslatedEpisodeTitle = textBoxTranslatedEpisodeTitle.Text.PadRight(32, ' ');
            _header.TranslatorsName = textBoxTranslatorsName.Text.PadRight(32, ' ');
            _header.SubtitleListReferenceCode = textBoxSubtitleListReferenceCode.Text.PadRight(16, ' ');
            _header.CountryOfOrigin = textBoxCountryOfOrigin.Text;
            if (_header.CountryOfOrigin.Length != 3)
            {
                _header.CountryOfOrigin = "USA";
            }

            _header.TimeCodeStatus = comboBoxTimeCodeStatus.SelectedIndex.ToString(CultureInfo.InvariantCulture);
            _header.TimeCodeStartOfProgramme = timeUpDownStartTime.TimeCode.ToHHMMSSFF().RemoveChar(':');

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
                    {
                        comboBoxJustificationCode.SelectedIndex = ebu.JustificationCodes[1];
                    }
                }
            }
        }

        private void numericUpDownMaxCharacters_ValueChanged(object sender, EventArgs e)
        {
            CheckMaxCharsPerRow();
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
            labelDisplayStandardCodeWarning.Text = string.Empty;
            CheckMaxCharsPerRow();
            if (_subtitle == null)
            {
                return;
            }

            var fontColorFound = false;
            var alignmentFound = false;
            if (comboBoxDisplayStandardCode.SelectedIndex == 0) // open subtitling
            {
                foreach (var paragraph in _subtitle.Paragraphs)
                {
                    if (!fontColorFound &&
                        paragraph.Text.Contains("<font color", StringComparison.OrdinalIgnoreCase))
                    {
                        labelDisplayStandardCodeWarning.Text =
                            (labelDisplayStandardCodeWarning.Text + Environment.NewLine +
                            LanguageSettings.Current.EbuSaveOptions.ColorRequiresTeletext).Trim();
                        fontColorFound = true;
                    }

                    if (!alignmentFound &&
                        (paragraph.Text.Contains("{\\an1}", StringComparison.OrdinalIgnoreCase) ||
                        paragraph.Text.Contains("{\\an2}", StringComparison.OrdinalIgnoreCase) ||
                        paragraph.Text.Contains("{\\an3}", StringComparison.OrdinalIgnoreCase) ||
                        paragraph.Text.Contains("{\\an4}", StringComparison.OrdinalIgnoreCase) ||
                        paragraph.Text.Contains("{\\an5}", StringComparison.OrdinalIgnoreCase) ||
                        paragraph.Text.Contains("{\\an6}", StringComparison.OrdinalIgnoreCase) ||
                        paragraph.Text.Contains("{\\an7}", StringComparison.OrdinalIgnoreCase) ||
                        paragraph.Text.Contains("{\\an8}", StringComparison.OrdinalIgnoreCase) ||
                        paragraph.Text.Contains("{\\an9}", StringComparison.OrdinalIgnoreCase)))
                    {
                        labelDisplayStandardCodeWarning.Text =
                            (labelDisplayStandardCodeWarning.Text + Environment.NewLine +
                             LanguageSettings.Current.EbuSaveOptions.AlignmentRequiresTeletext).Trim();

                        alignmentFound = true;
                    }
                }
            }
        }

        private void CheckMaxCharsPerRow()
        {
            labelMaxCharsPerRow38ForTeletext.Text = string.Empty;
            if (_subtitle == null)
            {
                return;
            }

            // Teletext should have max chars per row = 38
            if ((comboBoxDisplayStandardCode.SelectedIndex == 1 || comboBoxDisplayStandardCode.SelectedIndex == 2) &&
                numericUpDownMaxCharacters.Value != 38)
            {
                labelMaxCharsPerRow38ForTeletext.Text = LanguageSettings.Current.EbuSaveOptions.TeletextCharsShouldBe38;
            }
        }

        private void buttonChooseLanguageCode_Click(object sender, EventArgs e)
        {
            using (var form = new EbuLanguageCode(textBoxLanguageCode.Text))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxLanguageCode.Text = form.LanguageCode;
                }
            }
        }

        private void textBoxLanguageCode_TextChanged(object sender, EventArgs e)
        {
            labelLanguageCodeFriendlyName.Text = EbuLanguageCode.GetLanguageFromCode(textBoxLanguageCode.Text);
        }

        private void numericUpDownNewLineRows_ValueChanged(object sender, EventArgs e)
        {
            labelUseBox.Visible = numericUpDownNewLineRows.Value == 1 && !checkBoxTeletextBox.Checked;
        }

        private void checkBoxTeletextBox_CheckedChanged(object sender, EventArgs e)
        {
            labelUseBox.Visible = numericUpDownNewLineRows.Value == 1 && !checkBoxTeletextBox.Checked;
        }
    }
}

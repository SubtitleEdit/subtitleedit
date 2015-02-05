using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;

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
            labelCharacterCodeTable.Text = language.CharacterCodeTable;
            labelLanguageCode.Text = language.LanguageCode;
            labelOriginalProgramTitle.Text = language.OriginalProgramTitle;
            labelOriginalEpisodeTitle.Text = language.OriginalEpisodeTitle;
            labelTranslatedProgramTitle.Text = language.TranslatedProgramTitle;
            labelTranslatedEpisodeTitle.Text = language.TranslatedEpisodeTitle;
            labelTranslatorsName.Text = language.TranslatorsName;
            labelSubtitleListReferenceCode.Text = language.SubtitleListReferenceCode;
            labelCountryOfOrigin.Text = language.CountryOfOrigin;

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

            labelErrors.Text = language.Errors;

            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
        }

        internal void Initialize(Ebu.EbuGeneralSubtitleInformation header, byte justificationCode, string fileName, Subtitle subtitle)
        {
            _header = header;
            _subtitle = subtitle;

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

            this.Text = Configuration.Settings.Language.EbuSaveOptions.Title;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
        }

        private void CheckErrors(Subtitle subtitle)
        {
            textBoxErrors.Text = string.Empty;
            StringBuilder sb = new StringBuilder();
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

            if (header.DiskFormatCode == "STL30.01")
                comboBoxDiscFormatCode.SelectedIndex = 1;
            else
                comboBoxDiscFormatCode.SelectedIndex = 0;

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

            if (comboBoxDiscFormatCode.SelectedIndex == 1)
                _header.DiskFormatCode = "STL30.01";
            else
                _header.DiskFormatCode = "STL25.01";

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
            _header.RevisionNumber = numericUpDownRevisionNumber.Value.ToString("00");
            _header.MaximumNumberOfDisplayableCharactersInAnyTextRow = numericUpDownMaxCharacters.Value.ToString("00");
            _header.MaximumNumberOfDisplayableRows = numericUpDownMaxRows.Value.ToString("00");
            _header.DiskSequenceNumber = numericUpDownDiskSequenceNumber.Value.ToString();
            _header.TotalNumberOfDisks = numericUpDownTotalNumberOfDiscs.Value.ToString();
            JustificationCode = (byte)comboBoxJustificationCode.SelectedIndex;
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
                Ebu ebu = new Ebu();
                Subtitle temp = new Subtitle();
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
    }
}

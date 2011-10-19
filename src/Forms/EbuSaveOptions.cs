using System;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using System.Text;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class EbuSaveOptions : Form
    {
        Ebu.EbuGeneralSubtitleInformation _header;
        Subtitle _subtitle;

        public byte JustificationCode { get; private set; }

        public EbuSaveOptions()
        {
            InitializeComponent();

            var language = Configuration.Settings.Language.EbuSaveOtpions;
            Text = language.Title;
            tabPageHeader.Text = language.GeneralSubtitleInformation;
            tabPageTextAndTiming.Text = language.TextAndTimingInformation;
            tabPageErrors.Text = language.Errors;

            labelCodePageNumber.Text = language.CodePageNumber;
            labelDiskFormatCode.Text = language.DiskFormatCode;
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
            comboBoxJustificationCode.Items.Add(language.TextCentredText);
            comboBoxJustificationCode.Items.Add(language.TextRightJustifiedText);

            labelErrors.Text = language.Errors;

            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
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

            this.Text = Configuration.Settings.Language.EbuSaveOtpions.Title;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
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
                string[] arr = p.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in arr)
                {
                    if (line.Length > numericUpDownMaxCharacters.Value)
                    {
                        sb.AppendLine(string.Format(Configuration.Settings.Language.EbuSaveOtpions.MaxLengthError, i, numericUpDownMaxCharacters.Value, line.Length - numericUpDownMaxCharacters.Value, line));
                        errorCount++;
                    }
                }
                i++;
            }
            textBoxErrors.Text = sb.ToString();
            tabPageErrors.Text = string.Format(Configuration.Settings.Language.EbuSaveOtpions.ErrorsX, errorCount);
        }

        private void FillFromHeader(Ebu.EbuGeneralSubtitleInformation header)
        {
            textBoxCodePageNumber.Text = header.CodePageNumber;

            if (header.DiskFormatCode == "STL30.01")
                comboBoxDiscFormatCode.SelectedIndex = 1;
            else
                comboBoxDiscFormatCode.SelectedIndex = 0;

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

            if (comboBoxDiscFormatCode.SelectedIndex == 1)
                _header.DiskFormatCode = "STL30.01";
            else
                _header.DiskFormatCode = "STL25.01";

            _header.CharacterCodeTableNumber = "0" + comboBoxCharacterCodeTable.SelectedIndex.ToString();
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
            _header.RevisionNumber = numericUpDownRevisionNumber.Value.ToString().PadLeft(2, '0');
            _header.MaximumNumberOfDisplayableCharactersInAnyTextRow = numericUpDownMaxCharacters.Value.ToString().PadLeft(2, '0');
            _header.MaximumNumberOfDisplayableRows = numericUpDownMaxRows.Value.ToString().PadLeft(2, '0');
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
            openFileDialog1.Filter = "Ebu stl files (*.stl)|*.stl";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FillHeaderFromFile(openFileDialog1.FileName);
            }
        }

        private void FillHeaderFromFile(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                Ebu ebu = new Ebu();
                Subtitle temp = new Subtitle();
                ebu.LoadSubtitle(temp, null, fileName);
                FillFromHeader(ebu.Header);
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

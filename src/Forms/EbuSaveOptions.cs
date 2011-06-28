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
        }

        internal void Initialize(Ebu.EbuGeneralSubtitleInformation header, byte justificationCode, string fileName, Subtitle subtitle)
        {
            _header = header;
            _subtitle = subtitle;

            FillFromHeader(header);
            try
            {
                FillHeaderFromFile(fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EbuOptions unable to read existing file: " + fileName + "  - " + ex.Message);
            }

            string title = Path.GetFileNameWithoutExtension(fileName);
            textBoxOriginalProgramTitle.Text = title;
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
                        sb.AppendLine(string.Format("Line {0} exceeds max length ({1}) by {2}: {3}", i, numericUpDownMaxCharacters.Value, line.Length - numericUpDownMaxCharacters.Value, line));
                        errorCount++;
                    }
                }
                i++;
            }
            textBoxErrors.Text = sb.ToString();
            tabPageErrors.Text = string.Format("Errors: {0}", errorCount);
        }

        private void FillFromHeader(Ebu.EbuGeneralSubtitleInformation header)
        {
            comboBoxCharacterCodeTable.SelectedIndex = int.Parse(header.CharacterCodeTableNumber);
            textBoxLanguageCode.Text = header.LanguageCode;
            textBoxOriginalProgramTitle.Text = header.OriginalProgrammeTitle.TrimEnd();
            textBoxOriginalEpisodeTitle.Text = header.OriginalEpisodeTitle.TrimEnd();
            textBoxTranslatedProgramTitle.Text = header.TranslatedProgrammeTitle.TrimEnd();
            textBoxTranslatedEpisodeTitle.Text = header.TranslatedEpisodeTitle.TrimEnd();
            textBoxTranslatorsName.Text = header.TranslatorsName.TrimEnd();
            textBoxSubtitleListReferenceCode.Text = header.SubtitleListReferenceCode.TrimEnd();
            
            int number;
            if (int.TryParse(header.RevisionNumber, out number))
                numericUpDownRevisionNumber.Value = number;
            else
                numericUpDownRevisionNumber.Value = 1;

            if (int.TryParse(header.MaximumNumberOfDisplayableCharactersInAnyTextRow, out number))
                numericUpDownMaxCharacters.Value = number;

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
            _header.CharacterCodeTableNumber = "0" + comboBoxCharacterCodeTable.SelectedIndex.ToString();
            _header.OriginalProgrammeTitle = textBoxOriginalProgramTitle.Text.PadRight(32, ' ');
            _header.OriginalEpisodeTitle = textBoxOriginalEpisodeTitle.Text.PadRight(32, ' '); 
            _header.TranslatedProgrammeTitle = textBoxTranslatedProgramTitle.Text.PadRight(32, ' '); 
            _header.TranslatedEpisodeTitle = textBoxTranslatedEpisodeTitle.Text.PadRight(32, ' '); 
            _header.TranslatorsName = textBoxTranslatorsName.Text.PadRight(32, ' '); 
            _header.SubtitleListReferenceCode = textBoxSubtitleListReferenceCode.Text.PadRight(16, ' ');
            _header.RevisionNumber = numericUpDownRevisionNumber.Value.ToString().PadLeft(2, '0');
            _header.MaximumNumberOfDisplayableCharactersInAnyTextRow = numericUpDownMaxCharacters.Value.ToString().PadLeft(2, '0');
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
    }
}

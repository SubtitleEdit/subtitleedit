using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ExportText : Form
    {
        private Subtitle _subtitle = null;
        private string _fileName = null;

        public ExportText()
        {
            InitializeComponent();
            var l = Configuration.Settings.Language.ExportText;
            Text = l.Title;
            labelPreview.Text = l.Preview;
            groupBoxImportOptions.Text = l.ExportOptions;
            groupBoxFormatText.Text = l.FormatText;
            radioButtonFormatNone.Text = l.None;
            radioButtonFormatMergeAll.Text = l.MergeAllLines;
            radioButtonFormatUnbreak.Text = l.UnbreakLines;
            checkBoxRemoveStyling.Text = l.RemoveStyling;
            checkBoxShowLineNumbers.Text = l.ShowLineNumbers;
            checkBoxAddNewlineAfterLineNumber.Text = l.AddNewLineAfterLineNumber;
            checkBoxShowTimeCodes.Text = l.ShowTimeCode;
            checkBoxAddAfterText.Text = l.AddNewLineAfterTexts;
            checkBoxAddNewlineAfterTimeCodes.Text = l.AddNewLineAfterTimeCode;
            checkBoxAddNewLine2.Text = l.AddNewLineBetweenSubtitles;
            groupBoxTimeCodeFormat.Text = l.TimeCodeFormat;
            radioButtonTimeCodeSrt.Text = l.Srt;
            radioButtonTimeCodeMs.Text = l.Milliseconds;
            radioButtonTimeCodeHHMMSSFF.Text = l.HHMMSSFF;
            labelTimeCodeSeparator.Text = l.TimeCodeSeparator;
            labelEncoding.Text = Configuration.Settings.Language.Main.Controls.FileEncoding;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.Main.Menu.File.SaveAs;
        }

        internal void Initialize(Logic.Subtitle subtitle, string fileName)
        {
            _subtitle = subtitle;
            _fileName = fileName;
            textBoxText.ReadOnly = true;
            comboBoxTimeCodeSeparator.SelectedIndex = 0;
            GeneratePreview();

            comboBoxEncoding.Items.Clear();
            int encodingSelectedIndex = 0;
            comboBoxEncoding.Items.Add(Encoding.UTF8.EncodingName);
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.Name != Encoding.UTF8.BodyName)
                {
                    if (ei.Name != Encoding.UTF8.BodyName && ei.CodePage >= 949 && !ei.DisplayName.Contains("EBCDIC") && ei.CodePage != 1047)
                    {
                        comboBoxEncoding.Items.Add(ei.CodePage + ": " + ei.DisplayName);
                        if (ei.Name == Configuration.Settings.General.DefaultEncoding)
                            encodingSelectedIndex = comboBoxEncoding.Items.Count - 1;
                    }
                }
            }
            comboBoxEncoding.SelectedIndex = encodingSelectedIndex;
        }

        private void GeneratePreview()
        {
            groupBoxTimeCodeFormat.Enabled = checkBoxShowTimeCodes.Checked;
            checkBoxAddAfterText.Enabled = !radioButtonFormatMergeAll.Checked;
            checkBoxAddNewLine2.Enabled = !radioButtonFormatMergeAll.Checked;
            checkBoxAddNewlineAfterLineNumber.Enabled = checkBoxShowLineNumbers.Checked;
            checkBoxAddNewlineAfterTimeCodes.Enabled = checkBoxShowTimeCodes.Checked;

            string text = GeneratePlainText(_subtitle, checkBoxShowLineNumbers.Checked, checkBoxAddNewlineAfterLineNumber.Checked, checkBoxShowTimeCodes.Checked,
                                            radioButtonTimeCodeSrt.Checked, radioButtonTimeCodeHHMMSSFF.Checked, checkBoxAddNewlineAfterTimeCodes.Checked,
                                            comboBoxTimeCodeSeparator.Text, checkBoxRemoveStyling.Checked, radioButtonFormatUnbreak.Checked, checkBoxAddAfterText.Checked,
                                            checkBoxAddNewLine2.Checked, radioButtonFormatMergeAll.Checked);
            textBoxText.Text = text;
        }

        public static string GeneratePlainText(Subtitle subtitle, bool showLineNumbers, bool addNewlineAfterLineNumber, bool showTimecodes,
                                         bool timeCodeSrt, bool timeCodeHHMMSSFF, bool addNewlineAfterTimeCodes, string timeCodeSeparator,
                                         bool removeStyling, bool formatUnbreak, bool addAfterText, bool checkBoxAddNewLine2, bool formatMergeAll)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (showLineNumbers)
                {
                    sb.Append(p.Number);
                    if (addNewlineAfterLineNumber)
                        sb.AppendLine();
                    else
                        sb.Append(' ');
                }
                if (showTimecodes)
                {
                    if (timeCodeSrt)
                        sb.Append(p.StartTime + timeCodeSeparator + p.EndTime);
                    else if (timeCodeHHMMSSFF)
                        sb.Append(p.StartTime.ToHHMMSSFF() + timeCodeSeparator + p.EndTime.ToHHMMSSFF());
                    else
                        sb.Append(p.StartTime.TotalMilliseconds + timeCodeSeparator + p.EndTime.TotalMilliseconds);

                    if (addNewlineAfterTimeCodes)
                        sb.AppendLine();
                    else
                        sb.Append(' ');

                }
                string s = p.Text;
                if (removeStyling)
                {
                    s = Utilities.RemoveHtmlTags(s, true);
                }

                if (formatUnbreak)
                {
                    sb.Append(s.Replace(Environment.NewLine, " ").Replace("  ", " "));
                }
                else
                {
                    sb.Append(s);
                }
                if (addAfterText)
                    sb.AppendLine();
                if (checkBoxAddNewLine2)
                    sb.AppendLine();
                if (!addAfterText && !checkBoxAddNewLine2)
                    sb.Append(' ');
            }
            string text = sb.ToString().Trim();
            if (formatMergeAll)
            {
                text = text.Replace(Environment.NewLine, " ");
                text = text.Replace("  ", " ").Replace("  ", " ");
            }
            return text;
        }

        private Encoding GetCurrentEncoding()
        {
            if (comboBoxEncoding.Text == Encoding.UTF8.BodyName || comboBoxEncoding.Text == Encoding.UTF8.EncodingName || comboBoxEncoding.Text == "utf-8")
            {
                return Encoding.UTF8;
            }

            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.CodePage + ": " + ei.DisplayName == comboBoxEncoding.Text)
                    return ei.GetEncoding();
            }

            return Encoding.UTF8;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            GeneratePreview();
            saveFileDialog1.Title = Configuration.Settings.Language.Main.ExportPlainTextAs;
            saveFileDialog1.Filter = Configuration.Settings.Language.Main.TextFiles + "|*.txt";
            if (!string.IsNullOrEmpty(_fileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog1.FileName, textBoxText.Text, GetCurrentEncoding());
                DialogResult = DialogResult.OK;
            }
        }

        private void radioButtonFormatNone_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}

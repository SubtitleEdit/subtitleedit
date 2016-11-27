using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ExportText : Form
    {
        private Subtitle _subtitle;
        private string _fileName;
        private bool _loading;

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
            _loading = true;
            if (Configuration.Settings.Tools.ExportTextFormatText == "None")
                radioButtonFormatNone.Checked = true;
            else if (Configuration.Settings.Tools.ExportTextFormatText == "Unbreak")
                radioButtonFormatUnbreak.Checked = true;
            else
                radioButtonFormatMergeAll.Checked = true;
            checkBoxRemoveStyling.Checked = Configuration.Settings.Tools.ExportTextRemoveStyling;
            checkBoxShowLineNumbers.Checked = Configuration.Settings.Tools.ExportTextShowLineNumbers;
            checkBoxAddNewlineAfterLineNumber.Checked = Configuration.Settings.Tools.ExportTextShowLineNumbersNewLine;
            checkBoxShowTimeCodes.Checked = Configuration.Settings.Tools.ExportTextShowTimeCodes;
            checkBoxAddNewlineAfterTimeCodes.Checked = Configuration.Settings.Tools.ExportTextShowTimeCodesNewLine;
            checkBoxAddAfterText.Checked = Configuration.Settings.Tools.ExportTextNewLineAfterText;
            checkBoxAddNewLine2.Checked = Configuration.Settings.Tools.ExportTextNewLineBetweenSubtitles;
        }

        internal void Initialize(Subtitle subtitle, string fileName)
        {
            _subtitle = subtitle;
            _fileName = fileName;
            textBoxText.ReadOnly = true;
            comboBoxTimeCodeSeparator.SelectedIndex = 0;
            _loading = false;
            GeneratePreview();

            UiUtil.InitializeTextEncodingComboBox(comboBoxEncoding);
        }

        private void GeneratePreview()
        {
            if (_loading)
                return;

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
                    s = HtmlUtil.RemoveHtmlTags(s, true);
                }
                if (formatUnbreak)
                {
                    sb.Append(Utilities.UnbreakLine(s));
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
            return UiUtil.GetTextEncodingComboBoxCurrentEncoding(comboBoxEncoding);
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

        private void ExportText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ExportText_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (radioButtonFormatNone.Checked)
                Configuration.Settings.Tools.ExportTextFormatText = "None";
            else if (radioButtonFormatUnbreak.Checked)
                Configuration.Settings.Tools.ExportTextFormatText = "Unbreak";
            else
                Configuration.Settings.Tools.ExportTextFormatText = "MergeAll";
            Configuration.Settings.Tools.ExportTextRemoveStyling = checkBoxRemoveStyling.Checked;
            Configuration.Settings.Tools.ExportTextShowLineNumbers = checkBoxShowLineNumbers.Checked;
            Configuration.Settings.Tools.ExportTextShowLineNumbersNewLine = checkBoxAddNewlineAfterLineNumber.Checked;
            Configuration.Settings.Tools.ExportTextShowTimeCodes = checkBoxShowTimeCodes.Checked;
            Configuration.Settings.Tools.ExportTextShowTimeCodesNewLine = checkBoxAddNewlineAfterTimeCodes.Checked;
            Configuration.Settings.Tools.ExportTextNewLineAfterText = checkBoxAddAfterText.Checked;
            Configuration.Settings.Tools.ExportTextNewLineBetweenSubtitles = checkBoxAddNewLine2.Checked;
        }

        public void PrepareForBatchSettings()
        {
            groupBoxTimeCodeFormat.Visible = false;
            labelEncoding.Visible = false;
            comboBoxEncoding.Visible = false;
            buttonOK.Visible = false;
            buttonCancel.Text = Configuration.Settings.Language.General.Ok;
        }

    }
}

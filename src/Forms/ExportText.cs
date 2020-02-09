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
        public class ExportOptions
        {
            public bool ShowLineNumbers { get; set; }
            public bool AddNewlineAfterLineNumber { get; set; }
            public bool ShowTimecodes { get; set; }
            public bool TimeCodeSrt { get; set; }
            public bool TimeCodeHHMMSSFF { get; set; }
            public bool AddNewlineAfterTimeCodes { get; set; }
            public string TimeCodeSeparator { get; set; }
            public bool RemoveStyling { get; set; }
            public bool FormatUnbreak { get; set; }
            public bool AddNewAfterText { get; set; }
            public bool AddNewAfterText2 { get; set; }
            public bool FormatMergeAll { get; set; }
        }

        private Subtitle _subtitle;
        private string _fileName;
        private bool _loading;

        public ExportText()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
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
            comboBoxTimeCodeSeparator.Left = labelTimeCodeSeparator.Left + labelTimeCodeSeparator.Width + 5;
            labelEncoding.Text = Configuration.Settings.Language.Main.Controls.FileEncoding;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.Main.Menu.File.SaveAs;

            _loading = true;
        }

        internal void Initialize(Subtitle subtitle, string fileName)
        {
            _subtitle = subtitle;
            _fileName = fileName;
            textBoxText.ReadOnly = true;
            comboBoxTimeCodeSeparator.SelectedIndex = 0;
            LoadSettings();
            _loading = false;
            GeneratePreview();
            UiUtil.InitializeTextEncodingComboBox(comboBoxEncoding);
        }

        private void LoadSettings()
        {
            if (Configuration.Settings.Tools.ExportTextFormatText == "None")
            {
                radioButtonFormatNone.Checked = true;
            }
            else if (Configuration.Settings.Tools.ExportTextFormatText == "Unbreak")
            {
                radioButtonFormatUnbreak.Checked = true;
            }
            else
            {
                radioButtonFormatMergeAll.Checked = true;
            }

            checkBoxRemoveStyling.Checked = Configuration.Settings.Tools.ExportTextRemoveStyling;
            checkBoxShowLineNumbers.Checked = Configuration.Settings.Tools.ExportTextShowLineNumbers;
            checkBoxAddNewlineAfterLineNumber.Checked = Configuration.Settings.Tools.ExportTextShowLineNumbersNewLine;
            checkBoxShowTimeCodes.Checked = Configuration.Settings.Tools.ExportTextShowTimeCodes;
            checkBoxAddNewlineAfterTimeCodes.Checked = Configuration.Settings.Tools.ExportTextShowTimeCodesNewLine;
            checkBoxAddAfterText.Checked = Configuration.Settings.Tools.ExportTextNewLineAfterText;
            checkBoxAddNewLine2.Checked = Configuration.Settings.Tools.ExportTextNewLineBetweenSubtitles;
            checkBoxShowTimeCodes.Checked = Configuration.Settings.Tools.ExportTextShowTimeCodes;

            if (Configuration.Settings.Tools.ExportTextTimeCodeFormat == "Frames")
            {
                radioButtonTimeCodeHHMMSSFF.Checked = true;
            }

            if (Configuration.Settings.Tools.ExportTextTimeCodeFormat == "Milliseconds")
            {
                radioButtonTimeCodeMs.Checked = true;
            }
            else
            {
                radioButtonTimeCodeSrt.Checked = true;
            }

            if (Configuration.Settings.Tools.ExportTextTimeCodeSeparator == comboBoxTimeCodeSeparator.Items[0].ToString())
            {
                comboBoxTimeCodeSeparator.SelectedIndex = 0;
            }
            else if (Configuration.Settings.Tools.ExportTextTimeCodeSeparator == comboBoxTimeCodeSeparator.Items[1].ToString())
            {
                comboBoxTimeCodeSeparator.SelectedIndex = 1;
            }
        }

        private void GeneratePreview()
        {
            if (_loading)
            {
                return;
            }

            groupBoxTimeCodeFormat.Enabled = checkBoxShowTimeCodes.Checked;
            checkBoxAddAfterText.Enabled = !radioButtonFormatMergeAll.Checked;
            checkBoxAddNewLine2.Enabled = !radioButtonFormatMergeAll.Checked;
            checkBoxAddNewlineAfterLineNumber.Enabled = checkBoxShowLineNumbers.Checked;
            checkBoxAddNewlineAfterTimeCodes.Enabled = checkBoxShowTimeCodes.Checked;

            var exportOptions = new ExportOptions
            {
                ShowLineNumbers = checkBoxShowLineNumbers.Checked,
                AddNewlineAfterLineNumber = checkBoxAddNewlineAfterLineNumber.Checked,
                ShowTimecodes = checkBoxShowTimeCodes.Checked,
                TimeCodeSrt = radioButtonTimeCodeSrt.Checked,
                TimeCodeHHMMSSFF = radioButtonTimeCodeHHMMSSFF.Checked,
                AddNewlineAfterTimeCodes = checkBoxAddNewlineAfterTimeCodes.Checked,
                TimeCodeSeparator = comboBoxTimeCodeSeparator.Text,
                RemoveStyling = checkBoxRemoveStyling.Checked,
                FormatUnbreak = radioButtonFormatUnbreak.Checked,
                AddNewAfterText = checkBoxAddAfterText.Checked,
                AddNewAfterText2 = checkBoxAddNewLine2.Checked,
                FormatMergeAll = radioButtonFormatMergeAll.Checked
            };

            string text = GeneratePlainText(_subtitle, exportOptions);
            textBoxText.Text = text;
        }

        public static string GeneratePlainText(Subtitle subtitle, ExportOptions exportOptions)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (exportOptions.ShowLineNumbers)
                {
                    sb.Append(p.Number);
                    if (exportOptions.AddNewlineAfterLineNumber)
                    {
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                }
                if (exportOptions.ShowTimecodes)
                {
                    if (exportOptions.TimeCodeSrt)
                    {
                        sb.Append(p.StartTime + exportOptions.TimeCodeSeparator + p.EndTime);
                    }
                    else if (exportOptions.TimeCodeHHMMSSFF)
                    {
                        sb.Append(p.StartTime.ToHHMMSSFF() + exportOptions.TimeCodeSeparator + p.EndTime.ToHHMMSSFF());
                    }
                    else
                    {
                        sb.Append(p.StartTime.TotalMilliseconds + exportOptions.TimeCodeSeparator + p.EndTime.TotalMilliseconds);
                    }

                    if (exportOptions.AddNewlineAfterTimeCodes)
                    {
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                }
                string s = p.Text;
                if (exportOptions.RemoveStyling)
                {
                    s = HtmlUtil.RemoveHtmlTags(s, true);
                }
                if (exportOptions.FormatUnbreak)
                {
                    sb.Append(Utilities.UnbreakLine(s));
                }
                else
                {
                    sb.Append(s);
                }
                if (exportOptions.AddNewAfterText)
                {
                    sb.AppendLine();
                }

                if (exportOptions.AddNewAfterText2)
                {
                    sb.AppendLine();
                }

                if (!exportOptions.AddNewAfterText && !exportOptions.AddNewAfterText2)
                {
                    sb.Append(' ');
                }
            }
            string text = sb.ToString().Trim();
            if (exportOptions.FormatMergeAll)
            {
                text = text.Replace(Environment.NewLine, " ");
                text = text.FixExtraSpaces();
            }
            return text;
        }

        private TextEncoding GetCurrentEncoding()
        {
            return UiUtil.GetTextEncodingComboBoxCurrentEncoding(comboBoxEncoding);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            GeneratePreview();
            saveFileDialog1.Title = Configuration.Settings.Language.Main.ExportPlainTextAs;
            saveFileDialog1.Filter = Configuration.Settings.Language.Main.TextFiles + "|*.txt";
            if (!string.IsNullOrEmpty(_fileName))
            {
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);
            }

            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                FileUtil.WriteAllText(saveFileDialog1.FileName, textBoxText.Text, GetCurrentEncoding());
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
            {
                Configuration.Settings.Tools.ExportTextFormatText = "None";
            }
            else if (radioButtonFormatUnbreak.Checked)
            {
                Configuration.Settings.Tools.ExportTextFormatText = "Unbreak";
            }
            else
            {
                Configuration.Settings.Tools.ExportTextFormatText = "MergeAll";
            }

            Configuration.Settings.Tools.ExportTextRemoveStyling = checkBoxRemoveStyling.Checked;
            Configuration.Settings.Tools.ExportTextShowLineNumbers = checkBoxShowLineNumbers.Checked;
            Configuration.Settings.Tools.ExportTextShowLineNumbersNewLine = checkBoxAddNewlineAfterLineNumber.Checked;
            Configuration.Settings.Tools.ExportTextShowTimeCodes = checkBoxShowTimeCodes.Checked;
            Configuration.Settings.Tools.ExportTextShowTimeCodesNewLine = checkBoxAddNewlineAfterTimeCodes.Checked;
            Configuration.Settings.Tools.ExportTextNewLineAfterText = checkBoxAddAfterText.Checked;
            Configuration.Settings.Tools.ExportTextNewLineBetweenSubtitles = checkBoxAddNewLine2.Checked;

            if (radioButtonTimeCodeHHMMSSFF.Checked)
            {
                Configuration.Settings.Tools.ExportTextTimeCodeFormat = "Frames";
            }
            else if (radioButtonTimeCodeMs.Checked)
            {
                Configuration.Settings.Tools.ExportTextTimeCodeFormat = "Milliseconds";
            }
            else
            {
                Configuration.Settings.Tools.ExportTextTimeCodeFormat = "Srt";
            }

            Configuration.Settings.Tools.ExportTextTimeCodeSeparator = comboBoxTimeCodeSeparator.Items[comboBoxTimeCodeSeparator.SelectedIndex].ToString();
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

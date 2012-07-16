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
        }

        internal void Initialize(Logic.Subtitle subtitle, string fileName)
        {
            _subtitle = subtitle;
            _fileName = fileName;
            textBoxText.ReadOnly = true;
            comboBoxTimeCodeSeperator.SelectedIndex = 0;
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

            var sb = new StringBuilder();
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (checkBoxShowLineNumbers.Checked)
                {
                    sb.Append(p.Number.ToString());
                    if (checkBoxAddNewlineAfterLineNumber.Checked)
                        sb.AppendLine();
                    else
                        sb.Append(" ");
                }
                if (checkBoxShowTimeCodes.Checked)
                {
                    if (radioButtonTimeCodeSrt.Checked)
                        sb.Append(p.StartTime.ToString() + comboBoxTimeCodeSeperator.Text + p.EndTime.ToString());
                    else if (radioButtonTimeCodeHHMMSSFF.Checked)
                        sb.Append(p.StartTime.ToHHMMSSFF() + comboBoxTimeCodeSeperator.Text + p.EndTime.ToHHMMSSFF());
                    else
                        sb.Append(p.StartTime.TotalMilliseconds.ToString() + comboBoxTimeCodeSeperator.Text + p.EndTime.TotalMilliseconds.ToString());

                    if (checkBoxAddNewlineAfterTimeCodes.Checked)
                        sb.AppendLine();
                    else
                        sb.Append(" ");

                }
                string s = p.Text;
                if (checkBoxRemoveStyling.Checked)
                {
                    s = Utilities.RemoveHtmlTags(s);
                    int indexOfEndBracket = s.IndexOf("}");
                    if (s.StartsWith("{\\") && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                        s = s.Remove(0, indexOfEndBracket+1);
                }

                if (radioButtonFormatUnbreak.Checked)
                {
                    sb.Append(s.Replace(Environment.NewLine, " ").Replace("  ", " "));
                }
                else
                {
                    sb.Append(s);
                }
                if (checkBoxAddAfterText.Checked)
                    sb.AppendLine();
                if (checkBoxAddNewLine2.Checked)
                    sb.AppendLine();
                if (!checkBoxAddAfterText.Checked && !checkBoxAddNewLine2.Checked)
                    sb.Append(" ");
            }
            string text = sb.ToString().Trim();
            if (radioButtonFormatMergeAll.Checked)
            {
                text = text.Replace(Environment.NewLine, " ");
                text = text.Replace("  ", " ").Replace("  ", " ");
            }
            textBoxText.Text = text;
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

        private void ExportText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}

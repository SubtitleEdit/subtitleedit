using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ExportCustomText : PositionAndSizeForm
    {
        private readonly List<string> _templates = new List<string>();
        private readonly Subtitle _subtitle;
        private readonly Subtitle _translated;
        private readonly string _title;
        public string LogMessage { get; set; }

        public ExportCustomText(Subtitle subtitle, Subtitle original, string title)
        {
            InitializeComponent();

            if (original == null || original.Paragraphs == null || original.Paragraphs.Count == 0)
            {
                _subtitle = subtitle;
            }
            else
            {
                _subtitle = original;
                _translated = subtitle;
            }
            _title = title;

            comboBoxEncoding.Items.Clear();
            int encodingSelectedIndex = 0;
            comboBoxEncoding.Items.Add(Encoding.UTF8.EncodingName);
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.Name != Encoding.UTF8.BodyName && ei.CodePage >= 949 && !ei.DisplayName.Contains("EBCDIC") && ei.CodePage != 1047)
                {
                    comboBoxEncoding.Items.Add(ei.CodePage + ": " + ei.DisplayName);
                    if (ei.Name == Configuration.Settings.General.DefaultEncoding)
                        encodingSelectedIndex = comboBoxEncoding.Items.Count - 1;
                }
            }
            comboBoxEncoding.SelectedIndex = encodingSelectedIndex;
            if (string.IsNullOrEmpty(Configuration.Settings.Tools.ExportCustomTemplates))
            {
                _templates.Add("SubRipÆÆ{number}\r\n{start} --> {end}\r\n{text}\r\n\r\nÆhh:mm:ss,zzzÆ[Do not modify]Æ");
            }
            else
            {
                foreach (string template in Configuration.Settings.Tools.ExportCustomTemplates.Split('æ'))
                    _templates.Add(template);
            }
            ShowTemplates(_templates);

            var l = Configuration.Settings.Language.ExportCustomText;
            Text = l.Title;
            groupBoxFormats.Text = l.Formats;
            buttonSave.Text = l.SaveAs;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            groupBoxPreview.Text = Configuration.Settings.Language.General.Preview;
            labelEncoding.Text = Configuration.Settings.Language.Main.Controls.FileEncoding;
            columnHeader1.Text = Configuration.Settings.Language.General.Name;
            columnHeader2.Text = Configuration.Settings.Language.General.Text;
            buttonNew.Text = l.New;
            buttonEdit.Text = l.Edit;
            buttonDelete.Text = l.Delete;
            deleteToolStripMenuItem.Text = l.Delete;
            editToolStripMenuItem.Text = l.Edit;
            newToolStripMenuItem.Text = l.New;
        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private void ShowTemplates(List<string> templates)
        {
            listViewTemplates.Items.Clear();
            foreach (string s in templates)
            {
                var arr = s.Split('Æ');
                if (arr.Length == 6)
                {
                    var lvi = new ListViewItem(arr[0]);
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, arr[2].Replace(Environment.NewLine, "<br />")));
                    listViewTemplates.Items.Add(lvi);
                }
            }
            if (listViewTemplates.Items.Count > 0)
                listViewTemplates.Items[0].Selected = true;
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            New();
        }

        private void New()
        {
            using (var form = new ExportCustomTextFormat("NewÆÆ{number}\r\n{start} --> {end}\r\n{text}\r\n\r\nÆhh:mm:ss,zzzÆ[Do not modify]Æ"))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _templates.Add(form.FormatOk);
                    ShowTemplates(_templates);
                    listViewTemplates.Items[listViewTemplates.Items.Count - 1].Selected = true;
                }
            }
            SaveTemplates();
        }

        private void SaveTemplates()
        {
            var sb = new StringBuilder();
            foreach (string template in _templates)
            {
                sb.Append(template + 'æ');
            }
            Configuration.Settings.Tools.ExportCustomTemplates = sb.ToString().TrimEnd('æ');
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            Edit();
        }

        private void Edit()
        {
            if (listViewTemplates.SelectedItems.Count == 1)
            {
                int idx = listViewTemplates.SelectedItems[0].Index;
                using (var form = new ExportCustomTextFormat(_templates[idx]))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        _templates[idx] = form.FormatOk;
                        ShowTemplates(_templates);
                        if (idx < listViewTemplates.Items.Count)
                            listViewTemplates.Items[idx].Selected = true;
                    }
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
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

        private void buttonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = Configuration.Settings.Language.ExportCustomText.SaveSubtitleAs;
            if (!string.IsNullOrEmpty(_title))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_title) + ".txt";
            saveFileDialog1.Filter = Configuration.Settings.Language.General.AllFiles + "|*.*";
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog1.FileName, GenerateText(_subtitle, _translated, _title), GetCurrentEncoding());
                    LogMessage = string.Format(Configuration.Settings.Language.ExportCustomText.SubtitleExportedInCustomFormatToX, saveFileDialog1.FileName);
                    DialogResult = DialogResult.OK;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private string GenerateText(Subtitle subtitle, Subtitle translation, string title)
        {
            if (listViewTemplates.SelectedItems.Count != 1)
                return string.Empty;

            if (title == null)
                title = string.Empty;
            else
                title = Path.GetFileNameWithoutExtension(title);

            try
            {
                int idx = listViewTemplates.SelectedItems[0].Index;
                var arr = _templates[idx].Split('Æ');
                var sb = new StringBuilder();
                sb.Append(ExportCustomTextFormat.GetHeaderOrFooter(title, subtitle, arr[1]));
                string template = ExportCustomTextFormat.GetParagraphTemplate(arr[2]);
                for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    Paragraph p = _subtitle.Paragraphs[i];
                    string start = ExportCustomTextFormat.GetTimeCode(p.StartTime, arr[3]);
                    string end = ExportCustomTextFormat.GetTimeCode(p.EndTime, arr[3]);
                    string text = ExportCustomTextFormat.GetText(p.Text, arr[4]);

                    string translationText = string.Empty;
                    if (translation != null && translation.Paragraphs != null && translation.Paragraphs.Count > 0)
                    {
                        var trans = Utilities.GetOriginalParagraph(idx, p, translation.Paragraphs);
                        if (trans != null)
                        {
                            translationText = trans.Text;
                        }
                    }

                    string paragraph = ExportCustomTextFormat.GetParagraph(template, start, end, text, translationText, i, p.Duration, arr[3]);
                    sb.Append(paragraph);
                }
                sb.Append(ExportCustomTextFormat.GetHeaderOrFooter(title, subtitle, arr[5]));
                return sb.ToString();
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        private void listViewTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxPreview.Text = GenerateText(_subtitle, _translated, _title);
            buttonSave.Enabled = listViewTemplates.SelectedItems.Count == 1;
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void Delete()
        {
            if (listViewTemplates.SelectedItems.Count != 1)
                return;

            int idx = listViewTemplates.SelectedItems[0].Index;
            for (int i = listViewTemplates.Items.Count - 1; i >= 0; i--)
            {
                ListViewItem item = listViewTemplates.Items[i];
                if (item.Selected)
                {
                    string name = item.Text;
                    for (int j = _templates.Count - 1; j > 0; j--)
                    {
                        if (_templates[j].StartsWith(name + "ÆÆ"))
                            _templates.RemoveAt(j);
                    }
                    item.Remove();
                }
            }
            if (idx >= listViewTemplates.Items.Count)
                idx--;
            if (idx >= 0)
                listViewTemplates.Items[idx].Selected = true;

            SaveTemplates();
        }

        private void ExportCustomText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Edit();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            New();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listViewTemplates.SelectedItems.Count == 0)
            {
                toolStripMenuItem2.Visible = false;
                editToolStripMenuItem.Visible = false;
                deleteToolStripMenuItem.Visible = false;
            }
            else
            {
                toolStripMenuItem2.Visible = true;
                editToolStripMenuItem.Visible = true;
                deleteToolStripMenuItem.Visible = true;
            }
        }

    }
}
using Nikse.SubtitleEdit.Core.Common;
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
        private bool _batchConvert;
        public string LogMessage { get; set; }
        public string CurrentFormatName { get; set; }

        public ExportCustomText(Subtitle subtitle, Subtitle original, string title)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            if (original?.Paragraphs == null || original.Paragraphs.Count == 0)
            {
                _subtitle = subtitle;
            }
            else
            {
                _subtitle = original;
                _translated = subtitle;
            }
            _title = title;

            UiUtil.InitializeTextEncodingComboBox(comboBoxEncoding);

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.ExportCustomTemplates))
            {
                _templates.Add("SubRipÆÆ{number}\r\n{start} --> {end}\r\n{text}\r\n\r\nÆhh:mm:ss,zzzÆ[Do not modify]Æ");
            }
            else
            {
                foreach (string template in Configuration.Settings.Tools.ExportCustomTemplates.Split('æ'))
                {
                    _templates.Add(template);
                }
            }
            ShowTemplates(_templates);

            var l = LanguageSettings.Current.ExportCustomText;
            Text = l.Title;
            groupBoxFormats.Text = l.Formats;
            buttonSave.Text = l.SaveAs;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            groupBoxPreview.Text = LanguageSettings.Current.General.Preview;
            labelEncoding.Text = LanguageSettings.Current.Main.Controls.FileEncoding;
            columnHeader1.Text = LanguageSettings.Current.General.Name;
            columnHeader2.Text = LanguageSettings.Current.General.Text;
            buttonNew.Text = l.New;
            buttonEdit.Text = l.Edit;
            buttonDelete.Text = l.Delete;
            deleteToolStripMenuItem.Text = l.Delete;
            editToolStripMenuItem.Text = l.Edit;
            newToolStripMenuItem.Text = l.New;
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
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
                    lvi.SubItems.Add(arr[2].Replace(Environment.NewLine, "<br />"));
                    listViewTemplates.Items.Add(lvi);
                }
            }
            if (listViewTemplates.Items.Count > 0)
            {
                listViewTemplates.Items[0].Selected = true;
            }
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            New();
        }

        private bool NameExists(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }

            for (int i = 0; i < _templates.Count; i++)
            {
                if (_templates[i].StartsWith(name + "Æ", StringComparison.InvariantCulture))
                {
                    return true;
                }
            }
            return false;
        }

        private void New()
        {
            using (var form = new ExportCustomTextFormat("NewÆÆ{number}\r\n{start} --> {end}\r\n{text}\r\n\r\nÆhh:mm:ss,zzzÆ[Do not modify]Æ"))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    var arr = form.FormatOk.Split('Æ');
                    if (arr.Length == 6)
                    {
                        string name = arr[0];
                        int i = 1;
                        while (NameExists(name))
                        {
                            form.FormatOk = form.FormatOk.Remove(0, name.Length);
                            name = arr[0] + " (" + i + ")";
                            form.FormatOk = name + form.FormatOk;
                            i++;
                        }
                        _templates.Add(form.FormatOk);
                        ShowTemplates(_templates);
                        listViewTemplates.Items[listViewTemplates.Items.Count - 1].Selected = true;
                    }
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
                        SaveTemplates();
                        ShowTemplates(_templates);
                        if (idx < listViewTemplates.Items.Count)
                        {
                            listViewTemplates.Items[idx].Selected = true;
                        }
                    }
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private TextEncoding GetCurrentEncoding()
        {
            return UiUtil.GetTextEncodingComboBoxCurrentEncoding(comboBoxEncoding);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (_batchConvert)
            {
                if (listViewTemplates.SelectedItems.Count == 1)
                {
                    CurrentFormatName = listViewTemplates.SelectedItems[0].Text;
                }

                DialogResult = DialogResult.OK;
                return;
            }

            saveFileDialog1.Title = LanguageSettings.Current.ExportCustomText.SaveSubtitleAs;
            if (!string.IsNullOrEmpty(_title))
            {
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_title) + ".txt";
            }

            saveFileDialog1.Filter = LanguageSettings.Current.General.AllFiles + "|*.*";
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    FileUtil.WriteAllText(saveFileDialog1.FileName, GenerateText(_subtitle, _translated, _title), GetCurrentEncoding());
                    LogMessage = string.Format(LanguageSettings.Current.ExportCustomText.SubtitleExportedInCustomFormatToX, saveFileDialog1.FileName);
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
            {
                return string.Empty;
            }

            title = title == null ? string.Empty : Path.GetFileNameWithoutExtension(title);

            try
            {
                int idx = listViewTemplates.SelectedItems[0].Index;
                return GenerateCustomText(subtitle, translation, title, _templates[idx]);
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        internal static string GenerateCustomText(Subtitle subtitle, Subtitle translation, string title, string templateString)
        {
            var arr = templateString.Split('Æ');
            var sb = new StringBuilder();
            sb.Append(ExportCustomTextFormat.GetHeaderOrFooter(title, subtitle, arr[1]));
            string template = ExportCustomTextFormat.GetParagraphTemplate(arr[2]);
            var isXml = arr[1].Contains("<?xml version=", StringComparison.OrdinalIgnoreCase);
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string start = ExportCustomTextFormat.GetTimeCode(p.StartTime, arr[3]);
                string end = ExportCustomTextFormat.GetTimeCode(p.EndTime, arr[3]);

                string text = p.Text;
                if (isXml)
                {
                    text = text.Replace("<", "&lt;")
                               .Replace(">", "&gt;")
                               .Replace("&", "&amp;");
                }
                text = ExportCustomTextFormat.GetText(text, arr[4]);

                string translationText = string.Empty;
                if (translation?.Paragraphs != null && translation.Paragraphs.Count > 0)
                {
                    var trans = Utilities.GetOriginalParagraph(i, p, translation.Paragraphs);
                    if (trans != null)
                    {
                        translationText = trans.Text;
                    }
                }
                string paragraph = ExportCustomTextFormat.GetParagraph(template, start, end, text, translationText, i, p.Actor, p.Duration, arr[3], Utilities.GetCharactersPerSecond(p));
                sb.Append(paragraph);
            }
            sb.Append(ExportCustomTextFormat.GetHeaderOrFooter(title, subtitle, arr[5]));
            return sb.ToString();
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
            {
                return;
            }

            int idx = listViewTemplates.SelectedItems[0].Index;
            for (int i = listViewTemplates.Items.Count - 1; i >= 0; i--)
            {
                ListViewItem item = listViewTemplates.Items[i];
                if (item.Selected)
                {
                    string name = item.Text;
                    for (int j = _templates.Count - 1; j > 0; j--)
                    {
                        if (_templates[j].StartsWith(name + "Æ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            _templates.RemoveAt(j);
                        }
                    }
                    item.Remove();
                }
            }
            if (idx >= listViewTemplates.Items.Count)
            {
                idx--;
            }

            if (idx >= 0)
            {
                listViewTemplates.Items[idx].Selected = true;
            }

            SaveTemplates();
        }

        private void ExportCustomText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ExportCustomText_ResizeEnd(object sender, EventArgs e)
        {
            listViewTemplates.AutoSizeLastColumn();
        }

        private void ExportCustomText_Shown(object sender, EventArgs e)
        {
            ExportCustomText_ResizeEnd(sender, e);
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
            bool enableVisibility = listViewTemplates.SelectedItems.Count > 0;
            toolStripMenuItem2.Visible = enableVisibility;
            editToolStripMenuItem.Visible = enableVisibility;
            deleteToolStripMenuItem.Visible = enableVisibility;
        }

        private void listViewTemplates_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Edit();
        }

        public void InitializeForBatchConvert(string customTextTemplate)
        {
            _batchConvert = true;
            buttonSave.Text = LanguageSettings.Current.General.Ok;

            for (int index = 0; index < _templates.Count; index++)
            {
                var item = _templates[index];
                if (item.StartsWith(customTextTemplate + "Æ", StringComparison.Ordinal))
                {
                    listViewTemplates.Items[index].Selected = true;
                }
            }
        }

        private void textBoxPreview_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                textBoxPreview.SelectAll();
            }
        }
    }
}

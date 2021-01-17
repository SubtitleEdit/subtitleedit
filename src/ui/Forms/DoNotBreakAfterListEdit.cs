using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class DoNotBreakAfterListEdit : Form
    {
        private readonly List<string> _languages = new List<string>();
        private List<NoBreakAfterItem> _noBreakAfterList = new List<NoBreakAfterItem>();

        public DoNotBreakAfterListEdit()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.Settings.UseDoNotBreakAfterList;
            labelLanguage.Text = LanguageSettings.Current.ChooseLanguage.Language;
            buttonRemoveNoBreakAfter.Text = LanguageSettings.Current.DvdSubRip.Remove;
            buttonAddNoBreakAfter.Text = LanguageSettings.Current.DvdSubRip.Add;
            radioButtonText.Text = LanguageSettings.Current.General.Text;
            radioButtonRegEx.Text = LanguageSettings.Current.MultipleReplace.RegularExpression;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            radioButtonRegEx.Left = radioButtonText.Left + radioButtonText.Width + 10;
            var idx = 0;
            foreach (string fileName in Directory.GetFiles(Configuration.DictionariesDirectory, "*_NoBreakAfterList.xml"))
            {
                try
                {
                    string s = Path.GetFileName(fileName);
                    string languageId = s.Substring(0, s.IndexOf('_'));
                    var ci = CultureInfo.GetCultureInfoByIetfLanguageTag(languageId);
                    comboBoxDictionaries.Items.Add(ci.EnglishName + " (" + ci.NativeName + ")");
                    if ((Configuration.Settings.WordLists.LastLanguage ?? "en-US").StartsWith(languageId, StringComparison.OrdinalIgnoreCase))
                    {
                        idx = _languages.Count;
                    }
                    _languages.Add(fileName);
                }
                catch
                {
                    // ignored
                }
            }
            if (comboBoxDictionaries.Items.Count > 0 && idx < comboBoxDictionaries.Items.Count)
            {
                comboBoxDictionaries.SelectedIndex = idx;
            }
        }

        private void DoNotBreakAfterListEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void comboBoxDictionaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = comboBoxDictionaries.SelectedIndex;
            if (idx >= 0)
            {
                _noBreakAfterList = new List<NoBreakAfterItem>();
                var doc = new XmlDocument();
                doc.Load(_languages[idx]);
                foreach (XmlNode node in doc.DocumentElement.SelectNodes("Item"))
                {
                    if (!string.IsNullOrEmpty(node.InnerText))
                    {
                        if (node.Attributes["RegEx"] != null && node.Attributes["RegEx"].InnerText.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            var r = new Regex(node.InnerText, RegexOptions.Compiled);
                            _noBreakAfterList.Add(new NoBreakAfterItem(r, node.InnerText));
                        }
                        else
                        {
                            _noBreakAfterList.Add(new NoBreakAfterItem(node.InnerText));
                        }
                    }
                }
                _noBreakAfterList.Sort();
                ShowBreakAfterList(_noBreakAfterList);
            }
        }

        private void ShowBreakAfterList(List<NoBreakAfterItem> noBreakAfterList)
        {
            listBoxNoBreakAfter.Items.Clear();
            foreach (var item in noBreakAfterList)
            {
                if (item.Text != null)
                {
                    listBoxNoBreakAfter.Items.Add(item);
                }
            }
        }

        private void buttonRemoveNameEtc_Click(object sender, EventArgs e)
        {
            int first = 0;
            var list = new List<int>();
            foreach (int i in listBoxNoBreakAfter.SelectedIndices)
            {
                list.Add(i);
            }

            if (list.Count > 0)
            {
                first = list[0];
            }

            list.Sort();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                _noBreakAfterList.RemoveAt(list[i]);
            }
            ShowBreakAfterList(_noBreakAfterList);
            if (first >= _noBreakAfterList.Count)
            {
                first = _noBreakAfterList.Count - 1;
            }

            if (first >= 0)
            {
                listBoxNoBreakAfter.SelectedIndex = first;
            }
            comboBoxDictionaries.Enabled = false;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Utilities.ResetNoBreakAfterList();
            int idx = comboBoxDictionaries.SelectedIndex;
            if (idx >= 0)
            {
                var doc = new XmlDocument();
                doc.LoadXml("<NoBreakAfterList />");
                foreach (NoBreakAfterItem item in _noBreakAfterList)
                {
                    XmlNode node = doc.CreateElement("Item");
                    node.InnerText = item.Text;
                    if (item.Regex != null)
                    {
                        XmlAttribute attribute = doc.CreateAttribute("RegEx");
                        attribute.InnerText = true.ToString();
                        node.Attributes.Append(attribute);
                    }
                    doc.DocumentElement.AppendChild(node);
                }
                doc.Save(_languages[idx]);
            }
            DialogResult = DialogResult.OK;
        }

        private void buttonAddNames_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxNoBreakAfter.Text))
            {
                return;
            }

            NoBreakAfterItem item;
            if (radioButtonText.Checked)
            {
                item = new NoBreakAfterItem(textBoxNoBreakAfter.Text);
            }
            else
            {
                if (!RegexUtils.IsValidRegex(textBoxNoBreakAfter.Text))
                {
                    MessageBox.Show(LanguageSettings.Current.General.RegularExpressionIsNotValid);
                    return;
                }
                item = new NoBreakAfterItem(new Regex(textBoxNoBreakAfter.Text), textBoxNoBreakAfter.Text);
            }

            foreach (NoBreakAfterItem nbai in _noBreakAfterList)
            {
                if ((nbai.Regex == null && item.Regex == null || nbai.Regex != null && item.Regex != null) && nbai.Text == item.Text)
                {
                    MessageBox.Show("Text already defined");
                    textBoxNoBreakAfter.Focus();
                    textBoxNoBreakAfter.SelectAll();
                    return;
                }
            }
            _noBreakAfterList.Add(item);
            comboBoxDictionaries.Enabled = false;
            ShowBreakAfterList(_noBreakAfterList);
            for (int i = 0; i < listBoxNoBreakAfter.Items.Count; i++)
            {
                if (listBoxNoBreakAfter.Items[i].ToString() == item.Text)
                {
                    listBoxNoBreakAfter.SelectedIndex = i;
                    return;
                }
            }
            textBoxNoBreakAfter.Text = string.Empty;
        }

        private void RadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (sender == radioButtonRegEx)
            {
                textBoxNoBreakAfter.ContextMenuStrip = FindReplaceDialogHelper.GetRegExContextMenu(textBoxNoBreakAfter);
            }
            else
            {
                textBoxNoBreakAfter.ContextMenuStrip = null;
            }
        }

        private void listBoxNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = listBoxNoBreakAfter.SelectedIndex;
            if (idx >= 0 && idx < _noBreakAfterList.Count)
            {
                NoBreakAfterItem item = _noBreakAfterList[idx];
                textBoxNoBreakAfter.Text = item.Text;
                bool isRegEx = item.Regex != null;
                radioButtonRegEx.Checked = isRegEx;
                radioButtonText.Checked = !isRegEx;
            }
        }

        private void textBoxNoBreakAfter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonAddNames_Click(sender, e);
            }
        }

    }
}

using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class WordLists : Form
    {
        private string _listBoxSearchString = string.Empty;
        private DateTime _listBoxSearchStringLastUsed = DateTime.UtcNow;
        private List<string> _wordListNames = new List<string>();
        private List<string> _userWordList = new List<string>();
        private OcrFixReplaceList _ocrFixReplaceList;

        public WordLists()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.Settings;
            groupBoxWordLists.Text = language.WordLists;
            labelWordListLanguage.Text = language.Language;
            comboBoxWordListLanguage.Left = labelWordListLanguage.Left + labelWordListLanguage.Width + 4;
            groupBoxNamesIgonoreLists.Text = language.NamesIgnoreLists;
            groupBoxUserWordList.Text = language.UserWordList;
            groupBoxOcrFixList.Text = language.OcrFixList;
            buttonRemoveNameEtc.Text = language.Remove;
            buttonRemoveUserWord.Text = language.Remove;
            buttonRemoveOcrFix.Text = language.Remove;
            buttonAddNames.Text = language.AddName;
            buttonAddUserWord.Text = language.AddWord;
            buttonAddOcrFix.Text = language.AddPair;
            groupBoxWordListLocation.Text = language.Location;
            checkBoxNamesOnline.Text = language.UseOnlineNames;
            linkLabelOpenDictionaryFolder.Text = LanguageSettings.Current.GetDictionaries.OpenDictionariesFolder;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            InitComboBoxWordListLanguages();
            labelStatus.Text = string.Empty;

            var wordListSettings = Configuration.Settings.WordLists;
            checkBoxNamesOnline.Checked = wordListSettings.UseOnlineNames;
            textBoxNamesOnline.Text = wordListSettings.NamesUrl;

            Text = LanguageSettings.Current.Settings.WordLists;
        }

        private void InitComboBoxWordListLanguages()
        {
            //Examples: da_DK_user.xml, eng_OCRFixReplaceList.xml, en_names.xml
            var dir = Utilities.DictionaryFolder;
            if (Directory.Exists(dir))
            {
                var cultures = new List<CultureInfo>();
                // Specific culture e.g: en-US, en-GB...
                foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
                {
                    var seFile = Path.Combine(dir, culture.Name.Replace('-', '_') + "_se.xml");
                    var userFile = Path.Combine(dir, culture.Name.Replace('-', '_') + "_user.xml");
                    if (File.Exists(seFile) || File.Exists(userFile))
                    {
                        if (!cultures.Contains(culture))
                        {
                            cultures.Add(culture);
                        }
                    }
                }

                // Neutral culture e.g: "en" for all (en-US, en-GB, en-JM...)
                foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                {
                    var ocrFixGeneralFile = Path.Combine(dir, culture.GetThreeLetterIsoLanguageName() + "_OCRFixReplaceList.xml");
                    var ocrFixUserFile = Path.Combine(dir, culture.GetThreeLetterIsoLanguageName() + "_OCRFixReplaceList_User.xml");
                    var namesFile = Path.Combine(dir, culture.TwoLetterISOLanguageName + "_names.xml");
                    var seFile = Path.Combine(dir, culture.Name.Replace('-', '_') + "_se.xml");
                    if (File.Exists(ocrFixGeneralFile) || File.Exists(ocrFixUserFile) || File.Exists(namesFile) || File.Exists(seFile))
                    {
                        var alreadyInList = false;
                        foreach (var ci in cultures)
                        {
                            // If culture is already added to the list, it doesn't matter if it's "culture specific" do not re-add.
                            if (ci.GetThreeLetterIsoLanguageName().Equals(culture.GetThreeLetterIsoLanguageName(), StringComparison.Ordinal))
                            {
                                alreadyInList = true;
                                break;
                            }
                        }
                        if (!alreadyInList)
                        {
                            cultures.Add(culture);
                        }
                    }
                }

                // English is the default selected language
                Configuration.Settings.WordLists.LastLanguage = Configuration.Settings.WordLists.LastLanguage ?? "en-US";
                comboBoxWordListLanguage.BeginUpdate();
                var list = new List<Settings.ComboBoxLanguage>(cultures.Count);
                var idx = 0;
                for (var index = 0; index < cultures.Count; index++)
                {
                    var ci = cultures[index];
                    list.Add(new Settings.ComboBoxLanguage { CultureInfo = ci });
                    if (ci.Name.Equals(Configuration.Settings.WordLists.LastLanguage, StringComparison.Ordinal))
                    {
                        idx = index;
                    }
                }
                comboBoxWordListLanguage.Items.AddRange(list.ToArray<object>());
                if (comboBoxWordListLanguage.Items.Count > 0)
                {
                    comboBoxWordListLanguage.SelectedIndex = idx;
                }
                comboBoxWordListLanguage.EndUpdate();
            }
            else
            {
                groupBoxWordLists.Enabled = false;
            }
        }

        private async void ComboBoxWordListLanguageSelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemoveNameEtc.Enabled = false;
            buttonAddNames.Enabled = false;
            buttonRemoveUserWord.Enabled = false;
            buttonAddUserWord.Enabled = false;
            buttonRemoveOcrFix.Enabled = false;
            buttonAddOcrFix.Enabled = false;

            listViewNames.BeginUpdate();
            listBoxUserWordLists.BeginUpdate();
            listBoxOcrFixList.BeginUpdate();
            
            listViewNames.Items.Clear();
            listBoxUserWordLists.Items.Clear();
            listBoxOcrFixList.Items.Clear();
            if (comboBoxWordListLanguage.Items.Count > 0 && comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] is Settings.ComboBoxLanguage)
            {
                var language = GetCurrentWordListLanguage();

                buttonAddNames.Enabled = true;
                buttonAddUserWord.Enabled = true;
                buttonAddOcrFix.Enabled = true;

                // user word list
                LoadUserWords(language, true);

                // OCR fix words
                LoadOcrFixList(true);

                await LoadNamesAsync(language, true);
            }
            
            listViewNames.EndUpdate();
            listBoxUserWordLists.EndUpdate();
            listBoxOcrFixList.EndUpdate();
        }

        private void LoadOcrFixList(bool reloadListBox)
        {
            if (comboBoxWordListLanguage.Items.Count == 0 || !(comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] is Settings.ComboBoxLanguage cb))
            {
                return;
            }

            _ocrFixReplaceList = OcrFixReplaceList.FromLanguageId(cb.CultureInfo.GetThreeLetterIsoLanguageName());
            if (reloadListBox)
            {
                listBoxOcrFixList.BeginUpdate();
                listBoxOcrFixList.Items.Clear();
                // ReSharper disable once CoVariantArrayConversion
                listBoxOcrFixList.Items.AddRange(_ocrFixReplaceList.WordReplaceList.Select(p => p.Key + " --> " + p.Value).ToArray());
                // ReSharper disable once CoVariantArrayConversion
                listBoxOcrFixList.Items.AddRange(_ocrFixReplaceList.PartialLineWordBoundaryReplaceList.Select(p => p.Key + " --> " + p.Value).ToArray());
                listBoxOcrFixList.Sorted = true;
                listBoxOcrFixList.EndUpdate();
            }
        }

        private void LoadUserWords(string language, bool reloadListBox)
        {
            _userWordList = new List<string>();
            Utilities.LoadUserWordList(_userWordList, language);
            _userWordList.Sort();

            if (reloadListBox)
            {
                listBoxUserWordLists.BeginUpdate();
                listBoxUserWordLists.Items.Clear();
                listBoxUserWordLists.Items.AddRange(_userWordList.ToArray<object>());
                listBoxUserWordLists.EndUpdate();
            }
        }

        private async Task LoadNamesAsync(string language, bool reloadListBox)
        {
            // update all names
            _wordListNames = await GetNamesSortedFromSourceAsync().ConfigureAwait(true);
            
            if (reloadListBox)
            {
                listViewNames.BeginUpdate();
                listViewNames.Items.Clear();
                var list = new List<ListViewItem>(_wordListNames.Count);
                foreach (var item in _wordListNames)
                {
                    list.Add(new ListViewItem(item));
                }
                listViewNames.Items.AddRange(list.ToArray());
                listViewNames.EndUpdate();
            }

            async Task<List<string>> GetNamesSortedFromSourceAsync()
            {
                var nameList = await NameList.CreateAsync(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl)
                    .ConfigureAwait(false);
                var names = nameList.GetAllNames();
                names.Sort();
                return names;
            }
        }

        private string GetCurrentWordListLanguage()
        {
            var idx = comboBoxWordListLanguage.SelectedIndex;
            if (idx < 0)
            {
                return null;
            }

            var cb = comboBoxWordListLanguage.Items[idx] as Settings.ComboBoxLanguage;
            return cb?.CultureInfo.Name.Replace('-', '_');
        }

        private async void ButtonAddNamesClick(object sender, EventArgs e)
        {
            var languageIndex = comboBoxWordListLanguage.SelectedIndex;
            if (languageIndex < 0)
            {
                return;
            }

            var language = GetCurrentWordListLanguage();
            var text = textBoxNameEtc.Text.RemoveControlCharacters().Trim();
            if (!string.IsNullOrEmpty(language) && text.Length > 1 && !_wordListNames.Contains(text))
            {
                // adds new name
                var nameList = await NameList.CreateAsync(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
                nameList.Add(text);
                
                // reload
                await LoadNamesAsync(language, true).ConfigureAwait(true);
                
                labelStatus.Text = string.Format(LanguageSettings.Current.Settings.WordAddedX, text);
                textBoxNameEtc.Text = string.Empty;
                textBoxNameEtc.Focus();
                for (var i = 0; i < listViewNames.Items.Count; i++)
                {
                    var item = listViewNames.Items[i];
                    if (item.Text == text)
                    {
                        item.Selected = true;
                        item.Focused = true;
                        var top = i - 5;
                        if (top < 0)
                        {
                            top = 0;
                        }

                        listViewNames.EnsureVisible(top);
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show(LanguageSettings.Current.Settings.WordAlreadyExists, LanguageSettings.Current.General.Title, MessageBoxButtons.OK);
            }
        }

        private void ListViewNamesSelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemoveNameEtc.Enabled = listViewNames.SelectedItems.Count >= 1;
        }

        private async void ButtonRemoveNameEtcClick(object sender, EventArgs e)
        {
            if (listViewNames.SelectedItems.Count == 0)
            {
                return;
            }

            var language = GetCurrentWordListLanguage();
            var index = listViewNames.SelectedItems[0].Index;
            var text = listViewNames.Items[index].Text;
            var itemsToRemoveCount = listViewNames.SelectedIndices.Count;
            if (!string.IsNullOrEmpty(language) && index >= 0)
            {
                DialogResult result;
                if (itemsToRemoveCount == 1)
                {
                    result = MessageBox.Show(string.Format(LanguageSettings.Current.Settings.RemoveX, text), LanguageSettings.Current.General.Title, MessageBoxButtons.YesNoCancel);
                }
                else
                {
                    result = MessageBox.Show(string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, itemsToRemoveCount), LanguageSettings.Current.General.Title, MessageBoxButtons.YesNoCancel);
                }

                if (result == DialogResult.Yes)
                {
                    var removeCount = 0;
                    var namesList = await NameList.CreateAsync(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl)
                        .ConfigureAwait(true);
                    for (var idx = listViewNames.SelectedIndices.Count - 1; idx >= 0; idx--)
                    {
                        index = listViewNames.SelectedIndices[idx];
                        text = listViewNames.Items[index].Text;
                        namesList.Remove(text);
                        removeCount++;
                        listViewNames.Items.RemoveAt(index);
                    }

                    if (removeCount > 0)
                    {
                        await LoadNamesAsync(language, true).ConfigureAwait(true); // reload

                        if (index < listViewNames.Items.Count)
                        {
                            listViewNames.Items[index].Selected = true;
                        }
                        else if (listViewNames.Items.Count > 0)
                        {
                            listViewNames.Items[index - 1].Selected = true;
                        }

                        listViewNames.Focus();

                        buttonRemoveNameEtc.Enabled = false;
                        return;
                    }

                    if (removeCount < itemsToRemoveCount && Configuration.Settings.WordLists.UseOnlineNames && !string.IsNullOrEmpty(Configuration.Settings.WordLists.NamesUrl))
                    {
                        MessageBox.Show(LanguageSettings.Current.Settings.CannotUpdateNamesOnline, LanguageSettings.Current.General.Title, MessageBoxButtons.OK);
                        return;
                    }

                    if (removeCount == 0)
                    {
                        MessageBox.Show(LanguageSettings.Current.Settings.WordNotFound, LanguageSettings.Current.General.Title, MessageBoxButtons.OK);
                    }
                }
            }
        }

        private void TextBoxNameEtcKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ButtonAddNamesClick(null, null);
            }
        }

        private void ButtonAddUserWordClick(object sender, EventArgs e)
        {
            var languageIndex = comboBoxWordListLanguage.SelectedIndex;
            if (languageIndex < 0)
            {
                return;
            }

            var language = GetCurrentWordListLanguage();
            var text = textBoxUserWord.Text.RemoveControlCharacters().Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(language) && text.Length > 0 && !_userWordList.Contains(text))
            {
                Utilities.AddToUserDictionary(text, language);
                LoadUserWords(language, true);
                labelStatus.Text = string.Format(LanguageSettings.Current.Settings.WordAddedX, text);
                textBoxUserWord.Text = string.Empty;
                textBoxUserWord.Focus();

                for (var i = 0; i < listBoxUserWordLists.Items.Count; i++)
                {
                    if (listBoxUserWordLists.Items[i].ToString() == text)
                    {
                        listBoxUserWordLists.SelectedIndex = i;
                        var top = i - 5;
                        if (top < 0)
                        {
                            top = 0;
                        }

                        listBoxUserWordLists.TopIndex = top;
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show(LanguageSettings.Current.Settings.WordAlreadyExists, LanguageSettings.Current.General.Title, MessageBoxButtons.OK);
            }
        }

        private void TextBoxUserWordKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ButtonAddUserWordClick(null, null);
            }
        }

        private void ButtonRemoveUserWordClick(object sender, EventArgs e)
        {
            if (listBoxUserWordLists.SelectedIndices.Count == 0)
            {
                return;
            }

            var language = GetCurrentWordListLanguage();
            var index = listBoxUserWordLists.SelectedIndex;
            var itemsToRemoveCount = listBoxUserWordLists.SelectedIndices.Count;
            var text = listBoxUserWordLists.Items[index].ToString();
            if (!string.IsNullOrEmpty(language) && index >= 0)
            {
                DialogResult result;
                if (itemsToRemoveCount == 1)
                {
                    result = MessageBox.Show(string.Format(LanguageSettings.Current.Settings.RemoveX, text), LanguageSettings.Current.General.Title, MessageBoxButtons.YesNoCancel);
                }
                else
                {
                    result = MessageBox.Show(string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, itemsToRemoveCount), LanguageSettings.Current.General.Title, MessageBoxButtons.YesNoCancel);
                }

                if (result == DialogResult.Yes)
                {
                    var removeCount = 0;
                    var words = new List<string>();
                    var userWordFileName = Utilities.LoadUserWordList(words, language);

                    for (var idx = listBoxUserWordLists.SelectedIndices.Count - 1; idx >= 0; idx--)
                    {
                        index = listBoxUserWordLists.SelectedIndices[idx];
                        text = listBoxUserWordLists.Items[index].ToString();

                        if (words.Contains(text))
                        {
                            words.Remove(text);
                            removeCount++;
                        }
                        listBoxUserWordLists.Items.RemoveAt(index);
                    }

                    if (removeCount > 0)
                    {
                        words.Sort();
                        var doc = new XmlDocument();
                        doc.Load(userWordFileName);

                        if (doc.DocumentElement == null)
                        {
                            return;
                        }

                        doc.DocumentElement.RemoveAll();
                        foreach (var word in words)
                        {
                            XmlNode node = doc.CreateElement("word");
                            node.InnerText = word;
                            doc.DocumentElement?.AppendChild(node);
                        }
                        doc.Save(userWordFileName);
                        LoadUserWords(language, false); // reload
                        buttonRemoveUserWord.Enabled = false;

                        if (index < listBoxUserWordLists.Items.Count)
                        {
                            listBoxUserWordLists.SelectedIndex = index;
                        }
                        else if (listBoxUserWordLists.Items.Count > 0)
                        {
                            listBoxUserWordLists.SelectedIndex = index - 1;
                        }

                        listBoxUserWordLists.Focus();
                        return;
                    }

                    if (removeCount < itemsToRemoveCount)
                    {
                        MessageBox.Show(LanguageSettings.Current.Settings.WordNotFound, LanguageSettings.Current.General.Title, MessageBoxButtons.OK);
                    }
                }
            }
        }

        private void ListBoxUserWordListsSelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemoveUserWord.Enabled = listBoxUserWordLists.SelectedIndex >= 0;
        }

        private void ButtonAddOcrFixClick(object sender, EventArgs e)
        {
            var key = textBoxOcrFixKey.Text.RemoveControlCharacters().Trim();
            var value = textBoxOcrFixValue.Text.RemoveControlCharacters().Trim();
            if (key.Length == 0 || value.Length == 0 || key == value || Utilities.IsInteger(key))
            {
                return;
            }

            if (comboBoxWordListLanguage.Items.Count == 0 || !(comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] is Settings.ComboBoxLanguage))
            {
                return;
            }

            var added = _ocrFixReplaceList.AddWordOrPartial(key, value);
            if (!added)
            {
                MessageBox.Show(LanguageSettings.Current.Settings.WordAlreadyExists, LanguageSettings.Current.General.Title, MessageBoxButtons.OK);
                return;
            }

            LoadOcrFixList(true);
            textBoxOcrFixKey.Text = string.Empty;
            textBoxOcrFixValue.Text = string.Empty;
            textBoxOcrFixKey.Focus();

            for (var i = 0; i < listBoxOcrFixList.Items.Count; i++)
            {
                if (listBoxOcrFixList.Items[i].ToString() == key + " --> " + value)
                {
                    listBoxOcrFixList.SelectedIndex = i;
                    var top = i - 5;
                    if (top < 0)
                    {
                        top = 0;
                    }

                    listBoxOcrFixList.TopIndex = top;
                    break;
                }
            }
        }

        private void ListBoxOcrFixListSelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemoveOcrFix.Enabled = listBoxOcrFixList.SelectedIndex >= 0;
        }
        private void TextBoxOcrFixValueKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ButtonAddOcrFixClick(null, null);
            }
        }

        private void listViewNames_DoubleClick(object sender, EventArgs e)
        {
            if (listViewNames.SelectedItems.Count == 0)
            {
                return;
            }

            var idx = listViewNames.SelectedItems[0].Index;
            if (idx >= 0)
            {
                textBoxNameEtc.Text = listViewNames.Items[idx].Text;
            }
        }

        private void listBoxUserWordLists_DoubleClick(object sender, EventArgs e)
        {
            var idx = listBoxUserWordLists.SelectedIndex;
            if (idx >= 0)
            {
                textBoxUserWord.Text = (string)listBoxUserWordLists.Items[idx];
            }
        }

        private void listBoxOcrFixList_DoubleClick(object sender, EventArgs e)
        {
            var idx = listBoxOcrFixList.SelectedIndex;
            if (idx >= 0)
            {
                var text = (string)listBoxOcrFixList.Items[idx];
                var splitIdx = text.IndexOf(" --> ", StringComparison.Ordinal);
                if (splitIdx > 0)
                {
                    textBoxOcrFixKey.Text = text.Substring(0, splitIdx);
                    textBoxOcrFixValue.Text = text.Remove(0, splitIdx + " --> ".Length);
                }
            }
        }

        private void listViewNames_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape ||
                e.KeyCode == Keys.Tab ||
                e.KeyCode == Keys.Return ||
                e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.Up ||
                e.KeyCode == Keys.PageDown ||
                e.KeyCode == Keys.PageUp ||
                e.KeyCode == Keys.None ||
                e.KeyCode == UiUtil.HelpKeys ||
                e.KeyCode == Keys.Home ||
                e.KeyCode == Keys.End)
            {
                return;
            }

            e.SuppressKeyPress = true;
            if (TimeSpan.FromTicks(_listBoxSearchStringLastUsed.Ticks).TotalMilliseconds + 1800 <
                TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds)
            {
                _listBoxSearchString = string.Empty;
            }

            if (e.KeyCode == Keys.Delete)
            {
                ButtonRemoveNameEtcClick(null, null);
                return;
            }

            _listBoxSearchString += e.KeyCode.ToString();
            _listBoxSearchStringLastUsed = DateTime.UtcNow;
            FindAndSelectListViewItem(sender as ListView);
        }

        private void FindAndSelectListViewItem(ListView listView)
        {
            if (listView == null)
            {
                return;
            }

            listView.SelectedItems.Clear();
            var i = 0;
            foreach (ListViewItem s in listView.Items)
            {
                if (s.Text.StartsWith(_listBoxSearchString, StringComparison.OrdinalIgnoreCase))
                {
                    listView.Items[i].Selected = true;
                    listView.EnsureVisible(i);
                    break;
                }
                i++;
            }
        }

        private void ListBoxSearchReset(object sender, EventArgs e)
        {
            _listBoxSearchString = string.Empty;
        }

        private void linkLabelOpenDictionaryFolder_LinkClicked(object sender, EventArgs e)
        {
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            UiUtil.OpenFolder(dictionaryFolder);
        }

        private void ButtonRemoveOcrFixClick(object sender, EventArgs e)
        {
            var languageIndex = comboBoxWordListLanguage.SelectedIndex;
            if (languageIndex < 0)
            {
                return;
            }

            if (!(comboBoxWordListLanguage.Items[languageIndex] is Settings.ComboBoxLanguage))
            {
                return;
            }

            if (listBoxOcrFixList.SelectedIndices.Count == 0)
            {
                return;
            }

            var itemsToRemoveCount = listBoxOcrFixList.SelectedIndices.Count;

            var index = listBoxOcrFixList.SelectedIndex;
            var text = listBoxOcrFixList.Items[index].ToString();
            var key = text.Substring(0, text.IndexOf(" --> ", StringComparison.Ordinal));

            if (_ocrFixReplaceList.WordReplaceList.ContainsKey(key) || _ocrFixReplaceList.PartialLineWordBoundaryReplaceList.ContainsKey(key))
            {
                DialogResult result;
                if (itemsToRemoveCount == 1)
                {
                    result = MessageBox.Show(string.Format(LanguageSettings.Current.Settings.RemoveX, text), LanguageSettings.Current.General.Title, MessageBoxButtons.YesNoCancel);
                }
                else
                {
                    result = MessageBox.Show(string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, itemsToRemoveCount), LanguageSettings.Current.General.Title, MessageBoxButtons.YesNoCancel);
                }

                if (result == DialogResult.Yes)
                {
                    listBoxOcrFixList.BeginUpdate();
                    for (var idx = listBoxOcrFixList.SelectedIndices.Count - 1; idx >= 0; idx--)
                    {
                        index = listBoxOcrFixList.SelectedIndices[idx];
                        text = listBoxOcrFixList.Items[index].ToString();
                        key = text.Substring(0, text.IndexOf(" --> ", StringComparison.Ordinal));

                        if (_ocrFixReplaceList.WordReplaceList.ContainsKey(key) || _ocrFixReplaceList.PartialLineWordBoundaryReplaceList.ContainsKey(key))
                        {
                            _ocrFixReplaceList.RemoveWordOrPartial(key);
                        }
                        listBoxOcrFixList.Items.RemoveAt(index);
                    }
                    listBoxOcrFixList.EndUpdate();

                    LoadOcrFixList(false);
                    buttonRemoveOcrFix.Enabled = false;

                    if (index < listBoxOcrFixList.Items.Count)
                    {
                        listBoxOcrFixList.SelectedIndex = index;
                    }
                    else if (listBoxOcrFixList.Items.Count > 0)
                    {
                        listBoxOcrFixList.SelectedIndex = index - 1;
                    }

                    listBoxOcrFixList.Focus();
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var wordListSettings = Configuration.Settings.WordLists;
            wordListSettings.UseOnlineNames = checkBoxNamesOnline.Checked;
            wordListSettings.NamesUrl = textBoxNamesOnline.Text;
            if (comboBoxWordListLanguage.Items.Count > 0 && comboBoxWordListLanguage.SelectedIndex >= 0)
            {
                if (comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] is Settings.ComboBoxLanguage ci)
                {
                    Configuration.Settings.WordLists.LastLanguage = ci.CultureInfo.Name;
                }
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void WordLists_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void listBoxUserWordLists_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape ||
                e.KeyCode == Keys.Tab ||
                e.KeyCode == Keys.Return ||
                e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.Up ||
                e.KeyCode == Keys.PageDown ||
                e.KeyCode == Keys.PageUp ||
                e.KeyCode == Keys.None ||
                e.KeyCode == UiUtil.HelpKeys ||
                e.KeyCode == Keys.Home ||
                e.KeyCode == Keys.End)
            {
                return;
            }

            e.SuppressKeyPress = true;
            if (TimeSpan.FromTicks(_listBoxSearchStringLastUsed.Ticks).TotalMilliseconds + 1800 <
                TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds)
            {
                _listBoxSearchString = string.Empty;
            }

            if (e.KeyCode == Keys.Delete)
            {
                ButtonRemoveUserWordClick(null, null);
                return;
            }

            _listBoxSearchString += e.KeyCode.ToString();
            _listBoxSearchStringLastUsed = DateTime.UtcNow;
            FindAndSelectListViewItem(sender as ListView);
        }

        private void listBoxOcrFixList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape ||
                e.KeyCode == Keys.Tab ||
                e.KeyCode == Keys.Return ||
                e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.Up ||
                e.KeyCode == Keys.PageDown ||
                e.KeyCode == Keys.PageUp ||
                e.KeyCode == Keys.None ||
                e.KeyCode == UiUtil.HelpKeys ||
                e.KeyCode == Keys.Home ||
                e.KeyCode == Keys.End)
            {
                return;
            }

            e.SuppressKeyPress = true;
            if (TimeSpan.FromTicks(_listBoxSearchStringLastUsed.Ticks).TotalMilliseconds + 1800 <
                TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds)
            {
                _listBoxSearchString = string.Empty;
            }

            if (e.KeyCode == Keys.Delete)
            {
                ButtonRemoveOcrFixClick(null, null);
                return;
            }

            _listBoxSearchString += e.KeyCode.ToString();
            _listBoxSearchStringLastUsed = DateTime.UtcNow;
            FindAndSelectListViewItem(sender as ListView);
        }
    }
}

﻿using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic.Dictionaries;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AddToNamesList : PositionAndSizeForm
    {
        private LanguageStructure.Main _language;
        private Subtitle _subtitle;

        public AddToNamesList()
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.AddToNames.Title;
            labelDescription.Text = Configuration.Settings.Language.AddToNames.Description;
            labelLanguage.Text = Configuration.Settings.Language.SpellCheck.Language;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            FixLargeFonts();
        }

        public string NewName { get; private set; }

        private void FixLargeFonts()
        {
            if (labelDescription.Left + labelDescription.Width + 5 > Width)
                Width = labelDescription.Left + labelDescription.Width + 5;
            Utilities.FixLargeFonts(this, buttonOK);
        }

        private void CapFirstCharIfNotEmpty(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Trim().TrimEnd('.', '!', '?');
                if (text.Length > 1)
                    textBoxAddName.Text = char.ToUpper(textBoxAddName.Text[0]) + textBoxAddName.Text.Substring(1);
            }
        }

        public void Initialize(Subtitle subtitle, string text)
        {
            _subtitle = subtitle;

            CapFirstCharIfNotEmpty(text);

            comboBoxDictionaries.Items.Clear();
            string languageName = Utilities.AutoDetectLanguageName(Configuration.Settings.General.SpellCheckLanguage, _subtitle);
            foreach (string name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
                if (name.Contains("[" + languageName + "]"))
                    comboBoxDictionaries.SelectedIndex = comboBoxDictionaries.Items.Count - 1;
            }
        }

        internal void Initialize(Subtitle subtitle, string hunspellName, string text)
        {
            _subtitle = subtitle;

            CapFirstCharIfNotEmpty(text);

            comboBoxDictionaries.Items.Clear();
            foreach (string name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
                if (hunspellName != null && name.Equals(hunspellName, StringComparison.OrdinalIgnoreCase))
                    comboBoxDictionaries.SelectedIndex = comboBoxDictionaries.Items.Count - 1;
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxAddName.Text))
            {
                return;
            }

            NewName = textBoxAddName.Text.Trim();
            string languageName = null;
            _language = Configuration.Settings.Language.Main;

            if (!string.IsNullOrEmpty(Configuration.Settings.General.SpellCheckLanguage))
            {
                languageName = Configuration.Settings.General.SpellCheckLanguage;
            }
            else
            {
                List<string> list = Utilities.GetDictionaryLanguages();
                if (list.Count > 0)
                {
                    string name = list[0];
                    int start = name.LastIndexOf('[');
                    int end = name.LastIndexOf(']');
                    if (start > 0 && end > start)
                    {
                        start++;
                        name = name.Substring(start, end - start);
                        languageName = name;
                    }
                    else
                    {
                        MessageBox.Show(string.Format(_language.InvalidLanguageNameX, name));
                        return;
                    }
                }
            }

            languageName = Utilities.AutoDetectLanguageName(languageName, _subtitle);
            if (comboBoxDictionaries.Items.Count > 0)
            {
                string name = comboBoxDictionaries.SelectedItem.ToString();
                int start = name.LastIndexOf('[');
                int end = name.LastIndexOf(']');
                if (start >= 0 && end > start)
                {
                    start++;
                    name = name.Substring(start, end - start);
                    languageName = name;
                }
            }

            if (string.IsNullOrEmpty(languageName))
                languageName = "en_US";

            var namesList = new NamesList(Configuration.DictionariesFolder, languageName, Configuration.Settings.WordLists.UseOnlineNamesEtc, Configuration.Settings.WordLists.NamesEtcUrl);
            if (namesList.Add(textBoxAddName.Text))
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;
        }

    }
}
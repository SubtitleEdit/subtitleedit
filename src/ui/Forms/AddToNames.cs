﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AddToNameList : PositionAndSizeForm
    {
        private Subtitle _subtitle;

        public AddToNameList()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.AddToNames.Title;
            labelDescription.Text = LanguageSettings.Current.AddToNames.Description;
            labelLanguage.Text = LanguageSettings.Current.SpellCheck.Language;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            FixLargeFonts();
        }

        public string NewName { get; private set; }

        private void FixLargeFonts()
        {
            if (labelDescription.Left + labelDescription.Width + 5 > Width)
            {
                Width = labelDescription.Left + labelDescription.Width + 5;
            }

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public void Initialize(Subtitle subtitle, string text)
        {
            _subtitle = subtitle;

            if (!string.IsNullOrEmpty(text))
            {
                textBoxAddName.Text = text.Trim().TrimEnd('.', '!', '?', ',');
                if (textBoxAddName.Text.Length > 1)
                {
                    textBoxAddName.Text = char.ToUpper(textBoxAddName.Text[0]) + textBoxAddName.Text.Substring(1);
                }
            }

            comboBoxDictionaries.Items.Clear();
            var languageName = LanguageAutoDetect.AutoDetectLanguageName(Configuration.Settings.General.SpellCheckLanguage, _subtitle);
            var selIndex = -1;
            var dictionaries = Utilities.GetDictionaryLanguagesCultureNeutral();
            if (dictionaries.Count == 0)
            {
                textBoxAddName.Enabled = false;
                buttonOK.Enabled = false;
                MessageBox.Show($"No spell check dictionaries found in {Configuration.DictionariesDirectory}");
                return;
            }

            for (var index = 0; index < dictionaries.Count; index++)
            {
                var name = dictionaries[index];
                comboBoxDictionaries.Items.Add(name);
                if (selIndex == -1 && languageName.Length > 1 && name.Contains("[" + languageName.Substring(0, 2) + "]"))
                {
                    selIndex = index;
                }
            }
            comboBoxDictionaries.SelectedIndex = selIndex >= 0 ? selIndex : 0;
        }

        internal void Initialize(Subtitle subtitle, string hunspellName, string text)
        {
            _subtitle = subtitle;

            if (!string.IsNullOrEmpty(text))
            {
                textBoxAddName.Text = text.Trim().TrimEnd('.', '!', '?');
                if (textBoxAddName.Text.Length > 1)
                {
                    textBoxAddName.Text = char.ToUpper(textBoxAddName.Text[0]) + textBoxAddName.Text.Substring(1);
                }
            }

            comboBoxDictionaries.Items.Clear();
            foreach (var name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
                if (hunspellName != null && name.Equals(hunspellName, StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxDictionaries.SelectedIndex = comboBoxDictionaries.Items.Count - 1;
                }
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxAddName.Text) || comboBoxDictionaries.SelectedIndex < 0)
            {
                return;
            }

            NewName = textBoxAddName.Text.RemoveControlCharacters().Trim();
            string languageName;
            try
            {
                languageName = comboBoxDictionaries.Items[comboBoxDictionaries.SelectedIndex].ToString();
                languageName = languageName.Substring(languageName.LastIndexOf("[", StringComparison.Ordinal)).TrimStart('[').TrimEnd(']');
            }
            catch
            {
                languageName = "en";
            }

            var nameList = new NameList(Configuration.DictionariesDirectory, languageName, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            DialogResult = nameList.Add(textBoxAddName.Text) ? DialogResult.OK : DialogResult.Cancel;
        }

        private void AddToNameList_Shown(object sender, EventArgs e)
        {
            textBoxAddName.Focus();
        }
    }
}

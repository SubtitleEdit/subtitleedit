using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AddToUserDic : Form
    {
        public AddToUserDic()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.AddToUserDictionary.Title;
            labelDescription.Text = LanguageSettings.Current.AddToUserDictionary.Description;
            labelLanguage.Text = Configuration.Settings.General.Language;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void AddToUserDic_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            NewWord = textBoxAddName.Text.RemoveControlCharacters().Trim().ToLowerInvariant();
            if (NewWord.Length == 0)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            string language = comboBoxDictionaries.Text;
            if (language.IndexOf('[') >= 0)
            {
                language = language.Substring(language.IndexOf('[')).TrimStart('[');
            }

            if (language.IndexOf(']') > 0)
            {
                language = language.Substring(0, language.IndexOf(']'));
            }

            var userWordList = new List<string>();

            Utilities.LoadUserWordList(userWordList, language);
            if (!string.IsNullOrEmpty(language) && NewWord.Length > 0 && !userWordList.Contains(NewWord))
            {
                Utilities.AddToUserDictionary(NewWord, language);
                DialogResult = DialogResult.OK;
                return;
            }
            DialogResult = DialogResult.Cancel;
        }

        internal void Initialize(string hunspellName, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                textBoxAddName.Text = text.Trim().TrimEnd('.', '!', '?');
            }

            comboBoxDictionaries.Items.Clear();
            foreach (string name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
                if (hunspellName != null && name.Equals(hunspellName, StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxDictionaries.SelectedIndex = comboBoxDictionaries.Items.Count - 1;
                }
            }
        }

        public string NewWord { get; private set; }
    }
}

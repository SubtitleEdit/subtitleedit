using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

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
            {
                Width = labelDescription.Left + labelDescription.Width + 5;
            }

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public void Initialize(Subtitle subtitle, string nameCandidate)
        {
            string language = LanguageAutoDetect.AutoDetectLanguageName(Configuration.Settings.General.SpellCheckLanguage, subtitle);
            Initialize(subtitle, $"[{language}]", nameCandidate);
        }

        internal void Initialize(Subtitle subtitle, string selDictionaryName, string nameCandidate)
        {
            _subtitle = subtitle;

            if (!string.IsNullOrEmpty(nameCandidate))
            {
                nameCandidate = nameCandidate.RemoveControlCharacters();
                nameCandidate = nameCandidate.Trim().TrimEnd('.', '!', '?');
                nameCandidate = HtmlUtil.RemoveHtmlTags(nameCandidate, true);

                if (nameCandidate.ContainsLetter())
                {
                    textBoxAddName.Text = nameCandidate.CapitalizeFirstLetter();
                }
            }

            comboBoxDictionaries.BeginUpdate();
            comboBoxDictionaries.Items.Clear();

            // neutral dictionaries is detect using language inside brackets e.g: [en_US]
            bool isHunspell = !selDictionaryName.StartsWith('[');

            int selIndex = -1; 
            var dictionaries = isHunspell ? Utilities.GetDictionaryLanguages() : Utilities.GetDictionaryLanguagesCultureNeutral();
            for (int i = 0; i < dictionaries.Count; i++)
            {
                string dictionary = dictionaries[i];
                comboBoxDictionaries.Items.Add(dictionary);

                // default select index already set
                if (selIndex >= 0 || string.IsNullOrEmpty(selDictionaryName))
                {
                    continue;
                }

                if (isHunspell ?
                    dictionary.Equals(selDictionaryName, StringComparison.OrdinalIgnoreCase)
                    : dictionary.Contains($"[{selDictionaryName}]"))
                {
                    selIndex = i;
                }
            }

            comboBoxDictionaries.SelectedIndex = selIndex >= 0 ? selIndex : 0;
            comboBoxDictionaries.EndUpdate();
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

    }
}

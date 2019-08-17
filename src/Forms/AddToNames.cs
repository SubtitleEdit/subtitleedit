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

        public void Initialize(Subtitle subtitle, string text)
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
            string languageName = LanguageAutoDetect.AutoDetectLanguageName(Configuration.Settings.General.SpellCheckLanguage, _subtitle);
            int selIndex = -1;
            var dictionaries = Utilities.GetDictionaryLanguagesCultureNeutral();
            for (var index = 0; index < dictionaries.Count; index++)
            {
                string name = dictionaries[index];
                comboBoxDictionaries.Items.Add(name);
                if (selIndex == -1 && languageName.Length > 1 &&  name.Contains("[" + languageName.Substring(0, 2) + "]"))
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
            foreach (string name in Utilities.GetDictionaryLanguages())
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

    }
}

using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AddToNamesList : Form
    {
        LanguageStructure.Main _language;
        Subtitle _subtitle;

        public string NewName { get; private set; }

        public AddToNamesList()
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.AddToNames.Title;
            labelDescription.Text = Configuration.Settings.Language.AddToNames.Description;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            if (labelDescription.Left + labelDescription.Width + 5 > Width)
                Width = labelDescription.Left + labelDescription.Width + 5;

            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public void Initialize(Subtitle subtitle, string text)
        {
            _subtitle = subtitle;

            if (!string.IsNullOrEmpty(text))
            {
                textBoxAddName.Text = text.Trim().TrimEnd('.').TrimEnd('!').TrimEnd('?');
                if (textBoxAddName.Text.Length > 1)
                    textBoxAddName.Text = textBoxAddName.Text.Substring(0, 1).ToUpper() + textBoxAddName.Text.Substring(1);
            }

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

            if (!string.IsNullOrEmpty(text))
            {
                textBoxAddName.Text = text.Trim().TrimEnd('.').TrimEnd('!').TrimEnd('?');
                if (textBoxAddName.Text.Length > 1)
                    textBoxAddName.Text = textBoxAddName.Text.Substring(0, 1).ToUpper() + textBoxAddName.Text.Substring(1);
            }

            comboBoxDictionaries.Items.Clear();
            string languageName = Utilities.AutoDetectLanguageName(Configuration.Settings.General.SpellCheckLanguage, _subtitle);
            foreach (string name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
                if (hunspellName != null && name.ToLower() == hunspellName.ToLower())
                    comboBoxDictionaries.SelectedIndex = comboBoxDictionaries.Items.Count - 1;                 
            }
        }


        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (textBoxAddName.Text.Trim().Length > 0)
            {
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
                        int start = name.LastIndexOf("[");
                        int end = name.LastIndexOf("]");
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
                    int start = name.LastIndexOf("[");
                    int end = name.LastIndexOf("]");
                    if (start >= 0 && end > start)
                    {
                        start++;
                        name = name.Substring(start, end - start);
                        languageName = name;
                    }
                }

                if (string.IsNullOrEmpty(languageName))
                    languageName = "en_US";
                if (Utilities.AddWordToLocalNamesEtcList(textBoxAddName.Text, languageName))
                    DialogResult = DialogResult.OK;
                else
                    DialogResult = DialogResult.Cancel;
            }
        }

    }
}

using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class TimedTextNewLanguage : Form
    {
        public class LanguageComboBoxItem
        {
            public string Code { get; set; }
            public string FriendlyName { get; set; }
            public LanguageComboBoxItem(string code, string name)
            {
                Code = code;
                FriendlyName = name;
            }

            public override string ToString()
            {
                return Code + " - " + FriendlyName;
            }
        }

        public string Language { get; set; }

        public TimedTextNewLanguage(List<CultureInfo> moreLanguages, string currentLanguage)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.ChooseLanguage.Title;
            labelLanguage.Text = LanguageSettings.Current.ChooseLanguage.Language;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            comboBoxLanguage.BeginUpdate();
            comboBoxLanguage.Items.Clear();
            int selctedIndex = 0;
            for (int i = 0; i < moreLanguages.Count; i++)
            {
                var language = moreLanguages[i];
                var code = language.TwoLetterISOLanguageName.ToLowerInvariant();
                comboBoxLanguage.Items.Add(new LanguageComboBoxItem(code, language.EnglishName + " / " + language.NativeName));
                if (code == currentLanguage)
                {
                    selctedIndex = i;
                }
            }
            if (comboBoxLanguage.Items.Count > 0)
            {
                comboBoxLanguage.SelectedIndex = selctedIndex;
            }
            comboBoxLanguage.EndUpdate();
        }

        private void TimedTextNewLanguage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                Language = (comboBoxLanguage.SelectedItem as LanguageComboBoxItem).Code;
                DialogResult = DialogResult.OK;
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            Language = (comboBoxLanguage.SelectedItem as LanguageComboBoxItem).Code;
            DialogResult = DialogResult.OK;
        }
    }
}

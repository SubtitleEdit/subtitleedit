using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class DoNotBreakAfterListNew : Form
    {
        private readonly List<CultureInfo> _languages = new List<CultureInfo>();
        public CultureInfo ChosenLanguage { get; set; }

        public DoNotBreakAfterListNew()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.ExportCustomText.New;
            labelChooseLanguage.Text = LanguageSettings.Current.AudioToText.ChooseLanguage;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            var existingLanguages = new List<string>();
            foreach (var fileName in Directory.GetFiles(Configuration.DictionariesDirectory, "*_NoBreakAfterList.xml"))
            {
                try
                {
                    var s = Path.GetFileName(fileName);
                    var languageId = s.Substring(0, s.IndexOf('_'));
                    existingLanguages.Add(languageId);
                }
                catch
                {
                    // ignored
                }
            }

            var lookup = new HashSet<string>();
            comboBoxDictionaries.Items.Clear();
            comboBoxDictionaries.BeginUpdate();
            foreach (var l in Utilities.GetSubtitleLanguageCultures(false))
            {
                if (l.IsNeutralCulture && l.TwoLetterISOLanguageName.Length == 2 && !lookup.Contains(l.TwoLetterISOLanguageName))
                {
                    lookup.Add(l.TwoLetterISOLanguageName);
                    if (!existingLanguages.Contains(l.TwoLetterISOLanguageName))
                    {
                        comboBoxDictionaries.Items.Add(l.DisplayName);
                        _languages.Add(l);
                    }
                }
            }
            comboBoxDictionaries.EndUpdate();
        }

        private void DoNotBreakAfterListNew_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (comboBoxDictionaries.SelectedIndex <= 0)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            ChosenLanguage = _languages[comboBoxDictionaries.SelectedIndex];
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}

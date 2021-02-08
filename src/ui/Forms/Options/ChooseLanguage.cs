using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class ChooseLanguage : PositionAndSizeForm
    {
        private class TranslationInfo : IEquatable<TranslationInfo>
        {
            public string CultureName { get; }

            public string DisplayName { get; }

            public TranslationInfo(string cultureName, string displayName)
            {
                CultureName = cultureName;
                try
                {
                    DisplayName = displayName.CapitalizeFirstLetter(CultureInfo.GetCultureInfo(cultureName));
                }
                catch
                {
                    DisplayName = displayName.CapitalizeFirstLetter(CultureInfo.InvariantCulture);
                }
            }

            public bool Equals(TranslationInfo ti)
            {
                return !ReferenceEquals(ti, null) && CultureName.Equals(ti.CultureName, StringComparison.OrdinalIgnoreCase);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as TranslationInfo);
            }

            public override int GetHashCode()
            {
                return CultureName.ToUpperInvariant().GetHashCode();
            }

            public override string ToString()
            {
                return DisplayName;
            }
        }

        private readonly TranslationInfo _defaultTranslation;

        public string CultureName => !(comboBoxLanguages.SelectedItem is TranslationInfo translation) ? _defaultTranslation.CultureName : translation.CultureName;

        public ChooseLanguage()
        {
            TranslationInfo currentTranslation;
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var defaultLanguage = new Language();
            _defaultTranslation = new TranslationInfo(defaultLanguage.General.CultureName, defaultLanguage.Name);
            var currentLanguage = LanguageSettings.Current;
            if (currentLanguage == null)
            {
                currentTranslation = new TranslationInfo(CultureInfo.CurrentUICulture.Name, CultureInfo.CurrentUICulture.NativeName);
                LanguageSettings.Current = defaultLanguage;
            }
            else
            {
                currentTranslation = new TranslationInfo(currentLanguage.General.CultureName, currentLanguage.Name);
            }

            var translations = new HashSet<TranslationInfo> { _defaultTranslation };
            if (Directory.Exists(Path.Combine(Configuration.BaseDirectory, "Languages")))
            {
                var versionInfo = Utilities.AssemblyVersion.Split('.');
                var currentVersion = $"{versionInfo[0]}.{versionInfo[1]}.{versionInfo[2]}";
                var document = new XmlDocument { XmlResolver = null };

                foreach (var fileName in Directory.GetFiles(Path.Combine(Configuration.BaseDirectory, "Languages"), "*.xml"))
                {
                    document.Load(fileName);
                    try
                    {
                        var version = document.DocumentElement.SelectSingleNode("General/Version").InnerText.Trim();
                        if (version == currentVersion)
                        {
                            var cultureName = document.DocumentElement.SelectSingleNode("General/CultureName").InnerText.Trim();
                            var displayName = document.DocumentElement.Attributes["Name"].Value.Trim();
                            if (!string.IsNullOrEmpty(cultureName) && !string.IsNullOrEmpty(displayName))
                            {
                                translations.Add(new TranslationInfo(cultureName, displayName));
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            int index = -1;
            foreach (var translation in translations.OrderBy(ti => ti.DisplayName, StringComparer.CurrentCultureIgnoreCase).ThenBy(ti => ti.CultureName, StringComparer.Ordinal))
            {
                int i = comboBoxLanguages.Items.Add(translation);
                if (translation.Equals(currentTranslation) || (index < 0 && translation.Equals(_defaultTranslation)))
                {
                    index = i;
                }
            }
            comboBoxLanguages.SelectedIndex = index;
            comboBoxLanguages.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBoxLanguages.AutoCompleteMode = AutoCompleteMode.Append;

            Text = LanguageSettings.Current.ChooseLanguage.Title;
            labelLanguage.Text = LanguageSettings.Current.ChooseLanguage.Language;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void ChangeLanguage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#translate");
                e.SuppressKeyPress = true;
            }
        }

    }
}

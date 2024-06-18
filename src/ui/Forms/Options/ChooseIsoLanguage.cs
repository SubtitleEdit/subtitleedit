using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class ChooseIsoLanguage : PositionAndSizeForm
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

        public ChooseIsoLanguage()
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
            foreach (var iso in Iso639Dash2LanguageCode.List)
            {
                translations.Add(new TranslationInfo(iso.TwoLetterCode, iso.EnglishName));
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

            Text = LanguageSettings.Current.ChooseLanguage.Title;
            labelLanguage.Text = LanguageSettings.Current.ChooseLanguage.Language;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            comboBoxLanguages.UsePopupWindow = true;
        }

        private void ChangeLanguage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}

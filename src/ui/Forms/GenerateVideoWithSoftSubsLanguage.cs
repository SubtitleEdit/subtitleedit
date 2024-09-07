using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GenerateVideoWithSoftSubsLanguage : PositionAndSizeForm
    {
        public class LanguageTitleInfo : IEquatable<LanguageTitleInfo>
        {
            public string ThreeLetterLanguageCode { get; }

            public string Title { get; }

            public LanguageTitleInfo(string threeLetterLanguageCode, string displayName)
            {
                ThreeLetterLanguageCode = threeLetterLanguageCode;
                try
                {
                    Title = displayName.CapitalizeFirstLetter(CultureInfo.GetCultureInfo(threeLetterLanguageCode));
                }
                catch
                {
                    Title = displayName.CapitalizeFirstLetter(CultureInfo.InvariantCulture);
                }
            }

            public bool Equals(LanguageTitleInfo ti)
            {
                return !ReferenceEquals(ti, null) && ThreeLetterLanguageCode.Equals(ti.ThreeLetterLanguageCode, StringComparison.OrdinalIgnoreCase);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as LanguageTitleInfo);
            }

            public override int GetHashCode()
            {
                return ThreeLetterLanguageCode.ToUpperInvariant().GetHashCode();
            }

            public override string ToString()
            {
                return Title;
            }
        }

        private readonly LanguageTitleInfo _defaultLanguageTitle;

        public LanguageTitleInfo Result => new LanguageTitleInfo(textBoxLanguageCode.Text, textBoxTitle.Text);

        public GenerateVideoWithSoftSubsLanguage(string languageCode, string title)
        {
            LanguageTitleInfo currentLanguageTitle;
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var defaultLanguage = new Language();
            _defaultLanguageTitle = new LanguageTitleInfo(defaultLanguage.General.CultureName, defaultLanguage.Name);
            var currentLanguage = LanguageSettings.Current;
            if (currentLanguage == null)
            {
                currentLanguageTitle = new LanguageTitleInfo(CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName, CultureInfo.CurrentUICulture.EnglishName);
                LanguageSettings.Current = defaultLanguage;
                textBoxLanguageCode.Text = currentLanguageTitle.ThreeLetterLanguageCode;
                textBoxTitle.Text = currentLanguageTitle.Title;
            }
            else
            {
                var code = currentLanguage.General.CultureName;
                if (code != null && code.Length > 2)
                {
                    code = code.Substring(0, 2);
                    var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(code);
                    if (!string.IsNullOrEmpty(threeLetterCode))
                    {
                        code = threeLetterCode;
                    }
                }

                currentLanguageTitle = new LanguageTitleInfo(code, currentLanguage.Name);
                textBoxLanguageCode.Text = currentLanguageTitle.ThreeLetterLanguageCode;
                textBoxTitle.Text = currentLanguageTitle.Title;
            }

            var translations = new HashSet<LanguageTitleInfo> { _defaultLanguageTitle };
            foreach (var iso in Iso639Dash2LanguageCode.List)
            {
                translations.Add(new LanguageTitleInfo(iso.ThreeLetterCode, iso.EnglishName));
            }

            var index = -1;
            foreach (var translation in translations.OrderBy(ti => ti.Title, StringComparer.CurrentCultureIgnoreCase).ThenBy(ti => ti.ThreeLetterLanguageCode, StringComparer.Ordinal))
            {
                var i = comboBoxLanguages.Items.Add(translation);
                if (translation.Equals(currentLanguageTitle) || (index < 0 && translation.Equals(_defaultLanguageTitle)))
                {
                    index = i;
                }
            }
            comboBoxLanguages.SelectedIndex = index;

            Text = LanguageSettings.Current.ChooseLanguage.Title;
            labelLanguage.Text = LanguageSettings.Current.ChooseLanguage.Language;
            labelLanguageCode.Text = LanguageSettings.Current.EbuSaveOptions.LanguageCode;
            labelTitle.Text = LanguageSettings.Current.SubStationAlphaProperties.Title;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            comboBoxLanguages.UsePopupWindow = true;

            comboBoxLanguages.SelectedIndexChanged += (sender, args) =>
            {
                if (comboBoxLanguages.SelectedItem is LanguageTitleInfo translation)
                {
                    textBoxLanguageCode.Text = translation.ThreeLetterLanguageCode;
                    textBoxTitle.Text = translation.Title;
                }
            };

            if (!string.IsNullOrEmpty(languageCode))
            {
                textBoxLanguageCode.Text = languageCode;
            }

            if (!string.IsNullOrEmpty(title))
            {
                textBoxTitle.Text = title;
            }
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

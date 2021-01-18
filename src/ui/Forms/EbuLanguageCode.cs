using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class EbuLanguageCode : Form
    {

        public class LanguageItem
        {
            public string Code { get; set; }
            public string Language { get; set; }

            public override string ToString()
            {
                return $"{Code} - {Language}";
            }

            public LanguageItem(string code, string language)
            {
                Code = code;
                Language = language;
            }
        }

        public string LanguageCode { get; set; }

        private static readonly List<LanguageItem> Languages = new List<LanguageItem>
        {
            new LanguageItem("01", "Albanian"),
            new LanguageItem("02", "Breton"),
            new LanguageItem("03", "Catalan"),
            new LanguageItem("04", "Croatian"),
            new LanguageItem("05", "Welsh"),
            new LanguageItem("06", "Czech"),
            new LanguageItem("07", "Danish"),
            new LanguageItem("08", "German"),
            new LanguageItem("09", "English"),
            new LanguageItem("0A", "Spanish"),
            new LanguageItem("0B", "Esperanto"),
            new LanguageItem("0C", "Estonian"),
            new LanguageItem("0D", "Basque"),
            new LanguageItem("0E", "Faroese"),
            new LanguageItem("0F", "French"),
            new LanguageItem("10", "Frisian"),
            new LanguageItem("11", "Irish"),
            new LanguageItem("12", "Gaelic"),
            new LanguageItem("13", "Galician"),
            new LanguageItem("14", "Icelandic"),
            new LanguageItem("15", "Italian"),
            new LanguageItem("16", "Lappish"),
            new LanguageItem("17", "Latin"),
            new LanguageItem("18", "Latvian"),
            new LanguageItem("19", "Luxembourgi"),
            new LanguageItem("1A", "Lithuanian"),
            new LanguageItem("1B", "Hungarian"),
            new LanguageItem("1C", "Maltese"),
            new LanguageItem("1D", "Dutch"),
            new LanguageItem("1E", "Norwegian"),
            new LanguageItem("1F", "Occitan"),
            new LanguageItem("20", "Polish"),
            new LanguageItem("21", "Portuguese"),
            new LanguageItem("22", "Romanian"),
            new LanguageItem("23", "Romansh"),
            new LanguageItem("24", "Serbian"),
            new LanguageItem("25", "Slovak"),
            new LanguageItem("26", "Slovenian"),
            new LanguageItem("27", "Finnish"),
            new LanguageItem("28", "Swedish"),
            new LanguageItem("29", "Turkish"),
            new LanguageItem("2A", "Flemish"),
            new LanguageItem("2B", "Wallon"),
        };

        public static string GetLanguageFromCode(string code)
        {
            var item = Languages.FirstOrDefault(p => p.Code == code);
            if (item == null)
            {
                return string.Empty;
            }

            return item.Language;
        }

        public EbuLanguageCode(string languageCode)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            listBoxLanguages.Items.Clear();
            int i = 0;
            foreach (var language in Languages)
            {
                listBoxLanguages.Items.Add(language);
                if (languageCode.Equals(language.Code, StringComparison.Ordinal))
                {
                    listBoxLanguages.SelectedIndex = i;
                }

                i++;
            }

            LanguageCode = languageCode;
            Text = LanguageSettings.Current.EbuSaveOptions.Title;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedIndex = listBoxLanguages.SelectedIndex;
            if (selectedIndex < 0)
            {
                return;
            }

            var item = listBoxLanguages.Items[selectedIndex] as LanguageItem;
            if (item == null)
            {
                return;
            }

            LanguageCode = item.Code;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void EbuLanguageCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}

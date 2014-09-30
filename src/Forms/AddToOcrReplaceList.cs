using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class AddToOcrReplaceList : Form
    {
        private string _threeLetterIsoLanguageName;

        public AddToOcrReplaceList()
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.AddToOcrReplaceList.Title;
            labelDescription.Text = Configuration.Settings.Language.AddToOcrReplaceList.Description;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            var graphics = CreateGraphics();
            var textSize = graphics.MeasureString(buttonOK.Text, Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            string key = textBoxOcrFixKey.Text.Trim();
            string value = textBoxOcrFixValue.Text.Trim();
            if (key.Length == 0 || value.Length == 0 || key == value)
                return;

            try
            {
                var ci = new CultureInfo(LanguageString.Replace("_", "-"));
                _threeLetterIsoLanguageName = ci.ThreeLetterISOLanguageName;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }
            var ocrFixReplaceList = OcrFixReplaceList.FromLanguageId(_threeLetterIsoLanguageName);
            ocrFixReplaceList.AddWordOrPartial(key, value);
            DialogResult = DialogResult.OK;
            NewSource = key;
            NewTarget = value;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddToOcrReplaceList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        public string NewSource { get; set; }
        public string NewTarget { get; set; }

        internal void Initialize(string languageId, string hunspellName, string source)
        {
            if (!string.IsNullOrEmpty(source))
                textBoxOcrFixKey.Text = source;

            comboBoxDictionaries.Items.Clear();
            foreach (string name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
                if (hunspellName != null && name.Equals(hunspellName, StringComparison.OrdinalIgnoreCase))
                    comboBoxDictionaries.SelectedIndex = comboBoxDictionaries.Items.Count - 1;
            }
            _threeLetterIsoLanguageName = languageId;
        }

        public string LanguageString
        {
            get
            {
                string name = comboBoxDictionaries.SelectedItem.ToString();
                int start = name.LastIndexOf('[');
                int end = name.LastIndexOf(']');
                if (start >= 0 && end > start)
                {
                    start++;
                    name = name.Substring(start, end - start);
                    return name;
                }
                return null;
            }
        }
    }
}

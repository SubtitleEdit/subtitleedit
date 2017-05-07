using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChangeCasing : PositionAndSizeForm
    {
        private int _noOfLinesChanged;

        public ChangeCasing()
        {
            InitializeComponent();

            LanguageStructure.ChangeCasing language = Configuration.Settings.Language.ChangeCasing;
            Text = language.Title;
            groupBoxChangeCasing.Text = language.ChangeCasingTo;
            radioButtonNormal.Text = language.NormalCasing;
            checkBoxFixNames.Text = language.FixNamesCasing;
            radioButtonFixOnlyNames.Text = language.FixOnlyNamesCasing;
            checkBoxOnlyAllUpper.Text = language.OnlyChangeAllUppercaseLines;
            radioButtonUppercase.Text = language.AllUppercase;
            radioButtonLowercase.Text = language.AllLowercase;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();

            if (Configuration.Settings.Tools.ChangeCasingChoice == "NamesOnly")
                radioButtonFixOnlyNames.Checked = true;
            else if (Configuration.Settings.Tools.ChangeCasingChoice == "Uppercase")
                radioButtonUppercase.Checked = true;
            else if (Configuration.Settings.Tools.ChangeCasingChoice == "Lowercase")
                radioButtonLowercase.Checked = true;
        }

        public int LinesChanged
        {
            get { return _noOfLinesChanged; }
        }

        public bool ChangeNamesToo
        {
            get
            {
                return radioButtonFixOnlyNames.Checked ||
                       (radioButtonNormal.Checked && checkBoxFixNames.Checked);
            }
        }

        private void FixLargeFonts()
        {
            if (radioButtonNormal.Left + radioButtonNormal.Width + 40 > Width)
                Width = radioButtonNormal.Left + radioButtonNormal.Width + 40;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        internal void FixCasing(Subtitle subtitle, string language)
        {
            var nameList = new NameList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            var names = nameList.GetAllNames();

            // Longer names must be first
            names.Sort((s1, s2) => s2.Length.CompareTo(s1.Length));

            string lastLine = string.Empty;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                p.Text = FixCasing(p.Text, lastLine, names);

                // fix casing of English alone i to I
                if (radioButtonNormal.Checked && language.StartsWith("en", StringComparison.Ordinal))
                {
                    p.Text = FixEnglishAloneILowerToUpper(p.Text);
                }

                lastLine = p.Text;
            }
        }

        public static string FixEnglishAloneILowerToUpper(string text)
        {
            const string pre = " >¡¿♪♫([";
            const string post = " <!?.:;,♪♫)]";
            for (var indexOfI = text.IndexOf('i'); indexOfI >= 0; indexOfI = text.IndexOf('i', indexOfI + 1))
            {
                if (indexOfI == 0 || pre.Contains(text[indexOfI - 1]))
                {
                    if (indexOfI + 1 == text.Length || post.Contains(text[indexOfI + 1]))
                    {
                        text = text.Remove(indexOfI, 1).Insert(indexOfI, "I");
                    }
                }
            }
            return text;
        }

        private string FixCasing(string text, string lastLine, List<string> nameList)
        {
            string original = text;
            if (radioButtonNormal.Checked)
            {
                if (checkBoxOnlyAllUpper.Checked && text != text.ToUpper())
                    return text;

                if (text.Length > 1)
                {
                    // first all to lower
                    text = text.ToLower().Trim();
                    text = text.FixExtraSpaces();
                    var st = new StrippableText(text);
                    st.FixCasing(nameList, false, true, true, lastLine); // fix all casing but names (that's a seperate option)
                    text = st.MergedString;
                }
            }
            else if (radioButtonUppercase.Checked)
            {
                var st = new StrippableText(text);
                text = st.Pre + st.StrippedText.ToUpper() + st.Post;
                text = HtmlUtil.FixUpperTags(text); // tags inside text
            }
            else if (radioButtonLowercase.Checked)
            {
                text = text.ToLower();
            }
            if (original != text)
                _noOfLinesChanged++;
            return text;
        }

        private void FormChangeCasing_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (radioButtonNormal.Checked)
                Configuration.Settings.Tools.ChangeCasingChoice = "Normal";
            else if (radioButtonFixOnlyNames.Checked)
                Configuration.Settings.Tools.ChangeCasingChoice = "NamesOnly";
            else if (radioButtonUppercase.Checked)
                Configuration.Settings.Tools.ChangeCasingChoice = "Uppercase";
            else if (radioButtonLowercase.Checked)
                Configuration.Settings.Tools.ChangeCasingChoice = "Lowercase";
            DialogResult = DialogResult.OK;
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            var isNormalCasing = sender == radioButtonNormal;
            checkBoxFixNames.Enabled = isNormalCasing;
            checkBoxOnlyAllUpper.Enabled = isNormalCasing;
        }

    }
}

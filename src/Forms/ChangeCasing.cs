﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChangeCasing : PositionAndSizeForm
    {
        private static readonly Regex AloneI = new Regex(@"\bi\b", RegexOptions.Compiled);
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

            Graphics graphics = CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                var newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        internal void FixCasing(Subtitle subtitle, string language)
        {
            var namesList = new NamesList(Configuration.DictionariesFolder, language, Configuration.Settings.WordLists.UseOnlineNamesEtc, Configuration.Settings.WordLists.NamesEtcUrl);
            var namesEtc = namesList.GetAllNames();

            // Longer names must be first
            namesEtc.Sort((s1, s2) => s2.Length.CompareTo(s1.Length));

            string lastLine = string.Empty;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                p.Text = FixCasing(p.Text, lastLine, namesEtc);

                // fix casing of English alone i to I
                if (radioButtonNormal.Checked && language.StartsWith("en", StringComparison.Ordinal) && p.Text.Contains('i'))
                {
                    Match match = AloneI.Match(p.Text);
                    while (match.Success)
                    {
                        if (p.Text[match.Index] == 'i')
                        {
                            string prev = string.Empty;
                            string next = string.Empty;
                            if (match.Index > 0)
                                prev = p.Text[match.Index - 1].ToString(CultureInfo.InvariantCulture);
                            if (match.Index + 1 < p.Text.Length)
                                next = p.Text[match.Index + 1].ToString(CultureInfo.InvariantCulture);
                            if (prev != "<" || next != ">")
                            {
                                string oldText = p.Text;
                                p.Text = p.Text.Substring(0, match.Index) + "I";
                                if (match.Index + 1 < oldText.Length)
                                    p.Text += oldText.Substring(match.Index + 1);
                            }
                        }
                        match = match.NextMatch();
                    }
                }
                lastLine = p.Text;
            }
        }

        private string FixCasing(string text, string lastLine, List<string> namesEtc)
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
                    while (text.Contains("  ", StringComparison.Ordinal))
                        text = text.Replace("  ", " ");
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine);

                    var st = new StripableText(text);
                    st.FixCasing(namesEtc, false, true, true, lastLine); // fix all casing but names (that's a seperate option)
                    text = st.MergedString;
                }
            }
            else if (radioButtonUppercase.Checked)
            {
                var st = new StripableText(text);
                text = st.Pre + st.StrippedText.ToUpper() + st.Post;
                text = text.Replace("<I>", "<i>");
                text = text.Replace("</I>", "</i>");
                text = text.Replace("<B>", "<b>");
                text = text.Replace("</B>", "</b>");
                text = text.Replace("<U>", "<u>");
                text = text.Replace("</U>", "</u>");
                var idx = text.IndexOf("<FONT", StringComparison.Ordinal);
                while (idx >= 0)
                {
                    var endIdx = text.IndexOf('>', idx + 5);
                    if (endIdx < idx)
                        break;
                    text = text.Substring(0, endIdx - idx).ToLowerInvariant() + text.Substring(endIdx);
                    idx = text.IndexOf("<FONT", StringComparison.Ordinal);
                }
                text = text.Replace("</FONT>", "</font>");
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

    }
}
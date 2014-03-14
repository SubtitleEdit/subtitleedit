﻿using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class FormRemoveTextForHearImpaired : Form
    {
        private Subtitle _subtitle;
        private readonly LanguageStructure.RemoveTextFromHearImpaired _language;
        private List<string> _interjectionList;
        private List<int> _warnings;
        private int _warningIndex;

        public FormRemoveTextForHearImpaired()
        {
            InitializeComponent();

            checkBoxRemoveTextBetweenSquares.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets;
            checkBoxRemoveTextBetweenParentheses.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses;
            checkBoxRemoveTextBetweenBrackets.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets;
            checkBoxRemoveTextBetweenQuestionMarks.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks;
            checkBoxRemoveTextBetweenCustomTags.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom;
            comboBoxCustomStart.Text = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore;
            comboBoxCustomEnd.Text = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter;
            checkBoxOnlyIfInSeparateLine.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeperateLines;
            checkBoxRemoveTextBeforeColon.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon;
            checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase;
            checkBoxColonSeparateLine.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine;
            checkBoxRemoveInterjections.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections;
            checkBoxRemoveWhereContains.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContains;
            checkBoxRemoveIfAllUppercase.Checked = Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase;
            comboBoxRemoveIfTextContains.Text = Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContainsText;

            _language = Configuration.Settings.Language.RemoveTextFromHearImpaired;
            Text = _language.Title;
            groupBoxRemoveTextConditions.Text = _language.RemoveTextConditions;
            labelAnd.Text = _language.And;
            labelRemoveTextBetween.Text = _language.RemoveTextBetween;
            checkBoxRemoveTextBeforeColon.Text = _language.RemoveTextBeforeColon;
            checkBoxRemoveTextBeforeColonOnlyUppercase.Text = _language.OnlyIfTextIsUppercase;
            checkBoxOnlyIfInSeparateLine.Text = _language.OnlyIfInSeparateLine;
            checkBoxColonSeparateLine.Text = _language.OnlyIfInSeparateLine;
            checkBoxRemoveTextBetweenBrackets.Text = _language.Brackets;
            checkBoxRemoveTextBetweenParentheses.Text = _language.Parentheses;
            checkBoxRemoveTextBetweenQuestionMarks.Text = _language.QuestionMarks;
            checkBoxRemoveTextBetweenSquares.Text = _language.SquareBrackets;
            checkBoxRemoveWhereContains.Text = _language.RemoveTextIfContains;
            checkBoxRemoveIfAllUppercase.Text = _language.RemoveTextIfAllUppercase;
            checkBoxRemoveInterjections.Text = _language.RemoveInterjections;
            buttonEditInterjections.Text = _language.EditInterjections;
            buttonEditInterjections.Left = checkBoxRemoveInterjections.Left + checkBoxRemoveInterjections.Width;
            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.General.Before;
            listViewFixes.Columns[3].Text = Configuration.Settings.Language.General.After;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public static string RemoveStartEndNoise(string text)
        {
            string s = text.Trim();
            if (s.StartsWith("<b>") && s.Length > 3)
                s = s.Substring(3);
            if (s.StartsWith("<i>") && s.Length > 3)
                s = s.Substring(3);
            if (s.StartsWith("<u>") && s.Length > 3)
                s = s.Substring(3);
            if (s.StartsWith("<B>") && s.Length > 3)
                s = s.Substring(3);
            if (s.StartsWith("<I>") && s.Length > 3)
                s = s.Substring(3);
            if (s.StartsWith("<U>") && s.Length > 3)
                s = s.Substring(3);

            if (s.EndsWith("</b>") && s.Length > 4)
                s = s.Substring(0, s.Length - 4);
            if (s.EndsWith("</i>") && s.Length > 4)
                s = s.Substring(0, s.Length - 4);
            if (s.EndsWith("</u>") && s.Length > 4)
                s = s.Substring(0, s.Length - 4);
            if (s.EndsWith("</B>") && s.Length > 4)
                s = s.Substring(0, s.Length - 4);
            if (s.EndsWith("</I>") && s.Length > 4)
                s = s.Substring(0, s.Length - 4);
            if (s.EndsWith("</U>") && s.Length > 4)
                s = s.Substring(0, s.Length - 4);

            if (s.StartsWith("-") && s.Length > 2)
                s = s.TrimStart('-');

            return s.Trim();
        }

        private string RemoveTextBetweenTags(string startTag, string endTag, string text)
        {
            text = text.Trim();
            if (startTag == "?" || endTag == "?")
            {
                if (text.StartsWith(startTag) && text.EndsWith(endTag))
                    return string.Empty;
                return text;
            }

            int start = text.IndexOf(startTag);
            if (start == -1 || start == text.Length - 1)
                return text;

            int end = text.IndexOf(endTag, start + 1);
            while (start >= 0 && end > start)
            {
                text = text.Remove(start, (end - start) + 1);
                start = text.IndexOf(startTag);
                if (start >= 0 && start < text.Length - 1)
                    end = text.IndexOf(endTag, start);
                else
                    end = -1;
            }
            return text.Replace(" " + Environment.NewLine, Environment.NewLine).TrimEnd();
        }

        private string RemoveHearImpairedTags(string text)
        {
            if (checkBoxRemoveTextBetweenSquares.Checked)
            {
                text = RemoveTextBetweenTags("[", "]:", text);
                text = RemoveTextBetweenTags("[", "]", text);
            }

            if (checkBoxRemoveTextBetweenBrackets.Checked)
            {
                text = RemoveTextBetweenTags("{", "}:", text);
                text = RemoveTextBetweenTags("{", "}", text);
            }

            if (checkBoxRemoveTextBetweenQuestionMarks.Checked)
            {
                text = RemoveTextBetweenTags("?", "?:", text);
                text = RemoveTextBetweenTags("?", "?", text);
            }

            if (checkBoxRemoveTextBetweenParentheses.Checked)
            {
                text = RemoveTextBetweenTags("(", "):", text);
                text = RemoveTextBetweenTags("(", ")", text);
            }

            if (checkBoxRemoveTextBetweenCustomTags.Checked && comboBoxCustomStart.Text.Length > 0 && comboBoxCustomEnd.Text.Length > 0)
            {
                text = RemoveTextBetweenTags(comboBoxCustomStart.Text, comboBoxCustomEnd.Text, text);
            }

            return text;
        }

        private bool HasHearImpairedText(string text)
        {
            return RemoveHearImpairedTags(text) != text;
        }

        public bool HasHearImpariedTagsAtStart(string text)
        {
            if (checkBoxOnlyIfInSeparateLine.Checked)
                return StartAndEndsWithHearImpariedTags(text);

            return HasHearImpairedText(text);
        }

        public bool HasHearImpariedTagsAtEnd(string text)
        {
            if (checkBoxOnlyIfInSeparateLine.Checked)
                return StartAndEndsWithHearImpariedTags(text);

            return HasHearImpairedText(text);
        }

        private bool StartAndEndsWithHearImpariedTags(string text)
        {
            return (text.StartsWith("[") && text.EndsWith("]") && !text.Trim('[').Contains("[") && checkBoxRemoveTextBetweenSquares.Checked) ||
                   (text.StartsWith("{") && text.EndsWith("}") && !text.Trim('{').Contains("{") && checkBoxRemoveTextBetweenBrackets.Checked) ||
                   (text.StartsWith("?") && text.EndsWith("?") && !text.Trim('?').Contains("?") && checkBoxRemoveTextBetweenQuestionMarks.Checked) ||
                   (text.StartsWith("(") && text.EndsWith(")") && !text.Trim('(').Contains("(") && checkBoxRemoveTextBetweenParentheses.Checked) ||
                   (text.StartsWith("[") && text.EndsWith("]:") && !text.Trim('[').Contains("[") && checkBoxRemoveTextBetweenSquares.Checked) ||
                   (text.StartsWith("{") && text.EndsWith("}:") && !text.Trim('{').Contains("{") && checkBoxRemoveTextBetweenBrackets.Checked) ||
                   (text.StartsWith("?") && text.EndsWith("?:") && !text.Trim('?').Contains("?") && checkBoxRemoveTextBetweenQuestionMarks.Checked) ||
                   (text.StartsWith("(") && text.EndsWith("):") && !text.Trim('(').Contains("(") && checkBoxRemoveTextBetweenParentheses.Checked) ||
                   (checkBoxRemoveTextBetweenCustomTags.Checked &&
                    comboBoxCustomStart.Text.Length > 0 && comboBoxCustomEnd.Text.Length > 0 &&
                    text.StartsWith(comboBoxCustomStart.Text) && text.EndsWith(comboBoxCustomEnd.Text));
        }

        public void Initialize(Subtitle subtitle)
        {
            if (Environment.OSVersion.Version.Major < 6) // 6 == Vista/Win2008Server/Win7
            {
                string unicodeFontName = Utilities.WinXp2kUnicodeFontName;
                float fontSize = comboBoxCustomStart.Font.Size;
                comboBoxCustomStart.Font = new System.Drawing.Font(unicodeFontName, fontSize);
                comboBoxCustomEnd.Font = new System.Drawing.Font(unicodeFontName, fontSize);
                comboBoxRemoveIfTextContains.Font = new System.Drawing.Font(unicodeFontName, fontSize);
            }
            comboBoxRemoveIfTextContains.Left = checkBoxRemoveWhereContains.Left + checkBoxRemoveWhereContains.Width;

            _subtitle = subtitle;
            GeneratePreview();
        }

        public void InitializeSettingsOnly()
        {
            comboBoxRemoveIfTextContains.Left = checkBoxRemoveWhereContains.Left + checkBoxRemoveWhereContains.Width;
            groupBoxLinesFound.Visible = false;
            int h = groupBoxRemoveTextConditions.Top + groupBoxRemoveTextConditions.Height + buttonOK.Height + 50;
            MinimumSize = new System.Drawing.Size(MinimumSize.Width, h);
            this.Height = h;
        }

        private void AddWarning()
        {
            if (_warnings == null || _warningIndex < 0)
                return;

            _warnings.Add(_warningIndex);
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
                return;

            _warnings = new List<int>();
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            int count = 0;
            int prevIndex = -1;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                string prevText = string.Empty;
                var prev = _subtitle.GetParagraphOrDefault(prevIndex);
                if (prev != null)
                    prevText = prev.Text;
                prevIndex++;

                _warningIndex = prevIndex;
                string newText = RemoveTextFromHearImpaired(p.Text, prevText);
                bool hit = p.Text.Replace(" ", string.Empty) != newText.Replace(" ", string.Empty);
                if (hit)
                {
                    count++;
                    AddToListView(p, newText);
                }
            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(_language.LinesFoundX, count);
        }

        private string RemoveHearImpairedtagsInsideLine(string newText)
        {
            int i = 5;
            while (i < newText.Length)
            {
                string s = newText.Substring(i);
                if (i > 5 && s.Length > 2 && (s.StartsWith(".") || s.StartsWith("!") || s.StartsWith("?")))
                {
                    if (s[1] == ' ' || s.Substring(1).StartsWith("<i>") || s.Substring(1).StartsWith("</i>"))
                    {
                        string pre = " ";
                        if (s.Substring(1).StartsWith("<i>"))
                            pre = "<i>";
                        else if (s.Substring(1).StartsWith(" <i>"))
                            pre = " <i>";
                        else if (s.Substring(1).StartsWith("</i>"))
                            pre = "</i>";

                        s = s.Remove(0, 1 + pre.Length);
                        if (s.StartsWith(" ") && s.Length > 1)
                        {
                            pre += " ";
                            s = s.Remove(0, 1);
                        }

                        if (HasHearImpariedTagsAtStart(s))
                        {
                            s = RemoveStartEndTags(s);
                            newText = newText.Substring(0, i + 1) + pre + " " + s;
                            newText = newText.Replace("<i></i>", string.Empty);
                            newText = newText.Replace("<i> </i>", " ");
                            newText = newText.Replace("  ", " ");
                            newText = newText.Replace("  ", " ");
                            newText = newText.Replace(" " + Environment.NewLine, Environment.NewLine);
                        }
                    }
                }
                i++;
            }
            return newText;
        }

        private string RemoveColon(string text, string prevText)
        {
            if (!checkBoxRemoveTextBeforeColon.Checked)
                return text;

            if (text.IndexOf(":") < 0)
                return text;

            // House 7x01 line 52: and she would like you to do three things:
            // Okay or remove???
            if (text.IndexOf(':') > 0 && text.IndexOf(':') == text.Length - 1 && text != text.ToUpper())
                return text;

            string newText = string.Empty;
            string[] parts = text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int noOfNames = 0;
            int count = 0;
            bool removedInFirstLine = false;
            bool removedInSecondLine = false;
            foreach (string part in parts)
            {
                int indexOfColon = part.IndexOf(":");
                if (indexOfColon > 0)
                {
                    string pre = part.Substring(0, indexOfColon);
                    if (checkBoxRemoveTextBeforeColonOnlyUppercase.Checked && pre.Replace("<i>", string.Empty) != pre.Replace("<i>", string.Empty).ToUpper())
                    {
                        newText = newText + Environment.NewLine + part;
                        newText = newText.Trim();
                    }
                    else
                    {
                        StripableText st = new StripableText(pre);
                        if (count == 1 && Utilities.CountTagInText(text, Environment.NewLine) == 1 && removedInFirstLine && Utilities.CountTagInText(part, ":") == 1 &&
                            !newText.EndsWith(".") && !newText.EndsWith("!") && !newText.EndsWith("?") && !newText.EndsWith(".</i>") && !newText.EndsWith("!</i>") && !newText.EndsWith("?</i>") &&
                            part != part.ToUpper())
                        {
                            if (pre.Contains("<i>") && part.Contains("</i>"))
                                newText = newText + Environment.NewLine + "<i>" + part;
                            else if (pre.Contains("<b>") && part.Contains("</b>"))
                                newText = newText + Environment.NewLine + "<b>" + part;
                            else if (pre.Contains("[") && part.Contains("]"))
                                newText = newText + Environment.NewLine + "[" + part;
                            else if (pre.Contains("(") && part.EndsWith(")"))
                                newText = newText + Environment.NewLine + "(" + part;
                            else
                                newText = newText + Environment.NewLine + part;
                        }
                        else if (count == 1 && Utilities.CountTagInText(text, Environment.NewLine) == 1 && indexOfColon > 15 && part.Substring(0, indexOfColon).Contains(" ") && Utilities.CountTagInText(part, ":") == 1 &&
                            !newText.EndsWith(".") && !newText.EndsWith("!") && !newText.EndsWith("?") && !newText.EndsWith(".</i>") && !newText.EndsWith("!</i>") && !newText.EndsWith("?</i>") &&
                            part != part.ToUpper())
                        {
                            if (pre.Contains("<i>") && part.Contains("</i>"))
                                newText = newText + Environment.NewLine + "<i>" + part;
                            else if (pre.Contains("<b>") && part.Contains("</b>"))
                                newText = newText + Environment.NewLine + "<b>" + part;
                            else if (pre.Contains("[") && part.Contains("]"))
                                newText = newText + Environment.NewLine + "[" + part;
                            else if (pre.Contains("(") && part.EndsWith(")"))
                                newText = newText + Environment.NewLine + "(" + part;
                            else
                                newText = newText + Environment.NewLine + part;
                        }
                        else if (Utilities.CountTagInText(part, ":") == 1)
                        {
                            bool remove = true;
                            if (indexOfColon > 0 && indexOfColon < part.Length - 1)
                            {
                                if ("1234567890".Contains(part.Substring(indexOfColon - 1, 1)) && "1234567890".Contains(part.Substring(indexOfColon + 1, 1)))
                                    remove = false;
                            }
                            if (part.StartsWith("Previously on") || part.StartsWith("<i>Previously on"))
                                remove = false;

                            if (remove && checkBoxColonSeparateLine.Checked)
                            {
                                if (indexOfColon == part.Length - 1 || part.Substring(indexOfColon + 1).StartsWith(Environment.NewLine))
                                    remove = true;
                                else
                                    remove = false;
                            }

                            if (remove)
                            {
                                string content = part.Substring(indexOfColon + 1).Trim();
                                if (content.Length > 0)
                                {
                                    if (pre.Contains("<i>") && content.Contains("</i>"))
                                        newText = newText + Environment.NewLine + "<i>" + content;
                                    else if (pre.Contains("<b>") && content.Contains("</b>"))
                                        newText = newText + Environment.NewLine + "<b>" + content;
                                    else if (pre.Contains("[") && content.Contains("]"))
                                        newText = newText + Environment.NewLine + "[" + content;
                                    else if (pre.Contains("(") && content.EndsWith(")"))
                                        newText = newText + Environment.NewLine + "(" + content;
                                    else
                                        newText = newText + Environment.NewLine + content;

                                    if (count == 0)
                                        removedInFirstLine = true;
                                    else if (count == 1)
                                        removedInSecondLine = true;
                                }
                                newText = newText.Trim();

                                if (text.StartsWith("(") && newText.EndsWith(")") && !newText.Contains("("))
                                    newText = newText.TrimEnd(')');
                                else if (newText.EndsWith("</i>") && text.StartsWith("<i>") && !newText.StartsWith("<i>"))
                                    newText = "<i>" + newText;
                                else if (newText.EndsWith("</b>") && text.StartsWith("<b>") && !newText.StartsWith("<b>"))
                                    newText = "<b>" + newText;

                                if (!IsHIDescription(st.StrippedText))
                                    noOfNames++;
                            }
                            else
                            {
                                newText = newText + Environment.NewLine + part;
                                newText = newText.Trim();
                                if (newText.EndsWith("</i>") && text.StartsWith("<i>") && !newText.StartsWith("<i>"))
                                    newText = "<i>" + newText;
                                else if (newText.EndsWith("</b>") && text.StartsWith("<b>") && !newText.StartsWith("<b>"))
                                    newText = "<b>" + newText;
                            }
                        }
                        else
                        {
                            string s2 = part;
                            for (int k = 0; k < 2; k++)
                            {
                                if (s2.Contains(":"))
                                {
                                    int colonIndex = s2.IndexOf(":");
                                    string start = s2.Substring(0, colonIndex);

                                    bool doContinue = true;
                                    if (checkBoxRemoveTextBeforeColonOnlyUppercase.Checked && start != start.ToUpper())
                                        doContinue = false;
                                    if (doContinue)
                                    {
                                        int periodIndex = start.LastIndexOf(". ");
                                        int questIndex = start.LastIndexOf("? ");
                                        int exclaIndex = start.LastIndexOf("! ");
                                        int endIndex = periodIndex;
                                        if (endIndex == -1 || questIndex > endIndex)
                                            endIndex = questIndex;
                                        if (endIndex == -1 || exclaIndex > endIndex)
                                            endIndex = exclaIndex;
                                        if (colonIndex > 0 && colonIndex < s2.Length - 1)
                                        {
                                            if ("1234567890".Contains(s2.Substring(colonIndex - 1, 1)) && "1234567890".Contains(s2.Substring(colonIndex + 1, 1)))
                                                endIndex = -10;
                                        }
                                        if (endIndex == -1)
                                            s2 = s2.Remove(0, colonIndex - endIndex);
                                        else if (endIndex > 0)
                                            s2 = s2.Remove(endIndex + 1, colonIndex - endIndex);
                                    }

                                    if (count == 0)
                                        removedInFirstLine = true;
                                    else if (count == 1)
                                        removedInSecondLine = true;
                                }
                            }
                            newText = newText + Environment.NewLine + s2;
                            newText = newText.Trim();
                        }
                    }
                }
                else
                {
                    newText = newText + Environment.NewLine + part;
                    newText = newText.Trim();

                    if (newText.EndsWith("</i>") && text.StartsWith("<i>") && !newText.StartsWith("<i>"))
                        newText = "<i>" + newText;
                    else if (newText.EndsWith("</b>") && text.StartsWith("<b>") && !newText.StartsWith("<b>"))
                        newText = "<b>" + newText;
                }
                count++;
            }
            newText = newText.Trim();
            if (noOfNames > 0 && Utilities.CountTagInText(newText, Environment.NewLine) == 1)
            {
                int indexOfDialogChar = newText.IndexOf('-');
                bool insertDash = true;
                string[] arr = newText.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 2 && arr[0].Length > 1 && arr[1].Length > 1)
                {
                    string arr0 = new StripableText(arr[0]).StrippedText;
                    string arr1 = new StripableText(arr[1]).StrippedText;

                    //line continuation?
                    if (arr0.Length > 0 && arr1.Length > 1 && (Utilities.LowercaseLetters + ",").Contains(arr0.Substring(arr0.Length - 1)) &&
                        Utilities.LowercaseLetters.Contains(arr1.Substring(0, 1)))
                    {
                        if (new StripableText(arr[1]).Pre.Contains("...") == false)
                            insertDash = false;
                    }

                    if (arr0.Length > 0 && arr1.Length > 1 && !(arr[0].EndsWith(".") || arr[0].EndsWith("!") || arr[0].EndsWith("?") || arr[0].EndsWith("</i>")) &&
                        !(new StripableText(arr[1]).Pre.Contains("-")))
                    {
                        insertDash = false;
                    }

                    if (removedInFirstLine && !removedInSecondLine && !text.StartsWith("-") && !text.StartsWith("<i>-"))
                    {
                        if (insertDash && removedInFirstLine && (arr[1].StartsWith("-") || arr[1].StartsWith("<i>-")))
                            insertDash = true;
                        else
                            insertDash = false;
                    }
                }

                if (insertDash)
                {
                    if (indexOfDialogChar < 0 || indexOfDialogChar > 4)
                    {
                        StripableText st = new StripableText(newText, "", "");
                        newText = st.Pre + "- " + st.StrippedText + st.Post;
                    }

                    int indexOfNewLine = newText.IndexOf(Environment.NewLine);
                    string second = newText.Substring(indexOfNewLine).Trim();
                    indexOfDialogChar = second.IndexOf('-');
                    if (indexOfDialogChar < 0 || indexOfDialogChar > 6)
                    {
                        StripableText st = new StripableText(second, "", "");
                        second = st.Pre + "- " + st.StrippedText + st.Post;
                        newText = newText.Remove(indexOfNewLine) + Environment.NewLine + second;
                    }
                }
            }
            else if (!newText.Contains(Environment.NewLine) && newText.Contains("-"))
            {
                StripableText st = new StripableText(newText);
                if (st.Pre.Contains("-"))
                    newText = st.Pre.Replace("-", string.Empty) + st.StrippedText + st.Post;
            }
            else if (Utilities.CountTagInText(newText, Environment.NewLine) == 1 && removedInFirstLine == false && removedInSecondLine == true)
            {
                string noTags = Utilities.RemoveHtmlTags(newText, true).Trim();
                bool insertDash = noTags.StartsWith("-") && Utilities.CountTagInText(noTags, "-") == 1;
                if (insertDash)
                {
                    if (newText.Contains(Environment.NewLine + "<i>"))
                        newText = newText.Replace(Environment.NewLine + "<i>", Environment.NewLine + "<i>- ");
                    else
                        newText = newText.Replace(Environment.NewLine, Environment.NewLine + "- ");
                }
            }
            if (text.Contains("<i>") && !newText.Contains("<i>") && newText.EndsWith("</i>"))
                newText = "<i>" + newText;
            return newText;
        }

        internal string RemoveTextFromHearImpaired(string text, string prevText)
        {
            if (checkBoxRemoveWhereContains.Checked && comboBoxRemoveIfTextContains.Text.Length > 0 && text.Contains(comboBoxRemoveIfTextContains.Text))
            {
                return string.Empty;
            }

            string oldText = text;
            text = RemoveColon(text, prevText);
            string pre = " >-\"'‘`´♪¿¡.…—";
            string post = " -\"'`´♪.!?:…—";
            if (checkBoxRemoveTextBetweenCustomTags.Checked)
            {
                pre = pre.Replace(comboBoxCustomStart.Text, string.Empty);
                post = post.Replace(comboBoxCustomEnd.Text, string.Empty);
            }
            var st = new StripableText(text, pre, post);
            var sb = new StringBuilder();
            string[] parts = st.StrippedText.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int lineNumber = 0;
            bool removedDialogInFirstLine = false;
            int noOfNamesRemoved = 0;
            int noOfNamesRemovedNotInLineOne = 0;
            foreach (string s in parts)
            {
                StripableText stSub = new StripableText(s, pre, post);
                if (!StartAndEndsWithHearImpariedTags(stSub.StrippedText))
                {
                    if (removedDialogInFirstLine && stSub.Pre.Contains("- "))
                        stSub.Pre = stSub.Pre.Replace("- ", string.Empty);

                    string newText = stSub.StrippedText;

                    newText = RemoveHearImpairedTags(newText);

                    if (stSub.StrippedText.Length - newText.Length > 2)
                    {
                        string removedText = GetRemovedString(stSub.StrippedText, newText);
                        if (!IsHIDescription(removedText))
                        {
                            noOfNamesRemoved++;
                            if (lineNumber > 0)
                                noOfNamesRemovedNotInLineOne++;
                        }
                    }
                    sb.AppendLine(stSub.Pre + newText + stSub.Post);
                }
                else
                {
                    if (!IsHIDescription(stSub.StrippedText))
                    {
                        noOfNamesRemoved++;
                        if (lineNumber > 0)
                            noOfNamesRemovedNotInLineOne++;
                    }

                    if (st.Pre.Contains("- ") && lineNumber == 0)
                    {
                        st.Pre = st.Pre.Replace("- ", string.Empty);
                        removedDialogInFirstLine = true;
                    }

                    if (st.Pre.Contains("<i>") && stSub.Post.Contains("</i>"))
                        st.Pre = st.Pre.Replace("<i>", string.Empty);

                    if (s.Contains("<i>") && !s.Contains("</i>") && st.Post.Contains("</i>"))
                        st.Post = st.Post.Replace("</i>", string.Empty);
                }
                lineNumber++;
            }

            text = st.Pre + sb.ToString().Trim() + st.Post;
            text = text.Replace("<i></i>", string.Empty).Trim();
            text = RemoveColon(text, prevText);
            text = RemoveLineIfAllUppercase(text);
            text = RemoveHearImpairedtagsInsideLine(text);
            if (checkBoxRemoveInterjections.Checked)
                text = RemoveInterjections(text);

            st = new StripableText(text, " >-\"'‘`´♪¿¡.…—", " -\"'`´♪.!?:…—");
            text = st.StrippedText;
            if (StartAndEndsWithHearImpariedTags(text))
            {
                text = RemoveStartEndTags(text);
            }

            text = RemoveHearImpairedTags(text);

            // fix 3 lines to two liners - if only two lines
            if (noOfNamesRemoved >= 1 && Utilities.CountTagInText(text, Environment.NewLine) == 2)
            {
                string[] a = Utilities.RemoveHtmlTags(text).Replace(" ", string.Empty).Split("!?.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (a.Length == 2)
                {
                    StripableText temp = new StripableText(text);
                    temp.StrippedText = temp.StrippedText.Replace(Environment.NewLine, " ");
                    int splitIndex = temp.StrippedText.LastIndexOf("!");
                    if (splitIndex == -1)
                        splitIndex = temp.StrippedText.LastIndexOf("?");
                    if (splitIndex == -1)
                        splitIndex = temp.StrippedText.LastIndexOf(".");
                    if (splitIndex > 0)
                    {
                        text = temp.Pre + temp.StrippedText.Insert(splitIndex + 1, Environment.NewLine) + temp.Post;
                    }
                }
            }

            if (!text.StartsWith("-") && noOfNamesRemoved >= 1 && Utilities.CountTagInText(text, Environment.NewLine) == 1)
            {
                string[] arr = text.Split(Environment.NewLine.ToCharArray());
                string part0 = arr[0].Trim().Replace("</i>", string.Empty).Trim();
                if (!part0.EndsWith(",") && (!part0.EndsWith("-") || noOfNamesRemovedNotInLineOne > 0))
                {
                    if (part0.Length > 0 && ".!?".Contains(part0.Substring(part0.Length - 1)))
                    {
                        if (noOfNamesRemovedNotInLineOne > 0)
                        {
                            if (!st.Pre.Contains("-"))
                                text = "- " + text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                            if (!text.Contains(Environment.NewLine + "-") && !text.Contains(Environment.NewLine + "<i>-"))
                                text = text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(text))
                text = st.Pre + text + st.Post;

            if (oldText.Trim().StartsWith("- ") &&
                (oldText.Contains(Environment.NewLine + "- ") || oldText.Contains(Environment.NewLine + " - ")) &&
                text != null && !text.Contains(Environment.NewLine))
            {
                text = text.TrimStart().TrimStart('-').TrimStart();
            }

            if (oldText != text)
            {
                // insert spaces before "-"
                text = text.Replace(Environment.NewLine + "- <i>", Environment.NewLine + "<i>- ");
                text = text.Replace(Environment.NewLine + "-<i>", Environment.NewLine + "<i>- ");
                if (text.StartsWith("-") && text.Length > 2 && text[1] != ' ' && text[1] != '-')
                    text = text.Insert(1, " ");
                if (text.StartsWith("<i>-") && text.Length > 5 && text[4] != ' ' && text[4] != '-')
                    text = text.Insert(4, " ");
                if (text.Contains(Environment.NewLine + "-"))
                {
                    int index = text.IndexOf(Environment.NewLine + "-");
                    if (index + 4 < text.Length && text[index + Environment.NewLine.Length + 1] != ' ' && text[index + Environment.NewLine.Length + 1] != '-')
                        text = text.Insert(index + Environment.NewLine.Length + 1, " ");
                }
                if (text.Contains(Environment.NewLine + "<i>-"))
                {
                    int index = text.IndexOf(Environment.NewLine + "<i>-");
                    if (index + 5 < text.Length && text[index + Environment.NewLine.Length + 4] != ' ' && text[index + Environment.NewLine.Length + 4] != '-')
                        text = text.Insert(index + Environment.NewLine.Length + 4, " ");
                }
            }
            return text.Trim();
        }

        private bool IsHIDescription(string text)
        {
            text = text.ToLower();
            text = text.TrimEnd(" ()[]?{}".ToCharArray());
            text = text.TrimStart(" ()[]?{}".ToCharArray());

            if (text.Trim().Replace("mr. ", string.Empty).Replace("mrs. ", string.Empty).Replace("dr. ", string.Empty).Contains(" "))
                AddWarning();

            if (text == "sighing" ||
                text == "cackles" ||
                text == "cheers" ||
                text == "chitters" ||
                text == "chuckles" ||
                text == "exclaims" ||
                text == "gasps" ||
                text == "grunts" ||
                text == "groans" ||
                text == "growls" ||
                text == "explosion" ||
                text == "laughs" ||
                text == "noise" ||
                text.StartsWith("engine ") ||
                text == "roars" ||
                text == "scoff" ||
                text == "screeches" ||
                text == "shouts" ||
                text == "shrieks" ||
                text == "sigh" ||
                text == "sighs" ||
                text == "singing" ||
                text == "snores" ||
                text == "stutters" ||
                text == "thuds" ||
                text == "trumpets" ||
                text == "whispers" ||
                text == "whisper" ||
                text == "whistles" ||
                text.EndsWith("ing"))
                return true;
            return false;
        }

        private string GetRemovedString(string oldText, string newText)
        {
            oldText = oldText.ToLower();
            newText = newText.ToLower();
            int start = oldText.IndexOf(newText);
            string result;
            if (start > 0)
                result = oldText.Substring(0, oldText.Length - newText.Length);
            else
                result = oldText.Substring(newText.Length);
            result = result.TrimEnd(" ()[]?{}".ToCharArray());
            result = result.TrimStart(" ()[]?{}".ToCharArray());
            return result;
        }

        private static int CompareLength(string a, string b)
        {
            return b.Length.CompareTo(a.Length);
        }

        private string RemoveInterjections(string text)
        {
            string oldText = text;

            if (_interjectionList == null)
            {
                string[] arr = Configuration.Settings.Tools.Interjections.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                _interjectionList = new List<string>();
                foreach (string s in arr)
                {
                    if (!_interjectionList.Contains(s))
                        _interjectionList.Add(s);
                    string lower = s.ToLower();
                    if (!_interjectionList.Contains(lower))
                        _interjectionList.Add(lower);
                }
                _interjectionList.Sort(new Comparison<string>(CompareLength));
            }

            bool doRepeat = true;
            while (doRepeat)
            {
                doRepeat = false;
                foreach (string s in _interjectionList)
                {
                    if (text.Contains(s))
                    {
                        var regex = new Regex("\\b" + s + "\\b");
                        Match match = regex.Match(text);
                        if (match.Success)
                        {
                            int index = match.Index;
                            string temp = text.Remove(index, s.Length);

                            string pre = string.Empty;
                            if (index > 0)
                                doRepeat = true;

                            bool removeAfter = true;

                            if (temp.Length > index - s.Length + 3 && index > s.Length)
                            {
                                if (temp.Substring(index - s.Length + 1, 3) == ", !")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 2);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length + 1, 3) == ", ?")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 2);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length + 1, 3) == ", .")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 2);
                                    removeAfter = false;
                                }
                            }
                            if (removeAfter && temp.Length > index - s.Length + 2 && index > s.Length)
                            {
                                if (temp.Substring(index - s.Length, 3) == ", !")
                                {
                                    temp = temp.Remove(index - s.Length, 2);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length, 3) == ", ?")
                                {
                                    temp = temp.Remove(index - s.Length, 2);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length, 3) == ", .")
                                {
                                    temp = temp.Remove(index - s.Length, 2);
                                    removeAfter = false;
                                }
                            }
                            if (removeAfter && temp.Length > index - s.Length + 2 && index > s.Length)
                            {
                                if (temp.Substring(index - s.Length + 1, 2) == "-!")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 1);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length + 1, 2) == "-?")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 1);
                                    removeAfter = false;
                                }
                                else if (temp.Substring(index - s.Length + 1, 2) == "-.")
                                {
                                    temp = temp.Remove(index - s.Length + 1, 1);
                                    removeAfter = false;
                                }
                            }

                            if (removeAfter)
                            {
                                if (index == 0)
                                {
                                    if (!string.IsNullOrEmpty(temp) && temp.StartsWith("-"))
                                        temp = temp.Remove(0, 1).Trim();
                                }
                                else if (index == 3 && !string.IsNullOrEmpty(temp) && temp.StartsWith("<i>-"))
                                {
                                    temp = temp.Remove(3, 1);
                                }
                                else if (index > 0)
                                {
                                    pre = text.Substring(0, index);
                                    temp = temp.Remove(0, index);
                                    if (pre.EndsWith("-") && temp.StartsWith("-"))
                                        temp = temp.Remove(0, 1);
                                    if (pre.EndsWith("- ") && temp.StartsWith("-"))
                                        temp = temp.Remove(0, 1);
                                }

                                while (temp.Length > 0 && (temp.StartsWith(" ") || temp.StartsWith(",") || temp.StartsWith(".") || temp.StartsWith("!") || temp.StartsWith("?")))
                                {
                                    temp = temp.Remove(0, 1);
                                    doRepeat = true;
                                }
                                if (temp.Length > 0 && s[0].ToString() != s[0].ToString().ToLower())
                                {
                                    temp = temp.Remove(0, 1).Insert(0, temp[0].ToString().ToUpper());
                                    doRepeat = true;
                                }
                                temp = pre + temp;
                            }

                            if (temp.EndsWith(Environment.NewLine + "- "))
                                temp = temp.Remove(temp.Length - 4, 4);

                            var st = new StripableText(temp);
                            if (st.StrippedText.Length == 0)
                                return string.Empty;

                            if (!temp.Contains(Environment.NewLine) && text.Contains(Environment.NewLine) && temp.StartsWith("-"))
                                temp = temp.Remove(0, 1).Trim();

                            text = temp;
                        }
                    }
                }
            }
            string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (text != oldText && lines.Length == 2)
            {
                if (lines[0] == "-" && lines[1] == "-")
                    return string.Empty;
                if (lines[0].StartsWith("-") && lines[0].Length > 1 && lines[1].Trim() == "-")
                    return lines[0].Remove(0, 1).Trim();
                if (lines[1].StartsWith("-") && lines[1].Length > 1 && lines[0].Trim() == "-")
                    return lines[1].Remove(0, 1).Trim();
                if (lines[0].Length > 1 && (lines[1] == "-") || lines[1] == "." || lines[1] == "!" || lines[1] == "?")
                {
                    if (oldText.Contains(Environment.NewLine + "-") && lines[0].StartsWith("-"))
                        lines[0] = lines[0].Remove(0, 1);
                    return lines[0].Trim();
                }
            }

            return text;
        }

        private string RemoveStartEndTags(string text)
        {
            string newText = text;
            string s = text;
            if (s.StartsWith("[") && s.IndexOf("]") > 0 && checkBoxRemoveTextBetweenSquares.Checked)
                newText = s.Remove(0, s.IndexOf("]") + 1);
            else if (s.StartsWith("{") && s.IndexOf("}") > 0 && checkBoxRemoveTextBetweenBrackets.Checked)
                newText = s.Remove(0, s.IndexOf("}") + 1);
            else if (s.StartsWith("?") && s.IndexOf("?", 1) > 0 && checkBoxRemoveTextBetweenQuestionMarks.Checked)
                newText = s.Remove(0, s.IndexOf("?", 1) + 1);
            else if (s.StartsWith("(") && s.IndexOf(")") > 0 && checkBoxRemoveTextBetweenParentheses.Checked)
                newText = s.Remove(0, s.IndexOf(")") + 1);
            else if (s.StartsWith("[") && s.IndexOf("]:") > 0 && checkBoxRemoveTextBetweenSquares.Checked)
                newText = s.Remove(0, s.IndexOf("]:") + 2);
            else if (s.StartsWith("{") && s.IndexOf("}:") > 0 && checkBoxRemoveTextBetweenBrackets.Checked)
                newText = s.Remove(0, s.IndexOf("}:") + 2);
            else if (s.StartsWith("?") && s.IndexOf("?:", 1) > 0 && checkBoxRemoveTextBetweenQuestionMarks.Checked)
                newText = s.Remove(0, s.IndexOf("?:") + 2);
            else if (s.StartsWith("(") && s.IndexOf("):") > 0 && checkBoxRemoveTextBetweenParentheses.Checked)
                newText = s.Remove(0, s.IndexOf("):") + 2);
            else if (checkBoxRemoveTextBetweenCustomTags.Checked &&
                     s.Length > 0 && comboBoxCustomEnd.Text.Length > 0 && comboBoxCustomStart.Text.Length > 0 &&
                     s.StartsWith(comboBoxCustomStart.Text) && s.LastIndexOf(comboBoxCustomEnd.Text) > 0)
                newText = s.Remove(0, s.LastIndexOf(comboBoxCustomEnd.Text) + comboBoxCustomEnd.Text.Length);
            if (newText != text)
                newText = newText.TrimStart(' ');
            return newText;
        }

        private void AddToListView(Paragraph p, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };
            if (_warnings != null && _warnings.Contains(_warningIndex))
            {
                item.UseItemStyleForSubItems = true;
                item.BackColor = Color.PeachPuff;
            }
            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, p.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, newText.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);

            listViewFixes.Items.Add(item);
        }

        private void FormRemoveTextForHearImpaired_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        public int RemoveTextFromHearImpaired()
        {
            int count = 0;
            for (int i = _subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                if (IsFixAllowed(p))
                {
                    string prevText = string.Empty;
                    var prev = _subtitle.GetParagraphOrDefault(i - 1);
                    if (prev != null)
                        prevText = prev.Text;

                    string newText = RemoveTextFromHearImpaired(p.Text, prevText);
                    if (string.IsNullOrEmpty(newText))
                    {
                        _subtitle.Paragraphs.RemoveAt(i);
                    }
                    else
                    {
                        p.Text = newText;
                    }
                    count++;
                }
            }
            return count;
        }

        private bool IsFixAllowed(Paragraph p)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.Tag.ToString() == p.ToString())
                    return item.Checked;
            }
            return false;
        }

        private void CheckBoxRemoveTextBetweenCheckedChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void checkBoxRemoveInterjections_CheckedChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void buttonEditInterjections_Click(object sender, EventArgs e)
        {
            Interjections editInterjections = new Interjections();
            editInterjections.Initialize(Configuration.Settings.Tools.Interjections);
            if (editInterjections.ShowDialog(this) == DialogResult.OK)
            {
                Configuration.Settings.Tools.Interjections = editInterjections.GetInterjectionsSemiColonSeperatedString();
                if (checkBoxRemoveInterjections.Checked)
                {
                    Cursor = Cursors.WaitCursor;
                    GeneratePreview();
                    Cursor = Cursors.Default;
                }
            }
        }

        private void FormRemoveTextForHearImpaired_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets = checkBoxRemoveTextBetweenSquares.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses = checkBoxRemoveTextBetweenParentheses.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets = checkBoxRemoveTextBetweenBrackets.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks = checkBoxRemoveTextBetweenQuestionMarks.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom = checkBoxRemoveTextBetweenCustomTags.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore = comboBoxCustomStart.Text;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter = comboBoxCustomEnd.Text;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeperateLines = checkBoxOnlyIfInSeparateLine.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon = checkBoxRemoveTextBeforeColon.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase = checkBoxRemoveTextBeforeColonOnlyUppercase.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine = checkBoxColonSeparateLine.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections = checkBoxRemoveInterjections.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContains = checkBoxRemoveWhereContains.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase = checkBoxRemoveIfAllUppercase.Checked;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveIfContainsText = comboBoxRemoveIfTextContains.Text;
        }

        private void FormRemoveTextForHearImpaired_Resize(object sender, EventArgs e)
        {
            int availableWidth = listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width + 20);
            listViewFixes.Columns[2].Width = availableWidth / 2;
            listViewFixes.Columns[3].Width = availableWidth / 2;
        }

        private void checkBoxRemoveTextBeforeColon_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxRemoveTextBeforeColonOnlyUppercase.Enabled = checkBoxRemoveTextBeforeColon.Checked;
            checkBoxColonSeparateLine.Enabled = checkBoxRemoveTextBeforeColon.Checked;
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private string RemoveLineIfAllUppercase(string text)
        {
            if (!checkBoxRemoveIfAllUppercase.Checked)
                return text;

            string[] lines = text.Replace(Environment.NewLine, "\n").Replace("\r", "\n").Split('\n');
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                string lineNoHtml = Utilities.RemoveHtmlTags(line);
                string tmp = lineNoHtml.TrimEnd(new char[] { '.', '!', '?', ':', }).Trim();
                if (lineNoHtml != lineNoHtml.ToLower() && lineNoHtml == lineNoHtml.ToUpper())
                {
                    if (tmp == "YES" || tmp == "NO" || tmp == "WHY" || tmp == "HI" || tmp.Length == 1)
                    {
                        sb.AppendLine(line);
                    }
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString().Trim();
        }

        private void checkBoxRemoveIfAllUppercase_CheckedChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }
    }
}
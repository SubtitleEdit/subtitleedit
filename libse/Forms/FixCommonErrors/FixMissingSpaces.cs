using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixMissingSpaces : IFixCommonError
    {

        private static readonly Regex FixMissingSpacesReComma = new Regex(@"[^\s\d],[^\s]", RegexOptions.Compiled);
        private static readonly Regex FixMissingSpacesRePeriod = new Regex(@"[a-z][a-z][.][a-zA-Z]", RegexOptions.Compiled);
        private static readonly Regex FixMissingSpacesReQuestionMark = new Regex(@"[^\s\d]\?[a-zA-Z]", RegexOptions.Compiled);
        private static readonly Regex FixMissingSpacesReExclamation = new Regex(@"[^\s\d]\![a-zA-Z]", RegexOptions.Compiled);
        private static readonly Regex FixMissingSpacesReColon = new Regex(@"[^\s\d]\:[a-zA-Z]", RegexOptions.Compiled);
        private static readonly Regex Url = new Regex(@"\w\.(?:com|net|org)\b", RegexOptions.Compiled);

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string languageCode = callbacks.Language;
            string fixAction = language.FixMissingSpace;
            int missingSpaces = 0;
            const string expectedChars = @"""”<.";
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];

                // missing space after comma ","
                Match match = FixMissingSpacesReComma.Match(p.Text);
                while (match.Success)
                {
                    bool doFix = !expectedChars.Contains(p.Text[match.Index + 2]);

                    if (doFix && languageCode == "el" && (p.Text.Substring(match.Index).StartsWith("ό,τι", StringComparison.Ordinal) || p.Text.Substring(match.Index).StartsWith("ο,τι", StringComparison.Ordinal)))
                        doFix = false;

                    if (doFix && callbacks.AllowFix(p, fixAction))
                    {
                        missingSpaces++;
                        string oldText = p.Text;
                        p.Text = p.Text.Replace(match.Value, match.Value[0] + ", " + match.Value[match.Value.Length - 1]);
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    match = match.NextMatch();
                }

                bool allowFix = callbacks.AllowFix(p, fixAction);

                // missing space after "?"
                match = FixMissingSpacesReQuestionMark.Match(p.Text);
                while (match.Success)
                {
                    if (allowFix && !@"""<".Contains(p.Text[match.Index + 2]))
                    {
                        missingSpaces++;
                        string oldText = p.Text;
                        p.Text = p.Text.Replace(match.Value, match.Value[0] + "? " + match.Value[match.Value.Length - 1]);
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    match = FixMissingSpacesReQuestionMark.Match(p.Text, match.Index + 1);
                }

                // missing space after "!"
                match = FixMissingSpacesReExclamation.Match(p.Text);
                while (match.Success)
                {
                    if (allowFix && !@"""<".Contains(p.Text[match.Index + 2]))
                    {
                        missingSpaces++;
                        string oldText = p.Text;
                        p.Text = p.Text.Replace(match.Value, match.Value[0] + "! " + match.Value[match.Value.Length - 1]);
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    match = FixMissingSpacesReExclamation.Match(p.Text, match.Index + 1);
                }

                // missing space after ":"
                match = FixMissingSpacesReColon.Match(p.Text);
                while (match.Success)
                {
                    int start = match.Index;
                    start -= 4;
                    if (start < 0)
                        start = 0;
                    int indexOfStartCodeTag = p.Text.IndexOf('{', start);
                    int indexOfEndCodeTag = p.Text.IndexOf('}', start);
                    if (indexOfStartCodeTag >= 0 && indexOfEndCodeTag >= 0 && indexOfStartCodeTag < match.Index)
                    {
                        // we are inside a tag: like indexOfEndCodeTag "{y:i}Is this italic?"
                    }
                    else if (allowFix && !@"""<".Contains(p.Text[match.Index + 2]))
                    {
                        missingSpaces++;
                        string oldText = p.Text;
                        p.Text = p.Text.Replace(match.Value, match.Value[0] + ": " + match.Value[match.Value.Length - 1]);
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    match = FixMissingSpacesReColon.Match(p.Text, match.Index + 1);
                }

                // missing space after period "."
                match = FixMissingSpacesRePeriod.Match(p.Text);
                while (match.Success)
                {
                    if (!p.Text.Contains("www.", StringComparison.OrdinalIgnoreCase) &&
                        !p.Text.Contains("http://", StringComparison.OrdinalIgnoreCase) &&
                        !Url.IsMatch(p.Text)) // Skip urls.
                    {
                        bool isMatchAbbreviation = false;

                        string word = GetWordFromIndex(p.Text, match.Index);
                        if (Utilities.CountTagInText(word, '.') > 1)
                            isMatchAbbreviation = true;

                        if (!isMatchAbbreviation && word.Contains('@')) // skip emails
                            isMatchAbbreviation = true;

                        if (match.Value.Equals("h.d", StringComparison.OrdinalIgnoreCase) && match.Index > 0 && p.Text.Substring(match.Index - 1, 4).Equals("ph.d", StringComparison.OrdinalIgnoreCase))
                            isMatchAbbreviation = true;

                        if (!isMatchAbbreviation && callbacks.AllowFix(p, fixAction))
                        {
                            missingSpaces++;
                            string oldText = p.Text;
                            p.Text = p.Text.Replace(match.Value, match.Value.Replace(".", ". "));
                            callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                        }
                    }
                    match = match.NextMatch();
                }

                if (!p.Text.StartsWith("--", StringComparison.Ordinal))
                {
                    var arr = p.Text.SplitToLines();
                    if (arr.Length == 2 && arr[0].Length > 1 && arr[1].Length > 1)
                    {
                        if (arr[0][0] == '-' && arr[0][1] != ' ')
                            arr[0] = arr[0].Insert(1, " ");
                        if (arr[0].Length > 6 && arr[0].StartsWith("<i>-", StringComparison.OrdinalIgnoreCase) && arr[0][4] != ' ')
                            arr[0] = arr[0].Insert(4, " ");
                        if (arr[1][0] == '-' && arr[1][1] != ' ' && arr[1][1] != '-')
                            arr[1] = arr[1].Insert(1, " ");
                        if (arr[1].Length > 6 && arr[1].StartsWith("<i>-", StringComparison.OrdinalIgnoreCase) && arr[1][4] != ' ')
                            arr[1] = arr[1].Insert(4, " ");
                        string newText = arr[0] + Environment.NewLine + arr[1];
                        if (newText != p.Text && callbacks.AllowFix(p, fixAction))
                        {
                            missingSpaces++;
                            string oldText = p.Text;
                            p.Text = newText;
                            callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                        }
                    }
                }

                //fix missing spaces before/after quotes - Get a"get out of jail free"card. -> Get a "get out of jail free" card.
                if (Utilities.CountTagInText(p.Text, '"') == 2)
                {
                    int start = p.Text.IndexOf('"');
                    int end = p.Text.LastIndexOf('"');
                    string quote = p.Text.Substring(start, end - start + 1);
                    if (!quote.Contains(Environment.NewLine))
                    {
                        string newText = p.Text;
                        int indexOfFontTag = newText.IndexOf("<font ", StringComparison.OrdinalIgnoreCase);
                        bool isAfterAssTag = newText.Contains("{\\") && start > 0 && newText[start - 1] == '}';
                        if (!isAfterAssTag && start > 0 && !(Environment.NewLine + @" >[(♪♫¿").Contains(p.Text[start - 1]))
                        {
                            if (indexOfFontTag < 0 || start > newText.IndexOf('>', indexOfFontTag)) // font tags can contain "
                            {
                                newText = newText.Insert(start, " ");
                                end++;
                            }
                        }
                        if (end < newText.Length - 2 && !(Environment.NewLine + @" <,.!?:;])♪♫¿").Contains(p.Text[end + 1]))
                        {
                            if (indexOfFontTag < 0 || end > newText.IndexOf('>', indexOfFontTag)) // font tags can contain "
                            {
                                newText = newText.Insert(end + 1, " ");
                            }
                        }
                        if (newText != p.Text && callbacks.AllowFix(p, fixAction))
                        {
                            missingSpaces++;
                            string oldText = p.Text;
                            p.Text = newText;
                            callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                        }
                    }
                }

                //fix missing spaces before/after music quotes - #He's so happy# -> #He's so happy#
                if (p.Text.Length > 5 && p.Text.Contains(new[] { '#', '♪', '♫' }))
                {
                    string newText = p.Text;
                    if (@"#♪♫".Contains(newText[0]) && !@" <".Contains(newText[1]) && !newText.Substring(1).StartsWith(Environment.NewLine) &&
                        !newText.Substring(1).StartsWith('♪') && !newText.Substring(1).StartsWith('♫'))
                        newText = newText.Insert(1, " ");
                    if (@"#♪♫".Contains(newText[newText.Length - 1]) && !@" >".Contains(newText[newText.Length - 2]) &&
                        !newText.Substring(0, newText.Length - 1).EndsWith(Environment.NewLine, StringComparison.Ordinal) && !newText.Substring(0, newText.Length - 1).EndsWith('♪') &&
                        !newText.Substring(0, newText.Length - 1).EndsWith('♫'))
                        newText = newText.Insert(newText.Length - 1, " ");
                    if (newText != p.Text && callbacks.AllowFix(p, fixAction))
                    {
                        missingSpaces++;
                        string oldText = p.Text;
                        p.Text = newText;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

                //fix missing spaces in "Hey...move it!" to "Hey... move it!"
                int index = p.Text.IndexOf("...", StringComparison.Ordinal);
                if (index >= 0 && p.Text.Length > 5)
                {
                    string newText = p.Text;
                    while (index != -1)
                    {
                        if (newText.Length > index + 4 && index >= 1)
                        {
                            if (Utilities.AllLettersAndNumbers.Contains(newText[index + 3]) &&
                                Utilities.AllLettersAndNumbers.Contains(newText[index - 1]))
                                newText = newText.Insert(index + 3, " ");
                        }
                        index = newText.IndexOf("...", index + 2, StringComparison.Ordinal);
                    }
                    if (newText != p.Text && callbacks.AllowFix(p, fixAction))
                    {
                        missingSpaces++;
                        string oldText = p.Text;
                        p.Text = newText;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

                //fix missing spaces in "The<i>Bombshell</i> will gone." to "The <i>Bombshell</i> will gone."
                index = p.Text.IndexOf("<i>", StringComparison.OrdinalIgnoreCase);
                if (index >= 0 && p.Text.Length > 5)
                {
                    string newText = p.Text;
                    while (index != -1)
                    {
                        if (newText.Length > index + 6 && index > 1)
                        {
                            if (Utilities.AllLettersAndNumbers.Contains(newText[index + 3]) &&
                                Utilities.AllLettersAndNumbers.Contains(newText[index - 1]))
                                newText = newText.Insert(index, " ");
                        }
                        index = newText.IndexOf("<i>", index + 3, StringComparison.OrdinalIgnoreCase);
                    }
                    if (newText != p.Text && callbacks.AllowFix(p, fixAction))
                    {
                        missingSpaces++;
                        string oldText = p.Text;
                        p.Text = newText;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

                //fix missing spaces in "The <i>Bombshell</i>will gone." to "The <i>Bombshell</i> will gone."
                index = p.Text.IndexOf("</i>", StringComparison.OrdinalIgnoreCase);
                if (index > 3 && p.Text.Length > 5)
                {
                    string newText = p.Text;
                    while (index != -1)
                    {
                        if (newText.Length > index + 6 && index > 1)
                        {
                            if (Utilities.AllLettersAndNumbers.Contains(newText[index + 4]) &&
                                Utilities.AllLettersAndNumbers.Contains(newText[index - 1]))
                                newText = newText.Insert(index + 4, " ");
                        }
                        index = newText.IndexOf("</i>", index + 4, StringComparison.OrdinalIgnoreCase);
                    }
                    if (newText != p.Text && callbacks.AllowFix(p, fixAction))
                    {
                        missingSpaces++;
                        string oldText = p.Text;
                        p.Text = newText;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

                if (callbacks.Language == "fr") // special rules for French
                {
                    string newText = p.Text;
                    int j = 1;
                    while (j < newText.Length)
                    {
                        if (@"!?:;".Contains(newText[j]))
                        {
                            if (Utilities.AllLetters.Contains(newText[j - 1]))
                            {
                                newText = newText.Insert(j, " ");
                                j++;
                            }
                        }
                        j++;
                    }
                    if (newText != p.Text && callbacks.AllowFix(p, fixAction))
                    {
                        missingSpaces++;
                        string oldText = p.Text;
                        p.Text = newText;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(missingSpaces, language.FixMissingSpaces, string.Format(language.XMissingSpacesAdded, missingSpaces));
        }

        private static string GetWordFromIndex(string text, int index)
        {
            if (string.IsNullOrEmpty(text) || index < 0 || index >= text.Length)
                return string.Empty;

            int endIndex = index;
            for (int i = index; i < text.Length; i++)
            {
                if ((@" " + Environment.NewLine).Contains(text[i]))
                    break;
                endIndex = i;
            }

            int startIndex = index;
            for (int i = index; i >= 0; i--)
            {
                if ((@" " + Environment.NewLine).Contains(text[i]))
                    break;
                startIndex = i;
            }

            return text.Substring(startIndex, endIndex - startIndex + 1);
        }

    }
}

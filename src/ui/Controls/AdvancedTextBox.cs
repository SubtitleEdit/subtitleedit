using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SpellCheck;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SpellCheck;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    /// <summary>
    /// RichTextBox with syntax highlighting and spell check.
    /// </summary>
    public sealed class AdvancedTextBox : RichTextBox, IDoSpell
    {
        private bool _checkRtfChange = true;
        private int _mouseMoveSelectionLength;

        private bool IsLiveSpellCheckEnabled => Configuration.Settings.Tools.LiveSpellCheck && Parent?.Name == Main.MainTextBox;
        private Hunspell _hunspell;
        private SpellCheckWordLists _spellCheckWordLists;
        private List<SpellCheckWord> _words;
        private List<SpellCheckWord> _wrongWords;
        private List<string> _skipAllList;
        private HashSet<string> _skipOnceList;
        private SpellCheckWord _currentWord;
        private string _currentDictionary;
        private string _uiTextBoxOldText;

        private static readonly char[] SplitChars = { ' ', '.', ',', '?', '!', ':', ';', '"', '“', '”', '(', ')', '[', ']', '{', '}', '|', '<', '>', '/', '+', '¿', '¡', '…', '—', '–', '♪', '♫', '„', '«', '»', '‹', '›', '؛', '،', '؟' };

        public int CurrentLineIndex { get; set; }
        public string CurrentLanguage { get; set; }
        public bool LanguageChanged { get; set; }
        public bool IsWrongWord { get; set; }
        public bool IsSpellCheckerInitialized { get; set; }
        public bool IsDictionaryDownloaded { get; set; } = true;
        public bool IsSpellCheckRequested { get; set; }

        public class SuggestionParameter
        {
            public string InputWord { get; set; }
            public List<string> Suggestions { get; set; }
            public Hunspell Hunspell { get; set; }
            public bool Success { get; set; }

            public SuggestionParameter(string word, Hunspell hunspell)
            {
                InputWord = word;
                Suggestions = new List<string>();
                Hunspell = hunspell;
                Success = false;
            }
        }

        public AdvancedTextBox()
        {
            DetectUrls = false;

            SetTextPosInRtbIfCentered();

            // Live spell check events.
            KeyPress += UiTextBox_KeyPress;
            KeyDown += UiTextBox_KeyDown;
            MouseDown += UiTextBox_MouseDown;

            TextChanged += TextChangedHighlight;
            HandleCreated += (sender, args) =>
            {
                SetTextPosInRtbIfCentered();
            };
            MouseDown += (sender, args) =>
            {
                // avoid selection when centered and clicking to the left
                var charIndex = GetCharIndexFromPosition(args.Location);
                if (Configuration.Settings.General.CenterSubtitleInTextBox &&
                    _mouseMoveSelectionLength == 0 &&
                    (charIndex == 0 || charIndex >= 0 && base.Text[charIndex - 1] == '\n'))
                {
                    SelectionLength = 0;
                }
            };
            MouseMove += (sender, args) =>
            {
                _mouseMoveSelectionLength = SelectionLength;
            };
            KeyDown += (sender, args) =>
            {
                // fix annoying "beeps" when moving cursor position
                var startOfLineDirection = Keys.Left;
                var endOfLineDirection = Keys.Right;
                if (Configuration.Settings.General.RightToLeftMode)
                {
                    startOfLineDirection = Keys.Right;
                    endOfLineDirection = Keys.Left;
                }

                if ((args.KeyData == startOfLineDirection || args.KeyData == Keys.PageUp) && SelectionStart == 0 && SelectionLength == 0)
                {
                    args.SuppressKeyPress = true;
                }
                else if (args.KeyData == Keys.Up && SelectionStart <= Text.IndexOf('\n'))
                {
                    args.SuppressKeyPress = true;
                }
                else if (args.KeyData == Keys.Home && (SelectionStart == 0 || SelectionStart > 0 && Text[SelectionStart - 1] == '\n'))
                {
                    args.SuppressKeyPress = true;
                }
                else if (args.KeyData == (Keys.Home | Keys.Control) && SelectionStart == 0)
                {
                    args.SuppressKeyPress = true;
                }
                else if (args.KeyData == Keys.End && (SelectionStart >= Text.Length || SelectionStart + 1 < Text.Length && Text[SelectionStart + 1] == '\n'))
                {
                    args.SuppressKeyPress = true;
                }
                else if (args.KeyData == (Keys.End | Keys.Control) && SelectionStart >= Text.Length)
                {
                    args.SuppressKeyPress = true;
                }
                else if (args.KeyData == endOfLineDirection && SelectionStart >= Text.Length)
                {
                    if (IsLiveSpellCheckEnabled)
                    {
                        IsSpellCheckRequested = true;
                        TextChangedHighlight(this, EventArgs.Empty);
                    }

                    args.SuppressKeyPress = true;
                }
                else if (args.KeyData == Keys.Down && SelectionStart >= Text.Length)
                {
                    args.SuppressKeyPress = true;
                }
                else if (args.KeyData == Keys.PageDown && SelectionStart >= Text.Length)
                {
                    args.SuppressKeyPress = true;
                }
            };
        }

        private bool _fixedArabicComma;
        public override string Text
        {
            get
            {
                var s = base.Text;
                if (_fixedArabicComma)
                {
                    s = s.Replace("\u202A،", "،");
                    s = s.Replace("،\u202A", "،");
                }

                return string.Join(Environment.NewLine, s.SplitToLines());
            }
            set
            {
                _fixedArabicComma = false;
                var s = value ?? string.Empty;
                if (!Configuration.Settings.General.RightToLeftMode && !s.ContainsUnicodeControlChars())
                {
                    string textNoTags = HtmlUtil.RemoveHtmlTags(s, true);
                    if (textNoTags.EndsWith('،'))
                    {
                        s = Regex.Replace(s, @"،(?=(?:<[^>]+>)?(?:{[^}]+})?$)", "\u202A،");
                    }
                    else if (textNoTags.StartsWith('،'))
                    {
                        s = Regex.Replace(s, @"(?<=^(?:{[^}]+})?(?:<[^>]+>)?)،", "،\u202A");
                    }

                    _fixedArabicComma = true;
                }

                base.Text = string.Join("\n", s.SplitToLines());
            }
        }

        public new int SelectionStart
        {
            get
            {
                var text = base.Text;
                var extra = 0;
                var target = base.SelectionStart;
                for (int i = 0; i < target && i < text.Length; i++)
                {
                    if (text[i] == '\n')
                    {
                        extra++;
                    }
                }

                return target + extra;
            }
            set
            {
                var text = base.Text;
                var extra = 0;
                for (int i = 0; i < value && i < text.Length; i++)
                {
                    if (text[i] == '\n')
                    {
                        extra++;
                    }
                }

                base.SelectionStart = value - extra;
            }
        }

        public new int SelectionLength
        {
            get
            {
                var target = base.SelectionLength;
                if (target == 0)
                {
                    return 0;
                }

                var text = base.Text;
                var extra = 0;
                var start = SelectionStart;
                for (int i = start; i < target + start && i < text.Length; i++)
                {
                    if (text[i] == '\n')
                    {
                        extra++;
                    }
                }

                return target + extra;
            }
            set
            {
                var target = value;
                if (target == 0)
                {
                    base.SelectionLength = 0;
                    return;
                }

                var text = base.Text;
                var extra = 0;
                var start = SelectionStart;
                for (int i = start; i < target + start && i < text.Length; i++)
                {
                    if (text[i] == '\n')
                    {
                        extra++;
                    }
                }

                base.SelectionLength = target - extra;
            }
        }

        public new string SelectedText
        {
            get => string.Join(Environment.NewLine, base.SelectedText.SplitToLines());
            set => base.SelectedText = value;
        }

        private int GetIndexWithLineBreak(int index)
        {
            var text = base.Text;
            var extra = 0;
            for (int i = 0; i < index && i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    extra++;
                }
            }

            return index - extra;
        }

        private void DoFormattingActionOnRtb(Action formattingAction)
        {
            this.BeginRichTextBoxUpdate();
            var start = SelectionStart;
            var length = SelectionLength;
            formattingAction();
            SelectionStart = start;
            SelectionLength = length;
            this.EndRichTextBoxUpdate();
        }

        private void SetTextPosInRtbIfCentered()
        {
            if (Configuration.Settings.General.CenterSubtitleInTextBox)
            {
                DoFormattingActionOnRtb(() =>
                {
                    SelectAll();
                    SelectionAlignment = HorizontalAlignment.Center;
                });
            }
        }

        private void TagsChangedCheck()
        {
            // Request spell check if there is a change in tags to update highlighting indices.
            if (!string.IsNullOrEmpty(_uiTextBoxOldText)
                && HtmlUtil.RemoveHtmlTags(_uiTextBoxOldText, true) == HtmlUtil.RemoveHtmlTags(Text, true))
            {
                IsSpellCheckRequested = true;
            }

            _uiTextBoxOldText = Text;
        }

        private void TextChangedHighlight(object sender, EventArgs e)
        {
            if (_checkRtfChange)
            {
                _checkRtfChange = false;

                if (IsLiveSpellCheckEnabled)
                {
                    TagsChangedCheck();
                    DoFormattingActionOnRtb(() =>
                    {
                        HighlightHtmlText();
                        HighlightSpellCheckWords();
                    });
                }
                else
                {
                    DoFormattingActionOnRtb(HighlightHtmlText);
                }

                _checkRtfChange = true;
            }
        }

        private void HighlightHtmlText()
        {
            SelectAll();
            SelectionColor = ForeColor;
            SelectionBackColor = BackColor;

            bool htmlTagOn = false;
            bool htmlTagFontOn = false;
            int htmlTagStart = -1;
            bool assaTagOn = false;
            bool assaPrimaryColorTagOn = false;
            bool assaSecondaryColorTagOn = false;
            bool assaBorderColorTagOn = false;
            bool assaShadowColorTagOn = false;
            var assaTagStart = -1;
            int tagOn = -1;
            var text = Text;
            var textLength = text.Length;
            int i = 0;

            while (i < textLength)
            {
                var ch = text[i];
                if (assaTagOn)
                {
                    if (ch == '}' && tagOn >= 0)
                    {
                        assaTagOn = false;
                        SelectionStart = assaTagStart;
                        SelectionLength = i - assaTagStart + 1;
                        SelectionColor = Configuration.Settings.General.SubtitleTextBoxAssColor;
                        if (assaTagStart >= 0)
                        {
                            if (assaPrimaryColorTagOn)
                            {
                                var colorTag = text.IndexOf("\\c", assaTagStart, StringComparison.OrdinalIgnoreCase) != -1 ? "\\c" : "\\1c";
                                SetAssaColor(text, assaTagStart, colorTag);
                                assaPrimaryColorTagOn = false;
                            }

                            if (assaSecondaryColorTagOn)
                            {
                                SetAssaColor(text, assaTagStart, "\\2c");
                                assaSecondaryColorTagOn = false;
                            }

                            if (assaBorderColorTagOn)
                            {
                                SetAssaColor(text, assaTagStart, "\\3c");
                                assaBorderColorTagOn = false;
                            }

                            if (assaShadowColorTagOn)
                            {
                                SetAssaColor(text, assaTagStart, "\\4c");
                                assaShadowColorTagOn = false;
                            }
                        }

                        assaTagStart = -1;
                    }
                }
                else if (htmlTagOn)
                {
                    if (ch == '>' && tagOn >= 0)
                    {
                        htmlTagOn = false;
                        SelectionStart = htmlTagStart;
                        SelectionLength = i - htmlTagStart + 1;
                        SelectionColor = Configuration.Settings.General.SubtitleTextBoxHtmlColor;
                        if (htmlTagFontOn && htmlTagStart >= 0)
                        {
                            SetHtmlColor(text, htmlTagStart);
                            htmlTagFontOn = false;
                        }
                        htmlTagStart = -1;
                    }
                }
                else if (ch == '{' && i < textLength - 1 && text[i + 1] == '\\' && text.IndexOf('}', i) > 0)
                {
                    var s = text.Substring(i);
                    assaTagOn = true;
                    tagOn = i;
                    assaTagStart = i;
                    assaPrimaryColorTagOn = s.Contains("\\c", StringComparison.OrdinalIgnoreCase) || s.Contains("\\1c", StringComparison.OrdinalIgnoreCase);
                    assaSecondaryColorTagOn = s.Contains("\\2c", StringComparison.OrdinalIgnoreCase);
                    assaBorderColorTagOn = s.Contains("\\3c", StringComparison.OrdinalIgnoreCase);
                    assaShadowColorTagOn = s.Contains("\\4c", StringComparison.OrdinalIgnoreCase);
                }
                else if (ch == '<')
                {
                    var s = text.Substring(i);
                    if (s.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<b>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<u>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</i>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</b>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</u>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<box>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</box>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</font>", StringComparison.OrdinalIgnoreCase) ||
                        (s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase) &&
                         text.IndexOf("</font>", i, StringComparison.OrdinalIgnoreCase) > 0))
                    {
                        htmlTagOn = true;
                        htmlTagStart = i;
                        htmlTagFontOn = s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase);
                        tagOn = i;
                    }
                }

                i++;
            }
        }

        private void SetHtmlColor(string text, int htmlTagStart)
        {
            int colorStart = text.IndexOf(" color=", htmlTagStart, StringComparison.OrdinalIgnoreCase);
            if (colorStart > 0)
            {
                colorStart += " color=".Length;
                if (text[colorStart] == '"' || text[colorStart] == '\'')
                {
                    colorStart++;
                }

                int colorEnd = text.IndexOf('"', colorStart + 1);
                if (colorEnd > 0)
                {
                    var color = text.Substring(colorStart, colorEnd - colorStart);
                    try
                    {
                        Color c;
                        if (color.StartsWith("rgb(", StringComparison.Ordinal))
                        {
                            var arr = color.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            c = Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                        }
                        else
                        {
                            c = ColorTranslator.FromHtml(color);
                        }

                        SetForeColorAndChangeBackColorIfClose(colorStart, colorEnd, c);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private void SetAssaColor(string text, int assaTagStart, string colorTag)
        {
            int colorStart = text.IndexOf(colorTag, assaTagStart, StringComparison.OrdinalIgnoreCase);
            if (colorStart > 0)
            {
                colorStart += colorTag.Length;
                if (text[colorStart] == '&')
                {
                    colorStart++;
                }
                if (text[colorStart] == 'H')
                {
                    colorStart++;
                }

                int colorEnd = text.IndexOfAny(new[] { '}', '\\', '&' }, colorStart + 1);
                if (colorEnd > 0)
                {
                    var color = text.Substring(colorStart, colorEnd - colorStart);
                    try
                    {
                        if (color.Length > 0 && color.Length < 6)
                        {
                            color = color.PadLeft(6, '0');
                        }

                        if (color.Length == 6)
                        {
                            var rgbColor = string.Concat("#", color[4], color[5], color[2], color[3], color[0], color[1]); var c = ColorTranslator.FromHtml(rgbColor);
                            SetForeColorAndChangeBackColorIfClose(colorStart, colorEnd, c);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private void SetForeColorAndChangeBackColorIfClose(int colorStart, int colorEnd, Color c)
        {
            var backColor = BackColor;
            SelectionStart = colorStart;
            SelectionLength = colorEnd - colorStart;
            SelectionColor = c;

            var diff = Math.Abs(c.R - backColor.R) + Math.Abs(c.G - backColor.G) + Math.Abs(c.B - backColor.B);
            if (diff < 60)
            {
                SelectionBackColor = Color.FromArgb(byte.MaxValue - c.R, byte.MaxValue - c.G, byte.MaxValue - c.B, byte.MaxValue - c.R);
            }
        }

        private const int WM_PAINT = 0x0F;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_PAINT)
            {
                if (!Enabled && Configuration.Settings.General.UseDarkTheme)
                {
                    using (var g = Graphics.FromHwnd(Handle))
                    using (var sb = new SolidBrush(BackColor))
                    {
                        g.FillRectangle(sb, ClientRectangle);
                    }
                }
            }
        }

        #region LiveSpellCheck

        public async Task CheckForLanguageChange(Subtitle subtitle)
        {
            var detectedLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle, 100);
            if (CurrentLanguage != detectedLanguage)
            {
                DisposeHunspellAndDictionaries();
                await InitializeLiveSpellCheck(subtitle, CurrentLineIndex);
            }
        }

        public async Task InitializeLiveSpellCheck(Subtitle subtitle, int lineNumber)
        {
            if (lineNumber < 0)
            {
                return;
            }

            if (_spellCheckWordLists is null && _hunspell is null)
            {
                var detectedLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle, 300);
                var downloadedDictionaries = Utilities.GetDictionaryLanguagesCultureNeutral();
                var isDictionaryAvailable = false;
                foreach (var downloadedDictionary in downloadedDictionaries)
                {
                    if (downloadedDictionary.Contains($"[{detectedLanguage}]"))
                    {
                        isDictionaryAvailable = true;
                        break;
                    }
                }

                if (isDictionaryAvailable)
                {
                    IsDictionaryDownloaded = true;

                    var languageName = LanguageAutoDetect.AutoDetectLanguageName(string.Empty, subtitle);
                    if (languageName.Split('_', '-')[0] != detectedLanguage)
                    {
                        return;
                    }

                    await LoadDictionariesAsync(languageName);
                    IsSpellCheckerInitialized = true;
                    IsSpellCheckRequested = true;
                    TextChangedHighlight(this, EventArgs.Empty);
                }
                else
                {
                    IsDictionaryDownloaded = false;
                }

                LanguageChanged = true;
                CurrentLanguage = detectedLanguage;
            }
        }

        private async Task LoadDictionariesAsync(string languageName) =>
            await Task.Run(() => LoadDictionaries(languageName));

        private void LoadDictionaries(string languageName)
        {
            var dictionaryFolder = Utilities.DictionaryFolder;
            string dictionary = Utilities.DictionaryFolder + languageName;
            _spellCheckWordLists = new SpellCheckWordLists(dictionaryFolder, languageName, this);
            _skipAllList = new List<string>();
            _skipOnceList = new HashSet<string>();
            LoadHunspell(dictionary);
        }

        private void LoadHunspell(string dictionary)
        {
            _currentDictionary = dictionary;
            _hunspell?.Dispose();
            _hunspell = null;
            _hunspell = Hunspell.GetHunspell(dictionary);
        }

        public void DisposeHunspellAndDictionaries()
        {
            if (IsSpellCheckerInitialized)
            {
                _skipAllList = null;
                _skipOnceList = null;
                _spellCheckWordLists = null;
                _words = null;
                _wrongWords = null;
                _currentWord = null;
                _currentDictionary = null;
                CurrentLanguage = null;
                _hunspell?.Dispose();
                _hunspell = null;
                IsWrongWord = false;
                IsSpellCheckerInitialized = false;

                if (Configuration.Settings.General.SubtitleTextBoxSyntaxColor)
                {
                    IsSpellCheckRequested = true;
                    TextChangedHighlight(this, EventArgs.Empty);
                }
            }
        }

        public void DoLiveSpellCheck()
        {
            if (IsSpellCheckerInitialized && IsSpellCheckRequested)
            {
                _words = GetWords(Text);
                SetWrongWords();
            }
        }

        private List<SpellCheckWord> GetWords(string s)
        {
            s = _spellCheckWordLists.ReplaceHtmlTagsWithBlanks(s);
            s = _spellCheckWordLists.ReplaceAssTagsWithBlanks(s);
            s = _spellCheckWordLists.ReplaceKnownWordsOrNamesWithBlanks(s);
            return SpellCheckWordLists.Split(s);
        }

        private void SetWrongWords()
        {
            _wrongWords = new List<SpellCheckWord>();

            for (int i = 0; i < _words.Count; i++)
            {
                var currentWord = _words[i];
                var currentWordText = _words[i].Text;
                int minLength = 2;
                if (Configuration.Settings.Tools.CheckOneLetterWords)
                {
                    minLength = 1;
                }

                string key = CurrentLineIndex + "-" + currentWord.Text + "-" + currentWord.Index;
                if (DoSpell(currentWordText) || Utilities.IsNumber(currentWordText) || _skipAllList.Contains(currentWordText)
                    || _skipOnceList.Contains(key) || _spellCheckWordLists.HasUserWord(currentWordText) || _spellCheckWordLists.HasName(currentWordText)
                    || currentWordText.Length < minLength || currentWordText == "&")
                {
                    continue;
                }

                string prefix = string.Empty;
                string postfix = string.Empty;
                if (currentWordText.RemoveControlCharacters().Trim().Length >= minLength)
                {
                    if (currentWordText.Length > 0)
                    {
                        var trimChars = "'`*#\u200E\u200F\u202A\u202B\u202C\u202D\u202E\u200B\uFEFF";
                        var charHit = true;
                        while (charHit)
                        {
                            charHit = false;
                            foreach (char c in trimChars)
                            {
                                if (currentWordText.StartsWith(c))
                                {
                                    prefix += c;
                                    currentWordText = currentWordText.Substring(1);
                                    charHit = true;
                                }
                                if (currentWordText.EndsWith(c))
                                {
                                    postfix = c + postfix;
                                    currentWordText = currentWordText.Remove(currentWordText.Length - 1);
                                    charHit = true;
                                }
                            }
                        }
                    }
                }

                if (prefix == "'" && currentWordText.Length >= 1 && (DoSpell(prefix + currentWordText) || _spellCheckWordLists.HasUserWord(prefix + currentWordText)))
                {
                    continue;
                }
                else if (currentWordText.Length > 1)
                {
                    if ("`'".Contains(currentWordText[currentWordText.Length - 1]) && DoSpell(currentWordText.TrimEnd('\'').TrimEnd('`')))
                    {
                        continue;
                    }

                    if (currentWordText.EndsWith("'s", StringComparison.Ordinal) && currentWordText.Length > 4
                    && DoSpell(currentWordText.TrimEnd('s').TrimEnd('\'')))
                    {
                        continue;
                    }

                    if (currentWordText.EndsWith('\'') && DoSpell(currentWordText.TrimEnd('\'')))
                    {
                        continue;
                    }

                    string removeUnicode = currentWordText.Replace("\u200b", string.Empty); // zero width space
                    removeUnicode = removeUnicode.Replace("\u2060", string.Empty); // word joiner
                    removeUnicode = removeUnicode.Replace("\ufeff", string.Empty); // zero width no-break space
                    if (DoSpell(removeUnicode))
                    {
                        continue;
                    }

                    if (i > 0 && i < _words.Count && _words.ElementAtOrDefault(i + 1) != null &&
                        string.Equals(_words[i - 1].Text, "www", StringComparison.InvariantCultureIgnoreCase) &&
                        (string.Equals(_words[i + 1].Text, "com", StringComparison.InvariantCultureIgnoreCase) ||
                         string.Equals(_words[i + 1].Text, "org", StringComparison.InvariantCultureIgnoreCase) ||
                         string.Equals(_words[i + 1].Text, "net", StringComparison.InvariantCultureIgnoreCase)) &&
                        Text.IndexOf(_words[i - 1].Text + "." + currentWordText + "." +
                                     _words[i + 1].Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue; // do not spell check urls
                    }

                    if (CurrentLanguage == "ar")
                    {
                        var trimmed = currentWordText.Trim('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ',', '،');
                        if (trimmed != currentWordText)
                        {
                            if (_spellCheckWordLists.HasName(trimmed) || _skipAllList.Contains(trimmed.ToUpperInvariant())
                                || _spellCheckWordLists.HasUserWord(trimmed) || DoSpell(trimmed))
                            {
                                continue;
                            }
                        }
                    }

                    // check if dash concatenated word with previous or next word is in spell check dictionary
                    if (i > 0 && (Text[currentWord.Index - 1] == '-' || Text[currentWord.Index - 1] == '‑'))
                    {
                        var wordWithDash = _words[i - 1].Text + "-" + currentWordText;
                        if (DoSpell(wordWithDash))
                        {
                            continue;
                        }

                        wordWithDash = _words[i - 1].Text + "‑" + currentWordText; // non break hyphen
                        if (DoSpell(wordWithDash) || _spellCheckWordLists.HasUserWord(wordWithDash)
                        || _spellCheckWordLists.HasUserWord(wordWithDash.Replace("‑", "-")) || _spellCheckWordLists.HasUserWord("-" + currentWordText))
                        {
                            continue;
                        }

                    }

                    if (i < _words.Count - 1 && _words[i + 1].Index - 1 < Text.Length &&
                        (Text[_words[i + 1].Index - 1] == '-' || Text[_words[i + 1].Index - 1] == '‑'))
                    {
                        var wordWithDash = currentWordText + "-" + _words[i + 1].Text;
                        if (DoSpell(wordWithDash))
                        {
                            continue;
                        }

                        wordWithDash = currentWordText + "‑" + _words[i + 1].Text; // non break hyphen
                        if (DoSpell(wordWithDash) || _spellCheckWordLists.HasUserWord(wordWithDash) || _spellCheckWordLists.HasUserWord(wordWithDash.Replace("‑", "-")))
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    if (currentWordText == "'")
                    {
                        continue;
                    }
                    else if (CurrentLanguage == "en " && (currentWordText.Equals("a", StringComparison.OrdinalIgnoreCase) || currentWordText == "I"))
                    {
                        continue;
                    }
                    else if (CurrentLanguage == "da" && currentWordText.Equals("i", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                if (Configuration.Settings.Tools.SpellCheckEnglishAllowInQuoteAsIng && CurrentLanguage == "en"
                    && _words[i].Text.EndsWith("in'", StringComparison.OrdinalIgnoreCase) && DoSpell(currentWordText.TrimEnd('\'') + "g"))
                {
                    continue;
                }

                _wrongWords.Add(currentWord);
            }
        }

        public bool DoSpell(string word)
        {
            return _hunspell.Spell(word);
        }

        private void UiTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsLiveSpellCheckEnabled && e.KeyCode == Keys.Apps && _wrongWords?.Count > 0)
            {
                var cursorPos = SelectionStart;
                var wrongWord = _wrongWords.Find(word => cursorPos > word.Index && cursorPos < word.Index + word.Length);
                if (wrongWord != null)
                {
                    IsWrongWord = true;
                    _currentWord = wrongWord;
                }
                else
                {
                    IsWrongWord = false;
                }
            }
        }

        private void UiTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (IsLiveSpellCheckEnabled)
            {
                if (e.KeyChar == '\b' && SelectionStart != Text.Length || e.KeyChar == '\r' || e.KeyChar == '\n')
                {
                    IsSpellCheckRequested = true;
                    TextChangedHighlight(this, EventArgs.Empty);
                }
                else if (SplitChars.Contains(e.KeyChar) && SelectionStart == Text.Length || SelectionStart != Text.Length)
                {
                    IsSpellCheckRequested = true;
                }
            }
        }

        private void UiTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (IsLiveSpellCheckEnabled && _wrongWords?.Count > 0 && e.Clicks == 1 && e.Button == MouseButtons.Right)
            {
                int positionToSearch = GetCharIndexFromPosition(new Point(e.X, e.Y));
                var wrongWord = _wrongWords.Find(word => positionToSearch > GetIndexWithLineBreak(word.Index) && positionToSearch < GetIndexWithLineBreak(word.Index) + word.Length);
                if (wrongWord != null)
                {
                    IsWrongWord = true;
                    _currentWord = wrongWord;
                }
                else
                {
                    IsWrongWord = false;
                }
            }
        }

        private List<string> DoSuggest(string word)
        {
            var parameter = new SuggestionParameter(word, _hunspell);
            var suggestThread = new System.Threading.Thread(DoWork);
            suggestThread.Start(parameter);
            suggestThread.Join(3000); // wait max 3 seconds
            suggestThread.Abort();
            if (!parameter.Success)
            {
                LoadHunspell(_currentDictionary);
            }

            return parameter.Suggestions;
        }

        public void AddSuggestionsToMenu()
        {
            if (_currentWord != null)
            {
                var suggestions = DoSuggest(_currentWord.Text);
                if (suggestions?.Count > 0)
                {
                    foreach (var suggestion in suggestions)
                    {
                        ContextMenuStrip.Items.Add(suggestion, null, SuggestionSelected);
                    }
                }
            }
        }

        private static void DoWork(object data)
        {
            var parameter = (SuggestionParameter)data;
            parameter.Suggestions = parameter.Hunspell.Suggest(parameter.InputWord);
            parameter.Success = true;
        }

        private void SuggestionSelected(object sender, EventArgs e)
        {
            IsWrongWord = false;
            _wrongWords.Remove(_currentWord);
            var item = (ToolStripItem)sender;
            var correctWord = item.Text;
            var text = Text;
            var cursorPos = SelectionStart;
            var wordIndex = _currentWord.Index;
            text = text.Remove(wordIndex, _currentWord.Length);
            text = text.Insert(wordIndex, correctWord);
            Text = text;
            SelectionStart = cursorPos;
            IsSpellCheckRequested = true;
            TextChangedHighlight(this, EventArgs.Empty);
        }

        public void DoAction(SpellCheckAction action)
        {
            if (_currentWord != null)
            {
                switch (action)
                {
                    case SpellCheckAction.Skip:
                        string key = CurrentLineIndex + "-" + _currentWord.Text + "-" + _currentWord.Index;
                        _skipOnceList.Add(key);
                        break;
                    case SpellCheckAction.SkipAll:
                        _skipAllList.Add(_currentWord.Text);
                        break;
                    case SpellCheckAction.AddToDictionary:
                        _spellCheckWordLists.AddUserWord(_currentWord.Text);
                        break;
                    case SpellCheckAction.AddToNames:
                        _spellCheckWordLists.AddName(_currentWord.Text);
                        break;
                }

                if (_wrongWords.Contains(_currentWord))
                {
                    IsWrongWord = false;
                    _wrongWords.Remove(_currentWord);
                    IsSpellCheckRequested = true;
                    TextChangedHighlight(this, EventArgs.Empty);
                }
            }
        }

        private void HighlightSpellCheckWords()
        {
            DoLiveSpellCheck();
            if (_wrongWords?.Count > 0)
            {
                foreach (var wrongWord in _wrongWords)
                {
                    Select(GetIndexWithLineBreak(wrongWord.Index), wrongWord.Length);
                    SelectionColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                }
            }

            IsSpellCheckRequested = false;
        }

        #endregion
    }
}

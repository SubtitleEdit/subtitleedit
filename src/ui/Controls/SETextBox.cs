using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SpellCheck;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic.SpellCheck;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Controls
{
    /// <summary>
    /// TextBox that can be either a normal text box or a rich text box.
    /// </summary>
    public sealed class SETextBox : Panel, IDoSpell
    {
        public new event EventHandler TextChanged;
        public new event KeyEventHandler KeyDown;
        public new event MouseEventHandler MouseClick;
        public new event EventHandler Enter;
        public new event KeyEventHandler KeyUp;
        public new event EventHandler Leave;
        public new event MouseEventHandler MouseMove;

        private bool _checkRtfChange = true;
        private RichTextBox _richTextBoxTemp;
        private RichTextBox _uiTextBox;
        private SimpleTextBox _simpleTextBox;
        private int _mouseMoveSelectionLength;

        private Hunspell _hunspell;
        private SpellCheckWordLists _spellCheckWordLists;
        private List<SpellCheckWord> _words;
        private List<SpellCheckWord> _wrongWords;
        private List<string> _skipAllList;
        private HashSet<string> _skipOnceList;
        private SpellCheckWord _currentWord;
        private string _currentDictionary;
        private string _currentLanguage;
        private string _uiTextBoxOldText;

        private static readonly char[] SplitChars = { ' ', '.', ',', '?', '!', ':', ';', '"', '“', '”', '(', ')', '[', ']', '{', '}', '|', '<', '>', '/', '+', '\r', '\n', '\b', '¿', '¡', '…', '—', '–', '♪', '♫', '„', '«', '»', '‹', '›', '؛', '،', '؟' };
        
        public int CurrentLineIndex { get; set; }
        public bool IsWrongWord { get; set; }
        public bool IsSpellCheckerInitialized { get; set; }
        public bool IsDictionaryDownloaded { get; set; } = true;
        public bool IsSpellCheckRequested { get; set; } = false;

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

        public SETextBox()
        {
            Initialize(Configuration.Settings.General.SubtitleTextBoxSyntaxColor);
        }

        public void Initialize(bool useSyntaxColoring)
        {
            ContextMenuStrip oldContextMenuStrip = null;
            var oldEnabled = true;
            var oldText = string.Empty;
            if (_simpleTextBox != null)
            {
                oldContextMenuStrip = _simpleTextBox.ContextMenuStrip;
                oldEnabled = _simpleTextBox.Enabled;
                oldText = _simpleTextBox.Text;
            }
            else if (_uiTextBox != null)
            {
                oldContextMenuStrip = _uiTextBox.ContextMenuStrip;
                oldEnabled = _uiTextBox.Enabled;
                oldText = _uiTextBox.Text;
            }

            BorderStyle = BorderStyle.None;
            Padding = new Padding(1);
            BackColor = SystemColors.WindowFrame;
            Controls.Clear();
            _simpleTextBox?.Dispose();
            _richTextBoxTemp?.Dispose();
            _uiTextBox?.Dispose();
            _simpleTextBox = null;
            _uiTextBox = null;
            _richTextBoxTemp = null;
            if (useSyntaxColoring)
            {

                _richTextBoxTemp = new RichTextBox();
                _uiTextBox = new RichTextBox { BorderStyle = BorderStyle.None, Multiline = true };
                InitializeBackingControl(_uiTextBox);

                _uiTextBox.TextChanged += TextChangedHighlight;
                _uiTextBox.KeyPress += UiTextBox_KeyPress;
                _uiTextBox.KeyDown += UiTextBox_KeyDown;
                _uiTextBox.MouseDown += UiTextBox_MouseDown;
                // avoid selection when centered and clicking to the left
                _uiTextBox.MouseDown += (sender, args) =>
                {
                    var charIndex = _uiTextBox.GetCharIndexFromPosition(args.Location);
                    if (Configuration.Settings.General.CenterSubtitleInTextBox &&
                        _mouseMoveSelectionLength == 0 &&
                        (charIndex == 0 || charIndex >= 0 && _uiTextBox.Text[charIndex - 1] == '\n'))
                    {
                        _uiTextBox.SelectionLength = 0;
                    }
                };
                _uiTextBox.MouseMove += (sender, args) =>
                {
                    _mouseMoveSelectionLength = _uiTextBox.SelectionLength;
                };
                _uiTextBox.KeyDown += (sender, args) =>
                {
                    // fix annoying "beeps" when moving cursor position
                    if ((args.KeyData == Keys.Left || args.KeyData == Keys.PageUp) && _uiTextBox.SelectionStart == 0)
                    {
                        args.SuppressKeyPress = true;
                    }
                    else if (args.KeyData == Keys.Up && _uiTextBox.SelectionStart <= _uiTextBox.Text.IndexOf('\n'))
                    {
                        args.SuppressKeyPress = true;
                    }
                    else if (args.KeyData == Keys.Home && (_uiTextBox.SelectionStart == 0 || _uiTextBox.SelectionStart > 0 && _uiTextBox.Text[_uiTextBox.SelectionStart - 1] == '\n'))
                    {
                        args.SuppressKeyPress = true;
                    }
                    else if (args.KeyData == (Keys.Home | Keys.Control) && _uiTextBox.SelectionStart == 0)
                    {
                        args.SuppressKeyPress = true;
                    }
                    else if (args.KeyData == Keys.End && (_uiTextBox.SelectionStart >= _uiTextBox.Text.Length || _uiTextBox.SelectionStart + 1 < _uiTextBox.Text.Length && _uiTextBox.Text[_uiTextBox.SelectionStart] == '\n'))
                    {
                        args.SuppressKeyPress = true;
                    }
                    else if (args.KeyData == (Keys.End | Keys.Control) && _uiTextBox.SelectionStart >= _uiTextBox.Text.Length)
                    {
                        args.SuppressKeyPress = true;
                    }
                    else if (args.KeyData == Keys.Right && _uiTextBox.SelectionStart >= _uiTextBox.Text.Length)
                    {
                        if (Configuration.Settings.Tools.LiveSpellCheck)
                        {
                            IsSpellCheckRequested = true;
                            TextChangedHighlight(this, EventArgs.Empty);
                        }

                        args.SuppressKeyPress = true;
                    }
                    else if (args.KeyData == Keys.Down && _uiTextBox.SelectionStart >= _uiTextBox.Text.Length)
                    {
                        args.SuppressKeyPress = true;
                    }
                    else if (args.KeyData == Keys.PageDown && _uiTextBox.SelectionStart >= _uiTextBox.Text.Length)
                    {
                        args.SuppressKeyPress = true;
                    }
                };
                _uiTextBox.Enter += (sender, args) =>
                {
                    var text = _uiTextBox.Text;
                    if (string.IsNullOrWhiteSpace(text) || text.Length > 1000)
                    {
                        if (Configuration.Settings.General.CenterSubtitleInTextBox)
                        {
                            SuspendLayout();
                            _richTextBoxTemp.Text = text;
                            _richTextBoxTemp.SelectAll();
                            _richTextBoxTemp.SelectionAlignment = HorizontalAlignment.Center;
                            ResumeLayout(false);
                            _uiTextBox.Rtf = _richTextBoxTemp.Rtf;
                        } 
                    }
                };
            }
            else
            {
                _simpleTextBox = new SimpleTextBox { BorderStyle = BorderStyle.None, Multiline = true };
                InitializeBackingControl(_simpleTextBox);
            }

            UpdateFontAndColors();
            if (oldContextMenuStrip != null)
            {
                ContextMenuStrip = oldContextMenuStrip;
            }

            Enabled = oldEnabled;
            Text = oldText;
        }

        private void InitializeBackingControl(Control textBox)
        {
            Controls.Add(textBox);
            textBox.Dock = DockStyle.Fill;
            textBox.Enter += (sender, args) => { BackColor = SystemColors.Highlight; };
            textBox.Leave += (sender, args) => { BackColor = SystemColors.WindowFrame; };
            textBox.TextChanged += (sender, args) => { TextChanged?.Invoke(sender, args); };
            textBox.KeyDown += (sender, args) => { KeyDown?.Invoke(sender, args); };
            textBox.MouseClick += (sender, args) => { MouseClick?.Invoke(sender, args); };
            textBox.Enter += (sender, args) => { Enter?.Invoke(sender, args); };
            textBox.KeyUp += (sender, args) => { KeyUp?.Invoke(sender, args); };
            textBox.Leave += (sender, args) => { Leave?.Invoke(sender, args); };
            textBox.MouseMove += (sender, args) => { MouseMove?.Invoke(sender, args); };
        }

        private void UpdateFontAndColors()
        {
            UpdateFontAndColors(_uiTextBox);
            UpdateFontAndColors(_richTextBoxTemp);
            UpdateFontAndColors(_simpleTextBox);
        }

        public void UpdateFontAndColors(Control textBox)
        {
            if (textBox == null)
            {
                return;
            }

            var gs = Configuration.Settings.General;
            if (string.IsNullOrEmpty(gs.SubtitleFontName))
            {
                gs.SubtitleFontName = Font.Name;
            }

            try
            {
                textBox.Font = gs.SubtitleTextBoxFontBold ? new Font(gs.SubtitleFontName, gs.SubtitleTextBoxFontSize, FontStyle.Bold) : new Font(gs.SubtitleFontName, gs.SubtitleTextBoxFontSize);
                textBox.ForeColor = gs.SubtitleFontColor;
                textBox.BackColor = gs.SubtitleBackgroundColor;
            }
            catch
            {
                // ignore
            }
        }

        private bool _fixedArabicComma;
        public override string Text
        {
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.Text;
                }

                if (_uiTextBox != null)
                {

                    var s = _uiTextBox.Text;
                    if (_fixedArabicComma)
                    {
                        s = s.Replace("\u202A", string.Empty);
                    }

                    return string.Join(Environment.NewLine, s.SplitToLines());
                }

                return string.Empty;
            }
            set
            {
                if (_simpleTextBox != null)
                {
                    _simpleTextBox.Text = value;
                    return;
                }

                if (_uiTextBox != null)
                {
                    _fixedArabicComma = false;
                    var s = value;
                    if (!Configuration.Settings.General.RightToLeftMode && !s.ContainsUnicodeControlChars())
                    {
                        string textNoTags = HtmlUtil.RemoveHtmlTags(s, true);
                        if (textNoTags.EndsWith('،'))
                        {
                            s = s.Replace("،", "\u202A،");
                        }
                        else if (textNoTags.StartsWith('،'))
                        {
                            s = s.Replace("،", "،\u202A");
                        }

                        _fixedArabicComma = true;
                    }

                    _uiTextBox.Text = string.Join("\n", s.SplitToLines());
                }
            }
        }

        public int SelectionStart
        {
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.SelectionStart;
                }

                if (_uiTextBox != null)
                {
                    var text = _uiTextBox.Text;
                    var extra = 0;
                    var target = _uiTextBox.SelectionStart;
                    for (int i = 0; i < target && i < text.Length; i++)
                    {
                        if (text[i] == '\n')
                        {
                            extra++;
                        }
                    }

                    return target + extra;
                }


                return 0;
            }
            set
            {
                if (_simpleTextBox != null)
                {
                    _simpleTextBox.SelectionStart = value;
                }
                else if (_uiTextBox != null)
                {
                    var text = _uiTextBox.Text;
                    var extra = 0;
                    for (int i = 0; i < value && i < text.Length; i++)
                    {
                        if (text[i] == '\n')
                        {
                            extra++;
                        }
                    }

                    _uiTextBox.SelectionStart = value - extra;
                }
            }
        }

        public int SelectionLength
        {
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.SelectionLength;
                }

                if (_uiTextBox != null)
                {
                    var target = _uiTextBox.SelectionLength;
                    if (target == 0)
                    {
                        return 0;
                    }

                    var text = _uiTextBox.Text;
                    var extra = 0;
                    var start = _uiTextBox.SelectionStart;
                    for (int i = start; i < target + start && i < text.Length; i++)
                    {
                        if (text[i] == '\n')
                        {
                            extra++;
                        }
                    }

                    return target + extra;
                }

                return 0;
            }
            set
            {
                if (_simpleTextBox != null)
                {
                    _simpleTextBox.SelectionLength = value;
                }
                else if (_uiTextBox != null)
                {
                    var target = value;
                    if (target == 0)
                    {
                        _uiTextBox.SelectionLength = 0;
                        return;
                    }

                    _uiTextBox.SelectionLength = GetIndexWithLineBreak(value);
                }
            }
        }

        public bool HideSelection
        {
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.HideSelection;
                }

                return _uiTextBox != null && _uiTextBox.HideSelection;
            }
            set
            {
                if (_simpleTextBox != null)
                {
                    _simpleTextBox.HideSelection = value;
                }
                else if (_uiTextBox != null)
                {
                    _uiTextBox.HideSelection = value;
                }
            }
        }

        public string SelectedText
        {
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.SelectedText;
                }

                return _uiTextBox != null ? string.Join(Environment.NewLine, _uiTextBox.SelectedText.SplitToLines()) : string.Empty;
            }
            set
            {
                if (_simpleTextBox != null)
                {
                    _simpleTextBox.SelectedText = value;
                }
                else if (_uiTextBox != null)
                {
                    _uiTextBox.SelectedText = value;
                }
            }
        }

        public bool Multiline
        {
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.Multiline;
                }

                return _uiTextBox != null && _uiTextBox.Multiline;
            }
            set
            {
                if (_simpleTextBox != null)
                {
                    _simpleTextBox.Multiline = value;
                }
                else if (_uiTextBox != null)
                {
                    _uiTextBox.Multiline = value;
                }
            }
        }

        public new bool Enabled
        {
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.Enabled;
                }

                return _uiTextBox == null || _uiTextBox.Enabled;
            }
            set
            {
                if (_simpleTextBox != null)
                {
                    _simpleTextBox.Enabled = value;
                }
                else if (_uiTextBox != null)
                {
                    _uiTextBox.Enabled = value;
                }
            }
        }

        public RichTextBoxScrollBars ScrollBars
        {
            get
            {
                if (_simpleTextBox == null && _uiTextBox == null)
                {
                    return RichTextBoxScrollBars.None;
                }

                if (_simpleTextBox != null)
                {
                    if (_simpleTextBox.ScrollBars == System.Windows.Forms.ScrollBars.Both)
                    {
                        return RichTextBoxScrollBars.Both;
                    }

                    if (_simpleTextBox.ScrollBars == System.Windows.Forms.ScrollBars.Horizontal)
                    {
                        return RichTextBoxScrollBars.Horizontal;
                    }

                    return _simpleTextBox.ScrollBars == System.Windows.Forms.ScrollBars.Vertical ? RichTextBoxScrollBars.Vertical : RichTextBoxScrollBars.None;
                }

                return _uiTextBox.ScrollBars;
            }
            set
            {
                if (_simpleTextBox != null)
                {
                    if (value == RichTextBoxScrollBars.Both || value == RichTextBoxScrollBars.ForcedBoth)
                    {
                        _simpleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
                    }
                    else if (value == RichTextBoxScrollBars.Horizontal || value == RichTextBoxScrollBars.ForcedHorizontal)
                    {
                        _simpleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
                    }
                    else if (value == RichTextBoxScrollBars.Vertical || value == RichTextBoxScrollBars.ForcedVertical)
                    {
                        _simpleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
                    }

                    _simpleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
                }
                else if (_uiTextBox != null)
                {
                    _uiTextBox.ScrollBars = value;
                }
            }
        }

        public override ContextMenuStrip ContextMenuStrip
        {
            get => _simpleTextBox != null ? _simpleTextBox.ContextMenuStrip : _uiTextBox?.ContextMenuStrip;
            set
            {
                if (_simpleTextBox != null)
                {
                    _simpleTextBox.ContextMenuStrip = value;
                }
                else if (_uiTextBox != null)
                {
                    _uiTextBox.ContextMenuStrip = value;
                }
            }
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);

            if (_simpleTextBox != null)
            {
                return;
            }

            var text = _uiTextBox.Text;
            if (Configuration.Settings.General.CenterSubtitleInTextBox)
            {
                _richTextBoxTemp.RightToLeft = RightToLeft;
                SuspendLayout();
                _richTextBoxTemp.Text = text;
                _richTextBoxTemp.SelectAll();
                _richTextBoxTemp.SelectionAlignment = HorizontalAlignment.Center;
                ResumeLayout(false);
                _uiTextBox.Rtf = _richTextBoxTemp.Rtf;
            }
        }

        private int GetIndexWithLineBreak(int index)
        {
            var text = _uiTextBox.Text;
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

        public int GetCharIndexFromPosition(Point pt)
        {
            if (_simpleTextBox != null)
            {
                return _simpleTextBox.GetCharIndexFromPosition(pt);
            }

            return _uiTextBox?.GetCharIndexFromPosition(pt) ?? 0;
        }

        public void SelectAll()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.SelectAll();
            }
            else
            {
                _uiTextBox?.SelectAll();
            }
        }

        public void Clear()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Clear();
            }
            else
            {
                _uiTextBox?.Clear();
            }
        }

        public void Undo()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Undo();
            }
            else
            {
                _uiTextBox?.Undo();
            }
        }

        public void ClearUndo()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.ClearUndo();
            }
            else
            {
                _uiTextBox?.ClearUndo();
            }
        }

        public void Copy()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Copy();
            }
            else
            {
                _uiTextBox?.Copy();
            }
        }

        public void Cut()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Cut();
            }
            else
            {
                _uiTextBox?.Cut();
            }
        }

        public void Paste()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Paste();
            }
            else
            {
                _uiTextBox?.Paste();
            }
        }

        public override bool Focused
        {
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.Focused;
                }

                return _uiTextBox != null && _uiTextBox.Focused;
            }
        }

        public new void Focus()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Focus();
            }
            else
            {
                _uiTextBox?.Focus();
            }
        }

        private void TextChangedHighlight(object sender, EventArgs e)
        {
            if (_checkRtfChange)
            {
                _checkRtfChange = false;

                if (Configuration.Settings.Tools.LiveSpellCheck)
                { 
                    if (!string.IsNullOrEmpty(_uiTextBoxOldText)
                        && HtmlUtil.RemoveHtmlTags(_uiTextBoxOldText, true) == HtmlUtil.RemoveHtmlTags(_uiTextBox.Text, true))
                    {
                        IsSpellCheckRequested = true;
                    }

                    _uiTextBoxOldText = _uiTextBox.Text;
                }

                HighlightHtmlText();
                _checkRtfChange = true;
            }
        }

        private void HighlightHtmlText()
        {
            var text = _uiTextBox.Text;
            _richTextBoxTemp.SuspendLayout();
            _richTextBoxTemp.Clear();
            _richTextBoxTemp.Text = text;
            _richTextBoxTemp.SelectAll();
            _richTextBoxTemp.SelectionFont = _richTextBoxTemp.Font;
            _richTextBoxTemp.SelectionColor = _richTextBoxTemp.ForeColor;

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
                        _richTextBoxTemp.SelectionStart = assaTagStart;
                        _richTextBoxTemp.SelectionLength = i - assaTagStart + 1;
                        _richTextBoxTemp.SelectionColor = Configuration.Settings.General.SubtitleTextBoxAssColor;
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
                        _richTextBoxTemp.SelectionStart = htmlTagStart;
                        _richTextBoxTemp.SelectionLength = i - htmlTagStart + 1;
                        _richTextBoxTemp.SelectionColor = Configuration.Settings.General.SubtitleTextBoxHtmlColor;
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

            if (Configuration.Settings.General.CenterSubtitleInTextBox)
            {
                _richTextBoxTemp.SelectAll();
                _richTextBoxTemp.SelectionAlignment = HorizontalAlignment.Center;
            }

            if (Configuration.Settings.Tools.LiveSpellCheck)
            {
                DoLiveSpellCheck();
                if (_wrongWords?.Count > 0)
                {
                    foreach (var wrongWord in _wrongWords)
                    {
                        _richTextBoxTemp.Select(GetIndexWithLineBreak(wrongWord.Index), wrongWord.Length);
                        _richTextBoxTemp.SelectionColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                    }
                }

                IsSpellCheckRequested = false;
            }

            _richTextBoxTemp.ResumeLayout(false);

            if (_uiTextBox.Text.Length != text.Length)
            {
                return;
            }

            var start = SelectionStart;
            var length = SelectionLength;
            SuspendLayout();
            _uiTextBox.Rtf = _richTextBoxTemp.Rtf;
            SelectionStart = start;
            SelectionLength = length;
            ResumeLayout(true);
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

                int colorEnd = text.IndexOf('&', colorStart + 1);
                if (colorEnd > 0)
                {
                    var color = text.Substring(colorStart, colorEnd - colorStart);
                    try
                    {
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
            var backColor = _richTextBoxTemp.BackColor;
            _richTextBoxTemp.SelectionStart = colorStart;
            _richTextBoxTemp.SelectionLength = colorEnd - colorStart;
            _richTextBoxTemp.SelectionColor = c;

            var diff = Math.Abs(c.R - backColor.R) + Math.Abs(c.G - backColor.G) + Math.Abs(c.B - backColor.B);
            if (diff < 60)
            {
                _richTextBoxTemp.SelectionBackColor = Color.FromArgb(byte.MaxValue - c.R, byte.MaxValue - c.G, byte.MaxValue - c.B, byte.MaxValue - c.R);
            }
        }

        #region LiveSpellCheck

        public async Task CheckForLanguageChange(Subtitle subtitle)
        {
            var detectedLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            if (_currentLanguage != detectedLanguage)
            {
                DisposeHunspellAndDictionaries();
                await InitializeLiveSpellCheck(subtitle, CurrentLineIndex);
            }
        }

        public async Task InitializeLiveSpellCheck(Subtitle subtitle, int lineNumber)
        {
            if (_uiTextBox is null || lineNumber < 0)
            {
                return;
            }

            if (_spellCheckWordLists is null && _hunspell is null)
            {
                var detectedLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
                var availableDictionaries = Utilities.GetDictionaryLanguagesCultureNeutral();
                var isDictionaryAvailable = false;
                foreach (var availableDictionary in availableDictionaries)
                {
                    if (availableDictionary.Contains($"[{detectedLanguage}]"))
                    {
                        isDictionaryAvailable = true;
                        break;
                    }
                }

                if (isDictionaryAvailable)
                {
                    var languageName = LanguageAutoDetect.AutoDetectLanguageName(string.Empty, subtitle);
                    if (languageName.Split(new char[] { '_', '-' })[0] != detectedLanguage)
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
                    using (var gd = new GetDictionaries())
                    {
                        if (gd.ShowDialog(this) == DialogResult.OK)
                        {
                            await InitializeLiveSpellCheck(subtitle, lineNumber);
                        }
                    }
                }

                _currentLanguage = detectedLanguage;
            }
        }

        private async Task LoadDictionariesAsync(string languageName)
        {
            await Task.Run(() => LoadDictionaries(languageName));
        }

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
                _currentLanguage = null;
                _hunspell?.Dispose();
                _hunspell = null;
                IsWrongWord = false;
                IsSpellCheckerInitialized = false;

                if (Configuration.Settings.General.SubtitleTextBoxSyntaxColor)
                {
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
                        _words[i - 1].Text.ToLowerInvariant() == "www" &&
                        (_words[i + 1].Text.ToLowerInvariant() == "com" ||
                         _words[i + 1].Text.ToLowerInvariant() == "org" ||
                         _words[i + 1].Text.ToLowerInvariant() == "net") &&
                        Text.IndexOf(_words[i - 1].Text + "." + currentWordText + "." + 
                                     _words[i + 1].Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue; // do not spell check urls
                    }

                    if (_currentLanguage == "ar")
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
                    else if (_currentLanguage == "en " && (currentWordText.Equals("a", StringComparison.OrdinalIgnoreCase) || currentWordText == "I"))
                    {
                        continue;
                    }
                    else if (_currentLanguage == "da" && currentWordText.Equals("i", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                if (Configuration.Settings.Tools.SpellCheckEnglishAllowInQuoteAsIng && _currentLanguage == "en"
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
            if (Configuration.Settings.Tools.LiveSpellCheck && e.KeyCode == Keys.Apps && _wrongWords?.Count > 0)
            {
                var cursorPos = _uiTextBox.SelectionStart;
                var wrongWord = _wrongWords.Where(word => cursorPos > GetIndexWithLineBreak(word.Index) && cursorPos < GetIndexWithLineBreak(word.Index) + word.Length).FirstOrDefault();
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
            if (Configuration.Settings.Tools.LiveSpellCheck && SplitChars.Contains(e.KeyChar)
                && _uiTextBox.SelectionStart == _uiTextBox.Text.Length || _uiTextBox.SelectionStart != _uiTextBox.Text.Length)
            {
                IsSpellCheckRequested = true;
                if (e.KeyChar == '\b' || e.KeyChar == '\r' || e.KeyChar == '\n')
                {
                    TextChangedHighlight(this, EventArgs.Empty);
                }
            }
        }

        private void UiTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (_wrongWords?.Count > 0 && e.Clicks == 1 && e.Button == MouseButtons.Right)
            {
                int positionToSearch = _uiTextBox.GetCharIndexFromPosition(new Point(e.X, e.Y));
                var wrongWord = _wrongWords.Where(word => positionToSearch > GetIndexWithLineBreak(word.Index) && positionToSearch < GetIndexWithLineBreak(word.Index) + word.Length).FirstOrDefault();
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
                        _uiTextBox.ContextMenuStrip.Items.Add(suggestion, null, SuggestionSelected);
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
            var text = _uiTextBox.Text;
            var cursorPos = _uiTextBox.SelectionStart;
            var wordIndex = GetIndexWithLineBreak(_currentWord.Index);
            text = text.Remove(wordIndex, _currentWord.Length);
            text = text.Insert(wordIndex, correctWord);
            _uiTextBox.Text = text;
            _uiTextBox.SelectionStart = cursorPos;
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

        #endregion
    }
}

using Nikse.SubtitleEdit.Controls.WebBrowser;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    /// <summary>
    /// TextBox that can be either a normal text box or a rich text box (or a html text box).
    /// </summary>
    public sealed class SETextBox : Panel
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
        private WebBrowserEditBox _htmlBox;
        private const bool UseWebBrowser = false;

        public SETextBox()
        {
            Initialize(Configuration.Settings.General.SubtitleTextBoxSyntaxColor);
        }

        public void Initialize(bool useSyntaxColoring)
        {
            ContextMenuStrip oldContextMenuStrip = null;
            var oldEnabled = true;
            if (_simpleTextBox != null)
            {
                oldContextMenuStrip = _simpleTextBox.ContextMenuStrip;
                oldEnabled = _simpleTextBox.Enabled;
            }
            else if (_uiTextBox != null)
            {
                oldContextMenuStrip = _uiTextBox.ContextMenuStrip;
                oldEnabled = _uiTextBox.Enabled;
            }
            else if (_htmlBox != null)
            {
                oldContextMenuStrip = _htmlBox.ContextMenuStrip;
                oldEnabled = _htmlBox.Enabled;
            }

            BorderStyle = BorderStyle.None;
            Padding = new Padding(1);
            BackColor = SystemColors.WindowFrame;

            Controls.Clear();
            _simpleTextBox?.Dispose();
            _richTextBoxTemp?.Dispose();
            _uiTextBox?.Dispose();
            _htmlBox?.Dispose();
            _simpleTextBox = null;
            _htmlBox = null;
            _uiTextBox = null;
            _richTextBoxTemp = null;
            if (useSyntaxColoring)
            {
                if (UseWebBrowser)
                {
                    _uiTextBox = null;
                    _htmlBox = new WebBrowserEditBox();
                    Controls.Add(_htmlBox);
                    _htmlBox.Dock = DockStyle.Fill;
                    _htmlBox.Initialize();
                    _htmlBox.Enter += (sender, args) => { BackColor = SystemColors.Highlight; };
                    _htmlBox.Leave += (sender, args) => { BackColor = SystemColors.WindowFrame; };
                    _htmlBox.TextChanged += (sender, args) => { TextChanged?.Invoke(sender, args); };
                    _htmlBox.KeyDown += (sender, args) => { KeyDown?.Invoke(sender, args); };
                    _htmlBox.KeyUp += (sender, args) => { KeyUp?.Invoke(sender, args); };
                    _htmlBox.MouseClick += (sender, args) => { MouseClick?.Invoke(sender, args); };
                    _htmlBox.MouseMove += (sender, args) => { MouseMove?.Invoke(sender, args); };
                }
                else
                {
                    _htmlBox = null;
                    _richTextBoxTemp = new RichTextBox();
                    _uiTextBox = new RichTextBox { BorderStyle = BorderStyle.None, Multiline = true };
                    InitializeBackingControl(_uiTextBox);

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
                }
            }
            else
            {
                _simpleTextBox = new SimpleTextBox { BorderStyle = BorderStyle.None, Multiline = true };
                _simpleTextBox.KeyDown += (sender, args) =>
                {
                    if (args.KeyData == (Keys.Control | Keys.Back))
                    {
                        UiUtil.ApplyControlBackspace(_simpleTextBox);
                        args.SuppressKeyPress = true;
                    }
                };
                InitializeBackingControl(_simpleTextBox);
            }

            UpdateFontAndColors();
            if (oldContextMenuStrip != null)
            {
                ContextMenuStrip = oldContextMenuStrip;
            }

            Enabled = oldEnabled;
        }

        private void InitializeBackingControl(Control textBox)
        {
            textBox.TextChanged += TextChangedHighlight;
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

                if (_htmlBox != null)
                {
                    return _htmlBox.Text;
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
                    if (!Configuration.Settings.General.RightToLeftMode && !s.Contains('\u202A'))
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

                if (_htmlBox != null)
                {
                    _htmlBox.Text = value;
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
                    return _uiTextBox.SelectionStart;
                }

                if (_htmlBox != null)
                {
                    return _htmlBox.SelectionStart;
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
                    _uiTextBox.SelectionStart = value;
                }
                else if (_htmlBox != null)
                {
                    _htmlBox.SelectionStart = value;
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
                    return _uiTextBox.SelectionLength;
                }

                if (_htmlBox != null)
                {
                    return _htmlBox.SelectionLength;
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
                    _uiTextBox.SelectionLength = value;
                }
                else if (_htmlBox != null)
                {
                    _htmlBox.SelectionLength = value;
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

                if (_uiTextBox != null)
                {
                    return _uiTextBox.HideSelection;
                }

                if (_htmlBox != null)
                {
                    return _htmlBox.HideSelection;
                }

                return false;
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
                else if (_htmlBox != null)
                {
                    _htmlBox.HideSelection = value;
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

                if (_uiTextBox != null)
                {
                    return _uiTextBox.SelectedText;
                }

                if (_htmlBox != null)
                {
                    return _htmlBox.SelectedText;
                }

                return string.Empty;

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
                else if (_htmlBox != null)
                {
                    _htmlBox.SelectedText = value;
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

                if (_uiTextBox != null)
                {
                    return _uiTextBox.Multiline;
                }

                if (_htmlBox != null)
                {
                    return true;
                }

                return false;
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

                if (_uiTextBox != null)
                {
                    return _uiTextBox.Enabled;
                }

                if (_htmlBox != null)
                {
                    return _htmlBox.Enabled;
                }

                return true;
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
                else if (_htmlBox != null)
                {
                    _htmlBox.Enabled = value;
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

                    if (_simpleTextBox.ScrollBars == System.Windows.Forms.ScrollBars.Vertical)
                    {
                        return RichTextBoxScrollBars.Vertical;
                    }

                    return RichTextBoxScrollBars.None;
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
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.ContextMenuStrip;
                }

                if (_uiTextBox != null)
                {
                    return _uiTextBox.ContextMenuStrip;
                }

                if (_htmlBox != null)
                {
                    return _htmlBox.ContextMenuStrip;
                }

                return null;
            }
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
                else if (_htmlBox != null)
                {
                    _htmlBox.ContextMenuStrip = value;
                }
            }
        }

        public int GetCharIndexFromPosition(Point pt)
        {
            if (_simpleTextBox != null)
            {
                return _simpleTextBox.GetCharIndexFromPosition(pt);
            }

            if (_uiTextBox != null)
            {
                return _uiTextBox.GetCharIndexFromPosition(pt);
            }

            if (_htmlBox != null)
            {
                return _htmlBox.GetCharIndexFromPosition(pt);
            }

            return 0;
        }

        public void SelectAll()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.SelectAll();
            }
            else if (_uiTextBox != null)
            {
                _uiTextBox.SelectAll();
            }
            else if (_htmlBox != null)
            {
                _htmlBox.SelectAll();
            }
        }

        public void Clear()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Clear();
            }
            else if (_uiTextBox != null)
            {
                _uiTextBox.Clear();
            }
            else if (_htmlBox != null)
            {
                _htmlBox.Clear();
            }
        }

        public void Undo()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Undo();
            }
            else if (_uiTextBox != null)
            {
                _uiTextBox.Undo();
            }
            else if (_htmlBox != null)
            {
                _htmlBox.Undo();
            }
        }

        public void ClearUndo()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.ClearUndo();
            }
            else if (_uiTextBox != null)
            {
                _uiTextBox.ClearUndo();
            }
            else if (_htmlBox != null)
            {
                _htmlBox.ClearUndo();
            }
        }

        public void Copy()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Copy();
            }
            else if (_uiTextBox != null)
            {
                _uiTextBox.Copy();
            }
            else if (_htmlBox != null)
            {
                _htmlBox.Copy();
            }
        }

        public void Cut()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Cut();
            }
            else if (_uiTextBox != null)
            {
                _uiTextBox.Cut();
            }
            else if (_htmlBox != null)
            {
                _htmlBox.Cut();
            }
        }

        public void Paste()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Paste();
            }
            else if (_uiTextBox!= null)
            {
                _uiTextBox?.Paste();
            }
            else if (_htmlBox != null)
            {
                _htmlBox?.Paste();
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

                if (_uiTextBox != null)
                {
                    return _uiTextBox.Focused;
                }

                if (_htmlBox != null)
                {
                    return _htmlBox.Focused;
                }

                return false;
            }
        }

        public new void Focus()
        {
            if (_simpleTextBox != null)
            {
                _simpleTextBox.Focus();
            }
            else if (_uiTextBox != null)
            {
                _uiTextBox.Focus();
            }
            else if (_htmlBox != null)
            {
                _htmlBox.Focus();
            }
        }

        public void HighlightHtmlText()
        {
            if (Configuration.Settings.General.RightToLeftMode)
            {
                if (RightToLeft != RightToLeft.Yes)
                {
                    RightToLeft = RightToLeft.Yes;
                }
            }
            else
            {
                if (RightToLeft != RightToLeft.No)
                {
                    RightToLeft = RightToLeft.No;
                }
            }

            if (_simpleTextBox != null)
            {
                if (Configuration.Settings.General.CenterSubtitleInTextBox &&
                    _simpleTextBox.TextAlign != HorizontalAlignment.Center)
                {
                    _simpleTextBox.TextAlign = HorizontalAlignment.Center;
                }

                return;
            }

            _richTextBoxTemp.RightToLeft = RightToLeft;


            var text = _uiTextBox.Text;
            if (string.IsNullOrWhiteSpace(text) || text.Length > 1000)
            {
                if (Configuration.Settings.General.CenterSubtitleInTextBox)
                {
                    SuspendLayout();
                    _richTextBoxTemp.Text = text;
                    _richTextBoxTemp.SelectAll();
                    _richTextBoxTemp.SelectionAlignment = HorizontalAlignment.Center;

                    // fix cursor to start in middle (and not left)
                    _richTextBoxTemp.Rtf = _richTextBoxTemp.Rtf.Replace("\\pard\\par", "\\par");

                    ResumeLayout(false);
                    _uiTextBox.Rtf = _richTextBoxTemp.Rtf;
                }

                return;
            }

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
                                string colorTag = text.IndexOf("\\c", assaTagStart, StringComparison.OrdinalIgnoreCase) != -1 ? "\\c" : "\\1c";

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
                if (text.TrimEnd(' ').EndsWith('\n'))
                {
                    // fix cursor to start in middle (and not left)
                    _richTextBoxTemp.Rtf = _richTextBoxTemp.Rtf.Replace("\\pard\\par", "\\par");
                }
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
            ResumeLayout(false);
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

        private void SetForeColorAndChangeBackColorIfClose(int colorStart, int colorEnd, Color c)
        {
            _richTextBoxTemp.SelectionStart = colorStart;
            _richTextBoxTemp.SelectionLength = colorEnd - colorStart;
            _richTextBoxTemp.SelectionColor = c;

            var diff = Math.Abs(c.R - BackColor.R) + Math.Abs(c.G - BackColor.G) + Math.Abs(c.B - BackColor.B);
            if (diff < 60)
            {
                _richTextBoxTemp.SelectionBackColor = Color.FromArgb(byte.MaxValue - c.R, byte.MaxValue - c.G, byte.MaxValue - c.B, byte.MaxValue - c.R);
            }
        }

        private void TextChangedHighlight(object sender, EventArgs e)
        {
            if (_checkRtfChange)
            {
                _checkRtfChange = false;
                HighlightHtmlText();
                _checkRtfChange = true;
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

                int colorEnd = text.IndexOf('&', colorStart + 1);
                if (colorEnd > 0)
                {
                    var color = text.Substring(colorStart, colorEnd - colorStart);
                    try
                    {
                        if (color.Length == 7)
                        {
                            var rgbColor = string.Concat("#", color[5], color[6], color[3], color[4], color[1], color[2]);
                            var c = ColorTranslator.FromHtml(rgbColor);
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
    }
}

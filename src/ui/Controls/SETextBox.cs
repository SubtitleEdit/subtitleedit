using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Controls.Interfaces;

namespace Nikse.SubtitleEdit.Controls
{
    /// <summary>
    /// TextBox that can be either a normal text box or a rich text box.
    /// </summary>
    public sealed class SETextBox : Panel, ISelectedText
    {
        // ReSharper disable once InconsistentNaming
        public new event EventHandler TextChanged;
        // ReSharper disable once InconsistentNaming
        public new event KeyEventHandler KeyDown;
        // ReSharper disable once InconsistentNaming
        public new event MouseEventHandler MouseClick;
        // ReSharper disable once InconsistentNaming
        public new event EventHandler Enter;
        // ReSharper disable once InconsistentNaming
        public new event KeyEventHandler KeyUp;
        // ReSharper disable once InconsistentNaming
        public new event EventHandler Leave;
        // ReSharper disable once InconsistentNaming
        public new event MouseEventHandler MouseMove;

        private AdvancedTextBox _uiTextBox;
        private SimpleTextBox _simpleTextBox;

        public SETextBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable |
                     ControlStyles.AllPaintingInWmPaint, true);

            Initialize(Configuration.Settings.General.SubtitleTextBoxSyntaxColor, false);
        }

        public void Initialize(bool useSyntaxColoring, bool justTextBox)
        {
            var currentActiveTextBox = GetActiveTextBox();
            var oldContextMenuStrip = currentActiveTextBox?.ContextMenuStrip;
            var oldEnabled = currentActiveTextBox?.Enabled ?? true;
            var oldText = currentActiveTextBox?.Text ?? string.Empty;

            BorderStyle = BorderStyle.None;
            Padding = new Padding(1);
            BackColor = SystemColors.WindowFrame;
            Controls.Clear();
            _simpleTextBox?.Dispose();
            _uiTextBox?.Dispose();
            _simpleTextBox = null;
            _uiTextBox = null;
            if (useSyntaxColoring && !justTextBox)
            {
                _uiTextBox = new AdvancedTextBox { BorderStyle = BorderStyle.None, Multiline = true };
                InitializeBackingControl(_uiTextBox);
                UpdateFontAndColors(_uiTextBox);
            }
            else
            {
                _simpleTextBox = new SimpleTextBox { BorderStyle = BorderStyle.None, Multiline = true };
                InitializeBackingControl(_simpleTextBox);
                if (justTextBox)
                {
                    _simpleTextBox.ForeColor = UiUtil.ForeColor;
                    _simpleTextBox.BackColor = UiUtil.BackColor;
                    BackColor = Color.DarkGray;
                    _simpleTextBox.VisibleChanged += SimpleTextBox_VisibleChanged;
                }
                else
                {
                    UpdateFontAndColors(_simpleTextBox);
                }
            }

            if (oldContextMenuStrip != null)
            {
                ContextMenuStrip = oldContextMenuStrip;
            }

            Enabled = oldEnabled;
            Text = oldText;
        }

        private void SimpleTextBox_VisibleChanged(object sender, EventArgs e)
        {
            Padding = new Padding(1);
            BackColor = Color.DarkGray;
            Invalidate();
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
                var f = gs.SubtitleTextBoxFontBold ? new Font(gs.SubtitleFontName, gs.SubtitleTextBoxFontSize, FontStyle.Bold) : new Font(gs.SubtitleFontName, gs.SubtitleTextBoxFontSize);
                TextBoxFont = f;
                textBox.Font = f;
                textBox.ForeColor = gs.SubtitleFontColor;
                textBox.BackColor = gs.SubtitleBackgroundColor;
            }
            catch
            {
                // ignore
            }
        }

        public Font TextBoxFont
        {
            get
            {
                var activeTextBox = GetActiveTextBox();
                return activeTextBox != null ? activeTextBox.Font : Font;
            }
            set
            {
                var tb = GetActiveTextBox();
                if (tb != null)
                {
                    tb.Font = value;
                }
            }
        }

        public override string Text
        {
            get => GetActiveTextBox()?.Text ?? string.Empty;
            set
            {
                var tb = GetActiveTextBox();
                if (tb != null)
                {
                    tb.Text = value;
                }
            }
        }

        public int SelectionStart
        {
            get => GetActiveTextBox()?.SelectionStart ?? 0;
            set
            {
                var tb = GetActiveTextBox();
                if (tb != null)
                {
                    tb.SelectionStart = value;
                }
            }
        }

        public int SelectionLength
        {
            get => GetActiveTextBox()?.SelectionLength ?? 0;
            set
            {
                var tb = GetActiveTextBox();
                if (tb != null)
                {
                    tb.SelectionLength = value;
                }
            }
        }

        public bool HideSelection
        {
            get => GetActiveTextBox()?.HideSelection == true;
            set
            {
                var tb = GetActiveTextBox();
                if (tb != null)
                {
                    tb.HideSelection = value;
                }
            }
        }

        public string SelectedText
        {
            get => GetActiveTextBox()?.SelectedText ?? string.Empty;
            set
            {
                var tb = GetActiveTextBox();
                if (tb != null)
                {
                    tb.SelectedText = value;
                }
            }
        }

        public bool Multiline
        {
            get => GetActiveTextBox()?.Multiline == true;
            set
            {
                var tb = GetActiveTextBox();
                if (tb != null)
                {
                    tb.Multiline = value;
                }
            }
        }

        public new bool Enabled
        {
            get => GetActiveTextBox()?.Enabled == true;
            set
            {
                var tb = GetActiveTextBox();
                if (tb != null)
                {
                    tb.Enabled = value;
                }
            }
        }

        public RichTextBoxScrollBars ScrollBars
        {
            get
            {
                var tb = GetActiveTextBox();
                if (tb is AdvancedTextBox atb)
                {
                    return atb.ScrollBars;
                }

                if (tb is SimpleTextBox stb)
                {
                    switch (stb.ScrollBars)
                    {
                        case System.Windows.Forms.ScrollBars.Both:
                            return RichTextBoxScrollBars.Both;
                        case System.Windows.Forms.ScrollBars.Horizontal:
                            return RichTextBoxScrollBars.Horizontal;
                        case System.Windows.Forms.ScrollBars.Vertical:
                            return RichTextBoxScrollBars.Vertical;
                        default:
                            return RichTextBoxScrollBars.None;
                    }
                }

                return RichTextBoxScrollBars.None;
            }
            set
            {
                var tb = GetActiveTextBox();
                if (tb is SimpleTextBox stb)
                {
                    if (value == RichTextBoxScrollBars.Both || value == RichTextBoxScrollBars.ForcedBoth)
                    {
                        stb.ScrollBars = System.Windows.Forms.ScrollBars.Both;
                    }
                    else if (value == RichTextBoxScrollBars.Horizontal || value == RichTextBoxScrollBars.ForcedHorizontal)
                    {
                        stb.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
                    }
                    else if (value == RichTextBoxScrollBars.Vertical || value == RichTextBoxScrollBars.ForcedVertical)
                    {
                        stb.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
                    }
                    else
                    {
                        stb.ScrollBars = System.Windows.Forms.ScrollBars.None;
                    }
                }
                else if (tb is AdvancedTextBox)
                {
                    // auto show/hide for rich text box just works
                }
            }
        }

        public override ContextMenuStrip ContextMenuStrip
        {
            get => GetActiveTextBox()?.ContextMenuStrip;
            set
            {
                var tb = GetActiveTextBox();
                if (tb != null)
                {
                    tb.ContextMenuStrip = value;
                }
            }
        }

        public void SelectAll() => GetActiveTextBox()?.SelectAll();

        public void Clear() => GetActiveTextBox()?.Clear();

        public void Undo() => GetActiveTextBox()?.Undo();

        public void ClearUndo() => GetActiveTextBox()?.ClearUndo();

        public void Copy() => GetActiveTextBox()?.Copy();

        public void Cut() => GetActiveTextBox()?.Cut();

        public void Paste() => GetActiveTextBox()?.Paste();

        public override bool Focused => GetActiveTextBox()?.Focused == true;

        public new void Focus() => GetActiveTextBox()?.Focus();

        #region LiveSpellCheck

        public int CurrentLineIndex
        {
            get
            {
                if (_uiTextBox != null)
                {
                    return _uiTextBox.CurrentLineIndex;
                }

                return 0;
            }
            set
            {
                if (_uiTextBox != null)
                {
                    _uiTextBox.CurrentLineIndex = value;
                }
            }
        }

        public string CurrentLanguage
        {
            get
            {
                if (_uiTextBox != null)
                {
                    return _uiTextBox.CurrentLanguage;
                }

                return string.Empty;
            }
            set
            {
                if (_uiTextBox != null)
                {
                    _uiTextBox.CurrentLanguage = value;
                }
            }
        }

        public bool LanguageChanged
        {
            get
            {
                if (_uiTextBox != null)
                {
                    return _uiTextBox.LanguageChanged;
                }

                return false;
            }
            set
            {
                if (_uiTextBox != null)
                {
                    _uiTextBox.LanguageChanged = value;
                }
            }
        }

        public bool IsWrongWord
        {
            get
            {
                if (_uiTextBox != null)
                {
                    return _uiTextBox.IsWrongWord;
                }

                return false;
            }
            set
            {
                if (_uiTextBox != null)
                {
                    _uiTextBox.IsWrongWord = value;
                }
            }
        }

        public bool IsSpellCheckerInitialized
        {
            get
            {
                if (_uiTextBox != null)
                {
                    return _uiTextBox.IsSpellCheckerInitialized;
                }

                return false;
            }
            set
            {
                if (_uiTextBox != null)
                {
                    _uiTextBox.IsSpellCheckerInitialized = value;
                }
            }
        }

        public bool IsDictionaryDownloaded
        {
            get
            {
                if (_uiTextBox != null)
                {
                    return _uiTextBox.IsDictionaryDownloaded;
                }

                return true;
            }
            set
            {
                if (_uiTextBox != null)
                {
                    _uiTextBox.IsDictionaryDownloaded = value;
                }
            }
        }

        public bool IsSpellCheckRequested
        {
            get
            {
                if (_uiTextBox != null)
                {
                    return _uiTextBox.IsSpellCheckRequested;
                }

                return false;
            }
            set
            {
                if (_uiTextBox != null)
                {
                    _uiTextBox.IsSpellCheckRequested = value;
                }
            }
        }

        public int MaxLength
        {
            get
            {
                var tb = GetActiveTextBox();
                return tb?.MaxLength ?? 0;
            }
            set
            {
                var tb = GetActiveTextBox();
                if (tb != null)
                {
                    tb.MaxLength = value;
                }
            }
        }

        public bool UseSystemPasswordChar
        {
            get
            {
                if (GetActiveTextBox() is SimpleTextBox advancedTextBox)
                {
                    return advancedTextBox.UseSystemPasswordChar;
                }

                return false;
            }
            set
            {
                if (GetActiveTextBox() is SimpleTextBox stb)
                {
                    stb.UseSystemPasswordChar = value;
                }
            }
        }

        public bool ReadOnly
        {
            get => GetActiveTextBox()?.ReadOnly == true;
            set
            {
                var tb = GetActiveTextBox();
                if (tb != null)
                {
                    tb.ReadOnly = value;
                }
            }
        }

        public string[] Lines => GetActiveTextBox()?.Lines ?? Array.Empty<string>();

        public void CheckForLanguageChange(Subtitle subtitle)
        {
            _uiTextBox?.CheckForLanguageChange(subtitle);
        }

        public void InitializeLiveSpellCheck(Subtitle subtitle, int lineNumber)
        {
            _uiTextBox?.InitializeLiveSpellCheck(subtitle, lineNumber);
        }

        public void DisposeHunspellAndDictionaries() => _uiTextBox?.DisposeHunspellAndDictionaries();

        public void AddSuggestionsToMenu() => _uiTextBox?.AddSuggestionsToMenu();

        public void DoAction(SpellCheckAction action) => _uiTextBox?.DoAction(action);

        #endregion

        public void SetDarkTheme()
        {
            var activeTextBox = GetActiveTextBox();
            if (activeTextBox == null)
            {
                return;
            }

            activeTextBox.BackColor = DarkTheme.BackColor;
            activeTextBox.ForeColor = DarkTheme.ForeColor;
            activeTextBox.HandleCreated += SeTextBoxBoxHandleCreated;
            DarkTheme.SetWindowThemeDark(activeTextBox);
            DarkTheme.SetWindowThemeDark(this);
        }

        public void UndoDarkTheme()
        {
            var textBox = GetActiveTextBox();
            if (textBox == null)
            {
                return;
            }

            textBox.BackColor = SystemColors.Window;
            textBox.ForeColor = DefaultForeColor;
            textBox.HandleCreated -= SeTextBoxBoxHandleCreated;
            DarkTheme.SetWindowThemeNormal(this);
        }

        private static void SeTextBoxBoxHandleCreated(object sender, EventArgs e) => DarkTheme.SetWindowThemeDark((Control)sender);

        public void ScrollToCaret() => GetActiveTextBox()?.ScrollToCaret();

        private TextBoxBase GetActiveTextBox()
        {
            if (_uiTextBox != null)
            {
                return _uiTextBox;
            }

            return _simpleTextBox;
        }
    }
}

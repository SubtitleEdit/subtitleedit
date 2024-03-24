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

        public SETextBox(bool justTextBox)
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);

            Initialize(false, justTextBox);
        }


        public void Initialize(bool useSyntaxColoring, bool justTextBox)
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
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.Font;
                }

                if (_uiTextBox != null)
                {
                    return _uiTextBox.Font;
                }

                return Font;
            }
            set
            {
                if (_simpleTextBox != null)
                {
                    _simpleTextBox.Font = value;
                }

                if (_uiTextBox != null)
                {
                    _uiTextBox.Font = value;
                }
            }
        }

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
                    return _uiTextBox.Text;
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
                    _uiTextBox.Text = value;
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

                if (_uiTextBox != null)
                {
                    return _uiTextBox.SelectedText;
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
                    else
                    {
                        _simpleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
                    }
                }
                else if (_uiTextBox != null)
                {
                    // auto show/hide for rich text box just works
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
                if (_uiTextBox != null)
                {
                    return _uiTextBox.MaxLength;
                }

                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.MaxLength;
                }

                return 0;
            }
            set
            {
                if (_uiTextBox != null)
                {
                    _uiTextBox.MaxLength = value;
                }

                if (_simpleTextBox != null)
                {
                    _simpleTextBox.MaxLength = value;
                }
            }
        }

        public bool UseSystemPasswordChar
        {
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.UseSystemPasswordChar;
                }

                return false;
            }
            set
            {
                if (_simpleTextBox != null)
                {
                    _simpleTextBox.UseSystemPasswordChar = value;
                }
            }
        }

        public bool ReadOnly
        {
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.ReadOnly;
                }

                if (_uiTextBox != null)
                {
                    return _uiTextBox.ReadOnly;
                }

                return false;
            }

            set
            {
                if (_simpleTextBox != null)
                {
                    _simpleTextBox.ReadOnly = value;
                }

                if (_uiTextBox != null)
                {
                    _uiTextBox.ReadOnly = value;
                }
            }
        }

        public string[] Lines
        {
            get
            {
                if (_simpleTextBox != null)
                {
                    return _simpleTextBox.Lines;
                }

                if (_uiTextBox != null)
                {
                    return _uiTextBox.Lines;
                }

                return Array.Empty<string>();
            }
        }

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
            if (_uiTextBox != null)
            {
                _uiTextBox.BackColor = DarkTheme.BackColor;
                _uiTextBox.ForeColor = DarkTheme.ForeColor;
                _uiTextBox.HandleCreated += SeTextBoxBoxHandleCreated;
                DarkTheme.SetWindowThemeDark(_uiTextBox);
            }
            if (_simpleTextBox != null)
            {
                _simpleTextBox.BackColor = DarkTheme.BackColor;
                _simpleTextBox.ForeColor = DarkTheme.ForeColor;
                _simpleTextBox.HandleCreated += SeTextBoxBoxHandleCreated;
                DarkTheme.SetWindowThemeDark(_simpleTextBox);
            }
            DarkTheme.SetWindowThemeDark(this);
        }

        public void UndoDarkTheme()
        {
            if (_uiTextBox != null)
            {
                _uiTextBox.BackColor = SystemColors.Window;
                _uiTextBox.ForeColor = DefaultForeColor;
                _uiTextBox.HandleCreated -= SeTextBoxBoxHandleCreated;
                DarkTheme.SetWindowThemeNormal(_uiTextBox);
            }
            if (_simpleTextBox != null)
            {
                _simpleTextBox.BackColor = SystemColors.Window;
                _simpleTextBox.ForeColor = DefaultForeColor;
                _simpleTextBox.HandleCreated -= SeTextBoxBoxHandleCreated;
                DarkTheme.SetWindowThemeNormal(this);
            }
            DarkTheme.SetWindowThemeNormal(_simpleTextBox);
        }

        private static void SeTextBoxBoxHandleCreated(object sender, EventArgs e) => DarkTheme.SetWindowThemeDark((Control)sender);

        public void ScrollToCaret()
        {
            if (_uiTextBox != null)
            {
                _uiTextBox.ScrollToCaret();
            }
            if (_simpleTextBox != null)
            {
                _simpleTextBox.ScrollToCaret();
            }
        }
    }
}

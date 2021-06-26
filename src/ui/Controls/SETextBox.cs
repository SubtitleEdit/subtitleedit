using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    /// <summary>
    /// TextBox that can be either a normal text box or a rich text box.
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

        private AdvancedTextBox _uiTextBox;
        private SimpleTextBox _simpleTextBox;

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
            _uiTextBox?.Dispose();
            _simpleTextBox = null;
            _uiTextBox = null;
            if (useSyntaxColoring)
            {
                _uiTextBox = new AdvancedTextBox { BorderStyle = BorderStyle.None, Multiline = true };
                InitializeBackingControl(_uiTextBox);
                UpdateFontAndColors(_uiTextBox);
            }
            else
            {
                _simpleTextBox = new SimpleTextBox { BorderStyle = BorderStyle.None, Multiline = true };
                InitializeBackingControl(_simpleTextBox);
                UpdateFontAndColors(_simpleTextBox);
            }

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

        public async Task CheckForLanguageChange(Subtitle subtitle)
        {
            if (_uiTextBox == null)
            {
                return;
            }

            await _uiTextBox.CheckForLanguageChange(subtitle);
        }

        public async Task InitializeLiveSpellCheck(Subtitle subtitle, int lineNumber)
        {
            if (_uiTextBox == null)
            {
                return;
            }

            await _uiTextBox.InitializeLiveSpellCheck(subtitle, lineNumber);
        }

        public void DisposeHunspellAndDictionaries() => _uiTextBox?.DisposeHunspellAndDictionaries();

        public void AddSuggestionsToMenu() => _uiTextBox?.AddSuggestionsToMenu();

        public void DoAction(SpellCheckAction action) => _uiTextBox?.DoAction(action);

        #endregion
    }
}

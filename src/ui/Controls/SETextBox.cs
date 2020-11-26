using Nikse.SubtitleEdit.Controls.WebBrowser;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    /// <summary>
    /// TextBox with drag and drop.
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

        private string _dragText = string.Empty;
        private int _dragStartFrom;
        private long _dragStartTicks;
        private bool _dragRemoveOld;
        private bool _dragFromThis;
        private long _gotFocusTicks;
        private bool _checkRtfChange = true;
        private RichTextBox _richTextBoxTemp;
        private RichTextBox _uiTextBox;
        private TextBox _textBox;
        private int _mouseMoveSelectionLength;
        private WebBrowserEditBox _htmlBox;

        public SETextBox()
        {
            Initialize(Configuration.Settings.General.SubtitleTextBoxSyntaxColor);
        }

        private bool _useWebBrowser = false;
        public void Initialize(bool useSyntaxColoring)
        {
            ContextMenuStrip oldContextMenuStrip = null;
            var oldEnabled = true;
            if (_textBox != null)
            {
                oldContextMenuStrip = _textBox.ContextMenuStrip;
                oldEnabled = _textBox.Enabled;
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
            _textBox?.Dispose();
            _richTextBoxTemp?.Dispose();
            _uiTextBox?.Dispose();
            _htmlBox?.Dispose();
            _textBox = null;
            _htmlBox = null;
            _uiTextBox = null;
            _richTextBoxTemp = null;
            if (useSyntaxColoring)
            {
                if (_useWebBrowser)
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
                _textBox = new TextBox { BorderStyle = BorderStyle.None, Multiline = true };
                InitializeBackingControl(_textBox);
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
            textBox.AllowDrop = true;
            textBox.DragEnter += SETextBox_DragEnter;
            textBox.DragDrop += SETextBox_DragDrop;
            textBox.GotFocus += (sender, args) => { _gotFocusTicks = DateTime.UtcNow.Ticks; };
            textBox.MouseDown += SETextBox_MouseDown;
            textBox.MouseUp += SETextBox_MouseUp;
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
            UpdateFontAndColors(_textBox);
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
                if (_textBox != null)
                {
                    return _textBox.Text;
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
                if (_textBox != null)
                {
                    _textBox.Text = value;
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
                if (_textBox != null)
                {
                    return _textBox.SelectionStart;
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
                if (_textBox != null)
                {
                    _textBox.SelectionStart = value;
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
                if (_textBox != null)
                {
                    return _textBox.SelectionLength;
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
                if (_textBox != null)
                {
                    _textBox.SelectionLength = value;
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
                if (_textBox != null)
                {
                    return _textBox.HideSelection;
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
                if (_textBox != null)
                {
                    _textBox.HideSelection = value;
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
                if (_textBox != null)
                {
                    return _textBox.SelectedText;
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
                if (_textBox != null)
                {
                    _textBox.SelectedText = value;
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
                if (_textBox != null)
                {
                    return _textBox.Multiline;
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
                if (_textBox != null)
                {
                    _textBox.Multiline = value;
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
                if (_textBox != null)
                {
                    return _textBox.Enabled;
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
                if (_textBox != null)
                {
                    _textBox.Enabled = value;
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
                if (_textBox == null && _uiTextBox == null)
                {
                    return RichTextBoxScrollBars.None;
                }

                if (_textBox != null)
                {
                    if (_textBox.ScrollBars == System.Windows.Forms.ScrollBars.Both)
                    {
                        return RichTextBoxScrollBars.Both;
                    }

                    if (_textBox.ScrollBars == System.Windows.Forms.ScrollBars.Horizontal)
                    {
                        return RichTextBoxScrollBars.Horizontal;
                    }

                    if (_textBox.ScrollBars == System.Windows.Forms.ScrollBars.Vertical)
                    {
                        return RichTextBoxScrollBars.Vertical;
                    }

                    return RichTextBoxScrollBars.None;
                }

                return _uiTextBox.ScrollBars;
            }
            set
            {
                if (_textBox != null)
                {
                    if (value == RichTextBoxScrollBars.Both || value == RichTextBoxScrollBars.ForcedBoth)
                    {
                        _textBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
                    }
                    else if (value == RichTextBoxScrollBars.Horizontal || value == RichTextBoxScrollBars.ForcedHorizontal)
                    {
                        _textBox.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
                    }
                    else if (value == RichTextBoxScrollBars.Vertical || value == RichTextBoxScrollBars.ForcedVertical)
                    {
                        _textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
                    }

                    _textBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
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
                if (_textBox != null)
                {
                    return _textBox.ContextMenuStrip;
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
                if (_textBox != null)
                {
                    _textBox.ContextMenuStrip = value;
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
            if (_textBox != null)
            {
                return _textBox.GetCharIndexFromPosition(pt);
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
            if (_textBox != null)
            {
                _textBox.SelectAll();
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
            if (_textBox != null)
            {
                _textBox.Clear();
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
            if (_textBox != null)
            {
                _textBox.Undo();
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
            if (_textBox != null)
            {
                _textBox.ClearUndo();
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
            if (_textBox != null)
            {
                _textBox.Copy();
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
            if (_textBox != null)
            {
                _textBox.Cut();
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
            if (_textBox != null)
            {
                _textBox.Paste();
            }
            else if (_textBox != null)
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
                if (_textBox != null)
                {
                    return _textBox.Focused;
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
            if (_textBox != null)
            {
                _textBox.Focus();
            }
            else if (_textBox != null)
            {
                _uiTextBox.Focus();
            }
            else if (_htmlBox != null)
            {
                _htmlBox.Focus();
            }
        }

        private void SETextBox_MouseUp(object sender, MouseEventArgs e)
        {
            _dragRemoveOld = false;
            _dragFromThis = false;
        }

        private void SETextBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (MouseButtons == MouseButtons.Left && !string.IsNullOrEmpty(_dragText))
            {
                var pt = new Point(e.X, e.Y);
                int index = GetCharIndexFromPosition(pt);
                if (index >= _dragStartFrom && index <= _dragStartFrom + _dragText.Length)
                {
                    // re-make selection
                    SelectionStart = _dragStartFrom;
                    SelectionLength = _dragText.Length;

                    try
                    {
                        var dataObject = new DataObject();
                        dataObject.SetText(_dragText, TextDataFormat.UnicodeText);
                        dataObject.SetText(_dragText, TextDataFormat.Text);

                        _dragFromThis = true;
                        if (ModifierKeys == Keys.Control)
                        {
                            _dragRemoveOld = false;
                            DoDragDrop(dataObject, DragDropEffects.Copy);
                        }
                        else if (ModifierKeys == Keys.None)
                        {
                            _dragRemoveOld = true;
                            DoDragDrop(dataObject, DragDropEffects.Move);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private void SETextBox_DragDrop(object sender, DragEventArgs e)
        {
            var pt = PointToClient(new Point(e.X, e.Y));
            int index = GetCharIndexFromPosition(pt);

            string newText;
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                newText = (string)e.Data.GetData(DataFormats.UnicodeText);
            }
            else
            {
                newText = (string)e.Data.GetData(DataFormats.Text);
            }

            if (string.IsNullOrWhiteSpace(Text))
            {
                Text = newText;
            }
            else
            {
                bool justAppend = index == Text.Length - 1 && index > 0;
                const string expectedChars = ":;]<.!?؟";
                if (_dragFromThis)
                {
                    _dragFromThis = false;
                    long milliseconds = (DateTime.UtcNow.Ticks - _dragStartTicks) / 10000;
                    if (milliseconds < 400)
                    {
                        SelectionLength = 0;
                        if (justAppend)
                        {
                            index++;
                        }

                        SelectionStart = index;
                        return; // too fast - nobody can drag and drop this fast
                    }

                    if (index >= _dragStartFrom && index <= _dragStartFrom + _dragText.Length)
                    {
                        return; // don't drop same text at same position
                    }

                    if (_dragRemoveOld)
                    {
                        _dragRemoveOld = false;
                        Text = Text.Remove(_dragStartFrom, _dragText.Length);

                        // fix spaces
                        if (_dragStartFrom == 0 && Text.Length > 0 && Text[0] == ' ')
                        {
                            Text = Text.Remove(0, 1);
                            index--;
                        }
                        else if (_dragStartFrom > 1 && Text.Length > _dragStartFrom + 1 && Text[_dragStartFrom] == ' ' && Text[_dragStartFrom - 1] == ' ')
                        {
                            Text = Text.Remove(_dragStartFrom, 1);
                            if (_dragStartFrom < index)
                            {
                                index--;
                            }
                        }
                        else if (_dragStartFrom > 0 && Text.Length > _dragStartFrom + 1 && Text[_dragStartFrom] == ' ' && expectedChars.Contains(Text[_dragStartFrom + 1]))
                        {
                            Text = Text.Remove(_dragStartFrom, 1);
                            if (_dragStartFrom < index)
                            {
                                index--;
                            }
                        }

                        // fix index
                        if (index > _dragStartFrom)
                        {
                            index -= _dragText.Length;
                        }

                        if (index < 0)
                        {
                            index = 0;
                        }
                    }
                }
                if (justAppend)
                {
                    index = Text.Length;
                    Text += newText;
                }
                else
                {
                    Text = Text.Insert(index, newText);
                }

                // fix start spaces
                int endIndex = index + newText.Length;
                if (index > 0 && !newText.StartsWith(' ') && Text[index - 1] != ' ')
                {
                    Text = Text.Insert(index, " ");
                    endIndex++;
                }
                else if (index > 0 && newText.StartsWith(' ') && Text[index - 1] == ' ')
                {
                    Text = Text.Remove(index, 1);
                    endIndex--;
                }

                // fix end spaces
                if (endIndex < Text.Length && !newText.EndsWith(' ') && Text[endIndex] != ' ')
                {
                    bool lastWord = expectedChars.Contains(Text[endIndex]);
                    if (!lastWord)
                    {
                        Text = Text.Insert(endIndex, " ");
                    }
                }
                else if (endIndex < Text.Length && newText.EndsWith(' ') && Text[endIndex] == ' ')
                {
                    Text = Text.Remove(endIndex, 1);
                }

                SelectionStart = index + 1;
                UiUtil.SelectWordAtCaret(this);
            }

            _dragRemoveOld = false;
            _dragFromThis = false;
        }

        private void SETextBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                e.Effect = ModifierKeys == Keys.Control ? DragDropEffects.Copy : DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private const int WM_LBUTTONDOWN = 0x0201;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN)
            {
                long milliseconds = (DateTime.UtcNow.Ticks - _gotFocusTicks) / 10000;
                if (milliseconds > 10)
                {
                    _dragText = SelectedText;
                    _dragStartFrom = SelectionStart;
                    _dragStartTicks = DateTime.UtcNow.Ticks;
                }
            }
            base.WndProc(ref m);
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

            if (_textBox != null)
            {
                if (Configuration.Settings.General.CenterSubtitleInTextBox &&
                    _textBox.TextAlign != HorizontalAlignment.Center)
                {
                    _textBox.TextAlign = HorizontalAlignment.Center;
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

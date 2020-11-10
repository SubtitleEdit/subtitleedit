using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    /// <summary>
    /// TextBox with drag and drop.
    /// </summary>
    public sealed class SETextBox : RichTextBox
    {
        private string _dragText = string.Empty;
        private int _dragStartFrom;
        private long _dragStartTicks;
        private bool _dragRemoveOld;
        private bool _dragFromThis;
        private long _gotFocusTicks;
        private bool _checkRtfChange = true;

        public SETextBox()
        {
            AllowDrop = true;
            DragEnter += SETextBox_DragEnter;
            DragDrop += SETextBox_DragDrop;
            GotFocus += (sender, args) => { _gotFocusTicks = DateTime.UtcNow.Ticks; };
            MouseDown += SETextBox_MouseDown;
            MouseUp += SETextBox_MouseUp;
            TextChanged += TextChangedHighlight;
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

        private void HighlightHtmlText(bool ignoreActiveWord)
        {
            var text = Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var sb = new StringBuilder();
            if (Configuration.Settings.General.RightToLeftMode) // (RightToLeft == RightToLeft.Yes)
            {
                if (RightToLeft != RightToLeft.Yes)
                {
                    RightToLeft = RightToLeft.Yes;
                }
            }
            else if (RightToLeft != RightToLeft.No)
            {
                RightToLeft = RightToLeft.No;
            }

            sb.Append(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 " + Configuration.Settings.General.SubtitleFontName + @";}}
{\colortbl ;\red100\green149\blue237;\red95\green158\blue160;\red0\green0\blue0;\red200\green100\blue100;}
\viewkind4\uc1\pard" + GetRtfAlignmentCode() + @"\cf3" + (Configuration.Settings.General.SubtitleFontBold ? "\\b" : string.Empty) + @"\f0\fs" + (Configuration.Settings.General.SubtitleFontSize + 14) + " ");

            var start = SelectionStart;
            bool htmlTagOn = false;
            bool assaTagOn = false;
            int tagOn = -1;
            var textLength = text.Length;
            int i = 0;
            string normalFontColor = "\\cf3";
            string htmlFontColor = "\\cf2";
            string assaFontColor = "\\cf1";
            string badSpellFontColor = "\\cf4";
            bool wordOn = false;
            bool wordOnBad = false;
            while (i < textLength)
            {
                var ch = text[i];
                if (assaTagOn)
                {
                    AppendCharToRtf(sb, ch);
                    if (ch == '}' && tagOn >= 0)
                    {
                        assaTagOn = false;
                        sb.Append(normalFontColor + " ");
                    }
                }
                else if (htmlTagOn)
                {
                    AppendCharToRtf(sb, ch);
                    if (ch == '>' && tagOn >= 0)
                    {
                        htmlTagOn = false;
                        sb.Append(normalFontColor + " ");
                    }
                }
                else if (ch == '{' && i < textLength - 1 && text[i + 1] == '\\' && text.IndexOf('}', i) > 0)
                {
                    wordOn = false;
                    if (wordOnBad)
                    {

                    }
                    wordOnBad = false;
                    assaTagOn = true;
                    tagOn = i;
                    sb.Append(assaFontColor + " ");
                    AppendCharToRtf(sb, ch);
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
                        s.StartsWith("</font>", StringComparison.OrdinalIgnoreCase) ||
                        (s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase) &&
                         text.IndexOf("</font>", i, StringComparison.OrdinalIgnoreCase) > 0))
                    {
                        wordOn = false;
                        if (wordOnBad)
                        {

                        }
                        wordOnBad = false;
                        htmlTagOn = true;
                        tagOn = i;
                        sb.Append(htmlFontColor + " ");
                        AppendCharToRtf(sb, ch);
                    }
                    else
                    {
                        wordOn = AddWordWithSpellCheck(wordOn, ch, text, i, sb, normalFontColor, start, ref wordOnBad, ignoreActiveWord);
                        AppendCharToRtf(sb, ch);
                    }
                }
                else
                {
                    wordOn = AddWordWithSpellCheck(wordOn, ch, text, i, sb, normalFontColor, start, ref wordOnBad, ignoreActiveWord);
                    AppendCharToRtf(sb, ch);
                }

                i++;
            }

            sb.AppendLine("\\par");
            sb.Append("}");
            SuspendLayout();
            start = SelectionStart;
            var length = SelectionLength;
            Rtf = sb.ToString();
            SelectionStart = start;
            SelectionLength = length;
            ResumeLayout();
        }

        private static string GetRtfAlignmentCode()
        {
            if (Configuration.Settings.General.RightToLeftMode)
            {
                return @"\rtlpar\qr";
            }

            var result = Configuration.Settings.General.CenterSubtitleInTextBox ? @"\qc" : @"\ltrpar";
            return result;
        }

        private bool AddWordWithSpellCheck(bool wordOn, char ch, string text, int i, StringBuilder sb, string normalFontColor, int richTextIndex, ref bool wordOnBad, bool ignoreActiveWord)
        {

            if (!wordOn && char.IsLetterOrDigit(ch))
            {
                var word = GetSelectedWord(text, i, out var _);
                if (ignoreActiveWord && richTextIndex >= i && richTextIndex <= i + word.Length)
                {
                    return true;
                }

                //if (!IsWordSpelledCorrect(word))
                //{
                //    wordOnBad = true;
                //    sb.Append(badSpellFontColor + " ");
                //}

                return true;
            }

            if (wordOn && !char.IsLetterOrDigit(ch))
            {
                wordOn = false;
            }

            if (!wordOn && wordOnBad)
            {
                sb.Append(normalFontColor + " ");
                wordOnBad = false;
            }

            return wordOn;
        }

        private static void AppendCharToRtf(StringBuilder sb, char ch)
        {
            if (ch == '\r')
            {
                // nothing
            }
            else if (ch == '\n')
            {
                sb.AppendLine("\\par");
            }
            else if (ch >= 0x20 && ch < 0x80)
            {
                if (ch == '\\' || ch == '{' || ch == '}')
                {
                    sb.Append('\\');
                }
                sb.Append(ch);
            }
            else if (ch < 0x20 || (ch >= 0x80 && ch <= 0xFF))
            {
                sb.Append($"\\'{((byte)ch):X}");
            }
            else
            {
                sb.Append($"\\u{(short)ch}?");
            }
        }

        private static string GetSelectedWord(string txt, int pos, out int startPos)
        {
            startPos = pos;
            while (startPos >= 0)
            {
                // Allow letters and digits
                var ch = txt[startPos];
                if (!char.IsLetter(ch) && ch != '\'')
                {
                    break;
                }

                startPos--;
            }

            startPos++;

            // Find the end of the word.
            var endPos = pos;
            while (endPos < txt.Length)
            {
                char ch = txt[endPos];
                if (!char.IsLetter(ch) && ch != '\'')
                {
                    break;
                }

                endPos++;
            }

            endPos--;

            return startPos > endPos ? string.Empty : txt.Substring(startPos, endPos - startPos + 1);
        }

        private void TextChangedHighlight(object sender, EventArgs e)
        {
            if (_checkRtfChange)
            {
                _checkRtfChange = false;
                HighlightHtmlText(true);
                _checkRtfChange = true;
            }
        }
    }
}

using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    /// <summary>
    /// TextBox with drag and drop and double click selects current word.
    /// </summary>
    public sealed class SimpleTextBox : TextBox
    {
        private string _dragText = string.Empty;
        private int _dragStartFrom;
        private long _dragStartTicks;
        private bool _dragRemoveOld;
        private bool _dragFromThis;
        private long _gotFocusTicks;

        public SimpleTextBox()
        {
            SetAlignment();
            AllowDrop = true;

            DragEnter += SETextBox_DragEnter;
            DragDrop += SETextBox_DragDrop;
            MouseDown += SETextBox_MouseDown;
            MouseUp += SETextBox_MouseUp;
            KeyDown += SETextBox_KeyDown;

            // To fix issue where WM_LBUTTONDOWN got wrong "SelectedText" (only in undocked mode)
            GotFocus += (sender, args) => { _gotFocusTicks = DateTime.UtcNow.Ticks; };
        }

        private void SetAlignment()
        {
            if (Configuration.Settings.General.CenterSubtitleInTextBox && TextAlign != HorizontalAlignment.Center)
            {
                TextAlign = HorizontalAlignment.Center;
            }
        }

        //TODO: make textbox work with shift+up/down
        //[DllImport("user32.dll")]
        //public static extern bool GetCaretPos(out System.Drawing.Point lpPoint);

        private bool? KeyDownAtTop;
        private bool? KeyDownAtBottom;
        private void SETextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //var p = new Point();
            //bool result = GetCaretPos(out p);
            //int idx = GetCharIndexFromPosition(p);
            //Console.WriteLine(idx);


            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Back)
            {
                UiUtil.ApplyControlBackspace(this);
                e.SuppressKeyPress = true;
            }
            //else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Up)
            //{
            //    var line = GetLineFromCharIndex(SelectionStart);
            //    if (line == 0)
            //    {
            //        if (SelectionLength >= TextLength)
            //        {
            //            e.SuppressKeyPress = true;
            //            if (KeyDownAtBottom == true && Lines.Length > 1)
            //            {
            //                KeyDownAtTop = null;
            //                KeyDownAtBottom = null;
            //                var charIndex = GetFirstCharIndexFromLine(Lines.Length - 1) - Environment.NewLine.Length;
            //                SelectionStart = 0;
            //                SelectionLength = 0;
            //                Refresh();
            //                SelectionLength = charIndex;
            //                KeyDownAtTop = null;
            //                KeyDownAtBottom = null;
            //            }
            //            else if (KeyDownAtTop == true)
            //            {
            //                var pOld = GetPositionFromCharIndex(SelectionStart);
            //                var pNew = new Point(pOld.X, pOld.Y - Font.Height);
            //                var charIndex = GetCharIndexFromPosition(pNew);
            //                SelectionStart = charIndex;
            //                SelectionLength = charIndex + TextLength - charIndex;
            //                if (SelectionLength > 1)
            //                    SelectionLength -= 2;
            //                KeyDownAtTop = true;
            //                KeyDownAtBottom = false;
            //            }
            //        }
            //        else
            //        {
            //            //e.SuppressKeyPress = true;
            //            //var oldSelectionStart = SelectionStart;
            //            //var oldSelectionLength = SelectionLength;
            //            //SelectionStart = 0;
            //            //SelectionLength = oldSelectionStart + oldSelectionLength;
            //            //KeyDownAtTop = null;
            //            //KeyDownAtBottom = null;
            //        }
            //    }

            //}
            //else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Down)
            //{
            //    var line = GetLineFromCharIndex(SelectionStart + SelectionLength);
            //    if (line == Lines.Length - 1)
            //    {
            //        if (SelectionLength >= TextLength)
            //        {
            //            e.SuppressKeyPress = true;
            //            var pOld = GetPositionFromCharIndex(TextLength + 1);
            //            var pNew = new Point(pOld.X, pOld.Y + Font.Height);
            //            var charIndex = GetCharIndexFromPosition(pNew);
            //            SelectionStart = charIndex;
            //            SelectionLength = charIndex + TextLength - charIndex + 1;
            //            KeyDownAtTop = null;
            //            KeyDownAtBottom = null;
            //        }
            //        else
            //        {
            //            e.SuppressKeyPress = true;
            //            SelectionLength = Text.Length - SelectionStart + 1;
            //            KeyDownAtTop = false;
            //            KeyDownAtBottom = true;
            //        }
            //    }
            //}
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
                var index = GetCharIndexFromPosition(pt);
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
            var index = GetCharIndexFromPosition(pt);

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
                var justAppend = index == Text.Length - 1 && index > 0;
                const string expectedChars = ":;]<.!?؟";
                if (_dragFromThis)
                {
                    _dragFromThis = false;
                    var milliseconds = (DateTime.UtcNow.Ticks - _dragStartTicks) / 10000;
                    if (milliseconds < 400)
                    {
                        SelectionLength = 0;
                        if (justAppend)
                        {
                            index++;
                        }

                        SelectionStart = index;
                        return; // too fast - nobody can drag'n'drop this fast
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
                var endIndex = index + newText.Length;
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
                    var lastWord = expectedChars.Contains(Text[endIndex]);
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

        private const int WM_DBLCLICK = 0xA3;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_LBUTTONDOWN = 0x0201;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DBLCLICK || m.Msg == WM_LBUTTONDBLCLK)
            {
                UiUtil.SelectWordAtCaret(this);
                return;
            }

            if (m.Msg == WM_LBUTTONDOWN)
            {
                var milliseconds = (DateTime.UtcNow.Ticks - _gotFocusTicks) / 10000;
                if (milliseconds > 10)
                {
                    _dragText = SelectedText;
                    _dragStartFrom = SelectionStart;
                    _dragStartTicks = DateTime.UtcNow.Ticks;
                }
            }

            base.WndProc(ref m);
        }
    }
}

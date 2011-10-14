using System;
using System.Windows.Forms;
using System.Drawing;

namespace Nikse.SubtitleEdit.Controls
{
    /// <summary>
    /// TextBox where double click selects current word
    /// </summary>
    public class SETextBox : TextBox
    {

        string _dragText = string.Empty;
        int _dragStartFrom = 0;
        long _dragStartTicks = 0;

        public SETextBox()
        {
            AllowDrop = true;
            DragEnter += new DragEventHandler(SETextBox_DragEnter);
            DragDrop += new DragEventHandler(SETextBox_DragDrop);
            MouseDown += new MouseEventHandler(SETextBox_MouseDown);
        }

        void SETextBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (MouseButtons == System.Windows.Forms.MouseButtons.Left && !string.IsNullOrEmpty(_dragText))
            {
                Point pt = new Point(e.X, e.Y);
                int index = GetCharIndexFromPosition(pt);
                if (index >= _dragStartFrom && index <= _dragStartFrom + _dragText.Length)
                {
                    if (Control.ModifierKeys == Keys.Control)
                        DoDragDrop(_dragText, DragDropEffects.Copy);
                    else if (Control.ModifierKeys == Keys.None)
                        DoDragDrop(_dragText, DragDropEffects.Move);
                }
            }
        }

        void SETextBox_DragDrop(object sender, DragEventArgs e)
        {
            Point pt = new Point(e.X, e.Y);
            pt = PointToClient(pt);
            int index = GetCharIndexFromPosition(pt);

            string newText = (string)e.Data.GetData(DataFormats.Text);
            if (Text.Trim().Length == 0)
            {
                Text = newText;
            }
            else
            {
                if (sender == this)
                {
                    long milliseconds = (DateTime.Now.Ticks - _dragStartTicks) / 10000;
                    if (milliseconds < 400)
                        return; // too fast - nobody can drag'n'drop this fast

                    Text = Text.Remove(_dragStartFrom, _dragText.Length);
                    if (index > _dragStartFrom)
                        index -= _dragText.Length;
                    if (index < 0)
                        index = 0;                        
                }
                Text = Text.Insert(index, newText);

                // fix start spaces
                int endIndex = index + newText.Length;
                if (index > 0 && !newText.StartsWith(" ") && Text[index - 1] != ' ')
                {
                    Text = Text.Insert(index, " ");
                    endIndex++;
                }
                else if (index > 0 && newText.StartsWith(" ") && Text[index - 1] == ' ')
                {
                    Text = Text.Remove(index, 1);
                    endIndex--;
                }

                // fix end spaces
                if (endIndex < Text.Length && !newText.EndsWith(" ") && Text[endIndex] != ' ')
                    Text = Text.Insert(endIndex, " ");
                else if (endIndex < Text.Length && newText.EndsWith(" ") && Text[endIndex] == ' ')
                    Text = Text.Remove(endIndex, 1);

            }
        }

        void SETextBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                if (Control.ModifierKeys == Keys.Control)
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_DBLCLICK = 0xA3;
            const int WM_LBUTTONDBLCLK = 0x203;
            const int WM_LBUTTONDOWN = 0x0201;

            if (m.Msg == WM_DBLCLICK || m.Msg == WM_LBUTTONDBLCLK)
            {
                SelectCurrentWord(this);
                return;
            }
            if (m.Msg == WM_LBUTTONDOWN)
            {
                _dragText = SelectedText;
                _dragStartFrom = SelectionStart;
                _dragStartTicks = DateTime.Now.Ticks;
            }
            base.WndProc(ref m);
        }

        private void SelectCurrentWord(TextBox tb)
        {
            string breakChars = "\".!?,)([]<>:;♪{}-/#*| ¿¡" + Environment.NewLine + "\t";
            int selectionLength = 0;
            int i = tb.SelectionStart;
            while (i > 0 && breakChars.Contains(tb.Text.Substring(i - 1, 1)) == false)
                i--;
            tb.SelectionStart = i;
            for (; i < tb.Text.Length; i++)
            {
                if (breakChars.Contains(tb.Text.Substring(i, 1)))
                    break;
                selectionLength++;
            }
            if (tb.SelectionStart + selectionLength < tb.Text.Length && tb.Text[tb.SelectionStart + selectionLength] == ' ')
                selectionLength++;

            tb.SelectionLength = selectionLength;
            if (selectionLength > 0)
                this.OnMouseMove(null);
        }

    }
}

using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    /// <summary>
    /// TextBox where double click selects current word
    /// </summary>
    public class SETextBox : TextBox
    {

        protected override void WndProc(ref Message m)
        {
            const int WM_DBLCLICK = 0xA3;
            const int WM_LBUTTONDBLCLK = 0x203;

            if (m.Msg == WM_DBLCLICK || m.Msg == WM_LBUTTONDBLCLK)
            {
                SelectCurrentWord(this);
                return;
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
            tb.SelectionLength = selectionLength;
            if (selectionLength > 0)
                this.OnMouseMove(null);
        }

    }
}

using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public class NikseTextBox : TextBox
    {
        private const int WM_NCPAINT = 0x85;
        private const int WM_PAINT = 0x0f;
        
        Color _focusedColor = Color.FromArgb(0, 120, 215);
        public Color FocusedColor
        {
            get => _focusedColor;
            set
            {
                _focusedColor = value;
                Invalidate();
            }
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (Focused && (m.Msg == WM_PAINT || m.Msg == WM_NCPAINT))
            {
                using (var g = this.CreateGraphics())
                {
                    using (var p = new Pen(FocusedColor))
                    {
                        g.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
                    }
                }
            }
        }
    }
}

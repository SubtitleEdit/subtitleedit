using Nikse.SubtitleEdit.Logic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Controls.Interfaces;

namespace Nikse.SubtitleEdit.Controls
{
    public class NikseTextBox : TextBox, ISelectedText
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

        public NikseTextBox()
        {
            KeyDown += NikseTextBox_KeyDown;
        }

        private void NikseTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Back)
            {
                e.SuppressKeyPress = true;
                UiUtil.ApplyControlBackspace(this);
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
                        var widthSubtract = 1;
                        if (ScrollBars == ScrollBars.Vertical || ScrollBars == ScrollBars.Both)
                        {
                            widthSubtract += SystemInformation.VerticalScrollBarWidth + SystemInformation.BorderSize.Width;
                        }

                        g.DrawRectangle(p, new Rectangle(0, 0, Width - widthSubtract, Height - 1));
                    }
                }
            }
        }
    }
}

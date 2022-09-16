using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Nikse.SubtitleEdit.Controls
{
    // Derived from: https://github.com/dotnet/winforms/blob/46cd3ca36be6fe6107ea38edece3639be75fd045/src/System.Windows.Forms.Design/src/System/ComponentModel/Design/CollectionEditor.cs
    internal class SplitButton : Button
    {
        private PushButtonState _state;
        private const int PushButtonWidth = 14;
        private Rectangle _dropDownRectangle = new Rectangle();
        private bool _showSplit = true;
        private ContextMenuStrip _splitButtonContextMenuStrip;

        public bool ShowSplit
        {
            get => _showSplit;
            set
            {
                if (value != _showSplit)
                {
                    _showSplit = value;
                    Invalidate();
                }
            }
        }

        public ContextMenuStrip SplitButtonContextMenuStrip
        {
            get => _splitButtonContextMenuStrip;
            set
            {
                _splitButtonContextMenuStrip = value;
                Invalidate();
            }
        }

        private PushButtonState State
        {
            get => _state;
            set
            {
                if (!_state.Equals(value))
                {
                    _state = value;
                    Invalidate();
                }
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size preferredSize = base.GetPreferredSize(proposedSize);
            if (_showSplit && !string.IsNullOrEmpty(Text) && TextRenderer.MeasureText(Text, Font).Width + PushButtonWidth > preferredSize.Width)
            {
                return preferredSize + new Size(PushButtonWidth, 0);
            }

            return preferredSize;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData.Equals(Keys.Down) && _showSplit)
            {
                return true;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            if (!_showSplit)
            {
                base.OnGotFocus(e);
                return;
            }

            if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
            {
                State = PushButtonState.Default;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Down) && _showSplit)
            {
                ShowContextMenuStrip();
            }
            else
            {
                // We need to pass the unhandled characters (including Keys.Space) on
                // to base.OnKeyDown when it's not to drop the split menu
                base.OnKeyDown(e);
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            if (!_showSplit)
            {
                base.OnLostFocus(e);
                return;
            }
            if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
            {
                State = PushButtonState.Normal;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!_showSplit || e.Button != MouseButtons.Left)
            {
                base.OnMouseDown(e);
                return;
            }

            if (_dropDownRectangle.Contains(e.Location))
            {
                ShowContextMenuStrip();
            }
            else
            {
                State = PushButtonState.Pressed;
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (!_showSplit)
            {
                base.OnMouseEnter(e);
                return;
            }

            if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
            {
                State = PushButtonState.Hot;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!_showSplit)
            {
                base.OnMouseLeave(e);
                return;
            }

            if (!State.Equals(PushButtonState.Pressed) && !State.Equals(PushButtonState.Disabled))
            {
                if (Focused)
                {
                    State = PushButtonState.Default;
                }
                else
                {
                    State = PushButtonState.Normal;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!_showSplit || e.Button != MouseButtons.Left)
            {
                base.OnMouseUp(e);
                return;
            }

            if (_splitButtonContextMenuStrip == null || !_splitButtonContextMenuStrip.Visible)
            {
                SetButtonDrawState();
                if (Bounds.Contains(Parent.PointToClient(Cursor.Position)) && !_dropDownRectangle.Contains(e.Location))
                {
                    OnClick(new EventArgs());
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!_showSplit)
            {
                return;
            }

            Graphics g = e.Graphics;
            Rectangle bounds = new Rectangle(0, 0, Width, Height);
            TextFormatFlags formatFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;

            ButtonRenderer.DrawButton(g, bounds, State);

            _dropDownRectangle = new Rectangle(bounds.Right - PushButtonWidth - 1, 4, PushButtonWidth, bounds.Height - 8);

            if (RightToLeft == RightToLeft.Yes)
            {
                _dropDownRectangle.X = bounds.Left + 1;

                g.DrawLine(SystemPens.ButtonHighlight, bounds.Left + PushButtonWidth, 4, bounds.Left + PushButtonWidth, bounds.Bottom - 4);
                g.DrawLine(SystemPens.ButtonHighlight, bounds.Left + PushButtonWidth + 1, 4, bounds.Left + PushButtonWidth + 1, bounds.Bottom - 4);
                bounds.Offset(PushButtonWidth, 0);
                bounds.Width -= PushButtonWidth;
            }
            else
            {
                g.DrawLine(SystemPens.ButtonHighlight, bounds.Right - PushButtonWidth, 4, bounds.Right - PushButtonWidth, bounds.Bottom - 4);
                g.DrawLine(SystemPens.ButtonHighlight, bounds.Right - PushButtonWidth - 1, 4, bounds.Right - PushButtonWidth - 1, bounds.Bottom - 4);
                bounds.Width -= PushButtonWidth;
            }

            PaintArrow(g, _dropDownRectangle);

            // If we dont' use mnemonic, set formatFlag to NoPrefix as this will show ampersand.
            if (!UseMnemonic)
            {
                formatFlags |= TextFormatFlags.NoPrefix;
            }
            else if (!ShowKeyboardCues)
            {
                formatFlags |= TextFormatFlags.HidePrefix;
            }

            if (!string.IsNullOrEmpty(Text))
            {
                TextRenderer.DrawText(g, Text, Font, bounds, SystemColors.ControlText, formatFlags);
            }

            if (Focused)
            {
                bounds.Inflate(-4, -4);
            }
        }

        private void PaintArrow(Graphics g, Rectangle dropDownRect)
        {
            Point middle = new Point(Convert.ToInt32(dropDownRect.Left + dropDownRect.Width / 2), Convert.ToInt32(dropDownRect.Top + dropDownRect.Height / 2));

            // If the width is odd - favor pushing it over one pixel right.
            middle.X += (dropDownRect.Width % 2);

            Point[] arrow = new Point[] {
                    new Point(middle.X - 2, middle.Y - 1),
                    new Point(middle.X + 2 + 1, middle.Y - 1),
                    new Point(middle.X, middle.Y + 2)
                };

            g.FillPolygon(SystemBrushes.ControlText, arrow);
        }

        private void ShowContextMenuStrip()
        {
            State = PushButtonState.Pressed;
            if (_splitButtonContextMenuStrip != null)
            {
                _splitButtonContextMenuStrip.Closed += new ToolStripDropDownClosedEventHandler(ContextMenuStrip_Closed);
                _splitButtonContextMenuStrip.Show(this, 0, Height);
            }
        }

        private void ContextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (sender is ContextMenuStrip cms)
            {
                cms.Closed -= new ToolStripDropDownClosedEventHandler(ContextMenuStrip_Closed);
            }

            SetButtonDrawState();
        }

        private void SetButtonDrawState()
        {
            if (Bounds.Contains(Parent.PointToClient(Cursor.Position)))
            {
                State = PushButtonState.Hot;
            }
            else if (Focused)
            {
                State = PushButtonState.Default;
            }
            else
            {
                State = PushButtonState.Normal;
            }
        }
    }
}

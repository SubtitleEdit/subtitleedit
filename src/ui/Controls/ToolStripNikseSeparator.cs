using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public class ToolStripNikseSeparator : ToolStripItem
    {
        public ToolStripNikseSeparator() => ForeColor = SystemColors.ControlDark;

        public override bool CanSelect => DesignMode;

        protected override Size DefaultSize => new Size(6, 6);

        protected override Padding DefaultMargin => new Padding(2, 5, 2, 6);

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool DoubleClickEnabled
        {
            get => base.DoubleClickEnabled;
            set => base.DoubleClickEnabled = value;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool Enabled
        {
            get => base.Enabled;
            set => base.Enabled = value;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler EnabledChanged
        {
            add => base.EnabledChanged += value;
            remove => base.EnabledChanged -= value;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ToolStripItemDisplayStyle DisplayStyle
        {
            get => base.DisplayStyle;
            set => base.DisplayStyle = value;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler DisplayStyleChanged
        {
            add => base.DisplayStyleChanged += value;
            remove => base.DisplayStyleChanged -= value;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Font Font
        {
            get => base.Font;
            set => base.Font = value;
        }

        private bool IsVertical
        {
            get
            {
                ToolStrip toolStrip = Owner;
                if (toolStrip is ToolStripDropDownMenu)
                {
                    return false;
                }

                switch (toolStrip.LayoutStyle)
                {
                    case ToolStripLayoutStyle.VerticalStackWithOverflow:
                        return false;
                    default:
                        return true;
                }
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DefaultValue(ToolStripTextDirection.Horizontal)]
        public override ToolStripTextDirection TextDirection
        {
            get => base.TextDirection;
            set => base.TextDirection = value;
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            var toolStrip = Owner;
            if (toolStrip == null)
            {
                return new Size(6, 6);
            }

            if (toolStrip.LayoutStyle != ToolStripLayoutStyle.HorizontalStackWithOverflow || toolStrip.LayoutStyle != ToolStripLayoutStyle.VerticalStackWithOverflow)
            {
                constrainingSize.Width = 23;
                constrainingSize.Height = 23;
            }

            return this.IsVertical ? new Size(6, constrainingSize.Height) : new Size(constrainingSize.Width, 6);
        }

        private Color _foreColor;
        [Category("ToolStripNikseSeparator"), Description("Gets or sets the foreground color"), RefreshProperties(RefreshProperties.Repaint)]
        public new Color ForeColor
        {
            get => _foreColor;

            set
            {
                if (value.A == 0)
                {
                    return;
                }

                _foreColor = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Owner == null)
            {
                return;
            }

            base.OnPaint(e);
            using (var pen = new Pen(_foreColor, 1f))
            {
                e.Graphics.DrawLine(pen,
                     +Bounds.Width / 2,
                    Padding.Top,
                     Bounds.Width / 2,
                    Height - Padding.Bottom);
            }
        }

        protected override void SetBounds(Rectangle rect)
        {
            if (Owner is ToolStripDropDownMenu owner)
            {
                rect.X = 2;
                rect.Width = owner.Width - 4;
            }
            base.SetBounds(rect);
        }

        [ComVisible(true)]
        internal class ToolStripSeparatorAccessibleObject : ToolStripItem.ToolStripItemAccessibleObject
        {
            private ToolStripSeparator _ownerItem;

            public ToolStripSeparatorAccessibleObject(ToolStripSeparator ownerItem)
              : base(ownerItem)
            {
                _ownerItem = ownerItem;
            }

            public override AccessibleRole Role
            {
                get
                {
                    var accessibleRole = _ownerItem.AccessibleRole;
                    return accessibleRole != AccessibleRole.Default ? accessibleRole : AccessibleRole.Separator;
                }
            }
        }
    }
}


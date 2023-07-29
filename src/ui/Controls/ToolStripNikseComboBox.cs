using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public class ToolStripNikseComboBox : ToolStripControlHost
    {
        // ReSharper disable once InconsistentNaming
        public event EventHandler SelectedIndexChanged;

        // ReSharper disable once InconsistentNaming
        public event EventHandler DropDown;

        // ReSharper disable once InconsistentNaming
        public event EventHandler DropDownClosed;

        public ToolStripNikseComboBox(Control c) : base(c)
        {
            Init();
        }

        public ToolStripNikseComboBox()
            : base(ToolStripNikseComboBox.CreateControlInstance())
        {
            Init();
        }

        private void Init()
        {
            if (DesignMode)
            {
                return;
            }

            if (Control is ToolStripNikseComboBoxControl cbc)
            {
                cbc.Owner = this;
            }

            Padding = new Padding(2);

            ComboBox.SelectedIndexChanged += (sender, args) =>
            {
                SelectedIndexChanged?.Invoke(sender, args);
            };

            ComboBox.DropDown += (sender, args) =>
            {
                DropDown?.Invoke(sender, args);
            };

            ComboBox.DropDownClosed += (sender, args) =>
            {
                DropDownClosed?.Invoke(sender, args);
            };

            ComboBox.LostFocus += (sender, args) =>
            {
                Invalidate();
            };

            LostFocus += (sender, args) =>
            {
                Invalidate();
            };
        }

        public ToolStripNikseComboBox(Control c, string name) : base(c, name)
        {
            Init();
        }

        private static Control CreateControlInstance()
        {
            return new ToolStripNikseComboBoxControl();
        }

        protected override Size DefaultSize => new Size(100, 22);

        public override Size GetPreferredSize(Size constrainingSize)
        {
            var preferredSize = base.GetPreferredSize(constrainingSize);
            preferredSize.Width = Width;
            return preferredSize;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public NikseComboBoxCollection Items => ComboBox?.Items;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NikseComboBox ComboBox => Control as NikseComboBox;

        public int SelectedIndex
        {
            get => ComboBox.SelectedIndex;
            set => ComboBox.SelectedIndex = value;
        }

        public string SelectedText
        {
            get => ComboBox.SelectedText;
            set => ComboBox.SelectedText = value;
        }

        public ComboBoxStyle DropDownStyle
        {
            get => ComboBox.DropDownStyle;
            set => ComboBox.DropDownStyle = value;
        }

        public int DropDownHeight
        {
            get => ComboBox.DropDownHeight;
            set => ComboBox.DropDownHeight = value;
        }

        public Color ButtonForeColor
        {
            get => ComboBox.ButtonForeColor;
            set => ComboBox.ButtonForeColor = value;
        }

        public Color ButtonForeColorOver
        {
            get => ComboBox.ButtonForeColorOver;
            set => ComboBox.ButtonForeColorOver = value;
        }

        public Color ButtonForeColorDown
        {
            get => ComboBox.ButtonForeColorDown;
            set => ComboBox.ButtonForeColorDown = value;
        }

        public Color BorderColor
        {
            get => ComboBox.BorderColor;
            set => ComboBox.BorderColor = value;
        }

        public Color BackColorDisabled
        {
            get => ComboBox.BackColorDisabled;
            set => ComboBox.BackColorDisabled = value;
        }

        public new Color BackColor
        {
            get => ComboBox.BorderColor;
            set => ComboBox.BorderColor = value;
        }

        public object SelectedItem
        {
            get => ComboBox.SelectedItem;
            set => ComboBox.SelectedItem = value;
        }

        public bool DroppedDown => ComboBox.DroppedDown;

        internal class ToolStripNikseComboBoxControl : NikseComboBox
        {
            public ToolStripNikseComboBoxControl()
            {
                SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            }

            public ToolStripNikseComboBox Owner { get; set; }
        }

        public void BeginUpdate()
        {
            ComboBox?.BeginUpdate();
        }

        public void EndUpdate()
        {
            ComboBox?.EndUpdate();
        }
    }
}

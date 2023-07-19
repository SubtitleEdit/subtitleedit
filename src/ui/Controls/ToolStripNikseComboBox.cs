using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Nikse.SubtitleEdit.Controls
{
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.MenuStrip | ToolStripItemDesignerAvailability.ContextMenuStrip)]
    [DefaultProperty("Items")]
    public class ToolStripNikseComboBox : ToolStripControlHost
    {
        internal static readonly object EventDropDown = new object();
        internal static readonly object EventDropDownClosed = new object();
        internal static readonly object EventDropDownStyleChanged = new object();
        internal static readonly object EventSelectedIndexChanged = new object();
        internal static readonly object EventSelectionChangeCommitted = new object();
        internal static readonly object EventTextUpdate = new object();
        private static readonly Padding dropDownPadding = new Padding(2);
        private static readonly Padding padding = new Padding(1, 0, 1, 0);
        private Padding scaledDropDownPadding = ToolStripNikseComboBox.dropDownPadding;
        private Padding scaledPadding = ToolStripNikseComboBox.padding;

        public ToolStripNikseComboBox()
          : base(ToolStripNikseComboBox.CreateControlInstance())
        {
            (this.Control as ToolStripNikseComboBox.ToolStripNikseComboBoxControl).Owner = this;
        }

        public ToolStripNikseComboBox(string name)
          : this()
        {
            this.Name = name;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ToolStripNikseComboBox(Control c)
          : base(c)
        {
            throw new NotSupportedException();
        }

        private static Control CreateControlInstance()
        {
            NikseComboBox controlInstance = (NikseComboBox)new ToolStripNikseComboBox.ToolStripNikseComboBoxControl();
         //   controlInstance.FlatStyle = FlatStyle.Popup;
            return (Control)controlInstance;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Localizable(true)]
        [Description("ComboBoxAutoCompleteCustomSourceDescr")]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get => this.ComboBox.AutoCompleteCustomSource;
            set => this.ComboBox.AutoCompleteCustomSource = value;
        }

        [DefaultValue(AutoCompleteMode.None)]
        [Description("ComboBoxAutoCompleteModeDescr")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public AutoCompleteMode AutoCompleteMode
        {
            get => this.ComboBox.AutoCompleteMode;
            set => this.ComboBox.AutoCompleteMode = value;
        }

        [DefaultValue(AutoCompleteSource.None)]
        [Description("ComboBoxAutoCompleteSourceDescr")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public AutoCompleteSource AutoCompleteSource
        {
            get => this.ComboBox.AutoCompleteSource;
            set => this.ComboBox.AutoCompleteSource = value;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Image BackgroundImage
        {
            get => base.BackgroundImage;
            set => base.BackgroundImage = value;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ImageLayout BackgroundImageLayout
        {
            get => base.BackgroundImageLayout;
            set => base.BackgroundImageLayout = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ComboBox ComboBox => this.Control as ComboBox;

        protected override Size DefaultSize => new Size(100, 22);

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler DoubleClick
        {
            add => base.DoubleClick += value;
            remove => base.DoubleClick -= value;
        }

        [Category("CatBehavior")]
        [Description("ComboBoxOnDropDownDescr")]
        public event EventHandler DropDown
        {
            add => this.Events.AddHandler(ToolStripNikseComboBox.EventDropDown, (Delegate)value);
            remove => this.Events.RemoveHandler(ToolStripNikseComboBox.EventDropDown, (Delegate)value);
        }

        [Category("CatBehavior")]
        [Description("ComboBoxOnDropDownClosedDescr")]
        public event EventHandler DropDownClosed
        {
            add => this.Events.AddHandler(ToolStripNikseComboBox.EventDropDownClosed, (Delegate)value);
            remove => this.Events.RemoveHandler(ToolStripNikseComboBox.EventDropDownClosed, (Delegate)value);
        }

        [Category("CatBehavior")]
        [Description("ComboBoxDropDownStyleChangedDescr")]
        public event EventHandler DropDownStyleChanged
        {
            add => this.Events.AddHandler(ToolStripNikseComboBox.EventDropDownStyleChanged, (Delegate)value);
            remove => this.Events.RemoveHandler(ToolStripNikseComboBox.EventDropDownStyleChanged, (Delegate)value);
        }

        [Category("CatBehavior")]
        [Description("ComboBoxDropDownHeightDescr")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DefaultValue(106)]
        public int DropDownHeight
        {
            get => this.ComboBox.DropDownHeight;
            set => this.ComboBox.DropDownHeight = value;
        }

        [Category("CatAppearance")]
        [DefaultValue(ComboBoxStyle.DropDown)]
        [Description("ComboBoxStyleDescr")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public ComboBoxStyle DropDownStyle
        {
            get => this.ComboBox.DropDownStyle;
            set => this.ComboBox.DropDownStyle = value;
        }

        [Category("CatBehavior")]
        [Description("ComboBoxDropDownWidthDescr")]
        public int DropDownWidth
        {
            get => this.ComboBox.DropDownWidth;
            set => this.ComboBox.DropDownWidth = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("ComboBoxDroppedDownDescr")]
        public bool DroppedDown
        {
            get => this.ComboBox.DroppedDown;
            set => this.ComboBox.DroppedDown = value;
        }

        [Category("CatAppearance")]
        [DefaultValue(FlatStyle.Popup)]
        [Localizable(true)]
        [Description("ComboBoxFlatStyleDescr")]
        public FlatStyle FlatStyle
        {
            get => this.ComboBox.FlatStyle;
            set => this.ComboBox.FlatStyle = value;
        }

        [Category("CatBehavior")]
        [DefaultValue(true)]
        [Localizable(true)]
        [Description("ComboBoxIntegralHeightDescr")]
        public bool IntegralHeight
        {
            get => this.ComboBox.IntegralHeight;
            set => this.ComboBox.IntegralHeight = value;
        }

        [Category("CatData")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Localizable(true)]
        [Description("ComboBoxItemsDescr")]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public ComboBox.ObjectCollection Items => this.ComboBox.Items;

        [Category("CatBehavior")]
        [DefaultValue(8)]
        [Localizable(true)]
        [Description("ComboBoxMaxDropDownItemsDescr")]
        public int MaxDropDownItems
        {
            get => this.ComboBox.MaxDropDownItems;
            set => this.ComboBox.MaxDropDownItems = value;
        }

        [Category("CatBehavior")]
        [DefaultValue(0)]
        [Localizable(true)]
        [Description("ComboBoxMaxLengthDescr")]
        public int MaxLength
        {
            get => this.ComboBox.MaxLength;
            set => this.ComboBox.MaxLength = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("ComboBoxSelectedIndexDescr")]
        public int SelectedIndex
        {
            get => this.ComboBox.SelectedIndex;
            set => this.ComboBox.SelectedIndex = value;
        }

        [Category("CatBehavior")]
        [Description("selectedIndexChangedEventDescr")]
        public event EventHandler SelectedIndexChanged
        {
            add => this.Events.AddHandler(ToolStripNikseComboBox.EventSelectedIndexChanged, (Delegate)value);
            remove => this.Events.RemoveHandler(ToolStripNikseComboBox.EventSelectedIndexChanged, (Delegate)value);
        }

        [Browsable(false)]
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("ComboBoxSelectedItemDescr")]
        public object SelectedItem
        {
            get => this.ComboBox.SelectedItem;
            set => this.ComboBox.SelectedItem = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("ComboBoxSelectedTextDescr")]
        public string SelectedText
        {
            get => this.ComboBox.SelectedText;
            set => this.ComboBox.SelectedText = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("ComboBoxSelectionLengthDescr")]
        public int SelectionLength
        {
            get => this.ComboBox.SelectionLength;
            set => this.ComboBox.SelectionLength = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("ComboBoxSelectionStartDescr")]
        public int SelectionStart
        {
            get => this.ComboBox.SelectionStart;
            set => this.ComboBox.SelectionStart = value;
        }

        [Category("CatBehavior")]
        [DefaultValue(false)]
        [Description("ComboBoxSortedDescr")]
        public bool Sorted
        {
            get => this.ComboBox.Sorted;
            set => this.ComboBox.Sorted = value;
        }

        [Category("CatBehavior")]
        [Description("ComboBoxOnTextUpdateDescr")]
        public event EventHandler TextUpdate
        {
            add => this.Events.AddHandler(ToolStripNikseComboBox.EventTextUpdate, (Delegate)value);
            remove => this.Events.RemoveHandler(ToolStripNikseComboBox.EventTextUpdate, (Delegate)value);
        }

        public void BeginUpdate() => this.ComboBox.BeginUpdate();

        public void EndUpdate() => this.ComboBox.EndUpdate();

        public int FindString(string s) => this.ComboBox.FindString(s);

        public int FindString(string s, int startIndex) => this.ComboBox.FindString(s, startIndex);

        public int FindStringExact(string s) => this.ComboBox.FindStringExact(s);

        public int FindStringExact(string s, int startIndex) => this.ComboBox.FindStringExact(s, startIndex);

        public int GetItemHeight(int index) => this.ComboBox.GetItemHeight(index);

        public void Select(int start, int length) => this.ComboBox.Select(start, length);

        public void SelectAll() => this.ComboBox.SelectAll();

        public override Size GetPreferredSize(Size constrainingSize)
        {
            Size preferredSize = base.GetPreferredSize(constrainingSize);
            preferredSize.Width = Math.Max(preferredSize.Width, 75);
            return preferredSize;
        }

        private void HandleDropDown(object sender, EventArgs e) => this.OnDropDown(e);

        private void HandleDropDownClosed(object sender, EventArgs e) => this.OnDropDownClosed(e);

        //private void HandleDropDownStyleChanged(object sender, EventArgs e) => this.OnDropDownStyleChanged(e);

        //private void HandleSelectedIndexChanged(object sender, EventArgs e) => this.OnSelectedIndexChanged(e);

        //private void HandleSelectionChangeCommitted(object sender, EventArgs e) => this.OnSelectionChangeCommitted(e);

        //private void HandleTextUpdate(object sender, EventArgs e) => this.OnTextUpdate(e);

        protected virtual void OnDropDown(EventArgs e)
        {
            //if (this.ParentInternal != null)
            //{
            //    Application.ThreadContext.FromCurrent().RemoveMessageFilter((IMessageFilter)this.ParentInternal.RestoreFocusFilter);
            //    ToolStripManager.ModalMenuFilter.SuspendMenuMode();
            //}
            //this.RaiseEvent(ToolStripNikseComboBox.EventDropDown, e);
        }

        protected virtual void OnDropDownClosed(EventArgs e)
        {
            //if (this.ParentInternal != null)
            //{
            //    Application.ThreadContext.FromCurrent().RemoveMessageFilter((IMessageFilter)this.ParentInternal.RestoreFocusFilter);
            //    ToolStripManager.ModalMenuFilter.ResumeMenuMode();
            //}
            //this.RaiseEvent(ToolStripNikseComboBox.EventDropDownClosed, e);
        }

        //protected virtual void OnDropDownStyleChanged(EventArgs e) => this.RaiseEvent(ToolStripNikseComboBox.EventDropDownStyleChanged, e);

        //protected virtual void OnSelectedIndexChanged(EventArgs e) => this.RaiseEvent(ToolStripNikseComboBox.EventSelectedIndexChanged, e);

        //protected virtual void OnSelectionChangeCommitted(EventArgs e) => this.RaiseEvent(ToolStripNikseComboBox.EventSelectionChangeCommitted, e);

        //protected virtual void OnTextUpdate(EventArgs e) => this.RaiseEvent(ToolStripNikseComboBox.EventTextUpdate, e);

        protected override void OnSubscribeControlEvents(Control control)
        {
            if (control is ComboBox comboBox)
            {
                comboBox.DropDown += new EventHandler(this.HandleDropDown);
                comboBox.DropDownClosed += new EventHandler(this.HandleDropDownClosed);
                //comboBox.DropDownStyleChanged += new EventHandler(this.HandleDropDownStyleChanged);
                //comboBox.SelectedIndexChanged += new EventHandler(this.HandleSelectedIndexChanged);
                //comboBox.SelectionChangeCommitted += new EventHandler(this.HandleSelectionChangeCommitted);
                //comboBox.TextUpdate += new EventHandler(this.HandleTextUpdate);
            }
            base.OnSubscribeControlEvents(control);
        }

        protected override void OnUnsubscribeControlEvents(Control control)
        {
            if (control is ComboBox comboBox)
            {
                comboBox.DropDown -= new EventHandler(this.HandleDropDown);
                comboBox.DropDownClosed -= new EventHandler(this.HandleDropDownClosed);
                //comboBox.DropDownStyleChanged -= new EventHandler(this.HandleDropDownStyleChanged);
                //comboBox.SelectedIndexChanged -= new EventHandler(this.HandleSelectedIndexChanged);
                //comboBox.SelectionChangeCommitted -= new EventHandler(this.HandleSelectionChangeCommitted);
                //comboBox.TextUpdate -= new EventHandler(this.HandleTextUpdate);
            }
            base.OnUnsubscribeControlEvents(control);
        }

        //private bool ShouldSerializeDropDownWidth() => this.ComboBox.ShouldSerializeDropDownWidth();

        //internal override bool ShouldSerializeFont() => !object.Equals((object)this.Font, (object)ToolStripManager.DefaultFont);

        public override string ToString() => base.ToString() + ", Items.Count: " + this.Items.Count.ToString((IFormatProvider)CultureInfo.CurrentCulture);

        [ComVisible(true)]
        internal class ToolStripNikseComboBoxAccessibleObject : ToolStripItem.ToolStripItemAccessibleObject
        {
            private ToolStripNikseComboBox ownerItem;

            public ToolStripNikseComboBoxAccessibleObject(ToolStripNikseComboBox ownerItem)
              : base((ToolStripItem)ownerItem)
            {
                this.ownerItem = ownerItem;
            }

            public override string DefaultAction => string.Empty;

            public override void DoDefaultAction()
            {
            }

            //public override AccessibleRole Role
            //{
            //    get
            //    {
            //        AccessibleRole accessibleRole = this.Owner.AccessibleRole;
            //        return accessibleRole != AccessibleRole.Default ? accessibleRole : AccessibleRole.ComboBox;
            //    }
            //}

            //internal override UnsafeNativeMethods.IRawElementProviderFragment FragmentNavigate(
            //  UnsafeNativeMethods.NavigateDirection direction)
            //{
            //    return direction == UnsafeNativeMethods.NavigateDirection.FirstChild || direction == UnsafeNativeMethods.NavigateDirection.LastChild ? (UnsafeNativeMethods.IRawElementProviderFragment)this.ownerItem.ComboBox.AccessibilityObject : base.FragmentNavigate(direction);
            //}

            //internal override UnsafeNativeMethods.IRawElementProviderFragmentRoot FragmentRoot => (UnsafeNativeMethods.IRawElementProviderFragmentRoot)this.ownerItem.RootToolStrip.AccessibilityObject;
        }

        internal class ToolStripNikseComboBoxControl : NikseComboBox
        {
            private ToolStripNikseComboBox owner;

            public ToolStripNikseComboBoxControl()
            {
              //  this.FlatStyle = FlatStyle.Popup;
                //this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            }

            public ToolStripNikseComboBox Owner
            {
                get => this.owner;
                set => this.owner = value;
            }

            //private ProfessionalColorTable ColorTable => this.Owner != null && this.Owner.Renderer is ToolStripProfessionalRenderer renderer ? renderer.ColorTable : ProfessionalColors.ColorTable;

           // protected override AccessibleObject CreateAccessibilityInstance() => AccessibilityImprovements.Level3 ? (AccessibleObject)new ToolStripNikseComboBox.ToolStripNikseComboBoxControl.ToolStripNikseComboBoxControlAccessibleObject(this) : base.CreateAccessibilityInstance();

            //internal override ComboBox.FlatComboAdapter CreateFlatComboAdapterInstance() => (ComboBox.FlatComboAdapter)new ToolStripNikseComboBox.ToolStripNikseComboBoxControl.ToolStripNikseComboBoxFlatComboAdapter((ComboBox)this);

            //protected override bool IsInputKey(Keys keyData)
            //{
            //    if ((keyData & Keys.Alt) == Keys.Alt)
            //    {
            //        if (AccessibilityImprovements.Level5)
            //        {
            //            switch (keyData & Keys.KeyCode)
            //            {
            //                case Keys.Up:
            //                case Keys.Down:
            //                    return true;
            //            }
            //        }
            //        else if ((keyData & Keys.Down) == Keys.Down || (keyData & Keys.Up) == Keys.Up)
            //            return true;
            //    }
            //    return base.IsInputKey(keyData);
            //}

            //protected override void OnDropDownClosed(EventArgs e)
            //{
            //    base.OnDropDownClosed(e);
            //    this.Invalidate();
            //    this.Update();
            //}

//            internal override bool SupportsUiaProviders => AccessibilityImprovements.Level3;

            //internal class ToolStripNikseComboBoxFlatComboAdapter : ComboBox.FlatComboAdapter
            //{
            //    public ToolStripNikseComboBoxFlatComboAdapter(ComboBox comboBox)
            //      : base(comboBox, true)
            //    {
            //    }

            //    private static bool UseBaseAdapter(ComboBox comboBox) => !(comboBox is ToolStripNikseComboBox.ToolStripNikseComboBoxControl stripComboBoxControl) || !(stripComboBoxControl.Owner.Renderer is ToolStripProfessionalRenderer);

            //    private static ProfessionalColorTable GetColorTable(
            //      ToolStripNikseComboBox.ToolStripNikseComboBoxControl ToolStripNikseComboBoxControl)
            //    {
            //        return ToolStripNikseComboBoxControl != null ? ToolStripNikseComboBoxControl.ColorTable : ProfessionalColors.ColorTable;
            //    }

            //    protected override Color GetOuterBorderColor(ComboBox comboBox)
            //    {
            //        if (ToolStripNikseComboBox.ToolStripNikseComboBoxControl.ToolStripNikseComboBoxFlatComboAdapter.UseBaseAdapter(comboBox))
            //            return base.GetOuterBorderColor(comboBox);
            //        return !comboBox.Enabled ? ToolStripNikseComboBox.ToolStripNikseComboBoxControl.ToolStripNikseComboBoxFlatComboAdapter.GetColorTable(comboBox as ToolStripNikseComboBox.ToolStripNikseComboBoxControl).ComboBoxBorder : SystemColors.Window;
            //    }

            //    protected override Color GetPopupOuterBorderColor(ComboBox comboBox, bool focused)
            //    {
            //        if (ToolStripNikseComboBox.ToolStripNikseComboBoxControl.ToolStripNikseComboBoxFlatComboAdapter.UseBaseAdapter(comboBox))
            //            return base.GetPopupOuterBorderColor(comboBox, focused);
            //        if (!comboBox.Enabled)
            //            return SystemColors.ControlDark;
            //        return !focused ? SystemColors.Window : ToolStripNikseComboBox.ToolStripNikseComboBoxControl.ToolStripNikseComboBoxFlatComboAdapter.GetColorTable(comboBox as ToolStripNikseComboBox.ToolStripNikseComboBoxControl).ComboBoxBorder;
            //    }

            //    protected override void DrawFlatComboDropDown(
            //      ComboBox comboBox,
            //      Graphics g,
            //      Rectangle dropDownRect)
            //    {
            //        if (ToolStripNikseComboBox.ToolStripNikseComboBoxControl.ToolStripNikseComboBoxFlatComboAdapter.UseBaseAdapter(comboBox))
            //        {
            //            base.DrawFlatComboDropDown(comboBox, g, dropDownRect);
            //        }
            //        else
            //        {
            //            if (!comboBox.Enabled || !ToolStripManager.VisualStylesEnabled)
            //            {
            //                g.FillRectangle(SystemBrushes.Control, dropDownRect);
            //            }
            //            else
            //            {
            //                ToolStripNikseComboBox.ToolStripNikseComboBoxControl ToolStripNikseComboBoxControl = comboBox as ToolStripNikseComboBox.ToolStripNikseComboBoxControl;
            //                ProfessionalColorTable colorTable = ToolStripNikseComboBox.ToolStripNikseComboBoxControl.ToolStripNikseComboBoxFlatComboAdapter.GetColorTable(ToolStripNikseComboBoxControl);
            //                if (!comboBox.DroppedDown)
            //                {
            //                    if (comboBox.ContainsFocus || comboBox.MouseIsOver)
            //                    {
            //                        using (Brush brush = (Brush)new LinearGradientBrush(dropDownRect, colorTable.ComboBoxButtonSelectedGradientBegin, colorTable.ComboBoxButtonSelectedGradientEnd, LinearGradientMode.Vertical))
            //                            g.FillRectangle(brush, dropDownRect);
            //                    }
            //                    else if (ToolStripNikseComboBoxControl.Owner.IsOnOverflow)
            //                    {
            //                        using (Brush brush = (Brush)new SolidBrush(colorTable.ComboBoxButtonOnOverflow))
            //                            g.FillRectangle(brush, dropDownRect);
            //                    }
            //                    else
            //                    {
            //                        using (Brush brush = (Brush)new LinearGradientBrush(dropDownRect, colorTable.ComboBoxButtonGradientBegin, colorTable.ComboBoxButtonGradientEnd, LinearGradientMode.Vertical))
            //                            g.FillRectangle(brush, dropDownRect);
            //                    }
            //                }
            //                else
            //                {
            //                    using (Brush brush = (Brush)new LinearGradientBrush(dropDownRect, colorTable.ComboBoxButtonPressedGradientBegin, colorTable.ComboBoxButtonPressedGradientEnd, LinearGradientMode.Vertical))
            //                        g.FillRectangle(brush, dropDownRect);
            //                }
            //            }
            //            Brush brush1 = !comboBox.Enabled ? SystemBrushes.GrayText : (!AccessibilityImprovements.Level2 || !SystemInformation.HighContrast || !comboBox.ContainsFocus && !comboBox.MouseIsOver || !ToolStripManager.VisualStylesEnabled ? SystemBrushes.ControlText : SystemBrushes.HighlightText);
            //            Point point = new Point(dropDownRect.Left + dropDownRect.Width / 2, dropDownRect.Top + dropDownRect.Height / 2);
            //            point.X += dropDownRect.Width % 2;
            //            g.FillPolygon(brush1, new Point[3]
            //            {
            //  new Point(point.X - ComboBox.FlatComboAdapter.Offset2Pixels, point.Y - 1),
            //  new Point(point.X + ComboBox.FlatComboAdapter.Offset2Pixels + 1, point.Y - 1),
            //  new Point(point.X, point.Y + ComboBox.FlatComboAdapter.Offset2Pixels)
            //            });
            //        }
            //    }
            //}

            //internal class ToolStripNikseComboBoxControlAccessibleObject : ComboBox.ComboBoxUiaProvider
            //{
            //    private ComboBox.ChildAccessibleObject childAccessibleObject;

            //    public ToolStripNikseComboBoxControlAccessibleObject(
            //      ToolStripNikseComboBox.ToolStripNikseComboBoxControl ToolStripNikseComboBoxControl)
            //      : base((ComboBox)ToolStripNikseComboBoxControl)
            //    {
            //        this.childAccessibleObject = new ComboBox.ChildAccessibleObject((ComboBox)ToolStripNikseComboBoxControl, ToolStripNikseComboBoxControl.Handle);
            //    }

            //    internal override UnsafeNativeMethods.IRawElementProviderFragment FragmentNavigate(
            //      UnsafeNativeMethods.NavigateDirection direction)
            //    {
            //        switch (direction)
            //        {
            //            case UnsafeNativeMethods.NavigateDirection.Parent:
            //            case UnsafeNativeMethods.NavigateDirection.NextSibling:
            //            case UnsafeNativeMethods.NavigateDirection.PreviousSibling:
            //                if (this.Owner is ToolStripNikseComboBox.ToolStripNikseComboBoxControl owner)
            //                    return owner.Owner.AccessibilityObject.FragmentNavigate(direction);
            //                break;
            //        }
            //        return base.FragmentNavigate(direction);
            //    }

            //    internal override UnsafeNativeMethods.IRawElementProviderFragmentRoot FragmentRoot => this.Owner is ToolStripNikseComboBox.ToolStripNikseComboBoxControl owner ? (UnsafeNativeMethods.IRawElementProviderFragmentRoot)owner.Owner.Owner.AccessibilityObject : base.FragmentRoot;

            //    internal override object GetPropertyValue(int propertyID)
            //    {
            //        if (propertyID == 30003)
            //            return (object)50003;
            //        return propertyID == 30022 ? (object)((this.State & AccessibleStates.Offscreen) == AccessibleStates.Offscreen) : base.GetPropertyValue(propertyID);
            //    }

            //    internal override bool IsPatternSupported(int patternId) => patternId == 10005 || patternId == 10002 || base.IsPatternSupported(patternId);
            //}
        }
    }
}

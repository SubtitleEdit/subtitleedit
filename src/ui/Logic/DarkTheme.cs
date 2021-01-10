using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public static class DarkTheme
    {
        internal static readonly Color BackColor = Configuration.Settings.General.DarkThemeBackColor;
        internal static readonly Color ForeColor = Configuration.Settings.General.DarkThemeForeColor;

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return NativeMethods.DwmSetWindowAttribute(handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Configuration.IsRunningOnWindows && Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

        private static void SetWindowThemeDark(Control control)
        {
            if (Configuration.IsRunningOnWindows)
            {
                NativeMethods.SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
            }
        }

        public static IEnumerable<Control> GetAllControlByType(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>().ToList();

            return controls.SelectMany(ctrl => GetAllControlByType(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        private static List<T> GetSubControls<T>(Control c)
        {
            var type = c.GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var contextMenus = fields.Where(f => f.GetValue(c) != null &&
            (f.GetValue(c).GetType().IsSubclassOf(typeof(T)) || f.GetValue(c).GetType() == typeof(T)));
            var menus = contextMenus.Select(f => f.GetValue(c));
            return menus.Cast<T>().ToList();
        }

        public static void SetDarkTheme(Control ctrl, int iterations = 5)
        {
            if (iterations < 1)
            {
                // note: no need to restore the colors set are constants
                return;
            }

            if (ctrl is Form form)
            {
                UseImmersiveDarkMode(ctrl.Handle, true);

                var contextMenus = GetSubControls<ContextMenuStrip>(form);
                foreach (ContextMenuStrip cms in contextMenus)
                {
                    cms.BackColor = BackColor;
                    cms.ForeColor = ForeColor;
                    cms.Renderer = new MyRenderer();
                    foreach (Control inner in cms.Controls)
                    {
                        SetDarkTheme(inner, iterations - 1);
                    }
                }

                var toolStrips = GetSubControls<ToolStrip>(form);
                foreach (ToolStrip c in toolStrips)
                {
                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                    c.Renderer = new MyRenderer();
                }

                var toolStripComboBox = GetSubControls<ToolStripComboBox>(form);
                foreach (ToolStripComboBox c in toolStripComboBox)
                {
                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                    c.FlatStyle = FlatStyle.Flat;
                }

                var toolStripContentPanels = GetSubControls<ToolStripContentPanel>(form);
                foreach (ToolStripContentPanel c in toolStripContentPanels)
                {
                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                }

                var toolStripContainers = GetSubControls<ToolStripContainer>(form);
                foreach (ToolStripContainer c in toolStripContainers)
                {
                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                }

                var toolStripDropDownMenus = GetSubControls<ToolStripDropDownMenu>(form);
                foreach (ToolStripDropDownMenu c in toolStripDropDownMenus)
                {
                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                    foreach (ToolStripItem x in c.Items)
                    {
                        x.BackColor = BackColor;
                        x.ForeColor = ForeColor;
                    }
                }

                var toolStripMenuItems = GetSubControls<ToolStripMenuItem>(form);
                foreach (ToolStripMenuItem c in toolStripMenuItems)
                {
                    if (c.GetCurrentParent() is ToolStripDropDownMenu p)
                    {
                        p.BackColor = BackColor;
                        p.Renderer = new MyRenderer();
                    }

                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                }

                var toolStripSeparators = GetSubControls<ToolStripSeparator>(form);
                foreach (ToolStripSeparator c in toolStripSeparators)
                {
                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                }
            }

            FixControl(ctrl);
            foreach (Control c in GetSubControls<Control>(ctrl))
            {
                FixControl(c);
            }
        }

        private static void FixControl(Control c)
        {
            c.BackColor = BackColor;
            c.ForeColor = ForeColor;

            if (c is Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.EnabledChanged += Button_EnabledChanged;
                b.Paint += Button_Paint;
            }

            if (c is CheckBox cb)
            {
                cb.Paint += CheckBox_Paint;
            }

            if (c is RadioButton rb)
            {
                rb.Paint += RadioButton_Paint;
            }

            if (c is ComboBox cmBox)
            {
                cmBox.FlatStyle = FlatStyle.Flat;
            }

            if (c is NumericUpDown numeric)
            {
                numeric.BorderStyle = BorderStyle.FixedSingle;
            }

            if (c is Panel p)
            {
                if (p.BorderStyle != BorderStyle.None)
                {
                    p.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            if (c is ContextMenuStrip cms)
            {
                cms.Renderer = new MyRenderer();
            }

            if (c is LinkLabel linkLabel)
            {
                var linkColor = Color.FromArgb(0, 120, 215);
                linkLabel.ActiveLinkColor = linkColor;
                linkLabel.LinkColor = linkColor;
                linkLabel.VisitedLinkColor = linkColor;
                linkLabel.DisabledLinkColor = Color.FromArgb(0, 70, 170);
            }

            if (c is ToolStripDropDownMenu t)
            {
                foreach (var x in t.Items)
                {
                    if (x is ToolStripMenuItem item)
                    {
                        item.BackColor = BackColor;
                        item.ForeColor = ForeColor;
                    }
                }
            }

            if (c is TreeView || c is ListBox)
            {
                SetWindowThemeDark(c);
            }

            if (c is TabControl tc)
            {
                tc.DrawMode = TabDrawMode.OwnerDrawFixed;
                tc.DrawItem += TabControl1_DrawItem;
                foreach (TabPage tabPage in tc.TabPages)
                {
                    tabPage.Paint += TabPage_Paint;
                }
            }

            if (c is SubtitleListView seLV)
            {
                seLV.OwnerDraw = true;
                seLV.DrawColumnHeader += ListView_DrawColumnHeader;
                seLV.GridLines = Configuration.Settings.General.DarkThemeShowListViewGridLines;
                seLV.HandleCreated += ListView_HandleCreated;
                SetWindowThemeDark(seLV);
            }
            else if (c is ListView lv)
            {
                lv.GridLines = false;
                lv.OwnerDraw = true;
                lv.DrawItem += ListView_DrawItem;
                lv.DrawSubItem += ListView_DrawSubItem;
                lv.DrawColumnHeader += ListView_DrawColumnHeader;
                lv.HandleCreated += ListView_HandleCreated;
            }
        }

        private static void Button_EnabledChanged(object sender, EventArgs e)
        {
            var button = (Button)sender;
            button.ForeColor = button.Enabled ? ForeColor : Color.DimGray;
        }

        private static void Button_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Button button && !button.Enabled)
            {
                button.ForeColor = Color.DimGray;
                TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                TextRenderer.DrawText(e.Graphics, button.Text, button.Font, e.ClipRectangle, button.ForeColor, flags);
            }
        }

        private static void CheckBox_Paint(object sender, PaintEventArgs e)
        {
            if (sender is CheckBox checkBox && !checkBox.Enabled)
            {
                var checkBoxWidth = CheckBoxRenderer.GetGlyphSize(e.Graphics, System.Windows.Forms.VisualStyles.CheckBoxState.CheckedDisabled).Width;
                Rectangle textRectangleValue = new Rectangle
                {
                    X = e.ClipRectangle.X + checkBoxWidth,
                    Y = e.ClipRectangle.Y,
                    Width = e.ClipRectangle.X + e.ClipRectangle.Width - checkBoxWidth,
                    Height = e.ClipRectangle.Height
                };

                TextRenderer.DrawText(e.Graphics, checkBox.Text, checkBox.Font, textRectangleValue, Color.DimGray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private static void RadioButton_Paint(object sender, PaintEventArgs e)
        {
            if (sender is RadioButton radioButton && !radioButton.Enabled)
            {
                var radioButtonWidth = RadioButtonRenderer.GetGlyphSize(e.Graphics, System.Windows.Forms.VisualStyles.RadioButtonState.UncheckedDisabled).Width;
                Rectangle textRectangleValue = new Rectangle
                {
                    X = e.ClipRectangle.X + radioButtonWidth,
                    Y = e.ClipRectangle.Y,
                    Width = e.ClipRectangle.X + e.ClipRectangle.Width - radioButtonWidth,
                    Height = e.ClipRectangle.Height
                };

                TextRenderer.DrawText(e.Graphics, radioButton.Text, radioButton.Font, textRectangleValue, Color.DimGray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private static void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            var sz = e.Graphics.MeasureString((sender as TabControl)?.TabPages[e.Index].Text, e.Font);
            using (var br = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(br, e.Bounds);
                e.Graphics.DrawString((sender as TabControl)?.TabPages[e.Index].Text, e.Font, Brushes.WhiteSmoke, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 1);

                var rect = e.Bounds;
                rect.Offset(0, 1);
                rect.Inflate(0, -1);
                e.Graphics.DrawRectangle(new Pen(ForeColor, 1), rect);
                e.DrawFocusRectangle();
            }
        }

        private static void TabPage_Paint(object sender, PaintEventArgs e)
        {
            using (var fillBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(fillBrush, e.ClipRectangle);
            }
        }

        private static void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var lv = sender as ListView;
            if (!lv.Focused && (e.State & ListViewItemStates.Selected) != 0)
            {
                if (e.Item.Focused)
                {
                    e.DrawFocusRectangle();
                }
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private static void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var lv = sender as ListView;
            Color backgroundColor = lv.Items[e.ItemIndex].SubItems[e.ColumnIndex].BackColor;
            Color subBackgroundColor = Color.FromArgb(backgroundColor.A, Math.Max(backgroundColor.R - 39, 0), Math.Max(backgroundColor.G - 39, 0), Math.Max(backgroundColor.B - 39, 0));
            if (lv.Focused && backgroundColor == BackColor || lv.RightToLeftLayout)
            {
                e.DrawDefault = true;
                return;
            }

            using (var sf = new StringFormat())
            {
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                }

                if (e.Item.Selected)
                {
                    var subtitleFont = e.Item.Font;
                    Rectangle rect = e.Bounds;
                    if (Configuration.Settings != null)
                    {
                        backgroundColor = backgroundColor == BackColor ? Configuration.Settings.Tools.ListViewUnfocusedSelectedColor : subBackgroundColor;
                        using (var sb = new SolidBrush(backgroundColor))
                        {
                            e.Graphics.FillRectangle(sb, rect);
                        }
                    }
                    else
                    {
                        e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                    }

                    int addX = 0;

                    if (e.ColumnIndex == 0 && lv.CheckBoxes)
                    {
                        addX = 16;
                        var checkBoxState = e.Item.Checked ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
                        CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(e.Bounds.X + 4, e.Bounds.Y + 2), checkBoxState);
                    }

                    if (lv.Columns[e.ColumnIndex].TextAlign == HorizontalAlignment.Right)
                    {
                        var stringWidth = (int)e.Graphics.MeasureString(e.Item.SubItems[e.ColumnIndex].Text, subtitleFont).Width;
                        TextRenderer.DrawText(e.Graphics, e.Item.SubItems[e.ColumnIndex].Text, subtitleFont, new Point(e.Bounds.Right - stringWidth - 7, e.Bounds.Top + 2), e.Item.ForeColor, TextFormatFlags.NoPrefix);
                    }
                    else
                    {
                        TextRenderer.DrawText(e.Graphics, e.Item.SubItems[e.ColumnIndex].Text, subtitleFont, new Point(e.Bounds.Left + 3 + addX, e.Bounds.Top + 2), e.Item.ForeColor, TextFormatFlags.NoPrefix);
                    }
                }
                else
                {
                    e.DrawDefault = true;
                }
            }
        }

        private static void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            if (sender is ListView lv && lv.RightToLeftLayout)
            {
                // Until we find a solution for drawing it in RTL
                return;
            }

            int addY = 0;
            if (e.Font.Name != Configuration.Settings.General.SubtitleFontName)
            {
                addY = 5;
            }

            e.DrawDefault = false;
            using (var b = new SolidBrush(Color.FromArgb(Math.Max(BackColor.R - 9, 0), Math.Max(BackColor.G - 9, 0), Math.Max(BackColor.B - 9, 0))))
            {
                e.Graphics.FillRectangle(b, e.Bounds);
            }

            var strFormat = new StringFormat();
            switch (e.Header.TextAlign)
            {
                case HorizontalAlignment.Center:
                    strFormat.Alignment = StringAlignment.Center;
                    break;
                case HorizontalAlignment.Right:
                    strFormat.Alignment = StringAlignment.Far;
                    break;
            }

            using (var fc = new SolidBrush(ForeColor))
            {
                e.Graphics.DrawString(e.Header.Text, e.Font, fc, e.Bounds.X + 3, e.Bounds.Y + addY, strFormat);
                if (e.ColumnIndex != 0)
                {
                    e.Graphics.DrawLine(new Pen(ForeColor), e.Bounds.X, e.Bounds.Y, e.Bounds.X, e.Bounds.Height);
                }
            }
        }

        private static void ListView_HandleCreated(object sender, EventArgs e)
        {
            SetWindowThemeDark((Control)sender);
        }

        private class MyRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (var brush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(brush, e.ConnectedArea);
                }
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                if (!(e.ToolStrip is ToolStrip))
                {
                    base.OnRenderToolStripBorder(e);
                }
            }

            protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
            {
                var g = e.Graphics;
                var rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
                using (var p = new Pen(Color.FromArgb(81, 81, 81)))
                {
                    g.DrawRectangle(p, rect);
                }
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
            {
                var g = e.Graphics;
                var rect = new Rectangle(e.ImageRectangle.Left - 2, e.ImageRectangle.Top - 2,
                    e.ImageRectangle.Width + 4, e.ImageRectangle.Height + 4);

                using (var b = new SolidBrush(Color.FromArgb(81, 81, 81)))
                {
                    g.FillRectangle(b, rect);
                }

                using (var p = new Pen(Color.FromArgb(104, 151, 187)))
                {
                    var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                    g.DrawRectangle(p, modRect);
                }

                if (e.Item.ImageIndex == -1 && string.IsNullOrEmpty(e.Item.ImageKey) && e.Item.Image == null)
                {
                    g.DrawImageUnscaled(Properties.Resources.tick, new Point(e.ImageRectangle.Left, e.ImageRectangle.Top));
                }
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var g = e.Graphics;

                e.Item.ForeColor = e.Item.Enabled ? Color.FromArgb(220, 220, 220) : Color.FromArgb(153, 153, 153);

                if (e.Item.Enabled)
                {

                    var bgColor = e.Item.Selected ? Color.FromArgb(122, 128, 132) : e.Item.BackColor;

                    // Normal item
                    var rect = new Rectangle(2, 0, e.Item.Width - 3, e.Item.Height);

                    using (var b = new SolidBrush(bgColor))
                    {
                        g.FillRectangle(b, rect);
                    }

                    // Header item on open menu
                    if (e.Item.GetType() == typeof(ToolStripMenuItem))
                    {
                        if (((ToolStripMenuItem)e.Item).DropDown.Visible && e.Item.IsOnDropDown == false)
                        {
                            using (var b = new SolidBrush(Color.FromArgb(92, 92, 92)))
                            {
                                g.FillRectangle(b, rect);
                            }
                        }
                    }
                }
            }
        }

        internal static void SetDarkTheme(ToolStripItem item)
        {
            item.BackColor = BackColor;
            item.ForeColor = ForeColor;
            if (item is ToolStripSeparator)
            {
                item.Paint += ToolStripSeparatorPaint;
            }
        }

        private static void ToolStripSeparatorPaint(object sender, PaintEventArgs e)
        {
            var toolStripSeparator = (ToolStripSeparator)sender;
            var width = toolStripSeparator.Width;
            var height = toolStripSeparator.Height;
            e.Graphics.FillRectangle(new SolidBrush(BackColor), 0, 0, width, height);
            e.Graphics.DrawLine(new Pen(ForeColor), 4, height / 2, width - 4, height / 2);
        }
    }
}

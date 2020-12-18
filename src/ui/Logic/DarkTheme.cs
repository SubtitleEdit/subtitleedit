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
        public static IEnumerable<Control> GetAllControlByType(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>().ToList();

            return controls.SelectMany(ctrl => GetAllControlByType(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        private static void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            var sz = e.Graphics.MeasureString((sender as TabControl)?.TabPages[e.Index].Text, e.Font);
            using (var br = new SolidBrush(Configuration.Settings.General.DarkThemeBackColor))
            {
                e.Graphics.FillRectangle(br, e.Bounds);
                e.Graphics.DrawString((sender as TabControl)?.TabPages[e.Index].Text, e.Font, Brushes.WhiteSmoke, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 1);

                var rect = e.Bounds;
                rect.Offset(0, 1);
                rect.Inflate(0, -1);
                e.Graphics.DrawRectangle(new Pen(Configuration.Settings.General.DarkThemeForeColor, 1), rect);
                e.DrawFocusRectangle();
            }
        }

        private static void TabPage_Paint(object sender, PaintEventArgs e)
        {
            using (var fillBrush = new SolidBrush(Configuration.Settings.General.DarkThemeBackColor))
            {
                e.Graphics.FillRectangle(fillBrush, e.ClipRectangle);
            }
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

            if (ctrl is Form form) // https://www.dreamincode.net/forums/topic/64981-designing-a-custom-title-bar/
            {
                //form.FormBorderStyle = FormBorderStyle.None;
                //var title = new PictureBox
                //{
                //    Location = new Point(0, 0),
                //    Width = form.Width,
                //    Height = 50,
                //    BackColor = Color.Black,
                //    Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left
                //};
                //form.Controls.Add(title);
                //title.SendToBack();
                //title.MouseDown += (o, i) => { form.WindowState = FormWindowState.Minimized; };
                //title.MouseUp += (o, i) => { form.WindowState = FormWindowState.Maximized; };
                //title.MouseMove += (o, i) => { form.WindowState = FormWindowState.Normal; };

                //var minimize = new Label
                //{
                //    Text = "🗕",
                //    Location = new Point(form.Width - 60, 5),
                //    ForeColor = ForeColor,
                //    BackColor = Color.Black, // BackColor,
                //    Width =  15,
                //    Height = 15,
                //    Anchor = AnchorStyles.Top | AnchorStyles.Right
                //};
                //minimize.Click += (o, i) => { form.WindowState = FormWindowState.Minimized; };
                //form.Controls.Add(minimize);
                //minimize.BringToFront();

                //var maximize = new Label
                //{
                //    Text = "🗗", //🗗 Overlap or 🗖 maximize
                //    Location = new Point(form.Width - 40, 5),
                //    ForeColor = ForeColor,
                //    BackColor = Color.Black, // BackColor,
                //    Width = 15,
                //    Height = 15,
                //    Anchor = AnchorStyles.Top | AnchorStyles.Right
                //};

                //maximize.Click += (o, i) =>
                //{
                //    form.WindowState = form.WindowState == FormWindowState.Maximized ?  FormWindowState.Normal : FormWindowState.Maximized;
                //    maximize.Text = form.WindowState == FormWindowState.Maximized ? "🗗" : "🗖";
                //};
                //form.Controls.Add(maximize);
                //maximize.BringToFront();

                //var close = new Label
                //{
                //    Text = "🗙",
                //    Location = new Point(form.Width - 20, 5),
                //    ForeColor = ForeColor,
                //    BackColor = Color.Black, // BackColor,
                //    Width = 15,
                //    Height = 15,
                //    Anchor = AnchorStyles.Top | AnchorStyles.Right
                //};
                //close.Click += (o, i) => { form.Close(); };
                //form.Controls.Add(close);
                //close.BringToFront();

                var contextMenus = GetSubControls<ContextMenuStrip>(form);
                foreach (ContextMenuStrip cms in contextMenus)
                {
                    cms.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                    cms.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
                    cms.Renderer = new MyRenderer();
                    cms.ShowImageMargin = false;
                    cms.ShowCheckMargin = false;
                    foreach (Control inner in cms.Controls)
                    {
                        SetDarkTheme(inner, iterations - 1);
                    }
                }

                var toolStrips = GetSubControls<ToolStrip>(form);
                foreach (ToolStrip c in toolStrips)
                {
                    c.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                    c.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
                }

                var toolStripComboBox = GetSubControls<ToolStripComboBox>(form);
                foreach (ToolStripComboBox c in toolStripComboBox)
                {
                    c.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                    c.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
                    c.FlatStyle = FlatStyle.Flat;
                }

                var toolStripContentPanels = GetSubControls<ToolStripContentPanel>(form);
                foreach (ToolStripContentPanel c in toolStripContentPanels)
                {
                    c.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                    c.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
                }

                var toolStripContainers = GetSubControls<ToolStripContainer>(form);
                foreach (ToolStripContainer c in toolStripContainers)
                {
                    c.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                    c.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
                }

                var toolStripDropDownMenus = GetSubControls<ToolStripDropDownMenu>(form);
                foreach (ToolStripDropDownMenu c in toolStripDropDownMenus)
                {
                    c.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                    c.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
                    foreach (ToolStripItem x in c.Items)
                    {
                        x.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                        x.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
                    }
                }

                var toolStripMenuItems = GetSubControls<ToolStripMenuItem>(form);
                foreach (ToolStripMenuItem c in toolStripMenuItems)
                {
                    c.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                    c.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
                }

                var toolStripSeparators = GetSubControls<ToolStripSeparator>(form);
                foreach (ToolStripSeparator c in toolStripSeparators)
                {
                    if (c.GetCurrentParent() is ToolStripDropDownMenu p)
                    {
                        p.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                        p.ShowCheckMargin = false;
                        p.ShowImageMargin = false;
                    }

                    c.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                    c.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
                }
            }

            FixControl(ctrl);
            foreach (Control c in GetSubControls<Control>(ctrl))
            {
                if (c is TabControl tc)
                {
                    tc.DrawMode = TabDrawMode.OwnerDrawFixed;
                    tc.DrawItem += TabControl1_DrawItem;
                    foreach (TabPage tabPage in tc.TabPages)
                    {
                        tabPage.Paint += TabPage_Paint;
                    }
                }

                FixControl(c);
            }
        }

        private static void FixControl(Control c)
        {
            c.BackColor = Configuration.Settings.General.DarkThemeBackColor;
            c.ForeColor = Configuration.Settings.General.DarkThemeForeColor;

            if (c is Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
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
                p.BorderStyle = BorderStyle.FixedSingle;
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
                        item.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                        item.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
                    }
                }
            }

            if (c is SubtitleListView lv)
            {
                lv.OwnerDraw = true;
                lv.DrawColumnHeader += lv_DrawColumnHeader;
                lv.GridLines = Configuration.Settings.General.DarkThemeShowListViewGridLines;
                lv.BackColor = Configuration.Settings.General.DarkThemeBackColor;
                lv.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
            }
        }

        private static void lv_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            var lv = (ListView)sender;
            lv.BackColor = lv.BackColor = Configuration.Settings.General.DarkThemeBackColor;
            lv.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
            e.DrawDefault = false;
            using (var b = new SolidBrush(Color.FromArgb(21, 21, 21)))
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

            using (var fc = new SolidBrush(Configuration.Settings.General.DarkThemeForeColor))
            {
                e.Graphics.DrawString(e.Header.Text, e.Font, fc, e.Bounds.X + 3, e.Bounds.Y, strFormat);
                if (e.ColumnIndex != 0)
                {
                    e.Graphics.DrawLine(new Pen(Configuration.Settings.General.DarkThemeForeColor), e.Bounds.X, e.Bounds.Y, e.Bounds.X, e.Bounds.Height);
                }
            }
        }

        private class MyRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (var brush = new SolidBrush(Configuration.Settings.General.DarkThemeBackColor))
                {
                    e.Graphics.FillRectangle(brush, e.ConnectedArea);
                }
            }
        }

        internal static void SetDarkTheme(ToolStripItem item)
        {
            item.BackColor = Configuration.Settings.General.DarkThemeBackColor;
            item.ForeColor = Configuration.Settings.General.DarkThemeForeColor;
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
            e.Graphics.FillRectangle(new SolidBrush(Configuration.Settings.General.DarkThemeBackColor), 0, 0, width, height);
            e.Graphics.DrawLine(new Pen(Configuration.Settings.General.DarkThemeForeColor), 4, height / 2, width - 4, height / 2);
        }
    }
}

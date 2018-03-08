using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core;
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
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAllControlByType(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        static Color BackColor = Color.FromArgb(52, 52, 45);
        static Color ForeColor = Color.FromArgb(150, 150, 150);

        private static void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            using (Brush br = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(br, e.Bounds);
                SizeF sz = e.Graphics.MeasureString((sender as TabControl).TabPages[e.Index].Text, e.Font);
                e.Graphics.DrawString((sender as TabControl).TabPages[e.Index].Text, e.Font, Brushes.WhiteSmoke, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 1);

                Rectangle rect = e.Bounds;
                rect.Offset(0, 1);
                rect.Inflate(0, -1);
                e.Graphics.DrawRectangle(new Pen(ForeColor, 1), rect);
                e.DrawFocusRectangle();
            }
        }

        private static void Tabpage_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush fillBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(fillBrush, e.ClipRectangle);
            }
        }

        private static List<T> GetSubControls<T>(Control c)
        {
            var type = c.GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var contextMenus = fields.Where(f => f != null && f.GetValue(c) != null &&
            (f.GetValue(c).GetType().IsSubclassOf(typeof(T)) || f.GetValue(c).GetType() == typeof(T)));
            var menus = contextMenus.Select(f => f.GetValue(c));
            return menus.Cast<T>().ToList();
        }

        private static bool _isConfigUpdated;

        public static void SetDarkTheme(Control ctrl, int iterations = 5)
        {
            if (!_isConfigUpdated)
            {
                Configuration.Settings.General.SubtitleBackgroundColor = BackColor;
                Configuration.Settings.General.SubtitleFontColor = ForeColor;
                Configuration.Settings.VideoControls.WaveformBackgroundColor = BackColor;
                Configuration.Settings.VideoControls.WaveformGridColor = Color.FromArgb(62, 62, 60);
                // prevent re assignings
                _isConfigUpdated = true;
            }

            if (iterations < 1)
            {
                // note: no need to restore the colors set are constants
                //_isConfigUpdated = false;
                return;
            }

            if (ctrl is Form)
            {
                var contextMenus = GetSubControls<ContextMenuStrip>(ctrl);
                foreach (ContextMenuStrip cms in contextMenus)
                {
                    cms.BackColor = BackColor;
                    cms.ForeColor = ForeColor;
                    cms.Renderer = new MyRenderer();
                    cms.ShowImageMargin = false;
                    cms.ShowCheckMargin = false;
                    foreach (Control inner in cms.Controls)
                    {
                        SetDarkTheme(inner, iterations - 1);
                    }
                }

                var toolStrips = GetSubControls<ToolStrip>(ctrl);
                foreach (ToolStrip c in toolStrips)
                {
                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                }

                var toolStripContentPanels = GetSubControls<ToolStripContentPanel>(ctrl);
                foreach (ToolStripContentPanel c in toolStripContentPanels)
                {
                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                }

                var toolStripContainers = GetSubControls<ToolStripContainer>(ctrl);
                foreach (ToolStripContainer c in toolStripContainers)
                {
                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                }

                var toolStripDropDownMenus = GetSubControls<ToolStripDropDownMenu>(ctrl);
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

                var toolStripMenuItems = GetSubControls<ToolStripMenuItem>(ctrl);
                foreach (ToolStripMenuItem c in toolStripMenuItems)
                {
                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                }

                var toolStripSeparators = GetSubControls<ToolStripSeparator>(ctrl);
                foreach (ToolStripSeparator c in toolStripSeparators)
                {
                    if (c.GetCurrentParent() is ToolStripDropDownMenu p)
                    {
                        p.BackColor = BackColor;
                        p.ShowCheckMargin = false;
                        p.ShowImageMargin = false;
                    }

                    c.BackColor = BackColor;
                    c.ForeColor = ForeColor;
                    //c.Paint += C_Paint;
                }
            }
            FixControl(ctrl);
            foreach (Control c in GetSubControls<Control>(ctrl)) // form.Controls)
            {
                if (c is TabControl tc)
                {
                    tc.DrawMode = TabDrawMode.OwnerDrawFixed;
                    tc.DrawItem += TabControl1_DrawItem;
                    foreach (TabPage tabPage in tc.TabPages)
                    {
                        tabPage.Paint += Tabpage_Paint;
                    }
                }
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
            }
            if (c is Panel p)
            {
                p.BorderStyle = BorderStyle.FixedSingle;
            }
            if (c is ContextMenuStrip cms)
            {
                cms.Renderer = new MyRenderer();
            }
            if (c is ToolStripDropDownMenu t)
            {
                foreach (var x in t.Items)
                {
                    if (x is ToolStripMenuItem)
                    {
                        (x as ToolStripMenuItem).BackColor = BackColor;
                        (x as ToolStripMenuItem).ForeColor = ForeColor;
                    }
                }
            }
            if (c is SubtitleListView lv)
            {
                lv.OwnerDraw = true;
                lv.DrawColumnHeader += lv_DrawColumnHeader;
                lv.GridLines = false;
                lv.ForeColor = ForeColor;
                lv.BackColor = BackColor;
            }
        }

        private static void lv_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            var lv = (ListView)sender;
            lv.BackColor = BackColor;
            lv.ForeColor = ForeColor;
            e.DrawDefault = false;
            using (var b = new SolidBrush(BackColor))
                e.Graphics.FillRectangle(b, e.Bounds);

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
                e.Graphics.DrawString(e.Header.Text, e.Font, fc, e.Bounds, strFormat);
        }

        private class MyRenderer : ToolStripProfessionalRenderer
        {
            //            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            //            {
            //                Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
            ////                Color c = e.Item.Selected ? Color.Azure : Color.Beige;
            //                using (SolidBrush brush = new SolidBrush(BackColor))
            //                    e.Graphics.FillRectangle(brush, rc);
            //            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (SolidBrush brush = new SolidBrush(BackColor))
                    e.Graphics.FillRectangle(brush, e.ConnectedArea);
            }
        }

        internal static void SetDarkTheme(ToolStripMenuItem item)
        {
            item.BackColor = BackColor;
            item.ForeColor = ForeColor;
        }

        internal static void SetDarkTheme(ToolStripSeparator item)
        {
            item.BackColor = BackColor;
            item.ForeColor = ForeColor;
        }

        internal static void SetDarkTheme(ToolStripItem item)
        {
            item.BackColor = BackColor;
            item.ForeColor = ForeColor;
        }
    }
}

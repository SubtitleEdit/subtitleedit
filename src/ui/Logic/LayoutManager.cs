using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Controls;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public static class LayoutManager
    {
        public const int LayoutNoVideo = 8;
        public static SplitContainer MainSplitContainer;

        public static void SetLayout(int layout, Form form, Control videoPlayer, SubtitleListView subtitleListView, GroupBox groupBoxWaveform, GroupBox groupBoxEdit, SplitterEventHandler splitMoved)
        {
            if (layout > LayoutNoVideo || layout < 0)
            {
                layout = 0;
            }

            switch (layout)
            {
                case 0:
                    SetLayout0(form, videoPlayer, subtitleListView, groupBoxWaveform, groupBoxEdit, splitMoved);
                    break;
                case 1:
                    SetLayout1(form, videoPlayer, subtitleListView, groupBoxWaveform, groupBoxEdit, splitMoved);
                    break;
                case 2:
                    SetLayout2(form, videoPlayer, subtitleListView, groupBoxWaveform, groupBoxEdit, splitMoved);
                    break;
                case 3:
                    SetLayout3(form, videoPlayer, subtitleListView, groupBoxWaveform, groupBoxEdit, splitMoved);
                    break;
                case 4:
                    SetLayout4(form, videoPlayer, subtitleListView, groupBoxWaveform, groupBoxEdit, splitMoved);
                    break;
                case 5:
                    SetLayout5(form, videoPlayer, subtitleListView, groupBoxWaveform, groupBoxEdit, splitMoved);
                    break;
                case 6:
                    SetLayout6(form, videoPlayer, subtitleListView, groupBoxWaveform, groupBoxEdit, splitMoved);
                    break;
                case 7:
                    SetLayout7(form, videoPlayer, subtitleListView, groupBoxWaveform, groupBoxEdit, splitMoved);
                    break;
                case 8:
                    SetLayout8(form, videoPlayer, subtitleListView, groupBoxWaveform, groupBoxEdit, splitMoved);
                    break;
            }
        }

        // default layout (video right)
        private static void SetLayout0(Form form, Control videoPlayerContainer, SubtitleListView subtitleListView, GroupBox groupBoxWaveform, GroupBox groupBoxEdit, SplitterEventHandler splitMoved)
        {
            var spMain = new SplitContainer();
            MainSplitContainer = spMain;
            spMain.Orientation = Orientation.Horizontal;

            groupBoxWaveform.Parent?.Controls.Remove(groupBoxWaveform);
            spMain.Panel2.Controls.Add(groupBoxWaveform);
            groupBoxWaveform.Dock = DockStyle.Fill;

            var spLeftTop = new SplitContainer();
            spLeftTop.Orientation = Orientation.Vertical;
            spMain.Panel1.Controls.Add(spLeftTop);
            spLeftTop.Dock = DockStyle.Fill;

            videoPlayerContainer.Parent?.Controls.Remove(videoPlayerContainer);
            spLeftTop.Panel2.Controls.Add(videoPlayerContainer);
            videoPlayerContainer.Dock = DockStyle.Fill;

            var spLeftBottom = new SplitContainer();
            spLeftBottom.Orientation = Orientation.Horizontal;
            spLeftTop.Panel1.Controls.Add(spLeftBottom);
            spLeftBottom.Dock = DockStyle.Fill;

            subtitleListView.Parent?.Controls.Remove(subtitleListView);
            spLeftBottom.Panel1.Controls.Add(subtitleListView);
            subtitleListView.Dock = DockStyle.Fill;

            groupBoxEdit.Parent?.Controls.Remove(groupBoxEdit);
            spLeftBottom.Panel2.Controls.Add(groupBoxEdit);
            groupBoxEdit.Dock = DockStyle.Fill;

            form.Controls.Add(spMain);
            spMain.Dock = DockStyle.Fill;
            spMain.BringToFront();

            // auto size
            spLeftTop.SplitterDistance = form.Width / 2;
            spMain.SplitterDistance = CalculateWaveformHeight(form);
            spLeftBottom.SplitterDistance = CalculateListViewHeight(spLeftBottom);

            spMain.SplitterMoved += splitMoved;
            spLeftTop.SplitterMoved += splitMoved;
            spLeftBottom.SplitterMoved += splitMoved;
        }

        private static int CalculateWaveformHeight(Control control)
        {
            var h = control.Height - 400;

            if (control.Height < 800)
            {
                h = control.Height - 300;
            }

            return h < 0 ? 0 : h;
        }

        private static int CalculateListViewHeight(Control control)
        {
            var h = control.Height - 125;

            if (control.Height < 300)
            {
                h = control.Height - 100;
            }

            return h < 0 ? 0 : h;
        }

        // like default layout, but video left
        private static void SetLayout1(Form form, Control videoPlayerContainer, SubtitleListView subtitleListView, GroupBox groupBoxWaveform, GroupBox groupBoxEdit, SplitterEventHandler splitMoved)
        {
            var spMain = new SplitContainer();
            MainSplitContainer = spMain;
            spMain.Orientation = Orientation.Horizontal;

            groupBoxWaveform.Parent?.Controls.Remove(groupBoxWaveform);
            spMain.Panel2.Controls.Add(groupBoxWaveform);
            groupBoxWaveform.Dock = DockStyle.Fill;

            var spLeftTop = new SplitContainer();
            spLeftTop.Orientation = Orientation.Vertical;
            spMain.Panel1.Controls.Add(spLeftTop);
            spLeftTop.Dock = DockStyle.Fill;

            videoPlayerContainer.Parent?.Controls.Remove(videoPlayerContainer);
            spLeftTop.Panel1.Controls.Add(videoPlayerContainer);
            videoPlayerContainer.Dock = DockStyle.Fill;

            var spLeftBottom = new SplitContainer();
            spLeftBottom.Orientation = Orientation.Horizontal;
            spLeftTop.Panel2.Controls.Add(spLeftBottom);
            spLeftBottom.Dock = DockStyle.Fill;

            subtitleListView.Parent?.Controls.Remove(subtitleListView);
            spLeftBottom.Panel1.Controls.Add(subtitleListView);
            subtitleListView.Dock = DockStyle.Fill;

            groupBoxEdit.Parent?.Controls.Remove(groupBoxEdit);
            spLeftBottom.Panel2.Controls.Add(groupBoxEdit);
            groupBoxEdit.Dock = DockStyle.Fill;

            form.Controls.Add(spMain);
            spMain.Dock = DockStyle.Fill;
            spMain.BringToFront();

            // auto size
            spLeftTop.SplitterDistance = form.Width / 2;
            spMain.SplitterDistance = CalculateWaveformHeight(form);
            spLeftBottom.SplitterDistance = CalculateListViewHeight(spLeftBottom);

            spMain.SplitterMoved += splitMoved;
            spLeftTop.SplitterMoved += splitMoved;
            spLeftBottom.SplitterMoved += splitMoved;
        }

        // mobile - video right
        private static void SetLayout2(Form form, Control videoPlayerContainer, SubtitleListView subtitleListView, GroupBox groupBoxWaveform, GroupBox groupBoxEdit, SplitterEventHandler splitMoved)
        {
            var spMain = new SplitContainer();
            MainSplitContainer = spMain;
            spMain.Orientation = Orientation.Vertical;

            videoPlayerContainer.Parent?.Controls.Remove(videoPlayerContainer);
            spMain.Panel2.Controls.Add(videoPlayerContainer);
            videoPlayerContainer.Dock = DockStyle.Fill;

            var spLeftTop = new SplitContainer();
            spLeftTop.Orientation = Orientation.Horizontal;
            spMain.Panel1.Controls.Add(spLeftTop);
            spLeftTop.Dock = DockStyle.Fill;
            var lv = subtitleListView;
            lv.Parent?.Controls.Remove(lv);
            spLeftTop.Panel1.Controls.Add(lv);
            lv.Dock = DockStyle.Fill;

            var spLeftBottom = new SplitContainer();
            spLeftBottom.Orientation = Orientation.Horizontal;
            spLeftTop.Panel2.Controls.Add(spLeftBottom);
            spLeftBottom.Dock = DockStyle.Fill;

            var ge = groupBoxEdit;
            ge.Parent?.Controls.Remove(ge);
            spLeftBottom.Panel1.Controls.Add(ge);
            ge.Dock = DockStyle.Fill;

            var gv = groupBoxWaveform;
            gv.Parent?.Controls.Remove(gv);
            spLeftBottom.Panel2.Controls.Add(gv);
            gv.Dock = DockStyle.Fill;

            form.Controls.Add(spMain);
            spMain.Dock = DockStyle.Fill;
            spMain.BringToFront();

            // auto size
            spMain.SplitterDistance = (int)(form.Width * 0.75);
            spLeftTop.SplitterDistance = Math.Max(0, spLeftTop.Height - 125 - 270);
            spLeftBottom.SplitterDistance = Math.Max(0, spLeftBottom.Height - 270);

            spMain.SplitterMoved += splitMoved;
            spLeftTop.SplitterMoved += splitMoved;
            spLeftBottom.SplitterMoved += splitMoved;
        }

        // mobile - video left
        private static void SetLayout3(Form form, Control videoPlayerContainer, SubtitleListView subtitleListView, GroupBox groupBoxWaveform, GroupBox groupBoxEdit, SplitterEventHandler splitMoved)
        {
            var spMain = new SplitContainer();
            MainSplitContainer = spMain;
            spMain.Orientation = Orientation.Vertical;

            videoPlayerContainer.Parent?.Controls.Remove(videoPlayerContainer);
            spMain.Panel1.Controls.Add(videoPlayerContainer);
            videoPlayerContainer.Dock = DockStyle.Fill;

            var spLeftTop = new SplitContainer();
            spLeftTop.Orientation = Orientation.Horizontal;
            spMain.Panel2.Controls.Add(spLeftTop);
            spLeftTop.Dock = DockStyle.Fill;
            var lv = subtitleListView;
            lv.Parent?.Controls.Remove(lv);
            spLeftTop.Panel1.Controls.Add(lv);
            lv.Dock = DockStyle.Fill;

            var spLeftBottom = new SplitContainer();
            spLeftBottom.Orientation = Orientation.Horizontal;
            spLeftTop.Panel2.Controls.Add(spLeftBottom);
            spLeftBottom.Dock = DockStyle.Fill;

            var ge = groupBoxEdit;
            ge.Parent?.Controls.Remove(ge);
            spLeftBottom.Panel1.Controls.Add(ge);
            ge.Dock = DockStyle.Fill;

            var gv = groupBoxWaveform;
            gv.Parent?.Controls.Remove(gv);
            spLeftBottom.Panel2.Controls.Add(gv);
            gv.Dock = DockStyle.Fill;

            form.Controls.Add(spMain);
            spMain.Dock = DockStyle.Fill;
            spMain.BringToFront();

            // auto size
            spMain.SplitterDistance = (int)(form.Width * 0.25);
            spLeftTop.SplitterDistance = Math.Max(0, spLeftTop.Height - 125 - 270);
            spLeftBottom.SplitterDistance = Math.Max(0, spLeftBottom.Height - 270);

            spMain.SplitterMoved += splitMoved;
            spLeftTop.SplitterMoved += splitMoved;
            spLeftBottom.SplitterMoved += splitMoved;
        }

        // all stacked horizontal
        private static void SetLayout4(Form form, Control videoPlayerContainer, SubtitleListView subtitleListView, GroupBox groupBoxWaveform, GroupBox groupBoxEdit, SplitterEventHandler splitMoved)
        {
            var spMain = new SplitContainer();
            MainSplitContainer = spMain;
            spMain.Orientation = Orientation.Horizontal;

            videoPlayerContainer.Parent?.Controls.Remove(videoPlayerContainer);
            spMain.Panel1.Controls.Add(videoPlayerContainer);
            videoPlayerContainer.Dock = DockStyle.Fill;
            spMain.SplitterDistance = 20;

            var spLeftTop = new SplitContainer();
            spLeftTop.Orientation = Orientation.Horizontal;
            spMain.Panel2.Controls.Add(spLeftTop);
            spLeftTop.MinimumSize = new Size(0, 0);
            spLeftTop.Dock = DockStyle.Fill;
            var lv = subtitleListView;
            lv.Parent?.Controls.Remove(lv);
            spLeftTop.Panel1.Controls.Add(lv);
            lv.Dock = DockStyle.Fill;

            var spLeftBottom = new SplitContainer();
            spLeftBottom.Orientation = Orientation.Horizontal;
            spLeftTop.Panel2.Controls.Add(spLeftBottom);
            spLeftBottom.MinimumSize = new Size(0, 0);
            spLeftBottom.Dock = DockStyle.Fill;

            var ge = groupBoxEdit;
            ge.Parent?.Controls.Remove(ge);
            spLeftBottom.Panel1.Controls.Add(ge);
            ge.Dock = DockStyle.Fill;

            var gv = groupBoxWaveform;
            gv.Parent?.Controls.Remove(gv);
            spLeftBottom.Panel2.Controls.Add(gv);
            gv.Dock = DockStyle.Fill;

            form.Controls.Add(spMain);
            spMain.Dock = DockStyle.Fill;
            spMain.BringToFront();

            spMain.SplitterMoved += splitMoved;
            spLeftTop.SplitterMoved += splitMoved;
            spLeftBottom.SplitterMoved += splitMoved;
        }

        // stacked, no video player
        private static void SetLayout5(Form form, Control videoPlayerContainer, SubtitleListView subtitleListView, GroupBox groupBoxWaveform, GroupBox groupBoxEdit, SplitterEventHandler splitMoved)
        {
            videoPlayerContainer.Parent?.Controls.Remove(videoPlayerContainer);

            var spMain = new SplitContainer();
            MainSplitContainer = spMain;
            spMain.Orientation = Orientation.Horizontal;

            subtitleListView.Parent?.Controls.Remove(subtitleListView);
            spMain.Panel1.Controls.Add(subtitleListView);
            subtitleListView.Dock = DockStyle.Fill;
            spMain.SplitterDistance = 20;

            var spLeftTop = new SplitContainer();
            spLeftTop.Orientation = Orientation.Horizontal;
            spMain.Panel2.Controls.Add(spLeftTop);
            spLeftTop.MinimumSize = new Size(0, 0);
            spLeftTop.Dock = DockStyle.Fill;

            groupBoxEdit.Parent?.Controls.Remove(groupBoxEdit);
            spLeftTop.Panel1.Controls.Add(groupBoxEdit);
            groupBoxEdit.Dock = DockStyle.Fill;

            groupBoxWaveform.Parent?.Controls.Remove(groupBoxWaveform);
            spLeftTop.Panel2.Controls.Add(groupBoxWaveform);
            groupBoxWaveform.Dock = DockStyle.Fill;

            form.Controls.Add(spMain);
            spMain.Dock = DockStyle.Fill;
            spMain.BringToFront();

            // auto size
            spMain.SplitterDistance = Math.Max(0, spMain.Height - 125 - 270);
            spLeftTop.SplitterDistance = Math.Max(0, spLeftTop.Height - 270);

            spMain.SplitterMoved += splitMoved;
            spLeftTop.SplitterMoved += splitMoved;
        }

        // no waveform, video right
        private static void SetLayout6(Form form, Control videoPlayerContainer, SubtitleListView subtitleListView, GroupBox groupBoxWaveform, GroupBox groupBoxEdit, SplitterEventHandler splitMoved)
        {
            groupBoxWaveform.Parent?.Controls.Remove(groupBoxWaveform);

            var spMain = new SplitContainer();
            MainSplitContainer = spMain;
            spMain.Orientation = Orientation.Vertical;

            videoPlayerContainer.Parent?.Controls.Remove(videoPlayerContainer);
            spMain.Panel2.Controls.Add(videoPlayerContainer);
            videoPlayerContainer.Dock = DockStyle.Fill;

            var spLeftTop = new SplitContainer();
            spLeftTop.Orientation = Orientation.Horizontal;
            spMain.Panel1.Controls.Add(spLeftTop);
            spLeftTop.Dock = DockStyle.Fill;

            subtitleListView.Parent?.Controls.Remove(subtitleListView);
            spLeftTop.Panel1.Controls.Add(subtitleListView);
            subtitleListView.Dock = DockStyle.Fill;

            groupBoxEdit.Parent?.Controls.Remove(groupBoxEdit);
            spLeftTop.Panel2.Controls.Add(groupBoxEdit);
            groupBoxEdit.Dock = DockStyle.Fill;

            form.Controls.Add(spMain);
            spMain.Dock = DockStyle.Fill;
            spMain.BringToFront();

            // auto size
            spMain.SplitterDistance = (int)(spMain.Width * 0.4);
            spLeftTop.SplitterDistance = Math.Max(0, spLeftTop.Height - 125);

            spMain.SplitterMoved += splitMoved;
            spLeftTop.SplitterMoved += splitMoved;
        }

        // stacked, video but no waveform
        private static void SetLayout7(Form form, Control videoPlayerContainer, SubtitleListView subtitleListView, GroupBox groupBoxWaveform, GroupBox groupBoxEdit, SplitterEventHandler splitMoved)
        {
            var spMain = new SplitContainer();
            MainSplitContainer = spMain;
            spMain.Orientation = Orientation.Horizontal;

            subtitleListView.Parent?.Controls.Remove(subtitleListView);
            spMain.Panel1.Controls.Add(subtitleListView);
            subtitleListView.Dock = DockStyle.Fill;
            spMain.SplitterDistance = 20;

            var spLeftTop = new SplitContainer();
            spLeftTop.Orientation = Orientation.Horizontal;
            spMain.Panel2.Controls.Add(spLeftTop);
            spLeftTop.MinimumSize = new Size(0, 0);
            spLeftTop.Dock = DockStyle.Fill;

            groupBoxEdit.Parent?.Controls.Remove(groupBoxEdit);
            spLeftTop.Panel1.Controls.Add(groupBoxEdit);
            groupBoxEdit.Dock = DockStyle.Fill;

            videoPlayerContainer.Parent?.Controls.Remove(videoPlayerContainer);
            groupBoxWaveform.Parent?.Controls.Remove(groupBoxWaveform);
            spLeftTop.Panel2.Controls.Add(groupBoxWaveform);
            groupBoxWaveform.Dock = DockStyle.Fill;

            form.Controls.Add(spMain);
            spMain.Dock = DockStyle.Fill;
            spMain.BringToFront();

            // auto size
            spMain.SplitterDistance = Math.Max(0, spMain.Height - 125 - 270);
            spLeftTop.SplitterDistance = Math.Max(0, spLeftTop.Height - 270);

            spMain.SplitterMoved += splitMoved;
            spLeftTop.SplitterMoved += splitMoved;
        }

        // no video or waveform
        private static void SetLayout8(Form form, Control videoPlayerContainer, SubtitleListView subtitleListView, GroupBox groupBoxWaveform, GroupBox groupBoxEdit, SplitterEventHandler splitMoved)
        {
            videoPlayerContainer.Parent?.Controls.Remove(videoPlayerContainer);
            groupBoxWaveform.Parent?.Controls.Remove(groupBoxWaveform);

            var spMain = new SplitContainer();
            MainSplitContainer = spMain;
            spMain.Orientation = Orientation.Horizontal;

            subtitleListView.Parent?.Controls.Remove(subtitleListView);
            spMain.Panel1.Controls.Add(subtitleListView);
            subtitleListView.Dock = DockStyle.Fill;

            groupBoxEdit.Parent?.Controls.Remove(groupBoxEdit);
            spMain.Panel2.Controls.Add(groupBoxEdit);
            groupBoxEdit.Dock = DockStyle.Fill;

            form.Controls.Add(spMain);
            spMain.Dock = DockStyle.Fill;
            spMain.BringToFront();

            // auto size
            spMain.SplitterDistance = Math.Max(0, spMain.Height - 125);
        }

        public static string SaveLayout()
        {
            if (MainSplitContainer == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append(MainSplitContainer.SplitterDistance.ToString(CultureInfo.InvariantCulture));
            sb.Append(",");
            var splitContainers = GetAll(MainSplitContainer, MainSplitContainer.GetType());
            foreach (var c in splitContainers)
            {
                if (c is SplitContainer sc)
                {
                    sb.Append(sc.SplitterDistance.ToString(CultureInfo.InvariantCulture));
                    sb.Append(",");
                }
            }

            return sb.ToString().TrimEnd(',');
        }

        public static void RestoreLayout(string layoutSizes)
        {
            if (MainSplitContainer == null || string.IsNullOrEmpty(layoutSizes))
            {
                return;
            }

            var sizes = new List<int>();
            foreach (var s in layoutSizes.Split(','))
            {
                if (int.TryParse(s, out var number))
                {
                    sizes.Add(number);
                }
            }

            if (sizes.Count == 0)
            {
                return;
            }

            try
            {
                MainSplitContainer.SplitterDistance = sizes[0];
                var splitContainers = GetAll(MainSplitContainer, MainSplitContainer.GetType());
                for (var index = 0; index < splitContainers.Count; index++)
                {
                    var c = splitContainers[index];
                    if (c is SplitContainer sc)
                    {
                        sc.SplitterDistance = sizes[index + 1];
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        public static List<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>().ToList();
            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                .Concat(controls)
                .Where(c => c.GetType() == type)
                .ToList();
        }
    }
}

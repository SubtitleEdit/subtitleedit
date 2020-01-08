using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public sealed class SubtitleListView : ListView
    {
        public enum SubtitleColumn
        {
            Number,
            Start,
            End,
            Duration,
            CharactersPerSeconds,
            WordsPerMinute,
            Gap,
            Actor,
            Region,
            Text,
            TextAlternate,
            Extra,
            Network
        }

        private List<SubtitleColumn> SubtitleColumns { get; }

        public int GetColumnIndex(SubtitleColumn column)
        {
            return SubtitleColumns.IndexOf(column);
        }

        public int ColumnIndexNumber { get; private set; }
        public int ColumnIndexStart { get; private set; }
        public int ColumnIndexEnd { get; private set; }
        public int ColumnIndexDuration { get; private set; }
        public int ColumnIndexWpm { get; private set; }
        public int ColumnIndexCps { get; private set; }
        public int ColumnIndexGap { get; private set; }
        public int ColumnIndexActor { get; private set; }
        public int ColumnIndexRegion { get; private set; }
        public int ColumnIndexText { get; private set; }
        public int ColumnIndexTextAlternate { get; private set; }
        public int ColumnIndexExtra { get; private set; }
        public int ColumnIndexNetwork { get; private set; }

        public bool IsAlternateTextColumnVisible => ColumnIndexTextAlternate >= 0;
        private string _lineSeparatorString = " || ";

        private Font _subtitleFont = new Font("Tahoma", 8.25F);

        private string _subtitleFontName = "Tahoma";

        public override bool RightToLeftLayout
        {
            get => base.RightToLeftLayout;
            set
            {
                var hzAlignment = value ? HorizontalAlignment.Left : HorizontalAlignment.Right;
                if (ColumnIndexCps >= 0)
                {
                    Columns[ColumnIndexCps].TextAlign = hzAlignment;
                }

                if (ColumnIndexWpm >= 0)
                {
                    Columns[ColumnIndexWpm].TextAlign = hzAlignment;
                }

                base.RightToLeftLayout = value;
            }
        }

        public string SubtitleFontName
        {
            get => _subtitleFontName;
            set
            {
                _subtitleFontName = value;
                _subtitleFont = new Font(_subtitleFontName, SubtitleFontSize, GetFontStyle());
            }
        }

        private bool _subtitleFontBold;

        public bool SubtitleFontBold
        {
            get { return _subtitleFontBold; }
            set
            {
                _subtitleFontBold = value;
                _subtitleFont = new Font(_subtitleFontName, SubtitleFontSize, GetFontStyle());
            }
        }

        private int _subtitleFontSize = 8;

        public int SubtitleFontSize
        {
            get => _subtitleFontSize;
            set
            {
                _subtitleFontSize = value;
                _subtitleFont = new Font(_subtitleFontName, SubtitleFontSize, GetFontStyle());
            }
        }

        public bool UseSyntaxColoring { get; set; }
        private Settings _settings;
        private bool _saveColumnWidthChanges;

        public int FirstVisibleIndex { get; set; } = -1;

        public void InitializeLanguage(LanguageStructure.General general, Settings settings)
        {
            int idx = GetColumnIndex(SubtitleColumn.Number);
            if (idx >= 0)
            {
                Columns[idx].Text = general.NumberSymbol;
            }

            idx = GetColumnIndex(SubtitleColumn.Start);
            if (idx >= 0)
            {
                Columns[idx].Text = general.StartTime;
            }

            idx = GetColumnIndex(SubtitleColumn.End);
            if (idx >= 0)
            {
                Columns[idx].Text = general.EndTime;
            }

            idx = GetColumnIndex(SubtitleColumn.Duration);
            if (idx >= 0)
            {
                Columns[idx].Text = general.Duration;
            }

            idx = GetColumnIndex(SubtitleColumn.CharactersPerSeconds);
            if (idx >= 0)
            {
                Columns[idx].Text = general.CharsPerSec;
            }

            idx = GetColumnIndex(SubtitleColumn.WordsPerMinute);
            if (idx >= 0)
            {
                Columns[idx].Text = general.WordsPerMin;
            }

            idx = GetColumnIndex(SubtitleColumn.Gap);
            if (idx >= 0)
            {
                Columns[idx].Text = general.Gap;
            }

            idx = GetColumnIndex(SubtitleColumn.Actor);
            if (idx >= 0)
            {
                Columns[idx].Text = general.Actor;
            }

            idx = GetColumnIndex(SubtitleColumn.Region);
            if (idx >= 0)
            {
                Columns[idx].Text = general.Region;
            }

            idx = GetColumnIndex(SubtitleColumn.Text);
            if (idx >= 0)
            {
                Columns[idx].Text = general.Text;
            }

            if (settings.General.ListViewLineSeparatorString != null)
            {
                _lineSeparatorString = settings.General.ListViewLineSeparatorString;
            }

            if (!string.IsNullOrEmpty(settings.General.SubtitleFontName))
            {
                _subtitleFontName = settings.General.SubtitleFontName;
            }

            SubtitleFontBold = settings.General.SubtitleListViewFontBold;
            if (settings.General.SubtitleListViewFontSize > 6 && settings.General.SubtitleListViewFontSize < 72)
            {
                SubtitleFontSize = settings.General.SubtitleListViewFontSize;
            }

            ForeColor = settings.General.SubtitleFontColor;
            BackColor = settings.General.SubtitleBackgroundColor;
            _settings = settings;
        }

        public void InitializeTimestampColumnWidths(Control parentForm)
        {
            if (_settings != null && _settings.General.ListViewColumnsRememberSize && _settings.General.ListViewNumberWidth > 1 &&
                _settings.General.ListViewStartWidth > 1 && _settings.General.ListViewEndWidth > 1 && _settings.General.ListViewDurationWidth > 1)
            {
                int idx = GetColumnIndex(SubtitleColumn.Number);
                if (idx >= 0)
                {
                    Columns[idx].Width = Configuration.Settings.General.ListViewNumberWidth;
                }

                idx = GetColumnIndex(SubtitleColumn.Start);
                if (idx >= 0)
                {
                    Columns[idx].Width = _settings.General.ListViewStartWidth;
                }

                idx = GetColumnIndex(SubtitleColumn.End);
                if (idx >= 0)
                {
                    Columns[idx].Width = _settings.General.ListViewEndWidth;
                }

                idx = GetColumnIndex(SubtitleColumn.Duration);
                if (idx >= 0)
                {
                    Columns[idx].Width = _settings.General.ListViewDurationWidth;
                }

                idx = GetColumnIndex(SubtitleColumn.CharactersPerSeconds);
                if (idx >= 0)
                {
                    Columns[idx].Width = _settings.General.ListViewCpsWidth;
                }

                idx = GetColumnIndex(SubtitleColumn.WordsPerMinute);
                if (idx >= 0)
                {
                    Columns[idx].Width = _settings.General.ListViewWpmWidth;
                }

                idx = GetColumnIndex(SubtitleColumn.Gap);
                if (idx >= 0)
                {
                    Columns[idx].Width = _settings.General.ListViewGapWidth;
                }

                idx = GetColumnIndex(SubtitleColumn.Actor);
                if (idx >= 0)
                {
                    Columns[idx].Width = _settings.General.ListViewActorWidth;
                }

                idx = GetColumnIndex(SubtitleColumn.Region);
                if (idx >= 0)
                {
                    Columns[idx].Width = _settings.General.ListViewRegionWidth;
                }

                idx = GetColumnIndex(SubtitleColumn.Text);
                if (idx >= 0)
                {
                    Columns[idx].Width = _settings.General.ListViewTextWidth;
                }

                _saveColumnWidthChanges = true;
            }
            else if (parentForm != null)
            {
                using (var graphics = parentForm.CreateGraphics())
                {
                    var timestampSizeF = graphics.MeasureString(new TimeCode(0, 0, 33, 527).ToDisplayString(), Font);
                    var timestampWidth = (int)(timestampSizeF.Width + 0.5) + 11;

                    var idx = GetColumnIndex(SubtitleColumn.Start);
                    if (idx >= 0)
                    {
                        Columns[idx].Width = timestampWidth;
                    }

                    idx = GetColumnIndex(SubtitleColumn.End);
                    if (idx >= 0)
                    {
                        Columns[idx].Width = timestampWidth;
                    }

                    idx = GetColumnIndex(SubtitleColumn.Duration);
                    if (idx >= 0)
                    {
                        Columns[idx].Width = (int)(timestampWidth * 0.8);
                    }
                }
            }

            SubtitleListViewLastColumnFill(this, null);
        }

        public SubtitleListView()
        {
            DoubleBuffered = true;
            UseSyntaxColoring = true;
            Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AllowColumnReorder = true;
            HeaderStyle = ColumnHeaderStyle.Nonclickable;

            ColumnIndexNumber = -1;
            ColumnIndexStart = -1;
            ColumnIndexEnd = -1;
            ColumnIndexDuration = -1;
            ColumnIndexWpm = -1;
            ColumnIndexCps = -1;
            ColumnIndexGap = -1;
            ColumnIndexActor = -1;
            ColumnIndexRegion = -1;
            ColumnIndexText = -1;
            ColumnIndexTextAlternate = -1;
            ColumnIndexExtra = -1;
            ColumnIndexNetwork = -1;

            SubtitleColumns = new List<SubtitleColumn>
            {
                SubtitleColumn.Number,
                SubtitleColumn.Start,
                SubtitleColumn.End,
                SubtitleColumn.Duration,
                SubtitleColumn.Text
            };
            UpdateColumnIndexes();

            foreach (var c in SubtitleColumns)
            {
                switch (c)
                {
                    case SubtitleColumn.Number:
                        Columns.Add(new ColumnHeader { Width = 50 });
                        break;
                    case SubtitleColumn.Start:
                        Columns.Add(new ColumnHeader { Width = 80 });
                        break;
                    case SubtitleColumn.End:
                        Columns.Add(new ColumnHeader { Width = 80 });
                        break;
                    case SubtitleColumn.Duration:
                        Columns.Add(new ColumnHeader { Width = 55 });
                        break;
                    case SubtitleColumn.CharactersPerSeconds:
                        Columns.Add(new ColumnHeader { Width = 60 });
                        break;
                    case SubtitleColumn.WordsPerMinute:
                        Columns.Add(new ColumnHeader { Width = 65 });
                        break;
                    case SubtitleColumn.Gap:
                        Columns.Add(new ColumnHeader { Width = 60 });
                        break;
                    case SubtitleColumn.Actor:
                        Columns.Add(new ColumnHeader { Width = 80 });
                        break;
                    case SubtitleColumn.Region:
                        Columns.Add(new ColumnHeader { Width = 60 });
                        break;
                    case SubtitleColumn.Text:
                        Columns.Add(new ColumnHeader { Width = 300 });
                        break;
                }
            }

            if (Configuration.Settings != null && !Configuration.Settings.Tools.ListViewShowColumnEndTime)
            {
                HideColumn(SubtitleColumn.End);
            }

            if (Configuration.Settings != null && !Configuration.Settings.Tools.ListViewShowColumnDuration)
            {
                HideColumn(SubtitleColumn.Duration);
            }

            if (Configuration.Settings != null && Configuration.Settings.Tools.ListViewShowColumnCharsPerSec)
            {
                ShowCharsSecColumn(Configuration.Settings.Language.General.CharsPerSec);
            }

            if (Configuration.Settings != null && Configuration.Settings.Tools.ListViewShowColumnWordsPerMin)
            {
                ShowWordsMinColumn(Configuration.Settings.Language.General.WordsPerMin);
            }

            if (Configuration.Settings != null && Configuration.Settings.Tools.ListViewShowColumnGap)
            {
                ShowGapColumn(Configuration.Settings.Language.General.Gap);
            }

            SubtitleListViewLastColumnFill(this, null);

            FullRowSelect = true;
            View = View.Details;
            Resize += SubtitleListViewLastColumnFill;
            GridLines = true;
            ColumnWidthChanged += SubtitleListViewColumnWidthChanged;
            OwnerDraw = true;
            DrawItem += SubtitleListView_DrawItem;
            DrawSubItem += SubtitleListView_DrawSubItem;
            DrawColumnHeader += SubtitleListView_DrawColumnHeader;
        }

        private void UpdateColumnIndexes()
        {
            ColumnIndexNumber = GetColumnIndex(SubtitleColumn.Number);
            ColumnIndexStart = GetColumnIndex(SubtitleColumn.Start);
            ColumnIndexEnd = GetColumnIndex(SubtitleColumn.End);
            ColumnIndexDuration = GetColumnIndex(SubtitleColumn.Duration);
            ColumnIndexCps = GetColumnIndex(SubtitleColumn.CharactersPerSeconds);
            ColumnIndexWpm = GetColumnIndex(SubtitleColumn.WordsPerMinute);
            ColumnIndexGap = GetColumnIndex(SubtitleColumn.Gap);
            ColumnIndexActor = GetColumnIndex(SubtitleColumn.Actor);
            ColumnIndexRegion = GetColumnIndex(SubtitleColumn.Region);
            ColumnIndexText = GetColumnIndex(SubtitleColumn.Text);
            ColumnIndexTextAlternate = GetColumnIndex(SubtitleColumn.TextAlternate);
            ColumnIndexExtra = GetColumnIndex(SubtitleColumn.Extra);
            ColumnIndexNetwork = GetColumnIndex(SubtitleColumn.Network);
        }

        private void SubtitleListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void SubtitleListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color backgroundColor = Items[e.ItemIndex].SubItems[e.ColumnIndex].BackColor;
            if (Focused && backgroundColor == BackColor || RightToLeftLayout)
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

                    Rectangle rect = e.Bounds;
                    if (Configuration.Settings != null)
                    {
                        backgroundColor = backgroundColor == BackColor ? Configuration.Settings.Tools.ListViewUnfocusedSelectedColor : GetCustomColor(backgroundColor);
                        var sb = new SolidBrush(backgroundColor);
                        e.Graphics.FillRectangle(sb, rect);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                    }

                    int addX = 0;

                    if (e.ColumnIndex == 0 && StateImageList?.Images.Count > 0)
                    {
                        addX = 18;
                    }

                    if (e.ColumnIndex == 0 && e.Item.StateImageIndex >= 0 && StateImageList?.Images.Count > e.Item.StateImageIndex)
                    {
                        e.Graphics.DrawImage(StateImageList.Images[e.Item.StateImageIndex], new Rectangle(rect.X + 4, rect.Y + 2, 16, 16));
                    }

                    if (Columns[e.ColumnIndex].TextAlign == HorizontalAlignment.Right)
                    {
                        var stringWidth = (int)e.Graphics.MeasureString(e.Item.SubItems[e.ColumnIndex].Text, _subtitleFont).Width;
                        TextRenderer.DrawText(e.Graphics, e.Item.SubItems[e.ColumnIndex].Text, _subtitleFont, new Point(e.Bounds.Right - stringWidth - 7, e.Bounds.Top + 2), e.Item.ForeColor, TextFormatFlags.NoPrefix);
                    }
                    else
                    {
                        TextRenderer.DrawText(e.Graphics, e.Item.SubItems[e.ColumnIndex].Text, _subtitleFont, new Point(e.Bounds.Left + 3 + addX, e.Bounds.Top + 2), e.Item.ForeColor, TextFormatFlags.NoPrefix);
                    }
                }
                else
                {
                    e.DrawDefault = true;
                }
            }
        }

        private static Color GetCustomColor(Color color)
        {
            int r = Math.Max(color.R - 39, 0);
            int g = Math.Max(color.G - 39, 0);
            int b = Math.Max(color.B - 39, 0);
            return Color.FromArgb(color.A, r, g, b);
        }

        private void SubtitleListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (!Focused && (e.State & ListViewItemStates.Selected) != 0)
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

        private void SubtitleListViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (_settings != null && _saveColumnWidthChanges)
            {
                if (e.ColumnIndex == ColumnIndexNumber)
                {
                    Configuration.Settings.General.ListViewNumberWidth = Columns[ColumnIndexNumber].Width;
                }
                else if (e.ColumnIndex == ColumnIndexStart)
                {
                    Configuration.Settings.General.ListViewStartWidth = Columns[ColumnIndexStart].Width;
                }
                else if (e.ColumnIndex == ColumnIndexEnd)
                {
                    Configuration.Settings.General.ListViewEndWidth = Columns[ColumnIndexEnd].Width;
                }
                else if (e.ColumnIndex == ColumnIndexDuration)
                {
                    Configuration.Settings.General.ListViewDurationWidth = Columns[ColumnIndexDuration].Width;
                }
                else if (e.ColumnIndex == ColumnIndexCps)
                {
                    Configuration.Settings.General.ListViewCpsWidth = Columns[ColumnIndexCps].Width;
                }
                else if (e.ColumnIndex == ColumnIndexWpm)
                {
                    Configuration.Settings.General.ListViewWpmWidth = Columns[ColumnIndexWpm].Width;
                }
                else if (e.ColumnIndex == ColumnIndexGap)
                {
                    Configuration.Settings.General.ListViewGapWidth = Columns[ColumnIndexGap].Width;
                }
                else if (e.ColumnIndex == ColumnIndexActor)
                {
                    Configuration.Settings.General.ListViewActorWidth = Columns[ColumnIndexActor].Width;
                }
                else if (e.ColumnIndex == ColumnIndexRegion)
                {
                    Configuration.Settings.General.ListViewRegionWidth = Columns[ColumnIndexRegion].Width;
                }
                if (e.ColumnIndex == ColumnIndexText)
                {
                    Configuration.Settings.General.ListViewTextWidth = Columns[ColumnIndexText].Width;
                }
            }
        }

        public void AutoSizeColumns()
        {
            var numberIdx = GetColumnIndex(SubtitleColumn.Number);
            if (numberIdx >= 0)
            {
                Columns[numberIdx].Width = 50;
                Columns[numberIdx].Width = 50;
            }

            var startIdx = GetColumnIndex(SubtitleColumn.Start);
            var endIdx = GetColumnIndex(SubtitleColumn.End);
            var durationIdx = GetColumnIndex(SubtitleColumn.Duration);
            int timeStampWidth;
            try
            {
                using (var graphics = CreateGraphics())
                {
                    var timestampSizeF = graphics.MeasureString(new TimeCode(0, 0, 33, 527).ToDisplayString(), Font);
                    timeStampWidth = (int)(timestampSizeF.Width + 0.5) + 11;
                }
            }
            catch
            {
                timeStampWidth = 65;
            }
            if (startIdx >= 0)
            {
                Columns[startIdx].Width = timeStampWidth;
            }
            if (endIdx >= 0)
            {
                Columns[endIdx].Width = timeStampWidth;
            }
            if (durationIdx >= 0)
            {
                Columns[durationIdx].Width = (int)(timeStampWidth * 0.8);
            }

            var cpsIdx = GetColumnIndex(SubtitleColumn.CharactersPerSeconds);
            if (cpsIdx >= 0)
            {
                Columns[cpsIdx].Width = 60;
                Columns[cpsIdx].Width = 60;
            }

            var wpmIdx = GetColumnIndex(SubtitleColumn.WordsPerMinute);
            if (wpmIdx >= 0)
            {
                Columns[wpmIdx].Width = 65;
                Columns[wpmIdx].Width = 65;
            }

            var gapIdx = GetColumnIndex(SubtitleColumn.Gap);
            if (gapIdx >= 0)
            {
                Columns[gapIdx].Width = 60;
                Columns[gapIdx].Width = 60;
            }

            var actorIdx = GetColumnIndex(SubtitleColumn.Actor);
            if (actorIdx >= 0)
            {
                Columns[actorIdx].Width = 80;
                Columns[actorIdx].Width = 80;
            }

            var regionIdx = GetColumnIndex(SubtitleColumn.Region);
            if (regionIdx >= 0)
            {
                Columns[regionIdx].Width = 60;
                Columns[regionIdx].Width = 60;
            }

            int w = 0;
            for (int index = 0; index < SubtitleColumns.Count; index++)
            {
                var column = SubtitleColumns[index];
                int cw = Columns[index].Width;
                if (column != SubtitleColumn.Text && column != SubtitleColumn.TextAlternate)
                {
                    w += cw;
                }
            }
            int lengthAvailable = Width - w;
            if (ColumnIndexTextAlternate >= 0)
            {
                lengthAvailable = lengthAvailable / 2;
                Columns[ColumnIndexTextAlternate].Width = lengthAvailable;
                Columns[ColumnIndexTextAlternate].Width = lengthAvailable;
                Columns[ColumnIndexTextAlternate].Width = lengthAvailable;
            }
            Columns[ColumnIndexText].Width = lengthAvailable;
            Columns[ColumnIndexText].Width = lengthAvailable;
            Columns[ColumnIndexText].Width = lengthAvailable;
            SubtitleListViewLastColumnFill(this, null);
        }

        public void AutoSizeAllColumns(Form parentForm)
        {
            InitializeTimestampColumnWidths(parentForm);

            var numberIdx = GetColumnIndex(SubtitleColumn.Number);
            if (numberIdx >= 0)
            {
                if (_settings != null && _settings.General.ListViewColumnsRememberSize && _settings.General.ListViewNumberWidth > 1)
                {
                    Columns[numberIdx].Width = _settings.General.ListViewNumberWidth;
                }
                else
                {
                    Columns[numberIdx].Width = 50;
                }
            }

            var startIdx = GetColumnIndex(SubtitleColumn.Start);
            var endIdx = GetColumnIndex(SubtitleColumn.End);
            int timeStampWidth;
            try
            {
                using (var graphics = CreateGraphics())
                {
                    var timestampSizeF = graphics.MeasureString(new TimeCode(0, 0, 33, 527).ToDisplayString(), Font);
                    timeStampWidth = (int)(timestampSizeF.Width + 0.5) + 11;
                }
            }
            catch
            {
                timeStampWidth = 65;
            }
            if (startIdx >= 0)
            {
                if (_settings != null && _settings.General.ListViewColumnsRememberSize && _settings.General.ListViewStartWidth > 1)
                {
                    Columns[startIdx].Width = _settings.General.ListViewStartWidth;
                }
                else
                {
                    Columns[startIdx].Width = timeStampWidth;
                }
            }
            if (endIdx >= 0)
            {
                if (_settings != null && _settings.General.ListViewColumnsRememberSize && _settings.General.ListViewEndWidth > 1)
                {
                    Columns[endIdx].Width = _settings.General.ListViewEndWidth;
                }
                else
                {
                    Columns[endIdx].Width = timeStampWidth;
                }
            }

            int w = 0;
            for (int index = 0; index < SubtitleColumns.Count; index++)
            {
                var column = SubtitleColumns[index];
                int cw = Columns[index].Width;
                if (cw < 10 || column == SubtitleColumn.Extra || column == SubtitleColumn.Network)
                {
                    cw = 55;
                    if (column == SubtitleColumn.CharactersPerSeconds)
                    {
                        cw = 65;
                    }
                    else if (column == SubtitleColumn.WordsPerMinute)
                    {
                        cw = 70;
                    }
                    else if (column == SubtitleColumn.Gap)
                    {
                        cw = 60;
                    }
                    else if (column == SubtitleColumn.Actor)
                    {
                        cw = 70;
                    }
                    else if (column == SubtitleColumn.Region)
                    {
                        cw = 60;
                    }
                    else if (column != SubtitleColumn.Number)
                    {
                        cw = 120;
                    }

                    Columns[index].Width = cw;
                    Columns[index].Width = cw;
                    Columns[index].Width = cw;
                }
                if (column != SubtitleColumn.Text && column != SubtitleColumn.TextAlternate)
                {
                    w += cw;
                }
            }

            int lengthAvailable = Width - w;
            if (ColumnIndexTextAlternate >= 0 && Columns.Count > ColumnIndexTextAlternate)
            {
                lengthAvailable = lengthAvailable / 2;
                Columns[ColumnIndexTextAlternate].Width = lengthAvailable;
                Columns[ColumnIndexTextAlternate].Width = lengthAvailable;
                Columns[ColumnIndexTextAlternate].Width = lengthAvailable;
            }
            Columns[ColumnIndexText].Width = lengthAvailable;
            Columns[ColumnIndexText].Width = lengthAvailable;
            Columns[ColumnIndexText].Width = lengthAvailable;
            SubtitleListViewLastColumnFill(this, null);
        }

        public void ShowEndColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.End) == -1)
            {
                var ch = new ColumnHeader { Text = title, TextAlign = RightToLeftLayout ? HorizontalAlignment.Right : HorizontalAlignment.Left };
                if (ColumnIndexStart >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexStart + 1, SubtitleColumn.End);
                    Columns.Insert(ColumnIndexStart + 1, ch);
                }
                else if (ColumnIndexNumber >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexNumber + 1, SubtitleColumn.End);
                    Columns.Insert(ColumnIndexStart + 1, ch);
                }
                else
                {
                    SubtitleColumns.Add(SubtitleColumn.End);
                    Columns.Insert(0, ch);
                }
                UpdateColumnIndexes();

                try
                {
                    using (var graphics = Parent.CreateGraphics())
                    {
                        var timestampSizeF = graphics.MeasureString(new TimeCode(0, 0, 33, 527).ToDisplayString(), Font);
                        var timestampWidth = (int)(timestampSizeF.Width + 0.5) + 11;
                        Columns[ColumnIndexEnd].Width = timestampWidth;
                    }
                }
                catch
                {
                    Columns[ColumnIndexEnd].Width = 65;
                }

                AutoSizeAllColumns(null);
            }
        }

        public void ShowDurationColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.Duration) == -1)
            {
                var ch = new ColumnHeader { Text = title, TextAlign = RightToLeftLayout ? HorizontalAlignment.Right : HorizontalAlignment.Left };
                if (ColumnIndexEnd >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexEnd + 1, SubtitleColumn.Duration);
                    Columns.Insert(ColumnIndexEnd + 1, ch);
                }
                else if (ColumnIndexStart >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexStart + 1, SubtitleColumn.Duration);
                    Columns.Insert(ColumnIndexStart + 1, ch);
                }
                else if (ColumnIndexNumber >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexNumber + 1, SubtitleColumn.Duration);
                    Columns.Insert(ColumnIndexStart + 1, ch);
                }
                else
                {
                    SubtitleColumns.Add(SubtitleColumn.Duration);
                    Columns.Insert(0, ch);
                }
                UpdateColumnIndexes();

                try
                {
                    using (var graphics = Parent.CreateGraphics())
                    {
                        var timestampSizeF = graphics.MeasureString(new TimeCode(0, 0, 33, 527).ToDisplayString(), Font);
                        var timestampWidth = (int)(timestampSizeF.Width + 0.5) + 11;
                        Columns[ColumnIndexDuration].Width = (int)(timestampWidth * 0.8);
                    }
                }
                catch
                {
                    Columns[ColumnIndexDuration].Width = 55;
                }

                AutoSizeAllColumns(null);
            }
        }

        public void ShowAlternateTextColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.TextAlternate) == -1)
            {
                if (ColumnIndexText >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexText + 1, SubtitleColumn.TextAlternate);
                    Columns.Insert(ColumnIndexText + 1, new ColumnHeader { Text = title });
                }
                else
                {
                    SubtitleColumns.Add(SubtitleColumn.TextAlternate);
                    Columns.Add(new ColumnHeader { Text = title });
                }
                UpdateColumnIndexes();
                Columns[ColumnIndexTextAlternate].Width = 300;
                Columns[ColumnIndexTextAlternate].Width = 300;
                Columns[ColumnIndexTextAlternate].Width = 300;
                AutoSizeAllColumns(null);
            }
            else
            {
                Columns[ColumnIndexTextAlternate].Text = title;
            }
        }

        public void ShowExtraColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.Extra) == -1)
            {
                if (ColumnIndexNetwork >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexNetwork, SubtitleColumn.Extra);
                    Columns.Insert(ColumnIndexNetwork, new ColumnHeader { Text = title, Width = 120 });
                }
                else
                {
                    SubtitleColumns.Add(SubtitleColumn.Extra);
                    Columns.Add(new ColumnHeader { Text = title, Width = 120 });
                }
                UpdateColumnIndexes();
                Columns[ColumnIndexExtra].Width = 120;
                Columns[ColumnIndexExtra].Width = 120;
                Columns[ColumnIndexExtra].Width = 120;
                AutoSizeAllColumns(null);
            }
            else
            {
                Columns[ColumnIndexExtra].Text = title;
            }
        }

        public void ShowNetworkColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.Network) == -1)
            {
                SubtitleColumns.Add(SubtitleColumn.Network);
                Columns.Add(new ColumnHeader { Text = title, Width = 120 });
                UpdateColumnIndexes();
                AutoSizeAllColumns(null);
            }
        }

        public void ShowCharsSecColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.CharactersPerSeconds) == -1)
            {
                var ch = new ColumnHeader { Text = title, TextAlign = RightToLeftLayout ? HorizontalAlignment.Left : HorizontalAlignment.Right };
                if (ColumnIndexDuration >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexDuration + 1, SubtitleColumn.CharactersPerSeconds);
                    Columns.Insert(ColumnIndexDuration + 1, ch);
                }
                else if (ColumnIndexEnd >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexEnd + 1, SubtitleColumn.CharactersPerSeconds);
                    Columns.Insert(ColumnIndexEnd + 1, ch);
                }
                else if (ColumnIndexStart >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexStart + 1, SubtitleColumn.CharactersPerSeconds);
                    Columns.Insert(ColumnIndexStart + 1, ch);
                }
                else
                {
                    SubtitleColumns.Add(SubtitleColumn.CharactersPerSeconds);
                    Columns.Add(ch);
                }
                UpdateColumnIndexes();
                Columns[ColumnIndexCps].Width = 65;
                Columns[ColumnIndexCps].Width = 65;
                AutoSizeAllColumns(null);
            }
        }

        public void ShowWordsMinColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.WordsPerMinute) == -1)
            {
                var ch = new ColumnHeader { Text = title, TextAlign = RightToLeftLayout ? HorizontalAlignment.Left : HorizontalAlignment.Right };
                if (ColumnIndexCps >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexCps + 1, SubtitleColumn.WordsPerMinute);
                    Columns.Insert(ColumnIndexCps + 1, ch);
                }
                else if (ColumnIndexDuration >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexDuration + 1, SubtitleColumn.WordsPerMinute);
                    Columns.Insert(ColumnIndexDuration + 1, ch);
                }
                else if (ColumnIndexEnd >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexEnd + 1, SubtitleColumn.WordsPerMinute);
                    Columns.Insert(ColumnIndexEnd + 1, ch);
                }
                else if (ColumnIndexStart >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexStart + 1, SubtitleColumn.WordsPerMinute);
                    Columns.Insert(ColumnIndexStart + 1, ch);
                }
                else
                {
                    SubtitleColumns.Add(SubtitleColumn.WordsPerMinute);
                    Columns.Add(ch);
                }
                UpdateColumnIndexes();
                Columns[ColumnIndexWpm].Width = 70;
                Columns[ColumnIndexWpm].Width = 70;
                AutoSizeAllColumns(null);
            }
        }

        public void ShowGapColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.Gap) == -1)
            {
                var ch = new ColumnHeader { Text = title };
                if (ColumnIndexWpm >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexWpm + 1, SubtitleColumn.Gap);
                    Columns.Insert(ColumnIndexWpm + 1, ch);
                }
                else if (ColumnIndexCps >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexCps + 1, SubtitleColumn.Gap);
                    Columns.Insert(ColumnIndexCps + 1, ch);
                }
                else if (ColumnIndexDuration >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexDuration + 1, SubtitleColumn.Gap);
                    Columns.Insert(ColumnIndexDuration + 1, ch);
                }
                else if (ColumnIndexEnd >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexEnd + 1, SubtitleColumn.Gap);
                    Columns.Insert(ColumnIndexEnd + 1, ch);
                }
                else if (ColumnIndexStart >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexStart + 1, SubtitleColumn.Gap);
                    Columns.Insert(ColumnIndexStart + 1, ch);
                }
                else
                {
                    SubtitleColumns.Add(SubtitleColumn.Gap);
                    Columns.Add(ch);
                }
                UpdateColumnIndexes();
                Columns[ColumnIndexGap].Width = 80;
                Columns[ColumnIndexGap].Width = 80;
                AutoSizeAllColumns(null);
            }
        }

        public void ShowActorColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.Actor) == -1)
            {
                var ch = new ColumnHeader { Text = title };
                if (ColumnIndexGap >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexGap + 1, SubtitleColumn.Actor);
                    Columns.Insert(ColumnIndexGap + 1, ch);
                }
                else if (ColumnIndexWpm >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexWpm + 1, SubtitleColumn.Actor);
                    Columns.Insert(ColumnIndexWpm + 1, ch);
                }
                else if (ColumnIndexCps >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexCps + 1, SubtitleColumn.Actor);
                    Columns.Insert(ColumnIndexCps + 1, ch);
                }
                else if (ColumnIndexDuration >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexDuration + 1, SubtitleColumn.Actor);
                    Columns.Insert(ColumnIndexDuration + 1, ch);
                }
                else if (ColumnIndexEnd >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexEnd + 1, SubtitleColumn.Actor);
                    Columns.Insert(ColumnIndexEnd + 1, ch);
                }
                else if (ColumnIndexStart >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexStart + 1, SubtitleColumn.Actor);
                    Columns.Insert(ColumnIndexStart + 1, ch);
                }
                else
                {
                    SubtitleColumns.Add(SubtitleColumn.Actor);
                    Columns.Add(ch);
                }
                UpdateColumnIndexes();
                Columns[ColumnIndexActor].Width = 80;
                Columns[ColumnIndexActor].Width = 80;
                AutoSizeAllColumns(null);
            }
            else
            {
                Columns[GetColumnIndex(SubtitleColumn.Actor)].Text = title;
            }
        }

        public void ShowRegionColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.Region) == -1)
            {
                var ch = new ColumnHeader { Text = title };
                if (ColumnIndexActor >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexActor + 1, SubtitleColumn.Region);
                    Columns.Insert(ColumnIndexActor + 1, ch);
                }
                else if (ColumnIndexGap >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexGap + 1, SubtitleColumn.Region);
                    Columns.Insert(ColumnIndexGap + 1, ch);
                }
                else if (ColumnIndexWpm >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexWpm + 1, SubtitleColumn.Region);
                    Columns.Insert(ColumnIndexWpm + 1, ch);
                }
                else if (ColumnIndexCps >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexCps + 1, SubtitleColumn.Region);
                    Columns.Insert(ColumnIndexCps + 1, ch);
                }
                else if (ColumnIndexDuration >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexDuration + 1, SubtitleColumn.Region);
                    Columns.Insert(ColumnIndexDuration + 1, ch);
                }
                else if (ColumnIndexEnd >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexEnd + 1, SubtitleColumn.Region);
                    Columns.Insert(ColumnIndexEnd + 1, ch);
                }
                else if (ColumnIndexStart >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexStart + 1, SubtitleColumn.Region);
                    Columns.Insert(ColumnIndexStart + 1, ch);
                }
                else
                {
                    SubtitleColumns.Add(SubtitleColumn.Region);
                    Columns.Add(ch);
                }
                UpdateColumnIndexes();
                Columns[ColumnIndexRegion].Width = 80;
                Columns[ColumnIndexRegion].Width = 80;
                AutoSizeAllColumns(null);
            }
        }

        public void HideColumn(SubtitleColumn column)
        {
            var idx = GetColumnIndex(column);
            if (idx >= 0)
            {
                SubtitleColumns.RemoveAt(idx);
                UpdateColumnIndexes();
                Columns.RemoveAt(idx);
                AutoSizeAllColumns(null);
            }
        }

        public void SubtitleListViewLastColumnFill(object sender, EventArgs e)
        {
            int width = 0;
            for (int i = 0; i < Columns.Count - 1; i++)
            {
                width += Columns[i].Width;
            }
            if (Columns.Count > 0)
            {
                Columns[Columns.Count - 1].Width = Width - (width + 25);
            }
        }

        public void SaveFirstVisibleIndex()
        {
            if (TopItem != null)
            {
                FirstVisibleIndex = Items.Count > 0 ? TopItem.Index : -1;
            }
        }

        private void RestoreFirstVisibleIndex()
        {
            if (IsValidIndex(FirstVisibleIndex))
            {
                if (FirstVisibleIndex + 1 < Items.Count)
                {
                    FirstVisibleIndex++;
                }

                Items[Items.Count - 1].EnsureVisible();
                Items[FirstVisibleIndex].EnsureVisible();
            }
        }

        internal void Fill(Subtitle subtitle, Subtitle subtitleAlternate = null)
        {
            if (subtitleAlternate == null || subtitleAlternate.Paragraphs.Count == 0)
            {
                Fill(subtitle.Paragraphs);
            }
            else
            {
                Fill(subtitle.Paragraphs, subtitleAlternate.Paragraphs);
            }
        }

        internal void Fill(List<Paragraph> paragraphs)
        {
            SaveFirstVisibleIndex();
            BeginUpdate();
            Items.Clear();
            var x = ListViewItemSorter;
            ListViewItemSorter = null;
            var items = new ListViewItem[paragraphs.Count];
            for (var index = 0; index < paragraphs.Count; index++)
            {
                var paragraph = paragraphs[index];
                Paragraph next = null;
                if (index + 1 < paragraphs.Count)
                {
                    next = paragraphs[index + 1];
                }
                items[index] = MakeListViewItem(paragraph, next, null);
            }

            Items.AddRange(items);

            if (UseSyntaxColoring && _settings != null)
            {
                for (var index = 0; index < paragraphs.Count; index++)
                {
                    var paragraph = paragraphs[index];
                    var item = items[index];
                    SyntaxColorListViewItem(paragraphs, index, paragraph, item);
                }
            }

            ListViewItemSorter = x;
            EndUpdate();

            if (FirstVisibleIndex == 0)
            {
                FirstVisibleIndex = -1;
            }
        }

        internal void Fill(List<Paragraph> paragraphs, List<Paragraph> paragraphsAlternate)
        {
            SaveFirstVisibleIndex();
            BeginUpdate();
            Items.Clear();
            var x = ListViewItemSorter;
            ListViewItemSorter = null;
            var items = new ListViewItem[paragraphs.Count];
            for (var index = 0; index < paragraphs.Count; index++)
            {
                var paragraph = paragraphs[index];
                Paragraph alternate = Utilities.GetOriginalParagraph(index, paragraph, paragraphsAlternate);
                Paragraph next = null;
                if (index + 1 < paragraphs.Count)
                {
                    next = paragraphs[index + 1];
                }
                items[index] = MakeListViewItem(paragraph, next, alternate);
            }

            Items.AddRange(items);

            if (UseSyntaxColoring && _settings != null)
            {
                for (var index = 0; index < paragraphs.Count; index++)
                {
                    var paragraph = paragraphs[index];
                    var item = items[index];
                    SyntaxColorListViewItem(paragraphs, index, paragraph, item);
                }
            }

            ListViewItemSorter = x;
            EndUpdate();

            if (FirstVisibleIndex == 0)
            {
                FirstVisibleIndex = -1;
            }
        }

        public void SyntaxColorAllLines(Subtitle subtitle)
        {
            if (UseSyntaxColoring && _settings != null)
            {
                for (int index = 0; index < subtitle.Paragraphs.Count; index++)
                {
                    var paragraph = subtitle.Paragraphs[index];
                    SyntaxColorListViewItem(subtitle.Paragraphs, index, paragraph, Items[index]);
                }
            }
        }

        public void SyntaxColorLine(List<Paragraph> paragraphs, int i, Paragraph paragraph)
        {
            if (UseSyntaxColoring && _settings != null && IsValidIndex(i))
            {
                var item = Items[i];
                SyntaxColorListViewItem(paragraphs, i, paragraph, item);
            }
        }

        private void SyntaxColorListViewItem(List<Paragraph> paragraphs, int i, Paragraph paragraph, ListViewItem item)
        {
            if (item.UseItemStyleForSubItems)
            {
                item.UseItemStyleForSubItems = false;
                item.SubItems[ColumnIndexDuration].BackColor = BackColor;
            }
            if (ColumnIndexCps >= 0)
            {
                item.SubItems[ColumnIndexCps].BackColor = BackColor;
            }
            if (ColumnIndexWpm >= 0)
            {
                item.SubItems[ColumnIndexWpm].BackColor = paragraph.WordsPerMinute > Configuration.Settings.General.SubtitleMaximumWordsPerMinute ? Configuration.Settings.Tools.ListViewSyntaxErrorColor : BackColor;
            }
            if (ColumnIndexDuration >= 0)
            {
                item.SubItems[ColumnIndexDuration].BackColor = BackColor;
            }

            if (_settings.Tools.ListViewSyntaxColorDurationSmall)
            {
                double charactersPerSecond = Utilities.GetCharactersPerSecond(paragraph);
                if (charactersPerSecond > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                {
                    if (ColumnIndexCps >= 0)
                    {
                        item.SubItems[ColumnIndexCps].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                    }
                    else if (ColumnIndexDuration >= 0)
                    {
                        item.SubItems[ColumnIndexDuration].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                    }
                }
                if (paragraph.Duration.TotalMilliseconds < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds && ColumnIndexDuration >= 0)
                {
                    item.SubItems[ColumnIndexDuration].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                }
            }
            if (_settings.Tools.ListViewSyntaxColorDurationBig &&
                paragraph.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds &&
                ColumnIndexDuration >= 0)
            {
                item.SubItems[ColumnIndexDuration].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
            }

            if (_settings.Tools.ListViewSyntaxColorOverlap && i > 0 && i < paragraphs.Count && ColumnIndexEnd >= 0)
            {
                Paragraph prev = paragraphs[i - 1];
                if (paragraph.StartTime.TotalMilliseconds < prev.EndTime.TotalMilliseconds && !prev.EndTime.IsMaxTime)
                {
                    Items[i - 1].SubItems[ColumnIndexEnd].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                    item.SubItems[ColumnIndexStart].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                }
                else
                {
                    Items[i - 1].SubItems[ColumnIndexEnd].BackColor = BackColor;
                    item.SubItems[ColumnIndexStart].BackColor = BackColor;
                }
            }

            if (_settings.Tools.ListViewSyntaxColorGap && i >= 0 && i < paragraphs.Count - 1 && ColumnIndexGap >= 0)
            {
                Paragraph next = paragraphs[i + 1];
                if (next.StartTime.TotalMilliseconds - paragraph.EndTime.TotalMilliseconds < Configuration.Settings.General.MinimumMillisecondsBetweenLines)
                {
                    item.SubItems[ColumnIndexGap].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                }
                else
                {
                    item.SubItems[ColumnIndexGap].BackColor = BackColor;
                }
            }

            if (ColumnIndexTextAlternate >= 0 && item.SubItems.Count >= ColumnIndexTextAlternate)
            {
                item.SubItems[ColumnIndexTextAlternate].BackColor = BackColor;
            }

            if (ColumnIndexText >= item.SubItems.Count)
            {
                return;
            }

            if (_settings.Tools.ListViewSyntaxColorLongLines)
            {
                string s = HtmlUtil.RemoveHtmlTags(paragraph.Text, true);
                foreach (string line in s.SplitToLines())
                {
                    if (line.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                    {
                        item.SubItems[ColumnIndexText].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                        return;
                    }
                }
                int noOfLines = paragraph.NumberOfLines;
                // Length excluding new line characters. (\r\n)
                int len = noOfLines > 1 ? s.Length - Environment.NewLine.Length * (noOfLines - 1) : s.Length;
                if (len <= Configuration.Settings.General.SubtitleLineMaximumLength * noOfLines)
                {
                    if (noOfLines > Configuration.Settings.General.MaxNumberOfLines && _settings.Tools.ListViewSyntaxMoreThanXLines)
                    {
                        item.SubItems[ColumnIndexText].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                    }
                    else
                    {
                        item.SubItems[ColumnIndexText].BackColor = BackColor;
                    }
                }
                else
                {
                    item.SubItems[ColumnIndexText].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                }
            }
            if (_settings.Tools.ListViewSyntaxMoreThanXLines &&
                item.SubItems[ColumnIndexText].BackColor != Configuration.Settings.Tools.ListViewSyntaxErrorColor)
            {
                if (paragraph.NumberOfLines > Configuration.Settings.General.MaxNumberOfLines)
                {
                    item.SubItems[ColumnIndexText].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                }
            }
        }

        private string GetDisplayTime(TimeCode timeCode)
        {
            if (Configuration.Settings.General.CurrentVideoOffsetInMs != 0)
            {
                return new TimeCode(timeCode.TotalMilliseconds + Configuration.Settings.General.CurrentVideoOffsetInMs).ToDisplayString();
            }

            return timeCode.ToDisplayString();
        }

        private ListViewItem MakeListViewItem(Paragraph paragraph, Paragraph next, Paragraph paragraphAlternate)
        {
            var item = new ListViewItem(paragraph.Number.ToString(CultureInfo.InvariantCulture)) { Tag = paragraph, UseItemStyleForSubItems = false };
            foreach (var column in SubtitleColumns)
            {
                switch (column)
                {
                    case SubtitleColumn.Start:
                        item.SubItems.Add(GetDisplayTime(paragraph.StartTime));
                        break;
                    case SubtitleColumn.End:
                        item.SubItems.Add(GetDisplayTime(paragraph.EndTime));
                        break;
                    case SubtitleColumn.Duration:
                        item.SubItems.Add(paragraph.Duration.ToShortDisplayString());
                        break;
                    case SubtitleColumn.CharactersPerSeconds:
                        item.SubItems.Add($"{Utilities.GetCharactersPerSecond(paragraph):0.00}");
                        break;
                    case SubtitleColumn.WordsPerMinute:
                        item.SubItems.Add($"{paragraph.WordsPerMinute:0.00}");
                        break;
                    case SubtitleColumn.Gap:
                        item.SubItems.Add(GetGap(paragraph, next));
                        break;
                    case SubtitleColumn.Actor:
                        item.SubItems.Add(paragraph.Actor);
                        break;
                    case SubtitleColumn.Region:
                        item.SubItems.Add(paragraph.Region);
                        break;
                    case SubtitleColumn.Text:
                        item.SubItems.Add(paragraph.Text.Replace(Environment.NewLine, _lineSeparatorString));
                        break;
                    case SubtitleColumn.TextAlternate:
                        var text = paragraphAlternate != null ? paragraphAlternate.Text : string.Empty;
                        item.SubItems.Add(text.Replace(Environment.NewLine, _lineSeparatorString));
                        break;
                    case SubtitleColumn.Extra:
                        item.SubItems.Add(paragraph.Extra);
                        break;
                    case SubtitleColumn.Network:
                        item.SubItems.Add(string.Empty);
                        break;
                }
            }

            item.StateImageIndex = paragraph.Bookmark != null ? 0 : -1;
            item.Font = new Font(_subtitleFontName, SubtitleFontSize, GetFontStyle());
            return item;
        }

        public void SelectNone()
        {
            for (var i = Items.Count - 1; i >= 0; i--)
            {
                Items[i].Selected = false;
            }
        }

        public void SelectIndexAndEnsureVisibleFaster(int index)
        {
            var topItem = TopItem;
            if (!IsValidIndex(index) || topItem == null)
            {
                return;
            }

            BeginUpdate();
            var selectedIndices = new List<int>(SelectedIndices.Count);
            foreach (int selectedIndex in SelectedIndices)
            {
                selectedIndices.Add(selectedIndex);
            }
            foreach (int selectedIndex in selectedIndices)
            {
                Items[selectedIndex].Selected = false;
            }

            var selectedItem = Items[index];
            selectedItem.Selected = true;
            selectedItem.Focused = true;

            var topIndex = topItem.Index;
            var numberOfVisibleItems = (Height - 30) / GetItemRect(0).Height;
            int bottomIndex = topIndex + numberOfVisibleItems;
            if (index >= bottomIndex)
            {
                Items[Math.Min(Items.Count - 1, index + numberOfVisibleItems / 2)].EnsureVisible();
            }
            else if (index < topIndex)
            {
                Items[Math.Max(0, index - numberOfVisibleItems / 2)].EnsureVisible();
            }
            EndUpdate();
        }

        public void SelectIndexAndEnsureVisible(int index, bool focus)
        {
            if (!IsValidIndex(index))
            {
                return;
            }

            if (TopItem == null)
            {
                EnsureVisible(index);
                if (focus)
                {
                    Items[index].Focused = true;
                }
                return;
            }

            int bottomIndex = TopItem.Index + (Height - 25) / 16;
            int itemsBeforeAfterCount = (bottomIndex - TopItem.Index) / 2 - 1;
            if (itemsBeforeAfterCount < 0)
            {
                itemsBeforeAfterCount = 1;
            }

            int beforeIndex = index - itemsBeforeAfterCount;
            if (beforeIndex < 0)
            {
                beforeIndex = 0;
            }

            int afterIndex = index + itemsBeforeAfterCount;
            if (afterIndex >= Items.Count)
            {
                afterIndex = Items.Count - 1;
            }

            SelectNone();
            if (TopItem.Index <= beforeIndex && bottomIndex > afterIndex)
            {
                Items[index].Selected = true;
                Items[index].EnsureVisible();
                if (focus)
                {
                    Items[index].Focused = true;
                }
                EnsureVisible(index);
                return;
            }

            Items[beforeIndex].EnsureVisible();
            EnsureVisible(beforeIndex);
            Items[afterIndex].EnsureVisible();
            EnsureVisible(afterIndex);
            Items[index].Selected = true;
            Items[index].EnsureVisible();
            if (focus)
            {
                Items[index].Focused = true;
            }
            EnsureVisible(index);
        }

        public void SelectIndexAndEnsureVisible(int index)
        {
            SelectIndexAndEnsureVisible(index, false);
        }

        public void SelectIndexAndEnsureVisible(Paragraph p)
        {
            SelectNone();
            if (p == null)
            {
                return;
            }

            for (int index = 0; index < Items.Count; index++)
            {
                ListViewItem item = Items[index];
                if (item.Tag as Paragraph == p ||
                    item.Text == p.Number.ToString(CultureInfo.InvariantCulture) &&
                    (ColumnIndexStart < 0 || item.SubItems[ColumnIndexStart].Text == GetDisplayTime(p.StartTime)) &&
                    (ColumnIndexEnd < 0 || item.SubItems[ColumnIndexEnd].Text == GetDisplayTime(p.EndTime)) &&
                    item.SubItems[ColumnIndexText].Text == p.Text)
                {
                    RestoreFirstVisibleIndex();
                    item.Selected = true;
                    item.EnsureVisible();
                    return;
                }
            }
        }

        public Paragraph GetSelectedParagraph(Subtitle subtitle)
        {
            if (subtitle != null && SelectedItems.Count > 0)
            {
                return subtitle.GetParagraphOrDefault(SelectedItems[0].Index);
            }

            return null;
        }

        public string GetText(int index)
        {
            if (IsValidIndex(index))
            {
                return Items[index].SubItems[ColumnIndexText].Text.Replace(_lineSeparatorString, Environment.NewLine);
            }

            return null;
        }

        public string GetTextAlternate(int index)
        {
            if (IsValidIndex(index) && ColumnIndexTextAlternate >= 0)
            {
                return Items[index].SubItems[ColumnIndexTextAlternate].Text.Replace(_lineSeparatorString, Environment.NewLine);
            }

            return null;
        }

        public void SetText(int index, string text)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                if (ColumnIndexText >= 0)
                {
                    item.SubItems[ColumnIndexText].Text = text.Replace(Environment.NewLine, _lineSeparatorString);
                }

                if (item.Tag is Paragraph paragraph)
                {
                    UpdateCpsAndWpm(item, paragraph);
                }
            }
        }

        public void SetTimeAndText(int index, Paragraph paragraph, Paragraph next)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                if (ColumnIndexStart >= 0)
                {
                    item.SubItems[ColumnIndexStart].Text = GetDisplayTime(paragraph.StartTime);
                }

                if (ColumnIndexEnd >= 0)
                {
                    item.SubItems[ColumnIndexEnd].Text = GetDisplayTime(paragraph.EndTime);
                }

                if (ColumnIndexDuration >= 0)
                {
                    item.SubItems[ColumnIndexDuration].Text = paragraph.Duration.ToShortDisplayString();
                }

                if (ColumnIndexGap >= 0)
                {
                    item.SubItems[ColumnIndexGap].Text = GetGap(paragraph, next);
                }

                if (ColumnIndexActor >= 0)
                {
                    item.SubItems[ColumnIndexActor].Text = paragraph.Actor;
                }

                if (ColumnIndexRegion >= 0)
                {
                    item.SubItems[ColumnIndexRegion].Text = paragraph.Region;
                }

                if (ColumnIndexText >= 0)
                {
                    item.SubItems[ColumnIndexText].Text = paragraph.Text.Replace(Environment.NewLine, _lineSeparatorString);
                }

                UpdateCpsAndWpm(item, paragraph);
            }
        }

        private void UpdateCpsAndWpm(ListViewItem item, Paragraph paragraph)
        {
            if (ColumnIndexCps >= 0)
            {
                item.SubItems[ColumnIndexCps].Text = $"{Utilities.GetCharactersPerSecond(paragraph):0.00}";
            }
            if (ColumnIndexWpm >= 0)
            {
                item.SubItems[ColumnIndexWpm].Text = $"{paragraph.WordsPerMinute:0.00}";
            }
        }

        public void SetExtraText(int index, string text, Color color)
        {
            if (IsValidIndex(index))
            {
                if (GetColumnIndex(SubtitleColumn.Extra) == -1)
                {
                    ShowExtraColumn(string.Empty);
                }
                while (ColumnIndexExtra >= Items[index].SubItems.Count)
                {
                    Items[index].SubItems.Add(string.Empty);
                }

                if (ColumnIndexExtra >= 0)
                {
                    Items[index].SubItems[ColumnIndexExtra].Text = text;
                    Items[index].UseItemStyleForSubItems = false;
                    Items[index].SubItems[ColumnIndexExtra].BackColor = BackColor;
                    Items[index].SubItems[ColumnIndexExtra].ForeColor = color;
                }
            }
        }

        public void SetNetworkText(int index, string text, Color color)
        {
            if (IsValidIndex(index))
            {
                if (GetColumnIndex(SubtitleColumn.Network) == -1)
                {
                    ShowNetworkColumn(string.Empty);
                }
                while (ColumnIndexNetwork >= Items[index].SubItems.Count)
                {
                    Items[index].SubItems.Add(string.Empty);
                }

                if (ColumnIndexNetwork >= 0)
                {
                    Items[index].SubItems[ColumnIndexNetwork].Text = text;
                    Items[index].UseItemStyleForSubItems = false;
                    Items[index].SubItems[ColumnIndexNetwork].BackColor = Color.AntiqueWhite;
                    Items[index].SubItems[ColumnIndexNetwork].ForeColor = color;
                }
            }
        }

        public void SetAlternateText(int index, string text)
        {
            if (IsValidIndex(index) && Columns.Count >= ColumnIndexTextAlternate + 1)
            {
                if (GetColumnIndex(SubtitleColumn.TextAlternate) == -1)
                {
                    ShowAlternateTextColumn(string.Empty);
                }
                while (ColumnIndexTextAlternate >= Items[index].SubItems.Count)
                {
                    Items[index].SubItems.Add(string.Empty);
                }

                if (ColumnIndexTextAlternate >= 0)
                {
                    Items[index].SubItems[ColumnIndexTextAlternate].Text = text.Replace(Environment.NewLine, _lineSeparatorString);
                    Items[index].UseItemStyleForSubItems = false;
                    Items[index].SubItems[ColumnIndexTextAlternate].BackColor = BackColor;
                }
            }
        }

        public void SetDuration(int index, Paragraph paragraph, Paragraph next)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                if (ColumnIndexEnd >= 0)
                {
                    item.SubItems[ColumnIndexEnd].Text = GetDisplayTime(paragraph.EndTime);
                }

                if (ColumnIndexDuration >= 0)
                {
                    item.SubItems[ColumnIndexDuration].Text = paragraph.Duration.ToShortDisplayString();
                }

                if (ColumnIndexGap >= 0)
                {
                    item.SubItems[ColumnIndexGap].Text = GetGap(paragraph, next);
                }

                UpdateCpsAndWpm(item, paragraph);
            }
        }

        public void SetNumber(int index, string number)
        {
            if (IsValidIndex(index) && ColumnIndexNumber >= 0)
            {
                Items[index].SubItems[ColumnIndexNumber].Text = number;
            }
        }

        public void UpdateFrames(Subtitle subtitle)
        {
            if (Configuration.Settings?.General.UseTimeFormatHHMMSSFF == true)
            {
                BeginUpdate();
                for (int i = 0; i < subtitle.Paragraphs.Count; i++)
                {
                    if (IsValidIndex(i))
                    {
                        Paragraph p = subtitle.Paragraphs[i];
                        ListViewItem item = Items[i];
                        if (ColumnIndexStart >= 0)
                        {
                            item.SubItems[ColumnIndexStart].Text = GetDisplayTime(p.StartTime);
                        }

                        if (ColumnIndexEnd >= 0)
                        {
                            item.SubItems[ColumnIndexEnd].Text = GetDisplayTime(p.EndTime);
                        }

                        if (ColumnIndexDuration >= 0)
                        {
                            item.SubItems[ColumnIndexDuration].Text = p.Duration.ToShortDisplayString();
                        }
                    }
                }
                EndUpdate();
            }
        }

        public void SetStartTimeAndDuration(int index, Paragraph paragraph, Paragraph next, Paragraph prev)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                if (ColumnIndexStart >= 0)
                {
                    item.SubItems[ColumnIndexStart].Text = GetDisplayTime(paragraph.StartTime);
                }

                if (ColumnIndexEnd >= 0)
                {
                    item.SubItems[ColumnIndexEnd].Text = GetDisplayTime(paragraph.EndTime);
                }

                if (ColumnIndexDuration >= 0)
                {
                    item.SubItems[ColumnIndexDuration].Text = paragraph.Duration.ToShortDisplayString();
                }

                if (ColumnIndexGap >= 0)
                {
                    item.SubItems[ColumnIndexGap].Text = GetGap(paragraph, next);
                }

                UpdateCpsAndWpm(item, paragraph);
            }
            SetGap(index - 1, prev, paragraph);
        }

        private void SetGap(int index, Paragraph paragraph, Paragraph next)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                if (ColumnIndexGap >= 0)
                {
                    item.SubItems[ColumnIndexGap].Text = GetGap(paragraph, next);
                }
            }
        }

        private string GetGap(Paragraph paragraph, Paragraph next)
        {
            if (next == null || paragraph == null || next.StartTime.IsMaxTime || paragraph.EndTime.IsMaxTime)
            {
                return string.Empty;
            }

            return new TimeCode(next.StartTime.TotalMilliseconds - paragraph.EndTime.TotalMilliseconds).ToShortDisplayString();
        }

        public void SetBackgroundColor(int index, Color color, int columnNumber)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                if (item.UseItemStyleForSubItems)
                {
                    item.UseItemStyleForSubItems = false;
                }

                if (columnNumber >= 0 && columnNumber < item.SubItems.Count)
                {
                    item.SubItems[columnNumber].BackColor = color;
                }
            }
        }

        public void SetBackgroundColor(int index, Color color)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                item.BackColor = color;
                if (ColumnIndexStart >= 0)
                {
                    Items[index].SubItems[ColumnIndexStart].BackColor = color;
                }

                if (ColumnIndexEnd >= 0)
                {
                    Items[index].SubItems[ColumnIndexEnd].BackColor = color;
                }

                if (ColumnIndexDuration >= 0)
                {
                    Items[index].SubItems[ColumnIndexDuration].BackColor = color;
                }

                if (ColumnIndexCps >= 0)
                {
                    Items[index].SubItems[ColumnIndexCps].BackColor = color;
                }

                if (ColumnIndexWpm >= 0)
                {
                    Items[index].SubItems[ColumnIndexWpm].BackColor = color;
                }

                if (ColumnIndexGap >= 0)
                {
                    Items[index].SubItems[ColumnIndexGap].BackColor = color;
                }

                if (ColumnIndexText >= 0)
                {
                    Items[index].SubItems[ColumnIndexText].BackColor = color;
                }

                if (ColumnIndexTextAlternate >= 0)
                {
                    Items[index].SubItems[ColumnIndexTextAlternate].BackColor = color;
                }
            }
        }

        public Color GetBackgroundColor(int index)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                return item.BackColor;
            }
            return DefaultBackColor;
        }

        /// <summary>
        /// Removes all text and set background color
        /// </summary>
        /// <param name="index"></param>
        /// <param name="color"></param>
        public void ColorOut(int index, Color color)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                item.Text = string.Empty;
                if (ColumnIndexStart >= 0)
                {
                    item.SubItems[ColumnIndexStart].Text = string.Empty;
                }

                if (ColumnIndexEnd >= 0)
                {
                    item.SubItems[ColumnIndexEnd].Text = string.Empty;
                }

                if (ColumnIndexDuration >= 0)
                {
                    item.SubItems[ColumnIndexDuration].Text = string.Empty;
                }

                if (ColumnIndexText >= 0)
                {
                    item.SubItems[ColumnIndexText].Text = string.Empty;
                }

                if (ColumnIndexGap >= 0)
                {
                    item.SubItems[ColumnIndexGap].Text = string.Empty;
                }

                SetBackgroundColor(index, color);
            }
        }

        public void HideNonVobSubColumns()
        {
            var numberIdx = GetColumnIndex(SubtitleColumn.Number);
            if (numberIdx >= 0)
            {
                Columns[numberIdx].Width = 0;
            }

            HideColumn(SubtitleColumn.End);
            HideColumn(SubtitleColumn.Duration);
            HideColumn(SubtitleColumn.CharactersPerSeconds);
            HideColumn(SubtitleColumn.WordsPerMinute);
        }

        public void SetCustomResize(EventHandler handler)
        {
            if (handler == null)
            {
                return;
            }

            Resize -= SubtitleListViewLastColumnFill;
            Resize += handler;
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < Items.Count;
        }

        private FontStyle GetFontStyle() => SubtitleFontBold ? FontStyle.Bold : FontStyle.Regular;

        public void ShowState(int index, Paragraph paragraph)
        {
            if (IsValidIndex(index))
            {
                Items[index].StateImageIndex = paragraph.Bookmark != null ? 0 : -1;
            }
        }
    }
}

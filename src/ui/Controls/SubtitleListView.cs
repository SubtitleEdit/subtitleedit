﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Settings;

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
            TextOriginal,
            Extra,
            Network
        }

        private List<SubtitleColumn> SubtitleColumns { get; }

        public int GetColumnIndex(SubtitleColumn column)
        {
            return SubtitleColumns.IndexOf(column);
        }

        public const int InvalidIndex = -1;
        public int SelectedIndex => SelectedIndices.Count == 1 ? SelectedIndices[0] : InvalidIndex;

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
        public int ColumnIndexTextOriginal { get; private set; }
        public int ColumnIndexExtra { get; private set; }
        public int ColumnIndexNetwork { get; private set; }

        public bool IsOriginalTextColumnVisible => ColumnIndexTextOriginal >= 0;
        private string _lineSeparatorString = " || ";

        public string SubtitleFontName { get; set; } = "Tahoma";
        public bool SubtitleFontBold { get; set; }
        public int SubtitleFontSize { get; set; } = 8;

        public bool UseSyntaxColoring { get; set; }
        private Settings _settings;
        private bool _saveColumnWidthChanges;
        private readonly Timer _setLastColumnWidthTimer;

        public class SyntaxColorLineParameter
        {
            public List<Paragraph> Paragraphs { get; set; }
            public int Index { get; set; }
            public Paragraph Paragraph { get; set; }
        }

        public class SetStartAndDurationParameter
        {
            public int Index { get; set; }
            public Paragraph Paragraph { get; set; }
            public Paragraph Next { get; set; }
            public Paragraph Prev { get; set; }
        }

        private readonly List<SyntaxColorLineParameter> _syntaxColorList = new List<SyntaxColorLineParameter>();
        private readonly List<SetStartAndDurationParameter> _setStartAndDurationList = new List<SetStartAndDurationParameter>();
        private static readonly object SyntaxColorListLock = new object();
        private static readonly object SetStartTimeAndDurationLock = new object();
        private readonly Timer _syntaxColorLineTimer;
        private readonly Timer _setStartAndDurationTimer;

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
                SubtitleFontName = settings.General.SubtitleFontName;
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
            ColumnIndexTextOriginal = -1;
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

            if (Configuration.Settings != null && !Configuration.Settings.Tools.ListViewShowColumnStartTime)
            {
                HideColumn(SubtitleColumn.Start);
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
                ShowCharsSecColumn(LanguageSettings.Current.General.CharsPerSec);
            }

            if (Configuration.Settings != null && Configuration.Settings.Tools.ListViewShowColumnWordsPerMin)
            {
                ShowWordsMinColumn(LanguageSettings.Current.General.WordsPerMin);
            }

            if (Configuration.Settings != null && Configuration.Settings.Tools.ListViewShowColumnGap)
            {
                ShowGapColumn(LanguageSettings.Current.General.Gap);
            }

            _syntaxColorLineTimer = new Timer { Interval = 41 };
            _syntaxColorLineTimer.Tick += SyntaxColorLineTimerTick;

            _setStartAndDurationTimer = new Timer { Interval = 3 };
            _setStartAndDurationTimer.Tick += SetStartAndDurationTimerTick;

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

            _setLastColumnWidthTimer = new Timer { Interval = 50 };
            _setLastColumnWidthTimer.Tick += SetLastColumnWidthTimer_Tick;
        }

        private void SetLastColumnWidthTimer_Tick(object sender, EventArgs e)
        {
            _setLastColumnWidthTimer.Stop();
            if (Columns.Count > 0)
            {
                var width = 0;
                for (var i = 0; i < Columns.Count - 1; i++)
                {
                    width += Columns[i].Width;
                }
                Columns[Columns.Count - 1].Width = ClientSize.Width - width;
            }
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
            ColumnIndexTextOriginal = GetColumnIndex(SubtitleColumn.TextOriginal);
            ColumnIndexExtra = GetColumnIndex(SubtitleColumn.Extra);
            ColumnIndexNetwork = GetColumnIndex(SubtitleColumn.Network);
        }

        private static void SubtitleListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void SubtitleListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var rtl = Configuration.Settings?.General.RightToLeftMode == true;
            if (rtl)
            {
                e.DrawDefault = true;
                return;
            }

            var backgroundColor = Items[e.ItemIndex].SubItems[e.ColumnIndex].BackColor;
            var hasCustomColor = backgroundColor != BackColor;
            var foreColor = UiUtil.ForeColor;
            if (e.Item.Selected && !(Focused && e.ColumnIndex > 0) || Focused && hasCustomColor)
            {
                var rect = e.Bounds;
                if (Configuration.Settings != null)
                {
                    if (hasCustomColor)
                    {
                        if (e.Item.Selected)
                        {
                            backgroundColor = GetCustomColor(backgroundColor);
                        }
                    }
                    else if (Configuration.Settings.General.UseDarkTheme)
                    {
                        backgroundColor = Color.FromArgb(24, 52, 75);
                    }
                    else if (Focused)
                    {
                        backgroundColor = Color.FromArgb(0, 120, 215);
                        foreColor = Color.White;
                    }
                    else
                    {
                        backgroundColor = Color.FromArgb(204, 232, 255);
                    }

                    using (var sb = new SolidBrush(backgroundColor))
                    {
                        e.Graphics.FillRectangle(sb, rect);
                    }
                }
                else
                {
                    e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                }

                var addX = 0;
                if (e.ColumnIndex == 0 && StateImageList?.Images.Count > 0)
                {
                    addX = 20;
                }

                if (e.ColumnIndex == 0 && e.Item.StateImageIndex >= 0 && StateImageList?.Images.Count > e.Item.StateImageIndex)
                {
                    var r = rtl 
                        ? new Rectangle( rect.Width - 21, rect.Y + 3, 16, 16) 
                        : new Rectangle(rect.X + 4, rect.Y + 3, 16, 16);

                    e.Graphics.DrawImage(StateImageList.Images[e.Item.StateImageIndex], r);
                }

                using (var f = new Font(e.Item.SubItems[e.ColumnIndex].Font.FontFamily, e.Item.SubItems[e.ColumnIndex].Font.Size - 0.4f, e.Item.SubItems[e.ColumnIndex].Font.Style))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.Left | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPrefix;
                    if (Columns[e.ColumnIndex].TextAlign == HorizontalAlignment.Right)
                    {
                        flags |= TextFormatFlags.Right;
                    }
                    else
                    {
                        flags |= TextFormatFlags.Left;
                    }

                    if (RightToLeftLayout)
                    {
                        flags |= TextFormatFlags.RightToLeft;
                    }

                    var r = new Rectangle(rect.Left + 2 + addX, rect.Top + 2, rect.Width - 7 - addX, rect.Height - 2);
                    TextRenderer.DrawText(e.Graphics, e.Item.SubItems[e.ColumnIndex].Text, f, r, foreColor, flags);
                }
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        internal static StringFormat CreateStringFormat(Control control)
        {
            var stringFormat = new StringFormat
            {
                FormatFlags = 0,
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                HotkeyPrefix = HotkeyPrefix.None,
                Trimming = StringTrimming.None
            };
            stringFormat.Alignment = StringAlignment.Near;
            stringFormat.Trimming = StringTrimming.EllipsisCharacter;
            stringFormat.FormatFlags |= StringFormatFlags.LineLimit;
            if (control.RightToLeft == RightToLeft.Yes)
            {
                stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
            }

            if (control.AutoSize)
            {
                stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            }

            return stringFormat;
        }

        private static Color GetCustomColor(Color color)
        {
            var r = Math.Max(color.R - 39, 0);
            var g = Math.Max(color.G - 39, 0);
            var b = Math.Max(color.B - 39, 0);
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
            else if (Focused)
            {
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

            var extraIdx = GetColumnIndex(SubtitleColumn.Extra);
            if (extraIdx >= 0)
            {
                Columns[extraIdx].Width = 120;
                Columns[extraIdx].Width = 120;
            }

            int w = 0;
            for (int index = 0; index < SubtitleColumns.Count; index++)
            {
                var column = SubtitleColumns[index];
                int cw = Columns[index].Width;
                if (column != SubtitleColumn.Text && column != SubtitleColumn.TextOriginal)
                {
                    w += cw;
                }
            }
            int lengthAvailable = Width - w;
            if (ColumnIndexTextOriginal >= 0)
            {
                lengthAvailable /= 2;
                Columns[ColumnIndexTextOriginal].Width = lengthAvailable;
                Columns[ColumnIndexTextOriginal].Width = lengthAvailable;
                Columns[ColumnIndexTextOriginal].Width = lengthAvailable;
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
                if (column != SubtitleColumn.Text && column != SubtitleColumn.TextOriginal)
                {
                    w += cw;
                }
            }

            int lengthAvailable = Width - w;
            if (ColumnIndexTextOriginal >= 0 && Columns.Count > ColumnIndexTextOriginal)
            {
                lengthAvailable /= 2;
                Columns[ColumnIndexTextOriginal].Width = lengthAvailable;
                Columns[ColumnIndexTextOriginal].Width = lengthAvailable;
                Columns[ColumnIndexTextOriginal].Width = lengthAvailable;
            }
            Columns[ColumnIndexText].Width = lengthAvailable;
            Columns[ColumnIndexText].Width = lengthAvailable;
            Columns[ColumnIndexText].Width = lengthAvailable;
            SubtitleListViewLastColumnFill(this, null);
        }

        public void ShowStartColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.Start) == -1)
            {
                var ch = new ColumnHeader { Text = title };
                if (ColumnIndexNumber >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexNumber + 1, SubtitleColumn.Start);
                    Columns.Insert(ColumnIndexNumber + 1, ch);
                }
                else
                {
                    SubtitleColumns.Add(SubtitleColumn.Start);
                    Columns.Insert(0, ch);
                }
                UpdateColumnIndexes();

                try
                {
                    using (var graphics = Parent.CreateGraphics())
                    {
                        var timestampSizeF = graphics.MeasureString(new TimeCode(0, 0, 33, 527).ToDisplayString(), Font);
                        var timestampWidth = (int)(timestampSizeF.Width + 0.5) + 11;
                        Columns[ColumnIndexStart].Width = timestampWidth;
                    }
                }
                catch
                {
                    Columns[ColumnIndexStart].Width = 65;
                }

                AutoSizeAllColumns(null);
            }
        }

        public void ShowEndColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.End) == -1)
            {
                var ch = new ColumnHeader { Text = title };
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
                var ch = new ColumnHeader { Text = title };
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

        public void ShowOriginalTextColumn(string title)
        {
            if (GetColumnIndex(SubtitleColumn.TextOriginal) == -1)
            {
                if (ColumnIndexText >= 0)
                {
                    SubtitleColumns.Insert(ColumnIndexText + 1, SubtitleColumn.TextOriginal);
                    Columns.Insert(ColumnIndexText + 1, new ColumnHeader { Text = title });
                }
                else
                {
                    SubtitleColumns.Add(SubtitleColumn.TextOriginal);
                    Columns.Add(new ColumnHeader { Text = title });
                }
                UpdateColumnIndexes();
                Columns[ColumnIndexTextOriginal].Width = 300;
                Columns[ColumnIndexTextOriginal].Width = 300;
                Columns[ColumnIndexTextOriginal].Width = 300;
                AutoSizeAllColumns(null);
            }
            else
            {
                Columns[ColumnIndexTextOriginal].Text = title;
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
                var ch = new ColumnHeader { Text = title };
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
                var ch = new ColumnHeader { Text = title };
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
                foreach (ListViewItem lvi in Items)
                {
                    if (lvi.SubItems.Count - 1 > idx && lvi.SubItems[idx] != null)
                    {
                        lvi.SubItems.RemoveAt(idx);
                    }
                }
                AutoSizeAllColumns(null);
            }
        }

        public void SubtitleListViewLastColumnFill(object sender, EventArgs e)
        {
            if (DesignMode || _setLastColumnWidthTimer == null || !Visible)
            {
                return;
            }

            _setLastColumnWidthTimer.Stop();
            _setLastColumnWidthTimer.Start();
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

        internal void Fill(Subtitle subtitle, Subtitle subtitleOriginal = null)
        {
            if (subtitleOriginal == null || subtitleOriginal.Paragraphs.Count == 0)
            {
                Fill(subtitle.Paragraphs);
            }
            else
            {
                Fill(subtitle.Paragraphs, subtitleOriginal.Paragraphs);
            }
        }

        internal void Fill(List<Paragraph> paragraphs)
        {
            SaveFirstVisibleIndex();
            BeginUpdate();
            Items.Clear();
            var x = ListViewItemSorter;
            ListViewItemSorter = null;
            var font = new Font(SubtitleFontName, SubtitleFontSize, GetFontStyle());
            var items = new ListViewItem[paragraphs.Count];
            for (var index = 0; index < paragraphs.Count; index++)
            {
                var paragraph = paragraphs[index];
                Paragraph next = null;
                if (index + 1 < paragraphs.Count)
                {
                    next = paragraphs[index + 1];
                }
                items[index] = MakeListViewItem(paragraph, next, null, font);
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

        internal void Fill(List<Paragraph> paragraphs, List<Paragraph> paragraphsOriginal)
        {
            SaveFirstVisibleIndex();
            BeginUpdate();
            Items.Clear();
            var x = ListViewItemSorter;
            ListViewItemSorter = null;
            var items = new ListViewItem[paragraphs.Count];
            var font = new Font(SubtitleFontName, SubtitleFontSize, GetFontStyle());
            for (var index = 0; index < paragraphs.Count; index++)
            {
                var paragraph = paragraphs[index];
                Paragraph original = Utilities.GetOriginalParagraph(index, paragraph, paragraphsOriginal);
                Paragraph next = null;
                if (index + 1 < paragraphs.Count)
                {
                    next = paragraphs[index + 1];
                }
                items[index] = MakeListViewItem(paragraph, next, original, font);
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

        private void SyntaxColorLineTimerTick(object sender, EventArgs e)
        {
            var hashSet = new HashSet<int>();
            lock (SyntaxColorListLock)
            {
                _syntaxColorLineTimer.Stop();

                for (int i = _syntaxColorList.Count - 1; i >= 0; i--)
                {
                    var item = _syntaxColorList[i];
                    if (!hashSet.Contains(item.Index))
                    {
                        if (IsValidIndex(item.Index))
                        {
                            SyntaxColorListViewItem(item.Paragraphs, item.Index, item.Paragraph, Items[item.Index]);
                        }
                        hashSet.Add(item.Index);
                    }
                }
                _syntaxColorList.Clear();
            }
        }

        private void SetStartAndDurationTimerTick(object sender, EventArgs e)
        {
            var hashSet = new HashSet<int>();
            lock (SetStartTimeAndDurationLock)
            {
                _setStartAndDurationTimer.Stop();
                for (int i = _setStartAndDurationList.Count - 1; i >= 0; i--)
                {
                    var item = _setStartAndDurationList[i];
                    if (!hashSet.Contains(item.Index))
                    {
                        if (IsValidIndex(item.Index))
                        {
                            SetStartTimeAndDuration(item.Index, item.Paragraph, item.Next, item.Prev);
                        }
                        hashSet.Add(item.Index);
                    }
                }
                _setStartAndDurationList.Clear();
            }
        }

        /// <summary>
        /// Can handle multiple events to same line - but not line adding/splitting.
        /// </summary>
        public void SyntaxColorLineBackground(List<Paragraph> paragraphs, int i, Paragraph paragraph)
        {
            if (!UseSyntaxColoring || _settings == null)
            {
                return;
            }

            lock (SyntaxColorListLock)
            {
                _syntaxColorLineTimer.Stop();
                _syntaxColorList.Add(new SyntaxColorLineParameter { Index = i, Paragraphs = paragraphs, Paragraph = paragraph });
                _syntaxColorLineTimer.Start();
            }
        }

        /// <summary>
        /// Can handle multiple events to same line - but not line adding/splitting.
        /// </summary>
        public void SetStartTimeAndDurationBackground(int index, Paragraph paragraph, Paragraph next, Paragraph prev)
        {
            if (_settings == null)
            {
                return;
            }

            lock (SetStartTimeAndDurationLock)
            {
                _setStartAndDurationTimer.Stop();
                _setStartAndDurationList.Add(new SetStartAndDurationParameter { Index = index, Paragraph = paragraph, Next = next, Prev = prev });
                _setStartAndDurationTimer.Start();
            }
        }

        public void SyntaxColorLine(List<Paragraph> paragraphs, int i, Paragraph paragraph)
        {
            if (UseSyntaxColoring && _settings != null && IsValidIndex(i))
            {
                SyntaxColorListViewItem(paragraphs, i, paragraph, Items[i]);
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

            if (_settings.Tools.ListViewSyntaxColorDurationSmall && !paragraph.StartTime.IsMaxTime)
            {
                double charactersPerSecond = paragraph.GetCharactersPerSecond();
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
                if (paragraph.DurationTotalMilliseconds < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds && ColumnIndexDuration >= 0)
                {
                    item.SubItems[ColumnIndexDuration].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                }
            }
            if (_settings.Tools.ListViewSyntaxColorDurationBig &&
                paragraph.DurationTotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds &&
                ColumnIndexDuration >= 0)
            {
                item.SubItems[ColumnIndexDuration].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
            }

            if (_settings.Tools.ListViewSyntaxColorOverlap && i > 0 && i < paragraphs.Count && ColumnIndexEnd >= 0)
            {
                var prev = paragraphs[i - 1];
                if (paragraph.StartTime.TotalMilliseconds < prev.EndTime.TotalMilliseconds && !prev.EndTime.IsMaxTime)
                {
                    if (ColumnIndexEnd >= 0)
                    {
                        Items[i - 1].SubItems[ColumnIndexEnd].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                    }

                    if (ColumnIndexStart >= 0)
                    {
                        item.SubItems[ColumnIndexStart].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                    }
                }
                else
                {
                    if (ColumnIndexEnd >= 0)
                    {
                        Items[i - 1].SubItems[ColumnIndexEnd].BackColor = BackColor;
                    }

                    if (ColumnIndexStart >= 0)
                    {
                        item.SubItems[ColumnIndexStart].BackColor = BackColor;
                    }
                }
            }

            if (_settings.Tools.ListViewSyntaxColorGap && i >= 0 && i < paragraphs.Count - 1 && ColumnIndexGap >= 0 && !paragraph.StartTime.IsMaxTime)
            {
                var next = paragraphs[i + 1];
                var gapMilliseconds = (int)Math.Round(next.StartTime.TotalMilliseconds - paragraph.EndTime.TotalMilliseconds);
                item.SubItems[ColumnIndexGap].BackColor = gapMilliseconds < Configuration.Settings.General.MinimumMillisecondsBetweenLines ? Configuration.Settings.Tools.ListViewSyntaxErrorColor : BackColor;
            }

            if (ColumnIndexTextOriginal >= 0 && item.SubItems.Count >= ColumnIndexTextOriginal)
            {
                item.SubItems[ColumnIndexTextOriginal].BackColor = BackColor;
            }

            if (ColumnIndexText >= item.SubItems.Count)
            {
                return;
            }

            if (_settings.Tools.ListViewSyntaxColorLongLines)
            {
                var s = HtmlUtil.RemoveHtmlTags(paragraph.Text, true);
                foreach (var line in s.SplitToLines())
                {
                    if (line.CountCharacters(false) > Configuration.Settings.General.SubtitleLineMaximumLength)
                    {
                        item.SubItems[ColumnIndexText].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                        return;
                    }
                }
                var noOfLines = paragraph.NumberOfLines;
                if (s.CountCharacters(false) <= Configuration.Settings.General.SubtitleLineMaximumLength * noOfLines)
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
            if (_settings.Tools.ListViewSyntaxColorWideLines)
            {
                string s = HtmlUtil.RemoveHtmlTags(paragraph.Text, true);
                foreach (string line in s.SplitToLines())
                {
                    if (TextWidth.CalcPixelWidth(line) > Configuration.Settings.General.SubtitleLineMaximumPixelWidth)
                    {
                        item.SubItems[ColumnIndexText].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                        return;
                    }
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

        private static string GetDisplayTime(TimeCode timeCode)
        {
            if (Configuration.Settings.General.CurrentVideoOffsetInMs != 0)
            {
                return new TimeCode(timeCode.TotalMilliseconds + Configuration.Settings.General.CurrentVideoOffsetInMs).ToDisplayString();
            }

            return timeCode.ToDisplayString();
        }

        private ListViewItem MakeListViewItem(Paragraph paragraph, Paragraph next, Paragraph paragraphOriginal, Font font)
        {
            var item = new ListViewItem(paragraph.Number.ToString(CultureInfo.InvariantCulture))
            {
                Tag = paragraph,
                UseItemStyleForSubItems = false,
                StateImageIndex = paragraph.Bookmark != null ? 0 : -1,
                Font = font,
                ForeColor = ForeColor,
            };
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
                        item.SubItems.Add(GetDisplayDuration(paragraph));
                        break;
                    case SubtitleColumn.CharactersPerSeconds:
                        item.SubItems.Add($"{paragraph.GetCharactersPerSecond():0.00}");
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
                    case SubtitleColumn.TextOriginal:
                        var text = paragraphOriginal != null ? paragraphOriginal.Text : string.Empty;
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

            return item;
        }

        private static string GetDisplayDuration(Paragraph paragraph)
        {
            if (paragraph.StartTime.IsMaxTime || paragraph.EndTime.IsMaxTime)
            {
                return "-";
            }

            return paragraph.Duration.ToShortDisplayString();
        }

        public void SelectNone()
        {
            SelectedIndices.Clear();
        }

        public void SelectIndexAndEnsureVisibleFaster(int index)
        {
            var topItem = TopItem;
            if (!IsValidIndex(index) || topItem == null)
            {
                return;
            }

            BeginUpdate();
            SelectedIndices.Clear();

            var selectedItem = Items[index];
            selectedItem.Selected = true;
            selectedItem.Focused = true;

            var topIndex = topItem.Index;
            var itemHeight = GetItemRect(0).Height;
            if (itemHeight == 0)
            {
                return;
            }

            var numberOfVisibleItems = (Height - 30) / itemHeight;
            var bottomIndex = topIndex + numberOfVisibleItems;
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

            var itemHeight = GetItemRect(0).Height;
            if (itemHeight == 0)
            {
                return;
            }

            int bottomIndex = TopItem.Index + (Height - 30) / itemHeight;
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
            var currentItem = Items[index];
            if (TopItem.Index <= beforeIndex && bottomIndex > afterIndex)
            {
                EnsureVisible(index);
                currentItem.Selected = true;
                if (focus)
                {
                    currentItem.Focused = true;
                }

                return;
            }

            EnsureVisible(beforeIndex);
            EnsureVisible(afterIndex);
            currentItem.Selected = true;
            if (focus)
            {
                currentItem.Focused = true;
            }
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

        public void SetText(int index, string text)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                if (ColumnIndexText == 0)
                {
                    item.Text = text;
                }
                else if (ColumnIndexText > 0 && ColumnIndexText < item.SubItems.Count)
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
            if (paragraph == null)
            {
                return;
            }

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
                    item.SubItems[ColumnIndexDuration].Text = GetDisplayDuration(paragraph);
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
                item.SubItems[ColumnIndexCps].Text = $"{paragraph.GetCharactersPerSecond():0.00}";
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

        public void SetOriginalText(int index, string text)
        {
            if (IsValidIndex(index) && Columns.Count >= ColumnIndexTextOriginal + 1)
            {
                if (GetColumnIndex(SubtitleColumn.TextOriginal) == -1)
                {
                    ShowOriginalTextColumn(string.Empty);
                }
                while (ColumnIndexTextOriginal >= Items[index].SubItems.Count)
                {
                    Items[index].SubItems.Add(string.Empty);
                }

                if (ColumnIndexTextOriginal >= 0)
                {
                    Items[index].SubItems[ColumnIndexTextOriginal].Text = text.Replace(Environment.NewLine, _lineSeparatorString);
                    Items[index].UseItemStyleForSubItems = false;
                    Items[index].SubItems[ColumnIndexTextOriginal].BackColor = BackColor;
                }
            }
        }

        public void SetDuration(int index, Paragraph paragraph, Paragraph next)
        {
            if (paragraph == null)
            {
                return;
            }

            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                if (ColumnIndexEnd >= 0)
                {
                    item.SubItems[ColumnIndexEnd].Text = GetDisplayTime(paragraph.EndTime);
                }

                if (ColumnIndexDuration >= 0)
                {
                    item.SubItems[ColumnIndexDuration].Text = GetDisplayDuration(paragraph);
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
                            item.SubItems[ColumnIndexDuration].Text = GetDisplayDuration(p);
                        }
                    }
                }
                EndUpdate();
            }
        }

        public void SetStartTimeAndEndTimeSameDuration(int index, Paragraph paragraph)
        {
            if (paragraph == null)
            {
                return;
            }

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
            }
        }

        public void SetStartTimeAndDuration(int index, Paragraph paragraph, Paragraph next, Paragraph prev)
        {
            if (paragraph == null)
            {
                return;
            }

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
                    item.SubItems[ColumnIndexDuration].Text = GetDisplayDuration(paragraph);
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
            if (paragraph == null)
            {
                return;
            }

            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                if (ColumnIndexGap >= 0)
                {
                    var gapText = GetGap(paragraph, next);
                    item.SubItems[ColumnIndexGap].Text = gapText;
                    if (!string.IsNullOrEmpty(gapText))
                    {
                        var gapMilliseconds = (int)Math.Round(next.StartTime.TotalMilliseconds - paragraph.EndTime.TotalMilliseconds);
                        item.SubItems[ColumnIndexGap].BackColor = gapMilliseconds < Configuration.Settings.General.MinimumMillisecondsBetweenLines
                            ? Configuration.Settings.Tools.ListViewSyntaxErrorColor
                            : BackColor;
                    }
                }
            }
        }

        private static string GetGap(Paragraph paragraph, Paragraph next)
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
            UpdateItem(index, i => i.BackColor = color, si => si.BackColor = color);
        }

        public void SetForegroundColor(int index, Color color)
        {
            UpdateItem(index, i => i.ForeColor = color, si => si.ForeColor = color);
        }

        private void UpdateItem(int index, Action<ListViewItem> itemUpdater, Action<ListViewItem.ListViewSubItem> subItemUpdater)
        {
            if (!IsValidIndex(index))
            {
                return;
            }

            var item = Items[index];
            itemUpdater(item);

            if (ColumnIndexNumber >= 0)
            {
                subItemUpdater(Items[index].SubItems[ColumnIndexNumber]);
            }

            if (ColumnIndexStart >= 0)
            {
                subItemUpdater(Items[index].SubItems[ColumnIndexStart]);
            }

            if (ColumnIndexEnd >= 0)
            {
                subItemUpdater(Items[index].SubItems[ColumnIndexEnd]);
            }

            if (ColumnIndexDuration >= 0)
            {
                subItemUpdater(Items[index].SubItems[ColumnIndexDuration]);
            }

            if (ColumnIndexCps >= 0)
            {
                subItemUpdater(Items[index].SubItems[ColumnIndexCps]);
            }

            if (ColumnIndexWpm >= 0)
            {
                subItemUpdater(Items[index].SubItems[ColumnIndexWpm]);
            }

            if (ColumnIndexGap >= 0)
            {
                subItemUpdater(Items[index].SubItems[ColumnIndexGap]);
            }

            if (ColumnIndexText >= 0)
            {
                subItemUpdater(Items[index].SubItems[ColumnIndexText]);
            }

            if (ColumnIndexTextOriginal >= 0)
            {
                subItemUpdater(Items[index].SubItems[ColumnIndexTextOriginal]);
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
            if (paragraph == null)
            {
                return;
            }

            if (IsValidIndex(index))
            {
                Items[index].StateImageIndex = paragraph.Bookmark != null ? 0 : -1;
            }
        }

        /// <summary>
        /// Get SelectedIndices as array for faster performance.
        /// </summary>
        /// <returns>SelectedIndices as int array</returns>
        public int[] GetSelectedIndices()
        {
            var selectedIndices = new int[SelectedIndices.Count];
            SelectedIndices.CopyTo(selectedIndices, 0);
            return selectedIndices;
        }

        public void SwapTextAndOriginalText(Subtitle subtitle, Subtitle subtitleOriginal)
        {
            if (ColumnIndexTextOriginal == -1 || ColumnIndexText == -1)
            {
                return;
            }

            (Columns[ColumnIndexTextOriginal].Text, Columns[ColumnIndexText].Text) = (Columns[ColumnIndexText].Text, Columns[ColumnIndexTextOriginal].Text);
            (SubtitleColumns[ColumnIndexTextOriginal], SubtitleColumns[ColumnIndexText]) = (SubtitleColumns[ColumnIndexText], SubtitleColumns[ColumnIndexTextOriginal]);
            UpdateColumnIndexes();
            BeginUpdate();
            var i = 0;
            foreach (ListViewItem item in Items)
            {
                var p = subtitle.GetParagraphOrDefault(i);
                if (p != null && ColumnIndexText < item.SubItems.Count)
                {
                    item.SubItems[ColumnIndexText].Text = p.Text.Replace(Environment.NewLine, _lineSeparatorString);
                }

                var original = Utilities.GetOriginalParagraph(i, p, subtitleOriginal.Paragraphs);
                if (original != null && ColumnIndexTextOriginal < item.SubItems.Count)
                {
                    item.SubItems[ColumnIndexTextOriginal].Text = original.Text.Replace(Environment.NewLine, _lineSeparatorString);
                }

                i++;
            }

            EndUpdate();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _setLastColumnWidthTimer?.Dispose();
                _syntaxColorLineTimer?.Dispose();
                _setStartAndDurationTimer?.Dispose();
            }
        }
    }
}

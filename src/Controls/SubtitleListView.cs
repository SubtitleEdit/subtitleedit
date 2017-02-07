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

        public int ColumnIndexNumber = -1;
        public int ColumnIndexStart = -1;
        public int ColumnIndexEnd = -1;
        public int ColumnIndexDuration = -1;
        public int ColumnIndexCps = -1;
        public int ColumnIndexWpm = -1;
        public int ColumnIndexText = -1;
        public int ColumnIndexTextAlternate = -1;
        public int ColumnIndexExtra = -1;
        public int ColumnIndexNetwork = -1;

        public bool IsAlternateTextColumnVisible;
        private string _lineSeparatorString = " || ";

        private Font _subtitleFont = new Font("Tahoma", 8.25F);

        private string _subtitleFontName = "Tahoma";

        public override bool RightToLeftLayout {
            get { return base.RightToLeftLayout; }
            set
            {
                if (value)
                {
                    if (ColumnIndexCps >= 0)
                        Columns[ColumnIndexCps].TextAlign = HorizontalAlignment.Left;
                    if (ColumnIndexWpm >= 0)
                        Columns[ColumnIndexWpm].TextAlign = HorizontalAlignment.Left;
                }
                else
                {
                    if (ColumnIndexCps >= 0)
                        Columns[ColumnIndexCps].TextAlign = HorizontalAlignment.Right;
                    if (ColumnIndexWpm >= 0)
                        Columns[ColumnIndexWpm].TextAlign = HorizontalAlignment.Right;
                }
                base.RightToLeftLayout = value;
            }
        }

        public string SubtitleFontName
        {
            get { return _subtitleFontName; }
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
            get { return _subtitleFontSize; }
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
                Columns[idx].Text = general.NumberSymbol;

            idx = GetColumnIndex(SubtitleColumn.Start);
            if (idx >= 0)
                Columns[idx].Text = general.StartTime;

            idx = GetColumnIndex(SubtitleColumn.End);
            if (idx >= 0)
                Columns[idx].Text = general.EndTime;

            idx = GetColumnIndex(SubtitleColumn.Duration);
            if (idx >= 0)
                Columns[idx].Text = general.Duration;

            idx = GetColumnIndex(SubtitleColumn.CharactersPerSeconds);
            if (idx >= 0)
                Columns[idx].Text = general.CharsPerSec;

            idx = GetColumnIndex(SubtitleColumn.WordsPerMinute);
            if (idx >= 0)
                Columns[idx].Text = general.WordsPerMin;

            idx = GetColumnIndex(SubtitleColumn.Text);
            if (idx >= 0)
                Columns[idx].Text = general.Text;

            if (settings.General.ListViewLineSeparatorString != null)
                _lineSeparatorString = settings.General.ListViewLineSeparatorString;

            if (!string.IsNullOrEmpty(settings.General.SubtitleFontName))
                _subtitleFontName = settings.General.SubtitleFontName;
            SubtitleFontBold = settings.General.SubtitleFontBold;
            if (settings.General.SubtitleFontSize > 6 && settings.General.SubtitleFontSize < 72)
                SubtitleFontSize = settings.General.SubtitleFontSize;
            ForeColor = settings.General.SubtitleFontColor;
            BackColor = settings.General.SubtitleBackgroundColor;
            _settings = settings;
        }

        public void InitializeTimestampColumnWidths(Form parentForm)
        {
            if (_settings != null && _settings.General.ListViewColumnsRememberSize && _settings.General.ListViewNumberWidth > 1 &&
                _settings.General.ListViewStartWidth > 1 && _settings.General.ListViewEndWidth > 1 && _settings.General.ListViewDurationWidth > 1)
            {
                int idx = GetColumnIndex(SubtitleColumn.Number);
                if (idx >= 0)
                    Columns[idx].Width = _settings.General.ListViewNumberWidth;

                idx = GetColumnIndex(SubtitleColumn.Start);
                if (idx >= 0)
                    Columns[idx].Width = _settings.General.ListViewStartWidth;

                idx = GetColumnIndex(SubtitleColumn.End);
                if (idx >= 0)
                    Columns[idx].Width = _settings.General.ListViewEndWidth;

                idx = GetColumnIndex(SubtitleColumn.Duration);
                if (idx >= 0)
                    Columns[idx].Width = _settings.General.ListViewDurationWidth;

                idx = GetColumnIndex(SubtitleColumn.CharactersPerSeconds);
                if (idx >= 0)
                    Columns[idx].Width = _settings.General.ListViewCpsWidth;

                idx = GetColumnIndex(SubtitleColumn.WordsPerMinute);
                if (idx >= 0)
                    Columns[idx].Width = _settings.General.ListViewWpmWidth;

                idx = GetColumnIndex(SubtitleColumn.Text);
                if (idx >= 0)
                    Columns[idx].Width = _settings.General.ListViewTextWidth;

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
                        Columns[ColumnIndexStart].Width = timestampWidth;

                    idx = GetColumnIndex(SubtitleColumn.End);
                    if (idx >= 0)
                        Columns[ColumnIndexEnd].Width = timestampWidth;

                    idx = GetColumnIndex(SubtitleColumn.Duration);
                    if (idx >= 0)
                        Columns[ColumnIndexEnd].Width = (int)(timestampWidth * 0.8);
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
                        Columns.Add(new ColumnHeader { Width = 55 });
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
                        Columns.Add(new ColumnHeader { Width = 55 });
                        break;
                    case SubtitleColumn.WordsPerMinute:
                        Columns.Add(new ColumnHeader { Width = 55 });
                        break;
                    case SubtitleColumn.Text:
                        Columns.Add(new ColumnHeader { Width = 300 });
                        break;
                }
            }

            // add optional columns
            if (Configuration.Settings != null && Configuration.Settings.Tools.ListViewShowColumnCharsPerSec)
                ShowCharsSecColumn(Configuration.Settings.Language.General.CharsPerSec);

            if (Configuration.Settings != null && Configuration.Settings.Tools.ListViewShowColumnWordsPerMin)
                ShowWordsMinColumn(Configuration.Settings.Language.General.WordsPerMin);

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
                    if (Columns[e.ColumnIndex].TextAlign == HorizontalAlignment.Right)
                    {
                        var stringWidth = (int)e.Graphics.MeasureString(e.Item.SubItems[e.ColumnIndex].Text, _subtitleFont).Width;
                        TextRenderer.DrawText(e.Graphics, e.Item.SubItems[e.ColumnIndex].Text, _subtitleFont, new Point(e.Bounds.Right - stringWidth - 7, e.Bounds.Top + 2), e.Item.ForeColor, TextFormatFlags.NoPrefix);
                    }
                    else
                    {
                        TextRenderer.DrawText(e.Graphics, e.Item.SubItems[e.ColumnIndex].Text, _subtitleFont, new Point(e.Bounds.Left + 3, e.Bounds.Top + 2), e.Item.ForeColor, TextFormatFlags.NoPrefix);
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
                //Rectangle r = new Rectangle(e.Bounds.Left + 1, e.Bounds.Top + 1, e.Bounds.Width - 2, e.Bounds.Height - 2);
                //e.Graphics.FillRectangle(Brushes.LightGoldenrodYellow, r);
                if (e.Item.Focused)
                    e.DrawFocusRectangle();
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
                if (e.ColumnIndex == ColumnIndexText)
                {
                    Configuration.Settings.General.ListViewTextWidth = Columns[ColumnIndexText].Width;
                }
            }
        }

        public void AutoSizeAllColumns(Form parentForm)
        {
            InitializeTimestampColumnWidths(parentForm);

            // resize "number column"
            var numberIdx = GetColumnIndex(SubtitleColumn.Number);
            if (numberIdx > 0)
            {
                if (_settings != null && _settings.General.ListViewColumnsRememberSize && _settings.General.ListViewNumberWidth > 1)
                    Columns[numberIdx].Width = _settings.General.ListViewNumberWidth;
                else
                    Columns[numberIdx].Width = 55;
            }

            int w = 0;
            for (int index = 0; index < SubtitleColumns.Count; index++)
            {
                var column = SubtitleColumns[index];
                int cw = Columns[index].Width;
                if (cw < 55 || column == SubtitleColumn.Extra || column == SubtitleColumn.Network)
                {
                    cw = 55;
                    if (column == SubtitleColumn.CharactersPerSeconds)
                        cw = 65;
                    else if (column == SubtitleColumn.WordsPerMinute)
                        cw = 70;
                    else if (column != SubtitleColumn.Number)
                            cw = 120;
                    Columns[index].Width = cw;
                    Columns[index].Width = cw;
                    Columns[index].Width = cw;
                }
                if (column != SubtitleColumn.Text && column != SubtitleColumn.TextAlternate)
                    w += cw;
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
                IsAlternateTextColumnVisible = true;
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
                    SubtitleColumns.Add(SubtitleColumn.CharactersPerSeconds);
                    Columns.Add(ch);
                }
                UpdateColumnIndexes();
                Columns[ColumnIndexWpm].Width = 70;
                Columns[ColumnIndexWpm].Width = 70;
                AutoSizeAllColumns(null);
            }
        }

        public void HideColumn(SubtitleColumn column)
        {
            var idx = GetColumnIndex(column);
            if (idx >= 0)
            {
                SubtitleColumns.RemoveAt(idx);
                Columns.RemoveAt(idx);
                UpdateColumnIndexes();
                AutoSizeAllColumns(null);
            }
        }

        private void SubtitleListViewLastColumnFill(object sender, EventArgs e)
        {
            int width = 0;
            for (int i = 0; i < Columns.Count - 1; i++)
            {
                width += Columns[i].Width;
            }
            Columns[Columns.Count - 1].Width = Width - (width + 25);
        }

        public void SaveFirstVisibleIndex()
        {
            if (TopItem != null)
                FirstVisibleIndex = Items.Count > 0 ? TopItem.Index : -1;
        }

        private void RestoreFirstVisibleIndex()
        {
            if (IsValidIndex(FirstVisibleIndex))
            {
                if (FirstVisibleIndex + 1 < Items.Count)
                    FirstVisibleIndex++;

                Items[Items.Count - 1].EnsureVisible();
                Items[FirstVisibleIndex].EnsureVisible();
            }
        }

        internal void Fill(Subtitle subtitle, Subtitle subtitleAlternate = null)
        {
            if (subtitleAlternate == null)
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
            int i = 0;
            foreach (Paragraph paragraph in paragraphs)
            {
                Add(paragraph, null);
                SyntaxColorLine(paragraphs, i, paragraph);
                i++;
            }

            ListViewItemSorter = x;
            EndUpdate();

            if (FirstVisibleIndex == 0)
                FirstVisibleIndex = -1;
        }

        internal void Fill(List<Paragraph> paragraphs, List<Paragraph> paragraphsAlternate)
        {
            SaveFirstVisibleIndex();
            BeginUpdate();
            Items.Clear();
            var x = ListViewItemSorter;
            ListViewItemSorter = null;
            int i = 0;
            foreach (Paragraph paragraph in paragraphs)
            {
                Paragraph alternate = Utilities.GetOriginalParagraph(i, paragraph, paragraphsAlternate);
                Add(paragraph, alternate);
                SyntaxColorLine(paragraphs, i, paragraph);
                i++;
            }

            ListViewItemSorter = x;
            EndUpdate();

            if (FirstVisibleIndex == 0)
                FirstVisibleIndex = -1;
        }

        public void SyntaxColorAllLines(Subtitle subtitle)
        {
            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var paragraph = subtitle.Paragraphs[index];
                SyntaxColorLine(subtitle.Paragraphs, index, paragraph);
            }
        }

        public void SyntaxColorLine(List<Paragraph> paragraphs, int i, Paragraph paragraph)
        {
            if (UseSyntaxColoring && _settings != null && IsValidIndex(i))
            {
                var item = Items[i];
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

                bool durationChanged = false;
                if (_settings.Tools.ListViewSyntaxColorDurationSmall)
                {
                    double charactersPerSecond = Utilities.GetCharactersPerSecond(paragraph);
                    if (charactersPerSecond > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        if (ColumnIndexCps >= 0)
                        {
                            item.SubItems[ColumnIndexCps].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                        }
                        else
                        {
                            item.SubItems[ColumnIndexDuration].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                            durationChanged = true;
                        }
                    }
                    else if (paragraph.Duration.TotalMilliseconds < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                    {
                        item.SubItems[ColumnIndexDuration].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                        durationChanged = true;
                    }
                }
                if (_settings.Tools.ListViewSyntaxColorDurationBig)
                {
                    if (paragraph.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                    {
                        item.SubItems[ColumnIndexDuration].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                        durationChanged = true;
                    }
                }
                if (!durationChanged && item.SubItems[ColumnIndexDuration].BackColor != BackColor)
                    item.SubItems[ColumnIndexDuration].BackColor = BackColor;

                if (_settings.Tools.ListViewSyntaxColorOverlap && i > 0 && i < paragraphs.Count)
                {
                    Paragraph prev = paragraphs[i - 1];
                    if (paragraph.StartTime.TotalMilliseconds < prev.EndTime.TotalMilliseconds)
                    {
                        Items[i - 1].SubItems[ColumnIndexEnd].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                        item.SubItems[ColumnIndexStart].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                    }
                    else
                    {
                        if (Items[i - 1].SubItems[ColumnIndexEnd].BackColor != BackColor)
                            Items[i - 1].SubItems[ColumnIndexEnd].BackColor = BackColor;
                        if (item.SubItems[ColumnIndexStart].BackColor != BackColor)
                            item.SubItems[ColumnIndexStart].BackColor = BackColor;
                    }
                }

                if (_settings.Tools.ListViewSyntaxColorLongLines)
                {
                    int noOfLines = paragraph.Text.Split(Environment.NewLine[0]).Length;
                    string s = HtmlUtil.RemoveHtmlTags(paragraph.Text, true);
                    foreach (string line in s.SplitToLines())
                    {
                        if (line.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                        {
                            item.SubItems[ColumnIndexText].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                            return;
                        }
                    }
                    s = s.Replace(Environment.NewLine, string.Empty); // we don't count new line in total length
                    if (s.Length <= Configuration.Settings.General.SubtitleLineMaximumLength * noOfLines)
                    {
                        if (noOfLines > Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX && _settings.Tools.ListViewSyntaxMoreThanXLines)
                            item.SubItems[ColumnIndexText].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                        else if (item.SubItems[ColumnIndexText].BackColor != BackColor)
                            item.SubItems[ColumnIndexText].BackColor = BackColor;
                    }
                    else
                    {
                        item.SubItems[ColumnIndexText].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                    }
                }
                if (_settings.Tools.ListViewSyntaxMoreThanXLines &&
                    item.SubItems[ColumnIndexText].BackColor != Configuration.Settings.Tools.ListViewSyntaxErrorColor)
                {
                    int newLines = paragraph.Text.SplitToLines().Length;
                    if (newLines > Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX)
                        item.SubItems[ColumnIndexText].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                }
            }
        }

        private string GetDisplayTime(TimeCode timeCode)
        {
            if (Configuration.Settings.General.CurrentVideoOffsetInMs > 0)
                return new TimeCode(timeCode.TotalMilliseconds + Configuration.Settings.General.CurrentVideoOffsetInMs).ToDisplayString();
            return timeCode.ToDisplayString();
        }

        private void Add(Paragraph paragraph, Paragraph paragraphAlternate)
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
            item.Font = new Font(_subtitleFontName, SubtitleFontSize, GetFontStyle());
            Items.Add(item);
        }

        public void SelectNone()
        {
            for (var i = Items.Count - 1; i >= 0; i--)
                Items[i].Selected = false;
        }

        public void SelectIndexAndEnsureVisible(int index, bool focus)
        {
            if (!IsValidIndex(index) || TopItem == null)
                return;

            int bottomIndex = TopItem.Index + ((Height - 25) / 16);
            int itemsBeforeAfterCount = ((bottomIndex - TopItem.Index) / 2) - 1;
            if (itemsBeforeAfterCount < 0)
                itemsBeforeAfterCount = 1;

            int beforeIndex = index - itemsBeforeAfterCount;
            if (beforeIndex < 0)
                beforeIndex = 0;

            int afterIndex = index + itemsBeforeAfterCount;
            if (afterIndex >= Items.Count)
                afterIndex = Items.Count - 1;

            SelectNone();
            if (TopItem.Index <= beforeIndex && bottomIndex > afterIndex)
            {
                Items[index].Selected = true;
                Items[index].EnsureVisible();
                if (focus)
                    Items[index].Focused = true;
                return;
            }

            Items[beforeIndex].EnsureVisible();
            EnsureVisible(beforeIndex);
            Items[afterIndex].EnsureVisible();
            EnsureVisible(afterIndex);
            Items[index].Selected = true;
            Items[index].EnsureVisible();
            if (focus)
                Items[index].Focused = true;
        }

        public void SelectIndexAndEnsureVisible(int index)
        {
            SelectIndexAndEnsureVisible(index, false);
        }

        public void SelectIndexAndEnsureVisible(Paragraph p)
        {
            SelectNone();
            if (p == null)
                return;

            for (int index = 0; index < Items.Count; index++)
            {
                ListViewItem item = Items[index];
                if (item.Text == p.Number.ToString(CultureInfo.InvariantCulture) &&
                    item.SubItems[ColumnIndexStart].Text == GetDisplayTime(p.StartTime) &&
                    item.SubItems[ColumnIndexEnd].Text == GetDisplayTime(p.EndTime) &&
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
                return subtitle.GetParagraphOrDefault(SelectedItems[0].Index);
            return null;
        }

        public string GetText(int index)
        {
            if (IsValidIndex(index))
                return Items[index].SubItems[ColumnIndexText].Text.Replace(_lineSeparatorString, Environment.NewLine);
            return null;
        }

        public string GetTextAlternate(int index)
        {
            if (IsValidIndex(index) && ColumnIndexTextAlternate >= 0)
                return Items[index].SubItems[ColumnIndexTextAlternate].Text.Replace(_lineSeparatorString, Environment.NewLine);
            return null;
        }


        public void SetText(int index, string text)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                item.SubItems[ColumnIndexText].Text = text.Replace(Environment.NewLine, _lineSeparatorString);
                var paragraph = item.Tag as Paragraph;
                if (paragraph != null)
                    UpdateCpsAndWpm(item, paragraph);
            }
        }

        public void SetTimeAndText(int index, Paragraph paragraph)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                item.SubItems[ColumnIndexStart].Text = GetDisplayTime(paragraph.StartTime);
                item.SubItems[ColumnIndexEnd].Text = GetDisplayTime(paragraph.EndTime);
                item.SubItems[ColumnIndexDuration].Text = paragraph.Duration.ToShortDisplayString();
                item.SubItems[ColumnIndexText].Text = paragraph.Text.Replace(Environment.NewLine, _lineSeparatorString);
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
                    Items[index].SubItems.Add(string.Empty);

                Items[index].SubItems[ColumnIndexExtra].Text = text;
                Items[index].UseItemStyleForSubItems = false;
                Items[index].SubItems[ColumnIndexExtra].BackColor = Color.AntiqueWhite;
                Items[index].SubItems[ColumnIndexExtra].ForeColor = color;
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
                    Items[index].SubItems.Add(string.Empty);

                Items[index].SubItems[ColumnIndexNetwork].Text = text;
                Items[index].UseItemStyleForSubItems = false;
                Items[index].SubItems[ColumnIndexNetwork].BackColor = Color.AntiqueWhite;
                Items[index].SubItems[ColumnIndexNetwork].ForeColor = color;
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
                    Items[index].SubItems.Add(string.Empty);

                Items[index].SubItems[ColumnIndexTextAlternate].Text = text.Replace(Environment.NewLine, _lineSeparatorString);
                Items[index].UseItemStyleForSubItems = false;
                Items[index].SubItems[ColumnIndexTextAlternate].BackColor = Color.AntiqueWhite;
            }
        }

        public void SetDuration(int index, Paragraph paragraph)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                item.SubItems[ColumnIndexEnd].Text = GetDisplayTime(paragraph.EndTime);
                item.SubItems[ColumnIndexDuration].Text = paragraph.Duration.ToShortDisplayString();
                UpdateCpsAndWpm(item, paragraph);
            }
        }

        public void SetNumber(int index, string number)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                item.SubItems[ColumnIndexNumber].Text = number;
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
                        item.SubItems[ColumnIndexStart].Text = GetDisplayTime(p.StartTime);
                        item.SubItems[ColumnIndexEnd].Text = GetDisplayTime(p.EndTime);
                        item.SubItems[ColumnIndexDuration].Text = p.Duration.ToShortDisplayString();
                    }
                }
                EndUpdate();
            }
        }

        public void SetStartTimeAndDuration(int index, Paragraph paragraph)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                item.SubItems[ColumnIndexStart].Text = GetDisplayTime(paragraph.StartTime);
                item.SubItems[ColumnIndexEnd].Text = GetDisplayTime(paragraph.EndTime);
                item.SubItems[ColumnIndexDuration].Text = paragraph.Duration.ToShortDisplayString();
                UpdateCpsAndWpm(item, paragraph);
            }
        }

        public void SetBackgroundColor(int index, Color color, int columnNumber)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                if (item.UseItemStyleForSubItems)
                    item.UseItemStyleForSubItems = false;
                if (columnNumber >= 0 && columnNumber < item.SubItems.Count)
                    item.SubItems[columnNumber].BackColor = color;
            }
        }

        public void SetBackgroundColor(int index, Color color)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                item.BackColor = color;
                Items[index].SubItems[ColumnIndexStart].BackColor = color;
                Items[index].SubItems[ColumnIndexEnd].BackColor = color;
                Items[index].SubItems[ColumnIndexDuration].BackColor = color;
                Items[index].SubItems[ColumnIndexText].BackColor = color;
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
                item.SubItems[ColumnIndexStart].Text = string.Empty;
                item.SubItems[ColumnIndexEnd].Text = string.Empty;
                item.SubItems[ColumnIndexDuration].Text = string.Empty;
                item.SubItems[ColumnIndexText].Text = string.Empty;

                SetBackgroundColor(index, color);
            }
        }

        public void HideNonVobSubColumns()
        {
            Columns[ColumnIndexNumber].Width = 0;
            Columns[ColumnIndexEnd].Width = 0;
            Columns[ColumnIndexDuration].Width = 0;
            Columns[ColumnIndexText].Width = 0;
            if (ColumnIndexCps >= 0)
                Columns[ColumnIndexCps].Width = 0;
            if (ColumnIndexWpm >= 0)
                Columns[ColumnIndexWpm].Width = 0;
        }

        public void SetCustomResize(EventHandler handler)
        {
            if (handler == null)
                return;
            Resize -= SubtitleListViewLastColumnFill;
            Resize += handler;
        }

        private bool IsValidIndex(int index)
        {
            return (index >= 0 && index < Items.Count);
        }

        private FontStyle GetFontStyle() => SubtitleFontBold ? FontStyle.Bold : FontStyle.Regular;
    }
}
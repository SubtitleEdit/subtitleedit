using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public sealed class SubtitleListView : ListView
    {
        public const int ColumnIndexNumber = 0;
        public const int ColumnIndexStart = 1;
        public const int ColumnIndexEnd = 2;
        public const int ColumnIndexDuration = 3;
        public const int ColumnIndexText = 4;
        public const int ColumnIndexTextAlternate = 5;
        public int ColumnIndexExtra = 5;

        private int _firstVisibleIndex = -1;
        private string _lineSeparatorString = " || ";

        private Font _subtitleFont = new Font("Tahoma", 8.25F);

        private string _subtitleFontName = "Tahoma";

        public string SubtitleFontName
        {
            get { return _subtitleFontName; }
            set
            {
                _subtitleFontName = value;
                if (SubtitleFontBold)
                    _subtitleFont = new Font(_subtitleFontName, SubtitleFontSize, FontStyle.Bold);
                else
                    _subtitleFont = new Font(_subtitleFontName, SubtitleFontSize);
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

        public bool IsAlternateTextColumnVisible { get; private set; }
        public bool IsExtraColumnVisible { get; private set; }
        public bool DisplayExtraFromExtra { get; set; }
        public bool UseSyntaxColoring { get; set; }
        private Settings _settings;
        private bool _saveColumnWidthChanges;

        public int FirstVisibleIndex
        {
            get { return _firstVisibleIndex; }
            set { _firstVisibleIndex = value; }
        }

        public void InitializeLanguage(LanguageStructure.General general, Settings settings)
        {
            Columns[ColumnIndexNumber].Text = general.NumberSymbol;
            Columns[ColumnIndexStart].Text = general.StartTime;
            Columns[ColumnIndexEnd].Text = general.EndTime;
            Columns[ColumnIndexDuration].Text = general.Duration;
            Columns[ColumnIndexText].Text = general.Text;
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
                Columns[ColumnIndexNumber].Width = _settings.General.ListViewNumberWidth;
                Columns[ColumnIndexStart].Width = _settings.General.ListViewStartWidth;
                Columns[ColumnIndexEnd].Width = _settings.General.ListViewEndWidth;
                Columns[ColumnIndexDuration].Width = _settings.General.ListViewDurationWidth;
                Columns[ColumnIndexText].Width = _settings.General.ListViewTextWidth;
                _saveColumnWidthChanges = true;
            }
            else
            {
                using (var graphics = parentForm.CreateGraphics())
                {
                    var timestampSizeF = graphics.MeasureString(new TimeCode(0, 0, 33, 527).ToDisplayString(), Font);
                    var timestampWidth = (int)(timestampSizeF.Width + 0.5) + 11;
                    Columns[ColumnIndexStart].Width = timestampWidth;
                    Columns[ColumnIndexEnd].Width = timestampWidth;
                    Columns[ColumnIndexDuration].Width = (int)(timestampWidth * 0.8);
                }
            }

            SubtitleListViewResize(this, null);
        }

        public SubtitleListView()
        {
            DoubleBuffered = true;
            UseSyntaxColoring = true;
            Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Columns.AddRange(new[]
            {
                new ColumnHeader { Text="#", Width=55 },
                new ColumnHeader { Width = 80 },
                new ColumnHeader { Width = 80 },
                new ColumnHeader { Width= 55 },
                new ColumnHeader { Width = -2 } // -2 = as rest of space (300)
            });
            SubtitleListViewResize(this, null);

            FullRowSelect = true;
            View = View.Details;
            Resize += SubtitleListViewResize;
            GridLines = true;
            ColumnWidthChanged += SubtitleListViewColumnWidthChanged;
            OwnerDraw = true;
            DrawItem += SubtitleListView_DrawItem;
            DrawSubItem += SubtitleListView_DrawSubItem;
            DrawColumnHeader += SubtitleListView_DrawColumnHeader;
        }

        private void SubtitleListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private bool IsVerticalScrollbarVisible()
        {
            if (Items.Count < 2)
                return false;

            int singleRowHeight = GetItemRect(0).Height;
            int maxVisibleItems = (Height - TopItem.Bounds.Top) / singleRowHeight;

            return Items.Count > maxVisibleItems;
        }

        private void SubtitleListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color backgroundColor = Items[e.ItemIndex].SubItems[e.ColumnIndex].BackColor;
            if (Focused && backgroundColor == BackColor)
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
                    if (RightToLeftLayout)
                    {
                        int w = Columns.Count;
                        for (int i = 0; i < Columns.Count; i++)
                            w += Columns[i].Width;

                        int extra = 0;
                        int extra2 = 0;
                        if (!IsVerticalScrollbarVisible())
                        {
                            // no vertical scrollbar
                            extra = 14;
                            extra2 = 11;
                        }
                        else
                        {
                            // no vertical scrollbar
                            extra = -3;
                            extra2 = -5;
                        }

                        var rect = new Rectangle(w - (e.Bounds.Left + e.Bounds.Width + 2) + extra, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
                        if (Configuration.Settings != null)
                        {
                            if (backgroundColor == BackColor)
                                backgroundColor = Configuration.Settings.Tools.ListViewUnfocusedSelectedColor;
                            else
                            {
                                backgroundColor = GetCustomColor(backgroundColor);
                            }
                            var sb = new SolidBrush(backgroundColor);
                            e.Graphics.FillRectangle(sb, rect);
                        }
                        else
                        {
                            e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                        }
                        var rtlBounds = new Rectangle(w - (e.Bounds.Left + e.Bounds.Width) + extra2, e.Bounds.Top + 2, e.Bounds.Width, e.Bounds.Height);
                        sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                        e.Graphics.DrawString(e.SubItem.Text, Font, new SolidBrush(e.Item.ForeColor), rtlBounds, sf);
                    }
                    else
                    {
                        Rectangle rect = e.Bounds;
                        if (Configuration.Settings != null)
                        {
                            if (backgroundColor == BackColor)
                                backgroundColor = Configuration.Settings.Tools.ListViewUnfocusedSelectedColor;
                            else
                            {
                                backgroundColor = GetCustomColor(backgroundColor);
                            }
                            var sb = new SolidBrush(backgroundColor);
                            e.Graphics.FillRectangle(sb, rect);
                        }
                        else
                        {
                            e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                        }
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
                switch (e.ColumnIndex)
                {
                    case ColumnIndexNumber:
                        Configuration.Settings.General.ListViewNumberWidth = Columns[ColumnIndexNumber].Width;
                        break;
                    case ColumnIndexStart:
                        Configuration.Settings.General.ListViewStartWidth = Columns[ColumnIndexStart].Width;
                        break;
                    case ColumnIndexEnd:
                        Configuration.Settings.General.ListViewEndWidth = Columns[ColumnIndexEnd].Width;
                        break;
                    case ColumnIndexDuration:
                        Configuration.Settings.General.ListViewDurationWidth = Columns[ColumnIndexDuration].Width;
                        break;
                    case ColumnIndexText:
                        Configuration.Settings.General.ListViewTextWidth = Columns[ColumnIndexText].Width;
                        break;
                }
            }
        }

        public void AutoSizeAllColumns(Form parentForm)
        {
            if (_settings != null && _settings.General.ListViewColumnsRememberSize && _settings.General.ListViewNumberWidth > 1)
                Columns[ColumnIndexNumber].Width = _settings.General.ListViewNumberWidth;
            else
                Columns[ColumnIndexNumber].Width = 55;

            InitializeTimestampColumnWidths(parentForm);

            int length = Columns[ColumnIndexNumber].Width + Columns[ColumnIndexStart].Width + Columns[ColumnIndexEnd].Width + Columns[ColumnIndexDuration].Width;
            int lengthAvailable = Width - length;

            int numberOfRestColumns = 1;
            if (IsAlternateTextColumnVisible)
                numberOfRestColumns++;
            if (IsExtraColumnVisible)
                numberOfRestColumns++;

            if (IsAlternateTextColumnVisible && !IsExtraColumnVisible)
            {
                if (_settings != null && _settings.General.ListViewColumnsRememberSize && _settings.General.ListViewNumberWidth > 1 &&
                    _settings.General.ListViewStartWidth > 1 && _settings.General.ListViewEndWidth > 1 && _settings.General.ListViewDurationWidth > 1)
                {
                    int restWidth = lengthAvailable - 15 - Columns[ColumnIndexText].Width;
                    if (restWidth > 0)
                        Columns[ColumnIndexTextAlternate].Width = restWidth;
                }
                else
                {
                    int restWidth = (lengthAvailable / 2) - 15;
                    Columns[ColumnIndexText].Width = restWidth;
                    Columns[ColumnIndexTextAlternate].Width = restWidth;
                }
            }
            else if (!IsAlternateTextColumnVisible && !IsExtraColumnVisible)
            {
                int restWidth = lengthAvailable - 23;
                Columns[ColumnIndexText].Width = restWidth;
            }
            else if (!IsAlternateTextColumnVisible && IsExtraColumnVisible)
            {
                int restWidth = lengthAvailable - 15;
                Columns[ColumnIndexText].Width = (int)(restWidth * 0.6);
                Columns[ColumnIndexExtra].Width = (int)(restWidth * 0.4);
            }
            else
            {
                int restWidth = lengthAvailable - 15;
                Columns[ColumnIndexText].Width = (int)(restWidth * 0.4);
                Columns[ColumnIndexTextAlternate].Width = (int)(restWidth * 0.4);
                Columns[ColumnIndexExtra].Width = (int)(restWidth * 0.2);
            }
        }

        public void ShowAlternateTextColumn(string text)
        {
            if (!IsAlternateTextColumnVisible)
            {
                ColumnIndexExtra = ColumnIndexTextAlternate + 1;
                if (IsExtraColumnVisible)
                {
                    Columns.Insert(ColumnIndexTextAlternate, new ColumnHeader { Text = text, Width = -2 });
                }
                else
                {
                    Columns.Add(new ColumnHeader { Text = text, Width = -2 });
                }

                int length = Columns[ColumnIndexNumber].Width + Columns[ColumnIndexStart].Width + Columns[ColumnIndexEnd].Width + Columns[ColumnIndexDuration].Width;
                int lengthAvailable = Width - length;
                Columns[ColumnIndexText].Width = (lengthAvailable / 2) - 15;
                Columns[ColumnIndexTextAlternate].Width = -2;

                IsAlternateTextColumnVisible = true;
            }
        }

        public void HideAlternateTextColumn()
        {
            if (IsAlternateTextColumnVisible)
            {
                IsAlternateTextColumnVisible = false;
                Columns.RemoveAt(ColumnIndexTextAlternate);
                ColumnIndexExtra = ColumnIndexTextAlternate;
                SubtitleListViewResize(null, null);
            }
        }

        private void SubtitleListViewResize(object sender, EventArgs e)
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
                Add(paragraph);
                if (DisplayExtraFromExtra && IsExtraColumnVisible && Items[i].SubItems.Count > ColumnIndexExtra)
                    Items[i].SubItems[ColumnIndexExtra].Text = paragraph.Extra;
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
                Add(paragraph);
                Paragraph alternate = Utilities.GetOriginalParagraph(i, paragraph, paragraphsAlternate);
                if (alternate != null)
                    SetAlternateText(i, alternate.Text);
                if (DisplayExtraFromExtra && IsExtraColumnVisible)
                    SetExtraText(i, paragraph.Extra, ForeColor);
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
                bool durationChanged = false;
                if (_settings.Tools.ListViewSyntaxColorDurationSmall)
                {
                    double charactersPerSecond = Utilities.GetCharactersPerSecond(paragraph);
                    if (charactersPerSecond > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        item.SubItems[ColumnIndexDuration].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                        durationChanged = true;
                    }
                    else if (paragraph.Duration.TotalMilliseconds < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                    {
                        item.SubItems[ColumnIndexDuration].BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                        durationChanged = true;
                    }
                }
                if (_settings.Tools.ListViewSyntaxColorDurationBig)
                {
                    // double charactersPerSecond = Utilities.GetCharactersPerSecond(paragraph);
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
                    s = s.Replace(Environment.NewLine, string.Empty); // we don't count new line in total length... correct?
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

        private void Add(Paragraph paragraph)
        {
            var item = new ListViewItem(paragraph.Number.ToString(CultureInfo.InvariantCulture)) { Tag = paragraph, UseItemStyleForSubItems = false };
            item.SubItems.Add(GetDisplayTime(paragraph.StartTime));
            item.SubItems.Add(GetDisplayTime(paragraph.EndTime));
            item.SubItems.Add(paragraph.Duration.ToShortDisplayString());
            item.SubItems.Add(paragraph.Text.Replace(Environment.NewLine, _lineSeparatorString));
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

            foreach (ListViewItem item in Items)
            {
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
            if (IsValidIndex(index) && IsAlternateTextColumnVisible)
                return Items[index].SubItems[ColumnIndexTextAlternate].Text.Replace(_lineSeparatorString, Environment.NewLine);
            return null;
        }

        public void SetText(int index, string text)
        {
            if (IsValidIndex(index))
                Items[index].SubItems[ColumnIndexText].Text = text.Replace(Environment.NewLine, _lineSeparatorString);
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
            }
        }

        public void ShowExtraColumn(string title)
        {
            if (!IsExtraColumnVisible)
            {
                Columns.Add(new ColumnHeader { Text = title, Width = 80 });
                int length = Columns[ColumnIndexNumber].Width + Columns[ColumnIndexStart].Width + Columns[ColumnIndexEnd].Width + Columns[ColumnIndexDuration].Width;
                int lengthAvailable = Width - length;
                if (IsAlternateTextColumnVisible)
                {
                    int part = lengthAvailable / 5;
                    ColumnIndexExtra = ColumnIndexTextAlternate + 1;
                    Columns[ColumnIndexText].Width = part * 2;
                    Columns[ColumnIndexTextAlternate].Width = part * 2;
                    Columns[ColumnIndexExtra].Width = part;
                }
                else
                {
                    int part = lengthAvailable / 6;
                    Columns[ColumnIndexText].Width = part * 4;
                    Columns[ColumnIndexTextAlternate].Width = part * 2;
                }
                IsExtraColumnVisible = true;
            }
        }

        public void HideExtraColumn()
        {
            if (IsExtraColumnVisible)
            {
                IsExtraColumnVisible = false;
                ColumnIndexExtra = ColumnIndexTextAlternate;
                if (IsAlternateTextColumnVisible)
                    ColumnIndexExtra++;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].SubItems.Count == ColumnIndexExtra + 1)
                    {
                        Items[i].SubItems.RemoveAt(ColumnIndexExtra);
                    }
                }
                Columns.RemoveAt(ColumnIndexExtra);
                SubtitleListViewResize(null, null);
            }
        }

        public void SetExtraText(int index, string text, Color color)
        {
            if (IsValidIndex(index))
            {
                ColumnIndexExtra = ColumnIndexTextAlternate;
                if (IsAlternateTextColumnVisible)
                    ColumnIndexExtra++;
                if (!IsExtraColumnVisible)
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

        public void SetAlternateText(int index, string text)
        {
            if (IsValidIndex(index) && Columns.Count >= ColumnIndexTextAlternate + 1)
            {
                if (Items[index].SubItems.Count <= ColumnIndexTextAlternate)
                {
                    Items[index].SubItems.Add(text.Replace(Environment.NewLine, _lineSeparatorString));
                }
                else
                {
                    Items[index].SubItems[ColumnIndexTextAlternate].Text = text.Replace(Environment.NewLine, _lineSeparatorString);
                }
            }
        }

        public void SetDuration(int index, Paragraph paragraph)
        {
            if (IsValidIndex(index))
            {
                ListViewItem item = Items[index];
                item.SubItems[ColumnIndexEnd].Text = GetDisplayTime(paragraph.EndTime);
                item.SubItems[ColumnIndexDuration].Text = paragraph.Duration.ToShortDisplayString();
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
        }

        public void ShowAllColumns()
        {
            if (_settings != null && _settings.General.ListViewColumnsRememberSize && _settings.General.ListViewNumberWidth > 1 &&
                _settings.General.ListViewStartWidth > 1 && _settings.General.ListViewEndWidth > 1 && _settings.General.ListViewDurationWidth > 1)
            {
                Columns[ColumnIndexNumber].Width = _settings.General.ListViewNumberWidth;
                Columns[ColumnIndexStart].Width = _settings.General.ListViewStartWidth;
                Columns[ColumnIndexEnd].Width = _settings.General.ListViewEndWidth;
                Columns[ColumnIndexDuration].Width = _settings.General.ListViewDurationWidth;
                Columns[ColumnIndexText].Width = _settings.General.ListViewTextWidth;
                Columns[IsAlternateTextColumnVisible ? ColumnIndexTextAlternate : ColumnIndexText].Width = -2;
                return;
            }

            Columns[ColumnIndexNumber].Width = 45;
            Columns[ColumnIndexEnd].Width = 80;
            Columns[ColumnIndexDuration].Width = 55;
            if (IsAlternateTextColumnVisible)
            {
                Columns[ColumnIndexText].Width = 250;
                Columns[ColumnIndexTextAlternate].Width = -2;
            }
            else
            {
                Columns[ColumnIndexText].Width = -2;
            }
        }

        public void SetCustomResize(EventHandler handler)
        {
            if (handler == null)
                return;
            Resize -= SubtitleListViewResize;
            Resize += handler;
        }

        private bool IsValidIndex(int index)
        {
            return (index >= 0 && index < Items.Count);
        }

        private FontStyle GetFontStyle() => SubtitleFontBold ? FontStyle.Bold : FontStyle.Regular;
    }
}

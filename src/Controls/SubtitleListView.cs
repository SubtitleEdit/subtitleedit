using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

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
        public string SubtitleFontName = "Tahoma";
        public bool SubtitleFontBold;
        public int SubtitleFontSize = 8;
        public bool IsAlternateTextColumnVisible { get; private set; }
        public bool IsExtraColumnVisible { get; private set; }
        public bool DisplayExtraFromExtra { get; set; }
        Settings _settings = null;

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
                SubtitleFontName = settings.General.SubtitleFontName;
            SubtitleFontBold = settings.General.SubtitleFontBold;
            if (settings.General.SubtitleFontSize > 6 && settings.General.SubtitleFontSize < 72)
                SubtitleFontSize = settings.General.SubtitleFontSize;
            ForeColor = settings.General.SubtitleFontColor;
            BackColor = settings.General.SubtitleBackgroundColor;
            _settings = settings;
        }

        public void InitializeTimeStampColumWidths(Form parentForm)
        {
            Graphics graphics = parentForm.CreateGraphics();
            SizeF timestampSizeF = graphics.MeasureString("00:00:33,527", Font);
            int timeStampWidth = (int)(timestampSizeF.Width + 0.5) + 11;

            Columns[ColumnIndexStart].Width = timeStampWidth;
            Columns[ColumnIndexEnd].Width = timeStampWidth;
            Columns[ColumnIndexDuration].Width = (int) (timeStampWidth * 0.8);
            SubtitleListView_Resize(this, null);
        }

        public SubtitleListView()
        {
            Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Columns.AddRange(new[]
            {
                new ColumnHeader { Text="#", Width=55 },
                new ColumnHeader { Width = 80 },
                new ColumnHeader { Width = 80 },
                new ColumnHeader { Width= 55 },
                new ColumnHeader { Width = -2 } // -2 = as rest of space (300)
            });
            SubtitleListView_Resize(this, null);

            FullRowSelect = true;
            View = View.Details;
            Resize += SubtitleListView_Resize;
            GridLines = true;
        }

        public void AutoSizeAllColumns(Form parentForm)
        {
            Columns[ColumnIndexNumber].Width = 55;
            InitializeTimeStampColumWidths(parentForm);

            int length = Columns[ColumnIndexNumber].Width + Columns[ColumnIndexStart].Width + Columns[ColumnIndexEnd].Width + Columns[ColumnIndexDuration].Width;
            int lengthAvailable = Width - length;

            int numberOfRestColumns = 1;
            if (IsAlternateTextColumnVisible)
                numberOfRestColumns++;
            if (IsExtraColumnVisible)
                numberOfRestColumns++;

            if (IsAlternateTextColumnVisible && !IsExtraColumnVisible)
            {
                int restWidth = (lengthAvailable / 2) - 15;
                Columns[ColumnIndexText].Width = restWidth;
                Columns[ColumnIndexTextAlternate].Width = restWidth;
            }
            else if (!IsAlternateTextColumnVisible && !IsExtraColumnVisible)
            {
                int restWidth = lengthAvailable - 23;
                Columns[ColumnIndexText].Width = restWidth;
            }
            else if (!IsAlternateTextColumnVisible && IsExtraColumnVisible)
            {
                int restWidth = lengthAvailable - 15;
                Columns[ColumnIndexText].Width = (int) (restWidth * 0.6);
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
                SubtitleListView_Resize(null, null);
            }
        }

        void SubtitleListView_Resize(object sender, EventArgs e)
        {
            int width = 0;
            for (int i = 0; i < Columns.Count - 1; i++)
            {
                width += Columns[i].Width;
            }
            Columns[Columns.Count - 1].Width = Width - (width + 25);
        }

        private ListViewItem GetFirstVisibleItem()
        {
            foreach (ListViewItem item in Items)
            {
                if (ClientRectangle.Contains(new Rectangle(item.Bounds.Left, item.Bounds.Top, item.Bounds.Height, 10)))
                {
                    return item;
                }
            }
            return null;
        }

        public void SaveFirstVisibleIndex()
        {
            ListViewItem first = GetFirstVisibleItem();
            if (Items.Count > 0 && first != null)
                FirstVisibleIndex = first.Index;
            else
                FirstVisibleIndex = -1;
        }

        private void RestoreFirstVisibleIndex()
        {
            if (FirstVisibleIndex >= 0 && FirstVisibleIndex < Items.Count)
            {
                if (FirstVisibleIndex + 1 < Items.Count)
                    FirstVisibleIndex++;

                Items[Items.Count - 1].EnsureVisible();
                Items[FirstVisibleIndex].EnsureVisible();
            }
        }

        internal void Fill(Subtitle subtitle)
        {
            Fill(subtitle.Paragraphs);
        }

        internal void Fill(Subtitle subtitle, Subtitle subtitleAlternate)
        {
            Fill(subtitle.Paragraphs, subtitleAlternate.Paragraphs);
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
                Add(paragraph, i.ToString());
                if (DisplayExtraFromExtra && IsExtraColumnVisible)
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
                Add(paragraph, i.ToString());
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

        public void SyntaxColorLine(List<Paragraph> paragraphs, int i, Paragraph paragraph)
        {
            if (_settings != null)
            {
                Items[i].UseItemStyleForSubItems = false;
                if (_settings.Tools.ListViewSyntaxColorDuration)
                {
                    double charactersPerSecond = Utilities.GetCharactersPerSecond(paragraph);
                    if (charactersPerSecond > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds + 7)
                        Items[i].SubItems[ColumnIndexDuration].BackColor = System.Drawing.Color.Red;
                    else if (charactersPerSecond > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                        Items[i].SubItems[ColumnIndexDuration].BackColor = System.Drawing.Color.Orange;
                    else
                        Items[i].SubItems[ColumnIndexDuration].BackColor = SystemColors.ControlLightLight;
                }

                if (_settings.Tools.ListViewSyntaxColorOverlap && i > 0)
                {
                    Paragraph prev = paragraphs[i - 1];
                    if (paragraph.StartTime.TotalMilliseconds < prev.EndTime.TotalMilliseconds)
                    {
                        Items[i - 1].SubItems[ColumnIndexEnd].BackColor = Color.Orange;
                        Items[i].SubItems[ColumnIndexStart].BackColor = Color.Orange;
                    }
                    else
                    {
                        Items[i - 1].SubItems[ColumnIndexEnd].BackColor = SystemColors.ControlLightLight;
                        Items[i].SubItems[ColumnIndexStart].BackColor = SystemColors.ControlLightLight;
                    }
                }

                if (_settings.Tools.ListViewSyntaxColorLongLines)
                {
                    int noOfLines = paragraph.Text.Split(Environment.NewLine[0]).Length;
                    string s = Utilities.RemoveHtmlTags(paragraph.Text).Replace(Environment.NewLine, string.Empty); // we don't count new line in total length... correct?
                    if (s.Length < Configuration.Settings.General.SubtitleLineMaximumLength * 1.9)
                    {
                        if (noOfLines == 3)
                            Items[i].SubItems[ColumnIndexText].BackColor = System.Drawing.Color.Orange;
                        else if (noOfLines > 3)
                            Items[i].SubItems[ColumnIndexText].BackColor = System.Drawing.Color.Red;
                        else
                            Items[i].SubItems[ColumnIndexText].BackColor = SystemColors.ControlLightLight;
                    }
                    else if (s.Length < Configuration.Settings.General.SubtitleLineMaximumLength * 2.1)
                    {
                        Items[i].SubItems[ColumnIndexText].BackColor = System.Drawing.Color.Orange;
                    }
                    else
                    {
                        Items[i].SubItems[ColumnIndexText].BackColor = System.Drawing.Color.Red;
                    }
                }
            }
        }

        private void Add(Paragraph paragraph, string tag)
        {
            var item = new ListViewItem(paragraph.Number.ToString()) {Tag = tag};
            var subItem = new ListViewItem.ListViewSubItem(item, paragraph.StartTime.ToString());
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, paragraph.EndTime.ToString());
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, string.Format("{0},{1:000}", paragraph.Duration.Seconds, paragraph.Duration.Milliseconds));
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, paragraph.Text.Replace(Environment.NewLine, _lineSeparatorString));
            if (SubtitleFontBold)
                subItem.Font = new Font(SubtitleFontName, SubtitleFontSize , FontStyle.Bold);
            else
                subItem.Font = new Font(SubtitleFontName, SubtitleFontSize);

            item.UseItemStyleForSubItems = false;
            item.SubItems.Add(subItem);

            Items.Add(item);
        }

        public void SelectNone()
        {
            if (SelectedItems == null)
                return;
            foreach (ListViewItem item in SelectedItems)
                item.Selected = false;
        }

        public void SelectIndexAndEnsureVisible(int index)
        {
            SelectNone();
            if (index >= 0 && index < Items.Count)
            {
                ListViewItem item = Items[index];
                item.Selected = true;

                RestoreFirstVisibleIndex();

                if (!ClientRectangle.Contains(new Rectangle(item.Bounds.Left, item.Bounds.Top-5, item.Bounds.Height+ 10, 10)))
                    item.EnsureVisible();
                FocusedItem = item;
            }
        }

        public void SelectIndexAndEnsureVisible(Paragraph p)
        {
            SelectNone();
            if (p == null)
                return;

            foreach (ListViewItem item in Items)
            {
                if (item.Text == p.Number.ToString() &&
                    item.SubItems[ColumnIndexStart].Text == p.StartTime.ToString() &&
                    item.SubItems[ColumnIndexEnd].Text == p.EndTime.ToString() &&
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
            if (index >= 0 && index < Items.Count)
                return Items[index].SubItems[ColumnIndexText].Text.Replace(_lineSeparatorString, Environment.NewLine);
            return null;
        }

        public string GetTextAlternate(int index)
        {
            if (index >= 0 && index < Items.Count && IsAlternateTextColumnVisible)
                return Items[index].SubItems[ColumnIndexTextAlternate].Text.Replace(_lineSeparatorString, Environment.NewLine);
            return null;
        }

        public string GetStartTime(int index)
        {
            if (index >= 0 && index < Items.Count)
                return Items[index].SubItems[ColumnIndexStart].Text;
            return null;
        }

        public void SetText(int index, string text)
        {
            if (index >= 0 && index < Items.Count)
                Items[index].SubItems[ColumnIndexText].Text = text.Replace(Environment.NewLine, _lineSeparatorString);
        }

        public void SetTimeAndText(int index, Paragraph paragraph)
        {
            if (index >= 0 && index < Items.Count)
            {
                ListViewItem item = Items[index];
                item.SubItems[ColumnIndexStart].Text = paragraph.StartTime.ToString();
                item.SubItems[ColumnIndexEnd].Text = paragraph.EndTime.ToString();
                item.SubItems[ColumnIndexDuration].Text = string.Format("{0},{1:000}", paragraph.Duration.Seconds, paragraph.Duration.Milliseconds);
                Items[index].SubItems[ColumnIndexText].Text = paragraph.Text.Replace(Environment.NewLine, _lineSeparatorString);
            }
        }

        public void ShowExtraColumn(string title)
        {
            if (!IsExtraColumnVisible)
            {
                if (IsAlternateTextColumnVisible)
                    ColumnIndexExtra = ColumnIndexTextAlternate + 1;
                else
                    ColumnIndexExtra = ColumnIndexTextAlternate;

                Columns.Add(new ColumnHeader { Text = title, Width = 80 });

                int length = Columns[ColumnIndexNumber].Width + Columns[ColumnIndexStart].Width + Columns[ColumnIndexEnd].Width + Columns[ColumnIndexDuration].Width;
                int lengthAvailable = Width - length;

                if (IsAlternateTextColumnVisible)
                {
                    int part = lengthAvailable / 5;
                    Columns[ColumnIndexText].Width = part * 2;
                    Columns[ColumnIndexTextAlternate].Width = part * 2;
                    Columns[ColumnIndexTextAlternate].Width = part;
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

                if (IsAlternateTextColumnVisible)
                    ColumnIndexExtra = ColumnIndexTextAlternate + 1;
                else
                    ColumnIndexExtra = ColumnIndexTextAlternate;

                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].SubItems.Count == ColumnIndexExtra + 1)
                    {
                        Items[i].SubItems[ColumnIndexExtra].Text = string.Empty;
                        Items[i].SubItems[ColumnIndexExtra].BackColor = this.BackColor;
                        Items[i].SubItems[ColumnIndexExtra].ForeColor = this.ForeColor;
                    }
                }
                Columns.RemoveAt(ColumnIndexExtra);
                SubtitleListView_Resize(null, null);
            }
        }

        public void SetExtraText(int index, string text, Color color)
        {
            if (index >= 0 && index < Items.Count)
            {
                if (IsAlternateTextColumnVisible)
                    ColumnIndexExtra = ColumnIndexTextAlternate + 1;
                else
                    ColumnIndexExtra = ColumnIndexTextAlternate;
                if (!IsExtraColumnVisible)
                {
                    ShowExtraColumn(string.Empty);
                }
                if (Items[index].SubItems.Count <= ColumnIndexExtra)
                    Items[index].SubItems.Add(new ListViewItem.ListViewSubItem());
                if (Items[index].SubItems.Count <= ColumnIndexExtra)
                    Items[index].SubItems.Add(new ListViewItem.ListViewSubItem());
                Items[index].SubItems[ColumnIndexExtra].Text = text;


                Items[index].UseItemStyleForSubItems = false;
                Items[index].SubItems[ColumnIndexExtra].BackColor = Color.AntiqueWhite;
                Items[index].SubItems[ColumnIndexExtra].ForeColor = color;

            }
        }

        public void SetAlternateText(int index, string text)
        {
            if (index >= 0 && index < Items.Count && Columns.Count >= ColumnIndexTextAlternate + 1)
            {
                if (Items[index].SubItems.Count <= ColumnIndexTextAlternate)
                {
                    var subItem = new ListViewItem.ListViewSubItem(Items[index], text.Replace(Environment.NewLine, _lineSeparatorString));
                    Items[index].SubItems.Add(subItem);
                }
                else
                {
                    Items[index].SubItems[ColumnIndexTextAlternate].Text = text.Replace(Environment.NewLine, _lineSeparatorString);
                }
            }
        }

        public void SetDuration(int index, Paragraph paragraph)
        {
            if (index >= 0 && index < Items.Count)
            {
                ListViewItem item = Items[index];
                item.SubItems[ColumnIndexDuration].Text = string.Format("{0},{1:000}", paragraph.Duration.Seconds, paragraph.Duration.Milliseconds);
                item.SubItems[ColumnIndexEnd].Text = paragraph.EndTime.ToString();
            }
        }

        public void SetNumber(int index, string number)
        {
            if (index >= 0 && index < Items.Count)
            {
                ListViewItem item = Items[index];
                item.SubItems[ColumnIndexNumber].Text = number;
            }
        }

        public void SetStartTime(int index, Paragraph paragraph)
        {
            if (index >= 0 && index < Items.Count)
            {
                ListViewItem item = Items[index];
                item.SubItems[ColumnIndexStart].Text = paragraph.StartTime.ToString();
                item.SubItems[ColumnIndexEnd].Text = paragraph.EndTime.ToString();
            }
        }

        public void SetBackgroundColor(int index, Color color, int columnNumber)
        {
            if (index >= 0 && index < Items.Count)
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
            if (index >= 0 && index < Items.Count)
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
            if (index >= 0 && index < Items.Count)
            {
                ListViewItem item = Items[index];
                return item.BackColor;
            }
            return Control.DefaultBackColor;
        }

        /// <summary>
        /// Removes all text and set background color
        /// </summary>
        /// <param name="index"></param>
        /// <param name="color"></param>
        public void ColorOut(int index, Color color)
        {
            if (index >= 0 && index < Items.Count)
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

    }
}

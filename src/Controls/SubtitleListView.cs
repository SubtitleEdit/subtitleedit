using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Controls
{
    public class SubtitleListView : ListView
    {
        public const int ColumnIndexNumber = 0;
        public const int ColumnIndexStart = 1;
        public const int ColumnIndexEnd = 2;
        public const int ColumnIndexDuration = 3;
        public const int ColumnIndexText = 4;
        public const int ColumnIndexTextAlternate = 5;

        private int _firstVisibleIndex = -1;
        private string _lineSeparatorString = " || ";
        public string SubtitleFontName = "Microsoft Sans Serif";
        public bool SubtitleFontBold = false;
        public int SubtitleFontSize = 8;

        public int FirstVisibleIndex
        {
            get { return _firstVisibleIndex; }
            set { _firstVisibleIndex = value; }
        }

        public void InitializeLanguage(LanguageStructure.General general, Settings settings)
        {
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
        }

        public SubtitleListView()
        {
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

        public void AddAlternateTextColumn(string text)
        {
            if (!IsColumTextAlternateActive)
            {
                Columns.Add(new ColumnHeader { Text = text, Width = -2 });
                Columns[ColumnIndexText].Width = 250;
                SubtitleListView_Resize(null, null);
            }
        }

        public void RemoveAlternateTextColumn()
        {
            if (IsColumTextAlternateActive)
            {
                Columns.RemoveAt(ColumnIndexTextAlternate);
                SubtitleListView_Resize(null, null);
            }
        }

        public bool IsColumTextAlternateActive
        {
            get
            {
                return Columns.Count - 1 >= ColumnIndexTextAlternate;
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
                string alternateText = GetOriginalSubtitle(i, paragraph, paragraphsAlternate);
                SetAlternateText(i, alternateText);
                i++;
            }

            ListViewItemSorter = x;
            EndUpdate();

            if (FirstVisibleIndex == 0)
                FirstVisibleIndex = -1;
        }

        private string GetOriginalSubtitle(int index, Paragraph paragraph, List<Paragraph> originalParagraphs)
        {
            if (index < originalParagraphs.Count && Math.Abs(originalParagraphs[index].StartTime.TotalMilliseconds - paragraph.StartTime.TotalMilliseconds) < 50)
                return originalParagraphs[index].Text;

            foreach (Paragraph p in originalParagraphs)
            {
                if (p.StartTime.TotalMilliseconds == paragraph.StartTime.TotalMilliseconds)
                    return p.Text;
            }

            foreach (Paragraph p in originalParagraphs)
            {
                if (p.StartTime.TotalMilliseconds > paragraph.StartTime.TotalMilliseconds - 200 &&
                    p.StartTime.TotalMilliseconds < paragraph.StartTime.TotalMilliseconds + 1000)
                    return p.Text;
            }
            return string.Empty;
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
                subItem.Font = new Font(SubtitleFontName, subItem.Font.Size, FontStyle.Bold);
            else
                subItem.Font = new Font(SubtitleFontName, subItem.Font.Size);

            item.UseItemStyleForSubItems = false;
            item.SubItems.Add(subItem);

            Items.Add(item);
        }

        public void SelectNone()
        {
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
                Items[index].Text = string.Empty;
                Items[index].SubItems[ColumnIndexStart].Text = string.Empty;
                Items[index].SubItems[ColumnIndexEnd].Text = string.Empty;
                Items[index].SubItems[ColumnIndexDuration].Text = string.Empty;
                Items[index].SubItems[ColumnIndexText].Text = string.Empty;

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
            if (IsColumTextAlternateActive)
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

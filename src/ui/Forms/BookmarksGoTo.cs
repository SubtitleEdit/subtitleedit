using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class BookmarksGoTo : Form
    {
        private readonly Subtitle _subtitle;

        public BookmarksGoTo(Subtitle subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.Bookmarks.GoToBookmark;
            buttonExport.Text = LanguageSettings.Current.MultipleReplace.Export;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            columnHeaderNumber.Text = LanguageSettings.Current.General.NumberSymbol;
            columnHeaderStartTime.Text = LanguageSettings.Current.General.StartTime;
            columnHeaderText.Text = LanguageSettings.Current.General.Text;


            _subtitle = subtitle;
            foreach (var p in subtitle.Paragraphs)
            {
                if (p.Bookmark != null)
                {
                    ListViewItem item = new ListViewItem("#" + p.Number) { Tag = p };
                    item.SubItems.Add(p.StartTime.ToShortDisplayString());
                    item.SubItems.Add(p.Bookmark.Replace(Environment.NewLine, "  "));
                    listViewBookmarks.Items.Add(item);
                }
            }
        }

        public int BookmarkIndex { get; private set; }

        private void listViewBookmarks_DoubleClick(object sender, EventArgs e)
        {
            if (listViewBookmarks.SelectedItems.Count > 0)
            {
                var p = (Paragraph)listViewBookmarks.SelectedItems[0].Tag;
                BookmarkIndex = _subtitle.Paragraphs.IndexOf(p);
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewBookmarks.SelectedItems.Count > 0)
            {
                var p = (Paragraph)listViewBookmarks.SelectedItems[0].Tag;
                BookmarkIndex = _subtitle.Paragraphs.IndexOf(p);
                DialogResult = DialogResult.OK;
            }
        }

        private void BookmarksGoTo_KeyDown(object sender, KeyEventArgs e)
        {
            if (listViewBookmarks.Focused && e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#bookmarks");
                e.SuppressKeyPress = true;
            }
        }

        private void BookmarksGoTo_ResizeEnd(object sender, EventArgs e)
        {
            listViewBookmarks.AutoSizeLastColumn();
        }

        private void BookmarksGoTo_Shown(object sender, EventArgs e)
        {
            BookmarksGoTo_ResizeEnd(sender, e);
            listViewBookmarks.Focus();
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            ExportBookmarksAsCsv(_subtitle, this);
        }

        public static void ExportBookmarksAsCsv(Subtitle subtitle, Form form)
        {
            using (var saveDialog = new SaveFileDialog { FileName = string.Empty, Filter = "CSV|*.csv" })
            {
                if (saveDialog.ShowDialog(form) != DialogResult.OK)
                {
                    return;
                }

                var sb = new StringBuilder();
                foreach (var p in subtitle.Paragraphs.Where(p => p.Bookmark != null))
                {
                    sb.AppendLine(MakeParagraphCsvLine(p));
                }

                File.WriteAllText(saveDialog.FileName, sb.ToString());
            }
        }

        private static string MakeParagraphCsvLine(Paragraph paragraph)
        {
            const string separator = ";";
            var sb = new StringBuilder();
            sb.Append(paragraph.Number + separator);
            sb.Append(ToCsvText(paragraph.StartTime.ToDisplayString()) + separator);
            sb.Append(ToCsvText(paragraph.EndTime.ToDisplayString()) + separator);
            sb.Append(ToCsvText(paragraph.Duration.ToShortDisplayString()) + separator);
            sb.Append(ToCsvText(paragraph.Text) + separator);
            sb.Append(ToCsvText(paragraph.Bookmark) + separator);
            return sb.ToString();
        }

        private static string ToCsvText(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append("\"");
            foreach (var nextChar in input)
            {
                sb.Append(nextChar);
                if (nextChar == '"')
                {
                    sb.Append("\"");
                }
            }
            sb.Append("\"");
            return sb.ToString();
        }
    }
}

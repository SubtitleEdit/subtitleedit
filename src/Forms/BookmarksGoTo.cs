using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

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
            Text = Configuration.Settings.Language.Bookmarks.GoToBookmark;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            columnHeaderNumber.Text = Configuration.Settings.Language.General.NumberSymbol;
            columnHeaderStartTime.Text = Configuration.Settings.Language.General.StartTime;
            columnHeaderText.Text = Configuration.Settings.Language.General.Text;

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
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#bookmarks");
                e.SuppressKeyPress = true;
            }
        }
    }
}

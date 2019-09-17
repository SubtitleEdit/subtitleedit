using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class BookmarkAdd : Form
    {
        public BookmarkAdd(Paragraph p)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = p?.Bookmark == null ? Configuration.Settings.Language.Bookmarks.AddBookmark : Configuration.Settings.Language.Bookmarks.EditBookmark;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            textBoxBookmarkComment.Text = p?.Bookmark;
        }

        public string Comment => textBoxBookmarkComment.Text;

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void BookmarkAdd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Enter)
            {
                DialogResult = DialogResult.OK;
            }
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#bookmarks");
                e.SuppressKeyPress = true;
            }
        }
    }
}

using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class BookmarkAdd : Form
    {
        public BookmarkAdd(Paragraph p)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = p?.Bookmark == null ? LanguageSettings.Current.Bookmarks.AddBookmark : LanguageSettings.Current.Bookmarks.EditBookmark;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
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
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#bookmarks");
                e.SuppressKeyPress = true;
            }
        }
    }
}

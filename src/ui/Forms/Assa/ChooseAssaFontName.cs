using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public partial class ChooseAssaFontName : Form
    {
        private readonly List<AssaAttachmentFont> _attachments;
        public string FontName { get; set; }
        public AssaAttachmentFont FontAttachment { get; set; }

        public ChooseAssaFontName(List<AssaAttachmentFont> attachments)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _attachments = attachments;
            GetFonts(attachments);

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            if (listViewAttachments.Items.Count > 0)
            {
                listViewAttachments.Items[0].Selected = true;
                listViewAttachments.Items[0].Focused = true;
            }
        }

        private void GetFonts(List<AssaAttachmentFont> attachments)
        {
            foreach (var font in attachments)
            {
                var item = new ListViewItem(font.FontName);
                item.SubItems.Add(font.FileName);
                item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(font.Bytes.Length));
                listViewAttachments.Items.Add(item);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewAttachments.SelectedItems.Count > 0)
            {
                FontName = listViewAttachments.SelectedItems[0].Text;
                FontAttachment = _attachments[listViewAttachments.SelectedItems[0].Index];
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ChooseFontName_ResizeEnd(object sender, EventArgs e)
        {
            listViewAttachments.AutoSizeLastColumn();
        }

        private void ChooseFontName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(null, null);
            }
        }

        private void listViewAttachments_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewAttachments.SelectedItems.Count == 0)
            {
                return;
            }

            buttonOK_Click(null, null);
        }
    }
}

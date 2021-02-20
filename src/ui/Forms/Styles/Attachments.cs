using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Styles
{
    public sealed partial class Attachments : Form
    {
        private readonly List<AssaAttachment> _attachments;
        private readonly List<string> _imageExtensions = new List<string>
        {
            ".png",
            ".gif",
            ".jpg" ,
            ".gif" ,
            ".bmp" ,
            ".ico"
        };

        public string NewFooter { get; private set; }

        public Attachments(string source)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelInfo.Visible = false;
            textBoxInfo.Visible = false;
            textBoxInfo.ReadOnly = true;
            labelImageResizedToFit.Visible = false;
            labelImageResizedToFit.Text = LanguageSettings.Current.AssaAttachments.ImageResized;

            _attachments = new List<AssaAttachment>();
            ListAttachments(source.SplitToLines());
            if (listViewAttachments.Items.Count > 0)
            {
                listViewAttachments.Items[0].Selected = true;
                listViewAttachments.Items[0].Focused = true;
            }

            UpdateAfterListViewChange();
            Text = LanguageSettings.Current.AssaAttachments.Title;
            buttonAttachFont.Text = LanguageSettings.Current.AssaAttachments.AttachFont;
            buttonAttachGraphics.Text = LanguageSettings.Current.AssaAttachments.AttachGraphics;
            buttonExport.Text = LanguageSettings.Current.MultipleReplace.Export;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private void ListAttachments(List<string> lines)
        {
            bool attachmentOn = false;
            var attachmentContent = new StringBuilder();
            var attachmentFileName = string.Empty;
            var category = string.Empty;
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (attachmentOn)
                {
                    if (s == "[V4+ Styles]" || s == "[Events]")
                    {
                        AddToListView(attachmentFileName, attachmentContent.ToString(), category);
                        attachmentOn = false;
                        attachmentContent = new StringBuilder();
                        attachmentFileName = string.Empty;
                    }
                    else if (s == string.Empty)
                    {
                        AddToListView(attachmentFileName, attachmentContent.ToString(), category);
                        attachmentContent = new StringBuilder();
                        attachmentFileName = string.Empty;
                    }
                    else if (s.StartsWith("filename:") || s.StartsWith("fontname:"))
                    {
                        AddToListView(attachmentFileName, attachmentContent.ToString(), category);
                        attachmentContent = new StringBuilder();
                        attachmentFileName = s.Remove(0, 9).Trim();
                    }
                    else
                    {
                        attachmentContent.AppendLine(s);
                    }
                }
                else if (s == "[Fonts]" || s == "[Graphics]")
                {
                    category = s;
                    attachmentOn = true;
                    attachmentContent = new StringBuilder();
                    attachmentFileName = string.Empty;
                }
            }

            AddToListView(attachmentFileName, attachmentContent.ToString(), category);
        }

        private void AddToListView(string attachmentFileName, string attachmentContent, string category)
        {
            var content = attachmentContent.Trim();
            if (string.IsNullOrEmpty(attachmentFileName) || content.Length == 0)
            {
                return;
            }

            var item = new ListViewItem(attachmentFileName);
            var bytes = UUEncoding.UUDecode(content);
            _attachments.Add(new AssaAttachment { FileName = attachmentFileName, Bytes = bytes, Category = category, Content = content });
            item.SubItems.Add(GetType(attachmentFileName));
            item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(bytes.Length));
            listViewAttachments.Items.Add(item);
        }

        private string GetType(string attachmentFileName)
        {
            var ext = Path.GetExtension(attachmentFileName)?.ToLowerInvariant();
            if (ext == ".ttf")
            {
                return LanguageSettings.Current.AssaAttachments.Font;
            }

            if (_imageExtensions.Contains(ext))
            {
                return LanguageSettings.Current.AssaAttachments.Graphics;
            }

            return "Unknown";
        }

        private void listViewAttachments_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelInfo.Visible = false;
            textBoxInfo.Visible = false;
            labelImageResizedToFit.Visible = false;

            if (listViewAttachments.SelectedItems.Count == 0)
            {
                pictureBoxPreview.Image?.Dispose();
                pictureBoxPreview.Image = new Bitmap(1, 1);
                buttonExport.Enabled = false;
                return;
            }

            var item = listViewAttachments.SelectedItems[0];
            pictureBoxPreview.ContextMenuStrip = null;
            if (item.SubItems[1].Text == LanguageSettings.Current.AssaAttachments.Font)
            {
                ShowFont(_attachments[listViewAttachments.SelectedItems[0].Index].Bytes);
                pictureBoxPreview.ContextMenuStrip = contextMenuStripPreview;
            }
            else if (item.SubItems[1].Text == LanguageSettings.Current.AssaAttachments.Graphics)
            {
                ShowImage(_attachments[listViewAttachments.SelectedItems[0].Index].Bytes, item.SubItems[0].Text);
            }

            UpdateAfterListViewChange();
        }

        public void ShowFont(byte[] fontBytes)
        {
            if (pictureBoxPreview.Width <= 1 || pictureBoxPreview.Height <= 1)
            {
                return;
            }

            var privateFontCollection = new PrivateFontCollection();
            var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
            var pointer = handle.AddrOfPinnedObject();
            try
            {
                privateFontCollection.AddMemoryFont(pointer, fontBytes.Length);
            }
            finally
            {
                handle.Free();
            }

            var fontFamily = privateFontCollection.Families.FirstOrDefault();
            if (fontFamily == null)
            {
                return;
            }

            labelInfo.Text = LanguageSettings.Current.AssaAttachments.FontName;
            textBoxInfo.Text = fontFamily.Name;
            textBoxInfo.Left = labelInfo.Left + labelInfo.Width + 5;
            labelInfo.Visible = true;
            textBoxInfo.Visible = true;

            pictureBoxPreview.Image?.Dispose();
            pictureBoxPreview.Image = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);
            using (var font = new Font(fontFamily, 25, FontStyle.Regular))
            using (var graphics = Graphics.FromImage(pictureBoxPreview.Image))
            {
                graphics.DrawString(fontFamily.Name + Environment.NewLine +
                    Environment.NewLine +
                    Configuration.Settings.Tools.AssaAttachmentFontTextPreview,
                    font, Brushes.Orange, 12f, 23);
            }
            privateFontCollection.Dispose();
        }

        public void ShowImage(byte[] imageBytes, string fileName)
        {
            if (pictureBoxPreview.Width <= 1 || pictureBoxPreview.Height <= 1)
            {
                return;
            }

            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();

            if (ext == ".ico")
            {
                using (var ms = new MemoryStream(imageBytes))
                {
                    var icon = new Icon(ms);
                    pictureBoxPreview.Image?.Dispose();
                    pictureBoxPreview.SizeMode = PictureBoxSizeMode.Normal;
                    pictureBoxPreview.Image = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);
                    using (var graphics = Graphics.FromImage(pictureBoxPreview.Image))
                    {
                        graphics.DrawIcon(icon, 12, 23);
                    }

                    labelInfo.Text = LanguageSettings.Current.AssaAttachments.IconName;
                    textBoxInfo.Text = fileName;
                    textBoxInfo.Left = labelInfo.Left + labelInfo.Width + 5;
                    labelInfo.Visible = true;
                    textBoxInfo.Visible = true;
                    icon.Dispose();
                }
                return;
            }

            using (var ms = new MemoryStream(imageBytes))
            {
                var image = Image.FromStream(ms);
                pictureBoxPreview.Image?.Dispose();
                pictureBoxPreview.Image = image;

                if (pictureBoxPreview.Width > image.Width && pictureBoxPreview.Height > image.Height)
                {
                    pictureBoxPreview.SizeMode = PictureBoxSizeMode.Normal;
                }
                else
                {
                    pictureBoxPreview.SizeMode = PictureBoxSizeMode.Zoom;
                    labelImageResizedToFit.Top = pictureBoxPreview.Top + pictureBoxPreview.Height + 5;
                    labelImageResizedToFit.Visible = true;
                }

                labelInfo.Text = string.Format(LanguageSettings.Current.AssaAttachments.ImageName, image.Width, image.Height);
                textBoxInfo.Text = fileName;
                textBoxInfo.Left = labelInfo.Left + labelInfo.Width + 5;
                labelInfo.Visible = true;
                textBoxInfo.Visible = true;
            }
        }

        private void buttonAttachFont_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.Main.Menu.File.Open.RemoveChar('&');
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = "Font|*.ttf";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.Multiselect = true;
            var result = openFileDialog1.ShowDialog(this);
            if (result != DialogResult.OK || !File.Exists(openFileDialog1.FileName))
            {
                return;
            }

            foreach (var fileName in openFileDialog1.FileNames)
            {
                var attachmentFileName = Path.GetFileName(fileName);
                var attachmentContent = UUEncoding.UUEncode(FileUtil.ReadAllBytesShared(fileName));
                AddToListView(attachmentFileName, attachmentContent, "[Fonts]");
                listViewAttachments.Items[listViewAttachments.Items.Count - 1].Selected = true;
                listViewAttachments.Items[listViewAttachments.Items.Count - 1].Focused = true;
            }
            UpdateAfterListViewChange();
        }

        private void buttonAttachGraphics_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.Main.Menu.File.Open.RemoveChar('&');
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = "Images|*" + string.Join(";*", _imageExtensions).TrimEnd('*');
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.Multiselect = true;
            var result = openFileDialog1.ShowDialog(this);
            if (result != DialogResult.OK || !File.Exists(openFileDialog1.FileName))
            {
                return;
            }

            foreach (var fileName in openFileDialog1.FileNames)
            {
                var attachmentFileName = Path.GetFileName(fileName);
                var attachmentContent = UUEncoding.UUEncode(FileUtil.ReadAllBytesShared(fileName));
                AddToListView(attachmentFileName, attachmentContent, "[Graphics]");
                listViewAttachments.Items[listViewAttachments.Items.Count - 1].Selected = true;
                listViewAttachments.Items[listViewAttachments.Items.Count - 1].Focused = true;
            }

            UpdateAfterListViewChange();
        }

        private void toolStripMenuItemStorageRemove_Click(object sender, EventArgs e)
        {
            if (listViewAttachments.SelectedItems.Count == 0)
            {
                return;
            }

            string askText;
            if (listViewAttachments.SelectedItems.Count > 1)
            {
                askText = string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, listViewAttachments.SelectedItems.Count);
            }
            else
            {
                askText = LanguageSettings.Current.Main.DeleteOneLinePrompt;
            }

            if (Configuration.Settings.General.PromptDeleteLines && MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            var list = new List<int>();
            foreach (int i in listViewAttachments.SelectedIndices)
            {
                list.Add(i);
            }

            foreach (var index in list.OrderByDescending(p => p))
            {
                _attachments.RemoveAt(index);
                listViewAttachments.Items.RemoveAt(index);
            }

            var min = list.Min();
            if (min >= listViewAttachments.Items.Count)
            {
                min--;
            }

            if (listViewAttachments.Items.Count > 0)
            {
                listViewAttachments.Items[min].Selected = true;
                listViewAttachments.Items[min].Focused = true;
            }

            UpdateAfterListViewChange();
        }

        private void toolStripMenuItemStorageRemoveAll_Click(object sender, EventArgs e)
        {
            if (_attachments.Count == 0)
            {
                return;
            }

            string askText;
            if (_attachments.Count > 1)
            {
                askText = string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, _attachments.Count);
            }
            else
            {
                askText = LanguageSettings.Current.Main.DeleteOneLinePrompt;
            }

            if (MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            _attachments.Clear();
            listViewAttachments.Items.Clear();
            UpdateAfterListViewChange();
        }

        private void UpdateAfterListViewChange()
        {
            if (listViewAttachments.Items.Count > 0)
            {
                buttonExport.Enabled = true;
                return;
            }

            pictureBoxPreview.Image?.Dispose();
            pictureBoxPreview.Image = new Bitmap(1, 1);
            buttonExport.Enabled = false;
        }

        private void MoveUp(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == 0)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);
            var attachment = _attachments[idx];
            _attachments.RemoveAt(idx);
            idx--;
            _attachments.Insert(idx, attachment);

            listView.Items.Insert(idx, item);
            listView.Items[idx].Selected = true;
            listView.Items[idx].EnsureVisible();
            listView.Items[idx].Focused = true;
        }

        private void MoveDown(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx >= listView.Items.Count - 1)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);
            var attachment = _attachments[idx];
            _attachments.RemoveAt(idx);
            idx++;
            _attachments.Insert(idx, attachment);

            listView.Items.Insert(idx, item);
            listView.Items[idx].Selected = true;
            listView.Items[idx].EnsureVisible();
            listView.Items[idx].Focused = true;
        }

        private void MoveToTop(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == 0)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);
            var attachment = _attachments[idx];
            _attachments.RemoveAt(idx);
            _attachments.Insert(0, attachment);
            listView.Items.Insert(0, item);
            listView.Items[0].Selected = true;
            listView.Items[0].EnsureVisible();
            listView.Items[0].Focused = true;
        }

        private void MoveToBottom(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == listView.Items.Count - 1)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);
            var attachment = _attachments[idx];
            _attachments.RemoveAt(idx);
            _attachments.Add(attachment);

            listView.Items.Add(item);
            idx = listView.Items.Count - 1;
            listView.Items[idx].Selected = true;
            listView.Items[idx].EnsureVisible();
            listView.Items[idx].Focused = true;
        }

        private void toolStripMenuItemStorageMoveUp_Click(object sender, EventArgs e)
        {
            MoveUp(listViewAttachments);
        }

        private void toolStripMenuItemStorageMoveDown_Click(object sender, EventArgs e)
        {
            MoveDown(listViewAttachments);
        }

        private void toolStripMenuItemStorageMoveTop_Click(object sender, EventArgs e)
        {
            MoveToTop(listViewAttachments);
        }

        private void toolStripMenuItemStorageMoveBottom_Click(object sender, EventArgs e)
        {
            MoveToBottom(listViewAttachments);
        }

        private void toolStripMenuItemStorageImport_Click(object sender, EventArgs e)
        {
            buttonAttachFont_Click(null, null);
        }

        private void attachGraphicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonAttachGraphics_Click(null, null);
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            if (listViewAttachments.SelectedItems.Count == 0)
            {
                return;
            }

            var index = listViewAttachments.SelectedItems[0].Index;
            var item = listViewAttachments.Items[index];
            var ext = Path.GetExtension(item.SubItems[0].Text);
            saveFileDialog1.Title = LanguageSettings.Current.Main.Menu.File.SaveAs.RemoveChar('&');
            saveFileDialog1.FileName = item.SubItems[0].Text;
            saveFileDialog1.Filter = item.SubItems[1].Text + "|*" + ext;
            saveFileDialog1.FilterIndex = 0;
            var result = saveFileDialog1.ShowDialog(this);
            if (result != DialogResult.OK)
            {
                return;
            }

            File.WriteAllBytes(saveFileDialog1.FileName, _attachments[index].Bytes);
        }

        private void toolStripMenuItemStorageExport_Click(object sender, EventArgs e)
        {
            buttonExport_Click(null, null);
        }

        private void listViewAttachments_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp(null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                listViewAttachments.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                listViewAttachments.SelectFirstSelectedItemOnly();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift)) //InverseSelection
            {
                listViewAttachments.InverseSelection();
                e.SuppressKeyPress = true;
            }
        }

        private string UpdateFooter()
        {
            var sb = new StringBuilder();

            var fonts = _attachments.Where(p => p.Category == "[Fonts]").ToList();
            if (fonts.Count > 0)
            {
                sb.AppendLine("[Fonts]");
                foreach (var font in fonts)
                {
                    sb.AppendLine("fontname: " + font.FileName);
                    sb.AppendLine(font.Content.Trim());
                }
                sb.AppendLine();
            }

            var graphics = _attachments.Where(p => p.Category != "[Fonts]").ToList();
            if (graphics.Count > 0)
            {
                sb = new StringBuilder(sb.ToString().Trim());
                sb.AppendLine("[Graphics]");
                foreach (var g in graphics)
                {
                    sb.AppendLine("filename: " + g.FileName);
                    sb.AppendLine(g.Content.Trim());
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            NewFooter = UpdateFooter();
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void Attachments_Shown(object sender, EventArgs e)
        {
            Attachments_ResizeEnd(sender, e);
        }

        private void Attachments_ResizeEnd(object sender, EventArgs e)
        {
            listViewAttachments_SelectedIndexChanged(null, null);
            listViewAttachments.Columns[listViewAttachments.Columns.Count - 1].Width = -2;
        }

        private void Attachments_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void contextMenuStripPreview_Click(object sender, EventArgs e)
        {
            using (var form = new AttachmentPreviewText(Configuration.Settings.Tools.AssaAttachmentFontTextPreview))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                Configuration.Settings.Tools.AssaAttachmentFontTextPreview = form.PreviewText;
                listViewAttachments_SelectedIndexChanged(null, null);
            }
        }
    }
}
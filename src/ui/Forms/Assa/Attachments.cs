using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.Assa;
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

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public sealed partial class Attachments : Form
    {
        private readonly List<AssaAttachment> _attachments;
        private readonly List<string> _imageExtensions = new List<string>
        {
            ".png",
            ".jpg" ,
            ".gif" ,
            ".bmp" ,
            ".ico"
        };

        public string NewFooter { get; private set; }
        private bool _loading = true;

        public Attachments(string source)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelInfo.Visible = false;
            textBoxInfo.Visible = false;
            textBoxInfo.ReadOnly = true;
            labelFontsAndImages.Text = LanguageSettings.Current.AssaAttachments.FontsAndImages;
            labelImageResizedToFit.Visible = false;
            labelImageResizedToFit.Text = LanguageSettings.Current.AssaAttachments.ImageResized;

            _attachments = ListAttachments(source.SplitToLines());
            foreach (var attachment in _attachments)
            {
                AddToListView(attachment);
            }

            _loading = false;
            if (listViewAttachments.Items.Count > 0)
            {
                listViewAttachments.Items[0].Selected = true;
                listViewAttachments.Items[0].Focused = true;
            }

            toolStripMenuItemStorageMoveUp.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            toolStripMenuItemStorageMoveDown.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            toolStripMenuItemStorageMoveTop.Text = LanguageSettings.Current.MultipleReplace.MoveToTop;
            toolStripMenuItemStorageMoveBottom.Text = LanguageSettings.Current.MultipleReplace.MoveToBottom;
            toolStripMenuItemStorageAttach.Text = LanguageSettings.Current.AssaAttachments.AttachFiles;
            importToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Import;
            toolStripMenuItemStorageExport.Text = LanguageSettings.Current.MultipleReplace.Export;

            Text = LanguageSettings.Current.AssaAttachments.Title;
            buttonAttachFile.Text = LanguageSettings.Current.AssaAttachments.AttachFiles;
            buttonImport.Text = LanguageSettings.Current.MultipleReplace.Import;
            buttonExport.Text = LanguageSettings.Current.MultipleReplace.Export;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            UpdateAfterListViewChange();
        }

        private List<AssaAttachment> ListAttachments(List<string> lines)
        {
            var attachments = new List<AssaAttachment>();
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
                        AddToListIfNotEmpty(attachmentContent.ToString(), attachmentFileName, attachments, category);
                        attachmentOn = false;
                        attachmentContent = new StringBuilder();
                        attachmentFileName = string.Empty;
                    }
                    else if (s == string.Empty)
                    {
                        AddToListIfNotEmpty(attachmentContent.ToString(), attachmentFileName, attachments, category);
                        attachmentContent = new StringBuilder();
                        attachmentFileName = string.Empty;
                    }
                    else if (s.StartsWith("filename:") || s.StartsWith("fontname:"))
                    {
                        AddToListIfNotEmpty(attachmentContent.ToString(), attachmentFileName, attachments, category);
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

            AddToListIfNotEmpty(attachmentContent.ToString(), attachmentFileName, attachments, category);
            return attachments;
        }

        private static void AddToListIfNotEmpty(string attachmentContent, string attachmentFileName, List<AssaAttachment> attachments, string category)
        {
            var content = attachmentContent.Trim();
            if (!string.IsNullOrWhiteSpace(attachmentFileName) && !string.IsNullOrEmpty(content))
            {
                var bytes = UUEncoding.UUDecode(content);
                attachments.Add(new AssaAttachment { FileName = attachmentFileName, Bytes = bytes, Category = category, Content = content });
            }
        }

        private void AddToListView(AssaAttachment attachment)
        {
            var item = new ListViewItem(attachment.FileName);
            item.SubItems.Add(GetType(attachment.FileName));
            item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(attachment.Bytes.Length));
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
            if (_loading)
            {
                return;
            }

            labelInfo.Visible = false;
            textBoxInfo.Visible = false;
            labelImageResizedToFit.Visible = false;
            buttonExport.Enabled = listViewAttachments.SelectedItems.Count == 1;

            if (listViewAttachments.SelectedItems.Count == 0)
            {
                pictureBoxPreview.Image?.Dispose();
                pictureBoxPreview.Image = new Bitmap(1, 1);
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

        private void buttonAttachFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.Main.Menu.File.Open.RemoveChar('&');
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = $"{LanguageSettings.Current.AssaAttachments.FontsAndImages}|*.ttf;*{string.Join(";*", _imageExtensions).TrimEnd('*')}|{LanguageSettings.Current.General.Fonts}|*.ttf|{LanguageSettings.Current.General.Images}|*{string.Join(";*", _imageExtensions).TrimEnd('*')}";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.Multiselect = true;
            var result = openFileDialog1.ShowDialog(this);
            if (result != DialogResult.OK || !File.Exists(openFileDialog1.FileName))
            {
                return;
            }

            int skipCount = 0;
            var skipFiles = new List<string>();
            var newAttachments = new List<AssaAttachment>();
            foreach (var fileName in openFileDialog1.FileNames)
            {
                var attachmentFileName = Path.GetFileName(fileName);
                var attachmentContent = UUEncoding.UUEncode(FileUtil.ReadAllBytesShared(fileName));
                var ext = Path.GetExtension(attachmentFileName)?.ToLowerInvariant();
                if (ext == ".ttf")
                {
                    AddToListIfNotEmpty(attachmentContent, attachmentFileName, newAttachments, GetType(attachmentFileName));
                }
                else if (_imageExtensions.Contains(ext))
                {
                    AddToListIfNotEmpty(attachmentContent, attachmentFileName, newAttachments, GetType(attachmentFileName));
                }
                else
                {
                    skipFiles.Add(attachmentFileName);
                    skipCount++;
                }
            }

            var first = true;
            foreach (var attachment in newAttachments)
            {
                _attachments.Add(attachment);
                AddToListView(attachment);
                var idx = listViewAttachments.Items.Count - 1;
                if (first)
                {
                    listViewAttachments.SelectedIndices.Clear();
                    listViewAttachments.EnsureVisible(idx);
                    listViewAttachments.Items[idx].Focused = true;
                    first = false;
                }
                listViewAttachments.Items[idx].Selected = true;
            }

            UpdateAfterListViewChange();
            if (skipCount > 0)
            {
                MessageBox.Show(string.Format(LanguageSettings.Current.AssaAttachments.FilesSkippedX, skipCount + Environment.NewLine +
                                                Environment.NewLine +
                                                string.Join(Environment.NewLine, skipFiles)));
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.Main.Menu.File.Open.RemoveChar('&');
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = AdvancedSubStationAlpha.NameOfFormat + " |*.ass";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.Multiselect = false;
            var result = openFileDialog1.ShowDialog(this);
            if (result != DialogResult.OK || !File.Exists(openFileDialog1.FileName))
            {
                return;
            }

            int skipCount = 0;
            var skipFiles = new List<string>();
            var encoding = LanguageAutoDetect.GetEncodingFromFile(openFileDialog1.FileName);
            var newAttachments = ListAttachments(FileUtil.ReadAllLinesShared(openFileDialog1.FileName, encoding));
            var first = true;
            foreach (var attachment in newAttachments)
            {
                if (_attachments.Any(p => p.FileName.ToLowerInvariant() == attachment.FileName.ToLowerInvariant()))
                {
                    skipCount++;
                    skipFiles.Add(attachment.FileName);
                    continue;
                }

                _attachments.Add(attachment);
                AddToListView(attachment);
                var idx = listViewAttachments.Items.Count - 1;
                if (first)
                {
                    listViewAttachments.SelectedIndices.Clear();
                    listViewAttachments.EnsureVisible(idx);
                    listViewAttachments.Items[idx].Focused = true;
                    first = false;
                }
                listViewAttachments.Items[idx].Selected = true;
            }

            UpdateAfterListViewChange();
            if (skipCount > 0)
            {
                MessageBox.Show(string.Format(LanguageSettings.Current.AssaAttachments.FilesSkippedX, skipCount + Environment.NewLine +
                              Environment.NewLine +
                              string.Join(Environment.NewLine, skipFiles)));
            }

        }

        private void toolStripMenuItemStorageRemove_Click(object sender, EventArgs e)
        {
            if (listViewAttachments.SelectedItems.Count == 0)
            {
                return;
            }

            _loading = true;
            string askText;
            if (listViewAttachments.SelectedItems.Count > 1)
            {
                askText = string.Format(LanguageSettings.Current.AssaAttachments.RemoveXAttachments, listViewAttachments.SelectedItems.Count);
            }
            else
            {
                askText = LanguageSettings.Current.AssaAttachments.RemoveOneAttachment;
            }

            if (Configuration.Settings.General.PromptDeleteLines && MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            var idx = listViewAttachments.SelectedItems[0].Index;
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

            _loading = false;
            idx = Math.Min(idx, listViewAttachments.Items.Count - 1);
            if (listViewAttachments.Items.Count > 0)
            {
                listViewAttachments.SelectedIndices.Clear();
                listViewAttachments.Items[idx].Selected = true;
                listViewAttachments.Items[idx].Focused = true;
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
                askText = string.Format(LanguageSettings.Current.AssaAttachments.RemoveXAttachments, _attachments.Count);
            }
            else
            {
                askText = LanguageSettings.Current.AssaAttachments.RemoveOneAttachment;
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
            buttonExport.Enabled = listViewAttachments.SelectedItems.Count == 1;
            if (listViewAttachments.Items.Count > 0)
            {
                return;
            }

            pictureBoxPreview.Image?.Dispose();
            pictureBoxPreview.Image = new Bitmap(1, 1);
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
            buttonAttachFile_Click(null, null);
        }

        private void attachGraphicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonImport_Click(null, null);
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

            var fonts = _attachments.Where(p => p.Category == "[Fonts]" || p.Category == LanguageSettings.Current.AssaAttachments.Font).ToList();
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

            var graphics = _attachments.Where(p => p.Category != "[Fonts]" && p.Category != LanguageSettings.Current.AssaAttachments.Font).ToList();
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
            listViewAttachments.AutoSizeLastColumn();
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

        private void contextMenuStripAttachments_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            toolStripMenuItemStorageRemove.Visible = listViewAttachments.SelectedItems.Count > 0;
            toolStripMenuItemStorageRemoveAll.Visible = listViewAttachments.Items.Count > 1;
            toolStripSeparator7.Visible = listViewAttachments.SelectedItems.Count > 0 || listViewAttachments.Items.Count > 1;

            var allowMove = listViewAttachments.SelectedItems.Count == 1 && listViewAttachments.SelectedItems.Count > 0;
            toolStripMenuItemStorageMoveUp.Visible = allowMove;
            toolStripMenuItemStorageMoveDown.Visible = allowMove;
            toolStripMenuItemStorageMoveTop.Visible = allowMove;
            toolStripMenuItemStorageMoveBottom.Visible = allowMove;
            toolStripSeparator5.Visible = allowMove;
            toolStripMenuItemStorageExport.Visible = allowMove;
        }
    }
}
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr.Binary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class VobSubCharactersImport : Form
    {

        public class ListViewData
        {
            public bool Checked { get; set; }
            public BinaryOcrBitmap BinaryOcrBitmap { get; set; }
        }

        private readonly BinaryOcrDb _existingDb;
        private readonly List<ListViewData> _data = new List<ListViewData>();
        private int _selectCount;

        public VobSubCharactersImport(BinaryOcrDb binaryOcrDb)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _existingDb = binaryOcrDb;

            labelInfo.Text = string.Empty;
            labelCurrentImage.Text = string.Empty;
            Text = $"Import OCR images into \"{Path.GetFileName(_existingDb.FileName)}\"";
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private void VobSubCharactersImport_ResizeEnd(object sender, EventArgs e)
        {
            listView1.AutoSizeLastColumn();
        }

        private void VobSubCharactersImport_Shown(object sender, EventArgs e)
        {
            VobSubCharactersImport_ResizeEnd(sender, e);

            openFileDialog1.Filter = "Binary OCR db files|*.db";
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.InitialDirectory = Configuration.OcrDirectory;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                imageList1.ImageSize = new Size(40, 40);
                LoadImagesInListView();
            }
            else
            {
                Close();
            }
            listView1.ItemChecked += listView1_ItemChecked;
            UpdateSelectCount();
        }

        private void LoadImagesInListView()
        {
            int existingMatches = 0;
            listView1.SmallImageList = imageList1;
            listView1.BeginUpdate();
            var importDb = new BinaryOcrDb(openFileDialog1.FileName, true);
            var list = new List<BinaryOcrBitmap>();
            list.AddRange(importDb.CompareImages);
            list.AddRange(importDb.CompareImagesExpanded);
            foreach (var bob in list.OrderBy(p => p.Text))
            {
                if (bob.ExpandCount > 0 && _existingDb.FindExactMatchExpanded(bob) < 0)
                {
                    AddToListView(bob);
                }
                else if (bob.ExpandCount == 0 && _existingDb.FindExactMatch(bob) < 0)
                {
                    AddToListView(bob);
                }
                else
                {
                    existingMatches++;
                }
            }
            listView1.EndUpdate();
            if (listView1.Items.Count > 0)
            {
                listView1.Items[0].Selected = true;
                listView1.Items[0].Focused = true;
            }
            labelInfo.Text = $"Images found not in current db: {imageList1.Images.Count:#,##0} ({existingMatches:#,##0} matches already in current db)";
        }

        private void AddToListView(BinaryOcrBitmap bob)
        {
            _data.Add(new ListViewData { Checked = true, BinaryOcrBitmap = bob });

            var item = new ListViewItem(string.Empty) { Tag = bob, Checked = true };
            item.SubItems.Add(bob.ToString());
            listView1.Items.Add(item);
            imageList1.Images.Add(bob.ToOldBitmap(Color.Black));
            item.ImageIndex = imageList1.Images.Count - 1;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count < 1)
            {
                return;
            }

            var item = listView1.SelectedItems[0];
            var bob = (BinaryOcrBitmap)item.Tag;
            labelCurrentImage.Text = "Text: " + bob.Text + Environment.NewLine +
                                     "Size: " + bob.Width + "x" + bob.Height + Environment.NewLine +
                                     "Italic: " + (bob.Italic ? "Yes" : "No") + Environment.NewLine;

            if (pictureBox1.Image != null)
            {
                try
                {
                    var b = (Bitmap)pictureBox1.Image;
                    b.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            var bmp = bob.ToOldBitmap();
            pictureBox1.Image = bmp;
            pictureBox1.Width = bmp.Width + 2;
            pictureBox1.Height = bmp.Height + 2;
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        private void buttonFixesSelectAll_Click(object sender, EventArgs e)
        {
            listView1.ItemChecked -= listView1_ItemChecked;

            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = true;
            }

            foreach (ListViewData d in _data)
            {
                d.Checked = true;
            }

            UpdateSelectCount();

            listView1.ItemChecked += listView1_ItemChecked;
        }

        private void buttonFixesInverse_Click(object sender, EventArgs e)
        {
            listView1.ItemChecked -= listView1_ItemChecked;

            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = !item.Checked;
            }

            foreach (ListViewData d in _data)
            {
                d.Checked = !d.Checked;
            }

            UpdateSelectCount();

            listView1.ItemChecked += listView1_ItemChecked;
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            var idx = e.Item.Index;
            _data[idx].Checked = listView1.Items[idx].Checked;
            UpdateSelectCount();
        }

        private void UpdateSelectCount()
        {
            _selectCount = 0;
            foreach (ListViewData d in _data)
            {
                if (d.Checked)
                {
                    _selectCount++;
                }
            }
            buttonImport.Text = $"Import {_selectCount:#,##0}";
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            int count = 0;
            foreach (ListViewData d in _data)
            {
                if (d.Checked)
                {
                    if (d.BinaryOcrBitmap.ExpandCount == 0 && _existingDb.FindExactMatch(d.BinaryOcrBitmap) < 0)
                    {
                        count++;
                        _existingDb.Add(d.BinaryOcrBitmap);
                    }
                    else if (d.BinaryOcrBitmap.ExpandCount > 0 && _existingDb.FindExactMatchExpanded(d.BinaryOcrBitmap) < 0)
                    {
                        count++;
                        _existingDb.Add(d.BinaryOcrBitmap);
                    }
                }
            }
            _existingDb.Save();
            MessageBox.Show($"{count:#,##0} images imported");

            // reload
            listView1.ItemChecked -= listView1_ItemChecked;
            listView1.Items.Clear();
            _data.Clear();
            imageList1.Images.Clear();
            LoadImagesInListView();
            listView1.ItemChecked += listView1_ItemChecked;
            UpdateSelectCount();
        }

    }
}

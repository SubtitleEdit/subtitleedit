using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class VobSubNOcrEdit : Form
    {
        private NOcrDb _nOcrDb;
        private readonly List<NOcrChar> _nocrChars;
        private NOcrChar _nocrChar;
        private double _zoomFactor = 5.0;
        private bool _drawLineOn;
        private bool _startDone;
        private Point _start;
        private Point _end;
        private int _mx;
        private int _my;
        private readonly Bitmap _bitmap;
        private List<NOcrChar> _history = new List<NOcrChar>();
        private int _historyIndex = -1;
        public bool Changed { get; private set; }

        public VobSubNOcrEdit(NOcrDb nOcrDb, Bitmap bitmap, string fileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _nOcrDb = nOcrDb;
            _nocrChars = nOcrDb.OcrCharacters;
            _bitmap = bitmap;

            FillComboBox();

            if (bitmap != null)
            {
                pictureBoxCharacter.Image = bitmap;
                SizePictureBox();
            }

            labelInfo.Text = $"{_nocrChars.Count:#,###,##0} elements in database";
            labelNOcrCharInfo.Text = string.Empty;
            if (!string.IsNullOrEmpty(fileName))
            {
                Text = "nOCR DB - " + fileName;
            }

            if (comboBoxTexts.Items.Count > 0)
            {
                comboBoxTexts.SelectedIndex = 0;
            }
        }

        private void FillComboBox()
        {
            List<string> list = new List<string>();
            foreach (NOcrChar c in _nocrChars)
            {
                if (!list.Contains(c.Text))
                {
                    list.Add(c.Text);
                }
            }
            list.Sort();
            comboBoxTexts.Items.Clear();
            foreach (string s in list)
            {
                comboBoxTexts.Items.Add(s);
            }
        }

        private void VobSubNOcrEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                _drawLineOn = false;
                _startDone = false;
                pictureBoxCharacter.Invalidate();
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Z)
            {
                e.SuppressKeyPress = true;
                Undo();
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Y)
            {
                e.SuppressKeyPress = true;
                Redo();
            }
        }

        private void SizePictureBox()
        {
            if (pictureBoxCharacter.Image != null)
            {
                Bitmap bmp = pictureBoxCharacter.Image as Bitmap;
                pictureBoxCharacter.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBoxCharacter.Width = (int)Math.Round(bmp.Width * _zoomFactor);
                pictureBoxCharacter.Height = (int)Math.Round(bmp.Height * _zoomFactor);
                pictureBoxCharacter.Invalidate();
            }
        }

        private void buttonZoomIn_Click(object sender, EventArgs e)
        {
            if (_zoomFactor < 20)
            {
                _zoomFactor++;
                SizePictureBox();
            }
        }

        private void buttonZoomOut_Click(object sender, EventArgs e)
        {
            if (_zoomFactor > 1)
            {
                _zoomFactor--;
                SizePictureBox();
            }
        }

        private void ShowOcrPoints()
        {
            listBoxLinesForeground.Items.Clear();
            foreach (NOcrPoint op in _nocrChar.LinesForeground)
            {
                listBoxLinesForeground.Items.Add(op);
            }
            listBoxlinesBackground.Items.Clear();
            foreach (NOcrPoint op in _nocrChar.LinesBackground)
            {
                listBoxlinesBackground.Items.Add(op);
            }
            pictureBoxCharacter.Invalidate();
        }

        private bool IsMatch()
        {
            NikseBitmap nbmp = new NikseBitmap(pictureBoxCharacter.Image as Bitmap);
            foreach (NOcrPoint op in _nocrChar.LinesForeground)
            {
                foreach (Point point in op.ScaledGetPoints(_nocrChar, nbmp.Width, nbmp.Height))
                {
                    if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                    {
                        Color c = nbmp.GetPixel(point.X, point.Y);
                        if (c.A > 150 && c.R + c.G + c.B > VobSubOcr.NocrMinColor)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            foreach (NOcrPoint op in _nocrChar.LinesBackground)
            {
                foreach (Point point in op.ScaledGetPoints(_nocrChar, nbmp.Width, nbmp.Height))
                {
                    if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                    {
                        Color c = nbmp.GetPixel(point.X, point.Y);
                        if (c.A > 150 && c.R + c.G + c.B > VobSubOcr.NocrMinColor)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void listBoxFileNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelNOcrCharInfo.Text = string.Empty;
            if (listBoxFileNames.SelectedIndex < 0)
            {
                return;
            }

            _nocrChar = listBoxFileNames.Items[listBoxFileNames.SelectedIndex] as NOcrChar;
            if (_nocrChar == null)
            {
                pictureBoxCharacter.Invalidate();
                groupBoxCurrentCompareImage.Enabled = false;
                listBoxLinesForeground.Items.Clear();
                listBoxlinesBackground.Items.Clear();
            }
            else
            {
                textBoxText.Text = _nocrChar.Text;
                checkBoxItalic.Checked = _nocrChar.Italic;
                pictureBoxCharacter.Invalidate();
                groupBoxCurrentCompareImage.Enabled = true;
                labelNOcrCharInfo.Text = string.Format("Size: {0}x{1}, margin top: {2} ", _nocrChar.Width, _nocrChar.Height, _nocrChar.MarginTop);

                if (pictureBoxCharacter.Image != null)
                {
                    if (IsMatch())
                    {
                        groupBoxCurrentCompareImage.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        groupBoxCurrentCompareImage.BackColor = DefaultBackColor;
                    }
                }
                _drawLineOn = false;
                _history = new List<NOcrChar>();
                _historyIndex = -1;

                if (_bitmap == null)
                {
                    var bitmap = new Bitmap(_nocrChar.Width, _nocrChar.Height);
                    var nbmp = new NikseBitmap(bitmap);
                    nbmp.Fill(Color.White);
                    pictureBoxCharacter.Image = nbmp.GetBitmap();
                    SizePictureBox();
                    bitmap.Dispose();
                }
                ShowOcrPoints();
            }
        }

        private void comboBoxTexts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTexts.SelectedIndex < 0)
            {
                return;
            }

            listBoxFileNames.Items.Clear();
            string text = comboBoxTexts.Items[comboBoxTexts.SelectedIndex].ToString();
            foreach (NOcrChar c in _nocrChars)
            {
                if (c.Text == text)
                {
                    listBoxFileNames.Items.Add(c);
                }
            }
            if (listBoxFileNames.Items.Count > 0)
            {
                listBoxFileNames.SelectedIndex = 0;
            }
        }

        private void pictureBoxCharacter_Paint(object sender, PaintEventArgs e)
        {
            if (_nocrChar == null)
            {
                return;
            }

            var foreground = new Pen(new SolidBrush(Color.Green));
            var background = new Pen(new SolidBrush(Color.Red));
            var selPenF = new Pen(new SolidBrush(Color.GreenYellow), 3);
            var selPenB = new Pen(new SolidBrush(Color.DeepPink), 3);
            if (pictureBoxCharacter.Image != null)
            {
                foreach (NOcrPoint op in _nocrChar.LinesForeground)
                {
                    Point start = op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    Point end = op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    if (start.X == end.X && start.Y == end.Y)
                    {
                        end.X++;
                    }

                    e.Graphics.DrawLine(foreground, start, end);
                }
                foreach (NOcrPoint op in _nocrChar.LinesBackground)
                {
                    Point start = op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    Point end = op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    e.Graphics.DrawLine(background, start, end);
                }

                if (listBoxLinesForeground.Focused && listBoxLinesForeground.SelectedIndex >= 0)
                {
                    var op = (NOcrPoint)listBoxLinesForeground.Items[listBoxLinesForeground.SelectedIndex];
                    e.Graphics.DrawLine(selPenF, op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                }
                else if (listBoxlinesBackground.Focused && listBoxlinesBackground.SelectedIndex >= 0)
                {
                    var op = (NOcrPoint)listBoxlinesBackground.Items[listBoxlinesBackground.SelectedIndex];
                    e.Graphics.DrawLine(selPenB, op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                }
            }

            if (_drawLineOn)
            {
                if (_startDone)
                {
                    var p = foreground;
                    if (radioButtonCold.Checked)
                    {
                        p = background;
                    }

                    e.Graphics.DrawLine(p, new Point((int)Math.Round(_start.X * _zoomFactor), (int)Math.Round(_start.Y * _zoomFactor)), new Point(_mx, _my));
                }
            }
            foreground.Dispose();
            background.Dispose();
            selPenF.Dispose();
            selPenB.Dispose();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listBoxFileNames.Items.Count == 0 || _nocrChar == null)
            {
                return;
            }

            var oldComboBoxIndex = comboBoxTexts.SelectedIndex;
            var oldListBoxIndex = listBoxFileNames.SelectedIndex;

            _nOcrDb.Remove(_nocrChar);
            Changed = true;

            FillComboBox();
            if (comboBoxTexts.Items.Count > oldComboBoxIndex)
            {
                comboBoxTexts.SelectedIndex = oldComboBoxIndex;
                if (listBoxFileNames.Items.Count > oldListBoxIndex)
                {
                    listBoxFileNames.SelectedIndex = oldListBoxIndex;
                }
                else if (listBoxFileNames.Items.Count > 0)
                {
                    listBoxFileNames.SelectedIndex = listBoxFileNames.Items.Count - 1;
                }

                listBoxFileNames.Focus();
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddHistoryItem(NOcrChar nocrChar)
        {
            if (_historyIndex > 0 && _historyIndex < _history.Count - 1)
            {
                while (_history.Count > _historyIndex + 1)
                {
                    _history.RemoveAt(_history.Count - 1);
                }

                _historyIndex = _history.Count - 1;
            }
            _history.Add(new NOcrChar(nocrChar));
            _historyIndex++;
        }

        private void Redo()
        {
            if (_history.Count > 0 && _historyIndex < _history.Count - 1)
            {
                _historyIndex++;
                _nocrChar = new NOcrChar(_history[_historyIndex]);
                ShowOcrPoints();
            }
        }

        private void Undo()
        {
            _drawLineOn = false;
            _startDone = false;
            if (_history.Count > 0 && _historyIndex > 0)
            {
                _historyIndex--;
                _nocrChar = new NOcrChar(_history[_historyIndex]);
            }
            else if (_historyIndex == 0)
            {
                var c = new NOcrChar(_nocrChar);
                c.LinesForeground.Clear();
                c.LinesBackground.Clear();
                _nocrChar = c;
                _historyIndex--;
            }
            ShowOcrPoints();
        }

        private void pictureBoxCharacter_MouseClick(object sender, MouseEventArgs e)
        {
            if (_drawLineOn)
            {
                if (_startDone)
                {
                    Changed = true;
                    _end = new Point((int)Math.Round(e.Location.X / _zoomFactor), (int)Math.Round(e.Location.Y / _zoomFactor));
                    _nocrChar.Width = pictureBoxCharacter.Image.Width;
                    _nocrChar.Height = pictureBoxCharacter.Image.Height;
                    if (radioButtonHot.Checked)
                    {
                        _nocrChar.LinesForeground.Add(new NOcrPoint(_start, _end));
                    }
                    else
                    {
                        _nocrChar.LinesBackground.Add(new NOcrPoint(_start, _end));
                    }

                    _drawLineOn = false;
                    pictureBoxCharacter.Invalidate();
                    ShowOcrPoints();
                    AddHistoryItem(_nocrChar);

                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        _start = new Point((int)Math.Round(e.Location.X / _zoomFactor), (int)Math.Round(e.Location.Y / _zoomFactor));
                        _startDone = true;
                        _drawLineOn = true;
                        pictureBoxCharacter.Invalidate();
                    }
                }
                else
                {
                    _start = new Point((int)Math.Round(e.Location.X / _zoomFactor), (int)Math.Round(e.Location.Y / _zoomFactor));
                    _startDone = true;
                    pictureBoxCharacter.Invalidate();
                }
            }
            else
            {
                _startDone = false;
                _drawLineOn = true;
                _start = new Point((int)Math.Round(e.Location.X / _zoomFactor), (int)Math.Round(e.Location.Y / _zoomFactor));
                _startDone = true;
                pictureBoxCharacter.Invalidate();
            }
        }

        private void pictureBoxCharacter_MouseMove(object sender, MouseEventArgs e)
        {
            if (_drawLineOn)
            {
                _mx = e.X;
                _my = e.Y;
                pictureBoxCharacter.Invalidate();
            }
        }

        private void listBoxLinesForeground_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBoxCharacter.Invalidate();
        }

        private void listBoxLinesBackground_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBoxCharacter.Invalidate();
        }

        private void checkBoxItalic_CheckedChanged(object sender, EventArgs e)
        {
            if (_nocrChar != null)
            {
                _nocrChar.Italic = checkBoxItalic.Checked;
                Changed = true;
            }
        }

        private void textBoxText_TextChanged(object sender, EventArgs e)
        {
            if (_nocrChar != null)
            {
                _nocrChar.Text = textBoxText.Text;
                Changed = true;
            }
        }

        private void removeForegroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxLinesForeground.SelectedItems.Count == 1)
            {
                var idx = listBoxLinesForeground.SelectedIndex;
                var op = listBoxLinesForeground.Items[idx] as NOcrPoint;
                _nocrChar.LinesForeground.Remove(op);
                ShowOcrPoints();
                if (idx < listBoxLinesForeground.Items.Count)
                {
                    listBoxLinesForeground.SelectedIndex = idx;
                }
                else if (listBoxLinesForeground.Items.Count > 0)
                {
                    listBoxLinesForeground.SelectedIndex = listBoxLinesForeground.Items.Count - 1;
                }
            }
        }

        private void removeBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxlinesBackground.SelectedItems.Count == 1)
            {
                var idx = listBoxlinesBackground.SelectedIndex;
                var op = listBoxlinesBackground.Items[idx] as NOcrPoint;
                _nocrChar.LinesBackground.Remove(op);
                ShowOcrPoints();
                if (idx < listBoxlinesBackground.Items.Count)
                {
                    listBoxlinesBackground.SelectedIndex = idx;
                }
                else if (listBoxlinesBackground.Items.Count > 0)
                {
                    listBoxlinesBackground.SelectedIndex = listBoxlinesBackground.Items.Count - 1;
                }
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            int importedCount = 0;
            int notImportedCount = 0;
            openFileDialog1.Filter = "nOCR files|*.nocr";
            openFileDialog1.InitialDirectory = Configuration.DataDirectory;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Title = "Import existing nOCR database into current";
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                NOcrDb newDb = new NOcrDb(openFileDialog1.FileName);
                foreach (NOcrChar newChar in newDb.OcrCharacters)
                {
                    bool found = false;
                    foreach (NOcrChar oldChar in _nocrChars)
                    {
                        if (oldChar.Text == newChar.Text &&
                            oldChar.Width == newChar.Width &&
                            oldChar.Height == newChar.Height &&
                            oldChar.MarginTop == newChar.MarginTop &&
                            oldChar.ExpandCount == newChar.ExpandCount &&
                            oldChar.LinesForeground.Count == newChar.LinesForeground.Count &&
                            oldChar.LinesBackground.Count == newChar.LinesBackground.Count)
                        {
                            found = true;
                            for (int i = 0; i < oldChar.LinesForeground.Count; i++)
                            {
                                if (oldChar.LinesForeground[i].Start.X != newChar.LinesForeground[i].Start.X ||
                                    oldChar.LinesForeground[i].Start.Y != newChar.LinesForeground[i].Start.Y ||
                                    oldChar.LinesForeground[i].End.X != newChar.LinesForeground[i].End.X ||
                                    oldChar.LinesForeground[i].End.Y != newChar.LinesForeground[i].End.Y)
                                {
                                    found = false;
                                }
                            }
                            for (int i = 0; i < oldChar.LinesBackground.Count; i++)
                            {
                                if (oldChar.LinesBackground[i].Start.X != newChar.LinesBackground[i].Start.X ||
                                    oldChar.LinesBackground[i].Start.Y != newChar.LinesBackground[i].Start.Y ||
                                    oldChar.LinesBackground[i].End.X != newChar.LinesBackground[i].End.X ||
                                    oldChar.LinesBackground[i].End.Y != newChar.LinesBackground[i].End.Y)
                                {
                                    found = false;
                                }
                            }
                        }
                    }
                    if (!found)
                    {
                        _nocrChars.Add(newChar);
                        importedCount++;
                    }
                    else
                    {
                        notImportedCount++;
                    }
                }

                if (importedCount > 0)
                {
                    Changed = true;
                }

                MessageBox.Show($"Number of characters imported: {importedCount}\r\nNumber of characters not imported (already present): {notImportedCount}");
            }
        }

        private void listBoxLinesForeground_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                removeForegroundToolStripMenuItem_Click(null, null);
            }
        }

        private void listBoxlinesBackground_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                removeBackToolStripMenuItem_Click(null, null);
            }
        }
    }
}

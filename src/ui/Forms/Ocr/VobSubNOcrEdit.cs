using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class VobSubNOcrEdit : Form
    {
        private readonly NOcrDb _nOcrDb;
        private NOcrChar _nOcrChar;
        private List<NOcrChar> _nOcrChars;
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
            VobSubEditCharacters.MakeToolStripLetters(contextMenuStripLetters, InsertLanguageCharacter);
            UiUtil.FixFonts(this);

            _nOcrDb = nOcrDb;
            _bitmap = bitmap;

            FillComboBox();

            if (bitmap != null)
            {
                pictureBoxCharacter.Image = bitmap;
                SizePictureBox();
            }

            labelInfo.Text = $"{(_nOcrDb.OcrCharacters.Count + _nOcrDb.OcrCharactersExpanded.Count):#,###,##0} elements in database";
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

        private void InsertLanguageCharacter(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem toolStripMenuItem)
            {
                var start = textBoxText.SelectionStart;
                textBoxText.SelectedText = toolStripMenuItem.Text;
                textBoxText.SelectionLength = 0;
                textBoxText.SelectionStart = start + toolStripMenuItem.Text.Length;
            }
        }

        private void FillComboBox()
        {
            _nOcrChars = new List<NOcrChar>();
            _nOcrChars.AddRange(_nOcrDb.OcrCharacters);
            _nOcrChars.AddRange(_nOcrDb.OcrCharactersExpanded);
            var list = new List<string>();
            foreach (var c in _nOcrChars)
            {
                if (!list.Contains(c.Text) && c.Text != null)
                {
                    list.Add(c.Text);
                }
            }
            list.Sort();
            comboBoxTexts.Items.Clear();
            comboBoxTexts.Items.AddRange(list.ToArray<object>());
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
            foreach (var op in _nOcrChar.LinesForeground)
            {
                listBoxLinesForeground.Items.Add(op);
            }
            listBoxlinesBackground.Items.Clear();
            foreach (var op in _nOcrChar.LinesBackground)
            {
                listBoxlinesBackground.Items.Add(op);
            }
            pictureBoxCharacter.Invalidate();
        }

        private bool IsMatch()
        {
            var nikseBitmap = new NikseBitmap(pictureBoxCharacter.Image as Bitmap);
            foreach (var op in _nOcrChar.LinesForeground)
            {
                foreach (var point in op.ScaledGetPoints(_nOcrChar, nikseBitmap.Width, nikseBitmap.Height))
                {
                    if (point.X >= 0 && point.Y >= 0 && point.X < nikseBitmap.Width && point.Y < nikseBitmap.Height)
                    {
                        var a = nikseBitmap.GetAlpha(point.X, point.Y);
                        if (a <= 150)
                        {
                            return false;
                        }
                    }
                }
            }
            foreach (var op in _nOcrChar.LinesBackground)
            {
                foreach (var point in op.ScaledGetPoints(_nOcrChar, nikseBitmap.Width, nikseBitmap.Height))
                {
                    if (point.X >= 0 && point.Y >= 0 && point.X < nikseBitmap.Width && point.Y < nikseBitmap.Height)
                    {
                        var a = nikseBitmap.GetAlpha(point.X, point.Y);
                        if (a > 150)
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

            _nOcrChar = listBoxFileNames.Items[listBoxFileNames.SelectedIndex] as NOcrChar;
            if (_nOcrChar == null)
            {
                pictureBoxCharacter.Invalidate();
                groupBoxCurrentCompareImage.Enabled = false;
                listBoxLinesForeground.Items.Clear();
                listBoxlinesBackground.Items.Clear();
            }
            else
            {
                textBoxText.Text = _nOcrChar.Text;
                checkBoxItalic.Checked = _nOcrChar.Italic;
                pictureBoxCharacter.Invalidate();
                groupBoxCurrentCompareImage.Enabled = true;
                if (_nOcrChar.ExpandCount > 0)
                {
                    labelNOcrCharInfo.Text = string.Format("Size: {0}x{1}, margin top: {2}, expand count: {3}", _nOcrChar.Width, _nOcrChar.Height, _nOcrChar.MarginTop, _nOcrChar.ExpandCount);
                }
                else
                {
                    labelNOcrCharInfo.Text = string.Format("Size: {0}x{1}, margin top: {2} ", _nOcrChar.Width, _nOcrChar.Height, _nOcrChar.MarginTop);
                }

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
                    var bitmap = new Bitmap(_nOcrChar.Width, _nOcrChar.Height);
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
            foreach (NOcrChar c in _nOcrChars)
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
            if (_nOcrChar == null)
            {
                return;
            }

            var foreground = new Pen(new SolidBrush(Color.Green));
            var background = new Pen(new SolidBrush(Color.Red));
            var selPenF = new Pen(new SolidBrush(Color.GreenYellow), 3);
            var selPenB = new Pen(new SolidBrush(Color.DeepPink), 3);
            if (pictureBoxCharacter.Image != null)
            {
                foreach (NOcrPoint op in _nOcrChar.LinesForeground)
                {
                    Point start = op.GetScaledStart(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    Point end = op.GetScaledEnd(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    if (start.X == end.X && start.Y == end.Y)
                    {
                        end.X++;
                    }

                    e.Graphics.DrawLine(foreground, start, end);
                }
                foreach (NOcrPoint op in _nOcrChar.LinesBackground)
                {
                    Point start = op.GetScaledStart(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    Point end = op.GetScaledEnd(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    e.Graphics.DrawLine(background, start, end);
                }

                if (listBoxLinesForeground.Focused && listBoxLinesForeground.SelectedIndex >= 0)
                {
                    var op = (NOcrPoint)listBoxLinesForeground.Items[listBoxLinesForeground.SelectedIndex];
                    e.Graphics.DrawLine(selPenF, op.GetScaledStart(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                }
                else if (listBoxlinesBackground.Focused && listBoxlinesBackground.SelectedIndex >= 0)
                {
                    var op = (NOcrPoint)listBoxlinesBackground.Items[listBoxlinesBackground.SelectedIndex];
                    e.Graphics.DrawLine(selPenB, op.GetScaledStart(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
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
            if (listBoxFileNames.Items.Count == 0 || _nOcrChar == null)
            {
                return;
            }

            var oldComboBoxIndex = comboBoxTexts.SelectedIndex;
            var oldListBoxIndex = listBoxFileNames.SelectedIndex;

            _nOcrDb.Remove(_nOcrChar);
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
                _nOcrChar = new NOcrChar(_history[_historyIndex]);
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
                _nOcrChar = new NOcrChar(_history[_historyIndex]);
            }
            else if (_historyIndex == 0)
            {
                var c = new NOcrChar(_nOcrChar);
                c.LinesForeground.Clear();
                c.LinesBackground.Clear();
                _nOcrChar = c;
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
                    _nOcrChar.Width = pictureBoxCharacter.Image.Width;
                    _nOcrChar.Height = pictureBoxCharacter.Image.Height;
                    if (radioButtonHot.Checked)
                    {
                        _nOcrChar.LinesForeground.Add(new NOcrPoint(_start, _end));
                    }
                    else
                    {
                        _nOcrChar.LinesBackground.Add(new NOcrPoint(_start, _end));
                    }

                    _drawLineOn = false;
                    pictureBoxCharacter.Invalidate();
                    ShowOcrPoints();
                    AddHistoryItem(_nOcrChar);

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
            if (_nOcrChar != null)
            {
                _nOcrChar.Italic = checkBoxItalic.Checked;
                Changed = true;
            }

            if (checkBoxItalic.Checked)
            {
                labelTextAssociatedWithImage.Font = new Font(labelTextAssociatedWithImage.Font.FontFamily, labelTextAssociatedWithImage.Font.Size, FontStyle.Italic);
                textBoxText.Font = new Font(textBoxText.Font.FontFamily, textBoxText.Font.Size, FontStyle.Italic | FontStyle.Bold);
            }
            else
            {
                labelTextAssociatedWithImage.Font = new Font(labelTextAssociatedWithImage.Font.FontFamily, labelTextAssociatedWithImage.Font.Size);
                textBoxText.Font = new Font(textBoxText.Font.FontFamily, textBoxText.Font.Size, FontStyle.Bold);
            }
        }

        private void textBoxText_TextChanged(object sender, EventArgs e)
        {
            if (_nOcrChar != null)
            {
                _nOcrChar.Text = textBoxText.Text;
                Changed = true;
            }
        }

        private void removeForegroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxLinesForeground.SelectedItems.Count == 1)
            {
                var idx = listBoxLinesForeground.SelectedIndex;
                var op = listBoxLinesForeground.Items[idx] as NOcrPoint;
                _nOcrChar.LinesForeground.Remove(op);
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
                _nOcrChar.LinesBackground.Remove(op);
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
                var newDb = new NOcrDb(openFileDialog1.FileName);
                importedCount = ImportCharacters(importedCount, ref notImportedCount, newDb.OcrCharacters);
                importedCount += ImportCharacters(importedCount, ref notImportedCount, newDb.OcrCharactersExpanded);

                if (importedCount > 0)
                {
                    Changed = true;
                }

                MessageBox.Show($"Number of characters imported: {importedCount}\r\nNumber of characters not imported (already present): {notImportedCount}");
            }
        }

        private int ImportCharacters(int importedCount, ref int notImportedCount, List<NOcrChar> newChars)
        {
            var oldChars = new List<NOcrChar>();
            oldChars.AddRange(_nOcrDb.OcrCharacters);
            oldChars.AddRange(_nOcrDb.OcrCharactersExpanded);

            foreach (var newChar in newChars)
            {
                bool found = false;
                
                foreach (var oldChar in oldChars)
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
                    _nOcrDb.Add(newChar);
                    importedCount++;
                }
                else
                {
                    notImportedCount++;
                }
            }

            return importedCount;
        }

        private void listBoxLinesForeground_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                removeForegroundToolStripMenuItem_Click(null, null);
            }
        }

        private void listBoxLinesBackground_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                removeBackToolStripMenuItem_Click(null, null);
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxLinesForeground.SelectedItems.Count == 1)
            {
                _nOcrChar.LinesForeground.Clear();
                ShowOcrPoints();
            }
        }

        private void clearToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listBoxlinesBackground.SelectedItems.Count == 1)
            {
                _nOcrChar.LinesBackground.Clear();
                ShowOcrPoints();
            }
        }
    }
}

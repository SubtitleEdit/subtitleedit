using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.OCR;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VobSubOcrNOcrCharacter : Form
    {

        VobSubOcr _vobSubForm;
        List<VobSubOcr.ImageCompareAddition> _additions;
        NOcrChar _nocrChar = null;
        bool _drawLineOn;
        bool _startDone;
        Point _start;
        Point _end;
        double _zoomFactor = 3.0;
        int _imageWidth;
        int _imageHeight;
        int _mx;
        int _my;
        bool _warningNoNotForegroundLinesShown = false;
        List<NOcrChar> _history = new List<NOcrChar>();
        int _historyIndex = -1;

        public VobSubOcrNOcrCharacter()
        {
            InitializeComponent();
            FixLargeFonts();
            labelItalicOn.Visible = false;
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, this.Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public NOcrChar NOcrChar
        {
            get
            {
                return _nocrChar;
            }
        }

        public Point FormPosition
        {
            get
            {
                return new Point(Left, Top);
            }
        }

        public bool ExpandSelection { get; private set; }

        public bool ShrinkSelection { get; private set; }

        public bool IsItalic
        {
            get
            {
                return checkBoxItalic.Checked;
            }
        }

        internal void Initialize(Bitmap vobSubImage, ImageSplitterItem character, Point position, bool italicChecked, bool showShrink, VobSubOcr.CompareMatch bestGuess, List<VobSubOcr.ImageCompareAddition> additions, VobSubOcr vobSubForm)
        {
            NikseBitmap nbmp = new NikseBitmap(vobSubImage);
            nbmp.ReplaceTransparentWith(Color.Black);
            vobSubImage = nbmp.GetBitmap();

            radioButtonHot.Checked = true;
            ShrinkSelection = false;
            ExpandSelection = false;

            textBoxCharacters.Text = string.Empty;
            _vobSubForm = vobSubForm;
            _additions = additions;
            _nocrChar = new NOcrChar();
            _nocrChar.MarginTop = character.Y - character.ParentY;
            _imageWidth = character.Bitmap.Width;
            _imageHeight = character.Bitmap.Height;
            _drawLineOn = false;
            _warningNoNotForegroundLinesShown = false;

            buttonShrinkSelection.Visible = showShrink;

            if (position.X != -1 && position.Y != -1)
            {
                StartPosition = FormStartPosition.Manual;
                Left = position.X;
                Top = position.Y;
            }

            pictureBoxSubtitleImage.Image = vobSubImage;
            pictureBoxCharacter.Image = character.Bitmap;

            Bitmap org = (Bitmap)vobSubImage.Clone();
            Bitmap bm = new Bitmap(org.Width, org.Height);
            Graphics g = Graphics.FromImage(bm);
            g.DrawImage(org, 0, 0, org.Width, org.Height);
            g.DrawRectangle(Pens.Red, character.X, character.Y, character.Bitmap.Width, character.Bitmap.Height - 1);
            g.Dispose();
            pictureBoxSubtitleImage.Image = bm;

            pictureBoxCharacter.Top = labelCharacters.Top + 16;
            SizePictureBox();
            checkBoxItalic.Checked = italicChecked;

            _history = new List<NOcrChar>();
            _historyIndex = -1;
        }

        private void buttonExpandSelection_Click(object sender, EventArgs e)
        {
            ExpandSelection = true;
            DialogResult = DialogResult.OK;
        }

        private void buttonShrinkSelection_Click(object sender, EventArgs e)
        {
            ShrinkSelection = true;
            DialogResult = DialogResult.OK;
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
                        if (c.A > 150 && c.R + 100 + c.G + c.B > VobSubOcr.NocrMinColor)
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
                foreach (Point point in op.GetPoints(nbmp.Width, nbmp.Height))
                {
                    if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                    {
                        Color c = nbmp.GetPixel(point.X, point.Y);
                        if (c.A > 150 && c.R + 100 + c.G + c.B > VobSubOcr.NocrMinColor)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listBoxLinesForeground.Items.Count == 0)
            {
                MessageBox.Show("No foreground lines!");
                return;
            }
            if (listBoxlinesBackground.Items.Count == 0 && !_warningNoNotForegroundLinesShown)
            {
                MessageBox.Show("No not-foreground lines!");
                _warningNoNotForegroundLinesShown = true;
                return;
            }
            if (textBoxCharacters.Text.Length == 0)
            {
                MessageBox.Show("Text is empty!");
                return;
            }
            if (!IsMatch())
            {
                MessageBox.Show("Lines does not match image!");
                return;
            }
            _nocrChar.Text = textBoxCharacters.Text;
            _nocrChar.Italic = checkBoxItalic.Checked;
            DialogResult = DialogResult.OK;
        }

        private void pictureBoxCharacter_Paint(object sender, PaintEventArgs e)
        {
            NOcrPoint selectedPoint = null;
            if (listBoxLinesForeground.Focused && listBoxLinesForeground.SelectedIndex >= 0)
            {
                selectedPoint = (NOcrPoint)listBoxLinesForeground.Items[listBoxLinesForeground.SelectedIndex];
            }
            else if (listBoxlinesBackground.Focused && listBoxlinesBackground.SelectedIndex >= 0)
            {
                selectedPoint = (NOcrPoint)listBoxlinesBackground.Items[listBoxlinesBackground.SelectedIndex];
            }

            var foreground = new Pen(new SolidBrush(Color.Green));
            var background = new Pen(new SolidBrush(Color.Red));
            var selPenF = new Pen(new SolidBrush(Color.Green), 3);
            var selPenB = new Pen(new SolidBrush(Color.Red), 3);
            if (pictureBoxCharacter.Image != null)
            {
                foreach (NOcrPoint op in _nocrChar.LinesForeground)
                {
                    Point start = op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    Point end = op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    if (start.X == end.X && start.Y == end.Y)
                        end.X++;
                    e.Graphics.DrawLine(foreground, start, end);
                    if (op == selectedPoint)
                        e.Graphics.DrawLine(selPenF, op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));                    
                }
                foreach (NOcrPoint op in _nocrChar.LinesBackground)
                {
                    Point start = op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    Point end = op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    e.Graphics.DrawLine(background, start, end);
                    if (op == selectedPoint)
                        e.Graphics.DrawLine(selPenB, op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                }
            }

            if (_drawLineOn)
            {
                if (_startDone)
                {
                    var p = foreground;
                    if (radioButtonCold.Checked)
                        p = background;
                    e.Graphics.DrawLine(p, new Point((int)Math.Round(_start.X * _zoomFactor), (int)Math.Round(_start.Y * _zoomFactor)), new Point(_mx, _my));
                }
            }
            foreground.Dispose();
            background.Dispose();
            selPenF.Dispose();
            selPenB.Dispose();
        }

        private void SizePictureBox()
        {
            pictureBoxCharacter.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxCharacter.Width = (int)Math.Round(_imageWidth * _zoomFactor);
            pictureBoxCharacter.Height = (int)Math.Round(_imageHeight * _zoomFactor);
            pictureBoxCharacter.Invalidate();
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

        private void pictureBoxCharacter_MouseClick(object sender, MouseEventArgs e)
        {
            if (_drawLineOn)
            {
                if (_startDone)
                {
                    _end = new Point((int)Math.Round(e.Location.X / _zoomFactor), (int)Math.Round(e.Location.Y / _zoomFactor));
                    _nocrChar.Width = pictureBoxCharacter.Image.Width;
                    _nocrChar.Height = pictureBoxCharacter.Image.Height;
                    if (radioButtonHot.Checked)
                        _nocrChar.LinesForeground.Add(new NOcrPoint(_start, _end));
                    else
                        _nocrChar.LinesBackground.Add(new NOcrPoint(_start, _end));
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

        private void AddHistoryItem(NOcrChar nocrChar)
        {
            if (_historyIndex > 0 && _historyIndex < _history.Count - 1)
            {
                while (_history.Count > _historyIndex + 1)
                    _history.RemoveAt(_history.Count - 1);
                _historyIndex = _history.Count-1;
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

        private void pictureBoxCharacter_MouseMove(object sender, MouseEventArgs e)
        {
            if (_drawLineOn)
            {
                _mx = e.X;
                _my = e.Y;
                pictureBoxCharacter.Invalidate();
            }
        }

        private void VobSubOcrNOcrCharacter_KeyDown(object sender, KeyEventArgs e)
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

        private void removeForegroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxlinesBackground.SelectedItems.Count == 1)
            {
                var op = listBoxlinesBackground.Items[listBoxlinesBackground.SelectedIndex] as NOcrPoint;
                _nocrChar.LinesBackground.Remove(op);
            }
            ShowOcrPoints();
        }

        private void removeForegroundToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (listBoxLinesForeground.SelectedItems.Count == 1)
            {
                var op = listBoxLinesForeground.Items[listBoxLinesForeground.SelectedIndex] as NOcrPoint;
                _nocrChar.LinesForeground.Remove(op);
            }
            ShowOcrPoints();
        }

        private void textBoxCharacters_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                buttonOK_Click(null, null);
            }
        }

        private void checkBoxItalic_CheckedChanged(object sender, EventArgs e)
        {
            textBoxCharacters.Focus();

            if (checkBoxItalic.Checked)
            {
                labelCharactersAsText.Font = new System.Drawing.Font(labelCharactersAsText.Font.FontFamily, labelCharactersAsText.Font.Size, FontStyle.Italic);
                textBoxCharacters.Font = new System.Drawing.Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size, FontStyle.Italic);
                labelItalicOn.Visible = true;
            }
            else
            {
                labelCharactersAsText.Font = new System.Drawing.Font(labelCharactersAsText.Font.FontFamily, labelCharactersAsText.Font.Size);
                textBoxCharacters.Font = new System.Drawing.Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size);
                labelItalicOn.Visible = false;
            }
        }

        private void listBoxLinesForeground_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBoxCharacter.Invalidate();
        }

        private void listBoxlinesBackground_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBoxCharacter.Invalidate();
        }

    }
}

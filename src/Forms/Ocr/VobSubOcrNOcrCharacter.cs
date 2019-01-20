using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class VobSubOcrNOcrCharacter : Form
    {
        private NOcrChar _nocrChar;
        private bool _drawLineOn;
        private bool _startDone;
        private Point _start;
        private Point _end;
        private double _zoomFactor = 3.0;
        private int _imageWidth;
        private int _imageHeight;
        private int _mx;
        private int _my;
        private bool _warningNoNotForegroundLinesShown;
        private List<NOcrChar> _history = new List<NOcrChar>();
        private int _historyIndex = -1;

        public VobSubOcrNOcrCharacter()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonCancel);
            labelItalicOn.Visible = false;
        }

        public NOcrChar NOcrChar => _nocrChar;

        public Point FormPosition => new Point(Left, Top);

        public bool ExpandSelection { get; private set; }

        public bool ShrinkSelection { get; private set; }

        public bool IsItalic => checkBoxItalic.Checked;

        internal void Initialize(Bitmap vobSubImage, ImageSplitterItem character, Point position, bool italicChecked, bool showShrink)
        {
            listBoxLinesForeground.Items.Clear();
            listBoxlinesBackground.Items.Clear();
            NikseBitmap nbmp = new NikseBitmap(vobSubImage);
            nbmp.ReplaceTransparentWith(Color.Black);
            vobSubImage = nbmp.GetBitmap();

            radioButtonHot.Checked = true;
            ShrinkSelection = false;
            ExpandSelection = false;

            textBoxCharacters.Text = string.Empty;
            _nocrChar = new NOcrChar { MarginTop = character.Y - character.ParentY };
            _imageWidth = character.NikseBitmap.Width;
            _imageHeight = character.NikseBitmap.Height;
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
            pictureBoxCharacter.Image = character.NikseBitmap.GetBitmap();

            Bitmap org = (Bitmap)vobSubImage.Clone();
            Bitmap bm = new Bitmap(org.Width, org.Height);
            Graphics g = Graphics.FromImage(bm);
            g.DrawImage(org, 0, 0, org.Width, org.Height);
            g.DrawRectangle(Pens.Red, character.X, character.Y, character.NikseBitmap.Width, character.NikseBitmap.Height - 1);
            g.Dispose();
            pictureBoxSubtitleImage.Image = bm;

            pictureBoxCharacter.Top = labelCharacters.Top + 16;
            SizePictureBox();
            checkBoxItalic.Checked = italicChecked;

            _history = new List<NOcrChar>();
            _historyIndex = -1;

            _nocrChar.Width = _imageWidth;
            _nocrChar.Height = _imageHeight;
            GenerateLineSegments(150, false, _nocrChar, new NikseBitmap(pictureBoxCharacter.Image as Bitmap));
            ShowOcrPoints();
            pictureBoxCharacter.Invalidate();
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
                foreach (Point point in op.GetPoints())
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
                    {
                        end.X++;
                    }

                    e.Graphics.DrawLine(foreground, start, end);
                    if (op == selectedPoint)
                    {
                        e.Graphics.DrawLine(selPenF, op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                    }
                }
                foreach (NOcrPoint op in _nocrChar.LinesBackground)
                {
                    Point start = op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    Point end = op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    e.Graphics.DrawLine(background, start, end);
                    if (op == selectedPoint)
                    {
                        e.Graphics.DrawLine(selPenB, op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                    }
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
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.A)
            {
                _nocrChar.Width = _imageWidth;
                _nocrChar.Height = _imageHeight;
                GenerateLineSegments(150, false, _nocrChar, new NikseBitmap(pictureBoxCharacter.Image as Bitmap));
                ShowOcrPoints();
                pictureBoxCharacter.Invalidate();
                Redo();
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Y)
            {
                e.SuppressKeyPress = true;
                Redo();
            }
            if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Left && buttonShrinkSelection.Visible)
            {
                buttonShrinkSelection_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Right && buttonExpandSelection.Visible)
            {
                buttonExpandSelection_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Subtract && buttonShrinkSelection.Visible)
            {
                buttonShrinkSelection_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Add && buttonExpandSelection.Visible)
            {
                buttonExpandSelection_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }

        private static bool IsMatchPointForeGround(NOcrPoint op, bool loose, NikseBitmap nbmp, NOcrChar nOcrChar)
        {
            if (Math.Abs(op.Start.X - op.End.X) < 2 && Math.Abs(op.End.Y - op.Start.Y) < 2)
            {
                return false;
            }

            foreach (Point point in op.ScaledGetPoints(nOcrChar, nbmp.Width, nbmp.Height))
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

                    if (loose)
                    {
                        if (nbmp.Width > 10 && point.X + 1 < nbmp.Width)
                        {
                            c = nbmp.GetPixel(point.X + 1, point.Y);
                            if (c.A > 150 && c.R + 100 + c.G + c.B > VobSubOcr.NocrMinColor)
                            {
                            }
                            else
                            {
                                return false;
                            }
                        }

                        if (nbmp.Width > 10 && point.X >= 1)
                        {
                            c = nbmp.GetPixel(point.X - 1, point.Y);
                            if (c.A > 150 && c.R + 100 + c.G + c.B > VobSubOcr.NocrMinColor)
                            {
                            }
                            else
                            {
                                return false;
                            }
                        }

                        if (nbmp.Height > 10 && point.Y + 1 < nbmp.Height)
                        {
                            c = nbmp.GetPixel(point.X, point.Y + 1);
                            if (c.A > 150 && c.R + 100 + c.G + c.B > VobSubOcr.NocrMinColor)
                            {
                            }
                            else
                            {
                                return false;
                            }
                        }

                        if (nbmp.Height > 10 && point.Y >= 1)
                        {
                            c = nbmp.GetPixel(point.X, point.Y - 1);
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
            }
            return true;
        }

        private static bool IsMatchPointBackGround(NOcrPoint op, bool loose, NikseBitmap nbmp, NOcrChar nOcrChar)
        {
            foreach (Point point in op.ScaledGetPoints(nOcrChar, nbmp.Width, nbmp.Height))
            {
                if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                {
                    Color c = nbmp.GetPixel(point.X, point.Y);
                    if (c.A > 150 && c.R + 100 + c.G + c.B > VobSubOcr.NocrMinColor)
                    {
                        return false;
                    }

                    if (nbmp.Width > 10 && point.X + 1 < nbmp.Width)
                    {
                        c = nbmp.GetPixel(point.X + 1, point.Y);
                        if (c.A > 150 && c.R + 100 + c.G + c.B > VobSubOcr.NocrMinColor)
                        {
                            return false;
                        }
                    }

                    if (loose)
                    {
                        if (nbmp.Width > 10 && point.X >= 1)
                        {
                            c = nbmp.GetPixel(point.X - 1, point.Y);
                            if (c.A > 150 && c.R + 100 + c.G + c.B > VobSubOcr.NocrMinColor)
                            {
                                return false;
                            }
                        }

                        if (nbmp.Height > 10 && point.Y + 1 < nbmp.Height)
                        {
                            c = nbmp.GetPixel(point.X, point.Y + 1);
                            if (c.A > 150 && c.R + 100 + c.G + c.B > VobSubOcr.NocrMinColor)
                            {
                                return false;
                            }
                        }

                        if (nbmp.Height > 10 && point.Y >= 1)
                        {
                            c = nbmp.GetPixel(point.X, point.Y - 1);
                            if (c.A > 150 && c.R + 100 + c.G + c.B > VobSubOcr.NocrMinColor)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static void GenerateLineSegments(int numberOfLines, bool veryPrecise, NOcrChar nOcrChar, NikseBitmap nbmp)
        {
            const int giveUpCount = 10000;
            var r = new Random();
            int count = 0;
            int hits = 0;
            bool tempVeryPrecise = veryPrecise;
            while (hits < numberOfLines && count < giveUpCount)
            {
                var start = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                var end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));

                if (hits < 5 && count < 100) // a few large lines
                {
                    for (int k = 0; k < 500; k++)
                    {
                        if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) > nOcrChar.Height / 2)
                        {
                            break;
                        }
                        else
                        {
                            end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                        }
                    }
                }
                else // and a lot of small lines
                {
                    for (int k = 0; k < 500; k++)
                    {
                        if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 5)
                        {
                            break;
                        }
                        else
                        {
                            end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                        }
                    }
                }

                var op = new NOcrPoint(start, end);
                bool ok = true;
                foreach (NOcrPoint existingOp in nOcrChar.LinesForeground)
                {
                    if (existingOp.Start.X == op.Start.X && existingOp.Start.Y == op.Start.Y &&
                        existingOp.End.X == op.End.X && existingOp.End.Y == op.End.Y)
                    {
                        ok = false;
                    }
                }
                if (ok && IsMatchPointForeGround(op, !tempVeryPrecise, nbmp, nOcrChar))
                {
                    nOcrChar.LinesForeground.Add(op);
                    hits++;
                }
                count++;
                if (count > giveUpCount - 100 && !tempVeryPrecise)
                {
                    tempVeryPrecise = true;
                }
            }

            count = 0;
            hits = 0;
            tempVeryPrecise = veryPrecise;
            while (hits < numberOfLines && count < giveUpCount)
            {
                var start = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                var end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));

                if (hits < 5 && count < 100) // a few large lines
                {
                    for (int k = 0; k < 500; k++)
                    {
                        if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) > nOcrChar.Height / 2)
                        {
                            break;
                        }
                        else
                        {
                            end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                        }
                    }
                }
                else // and a lot of small lines
                {
                    for (int k = 0; k < 500; k++)
                    {
                        if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 5)
                        {
                            break;
                        }
                        else
                        {
                            end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                        }
                    }
                }

                var op = new NOcrPoint(start, end);
                bool ok = true;
                foreach (NOcrPoint existingOp in nOcrChar.LinesBackground)
                {
                    if (existingOp.Start.X == op.Start.X && existingOp.Start.Y == op.Start.Y &&
                        existingOp.End.X == op.End.X && existingOp.End.Y == op.End.Y)
                    {
                        ok = false;
                    }
                }
                if (ok && IsMatchPointBackGround(op, !tempVeryPrecise, nbmp, nOcrChar))
                {
                    nOcrChar.LinesBackground.Add(op);
                    hits++;
                }
                count++;

                if (count > giveUpCount - 100 && !tempVeryPrecise)
                {
                    tempVeryPrecise = true;
                }
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
                labelCharactersAsText.Font = new Font(labelCharactersAsText.Font.FontFamily, labelCharactersAsText.Font.Size, FontStyle.Italic);
                textBoxCharacters.Font = new Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size, FontStyle.Italic);
                labelItalicOn.Visible = true;
            }
            else
            {
                labelCharactersAsText.Font = new Font(labelCharactersAsText.Font.FontFamily, labelCharactersAsText.Font.Size);
                textBoxCharacters.Font = new Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size);
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

        private void InsertLetter(object sender, EventArgs e)
        {
            textBoxCharacters.SelectedText = (sender as ToolStripMenuItem).Text;
        }

        private void toolStripMenuItemMusicSymbol1_Click(object sender, EventArgs e)
        {
            textBoxCharacters.SelectedText = (sender as ToolStripMenuItem).Text;
        }

        private void toolStripMenuItemMusicSymbol2_Click(object sender, EventArgs e)
        {
            textBoxCharacters.SelectedText = (sender as ToolStripMenuItem).Text;
        }

    }
}

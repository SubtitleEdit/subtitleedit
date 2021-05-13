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
    public partial class VobSubOcrNOcrCharacter : Form
    {
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
            VobSubEditCharacters.MakeToolStripLetters(contextMenuStripLetters, InsertLanguageCharacter);
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonCancel);
            checkBoxAutoSubmitOfFirstChar.Text = LanguageSettings.Current.VobSubOcrCharacter.AutoSubmitOnFirstChar;
            labelItalicOn.Visible = false;
        }

        private void InsertLanguageCharacter(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem toolStripMenuItem)
            {
                var start = textBoxCharacters.SelectionStart;
                textBoxCharacters.SelectedText = toolStripMenuItem.Text;
                textBoxCharacters.SelectionLength = 0;
                textBoxCharacters.SelectionStart = start + toolStripMenuItem.Text.Length;
            }
        }

        public NOcrChar NOcrChar { get; private set; }

        public Point FormPosition => new Point(Left, Top);

        public bool ExpandSelection { get; private set; }

        public bool ShrinkSelection { get; private set; }

        public bool IsItalic => checkBoxItalic.Checked;

        internal void Initialize(Bitmap vobSubImage, ImageSplitterItem character, Point position, bool italicChecked, bool showExpand, bool showShrink, string text)
        {
            listBoxLinesForeground.Items.Clear();
            listBoxlinesBackground.Items.Clear();
            var nbmp = new NikseBitmap(vobSubImage);
            vobSubImage = nbmp.GetBitmap();

            radioButtonHot.Checked = true;
            ShrinkSelection = false;
            ExpandSelection = false;

            textBoxCharacters.Text = text;
            NOcrChar = new NOcrChar { MarginTop = character.Top };
            _imageWidth = character.NikseBitmap.Width;
            _imageHeight = character.NikseBitmap.Height;
            _drawLineOn = false;
            _warningNoNotForegroundLinesShown = false;

            buttonExpandSelection.Visible = showExpand;
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

            NOcrChar.Width = _imageWidth;
            NOcrChar.Height = _imageHeight;
            numericUpDownLinesToDraw.Value = Configuration.Settings.VobSubOcr.LineOcrLinesToAutoGuess;
            GenerateLineSegments((int)numericUpDownLinesToDraw.Value, false, NOcrChar, new NikseBitmap(pictureBoxCharacter.Image as Bitmap));
            ShowOcrPoints();
            pictureBoxCharacter.Invalidate();
            numericUpDownLinesToDraw.ValueChanged += (sender, args) =>
            {
                Configuration.Settings.VobSubOcr.LineOcrLinesToAutoGuess = (int)numericUpDownLinesToDraw.Value;
                buttonGuessAgain_Click(null, null);
            };

            textBoxCharacters.Focus();
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
            var nbmp = new NikseBitmap(pictureBoxCharacter.Image as Bitmap);
            foreach (var op in NOcrChar.LinesForeground)
            {
                foreach (var point in op.ScaledGetPoints(NOcrChar, nbmp.Width, nbmp.Height))
                {
                    if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                    {
                        var c = nbmp.GetPixel(point.X, point.Y);
                        if (c.A > 150)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            foreach (var op in NOcrChar.LinesBackground)
            {
                foreach (var point in op.GetPoints())
                {
                    if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                    {
                        var c = nbmp.GetPixel(point.X, point.Y);
                        if (c.A > 150)
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
            NOcrChar.Text = textBoxCharacters.Text;
            NOcrChar.Italic = checkBoxItalic.Checked;
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
                foreach (NOcrPoint op in NOcrChar.LinesForeground)
                {
                    Point start = op.GetScaledStart(NOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    Point end = op.GetScaledEnd(NOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    if (start.X == end.X && start.Y == end.Y)
                    {
                        end.X++;
                    }

                    e.Graphics.DrawLine(foreground, start, end);
                    if (op == selectedPoint)
                    {
                        e.Graphics.DrawLine(selPenF, op.GetScaledStart(NOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(NOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                    }
                }
                foreach (NOcrPoint op in NOcrChar.LinesBackground)
                {
                    Point start = op.GetScaledStart(NOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    Point end = op.GetScaledEnd(NOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height);
                    e.Graphics.DrawLine(background, start, end);
                    if (op == selectedPoint)
                    {
                        e.Graphics.DrawLine(selPenB, op.GetScaledStart(NOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(NOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
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
            foreach (NOcrPoint op in NOcrChar.LinesForeground)
            {
                listBoxLinesForeground.Items.Add(op);
            }
            listBoxlinesBackground.Items.Clear();
            foreach (NOcrPoint op in NOcrChar.LinesBackground)
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
                    NOcrChar.Width = pictureBoxCharacter.Image.Width;
                    NOcrChar.Height = pictureBoxCharacter.Image.Height;
                    if (radioButtonHot.Checked)
                    {
                        NOcrChar.LinesForeground.Add(new NOcrPoint(_start, _end));
                    }
                    else
                    {
                        NOcrChar.LinesBackground.Add(new NOcrPoint(_start, _end));
                    }

                    _drawLineOn = false;
                    pictureBoxCharacter.Invalidate();
                    ShowOcrPoints();
                    AddHistoryItem(NOcrChar);

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
                NOcrChar = new NOcrChar(_history[_historyIndex]);
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
                NOcrChar = new NOcrChar(_history[_historyIndex]);
            }
            else if (_historyIndex == 0)
            {
                var c = new NOcrChar(NOcrChar);
                c.LinesForeground.Clear();
                c.LinesBackground.Clear();
                NOcrChar = c;
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
                NOcrChar.Width = _imageWidth;
                NOcrChar.Height = _imageHeight;
                GenerateLineSegments((int)numericUpDownLinesToDraw.Value, false, NOcrChar, new NikseBitmap(pictureBoxCharacter.Image as Bitmap));
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
                    if (c.A > 150)
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
                            if (c.A > 150)
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
                            if (c.A > 150)
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
                            if (c.A > 150)
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
                            if (c.A > 150)
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
                    if (c.A > 150)
                    {
                        return false;
                    }

                    if (nbmp.Width > 10 && point.X + 1 < nbmp.Width)
                    {
                        c = nbmp.GetPixel(point.X + 1, point.Y);
                        if (c.A > 150)
                        {
                            return false;
                        }
                    }

                    if (loose)
                    {
                        if (nbmp.Width > 10 && point.X >= 1)
                        {
                            c = nbmp.GetPixel(point.X - 1, point.Y);
                            if (c.A > 150)
                            {
                                return false;
                            }
                        }

                        if (nbmp.Height > 10 && point.Y + 1 < nbmp.Height)
                        {
                            c = nbmp.GetPixel(point.X, point.Y + 1);
                            if (c.A > 150)
                            {
                                return false;
                            }
                        }

                        if (nbmp.Height > 10 && point.Y >= 1)
                        {
                            c = nbmp.GetPixel(point.X, point.Y - 1);
                            if (c.A > 150)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static void GenerateLineSegments(int maxNumberOfLines, bool veryPrecise, NOcrChar nOcrChar, NikseBitmap nbmp)
        {
            const int giveUpCount = 15000;
            var r = new Random();
            int count = 0;
            int hits = 0;
            bool tempVeryPrecise = veryPrecise;
            int verticalLineX = 2;
            int horizontalLineY = 2;
            while (hits < maxNumberOfLines && count < giveUpCount)
            {
                var start = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                var end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));

                if (hits < 5 && count < 200 && nOcrChar.Width > 4 && nOcrChar.Height > 4) // vertical lines
                {
                    start = new Point(0, 0);
                    end = new Point(0, 0);
                    for (; verticalLineX < nOcrChar.Width - 3; verticalLineX += 1)
                    {
                        start = new Point(verticalLineX, 2);
                        end = new Point(verticalLineX, nOcrChar.Height - 3);

                        if (IsMatchPointForeGround(new NOcrPoint(start, end), true, nbmp, nOcrChar))
                        {
                            verticalLineX++;
                            break;
                        }
                    }
                }
                else if (hits < 10 && count < 400 && nOcrChar.Width > 4 && nOcrChar.Height > 4) // horizontal lines
                {
                    start = new Point(0, 0);
                    end = new Point(0, 0);
                    for (; horizontalLineY < nOcrChar.Height - 3; horizontalLineY += 1)
                    {
                        start = new Point(2, horizontalLineY);
                        end = new Point(nOcrChar.Width - 3, horizontalLineY);

                        if (IsMatchPointForeGround(new NOcrPoint(start, end), true, nbmp, nOcrChar))
                        {
                            horizontalLineY++;
                            break;
                        }
                    }
                }
                else if (hits < 20 && count < 2000) // a few large lines
                {
                    for (int k = 0; k < 500; k++)
                    {
                        if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) > nOcrChar.Height / 2)
                        {
                            break;
                        }

                        end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                    }
                }
                else if (hits < 30 && count < 3000) // some medium lines
                {
                    for (int k = 0; k < 500; k++)
                    {
                        if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 15)
                        {
                            break;
                        }

                        end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                    }
                }
                else // and a lot of small lines
                {
                    for (int k = 0; k < 500; k++)
                    {
                        if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 15)
                        {
                            break;
                        }

                        end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
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

                if (end.X == start.X && end.Y == start.Y)
                {
                    ok = false;
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
            horizontalLineY = 2;
            tempVeryPrecise = veryPrecise;
            while (hits < maxNumberOfLines && count < giveUpCount)
            {
                var start = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
                var end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));

                if (hits < 5 && count < 400 && nOcrChar.Width > 4 && nOcrChar.Height > 4) // horizontal lines
                {
                    for (; horizontalLineY < nOcrChar.Height - 3; horizontalLineY += 1)
                    {
                        start = new Point(2, horizontalLineY);
                        end = new Point(nOcrChar.Width - 2, horizontalLineY);

                        if (IsMatchPointBackGround(new NOcrPoint(start, end), true, nbmp, nOcrChar))
                        {
                            horizontalLineY++;
                            break;
                        }
                    }
                }
                if (hits < 10 && count < 1000) // a few large lines
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
                else if (hits < 30 && count < 2000) // some medium lines
                {
                    for (int k = 0; k < 500; k++)
                    {
                        if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 15)
                        {
                            break;
                        }

                        end = new Point(r.Next(nOcrChar.Width), r.Next(nOcrChar.Height));
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

            RemoveDuplicates(nOcrChar.LinesForeground);
            RemoveDuplicates(nOcrChar.LinesBackground);
        }

        private static void RemoveDuplicates(List<NOcrPoint> lines)
        {
            var indicesToDelete = new List<int>();
            for (var index = 0; index < lines.Count; index++)
            {
                var outerPoint = lines[index];
                for (var innerIndex = 0; innerIndex < lines.Count; innerIndex++)
                {
                    var innerPoint = lines[innerIndex];
                    if (innerPoint != outerPoint)
                    {
                        if (innerPoint.Start.X == innerPoint.End.X && outerPoint.Start.X == outerPoint.End.X && innerPoint.Start.X == outerPoint.Start.X)
                        {
                            // same y
                            if (Math.Max(innerPoint.Start.Y, innerPoint.End.Y) <= Math.Max(outerPoint.Start.Y, outerPoint.End.Y) &&
                                Math.Min(innerPoint.Start.Y, innerPoint.End.Y) >= Math.Min(outerPoint.Start.Y, outerPoint.End.Y))
                            {
                                if (!indicesToDelete.Contains(innerIndex))
                                {
                                    indicesToDelete.Add(innerIndex);
                                }
                            }
                        }
                        else if (innerPoint.Start.Y == innerPoint.End.Y && outerPoint.Start.Y == outerPoint.End.Y && innerPoint.Start.Y == outerPoint.Start.Y)
                        {
                            // same x
                            if (Math.Max(innerPoint.Start.X, innerPoint.End.X) <= Math.Max(outerPoint.Start.X, outerPoint.End.X) &&
                                Math.Min(innerPoint.Start.X, innerPoint.End.X) >= Math.Min(outerPoint.Start.X, outerPoint.End.X))
                            {
                                if (!indicesToDelete.Contains(innerIndex))
                                {
                                    indicesToDelete.Add(innerIndex);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var i in indicesToDelete.OrderByDescending(p => p))
            {
                lines.RemoveAt(i);
            }
        }

        private void removeBackgroundLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxlinesBackground.SelectedItems.Count == 1)
            {
                var idx = listBoxlinesBackground.SelectedIndex;
                var op = listBoxlinesBackground.Items[idx] as NOcrPoint;
                NOcrChar.LinesBackground.Remove(op);
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
            else
            {
                ShowOcrPoints();
            }
        }

        private void removeForegroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxLinesForeground.SelectedItems.Count == 1)
            {
                var idx = listBoxLinesForeground.SelectedIndex;
                var op = listBoxLinesForeground.Items[idx] as NOcrPoint;
                NOcrChar.LinesForeground.Remove(op);
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
            else
            {
                ShowOcrPoints();
            }
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
                textBoxCharacters.Font = new Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size, FontStyle.Italic | FontStyle.Bold);
                labelItalicOn.Visible = true;
            }
            else
            {
                labelCharactersAsText.Font = new Font(labelCharactersAsText.Font.FontFamily, labelCharactersAsText.Font.Size);
                textBoxCharacters.Font = new Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size, FontStyle.Bold);
                labelItalicOn.Visible = false;
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

        private void textBoxCharacters_KeyUp(object sender, KeyEventArgs e)
        {
            if (checkBoxAutoSubmitOfFirstChar.Checked && textBoxCharacters.Text.Length > 0)
            {
                buttonOK_Click(null, null);
            }
        }

        private void checkBoxAutoSubmitOfFirstChar_CheckedChanged(object sender, EventArgs e)
        {
            textBoxCharacters.Focus();
        }

        private void VobSubOcrNOcrCharacter_Load(object sender, EventArgs e)
        {
            textBoxCharacters.Focus();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NOcrChar.LinesForeground.Clear();
            ShowOcrPoints();
        }

        private void clearToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            NOcrChar.LinesBackground.Clear();
            ShowOcrPoints();
        }

        private void buttonGuessAgain_Click(object sender, EventArgs e)
        {
            listBoxLinesForeground.BeginUpdate();
            listBoxlinesBackground.BeginUpdate();
            NOcrChar.LinesForeground.Clear();
            NOcrChar.LinesBackground.Clear();
            GenerateLineSegments((int)numericUpDownLinesToDraw.Value, false, NOcrChar, new NikseBitmap(pictureBoxCharacter.Image as Bitmap));
            ShowOcrPoints();
            listBoxLinesForeground.EndUpdate();
            listBoxlinesBackground.EndUpdate();
        }

        private void buttonAbort_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
        }

        private void listBoxLinesForeground_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void listBoxLinesForeground_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                removeForegroundToolStripMenuItem_Click(null, null);
            }
        }

        private void listBoxLinesBackground_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                removeBackgroundLineToolStripMenuItem_Click(null, null);
            }
        }

        private void VobSubOcrNOcrCharacter_Shown(object sender, EventArgs e)
        {
            textBoxCharacters.Focus();
        }
    }
}

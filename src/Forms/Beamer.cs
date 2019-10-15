using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class Beamer : Form
    {
        private readonly Subtitle _subtitle;
        private int _index;
        private bool _fullscreen;
        private Color _subtitleColor;
        private string _subtitleFontName;
        private float _subtitleFontSize;
        private Color _borderColor;
        private float _borderWidth;
        private readonly bool _isLoading;
        private int _marginLeft;
        private int _marginBottom = 25;
        private int _showIndex = -2;
        private double _millisecondsFactor = 1.0;
        private readonly Main _main;
        private bool _noTimerAction;
        private long _videoStartTick;
        private readonly Keys _mainGeneralGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);

        public Beamer(Main main, Subtitle subtitle, int index)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _main = main;
            _subtitle = subtitle;
            _index = index;
            _isLoading = true;
            _subtitleColor = Color.White;
            _subtitleFontName = "Verdana";
            _subtitleFontSize = 75.0f;
            _borderColor = Color.Black;
            _borderWidth = 2.0f;

            var language = Configuration.Settings.Language.Beamer;
            Text = language.Title;
            groupBoxImageSettings.Text = Configuration.Settings.Language.ExportPngXml.ImageSettings;
            labelSubtitleFont.Text = Configuration.Settings.Language.ExportPngXml.FontFamily;
            labelSubtitleFontSize.Text = Configuration.Settings.Language.ExportPngXml.FontSize;
            buttonColor.Text = Configuration.Settings.Language.ExportPngXml.FontColor;
            buttonBorderColor.Text = Configuration.Settings.Language.ExportPngXml.BorderColor;
            labelBorderWidth.Text = Configuration.Settings.Language.ExportPngXml.BorderWidth;

            _subtitleFontName = Configuration.Settings.SubtitleBeaming.FontName;
            _subtitleFontSize = Configuration.Settings.SubtitleBeaming.FontSize;
            if (_subtitleFontSize > 100 || _subtitleFontSize < 10)
            {
                _subtitleFontSize = 60;
            }
            _subtitleColor = Configuration.Settings.SubtitleBeaming.FontColor;
            _borderColor = Configuration.Settings.SubtitleBeaming.BorderColor;
            _borderWidth = Configuration.Settings.SubtitleBeaming.BorderWidth;

            panelColor.BackColor = _subtitleColor;
            panelBorderColor.BackColor = _borderColor;

            if (Configuration.Settings.SubtitleBeaming.BorderWidth > 0 && Configuration.Settings.SubtitleBeaming.BorderWidth < 5)
            {
                comboBoxBorderWidth.SelectedIndex = (int)_borderWidth;
            }
            else
            {
                comboBoxBorderWidth.SelectedIndex = 2;
            }

            comboBoxHAlign.SelectedIndexChanged -= ComboBoxHAlignSelectedIndexChanged;
            comboBoxHAlign.SelectedIndex = 1;
            comboBoxHAlign.SelectedIndexChanged += ComboBoxHAlignSelectedIndexChanged;

            comboBoxSubtitleFont.SelectedIndexChanged -= ComboBoxSubtitleFontSizeSelectedIndexChanged;
            foreach (var x in FontFamily.Families)
            {
                comboBoxSubtitleFont.Items.Add(x.Name);
                if (x.Name.Equals(_subtitleFontName, StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
                }
            }
            comboBoxSubtitleFont.SelectedIndexChanged += ComboBoxSubtitleFontSizeSelectedIndexChanged;

            comboBoxSubtitleFontSize.SelectedIndex = (_subtitleFontSize >= 10 && _subtitleFontSize <= 100) ? (int)(_subtitleFontSize - 10) : 40;
            _isLoading = false;
            ShowCurrent();
        }

        private void ButtonColorClick(object sender, EventArgs e)
        {
            colorDialog1.Color = panelColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelColor.BackColor = colorDialog1.Color;
                ShowCurrent();
            }
        }

        private void ButtonBorderColorClick(object sender, EventArgs e)
        {
            colorDialog1.Color = panelBorderColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelBorderColor.BackColor = colorDialog1.Color;
                ShowCurrent();
            }
        }

        private void ComboBoxSubtitleFontSelectedValueChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

        private void ComboBoxSubtitleFontSizeSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

        private void ComboBoxBorderWidthSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

        private void ShowCurrent()
        {
            SetupImageParameters();
            if (_fullscreen)
            {
                if (_index > 0 && _index < _subtitle.Paragraphs.Count)
                {
                    string text = _subtitle.Paragraphs[_index].Text;
                    var bmp = GenerateImageFromTextWithStyle(text);
                    pictureBox1.Image = bmp;
                    pictureBox1.Height = bmp.Height;
                    pictureBox1.Width = bmp.Width;
                    pictureBox1.Left = (Width - bmp.Width) / 2 + _marginLeft;
                    pictureBox1.Top = Height - (pictureBox1.Height + _marginBottom);
                    _showIndex = _index;
                    _main.FocusParagraph(_index);
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
            else
            {
                string text = "Testing 123" + Environment.NewLine + "Subtitle Edit";
                if (_index >= 0 && _index < _subtitle.Paragraphs.Count && _subtitle.Paragraphs[_index].Text.Length > 1)
                {
                    text = _subtitle.Paragraphs[_index].Text;
                    _main.FocusParagraph(_index);
                }
                var bmp = GenerateImageFromTextWithStyle(text);
                pictureBox1.Top = groupBoxImageSettings.Top + groupBoxImageSettings.Height + 5;
                // Alignment direction.
                switch (comboBoxHAlign.SelectedIndex)
                {
                    case 0: // Left.
                        pictureBox1.Left = 5;
                        break;
                    case 1: // Center.
                        pictureBox1.Left = ((groupBoxImageSettings.Width - bmp.Width) / 2);
                        break;
                    case 2: // Right.
                        pictureBox1.Left = groupBoxImageSettings.Width - bmp.Width;
                        break;
                }
                pictureBox1.Image = bmp;
                pictureBox1.Height = bmp.Height;
                pictureBox1.Width = bmp.Width;
                _showIndex = -2;
            }
        }

        private void SetupImageParameters()
        {
            if (_isLoading)
            {
                return;
            }

            _subtitleColor = panelColor.BackColor;
            _borderColor = panelBorderColor.BackColor;
            _subtitleFontName = comboBoxSubtitleFont.SelectedItem.ToString();
            _subtitleFontSize = float.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString());
            _borderWidth = float.Parse(comboBoxBorderWidth.SelectedItem.ToString());
        }

        private Bitmap GenerateImageFromTextWithStyle(string text)
        {
            const bool subtitleFontBold = false;
            bool subtitleAlignLeft = comboBoxHAlign.SelectedIndex == 0;

            // remove styles for display text (except italic)
            text = Utilities.RemoveSsaTags(text);
            text = text.Replace("<b>", string.Empty);
            text = text.Replace("</b>", string.Empty);
            text = text.Replace("<B>", string.Empty);
            text = text.Replace("</B>", string.Empty);
            text = text.Replace("<u>", string.Empty);
            text = text.Replace("</u>", string.Empty);
            text = text.Replace("<U>", string.Empty);
            text = text.Replace("</U>", string.Empty);

            Font font;
            try
            {
                font = new Font(_subtitleFontName, _subtitleFontSize, FontStyle.Regular);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                font = new Font(FontFamily.Families[0].Name, _subtitleFontSize);
            }
            var bmp = new Bitmap(400, 200);
            var g = Graphics.FromImage(bmp);

            SizeF textSize = g.MeasureString("Hj!", font);
            var lineHeight = (textSize.Height * 0.64f);

            textSize = g.MeasureString(HtmlUtil.RemoveHtmlTags(text), font);
            g.Dispose();
            bmp.Dispose();
            int sizeX = (int)(textSize.Width * 0.8) + 40;
            int sizeY = (int)(textSize.Height * 0.8) + 30;
            if (sizeX < 1)
            {
                sizeX = 1;
            }
            if (sizeY < 1)
            {
                sizeY = 1;
            }
            bmp = new Bitmap(sizeX, sizeY);
            g = Graphics.FromImage(bmp);

            var lefts = new List<float>();
            foreach (var line in HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic, HtmlUtil.TagFont).SplitToLines())
            {
                if (subtitleAlignLeft)
                {
                    lefts.Add(5);
                }
                else
                {
                    lefts.Add((float)(bmp.Width - g.MeasureString(line, font).Width * 0.8 + 15) / 2);
                }
            }

            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;

            var sf = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near
            };
            // draw the text to a path
            var path = new GraphicsPath();

            // display italic
            var sb = new StringBuilder();
            int i = 0;
            bool isItalic = false;
            float left = 5;
            if (lefts.Count > 0)
            {
                left = lefts[0];
            }
            float top = 5;
            bool newLine = false;
            int lineNumber = 0;
            float leftMargin = left;
            int newLinePathPoint = -1;
            Color c = _subtitleColor;
            var colorStack = new Stack<Color>();
            var lastText = new StringBuilder();
            while (i < text.Length)
            {
                if (text.Substring(i).StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                {
                    float addLeft = 0;
                    int oldPathPointIndex = path.PointCount;
                    if (oldPathPointIndex < 0)
                    {
                        oldPathPointIndex = 0;
                    }

                    if (sb.Length > 0)
                    {
                        TextDraw.DrawText(font, sf, path, sb, isItalic, subtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                    }
                    if (path.PointCount > 0)
                    {
                        PointF[] list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
                        for (int k = oldPathPointIndex; k < list.Length; k++)
                        {
                            if (list[k].X > addLeft)
                            {
                                addLeft = list[k].X;
                            }
                        }
                    }
                    if (Math.Abs(addLeft) < 0.001)
                    {
                        addLeft = left + 2;
                    }
                    left = addLeft;

                    if (_borderWidth > 0)
                    {
                        g.DrawPath(new Pen(_borderColor, _borderWidth), path);
                    }

                    g.FillPath(new SolidBrush(c), path);
                    path.Reset();
                    path = new GraphicsPath();
                    sb.Clear();

                    int endIndex = text.Substring(i).IndexOf('>');
                    if (endIndex < 0)
                    {
                        i += 9999;
                    }
                    else
                    {
                        string fontContent = text.Substring(i, endIndex);
                        if (fontContent.Contains(" color=", StringComparison.OrdinalIgnoreCase))
                        {
                            var arr = fontContent.Substring(fontContent.IndexOf(" color=", StringComparison.OrdinalIgnoreCase) + 7).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (arr.Length > 0)
                            {
                                string fontColor = arr[0].Trim('\'').Trim('"').Trim('\'');
                                try
                                {
                                    colorStack.Push(c); // save old color
                                    if (fontColor.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
                                    {
                                        arr = fontColor.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                        c = Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                                    }
                                    else
                                    {
                                        c = ColorTranslator.FromHtml(fontColor);
                                    }
                                }
                                catch
                                {
                                    c = _subtitleColor;
                                }
                            }
                        }
                        i += endIndex;
                    }
                }
                else if (text.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                {
                    if (text.Substring(i).ToLowerInvariant().Replace("</font>", string.Empty).Replace("</FONT>", string.Empty).Length > 0)
                    {
                        if (lastText.EndsWith(' ') && !sb.StartsWith(' '))
                        {
                            string t = sb.ToString();
                            sb.Clear();
                            sb.Append(' ');
                            sb.Append(t);
                        }

                        float addLeft = 0;
                        int oldPathPointIndex = path.PointCount - 1;
                        if (oldPathPointIndex < 0)
                        {
                            oldPathPointIndex = 0;
                        }
                        if (sb.Length > 0)
                        {
                            TextDraw.DrawText(font, sf, path, sb, isItalic, subtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                        }
                        if (path.PointCount > 0)
                        {
                            PointF[] list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
                            for (int k = oldPathPointIndex; k < list.Length; k++)
                            {
                                if (list[k].X > addLeft)
                                {
                                    addLeft = list[k].X;
                                }
                            }
                        }
                        if (Math.Abs(addLeft) < 0.001)
                        {
                            addLeft = left + 2;
                        }
                        left = addLeft;

                        if (_borderWidth > 0)
                        {
                            g.DrawPath(new Pen(_borderColor, _borderWidth), path);
                        }
                        g.FillPath(new SolidBrush(c), path);
                        path.Reset();
                        sb.Clear();
                        if (colorStack.Count > 0)
                        {
                            c = colorStack.Pop();
                        }
                    }
                    i += 6;
                }
                else if (text.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                {
                    if (sb.Length > 0)
                    {
                        TextDraw.DrawText(font, sf, path, sb, isItalic, subtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                    }
                    isItalic = true;
                    i += 2;
                }
                else if (text.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase) && isItalic)
                {
                    if (lastText.EndsWith(' ') && !sb.StartsWith(' '))
                    {
                        string t = sb.ToString();
                        sb.Clear();
                        sb.Append(' ');
                        sb.Append(t);
                    }
                    TextDraw.DrawText(font, sf, path, sb, isItalic, subtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                    isItalic = false;
                    i += 3;
                }
                else if (text.Substring(i).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                {
                    TextDraw.DrawText(font, sf, path, sb, isItalic, subtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                    top += lineHeight;
                    newLine = true;
                    i += Environment.NewLine.Length - 1;
                    lineNumber++;
                    if (lineNumber < lefts.Count)
                    {
                        leftMargin = lefts[lineNumber];
                        left = leftMargin;
                    }
                }
                else
                {
                    sb.Append(text[i]);
                }
                i++;
            }
            if (sb.Length > 0)
            {
                TextDraw.DrawText(font, sf, path, sb, isItalic, subtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
            }
            sf.Dispose();

            if (_borderWidth > 0)
            {
                g.DrawPath(new Pen(_borderColor, _borderWidth), path);
            }
            g.FillPath(new SolidBrush(c), path);
            g.Dispose();
            var nbmp = new NikseBitmap(bmp);
            nbmp.CropTransparentSidesAndBottom(2, true);
            return nbmp.GetBitmap();
        }

        private void Timer1Tick(object sender, EventArgs e)
        {
            if (_noTimerAction)
            {
                return;
            }

            double positionInMilliseconds = (DateTime.Now.Ticks - _videoStartTick) / 10000.0D; // 10,000 ticks = 1 millisecond
            positionInMilliseconds *= _millisecondsFactor;
            int index = 0;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (p.StartTime.TotalMilliseconds <= positionInMilliseconds &&
                    p.EndTime.TotalMilliseconds > positionInMilliseconds)
                {
                    break;
                }
                index++;
            }
            if (index == _subtitle.Paragraphs.Count)
            {
                index = -1;
            }
            if (index == -1)
            {
                pictureBox1.Image = null;
            }
            else if (index != _showIndex)
            {
                _index = index;
                ShowCurrent();
            }
        }

        private void ButtonStartClick(object sender, EventArgs e)
        {
            if (_index >= _subtitle.Paragraphs.Count - 1)
            {
                _index = -1;
                _videoStartTick = DateTime.Now.Ticks;
            }
            else if (_index >= 0)
            {
                _videoStartTick = DateTime.Now.Ticks - ((long)(_subtitle.Paragraphs[_index].StartTime.TotalMilliseconds) * 10000); //10,000 ticks = 1 millisecond
            }

            groupBoxImageSettings.Hide();
            buttonStart.Hide();
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            WindowState = FormWindowState.Maximized;
            _fullscreen = true;
            pictureBox1.Image = null;
            Cursor.Hide();
            _marginBottom = Height - 200;
            timer1.Start();
        }

        private void BeamerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Home)
            {
                _index = 0;
                ShowCurrent();
                e.Handled = true;
                return;
            }
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.End)
            {
                _index = _subtitle.Paragraphs.Count - 1;
                ShowCurrent();
                e.Handled = true;
                return;
            }

            if (!_fullscreen)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                }
                else if (e.KeyCode == Keys.Space || (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt) || _mainGeneralGoToPrevSubtitle == e.KeyData)
                {
                    if (_index < _subtitle.Paragraphs.Count - 1)
                    {
                        _index++;
                    }
                    ShowCurrent();
                    e.Handled = true;
                }
                else if (_mainGeneralGoToPrevSubtitle == e.KeyData || (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt))
                {
                    if (_index > 0)
                    {
                        _index--;
                    }
                    ShowCurrent();
                    e.Handled = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageDown)
                {
                    if (_index < _subtitle.Paragraphs.Count - 21)
                    {
                        _index += 20;
                    }
                    else
                    {
                        _index = _subtitle.Paragraphs.Count - 1;
                    }
                    ShowCurrent();
                    e.Handled = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageUp)
                {
                    if (_index > 20)
                    {
                        _index -= 20;
                    }
                    else
                    {
                        _index = 0;
                    }
                    ShowCurrent();
                    e.Handled = true;
                }
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                groupBoxImageSettings.Show();
                buttonStart.Show();
                timer1.Stop();
                Cursor.Show();
                FormBorderStyle = FormBorderStyle.FixedDialog;
                BackColor = DefaultBackColor;
                WindowState = FormWindowState.Normal;
                _showIndex = -2;
                _fullscreen = false;
                ShowCurrent();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Pause)
            {
                timer1.Stop();
                timer1.Enabled = false;
            }
            else if (e.KeyCode == Keys.Space || (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt))
            {
                bool timer1Enabled = timer1.Enabled;
                timer1.Enabled = false;
                System.Threading.Thread.Sleep(100);
                if (_index < _subtitle.Paragraphs.Count - 1)
                {
                    _index++;
                }
                _videoStartTick = DateTime.Now.Ticks - ((long)(_subtitle.Paragraphs[_index].StartTime.TotalMilliseconds) * 10000); //10,000 ticks = 1 millisecond

                ShowCurrent();
                _noTimerAction = false;
                if (timer1Enabled || _fullscreen)
                {
                    timer1.Start();
                }

                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
            {
                bool timer1Enabled = timer1.Enabled;
                timer1.Enabled = false;
                System.Threading.Thread.Sleep(100);
                if (_index > 0)
                {
                    _index--;
                }
                _videoStartTick = DateTime.Now.Ticks - ((long)(_subtitle.Paragraphs[_index].StartTime.TotalMilliseconds) * 10000); //10,000 ticks = 1 millisecond
                ShowCurrent();
                if (timer1Enabled)
                {
                    timer1.Start();
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageDown)
            {
                if (_index < _subtitle.Paragraphs.Count - 21)
                {
                    _index += 20;
                }
                else
                {
                    _index = _subtitle.Paragraphs.Count - 1;
                }
                ShowCurrent();
                e.Handled = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageUp)
            {
                if (_index > 20)
                {
                    _index -= 20;
                }
                else
                {
                    _index = 0;
                }
                ShowCurrent();
                e.Handled = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Add)
            {
                if (comboBoxSubtitleFontSize.SelectedIndex < comboBoxSubtitleFontSize.Items.Count - 1)
                {
                    comboBoxSubtitleFontSize.SelectedIndex = comboBoxSubtitleFontSize.SelectedIndex + 1;
                    ShowCurrent();
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Subtract)
            {
                if (comboBoxSubtitleFontSize.SelectedIndex > 0)
                {
                    comboBoxSubtitleFontSize.SelectedIndex = comboBoxSubtitleFontSize.SelectedIndex - 1;
                    ShowCurrent();
                }
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Add)
            {
                _millisecondsFactor += 0.001;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Subtract)
            {
                _millisecondsFactor -= 0.001;
            }
            else if (e.KeyCode == Keys.Up)
            {
                _marginBottom++;
                ShowCurrent();
            }
            else if (e.KeyCode == Keys.Down)
            {
                _marginBottom--;
                ShowCurrent();
            }
            else if (e.KeyCode == Keys.Left)
            {
                _marginLeft--;
                ShowCurrent();
            }
            else if (e.KeyCode == Keys.Right)
            {
                _marginLeft++;
                ShowCurrent();
            }
            else if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {
                timer1.Stop();
                int temp = _index;
                _index = -1;
                try
                {
                    ShowCurrent();
                }
                finally
                {
                    _index = temp;
                }
            }
        }

        private void BeamerFormClosing(object sender, FormClosingEventArgs e)
        {
            Cursor.Show();

            // Save user-configurations.
            Configuration.Settings.SubtitleBeaming.FontName = _subtitleFontName;
            Configuration.Settings.SubtitleBeaming.FontSize = (int)_subtitleFontSize;
            Configuration.Settings.SubtitleBeaming.FontColor = _subtitleColor;
            Configuration.Settings.SubtitleBeaming.BorderColor = _borderColor;
            Configuration.Settings.SubtitleBeaming.BorderWidth = (int)_borderWidth;
        }

        private void ComboBoxHAlignSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

    }
}

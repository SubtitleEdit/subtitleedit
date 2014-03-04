using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.OCR;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VobSubNOcrTrain : Form
    {

        private Color _subtitleColor = Color.White;
        private string _subtitleFontName = "Verdana";
        private float _subtitleFontSize = 25.0f;
        private Color _borderColor = Color.Black;
        private float _borderWidth = 2.0f;

        public VobSubNOcrTrain()
        {
            InitializeComponent();

            labelInfo.Text = string.Empty;
            foreach (var x in FontFamily.Families)
            {
                if (x.IsStyleAvailable(FontStyle.Regular) && x.IsStyleAvailable(FontStyle.Bold))
                {
                    ListViewItem item = new ListViewItem(x.Name);
                    item.Font = new Font(x.Name, 14);
                    listViewFonts.Items.Add(item);
                }
            }
            comboBoxSubtitleFontSize.SelectedIndex = 10;

        }

        internal void Initialize(Logic.OCR.NOcrDb _nOcrDb)
        {
            if (_nOcrDb != null)
            {

            }
        }

        private void buttonInputChoose_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBoxInputFile.Text = openFileDialog1.FileName;
            }
        }

        private void buttonNOcrDbChoose_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBoxNOcrDb.Text = openFileDialog1.FileName;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }




        private void buttonTrain_Click(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists(textBoxInputFile.Text))
            {
                return;
            }

            int numberOfCharactersLeaned = 0;
            int numberOfCharactersSkipped = 0;
            var nOcrD = new NOcrDb(textBoxNOcrDb.Text);
            var lines = new List<string>();
            foreach (string line in System.IO.File.ReadAllLines(textBoxInputFile.Text))
                lines.Add(line);
            var format = new SubRip();
            var sub = new Subtitle();
            format.LoadSubtitle(sub, lines, textBoxInputFile.Text);

            var charactersLearned = new List<string>();
            foreach (ListViewItem item in listViewFonts.Items)
            {

                if (item.Checked)
                {
                    _subtitleFontName = item.Text;
                    _subtitleFontSize = Convert.ToInt32(comboBoxSubtitleFontSize.Items[comboBoxSubtitleFontSize.SelectedIndex].ToString());
                    charactersLearned = new List<string>();

                    foreach (Paragraph p in sub.Paragraphs)
                    {
                        foreach (char ch in p.Text)
                        {
                            string s = ch.ToString();
                            if (s.Trim().Length > 0)
                            {
                                if (!charactersLearned.Contains(s))
                                {
                                    TrainLetter(ref numberOfCharactersLeaned, ref numberOfCharactersSkipped, nOcrD, charactersLearned, s, false);
                                    if (checkBoxBold.Checked)
                                        TrainLetter(ref numberOfCharactersLeaned, ref numberOfCharactersSkipped, nOcrD, charactersLearned, s, true);
                                }
                            }
                        }
                    }
                }
            }
            nOcrD.Save();
        }

        private void TrainLetter(ref int numberOfCharactersLeaned, ref int numberOfCharactersSkipped, NOcrDb nOcrD, List<string> charactersLearned, string s, bool bold)
        {
            Bitmap bmp = GenerateImageFromTextWithStyle(s, bold);
            var nbmp = new NikseBitmap(bmp);
            nbmp.MakeTwoColor(210, 280);
            var list = NikseBitmapImageSplitter.SplitBitmapToLettersNew(nbmp, 10, false, false, 25);
            if (list.Count == 1)
            {
                NOcrChar match = nOcrD.GetMatch(list[0].NikseBitmap);
                if (match == null)
                {

                    pictureBox1.Image = list[0].NikseBitmap.GetBitmap();
                    this.Refresh();
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100);



                    NOcrChar nOcrChar = new NOcrChar(s);
                    nOcrChar.Width = list[0].NikseBitmap.Width;
                    nOcrChar.Height = list[0].NikseBitmap.Height;
                    VobSubOcrNOcrCharacter.GenerateLineSegments((int)numericUpDownSegmentsPerCharacter.Value, checkBoxVeryAccurate.Checked, nOcrChar, list[0].NikseBitmap);
                    nOcrD.Add(nOcrChar);

                    charactersLearned.Add(s);
                    numberOfCharactersLeaned++;
                    labelInfo.Text = string.Format("Now training font '{1}', total characters leaned is {0}, {2} skipped", numberOfCharactersLeaned, _subtitleFontName, numberOfCharactersSkipped);
                    bmp.Dispose();
                }
                else
                {
                    numberOfCharactersSkipped++;
                }
            }
        }

        private Bitmap GenerateImageFromTextWithStyle(string text, bool bold)
        {
            bool subtitleFontBold = bold;
            bool subtitleAlignLeft = true;
            bool subtitleAlignRight = false;

            text = Utilities.RemoveHtmlTags(text);

            Font font;
            try
            {
                var fontStyle = FontStyle.Regular;
                if (subtitleFontBold)
                    fontStyle = FontStyle.Bold;
                font = new Font(_subtitleFontName, _subtitleFontSize, fontStyle);
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

            textSize = g.MeasureString(Utilities.RemoveHtmlTags(text), font);
            g.Dispose();
            bmp.Dispose();
            int sizeX = (int)(textSize.Width * 0.8) + 40;
            int sizeY = (int)(textSize.Height * 0.8) + 30;
            if (sizeX < 1)
                sizeX = 1;
            if (sizeY < 1)
                sizeY = 1;
            bmp = new Bitmap(sizeX, sizeY);
            g = Graphics.FromImage(bmp);

            var lefts = new List<float>();
            foreach (string line in Utilities.RemoveHtmlFontTag(text.Replace("<i>", string.Empty).Replace("</i>", string.Empty)).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (subtitleAlignLeft)
                    lefts.Add(5);
                else if (subtitleAlignRight)
                    lefts.Add(bmp.Width - (TextDraw.MeasureTextWidth(font, line, subtitleFontBold) + 15));
                else
                    lefts.Add((float)(bmp.Width - g.MeasureString(line, font).Width * 0.8 + 15) / 2);
            }

            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;

            var sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;// draw the text to a path
            var path = new GraphicsPath();

            // display italic
            var sb = new StringBuilder();
            int i = 0;
            bool isItalic = false;
            float left = 5;
            if (lefts.Count > 0)
                left = lefts[0];
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
                if (text.Substring(i).StartsWith(Environment.NewLine))
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
                    sb.Append(text.Substring(i, 1));
                }
                i++;
            }
            if (sb.Length > 0)
                TextDraw.DrawText(font, sf, path, sb, isItalic, subtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);

            if (_borderWidth > 0)
                g.DrawPath(new Pen(_borderColor, _borderWidth), path);
            g.FillPath(new SolidBrush(c), path);
            g.Dispose();
            var nbmp = new NikseBitmap(bmp);
            nbmp.CropTransparentSidesAndBottom(2, true);
            return nbmp.GetBitmap();
        }

    }
}

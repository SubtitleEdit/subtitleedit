using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class VobSubNOcrTrain : Form
    {

        private readonly Color _subtitleColor = Color.White;
        private string _subtitleFontName = "Verdana";
        private float _subtitleFontSize = 25.0f;
        private readonly Color _borderColor = Color.Black;
        private const float BorderWidth = 2.0f;

        public VobSubNOcrTrain()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelInfo.Text = string.Empty;
            foreach (var x in FontFamily.Families)
            {
                if (x.IsStyleAvailable(FontStyle.Regular) && x.IsStyleAvailable(FontStyle.Bold))
                {
                    ListViewItem item = new ListViewItem(x.Name) { Font = new Font(x.Name, 14) };
                    listViewFonts.Items.Add(item);
                }
            }
            comboBoxSubtitleFontSize.SelectedIndex = 10;
        }

        internal void Initialize()
        {
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
            if (!File.Exists(textBoxInputFile.Text))
            {
                return;
            }

            int numberOfCharactersLeaned = 0;
            int numberOfCharactersSkipped = 0;
            var nOcrD = new NOcrDb(textBoxNOcrDb.Text);
            var lines = new List<string>();
            foreach (string line in File.ReadAllLines(textBoxInputFile.Text))
            {
                lines.Add(line);
            }

            var format = new SubRip();
            var sub = new Subtitle();
            format.LoadSubtitle(sub, lines, textBoxInputFile.Text);

            foreach (ListViewItem item in listViewFonts.Items)
            {
                if (item.Checked)
                {
                    _subtitleFontName = item.Text;
                    _subtitleFontSize = Convert.ToInt32(comboBoxSubtitleFontSize.Items[comboBoxSubtitleFontSize.SelectedIndex].ToString());
                    var charactersLearned = new List<string>();

                    foreach (Paragraph p in sub.Paragraphs)
                    {
                        foreach (char ch in p.Text)
                        {
                            if (!char.IsWhiteSpace(ch))
                            {
                                var s = ch.ToString();
                                if (!charactersLearned.Contains(s))
                                {
                                    TrainLetter(ref numberOfCharactersLeaned, ref numberOfCharactersSkipped, nOcrD, charactersLearned, s, false);
                                    if (checkBoxBold.Checked)
                                    {
                                        TrainLetter(ref numberOfCharactersLeaned, ref numberOfCharactersSkipped, nOcrD, charactersLearned, s, true);
                                    }
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
            nbmp.MakeTwoColor(280);
            var list = NikseBitmapImageSplitter.SplitBitmapToLettersNew(nbmp, 10, false, false, 25);
            if (list.Count == 1)
            {
                NOcrChar match = nOcrD.GetMatch(list[0].NikseBitmap);
                if (match == null)
                {
                    pictureBox1.Image = list[0].NikseBitmap.GetBitmap();
                    Refresh();
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100);

                    NOcrChar nOcrChar = new NOcrChar(s)
                    {
                        Width = list[0].NikseBitmap.Width,
                        Height = list[0].NikseBitmap.Height
                    };
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

            text = HtmlUtil.RemoveHtmlTags(text);

            Font font;
            try
            {
                var fontStyle = FontStyle.Regular;
                if (subtitleFontBold)
                {
                    fontStyle = FontStyle.Bold;
                }

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
            const float left = 5f;
            float top = 5;
            bool newLine = false;
            const float leftMargin = left;
            int newLinePathPoint = -1;
            Color c = _subtitleColor;
            int textLen = text.Length;
            while (i < textLen)
            {
                char ch = text[i];
                if (ch == '\n' || ch == '\r')
                {
                    TextDraw.DrawText(font, sf, path, sb, false, subtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);

                    top += lineHeight;
                    newLine = true;
                    if (i + 1 < textLen && text[i + 1] == '\n' && text[i] == '\r')
                    {
                        i += 2; // CR+LF (Microsoft Windows)
                    }
                    else
                    {
                        i++; // LF (Unix and Unix-like systems )
                    }
                }
                else
                {
                    sb.Append(ch);
                }
                i++;
            }
            if (sb.Length > 0)
            {
                TextDraw.DrawText(font, sf, path, sb, false, subtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
            }

            sf.Dispose();

            g.DrawPath(new Pen(_borderColor, BorderWidth), path);
            g.FillPath(new SolidBrush(c), path);
            g.Dispose();
            var nbmp = new NikseBitmap(bmp);
            nbmp.CropTransparentSidesAndBottom(2, true);
            return nbmp.GetBitmap();
        }

    }
}

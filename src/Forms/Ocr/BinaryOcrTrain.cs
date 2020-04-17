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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic.Ocr.Binary;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class BinaryOcrTrain : Form
    {
        private readonly Color _subtitleColor = Color.White;
        private string _subtitleFontName = "Verdana";
        private float _subtitleFontSize = 25.0f;
        private readonly Color _borderColor = Color.Black;
        private const float BorderWidth = 2.0f;

        public BinaryOcrTrain()
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
            comboBoxSubtitleFontSize.SelectedIndex = 5;
            comboBoxFontSizeEnd.SelectedIndex = 15;
        }

        private void TrainLetter(ref int numberOfCharactersLeaned, ref int numberOfCharactersSkipped, BinaryOcrDb db, List<string> charactersLearned, string s, bool bold, bool italic)
        {
            Bitmap bmp = GenerateImageFromTextWithStyle(s, bold, italic);
            var nbmp = new NikseBitmap(bmp);
            nbmp.MakeTwoColor(280);
            var list = NikseBitmapImageSplitter.SplitBitmapToLettersNew(nbmp, 10, false, false, 25);
            if (list.Count == 1)
            {
                var match = db.FindExactMatch(new BinaryOcrBitmap(list[0].NikseBitmap));
                if (match < 0)
                {
                    pictureBox1.Image = list[0].NikseBitmap.GetBitmap();
                    labelInfo.Refresh();
                    Application.DoEvents();
                    db.Add(new BinaryOcrBitmap(list[0].NikseBitmap, false, 0, s, 0, 0));
                    charactersLearned.Add(s);
                    numberOfCharactersLeaned++;
                    labelInfo.Text = string.Format("Now training font '{1}', total characters learned is {0}, {2} skipped", numberOfCharactersLeaned, _subtitleFontName, numberOfCharactersSkipped);
                    bmp.Dispose();
                }
                else
                {
                    numberOfCharactersSkipped++;
                }
            }
        }

        private Bitmap GenerateImageFromTextWithStyle(string text, bool bold, bool italic)
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
                    TextDraw.DrawText(font, sf, path, sb, italic, subtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);

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

        private void buttonTrain_Click(object sender, EventArgs e)
        {
            var startFontSize = Convert.ToInt32(comboBoxSubtitleFontSize.Items[comboBoxSubtitleFontSize.SelectedIndex].ToString());
            var endFontSize = Convert.ToInt32(comboBoxFontSizeEnd.Items[comboBoxFontSizeEnd.SelectedIndex].ToString());

            if (!File.Exists(textBoxInputFile.Text))
            {
                MessageBox.Show($"Input file '{textBoxInputFile.Text}' does not exist!");
                return;
            }

            if (!Directory.Exists(textBoxNOcrDb.Text))
            {
                MessageBox.Show($"Output folder '{textBoxNOcrDb.Text}' does not exist!");
                return;
            }

            if (listViewFonts.CheckedItems.Count == 1)
            {
                MessageBox.Show("Please select at least one font!");
                return;
            }

            foreach (ListViewItem fontItem in listViewFonts.CheckedItems)
            {
                _subtitleFontName = fontItem.Text;
                for (_subtitleFontSize = startFontSize; _subtitleFontSize <= endFontSize; _subtitleFontSize++)
                {
                    int numberOfCharactersLeaned = 0;
                    int numberOfCharactersSkipped = 0;
                    var bicDb = new BinaryOcrDb(Path.Combine(textBoxNOcrDb.Text, $"{_subtitleFontName}_{_subtitleFontSize}.db"));
                    var lines = File.ReadAllLines(textBoxInputFile.Text).ToList();
                    var format = new SubRip();
                    var sub = new Subtitle();
                    format.LoadSubtitle(sub, lines, textBoxInputFile.Text);
                    var charactersLearned = new List<string>();
                    foreach (var p in sub.Paragraphs)
                    {
                        foreach (char ch in p.Text)
                        {
                            if (!char.IsWhiteSpace(ch))
                            {
                                var s = ch.ToString();
                                if (!charactersLearned.Contains(s))
                                {
                                    TrainLetter(ref numberOfCharactersLeaned, ref numberOfCharactersSkipped, bicDb, charactersLearned, s, false, false);
                                    TrainLetter(ref numberOfCharactersLeaned, ref numberOfCharactersSkipped, bicDb, charactersLearned, s, false, true);
                                    if (checkBoxBold.Checked)
                                    {
                                        TrainLetter(ref numberOfCharactersLeaned, ref numberOfCharactersSkipped, bicDb, charactersLearned, s, true, false);
                                    }
                                }
                            }
                        }
                    }

                    bicDb.Save();
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonInputChoose_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBoxInputFile.Text = openFileDialog1.FileName;
            }
        }
    }
}

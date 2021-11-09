using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr.Binary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class BinaryOcrTrain : Form
    {
        public readonly List<string> AutoDetectedFonts = new List<string>();
        private BinaryOcrBitmap _autoDetectFontBob;
        private string _autoDetectFontText;

        private readonly Color _subtitleColor = Color.White;
        private string _subtitleFontName = "Verdana";
        private float _subtitleFontSize = 25.0f;
        private readonly Color _borderColor = Color.Black;
        private const float BorderWidth = 2.0f;
        private bool _abort;

        public BinaryOcrTrain()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.VobSubOcr;
            Text = language.OcrTraining;
            groupBoxInput.Text = LanguageSettings.Current.BatchConvert.Input;
            labelSubtitleForTraining.Text = language.SubtitleTrainingFile;
            labelLetterCombi.Text = language.LetterCombinations;
            groupBoxTrainingOptions.Text = language.TrainingOptions;
            labelSubtitleFont.Text = LanguageSettings.Current.Settings.SubtitleFont;
            labelSubtitleFontSize.Text = LanguageSettings.Current.Settings.SubtitleFontSize;
            buttonTrain.Text = language.StartTraining;
            comboBoxSubtitleFontSize.Left = labelSubtitleFontSize.Left + labelSubtitleFontSize.Width + 5;

            labelInfo.Text = string.Empty;
            var selectedFonts = Configuration.Settings.Tools.OcrTrainFonts.Split(';');
            foreach (var x in FontFamily.Families)
            {
                if (x.IsStyleAvailable(FontStyle.Regular) && x.IsStyleAvailable(FontStyle.Bold))
                {
                    var chk = selectedFonts.Contains(x.Name);
                    ListViewItem item = new ListViewItem(x.Name) { Font = new Font(x.Name, 14), Checked = chk };
                    listViewFonts.Items.Add(item);
                }
            }
            comboBoxSubtitleFontSize.SelectedIndex = 5;
            comboBoxFontSizeEnd.SelectedIndex = 15;
            textBoxInputFile.Text = Configuration.Settings.Tools.OcrTrainSrtFile;
        }

        private void TrainLetter(ref int numberOfCharactersLearned, ref int numberOfCharactersSkipped, BinaryOcrDb db, string s, bool bold, bool italic)
        {
            var bmp = GenerateImageFromTextWithStyle("H  " + s, bold, italic);
            var nbmp = new NikseBitmap(bmp);
            nbmp.MakeTwoColor(280);
            var list = NikseBitmapImageSplitter.SplitBitmapToLettersNew(nbmp, 10, false, false, 25, false);
            if (list.Count == 3)
            {
                var item = list[2];
                var bob = new BinaryOcrBitmap(item.NikseBitmap, italic, 0, s, item.X, item.Y);
                var match = db.FindExactMatch(bob);
                if (match < 0)
                {
                    db.Add(bob);
                    numberOfCharactersLearned++;
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
            var bmp = new Bitmap(400, 300);
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
                TextDraw.DrawText(font, sf, path, sb, italic, subtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
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
            if (buttonTrain.Text == LanguageSettings.Current.SpellCheck.Abort)
            {
                _abort = true;
                return;
            }

            _abort = false;
            buttonTrain.Text = LanguageSettings.Current.SpellCheck.Abort;
            buttonOK.Enabled = false;

            saveFileDialog1.DefaultExt = ".db";
            saveFileDialog1.Filter = "*Binary Image Compare DB files|*.db";
            if (saveFileDialog1.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            buttonTrain.Text = LanguageSettings.Current.SpellCheck.Abort;
            var startFontSize = Convert.ToInt32(comboBoxSubtitleFontSize.Items[comboBoxSubtitleFontSize.SelectedIndex].ToString());
            var endFontSize = Convert.ToInt32(comboBoxFontSizeEnd.Items[comboBoxFontSizeEnd.SelectedIndex].ToString());

            if (!File.Exists(textBoxInputFile.Text))
            {
                MessageBox.Show($"Input file '{textBoxInputFile.Text}' does not exist!");
                return;
            }

            if (listViewFonts.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one font!");
                return;
            }

            var bicDb = new BinaryOcrDb(saveFileDialog1.FileName);
            int numberOfCharactersLearned = 0;
            int numberOfCharactersSkipped = 0;
            foreach (ListViewItem fontItem in listViewFonts.CheckedItems)
            {
                _subtitleFontName = fontItem.Text;
                for (_subtitleFontSize = startFontSize; _subtitleFontSize <= endFontSize; _subtitleFontSize++)
                {
                    var lines = File.ReadAllLines(textBoxInputFile.Text).ToList();
                    var format = new SubRip();
                    var sub = new Subtitle();
                    format.LoadSubtitle(sub, lines, textBoxInputFile.Text);
                    var charactersLearned = new List<string>();
                    foreach (var p in sub.Paragraphs)
                    {
                        labelInfo.Refresh();
                        Application.DoEvents();
                        foreach (char ch in p.Text)
                        {
                            if (!char.IsWhiteSpace(ch))
                            {
                                var s = ch.ToString();
                                if (!charactersLearned.Contains(s))
                                {
                                    charactersLearned.Add(s);
                                    TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, bicDb, s, false, false);
                                    if (checkBoxBold.Checked)
                                    {
                                        TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, bicDb, s, true, false);
                                    }
                                    if (checkBoxItalic.Checked)
                                    {
                                        TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, bicDb, s, false, true);
                                    }
                                }
                                labelInfo.Text = string.Format(LanguageSettings.Current.VobSubOcr.NowTraining, numberOfCharactersLearned, _subtitleFontName, numberOfCharactersSkipped);
                            }
                        }

                        foreach (var text in textBoxMerged.Text.Split(' '))
                        {
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                if (!charactersLearned.Contains(text) && text.Length > 1 && text.Length <= 3)
                                {
                                    charactersLearned.Add(text);
                                    TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, bicDb, text, false, false);
                                    if (checkBoxBold.Checked)
                                    {
                                        TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, bicDb, text, true, false);
                                    }
                                    if (checkBoxItalic.Checked)
                                    {
                                        TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, bicDb, text, false, true);
                                    }
                                }
                            }
                        }

                        if (_abort)
                        {
                            break;
                        }
                    }
                    if (_abort)
                    {
                        break;
                    }
                }

                if (_abort)
                {
                    break;
                }
            }
            bicDb.Save();
            if (_abort)
            {
                labelInfo.Text = "Partially (aborted) training completed and saved in " + saveFileDialog1.FileName;
            }
            else
            {
                labelInfo.Text = "Training completed and saved in " + saveFileDialog1.FileName;
            }
            buttonOK.Enabled = true;
            buttonTrain.Text = "Start training";
            _abort = false;
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

        private void SelectAll_Click(object sender, EventArgs e)
        {
            listViewFonts.BeginUpdate();
            foreach (ListViewItem fontItem in listViewFonts.Items)
            {
                fontItem.Checked = true;
            }
            listViewFonts.EndUpdate();
        }

        public void InitializeDetectFont(BinaryOcrBitmap bob, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            _autoDetectFontBob = bob;
            _autoDetectFontText = text;
        }

        private void BinaryOcrTrain_ResizeEnd(object sender, EventArgs e)
        {
            listViewFonts.AutoSizeLastColumn();
        }

        private void BinaryOcrTrain_Shown(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_autoDetectFontText))
            {
                return;
            }

            BinaryOcrTrain_ResizeEnd(sender, e);
            SelectAll_Click(this, EventArgs.Empty);
            int numberOfCharactersLearned = 0;
            int numberOfCharactersSkipped = 0;
            foreach (ListViewItem fontItem in listViewFonts.CheckedItems)
            {
                _subtitleFontName = fontItem.Text;
                labelInfo.Text = $"Checking font '{_subtitleFontName}'... {AutoDetectedFonts.Count} hits";
                labelInfo.Refresh();
                Application.DoEvents();

                for (_subtitleFontSize = 20; _subtitleFontSize <= 100; _subtitleFontSize++)
                {
                    if (!string.IsNullOrEmpty(_autoDetectFontText))
                    {
                        var s = _autoDetectFontText;
                        var bicDb = new BinaryOcrDb(null);
                        TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, bicDb, s, false, false);
                        if (bicDb.FindExactMatch(_autoDetectFontBob) >= 0)
                        {
                            AutoDetectedFonts.Add(_subtitleFontName + " " + _subtitleFontSize);
                        }
                        else
                        {
                            // allow for error %
                            var smallestDifference = int.MaxValue;
                            foreach (var compareItem in bicDb.CompareImages)
                            {
                                if (compareItem.Width == _autoDetectFontBob.Width && compareItem.Height == _autoDetectFontBob.Height) // precise math in size
                                {
                                    if (Math.Abs(compareItem.NumberOfColoredPixels - _autoDetectFontBob.NumberOfColoredPixels) < 3)
                                    {
                                        int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem, _autoDetectFontBob);
                                        if (dif < smallestDifference)
                                        {
                                            if (!BinaryOcrDb.AllowEqual(compareItem, _autoDetectFontBob))
                                            {
                                                continue;
                                            }

                                            smallestDifference = dif;
                                            if (dif < 3)
                                            {
                                                break; // foreach ending
                                            }
                                        }
                                    }
                                }
                            }

                            if (smallestDifference < 13)
                            {
                                AutoDetectedFonts.Add($"{_subtitleFontName} {_subtitleFontSize} - diff={smallestDifference}");
                            }
                        }

                        bicDb = new BinaryOcrDb(null);
                        TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, bicDb, s, false, true);
                        if (bicDb.FindExactMatch(_autoDetectFontBob) >= 0)
                        {
                            AutoDetectedFonts.Add(_subtitleFontName + " " + _subtitleFontSize + " italic");
                        }

                        if (checkBoxBold.Checked)
                        {
                            bicDb = new BinaryOcrDb(null);
                            TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, bicDb, s, true, false);
                            if (bicDb.FindExactMatch(_autoDetectFontBob) >= 0)
                            {
                                AutoDetectedFonts.Add(_subtitleFontName + " " + _subtitleFontSize + " bold");
                            }
                        }
                    }
                }
            }

            DialogResult = DialogResult.OK;
        }

        private void BinaryOcrTrain_FormClosing(object sender, FormClosingEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (ListViewItem item in listViewFonts.Items)
            {
                if (item.Checked)
                {
                    sb.Append(item.Text);
                    sb.Append(";");
                }
            }

            Configuration.Settings.Tools.OcrTrainFonts = sb.ToString().Trim(';');
            Configuration.Settings.Tools.OcrTrainSrtFile = textBoxInputFile.Text;
        }
    }
}

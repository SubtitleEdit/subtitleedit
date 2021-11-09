using Nikse.SubtitleEdit.Core.Common;
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

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class VobSubNOcrTrain : Form
    {
        private readonly Color _subtitleColor = Color.White;
        private string _subtitleFontName = "Verdana";
        private float _subtitleFontSize = 25.0f;
        private readonly Color _borderColor = Color.Black;
        private const float BorderWidth = 2.0f;
        private bool _abort;

        public VobSubNOcrTrain()
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
            labelLineSegments.Text = language.NumberOfSegments;
            comboBoxSubtitleFontSize.Left = labelSubtitleFontSize.Left + labelSubtitleFontSize.Width + 5;
            numericUpDownSegmentsPerCharacter.Left = labelLineSegments.Left + labelLineSegments.Width + 5;

            labelInfo.Text = string.Empty;
            textBoxMerged.Text = Configuration.Settings.Tools.OcrTrainMergedLetters;
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
            comboBoxSubtitleFontSize.SelectedIndex = 50;
            textBoxInputFile.Text = Configuration.Settings.Tools.OcrTrainSrtFile;
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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonTrain_Click(object sender, EventArgs e)
        {
            if (buttonTrain.Text == LanguageSettings.Current.SpellCheck.Abort)
            {
                _abort = true;
                return;
            }

            if (!File.Exists(textBoxInputFile.Text))
            {
                return;
            }

            _abort = false;
            buttonTrain.Text = LanguageSettings.Current.SpellCheck.Abort;
            buttonOK.Enabled = false;

            int numberOfCharactersLearned = 0;
            int numberOfCharactersSkipped = 0;
            var nOcrD = new NOcrDb(null);
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

                    foreach (var p in sub.Paragraphs)
                    {
                        foreach (char ch in p.Text)
                        {
                            if (!char.IsWhiteSpace(ch))
                            {
                                var s = ch.ToString();
                                if (!charactersLearned.Contains(s))
                                {
                                    charactersLearned.Add(s);
                                    TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, nOcrD, s, false, false, false);
                                    if (checkBoxBold.Checked)
                                    {
                                        TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, nOcrD, s, true, false, false);
                                    }
                                    if (checkBoxItalic.Checked)
                                    {
                                        TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, nOcrD, s, false, true, false);
                                    }
                                }
                            }
                        }

                        if (_abort)
                        {
                            break;
                        }
                    }

                    foreach (var text in textBoxMerged.Text.Split(' '))
                    {
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            if (!charactersLearned.Contains(text) && text.Length > 1 && text.Length <= 3)
                            {
                                charactersLearned.Add(text);
                                TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, nOcrD, text, false, false, true);
                                if (checkBoxBold.Checked)
                                {
                                    TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, nOcrD, text, true, false, true);
                                }
                                if (checkBoxItalic.Checked)
                                {
                                    TrainLetter(ref numberOfCharactersLearned, ref numberOfCharactersSkipped, nOcrD, text, false, true, true);
                                }
                            }
                        }
                    }

                    if (_abort)
                    {
                        break;
                    }
                }
            }

            labelInfo.Text = "Saving...";
            labelInfo.Refresh();
            saveFileDialog1.Filter = "nOCR DB|*.nocr";
            saveFileDialog1.InitialDirectory = Configuration.OcrDirectory;
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                labelInfo.Text = $"Saving to {saveFileDialog1.FileName}...";
                nOcrD.FileName = saveFileDialog1.FileName;
                nOcrD.Save();
                labelInfo.Text = "Training done";
            }
            else
            {
                labelInfo.Text = "Saving aborted";
            }

            buttonOK.Enabled = true;
            buttonTrain.Text = "Start training";
            _abort = false;
        }

        private void TrainLetter(ref int numberOfCharactersLearned, ref int numberOfCharactersSkipped, NOcrDb nOcrD, string s, bool bold, bool italic, bool doubleLetter)
        {
            var bmp = GenerateImageFromTextWithStyle("H   " + s, bold, italic);
            var nikseBitmap = new NikseBitmap(bmp);
            nikseBitmap.MakeTwoColor(280);
            nikseBitmap.CropTop(0, Color.FromArgb(0, 0, 0, 0));
            var list = NikseBitmapImageSplitter.SplitBitmapToLettersNew(nikseBitmap, 10, false, false, 25, false);
            if (list.Count == 3)
            {
                var item = list[2];
                var match = nOcrD.GetMatch(item.NikseBitmap, item.Top, false, 25);
                if (match == null || match.Text != s)
                {
                    labelInfo.Refresh();
                    Application.DoEvents();
                    var nOcrChar = new NOcrChar(s)
                    {
                        Width = item.NikseBitmap.Width,
                        Height = item.NikseBitmap.Height,
                        MarginTop = item.Top,
                        Italic = italic,
                    };
                    VobSubOcrNOcrCharacter.GenerateLineSegments((int)numericUpDownSegmentsPerCharacter.Value + (doubleLetter ? 20 : 0), false, nOcrChar, item.NikseBitmap);
                    nOcrD.Add(nOcrChar);

                    numberOfCharactersLearned++;
                    labelInfo.Text = string.Format(LanguageSettings.Current.VobSubOcr.NowTraining, numberOfCharactersLearned, _subtitleFontName, numberOfCharactersSkipped);
                    bmp.Dispose();
                }
                else
                {
                    numberOfCharactersSkipped++;
                }
            }
            else if (!doubleLetter)
            {

                if (list.Count == 4 && list[2].NikseBitmap != null && list[3].NikseBitmap != null)
                {
                    // e.g. quote (")
                    var expandItem = VobSubOcr.GetExpandedSelectionNew(nikseBitmap, new List<ImageSplitterItem> { list[2], list[3] });
                    var match = nOcrD.GetMatchExpanded(nikseBitmap, expandItem, 2, list);
                    if (match != null && match.Text == s)
                    {
                        numberOfCharactersSkipped++;
                        return;
                    }

                    var nOcrChar = new NOcrChar(s)
                    {
                        Width = expandItem.NikseBitmap.Width,
                        Height = expandItem.NikseBitmap.Height,
                        MarginTop = expandItem.Top,
                        Italic = italic,
                        ExpandCount = 2,
                    };
                    VobSubOcrNOcrCharacter.GenerateLineSegments((int)numericUpDownSegmentsPerCharacter.Value + 5, false, nOcrChar, expandItem.NikseBitmap);
                    nOcrD.Add(nOcrChar);
                    return;
                }

                if (list.Count == 5 && list[2].NikseBitmap != null && list[3].NikseBitmap != null && list[4].NikseBitmap != null)
                {
                    // e.g. "%"
                    var expandItem = VobSubOcr.GetExpandedSelectionNew(nikseBitmap, new List<ImageSplitterItem> { list[2], list[3], list[4] });
                    var match = nOcrD.GetMatchExpanded(nikseBitmap, expandItem, 2, list);
                    if (match != null && match.Text == s)
                    {
                        numberOfCharactersSkipped++;
                        return;
                    }

                    var nOcrChar = new NOcrChar(s)
                    {
                        Width = expandItem.NikseBitmap.Width,
                        Height = expandItem.NikseBitmap.Height,
                        MarginTop = expandItem.Top,
                        Italic = italic,
                        ExpandCount = 3,
                    };
                    nOcrD.Add(nOcrChar);
                    VobSubOcrNOcrCharacter.GenerateLineSegments((int)numericUpDownSegmentsPerCharacter.Value + 10, false, nOcrChar, expandItem.NikseBitmap);
                    return;
                }

                numberOfCharactersSkipped++;
            }
        }

        private Bitmap GenerateImageFromTextWithStyle(string text, bool bold, bool italic)
        {
            text = HtmlUtil.RemoveHtmlTags(text);
            Font font;
            try
            {
                var fontStyle = FontStyle.Regular;
                if (bold)
                {
                    fontStyle = FontStyle.Bold;
                }
                else if (italic)
                {
                    fontStyle = FontStyle.Italic;
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
            var lineHeight = textSize.Height * 0.64f;

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
                    TextDraw.DrawText(font, sf, path, sb, italic, bold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);

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
                TextDraw.DrawText(font, sf, path, sb, italic, bold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
            }

            sf.Dispose();

            g.DrawPath(new Pen(_borderColor, BorderWidth), path);
            g.FillPath(new SolidBrush(c), path);
            g.Dispose();
            var nbmp = new NikseBitmap(bmp);
            nbmp.CropTransparentSidesAndBottom(2, true);
            return nbmp.GetBitmap();
        }

        private void VobSubNOcrTrain_FormClosing(object sender, FormClosingEventArgs e)
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
            Configuration.Settings.Tools.OcrTrainMergedLetters = textBoxMerged.Text;
        }

        private void VobSubNOcrTrain_ResizeEnd(object sender, EventArgs e)
        {
            listViewFonts.AutoSizeLastColumn();
        }

        private void VobSubNOcrTrain_Shown(object sender, EventArgs e)
        {
            VobSubNOcrTrain_ResizeEnd(sender, e);
        }
    }
}

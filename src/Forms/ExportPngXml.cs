using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VobSub;

namespace Nikse.SubtitleEdit.Forms
{

    public sealed partial class ExportPngXml : Form
    {
        private class MakeBitmapParameter
        {
            public Bitmap Bitmap { get; set; }
            public Paragraph P { get; set; }
            public string Type { get; set; }
            public Color SubtitleColor { get; set; }
            public string SubtitleFontName { get; set; }
            public float SubtitleFontSize { get; set; }
            public bool SubtitleFontBold { get; set; }
            public Color BorderColor { get; set; }
            public float BorderWidth { get; set; }
            public bool AntiAlias { get; set; }
            public bool AlignLeft { get; set; }
            public bool AlignRight { get; set; }
            public byte[] Buffer { get; set; }
            public int ScreenWidth { get; set; }
            public int ScreenHeight { get; set; }
            public double FramesPerSeconds { get; set; }
            public int BottomMargin { get; set; }
            public bool Saved { get; set; }
        }

        private Subtitle _subtitle;
        private Color _subtitleColor = Color.White;
        private string _subtitleFontName = "Verdana";
        private float _subtitleFontSize = 75.0f;
        private bool _subtitleFontBold = false;
        private Color _borderColor = Color.Black;
        private float _borderWidth = 2.0f;
        private bool _isLoading = true;
        private string _exportType = "BDNXML";
        private string _fileName;

        public ExportPngXml()
        {
            InitializeComponent();
            comboBoxImageFormat.SelectedIndex = 4;
        }

        private double FrameRate 
        {
            get 
            {
                if (comboBoxFramerate.SelectedItem == null)
                    return 25;

                string s = comboBoxFramerate.SelectedItem.ToString();
                s = s.Replace(",", ".").Trim();
                double d;
                if (double.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                    return d;
                return 25;
            }
        }

        private string BdnXmlTimeCode(TimeCode timecode)
        {
            int frames = (int)Math.Round(timecode.Milliseconds / (1000.0 / FrameRate));
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", timecode.Hours, timecode.Minutes, timecode.Seconds, frames);
        }

        public static void DoWork(object data)
        {
            var paramter = (MakeBitmapParameter)data;
            paramter.Bitmap = GenerateImageFromTextWithStyle(paramter);
            if (paramter.Type == "BLURAYSUP")
            {
                var brSub = new Logic.BluRaySup.BluRaySupPicture
                                {
                                    StartTime = (long) paramter.P.StartTime.TotalMilliseconds,
                                    EndTime = (long) paramter.P.EndTime.TotalMilliseconds,
                                    Width = paramter.ScreenWidth,
                                    Height = paramter.ScreenHeight
                                };
                paramter.Buffer = Logic.BluRaySup.BluRaySupPicture.CreateSupFrame(brSub, paramter.Bitmap, paramter.FramesPerSeconds, paramter.BottomMargin);
            }
            else if (paramter.Type == "VOBSUB")
            {

            }
            else if (paramter.Type == "FAB")
            {

            }
        }

        private MakeBitmapParameter MakeMakeBitmapParameter(int index, int screenWidth,int screenHeight)
        {
            var parameter = new MakeBitmapParameter
                                {
                                    Type = _exportType,
                                    SubtitleColor = _subtitleColor,
                                    SubtitleFontName = _subtitleFontName,
                                    SubtitleFontSize = _subtitleFontSize,
                                    SubtitleFontBold = _subtitleFontBold,
                                    BorderColor = _borderColor,
                                    BorderWidth = _borderWidth,
                                    AntiAlias = checkBoxAntiAlias.Checked,
                                    AlignLeft = comboBoxHAlign.SelectedIndex == 0,
                                    AlignRight = comboBoxHAlign.SelectedIndex == 2,
                                    ScreenWidth = screenWidth,
                                    ScreenHeight = screenHeight,
                                    Bitmap = null,
                                    FramesPerSeconds = FrameRate,
                                    BottomMargin =  comboBoxBottomMargin.SelectedIndex,
                                    Saved = false,
                                };
            if (index < _subtitle.Paragraphs.Count)
            {
                parameter.P = _subtitle.Paragraphs[index];
            }
            else
            {
                parameter.P = null;
            }
            return parameter;
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            buttonExport.Enabled = false;
            SetupImageParameters();

            if (!string.IsNullOrEmpty(_fileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            if (_exportType == "BLURAYSUP")
            {
                saveFileDialog1.Title = "Choose Blu-ray sup file name...";
                saveFileDialog1.DefaultExt = "*.sup";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Blu-Ray sup|*.sup";
            }
            else if (_exportType == "VOBSUB")
            {
                saveFileDialog1.Title = "Choose Vobsub file name...";
                saveFileDialog1.DefaultExt = "*.sub";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "VobSub|*.sub";
            }
            else if (_exportType == "FAB")
            {
                saveFileDialog1.Title = "Choose FAB image script file name...";
                saveFileDialog1.DefaultExt = "*.txt";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "FAB image scripts|*.txt";
            }

            if (_exportType == "BLURAYSUP" &&  saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "VOBSUB" && saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "BDNXML" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "FAB" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                int width = 1920;
                int height = 1080;
                if (comboBoxResolution.SelectedIndex == 1)
                {
                    width = 1440;
                    height = 1080;
                }
                else if (comboBoxResolution.SelectedIndex == 2)
                {
                    width = 1280;
                    height = 720;
                }
                else if (comboBoxResolution.SelectedIndex == 3)
                {
                    width = 960;
                    height = 720;
                }
                else if (comboBoxResolution.SelectedIndex == 4)
                {
                    width = 848;
                    height = 480;
                }
                else if (comboBoxResolution.SelectedIndex == 5)
                {
                    width = 720;
                    height = 576;
                }
                else if (comboBoxResolution.SelectedIndex == 6)
                {
                    width = 720;
                    height = 480;
                }
                else if (comboBoxResolution.SelectedIndex == 7)
                {
                    width = 640;
                    height = 352;
                }
                else if (comboBoxResolution.SelectedIndex == 8)
                {
                    width = 640;
                    height = 272;
                }

                FileStream binarySubtitleFile = null;
                VobSubWriter vobSubWriter = null;
                if (_exportType == "BLURAYSUP")
                    binarySubtitleFile = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                else if (_exportType == "VOBSUB")
                    vobSubWriter = new VobSubWriter(saveFileDialog1.FileName, width, height, comboBoxBottomMargin.SelectedIndex, 32, _subtitleColor, _borderColor, GetOutlineColor(_borderColor), IfoParser.ArrayOfLanguage[comboBoxLanguage.SelectedIndex], IfoParser.ArrayOfLanguageCode[comboBoxLanguage.SelectedIndex]);

                progressBar1.Value = 0;
                progressBar1.Maximum = _subtitle.Paragraphs.Count-1;
                progressBar1.Visible = true;

                int border = comboBoxBottomMargin.SelectedIndex;
                int imagesSavedCount = 0;
                var sb = new StringBuilder();

                var threadEqual = new Thread(DoWork);
                var paramEqual = MakeMakeBitmapParameter(0, width, height);

                var threadUnEqual = new Thread(DoWork);
                var paramUnEqual = MakeMakeBitmapParameter(1, width, height);

                threadEqual.Start(paramEqual);
                int i = 1;
                for (; i < _subtitle.Paragraphs.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        paramEqual = MakeMakeBitmapParameter(i, width, height);
                        threadEqual = new Thread(DoWork);
                        threadEqual.Start(paramEqual);

                        if (threadUnEqual.ThreadState == ThreadState.Running)
                            threadUnEqual.Join(3000);
                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramUnEqual, i);
                    }
                    else
                    {
                        paramUnEqual = MakeMakeBitmapParameter(i, width, height);
                        threadUnEqual = new Thread(DoWork);
                        threadUnEqual.Start(paramUnEqual);

                        if (threadEqual.ThreadState == ThreadState.Running)
                            threadEqual.Join(3000);
                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramEqual, i);
                    }
                    progressBar1.Refresh();
                    Application.DoEvents();
                    progressBar1.Value = i;
                }

                if (i % 2 == 0)
                {
                    if (threadEqual.ThreadState == ThreadState.Running)
                        threadEqual.Join(3000);
                    imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramEqual, i);
                    if (threadUnEqual.ThreadState == ThreadState.Running)
                        threadUnEqual.Join(3000);
                    imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramUnEqual, i);
                }
                else
                {
                    if (threadUnEqual.ThreadState == ThreadState.Running)
                        threadUnEqual.Join(3000);
                    imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter,
                                                      binarySubtitleFile, paramUnEqual, i);
                        if (threadEqual.ThreadState == ThreadState.Running)
                        threadEqual.Join(3000);
                    imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter,
                                                      binarySubtitleFile, paramEqual, i);
                }

                progressBar1.Visible = false;
                if (_exportType == "BLURAYSUP")
                {
                    binarySubtitleFile.Close();
                    MessageBox.Show(string.Format(Configuration.Settings.Language.Main.SavedSubtitleX, saveFileDialog1.FileName));
                }
                else if (_exportType == "VOBSUB")
                {
                    vobSubWriter.CloseSubFile();
                    vobSubWriter.WriteIdxFile();
                    MessageBox.Show(string.Format(Configuration.Settings.Language.Main.SavedSubtitleX, saveFileDialog1.FileName));
                }
                else if (_exportType == "FAB")
                {
                    File.WriteAllText(Path.Combine(folderBrowserDialog1.SelectedPath, "Fab_Image_script.txt"), sb.ToString());
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath));
                }
                else
                {
                    string videoFormat = "1080p";
                    if (comboBoxResolution.SelectedIndex == 2)
                        videoFormat = "720p";
                    else if (comboBoxResolution.SelectedIndex == 3)
                        videoFormat = "960x720";
                    else if (comboBoxResolution.SelectedIndex == 4)
                        videoFormat = "480p";
                    else if (comboBoxResolution.SelectedIndex == 5)
                        videoFormat = "720x576";
                    else if (comboBoxResolution.SelectedIndex == 6)
                        videoFormat = "720x480";
                    else if (comboBoxResolution.SelectedIndex == 7)
                        videoFormat = "640x352";
                    else if (comboBoxResolution.SelectedIndex == 8)
                        videoFormat = "640x272";

                    var doc = new XmlDocument();
                    Paragraph first = _subtitle.Paragraphs[0];
                    Paragraph last = _subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1];
                    doc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                                "<BDN Version=\"0.93\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"BD-03-006-0093b BDN File Format.xsd\">" + Environment.NewLine +
                                "<Description>" + Environment.NewLine +
                                "<Name Title=\"subtitle_exp\" Content=\"\"/>" + Environment.NewLine +
                                "<Language Code=\"eng\"/>" + Environment.NewLine +
                                "<Format VideoFormat=\""+videoFormat + "\" FrameRate=\""+  FrameRate.ToString(CultureInfo.InvariantCulture) +  "\" DropFrame=\"False\"/>" + Environment.NewLine +
                                "<Events Type=\"Graphic\" FirstEventInTC=\"" + BdnXmlTimeCode(first.StartTime) + "\" LastEventOutTC=\"" + BdnXmlTimeCode(last.EndTime) + "\" NumberofEvents=\"" + imagesSavedCount.ToString() + "\"/>" + Environment.NewLine +
                                "</Description>" + Environment.NewLine +
                                "<Events>" + Environment.NewLine +
                                "</Events>" + Environment.NewLine +
                                "</BDN>");
                    XmlNode events = doc.DocumentElement.SelectSingleNode("Events");
                    events.InnerXml = sb.ToString();
                    File.WriteAllText(Path.Combine(folderBrowserDialog1.SelectedPath, "BDN_Index.xml"), doc.OuterXml);
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath));
                }
            }
            buttonExport.Enabled = true;
        }

        private int WriteParagraph(int width, StringBuilder sb, int border, int height, int imagesSavedCount,
                                   VobSubWriter vobSubWriter, FileStream binarySubtitleFile, MakeBitmapParameter param,
                                   int i)
        {
            if (param.Bitmap != null)
            {
                if (_exportType == "BLURAYSUP")
                {
                    if (!param.Saved)
                        binarySubtitleFile.Write(param.Buffer, 0, param.Buffer.Length);
                    param.Saved = true;
                }
                else if (_exportType == "VOBSUB")
                {
                    if (!param.Saved)
                        vobSubWriter.WriteParagraph(param.P, param.Bitmap);
                    param.Saved = true;
                }
                else if (_exportType == "FAB")
                {
                    if (!param.Saved)
                    {
                        string numberString = string.Format("IMAGE{0:000}", i);
                        string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLower());
                        var imageFormat = System.Drawing.Imaging.ImageFormat.Png; 
                        if (comboBoxImageFormat.SelectedIndex == 0)
                            imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                        else if (comboBoxImageFormat.SelectedIndex == 1)
                            imageFormat = System.Drawing.Imaging.ImageFormat.Exif;
                        else if (comboBoxImageFormat.SelectedIndex == 2)
                            imageFormat = System.Drawing.Imaging.ImageFormat.Gif;
                        else if (comboBoxImageFormat.SelectedIndex == 3)
                            imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                        else if (comboBoxImageFormat.SelectedIndex == 4)
                            imageFormat = System.Drawing.Imaging.ImageFormat.Png;
                        else if (comboBoxImageFormat.SelectedIndex == 5)
                            imageFormat = System.Drawing.Imaging.ImageFormat.Tiff;

                        param.Bitmap.Save(fileName, imageFormat);
                        imagesSavedCount++;

                        //RACE001.TIF 00;00;02;02 00;00;03;15 000 000 720 480
                        //RACE002.TIF 00;00;05;18 00;00;09;20 000 000 720 480
                        int top = param.ScreenHeight - (param.Bitmap.Height + 20); // bottom margin=20
                        sb.AppendLine(string.Format("{0} {1} {2} {3} {4} {5} {6}", Path.GetFileName(fileName), FormatFabTime(param.P.StartTime, param), FormatFabTime(param.P.EndTime, param), 0, top, param.ScreenWidth, param.ScreenHeight));
                        param.Saved = true;
                    }
                }
                else
                {
                    if (!param.Saved)
                    {
                        string numberString = string.Format("{0:0000}", i);
                        string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + ".png");
                        param.Bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                        imagesSavedCount++;

                        //<Event InTC="00:00:24:07" OutTC="00:00:31:13" Forced="False">
                        //  <Graphic Width="696" Height="111" X="612" Y="930">subtitle_exp_0001.png</Graphic>
                        //</Event>
                        sb.AppendLine("<Event InTC=\"" + BdnXmlTimeCode(param.P.StartTime) + "\" OutTC=\"" +
                                      BdnXmlTimeCode(param.P.EndTime) + "\" Forced=\"False\">");
                        int x = (width - param.Bitmap.Width) / 2;
                        int y = height - (param.Bitmap.Height + border);
                        sb.AppendLine("  <Graphic Width=\"" + param.Bitmap.Width.ToString() + "\" Height=\"" +
                                      param.Bitmap.Height.ToString() + "\" X=\"" + x.ToString() + "\" Y=\"" + y.ToString() +
                                      "\">" + numberString + ".png</Graphic>");
                        sb.AppendLine("</Event>");
                        param.Saved = true;
                    }
                }
            }
            return imagesSavedCount;
        }

        private string FormatFabTime(TimeCode time, MakeBitmapParameter param)
        {
            if (param.Bitmap.Width == 720 && param.Bitmap.Width == 480) // NTSC
                return string.Format("{0:00};{1:00};{2:00};{3:00}", time.Hours, time.Minutes, time.Seconds, Logic.SubtitleFormats.SubtitleFormat.MillisecondsToFrames(time.Milliseconds));
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, Logic.SubtitleFormats.SubtitleFormat.MillisecondsToFrames(time.Milliseconds));
        }

        private void SetupImageParameters()
        {
            if (_isLoading)
                return;

            _subtitleColor = panelColor.BackColor;
            _borderColor = panelBorderColor.BackColor;
            _subtitleFontName = comboBoxSubtitleFont.SelectedItem.ToString();
            _subtitleFontSize = float.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString());
            _subtitleFontBold = checkBoxBold.Checked;
            _borderWidth = float.Parse(comboBoxBorderWidth.SelectedItem.ToString());
        }

        private static Color GetOutlineColor(Color borderColor)
        {
            if (borderColor.R + borderColor.G + borderColor.B < 30)
                return Color.FromArgb(200, 75, 75, 75);
            return Color.FromArgb(150, borderColor.R, borderColor.G, borderColor.B);
        }

        private Bitmap GenerateImageFromTextWithStyle(string text)
        {
            var mbp = new MakeBitmapParameter();
            mbp.AlignLeft = comboBoxHAlign.SelectedIndex == 0;
            mbp.AlignRight = comboBoxHAlign.SelectedIndex == 2;
            mbp.AntiAlias = checkBoxAntiAlias.Checked;
            mbp.BorderWidth = _borderWidth;
            mbp.BorderColor = _borderColor;
            mbp.SubtitleFontName = _subtitleFontName;
            mbp.SubtitleColor = _subtitleColor;
            mbp.SubtitleFontSize = _subtitleFontSize;
            mbp.SubtitleFontBold = _subtitleFontBold;
            mbp.P = new Paragraph(text, 0, 0);

            var bmp = GenerateImageFromTextWithStyle(mbp);
            if (_exportType == "VOBSUB")
            {
                var nbmp = new NikseBitmap(bmp);
                nbmp.ConverToFourColors(Color.Transparent, _subtitleColor, _borderColor, GetOutlineColor(_borderColor));
                bmp = nbmp.GetBitmap();
            }
            return bmp;
        }

        private static float MeasureTextWidth(Font font, string text, bool bold)
        {
            var sf = new StringFormat {Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near};
            var path = new GraphicsPath();

            var sb = new StringBuilder(text);
            bool isItalic = false;
            bool newLine = false;
            int addX = 0;
            int leftMargin = 0;
            TextDraw.DrawText(font, sf, path, sb, isItalic, bold, 0, 0, ref newLine, addX, leftMargin);

            float width = 0;
            int index = path.PathPoints.Length - 30;
            if (index < 0)
                index = 0;
            for (int i = index; i < path.PathPoints.Length; i++)
            {
                if (path.PathPoints[i].X > width)
                    width = path.PathPoints[i].X;
            }
            int max = 30;
            if (30 >= path.PathPoints.Length)
                max = path.PathPoints.Length;
            for (int i = 0; i < max; i++)
            {
                if (path.PathPoints[i].X > width)
                    width = path.PathPoints[i].X;
            }

            return width;
        }

        private static Bitmap GenerateImageFromTextWithStyle(MakeBitmapParameter parameter)
        {
            string text = parameter.P.Text;

            // remove styles for display text (except italic)
            text = RemoveSubStationAlphaFormatting(text);
            text = text.Replace("<b>", string.Empty);
            text = text.Replace("</b>", string.Empty);
            text = text.Replace("<B>", string.Empty);
            text = text.Replace("</B>", string.Empty);
            text = text.Replace("<u>", string.Empty);
            text = text.Replace("</u>", string.Empty);
            text = text.Replace("<U>", string.Empty);
            text = text.Replace("</U>", string.Empty);
            text = Utilities.RemoveHtmlFontTag(text);

            Font font;
            try
            {
                var fontStyle = FontStyle.Regular;
                if (parameter.SubtitleFontBold)
                    fontStyle = FontStyle.Bold;
                font = new Font(parameter.SubtitleFontName, parameter.SubtitleFontSize, fontStyle);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                font = new Font(FontFamily.Families[0].Name, parameter.SubtitleFontSize);
            }
            var bmp = new Bitmap(400, 200);
            var g = Graphics.FromImage(bmp);

            SizeF textSize = g.MeasureString("Hj!", font);
            var lineHeight = (textSize.Height * 0.64f);
            float italicSpacing = textSize.Width / 8;

            textSize = g.MeasureString(text, font);
            g.Dispose();
            bmp.Dispose();
            bmp = new Bitmap((int)(textSize.Width * 0.8), (int)(textSize.Height * 0.7) + 10);
            g = Graphics.FromImage(bmp);

            var lefts = new List<float>();
            foreach (string line in text.Replace("<i>", string.Empty).Replace("</i>", string.Empty).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (parameter.AlignLeft)
                    lefts.Add(5);
                else if (parameter.AlignRight)
                    lefts.Add(bmp.Width - (MeasureTextWidth(font, line, parameter.SubtitleFontBold) + 15));
                else
                    lefts.Add((float)(bmp.Width - g.MeasureString(line, font).Width * 0.8+15) / 2);
            }

            if (parameter.AntiAlias)
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = SmoothingMode.AntiAlias;
            }
            var sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;// draw the text to a path
            var path = new GraphicsPath();

            // display italic
            var sb = new StringBuilder();
            int i = 0;
            bool isItalic = false;
            float left = 5;
            if (lefts.Count >= 0)
                left = lefts[0];
            float top = 5;
            bool newLine = false;
            float addX = 0;
            int lineNumber = 0;
            float leftMargin = left;
            bool italicFromStart = false;
            bool firstLinePart = true;
            while (i < text.Length)
            {
                if (text.Substring(i).ToLower().StartsWith("<i>"))
                {
                    italicFromStart = i == 0;
                    if (sb.Length > 0)
                    {
                        TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, left, top, ref newLine, addX, leftMargin);
                        addX = 0;
                        firstLinePart = false;
                    }
                    isItalic = true;
                    i += 2;
                }
                else if (text.Substring(i).ToLower().StartsWith("</i>") && isItalic)
                {
                    if (italicFromStart || firstLinePart || !isItalic)
                        addX = 0;
                    else
                        addX = italicSpacing;
                    TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, left, top, ref newLine, addX, leftMargin);
                    firstLinePart = false;
                    addX = 1;
                    if (parameter.SubtitleFontName.StartsWith("Arial"))
                        addX = 3;
                    isItalic = false;
                    i += 3;
                }
                else if (text.Substring(i).StartsWith(Environment.NewLine))
                {
                    if (italicFromStart || firstLinePart || !isItalic)
                        addX = 0;
                    else
                        addX = italicSpacing;

                    TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, left, top, ref newLine, addX, leftMargin);
                    firstLinePart = true;

                    addX = 0;
                    top += lineHeight;
                    newLine = true;
                    i += Environment.NewLine.Length - 1;
                    addX = 0;
                    lineNumber++;
                    if (lineNumber < lefts.Count)
                        leftMargin = lefts[lineNumber];
                    if (isItalic)
                        italicFromStart = true;
                }
                else
                {
                    sb.Append(text.Substring(i, 1));
                }
                i++;
            }
            if (sb.Length > 0)
            {
                if ((italicFromStart || firstLinePart) && !isItalic)
                    addX = 0;
                else
                    addX = italicSpacing;
                TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, left, top, ref newLine, addX, leftMargin);
            }

            if (parameter.BorderWidth > 0)
                g.DrawPath(new Pen(parameter.BorderColor, parameter.BorderWidth), path);
            g.FillPath(new SolidBrush(parameter.SubtitleColor), path);
            g.Dispose();
            NikseBitmap nbmp = new NikseBitmap(bmp);
            nbmp.CropTransparentSides(5);

            return nbmp.GetBitmap();
        }

        private static string RemoveSubStationAlphaFormatting(string s)
        {
            int indexOfBegin = s.IndexOf("{");
            while (indexOfBegin >= 0 && s.IndexOf("}") > indexOfBegin)
            {
                int indexOfEnd = s.IndexOf("}");
                s = s.Remove(indexOfBegin, (indexOfEnd - indexOfBegin) + 1);
                indexOfBegin = s.IndexOf("{");
            }
            return s;
        }

        internal void Initialize(Subtitle subtitle, string exportType, string fileName)
        {
            _exportType = exportType;
            _fileName = fileName;
            if (exportType == "BLURAYSUP")
                Text = "Blu-ray SUP";
            else if (exportType == "VOBSUB")
                Text = "VobSub (sub/idx)";
            else if (exportType == "FAB")
                Text = "FAB Image Script";
            else
                Text = Configuration.Settings.Language.ExportPngXml.Title;
            groupBoxImageSettings.Text = Configuration.Settings.Language.ExportPngXml.ImageSettings;
            labelSubtitleFont.Text = Configuration.Settings.Language.ExportPngXml.FontFamily;
            labelSubtitleFontSize.Text = Configuration.Settings.Language.ExportPngXml.FontSize;
            buttonColor.Text = Configuration.Settings.Language.ExportPngXml.FontColor;
            checkBoxAntiAlias.Text = Configuration.Settings.Language.ExportPngXml.AntiAlias;
            checkBoxBold.Text = Configuration.Settings.Language.General.Bold;
            buttonBorderColor.Text = Configuration.Settings.Language.ExportPngXml.BorderColor;
            labelBorderWidth.Text = Configuration.Settings.Language.ExportPngXml.BorderWidth;
            buttonExport.Text = Configuration.Settings.Language.ExportPngXml.ExportAllLines;
            buttonCancel.Text = Configuration.Settings.Language.General.OK;
            labelImageResolution.Text = string.Empty;
            labelLanguage.Text = Configuration.Settings.Language.ChooseLanguage.Language;
            labelFrameRate.Text = Configuration.Settings.Language.General.FrameRate;
            labelHorizontalAlign.Text = Configuration.Settings.Language.ExportPngXml.Align;
            if (Configuration.Settings.Language.ExportPngXml.Left != null &&
                Configuration.Settings.Language.ExportPngXml.Center != null &&
                Configuration.Settings.Language.ExportPngXml.Right != null)
            {
                comboBoxHAlign.Items.Clear();
                comboBoxHAlign.Items.Add(Configuration.Settings.Language.ExportPngXml.Left);
                comboBoxHAlign.Items.Add(Configuration.Settings.Language.ExportPngXml.Center);
                comboBoxHAlign.Items.Add(Configuration.Settings.Language.ExportPngXml.Right);
            }

            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            Utilities.InitializeSubtitleFont(subtitleListView1);
            subtitleListView1.AutoSizeAllColumns(this);

            _subtitle = subtitle;
            panelColor.BackColor = _subtitleColor;
            panelBorderColor.BackColor = _borderColor;
            comboBoxBorderWidth.SelectedIndex = 2;
            comboBoxHAlign.SelectedIndex = 1;
            comboBoxResolution.SelectedIndex = 0;

            if (exportType == "VOBSUB")
            {
                comboBoxBorderWidth.SelectedIndex = 3;
                comboBoxResolution.SelectedIndex = 5;
                labelLanguage.Visible = true;
                comboBoxLanguage.Visible = true;
                comboBoxLanguage.Items.Clear();
                string languageCode = Utilities.AutoDetectGoogleLanguage(subtitle);
                for (int i = 0; i < IfoParser.ArrayOfLanguage.Count; i++)
                {
                    comboBoxLanguage.Items.Add(IfoParser.ArrayOfLanguage[i]);
                    if (IfoParser.ArrayOfLanguageCode[i] == languageCode)
                        comboBoxLanguage.SelectedIndex = i;
                }
                comboBoxLanguage.SelectedIndex = 25;
            }
            comboBoxImageFormat.Visible = exportType == "FAB";
            labelImageFormat.Visible = exportType == "FAB";
            labelFrameRate.Visible = exportType == "BDNXML" || exportType == "BLURAYSUP";
            comboBoxFramerate.Visible = exportType == "BDNXML" || exportType == "BLURAYSUP";
            if (exportType == "BDNXML")
            {
                labelFrameRate.Top = labelLanguage.Top;
                comboBoxFramerate.Top = comboBoxLanguage.Top;
                comboBoxFramerate.Items.Add("23.976");
                comboBoxFramerate.Items.Add("24");
                comboBoxFramerate.Items.Add("25");
                comboBoxFramerate.Items.Add("29.97");
                comboBoxFramerate.Items.Add("50");
                comboBoxFramerate.Items.Add("59.94");
                comboBoxFramerate.SelectedIndex = 2;
            }
            else if (exportType == "BLURAYSUP")
            {
                labelFrameRate.Top = labelLanguage.Top;
                comboBoxFramerate.Top = comboBoxLanguage.Top;
                comboBoxFramerate.Items.Add("23.976");
                comboBoxFramerate.Items.Add("24");
                comboBoxFramerate.Items.Add("25");
                comboBoxFramerate.Items.Add("29.97");
                comboBoxFramerate.Items.Add("50");
                comboBoxFramerate.Items.Add("59.94");
                comboBoxFramerate.SelectedIndex = 1;
                comboBoxFramerate.DropDownStyle = ComboBoxStyle.DropDownList;
            }

            for (int i=0; i<1000; i++)
                comboBoxBottomMargin.Items.Add(i);
            comboBoxBottomMargin.SelectedIndex = 15;
            if (exportType == "BLURAYSUP")
                comboBoxBottomMargin.SelectedIndex = 20;
            if (_exportType == "BLURAYSUP" || _exportType == "VOBSUB")
            {
                comboBoxBottomMargin.Visible = true;
                labelBottomMargin.Visible = true;
            }
            else
            {
                comboBoxBottomMargin.Visible = false;
                labelBottomMargin.Visible = false;
            }

            foreach (var x in FontFamily.Families)
            {
                comboBoxSubtitleFont.Items.Add(x.Name);
                if (string.Compare(x.Name, _subtitleFontName, true) == 0)
                    comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
            }
            if (comboBoxSubtitleFont.SelectedIndex == -1)
                comboBoxSubtitleFont.SelectedIndex = 0; // take first font if default font not found (e.g. linux)

            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        private void subtitleListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupImageParameters();
            if (subtitleListView1.SelectedItems.Count > 0)
            {
                var bmp = GenerateImageFromTextWithStyle(_subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index].Text);
                pictureBox1.Image = bmp;
                labelImageResolution.Text = string.Format("{0}x{1}", bmp.Width, bmp.Height);
            }
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelColor.BackColor = colorDialog1.Color;
                subtitleListView1_SelectedIndexChanged(null, null);
            }
        }

        private void panelColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonColor_Click(null, null);
        }


        private void buttonBorderColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelBorderColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelBorderColor.BackColor = colorDialog1.Color;
                subtitleListView1_SelectedIndexChanged(null, null);
            }
        }

        private void panelBorderColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonBorderColor_Click(null, null);
        }

        private void comboBoxSubtitleFont_SelectedValueChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void comboBoxSubtitleFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void comboBoxBorderWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void checkBoxAntiAlias_CheckedChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void ExportPngXml_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.F1)
            {
                Utilities.ShowHelp(string.Empty);
                e.SuppressKeyPress = true;
            }
        }

        private void ExportPngXml_Shown(object sender, EventArgs e)
        {
            _isLoading = false;
            if (_exportType == "VOBSUB")
                comboBoxSubtitleFontSize.SelectedIndex = 7;
            else
                comboBoxSubtitleFontSize.SelectedIndex = 16;
        }

        private void comboBoxHAlign_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void checkBoxBold_CheckedChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

    }
}

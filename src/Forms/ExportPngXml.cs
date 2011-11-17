using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
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
            public Paragraph P  { get; set; }
            public string Type { get; set; }
            public Color SubtitleColor { get; set; }
            public string SubtitleFontName { get; set; }
            public float SubtitleFontSize { get; set; }
            public Color BorderColor { get; set; }
            public float BorderWidth { get; set; }
            public bool AntiAlias { get; set; }
            public bool AlignLeft { get; set; }
            public byte[] Buffer { get; set; }
            public int ScreenWidth { get; set; }
            public int ScreenHeight { get; set; }
        }

        Subtitle _subtitle;
        Color _subtitleColor = Color.White;
        string _subtitleFontName = "Verdana";
        float _subtitleFontSize = 75.0f;
        Color _borderColor = Color.Black;
        float _borderWidth = 2.0f;
        bool _isLoading = true;
        string _exportType = "BDNXML";
        string _fileName;

        public ExportPngXml()
        {
            InitializeComponent();
        }

        private static string BdnXmlTimeCode(TimeCode timecode)
        {
            int frames = timecode.Milliseconds / 40; // 40==25fps (1000/25)
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
                paramter.Buffer = Logic.BluRaySup.BluRaySupPicture.CreateSupFrame(brSub, paramter.Bitmap);
            }
            else if (paramter.Type == "VOBSUB")
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
                                    BorderColor = _borderColor,
                                    BorderWidth = _borderWidth,
                                    AntiAlias = checkBoxAntiAlias.Checked,
                                    AlignLeft = comboBoxHAlign.SelectedIndex == 0,
                                    ScreenWidth = screenWidth,
                                    ScreenHeight = screenHeight,
                                    Bitmap = null,
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

            if (_exportType == "BLURAYSUP" &&  saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "VOBSUB" && saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "BDNXML" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                FileStream binarySubtitleFile = null;
                VobSubWriter vobSubWriter = null;
                if (_exportType == "BLURAYSUP")
                    binarySubtitleFile = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                else if (_exportType == "VOBSUB")
                    vobSubWriter = new VobSubWriter(saveFileDialog1.FileName);

                progressBar1.Value = 0;
                progressBar1.Maximum = _subtitle.Paragraphs.Count-1;
                progressBar1.Visible = true;

                int width = 1920;
                int height = 1080;
                if (comboBoxResolution.SelectedIndex == 1)
                {
                    width = 1280;
                    height = 720;
                }
                else if (comboBoxResolution.SelectedIndex == 2)
                {
                    width = 848;
                    height = 480;
                }
                else if (comboBoxResolution.SelectedIndex == 3)
                {
                    width = 720;
                    height = 576;
                }
                else if (comboBoxResolution.SelectedIndex == 4)
                {
                    width = 720;
                    height = 480;
                }

                const int border = 25;
                int imagesSavedCount = 0;
                var sb = new StringBuilder();

                // We call in a seperate thread... or app will crash sometimes :(
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
                        if (threadEqual.ThreadState == ThreadState.Running)
                          threadEqual.Join(3000);
                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramEqual, i);

                        paramEqual = MakeMakeBitmapParameter(i, width, height);
                        threadEqual = new Thread(DoWork);
                        threadEqual.Start(paramEqual);
                    }
                    else
                    {
                        if (threadUnEqual.ThreadState == ThreadState.Running)
                            threadUnEqual.Join(3000);

                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramUnEqual, i);

                        paramUnEqual = MakeMakeBitmapParameter(i, width, height);
                        threadUnEqual = new Thread(DoWork);
                        threadUnEqual.Start(paramUnEqual);
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
                else
                {
                    var doc = new XmlDocument();
                    Paragraph first = _subtitle.Paragraphs[0];
                    Paragraph last = _subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1];
                    doc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                                "<BDN Version=\"0.93\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"BD-03-006-0093b BDN File Format.xsd\">" + Environment.NewLine +
                                "<Description>" + Environment.NewLine +
                                "<Name Title=\"subtitle_exp\" Content=\"\"/>" + Environment.NewLine +
                                "<Language Code=\"eng\"/>" + Environment.NewLine +
                                "<Format VideoFormat=\"1080p\" FrameRate=\"25\" DropFrame=\"False\"/>" + Environment.NewLine +
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
        }

        private int WriteParagraph(int width, StringBuilder sb, int border, int height, int imagesSavedCount,
                                   VobSubWriter vobSubWriter, FileStream binarySubtitleFile, MakeBitmapParameter paramEqual,
                                   int i)
        {
            if (paramEqual.Bitmap != null)
            {
                if (_exportType == "BLURAYSUP")
                {
                    binarySubtitleFile.Write(paramEqual.Buffer, 0, paramEqual.Buffer.Length);
                }
                else if (_exportType == "VOBSUB")
                {
                    vobSubWriter.WriteParagraph(paramEqual.P, paramEqual.Bitmap);
                }
                else
                {
                    string numberString = string.Format("{0:0000}", i + 1);
                    string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + ".png");
                    paramEqual.Bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                    imagesSavedCount++;

                    //<Event InTC="00:00:24:07" OutTC="00:00:31:13" Forced="False">
                    //  <Graphic Width="696" Height="111" X="612" Y="930">subtitle_exp_0001.png</Graphic>
                    //</Event>
                    sb.AppendLine("<Event InTC=\"" + BdnXmlTimeCode(paramEqual.P.StartTime) + "\" OutTC=\"" +
                                  BdnXmlTimeCode(paramEqual.P.EndTime) + "\" Forced=\"False\">");
                    int x = (width - paramEqual.Bitmap.Width)/2;
                    int y = height - (paramEqual.Bitmap.Height + border);
                    sb.AppendLine("  <Graphic Width=\"" + paramEqual.Bitmap.Width.ToString() + "\" Height=\"" +
                                  paramEqual.Bitmap.Height.ToString() + "\" X=\"" + x.ToString() + "\" Y=\"" + y.ToString() +
                                  "\">" + numberString + ".png</Graphic>");
                    sb.AppendLine("</Event>");
                }
            }
            return imagesSavedCount;
        }

        private void SetupImageParameters()
        {
            if (_isLoading)
                return;

            _subtitleColor = panelColor.BackColor;
            _borderColor = panelBorderColor.BackColor;
            _subtitleFontName = comboBoxSubtitleFont.SelectedItem.ToString();
            _subtitleFontSize = float.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString());
            _borderWidth = float.Parse(comboBoxBorderWidth.SelectedItem.ToString());
        }

        private Bitmap GenerateImageFromTextWithStyle(string text)
        {
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
                font = new Font(_subtitleFontName, _subtitleFontSize);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                font = new Font("Verdana", _subtitleFontSize);
            }
            var bmp = new Bitmap(400, 200);
            var g = Graphics.FromImage(bmp);

            SizeF textSize = g.MeasureString("Hj!", font);
            var lineHeight = (textSize.Height * 0.64f);
            float italicSpacing = textSize.Width / 8;

            textSize = g.MeasureString(text, font);
            g.Dispose();
            bmp.Dispose();
            bmp = new Bitmap((int)(textSize.Width * 0.8), (int)(textSize.Height * 0.7)+10);
            g = Graphics.FromImage(bmp);

            var lefts = new List<float>();
            foreach (string line in text.Replace("<i>", "i").Replace("</i>", "i").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (comboBoxHAlign.SelectedIndex == 0) // left
                    lefts.Add(5);
                else
                    lefts.Add((float)(bmp.Width - g.MeasureString(line, font).Width * 0.8) / 2);
            }


            if (checkBoxAntiAlias.Checked)
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
            while (i < text.Length)
            {
                if (text.Substring(i).ToLower().StartsWith("<i>"))
                {
                    italicFromStart = i == 0;
                    if (sb.Length > 0)
                    {
                        TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);
                        addX = 0;
                    }
                    isItalic = true;
                    i += 2;
                }
                else if (text.Substring(i).ToLower().StartsWith("</i>") && isItalic)
                {
                    if (italicFromStart)
                        addX = 0;
                    else
                        addX = italicSpacing;
                    TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);
                    addX = 1;
                    if (_subtitleFontName.StartsWith("Arial"))
                        addX = 3;
                    isItalic = false;
                    i += 3;
                }
                else if (text.Substring(i).StartsWith(Environment.NewLine))
                {
                    if (italicFromStart)
                        addX = 0;
                    else
                        addX = italicSpacing;

                    TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);

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
                if (italicFromStart)
                    addX = 0;
                else
                    addX = italicSpacing;
                TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);
            }

            if (_borderWidth > 0)
                g.DrawPath(new Pen(_borderColor, _borderWidth), path);
            g.FillPath(new SolidBrush(_subtitleColor), path);
            g.Dispose();
            return bmp;
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
                font = new Font(parameter.SubtitleFontName, parameter.SubtitleFontSize);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                font = new Font("Verdana", parameter.SubtitleFontSize);
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
            foreach (string line in text.Replace("<i>", "i").Replace("</i>", "i").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (parameter.AlignLeft) //comboBoxHAlign.SelectedIndex == 0) // left
                    lefts.Add(5);
                else
                    lefts.Add((float)(bmp.Width - g.MeasureString(line, font).Width * 0.8) / 2);
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
            while (i < text.Length)
            {
                if (text.Substring(i).ToLower().StartsWith("<i>"))
                {
                    italicFromStart = i == 0;
                    if (sb.Length > 0)
                    {
                        TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);
                        addX = 0;
                    }
                    isItalic = true;
                    i += 2;
                }
                else if (text.Substring(i).ToLower().StartsWith("</i>") && isItalic)
                {
                    if (italicFromStart)
                        addX = 0;
                    else
                        addX = italicSpacing;
                    TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);
                    addX = 1;
                    if (parameter.SubtitleFontName.StartsWith("Arial"))
                        addX = 3;
                    isItalic = false;
                    i += 3;
                }
                else if (text.Substring(i).StartsWith(Environment.NewLine))
                {
                    if (italicFromStart)
                        addX = 0;
                    else
                        addX = italicSpacing;

                    TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);

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
                if (italicFromStart)
                    addX = 0;
                else
                    addX = italicSpacing;
                TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);
            }

            if (parameter.BorderWidth > 0)
                g.DrawPath(new Pen(parameter.BorderColor, parameter.BorderWidth), path);
            g.FillPath(new SolidBrush(parameter.SubtitleColor), path);
            g.Dispose();
            return bmp;
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
            else
                Text = Configuration.Settings.Language.ExportPngXml.Title;
            groupBoxImageSettings.Text = Configuration.Settings.Language.ExportPngXml.ImageSettings;
            labelSubtitleFont.Text = Configuration.Settings.Language.ExportPngXml.FontFamily;
            labelSubtitleFontSize.Text = Configuration.Settings.Language.ExportPngXml.FontSize;
            buttonColor.Text = Configuration.Settings.Language.ExportPngXml.FontColor;
            checkBoxAntiAlias.Text = Configuration.Settings.Language.ExportPngXml.AntiAlias;
            buttonBorderColor.Text = Configuration.Settings.Language.ExportPngXml.BorderColor;
            labelBorderWidth.Text = Configuration.Settings.Language.ExportPngXml.BorderWidth;
            buttonExport.Text = Configuration.Settings.Language.ExportPngXml.ExportAllLines;
            buttonCancel.Text = Configuration.Settings.Language.General.OK;
            labelImageResolution.Text = string.Empty;
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
                comboBoxBorderWidth.SelectedIndex = 3;

            foreach (var x in FontFamily.Families)
            {
                comboBoxSubtitleFont.Items.Add(x.Name);
                if (string.Compare(x.Name, _subtitleFontName, true) == 0)
                    comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
            }

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

    }
}

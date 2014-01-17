using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.VobSub;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

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
            public bool SimpleRendering { get; set; }
            public bool AlignLeft { get; set; }
            public bool AlignRight { get; set; }
            public byte[] Buffer { get; set; }
            public int ScreenWidth { get; set; }
            public int ScreenHeight { get; set; }
            public int Type3D { get; set; }
            public int Depth3D { get; set; }
            public double FramesPerSeconds { get; set; }
            public int BottomMargin { get; set; }
            public bool Saved { get; set; }
            public ContentAlignment Alignment { get; set; }
            public Color BackgroundColor { get; set; }
            public string SavDialogFileName { get; set; }
            public string Error { get; set; }
            public bool LineJoinRound { get; set; }
            public Color ShadowColor { get; set; }
            public int ShadowWidth { get; set; }
            public int ShadowAlpha { get; set; }
            public int LineHeight { get; set; }


            public MakeBitmapParameter()
            {
                BackgroundColor = Color.Transparent;
            }
        }

        private Subtitle _subtitle;
        private SubtitleFormat _format;
        private Color _subtitleColor = Color.White;
        private string _subtitleFontName = "Verdana";
        private float _subtitleFontSize = 75.0f;
        private bool _subtitleFontBold;
        private Color _borderColor = Color.Black;
        private float _borderWidth = 2.0f;
        private bool _isLoading = true;
        private string _exportType = "BDNXML";
        private string _fileName;
        private VobSubOcr _vobSubOcr;

        public ExportPngXml()
        {
            InitializeComponent();
            comboBoxImageFormat.SelectedIndex = 4;
            _subtitleColor = Configuration.Settings.Tools.ExportFontColor;
            _borderColor = Configuration.Settings.Tools.ExportBorderColor;
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

        private static ContentAlignment GetAlignmentFromParagraph(Paragraph p, SubtitleFormat format, Subtitle subtitle)
        {
            ContentAlignment alignment = ContentAlignment.BottomCenter;
            if (format.HasStyleSupport && !string.IsNullOrEmpty(p.Extra))
            {
                if (format.GetType() == typeof(SubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, subtitle.Header);
                    alignment = GetSsaAlignment("{\\a" + style.Alignment + "}", alignment);
                }
                else if (format.GetType() == typeof(AdvancedSubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, subtitle.Header);
                    alignment = GetAssAlignment("{\\an" + style.Alignment + "}", alignment);
                }
            }

            string text = p.Text;
            if (format.GetType() == typeof(SubStationAlpha) && text.Length > 5)
            {
                text = p.Text.Substring(0, 6);
                alignment = GetSsaAlignment(text, alignment);

            }
            else if (text.Length > 6)
            {
                text = p.Text.Substring(0, 6);
                alignment = GetAssAlignment(text, alignment);
            }
            return alignment;
        }

        private static ContentAlignment GetSsaAlignment(string text, ContentAlignment defaultAlignment)
        {
            //1: Bottom left
            //2: Bottom center
            //3: Bottom right
            //9: Middle left
            //10: Middle center
            //11: Middle right
            //5: Top left
            //6: Top center
            //7: Top right
            switch (text)
            {
                case "{\\a1}":
                    return ContentAlignment.BottomLeft;
                case "{\\a2}":
                    return ContentAlignment.BottomCenter;
                case "{\\a3}":
                    return ContentAlignment.BottomRight;
                case "{\\a9}":
                    return ContentAlignment.MiddleLeft;
                case "{\\a10}":
                    return ContentAlignment.MiddleCenter;
                case "{\\a11}":
                    return ContentAlignment.MiddleRight;
                case "{\\a5}":
                    return ContentAlignment.TopLeft;
                case "{\\a6}":
                    return ContentAlignment.TopCenter;
                case "{\\a7}":
                    return ContentAlignment.TopRight;
            }
            return defaultAlignment;
        }

        private static ContentAlignment GetAssAlignment(string text, ContentAlignment defaultAlignment)
        {
            //1: Bottom left
            //2: Bottom center
            //3: Bottom right
            //4: Middle left
            //5: Middle center
            //6: Middle right
            //7: Top left
            //8: Top center
            //9: Top right
            switch (text)
            {
                case "{\\an1}":
                    return ContentAlignment.BottomLeft;
                case "{\\an2}":
                    return ContentAlignment.BottomCenter;
                case "{\\an3}":
                    return ContentAlignment.BottomRight;
                case "{\\an4}":
                    return ContentAlignment.MiddleLeft;
                case "{\\an5}":
                    return ContentAlignment.MiddleCenter;
                case "{\\an6}":
                    return ContentAlignment.MiddleRight;
                case "{\\an7}":
                    return ContentAlignment.TopLeft;
                case "{\\an8}":
                    return ContentAlignment.TopCenter;
                case "{\\an9}":
                    return ContentAlignment.TopRight;
            }
            return defaultAlignment;
        }

        public static void DoWork(object data)
        {
            var parameter = (MakeBitmapParameter)data;

            if (parameter.Type == "VOBSUB" || parameter.Type == "STL" || parameter.Type == "SPUMUX")
            {
                parameter.LineJoinRound = true;
            }
            parameter.Bitmap = GenerateImageFromTextWithStyle(parameter);
            if (parameter.Type == "BLURAYSUP")
            {
                var brSub = new Logic.BluRaySup.BluRaySupPicture
                                {
                                    StartTime = (long) parameter.P.StartTime.TotalMilliseconds,
                                    EndTime = (long) parameter.P.EndTime.TotalMilliseconds,
                                    Width = parameter.ScreenWidth,
                                    Height = parameter.ScreenHeight
                                };
                parameter.Buffer = Logic.BluRaySup.BluRaySupPicture.CreateSupFrame(brSub, parameter.Bitmap, parameter.FramesPerSeconds, parameter.BottomMargin, parameter.Alignment);
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
                                    SimpleRendering = checkBoxSimpleRender.Checked,
                                    AlignLeft = comboBoxHAlign.SelectedIndex == 0,
                                    AlignRight = comboBoxHAlign.SelectedIndex == 2,
                                    ScreenWidth = screenWidth,
                                    ScreenHeight = screenHeight,
                                    Bitmap = null,
                                    FramesPerSeconds = FrameRate,
                                    BottomMargin =  comboBoxBottomMargin.SelectedIndex,
                                    Saved = false,
                                    Alignment = ContentAlignment.BottomCenter,
                                    Type3D = comboBox3D.SelectedIndex,
                                    Depth3D = (int)numericUpDownDepth3D.Value,
                                    BackgroundColor = Color.Transparent,
                                    SavDialogFileName = saveFileDialog1.FileName,
                                    ShadowColor = panelShadowColor.BackColor,
                                    ShadowWidth = (int)comboBoxShadowWidth.SelectedIndex,
                                    ShadowAlpha = (int)numericUpDownShadowTransparency.Value,
                                    LineHeight = (int)numericUpDownLineSpacing.Value,
                                };
            if (index < _subtitle.Paragraphs.Count)
            {
                parameter.P = _subtitle.Paragraphs[index];
                parameter.Alignment = GetAlignmentFromParagraph(parameter.P,_format, _subtitle);

                if (_format.HasStyleSupport && !string.IsNullOrEmpty(parameter.P.Extra))
                {
                    if (_format.GetType() == typeof(SubStationAlpha))
                    {
                        var style = AdvancedSubStationAlpha.GetSsaStyle(parameter.P.Extra, _subtitle.Header);
                        parameter.SubtitleColor = style.Primary;
                        parameter.SubtitleFontBold = style.Bold;
                        parameter.SubtitleFontSize = style.FontSize;
                        if (style.BorderStyle == "3")
                        {
                            parameter.BackgroundColor = style.Background;
                        }
                    }
                    else if (_format.GetType() == typeof(AdvancedSubStationAlpha))
                    {
                        var style = AdvancedSubStationAlpha.GetSsaStyle(parameter.P.Extra, _subtitle.Header);
                        parameter.SubtitleColor = style.Primary;
                        parameter.SubtitleFontBold = style.Bold;
                        parameter.SubtitleFontSize = style.FontSize;
                        if (style.BorderStyle == "3")
                        {
                            parameter.BackgroundColor = style.Outline;
                        }
                    }
                }
            }
            else
            {
                parameter.P = null;
            }
            return parameter;
        }

        private void ButtonExportClick(object sender, EventArgs e)
        {
            List<string> errors = new List<string>();
            buttonExport.Enabled = false;
            SetupImageParameters();

            if (!string.IsNullOrEmpty(_fileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            if (_exportType == "BLURAYSUP")
            {
                saveFileDialog1.Title = Configuration.Settings.Language.ExportPngXml.SaveBluRraySupAs;
                saveFileDialog1.DefaultExt = "*.sup";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Blu-Ray sup|*.sup";
            }
            else if (_exportType == "VOBSUB")
            {
                saveFileDialog1.Title = Configuration.Settings.Language.ExportPngXml.SaveVobSubAs;
                saveFileDialog1.DefaultExt = "*.sub";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "VobSub|*.sub";
            }
            else if (_exportType == "FAB")
            {
                saveFileDialog1.Title = Configuration.Settings.Language.ExportPngXml.SaveFabImageScriptAs;
                saveFileDialog1.DefaultExt = "*.txt";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "FAB image scripts|*.txt";
            }
            else if (_exportType == "STL")
            {
                saveFileDialog1.Title = Configuration.Settings.Language.ExportPngXml.SaveDvdStudioProStlAs;
                saveFileDialog1.DefaultExt = "*.txt";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "DVD Studio Pro STL|*.stl";
            }
            else if (_exportType == "FCP")
            {
                saveFileDialog1.Title = "Save FCP XML as..."; //TODO: Configuration.Settings.Language.ExportPngXml.SaveFcpAs;
                saveFileDialog1.DefaultExt = "*.xml";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Xml files|*.xml";
            }
            else if (_exportType == "DOST")
            {
                saveFileDialog1.Title = "Save DOST XML as..."; //TODO: Configuration.Settings.Language.ExportPngXml.SaveDostAs;
                saveFileDialog1.DefaultExt = "*.dost";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Dost files|*.dost";
            }

            if (_exportType == "BLURAYSUP" && saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "VOBSUB" && saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "BDNXML" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "FAB" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "IMAGE/FRAME" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "STL" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "SPUMUX" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "FCP" && saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "DOST" && saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                int width = 1920;
                int height = 1080;
                GetResolution(ref width, ref height);

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
                if (_exportType == "STL")
                {
                    sb.AppendLine("$SetFilePathToken =" + folderBrowserDialog1.SelectedPath);
                    sb.AppendLine();
                }

                if (_vobSubOcr != null)
                {
                    int i = 0;
                    foreach (Paragraph p in _subtitle.Paragraphs)
                    {
                        var mp = MakeMakeBitmapParameter(i, width, height);
                        mp.Bitmap = _vobSubOcr.GetSubtitleBitmap(i);

                        if (_exportType == "BLURAYSUP")
                        {
                            var brSub = new Logic.BluRaySup.BluRaySupPicture
                            {
                                StartTime = (long)mp.P.StartTime.TotalMilliseconds,
                                EndTime = (long)mp.P.EndTime.TotalMilliseconds,
                                Width = mp.ScreenWidth,
                                Height = mp.ScreenHeight
                            };
                            mp.Buffer = Logic.BluRaySup.BluRaySupPicture.CreateSupFrame(brSub, mp.Bitmap, mp.FramesPerSeconds, mp.BottomMargin, mp.Alignment);
                        }

                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, mp, i);
                        i++;
                        progressBar1.Refresh();
                        Application.DoEvents();
                        if (i < progressBar1.Maximum)
                            progressBar1.Value = i;
                    }
                }
                else
                {
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
                            if (!string.IsNullOrEmpty(paramUnEqual.Error))
                            {
                                errors.Add(paramUnEqual.Error);
                            }
                        }
                        else
                        {
                            paramUnEqual = MakeMakeBitmapParameter(i, width, height);
                            threadUnEqual = new Thread(DoWork);
                            threadUnEqual.Start(paramUnEqual);

                            if (threadEqual.ThreadState == ThreadState.Running)
                                threadEqual.Join(3000);
                            imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramEqual, i);
                            if (!string.IsNullOrEmpty(paramEqual.Error))
                            {
                                errors.Add(paramEqual.Error);
                            }
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
                }

                if (errors.Count > 0)
                {
                    var errorSB = new StringBuilder();
                    for (int i = 0; i < 20; i++)
                    {
                        if (i < errors.Count)
                            errorSB.AppendLine(errors[i]);
                    }
                    if (errors.Count > 20)
                        errorSB.AppendLine("...");
                    if (!string.IsNullOrEmpty(Configuration.Settings.Language.ExportPngXml.SomeLinesWereTooLongX)) //TODO: Fix in 3.4
                        MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.SomeLinesWereTooLongX, errorSB.ToString()));
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
                else if (_exportType == "IMAGE/FRAME")
                {
                    var empty = new Bitmap(width, height);
                    imagesSavedCount++;
                    string numberString = string.Format("{0:00000}", imagesSavedCount);
                    string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLower());
                    empty.Save(fileName, ImageFormat);

                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath));
                }
                else if (_exportType == "STL")
                {
                    File.WriteAllText(Path.Combine(folderBrowserDialog1.SelectedPath, "DVD_Studio_Pro_Image_script.stl"), sb.ToString());
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath));
                }
                else if (_exportType == "SPUMUX")
                {
                    string s = "<subpictures>" + Environment.NewLine +
                               "\t<stream>" + Environment.NewLine +
                               sb +
                               "\t</stream>" + Environment.NewLine +
                               "</subpictures>";
                    File.WriteAllText(Path.Combine(folderBrowserDialog1.SelectedPath, "spu.xml"), s);
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath));
                }
                else if (_exportType == "FCP")
                {
                    string fileNameNoPath = Path.GetFileName(saveFileDialog1.FileName);
                    string fileNameNoExt = Path.GetFileNameWithoutExtension(fileNameNoPath);

                    int duration = 0;
                    if (_subtitle.Paragraphs.Count > 0)
                        duration = (int)Math.Round(_subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1].EndTime.TotalSeconds * 25.0);
                    string s = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
"<!DOCTYPE xmeml[]>" + Environment.NewLine +
"<xmeml version=\"4\">" + Environment.NewLine +
"  <sequence id=\"" + fileNameNoExt  + "\">" + Environment.NewLine +
"    <updatebehavior>add</updatebehavior>" + Environment.NewLine +
"    <name>" + fileNameNoExt  + @"</name>
    <duration>" + duration.ToString(CultureInfo.InvariantCulture) + @"</duration>
    <rate>
      <ntsc>FALSE</ntsc>
      <timebase>25</timebase>
    </rate>
    <timecode>
      <rate>
        <ntsc>FALSE</ntsc>
        <timebase>25</timebase>
      </rate>
      <string>00:00:00:00</string>
      <frame>0</frame>
      <source>source</source>
      <displayformat>NDF</displayformat>
    </timecode>
    <in>0</in>
    <out>36066</out>
    <media>
      <video>
        <track>
          <enabled>TRUE</enabled>
          <locked>FALSE</locked>
        </track>
        <track>
" + sb +
@"   <enabled>TRUE</enabled>
          <locked>FALSE</locked>
        </track>
      </video>
      <audio>
        <track>
          <enabled>TRUE</enabled>
          <locked>FALSE</locked>
          <outputchannelindex>1</outputchannelindex>
        </track>
        <track>
          <enabled>TRUE</enabled>
          <locked>FALSE</locked>
          <outputchannelindex>2</outputchannelindex>
        </track>
        <track>
          <enabled>TRUE</enabled>
          <locked>FALSE</locked>
          <outputchannelindex>3</outputchannelindex>
        </track>
        <track>
          <enabled>TRUE</enabled>
          <locked>FALSE</locked>
          <outputchannelindex>4</outputchannelindex>
        </track>
      </audio>
    </media>
    <ismasterclip>FALSE</ismasterclip>
  </sequence>
</xmeml>";
                    File.WriteAllText(Path.Combine(folderBrowserDialog1.SelectedPath, saveFileDialog1.FileName), s);
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, Path.GetDirectoryName(saveFileDialog1.FileName)));
                }
                else if (_exportType == "DOST")
                {
                    string header = @"$FORMAT=480
$VERSION=1.2
$ULEAD=TRUE
$DROP=[DROPVALUE]" + Environment.NewLine + Environment.NewLine +
                    "NO\tINTIME\t\tOUTTIME\t\tXPOS\tYPOS\tFILENAME\tFADEIN\tFADEOUT";

                    string dropValue = "30000";
                    if (comboBoxFramerate.Items[comboBoxFramerate.SelectedIndex].ToString() == "23.98")
                        dropValue = "23976";
                    else if (comboBoxFramerate.Items[comboBoxFramerate.SelectedIndex].ToString() == "24")
                        dropValue = "24000";
                    else if (comboBoxFramerate.Items[comboBoxFramerate.SelectedIndex].ToString() == "25")
                        dropValue = "25000";
                    else if (comboBoxFramerate.Items[comboBoxFramerate.SelectedIndex].ToString() == "29.97")
                        dropValue = "29970";
                    else if (comboBoxFramerate.Items[comboBoxFramerate.SelectedIndex].ToString() == "30")
                        dropValue = "30000";
                    else if (comboBoxFramerate.Items[comboBoxFramerate.SelectedIndex].ToString() == "59.94")
                        dropValue = "59940";
                    header = header.Replace("[DROPVALUE]", dropValue);

                    File.WriteAllText(saveFileDialog1.FileName, header + Environment.NewLine + sb);
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, Path.GetDirectoryName(saveFileDialog1.FileName)));
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
                                "<Events Type=\"Graphic\" FirstEventInTC=\"" + BdnXmlTimeCode(first.StartTime) + "\" LastEventOutTC=\"" + BdnXmlTimeCode(last.EndTime) + "\" NumberofEvents=\"" + imagesSavedCount.ToString(CultureInfo.InvariantCulture) + "\"/>" + Environment.NewLine +
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

        private void SetResolution(string xAndY)
        {
            if (string.IsNullOrEmpty(xAndY))
                return;

            xAndY = xAndY.ToLower();
            string[] parts = xAndY.Split('x');
            if (parts.Length == 2 && Utilities.IsInteger(parts[0]) && Utilities.IsInteger(parts[1]))
            {
                if (xAndY == "1920x1080")
                {
                    comboBoxResolution.SelectedIndex = 0;
                }
                else if (xAndY == "1440x1080")
                {
                    comboBoxResolution.SelectedIndex = 2;
                }
                else if (xAndY == "960x720")
                {
                    comboBoxResolution.SelectedIndex = 3;
                }
                else if (xAndY == "848x480")
                {
                    comboBoxResolution.SelectedIndex = 4;
                }
                else if (xAndY == "720x576")
                {
                    comboBoxResolution.SelectedIndex = 5;
                }
                else if (xAndY == "720x480")
                {
                    comboBoxResolution.SelectedIndex = 6;
                }
                else if (xAndY == "640x352")
                {
                    comboBoxResolution.SelectedIndex = 7;
                }
                else
                {
                    comboBoxResolution.Items[8] = xAndY;
                    comboBoxResolution.SelectedIndex = 8;
                }
            }
        }

        private void GetResolution(ref int width, ref int height)
        {
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
                string[] arr = comboBoxResolution.Text.Split('x');
                width = int.Parse(arr[0]);
                height = int.Parse(arr[1]);
            }
            else
            {
                width = 1920;
                height = 1080;
            }
        }


        private int WriteParagraph(int width, StringBuilder sb, int border, int height, int imagesSavedCount,
                                   VobSubWriter vobSubWriter, FileStream binarySubtitleFile, MakeBitmapParameter param, int i)
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
                        vobSubWriter.WriteParagraph(param.P, param.Bitmap, param.Alignment);
                    param.Saved = true;
                }
                else if (_exportType == "FAB")
                {
                    if (!param.Saved)
                    {
                        string numberString = string.Format("IMAGE{0:000}", i);
                        string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLower());
                        param.Bitmap.Save(fileName, ImageFormat);
                        imagesSavedCount++;

                        //RACE001.TIF 00;00;02;02 00;00;03;15 000 000 720 480
                        //RACE002.TIF 00;00;05;18 00;00;09;20 000 000 720 480
                        int top = param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin);
                        int left = (param.ScreenWidth - param.Bitmap.Width) / 2;

                        if (param.Alignment == ContentAlignment.BottomLeft || param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.TopLeft)
                            left = param.BottomMargin;
                        else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
                            left = param.ScreenWidth - param.Bitmap.Width - param.BottomMargin;
                        if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
                            top = param.BottomMargin;
                        if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
                            top = param.ScreenHeight - (param.Bitmap.Height / 2);

                        sb.AppendLine(string.Format("{0} {1} {2} {3} {4} {5} {6}", Path.GetFileName(fileName), FormatFabTime(param.P.StartTime, param), FormatFabTime(param.P.EndTime, param), left, top, left + param.Bitmap.Width, top + param.Bitmap.Height));
                        param.Saved = true;
                    }
                }
                else if (_exportType == "STL")
                {
                    if (!param.Saved)
                    {
                        string numberString = string.Format("IMAGE{0:000}", i);
                        string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLower());
                        param.Bitmap.Save(fileName, ImageFormat);
                        imagesSavedCount++;

                        const string paragraphWriteFormat = "{0} , {1} , {2}\r\n";
                        const string timeFormat = "{0:00}:{1:00}:{2:00}:{3:00}";

                        double factor = (1000.0 / Configuration.Settings.General.CurrentFrameRate);
                        string startTime = string.Format(timeFormat, param.P.StartTime.Hours, param.P.StartTime.Minutes, param.P.StartTime.Seconds, (int)Math.Round(param.P.StartTime.Milliseconds / factor));
                        string endTime = string.Format(timeFormat, param.P.EndTime.Hours, param.P.EndTime.Minutes, param.P.EndTime.Seconds, (int)Math.Round(param.P.EndTime.Milliseconds / factor));
                        sb.Append(string.Format(paragraphWriteFormat, startTime, endTime, fileName));

                        param.Saved = true;
                    }
                }
                else if (_exportType == "SPUMUX")
                {
                    if (!param.Saved)
                    {
                        string numberString = string.Format("IMAGE{0:000}", i);
                        string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLower());


                        foreach (var encoder in ImageCodecInfo.GetImageEncoders())
                        {
                            if (encoder.FormatID == ImageFormat.Png.Guid)
                            {
                                var parameters = new EncoderParameters();
                                parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 8);

                                var nbmp = new NikseBitmap(param.Bitmap);
                                var b = nbmp.ConverTo8BitsPerPixel();
                                b.Save(fileName, encoder, parameters);
                                b.Dispose();

                                break;
                            }
                        }
                        imagesSavedCount++;

                        const string paragraphWriteFormat = "\t\t<spu start=\"{0}\" end=\"{1}\" image=\"{2}\"  />";
                        const string timeFormat = "{0:00}:{1:00}:{2:00}:{3:00}";

                        double factor = (1000.0 / Configuration.Settings.General.CurrentFrameRate);
                        string startTime = string.Format(timeFormat, param.P.StartTime.Hours, param.P.StartTime.Minutes, param.P.StartTime.Seconds, (int)Math.Round(param.P.StartTime.Milliseconds / factor));
                        string endTime = string.Format(timeFormat, param.P.EndTime.Hours, param.P.EndTime.Minutes, param.P.EndTime.Seconds, (int)Math.Round(param.P.EndTime.Milliseconds / factor));
                        sb.AppendLine(string.Format(paragraphWriteFormat, startTime, endTime, fileName));

                        param.Saved = true;
                    }
                }
                else if (_exportType == "FCP")
                {
                    if (!param.Saved)
                    {

                        string numberString = string.Format(Path.GetFileNameWithoutExtension(Path.GetFileName(param.SavDialogFileName)) + "{0:0000}", i);
                        string fileName = numberString + "." + comboBoxImageFormat.Text.ToLower();
                        string fileNameNoPath = Path.GetFileName(fileName);
                        string fileNameNoExt = Path.GetFileNameWithoutExtension(fileNameNoPath);
                        string template = " <clipitem id=\"" + fileNameNoPath + "\">" + Environment.NewLine +
@"            <name>" + fileNameNoPath + @"</name>
            <duration>[DURATION]</duration>
            <rate>
              <ntsc>FALSE</ntsc>
              <timebase>25</timebase>
            </rate>
            <in>[IN]</in>
            <out>[OUT]</out>
            <start>[START]</start>
            <end>[END]</end>
            <pixelaspectratio>PAL-601</pixelaspectratio>
            <stillframe>TRUE</stillframe>
            <anamorphic>FALSE</anamorphic>
            <alphatype>straight</alphatype>
            <masterclipid>" + fileNameNoPath + @"1</masterclipid>" + Environment.NewLine +
"           <file id=\"" + fileNameNoExt + "\">" + @"
              <name>" + fileNameNoPath + @"</name>
              <pathurl>file://localhost/" + fileNameNoPath + @"</pathurl>
              <rate>
                <timebase>25</timebase>
              </rate>
              <duration>[DURATION]</duration>
              <width>" + param.ScreenWidth.ToString() + @"</width>
              <height>" + param.ScreenHeight.ToString() + @"</height>
              <media>
                <video>
                  <duration>[DURATION]</duration>
                  <stillframe>TRUE</stillframe>
                  <samplecharacteristics>
                    <width>720</width>
                    <height>576</height>
                  </samplecharacteristics>
                </video>
              </media>
            </file>
            <sourcetrack>
              <mediatype>video</mediatype>
            </sourcetrack>
            <fielddominance>none</fielddominance>
          </clipitem>";

                        fileName = Path.Combine(Path.GetDirectoryName(param.SavDialogFileName), fileName);

                        if (comboBoxImageFormat.Text == "8-bit png")
                        {
                            foreach (var encoder in ImageCodecInfo.GetImageEncoders())
                            {
                                if (encoder.FormatID == ImageFormat.Png.Guid)
                                {
                                    var parameters = new EncoderParameters();
                                    parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 8);

                                    var nbmp = new NikseBitmap(param.Bitmap);
                                    var b = nbmp.ConverTo8BitsPerPixel();
                                    b.Save(fileName, encoder, parameters);
                                    b.Dispose();

                                    break;
                                }
                            }
                        }
                        else
                        { 
                            param.Bitmap.Save(fileName, ImageFormat);
                        }
                        imagesSavedCount++;

                        int duration = (int)Math.Round(param.P.Duration.TotalSeconds * 25.0);
                        int start = (int)Math.Round(param.P.StartTime.TotalSeconds * 25.0);
                        int end = (int)Math.Round(param.P.EndTime.TotalSeconds * 25.0);

                        template = template.Replace("[DURATION]", duration.ToString(CultureInfo.InvariantCulture));
                        template = template.Replace("[IN]", start.ToString(CultureInfo.InvariantCulture));
                        template = template.Replace("[OUT]", end.ToString(CultureInfo.InvariantCulture));
                        template = template.Replace("[START]", start.ToString(CultureInfo.InvariantCulture));
                        template = template.Replace("[END]", end.ToString(CultureInfo.InvariantCulture));
                        sb.AppendLine(template);

                        param.Saved = true;
                    }
                }
                else if (_exportType == "DOST")
                {
                    if (!param.Saved)
                    {
                        string numberString = string.Format("{0:0000}", i);
                        string fileName = Path.Combine(Path.GetDirectoryName(saveFileDialog1.FileName), Path.GetFileNameWithoutExtension(saveFileDialog1.FileName)) + "_" + numberString + ".png";

                        foreach (var encoder in ImageCodecInfo.GetImageEncoders())
                        {
                            if (encoder.FormatID == ImageFormat.Png.Guid)
                            {
                                var parameters = new EncoderParameters(1);
                                parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 8);

                                var nbmp = new NikseBitmap(param.Bitmap);
                                var b = nbmp.ConverTo8BitsPerPixel();
                                b.Save(fileName, encoder, parameters);
                                b.Dispose();

                                break;
                            }
                        }
                        imagesSavedCount++;

                        const string paragraphWriteFormat = "{0}\t{1}\t{2}\t{4}\t{5}\t{3}\t0\t0";

                        int top = param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin);
                        int left = (param.ScreenWidth - param.Bitmap.Width) / 2;
                        if (param.Alignment == ContentAlignment.BottomLeft || param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.TopLeft)
                            left = param.BottomMargin;
                        else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
                            left = param.ScreenWidth - param.Bitmap.Width - param.BottomMargin;
                        if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
                            top = param.BottomMargin;
                        if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
                            top = param.ScreenHeight - (param.Bitmap.Height / 2);

                        string startTime = BdnXmlTimeCode(param.P.StartTime);
                        string endTime = BdnXmlTimeCode(param.P.EndTime);
                        sb.AppendLine(string.Format(paragraphWriteFormat, numberString, startTime, endTime, Path.GetFileName(fileName), left, top));

                        param.Saved = true;
                    }
                }
                else if (_exportType == "IMAGE/FRAME")
                {
                    if (!param.Saved)
                    {
                        var imageFormat = ImageFormat;

                        int lastFrame = imagesSavedCount;
                        int startFrame = (int)Math.Round(param.P.StartTime.TotalMilliseconds / (1000.0 / param.FramesPerSeconds));
                        var empty = new Bitmap(param.ScreenWidth, param.ScreenHeight);

                        if (imagesSavedCount == 0 && checkBoxSkipEmptyFrameAtStart.Checked)
                        {
                        }
                        else
                        {
                            // Save empty picture for each frame up to start frame
                            for (int k = lastFrame + 1; k < startFrame; k++)
                            {
                                string numberString = string.Format("{0:00000}", k);
                                string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLower());
                                empty.Save(fileName, imageFormat);
                                imagesSavedCount++;
                            }
                        }

                        int endFrame = (int)Math.Round(param.P.EndTime.TotalMilliseconds / (1000.0 / param.FramesPerSeconds));
                        var fullSize = new Bitmap(param.ScreenWidth, param.ScreenHeight);
                        Graphics g = Graphics.FromImage(fullSize);
                        g.DrawImage(param.Bitmap, (param.ScreenWidth - param.Bitmap.Width) / 2, param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin));
                        g.Dispose();

                        if (imagesSavedCount > startFrame)
                            startFrame = imagesSavedCount; // no overlapping

                        // Save sub picture for each frame in duration
                        for (int k = startFrame; k <= endFrame; k++)
                        {
                            string numberString = string.Format("{0:00000}", k);
                            string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLower());
                            fullSize.Save(fileName, imageFormat);
                            imagesSavedCount++;
                        }
                        fullSize.Dispose();
                        param.Saved = true;
                    }
                }
                else
                {
                    if (!param.Saved)
                    {
                        string numberString = string.Format("{0:0000}", i);
                        string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + ".png");
                        param.Bitmap.Save(fileName, ImageFormat.Png);
                        imagesSavedCount++;

                        //<Event InTC="00:00:24:07" OutTC="00:00:31:13" Forced="False">
                        //  <Graphic Width="696" Height="111" X="612" Y="930">subtitle_exp_0001.png</Graphic>
                        //</Event>
                        sb.AppendLine("<Event InTC=\"" + BdnXmlTimeCode(param.P.StartTime) + "\" OutTC=\"" +
                                      BdnXmlTimeCode(param.P.EndTime) + "\" Forced=\"False\">");

                        int x = (width - param.Bitmap.Width) / 2;
                        int y = height - (param.Bitmap.Height + param.BottomMargin);
                        switch (param.Alignment)
                        {
                            case ContentAlignment.BottomLeft:
                                x = border;
                                y = height - (param.Bitmap.Height + param.BottomMargin);
                                break;
                            case ContentAlignment.BottomRight:
                                x = height - param.Bitmap.Width - border;
                                y = height - (param.Bitmap.Height + param.BottomMargin);
                                break;
                            case ContentAlignment.MiddleCenter:
                                x = (width - param.Bitmap.Width) / 2;
                                y = (height - param.Bitmap.Height) / 2;
                                break;
                            case ContentAlignment.MiddleLeft:
                                x = border;
                                y = (height - param.Bitmap.Height) / 2;
                                break;
                            case ContentAlignment.MiddleRight:
                                x = width - param.Bitmap.Width - border;
                                y = (height - param.Bitmap.Height) / 2;
                                break;
                            case ContentAlignment.TopCenter:
                                x = (width - param.Bitmap.Width) / 2;
                                y = border;
                                break;
                            case ContentAlignment.TopLeft:
                                x = border;
                                y = border;
                                break;
                            case ContentAlignment.TopRight:
                                x = width - param.Bitmap.Width - border;
                                y = border;
                                break;
                            default: // ContentAlignment.BottomCenter:
                                break;
                        }

                        sb.AppendLine("  <Graphic Width=\"" + param.Bitmap.Width.ToString(CultureInfo.InvariantCulture) + "\" Height=\"" +
                                      param.Bitmap.Height.ToString(CultureInfo.InvariantCulture) + "\" X=\"" + x.ToString(CultureInfo.InvariantCulture) + "\" Y=\"" + y.ToString(CultureInfo.InvariantCulture) +
                                      "\">" + numberString + ".png</Graphic>");
                        sb.AppendLine("</Event>");
                        param.Saved = true;
                    }
                }
            }
            return imagesSavedCount;
        }

        private ImageFormat ImageFormat
        {
            get
            {
                var imageFormat = ImageFormat.Png;
                if (comboBoxImageFormat.SelectedIndex == 0)
                    imageFormat = ImageFormat.Bmp;
                else if (comboBoxImageFormat.SelectedIndex == 1)
                    imageFormat = ImageFormat.Exif;
                else if (comboBoxImageFormat.SelectedIndex == 2)
                    imageFormat = ImageFormat.Gif;
                else if (comboBoxImageFormat.SelectedIndex == 3)
                    imageFormat = ImageFormat.Jpeg;
                else if (comboBoxImageFormat.SelectedIndex == 4)
                    imageFormat = ImageFormat.Png;
                else if (comboBoxImageFormat.SelectedIndex == 5)
                    imageFormat = ImageFormat.Tiff;
                return imageFormat;
            }
        }

        private string FormatFabTime(TimeCode time, MakeBitmapParameter param)
        {
            if (param.Bitmap.Width == 720 && param.Bitmap.Width == 480) // NTSC
                return string.Format("{0:00};{1:00};{2:00};{3:00}", time.Hours, time.Minutes, time.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(time.Milliseconds));
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        private void SetupImageParameters()
        {
            if (_isLoading)
                return;

            if (subtitleListView1.SelectedItems.Count > 0 && _format.HasStyleSupport)
            {
                Paragraph p = _subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index];
                if (_format.GetType() == typeof(AdvancedSubStationAlpha) || _format.GetType() == typeof(SubStationAlpha))
                {
                    if (!string.IsNullOrEmpty(p.Extra))
                    {
                        comboBoxSubtitleFont.Enabled = false;
                        comboBoxSubtitleFontSize.Enabled = false;
                        buttonBorderColor.Enabled = false;
                        comboBoxHAlign.Enabled = false;
                        panelBorderColor.Enabled = false;
                        checkBoxBold.Enabled = false;
                        buttonColor.Enabled = false;
                        panelColor.Enabled = false;
                        comboBoxBorderWidth.Enabled = false;
                        comboBoxBottomMargin.Enabled = false;

                        SsaStyle style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, _subtitle.Header);
                        if (style != null)
                        {
                            panelColor.BackColor = style.Primary;
                            if (_format.GetType() == typeof(AdvancedSubStationAlpha))
                                panelBorderColor.BackColor = style.Outline;
                            else
                                panelBorderColor.BackColor = style.Background;

                            int i;
                            for (i = 0; i < comboBoxSubtitleFont.Items.Count; i++)
                            {
                                if (string.Compare(comboBoxSubtitleFont.Items[i].ToString(), style.FontName, true) == 0)
                                    comboBoxSubtitleFont.SelectedIndex = i;
                            }
                            for (i = 0; i < comboBoxSubtitleFontSize.Items.Count; i++)
                            {
                                if (string.Compare(comboBoxSubtitleFontSize.Items[i].ToString(), style.FontSize.ToString(), true) == 0)
                                    comboBoxSubtitleFontSize.SelectedIndex = i;
                            }
                            checkBoxBold.Checked = style.Bold;
                            for (i = 0; i < comboBoxBorderWidth.Items.Count; i++)
                            {
                                if (string.Compare(comboBoxBorderWidth.Items[i].ToString(), style.OutlineWidth.ToString(), true) == 0)
                                    comboBoxBorderWidth.SelectedIndex = i;
                            }
                        }
                    }
                }
                else if (_format.GetType() == typeof(TimedText10))
                {
                    if (!string.IsNullOrEmpty(p.Extra))
                    {
                    }
                }
            }

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

        private static Font SetFont(MakeBitmapParameter parameter, float fontSize)
        {
            Font font;
            try
            {
                var fontStyle = FontStyle.Regular;
                if (parameter.SubtitleFontBold)
                    fontStyle = FontStyle.Bold;
                font = new Font(parameter.SubtitleFontName, fontSize, fontStyle);
            }
            catch (Exception exception)
            {
                try
                {
                    var fontStyle = FontStyle.Regular;
                    if (!parameter.SubtitleFontBold)
                        fontStyle = FontStyle.Bold;
                    font = new Font(parameter.SubtitleFontName, fontSize, fontStyle);
                }
                catch
                {
                    MessageBox.Show(exception.Message);

                    if (FontFamily.Families[0].IsStyleAvailable(FontStyle.Regular))
                        font = new Font(FontFamily.Families[0].Name, fontSize);
                    else if (FontFamily.Families.Length > 1 && FontFamily.Families[1].IsStyleAvailable(FontStyle.Regular))
                        font = new Font(FontFamily.Families[1].Name, fontSize);
                    else if (FontFamily.Families.Length > 2 && FontFamily.Families[1].IsStyleAvailable(FontStyle.Regular))
                        font = new Font(FontFamily.Families[2].Name, fontSize);
                    else
                        font = new Font("Arial", fontSize);
                }
            }
            return font;
        }

        private Bitmap GenerateImageFromTextWithStyle(Paragraph p, out MakeBitmapParameter mbp)
        {
            mbp = new MakeBitmapParameter();
            mbp.P = p;

            if (_vobSubOcr != null)
            {
                var index = _subtitle.GetIndex(p);
                if (index >= 0)
                    return _vobSubOcr.GetSubtitleBitmap(index);
            }

            mbp.AlignLeft = comboBoxHAlign.SelectedIndex == 0;
            mbp.AlignRight = comboBoxHAlign.SelectedIndex == 2;
            mbp.SimpleRendering = checkBoxSimpleRender.Checked;
            mbp.BorderWidth = _borderWidth;
            mbp.BorderColor = _borderColor;
            mbp.SubtitleFontName = _subtitleFontName;
            mbp.SubtitleColor = _subtitleColor;
            mbp.SubtitleFontSize = _subtitleFontSize;
            mbp.SubtitleFontBold = _subtitleFontBold;
            mbp.LineHeight = (int) numericUpDownLineSpacing.Value;

            if (_format.HasStyleSupport && !string.IsNullOrEmpty(p.Extra))
            {
                if (_format.GetType() == typeof(SubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, _subtitle.Header);
                    mbp.SubtitleColor = style.Primary;
                    mbp.SubtitleFontBold = style.Bold;
                    mbp.SubtitleFontSize = style.FontSize;
                    if (style.BorderStyle == "3")
                    {
                        mbp.BackgroundColor = style.Background;
                    }
                }
                else if (_format.GetType() == typeof(AdvancedSubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, _subtitle.Header);
                    mbp.SubtitleColor = style.Primary;
                    mbp.SubtitleFontBold = style.Bold;
                    mbp.SubtitleFontSize = style.FontSize;
                    if (style.BorderStyle == "3")
                    {
                        mbp.BackgroundColor = style.Outline;
                    }
                }
            }

            int width = 0;
            int height = 0;
            GetResolution(ref width, ref height);
            mbp.ScreenWidth = width;
            mbp.ScreenHeight = height;
            mbp.Type3D = comboBox3D.SelectedIndex;
            mbp.Depth3D = (int)numericUpDownDepth3D.Value;
            mbp.BottomMargin = comboBoxBottomMargin.SelectedIndex;
            mbp.ShadowWidth = comboBoxShadowWidth.SelectedIndex;
            mbp.ShadowAlpha = (int)numericUpDownShadowTransparency.Value;
            mbp.ShadowColor = panelShadowColor.BackColor;
            mbp.LineHeight = (int)numericUpDownLineSpacing.Value;
            if (_exportType == "VOBSUB" || _exportType == "STL" || _exportType == "SPUMUX")
            {
                mbp.LineJoinRound = true;
            }
            var bmp = GenerateImageFromTextWithStyle(mbp);
            if (_exportType == "VOBSUB" || _exportType == "STL" || _exportType == "SPUMUX")
            {
                var nbmp = new NikseBitmap(bmp);
                nbmp.ConverToFourColors(Color.Transparent, _subtitleColor, _borderColor, GetOutlineColor(_borderColor));
                bmp = nbmp.GetBitmap();
            }
            return bmp;
        }

        private static Bitmap GenerateImageFromTextWithStyle(MakeBitmapParameter parameter)
        {
            string text = parameter.P.Text;

            // remove styles for display text (except italic)
            text = RemoveSubStationAlphaFormatting(text);

            text = text.Replace("<I>", "<i>");
            text = text.Replace("</I>", "</i>");

            text = text.Replace("<B>", "<b>");
            text = text.Replace("</B>", "</b>");

            text = text.Replace("<u>", string.Empty);
            text = text.Replace("</u>", string.Empty);
            text = text.Replace("<U>", string.Empty);
            text = text.Replace("</U>", string.Empty);

            //var bmp = new Bitmap(400, 200);
            //var g = Graphics.FromImage(bmp);
            //if (!parameter.SimpleRendering)
            //    parameter.SubtitleFontSize = g.DpiY * parameter.SubtitleFontSize / 72;
            //Font font;
            //try
            //{
            //    var fontStyle = FontStyle.Regular;
            //    if (parameter.SubtitleFontBold)
            //        fontStyle = FontStyle.Bold;
            //    font = new Font(parameter.SubtitleFontName, parameter.SubtitleFontSize, fontStyle);
            //}
            //catch (Exception exception)
            //{
            //    try
            //    {
            //        var fontStyle = FontStyle.Regular;
            //        if (!parameter.SubtitleFontBold)
            //            fontStyle = FontStyle.Bold;
            //        font = new Font(parameter.SubtitleFontName, parameter.SubtitleFontSize, fontStyle);
            //    }
            //    catch
            //    {
            //        MessageBox.Show(exception.Message);

            //        if (FontFamily.Families[0].IsStyleAvailable(FontStyle.Regular))
            //            font = new Font(FontFamily.Families[0].Name, parameter.SubtitleFontSize);
            //        else if (FontFamily.Families.Length > 1 && FontFamily.Families[1].IsStyleAvailable(FontStyle.Regular))
            //            font = new Font(FontFamily.Families[1].Name, parameter.SubtitleFontSize);
            //        else if (FontFamily.Families.Length > 2 && FontFamily.Families[1].IsStyleAvailable(FontStyle.Regular))
            //            font = new Font(FontFamily.Families[2].Name, parameter.SubtitleFontSize);
            //        else
            //            font = new Font("Arial", parameter.SubtitleFontSize);
            //    }
            //}


            //SizeF textSize = g.MeasureString("Hj!", font);
            //var lineHeight = (textSize.Height * 0.64f);
            var bmp = new Bitmap(400, 200);
            var g = Graphics.FromImage(bmp);
            var fontSize = g.DpiY * parameter.SubtitleFontSize / 72;
            Font font = SetFont(parameter, parameter.SubtitleFontSize);
            var lineHeight = parameter.LineHeight; // (textSize.Height * 0.64f);


            var textSize = g.MeasureString(Utilities.RemoveHtmlTags(text), font);
            g.Dispose();
            bmp.Dispose();
            int sizeX = (int)(textSize.Width * 1.8) + 150;
            int sizeY = (int)(textSize.Height * 0.9) + 50;
            if (sizeX < 1)
                sizeX = 1;
            if (sizeY < 1)
                sizeY = 1;
            bmp = new Bitmap(sizeX, sizeY);
            g = Graphics.FromImage(bmp);
            if (parameter.BackgroundColor != Color.Transparent)
                g.FillRectangle(new SolidBrush(parameter.BackgroundColor), 0, 0, bmp.Width, bmp.Height);

            // align lines with gjpqy, a bit lower
            var lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int baseLinePadding = 13;
            if (parameter.SubtitleFontSize < 30)
                baseLinePadding = 12;
            if (parameter.SubtitleFontSize < 25)
                baseLinePadding = 9;
            if (lines.Length > 0)
            {
                if (lines[lines.Length - 1].Contains("g") || lines[lines.Length - 1].Contains("j") || lines[lines.Length - 1].Contains("p") || lines[lines.Length - 1].Contains("q") || lines[lines.Length - 1].Contains("y") || lines[lines.Length - 1].Contains(","))
                {
                    string textNoBelow = lines[lines.Length - 1].Replace("g", "a").Replace("j", "a").Replace("p", "a").Replace("q", "a").Replace("y", "a").Replace(",", "a");
                    baseLinePadding -= (int)Math.Round((TextDraw.MeasureTextHeight(font, lines[lines.Length - 1], parameter.SubtitleFontBold) - TextDraw.MeasureTextHeight(font, textNoBelow, parameter.SubtitleFontBold)));
                }
                else
                {
                    baseLinePadding += 1;
                }
                if (baseLinePadding < 0)
                    baseLinePadding = 0;
            }

            //TODO: Better baseline - test http://bobpowell.net/formattingtext.aspx
            //float baselineOffset=font.SizeInPoints/font.FontFamily.GetEmHeight(font.Style)*font.FontFamily.GetCellAscent(font.Style);
            //float baselineOffsetPixels = g.DpiY/72f*baselineOffset;
            //baseLinePadding = (int)Math.Round(baselineOffsetPixels);

            var lefts = new List<float>();
            foreach (string line in Utilities.RemoveHtmlFontTag(text.Replace("<i>", string.Empty).Replace("</i>", string.Empty)).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (parameter.AlignLeft)
                    lefts.Add(5);
                else if (parameter.AlignRight)
                    lefts.Add(bmp.Width - (TextDraw.MeasureTextWidth(font, line, parameter.SubtitleFontBold) + 15));
                else
                    lefts.Add((bmp.Width - TextDraw.MeasureTextWidth(font, line, parameter.SubtitleFontBold) + 15) / 2);
            }

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            var sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;// draw the text to a path


            if (parameter.SimpleRendering)
            {
                if (text.StartsWith("<font ") && Utilities.CountTagInText(text, "<font") == 1)
                {
                    parameter.SubtitleColor = Utilities.GetColorFromFontString(text, parameter.SubtitleColor);
                }

                text = Utilities.RemoveHtmlTags(text, true); //TODO: Perhaps check single color...
                System.Drawing.SolidBrush brush = new SolidBrush(parameter.BorderColor);
                int x = 3;
                int y = 3;
                sf.Alignment = StringAlignment.Near;
                if (parameter.AlignLeft)
                {
                    sf.Alignment = StringAlignment.Near;
                }
                else if (parameter.AlignRight)
                {
                    sf.Alignment = StringAlignment.Far;
                    x = parameter.ScreenWidth - 5;
                }
                else
                {
                    sf.Alignment = StringAlignment.Center;
                    x = parameter.ScreenWidth / 2;
                }

                bmp = new Bitmap(parameter.ScreenWidth, sizeY); // parameter.ScreenHeight / 3);

                Graphics surface = Graphics.FromImage(bmp);
                surface.CompositingQuality = CompositingQuality.HighSpeed;
                surface.InterpolationMode = InterpolationMode.Default;
                surface.SmoothingMode = SmoothingMode.HighSpeed;
                surface.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                for (int j = 0; j < parameter.BorderWidth; j++)
                {
                    surface.DrawString(text, font, brush, new PointF { X = x + j, Y = y - 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j, Y = y - 0 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j, Y = y + 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j + 1, Y = y - 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j + 1, Y = y - 0 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j + 1, Y = y + 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j - 1, Y = y - 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j - 1, Y = y - 0 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j - 1, Y = y + 1 + j }, sf);

                    surface.DrawString(text, font, brush, new PointF { X = x - j, Y = y - 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j, Y = y - 0 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j, Y = y + 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j + 1, Y = y - 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j + 1, Y = y - 0 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j + 1, Y = y + 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j - 1, Y = y - 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j - 1, Y = y - 0 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j - 1, Y = y + 1 + j }, sf);

                    surface.DrawString(text, font, brush, new PointF { X = x - j, Y = y - 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j, Y = y - 0 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j, Y = y + 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j + 1, Y = y - 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j + 1, Y = y - 0 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j + 1, Y = y + 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j - 1, Y = y - 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j - 1, Y = y - 0 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - j - 1, Y = y + 1 - j }, sf);

                    surface.DrawString(text, font, brush, new PointF { X = x + j, Y = y - 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j, Y = y - 0 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j, Y = y + 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j + 1, Y = y - 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j + 1, Y = y - 0 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j + 1, Y = y + 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j - 1, Y = y - 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j - 1, Y = y - 0 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j - 1, Y = y + 1 - j }, sf);

                    surface.DrawString(text, font, brush, new PointF { X = x + j, Y = y - 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j, Y = y - 0 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j, Y = y + 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j + 1, Y = y - 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j + 1, Y = y - 0 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j + 1, Y = y + 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j - 1, Y = y - 1 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j - 1, Y = y - 0 + j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + j - 1, Y = y + 1 + j }, sf);

                    surface.DrawString(text, font, brush, new PointF { X = x, Y = y - 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x, Y = y - 0 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x, Y = y + 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + 1, Y = y - 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + 1, Y = y - 0 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x + 1, Y = y + 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - 1, Y = y - 1 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - 1, Y = y - 0 - j }, sf);
                    surface.DrawString(text, font, brush, new PointF { X = x - 1, Y = y + 1 - j }, sf);

                }
                brush.Dispose();
                brush = new SolidBrush(parameter.SubtitleColor);
                surface.CompositingQuality = CompositingQuality.HighQuality;
                surface.SmoothingMode = SmoothingMode.HighQuality;
                surface.InterpolationMode = InterpolationMode.HighQualityBicubic;
                surface.DrawString(text, font, brush, new PointF { X = x, Y = y }, sf);
                surface.Dispose();
                brush.Dispose();
            }
            else
            {
                var path = new GraphicsPath();
                var sb = new StringBuilder();
                int i = 0;
                bool isItalic = false;
                bool isBold = parameter.SubtitleFontBold;
                float left = 5;
                if (lefts.Count > 0)
                    left = lefts[0];
                float top = 5;
                bool newLine = false;
                int lineNumber = 0;
                float leftMargin = left;
                int newLinePathPoint = -1;
                Color c = parameter.SubtitleColor;
                var colorStack = new Stack<Color>();
                var lastText = new StringBuilder();
                while (i < text.Length)
                {
                    if (text.Substring(i).ToLower().StartsWith("<font "))
                    {
                        float addLeft = 0;
                        int oldPathPointIndex = path.PointCount;
                        if (oldPathPointIndex < 0)
                            oldPathPointIndex = 0;

                        if (sb.Length > 0)
                        {
                            lastText.Append(sb.ToString());
                            TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                        }
                        if (path.PointCount > 0)
                        {
                            PointF[] list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
                            for (int k = oldPathPointIndex; k < list.Length; k++)
                            {
                                if (list[k].X > addLeft)
                                    addLeft = list[k].X;
                            }
                        }
                        if (path.PointCount == 0)
                            addLeft = left;
                        else if (addLeft < 0.01)
                            addLeft = left + 2;
                        left = addLeft;

                        DrawShadowAndPAth(parameter, g, path);
                        var p2 = new SolidBrush(c);
                        g.FillPath(p2, path);
                        p2.Dispose();
                        path.Reset();
                        path = new GraphicsPath();
                        sb = new StringBuilder();

                        int endIndex = text.Substring(i).IndexOf(">");
                        if (endIndex == -1)
                        {
                            i += 9999;
                        }
                        else
                        {
                            string fontContent = text.Substring(i, endIndex);
                            if (fontContent.Contains(" color="))
                            {
                                string[] arr = fontContent.Substring(fontContent.IndexOf(" color=") + 7).Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                if (arr.Length > 0)
                                {
                                    string fontColor = arr[0].Trim('\'').Trim('"').Trim('\'');
                                    try
                                    {
                                        colorStack.Push(c); // save old color
                                        if (fontColor.StartsWith("rgb("))
                                        {
                                            arr = fontColor.Remove(0, 4).TrimEnd(')').Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                            c = Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                                        }
                                        else
                                        {
                                            c = ColorTranslator.FromHtml(fontColor);
                                        }
                                    }
                                    catch
                                    {
                                        c = parameter.SubtitleColor;
                                    }
                                }
                            }
                            i += endIndex;
                        }
                    }
                    else if (text.Substring(i).ToLower().StartsWith("</font>"))
                    {
                        if (text.Substring(i).ToLower().Replace("</font>", string.Empty).Length > 0)
                        {
                            if (lastText.ToString().EndsWith(" ") && !sb.ToString().StartsWith(" "))
                            {
                                string t = sb.ToString();
                                sb = new StringBuilder();
                                sb.Append(" " + t);
                            }

                            float addLeft = 0;
                            int oldPathPointIndex = path.PointCount - 1;
                            if (oldPathPointIndex < 0)
                                oldPathPointIndex = 0;
                            if (sb.Length > 0)
                            {
                                if (lastText.Length > 0  && left > 2)
                                    left -= 1.5f;

                                lastText.Append(sb.ToString());

                                TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                            }
                            if (path.PointCount > 0)
                            {
                                PointF[] list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
                                for (int k = oldPathPointIndex; k < list.Length; k++)
                                {
                                    if (list[k].X > addLeft)
                                        addLeft = list[k].X;
                                }
                            }
                            if (addLeft < 0.01)
                                addLeft = left + 2;
                            left = addLeft;

                            DrawShadowAndPAth(parameter, g, path);
                            g.FillPath(new SolidBrush(c), path);
                            path.Reset();
                            sb = new StringBuilder();
                            if (colorStack.Count > 0)
                                c = colorStack.Pop();
                            if (left >= 3)
                                left -= 2.5f;
                        }
                        i += 6;
                    }
                    else if (text.Substring(i).ToLower().StartsWith("<i>"))
                    {
                        if (sb.Length > 0)
                        {
                            lastText.Append(sb);
                            TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                        }
                        isItalic = true;
                        i += 2;
                    }
                    else if (text.Substring(i).ToLower().StartsWith("</i>") && isItalic)
                    {
                        if (lastText.ToString().EndsWith(" ") && !sb.ToString().StartsWith(" "))
                        {
                            string t = sb.ToString();
                            sb = new StringBuilder();
                            sb.Append(" " + t);
                        }
                        lastText.Append(sb);
                        TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                        isItalic = false;
                        i += 3;
                    }
                    else if (text.Substring(i).ToLower().StartsWith("<b>"))
                    {
                        if (sb.Length > 0)
                        {
                            lastText.Append(sb);
                            TextDraw.DrawText(font, sf, path, sb, isItalic, isBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                        }
                        isBold = true;
                        i += 2;
                    }
                    else if (text.Substring(i).ToLower().StartsWith("</b>") && isBold)
                    {
                        if (lastText.ToString().EndsWith(" ") && !sb.ToString().StartsWith(" "))
                        {
                            string t = sb.ToString();
                            sb = new StringBuilder();
                            sb.Append(" " + t);
                        }
                        lastText.Append(sb);
                        TextDraw.DrawText(font, sf, path, sb, isItalic, isBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                        isBold = false;
                        i += 3;
                    }
                    else if (text.Substring(i).StartsWith(Environment.NewLine))
                    {
                        lastText.Append(sb);
                        TextDraw.DrawText(font, sf, path, sb, isItalic, isBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);

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
                    TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);

                DrawShadowAndPAth(parameter, g, path);
                g.FillPath(new SolidBrush(c), path);
                g.Dispose();
            }


            var nbmp = new NikseBitmap(bmp);
            if (parameter.BackgroundColor == Color.Transparent)
            {
                nbmp.CropTransparentSidesAndBottom(baseLinePadding, true);
                nbmp.CropTransparentSidesAndBottom(2, false);
            }
            else
            {
                nbmp.CropSidesAndBottom(baseLinePadding, parameter.BackgroundColor, true);
                nbmp.CropSidesAndBottom(4, parameter.BackgroundColor, true);
                nbmp.CropTop(4, parameter.BackgroundColor);
            }

            if (nbmp.Width > parameter.ScreenWidth)
            {
                parameter.Error = "#" + parameter.P.Number.ToString(CultureInfo.InvariantCulture) + ": " + nbmp.Width.ToString(CultureInfo.InvariantCulture) + " > " + parameter.ScreenWidth.ToString(CultureInfo.InvariantCulture);
            }

            if (parameter.Type3D == 1) // Half-side-by-side 3D
            {
                Bitmap singleBmp = nbmp.GetBitmap();
                Bitmap singleHalfBmp = ScaleToHalfWidth(singleBmp);
                singleBmp.Dispose();
                Bitmap sideBySideBmp = new Bitmap(parameter.ScreenWidth, singleHalfBmp.Height);
                int singleWidth = parameter.ScreenWidth / 2;
                int singleLeftMargin = (singleWidth - singleHalfBmp.Width) / 2;

                using (Graphics gSideBySide = Graphics.FromImage(sideBySideBmp))
                {
                    gSideBySide.DrawImage(singleHalfBmp, singleLeftMargin + parameter.Depth3D, 0);
                    gSideBySide.DrawImage(singleHalfBmp, singleWidth + singleLeftMargin - parameter.Depth3D, 0);
                }
                nbmp = new NikseBitmap(sideBySideBmp);
                if (parameter.BackgroundColor == Color.Transparent)
                    nbmp.CropTransparentSidesAndBottom(2, true);
                else
                    nbmp.CropSidesAndBottom(4, parameter.BackgroundColor, true);
            }
            else if (parameter.Type3D == 2) // Half-Top/Bottom 3D
            {
                Bitmap singleBmp = nbmp.GetBitmap();
                Bitmap singleHalfBmp = ScaleToHalfHeight(singleBmp);
                singleBmp.Dispose();
                Bitmap topBottomBmp = new Bitmap(parameter.ScreenWidth, parameter.ScreenHeight - parameter.BottomMargin);
                int singleHeight = parameter.ScreenHeight / 2;
                int leftM = (parameter.ScreenWidth / 2) - (singleHalfBmp.Width / 2);

                using (Graphics gTopBottom = Graphics.FromImage(topBottomBmp))
                {
                    gTopBottom.DrawImage(singleHalfBmp, leftM + parameter.Depth3D, singleHeight - singleHalfBmp.Height - parameter.BottomMargin);
                    gTopBottom.DrawImage(singleHalfBmp, leftM - parameter.Depth3D, parameter.ScreenHeight - parameter.BottomMargin - singleHalfBmp.Height);
                }
                nbmp = new NikseBitmap(topBottomBmp);
                if (parameter.BackgroundColor == Color.Transparent)
                {
                    nbmp.CropTop(2, Color.Transparent);
                    nbmp.CropTransparentSidesAndBottom(2, false);
                }
                else
                {
                    nbmp.CropTop(4, parameter.BackgroundColor);
                    nbmp.CropSidesAndBottom(4, parameter.BackgroundColor, false);
                }
            }
            return nbmp.GetBitmap();
        }

        private static void DrawShadowAndPAth(MakeBitmapParameter parameter, Graphics g, GraphicsPath path)
        {
            if (parameter.ShadowWidth > 0)
            {
                var shadowPath = (GraphicsPath)path.Clone();
                for (int k = 0; k < parameter.ShadowWidth; k++)
                {
                    var translateMatrix = new Matrix();
                    translateMatrix.Translate(1, 1);
                    shadowPath.Transform(translateMatrix);

                    var p1 = new Pen(Color.FromArgb(parameter.ShadowAlpha, parameter.ShadowColor), parameter.BorderWidth);
                    if (parameter.LineJoinRound)
                        p1.LineJoin = LineJoin.Round;
                    g.DrawPath(p1, shadowPath);
                    p1.Dispose();
                }
            }

            if (parameter.BorderWidth > 0)
            {
                var p1 = new Pen(parameter.BorderColor, parameter.BorderWidth);
                if (parameter.LineJoinRound)
                    p1.LineJoin = LineJoin.Round;
                g.DrawPath(p1, path);
                p1.Dispose();
            }
        }

        private static Bitmap ScaleToHalfWidth(Bitmap bmp)
        {
            int w = bmp.Width / 2;
            Bitmap newImage = new Bitmap(w, bmp.Height);
            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(bmp, new Rectangle(0, 0, w, bmp.Height));
            }
            return newImage;
        }

        private static Bitmap ScaleToHalfHeight(Bitmap bmp)
        {
            int h = bmp.Height / 2;
            Bitmap newImage = new Bitmap(bmp.Width, h);
            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, h));
            }
            return newImage;
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

        internal void Initialize(Subtitle subtitle, SubtitleFormat format, string exportType, string fileName, VideoInfo videoInfo)
        {
            _exportType = exportType;
            _fileName = fileName;
            _format = format;
            if (exportType == "BLURAYSUP")
                Text = "Blu-ray SUP";
            else if (exportType == "VOBSUB")
                Text = "VobSub (sub/idx)";
            else if (exportType == "FAB")
                Text = "FAB Image Script";
            else if (exportType == "IMAGE/FRAME")
                Text = "Image per frame";
            else if (exportType == "STL")
                Text = "DVD Studio Pro STL";
            else if (exportType == "FCP")
                Text = "Final Cut Pro";
            else if (exportType == "DOST")
                Text = "DOST";
            else
                Text = Configuration.Settings.Language.ExportPngXml.Title;

            if (_exportType == "VOBSUB" && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportVobSubFontName))
                _subtitleFontName = Configuration.Settings.Tools.ExportVobSubFontName;
            else if ((_exportType == "BLURAYSUP" || _exportType == "DOST") && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportBluRayFontName))
                _subtitleFontName = Configuration.Settings.Tools.ExportBluRayFontName;
            else if (_exportType == "FCP" && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportFcpFontName))
                _subtitleFontName = Configuration.Settings.Tools.ExportFcpFontName;
            if (_exportType == "VOBSUB" && Configuration.Settings.Tools.ExportVobSubFontSize > 0)
                _subtitleFontSize = Configuration.Settings.Tools.ExportVobSubFontSize;
            else if ((_exportType == "BLURAYSUP" || _exportType == "DOST") && Configuration.Settings.Tools.ExportBluRayFontSize > 0)
                _subtitleFontSize = Configuration.Settings.Tools.ExportBluRayFontSize;
            else if (_exportType == "FCP" && Configuration.Settings.Tools.ExportFcpFontSize > 0)
                _subtitleFontSize = Configuration.Settings.Tools.ExportFcpFontSize;

            if (_exportType == "FCP")
            {
                comboBoxImageFormat.Items.Add("8-bit png");
            }

            if (_exportType == "VOBSUB")
            {
                comboBoxSubtitleFontSize.SelectedIndex = 7;
                int i = 0;
                foreach (string item in comboBoxSubtitleFontSize.Items)
                {
                    if (item == Convert.ToInt32(_subtitleFontSize).ToString(CultureInfo.InvariantCulture))
                        comboBoxSubtitleFontSize.SelectedIndex = i;
                    i++;
                }
                checkBoxSimpleRender.Checked = true;
            }
            else if (_exportType == "BLURAYSUP" || _exportType == "DOST" || _exportType == "FCP")
            {
                comboBoxSubtitleFontSize.SelectedIndex = 16;
                int i = 0;
                foreach (string item in comboBoxSubtitleFontSize.Items)
                {
                    if (item == Convert.ToInt32(_subtitleFontSize).ToString(CultureInfo.InvariantCulture))
                        comboBoxSubtitleFontSize.SelectedIndex = i;
                    i++;
                }
            }
            else
            {
                comboBoxSubtitleFontSize.SelectedIndex = 16;
            }

            groupBoxImageSettings.Text = Configuration.Settings.Language.ExportPngXml.ImageSettings;
            labelSubtitleFont.Text = Configuration.Settings.Language.ExportPngXml.FontFamily;
            labelSubtitleFontSize.Text = Configuration.Settings.Language.ExportPngXml.FontSize;
            labelResolution.Text = Configuration.Settings.Language.ExportPngXml.VideoResolution;
            buttonColor.Text = Configuration.Settings.Language.ExportPngXml.FontColor;
            checkBoxSimpleRender.Text = Configuration.Settings.Language.ExportPngXml.SimpleRendering;

            comboBox3D.Items.Clear();
            comboBox3D.Items.Add(Configuration.Settings.Language.General.None);
            comboBox3D.Items.Add(Configuration.Settings.Language.ExportPngXml.SideBySide3D);
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.ExportPngXml.HalfTopBottom3D)) //TODO: Remove in SE 3.4
                comboBox3D.Items.Add(Configuration.Settings.Language.ExportPngXml.HalfTopBottom3D);
            comboBox3D.SelectedIndex = 0;

            if (!string.IsNullOrEmpty(Configuration.Settings.Language.ExportPngXml.Depth)) //TODO: Remove in SE 3.4
                labelDepth.Text = Configuration.Settings.Language.ExportPngXml.Depth;

            numericUpDownDepth3D.Left = labelDepth.Left + labelDepth.Width + 3;

            if (!string.IsNullOrEmpty(Configuration.Settings.Language.ExportPngXml.Text3D)) //TODO: Remove in SE 3.4
                label3D.Text = Configuration.Settings.Language.ExportPngXml.Text3D;

            comboBox3D.Left = label3D.Left + label3D.Width + 3;

            checkBoxBold.Text = Configuration.Settings.Language.General.Bold;
            buttonBorderColor.Text = Configuration.Settings.Language.ExportPngXml.BorderColor;
            labelBorderWidth.Text = Configuration.Settings.Language.ExportPngXml.BorderWidth;
            labelImageFormat.Text = Configuration.Settings.Language.ExportPngXml.ImageFormat;
            buttonExport.Text = Configuration.Settings.Language.ExportPngXml.ExportAllLines;
            buttonCancel.Text = Configuration.Settings.Language.General.OK;
            labelLanguage.Text = Configuration.Settings.Language.ChooseLanguage.Language;
            labelFrameRate.Text = Configuration.Settings.Language.General.FrameRate;
            labelHorizontalAlign.Text = Configuration.Settings.Language.ExportPngXml.Align;
            labelBottomMargin.Text = Configuration.Settings.Language.ExportPngXml.BottomMargin;
            if (Configuration.Settings.Language.ExportPngXml.Left != null &&
                Configuration.Settings.Language.ExportPngXml.Center != null &&
                Configuration.Settings.Language.ExportPngXml.Right != null)
            {
                comboBoxHAlign.Items.Clear();
                comboBoxHAlign.Items.Add(Configuration.Settings.Language.ExportPngXml.Left);
                comboBoxHAlign.Items.Add(Configuration.Settings.Language.ExportPngXml.Center);
                comboBoxHAlign.Items.Add(Configuration.Settings.Language.ExportPngXml.Right);
            }

            if (!string.IsNullOrEmpty(Configuration.Settings.Language.ExportPngXml.ShadowWidth)) //TODO: Remove in 3.4
            {
                buttonShadowColor.Text = Configuration.Settings.Language.ExportPngXml.ShadowColor;
                labelShadowWidth.Text = Configuration.Settings.Language.ExportPngXml.ShadowWidth;
                labelShadowTransparency.Text = Configuration.Settings.Language.ExportPngXml.Transparency;
            }
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.ExportPngXml.LineHeight)) //TODO: Remove in 3.4
                labelLineHeight.Text = Configuration.Settings.Language.ExportPngXml.LineHeight;

            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            Utilities.InitializeSubtitleFont(subtitleListView1);
            subtitleListView1.AutoSizeAllColumns(this);

            _subtitle = subtitle;
            panelColor.BackColor = _subtitleColor;
            panelBorderColor.BackColor = _borderColor;
            comboBoxBorderWidth.SelectedIndex = 2;
            comboBoxHAlign.SelectedIndex = 1;
            comboBoxResolution.SelectedIndex = 0;
            if ((_exportType == "BLURAYSUP" || _exportType == "DOST") && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportBluRayVideoResolution))
                SetResolution(Configuration.Settings.Tools.ExportBluRayVideoResolution);

            if (exportType == "VOBSUB")
            {
                comboBoxBorderWidth.SelectedIndex = 3;
                if (_exportType == "VOBSUB" && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportVobSubVideoResolution))
                    SetResolution(Configuration.Settings.Tools.ExportVobSubVideoResolution);
                else
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
                if (comboBoxLanguage.Items.Count > 25)
                    comboBoxLanguage.SelectedIndex = 25;
            }

            bool showImageFormat = exportType == "FAB" || exportType == "IMAGE/FRAME" || exportType == "STL" || exportType == "FCP";
            comboBoxImageFormat.Visible = showImageFormat;
            labelImageFormat.Visible = showImageFormat;
            labelFrameRate.Visible = exportType == "BDNXML" || exportType == "BLURAYSUP" || exportType == "DOST" || exportType == "IMAGE/FRAME";
            comboBoxFramerate.Visible = exportType == "BDNXML" || exportType == "BLURAYSUP" || exportType == "DOST" || exportType == "IMAGE/FRAME";
            if (exportType == "BDNXML")
            {
                labelFrameRate.Top = labelLanguage.Top;
                comboBoxFramerate.Top = comboBoxLanguage.Top;
                comboBoxFramerate.Items.Add("23.976");
                comboBoxFramerate.Items.Add("24");
                comboBoxFramerate.Items.Add("25");
                comboBoxFramerate.Items.Add("29.97");
                comboBoxFramerate.Items.Add("30");
                comboBoxFramerate.Items.Add("50");
                comboBoxFramerate.Items.Add("59.94");
                comboBoxFramerate.SelectedIndex = 2;
            }
            else if (exportType == "DOST")
            {
                labelFrameRate.Top = labelLanguage.Top;
                comboBoxFramerate.Top = comboBoxLanguage.Top;
                comboBoxFramerate.Items.Add("23.98");
                comboBoxFramerate.Items.Add("24");
                comboBoxFramerate.Items.Add("25");
                comboBoxFramerate.Items.Add("29.97");
                comboBoxFramerate.Items.Add("30");
                comboBoxFramerate.Items.Add("59.94");
                comboBoxFramerate.SelectedIndex = 2;
            }
            else if (exportType == "IMAGE/FRAME")
            {
                labelFrameRate.Top = labelLanguage.Top;
                comboBoxFramerate.Top = comboBoxLanguage.Top;
                comboBoxFramerate.Items.Add("23.976");
                comboBoxFramerate.Items.Add("24");
                comboBoxFramerate.Items.Add("25");
                comboBoxFramerate.Items.Add("29.97");
                comboBoxFramerate.Items.Add("30");
                comboBoxFramerate.Items.Add("50");
                comboBoxFramerate.Items.Add("59.94");
                comboBoxFramerate.Items.Add("60");
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
            if (comboBoxFramerate.Items.Count >= 2)
            {
                if (Configuration.Settings.General.CurrentFrameRate == 24)
                    comboBoxFramerate.SelectedIndex = 1;
                else if (Configuration.Settings.General.CurrentFrameRate == 25)
                    comboBoxFramerate.SelectedIndex = 2;
            }

            for (int i=0; i<1000; i++)
                comboBoxBottomMargin.Items.Add(i);
            if (Configuration.Settings.Tools.ExportBottomMargin >= 0 && Configuration.Settings.Tools.ExportBottomMargin < comboBoxBottomMargin.Items.Count)
                comboBoxBottomMargin.SelectedIndex = Configuration.Settings.Tools.ExportBottomMargin;

            if (exportType == "BLURAYSUP" || exportType == "IMAGE/FRAME" && Configuration.Settings.Tools.ExportBluRayBottomMargin >= 0 && Configuration.Settings.Tools.ExportBluRayBottomMargin < comboBoxBottomMargin.Items.Count)
                comboBoxBottomMargin.SelectedIndex = Configuration.Settings.Tools.ExportBluRayBottomMargin;

            if (_exportType == "BLURAYSUP" || _exportType == "VOBSUB" || _exportType == "IMAGE/FRAME" || _exportType == "BDNXML")
            {
                comboBoxBottomMargin.Visible = true;
                labelBottomMargin.Visible = true;
            }
            else
            {
                comboBoxBottomMargin.Visible = false;
                labelBottomMargin.Visible = false;
            }

            checkBoxSkipEmptyFrameAtStart.Visible = exportType == "IMAGE/FRAME";

            foreach (var x in FontFamily.Families)
            {
                if (x.IsStyleAvailable(FontStyle.Regular) || x.IsStyleAvailable(FontStyle.Bold))
                {
                    comboBoxSubtitleFont.Items.Add(x.Name);
                    if (string.Compare(x.Name, _subtitleFontName, true) == 0)
                        comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
                }
            }
            if (comboBoxSubtitleFont.SelectedIndex == -1)
                comboBoxSubtitleFont.SelectedIndex = 0; // take first font if default font not found (e.g. linux)

            if (videoInfo != null && videoInfo.Height > 0 && videoInfo.Width > 0)
            {
                comboBoxResolution.Items[8] = videoInfo.Width + "x" + videoInfo.Height;
                comboBoxResolution.SelectedIndex = 8;
            }

            if (_subtitleFontSize == Configuration.Settings.Tools.ExportLastFontSize && Configuration.Settings.Tools.ExportLastLineHeight >= numericUpDownLineSpacing.Minimum &&
                Configuration.Settings.Tools.ExportLastLineHeight <= numericUpDownLineSpacing.Maximum && Configuration.Settings.Tools.ExportLastLineHeight > 0)
            {
                numericUpDownLineSpacing.Value = Configuration.Settings.Tools.ExportLastLineHeight;
            }

            if (Configuration.Settings.Tools.ExportLastBorderWidth >= 0 && Configuration.Settings.Tools.ExportLastBorderWidth < comboBoxBorderWidth.Items.Count)
            {
                try
                {
                    comboBoxBorderWidth.SelectedIndex = Configuration.Settings.Tools.ExportLastBorderWidth;
                }
                catch
                { 
                }
            }
            checkBoxBold.Checked = Configuration.Settings.Tools.ExportLastFontBold;

            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        internal void InitializeFromVobSubOcr(Subtitle subtitle, SubtitleFormat format, string exportType, string fileName, VobSubOcr vobSubOcr, string languageString)
        {
            _vobSubOcr = vobSubOcr;
            Initialize(subtitle, format, exportType, fileName, null);

            //set language
            if (!string.IsNullOrEmpty(languageString))
            {
                if (languageString.Contains("(") && !languageString.StartsWith("("))
                    languageString = languageString.Substring(0, languageString.IndexOf("(") - 1).Trim();
                for (int i = 0; i < comboBoxLanguage.Items.Count; i++)
                {
                    string l = comboBoxLanguage.Items[i].ToString();
                    if (l == languageString && i < comboBoxLanguage.Items.Count)
                        comboBoxLanguage.SelectedIndex = i;
                }
            }

            //Disable options not available when exporting existing images
            comboBoxSubtitleFont.Enabled = false;
            comboBoxSubtitleFontSize.Enabled = false;

            buttonColor.Visible = false;
            panelColor.Visible = false;
            checkBoxBold.Visible = false;
            checkBoxSimpleRender.Visible = false;
            comboBox3D.Enabled = false;
            numericUpDownDepth3D.Enabled = false;

            buttonBorderColor.Visible = false;
            panelBorderColor.Visible = false;
            labelBorderWidth.Visible = false;
            comboBoxBorderWidth.Visible = false;

            buttonShadowColor.Visible = false;
            panelShadowColor.Visible = false;
            labelShadowWidth.Visible = false;
            comboBoxShadowWidth.Visible = false;
            labelShadowTransparency.Visible = false;
            numericUpDownShadowTransparency.Visible = false;
            labelLineHeight.Visible = false;
            numericUpDownLineSpacing.Visible = false;
        }

        private void subtitleListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupImageParameters();
            if (subtitleListView1.SelectedItems.Count > 0)
            {
                MakeBitmapParameter mbp;
                var bmp = GenerateImageFromTextWithStyle(_subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index], out mbp);
                pictureBox1.Image = bmp;

                int w = groupBoxExportImage.Width - 4;
                pictureBox1.Width = bmp.Width;
                pictureBox1.Height = bmp.Height;
                pictureBox1.Top = groupBoxExportImage.Height - bmp.Height - int.Parse(comboBoxBottomMargin.Text);
                pictureBox1.Left = (w - bmp.Width) / 2;
                var alignment = GetAlignmentFromParagraph(_subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index], _format, _subtitle);
                if (_exportType == "BDNXML" || _exportType == "BLURAYSUP" || _exportType == "VOBSUB")
                {
                    if (alignment == ContentAlignment.BottomLeft || alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.TopLeft)
                        pictureBox1.Left = int.Parse(comboBoxBottomMargin.Text);
                    else if (alignment == ContentAlignment.BottomRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.TopRight)
                        pictureBox1.Left = w - bmp.Width - int.Parse(comboBoxBottomMargin.Text);

                    if (alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.MiddleCenter || alignment == ContentAlignment.MiddleRight)
                        pictureBox1.Top = (groupBoxExportImage.Height - 4 - bmp.Height) / 2;
                    else if (alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.TopRight)
                        pictureBox1.Top = int.Parse(comboBoxBottomMargin.Text);
                }
                if (bmp.Width > groupBoxExportImage.Width + 20 || bmp.Height > groupBoxExportImage.Height + 20)
                {
                    pictureBox1.Left = 5;
                    pictureBox1.Top = 20;
                    pictureBox1.Width = groupBoxExportImage.Width - 10;
                    pictureBox1.Height = groupBoxExportImage.Height - 30;
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
                }
                groupBoxExportImage.Text = string.Format("{0}x{1}", bmp.Width, bmp.Height);
                if (!string.IsNullOrEmpty(mbp.Error))
                {
                    groupBoxExportImage.BackColor = Color.Red;
                    groupBoxExportImage.Text = groupBoxExportImage.Text + " - " + mbp.Error;
                }
                else
                {
                    groupBoxExportImage.BackColor = groupBoxImageSettings.BackColor;
                }
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
            Bitmap bmp = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                var mbp = new MakeBitmapParameter();
                mbp.SubtitleFontName = _subtitleFontName;
                mbp.SubtitleFontSize = float.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString());
                mbp.SubtitleFontBold = _subtitleFontBold;
                var fontSize = g.DpiY * mbp.SubtitleFontSize / 72;
                Font font = SetFont(mbp, fontSize);

                SizeF textSize = g.MeasureString("Hj!", font);
                int lineHeight = (int)Math.Round(textSize.Height * 0.64f);
                if (lineHeight >= numericUpDownLineSpacing.Minimum && lineHeight <= numericUpDownLineSpacing.Maximum && lineHeight != numericUpDownLineSpacing.Value)
                    numericUpDownLineSpacing.Value = lineHeight;
                else if (lineHeight > numericUpDownLineSpacing.Maximum)
                    numericUpDownLineSpacing.Value = numericUpDownLineSpacing.Maximum;
            }
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
                Utilities.ShowHelp("#export");
                e.SuppressKeyPress = true;
            }
        }

        private void ExportPngXml_Shown(object sender, EventArgs e)
        {
            comboBoxShadowWidth.SelectedIndex = 0;
            panelShadowColor.BackColor = Color.Black;
            bool shadowVisible = _exportType == "BDNXML" || _exportType == "BLURAYSUP" || _exportType == "DOST" || _exportType == "IMAGE/FRAME" || _exportType == "FCP";
            labelShadowWidth.Visible = shadowVisible;
            buttonShadowColor.Visible = shadowVisible;
            comboBoxShadowWidth.Visible = shadowVisible;
            if (comboBoxShadowWidth.Visible && Configuration.Settings.Tools.ExportBluRayShadow < comboBoxShadowWidth.Items.Count)
                comboBoxShadowWidth.SelectedIndex = Configuration.Settings.Tools.ExportBluRayShadow;
            panelShadowColor.Visible = shadowVisible;
            labelShadowTransparency.Visible = shadowVisible;
            numericUpDownShadowTransparency.Visible = shadowVisible;
            _isLoading = false;
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void comboBoxHAlign_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void checkBoxBold_CheckedChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void buttonCustomResolution_Click(object sender, EventArgs e)
        {
            ChooseResolution cr = new ChooseResolution();
            if (cr.ShowDialog(this) == DialogResult.OK)
            {
                comboBoxResolution.Items[8] = cr.VideoWidth+ "x" + cr.VideoHeight;
                comboBoxResolution.SelectedIndex = 8;
            }
        }

        private void ExportPngXml_ResizeEnd(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void comboBoxBottomMargin_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void checkBoxSideBySide3D_CheckedChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void comboBoxResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void ExportPngXml_SizeChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void ExportPngXml_FormClosing(object sender, FormClosingEventArgs e)
        {
            int width = 1920;
            int height = 1080;
            GetResolution(ref width, ref height);
            string res = string.Format("{0}x{1}", width, height);

            if (_exportType == "VOBSUB")
            {
                Configuration.Settings.Tools.ExportVobSubFontName = _subtitleFontName;
                Configuration.Settings.Tools.ExportVobSubFontSize = (int)_subtitleFontSize;
                Configuration.Settings.Tools.ExportVobSubVideoResolution = res;
            }
            else if (_exportType == "BLURAYSUP")
            {
                Configuration.Settings.Tools.ExportBluRayFontName = _subtitleFontName;
                Configuration.Settings.Tools.ExportBluRayFontSize = (int)_subtitleFontSize;
                Configuration.Settings.Tools.ExportBluRayVideoResolution = res;
            }
            else if (_exportType == "FCP")
            {
                Configuration.Settings.Tools.ExportFcpFontName = _subtitleFontName;
                Configuration.Settings.Tools.ExportFcpFontSize = (int)_subtitleFontSize;
            }
            Configuration.Settings.Tools.ExportFontColor = _subtitleColor;
            Configuration.Settings.Tools.ExportBorderColor = _borderColor;
            if (_exportType == "BLURAYSUP" || _exportType == "DOST")
                Configuration.Settings.Tools.ExportBluRayBottomMargin = comboBoxBottomMargin.SelectedIndex;
            else
                Configuration.Settings.Tools.ExportBottomMargin = comboBoxBottomMargin.SelectedIndex;

            if (comboBoxShadowWidth.Visible)
                Configuration.Settings.Tools.ExportBluRayShadow = comboBoxShadowWidth.SelectedIndex;

            Configuration.Settings.Tools.ExportLastFontSize = (int)_subtitleFontSize;
            Configuration.Settings.Tools.ExportLastLineHeight = (int)numericUpDownLineSpacing.Value;
            Configuration.Settings.Tools.ExportLastBorderWidth = (int)comboBoxBorderWidth.SelectedIndex;
            Configuration.Settings.Tools.ExportLastFontBold = checkBoxBold.Checked;
        }

        private void numericUpDownDepth3D_ValueChanged(object sender, EventArgs e)
        {
            if (!timerPreview.Enabled)
                timerPreview.Start();
        }

        private void comboBox3D_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void timerPreview_Tick(object sender, EventArgs e)
        {
            timerPreview.Stop();
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void saveImageAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count != 1)
                return;

            int selectedIndex = subtitleListView1.SelectedItems[0].Index;

            saveFileDialog1.Title = Configuration.Settings.Language.VobSubOcr.SaveSubtitleImageAs;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.FileName = "Image" + selectedIndex;
            saveFileDialog1.Filter = "PNG image|*.png|BMP image|*.bmp|GIF image|*.gif|TIFF image|*.tiff";
            saveFileDialog1.FilterIndex = 0;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                Bitmap bmp = pictureBox1.Image as Bitmap;
                if (bmp == null)
                {
                    MessageBox.Show("No image!");
                    return;
                }

                try
                {
                    if (saveFileDialog1.FilterIndex == 0)
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    else if (saveFileDialog1.FilterIndex == 1)
                        bmp.Save(saveFileDialog1.FileName);
                    else if (saveFileDialog1.FilterIndex == 2)
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                    else
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Tiff);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void buttonShadowColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelShadowColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelShadowColor.BackColor = colorDialog1.Color;
                subtitleListView1_SelectedIndexChanged(null, null);
            }
        }

        private void panelShadowColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonShadowColor_Click(sender, e);
        }

        private void comboBoxShadowWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void numericUpDownShadowTransparency_ValueChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void comboBoxSubtitleFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                var mbp = new MakeBitmapParameter();
                mbp.SubtitleFontName = _subtitleFontName;
                mbp.SubtitleFontSize = float.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString());
                mbp.SubtitleFontBold = _subtitleFontBold;
                var fontSize = g.DpiY * mbp.SubtitleFontSize / 72;
                Font font = SetFont(mbp, fontSize);

                SizeF textSize = g.MeasureString("Hj!", font);
                int lineHeight = (int)Math.Round(textSize.Height * 0.64f);
                if (lineHeight >= numericUpDownLineSpacing.Minimum && lineHeight <= numericUpDownLineSpacing.Maximum && lineHeight != numericUpDownLineSpacing.Value)
                    numericUpDownLineSpacing.Value = lineHeight;
            }
            bmp.Dispose();
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void numericUpDownLineSpacing_ValueChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }


    }
}

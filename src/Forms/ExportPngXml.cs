using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ExportPngXml : PositionAndSizeForm
    {

        internal class MakeBitmapParameter
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
            public bool BoxSingleLine { get; set; }
            public bool SimpleRendering { get; set; }
            public bool AlignLeft { get; set; }
            public bool AlignRight { get; set; }
            public byte[] Buffer { get; set; }
            public int ScreenWidth { get; set; }
            public int ScreenHeight { get; set; }
            public string VideoResolution { get; set; }
            public int Type3D { get; set; }
            public int Depth3D { get; set; }
            public double FramesPerSeconds { get; set; }
            public int BottomMargin { get; set; }
            public int LeftRightMargin { get; set; }
            public bool Saved { get; set; }
            public ContentAlignment Alignment { get; set; }
            public Point? OverridePosition { get; set; }
            public Color BackgroundColor { get; set; }
            public string SavDialogFileName { get; set; }
            public string Error { get; set; }
            public string LineJoin { get; set; }
            public Color ShadowColor { get; set; }
            public int ShadowWidth { get; set; }
            public int ShadowAlpha { get; set; }
            public Dictionary<string, int> LineHeight { get; set; }
            public bool Forced { get; set; }
            public bool FullFrame { get; set; }
            public Color FullFrameBackgroundcolor { get; set; }

            public MakeBitmapParameter()
            {
                BackgroundColor = Color.Transparent;
            }
        }

        private Subtitle _subtitle;
        private SubtitleFormat _format;
        private static string _formtName;
        private Color _subtitleColor;
        private string _subtitleFontName = "Verdana";
        private float _subtitleFontSize = 25.0f;
        private bool _subtitleFontBold;
        private Color _borderColor;
        private float _borderWidth = 2.0f;
        private bool _isLoading = true;
        private string _exportType = "BDNXML";
        private string _fileName;
        private VobSubOcr _vobSubOcr;
        private readonly System.Windows.Forms.Timer _previewTimer = new System.Windows.Forms.Timer();
        private string _videoFileName;
        private readonly Dictionary<string, int> _lineHeights;

        private const string BoxMultiLineText = "BoxMultiLine";
        private const string BoxSingleLineText = "BoxSingleLine";

        public ExportPngXml()
        {
            InitializeComponent();

            var toolTip = new ToolTip { ShowAlways = true };
            toolTip.SetToolTip(panelFullFrameBackground, Configuration.Settings.Language.ExportPngXml.ChooseBackgroundColor);
            _lineHeights = new Dictionary<string, int>();
            comboBoxImageFormat.SelectedIndex = 4;
            _subtitleColor = Configuration.Settings.Tools.ExportFontColor;
            _borderColor = Configuration.Settings.Tools.ExportBorderColor;
            _previewTimer.Tick += previewTimer_Tick;
            _previewTimer.Interval = 100;
            labelLineHeightStyle.Text = string.Empty;
            _subtitleColor = Color.White;
            _borderColor = Color.Black;
        }

        private void previewTimer_Tick(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            GeneratePreview();
        }

        private double FrameRate
        {
            get
            {
                if (comboBoxFrameRate.SelectedItem == null)
                    return 25;

                string s = comboBoxFrameRate.SelectedItem.ToString();
                s = s.Replace(",", ".").Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".").Trim();
                double d;
                if (double.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                    return d;
                return 25;
            }
        }

        private int MillisecondsToFramesMaxFrameRate(double milliseconds)
        {
            int frames = (int)Math.Round(milliseconds / (TimeCode.BaseUnit / FrameRate));
            if (frames >= FrameRate)
                frames = (int)(FrameRate - 0.01);
            return frames;
        }

        private string ToHHMMSSFF(TimeCode timecode)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", timecode.Hours, timecode.Minutes, timecode.Seconds, MillisecondsToFramesMaxFrameRate(timecode.Milliseconds));
        }

        private static ContentAlignment GetAlignmentFromParagraph(MakeBitmapParameter p, SubtitleFormat format, Subtitle subtitle)
        {
            var alignment = ContentAlignment.BottomCenter;
            if (p.AlignLeft)
                alignment = ContentAlignment.BottomLeft;
            else if (p.AlignRight)
                alignment = ContentAlignment.BottomRight;

            if (format.HasStyleSupport && !string.IsNullOrEmpty(p.P.Extra))
            {
                if (format.GetType() == typeof(SubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(p.P.Extra, subtitle.Header);
                    alignment = GetSsaAlignment("{\\a" + style.Alignment + "}", alignment);
                }
                else if (format.GetType() == typeof(AdvancedSubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(p.P.Extra, subtitle.Header);
                    alignment = GetAssAlignment("{\\an" + style.Alignment + "}", alignment);
                }
            }

            string text = p.P.Text;
            if (format.GetType() == typeof(SubStationAlpha) && text.Length > 5)
            {
                text = p.P.Text.Substring(0, 6);
                alignment = GetSsaAlignment(text, alignment);
            }
            else if (text.Length > 6)
            {
                text = p.P.Text.Substring(0, 6);
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

            parameter.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
            parameter.Bitmap = GenerateImageFromTextWithStyle(parameter);
            if (parameter.Type == "BLURAYSUP")
            {
                MakeBluRaySupImage(parameter);
            }
        }

        internal static void MakeBluRaySupImage(MakeBitmapParameter param)
        {
            var brSub = new BluRaySupPicture
            {
                StartTime = (long)param.P.StartTime.TotalMilliseconds,
                EndTime = (long)param.P.EndTime.TotalMilliseconds,
                Width = param.ScreenWidth,
                Height = param.ScreenHeight,
                IsForced = param.Forced,
                CompositionNumber = param.P.Number * 2
            };
            if (param.FullFrame)
            {
                var nbmp = new NikseBitmap(param.Bitmap);
                nbmp.ReplaceTransparentWith(param.FullFrameBackgroundcolor);
                using (var bmp = nbmp.GetBitmap())
                {
                    int top = param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin);
                    int left = (param.ScreenWidth - param.Bitmap.Width) / 2;

                    var b = new NikseBitmap(param.ScreenWidth, param.ScreenHeight);
                    {
                        b.Fill(param.FullFrameBackgroundcolor);
                        using (var fullSize = b.GetBitmap())
                        {
                            if (param.Alignment == ContentAlignment.BottomLeft || param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.TopLeft)
                                left = param.LeftRightMargin;
                            else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
                                left = param.ScreenWidth - param.Bitmap.Width - param.LeftRightMargin;
                            if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
                                top = param.BottomMargin;
                            if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
                                top = param.ScreenHeight - (param.Bitmap.Height / 2);

                            if (param.OverridePosition != null &&
                                param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.ScreenWidth &&
                                param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.ScreenHeight)
                            {
                                left = param.OverridePosition.Value.X;
                                top = param.OverridePosition.Value.Y;
                            }

                            using (var g = Graphics.FromImage(fullSize))
                            {
                                g.DrawImage(bmp, left, top);
                                g.Dispose();
                            }
                            param.Buffer = BluRaySupPicture.CreateSupFrame(brSub, fullSize, param.FramesPerSeconds, 0, 0, ContentAlignment.BottomCenter);
                        }
                    }
                }
            }
            else
            {
                param.Buffer = BluRaySupPicture.CreateSupFrame(brSub, param.Bitmap, param.FramesPerSeconds, param.BottomMargin, param.LeftRightMargin, param.Alignment, param.OverridePosition);
            }
        }

        internal MakeBitmapParameter MakeMakeBitmapParameter(int index, int screenWidth, int screenHeight)
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
                VideoResolution = comboBoxResolution.Text,
                Bitmap = null,
                FramesPerSeconds = FrameRate,
                BottomMargin = comboBoxBottomMargin.SelectedIndex,
                LeftRightMargin = comboBoxLeftRightMargin.SelectedIndex,
                Saved = false,
                Alignment = ContentAlignment.BottomCenter,
                Type3D = comboBox3D.SelectedIndex,
                Depth3D = (int)numericUpDownDepth3D.Value,
                BackgroundColor = Color.Transparent,
                SavDialogFileName = saveFileDialog1.FileName,
                ShadowColor = panelShadowColor.BackColor,
                ShadowWidth = comboBoxShadowWidth.SelectedIndex,
                ShadowAlpha = (int)numericUpDownShadowTransparency.Value,
                LineHeight = _lineHeights,
                FullFrame = checkBoxFullFrameImage.Checked,
                FullFrameBackgroundcolor = panelFullFrameBackground.BackColor,
            };
            if (index < _subtitle.Paragraphs.Count)
            {
                parameter.P = _subtitle.Paragraphs[index];
                parameter.Alignment = GetAlignmentFromParagraph(parameter, _format, _subtitle);
                parameter.OverridePosition = GetAssPoint(parameter.P.Text);
                parameter.Forced = subtitleListView1.Items[index].Checked;

                if (_format.HasStyleSupport && !string.IsNullOrEmpty(parameter.P.Extra))
                {
                    if (_format.GetType() == typeof(SubStationAlpha))
                    {
                        var style = AdvancedSubStationAlpha.GetSsaStyle(parameter.P.Extra, _subtitle.Header);
                        parameter.SubtitleColor = style.Primary;
                        parameter.SubtitleFontBold = style.Bold;
                        parameter.SubtitleFontSize = style.FontSize;
                        parameter.SubtitleFontName = style.FontName;
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
                        parameter.SubtitleFontName = style.FontName;
                        if (style.BorderStyle == "3")
                        {
                            parameter.BackgroundColor = style.Outline;
                        }
                    }
                }

                if (comboBoxBorderWidth.SelectedItem.ToString() == Configuration.Settings.Language.ExportPngXml.BorderStyleBoxForEachLine)
                {
                    parameter.BoxSingleLine = true;
                    parameter.BackgroundColor = panelBorderColor.BackColor;
                    parameter.BorderWidth = 0;
                }
                else if (comboBoxBorderWidth.SelectedItem.ToString() == Configuration.Settings.Language.ExportPngXml.BorderStyleOneBox)
                {
                    parameter.BoxSingleLine = false;
                    parameter.BackgroundColor = panelBorderColor.BackColor;
                    parameter.BorderWidth = 0;
                }
                else
                {
                    _borderWidth = float.Parse(Utilities.RemoveNonNumbers(comboBoxBorderWidth.SelectedItem.ToString()));
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
            FixStartEndWithSameTimeCode();

            if (Configuration.Settings.General.CurrentVideoOffsetInMs > 0)
            {
                _subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(Configuration.Settings.General.CurrentVideoOffsetInMs));
            }

            var errors = new List<string>();
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
                saveFileDialog1.Title = Configuration.Settings.Language.ExportPngXml.SaveFcpAs;
                saveFileDialog1.DefaultExt = "*.xml";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Xml files|*.xml";
            }
            else if (_exportType == "DOST")
            {
                saveFileDialog1.Title = Configuration.Settings.Language.ExportPngXml.SaveDostAs;
                saveFileDialog1.DefaultExt = "*.dost";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Dost files|*.dost";
            }
            else if (_exportType == "DCINEMA_INTEROP")
            {
                saveFileDialog1.Title = Configuration.Settings.Language.ExportPngXml.SaveDigitalCinemaInteropAs;
                saveFileDialog1.DefaultExt = "*.xml";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Xml files|*.xml";
            }
            else if (_exportType == "EDL")
            {
                saveFileDialog1.Title = Configuration.Settings.Language.ExportPngXml.SavePremiereEdlAs;
                saveFileDialog1.DefaultExt = "*.edl";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "EDL files|*.edl";
            }

            if (_exportType == "BLURAYSUP" && saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "VOBSUB" && saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "BDNXML" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "FAB" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "IMAGE/FRAME" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "STL" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "SPUMUX" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "FCP" && saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "DOST" && saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "DCINEMA_INTEROP" && saveFileDialog1.ShowDialog(this) == DialogResult.OK ||
                _exportType == "EDL" && saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                int width;
                int height;
                GetResolution(out width, out height);

                FileStream binarySubtitleFile = null;
                VobSubWriter vobSubWriter = null;
                if (_exportType == "BLURAYSUP")
                    binarySubtitleFile = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                else if (_exportType == "VOBSUB")
                    vobSubWriter = new VobSubWriter(saveFileDialog1.FileName, width, height, comboBoxBottomMargin.SelectedIndex, comboBoxLeftRightMargin.SelectedIndex, 32, _subtitleColor, _borderColor, !checkBoxTransAntiAliase.Checked, (DvdSubtitleLanguage)comboBoxLanguage.SelectedItem);

                progressBar1.Value = 0;
                progressBar1.Maximum = _subtitle.Paragraphs.Count - 1;
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
                            MakeBluRaySupImage(mp);
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
                                threadUnEqual.Join();
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
                                threadEqual.Join();
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
                            threadEqual.Join();
                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramEqual, i);
                        if (threadUnEqual.ThreadState == ThreadState.Running)
                            threadUnEqual.Join();
                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramUnEqual, i);
                    }
                    else
                    {
                        if (threadUnEqual.ThreadState == ThreadState.Running)
                            threadUnEqual.Join();
                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramUnEqual, i);
                        if (threadEqual.ThreadState == ThreadState.Running)
                            threadEqual.Join();
                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramEqual, i);
                    }
                }

                if (errors.Count > 0)
                {
                    var errorSb = new StringBuilder();
                    for (int i = 0; i < 20; i++)
                    {
                        if (i < errors.Count)
                            errorSb.AppendLine(errors[i]);
                    }
                    if (errors.Count > 20)
                        errorSb.AppendLine("...");
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.SomeLinesWereTooLongX, errorSb));
                }

                progressBar1.Visible = false;
                if (_exportType == "BLURAYSUP")
                {
                    binarySubtitleFile.Close();
                    MessageBox.Show(string.Format(Configuration.Settings.Language.Main.SavedSubtitleX, saveFileDialog1.FileName));
                }
                else if (_exportType == "VOBSUB")
                {
                    vobSubWriter.WriteIdxFile();
                    vobSubWriter.Dispose();
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
                    SaveImage(empty, fileName, ImageFormat);

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
                               "  <sequence id=\"" + System.Security.SecurityElement.Escape(fileNameNoExt) + "\">" + Environment.NewLine +
                               "    <updatebehavior>add</updatebehavior>" + Environment.NewLine +
                               "    <name>" + System.Security.SecurityElement.Escape(fileNameNoExt) + @"</name>
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
    <out>[OUT]</out>
    <media>
      <video>
        <format>
          <samplecharacteristics>
            <rate>
              <timebase>25</timebase>
            </rate>
            <width>1920</width>
            <height>1080</height>
            <anamorphic>FALSE</anamorphic>
            <pixelaspectratio>square</pixelaspectratio>
            <fielddominance>none</fielddominance>
            <colordepth>32</colordepth>
          </samplecharacteristics>
        </format>
        <track>
          <enabled>TRUE</enabled>
          <locked>FALSE</locked>
        </track>
        <track>
" + sb + @"   <enabled>TRUE</enabled>
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
                    s = s.Replace("<timebase>25</timebase>", "<timebase>" + comboBoxFrameRate.Text + "</timebase>");

                    if (_subtitle.Paragraphs.Count > 0)
                    {
                        var end = (int)Math.Round(_subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1].EndTime.TotalSeconds * FrameRate);
                        end++;
                        s = s.Replace("[OUT]", end.ToString(CultureInfo.InvariantCulture));
                    }

                    if (comboBoxLanguage.Text == "NTSC")
                        s = s.Replace("<ntsc>FALSE</ntsc>", "<ntsc>TRUE</ntsc>");

                    s = s.Replace("<width>1920</width>", "<width>" + width.ToString(CultureInfo.InvariantCulture) + "</width>");
                    s = s.Replace("<height>1080</height>", "<height>" + height.ToString(CultureInfo.InvariantCulture) + "</height>");

                    if (comboBoxImageFormat.Text.Contains("8-bit"))
                        s = s.Replace("<colordepth>32</colordepth>", "<colordepth>8</colordepth>");

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
                    if (comboBoxFrameRate.SelectedIndex == -1)
                    {
                        var numberAsString = comboBoxFrameRate.Text.Trim().Replace(".", string.Empty).Replace(",", string.Empty).Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, string.Empty);
                        if (numberAsString.Length > 0 && Utilities.IsInteger(numberAsString))
                            dropValue = numberAsString;
                    }
                    else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "23.98")
                        dropValue = "23976";
                    else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "24")
                        dropValue = "24000";
                    else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "25")
                        dropValue = "25000";
                    else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "29.97")
                        dropValue = "29970";
                    else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "30")
                        dropValue = "30000";
                    else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "59.94")
                        dropValue = "59940";
                    header = header.Replace("[DROPVALUE]", dropValue);
                    comboBoxFrameRate.SelectedIndex = 0;

                    File.WriteAllText(saveFileDialog1.FileName, header + Environment.NewLine + sb);
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, Path.GetDirectoryName(saveFileDialog1.FileName)));
                }
                else if (_exportType == "DCINEMA_INTEROP")
                {
                    var doc = new XmlDocument();
                    string title = "unknown";
                    if (!string.IsNullOrEmpty(_fileName))
                        title = Path.GetFileNameWithoutExtension(_fileName);

                    string guid = Guid.NewGuid().ToString().Replace("-", string.Empty).Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
                    doc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                                "<DCSubtitle Version=\"1.1\">" + Environment.NewLine +
                                "<SubtitleID>" + guid + "</SubtitleID>" + Environment.NewLine +
                                "<MovieTitle>" + title + "</MovieTitle>" + Environment.NewLine +
                                "<ReelNumber>1</ReelNumber>" + Environment.NewLine +
                                "<Language>English</Language>" + Environment.NewLine +
                                sb +
                                "</DCSubtitle>");
                    string fName = saveFileDialog1.FileName;
                    if (!fName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        fName += ".xml";
                    File.WriteAllText(fName, SubtitleFormat.ToUtf8XmlString(doc));
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, Path.GetDirectoryName(fName)));
                }
                else if (_exportType == "EDL")
                {
                    string header = "TITLE: ( no title )" + Environment.NewLine + Environment.NewLine;
                    File.WriteAllText(saveFileDialog1.FileName, header + sb);
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, Path.GetDirectoryName(saveFileDialog1.FileName)));
                }
                else if (_exportType == "DCINEMA_INTEROP")
                {
                    var doc = new XmlDocument();
                    string title = "unknown";
                    if (!string.IsNullOrEmpty(_fileName))
                        title = Path.GetFileNameWithoutExtension(_fileName);

                    string guid = Guid.NewGuid().ToString().Replace("-", string.Empty).Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
                    doc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                                "<DCSubtitle Version=\"1.1\">" + Environment.NewLine +
                                "<SubtitleID>" + guid + "</SubtitleID>" + Environment.NewLine +
                                "<MovieTitle>" + title + "</MovieTitle>" + Environment.NewLine +
                                "<ReelNumber>1</ReelNumber>" + Environment.NewLine +
                                "<Language>English</Language>" + Environment.NewLine +
                                sb +
                                "</DCSubtitle>");
                    string fName = saveFileDialog1.FileName;
                    if (!fName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        fName += ".xml";
                    File.WriteAllText(fName, SubtitleFormat.ToUtf8XmlString(doc));
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, Path.GetDirectoryName(fName)));
                }
                else
                {
                    int resW;
                    int resH;
                    GetResolution(out resW, out resH);
                    string videoFormat = "1080p";
                    if (resW == 1920 && resH == 1080)
                        videoFormat = "1080p";
                    else if (resW == 1280 && resH == 720)
                        videoFormat = "720p";
                    else if (resW == 848 && resH == 480)
                        videoFormat = "480p";
                    else if (resW > 0 && resH > 0)
                        videoFormat = resW + "x" + resH;

                    var doc = new XmlDocument();
                    Paragraph first = _subtitle.Paragraphs[0];
                    Paragraph last = _subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1];
                    doc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                                "<BDN Version=\"0.93\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"BD-03-006-0093b BDN File Format.xsd\">" + Environment.NewLine +
                                "<Description>" + Environment.NewLine +
                                "<Name Title=\"subtitle_exp\" Content=\"\"/>" + Environment.NewLine +
                                "<Language Code=\"eng\"/>" + Environment.NewLine +
                                "<Format VideoFormat=\"" + videoFormat + "\" FrameRate=\"" + FrameRate.ToString(CultureInfo.InvariantCulture) + "\" DropFrame=\"False\"/>" + Environment.NewLine +
                                "<Events Type=\"Graphic\" FirstEventInTC=\"" + ToHHMMSSFF(first.StartTime) + "\" LastEventOutTC=\"" + ToHHMMSSFF(last.EndTime) + "\" NumberofEvents=\"" + imagesSavedCount.ToString(CultureInfo.InvariantCulture) + "\"/>" + Environment.NewLine +
                                "</Description>" + Environment.NewLine +
                                "<Events>" + Environment.NewLine +
                                "</Events>" + Environment.NewLine +
                                "</BDN>");
                    XmlNode events = doc.DocumentElement.SelectSingleNode("Events");
                    doc.PreserveWhitespace = true;
                    events.InnerXml = sb.ToString();
                    File.WriteAllText(Path.Combine(folderBrowserDialog1.SelectedPath, "BDN_Index.xml"), FormatUtf8Xml(doc), Encoding.UTF8);
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath));
                }
            }
            buttonExport.Enabled = true;

            if (Configuration.Settings.General.CurrentVideoOffsetInMs > 0)
            {
                _subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(-Configuration.Settings.General.CurrentVideoOffsetInMs));
            }
        }

        private static string FormatUtf8Xml(XmlDocument doc)
        {
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 }))
            {
                doc.Save(writer);
            }
            return sb.ToString().Replace(" encoding=\"utf-16\"", " encoding=\"utf-8\"").Trim(); // "replace hack" due to missing encoding (encoding only works if it's the only parameter...)
        }

        private void SaveImage(Bitmap bmp, string fileName, ImageFormat imageFormat)
        {
            if (Equals(imageFormat, ImageFormat.Icon))
            {
                var nikseBitmap = new NikseBitmap(bmp);
                nikseBitmap.SaveAsTarga(fileName);
            }
            else
            {
                bmp.Save(fileName, imageFormat);
            }
        }

        private void FixStartEndWithSameTimeCode()
        {
            for (int i = 0; i < _subtitle.Paragraphs.Count - 1; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                Paragraph next = _subtitle.Paragraphs[i + 1];
                if (Math.Abs(p.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds) < 0.1)
                    p.EndTime.TotalMilliseconds--;
            }
        }

        private void SetResolution(string xAndY)
        {
            if (string.IsNullOrEmpty(xAndY))
                return;

            xAndY = xAndY.ToLower();

            if (_exportType == "FCP")
            {
                switch (xAndY)
                {
                    case "720x480":
                        xAndY = "NTSC-601";
                        break;
                    case "720x576":
                        xAndY = "PAL-601";
                        break;
                    case "640x480":
                        xAndY = "square";
                        break;
                    case "1280x720":
                        xAndY = "DVCPROHD-720P";
                        break;
                    case "960x720":
                        xAndY = "HD-(960x720)";
                        break;
                    case "1920x1080":
                        xAndY = "DVCPROHD-1080i60";
                        break;
                    case "1280x1080":
                        xAndY = "HD-(1280x1080)";
                        break;
                    case "1440x1080":
                        xAndY = "HD-(1440x1080)";
                        break;
                }
            }

            if (_exportType == "FCP" || Regex.IsMatch(xAndY, @"\d+x\d+", RegexOptions.IgnoreCase))
            {
                for (int i = 0; i < comboBoxResolution.Items.Count; i++)
                {
                    if (comboBoxResolution.Items[i].ToString().Contains(xAndY))
                    {
                        comboBoxResolution.SelectedIndex = i;
                        return;
                    }
                }
                comboBoxResolution.Items[comboBoxResolution.Items.Count - 1] = xAndY;
                comboBoxResolution.SelectedIndex = comboBoxResolution.Items.Count - 1;
            }
        }

        private void GetResolution(out int width, out int height)
        {
            width = 1920;
            height = 1080;
            if (comboBoxResolution.SelectedIndex < 0)
                return;

            string text = comboBoxResolution.Text.Trim();

            if (_exportType == "FCP")
            {
                if (text == "NTSC-601")
                {
                    width = 720;
                    height = 480;
                    return;
                }

                if (text == "PAL-601")
                {
                    width = 720;
                    height = 576;
                    return;
                }

                if (text == "square")
                {
                    width = 640;
                    height = 480;
                    return;
                }

                if (text == "DVCPROHD-720P")
                {
                    width = 1280;
                    height = 720;
                    return;
                }

                if (text == "HD-(960x720)")
                {
                    width = 960;
                    height = 720;
                    return;
                }

                if (text == "DVCPROHD-1080i60")
                {
                    width = 1920;
                    height = 1080;
                    return;
                }

                if (text == "HD-(1280x1080)")
                {
                    width = 1280;
                    height = 1080;
                    return;
                }

                if (text == "DVCPROHD-1080i50")
                {
                    width = 1920;
                    height = 1080;
                    return;
                }

                if (text == "HD-(1440x1080)")
                {
                    width = 1440;
                    height = 1080;
                    return;
                }
            }

            if (text.Contains('('))
                text = text.Remove(0, text.IndexOf('(')).Trim();
            text = text.TrimStart('(').TrimEnd(')').Trim();
            string[] arr = text.Split('x');
            width = int.Parse(arr[0]);
            height = int.Parse(arr[1]);
        }

        private int WriteParagraph(int width, StringBuilder sb, int border, int height, int imagesSavedCount, VobSubWriter vobSubWriter, FileStream binarySubtitleFile, MakeBitmapParameter param, int i)
        {
            if (param.Bitmap != null)
            {
                if (_exportType == "BLURAYSUP")
                {
                    if (!param.Saved)
                    {
                        binarySubtitleFile.Write(param.Buffer, 0, param.Buffer.Length);
                    }
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

                        if (checkBoxFullFrameImage.Visible && checkBoxFullFrameImage.Checked)
                        {
                            var nbmp = new NikseBitmap(param.Bitmap);
                            nbmp.ReplaceTransparentWith(panelFullFrameBackground.BackColor);
                            using (var bmp = nbmp.GetBitmap())
                            {
                                // param.Bitmap.Save(fileName, ImageFormat);
                                imagesSavedCount++;

                                //RACE001.TIF 00;00;02;02 00;00;03;15 000 000 720 480
                                //RACE002.TIF 00;00;05;18 00;00;09;20 000 000 720 480
                                int top = param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin);
                                int left = (param.ScreenWidth - param.Bitmap.Width) / 2;

                                var b = new NikseBitmap(param.ScreenWidth, param.ScreenHeight);
                                {
                                    b.Fill(panelFullFrameBackground.BackColor);
                                    using (var fullSize = b.GetBitmap())
                                    {
                                        if (param.Alignment == ContentAlignment.BottomLeft || param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.TopLeft)
                                            left = param.LeftRightMargin;
                                        else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
                                            left = param.ScreenWidth - param.Bitmap.Width - param.LeftRightMargin;
                                        if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
                                            top = param.BottomMargin;
                                        if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
                                            top = (param.ScreenHeight - param.Bitmap.Height) / 2;

                                        if (param.OverridePosition.HasValue &&
                                            param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.Bitmap.Width &&
                                            param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.Bitmap.Height)
                                        {
                                            left = param.OverridePosition.Value.X;
                                            top = param.OverridePosition.Value.Y;
                                        }

                                        using (var g = Graphics.FromImage(fullSize))
                                        {
                                            g.DrawImage(bmp, left, top);
                                            g.Dispose();
                                        }
                                        SaveImage(fullSize, fileName, ImageFormat);
                                    }
                                }
                                left = 0;
                                top = 0;
                                sb.AppendLine(string.Format("{0} {1} {2} {3} {4} {5} {6}", Path.GetFileName(fileName), FormatFabTime(param.P.StartTime, param), FormatFabTime(param.P.EndTime, param), left, top, left + param.ScreenWidth, top + param.ScreenHeight));
                            }
                        }
                        else
                        {
                            SaveImage(param.Bitmap, fileName, ImageFormat);

                            imagesSavedCount++;

                            //RACE001.TIF 00;00;02;02 00;00;03;15 000 000 720 480
                            //RACE002.TIF 00;00;05;18 00;00;09;20 000 000 720 480
                            int top = param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin);
                            int left = (param.ScreenWidth - param.Bitmap.Width) / 2;

                            if (param.Alignment == ContentAlignment.BottomLeft || param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.TopLeft)
                                left = param.LeftRightMargin;
                            else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
                                left = param.ScreenWidth - param.Bitmap.Width - param.LeftRightMargin;
                            if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
                                top = param.BottomMargin;
                            if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
                                top = (param.ScreenHeight - param.Bitmap.Height) / 2;

                            if (param.OverridePosition.HasValue &&
                                param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.Bitmap.Width &&
                                param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.Bitmap.Height)
                            {
                                left = param.OverridePosition.Value.X;
                                top = param.OverridePosition.Value.Y;
                            }

                            sb.AppendLine(string.Format("{0} {1} {2} {3} {4} {5} {6}", Path.GetFileName(fileName), FormatFabTime(param.P.StartTime, param), FormatFabTime(param.P.EndTime, param), left, top, left + param.Bitmap.Width, top + param.Bitmap.Height));
                        }
                        param.Saved = true;
                    }
                }
                else if (_exportType == "STL")
                {
                    if (!param.Saved)
                    {
                        string numberString = string.Format("IMAGE{0:000}", i);
                        string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLower());
                        SaveImage(param.Bitmap, fileName, ImageFormat);

                        imagesSavedCount++;

                        const string paragraphWriteFormat = "{0} , {1} , {2}\r\n";
                        const string timeFormat = "{0:00}:{1:00}:{2:00}:{3:00}";

                        double factor = (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate);
                        string startTime = string.Format(timeFormat, param.P.StartTime.Hours, param.P.StartTime.Minutes, param.P.StartTime.Seconds, (int)Math.Round(param.P.StartTime.Milliseconds / factor));
                        string endTime = string.Format(timeFormat, param.P.EndTime.Hours, param.P.EndTime.Minutes, param.P.EndTime.Seconds, (int)Math.Round(param.P.EndTime.Milliseconds / factor));
                        sb.AppendFormat(paragraphWriteFormat, startTime, endTime, fileName);

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

                        double factor = (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate);
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
                        string template = " <clipitem id=\"" + System.Security.SecurityElement.Escape(fileNameNoPath) + "\">" + Environment.NewLine +

                                          // <pathurl>file://localhost/" + fileNameNoPath.Replace(" ", "%20") + @"</pathurl>

                                          @"            <name>" + System.Security.SecurityElement.Escape(fileNameNoPath) + @"</name>
            <duration>[DURATION]</duration>
            <rate>
              <ntsc>FALSE</ntsc>
              <timebase>25</timebase>
            </rate>
            <in>[IN]</in>
            <out>[OUT]</out>
            <start>[START]</start>
            <end>[END]</end>
            <pixelaspectratio>" + param.VideoResolution + @"</pixelaspectratio>
            <stillframe>TRUE</stillframe>
            <anamorphic>FALSE</anamorphic>
            <alphatype>straight</alphatype>
            <masterclipid>" + System.Security.SecurityElement.Escape(fileNameNoPath) + @"1</masterclipid>" + Environment.NewLine +
                                          "           <file id=\"" + fileNameNoExt + "\">" + @"
              <name>" + System.Security.SecurityElement.Escape(fileNameNoPath) + @"</name>
              <pathurl>" + Utilities.UrlEncode(fileNameNoPath) + @"</pathurl>
              <rate>
                <timebase>25</timebase>
              </rate>
              <duration>[DURATION]</duration>
              <width>" + param.ScreenWidth + @"</width>
              <height>" + param.ScreenHeight + @"</height>
              <media>
                <video>
                  <duration>[DURATION]</duration>
                  <stillframe>TRUE</stillframe>
                  <samplecharacteristics>
                    <width>" + param.ScreenWidth + @"</width>
                    <height>" + param.ScreenHeight + @"</height>
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

                        var outBitmap = param.Bitmap;
                        if (checkBoxFullFrameImage.Visible && checkBoxFullFrameImage.Checked)
                        {
                            var nbmp = new NikseBitmap(param.Bitmap);
                            nbmp.ReplaceTransparentWith(panelFullFrameBackground.BackColor);
                            using (var bmp = nbmp.GetBitmap())
                            {
                                int top = param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin);
                                int left = (param.ScreenWidth - param.Bitmap.Width) / 2;

                                var b = new NikseBitmap(param.ScreenWidth, param.ScreenHeight);
                                {
                                    b.Fill(panelFullFrameBackground.BackColor);
                                    outBitmap = b.GetBitmap();
                                    {
                                        if (param.Alignment == ContentAlignment.BottomLeft || param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.TopLeft)
                                            left = param.LeftRightMargin;
                                        else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
                                            left = param.ScreenWidth - param.Bitmap.Width - param.LeftRightMargin;
                                        if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
                                            top = param.BottomMargin;
                                        if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
                                            top = (param.ScreenHeight - param.Bitmap.Height) / 2;

                                        if (param.OverridePosition.HasValue &&
                                            param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.Bitmap.Width &&
                                            param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.Bitmap.Height)
                                        {
                                            left = param.OverridePosition.Value.X;
                                            top = param.OverridePosition.Value.Y;
                                        }

                                        using (var g = Graphics.FromImage(outBitmap))
                                        {
                                            g.DrawImage(bmp, left, top);
                                            g.Dispose();
                                        }
                                    }
                                }
                            }
                        }


                        if (comboBoxImageFormat.Text == "8-bit png")
                        {
                            foreach (var encoder in ImageCodecInfo.GetImageEncoders())
                            {
                                if (encoder.FormatID == ImageFormat.Png.Guid)
                                {
                                    var parameters = new EncoderParameters();
                                    parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 8);

                                    var nbmp = new NikseBitmap(outBitmap);
                                    var b = nbmp.ConverTo8BitsPerPixel();
                                    b.Save(fileName, encoder, parameters);
                                    b.Dispose();

                                    break;
                                }
                            }
                        }
                        else
                        {
                            SaveImage(outBitmap, fileName, ImageFormat);
                        }
                        imagesSavedCount++;

                        int duration = (int)Math.Round(param.P.Duration.TotalSeconds * param.FramesPerSeconds);
                        int start = (int)Math.Round(param.P.StartTime.TotalSeconds * param.FramesPerSeconds);
                        int end = (int)Math.Round(param.P.EndTime.TotalSeconds * param.FramesPerSeconds);

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
                        string fileName = Path.Combine(Path.GetDirectoryName(saveFileDialog1.FileName), Path.GetFileNameWithoutExtension(saveFileDialog1.FileName).Replace(" ", "_")) + "_" + numberString + ".png";

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

                        const string paragraphWriteFormat = "{0}\t{1}\t{2}\t{4}\t{5}\t{3}\t0\t0";

                        int top = param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin);
                        int left = (param.ScreenWidth - param.Bitmap.Width) / 2;
                        if (param.Alignment == ContentAlignment.BottomLeft || param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.TopLeft)
                            left = param.LeftRightMargin;
                        else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
                            left = param.ScreenWidth - param.Bitmap.Width - param.LeftRightMargin;
                        if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
                            top = param.BottomMargin;
                        if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
                            top = param.ScreenHeight - (param.Bitmap.Height / 2);

                        if (param.OverridePosition.HasValue &&
                            param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.Bitmap.Width &&
                            param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.Bitmap.Height)
                        {
                            left = param.OverridePosition.Value.X;
                            top = param.OverridePosition.Value.Y;
                        }

                        string startTime = ToHHMMSSFF(param.P.StartTime);
                        string endTime = ToHHMMSSFF(param.P.EndTime);
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
                        int startFrame = (int)Math.Round(param.P.StartTime.TotalMilliseconds / (TimeCode.BaseUnit / param.FramesPerSeconds));
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

                        int endFrame = (int)Math.Round(param.P.EndTime.TotalMilliseconds / (TimeCode.BaseUnit / param.FramesPerSeconds));
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
                else if (_exportType == "DCINEMA_INTEROP")
                {
                    if (!param.Saved)
                    {
                        string numberString = string.Format("{0:0000}", i);
                        string fileName = Path.Combine(Path.GetDirectoryName(saveFileDialog1.FileName), numberString + ".png");
                        param.Bitmap.Save(fileName, ImageFormat.Png);
                        imagesSavedCount++;
                        param.Saved = true;

                        string verticalAlignment = "bottom";
                        string horizontalAlignment = "center";
                        string vPos = "9.7";
                        string hPos = "0";

                        switch (param.Alignment)
                        {
                            case ContentAlignment.BottomLeft:
                                verticalAlignment = "bottom";
                                horizontalAlignment = "left";
                                hPos = "10";
                                break;
                            case ContentAlignment.BottomRight:
                                verticalAlignment = "bottom";
                                horizontalAlignment = "right";
                                hPos = "10";
                                break;
                            case ContentAlignment.MiddleCenter:
                                verticalAlignment = "center";
                                vPos = "0";
                                break;
                            case ContentAlignment.MiddleLeft:
                                verticalAlignment = "center";
                                horizontalAlignment = "left";
                                hPos = "10";
                                vPos = "0";
                                break;
                            case ContentAlignment.MiddleRight:
                                verticalAlignment = "center";
                                horizontalAlignment = "right";
                                hPos = "10";
                                vPos = "0";
                                break;
                            case ContentAlignment.TopCenter:
                                verticalAlignment = "top";
                                break;
                            case ContentAlignment.TopLeft:
                                verticalAlignment = "top";
                                horizontalAlignment = "left";
                                hPos = "10";
                                break;
                            case ContentAlignment.TopRight:
                                verticalAlignment = "top";
                                horizontalAlignment = "right";
                                hPos = "10";
                                break;
                        }

                        sb.AppendLine("<Subtitle FadeDownTime=\"" + 0 + "\" FadeUpTime=\"" + 0 + "\" TimeOut=\"" + DCSubtitle.ConvertToTimeString(param.P.EndTime) + "\" TimeIn=\"" + DCSubtitle.ConvertToTimeString(param.P.StartTime) + "\" SpotNumber=\"" + param.P.Number + "\">");
                        if (param.Depth3D == 0)

                            sb.AppendLine("<Image VPosition=\"" + vPos + "\" HPosition=\"" + hPos + "\" VAlign=\"" + verticalAlignment + "\" HAlign=\"" + horizontalAlignment + "\">" + numberString + ".png" + "</Image>");

                        else
                            sb.AppendLine("<Image VPosition=\"" + vPos + "\" HPosition=\"" + hPos + "\" ZPosition=\"" + param.Depth3D + "\" VAlign=\"" + verticalAlignment + "\" HAlign=\"" + horizontalAlignment + "\">" + numberString + ".png" + "</Image>");
                        sb.AppendLine("</Subtitle>");
                    }
                }
                else if (_exportType == "EDL")
                {
                    if (!param.Saved)
                    {
                        // 001  7M6C7986 V     C        14:14:55:21 14:15:16:24 01:00:10:18 01:00:31:21
                        var fileName1 = "IMG" + i.ToString(CultureInfo.InvariantCulture).PadLeft(5, '0');

                        var fullSize = new Bitmap(param.ScreenWidth, param.ScreenHeight);
                        using (var g = Graphics.FromImage(fullSize))
                        {
                            g.DrawImage(param.Bitmap, (param.ScreenWidth - param.Bitmap.Width) / 2, param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin));
                        }
                        var fileName2 = Path.Combine(Path.GetDirectoryName(param.SavDialogFileName), fileName1 + ".PNG");
                        fullSize.Save(fileName2, ImageFormat.Png);
                        fullSize.Dispose();

                        string line = string.Format("{0:000}  {1}  V     C        {2} {3} {4} {5}", i, fileName1, new TimeCode(0).ToHHMMSSFF(), param.P.Duration.ToHHMMSSFF(), param.P.StartTime.ToHHMMSSFF(), param.P.EndTime.ToHHMMSSFF());
                        sb.AppendLine(line);
                        sb.AppendLine();

                        imagesSavedCount++;
                        param.Saved = true;
                    }
                }
                else // BDNXML
                {
                    if (!param.Saved)
                    {
                        string numberString = string.Format("{0:0000}", i);
                        string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + ".png");

                        if (comboBoxImageFormat.Text == "Png 8-bit")
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
                            param.Bitmap.Save(fileName, ImageFormat.Png);
                        }

                        imagesSavedCount++;

                        //<Event InTC="00:00:24:07" OutTC="00:00:31:13" Forced="False">
                        //  <Graphic Width="696" Height="111" X="612" Y="930">subtitle_exp_0001.png</Graphic>
                        //</Event>
                        sb.AppendLine("<Event InTC=\"" + ToHHMMSSFF(param.P.StartTime) + "\" OutTC=\"" +
                                      ToHHMMSSFF(param.P.EndTime) + "\" Forced=\"" + param.Forced.ToString().ToLower() + "\">");

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
                        }

                        if (param.OverridePosition.HasValue &&
                            param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.Bitmap.Width &&
                            param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.Bitmap.Height)
                        {
                            x = param.OverridePosition.Value.X;
                            y = param.OverridePosition.Value.Y;
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

                if (string.Compare(comboBoxImageFormat.Text, "tga", StringComparison.OrdinalIgnoreCase) == 0)
                    return ImageFormat.Icon;

                return imageFormat;
            }
        }

        private static string FormatFabTime(TimeCode time, MakeBitmapParameter param)
        {
            if (param.Bitmap.Width == 720 && param.Bitmap.Width == 480) // NTSC
                return string.Format("{0:00};{1:00};{2:00};{3:00}", time.Hours, time.Minutes, time.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(time.Milliseconds));

            // drop frame
            if (Math.Abs(param.FramesPerSeconds - 24 * (999 / 1000)) < 0.01 ||
                Math.Abs(param.FramesPerSeconds - 29 * (999 / 1000)) < 0.01 ||
                Math.Abs(param.FramesPerSeconds - 59 * (999 / 1000)) < 0.01)
                return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", time.Hours, time.Minutes, time.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(time.Milliseconds));

            return string.Format("{0:00};{1:00};{2:00};{3:00}", time.Hours, time.Minutes, time.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(time.Milliseconds));
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
                                if (comboBoxSubtitleFont.Items[i].ToString().Equals(style.FontName, StringComparison.OrdinalIgnoreCase))
                                    comboBoxSubtitleFont.SelectedIndex = i;
                            }
                            for (i = 0; i < comboBoxSubtitleFontSize.Items.Count; i++)
                            {
                                if (comboBoxSubtitleFontSize.Items[i].ToString().Equals(style.FontSize.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                                    comboBoxSubtitleFontSize.SelectedIndex = i;
                            }
                            checkBoxBold.Checked = style.Bold;
                            for (i = 0; i < comboBoxBorderWidth.Items.Count; i++)
                            {
                                if (Utilities.RemoveNonNumbers(comboBoxBorderWidth.Items[i].ToString()).Equals(style.OutlineWidth.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
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

            _borderWidth = GetBorderWidth();
        }

        private float GetBorderWidth()
        {
            if (comboBoxBorderWidth.SelectedItem.ToString() == Configuration.Settings.Language.ExportPngXml.BorderStyleBoxForEachLine ||
                comboBoxBorderWidth.SelectedItem.ToString() == Configuration.Settings.Language.ExportPngXml.BorderStyleOneBox)
            {
                return 0;
            }
            else
            {
                float f;
                if (float.TryParse(Utilities.RemoveNonNumbers(comboBoxBorderWidth.SelectedItem.ToString()), out f))
                    return f;
                return 0;
            }
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
            mbp.LineHeight = _lineHeights;
            mbp.FullFrame = checkBoxFullFrameImage.Checked;
            mbp.FullFrameBackgroundcolor = panelFullFrameBackground.BackColor;
            mbp.OverridePosition = GetAssPoint(p.Text);

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

            if (comboBoxBorderWidth.SelectedItem.ToString() == Configuration.Settings.Language.ExportPngXml.BorderStyleBoxForEachLine)
            {
                _borderWidth = 0;
                mbp.BackgroundColor = panelBorderColor.BackColor;
                mbp.BoxSingleLine = true;
            }
            else if (comboBoxBorderWidth.SelectedItem.ToString() == Configuration.Settings.Language.ExportPngXml.BorderStyleOneBox)
            {
                mbp.BoxSingleLine = false;
                _borderWidth = 0;
                mbp.BackgroundColor = panelBorderColor.BackColor;
            }

            int width;
            int height;
            GetResolution(out width, out height);
            mbp.ScreenWidth = width;
            mbp.ScreenHeight = height;
            mbp.VideoResolution = comboBoxResolution.Text;
            mbp.Type3D = comboBox3D.SelectedIndex;
            mbp.Depth3D = (int)numericUpDownDepth3D.Value;
            mbp.BottomMargin = comboBoxBottomMargin.SelectedIndex;
            mbp.ShadowWidth = comboBoxShadowWidth.SelectedIndex;
            mbp.ShadowAlpha = (int)numericUpDownShadowTransparency.Value;
            mbp.ShadowColor = panelShadowColor.BackColor;
            mbp.LineHeight = _lineHeights;
            mbp.Forced = subtitleListView1.Items[_subtitle.GetIndex(p)].Checked;
            mbp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
            var bmp = GenerateImageFromTextWithStyle(mbp);
            if (_exportType == "VOBSUB" || _exportType == "STL" || _exportType == "SPUMUX")
            {
                var nbmp = new NikseBitmap(bmp);
                nbmp.ConverToFourColors(Color.Transparent, _subtitleColor, _borderColor, !checkBoxTransAntiAliase.Checked);
                var temp = nbmp.GetBitmap();
                bmp.Dispose();
                return temp;
            }
            return bmp;
        }

        private static int CalcWidthViaDraw(string text, MakeBitmapParameter parameter)
        {
            //text = HtmlUtil.RemoveHtmlTags(text, true).Trim();
            text = text.Trim();
            var path = new GraphicsPath();
            var sb = new StringBuilder();
            int i = 0;
            bool isItalic = false;
            bool isBold = parameter.SubtitleFontBold;
            const float top = 5f;
            bool newLine = false;
            float left = 1.0f;
            float leftMargin = left;
            int newLinePathPoint = -1;
            Color c = parameter.SubtitleColor;
            var colorStack = new Stack<Color>();
            var lastText = new StringBuilder();
            var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
            var bmp = new Bitmap(parameter.ScreenWidth, 200);
            var g = Graphics.FromImage(bmp);

            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            Font font = SetFont(parameter, parameter.SubtitleFontSize);
            Stack<Font> fontStack = new Stack<Font>();
            while (i < text.Length)
            {
                if (text.Substring(i).StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                {
                    float addLeft = 0;
                    int oldPathPointIndex = path.PointCount;
                    if (oldPathPointIndex < 0)
                        oldPathPointIndex = 0;

                    if (sb.Length > 0)
                    {
                        lastText.Append(sb);
                        TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                    }
                    if (path.PointCount > 0)
                    {
                        var list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
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

                    DrawShadowAndPath(parameter, g, path);
                    var p2 = new SolidBrush(c);
                    g.FillPath(p2, path);
                    p2.Dispose();
                    path.Reset();
                    path = new GraphicsPath();
                    sb = new StringBuilder();

                    int endIndex = text.Substring(i).IndexOf('>');
                    if (endIndex < 0)
                    {
                        i += 9999;
                    }
                    else
                    {
                        string fontContent = text.Substring(i, endIndex);
                        if (fontContent.Contains(" color="))
                        {
                            string[] arr = fontContent.Substring(fontContent.IndexOf(" color=", StringComparison.Ordinal) + 7).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (arr.Length > 0)
                            {
                                string fontColor = arr[0].Trim('\'').Trim('"').Trim('\'');
                                try
                                {
                                    colorStack.Push(c); // save old color
                                    if (fontColor.StartsWith("rgb("))
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
                                    c = parameter.SubtitleColor;
                                }
                            }
                        }
                        if (fontContent.Contains(" face=") || fontContent.Contains(" size="))
                        {
                            float fontSize = parameter.SubtitleFontSize;
                            string fontFace = parameter.SubtitleFontName;

                            string[] arr = fontContent.Substring(fontContent.IndexOf(" face=", StringComparison.Ordinal) + 6).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (arr.Length > 0)
                            {
                                fontFace = arr[0].Trim('\'').Trim('"').Trim('\'');
                            }

                            arr = fontContent.Substring(fontContent.IndexOf(" size=", StringComparison.Ordinal) + 6).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (arr.Length > 0)
                            {
                                string temp = arr[0].Trim('\'').Trim('"').Trim('\'');
                                float f;
                                if (float.TryParse(temp, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out f))
                                {
                                    fontSize = f;
                                }
                            }

                            try
                            {
                                fontStack.Push(font); // save old cfont
                                var p = new MakeBitmapParameter() { SubtitleFontName = fontFace, SubtitleFontSize = fontSize };
                                font = SetFont(p, p.SubtitleFontSize);
                            }
                            catch
                            {
                                font = fontStack.Pop();
                            }
                        }
                        i += endIndex;
                    }
                }
                else if (text.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                {
                    if (text.Substring(i).ToLower().Replace("</font>", string.Empty).Length > 0)
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
                            oldPathPointIndex = 0;
                        if (sb.Length > 0)
                        {
                            if (lastText.Length > 0 && left > 2)
                                left -= 1.5f;

                            lastText.Append(sb);

                            TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                        }
                        if (path.PointCount > 0)
                        {
                            var list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
                            for (int k = oldPathPointIndex; k < list.Length; k++)
                            {
                                if (list[k].X > addLeft)
                                    addLeft = list[k].X;
                            }
                        }
                        if (addLeft < 0.01)
                            addLeft = left + 2;
                        left = addLeft;

                        DrawShadowAndPath(parameter, g, path);
                        g.FillPath(new SolidBrush(c), path);
                        path.Reset();
                        sb = new StringBuilder();
                        if (colorStack.Count > 0)
                            c = colorStack.Pop();
                        if (left >= 3)
                            left -= 2.5f;
                    }
                    if (fontStack.Count > 0)
                    {
                        font = fontStack.Pop();
                    }
                    i += 6;
                }
                else if (text.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                {
                    if (sb.Length > 0)
                    {
                        lastText.Append(sb);
                        TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
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
                    lastText.Append(sb);
                    TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                    isItalic = false;
                    i += 3;
                }
                else if (text.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                {
                    if (sb.Length > 0)
                    {
                        lastText.Append(sb);
                        TextDraw.DrawText(font, sf, path, sb, isItalic, isBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                    }
                    isBold = true;
                    i += 2;
                }
                else if (text.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase) && isBold)
                {
                    if (lastText.EndsWith(' ') && !sb.StartsWith(' '))
                    {
                        string t = sb.ToString();
                        sb.Clear();
                        sb.Append(' ');
                        sb.Append(t);
                    }
                    lastText.Append(sb);
                    TextDraw.DrawText(font, sf, path, sb, isItalic, isBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                    isBold = false;
                    i += 3;
                }
                else
                {
                    sb.Append(text[i]);
                }
                i++;
            }
            if (sb.Length > 0)
                TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);

            DrawShadowAndPath(parameter, g, path);
            g.FillPath(new SolidBrush(c), path);
            g.Dispose();

            var nbmp = new NikseBitmap(bmp);
            nbmp.CropTransparentSidesAndBottom(0, true);
            bmp.Dispose();
            font.Dispose();
            sf.Dispose();
            return nbmp.Width;
        }

        internal static Bitmap GenerateImageFromTextWithStyle(MakeBitmapParameter parameter)
        {
            Bitmap bmp = null;
            if (!parameter.SimpleRendering && parameter.P.Text.Contains(Environment.NewLine) && (parameter.BoxSingleLine || parameter.P.Text.Contains(BoxSingleLineText)))
            {
                string old = parameter.P.Text;
                int oldType3D = parameter.Type3D;
                if (parameter.Type3D == 2) // Half-Top/Bottom 3D
                {
                    parameter.Type3D = 0; // fix later
                }
                Color oldBackgroundColor = parameter.BackgroundColor;
                if (parameter.P.Text.Contains(BoxSingleLineText))
                {
                    parameter.P.Text = parameter.P.Text.Replace("<" + BoxSingleLineText + ">", string.Empty).Replace("</" + BoxSingleLineText + ">", string.Empty);
                    parameter.BackgroundColor = parameter.BorderColor;
                }

                bool italicOn = false;
                string fontTag = string.Empty;
                foreach (string line in parameter.P.Text.SplitToLines())
                {
                    parameter.P.Text = line;
                    if (italicOn)
                    {
                        parameter.P.Text = "<i>" + parameter.P.Text;
                    }
                    italicOn = parameter.P.Text.Contains("<i>") && !parameter.P.Text.Contains("</i>");

                    parameter.P.Text = fontTag + parameter.P.Text;
                    if (parameter.P.Text.Contains("<font ") && !parameter.P.Text.Contains("</font>"))
                    {
                        int start = parameter.P.Text.LastIndexOf("<font ", StringComparison.Ordinal);
                        int end = parameter.P.Text.IndexOf('>', start);
                        fontTag = parameter.P.Text.Substring(start, end - start + 1);
                    }

                    var lineImage = GenerateImageFromTextWithStyleInner(parameter);
                    if (bmp == null)
                    {
                        bmp = lineImage;
                    }
                    else
                    {
                        int w = Math.Max(bmp.Width, lineImage.Width);
                        int h = bmp.Height + lineImage.Height;

                        int l1;
                        if (parameter.AlignLeft)
                            l1 = 0;
                        else if (parameter.AlignRight)
                            l1 = w - bmp.Width;
                        else
                            l1 = (int)Math.Round(((w - bmp.Width) / 2.0));

                        int l2;
                        if (parameter.AlignLeft)
                            l2 = 0;
                        else if (parameter.AlignRight)
                            l2 = w - lineImage.Width;
                        else
                            l2 = (int)Math.Round(((w - lineImage.Width) / 2.0));

                        var style = GetStyleName(parameter.P);
                        var lineHeight = 25;
                        if (parameter.LineHeight.ContainsKey(style))
                            lineHeight = parameter.LineHeight[style];
                        else if (parameter.LineHeight.Count > 0)
                            lineHeight = parameter.LineHeight.First().Value;
                        if (lineHeight > lineImage.Height)
                        {
                            h += lineHeight - lineImage.Height;
                            var largeImage = new Bitmap(w, h);
                            var g = Graphics.FromImage(largeImage);
                            g.DrawImageUnscaled(bmp, new Point(l1, 0));
                            g.DrawImageUnscaled(lineImage, new Point(l2, bmp.Height + lineHeight - lineImage.Height));
                            bmp.Dispose();
                            bmp = largeImage;
                            g.Dispose();
                        }
                        else
                        {
                            var largeImage = new Bitmap(w, h);
                            var g = Graphics.FromImage(largeImage);
                            g.DrawImageUnscaled(bmp, new Point(l1, 0));
                            g.DrawImageUnscaled(lineImage, new Point(l2, bmp.Height));
                            bmp.Dispose();
                            bmp = largeImage;
                            g.Dispose();
                        }
                    }
                }
                parameter.P.Text = old;
                parameter.Type3D = oldType3D;
                parameter.BackgroundColor = oldBackgroundColor;

                if (parameter.Type3D == 2) // Half-side-by-side 3D - due to per line we need to do this after making lines
                {
                    var newBmp = Make3DTopBottom(parameter, new NikseBitmap(bmp)).GetBitmap();
                    if (bmp != null)
                    {
                        bmp.Dispose();
                    }
                    bmp = newBmp;
                }
            }
            else
            {
                Color oldBackgroundColor = parameter.BackgroundColor;
                string oldText = parameter.P.Text;
                if (parameter.P.Text.Contains(BoxMultiLineText) || parameter.P.Text.Contains(BoxSingleLineText))
                {
                    parameter.P.Text = parameter.P.Text.Replace("<" + BoxMultiLineText + ">", string.Empty).Replace("</" + BoxMultiLineText + ">", string.Empty);
                    parameter.P.Text = parameter.P.Text.Replace("<" + BoxSingleLineText + ">", string.Empty).Replace("</" + BoxSingleLineText + ">", string.Empty);
                    parameter.BackgroundColor = parameter.BorderColor;
                }
                bmp = GenerateImageFromTextWithStyleInner(parameter);
                parameter.P.Text = oldText;
                parameter.BackgroundColor = oldBackgroundColor;
            }
            return bmp;
        }

        private static Bitmap GenerateImageFromTextWithStyleInner(MakeBitmapParameter parameter)
        {
            string text = parameter.P.Text;

            text = AssToHtmlTagsIfKnow(text);

            text = text.Replace("<I>", "<i>");
            text = text.Replace("</I>", "</i>");
            text = HtmlUtil.FixInvalidItalicTags(text);

            text = text.Replace("<B>", "<b>");
            text = text.Replace("</B>", "</b>");

            // no support for underline
            text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagUnderline);

            Font font = null;
            Bitmap bmp = null;
            try
            {
                font = SetFont(parameter, parameter.SubtitleFontSize);
                SizeF textSize;
                using (var bmpTemp = new Bitmap(1, 1))
                using (var g = Graphics.FromImage(bmpTemp))
                {
                    textSize = g.MeasureString(HtmlUtil.RemoveHtmlTags(text), font);
                }
                int sizeX = (int)(textSize.Width * 1.8) + 150;
                int sizeY = (int)(textSize.Height * 0.9) + 50;
                if (sizeX < 1)
                    sizeX = 1;
                if (sizeY < 1)
                    sizeY = 1;
                if (parameter.BackgroundColor != Color.Transparent)
                {
                    var nbmpTemp = new NikseBitmap(sizeX, sizeY);
                    nbmpTemp.Fill(parameter.BackgroundColor);
                    bmp = nbmpTemp.GetBitmap();
                }
                else
                {
                    bmp = new Bitmap(sizeX, sizeY);
                }

                // align lines with descenders, a bit lower
                var lines = text.SplitToLines();
                int baseLinePadding = 13;
                if (parameter.SubtitleFontSize < 30)
                    baseLinePadding = 12;
                if (parameter.SubtitleFontSize < 25)
                    baseLinePadding = 9;
                if (lines.Length > 0)
                {
        			var lastLine = lines[lines.Length - 1];
                    baseLinePadding -= (int)Math.Floor((TextDraw.MeasureTextHeight(font, lastLine, parameter.SubtitleFontBold) - TextDraw.MeasureTextHeight(font, "a,", parameter.SubtitleFontBold)));
                    if (baseLinePadding < 0)
                        baseLinePadding = 0;
                }

                // TODO: Better baseline - test http://bobpowell.net/formattingtext.aspx
                //float baselineOffset=font.SizeInPoints/font.FontFamily.GetEmHeight(font.Style)*font.FontFamily.GetCellAscent(font.Style);
                //float baselineOffsetPixels = g.DpiY/72f*baselineOffset;
                //baseLinePadding = (int)Math.Round(baselineOffsetPixels);

                var lefts = new List<float>();
                if (text.Contains("<font", StringComparison.OrdinalIgnoreCase) || text.Contains("<i>", StringComparison.OrdinalIgnoreCase) || text.Contains("<b>", StringComparison.OrdinalIgnoreCase))
                {
                    bool tempItalicOn = false;
                    bool tempBoldOn = false;
                    foreach (string line in text.SplitToLines())
                    {
                        var tempLine = HtmlUtil.RemoveOpenCloseTags(line, HtmlUtil.TagFont);

                        if (tempItalicOn)
                            tempLine = "<i>" + tempLine;

                        if (tempBoldOn)
                            tempLine = "<b>" + tempLine;

                        if (tempLine.Contains("<i>") && !tempLine.Contains("</i>"))
                            tempItalicOn = true;

                        if (tempLine.Contains("<b>") && !tempLine.Contains("</b>"))
                            tempBoldOn = true;

                        if (parameter.AlignLeft)
                            lefts.Add(5);
                        else if (parameter.AlignRight)
                            lefts.Add(bmp.Width - CalcWidthViaDraw(tempLine, parameter) - 15); // calculate via drawing+crop
                        else
                            lefts.Add((float)((bmp.Width - CalcWidthViaDraw(tempLine, parameter) + 5.0) / 2.0)); // calculate via drawing+crop

                        if (line.Contains("</i>"))
                            tempItalicOn = false;

                        if (line.Contains("</b>"))
                            tempBoldOn = false;
                    }
                }
                else
                {
                    foreach (var line in HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic, HtmlUtil.TagFont).SplitToLines())
                    {
                        if (parameter.AlignLeft)
                            lefts.Add(5);
                        else if (parameter.AlignRight)
                            lefts.Add(bmp.Width - (TextDraw.MeasureTextWidth(font, line, parameter.SubtitleFontBold) + 15));
                        else
                            lefts.Add((float)((bmp.Width - TextDraw.MeasureTextWidth(font, line, parameter.SubtitleFontBold) + 15) / 2.0));
                    }
                }

                var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

                using (var g = Graphics.FromImage(bmp))
                {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    if (parameter.SimpleRendering)
                    {
                        if (text.StartsWith("<font ", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<font") == 1)
                        {
                            parameter.SubtitleColor = Utilities.GetColorFromFontString(text, parameter.SubtitleColor);
                        }

                        text = HtmlUtil.RemoveHtmlTags(text, true); // TODO: Perhaps check single color...
                        var brush = new SolidBrush(parameter.BorderColor);
                        int x = 3;
                        const int y = 3;
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

                        bmp = new Bitmap(parameter.ScreenWidth, sizeY);

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
                        var fontStack = new Stack<Font>();
                        var lastText = new StringBuilder();
                        int numberOfCharsOnCurrentLine = 0;
                        for (var i = 0; i < text.Length; i++)
                        {
                            if (text.Substring(i).StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                            {
                                float addLeft = 0;
                                int oldPathPointIndex = path.PointCount;
                                if (oldPathPointIndex < 0)
                                    oldPathPointIndex = 0;

                                if (sb.Length > 0)
                                {
                                    lastText.Append(sb);
                                    TextDraw.DrawText(font, sf, path, sb, isItalic, isBold || parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                                }
                                if (path.PointCount > 0)
                                {
                                    var list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
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

                                DrawShadowAndPath(parameter, g, path);
                                var p2 = new SolidBrush(c);
                                g.FillPath(p2, path);
                                p2.Dispose();
                                path.Reset();
                                path = new GraphicsPath();
                                sb = new StringBuilder();

                                int endIndex = text.Substring(i).IndexOf('>');
                                if (endIndex < 0)
                                {
                                    i += 9999;
                                }
                                else
                                {
                                    string fontContent = text.Substring(i, endIndex);
                                    if (fontContent.Contains(" color="))
                                    {
                                        string[] arr = fontContent.Substring(fontContent.IndexOf(" color=", StringComparison.Ordinal) + 7).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (arr.Length > 0)
                                        {
                                            string fontColor = arr[0].Trim('\'').Trim('"').Trim('\'');
                                            try
                                            {
                                                colorStack.Push(c); // save old color
                                                if (fontColor.StartsWith("rgb(", StringComparison.Ordinal))
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
                                                c = parameter.SubtitleColor;
                                            }
                                        }
                                    }
                                    if (fontContent.Contains(" face=") || fontContent.Contains(" size="))
                                    {
                                        float fontSize = parameter.SubtitleFontSize;
                                        string fontFace = parameter.SubtitleFontName;

                                        string[] arr = fontContent.Substring(fontContent.IndexOf(" face=", StringComparison.Ordinal) + 6).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (arr.Length > 0)
                                        {
                                            fontFace = arr[0].Trim('\'').Trim('"').Trim('\'');
                                        }

                                        arr = fontContent.Substring(fontContent.IndexOf(" size=", StringComparison.Ordinal) + 6).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (arr.Length > 0)
                                        {
                                            string temp = arr[0].Trim('\'').Trim('"').Trim('\'');
                                            float f;
                                            if (float.TryParse(temp, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out f))
                                            {
                                                fontSize = f;
                                            }
                                        }

                                        try
                                        {
                                            fontStack.Push(font); // save old cfont
                                            var p = new MakeBitmapParameter() { SubtitleFontName = fontFace, SubtitleFontSize = fontSize };
                                            font = SetFont(p, p.SubtitleFontSize);
                                        }
                                        catch
                                        {
                                            font = fontStack.Pop();
                                        }
                                    }


                                    i += endIndex;
                                }
                            }
                            else if (text.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                            {
                                if (text.Substring(i).ToLower().Replace("</font>", string.Empty).Length > 0)
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
                                        oldPathPointIndex = 0;
                                    if (sb.Length > 0)
                                    {
                                        if (lastText.Length > 0 && left > 2)
                                            left -= 1.5f;

                                        lastText.Append(sb);

                                        TextDraw.DrawText(font, sf, path, sb, isItalic, isBold || parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                                    }
                                    if (path.PointCount > 0)
                                    {
                                        var list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
                                        for (int k = oldPathPointIndex; k < list.Length; k++)
                                        {
                                            if (list[k].X > addLeft)
                                                addLeft = list[k].X;
                                        }
                                    }
                                    if (addLeft < 0.01)
                                        addLeft = left + 2;
                                    left = addLeft;

                                    DrawShadowAndPath(parameter, g, path);
                                    g.FillPath(new SolidBrush(c), path);
                                    path.Reset();
                                    sb = new StringBuilder();
                                    if (colorStack.Count > 0)
                                        c = colorStack.Pop();
                                    if (left >= 3)
                                        left -= 2.5f;
                                }
                                if (fontStack.Count > 0)
                                {
                                    font = fontStack.Pop();
                                }
                                i += 6;
                            }
                            else if (text.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                            {
                                if (sb.Length > 0)
                                {
                                    lastText.Append(sb);
                                    TextDraw.DrawText(font, sf, path, sb, isItalic, isBold || parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
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
                                lastText.Append(sb);
                                TextDraw.DrawText(font, sf, path, sb, isItalic, isBold || parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                                isItalic = false;
                                i += 3;
                            }
                            else if (text.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                            {
                                if (sb.Length > 0)
                                {
                                    lastText.Append(sb);
                                    TextDraw.DrawText(font, sf, path, sb, isItalic, isBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                                }
                                isBold = true;
                                i += 2;
                            }
                            else if (text.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase) && isBold)
                            {
                                if (lastText.EndsWith(' ') && !sb.StartsWith(' '))
                                {
                                    string t = sb.ToString();
                                    sb.Clear();
                                    sb.Append(' ');
                                    sb.Append(t);
                                }
                                lastText.Append(sb);
                                TextDraw.DrawText(font, sf, path, sb, isItalic, isBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                                isBold = false;
                                i += 3;
                            }
                            else if (text.Substring(i).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                            {
                                lastText.Append(sb);
                                TextDraw.DrawText(font, sf, path, sb, isItalic, isBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);

                                var style = GetStyleName(parameter.P);
                                var lineHeight = (int)Math.Round(textSize.Height * 0.64f);
                                if (parameter.LineHeight.ContainsKey(style))
                                    lineHeight = parameter.LineHeight[style];
                                top += lineHeight;
                                newLine = true;
                                i += Environment.NewLine.Length - 1;
                                lineNumber++;
                                if (lineNumber < lefts.Count)
                                {
                                    leftMargin = lefts[lineNumber];
                                    left = leftMargin;
                                }
                                numberOfCharsOnCurrentLine = 0;
                            }
                            else
                            {
                                if (numberOfCharsOnCurrentLine != 0 || text[i] != ' ')
                                {
                                    sb.Append(text[i]);
                                    numberOfCharsOnCurrentLine++;
                                }
                            }
                        }
                        if (sb.Length > 0)
                            TextDraw.DrawText(font, sf, path, sb, isItalic, isBold || parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);

                        DrawShadowAndPath(parameter, g, path);
                        g.FillPath(new SolidBrush(c), path);
                    }
                }
                sf.Dispose();

                var nbmp = new NikseBitmap(bmp);
                if (parameter.BackgroundColor == Color.Transparent)
                {
                    nbmp.CropTransparentSidesAndBottom(baseLinePadding, true);
                    nbmp.CropTransparentSidesAndBottom(2, false);
                }
                else
                {
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
                    var sideBySideBmp = new Bitmap(parameter.ScreenWidth, singleHalfBmp.Height);
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
                    nbmp = Make3DTopBottom(parameter, nbmp);
                }
                return nbmp.GetBitmap();
            }
            finally
            {
                if (font != null)
                {
                    font.Dispose();
                }
                if (bmp != null)
                {
                    bmp.Dispose();
                }
            }
        }

        private static Point? GetAssPoint(string s)
        {
            int k = s.IndexOf("{\\", StringComparison.Ordinal);
            while (k >= 0)
            {
                int l = s.IndexOf('}', k + 1);
                if (l < k)
                    break;
                var assTags = s.Substring(k + 1, l - k - 1).Split('\\');
                foreach (var assTag in assTags)
                {
                    if (assTag.StartsWith("pos(", StringComparison.Ordinal))
                    {
                        var numbers = assTag.Remove(0, 4).TrimEnd(')').Trim().Split(',');
                        if (numbers.Length == 2 && Utilities.IsInteger(numbers[0]) && Utilities.IsInteger(numbers[1]))
                            return new Point(int.Parse(numbers[0]), int.Parse(numbers[1]));
                    }
                }
                k = s.IndexOf("{\\", k + 1, StringComparison.Ordinal);
            }
            return null;
        }

        private static string AssToHtmlTagsIfKnow(string s)
        {
            int k = s.IndexOf("{\\", StringComparison.Ordinal);
            while (k >= 0)
            {
                int l = s.IndexOf('}', k + 1);
                if (l < k)
                    break;
                var assTags = s.Substring(k + 1, l - k - 1).Split('\\');
                var sb = new StringBuilder();
                foreach (var assTag in assTags)
                {
                    if (assTag == "i1")
                    {
                        sb.Append("<i>");
                    }
                    else if (assTag == "i" || assTag == "i0")
                    {
                        sb.Append("</i>");
                    }
                    else if (assTag == "b1" || assTag == "b2" || assTag == "b3" || assTag == "b4")
                    {
                        sb.Append("<b>");
                    }
                    else if (assTag == "b" || assTag == "b0")
                    {
                        sb.Append("</b>");
                    }
                }
                s = s.Remove(k, l - k + 1);
                s = s.Insert(k, sb.ToString());
                k = s.IndexOf("{\\", k, StringComparison.Ordinal);
            }
            return s;
        }

        private static NikseBitmap Make3DTopBottom(MakeBitmapParameter parameter, NikseBitmap nbmp)
        {
            Bitmap singleBmp = nbmp.GetBitmap();
            Bitmap singleHalfBmp = ScaleToHalfHeight(singleBmp);
            singleBmp.Dispose();
            var topBottomBmp = new Bitmap(parameter.ScreenWidth, parameter.ScreenHeight - parameter.BottomMargin);
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
            return nbmp;
        }

        private static void DrawShadowAndPath(MakeBitmapParameter parameter, Graphics g, GraphicsPath path)
        {
            if (parameter.ShadowWidth > 0)
            {
                var shadowPath = (GraphicsPath)path.Clone();
                for (int k = 0; k < parameter.ShadowWidth; k++)
                {
                    var translateMatrix = new Matrix();
                    translateMatrix.Translate(1, 1);
                    shadowPath.Transform(translateMatrix);

                    using (var p1 = new Pen(Color.FromArgb(parameter.ShadowAlpha, parameter.ShadowColor), parameter.BorderWidth))
                    {
                        SetLineJoin(parameter.LineJoin, p1);
                        g.DrawPath(p1, shadowPath);
                    }
                }
            }

            if (parameter.BorderWidth > 0)
            {
                var p1 = new Pen(parameter.BorderColor, parameter.BorderWidth);
                SetLineJoin(parameter.LineJoin, p1);
                g.DrawPath(p1, path);
                p1.Dispose();
            }
        }

        private static void SetLineJoin(string lineJoin, Pen pen)
        {
            if (!string.IsNullOrWhiteSpace(lineJoin))
            {
                if (string.Compare(lineJoin, "Round", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    pen.LineJoin = LineJoin.Round;
                }
                else if (string.Compare(lineJoin, "Bevel", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    pen.LineJoin = LineJoin.Bevel;
                }
                else if (string.Compare(lineJoin, "Miter", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    pen.LineJoin = LineJoin.Miter;
                }
                else if (string.Compare(lineJoin, "MiterClipped", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    pen.LineJoin = LineJoin.MiterClipped;
                }
            }
        }

        private static Bitmap ScaleToHalfWidth(Bitmap bmp)
        {
            int w = bmp.Width / 2;
            var newImage = new Bitmap(w, bmp.Height);
            using (var gr = Graphics.FromImage(newImage))
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
            var newImage = new Bitmap(bmp.Width, h);
            using (var gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, h));
            }
            return newImage;
        }

        internal void Initialize(Subtitle subtitle, SubtitleFormat format, string exportType, string fileName, VideoInfo videoInfo, string videoFileName)
        {
            _exportType = exportType;
            _fileName = fileName;
            _format = format;
            _videoFileName = videoFileName;
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
            else if (exportType == "EDL")
                Text = "EDL";
            else if (exportType == "DCINEMA_INTEROP")
                Text = "DCinema interop/png";
            else
                Text = Configuration.Settings.Language.ExportPngXml.Title;

            if (_exportType == "VOBSUB" && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportVobSubFontName))
                _subtitleFontName = Configuration.Settings.Tools.ExportVobSubFontName;
            else if ((_exportType == "BLURAYSUP" || _exportType == "DOST") && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportBluRayFontName))
                _subtitleFontName = Configuration.Settings.Tools.ExportBluRayFontName;
            else if (_exportType == "FCP" && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportFcpFontName))
                _subtitleFontName = Configuration.Settings.Tools.ExportFcpFontName;
            else if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ExportFontNameOther))
                _subtitleFontName = Configuration.Settings.Tools.ExportFontNameOther;
            if (_exportType == "VOBSUB" && Configuration.Settings.Tools.ExportVobSubFontSize > 0)
                _subtitleFontSize = Configuration.Settings.Tools.ExportVobSubFontSize;
            else if ((_exportType == "BLURAYSUP" || _exportType == "DOST") && Configuration.Settings.Tools.ExportBluRayFontSize > 0)
                _subtitleFontSize = Configuration.Settings.Tools.ExportBluRayFontSize;
            else if (_exportType == "FCP" && Configuration.Settings.Tools.ExportFcpFontSize > 0)
                _subtitleFontSize = Configuration.Settings.Tools.ExportFcpFontSize;
            else if (Configuration.Settings.Tools.ExportLastFontSize > 0)
                _subtitleFontSize = Configuration.Settings.Tools.ExportLastFontSize;

            if (_exportType == "FCP")
            {
                comboBoxImageFormat.Items.Add("8-bit png");
                int i = 0;
                foreach (string item in comboBoxImageFormat.Items)
                {
                    if (item == Configuration.Settings.Tools.ExportFcpImageType)
                    {
                        comboBoxImageFormat.SelectedIndex = i;
                        break;
                    }
                    i++;
                }
            }

            if (_exportType == "VOBSUB")
            {
                comboBoxSubtitleFontSize.SelectedIndex = 7;
                int i = 0;
                foreach (string item in comboBoxSubtitleFontSize.Items)
                {
                    if (item == Convert.ToInt32(_subtitleFontSize).ToString(CultureInfo.InvariantCulture))
                    {
                        comboBoxSubtitleFontSize.SelectedIndex = i;
                        break;
                    }
                    i++;
                }
                checkBoxSimpleRender.Checked = Configuration.Settings.Tools.ExportVobSubSimpleRendering;
                checkBoxTransAntiAliase.Checked = Configuration.Settings.Tools.ExportVobAntiAliasingWithTransparency;
            }
            else if (_exportType == "BLURAYSUP" || _exportType == "DOST" || _exportType == "FCP")
            {
                comboBoxSubtitleFontSize.SelectedIndex = 16;
                int i = 0;
                foreach (string item in comboBoxSubtitleFontSize.Items)
                {
                    if (item == Convert.ToInt32(_subtitleFontSize).ToString(CultureInfo.InvariantCulture))
                    {
                        comboBoxSubtitleFontSize.SelectedIndex = i;
                        break;
                    }
                    i++;
                }
            }
            else
            {
                comboBoxSubtitleFontSize.SelectedIndex = 16;
                int i = 0;
                foreach (string item in comboBoxSubtitleFontSize.Items)
                {
                    if (item == Convert.ToInt32(_subtitleFontSize).ToString(CultureInfo.InvariantCulture))
                    {
                        comboBoxSubtitleFontSize.SelectedIndex = i;
                        break;
                    }
                    i++;
                }
            }

            groupBoxImageSettings.Text = Configuration.Settings.Language.ExportPngXml.ImageSettings;
            labelSubtitleFont.Text = Configuration.Settings.Language.ExportPngXml.FontFamily;
            labelSubtitleFontSize.Text = Configuration.Settings.Language.ExportPngXml.FontSize;
            labelResolution.Text = Configuration.Settings.Language.ExportPngXml.VideoResolution;
            buttonColor.Text = Configuration.Settings.Language.ExportPngXml.FontColor;
            checkBoxBold.Text = Configuration.Settings.Language.General.Bold;
            checkBoxSimpleRender.Text = Configuration.Settings.Language.ExportPngXml.SimpleRendering;
            checkBoxTransAntiAliase.Text = Configuration.Settings.Language.ExportPngXml.AntiAliasingWithTransparency;

            normalToolStripMenuItem.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.Normal;
            italicToolStripMenuItem.Text = Configuration.Settings.Language.General.Italic;
            boxSingleLineToolStripMenuItem.Text = Configuration.Settings.Language.ExportPngXml.BoxSingleLine;
            boxMultiLineToolStripMenuItem.Text = Configuration.Settings.Language.ExportPngXml.BoxMultiLine;

            comboBox3D.Items.Clear();
            comboBox3D.Items.Add(Configuration.Settings.Language.General.None);
            comboBox3D.Items.Add(Configuration.Settings.Language.ExportPngXml.SideBySide3D);
            comboBox3D.Items.Add(Configuration.Settings.Language.ExportPngXml.HalfTopBottom3D);
            comboBox3D.SelectedIndex = 0;

            labelDepth.Text = Configuration.Settings.Language.ExportPngXml.Depth;

            numericUpDownDepth3D.Left = labelDepth.Left + labelDepth.Width + 3;

            label3D.Text = Configuration.Settings.Language.ExportPngXml.Text3D;

            comboBox3D.Left = label3D.Left + label3D.Width + 3;

            buttonBorderColor.Text = Configuration.Settings.Language.ExportPngXml.BorderColor;
            //labelBorderWidth.Text = Configuration.Settings.Language.ExportPngXml.BorderWidth;
            labelBorderWidth.Text = Configuration.Settings.Language.ExportPngXml.BorderStyle;
            labelImageFormat.Text = Configuration.Settings.Language.ExportPngXml.ImageFormat;
            checkBoxFullFrameImage.Text = Configuration.Settings.Language.ExportPngXml.FullFrameImage;

            buttonExport.Text = Configuration.Settings.Language.ExportPngXml.ExportAllLines;
            buttonCancel.Text = Configuration.Settings.Language.General.Ok;
            labelLanguage.Text = Configuration.Settings.Language.ChooseLanguage.Language;
            labelFrameRate.Text = Configuration.Settings.Language.General.FrameRate;
            labelHorizontalAlign.Text = Configuration.Settings.Language.ExportPngXml.Align;
            labelBottomMargin.Text = Configuration.Settings.Language.ExportPngXml.BottomMargin;
            labelLeftRightMargin.Text = Configuration.Settings.Language.ExportPngXml.LeftRightMargin;
            if (Configuration.Settings.Language.ExportPngXml.Left != null &&
                Configuration.Settings.Language.ExportPngXml.Center != null &&
                Configuration.Settings.Language.ExportPngXml.Right != null)
            {
                comboBoxHAlign.Items.Clear();
                comboBoxHAlign.Items.Add(Configuration.Settings.Language.ExportPngXml.Left);
                comboBoxHAlign.Items.Add(Configuration.Settings.Language.ExportPngXml.Center);
                comboBoxHAlign.Items.Add(Configuration.Settings.Language.ExportPngXml.Right);
            }

            buttonShadowColor.Text = Configuration.Settings.Language.ExportPngXml.ShadowColor;
            labelShadowWidth.Text = Configuration.Settings.Language.ExportPngXml.ShadowWidth;
            labelShadowTransparency.Text = Configuration.Settings.Language.ExportPngXml.Transparency;
            labelLineHeight.Text = Configuration.Settings.Language.ExportPngXml.LineHeight;

            linkLabelPreview.Text = Configuration.Settings.Language.General.Preview;
            linkLabelPreview.Left = groupBoxExportImage.Width - linkLabelPreview.Width - 3;

            saveImageAsToolStripMenuItem.Text = Configuration.Settings.Language.ExportPngXml.SaveImageAs;

            SubtitleListView1InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(subtitleListView1);
            SubtitleListView1AutoSizeAllColumns();

            _subtitle = new Subtitle(subtitle);
            _subtitle.Header = subtitle.Header;
            _subtitle.Footer = subtitle.Footer;

            panelColor.BackColor = _subtitleColor;
            panelBorderColor.BackColor = _borderColor;
            InitBorderStyle();
            comboBoxHAlign.SelectedIndex = 1;
            comboBoxResolution.SelectedIndex = 3;

            if (Configuration.Settings.Tools.ExportLastShadowTransparency <= numericUpDownShadowTransparency.Maximum && Configuration.Settings.Tools.ExportLastShadowTransparency > 0)
            {
                numericUpDownShadowTransparency.Value = Configuration.Settings.Tools.ExportLastShadowTransparency;
            }

            if ((_exportType == "BLURAYSUP" || _exportType == "DOST") && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportBluRayVideoResolution))
                SetResolution(Configuration.Settings.Tools.ExportBluRayVideoResolution);

            if (exportType == "VOBSUB")
            {
                comboBoxBorderWidth.SelectedIndex = 6;
                if (_exportType == "VOBSUB" && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportVobSubVideoResolution))
                    SetResolution(Configuration.Settings.Tools.ExportVobSubVideoResolution);
                else
                    comboBoxResolution.SelectedIndex = 8;
                labelLanguage.Visible = true;
                comboBoxLanguage.Visible = true;
                comboBoxLanguage.Items.Clear();
                string languageCode = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle);
                if (languageCode == null)
                    languageCode = Configuration.Settings.Tools.ExportVobSubLanguage;
                int index = -1;
                foreach (var language in DvdSubtitleLanguage.CompliantLanguages)
                {
                    int i = comboBoxLanguage.Items.Add(language);
                    if (language.Code == languageCode || (index < 0 && language.Code == "en"))
                        index = i;
                }
                comboBoxLanguage.SelectedIndex = index;
            }

            bool showImageFormat = exportType == "FAB" || exportType == "IMAGE/FRAME" || exportType == "STL" || exportType == "FCP" || exportType == "BDNXML";
            checkBoxFullFrameImage.Visible = exportType == "FAB" || exportType == "BLURAYSUP" || exportType == "FCP";
            comboBoxImageFormat.Visible = showImageFormat;
            labelImageFormat.Visible = showImageFormat;
            labelFrameRate.Visible = exportType == "BDNXML" || exportType == "BLURAYSUP" || exportType == "DOST" || exportType == "IMAGE/FRAME";
            comboBoxFrameRate.Visible = exportType == "BDNXML" || exportType == "BLURAYSUP" || exportType == "DOST" || exportType == "IMAGE/FRAME" || exportType == "FAB" || exportType == "FCP";
            checkBoxTransAntiAliase.Visible = exportType == "VOBSUB";
            if (exportType == "BDNXML")
            {
                labelFrameRate.Top = labelLanguage.Top;
                comboBoxFrameRate.Top = comboBoxLanguage.Top;
                comboBoxFrameRate.Items.Add("23.976");
                comboBoxFrameRate.Items.Add("24");
                comboBoxFrameRate.Items.Add("25");
                comboBoxFrameRate.Items.Add("29.97");
                comboBoxFrameRate.Items.Add("30");
                comboBoxFrameRate.Items.Add("50");
                comboBoxFrameRate.Items.Add("59.94");
                comboBoxFrameRate.SelectedIndex = 2;

                comboBoxImageFormat.Items.Clear();
                comboBoxImageFormat.Items.Add("Png 32-bit");
                comboBoxImageFormat.Items.Add("Png 8-bit");
                if (comboBoxImageFormat.Items[1].ToString() == Configuration.Settings.Tools.ExportBdnXmlImageType)
                    comboBoxImageFormat.SelectedIndex = 1;
                else
                    comboBoxImageFormat.SelectedIndex = 0;
            }
            else if (exportType == "DOST")
            {
                labelFrameRate.Top = labelLanguage.Top;
                comboBoxFrameRate.Top = comboBoxLanguage.Top;
                comboBoxFrameRate.Items.Add("23.98");
                comboBoxFrameRate.Items.Add("24");
                comboBoxFrameRate.Items.Add("25");
                comboBoxFrameRate.Items.Add("29.97");
                comboBoxFrameRate.Items.Add("30");
                comboBoxFrameRate.Items.Add("59.94");
                comboBoxFrameRate.SelectedIndex = 2;
            }
            else if (exportType == "IMAGE/FRAME")
            {
                labelFrameRate.Top = labelLanguage.Top;
                comboBoxFrameRate.Top = comboBoxLanguage.Top;
                comboBoxFrameRate.Items.Add("23.976");
                comboBoxFrameRate.Items.Add("24");
                comboBoxFrameRate.Items.Add("25");
                comboBoxFrameRate.Items.Add("29.97");
                comboBoxFrameRate.Items.Add("30");
                comboBoxFrameRate.Items.Add("50");
                comboBoxFrameRate.Items.Add("59.94");
                comboBoxFrameRate.Items.Add("60");
                comboBoxFrameRate.SelectedIndex = 2;
            }
            else if (exportType == "BLURAYSUP")
            {
                labelFrameRate.Top = labelLanguage.Top;
                comboBoxFrameRate.Top = comboBoxLanguage.Top;
                comboBoxFrameRate.Items.Add("23.976");
                comboBoxFrameRate.Items.Add("24");
                comboBoxFrameRate.Items.Add("25");
                comboBoxFrameRate.Items.Add("29.97");
                comboBoxFrameRate.Items.Add("50");
                comboBoxFrameRate.Items.Add("59.94");
                comboBoxFrameRate.SelectedIndex = 1;
                comboBoxFrameRate.DropDownStyle = ComboBoxStyle.DropDownList;

                checkBoxFullFrameImage.Top = comboBoxImageFormat.Top + 2;
                panelFullFrameBackground.Top = checkBoxFullFrameImage.Top;
            }
            else if (exportType == "FAB")
            {
                labelFrameRate.Visible = true;
                comboBoxFrameRate.Items.Add("23.976");
                comboBoxFrameRate.Items.Add("24");
                comboBoxFrameRate.Items.Add("25");
                comboBoxFrameRate.Items.Add("29.97");
                comboBoxFrameRate.Items.Add("50");
                comboBoxFrameRate.Items.Add("59.94");
                comboBoxFrameRate.SelectedIndex = 1;
                comboBoxFrameRate.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            else if (exportType == "FCP")
            {
                labelFrameRate.Visible = true;
                comboBoxFrameRate.Items.Add("23.976");
                comboBoxFrameRate.Items.Add("24");
                comboBoxFrameRate.Items.Add("25");
                comboBoxFrameRate.Items.Add("29.97");
                comboBoxFrameRate.Items.Add("50");
                comboBoxFrameRate.Items.Add("59.94");
                comboBoxFrameRate.Items.Add("60");
                comboBoxFrameRate.SelectedIndex = 2;
                comboBoxFrameRate.DropDownStyle = ComboBoxStyle.DropDownList;

                checkBoxFullFrameImage.Top = comboBoxFrameRate.Top + comboBoxFrameRate.Height + 5;
                panelFullFrameBackground.Top = checkBoxFullFrameImage.Top;
            }
            if (comboBoxFrameRate.Items.Count >= 2)
            {
                SetLastFrameRate(Configuration.Settings.Tools.ExportLastFrameRate);
            }
            checkBoxFullFrameImage.Checked = Configuration.Settings.Tools.ExportFullFrame;
            panelShadowColor.BackColor = Configuration.Settings.Tools.ExportShadowColor;

            for (int i = 0; i < 1000; i++)
                comboBoxBottomMargin.Items.Add(i);
            if (Configuration.Settings.Tools.ExportBottomMargin >= 0 && Configuration.Settings.Tools.ExportBottomMargin < comboBoxBottomMargin.Items.Count)
                comboBoxBottomMargin.SelectedIndex = Configuration.Settings.Tools.ExportBottomMargin;

            if (exportType == "BLURAYSUP" || exportType == "IMAGE/FRAME" && Configuration.Settings.Tools.ExportBluRayBottomMargin >= 0 && Configuration.Settings.Tools.ExportBluRayBottomMargin < comboBoxBottomMargin.Items.Count)
                comboBoxBottomMargin.SelectedIndex = Configuration.Settings.Tools.ExportBluRayBottomMargin;

            if (_exportType == "BLURAYSUP" || _exportType == "VOBSUB" || _exportType == "IMAGE/FRAME" || _exportType == "BDNXML" || _exportType == "DOST" || _exportType == "FAB" || _exportType == "EDL")
            {
                comboBoxBottomMargin.Visible = true;
                labelBottomMargin.Visible = true;

                comboBoxLeftRightMargin.Visible = true;
                labelLeftRightMargin.Visible = true;
                comboBoxLeftRightMargin.SelectedIndex = 10;
            }
            else
            {
                comboBoxBottomMargin.Visible = false;
                labelBottomMargin.Visible = false;

                comboBoxLeftRightMargin.Visible = false;
                labelLeftRightMargin.Visible = false;
            }

            checkBoxSkipEmptyFrameAtStart.Visible = exportType == "IMAGE/FRAME";

            foreach (var x in FontFamily.Families)
            {
                if (x.IsStyleAvailable(FontStyle.Regular) || x.IsStyleAvailable(FontStyle.Bold))
                {
                    comboBoxSubtitleFont.Items.Add(x.Name);
                    if (x.Name.Equals(_subtitleFontName, StringComparison.OrdinalIgnoreCase))
                        comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
                }
            }
            if (comboBoxSubtitleFont.SelectedIndex == -1)
                comboBoxSubtitleFont.SelectedIndex = 0; // take first font if default font not found (e.g. linux)

            if (videoInfo != null && videoInfo.Height > 0 && videoInfo.Width > 0)
            {
                comboBoxResolution.Items[comboBoxResolution.Items.Count - 1] = videoInfo.Width + "x" + videoInfo.Height;
                comboBoxResolution.SelectedIndex = comboBoxResolution.Items.Count - 1;
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
            _borderWidth = GetBorderWidth();
            checkBoxBold.Checked = Configuration.Settings.Tools.ExportLastFontBold;

            if (Configuration.Settings.Tools.Export3DType >= 0 && Configuration.Settings.Tools.Export3DType < comboBox3D.Items.Count)
                comboBox3D.SelectedIndex = Configuration.Settings.Tools.Export3DType;
            if (Configuration.Settings.Tools.Export3DDepth >= numericUpDownDepth3D.Minimum && Configuration.Settings.Tools.Export3DDepth <= numericUpDownDepth3D.Maximum)
                numericUpDownDepth3D.Value = Configuration.Settings.Tools.Export3DDepth;

            if (Configuration.Settings.Tools.ExportHorizontalAlignment >= 0 && Configuration.Settings.Tools.ExportHorizontalAlignment < comboBoxHAlign.Items.Count)
                comboBoxHAlign.SelectedIndex = Configuration.Settings.Tools.ExportHorizontalAlignment;

            if (exportType == "DCINEMA_INTEROP")
            {
                comboBox3D.Visible = false;
                numericUpDownDepth3D.Enabled = true;
                labelDepth.Enabled = true;
                labelDepth.Text = Configuration.Settings.Language.DCinemaProperties.ZPosition;
            }

            if (_exportType == "FCP")
            {
                comboBoxResolution.Items.Clear();
                comboBoxResolution.Items.Add("NTSC-601");
                comboBoxResolution.Items.Add("PAL-601");
                comboBoxResolution.Items.Add("square");
                comboBoxResolution.Items.Add("DVCPROHD-720P");
                comboBoxResolution.Items.Add("HD-(960x720)");
                comboBoxResolution.Items.Add("DVCPROHD-1080i60");
                comboBoxResolution.Items.Add("HD-(1280x1080)");
                comboBoxResolution.Items.Add("DVCPROHD-1080i50");
                comboBoxResolution.Items.Add("HD-(1440x1080)");
                comboBoxResolution.SelectedIndex = 3; // 720p
                if ((_exportType == "FCP") && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportFcpVideoResolution))
                    SetResolution(Configuration.Settings.Tools.ExportFcpVideoResolution);

                buttonCustomResolution.Visible = true; // we still allow for custom resolutions

                labelLanguage.Text = "NTSC/PAL";
                comboBoxLanguage.Items.Clear();
                comboBoxLanguage.Items.Add("PAL");
                comboBoxLanguage.Items.Add("NTSC");
                comboBoxLanguage.SelectedIndex = 0;
                comboBoxLanguage.Visible = true;
                labelLanguage.Visible = true;
                if (Configuration.Settings.Tools.ExportFcpPalNtsc == "NTSC")
                {
                    comboBoxLanguage.SelectedIndex = 1;
                }
            }

            comboBoxShadowWidth.SelectedIndex = 0;
            bool shadowVisible = _exportType == "BDNXML" || _exportType == "BLURAYSUP" || _exportType == "DOST" || _exportType == "IMAGE/FRAME" || _exportType == "FCP" || _exportType == "DCINEMA_INTEROP" || _exportType == "EDL";
            labelShadowWidth.Visible = shadowVisible;
            buttonShadowColor.Visible = shadowVisible;
            comboBoxShadowWidth.Visible = shadowVisible;
            if (shadowVisible && Configuration.Settings.Tools.ExportBluRayShadow < comboBoxShadowWidth.Items.Count)
                comboBoxShadowWidth.SelectedIndex = Configuration.Settings.Tools.ExportBluRayShadow;
            panelShadowColor.Visible = shadowVisible;
            labelShadowTransparency.Visible = shadowVisible;
            numericUpDownShadowTransparency.Visible = shadowVisible;

            if (exportType == "BLURAYSUP" || exportType == "VOBSUB" || exportType == "BDNXML")
            {
                subtitleListView1.CheckBoxes = true;
                subtitleListView1.Columns.Insert(0, Configuration.Settings.Language.ExportPngXml.Forced);

                SubtitleListView1Fill(_subtitle);

                if (_vobSubOcr != null)
                {
                    for (int index = 0; index < _subtitle.Paragraphs.Count; index++)
                    {
                        if (_vobSubOcr.GetIsForced(index))
                            subtitleListView1.Items[index].Checked = true;
                    }
                }

                SubtitleListView1SelectIndexAndEnsureVisible(0);
            }
            else
            {
                SubtitleListView1Fill(_subtitle);
                SubtitleListView1SelectIndexAndEnsureVisible(0);
            }
        }

        private void InitBorderStyle()
        {
            comboBoxBorderWidth.Items.Clear();

            comboBoxBorderWidth.Items.Add(Configuration.Settings.Language.ExportPngXml.BorderStyleBoxForEachLine);
            comboBoxBorderWidth.Items.Add(Configuration.Settings.Language.ExportPngXml.BorderStyleOneBox);

            for (int i = 0; i < 16; i++)
            {
                comboBoxBorderWidth.Items.Add(string.Format(Configuration.Settings.Language.ExportPngXml.BorderStyleNormalWidthX, i));
            }
            comboBoxBorderWidth.SelectedIndex = 4;
        }

        private void SetLastFrameRate(double lastFrameRate)
        {
            for (int i = 0; i < comboBoxFrameRate.Items.Count; i++)
            {
                double d;
                if (double.TryParse(comboBoxFrameRate.Items[i].ToString().Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                {
                    if (Math.Abs(lastFrameRate - d) < 0.01)
                    {
                        comboBoxFrameRate.SelectedIndex = i;
                        return;
                    }
                }
            }
        }

        internal void InitializeFromVobSubOcr(Subtitle subtitle, SubtitleFormat format, string exportType, string fileName, VobSubOcr vobSubOcr, string languageString)
        {
            _vobSubOcr = vobSubOcr;
            Initialize(subtitle, format, exportType, fileName, null, _videoFileName);

            //set language
            if (!string.IsNullOrEmpty(languageString))
            {
                if (languageString.Contains('(') && languageString[0] != '(')
                    languageString = languageString.Substring(0, languageString.IndexOf('(') - 1).Trim();
                for (int i = 0; i < comboBoxLanguage.Items.Count; i++)
                {
                    string l = comboBoxLanguage.Items[i].ToString();
                    if (l == languageString)
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
            _previewTimer.Stop();
            UpdateLineSpacing();
            _previewTimer.Start();
        }

        private void GeneratePreview()
        {
            SetupImageParameters();
            if (subtitleListView1.SelectedItems.Count > 0)
            {
                MakeBitmapParameter mbp;
                var bmp = GenerateImageFromTextWithStyle(_subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index], out mbp);
                if (checkBoxFullFrameImage.Visible && checkBoxFullFrameImage.Checked)
                {
                    var nbmp = new NikseBitmap(bmp);
                    nbmp.ReplaceTransparentWith(panelFullFrameBackground.BackColor);
                    bmp.Dispose();
                    bmp = nbmp.GetBitmap();
                }
                else
                {
                    groupBoxExportImage.BackColor = DefaultBackColor;
                }
                pictureBox1.Image = bmp;

                int w = groupBoxExportImage.Width - 4;
                pictureBox1.Width = bmp.Width;
                pictureBox1.Height = bmp.Height;
                pictureBox1.Top = groupBoxExportImage.Height - bmp.Height - int.Parse(comboBoxBottomMargin.Text);
                pictureBox1.Left = (w - bmp.Width) / 2;
                var alignment = GetAlignmentFromParagraph(mbp, _format, _subtitle);

                // fix alignment from UI
                if (comboBoxHAlign.Visible && alignment == ContentAlignment.BottomCenter && _format.GetType() != typeof(AdvancedSubStationAlpha) && _format.GetType() != typeof(SubStationAlpha))
                {
                    if (comboBoxHAlign.SelectedIndex == 0)
                    {
                        alignment = ContentAlignment.BottomLeft;
                    }
                    else if (comboBoxHAlign.SelectedIndex == 2)
                    {
                        alignment = ContentAlignment.BottomRight;
                    }
                }

                if (comboBoxHAlign.Visible)
                {
                    if (comboBoxLeftRightMargin.Visible)
                    {
                        if (alignment == ContentAlignment.BottomLeft || alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.TopLeft)
                            pictureBox1.Left = int.Parse(comboBoxLeftRightMargin.Text);
                        else if (alignment == ContentAlignment.BottomRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.TopRight)
                            pictureBox1.Left = w - bmp.Width - int.Parse(comboBoxLeftRightMargin.Text);
                    }

                    if (alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.MiddleCenter || alignment == ContentAlignment.MiddleRight)
                        pictureBox1.Top = (groupBoxExportImage.Height - 4 - bmp.Height) / 2;
                    else if (comboBoxBottomMargin.Visible && alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.TopRight)
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
                    if (checkBoxFullFrameImage.Visible && checkBoxFullFrameImage.Checked)
                    {
                        groupBoxExportImage.BackColor = panelFullFrameBackground.BackColor;
                    }
                    else
                    {
                        groupBoxExportImage.BackColor = groupBoxImageSettings.BackColor;
                    }
                }
            }
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            bool showAlpha = _exportType == "FAB" || _exportType == "BDNXML";
            using (var colorChooser = new ColorChooser { Color = panelColor.BackColor, ShowAlpha = showAlpha })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelColor.BackColor = colorChooser.Color;
                    subtitleListView1_SelectedIndexChanged(null, null);
                }
            }
        }

        private void panelColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonColor_Click(null, null);
        }

        private void buttonBorderColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelBorderColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelBorderColor.BackColor = colorChooser.Color;
                    subtitleListView1_SelectedIndexChanged(null, null);
                }
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
            UpdateLineSpacing();
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void UpdateLineSpacing()
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

                var style = string.Empty;
                if (subtitleListView1.SelectedIndices.Count > 0)
                    style = GetStyleName(_subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index]);
                if (_lineHeights.ContainsKey(style))
                    numericUpDownLineSpacing.Value = _lineHeights[style];
                else if (lineHeight >= numericUpDownLineSpacing.Minimum && lineHeight <= numericUpDownLineSpacing.Maximum && lineHeight != numericUpDownLineSpacing.Value)
                    numericUpDownLineSpacing.Value = lineHeight;
                else if (lineHeight > numericUpDownLineSpacing.Maximum)
                    numericUpDownLineSpacing.Value = numericUpDownLineSpacing.Maximum;
            }
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
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.G && subtitleListView1.Items.Count > 1)
            {
                using (var goToLine = new GoToLine())
                {
                    goToLine.Initialize(1, subtitleListView1.Items.Count);
                    if (goToLine.ShowDialog(this) == DialogResult.OK)
                    {
                        subtitleListView1.Items[goToLine.LineNumber - 1].Selected = true;
                        subtitleListView1.Items[goToLine.LineNumber - 1].EnsureVisible();
                        subtitleListView1.Items[goToLine.LineNumber - 1].Focused = true;
                    }
                }
            }
        }

        private void ExportPngXml_Shown(object sender, EventArgs e)
        {
            _isLoading = false;
            subtitleListView1_SelectedIndexChanged(null, null);
            _formtName = _format != null ? _format.Name : string.Empty;
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
            using (var cr = new ChooseResolution())
            {
                if (cr.ShowDialog(this) == DialogResult.OK)
                {
                    comboBoxResolution.Items[comboBoxResolution.Items.Count - 1] = cr.VideoWidth + "x" + cr.VideoHeight;
                    comboBoxResolution.SelectedIndex = comboBoxResolution.Items.Count - 1;
                }
            }
        }

        private void ExportPngXml_ResizeEnd(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
            subtitleListView1.Columns[subtitleListView1.Columns.Count - 1].Width = -2;
        }

        private void comboBoxBottomMargin_SelectedIndexChanged(object sender, EventArgs e)
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
            int width;
            int height;
            GetResolution(out width, out height);
            string res = string.Format("{0}x{1}", width, height);

            if (_exportType == "VOBSUB")
            {
                Configuration.Settings.Tools.ExportVobSubFontName = _subtitleFontName;
                Configuration.Settings.Tools.ExportVobSubFontSize = (int)_subtitleFontSize;
                Configuration.Settings.Tools.ExportVobSubVideoResolution = res;
                Configuration.Settings.Tools.ExportVobSubLanguage = comboBoxLanguage.Text;
                Configuration.Settings.Tools.ExportVobSubSimpleRendering = checkBoxSimpleRender.Checked;
                Configuration.Settings.Tools.ExportVobAntiAliasingWithTransparency = checkBoxTransAntiAliase.Checked;
            }
            else if (_exportType == "BLURAYSUP")
            {
                Configuration.Settings.Tools.ExportBluRayFontName = _subtitleFontName;
                Configuration.Settings.Tools.ExportBluRayFontSize = (int)_subtitleFontSize;
                Configuration.Settings.Tools.ExportBluRayVideoResolution = res;
            }
            else if (_exportType == "BDNXML")
            {
                Configuration.Settings.Tools.ExportBdnXmlImageType = comboBoxImageFormat.SelectedItem.ToString();
            }
            else if (_exportType == "FCP")
            {
                Configuration.Settings.Tools.ExportFcpFontName = _subtitleFontName;
                Configuration.Settings.Tools.ExportFcpFontSize = (int)_subtitleFontSize;
                if (comboBoxImageFormat.SelectedItem != null)
                    Configuration.Settings.Tools.ExportFcpImageType = comboBoxImageFormat.SelectedItem.ToString();
                Configuration.Settings.Tools.ExportFcpVideoResolution = res;
                Configuration.Settings.Tools.ExportFcpPalNtsc = comboBoxLanguage.SelectedIndex == 0 ? "PAL" : "NTSC";
            }
            Configuration.Settings.Tools.ExportLastShadowTransparency = (int)numericUpDownShadowTransparency.Value;
            Configuration.Settings.Tools.ExportLastFrameRate = FrameRate;
            Configuration.Settings.Tools.ExportFullFrame = checkBoxFullFrameImage.Checked;
            Configuration.Settings.Tools.ExportShadowColor = panelShadowColor.BackColor;
            Configuration.Settings.Tools.ExportFontColor = _subtitleColor;
            Configuration.Settings.Tools.ExportBorderColor = _borderColor;
            if (_exportType == "BLURAYSUP" || _exportType == "DOST")
                Configuration.Settings.Tools.ExportBluRayBottomMargin = comboBoxBottomMargin.SelectedIndex;
            else
                Configuration.Settings.Tools.ExportBottomMargin = comboBoxBottomMargin.SelectedIndex;

            Configuration.Settings.Tools.ExportHorizontalAlignment = comboBoxHAlign.SelectedIndex;
            Configuration.Settings.Tools.Export3DType = comboBox3D.SelectedIndex;
            Configuration.Settings.Tools.Export3DDepth = (int)numericUpDownDepth3D.Value;

            if (comboBoxShadowWidth.Visible)
                Configuration.Settings.Tools.ExportBluRayShadow = comboBoxShadowWidth.SelectedIndex;

            Configuration.Settings.Tools.ExportFontNameOther = _subtitleFontName;
            Configuration.Settings.Tools.ExportLastFontSize = (int)_subtitleFontSize;
            Configuration.Settings.Tools.ExportLastLineHeight = (int)numericUpDownLineSpacing.Value;
            Configuration.Settings.Tools.ExportLastBorderWidth = comboBoxBorderWidth.SelectedIndex;
            Configuration.Settings.Tools.ExportLastFontBold = checkBoxBold.Checked;
        }

        private void numericUpDownDepth3D_ValueChanged(object sender, EventArgs e)
        {
            if (!timerPreview.Enabled)
                timerPreview.Start();
        }

        private void comboBox3D_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enable = comboBox3D.SelectedIndex > 0;
            labelDepth.Enabled = enable;
            numericUpDownDepth3D.Enabled = enable;
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
                var bmp = pictureBox1.Image as Bitmap;
                if (bmp == null)
                {
                    MessageBox.Show("No image!");
                    return;
                }

                try
                {
                    if (saveFileDialog1.FilterIndex == 0)
                        bmp.Save(saveFileDialog1.FileName, ImageFormat.Png);
                    else if (saveFileDialog1.FilterIndex == 1)
                        bmp.Save(saveFileDialog1.FileName);
                    else if (saveFileDialog1.FilterIndex == 2)
                        bmp.Save(saveFileDialog1.FileName, ImageFormat.Gif);
                    else
                        bmp.Save(saveFileDialog1.FileName, ImageFormat.Tiff);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void buttonShadowColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelShadowColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelShadowColor.BackColor = colorChooser.Color;
                    subtitleListView1_SelectedIndexChanged(null, null);
                    numericUpDownShadowTransparency.Value = colorChooser.Color.A;
                }
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
            var bmp = new Bitmap(100, 100);
            using (var g = Graphics.FromImage(bmp))
            {
                var mbp = new MakeBitmapParameter
                {
                    SubtitleFontName = _subtitleFontName,
                    SubtitleFontSize = float.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString()),
                    SubtitleFontBold = _subtitleFontBold
                };
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
            var value = (int)numericUpDownLineSpacing.Value;
            var style = string.Empty;
            if (subtitleListView1.SelectedIndices.Count > 0)
                style = GetStyleName(_subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index]);
            if (_lineHeights.ContainsKey(style))
                _lineHeights[style] = value;
            else
                _lineHeights.Add(style, value);
            labelLineHeightStyle.Text = style;
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private static string GetStyleName(Paragraph paragraph)
        {
            if ((_formtName == AdvancedSubStationAlpha.NameOfFormat || _formtName == SubStationAlpha.NameOfFormat) && !string.IsNullOrEmpty(paragraph.Extra))
            {
                return paragraph.Extra;
            }

            return string.Empty;
        }

        private void ListViewToggleTag(string tag)
        {
            if (_subtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                var indexes = new List<int>();
                foreach (ListViewItem item in subtitleListView1.SelectedItems)
                    indexes.Add(item.Index);

                subtitleListView1.BeginUpdate();
                foreach (int i in indexes)
                {
                    if (tag == BoxMultiLineText)
                    {
                        _subtitle.Paragraphs[i].Text = _subtitle.Paragraphs[i].Text.Replace("<" + BoxSingleLineText + ">", string.Empty).Replace("</" + BoxSingleLineText + ">", string.Empty);
                    }
                    else if (tag == BoxSingleLineText)
                    {
                        _subtitle.Paragraphs[i].Text = _subtitle.Paragraphs[i].Text.Replace("<" + BoxMultiLineText + ">", string.Empty).Replace("</" + BoxMultiLineText + ">", string.Empty);
                    }

                    if (_subtitle.Paragraphs[i].Text.Contains("<" + tag + ">"))
                    {
                        _subtitle.Paragraphs[i].Text = _subtitle.Paragraphs[i].Text.Replace("<" + tag + ">", string.Empty);
                        _subtitle.Paragraphs[i].Text = _subtitle.Paragraphs[i].Text.Replace("</" + tag + ">", string.Empty);
                    }
                    else
                    {
                        int indexOfEndBracket = _subtitle.Paragraphs[i].Text.IndexOf('}');
                        if (_subtitle.Paragraphs[i].Text.StartsWith("{\\") && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                            _subtitle.Paragraphs[i].Text = string.Format("{2}<{0}>{1}</{0}>", tag, _subtitle.Paragraphs[i].Text.Remove(0, indexOfEndBracket + 1), _subtitle.Paragraphs[i].Text.Substring(0, indexOfEndBracket + 1));
                        else
                            _subtitle.Paragraphs[i].Text = string.Format("<{0}>{1}</{0}>", tag, _subtitle.Paragraphs[i].Text);
                    }
                    SubtitleListView1SetText(i, _subtitle.Paragraphs[i].Text);
                }
                subtitleListView1.EndUpdate();
            }
        }

        private void boxMultiLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewToggleTag(BoxMultiLineText);
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void boxSingleLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewToggleTag(BoxSingleLineText);
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void italicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewToggleTag("i");
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                bool isSsa = _format.Name == SubStationAlpha.NameOfFormat ||
                             _format.Name == AdvancedSubStationAlpha.NameOfFormat;

                foreach (ListViewItem item in subtitleListView1.SelectedItems)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(item.Index);
                    if (p != null)
                    {
                        int indexOfEndBracket = p.Text.IndexOf('}');
                        if (p.Text.StartsWith("{\\") && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                            p.Text = p.Text.Remove(0, indexOfEndBracket + 1);
                        p.Text = HtmlUtil.RemoveHtmlTags(p.Text);
                        p.Text = p.Text.Replace("<" + BoxSingleLineText + ">", string.Empty).Replace("</" + BoxSingleLineText + ">", string.Empty);
                        p.Text = p.Text.Replace("<" + BoxMultiLineText + ">", string.Empty).Replace("</" + BoxMultiLineText + ">", string.Empty);

                        if (isSsa)
                            p.Text = Utilities.RemoveSsaTags(p.Text);
                        SubtitleListView1SetText(item.Index, p.Text);
                    }
                }
            }
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void subtitleListView1_KeyDown(object sender, KeyEventArgs e)
        {
            var italicShortCut = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxItalic);
            if (e.KeyData == italicShortCut)
            {
                ListViewToggleTag("i");
                subtitleListView1_SelectedIndexChanged(null, null);
            }
            else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control) //SelectAll
            {
                subtitleListView1.BeginUpdate();
                foreach (ListViewItem item in subtitleListView1.Items)
                    item.Selected = true;
                subtitleListView1.EndUpdate();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control) //SelectFirstSelectedItemOnly
            {
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    bool skipFirst = true;
                    foreach (ListViewItem item in subtitleListView1.SelectedItems)
                    {
                        if (skipFirst)
                            skipFirst = false;
                        else
                            item.Selected = false;
                    }
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift)) //InverseSelection
            {
                subtitleListView1.BeginUpdate();
                foreach (ListViewItem item in subtitleListView1.Items)
                    item.Selected = !item.Selected;
                subtitleListView1.EndUpdate();
                e.SuppressKeyPress = true;
            }
        }

        public void SubtitleListView1SelectNone()
        {
            foreach (ListViewItem item in subtitleListView1.SelectedItems)
                item.Selected = false;
        }

        private void SubtitleListView1SelectIndexAndEnsureVisible(int index)
        {
            if (index < 0 || index >= subtitleListView1.Items.Count || subtitleListView1.Items.Count == 0)
                return;
            if (subtitleListView1.TopItem == null)
                return;

            int bottomIndex = subtitleListView1.TopItem.Index + ((Height - 25) / 16);
            int itemsBeforeAfterCount = ((bottomIndex - subtitleListView1.TopItem.Index) / 2) - 1;
            if (itemsBeforeAfterCount < 0)
                itemsBeforeAfterCount = 1;

            int beforeIndex = index - itemsBeforeAfterCount;
            if (beforeIndex < 0)
                beforeIndex = 0;

            int afterIndex = index + itemsBeforeAfterCount;
            if (afterIndex >= subtitleListView1.Items.Count)
                afterIndex = subtitleListView1.Items.Count - 1;

            SubtitleListView1SelectNone();
            if (subtitleListView1.TopItem.Index <= beforeIndex && bottomIndex > afterIndex)
            {
                subtitleListView1.Items[index].Selected = true;
                subtitleListView1.Items[index].EnsureVisible();
                return;
            }

            subtitleListView1.Items[beforeIndex].EnsureVisible();
            subtitleListView1.EnsureVisible(beforeIndex);
            subtitleListView1.Items[afterIndex].EnsureVisible();
            subtitleListView1.EnsureVisible(afterIndex);
            subtitleListView1.Items[index].Selected = true;
            subtitleListView1.Items[index].EnsureVisible();
        }

        private void SubtitleListView1Add(Paragraph paragraph)
        {
            var item = new ListViewItem(paragraph.Number.ToString(CultureInfo.InvariantCulture)) { Tag = paragraph, UseItemStyleForSubItems = false };
            ListViewItem.ListViewSubItem subItem;

            if (subtitleListView1.CheckBoxes)
            {
                item.Text = string.Empty;
                subItem = new ListViewItem.ListViewSubItem(item, paragraph.Number.ToString(CultureInfo.InvariantCulture)) { Tag = paragraph };
                item.SubItems.Add(subItem);
            }

            subItem = new ListViewItem.ListViewSubItem(item, paragraph.StartTime.ToDisplayString());
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, paragraph.EndTime.ToDisplayString());
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, paragraph.Duration.ToShortDisplayString());
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, paragraph.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            try
            {
                subItem.Font = new Font(_subtitleFontName, Font.Size);
            }
            catch
            {
                subItem.Font = new Font(_subtitleFontName, Font.Size, FontStyle.Bold);
            }
            item.SubItems.Add(subItem);

            subtitleListView1.Items.Add(item);
        }

        private void SubtitleListView1Fill(Subtitle subtitle)
        {
            subtitleListView1.BeginUpdate();
            subtitleListView1.Items.Clear();
            foreach (Paragraph paragraph in subtitle.Paragraphs)
            {
                SubtitleListView1Add(paragraph);
            }
            subtitleListView1.EndUpdate();
        }

        private void SubtitleListView1AutoSizeAllColumns()
        {
            int columnIndexNumber = 0;
            int columnIndexStart = 1;
            int columnIndexEnd = 2;
            int columnIndexDuration = 3;
            int columnIndexText = 4;

            if (subtitleListView1.CheckBoxes)
            {
                subtitleListView1.Columns[0].Width = 60;
                columnIndexNumber++;
                columnIndexStart++;
                columnIndexEnd++;
                columnIndexDuration++;
                columnIndexText++;
            }

            var setings = Configuration.Settings;
            if (setings != null && setings.General.ListViewColumnsRememberSize && setings.General.ListViewNumberWidth > 1)
                subtitleListView1.Columns[columnIndexNumber].Width = setings.General.ListViewNumberWidth;
            else
                subtitleListView1.Columns[columnIndexNumber].Width = 55;
            subtitleListView1.Columns[columnIndexStart].Width = 90;
            subtitleListView1.Columns[columnIndexEnd].Width = 90;
            subtitleListView1.Columns[columnIndexDuration].Width = 60;
            subtitleListView1.Columns[columnIndexText].Width = -2;
        }

        private void SubtitleListView1InitializeLanguage(LanguageStructure.General general, Core.Settings settings)
        {
            int columnIndexNumber = 0;
            int columnIndexStart = 1;
            int columnIndexEnd = 2;
            int columnIndexDuration = 3;
            int columnIndexText = 4;

            if (subtitleListView1.CheckBoxes)
            {
                columnIndexNumber++;
                columnIndexStart++;
                columnIndexEnd++;
                columnIndexDuration++;
                columnIndexText++;
            }

            subtitleListView1.Columns[columnIndexNumber].Text = general.NumberSymbol;
            subtitleListView1.Columns[columnIndexStart].Text = general.StartTime;
            subtitleListView1.Columns[columnIndexEnd].Text = general.EndTime;
            subtitleListView1.Columns[columnIndexDuration].Text = general.Duration;
            subtitleListView1.Columns[columnIndexText].Text = general.Text;
            subtitleListView1.ForeColor = settings.General.SubtitleFontColor;
            subtitleListView1.BackColor = settings.General.SubtitleBackgroundColor;
        }

        private void SubtitleListView1SetText(int index, string text)
        {
            int columnIndexText = 4;

            if (subtitleListView1.CheckBoxes)
            {
                columnIndexText++;
            }

            subtitleListView1.Items[index].SubItems[columnIndexText].Text = text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString);
        }

        private void FillPreviewBackground(Bitmap bmp, Graphics g, Paragraph p)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_videoFileName) && LibVlcDynamic.IsInstalled)
                {
                    using (var vlc = new LibVlcDynamic())
                    {
                        vlc.Initialize(panelVlcTemp, _videoFileName, null, null);
                        Application.DoEvents();
                        vlc.Volume = 0;
                        vlc.Pause();
                        vlc.CurrentPosition = p.StartTime.TotalSeconds;
                        Application.DoEvents();
                        var fileName = Path.GetTempFileName() + ".bmp";
                        vlc.TakeSnapshot(fileName, (uint)bmp.Width, (uint)bmp.Height);
                        Application.DoEvents();
                        Thread.Sleep(200);
                        using (var tempBmp = new Bitmap(fileName))
                        {
                            g.DrawImageUnscaled(tempBmp, new Point(0, 0));
                        }
                    }
                    return;
                }
            }
            catch
            {
                // Was unable to grap screenshot via vlc
            }

            // Draw background with generated image
            var rect = new Rectangle(0, 0, bmp.Width - 1, bmp.Height - 1);
            using (var br = new LinearGradientBrush(rect, Color.Black, Color.Black, 0, false))
            {
                var cb = new ColorBlend
                {
                    Positions = new[] { 0, 1 / 6f, 2 / 6f, 3 / 6f, 4 / 6f, 5 / 6f, 1 },
                    Colors = new[] { Color.Black, Color.Black, Color.White, Color.Black, Color.Black, Color.White, Color.Black }
                };
                br.InterpolationColors = cb;
                br.RotateTransform(0);
                g.FillRectangle(br, rect);
            }
        }

        private void linkLabelPreview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelPreview.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                int width;
                int height;
                GetResolution(out width, out height);
                using (var bmp = new Bitmap(width, height))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        var p = _subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index];
                        FillPreviewBackground(bmp, g, p);

                        var nBmp = new NikseBitmap(pictureBox1.Image as Bitmap);
                        nBmp.CropSidesAndBottom(100, Color.Transparent, true);
                        using (var textBmp = nBmp.GetBitmap())
                        {
                            var bp = MakeMakeBitmapParameter(subtitleListView1.SelectedItems[0].Index, width, height);
                            var alignment = GetAlignmentFromParagraph(bp, _format, _subtitle);
                            if (comboBoxHAlign.Visible && alignment == ContentAlignment.BottomCenter && _format.GetType() != typeof(AdvancedSubStationAlpha) && _format.GetType() != typeof(SubStationAlpha))
                            {
                                if (comboBoxHAlign.SelectedIndex == 0)
                                {
                                    alignment = ContentAlignment.BottomLeft;
                                }
                                else if (comboBoxHAlign.SelectedIndex == 2)
                                {
                                    alignment = ContentAlignment.BottomRight;
                                }
                            }

                            int x = (bmp.Width - textBmp.Width) / 2;
                            if (alignment == ContentAlignment.BottomLeft || alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.TopLeft)
                                x = int.Parse(comboBoxBottomMargin.Text);
                            else if (alignment == ContentAlignment.BottomRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.TopRight)
                                x = bmp.Width - textBmp.Width - int.Parse(comboBoxBottomMargin.Text);

                            int y = bmp.Height - textBmp.Height - int.Parse(comboBoxBottomMargin.Text);
                            if (alignment == ContentAlignment.BottomLeft || alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.TopLeft)
                                x = int.Parse(comboBoxBottomMargin.Text);
                            else if (alignment == ContentAlignment.BottomRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.TopRight)
                                x = bmp.Width - textBmp.Width - int.Parse(comboBoxBottomMargin.Text);
                            if (alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.MiddleCenter || alignment == ContentAlignment.MiddleRight)
                                y = (groupBoxExportImage.Height - 4 - textBmp.Height) / 2;
                            else if (alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.TopRight)
                                y = int.Parse(comboBoxBottomMargin.Text);

                            g.DrawImageUnscaled(textBmp, new Point(x, y));
                        }
                    }

                    using (var form = new ExportPngXmlPreview(bmp))
                    {
                        Cursor = Cursors.Default;
                        form.ShowDialog(this);
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
                linkLabelPreview.Enabled = true;
            }
        }

        private void comboBoxLeftRightMargin_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void panelFullFrameBackground_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelFullFrameBackground.BackColor, Text = Configuration.Settings.Language.ExportPngXml.ChooseBackgroundColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelFullFrameBackground.BackColor = colorChooser.Color;
                    subtitleListView1_SelectedIndexChanged(null, null);
                }
            }
        }

        private void checkBoxFullFrameImage_CheckedChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
            panelFullFrameBackground.Visible = checkBoxFullFrameImage.Checked;
            comboBoxBottomMargin.Visible = checkBoxFullFrameImage.Checked;
            labelBottomMargin.Visible = checkBoxFullFrameImage.Checked;
        }

        public void DisableSaveButtonAndCheckBoxes()
        {
            buttonExport.Visible = false;
            subtitleListView1.CheckBoxes = false;
        }

    }
}

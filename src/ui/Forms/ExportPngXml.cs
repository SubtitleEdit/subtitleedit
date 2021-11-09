using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Interfaces;
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
        internal class ExportFormats
        {
            internal const string BluraySup = "BLURAYSUP";
            internal const string VobSub = "VOBSUB";
            internal const string Fab = "FAB";
            internal const string Stl = "STL";
            internal const string Fcp = "FCP";
            internal const string Dost = "DOSTIMAGE";
            internal const string DCinemaInterop = "DCINEMA_INTEROP";
            internal const string DCinemaSmpte2014 = "DCINEMA_SMPTE_2014";
            internal const string BdnXml = "BDNXML";
            internal const string Edl = "EDL";
            internal const string EdlClipName = "EDL_CLIPNAME";
            internal const string ImageFrame = "IMAGE/FRAME";
            internal const string Spumux = "SPUMUX";
        }

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
            public bool JustifyLeft { get; set; }
            public bool JustifyTop { get; set; }
            public bool JustifyRight { get; set; }
            public byte[] Buffer { get; set; }
            public int ScreenWidth { get; set; }
            public int ScreenHeight { get; set; }
            public string VideoResolution { get; set; }
            public int Type3D { get; set; }
            public int Depth3D { get; set; }
            public double FramesPerSeconds { get; set; }
            public int BottomMargin { get; set; }
            public int LeftMargin { get; set; }
            public int RightMargin { get; set; }
            public bool Saved { get; set; }
            public ContentAlignment Alignment { get; set; }
            public Point? OverridePosition { get; set; }
            public Color BackgroundColor { get; set; }
            public string SavDialogFileName { get; set; }
            public string Error { get; set; }
            public string LineJoin { get; set; }
            public Color ShadowColor { get; set; }
            public float ShadowWidth { get; set; }
            public int ShadowAlpha { get; set; }
            public Dictionary<string, int> LineHeight { get; set; }
            public bool Forced { get; set; }
            public bool FullFrame { get; set; }
            public Color FullFrameBackgroundColor { get; set; }

            public MakeBitmapParameter()
            {
                BackgroundColor = Color.Transparent;
            }
        }

        private Subtitle _subtitle;
        private SubtitleFormat _format;
        private static string _formatName;
        private Color _subtitleColor;
        private string _subtitleFontName = "Verdana";
        private float _subtitleFontSize = 25.0f;
        private bool _subtitleFontBold;
        private Color _borderColor;
        private float _borderWidth = 2.0f;
        private bool _isLoading = true;
        private string _language = "en";
        private string _exportType = ExportFormats.BdnXml;
        private string _fileName;
        private string _outputFileName;
        private IBinaryParagraphList _vobSubOcr;
        private readonly System.Windows.Forms.Timer _previewTimer = new System.Windows.Forms.Timer();
        private string _videoFileName;
        private readonly Dictionary<string, int> _lineHeights;
        private static int _boxBorderSize = 8;

        private const string BoxMultiLineText = "BoxMultiLine";
        private const string BoxSingleLineText = "BoxSingleLine";

        public string GetOutputFileName()
        {
            return _outputFileName;
        }

        public ExportPngXml()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var toolTip = new ToolTip { ShowAlways = true };
            toolTip.SetToolTip(panelFullFrameBackground, LanguageSettings.Current.ExportPngXml.ChooseBackgroundColor);
            _lineHeights = new Dictionary<string, int>();
            comboBoxImageFormat.SelectedIndex = 4;
            _subtitleColor = Color.FromArgb(byte.MaxValue, Configuration.Settings.Tools.ExportFontColor);
            _borderColor = Configuration.Settings.Tools.ExportBorderColor;
            _boxBorderSize = Configuration.Settings.Tools.ExportBoxBorderSize;
            _previewTimer.Tick += previewTimer_Tick;
            _previewTimer.Interval = 100;
            labelLineHeightStyle.Text = string.Empty;
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
                {
                    return 25;
                }

                string s = comboBoxFrameRate.SelectedItem.ToString();
                s = s.Replace(",", ".").Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".").Trim();
                if (double.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d))
                {
                    return d;
                }

                return 25;
            }
        }

        private int MillisecondsToFramesMaxFrameRate(double milliseconds)
        {
            int frames = (int)Math.Round(milliseconds / (TimeCode.BaseUnit / FrameRate));
            if (frames >= FrameRate)
            {
                frames = (int)(FrameRate - 0.01);
            }

            return frames;
        }

        private string ToHHMMSSFF(TimeCode timeCode)
        {
            return $"{timeCode.Hours:00}:{timeCode.Minutes:00}:{timeCode.Seconds:00}:{MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds):00}";
        }

        private static ContentAlignment GetAlignmentFromParagraph(MakeBitmapParameter p, SubtitleFormat format, Subtitle subtitle)
        {
            var alignment = ContentAlignment.BottomCenter;
            if (p.AlignLeft)
            {
                alignment = ContentAlignment.BottomLeft;
            }
            else if (p.AlignRight)
            {
                alignment = ContentAlignment.BottomRight;
            }

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
            if (parameter.Type == ExportFormats.BluraySup)
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
                nbmp.ReplaceTransparentWith(param.FullFrameBackgroundColor);
                using (var bmp = nbmp.GetBitmap())
                {
                    int top = param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin);
                    int left = (param.ScreenWidth - param.Bitmap.Width) / 2;

                    var b = new NikseBitmap(param.ScreenWidth, param.ScreenHeight);
                    {
                        b.Fill(param.FullFrameBackgroundColor);
                        using (var fullSize = b.GetBitmap())
                        {
                            if (param.Alignment == ContentAlignment.BottomLeft || param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.TopLeft)
                            {
                                left = param.LeftMargin;
                            }
                            else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
                            {
                                left = param.ScreenWidth - param.Bitmap.Width - param.RightMargin;
                            }

                            if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
                            {
                                top = param.BottomMargin;
                            }

                            if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
                            {
                                top = param.ScreenHeight - (param.Bitmap.Height / 2);
                            }

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
                if (param.OverridePosition != null &&
                    param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.ScreenWidth &&
                    param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.ScreenHeight)
                {
                    param.LeftMargin = param.OverridePosition.Value.X;
                    param.BottomMargin = param.ScreenHeight - param.OverridePosition.Value.Y - param.Bitmap.Height;
                }

                var margin = (param.Alignment == ContentAlignment.TopRight ||
                              param.Alignment == ContentAlignment.MiddleRight ||
                              param.Alignment == ContentAlignment.BottomRight)
                    ? param.RightMargin
                    : param.LeftMargin;

                param.Buffer = BluRaySupPicture.CreateSupFrame(brSub, param.Bitmap, param.FramesPerSeconds, param.BottomMargin, margin, param.Alignment, param.OverridePosition);
            }
        }

        internal MakeBitmapParameter MakeMakeBitmapParameter(int index, int screenWidth, int screenHeight)
        {
            var p = _subtitle.GetParagraphOrDefault(index);
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
                JustifyLeft = GetJustifyLeft(p.Text), // center, left justify
                JustifyTop = comboBoxHAlign.SelectedIndex == 4, // center, top justify
                JustifyRight = comboBoxHAlign.SelectedIndex == 5, // center, right justify
                ScreenWidth = screenWidth,
                ScreenHeight = screenHeight,
                VideoResolution = comboBoxResolution.Text,
                Bitmap = null,
                FramesPerSeconds = FrameRate,
                BottomMargin = GetBottomMarginInPixels(p),
                LeftMargin = GetLeftMarginInPixels(p),
                RightMargin = GetRightMarginInPixels(p),
                Saved = false,
                Alignment = ContentAlignment.BottomCenter,
                Type3D = comboBox3D.SelectedIndex,
                Depth3D = (int)numericUpDownDepth3D.Value,
                BackgroundColor = Color.Transparent,
                SavDialogFileName = saveFileDialog1.FileName,
                ShadowColor = panelShadowColor.BackColor,
                ShadowWidth = GetShadowWidth(),
                ShadowAlpha = (int)numericUpDownShadowTransparency.Value,
                LineHeight = _lineHeights,
                FullFrame = checkBoxFullFrameImage.Checked,
                FullFrameBackgroundColor = panelFullFrameBackground.BackColor,
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
                        parameter.BottomMargin = style.MarginVertical;
                        if (style.BorderStyle == "3")
                        {
                            parameter.BackgroundColor = style.Background;
                        }
                        parameter.ShadowColor = style.Outline;
                    }
                    else if (_format.GetType() == typeof(AdvancedSubStationAlpha))
                    {
                        var style = AdvancedSubStationAlpha.GetSsaStyle(parameter.P.Extra, _subtitle.Header);
                        parameter.SubtitleColor = style.Primary;
                        parameter.SubtitleFontBold = style.Bold;
                        parameter.SubtitleFontSize = style.FontSize;
                        parameter.SubtitleFontName = style.FontName;
                        parameter.BottomMargin = style.MarginVertical;
                        if (style.BorderStyle == "3")
                        {
                            parameter.BackgroundColor = style.Outline;
                        }
                        parameter.ShadowAlpha = style.Background.A;
                        parameter.ShadowColor = style.Background;
                    }
                }

                if (comboBoxBorderWidth.SelectedItem.ToString() == LanguageSettings.Current.ExportPngXml.BorderStyleBoxForEachLine)
                {
                    parameter.BoxSingleLine = true;
                    parameter.BackgroundColor = panelBorderColor.BackColor;
                    parameter.BorderWidth = 0;
                }
                else if (comboBoxBorderWidth.SelectedItem.ToString() == LanguageSettings.Current.ExportPngXml.BorderStyleOneBox)
                {
                    parameter.BoxSingleLine = false;
                    parameter.BackgroundColor = panelBorderColor.BackColor;
                    parameter.BorderWidth = 0;
                }
                else
                {
                    _borderWidth = GetBorderWidth();
                }
            }
            else
            {
                parameter.P = null;
            }
            return parameter;
        }

        private bool GetJustifyLeft(string text)
        {
            if (comboBoxHAlign.SelectedIndex == 6 && !string.IsNullOrEmpty(text))
            {
                var s = Utilities.RemoveUnneededSpaces(text, _language);
                var dialogHelper = new DialogSplitMerge { TwoLetterLanguageCode = _language };
                var lines = s.SplitToLines();
                return dialogHelper.IsDialog(lines) || HasTwoSpeakers(lines);
            }

            return comboBoxHAlign.SelectedIndex == 3;
        }

        private static bool HasTwoSpeakers(List<string> lines)
        {
            return lines.Count == 2 && lines[0].HasSentenceEnding() && lines[0].Contains(':') && lines[1].Contains(':');
        }

        private void ButtonExportClick(object sender, EventArgs e)
        {
            FixStartEndWithSameTimeCode();
            var singleFile = false;

            if (Configuration.Settings.General.CurrentVideoOffsetInMs != 0)
            {
                _subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(Configuration.Settings.General.CurrentVideoOffsetInMs));
            }

            var errors = new List<string>();
            buttonExport.Enabled = false;
            SetupImageParameters();

            if (!string.IsNullOrEmpty(_fileName))
            {
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);
            }

            if (_exportType == ExportFormats.BluraySup)
            {
                saveFileDialog1.Title = LanguageSettings.Current.ExportPngXml.SaveBluRaySupAs;
                saveFileDialog1.DefaultExt = "*.sup";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Blu-Ray sup|*.sup";
                singleFile = true;
            }
            else if (_exportType == ExportFormats.VobSub)
            {
                saveFileDialog1.Title = LanguageSettings.Current.ExportPngXml.SaveVobSubAs;
                saveFileDialog1.DefaultExt = "*.sub";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "VobSub|*.sub";
                singleFile = true;
            }
            else if (_exportType == ExportFormats.Fab)
            {
                saveFileDialog1.Title = LanguageSettings.Current.ExportPngXml.SaveFabImageScriptAs;
                saveFileDialog1.DefaultExt = "*.txt";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "FAB image scripts|*.txt";
                singleFile = false;
            }
            else if (_exportType == ExportFormats.Stl)
            {
                saveFileDialog1.Title = LanguageSettings.Current.ExportPngXml.SaveDvdStudioProStlAs;
                saveFileDialog1.DefaultExt = "*.txt";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "DVD Studio Pro STL|*.stl";
                singleFile = true;
            }
            else if (_exportType == ExportFormats.Fcp)
            {
                saveFileDialog1.Title = LanguageSettings.Current.ExportPngXml.SaveFcpAs;
                saveFileDialog1.DefaultExt = "*.xml";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Xml files|*.xml";
                singleFile = true;
            }
            else if (_exportType == ExportFormats.Dost)
            {
                saveFileDialog1.Title = LanguageSettings.Current.ExportPngXml.SaveDostAs;
                saveFileDialog1.DefaultExt = "*.dost";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Dost files|*.dost";
                singleFile = true;
            }
            else if (_exportType == ExportFormats.DCinemaInterop)
            {
                saveFileDialog1.Title = LanguageSettings.Current.ExportPngXml.SaveDigitalCinemaInteropAs;
                saveFileDialog1.DefaultExt = "*.xml";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Xml files|*.xml";
                singleFile = true;
            }
            else if (_exportType == ExportFormats.DCinemaSmpte2014)
            {
                saveFileDialog1.Title = LanguageSettings.Current.ExportPngXml.SaveDigitalCinemaSmpte2014;
                saveFileDialog1.DefaultExt = "*.xml";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "Xml files|*.xml";
                singleFile = true;
            }
            else if (_exportType == ExportFormats.Edl || _exportType == ExportFormats.EdlClipName)
            {
                saveFileDialog1.Title = LanguageSettings.Current.ExportPngXml.SavePremiereEdlAs;
                saveFileDialog1.DefaultExt = "*.edl";
                saveFileDialog1.AddExtension = true;
                saveFileDialog1.Filter = "EDL files|*.edl";
                singleFile = true;
            }

            if (singleFile && saveFileDialog1.ShowDialog(this) == DialogResult.OK || !singleFile && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                GetResolution(out var width, out var height);

                _outputFileName = singleFile ? saveFileDialog1.FileName : folderBrowserDialog1.SelectedPath;

                FileStream binarySubtitleFile = null;
                VobSubWriter vobSubWriter = null;
                Paragraph p = null;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    p = _subtitle.GetParagraphOrDefault(subtitleListView1.SelectedItems[0].Index);
                }

                if (_exportType == ExportFormats.BluraySup)
                {
                    binarySubtitleFile = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                }
                else if (_exportType == ExportFormats.VobSub)
                {
                    vobSubWriter = new VobSubWriter(saveFileDialog1.FileName, width, height, GetBottomMarginInPixels(p), GetLeftMarginInPixels(p), 32, _subtitleColor, _borderColor, !checkBoxTransAntiAliase.Checked, (DvdSubtitleLanguage)comboBoxLanguage.SelectedItem);
                }

                progressBar1.Value = 0;
                progressBar1.Maximum = _subtitle.Paragraphs.Count - 1;
                progressBar1.Visible = true;
                _previewTimer.Tick -= previewTimer_Tick;

                int border = GetBottomMarginInPixels(p);
                int imagesSavedCount = 0;
                var sb = new StringBuilder();
                if (_exportType == ExportFormats.Stl)
                {
                    sb.AppendLine("$SetFilePathToken =" + Path.GetDirectoryName(saveFileDialog1.FileName));
                    sb.AppendLine();
                }

                if (_vobSubOcr != null)
                {
                    int i = 0;
                    while (i < _subtitle.Paragraphs.Count)
                    {
                        var mp = MakeMakeBitmapParameter(i, width, height);
                        mp.Bitmap = _vobSubOcr.GetSubtitleBitmap(i++);
                        var exp = GetResizeScale();
                        if (Math.Abs(exp - 1) > 0.01)
                        {
                            var resizedBitmap = ResizeBitmap(mp.Bitmap, (int)Math.Round(mp.Bitmap.Width * exp), (int)Math.Round(mp.Bitmap.Height * exp));
                            mp.Bitmap.Dispose();
                            mp.Bitmap = resizedBitmap;
                        }
                        if (_exportType == ExportFormats.BluraySup)
                        {
                            MakeBluRaySupImage(mp);
                        }
                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, mp, i);
                        progressBar1.Refresh();
                        Application.DoEvents();
                        if (i < progressBar1.Maximum)
                        {
                            progressBar1.Value = i;
                        }
                    }
                }
                else
                {
                    var threadEqual = new Thread(DoWork);
                    var paramEqual = MakeMakeBitmapParameter(0, width, height);

                    var threadUnEqual = new Thread(DoWork);
                    var paramUnEqual = new MakeBitmapParameter();
                    if (_subtitle.Paragraphs.Count > 1)
                    {
                        MakeMakeBitmapParameter(1, width, height);
                    }

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
                            {
                                threadUnEqual.Join();
                            }

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
                            {
                                threadEqual.Join();
                            }

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
                        {
                            threadEqual.Join();
                        }

                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramEqual, i);
                        if (threadUnEqual.ThreadState == ThreadState.Running)
                        {
                            threadUnEqual.Join();
                        }

                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramUnEqual, i);
                    }
                    else
                    {
                        if (threadUnEqual.ThreadState == ThreadState.Running)
                        {
                            threadUnEqual.Join();
                        }

                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramUnEqual, i);
                        if (threadEqual.ThreadState == ThreadState.Running)
                        {
                            threadEqual.Join();
                        }

                        imagesSavedCount = WriteParagraph(width, sb, border, height, imagesSavedCount, vobSubWriter, binarySubtitleFile, paramEqual, i);
                    }
                }

                if (errors.Count > 0)
                {
                    var errorSb = new StringBuilder();
                    var maxErrorsToDisplay = Math.Min(20, errors.Count);
                    for (var i = 0; i < maxErrorsToDisplay; i++)
                    {
                        errorSb.AppendLine(errors[i]);
                    }
                    if (errors.Count > 20)
                    {
                        errorSb.AppendLine("...");
                    }

                    MessageBox.Show(string.Format(LanguageSettings.Current.ExportPngXml.SomeLinesWereTooLongX, errorSb));
                }

                _previewTimer.Tick += previewTimer_Tick;
                progressBar1.Visible = false;
                if (_exportType == ExportFormats.BluraySup)
                {
                    binarySubtitleFile.Close();
                    MessageBox.Show(string.Format(LanguageSettings.Current.Main.SavedSubtitleX, saveFileDialog1.FileName));
                }
                else if (_exportType == ExportFormats.VobSub)
                {
                    vobSubWriter.WriteIdxFile();
                    vobSubWriter.Dispose();
                    MessageBox.Show(string.Format(LanguageSettings.Current.Main.SavedSubtitleX, saveFileDialog1.FileName));
                }
                else if (_exportType == ExportFormats.Fab)
                {
                    File.WriteAllText(Path.Combine(folderBrowserDialog1.SelectedPath, "Fab_Image_script.txt"), sb.ToString());
                    var text = string.Format(LanguageSettings.Current.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath);
                    MessageBoxShowWithFolderName(text, folderBrowserDialog1.SelectedPath);
                }
                else if (_exportType == ExportFormats.ImageFrame)
                {
                    var empty = new Bitmap(width, height);
                    imagesSavedCount++;
                    string numberString = $"{imagesSavedCount:00000}";
                    string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLowerInvariant());
                    SaveImage(empty, fileName, ImageFormat);
                    var text = string.Format(LanguageSettings.Current.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath);
                    MessageBoxShowWithFolderName(text, folderBrowserDialog1.SelectedPath);
                }
                else if (_exportType == ExportFormats.Stl)
                {
                    File.WriteAllText(saveFileDialog1.FileName, sb.ToString());
                    var text = string.Format(LanguageSettings.Current.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath);
                    MessageBoxShowWithFolderName(text, Path.GetDirectoryName(saveFileDialog1.FileName));
                }
                else if (_exportType == ExportFormats.Spumux)
                {
                    string s = "<subpictures>" + Environment.NewLine +
                               "\t<stream>" + Environment.NewLine +
                               sb +
                               "\t</stream>" + Environment.NewLine +
                               "</subpictures>";
                    File.WriteAllText(Path.Combine(folderBrowserDialog1.SelectedPath, "spu.xml"), s);
                    var text = string.Format(LanguageSettings.Current.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath);
                    MessageBoxShowWithFolderName(text, folderBrowserDialog1.SelectedPath);
                }
                else if (_exportType == ExportFormats.Fcp)
                {
                    WriteFcpFile(width, height, sb, saveFileDialog1.FileName);
                    var text = string.Format(LanguageSettings.Current.ExportPngXml.XImagesSavedInY, imagesSavedCount, Path.GetDirectoryName(saveFileDialog1.FileName));
                    MessageBoxShowWithFolderName(text, Path.GetDirectoryName(saveFileDialog1.FileName));
                }
                else if (_exportType == ExportFormats.Dost)
                {
                    WriteDostFile(saveFileDialog1.FileName, sb.ToString());
                    var text = string.Format(LanguageSettings.Current.ExportPngXml.XImagesSavedInY, imagesSavedCount, Path.GetDirectoryName(saveFileDialog1.FileName));
                    MessageBoxShowWithFolderName(text, Path.GetDirectoryName(saveFileDialog1.FileName));
                }
                else if (_exportType == ExportFormats.DCinemaInterop)
                {
                    var doc = new XmlDocument();
                    string title = "unknown";
                    if (!string.IsNullOrEmpty(_fileName))
                    {
                        title = Path.GetFileNameWithoutExtension(_fileName);
                    }

                    string guid = Guid.NewGuid().ToString().RemoveChar('-').Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
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
                    {
                        fName += ".xml";
                    }

                    File.WriteAllText(fName, SubtitleFormat.ToUtf8XmlString(doc));
                    var text = string.Format(LanguageSettings.Current.ExportPngXml.XImagesSavedInY, imagesSavedCount, Path.GetDirectoryName(fName));
                    MessageBoxShowWithFolderName(text, Path.GetDirectoryName(fName));
                }
                else if (_exportType == ExportFormats.DCinemaSmpte2014)
                {
                    var doc = new XmlDocument();
                    string title = "unknown";
                    if (!string.IsNullOrEmpty(_fileName))
                    {
                        title = Path.GetFileNameWithoutExtension(_fileName);
                    }

                    string guid = Guid.NewGuid().ToString().RemoveChar('-').Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
                    string xml =
                        "<dcst:SubtitleReel xmlns:dcst=\"http://www.smpte-ra.org/schemas/428-7/2014/DCST\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" + Environment.NewLine +
                        "  <dcst:Id>urn:uuid:" + guid + "</dcst:Id>" + Environment.NewLine +
                        "  <dcst:ContentTitleText></dcst:ContentTitleText> " + Environment.NewLine +
                        "  <dcst:AnnotationText>This is a subtitle file</dcst:AnnotationText>" + Environment.NewLine +
                        "  <dcst:IssueDate>2014-01-01T00:00:00.000-00:00</dcst:IssueDate>" + Environment.NewLine +
                        "  <dcst:ReelNumber>1</dcst:ReelNumber>" + Environment.NewLine +
                        "  <dcst:Language>en</dcst:Language>" + Environment.NewLine +
                        "  <dcst:EditRate>25 1</dcst:EditRate>" + Environment.NewLine +
                        "  <dcst:TimeCodeRate>25</dcst:TimeCodeRate>" + Environment.NewLine +
                        "  <dcst:StartTime>00:00:00:00</dcst:StartTime> " + Environment.NewLine +
                        "  <dcst:LoadFont ID=\"theFontId\">urn:uuid:3dec6dc0-39d0-498d-97d0-928d2eb78391</dcst:LoadFont>" + Environment.NewLine +
                        "  <dcst:SubtitleList>" + Environment.NewLine +
                           sb +
                        "  </dcst:SubtitleList>" + Environment.NewLine +
                        "</dcst:SubtitleReel>";


                    doc.LoadXml(xml);
                    string fName = saveFileDialog1.FileName;
                    if (!fName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        fName += ".xml";
                    }

                    File.WriteAllText(fName, SubtitleFormat.ToUtf8XmlString(doc));
                    var text = string.Format(LanguageSettings.Current.ExportPngXml.XImagesSavedInY, imagesSavedCount, Path.GetDirectoryName(fName));
                    MessageBoxShowWithFolderName(text, Path.GetDirectoryName(fName));
                }
                else if (_exportType == ExportFormats.Edl || _exportType == ExportFormats.EdlClipName)
                {
                    var title = Path.GetFileNameWithoutExtension(saveFileDialog1.FileName);
                    if (string.IsNullOrEmpty(title))
                    {
                        title = "( no title )";
                    }

                    string header = "TITLE: " + title + Environment.NewLine + Environment.NewLine;
                    File.WriteAllText(saveFileDialog1.FileName, header + sb);
                    var text = string.Format(LanguageSettings.Current.ExportPngXml.XImagesSavedInY, imagesSavedCount, Path.GetDirectoryName(saveFileDialog1.FileName));
                    MessageBoxShowWithFolderName(text, Path.GetDirectoryName(saveFileDialog1.FileName));
                }
                else
                {
                    WriteBdnXmlFile(imagesSavedCount, sb, Path.Combine(folderBrowserDialog1.SelectedPath, "BDN_Index.xml"));
                    var text = string.Format(LanguageSettings.Current.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath);
                    MessageBoxShowWithFolderName(text, folderBrowserDialog1.SelectedPath);
                }
            }
            buttonExport.Enabled = true;

            if (Configuration.Settings.General.CurrentVideoOffsetInMs != 0)
            {
                _subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(-Configuration.Settings.General.CurrentVideoOffsetInMs));
            }
        }

        private void MessageBoxShowWithFolderName(string text, string folderName)
        {
            using (var f = new ExportPngXmlDialogOpenFolder(text, folderName))
            {
                f.ShowDialog(this);
            }
        }

        internal void WriteFcpFile(int width, int height, StringBuilder sb, string fileName)
        {
            string fileNameNoPath = Path.GetFileName(fileName);
            string fileNameNoExt = Path.GetFileNameWithoutExtension(fileNameNoPath);

            int duration = 0;
            if (_subtitle.Paragraphs.Count > 0)
            {
                duration = (int)Math.Round(_subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1].EndTime.TotalSeconds * 25.0);
            }

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
              <ntsc>FALSE</ntsc>
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
            if (comboBoxFrameRate.Text == "29.97")
            {
                s = s.Replace("<displayformat>NDF</displayformat>", "<displayformat>DF</displayformat>"); //Non Drop Frame or Drop Frame
                s = s.Replace("<timebase>25</timebase>", "<timebase>30</timebase>");
                s = s.Replace("<ntsc>FALSE</ntsc>", "<ntsc>TRUE</ntsc>");
            }
            else if (comboBoxFrameRate.Text == "23.976")
            {
                s = s.Replace("<displayformat>NDF</displayformat>", "<displayformat>DF</displayformat>"); //Non Drop Frame or Drop Frame
                s = s.Replace("<timebase>25</timebase>", "<timebase>24</timebase>");
                s = s.Replace("<ntsc>FALSE</ntsc>", "<ntsc>TRUE</ntsc>");
            }
            else if (comboBoxFrameRate.Text == "59.94")
            {
                s = s.Replace("<displayformat>NDF</displayformat>", "<displayformat>DF</displayformat>"); //Non Drop Frame or Drop Frame
                s = s.Replace("<timebase>25</timebase>", "<timebase>60</timebase>");
                s = s.Replace("<ntsc>FALSE</ntsc>", "<ntsc>TRUE</ntsc>");
            }

            else
            {
                s = s.Replace("<timebase>25</timebase>", "<timebase>" + comboBoxFrameRate.Text + "</timebase>");
            }

            if (_subtitle.Paragraphs.Count > 0)
            {
                var end = SubtitleFormat.MillisecondsToFrames(_subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds, FrameRate);
                end++;
                s = s.Replace("[OUT]", end.ToString(CultureInfo.InvariantCulture));
            }

            if (comboBoxLanguage.Text == "NTSC")
            {
                s = s.Replace("<ntsc>FALSE</ntsc>", "<ntsc>TRUE</ntsc>");
            }

            s = s.Replace("<width>1920</width>", "<width>" + width.ToString(CultureInfo.InvariantCulture) + "</width>");
            s = s.Replace("<height>1080</height>", "<height>" + height.ToString(CultureInfo.InvariantCulture) + "</height>");

            if (comboBoxImageFormat.Text.Contains("8-bit"))
            {
                s = s.Replace("<colordepth>32</colordepth>", "<colordepth>8</colordepth>");
            }

            File.WriteAllText(fileName, s);
        }

        internal void WriteBdnXmlFile(int imagesSavedCount, StringBuilder sb, string fileName)
        {
            GetResolution(out var resW, out var resH);
            string videoFormat = "1080p";
            if (resW == 1920 && resH == 1080)
            {
                videoFormat = "1080p";
            }
            else if (resW == 1280 && resH == 720)
            {
                videoFormat = "720p";
            }
            else if (resW == 848 && resH == 480)
            {
                videoFormat = "480p";
            }
            else if (resW > 0 && resH > 0)
            {
                videoFormat = resW + "x" + resH;
            }

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
            File.WriteAllText(fileName, FormatUtf8Xml(doc), Encoding.UTF8);
        }

        internal void WriteDostFile(string fileName, string body)
        {
            string header = @"$FORMAT=480
$VERSION=1.2
$ULEAD=TRUE
$DROP=[DROPVALUE]" + Environment.NewLine + Environment.NewLine +
                                                "NO\tINTIME\t\tOUTTIME\t\tXPOS\tYPOS\tFILENAME\tFADEIN\tFADEOUT";

            string dropValue = "30000";
            if (comboBoxFrameRate.SelectedIndex == -1)
            {
                var numberAsString = comboBoxFrameRate.Text.Trim().RemoveChar('.').RemoveChar(',').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, string.Empty);
                if (numberAsString.Length > 0 && Utilities.IsInteger(numberAsString))
                {
                    dropValue = numberAsString;
                }
            }
            else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "23.98")
            {
                dropValue = "23976";
            }
            else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "24")
            {
                dropValue = "24000";
            }
            else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "25")
            {
                dropValue = "25000";
            }
            else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "29.97")
            {
                dropValue = "29970";
            }
            else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "30")
            {
                dropValue = "30000";
            }
            else if (comboBoxFrameRate.Items[comboBoxFrameRate.SelectedIndex].ToString() == "59.94")
            {
                dropValue = "59940";
            }

            header = header.Replace("[DROPVALUE]", dropValue);
            comboBoxFrameRate.SelectedIndex = 0;

            File.WriteAllText(fileName, header + Environment.NewLine + body);
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
                {
                    p.EndTime.TotalMilliseconds--;
                }
            }
        }

        private void SetResolution(string inputXAndY)
        {
            if (string.IsNullOrEmpty(inputXAndY))
            {
                return;
            }

            var xAndY = inputXAndY.ToLowerInvariant();

            if (_exportType == ExportFormats.Fcp)
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
                        xAndY = "FullHD 1920x1080";
                        break;
                    case "1280x1080":
                        xAndY = "HD-(1280x1080)";
                        break;
                    case "1440x1080":
                        xAndY = "HD-(1440x1080)";
                        break;
                }
            }

            if (_exportType == ExportFormats.Fcp || Regex.IsMatch(xAndY, @"\d+x\d+", RegexOptions.IgnoreCase))
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
            {
                return;
            }

            string text = comboBoxResolution.Text.Trim();

            if (_exportType == ExportFormats.Fcp)
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

                if (text == "FullHD 1920x1080")
                {
                    width = 1920;
                    height = 1080;
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
            {
                text = text.Remove(0, text.IndexOf('(')).Trim();
            }

            text = text.TrimStart('(').TrimEnd(')').Trim();
            string[] arr = text.Split('x');
            width = int.Parse(arr[0]);
            height = int.Parse(arr[1]);
        }

        private int WriteParagraph(int width, StringBuilder sb, int border, int height, int imagesSavedCount, VobSubWriter vobSubWriter, FileStream binarySubtitleFile, MakeBitmapParameter param, int i)
        {
            if (param.Bitmap != null)
            {
                if (_exportType == ExportFormats.BluraySup)
                {
                    if (!param.Saved)
                    {
                        binarySubtitleFile.Write(param.Buffer, 0, param.Buffer.Length);
                    }
                    param.Saved = true;
                }
                else if (_exportType == ExportFormats.VobSub)
                {
                    if (!param.Saved)
                    {
                        vobSubWriter.WriteParagraph(param.P, param.Bitmap, param.Alignment);
                    }

                    param.Saved = true;
                }
                else if (_exportType == ExportFormats.Fab)
                {
                    if (!param.Saved)
                    {
                        string numberString = $"IMAGE{i:000}";
                        string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLowerInvariant());

                        if (checkBoxFullFrameImage.Checked)
                        {
                            var nbmp = new NikseBitmap(param.Bitmap);
                            nbmp.ReplaceTransparentWith(panelFullFrameBackground.BackColor);
                            using (var bmp = nbmp.GetBitmap())
                            {
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
                                        {
                                            left = param.LeftMargin;
                                        }
                                        else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
                                        {
                                            left = param.ScreenWidth - param.Bitmap.Width - param.RightMargin;
                                        }

                                        if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
                                        {
                                            top = param.BottomMargin;
                                        }

                                        if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
                                        {
                                            top = (param.ScreenHeight - param.Bitmap.Height) / 2;
                                        }

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
                                sb.AppendLine($"{Path.GetFileName(fileName)} {FormatFabTime(param.P.StartTime, param)} {FormatFabTime(param.P.EndTime, param)} {left} {top} {left + param.ScreenWidth} {top + param.ScreenHeight}");
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
                            {
                                left = param.LeftMargin;
                            }
                            else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
                            {
                                left = param.ScreenWidth - param.Bitmap.Width - param.RightMargin;
                            }

                            if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
                            {
                                top = param.BottomMargin;
                            }

                            if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
                            {
                                top = (param.ScreenHeight - param.Bitmap.Height) / 2;
                            }

                            if (param.OverridePosition.HasValue &&
                                param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.Bitmap.Width &&
                                param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.Bitmap.Height)
                            {
                                left = param.OverridePosition.Value.X;
                                top = param.OverridePosition.Value.Y;
                            }

                            sb.AppendLine($"{Path.GetFileName(fileName)} {FormatFabTime(param.P.StartTime, param)} {FormatFabTime(param.P.EndTime, param)} {left} {top} {left + param.Bitmap.Width} {top + param.Bitmap.Height}");
                        }
                        param.Saved = true;
                    }
                }
                else if (_exportType == ExportFormats.Stl)
                {
                    if (!param.Saved)
                    {
                        string numberString = $"IMAGE{i:000}";
                        var path = Path.GetDirectoryName(saveFileDialog1.FileName);
                        string fileName = Path.Combine(path, numberString + "." + comboBoxImageFormat.Text.ToLowerInvariant());
                        SaveImage(param.Bitmap, fileName, ImageFormat);

                        imagesSavedCount++;

                        const string paragraphWriteFormat = "{0} , {1} , {2}\r\n";
                        const string timeFormat = "{0:00}:{1:00}:{2:00}:{3:00}";

                        double factor = TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate;
                        string startTime = string.Format(timeFormat, param.P.StartTime.Hours, param.P.StartTime.Minutes, param.P.StartTime.Seconds, (int)Math.Round(param.P.StartTime.Milliseconds / factor));
                        string endTime = string.Format(timeFormat, param.P.EndTime.Hours, param.P.EndTime.Minutes, param.P.EndTime.Seconds, (int)Math.Round(param.P.EndTime.Milliseconds / factor));
                        sb.AppendFormat(paragraphWriteFormat, startTime, endTime, fileName);

                        param.Saved = true;
                    }
                }
                else if (_exportType == ExportFormats.Spumux)
                {
                    if (!param.Saved)
                    {
                        string numberString = $"IMAGE{i:000}";
                        string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLowerInvariant());
                        foreach (var encoder in ImageCodecInfo.GetImageEncoders())
                        {
                            if (encoder.FormatID == ImageFormat.Png.Guid)
                            {
                                var parameters = new EncoderParameters { Param = { [0] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 4) } };

                                var nbmp = new NikseBitmap(param.Bitmap);
                                var b = nbmp.ConvertTo8BitsPerPixel();
                                b.Save(fileName, encoder, parameters);
                                b.Dispose();

                                break;
                            }
                        }
                        imagesSavedCount++;

                        const string paragraphWriteFormat = "\t\t<spu start=\"{0}\" end=\"{1}\" image=\"{2}\" xoffset=\"{3}\" yoffset=\"{4}\" />";
                        const string timeFormat = "{0:00}:{1:00}:{2:00}.{3:00}";

                        double factor = TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate;
                        string startTime = string.Format(timeFormat, param.P.StartTime.Hours, param.P.StartTime.Minutes, param.P.StartTime.Seconds, (int)Math.Round(param.P.StartTime.Milliseconds / factor));
                        string endTime = string.Format(timeFormat, param.P.EndTime.Hours, param.P.EndTime.Minutes, param.P.EndTime.Seconds, (int)Math.Round(param.P.EndTime.Milliseconds / factor));
                        int left = (param.ScreenWidth - param.Bitmap.Width) / 2;
                        if (param.Alignment == ContentAlignment.BottomLeft || param.Alignment == ContentAlignment.TopLeft)
                        {
                            left = param.LeftMargin;
                        }
                        else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.TopRight)
                        {
                            left = param.ScreenWidth - param.Bitmap.Width - param.LeftMargin;
                        }
                        int top = param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin);
                        if (param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopRight)
                        {
                            top = param.BottomMargin;
                        }
                        sb.AppendLine(string.Format(paragraphWriteFormat, startTime, endTime, fileName, left, top));

                        param.Saved = true;
                    }
                }
                else if (_exportType == ExportFormats.Fcp)
                {
                    if (!param.Saved)
                    {
                        imagesSavedCount = WriteFcpParagraph(sb, imagesSavedCount, param, i, saveFileDialog1.FileName);

                        param.Saved = true;
                    }
                }
                else if (_exportType == ExportFormats.Dost)
                {
                    if (!param.Saved)
                    {
                        imagesSavedCount = WriteParagraphDost(sb, imagesSavedCount, param, i, saveFileDialog1.FileName);
                        param.Saved = true;
                    }
                }
                else if (_exportType == ExportFormats.ImageFrame)
                {
                    if (!param.Saved)
                    {
                        var imageFormat = ImageFormat;

                        int lastFrame = imagesSavedCount;
                        int startFrame = (int)Math.Round(param.P.StartTime.TotalMilliseconds / (TimeCode.BaseUnit / param.FramesPerSeconds));
                        var empty = new Bitmap(param.ScreenWidth, param.ScreenHeight);

                        if (imagesSavedCount != 0 || !checkBoxSkipEmptyFrameAtStart.Checked)
                        {
                            // Save empty picture for each frame up to start frame
                            for (int k = lastFrame + 1; k < startFrame; k++)
                            {
                                string numberString = $"{k:00000}";
                                string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLowerInvariant());
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
                        {
                            startFrame = imagesSavedCount; // no overlapping
                        }

                        // Save sub picture for each frame in duration
                        for (int k = startFrame; k <= endFrame; k++)
                        {
                            string numberString = $"{k:00000}";
                            string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + "." + comboBoxImageFormat.Text.ToLowerInvariant());
                            fullSize.Save(fileName, imageFormat);
                            imagesSavedCount++;
                        }
                        fullSize.Dispose();
                        param.Saved = true;
                    }
                }
                else if (_exportType == ExportFormats.DCinemaInterop)
                {
                    if (!param.Saved)
                    {
                        string numberString = $"{i:0000}";
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

                        sb.AppendLine("<Subtitle FadeDownTime=\"" + 0 + "\" FadeUpTime=\"" + 0 + "\" TimeOut=\"" + DCinemaInterop.ConvertToTimeString(param.P.EndTime) + "\" TimeIn=\"" + DCinemaInterop.ConvertToTimeString(param.P.StartTime) + "\" SpotNumber=\"" + param.P.Number + "\">");
                        if (param.Depth3D == 0)
                        {
                            sb.AppendLine("<Image VPosition=\"" + vPos + "\" HPosition=\"" + hPos + "\" VAlign=\"" + verticalAlignment + "\" HAlign=\"" + horizontalAlignment + "\">" + numberString + ".png" + "</Image>");
                        }
                        else
                        {
                            sb.AppendLine("<Image VPosition=\"" + vPos + "\" HPosition=\"" + hPos + "\" ZPosition=\"" + param.Depth3D + "\" VAlign=\"" + verticalAlignment + "\" HAlign=\"" + horizontalAlignment + "\">" + numberString + ".png" + "</Image>");
                        }

                        sb.AppendLine("</Subtitle>");
                    }
                }
                else if (_exportType == ExportFormats.DCinemaSmpte2014)
                {
                    if (!param.Saved)
                    {
                        string numberString = $"{i:0000}";
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

                        sb.AppendLine("<Subtitle FadeDownTime=\"" + 0 + "\" FadeUpTime=\"" + 0 + "\" TimeOut=\"" + DCinemaInterop.ConvertToTimeString(param.P.EndTime) + "\" TimeIn=\"" + DCinemaInterop.ConvertToTimeString(param.P.StartTime) + "\" SpotNumber=\"" + param.P.Number + "\">");
                        if (param.Depth3D == 0)
                        {
                            sb.AppendLine("<Image VPosition=\"" + vPos + "\" HPosition=\"" + hPos + "\" VAlign=\"" + verticalAlignment + "\" HAlign=\"" + horizontalAlignment + "\">" + numberString + ".png" + "</Image>");
                        }
                        else
                        {
                            sb.AppendLine("<Image VPosition=\"" + vPos + "\" HPosition=\"" + hPos + "\" ZPosition=\"" + param.Depth3D + "\" VAlign=\"" + verticalAlignment + "\" HAlign=\"" + horizontalAlignment + "\">" + numberString + ".png" + "</Image>");
                        }

                        sb.AppendLine("</Subtitle>");
                    }
                }
                else if (_exportType == ExportFormats.Edl || _exportType == ExportFormats.EdlClipName)
                {
                    if (!param.Saved)
                    {
                        // 001  7M6C7986 V     C        14:14:55:21 14:15:16:24 01:00:10:18 01:00:31:21
                        var fileName1 = "IMG" + i.ToString(CultureInfo.InvariantCulture).PadLeft(5, '0');
                        if (!string.IsNullOrEmpty(_outputFileName))
                        {
                            var prefix = Path.GetFileNameWithoutExtension(_outputFileName);
                            if (!string.IsNullOrEmpty(prefix))
                            {
                                fileName1 = prefix
                                    .Replace(' ', '_')
                                    .Replace('.', '_')
                                    + "_" + fileName1;
                            }
                        }

                        var fullSize = new Bitmap(param.ScreenWidth, param.ScreenHeight);
                        using (var g = Graphics.FromImage(fullSize))
                        {
                            g.DrawImage(param.Bitmap, (param.ScreenWidth - param.Bitmap.Width) / 2, param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin));
                        }
                        var fileName2 = Path.Combine(Path.GetDirectoryName(param.SavDialogFileName), fileName1 + ".PNG");
                        fullSize.Save(fileName2, ImageFormat.Png);
                        fullSize.Dispose();

                        string line = $"{i:000}  {fileName1}  V     C        {new TimeCode().ToHHMMSSFF()} {param.P.Duration.ToHHMMSSFF()} {param.P.StartTime.ToHHMMSSFF()} {param.P.EndTime.ToHHMMSSFF()}";
                        sb.AppendLine(line);
                        if (_exportType == ExportFormats.EdlClipName)
                        {
                            sb.AppendLine("* FROM CLIP NAME: " + fileName1 + ".PNG");
                        }
                        sb.AppendLine();

                        imagesSavedCount++;
                        param.Saved = true;
                    }
                }
                else // BDNXML
                {
                    if (!param.Saved)
                    {
                        imagesSavedCount = WriteBdnXmlParagraph(width, sb, border, height, imagesSavedCount, param, i, folderBrowserDialog1.SelectedPath);
                        param.Saved = true;
                    }
                }
            }
            return imagesSavedCount;
        }

        internal int WriteFcpParagraph(StringBuilder sb, int imagesSavedCount, MakeBitmapParameter param, int i, string fileName)
        {
            string numberString = string.Format(Path.GetFileNameWithoutExtension(Path.GetFileName(fileName)) + "{0:0000}", i).RemoveChar(' ');
            var fileNameShort = numberString + "." + comboBoxImageFormat.Text.ToLowerInvariant();
            var targetImageFileName = Path.Combine(Path.GetDirectoryName(fileName), fileNameShort);
            string fileNameNoPath = Path.GetFileName(fileNameShort);
            string fileNameNoExt = Path.GetFileNameWithoutExtension(fileNameNoPath);
            string pathUrl = "file://localhost/" + targetImageFileName.Replace("\\", "/").Replace(" ", "%20");
            if (!checkBoxFcpFullPathUrl.Checked)
            {
                pathUrl = fileNameShort;
            }

            string template = " <clipitem id=\"" + System.Security.SecurityElement.Escape(fileNameNoPath) + "\">" + Environment.NewLine +
@"            <name>" + System.Security.SecurityElement.Escape(fileNameNoPath) + @"</name>
            <duration>[DURATION]</duration>
            <rate>
              <timebase>[TIMEBASE]</timebase>
              <ntsc>[NTSC]</ntsc>
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
              <pathurl>" + pathUrl + @"</pathurl>
              <rate>
                <timebase>[TIMEBASE]</timebase>
                <ntsc>[NTSC]</ntsc>
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

            var outBitmap = param.Bitmap;
            if (checkBoxFullFrameImage.Checked)
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
                            {
                                left = param.LeftMargin;
                            }
                            else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
                            {
                                left = param.ScreenWidth - param.Bitmap.Width - param.RightMargin;
                            }

                            if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
                            {
                                top = param.BottomMargin;
                            }

                            if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
                            {
                                top = (param.ScreenHeight - param.Bitmap.Height) / 2;
                            }

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
                        var b = nbmp.ConvertTo8BitsPerPixel();
                        b.Save(targetImageFileName, encoder, parameters);
                        b.Dispose();

                        break;
                    }
                }
            }
            else
            {
                SaveImage(outBitmap, targetImageFileName, ImageFormat);
            }
            imagesSavedCount++;

            int timeBase = 25;
            string ntsc = "FALSE";
            if (comboBoxLanguage.SelectedItem.ToString().Equals("NTSC", StringComparison.Ordinal))
            {
                ntsc = "TRUE";
            }

            if (Math.Abs(param.FramesPerSeconds - 29.97) < 0.01)
            {
                timeBase = 30;
                ntsc = "TRUE";
            }
            else if (Math.Abs(param.FramesPerSeconds - 23.976) < 0.01)
            {
                timeBase = 24;
                ntsc = "TRUE";
            }
            else if (Math.Abs(param.FramesPerSeconds - 59.94) < 0.01)
            {
                timeBase = 60;
                ntsc = "TRUE";
            }

            var duration = SubtitleFormat.MillisecondsToFrames(param.P.Duration.TotalMilliseconds, param.FramesPerSeconds);
            var start = SubtitleFormat.MillisecondsToFrames(param.P.StartTime.TotalMilliseconds, param.FramesPerSeconds);
            var end = SubtitleFormat.MillisecondsToFrames(param.P.EndTime.TotalMilliseconds, param.FramesPerSeconds);

            template = template.Replace("[DURATION]", duration.ToString(CultureInfo.InvariantCulture));
            template = template.Replace("[IN]", start.ToString(CultureInfo.InvariantCulture));
            template = template.Replace("[OUT]", end.ToString(CultureInfo.InvariantCulture));
            template = template.Replace("[START]", start.ToString(CultureInfo.InvariantCulture));
            template = template.Replace("[END]", end.ToString(CultureInfo.InvariantCulture));
            template = template.Replace("[TIMEBASE]", timeBase.ToString(CultureInfo.InvariantCulture));
            template = template.Replace("[NTSC]", ntsc);
            sb.AppendLine(template);
            return imagesSavedCount;
        }

        internal int WriteBdnXmlParagraph(int width, StringBuilder sb, int border, int height, int imagesSavedCount, MakeBitmapParameter param, int i, string path)
        {
            string numberString = $"{i:0000}";
            string fileName = Path.Combine(path, numberString + ".png");

            if (comboBoxImageFormat.Text == "Png 8-bit")
            {
                foreach (var encoder in ImageCodecInfo.GetImageEncoders())
                {
                    if (encoder.FormatID == ImageFormat.Png.Guid)
                    {
                        var parameters = new EncoderParameters();
                        parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 8);

                        var nbmp = new NikseBitmap(param.Bitmap);
                        var b = nbmp.ConvertTo8BitsPerPixel();
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
                          ToHHMMSSFF(param.P.EndTime) + "\" Forced=\"" + param.Forced.ToString().ToLowerInvariant() + "\">");

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
            return imagesSavedCount;
        }

        internal int WriteParagraphDost(StringBuilder sb, int imagesSavedCount, MakeBitmapParameter param, int i, string fileName)
        {
            string numberString = string.Format("{0:0000}", i);
            fileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName).Replace(" ", "_")) + "_" + numberString + ".png";

            foreach (var encoder in ImageCodecInfo.GetImageEncoders())
            {
                if (encoder.FormatID == ImageFormat.Png.Guid)
                {
                    var parameters = new EncoderParameters();
                    parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 8);

                    var nbmp = new NikseBitmap(param.Bitmap);
                    var b = nbmp.ConvertTo8BitsPerPixel();
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
            {
                left = param.LeftMargin;
            }
            else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
            {
                left = param.ScreenWidth - param.Bitmap.Width - param.RightMargin;
            }

            if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
            {
                top = param.BottomMargin;
            }

            if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
            {
                top = param.ScreenHeight - (param.Bitmap.Height / 2);
            }

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
            return imagesSavedCount;
        }

        private ImageFormat ImageFormat
        {
            get
            {
                var imageFormat = ImageFormat.Png;
                if (comboBoxImageFormat.SelectedIndex == 0)
                {
                    imageFormat = ImageFormat.Bmp;
                }
                else if (comboBoxImageFormat.SelectedIndex == 1)
                {
                    imageFormat = ImageFormat.Exif;
                }
                else if (comboBoxImageFormat.SelectedIndex == 2)
                {
                    imageFormat = ImageFormat.Gif;
                }
                else if (comboBoxImageFormat.SelectedIndex == 3)
                {
                    imageFormat = ImageFormat.Jpeg;
                }
                else if (comboBoxImageFormat.SelectedIndex == 4)
                {
                    imageFormat = ImageFormat.Png;
                }
                else if (comboBoxImageFormat.SelectedIndex == 5)
                {
                    imageFormat = ImageFormat.Tiff;
                }

                if (string.Compare(comboBoxImageFormat.Text, "tga", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return ImageFormat.Icon;
                }

                return imageFormat;
            }
        }

        private static string FormatFabTime(TimeCode time, MakeBitmapParameter param)
        {
            if (param.Bitmap.Width == 720 && param.Bitmap.Height == 480) // NTSC
            {
                return $"{time.Hours:00};{time.Minutes:00};{time.Seconds:00};{SubtitleFormat.MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
            }

            // drop frame
            if (Math.Abs(param.FramesPerSeconds - 24 * (999 / 1000)) < 0.01 ||
                Math.Abs(param.FramesPerSeconds - 29 * (999 / 1000)) < 0.01 ||
                Math.Abs(param.FramesPerSeconds - 59 * (999 / 1000)) < 0.01)
            {
                return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}:{SubtitleFormat.MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
            }

            return $"{time.Hours:00};{time.Minutes:00};{time.Seconds:00};{SubtitleFormat.MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}";
        }

        private void SetupImageParameters()
        {
            if (subtitleListView1.SelectedItems.Count > 0 && _format.HasStyleSupport)
            {
                Paragraph p = _subtitle.GetParagraphOrDefault(subtitleListView1.SelectedItems[0].Index);
                if (p != null && (_format.GetType() == typeof(AdvancedSubStationAlpha) || _format.GetType() == typeof(SubStationAlpha)))
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
                        comboBoxBottomMarginUnit.Enabled = false;
                        comboBoxBottomMarginUnit.SelectedIndex = 1; // px
                        comboBoxLeftRightMargin.Enabled = false;
                        comboBoxLeftRightMarginUnit.Enabled = false;
                        comboBoxLeftRightMarginUnit.SelectedIndex = 1; // px
                        comboBoxShadowWidth.Enabled = false;
                        buttonShadowColor.Enabled = false;
                        panelShadowColor.Enabled = false;
                        numericUpDownShadowTransparency.Enabled = _format.GetType() != typeof(AdvancedSubStationAlpha);

                        SsaStyle style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, _subtitle.Header);
                        if (style != null)
                        {
                            int i;
                            for (i = 0; i < comboBoxBottomMargin.Items.Count; i++)
                            {
                                if (comboBoxBottomMargin.Items[i].ToString().Equals(style.MarginVertical.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                                {
                                    comboBoxBottomMargin.SelectedIndex = i;
                                    break;
                                }
                            }

                            for (i = 0; i < comboBoxLeftRightMarginUnit.Items.Count; i++)
                            {
                                if (comboBoxLeftRightMarginUnit.Items[i].ToString().Equals(style.MarginLeft.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                                {
                                    comboBoxLeftRightMarginUnit.SelectedIndex = i;
                                    break;
                                }
                            }

                            panelColor.BackColor = style.Primary;
                            panelBorderColor.BackColor = _format.GetType() == typeof(AdvancedSubStationAlpha) ? style.Outline : style.Background;

                            for (i = 0; i < comboBoxSubtitleFont.Items.Count; i++)
                            {
                                if (comboBoxSubtitleFont.Items[i].ToString().Equals(style.FontName, StringComparison.OrdinalIgnoreCase))
                                {
                                    comboBoxSubtitleFont.SelectedIndex = i;
                                    break;
                                }
                            }
                            for (i = 0; i < comboBoxSubtitleFontSize.Items.Count; i++)
                            {
                                if (comboBoxSubtitleFontSize.Items[i].ToString().Equals(style.FontSize.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                                {
                                    comboBoxSubtitleFontSize.SelectedIndex = i;
                                    break;
                                }
                            }
                            checkBoxBold.Checked = style.Bold;
                            comboBoxBorderWidth.Items.Clear();
                            comboBoxBorderWidth.Items.Add(style.OutlineWidth.ToString(CultureInfo.InvariantCulture));
                            comboBoxBorderWidth.SelectedIndex = 0;

                            comboBoxShadowWidth.Items.Clear();
                            comboBoxShadowWidth.Items.Add(style.ShadowWidth.ToString(CultureInfo.InvariantCulture));
                            comboBoxShadowWidth.SelectedIndex = 0;
                            if (_format.GetType() == typeof(AdvancedSubStationAlpha))
                            {
                                panelShadowColor.BackColor = style.Background;
                                numericUpDownShadowTransparency.Value = style.Background.A;
                            }
                            else
                            {
                                panelShadowColor.BackColor = style.Outline;
                                numericUpDownShadowTransparency.Value = style.Outline.A;
                            }
                        }
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
            if (comboBoxBorderWidth.SelectedItem.ToString() == LanguageSettings.Current.ExportPngXml.BorderStyleBoxForEachLine ||
                comboBoxBorderWidth.SelectedItem.ToString() == LanguageSettings.Current.ExportPngXml.BorderStyleOneBox)
            {
                return 0;
            }

            if (float.TryParse(comboBoxBorderWidth.SelectedItem.ToString(), out var f))
            {
                return f;
            }

            if (float.TryParse(Utilities.RemoveNonNumbers(comboBoxBorderWidth.SelectedItem.ToString()), out f))
            {
                return f;
            }

            return 0;
        }

        private float GetShadowWidth()
        {
            if (float.TryParse(comboBoxShadowWidth.SelectedItem.ToString(), out var f))
            {
                return f;
            }

            if (float.TryParse(Utilities.RemoveNonNumbers(comboBoxShadowWidth.SelectedItem.ToString()), out f))
            {
                return f;
            }

            return 0;
        }

        private static Font GetFont(MakeBitmapParameter parameter, float fontSize)
        {
            Font font;
            try
            {
                var fontStyle = FontStyle.Regular;
                if (parameter.SubtitleFontBold)
                {
                    fontStyle = FontStyle.Bold;
                }

                font = new Font(parameter.SubtitleFontName, fontSize, fontStyle);
            }
            catch (Exception exception)
            {
                try
                {
                    var fontStyle = FontStyle.Regular;
                    if (!parameter.SubtitleFontBold)
                    {
                        fontStyle = FontStyle.Bold;
                    }

                    font = new Font(parameter.SubtitleFontName, fontSize, fontStyle);
                }
                catch
                {
                    MessageBox.Show(exception.Message);

                    if (FontFamily.Families[0].IsStyleAvailable(FontStyle.Regular))
                    {
                        font = new Font(FontFamily.Families[0].Name, fontSize);
                    }
                    else if (FontFamily.Families.Length > 1 && FontFamily.Families[1].IsStyleAvailable(FontStyle.Regular))
                    {
                        font = new Font(FontFamily.Families[1].Name, fontSize);
                    }
                    else if (FontFamily.Families.Length > 2 && FontFamily.Families[1].IsStyleAvailable(FontStyle.Regular))
                    {
                        font = new Font(FontFamily.Families[2].Name, fontSize);
                    }
                    else
                    {
                        font = new Font("Arial", fontSize);
                    }
                }
            }
            return font;
        }

        private double GetResizeScale()
        {
            if (comboBoxResizePercentage.SelectedItem == null)
            {
                return 1.0;
            }

            var p = int.Parse(comboBoxResizePercentage.SelectedItem.ToString().Replace("%", string.Empty));
            return p / 100.0;
        }

        private Bitmap GenerateImageFromTextWithStyle(Paragraph p, out MakeBitmapParameter mbp)
        {
            mbp = new MakeBitmapParameter { P = p };

            if (_vobSubOcr != null)
            {
                var index = _subtitle.GetIndex(p);
                if (index >= 0)
                {
                    var b = _vobSubOcr.GetSubtitleBitmap(index);
                    var exp = GetResizeScale();
                    if (Math.Abs(exp - 1) > 0.01)
                    {
                        var resizedBitmap = ResizeBitmap(b, (int)Math.Round(b.Width * exp), (int)Math.Round(b.Height * exp));
                        b.Dispose();
                        return resizedBitmap;
                    }
                    return b;
                }

            }

            mbp.AlignLeft = comboBoxHAlign.SelectedIndex == 0;
            mbp.AlignRight = comboBoxHAlign.SelectedIndex == 2;
            mbp.JustifyLeft = GetJustifyLeft(p.Text);
            mbp.JustifyTop = comboBoxHAlign.SelectedIndex == 4;
            mbp.JustifyRight = comboBoxHAlign.SelectedIndex == 5;
            mbp.SimpleRendering = checkBoxSimpleRender.Checked;
            mbp.BorderWidth = _borderWidth;
            mbp.BorderColor = _borderColor;
            mbp.SubtitleFontName = _subtitleFontName;
            mbp.SubtitleColor = _subtitleColor;
            mbp.SubtitleFontSize = _subtitleFontSize;
            mbp.SubtitleFontBold = _subtitleFontBold;
            mbp.LineHeight = _lineHeights;
            mbp.FullFrame = checkBoxFullFrameImage.Checked;
            mbp.FullFrameBackgroundColor = panelFullFrameBackground.BackColor;
            mbp.OverridePosition = GetAssPoint(p.Text);

            if (_format.HasStyleSupport && !string.IsNullOrEmpty(p.Extra))
            {
                if (_format.GetType() == typeof(SubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, _subtitle.Header);
                    mbp.SubtitleColor = style.Primary;
                    mbp.SubtitleFontBold = style.Bold;
                    mbp.SubtitleFontSize = style.FontSize;
                    mbp.BottomMargin = style.MarginVertical;
                    if (style.BorderStyle == "3")
                    {
                        mbp.BackgroundColor = style.Background;
                    }
                    mbp.ShadowColor = style.Outline;
                }
                else if (_format.GetType() == typeof(AdvancedSubStationAlpha))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, _subtitle.Header);
                    mbp.SubtitleColor = style.Primary;
                    mbp.SubtitleFontBold = style.Bold;
                    mbp.SubtitleFontSize = style.FontSize;
                    mbp.BottomMargin = style.MarginVertical;
                    if (style.BorderStyle == "3")
                    {
                        mbp.BackgroundColor = style.Outline;
                    }
                    mbp.ShadowAlpha = style.Background.A;
                    mbp.ShadowColor = style.Background;
                }
            }

            if (comboBoxBorderWidth.SelectedItem.ToString() == LanguageSettings.Current.ExportPngXml.BorderStyleBoxForEachLine)
            {
                _borderWidth = 0;
                mbp.BackgroundColor = panelBorderColor.BackColor;
                mbp.BoxSingleLine = true;
            }
            else if (comboBoxBorderWidth.SelectedItem.ToString() == LanguageSettings.Current.ExportPngXml.BorderStyleOneBox)
            {
                mbp.BoxSingleLine = false;
                _borderWidth = 0;
                mbp.BackgroundColor = panelBorderColor.BackColor;
            }

            GetResolution(out var width, out var height);
            mbp.ScreenWidth = width;
            mbp.ScreenHeight = height;
            mbp.VideoResolution = comboBoxResolution.Text;
            mbp.Type3D = comboBox3D.SelectedIndex;
            mbp.Depth3D = (int)numericUpDownDepth3D.Value;
            mbp.BottomMargin = GetBottomMarginInPixels(p);
            mbp.ShadowWidth = GetShadowWidth();
            mbp.ShadowAlpha = (int)numericUpDownShadowTransparency.Value;
            mbp.ShadowColor = panelShadowColor.BackColor;
            mbp.LineHeight = _lineHeights;
            mbp.Forced = subtitleListView1.Items[_subtitle.GetIndex(p)].Checked;
            mbp.LineJoin = Configuration.Settings.Tools.ExportPenLineJoin;
            var bmp = GenerateImageFromTextWithStyle(mbp);
            if (_exportType == ExportFormats.VobSub || _exportType == ExportFormats.Stl || _exportType == ExportFormats.Spumux)
            {
                var nbmp = new NikseBitmap(bmp);
                nbmp.ConvertToFourColors(Color.Transparent, _subtitleColor, _borderColor, !checkBoxTransAntiAliase.Checked);

                if (_exportType == ExportFormats.Spumux)
                {
                    nbmp.EnsureEvenLines(mbp.BoxSingleLine ? Color.Transparent : mbp.BackgroundColor);
                }

                var temp = nbmp.GetBitmap();
                bmp.Dispose();
                return temp;
            }
            return bmp;
        }

        private static int CalcWidthViaDraw(string text, MakeBitmapParameter parameter)
        {
            var nbmp = GenerateBitmapForCalc(text, parameter);
            nbmp.CropTransparentSidesAndBottom(0, true);
            return nbmp.Width;
        }

        private static NikseBitmap GenerateBitmapForCalc(string text, MakeBitmapParameter parameter)
        {
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

            Font font = GetFont(parameter, parameter.SubtitleFontSize);
            var fontStack = new Stack<Font>();
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
                        lastText.Append(sb);
                        TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                    }

                    addLeft = GetLastPositionFromPath(path, oldPathPointIndex, addLeft);
                    if (path.PointCount == 0)
                    {
                        addLeft = left;
                    }
                    else if (addLeft < 0.01)
                    {
                        addLeft = left + 2;
                    }

                    left = addLeft;

                    DrawShadowAndPath(parameter, g, path);
                    var p2 = new SolidBrush(c);
                    g.FillPath(p2, path);
                    p2.Dispose();
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
                            string[] arr = fontContent.Substring(fontContent.IndexOf(" color=", StringComparison.OrdinalIgnoreCase) + 7).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                                    c = parameter.SubtitleColor;
                                }
                            }
                        }
                        if (fontContent.Contains(" face=", StringComparison.OrdinalIgnoreCase) || fontContent.Contains(" size=", StringComparison.OrdinalIgnoreCase))
                        {
                            float fontSize = parameter.SubtitleFontSize;
                            string fontFace = parameter.SubtitleFontName;

                            string[] arr = fontContent.Substring(fontContent.IndexOf(" face=", StringComparison.OrdinalIgnoreCase) + 6).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (arr.Length > 0)
                            {
                                fontFace = arr[0].Trim('\'').Trim('"').Trim('\'');
                            }

                            arr = fontContent.Substring(fontContent.IndexOf(" size=", StringComparison.OrdinalIgnoreCase) + 6).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (arr.Length > 0)
                            {
                                string temp = arr[0].Trim('\'').Trim('"').Trim('\'');
                                if (float.TryParse(temp, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var f))
                                {
                                    fontSize = f;
                                }
                            }

                            try
                            {
                                fontStack.Push(font); // save old cfont
                                var p = new MakeBitmapParameter { SubtitleFontName = fontFace, SubtitleFontSize = fontSize };
                                font = GetFont(p, p.SubtitleFontSize);
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
                            if (lastText.Length > 0 && left > 2)
                            {
                                left -= 1.5f;
                            }

                            lastText.Append(sb);

                            TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                        }

                        addLeft = GetLastPositionFromPath(path, oldPathPointIndex, addLeft);
                        if (addLeft < 0.01)
                        {
                            addLeft = left + 2;
                        }

                        left = addLeft;

                        DrawShadowAndPath(parameter, g, path);
                        g.FillPath(new SolidBrush(c), path);
                        path.Reset();
                        sb = new StringBuilder();
                        if (colorStack.Count > 0)
                        {
                            c = colorStack.Pop();
                        }

                        if (left >= 3)
                        {
                            left -= 2.5f;
                        }
                    }
                    if (fontStack.Count > 0)
                    {
                        font.Dispose();
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
            {
                TextDraw.DrawText(font, sf, path, sb, isItalic, parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
            }

            DrawShadowAndPath(parameter, g, path);
            g.FillPath(new SolidBrush(c), path);
            g.Dispose();

            var nbmp = new NikseBitmap(bmp);
            bmp.Dispose();
            font.Dispose();
            sf.Dispose();
            return nbmp;
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
                var oldBackgroundColor = parameter.BackgroundColor;
                if (parameter.P.Text.Contains(BoxSingleLineText))
                {
                    parameter.P.Text = parameter.P.Text.Replace("<" + BoxSingleLineText + ">", string.Empty).Replace("</" + BoxSingleLineText + ">", string.Empty);
                    parameter.BackgroundColor = parameter.BorderColor;
                }

                var italicOn = false;
                var boldOn = false;
                var fontTag = string.Empty;
                var lineWidts = new List<int>();
                foreach (var line in parameter.P.Text.SplitToLines())
                {
                    parameter.P.Text = line;

                    if (italicOn)
                    {
                        parameter.P.Text = "<i>" + parameter.P.Text;
                    }
                    italicOn = parameter.P.Text.Contains("<i>", StringComparison.OrdinalIgnoreCase) && !parameter.P.Text.Contains("</i>", StringComparison.OrdinalIgnoreCase);
                    if (italicOn)
                    {
                        parameter.P.Text += "</i>";
                    }

                    if (boldOn)
                    {
                        parameter.P.Text = "<b>" + parameter.P.Text;
                    }
                    boldOn = parameter.P.Text.Contains("<b>", StringComparison.OrdinalIgnoreCase) && !parameter.P.Text.Contains("</b>", StringComparison.OrdinalIgnoreCase);
                    if (boldOn)
                    {
                        parameter.P.Text += "</b>";
                    }

                    parameter.P.Text = fontTag + parameter.P.Text;
                    if (parameter.P.Text.Contains("<font ", StringComparison.OrdinalIgnoreCase) && !parameter.P.Text.Contains("</font>", StringComparison.OrdinalIgnoreCase))
                    {
                        int start = parameter.P.Text.LastIndexOf("<font ", StringComparison.OrdinalIgnoreCase);
                        int end = parameter.P.Text.IndexOf('>', start);
                        fontTag = parameter.P.Text.Substring(start, end - start + 1);
                    }

                    var lineImage = GenerateImageFromTextWithStyleInner(parameter);
                    lineWidts.Add(lineImage.Width);
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
                        {
                            l1 = 0;
                        }
                        else if (parameter.AlignRight)
                        {
                            l1 = w - bmp.Width;
                        }
                        else
                        {
                            l1 = (int)Math.Round((w - bmp.Width) / 2.0);

                            if (parameter.JustifyLeft)
                            {
                                l1 = 0;
                            }
                            else if (parameter.JustifyRight)
                            {
                                l1 = w - lineImage.Width;
                            }
                        }

                        int l2;
                        if (parameter.AlignLeft)
                        {
                            l2 = 0;
                        }
                        else if (parameter.AlignRight)
                        {
                            l2 = w - lineImage.Width;
                        }
                        else
                        {
                            l2 = (int)Math.Round((w - lineImage.Width) / 2.0);

                            if (parameter.JustifyLeft)
                            {
                                l2 = 0;
                            }
                            else if (parameter.JustifyRight)
                            {
                                l2 = w - lineImage.Width;
                                if (parameter.BoxSingleLine)
                                {
                                    l1 = w - lineWidts[0];
                                }
                            }
                        }

                        var style = GetStyleName(parameter.P);
                        var lineHeight = 25;
                        if (parameter.LineHeight.ContainsKey(style))
                        {
                            lineHeight = parameter.LineHeight[style];
                        }
                        else if (parameter.LineHeight.Count > 0)
                        {
                            lineHeight = parameter.LineHeight.First().Value;
                        }

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
                    bmp?.Dispose();
                    bmp = newBmp;
                }
            }
            else
            {
                var oldBackgroundColor = parameter.BackgroundColor;
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

        private static readonly Dictionary<string, int> PaddingDictionary = new Dictionary<string, int>();
        private static Bitmap GenerateImageFromTextWithStyleInner(MakeBitmapParameter parameter) // for UI
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
                font = GetFont(parameter, parameter.SubtitleFontSize);
                SizeF textSize;
                using (var bmpTemp = new Bitmap(1, 1))
                using (var g = Graphics.FromImage(bmpTemp))
                {
                    textSize = g.MeasureString(HtmlUtil.RemoveHtmlTags(text), font);
                }
                int sizeX = (int)(textSize.Width * 1.8) + 150;
                int sizeY = (int)(textSize.Height * 0.9) + 50;
                if (sizeX < 1)
                {
                    sizeX = 1;
                }

                if (sizeY < 1)
                {
                    sizeY = 1;
                }

                if (parameter.BackgroundColor != Color.Transparent)
                {
                    var nbmpTemp = new NikseBitmap(sizeX, sizeY + _boxBorderSize * 2); // make room for box border above+under text
                    nbmpTemp.Fill(parameter.BackgroundColor);
                    bmp = nbmpTemp.GetBitmap();
                }
                else
                {
                    bmp = new Bitmap(sizeX, sizeY);
                }

                var paddingKey = font.Name + font.Size.ToString(CultureInfo.InvariantCulture);
                int baseLinePadding;
                if (PaddingDictionary.ContainsKey(paddingKey))
                {
                    baseLinePadding = PaddingDictionary[paddingKey];
                }
                else
                {
                    baseLinePadding = (int)Math.Round(TextDraw.MeasureTextHeight(font, "yjK)", parameter.SubtitleFontBold) - TextDraw.MeasureTextHeight(font, "ac", parameter.SubtitleFontBold));
                    PaddingDictionary.Add(paddingKey, baseLinePadding);
                }

                // align lines with "gjpqy,ýęçÇ/()[]" a bit lower
                var lines = text.SplitToLines();
                if (lines.Count > 0)
                {
                    var lastLine = lines[lines.Count - 1];
                    if (lastLine.Contains(new[] { 'g', 'j', 'p', 'q', 'y', ',', 'ý', 'ę', 'ç', 'Ç', '/', '(', ')', '[', ']' }))
                    {
                        var textNoBelow = lastLine.Replace('g', 'a').Replace('j', 'a').Replace('p', 'a').Replace('q', 'a').Replace('y', 'a').Replace(',', 'a').Replace('ý', 'a').Replace('ę', 'a').Replace('ç', 'a').Replace('Ç', 'a').Replace('/', 'a').Replace('(', 'a').Replace(')', 'a').Replace('[', 'a').Replace(']', 'a');
                        baseLinePadding -= (int)Math.Round(TextDraw.MeasureTextHeight(font, lastLine, parameter.SubtitleFontBold) - TextDraw.MeasureTextHeight(font, textNoBelow, parameter.SubtitleFontBold));
                    }
                    else
                    {
                        baseLinePadding += 1;
                    }
                    if (baseLinePadding < 0)
                    {
                        baseLinePadding = 0;
                    }
                }

                if (lines.Count == 1 && parameter.JustifyTop) // align top
                {
                    baseLinePadding += (int)Math.Round(TextDraw.MeasureTextHeight(font, "yjK)", parameter.SubtitleFontBold));
                }

                // TODO: Better baseline - test http://bobpowell.net/formattingtext.aspx
                //float baselineOffset=font.SizeInPoints/font.FontFamily.GetEmHeight(font.Style)*font.FontFamily.GetCellAscent(font.Style);
                //float baselineOffsetPixels = g.DpiY/72f*baselineOffset;
                //baseLinePadding = (int)Math.Round(baselineOffsetPixels);

                var lefts = new List<float>();
                var widths = new List<float>();
                if (text.Contains("<font", StringComparison.OrdinalIgnoreCase) || text.Contains("<i>", StringComparison.OrdinalIgnoreCase) || text.Contains("<b>", StringComparison.OrdinalIgnoreCase))
                {
                    bool tempItalicOn = false;
                    bool tempBoldOn = false;
                    var tempFontOn = string.Empty;
                    foreach (string line in text.SplitToLines())
                    {
                        var tempLine = line;

                        if (tempItalicOn)
                        {
                            tempLine = "<i>" + tempLine;
                        }

                        if (tempBoldOn)
                        {
                            tempLine = "<b>" + tempLine;
                        }

                        if (!string.IsNullOrEmpty(tempFontOn))
                        {
                            tempLine = tempFontOn + tempLine;
                        }

                        if (tempLine.LastIndexOf("<font ", StringComparison.Ordinal) >= 0 &&
                            tempLine.LastIndexOf("</font>", StringComparison.Ordinal) <
                            tempLine.LastIndexOf("<font ", StringComparison.Ordinal))
                        {
                            var start = tempLine.LastIndexOf("<font ", StringComparison.Ordinal);
                            var end = tempLine.IndexOf('>', start);
                            if (end > 0)
                            {
                                tempFontOn = tempLine.Substring(start, end - start + 1);
                            }
                        }
                        else if (tempLine.LastIndexOf("</font>", StringComparison.Ordinal) >= 0)
                        {
                            tempFontOn = string.Empty;
                        }

                        if (tempLine.Contains("<i>") && !tempLine.Contains("</i>"))
                        {
                            tempItalicOn = true;
                        }

                        if (tempLine.Contains("<b>") && !tempLine.Contains("</b>"))
                        {
                            tempBoldOn = true;
                        }

                        int w;
                        if (text.Contains("<font", StringComparison.OrdinalIgnoreCase))
                        {
                            var tempBmp = GenerateBitmapForCalc(tempLine, parameter);
                            tempBmp.CropTransparentSidesAndBottom(0, false);
                            w = tempBmp.Width;
                        }
                        else
                        {
                            tempLine = HtmlUtil.RemoveOpenCloseTags(tempLine, HtmlUtil.TagFont);
                            w = CalcWidthViaDraw(tempLine, parameter);
                        }

                        widths.Add(w);
                        if (parameter.AlignLeft)
                        {
                            lefts.Add(5);
                        }
                        else if (parameter.AlignRight)
                        {
                            lefts.Add(bmp.Width - w - 15); // calculate via drawing+crop
                        }
                        else
                        {
                            lefts.Add((float)((bmp.Width - w + 5.0) / 2.0)); // calculate via drawing+crop
                        }

                        if (line.Contains("</i>"))
                        {
                            tempItalicOn = false;
                        }

                        if (line.Contains("</b>"))
                        {
                            tempBoldOn = false;
                        }
                    }
                }
                else
                {
                    foreach (var line in HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic, HtmlUtil.TagFont).SplitToLines())
                    {
                        if (parameter.JustifyRight)
                        {
                            var w = TextDraw.MeasureTextWidth(font, line, parameter.SubtitleFontBold);
                            widths.Add(w);
                        }

                        if (parameter.AlignLeft)
                        {
                            lefts.Add(5);
                        }
                        else if (parameter.AlignRight)
                        {
                            lefts.Add(bmp.Width - (TextDraw.MeasureTextWidth(font, line, parameter.SubtitleFontBold) + 15));
                        }
                        else
                        {
                            lefts.Add((float)((bmp.Width - TextDraw.MeasureTextWidth(font, line, parameter.SubtitleFontBold) + 15) / 2.0));
                        }
                    }
                }

                if (parameter.JustifyLeft)
                {
                    // left justify centered lines
                    var minX = lefts.Min(p => p);
                    for (var index = 0; index < lefts.Count; index++)
                    {
                        lefts[index] = minX;
                    }
                }
                else if (parameter.JustifyRight)
                {
                    // right justify centered lines
                    var maxX = widths.Max(p => p);
                    var minX = lefts.Min(p => p);
                    for (var index = 0; index < lefts.Count; index++)
                    {
                        lefts[index] = minX + maxX - widths[index];
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
                            if (parameter.JustifyLeft)
                            {
                                sf.Alignment = StringAlignment.Near;
                            }
                            else if (parameter.JustifyRight)
                            {
                                sf.Alignment = StringAlignment.Far;
                            }
                        }

                        bmp.Dispose();
                        bmp = new Bitmap(parameter.ScreenWidth, sizeY);

                        var surface = Graphics.FromImage(bmp);
                        surface.CompositingQuality = CompositingQuality.HighSpeed;
                        surface.InterpolationMode = InterpolationMode.Default;
                        surface.SmoothingMode = SmoothingMode.HighSpeed;
                        surface.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        var newFontSize = (float)(font.Size * 0.7); // make simple rendering close to same size as normal renderer
                        using (var newFont = new Font(font.FontFamily, newFontSize, font.Style))
                        {
                            for (int j = 0; j < parameter.BorderWidth; j++)
                            {
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j, Y = y - 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j, Y = y - 0 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j, Y = y + 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j + 1, Y = y - 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j + 1, Y = y - 0 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j + 1, Y = y + 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j - 1, Y = y - 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j - 1, Y = y - 0 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j - 1, Y = y + 1 + j }, sf);

                                surface.DrawString(text, newFont, brush, new PointF { X = x - j, Y = y - 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j, Y = y - 0 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j, Y = y + 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j + 1, Y = y - 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j + 1, Y = y - 0 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j + 1, Y = y + 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j - 1, Y = y - 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j - 1, Y = y - 0 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j - 1, Y = y + 1 + j }, sf);

                                surface.DrawString(text, newFont, brush, new PointF { X = x - j, Y = y - 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j, Y = y - 0 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j, Y = y + 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j + 1, Y = y - 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j + 1, Y = y - 0 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j + 1, Y = y + 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j - 1, Y = y - 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j - 1, Y = y - 0 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - j - 1, Y = y + 1 - j }, sf);

                                surface.DrawString(text, newFont, brush, new PointF { X = x + j, Y = y - 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j, Y = y - 0 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j, Y = y + 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j + 1, Y = y - 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j + 1, Y = y - 0 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j + 1, Y = y + 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j - 1, Y = y - 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j - 1, Y = y - 0 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j - 1, Y = y + 1 - j }, sf);

                                surface.DrawString(text, newFont, brush, new PointF { X = x + j, Y = y - 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j, Y = y - 0 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j, Y = y + 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j + 1, Y = y - 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j + 1, Y = y - 0 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j + 1, Y = y + 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j - 1, Y = y - 1 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j - 1, Y = y - 0 + j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + j - 1, Y = y + 1 + j }, sf);

                                surface.DrawString(text, newFont, brush, new PointF { X = x, Y = y - 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x, Y = y - 0 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x, Y = y + 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + 1, Y = y - 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + 1, Y = y - 0 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x + 1, Y = y + 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - 1, Y = y - 1 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - 1, Y = y - 0 - j }, sf);
                                surface.DrawString(text, newFont, brush, new PointF { X = x - 1, Y = y + 1 - j }, sf);
                            }
                            brush.Dispose();
                            brush = new SolidBrush(parameter.SubtitleColor);
                            surface.CompositingQuality = CompositingQuality.HighQuality;
                            surface.SmoothingMode = SmoothingMode.HighQuality;
                            surface.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            surface.DrawString(text, newFont, brush, new PointF { X = x, Y = y }, sf);
                        }
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
                        {
                            left = lefts[0];
                        }

                        float top = 5;
                        if (top < _boxBorderSize && parameter.BackgroundColor != Color.Transparent)
                        {
                            top = _boxBorderSize; // make text down so box border will be satisfied
                        }

                        bool newLine = false;
                        int lineNumber = 0;
                        float leftMargin = left;
                        int newLinePathPoint = -1;
                        Color c = parameter.SubtitleColor;
                        var colorStack = new Stack<Color>();
                        var fontStack = new Stack<Font>();
                        var lastText = new StringBuilder();
                        for (var i = 0; i < text.Length; i++)
                        {
                            if (text.Substring(i).StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                            {
                                float addLeft = 0;
                                int oldPathPointIndex = path.PointCount - 1;
                                if (oldPathPointIndex < 0)
                                {
                                    oldPathPointIndex = 0;
                                }

                                if (sb.Length > 0)
                                {
                                    lastText.Append(sb);
                                    TextDraw.DrawText(font, sf, path, sb, isItalic, isBold || parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                                }

                                addLeft = GetLastPositionFromPath(path, oldPathPointIndex, addLeft);
                                if (path.PointCount == 0)
                                {
                                    addLeft = left;
                                }
                                else if (addLeft < 0.01)
                                {
                                    addLeft = left + 2;
                                }

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
                                    if (fontContent.Contains(" color=", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string[] arr = fontContent.Substring(fontContent.IndexOf(" color=", StringComparison.OrdinalIgnoreCase) + 7).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
                                                c = parameter.SubtitleColor;
                                            }
                                        }
                                    }
                                    if (fontContent.Contains(" face=", StringComparison.OrdinalIgnoreCase) || fontContent.Contains(" size=", StringComparison.OrdinalIgnoreCase))
                                    {
                                        float fontSize = parameter.SubtitleFontSize;
                                        string fontFace = parameter.SubtitleFontName;

                                        string[] arr = fontContent.Substring(fontContent.IndexOf(" face=", StringComparison.OrdinalIgnoreCase) + 6).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (arr.Length > 0)
                                        {
                                            fontFace = arr[0].Trim('\'').Trim('"').Trim('\'');
                                        }

                                        arr = fontContent.Substring(fontContent.IndexOf(" size=", StringComparison.OrdinalIgnoreCase) + 6).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (arr.Length > 0)
                                        {
                                            string temp = arr[0].Trim('\'').Trim('"').Trim('\'');
                                            if (float.TryParse(temp, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var f))
                                            {
                                                fontSize = f;
                                            }
                                        }

                                        try
                                        {
                                            fontStack.Push(font); // save old cfont
                                            var p = new MakeBitmapParameter { SubtitleFontName = fontFace, SubtitleFontSize = fontSize };
                                            font = GetFont(p, p.SubtitleFontSize);
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
                                        if (lastText.Length > 0 && left > 2)
                                        {
                                            left -= 1.5f;
                                        }

                                        lastText.Append(sb);

                                        TextDraw.DrawText(font, sf, path, sb, isItalic, isBold || parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                                    }

                                    addLeft = GetLastPositionFromPath(path, oldPathPointIndex, addLeft);
                                    if (addLeft < 0.01)
                                    {
                                        addLeft = left + 2;
                                    }

                                    left = addLeft;

                                    DrawShadowAndPath(parameter, g, path);
                                    g.FillPath(new SolidBrush(c), path);
                                    path.Reset();
                                    sb.Clear();
                                    if (colorStack.Count > 0)
                                    {
                                        c = colorStack.Pop();
                                    }

                                    if (left >= 3)
                                    {
                                        left -= 2.5f;
                                    }
                                }
                                if (fontStack.Count > 0)
                                {
                                    font.Dispose();
                                    font = fontStack.Pop();
                                }
                                i += 6;
                            }
                            else if (text.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                            {
                                if (sb.Length > 0)
                                {
                                    float addLeft = 0;
                                    int oldPathPointIndex = path.PointCount - 1;
                                    if (oldPathPointIndex < 0)
                                    {
                                        oldPathPointIndex = 0;
                                    }

                                    lastText.Append(sb);
                                    TextDraw.DrawText(font, sf, path, sb, isItalic, isBold || parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);

                                    addLeft = GetLastPositionFromPath(path, oldPathPointIndex, addLeft);
                                    if (addLeft < 0.01)
                                    {
                                        addLeft = left + 2;
                                    }

                                    left = addLeft;
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

                                if (sb.Length > 0)
                                {
                                    float addLeft = 0;
                                    int oldPathPointIndex = path.PointCount - 1;
                                    if (oldPathPointIndex < 0)
                                    {
                                        oldPathPointIndex = 0;
                                    }

                                    if (sb.Length > 0)
                                    {
                                        if (lastText.Length > 0 && left > 2)
                                        {
                                            left -= 1.5f;
                                        }

                                        lastText.Append(sb);
                                        TextDraw.DrawText(font, sf, path, sb, isItalic, isBold || parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                                    }

                                    addLeft = GetLastPositionFromPath(path, oldPathPointIndex, addLeft);
                                    if (addLeft < 0.01)
                                    {
                                        addLeft = left + 2;
                                    }

                                    left = addLeft;
                                }

                                isItalic = false;
                                i += 3;
                            }
                            else if (text.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase) && !isItalic)
                            {
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

                                if (sb.Length > 0)
                                {
                                    float addLeft = 0;
                                    int oldPathPointIndex = path.PointCount - 1;
                                    if (oldPathPointIndex < 0)
                                    {
                                        oldPathPointIndex = 0;
                                    }

                                    if (sb.Length > 0)
                                    {
                                        if (lastText.Length > 0 && left > 2)
                                        {
                                            left -= 1.5f;
                                        }

                                        lastText.Append(sb);
                                        TextDraw.DrawText(font, sf, path, sb, isItalic, isBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                                    }

                                    addLeft = GetLastPositionFromPath(path, oldPathPointIndex, addLeft);
                                    if (addLeft < 0.01)
                                    {
                                        addLeft = left + 2;
                                    }

                                    left = addLeft;
                                }

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
                                {
                                    lineHeight = parameter.LineHeight[style];
                                }

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
                        }
                        if (sb.Length > 0)
                        {
                            TextDraw.DrawText(font, sf, path, sb, isItalic, isBold || parameter.SubtitleFontBold, false, left, top, ref newLine, leftMargin, ref newLinePathPoint);
                        }

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
                    nbmp.CropSidesAndBottom(_boxBorderSize, parameter.BackgroundColor, true);
                    nbmp.CropTop(_boxBorderSize, parameter.BackgroundColor);
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
                    {
                        nbmp.CropTransparentSidesAndBottom(2, true);
                    }
                    else
                    {
                        nbmp.CropSidesAndBottom(_boxBorderSize, parameter.BackgroundColor, true);
                    }
                }
                else if (parameter.Type3D == 2) // Half-Top/Bottom 3D
                {
                    nbmp = Make3DTopBottom(parameter, nbmp);
                }
                return nbmp.GetBitmap();
            }
            finally
            {
                font?.Dispose();
                bmp?.Dispose();
            }
        }

        private static float GetLastPositionFromPath(GraphicsPath path, int oldPathPointIndex, float addLeft)
        {
            if (path.PointCount > 0)
            {
                var list = (PointF[])path.PathPoints.Clone(); // avoid using very slow path.PathPoints indexer!!!
                for (int k = oldPathPointIndex + 1; k < list.Length; k++)
                {
                    if (list[k].X > addLeft)
                    {
                        addLeft = list[k].X;
                    }
                }
            }
            return addLeft;
        }

        private static Point? GetAssPoint(string s)
        {
            int k = s.IndexOf("{\\", StringComparison.Ordinal);
            while (k >= 0)
            {
                int l = s.IndexOf('}', k + 1);
                if (l < k)
                {
                    break;
                }

                var assTags = s.Substring(k + 1, l - k - 1).Split('\\');
                foreach (var assTag in assTags)
                {
                    if (assTag.StartsWith("pos(", StringComparison.Ordinal))
                    {
                        var numbers = assTag.Remove(0, 4).TrimEnd(')').Trim().Split(',');
                        if (numbers.Length == 2 && Utilities.IsInteger(numbers[0]) && Utilities.IsInteger(numbers[1]))
                        {
                            return new Point(int.Parse(numbers[0]), int.Parse(numbers[1]));
                        }
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
                {
                    break;
                }

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
                nbmp.CropTop(_boxBorderSize, parameter.BackgroundColor);
                nbmp.CropSidesAndBottom(_boxBorderSize, parameter.BackgroundColor, false);
            }
            return nbmp;
        }

        private static void DrawShadowAndPath(MakeBitmapParameter parameter, Graphics g, GraphicsPath path)
        {
            if (parameter.ShadowWidth > 0)
            {
                var shadowAlpha = parameter.ShadowAlpha;
                if (parameter.ShadowWidth > 1)
                {
                    shadowAlpha = (int)Math.Round(shadowAlpha * 0.8);
                }

                var shadowPath = (GraphicsPath)path.Clone();
                for (int k = 0; k < parameter.ShadowWidth; k++)
                {
                    var translateMatrix = new Matrix();
                    translateMatrix.Translate(1, 1);
                    shadowPath.Transform(translateMatrix);

                    using (var p1 = new Pen(new SolidBrush(Color.FromArgb(shadowAlpha, parameter.ShadowColor)), parameter.BorderWidth))
                    {
                        SetLineJoin(parameter.LineJoin, p1);
                        g.DrawPath(p1, shadowPath);
                    }
                }
            }

            if (parameter.BorderWidth > 0)
            {
                var p1 = new Pen(parameter.BorderColor, (float)(parameter.BorderWidth * 1.1));
                SetLineJoin(parameter.LineJoin, p1);
                g.DrawPath(p1, path);
                p1.Dispose();
            }
        }

        private static void SetLineJoin(string lineJoin, Pen pen)
        {
            if (string.IsNullOrWhiteSpace(lineJoin))
            {
                return;
            }

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

        private bool _allowCustomBottomMargin;

        internal void Initialize(Subtitle subtitle, SubtitleFormat format, string exportType, string fileName, VideoInfo videoInfo, string videoFileName)
        {
            checkBoxFullFrameImage.Checked = false;
            checkBoxFullFrameImage.Visible = false;
            _exportType = exportType;
            _fileName = fileName;
            _format = format;
            _formatName = _format != null ? _format.Name : string.Empty;
            if (_formatName == AdvancedSubStationAlpha.NameOfFormat || _formatName == SubStationAlpha.NameOfFormat)
            {
                CalculateHeights(subtitle);
            }
            _videoFileName = videoFileName;
            if (exportType == ExportFormats.BluraySup)
            {
                Text = "Blu-ray SUP";
            }
            else if (exportType == ExportFormats.VobSub)
            {
                Text = "VobSub (sub/idx)";
            }
            else if (exportType == ExportFormats.Fab)
            {
                Text = "FAB Image Script";
            }
            else if (exportType == ExportFormats.ImageFrame)
            {
                Text = "Image per frame";
            }
            else if (exportType == ExportFormats.Stl)
            {
                Text = "DVD Studio Pro STL";
            }
            else if (exportType == ExportFormats.Fcp)
            {
                Text = "Final Cut Pro";
            }
            else if (exportType == ExportFormats.Dost)
            {
                Text = ExportFormats.Dost;
            }
            else if (exportType == ExportFormats.Edl)
            {
                Text = ExportFormats.Edl;
            }
            else if (_exportType == ExportFormats.EdlClipName)
            {
                Text = "EDL/CLIPNAME";
            }
            else if (exportType == ExportFormats.DCinemaInterop)
            {
                Text = "DCinema interop/png";
            }
            else if (exportType == ExportFormats.DCinemaSmpte2014)
            {
                Text = "DCinema SMPTE 2014/png";
            }
            else if (exportType == ExportFormats.Spumux)
            {
                Text = ExportFormats.Spumux;
            }
            else
            {
                Text = LanguageSettings.Current.ExportPngXml.Title;
            }

            if (_exportType == ExportFormats.VobSub && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportVobSubFontName))
            {
                _subtitleFontName = Configuration.Settings.Tools.ExportVobSubFontName;
            }
            else if ((_exportType == ExportFormats.BluraySup || _exportType == ExportFormats.Dost) && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportBluRayFontName))
            {
                _subtitleFontName = Configuration.Settings.Tools.ExportBluRayFontName;
            }
            else if (_exportType == ExportFormats.Fcp && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportFcpFontName))
            {
                _subtitleFontName = Configuration.Settings.Tools.ExportFcpFontName;
            }
            else if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ExportFontNameOther))
            {
                _subtitleFontName = Configuration.Settings.Tools.ExportFontNameOther;
            }

            if (_exportType == ExportFormats.VobSub && Configuration.Settings.Tools.ExportVobSubFontSize > 0)
            {
                _subtitleFontSize = Configuration.Settings.Tools.ExportVobSubFontSize;
            }
            else if ((_exportType == ExportFormats.BluraySup || _exportType == ExportFormats.Dost) && Configuration.Settings.Tools.ExportBluRayFontSize > 0)
            {
                _subtitleFontSize = Configuration.Settings.Tools.ExportBluRayFontSize;
            }
            else if (_exportType == ExportFormats.Fcp && Configuration.Settings.Tools.ExportFcpFontSize > 0)
            {
                _subtitleFontSize = Configuration.Settings.Tools.ExportFcpFontSize;
            }
            else if (Configuration.Settings.Tools.ExportLastFontSize > 0)
            {
                _subtitleFontSize = Configuration.Settings.Tools.ExportLastFontSize;
            }

            if (_exportType == ExportFormats.Fcp)
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

            if (_exportType == ExportFormats.VobSub)
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
                checkBoxTransAntiAliase.Checked = Configuration.Settings.Tools.ExportVobAntiAliasingWithTransparency;
            }
            else if (_exportType == ExportFormats.BluraySup || _exportType == ExportFormats.Dost || _exportType == ExportFormats.Fcp)
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

            groupBoxImageSettings.Text = LanguageSettings.Current.ExportPngXml.ImageSettings;
            labelSubtitleFont.Text = LanguageSettings.Current.ExportPngXml.FontFamily;
            labelSubtitleFontSize.Text = LanguageSettings.Current.ExportPngXml.FontSize;
            labelResolution.Text = LanguageSettings.Current.ExportPngXml.VideoResolution;
            buttonColor.Text = LanguageSettings.Current.ExportPngXml.FontColor;
            checkBoxBold.Text = LanguageSettings.Current.General.Bold;
            checkBoxSimpleRender.Text = LanguageSettings.Current.ExportPngXml.SimpleRendering;
            checkBoxTransAntiAliase.Text = LanguageSettings.Current.ExportPngXml.AntiAliasingWithTransparency;
            labelResize.Text = LanguageSettings.Current.General.Size;
            normalToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.RemoveFormattingAll;
            italicToolStripMenuItem.Text = LanguageSettings.Current.General.Italic;
            boxSingleLineToolStripMenuItem.Text = LanguageSettings.Current.ExportPngXml.BoxSingleLine;
            boxMultiLineToolStripMenuItem.Text = LanguageSettings.Current.ExportPngXml.BoxMultiLine;
            adjustTimeCodesToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.Synchronization.AdjustAllTimes;
            adjustDisplayTimeToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.Tools.AdjustDisplayDuration;

            checkBoxFcpFullPathUrl.Text = LanguageSettings.Current.ExportPngXml.FcpUseFullPathUrl;
            checkBoxFcpFullPathUrl.Visible = exportType == ExportFormats.Fcp;
            checkBoxFcpFullPathUrl.Checked = Configuration.Settings.Tools.ExportFcpFullPathUrl;


            comboBox3D.Items.Clear();
            comboBox3D.Items.Add(LanguageSettings.Current.General.None);
            comboBox3D.Items.Add(LanguageSettings.Current.ExportPngXml.SideBySide3D);
            comboBox3D.Items.Add(LanguageSettings.Current.ExportPngXml.HalfTopBottom3D);
            comboBox3D.SelectedIndex = 0;

            labelDepth.Text = LanguageSettings.Current.ExportPngXml.Depth;

            numericUpDownDepth3D.Left = labelDepth.Left + labelDepth.Width + 3;

            label3D.Text = LanguageSettings.Current.ExportPngXml.Text3D;

            comboBox3D.Left = label3D.Left + label3D.Width + 3;

            buttonBorderColor.Text = LanguageSettings.Current.ExportPngXml.BorderColor;
            labelBorderWidth.Text = LanguageSettings.Current.ExportPngXml.BorderStyle;
            labelImageFormat.Text = LanguageSettings.Current.ExportPngXml.ImageFormat;
            checkBoxFullFrameImage.Text = LanguageSettings.Current.ExportPngXml.FullFrameImage;

            buttonExport.Text = LanguageSettings.Current.ExportPngXml.ExportAllLines;
            buttonCancel.Text = LanguageSettings.Current.General.Ok;
            labelLanguage.Text = LanguageSettings.Current.ChooseLanguage.Language;
            labelFrameRate.Text = LanguageSettings.Current.General.FrameRate;
            labelHorizontalAlign.Text = LanguageSettings.Current.ExportPngXml.Align;
            labelBottomMargin.Text = LanguageSettings.Current.ExportPngXml.BottomMargin;
            labelLeftRightMargin.Text = LanguageSettings.Current.ExportPngXml.LeftRightMargin;

            comboBoxHAlign.Items.Clear();
            comboBoxHAlign.Items.Add(LanguageSettings.Current.ExportPngXml.Left);
            comboBoxHAlign.Items.Add(LanguageSettings.Current.ExportPngXml.Center);
            comboBoxHAlign.Items.Add(LanguageSettings.Current.ExportPngXml.Right);
            comboBoxHAlign.Items.Add(LanguageSettings.Current.ExportPngXml.CenterLeftJustify);
            comboBoxHAlign.Items.Add(LanguageSettings.Current.ExportPngXml.CenterTopJustify);
            comboBoxHAlign.Items.Add(LanguageSettings.Current.ExportPngXml.CenterRightJustify);
            comboBoxHAlign.Items.Add(LanguageSettings.Current.ExportPngXml.CenterLeftJustifyDialogs);

            buttonShadowColor.Text = LanguageSettings.Current.ExportPngXml.ShadowColor;
            labelShadowWidth.Text = LanguageSettings.Current.ExportPngXml.ShadowWidth;
            labelShadowTransparency.Text = LanguageSettings.Current.ExportPngXml.Transparency;
            labelLineHeight.Text = LanguageSettings.Current.ExportPngXml.LineHeight;

            linkLabelPreview.Text = LanguageSettings.Current.General.Preview;
            linkLabelPreview.Left = groupBoxExportImage.Width - linkLabelPreview.Width - 3;

            saveImageAsToolStripMenuItem.Text = LanguageSettings.Current.ExportPngXml.SaveImageAs;

            SubtitleListView1InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(subtitleListView1);
            SubtitleListView1AutoSizeAllColumns();

            _subtitle = new Subtitle(subtitle);

            panelColor.BackColor = _subtitleColor;
            panelBorderColor.BackColor = _borderColor;
            InitBorderStyle();
            comboBoxHAlign.SelectedIndex = 1;
            comboBoxResolution.SelectedIndex = 3;

            if (Configuration.Settings.Tools.ExportLastShadowTransparency <= numericUpDownShadowTransparency.Maximum && Configuration.Settings.Tools.ExportLastShadowTransparency > 0)
            {
                numericUpDownShadowTransparency.Value = Configuration.Settings.Tools.ExportLastShadowTransparency;
            }

            if ((_exportType == ExportFormats.BluraySup || _exportType == ExportFormats.Dost) && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportBluRayVideoResolution))
            {
                SetResolution(Configuration.Settings.Tools.ExportBluRayVideoResolution);
            }

            _language = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle);
            if (exportType == ExportFormats.VobSub)
            {
                comboBoxBorderWidth.SelectedIndex = 6;
                if (_exportType == ExportFormats.VobSub && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportVobSubVideoResolution))
                {
                    SetResolution(Configuration.Settings.Tools.ExportVobSubVideoResolution);
                }
                else
                {
                    comboBoxResolution.SelectedIndex = 8;
                }

                labelLanguage.Visible = true;
                comboBoxLanguage.Visible = true;
                comboBoxLanguage.Items.Clear();
                if (_language == null)
                {
                    _language = Configuration.Settings.Tools.ExportVobSubLanguage;
                }

                int index = -1;
                foreach (var language in DvdSubtitleLanguage.CompliantLanguages)
                {
                    int i = comboBoxLanguage.Items.Add(language);
                    if (language.Code == _language || index < 0 && language.Code == "en")
                    {
                        index = i;
                    }
                }
                comboBoxLanguage.SelectedIndex = index;
            }

            bool showImageFormat = exportType == ExportFormats.Fab || exportType == ExportFormats.ImageFrame || exportType == ExportFormats.Stl || exportType == ExportFormats.Fcp || exportType == ExportFormats.BdnXml;
            if (exportType == ExportFormats.Fab || exportType == ExportFormats.BluraySup || exportType == ExportFormats.Fcp)
            {
                checkBoxFullFrameImage.Visible = exportType == ExportFormats.Fab || exportType == ExportFormats.BluraySup || exportType == ExportFormats.Fcp;
            }
            else
            {
                checkBoxFullFrameImage.Checked = false;
            }

            comboBoxImageFormat.Visible = showImageFormat;
            labelImageFormat.Visible = showImageFormat;
            labelFrameRate.Visible = exportType == ExportFormats.BdnXml || exportType == ExportFormats.BluraySup || exportType == ExportFormats.Dost || exportType == ExportFormats.ImageFrame;
            comboBoxFrameRate.Visible = exportType == ExportFormats.BdnXml || exportType == ExportFormats.BluraySup || exportType == ExportFormats.Dost || exportType == ExportFormats.ImageFrame || exportType == ExportFormats.Fab || exportType == ExportFormats.Fcp;
            checkBoxTransAntiAliase.Visible = exportType == ExportFormats.VobSub;
            if (exportType == ExportFormats.BdnXml)
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

                comboBoxImageFormat.Items.Clear();
                comboBoxImageFormat.Items.Add("Png 32-bit");
                comboBoxImageFormat.Items.Add("Png 8-bit");
                if (comboBoxImageFormat.Items[1].ToString() == Configuration.Settings.Tools.ExportBdnXmlImageType)
                {
                    comboBoxImageFormat.SelectedIndex = 1;
                }
                else
                {
                    comboBoxImageFormat.SelectedIndex = 0;
                }
            }
            else if (exportType == ExportFormats.Dost)
            {
                labelFrameRate.Top = labelLanguage.Top;
                comboBoxFrameRate.Top = comboBoxLanguage.Top;
                comboBoxFrameRate.Items.Add("23.98");
                comboBoxFrameRate.Items.Add("24");
                comboBoxFrameRate.Items.Add("25");
                comboBoxFrameRate.Items.Add("29.97");
                comboBoxFrameRate.Items.Add("30");
                comboBoxFrameRate.Items.Add("59.94");
                comboBoxFrameRate.Items.Add("60");
                comboBoxFrameRate.SelectedIndex = 2;
            }
            else if (exportType == ExportFormats.ImageFrame)
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
            else if (exportType == ExportFormats.BluraySup)
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
                comboBoxFrameRate.SelectedIndex = 1;
                comboBoxFrameRate.DropDownStyle = ComboBoxStyle.DropDownList;

                checkBoxFullFrameImage.Top = comboBoxImageFormat.Top + 2;
                panelFullFrameBackground.Top = checkBoxFullFrameImage.Top;
            }
            else if (exportType == ExportFormats.Fab)
            {
                labelFrameRate.Visible = true;
                comboBoxFrameRate.Items.Add("23.976");
                comboBoxFrameRate.Items.Add("24");
                comboBoxFrameRate.Items.Add("25");
                comboBoxFrameRate.Items.Add("29.97");
                comboBoxFrameRate.Items.Add("30");
                comboBoxFrameRate.Items.Add("50");
                comboBoxFrameRate.Items.Add("59.94");
                comboBoxFrameRate.Items.Add("60");
                comboBoxFrameRate.SelectedIndex = 1;
                comboBoxFrameRate.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            else if (exportType == ExportFormats.Fcp)
            {
                labelFrameRate.Visible = true;
                comboBoxFrameRate.Items.Add("23.976");
                comboBoxFrameRate.Items.Add("24");
                comboBoxFrameRate.Items.Add("25");
                comboBoxFrameRate.Items.Add("29.97");
                comboBoxFrameRate.Items.Add("30");
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

            comboBoxBottomMarginUnit.SelectedIndex = Configuration.Settings.Tools.ExportBottomMarginUnit == "%" ? 0 : 1;
            comboBoxLeftRightMarginUnit.SelectedIndex = Configuration.Settings.Tools.ExportLeftRightMarginUnit == "%" ? 0 : 1;

            _allowCustomBottomMargin = _exportType == ExportFormats.BluraySup || _exportType == ExportFormats.VobSub || _exportType == ExportFormats.ImageFrame || _exportType == ExportFormats.BdnXml || _exportType == ExportFormats.Dost || _exportType == ExportFormats.Fab || _exportType == ExportFormats.Edl || _exportType == ExportFormats.EdlClipName;
            if (_allowCustomBottomMargin)
            {
                comboBoxBottomMarginUnit.Visible = true;
                comboBoxBottomMargin.Visible = true;
                labelBottomMargin.Visible = true;

                comboBoxLeftRightMarginUnit.Visible = true;
                comboBoxLeftRightMargin.Visible = true;
                labelLeftRightMargin.Visible = true;
                comboBoxBottomMarginUnit_SelectedIndexChanged(null, null);
            }
            else
            {
                comboBoxBottomMarginUnit.Visible = false;
                comboBoxBottomMargin.Visible = false;
                labelBottomMargin.Visible = false;

                comboBoxLeftRightMarginUnit.Visible = false;
                comboBoxLeftRightMargin.Visible = false;
                labelLeftRightMargin.Visible = false;
            }

            checkBoxSkipEmptyFrameAtStart.Visible = exportType == ExportFormats.ImageFrame;

            foreach (var x in FontFamily.Families)
            {
                if (x.IsStyleAvailable(FontStyle.Regular) || x.IsStyleAvailable(FontStyle.Bold))
                {
                    comboBoxSubtitleFont.Items.Add(x.Name);
                    if (x.Name.Equals(_subtitleFontName, StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
                    }
                }
            }
            if (comboBoxSubtitleFont.SelectedIndex == -1)
            {
                comboBoxSubtitleFont.SelectedIndex = 0; // take first font if default font was not found (e.g. linux)
            }

            if (videoInfo != null && videoInfo.Height > 0 && videoInfo.Width > 0)
            {
                comboBoxResolution.Items[comboBoxResolution.Items.Count - 1] = videoInfo.Width + "x" + videoInfo.Height;
                comboBoxResolution.SelectedIndex = comboBoxResolution.Items.Count - 1;
            }

            if (_lineHeights != null && _lineHeights.Count > 0)
            {
                numericUpDownLineSpacing.Value = _lineHeights.First().Value;
            }
            else if (Math.Abs(_subtitleFontSize - Configuration.Settings.Tools.ExportLastFontSize) < 0.01 && Configuration.Settings.Tools.ExportLastLineHeight >= numericUpDownLineSpacing.Minimum &&
                Configuration.Settings.Tools.ExportLastLineHeight <= numericUpDownLineSpacing.Maximum && Configuration.Settings.Tools.ExportLastLineHeight > 0)
            {
                numericUpDownLineSpacing.Value = Configuration.Settings.Tools.ExportLastLineHeight;
            }
            else
            {
                var lineHeight = (int)Math.Round(GetFontHeight() * 0.64f);
                if (lineHeight >= numericUpDownLineSpacing.Minimum &&
                    lineHeight <= numericUpDownLineSpacing.Maximum)
                {
                    numericUpDownLineSpacing.Value = lineHeight;
                }
            }

            if (Configuration.Settings.Tools.ExportLastBorderWidth >= 0 && Configuration.Settings.Tools.ExportLastBorderWidth < comboBoxBorderWidth.Items.Count)
            {
                try
                {
                    comboBoxBorderWidth.SelectedIndex = Configuration.Settings.Tools.ExportLastBorderWidth;
                }
                catch
                {
                    // ignore error
                }
            }
            _borderWidth = GetBorderWidth();
            checkBoxBold.Checked = Configuration.Settings.Tools.ExportLastFontBold;
            _subtitleFontBold = Configuration.Settings.Tools.ExportLastFontBold;

            if (Configuration.Settings.Tools.Export3DType >= 0 && Configuration.Settings.Tools.Export3DType < comboBox3D.Items.Count)
            {
                comboBox3D.SelectedIndex = Configuration.Settings.Tools.Export3DType;
            }

            if (Configuration.Settings.Tools.Export3DDepth >= numericUpDownDepth3D.Minimum && Configuration.Settings.Tools.Export3DDepth <= numericUpDownDepth3D.Maximum)
            {
                numericUpDownDepth3D.Value = Configuration.Settings.Tools.Export3DDepth;
            }

            if (Configuration.Settings.Tools.ExportHorizontalAlignment >= 0 && Configuration.Settings.Tools.ExportHorizontalAlignment < comboBoxHAlign.Items.Count)
            {
                comboBoxHAlign.SelectedIndex = Configuration.Settings.Tools.ExportHorizontalAlignment;
            }

            if (exportType == ExportFormats.DCinemaInterop || exportType == ExportFormats.DCinemaSmpte2014)
            {
                comboBox3D.Visible = false;
                numericUpDownDepth3D.Enabled = true;
                labelDepth.Enabled = true;
                labelDepth.Text = LanguageSettings.Current.DCinemaProperties.ZPosition;
            }

            if (_exportType == ExportFormats.Fcp)
            {
                comboBoxResolution.Items.Clear();
                comboBoxResolution.Items.Add("NTSC-601");
                comboBoxResolution.Items.Add("PAL-601");
                comboBoxResolution.Items.Add("square");
                comboBoxResolution.Items.Add("DVCPROHD-720P");
                comboBoxResolution.Items.Add("HD-(960x720)");
                comboBoxResolution.Items.Add("DVCPROHD-1080i60");
                comboBoxResolution.Items.Add("HD-(1280x1080)");
                comboBoxResolution.Items.Add("FullHD 1920x1080");
                comboBoxResolution.Items.Add("DVCPROHD-1080i50");
                comboBoxResolution.Items.Add("HD-(1440x1080)");
                comboBoxResolution.SelectedIndex = 7; // FullHD
                if (_exportType == ExportFormats.Fcp && !string.IsNullOrEmpty(Configuration.Settings.Tools.ExportFcpVideoResolution))
                {
                    SetResolution(Configuration.Settings.Tools.ExportFcpVideoResolution);
                }

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
            bool shadowVisible = _exportType == ExportFormats.BdnXml || _exportType == ExportFormats.BluraySup || _exportType == ExportFormats.Dost || _exportType == ExportFormats.ImageFrame || _exportType == ExportFormats.Fcp || _exportType == ExportFormats.DCinemaInterop || exportType == ExportFormats.DCinemaSmpte2014 || _exportType == ExportFormats.Edl || _exportType == ExportFormats.EdlClipName;
            labelShadowWidth.Visible = shadowVisible;
            buttonShadowColor.Visible = shadowVisible;
            comboBoxShadowWidth.Visible = shadowVisible;
            if (shadowVisible && Configuration.Settings.Tools.ExportBluRayShadow < comboBoxShadowWidth.Items.Count)
            {
                comboBoxShadowWidth.SelectedIndex = Configuration.Settings.Tools.ExportBluRayShadow;
            }

            panelShadowColor.Visible = shadowVisible;
            labelShadowTransparency.Visible = shadowVisible;
            numericUpDownShadowTransparency.Visible = shadowVisible;


            if (checkBoxSimpleRender.Enabled)
            {
                checkBoxSimpleRender.Checked = Configuration.Settings.Tools.ExportVobSubSimpleRendering || _language == "ar";
            }

            if (exportType == ExportFormats.BluraySup || exportType == ExportFormats.VobSub || exportType == ExportFormats.BdnXml)
            {
                subtitleListView1.CheckBoxes = true;
                subtitleListView1.Columns.Insert(0, LanguageSettings.Current.ExportPngXml.Forced);
                SubtitleListView1Fill(_subtitle);
                if (_vobSubOcr != null)
                {
                    for (int index = 0; index < _subtitle.Paragraphs.Count; index++)
                    {
                        if (_vobSubOcr.GetIsForced(index))
                        {
                            subtitleListView1.Items[index].Checked = true;
                        }
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

        private float GetFontHeight()
        {
            var mbp = new MakeBitmapParameter
            {
                SubtitleFontName = _subtitleFontName,
                SubtitleFontSize = float.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString()),
                SubtitleFontBold = _subtitleFontBold
            };
            var fontSize = (float)TextDraw.GetFontSize(mbp.SubtitleFontSize);
            using (var font = GetFont(mbp, fontSize))
            using (var bmp = new Bitmap(100, 100))
            using (var g = Graphics.FromImage(bmp))
            {
                var textSize = g.MeasureString("Hj!", font);
                return textSize.Height;
            }
        }

        private void CalculateHeights(Subtitle subtitle)
        {
            foreach (var paragraph in subtitle.Paragraphs)
            {
                var styleName = paragraph.Extra;
                if (!string.IsNullOrEmpty(paragraph.Extra) && !_lineHeights.ContainsKey(styleName))
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(paragraph.Extra, subtitle.Header);
                    using (var bmp = new Bitmap(100, 100))
                    {
                        using (var g = Graphics.FromImage(bmp))
                        {
                            var mbp = new MakeBitmapParameter
                            {
                                SubtitleFontName = style.FontName,
                                SubtitleFontSize = style.FontSize,
                                SubtitleFontBold = style.Bold
                            };
                            var fontSize = (float)TextDraw.GetFontSize(mbp.SubtitleFontSize);
                            Font font = GetFont(mbp, fontSize);
                            SizeF textSize = g.MeasureString("Hj!", font);
                            int lineHeight = (int)Math.Round(textSize.Height * 0.64f);
                            if (fontSize < 30)
                            {
                                lineHeight = (int)Math.Round(textSize.Height * 0.69f);
                            }

                            _lineHeights.Add(styleName, lineHeight);
                        }
                    }
                }
            }
        }

        private void InitBorderStyle()
        {
            comboBoxBorderWidth.Items.Clear();
            comboBoxBorderWidth.Items.Add(LanguageSettings.Current.ExportPngXml.BorderStyleBoxForEachLine);
            comboBoxBorderWidth.Items.Add(LanguageSettings.Current.ExportPngXml.BorderStyleOneBox);
            for (int i = 0; i < 16; i++)
            {
                comboBoxBorderWidth.Items.Add(string.Format(LanguageSettings.Current.ExportPngXml.BorderStyleNormalWidthX, i));
            }
            comboBoxBorderWidth.SelectedIndex = 4;
        }

        private void SetLastFrameRate(double lastFrameRate)
        {
            for (int i = 0; i < comboBoxFrameRate.Items.Count; i++)
            {
                if (double.TryParse(comboBoxFrameRate.Items[i].ToString().Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d))
                {
                    if (Math.Abs(lastFrameRate - d) < 0.01)
                    {
                        comboBoxFrameRate.SelectedIndex = i;
                        return;
                    }
                }
            }
        }

        internal void SetResolution(Point resolution)
        {
            comboBoxResolution.Items.Add(resolution.X + "x" + resolution.Y);
            comboBoxResolution.SelectedIndex = comboBoxResolution.Items.Count - 1;
        }

        internal void InitializeFromVobSubOcr(Subtitle subtitle, SubtitleFormat format, string exportType, string fileName, IBinaryParagraphList vobSubOcr, string languageString)
        {
            _vobSubOcr = vobSubOcr;
            if (_vobSubOcr != null && exportType != ExportFormats.VobSub)
            {
                comboBoxResizePercentage.Items.Clear();
                for (int i = 50; i < 400; i++)
                {
                    comboBoxResizePercentage.Items.Add(i + "%");
                }
                comboBoxResizePercentage.Items.Add("500%");
                comboBoxResizePercentage.SelectedIndex = 50;
                comboBoxResizePercentage.Visible = true;
                labelResize.Visible = true;
                labelResize.Left = buttonColor.Left;
                labelResize.Top = buttonColor.Top;
                comboBoxResizePercentage.Left = labelResize.Left + labelResize.Width + 5;
                comboBoxResizePercentage.Top = labelResize.Top - 4;
            }
            else
            {
                comboBoxResizePercentage.Visible = false;
                labelResize.Visible = false;
            }

            Initialize(subtitle, format, exportType, fileName, null, _videoFileName);

            //set language
            if (!string.IsNullOrEmpty(languageString))
            {
                if (languageString.Contains('(') && languageString[0] != '(')
                {
                    languageString = languageString.Substring(0, languageString.IndexOf('(') - 1).Trim();
                }

                for (int i = 0; i < comboBoxLanguage.Items.Count; i++)
                {
                    string l = comboBoxLanguage.Items[i].ToString();
                    if (l == languageString)
                    {
                        comboBoxLanguage.SelectedIndex = i;
                        break;
                    }
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
            if (_isLoading)
            {
                return;
            }

            _previewTimer.Stop();
            UpdateLineSpacing();
            _previewTimer.Start();
        }

        internal int GetBottomMarginInPixels(Paragraph p)
        {
            if (!_allowCustomBottomMargin)
            {
                return 20;
            }

            if (!string.IsNullOrEmpty(p?.Extra) && (_formatName == AdvancedSubStationAlpha.NameOfFormat || _formatName == SubStationAlpha.NameOfFormat))
            {
                var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, _subtitle.Header);
                return style.MarginVertical;
            }

            var s = comboBoxBottomMargin.Text;
            if (comboBoxBottomMarginUnit.SelectedIndex == 0) // %
            {
                GetResolution(out _, out var height);
                return (int)Math.Round(int.Parse(s.TrimEnd('%')) / 100.0 * height);
            }

            // pixels
            return int.Parse(s);
        }

        private int GetLeftMarginInPixels(Paragraph p)
        {
            if (!string.IsNullOrEmpty(p?.Extra) && (_formatName == AdvancedSubStationAlpha.NameOfFormat || _formatName == SubStationAlpha.NameOfFormat))
            {
                var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, _subtitle.Header);
                return style.MarginLeft;
            }

            var s = comboBoxLeftRightMargin.Text;
            if (comboBoxLeftRightMarginUnit.SelectedIndex == 0) // %
            {
                GetResolution(out var width, out _);
                return (int)Math.Round(int.Parse(s) / 100.0 * width);
            }

            // pixels
            return int.Parse(s);
        }

        private int GetRightMarginInPixels(Paragraph p)
        {
            if (!string.IsNullOrEmpty(p?.Extra) && (_formatName == AdvancedSubStationAlpha.NameOfFormat || _formatName == SubStationAlpha.NameOfFormat))
            {
                var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, _subtitle.Header);
                return style.MarginRight;
            }
            return GetLeftMarginInPixels(p);
        }



        private void GeneratePreview()
        {
            SetupImageParameters();
            if (subtitleListView1.SelectedItems.Count > 0)
            {
                var p = _subtitle.GetParagraphOrDefault(subtitleListView1.SelectedItems[0].Index);
                if (p == null)
                {
                    return;
                }
                var bmp = GenerateImageFromTextWithStyle(p, out var mbp);
                if (checkBoxFullFrameImage.Checked)
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
                pictureBox1.Top = groupBoxExportImage.Height - bmp.Height - GetBottomMarginInPixels(p);
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
                        {
                            pictureBox1.Left = GetLeftMarginInPixels(p);
                        }
                        else if (alignment == ContentAlignment.BottomRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.TopRight)
                        {
                            pictureBox1.Left = w - bmp.Width - GetRightMarginInPixels(p);
                        }
                    }

                    if (alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.MiddleCenter || alignment == ContentAlignment.MiddleRight)
                    {
                        pictureBox1.Top = (groupBoxExportImage.Height - 4 - bmp.Height) / 2;
                    }
                    else if (_allowCustomBottomMargin && alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.TopRight)
                    {
                        pictureBox1.Top = GetBottomMarginInPixels(p);
                    }
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
                groupBoxExportImage.Text = $"{bmp.Width}x{bmp.Height}";
                if (!string.IsNullOrEmpty(mbp.Error))
                {
                    groupBoxExportImage.BackColor = Color.Red;
                    groupBoxExportImage.Text = groupBoxExportImage.Text + " - " + mbp.Error;
                }
                else
                {
                    if (checkBoxFullFrameImage.Checked)
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
            bool showAlpha = _exportType == ExportFormats.Fab || _exportType == ExportFormats.BdnXml;
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
            if (_isLoading)
            {
                return;
            }

            if (_formatName != AdvancedSubStationAlpha.NameOfFormat && _formatName != SubStationAlpha.NameOfFormat &&
                comboBoxSubtitleFontSize.Enabled)
            {
                var lineHeight = (int)Math.Round(GetFontHeight() * 0.64f);
                if (lineHeight >= numericUpDownLineSpacing.Minimum &&
                    lineHeight <= numericUpDownLineSpacing.Maximum)
                {
                    numericUpDownLineSpacing.Value = lineHeight;
                }
            }
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void UpdateLineSpacing()
        {
            var style = string.Empty;
            if (subtitleListView1.SelectedIndices.Count > 0)
            {
                style = GetStyleName(_subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index]);
                if (!string.IsNullOrEmpty(style) && _lineHeights.ContainsKey(style))
                {
                    numericUpDownLineSpacing.Value = _lineHeights[style];
                }
            }
            labelLineHeightStyle.Text = style;
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
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#export");
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
            ExportPngXml_ResizeEnd(sender, e);
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
            subtitleListView1.AutoSizeLastColumn();
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
            subtitleListView1.AutoSizeLastColumn();
        }

        private void ExportPngXml_FormClosing(object sender, FormClosingEventArgs e)
        {
            GetResolution(out var width, out var height);
            string res = $"{width}x{height}";

            if (_exportType == ExportFormats.VobSub)
            {
                Configuration.Settings.Tools.ExportVobSubFontName = _subtitleFontName;
                Configuration.Settings.Tools.ExportVobSubFontSize = (int)_subtitleFontSize;
                Configuration.Settings.Tools.ExportVobSubVideoResolution = res;
                Configuration.Settings.Tools.ExportVobSubLanguage = comboBoxLanguage.Text;
                Configuration.Settings.Tools.ExportVobAntiAliasingWithTransparency = checkBoxTransAntiAliase.Checked;
            }
            else if (_exportType == ExportFormats.BluraySup || _exportType == ExportFormats.Dost)
            {
                Configuration.Settings.Tools.ExportBluRayFontName = _subtitleFontName;
                Configuration.Settings.Tools.ExportBluRayFontSize = (int)_subtitleFontSize;
                Configuration.Settings.Tools.ExportBluRayVideoResolution = res;
            }
            else if (_exportType == ExportFormats.BdnXml || _exportType == ExportFormats.Fcp)
            {
                Configuration.Settings.Tools.ExportBdnXmlImageType = comboBoxImageFormat.SelectedItem.ToString();
            }
            if (_exportType == ExportFormats.Fcp)
            {
                Configuration.Settings.Tools.ExportFcpFontName = _subtitleFontName;
                Configuration.Settings.Tools.ExportFcpFontSize = (int)_subtitleFontSize;
                if (comboBoxImageFormat.SelectedItem != null)
                {
                    Configuration.Settings.Tools.ExportFcpImageType = comboBoxImageFormat.SelectedItem.ToString();
                }

                Configuration.Settings.Tools.ExportFcpVideoResolution = res;
                Configuration.Settings.Tools.ExportFcpPalNtsc = comboBoxLanguage.SelectedIndex == 0 ? "PAL" : "NTSC";
            }
            Configuration.Settings.Tools.ExportFcpFullPathUrl = checkBoxFcpFullPathUrl.Checked;
            Configuration.Settings.Tools.ExportLastShadowTransparency = (int)numericUpDownShadowTransparency.Value;
            Configuration.Settings.Tools.ExportLastFrameRate = FrameRate;
            Configuration.Settings.Tools.ExportFullFrame = checkBoxFullFrameImage.Checked;
            Configuration.Settings.Tools.ExportShadowColor = panelShadowColor.BackColor;
            Configuration.Settings.Tools.ExportFontColor = _subtitleColor;
            Configuration.Settings.Tools.ExportBorderColor = _borderColor;

            if (checkBoxSimpleRender.Enabled)
            {
                Configuration.Settings.Tools.ExportVobSubSimpleRendering = checkBoxSimpleRender.Checked;
            }

            if (_exportType == ExportFormats.BluraySup)
            {
                if (comboBoxBottomMarginUnit.SelectedIndex == 0) // %
                {
                    Configuration.Settings.Tools.ExportBluRayBottomMarginPercent = comboBoxBottomMargin.SelectedIndex;
                }
                else // pixels
                {
                    Configuration.Settings.Tools.ExportBluRayBottomMarginPixels = comboBoxBottomMargin.SelectedIndex;
                }
            }
            else if (_allowCustomBottomMargin)
            {
                if (comboBoxBottomMarginUnit.SelectedIndex == 0) // %
                {
                    Configuration.Settings.Tools.ExportBottomMarginPercent = comboBoxBottomMargin.SelectedIndex;
                }
                else // pixels
                {
                    Configuration.Settings.Tools.ExportBottomMarginPixels = comboBoxBottomMargin.SelectedIndex;
                }
            }

            if (comboBoxLeftRightMargin.Visible)
            {
                if (comboBoxLeftRightMarginUnit.SelectedIndex == 0) // %
                {
                    Configuration.Settings.Tools.ExportLeftRightMarginPercent = comboBoxLeftRightMargin.SelectedIndex;
                }
                else // pixels
                {
                    Configuration.Settings.Tools.ExportLeftRightMarginPixels = comboBoxLeftRightMargin.SelectedIndex;
                }
            }

            if (comboBoxBottomMarginUnit.Visible)
            {
                Configuration.Settings.Tools.ExportBottomMarginUnit = comboBoxBottomMarginUnit.SelectedIndex == 0 ? "%" : "px";
            }

            if (comboBoxLeftRightMarginUnit.Visible)
            {
                Configuration.Settings.Tools.ExportLeftRightMarginUnit = comboBoxLeftRightMarginUnit.SelectedIndex == 0 ? "%" : "px";
            }

            Configuration.Settings.Tools.ExportHorizontalAlignment = comboBoxHAlign.SelectedIndex;
            Configuration.Settings.Tools.Export3DType = comboBox3D.SelectedIndex;
            Configuration.Settings.Tools.Export3DDepth = (int)numericUpDownDepth3D.Value;

            if (comboBoxShadowWidth.Visible)
            {
                Configuration.Settings.Tools.ExportBluRayShadow = comboBoxShadowWidth.SelectedIndex;
            }

            Configuration.Settings.Tools.ExportFontNameOther = _subtitleFontName;
            Configuration.Settings.Tools.ExportLastFontSize = (int)_subtitleFontSize;
            Configuration.Settings.Tools.ExportLastLineHeight = (int)numericUpDownLineSpacing.Value;
            Configuration.Settings.Tools.ExportLastBorderWidth = comboBoxBorderWidth.SelectedIndex;
            Configuration.Settings.Tools.ExportLastFontBold = checkBoxBold.Checked;
        }

        private void numericUpDownDepth3D_ValueChanged(object sender, EventArgs e)
        {
            if (!timerPreview.Enabled)
            {
                timerPreview.Start();
            }
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
            {
                return;
            }

            int selectedIndex = subtitleListView1.SelectedItems[0].Index;

            saveFileDialog1.Title = LanguageSettings.Current.VobSubOcr.SaveSubtitleImageAs;
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
                    {
                        bmp.Save(saveFileDialog1.FileName, ImageFormat.Png);
                    }
                    else if (saveFileDialog1.FilterIndex == 1)
                    {
                        bmp.Save(saveFileDialog1.FileName);
                    }
                    else if (saveFileDialog1.FilterIndex == 2)
                    {
                        bmp.Save(saveFileDialog1.FileName, ImageFormat.Gif);
                    }
                    else
                    {
                        bmp.Save(saveFileDialog1.FileName, ImageFormat.Tiff);
                    }
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
            if (_isLoading)
            {
                return;
            }

            if (_formatName == AdvancedSubStationAlpha.NameOfFormat || _formatName == SubStationAlpha.NameOfFormat)
            {
                return;
            }

            var lineHeight = (int)Math.Round(GetFontHeight() * 0.64f);
            if (lineHeight >= numericUpDownLineSpacing.Minimum &&
                lineHeight <= numericUpDownLineSpacing.Maximum)
            {
                numericUpDownLineSpacing.ValueChanged -= numericUpDownLineSpacing_ValueChanged;
                numericUpDownLineSpacing.Value = lineHeight;
                numericUpDownLineSpacing.ValueChanged += numericUpDownLineSpacing_ValueChanged;
            }
            subtitleListView1_SelectedIndexChanged(null, null);

            // change font
            if (!comboBoxSubtitleFont.Enabled)
            {
                return;
            }
            try
            {
                var fontName = comboBoxSubtitleFont.SelectedItem.ToString();
                int columnIndexText = 4;
                if (subtitleListView1.CheckBoxes)
                {
                    columnIndexText++;
                }

                for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                {

                    subtitleListView1.Items[i].SubItems[columnIndexText].Font = new Font(fontName, Font.Size);
                }
            }
            catch
            {
                // ignore unable to set font errors
            }
        }

        private void numericUpDownLineSpacing_ValueChanged(object sender, EventArgs e)
        {
            var value = (int)numericUpDownLineSpacing.Value;
            var style = string.Empty;
            if (subtitleListView1.SelectedIndices.Count > 0)
            {
                style = GetStyleName(_subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index]);
            }

            if (_lineHeights.ContainsKey(style))
            {
                _lineHeights[style] = value;
            }

            if (_formatName != AdvancedSubStationAlpha.NameOfFormat && _formatName != SubStationAlpha.NameOfFormat)
            {
                _lineHeights.Clear();
                _lineHeights.Add(string.Empty, value);
            }
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private static string GetStyleName(Paragraph paragraph)
        {
            if ((_formatName == AdvancedSubStationAlpha.NameOfFormat || _formatName == SubStationAlpha.NameOfFormat) && !string.IsNullOrEmpty(paragraph.Extra))
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
                {
                    indexes.Add(item.Index);
                }

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
                        if (_subtitle.Paragraphs[i].Text.StartsWith("{\\", StringComparison.Ordinal) && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                        {
                            _subtitle.Paragraphs[i].Text = string.Format("{2}<{0}>{1}</{0}>", tag, _subtitle.Paragraphs[i].Text.Remove(0, indexOfEndBracket + 1), _subtitle.Paragraphs[i].Text.Substring(0, indexOfEndBracket + 1));
                        }
                        else
                        {
                            _subtitle.Paragraphs[i].Text = string.Format("<{0}>{1}</{0}>", tag, _subtitle.Paragraphs[i].Text);
                        }
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
                        if (p.Text.StartsWith("{\\", StringComparison.Ordinal) && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                        {
                            p.Text = p.Text.Remove(0, indexOfEndBracket + 1);
                        }

                        p.Text = HtmlUtil.RemoveHtmlTags(p.Text);
                        p.Text = p.Text.Replace("<" + BoxSingleLineText + ">", string.Empty).Replace("</" + BoxSingleLineText + ">", string.Empty);
                        p.Text = p.Text.Replace("<" + BoxMultiLineText + ">", string.Empty).Replace("</" + BoxMultiLineText + ">", string.Empty);

                        if (isSsa)
                        {
                            p.Text = Utilities.RemoveSsaTags(p.Text);
                        }

                        SubtitleListView1SetText(item.Index, p.Text);
                    }
                }
            }
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void subtitleListView1_KeyDown(object sender, KeyEventArgs e)
        {
            var italicShortCut = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewItalic);
            if (e.KeyData == italicShortCut)
            {
                ListViewToggleTag("i");
                subtitleListView1_SelectedIndexChanged(null, null);
            }
            else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                subtitleListView1.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                subtitleListView1.SelectFirstSelectedItemOnly();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift))
            {
                subtitleListView1.InverseSelection();
                e.SuppressKeyPress = true;
            }
        }

        public void SubtitleListView1SelectNone()
        {
            foreach (ListViewItem item in subtitleListView1.SelectedItems)
            {
                item.Selected = false;
            }
        }

        private void SubtitleListView1SelectIndexAndEnsureVisible(int index)
        {
            if (index < 0 || index >= subtitleListView1.Items.Count || subtitleListView1.Items.Count == 0)
            {
                return;
            }

            if (subtitleListView1.TopItem == null)
            {
                return;
            }

            int bottomIndex = subtitleListView1.TopItem.Index + (Height - 25) / 16;
            int itemsBeforeAfterCount = (bottomIndex - subtitleListView1.TopItem.Index) / 2 - 1;
            if (itemsBeforeAfterCount < 0)
            {
                itemsBeforeAfterCount = 1;
            }

            int beforeIndex = index - itemsBeforeAfterCount;
            if (beforeIndex < 0)
            {
                beforeIndex = 0;
            }

            int afterIndex = index + itemsBeforeAfterCount;
            if (afterIndex >= subtitleListView1.Items.Count)
            {
                afterIndex = subtitleListView1.Items.Count - 1;
            }

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

            subItem = new ListViewItem.ListViewSubItem(item, UiUtil.GetListViewTextFromString(paragraph.Text));
            try
            {
                if (_formatName == AdvancedSubStationAlpha.NameOfFormat || _formatName == SubStationAlpha.NameOfFormat)
                {
                    var style = AdvancedSubStationAlpha.GetSsaStyle(paragraph.Extra, _subtitle.Header);
                    subItem.Font = new Font(style.FontName, Font.Size);
                }
                else
                {
                    subItem.Font = new Font(_subtitleFontName, Font.Size);
                }
            }
            catch
            {
                subItem.Font = new Font(_subtitleFontName, Font.Size, FontStyle.Bold);
            }
            item.SubItems.Add(subItem);

            if (_formatName == AdvancedSubStationAlpha.NameOfFormat || _formatName == SubStationAlpha.NameOfFormat)
            {
                subItem = new ListViewItem.ListViewSubItem(item, paragraph.Extra);
                item.SubItems.Add(subItem);
            }

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
            int columnIndexStyle = 5;

            if (subtitleListView1.CheckBoxes)
            {
                subtitleListView1.Columns[0].Width = 60;
                columnIndexNumber++;
                columnIndexStart++;
                columnIndexEnd++;
                columnIndexDuration++;
                columnIndexText++;
            }

            var settings = Configuration.Settings;
            if (settings != null && settings.General.ListViewColumnsRememberSize && settings.General.ListViewNumberWidth > 1)
            {
                subtitleListView1.Columns[columnIndexNumber].Width = settings.General.ListViewNumberWidth;
            }
            else
            {
                subtitleListView1.Columns[columnIndexNumber].Width = 55;
            }

            subtitleListView1.Columns[columnIndexStart].Width = 90;
            subtitleListView1.Columns[columnIndexEnd].Width = 90;
            subtitleListView1.Columns[columnIndexDuration].Width = 60;
            if (_formatName == AdvancedSubStationAlpha.NameOfFormat || _formatName == SubStationAlpha.NameOfFormat)
            {
                subtitleListView1.Columns.Add("style", LanguageSettings.Current.General.Style, 90);
                subtitleListView1.Columns[columnIndexText].Width = subtitleListView1.Width -
                                                                   subtitleListView1.Columns[columnIndexNumber].Width -
                                                                   subtitleListView1.Columns[columnIndexStart].Width -
                                                                   subtitleListView1.Columns[columnIndexEnd].Width -
                                                                   subtitleListView1.Columns[columnIndexDuration].Width -
                                                                   175;
                subtitleListView1.Columns[columnIndexStyle].Width = -2;
            }
            else
            {
                subtitleListView1.Columns[columnIndexText].Width = -2;
            }
        }

        private void SubtitleListView1InitializeLanguage(LanguageStructure.General general, Core.Common.Settings settings)
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

            subtitleListView1.Items[index].SubItems[columnIndexText].Text = UiUtil.GetListViewTextFromString(text);
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
                        var fileName = FileUtil.GetTempFileName(".bmp");
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
                GetResolution(out var width, out var height);
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
                            {
                                x = GetBottomMarginInPixels(p);
                            }
                            else if (alignment == ContentAlignment.BottomRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.TopRight)
                            {
                                x = bmp.Width - textBmp.Width - GetBottomMarginInPixels(p);
                            }

                            int y = bmp.Height - textBmp.Height - GetBottomMarginInPixels(p);
                            if (alignment == ContentAlignment.BottomLeft || alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.TopLeft)
                            {
                                x = GetBottomMarginInPixels(p);
                            }
                            else if (alignment == ContentAlignment.BottomRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.TopRight)
                            {
                                x = bmp.Width - textBmp.Width - GetBottomMarginInPixels(p);
                            }

                            if (alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.MiddleCenter || alignment == ContentAlignment.MiddleRight)
                            {
                                y = (groupBoxExportImage.Height - 4 - textBmp.Height) / 2;
                            }
                            else if (alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.TopRight)
                            {
                                y = GetBottomMarginInPixels(p);
                            }

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
            using (var colorChooser = new ColorChooser { Color = panelFullFrameBackground.BackColor, Text = LanguageSettings.Current.ExportPngXml.ChooseBackgroundColor })
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
            if (_exportType == ExportFormats.BluraySup || _exportType == ExportFormats.VobSub || _exportType == ExportFormats.ImageFrame || _exportType == ExportFormats.BdnXml || _exportType == ExportFormats.Dost || _exportType == ExportFormats.Fab || _exportType == ExportFormats.Edl || _exportType == ExportFormats.EdlClipName)
            {
                return;
            }
            _allowCustomBottomMargin = checkBoxFullFrameImage.Checked;
            comboBoxBottomMargin.Visible = checkBoxFullFrameImage.Checked;
            labelBottomMargin.Visible = checkBoxFullFrameImage.Checked;
            comboBoxBottomMarginUnit.Visible = checkBoxFullFrameImage.Checked;
        }

        public void DisableSaveButtonAndCheckBoxes()
        {
            buttonExport.Visible = false;
            subtitleListView1.CheckBoxes = false;
        }

        private void comboBoxBottomMarginUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxBottomMargin.BeginUpdate();
            comboBoxBottomMargin.Items.Clear();
            if (comboBoxBottomMarginUnit.SelectedIndex == 0)
            {
                for (int i = 0; i <= 95; i++)
                {
                    comboBoxBottomMargin.Items.Add(i);
                }

                var exportMarginPercent = _exportType == ExportFormats.BluraySup ? Configuration.Settings.Tools.ExportBluRayBottomMarginPercent : Configuration.Settings.Tools.ExportBottomMarginPercent;
                if (exportMarginPercent >= 0 && exportMarginPercent < comboBoxBottomMargin.Items.Count)
                {
                    comboBoxBottomMargin.SelectedIndex = exportMarginPercent;
                }
            }
            else
            {
                for (int i = 0; i <= 1000; i++)
                {
                    comboBoxBottomMargin.Items.Add(i);
                }

                var exportMarginPixels = _exportType == ExportFormats.BluraySup ? Configuration.Settings.Tools.ExportBluRayBottomMarginPixels : Configuration.Settings.Tools.ExportBottomMarginPixels;
                if (exportMarginPixels >= 0 && exportMarginPixels < comboBoxBottomMargin.Items.Count)
                {
                    comboBoxBottomMargin.SelectedIndex = exportMarginPixels;
                }
            }
            if (comboBoxBottomMargin.SelectedIndex == -1)
            {
                comboBoxBottomMargin.SelectedIndex = 0;
            }
            comboBoxBottomMargin.EndUpdate();
        }

        private void comboBoxLeftRightMarginUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxLeftRightMargin.BeginUpdate();
            comboBoxLeftRightMargin.Items.Clear();
            if (comboBoxLeftRightMarginUnit.SelectedIndex == 0)
            {
                for (int i = 0; i < 95; i++)
                {
                    comboBoxLeftRightMargin.Items.Add(i);
                }

                if (Configuration.Settings.Tools.ExportLeftRightMarginPercent >= 0 && Configuration.Settings.Tools.ExportLeftRightMarginPercent < comboBoxLeftRightMargin.Items.Count)
                {
                    comboBoxLeftRightMargin.SelectedIndex = Configuration.Settings.Tools.ExportLeftRightMarginPercent;
                }
            }
            else
            {
                for (int i = 0; i <= 1000; i++)
                {
                    comboBoxLeftRightMargin.Items.Add(i);
                }

                if (Configuration.Settings.Tools.ExportLeftRightMarginPixels >= 0 && Configuration.Settings.Tools.ExportLeftRightMarginPixels < comboBoxLeftRightMargin.Items.Count)
                {
                    comboBoxLeftRightMargin.SelectedIndex = Configuration.Settings.Tools.ExportLeftRightMarginPixels;
                }
            }
            if (comboBoxLeftRightMargin.SelectedIndex == -1)
            {
                comboBoxLeftRightMargin.SelectedIndex = 0;
            }
            comboBoxLeftRightMargin.EndUpdate();
        }

        private void numericUpDownLineSpacing_KeyUp(object sender, KeyEventArgs e)
        {
            _previewTimer.Start();
        }

        public static Bitmap ResizeBitmap(Bitmap b, int width, int height)
        {
            Bitmap newImage = new Bitmap(width, height);
            using (var g = Graphics.FromImage(newImage))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(b, new Rectangle(0, 0, width, height));
            }
            return newImage;
        }

        private void comboBoxResizePercentage_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void adjustTimeCodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var showEarlierOrLater = new ShowEarlierLater())
            {
                showEarlierOrLater.Initialize(ShowEarlierOrLater, false);
                showEarlierOrLater.ShowDialog(this);
            }
        }

        public void ShowEarlierOrLater(double adjustMilliseconds, SelectionChoice selection)
        {
            adjustMilliseconds /= TimeCode.BaseUnit;
            subtitleListView1.BeginUpdate();
            int startFrom = 0;
            if (selection == SelectionChoice.SelectionAndForward)
            {
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    startFrom = subtitleListView1.SelectedItems[0].Index;
                }
                else
                {
                    startFrom = _subtitle.Paragraphs.Count;
                }
            }
            for (int i = startFrom; i < _subtitle.Paragraphs.Count; i++)
            {
                switch (selection)
                {
                    case SelectionChoice.SelectionOnly:
                        if (subtitleListView1.Items[i].Selected)
                        {
                            _subtitle.Paragraphs[i].Adjust(1.0, adjustMilliseconds);
                            ShowTimeInListView(i);
                        }
                        break;
                    case SelectionChoice.AllLines:
                    case SelectionChoice.SelectionAndForward:
                        _subtitle.Paragraphs[i].Adjust(1.0, adjustMilliseconds);
                        ShowTimeInListView(i);
                        break;
                }
            }
            subtitleListView1.EndUpdate();
        }

        private void ShowTimeInListView(int index)
        {
            int startIndex = 1;
            if (subtitleListView1.CheckBoxes)
            {
                startIndex++;
            }
            subtitleListView1.Items[index].SubItems[startIndex].Text = _subtitle.Paragraphs[index].StartTime.ToDisplayString();
            subtitleListView1.Items[index].SubItems[startIndex + 1].Text = _subtitle.Paragraphs[index].EndTime.ToDisplayString();
            subtitleListView1.Items[index].SubItems[startIndex + 2].Text = _subtitle.Paragraphs[index].Duration.ToShortDisplayString();
        }

        private void contextMenuStripListView_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool showImageExportMenuItems = _vobSubOcr != null;
            toolStripSeparatorAdjust.Visible = showImageExportMenuItems;
            adjustTimeCodesToolStripMenuItem.Visible = showImageExportMenuItems;
            adjustDisplayTimeToolStripMenuItem.Visible = showImageExportMenuItems;
        }

        private void adjustDisplayTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var adjustDisplayTime = new AdjustDisplayDuration())
            {
                adjustDisplayTime.HideRecalculate();
                if (adjustDisplayTime.ShowDialog(this) == DialogResult.OK)
                {
                    if (adjustDisplayTime.AdjustUsingPercent)
                    {
                        double percent = double.Parse(adjustDisplayTime.AdjustValue, CultureInfo.InvariantCulture);
                        _subtitle.AdjustDisplayTimeUsingPercent(percent, null);
                    }
                    else if (adjustDisplayTime.AdjustUsingSeconds)
                    {
                        double seconds = double.Parse(adjustDisplayTime.AdjustValue, CultureInfo.InvariantCulture);
                        _subtitle.AdjustDisplayTimeUsingSeconds(seconds, null);
                    }
                    else if (adjustDisplayTime.AdjustUsingRecalc)
                    {
                        double maxCharSeconds = (double)(adjustDisplayTime.MaxCharactersPerSecond);
                        _subtitle.RecalculateDisplayTimes(maxCharSeconds, null, (double)adjustDisplayTime.OptimalCharactersPerSecond);
                    }
                    else // fixed duration
                    {
                        _subtitle.SetFixedDuration(null, adjustDisplayTime.FixedMilliseconds);
                    }
                }
            }
            subtitleListView1.BeginUpdate();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                ShowTimeInListView(i);
            }
            subtitleListView1.EndUpdate();
        }
    }
}

using NCalc;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.Options;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public sealed partial class AssSetBackground : Form
    {
        public class PositionAndSize
        {
            public int Top { get; set; }
            public int Bottom { get; set; }
            public int Left { get; set; }
            public int Right { get; set; }
            public string Id { get; set; }
        }

        public Subtitle UpdatedSubtitle { get; private set; }
        private readonly Subtitle _subtitleWithNewHeader;
        private Subtitle _drawing;
        private readonly int[] _selectedIndices;
        private LibMpvDynamic _mpv;
        private string _mpvTextFileName;
        private bool _closing;
        private bool _videoLoaded;
        private bool _updatePreview = true;
        private bool _updateDrawingFile;
        private readonly string _videoFileName;
        private readonly VideoInfo _videoInfo;
        private double _videoPositionSeconds;
        private bool _loading = true;
        private string _assaBox;
        private readonly Random _random = new Random();
        private int _top;
        private int _bottom;
        private int _left;
        private int _right;
        private Color _boxColor;
        private Color _boxShadowColor;
        private Color _boxOutlineColor;
        private long _totalFrames;
        private FileSystemWatcher _drawingFileWatcher;

        public AssSetBackground(Subtitle subtitle, int[] selectedIndices, string videoFileName, VideoInfo videoInfo, double videoPositionSeconds)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _videoFileName = videoFileName;
            _videoInfo = videoInfo;
            _videoPositionSeconds = videoPositionSeconds;

            _subtitleWithNewHeader = new Subtitle(subtitle, false);
            if (string.IsNullOrWhiteSpace(_subtitleWithNewHeader.Header))
            {
                _subtitleWithNewHeader.Header = AdvancedSubStationAlpha.DefaultHeader;
            }

            _selectedIndices = selectedIndices;
            comboBoxBoxStyle.Items.Clear();
            comboBoxBoxStyle.Items.Add(LanguageSettings.Current.AssaProgressBarGenerator.SquareCorners);
            comboBoxBoxStyle.Items.Add(LanguageSettings.Current.AssaProgressBarGenerator.RoundedCorners);
            comboBoxBoxStyle.Items.Add(LanguageSettings.Current.AssaSetBackgroundBox.Spikes);

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            if (Configuration.Settings.Tools.AssaBgBoxStyle == "spikes")
            {
                comboBoxBoxStyle.SelectedIndex = 2;
            }
            else if (Configuration.Settings.Tools.AssaBgBoxStyle == "rounded")
            {
                comboBoxBoxStyle.SelectedIndex = 1;
            }
            else
            {
                comboBoxBoxStyle.SelectedIndex = 0;
            }

            Text = LanguageSettings.Current.AssaSetBackgroundBox.Title;
            groupBoxPadding.Text = LanguageSettings.Current.AssaSetBackgroundBox.Padding;
            groupBoxFillWidth.Text = LanguageSettings.Current.AssaSetBackgroundBox.FillWidth;
            checkBoxFillWidth.Text = LanguageSettings.Current.AssaSetBackgroundBox.FillWidth;
            groupBoxDrawing.Text = LanguageSettings.Current.AssaSetBackgroundBox.Drawing;
            groupBoxStyle.Text = LanguageSettings.Current.General.Style;
            groupBoxAlignment.Text = LanguageSettings.Current.SubStationAlphaStyles.Alignment;
            labelPaddingLeft.Text = LanguageSettings.Current.ExportPngXml.Left;
            labelPaddingRight.Text = LanguageSettings.Current.ExportPngXml.Right;
            labelPaddingBottom.Text = LanguageSettings.Current.AssaProgressBarGenerator.Bottom;
            labelPaddingTop.Text = LanguageSettings.Current.AssaProgressBarGenerator.Top;
            checkBoxOnlyDrawing.Text = LanguageSettings.Current.AssaSetBackgroundBox.OnlyDrawing;
            buttonPrimaryColor.Text = LanguageSettings.Current.AssaSetBackgroundBox.BoxColor;
            buttonOutlineColor.Text = LanguageSettings.Current.SubStationAlphaStyles.Outline;
            buttonShadowColor.Text = LanguageSettings.Current.SubStationAlphaStyles.Shadow;
            labelRadius.Text = LanguageSettings.Current.AssaSetBackgroundBox.Radius;
            labelDrawingMarginH.Text = LanguageSettings.Current.AssaSetBackgroundBox.MarginX;
            labelDrawingMarginV.Text = LanguageSettings.Current.AssaSetBackgroundBox.MarginY;
            labelChooseDrawing.Text = LanguageSettings.Current.AssaSetBackgroundBox.DrawingFile;
            groupBoxPreview.Text = LanguageSettings.Current.General.Preview;
            labelFillWidthMarginLeft.Text = LanguageSettings.Current.SubStationAlphaStyles.MarginLeft;
            labelFillWidthMarginRight.Text = LanguageSettings.Current.SubStationAlphaStyles.MarginRight;
            labelStep.Text = LanguageSettings.Current.AssaSetBackgroundBox.Step;
            labelMaxSpike.Text = LanguageSettings.Current.AssaSetBackgroundBox.MaxSpike;
            buttonDrawingClear.Text = LanguageSettings.Current.DvdSubRip.Clear;


            SafeNumericUpDownAssign(numericUpDownPaddingLeft, Configuration.Settings.Tools.AssaBgBoxPaddingLeft);
            SafeNumericUpDownAssign(numericUpDownPaddingLeft, Configuration.Settings.Tools.AssaBgBoxPaddingLeft);
            SafeNumericUpDownAssign(numericUpDownPaddingRight, Configuration.Settings.Tools.AssaBgBoxPaddingRight);
            SafeNumericUpDownAssign(numericUpDownPaddingTop, Configuration.Settings.Tools.AssaBgBoxPaddingTop);
            SafeNumericUpDownAssign(numericUpDownPaddingBottom, Configuration.Settings.Tools.AssaBgBoxPaddingBottom);
            panelPrimaryColor.BackColor = Configuration.Settings.Tools.AssaBgBoxColor;
            panelOutlineColor.BackColor = Configuration.Settings.Tools.AssaBgBoxOutlineColor;
            panelShadowColor.BackColor = Configuration.Settings.Tools.AssaBgBoxShadowColor;
            SafeNumericUpDownAssign(numericUpDownRadius, Configuration.Settings.Tools.AssaBgBoxStyleRadius);
            SafeNumericUpDownAssign(numericUpDownOutlineWidth, Configuration.Settings.Tools.AssaBgBoxOutlineWidth);
            labelFileName.Text = Configuration.Settings.Tools.AssaBgBoxDrawing;
            ReadDrawingFile(Configuration.Settings.Tools.AssaBgBoxDrawing);
            SafeNumericUpDownAssign(numericUpDownDrawingMarginH, Configuration.Settings.Tools.AssaBgBoxDrawingMarginH);
            SafeNumericUpDownAssign(numericUpDownDrawingMarginV, Configuration.Settings.Tools.AssaBgBoxDrawingMarginV);
            checkBoxOnlyDrawing.Checked = Configuration.Settings.Tools.AssaBgBoxDrawingOnly;

            switch (Configuration.Settings.Tools.AssaBgBoxDrawingAlignment)
            {
                case "2":
                    radioButtonBottomLeft.Checked = true;
                    break;
                case "3":
                    radioButtonBottomRight.Checked = true;
                    break;
                case "4":
                    radioButtonMiddleLeft.Checked = true;
                    break;
                case "5":
                    radioButtonMiddleCenter.Checked = true;
                    break;
                case "6":
                    radioButtonMiddleRight.Checked = true;
                    break;
                case "7":
                    radioButtonTopLeft.Checked = true;
                    break;
                case "8":
                    radioButtonTopCenter.Checked = true;
                    break;
                case "9":
                    radioButtonTopRight.Checked = true;
                    break;
                default:
                    radioButtonTopLeft.Checked = true;
                    break;
            }

            _boxColor = panelPrimaryColor.BackColor;
            _boxOutlineColor = panelOutlineColor.BackColor;
            _boxShadowColor = panelShadowColor.BackColor;

            buttonChooseDrawing.Left = labelChooseDrawing.Left + labelChooseDrawing.Width + 5;
            buttonDrawingClear.Left = buttonChooseDrawing.Left + buttonChooseDrawing.Width + 5;
            buttonAssaDraw.Left = buttonDrawingClear.Left + buttonDrawingClear.Width + 5;

            var left = Math.Max(labelFillWidthMarginLeft.Left + labelFillWidthMarginLeft.Width + 5, labelFillWidthMarginRight.Left + labelFillWidthMarginRight.Width + 5);
            numericUpDownFillWidthMarginLeft.Left = left;
            numericUpDownFillWidthMarginRight.Left = left;

            left = Math.Max(labelDrawingMarginH.Left + labelDrawingMarginH.Width + 5, labelDrawingMarginV.Left + labelDrawingMarginV.Width + 5);
            numericUpDownDrawingMarginH.Left = left;
            numericUpDownDrawingMarginV.Left = left;

            groupBoxAlignment.Left = numericUpDownDrawingMarginH.Left + numericUpDownDrawingMarginH.Width + 10;

            UiUtil.FixLargeFonts(this, buttonOK);

            if (_videoInfo == null)
            {
                _videoInfo = UiUtil.GetVideoInfo(_videoFileName);
            }

            comboBoxBoxStyle_SelectedIndexChanged(null, null);
            SetPosition_ResizeEnd(null, null);
            buttonAssaDraw.Visible = File.Exists(Path.Combine(Configuration.PluginsDirectory, "AssaDraw.dll"));
            progressBar1.Visible = false;
            labelProgress.Visible = false;
        }

        private static void SafeNumericUpDownAssign(NumericUpDown numericUpDown, int value)
        {
            if (value < numericUpDown.Minimum)
            {
                numericUpDown.Value = numericUpDown.Minimum;
            }
            else if (value > numericUpDown.Maximum)
            {
                numericUpDown.Value = numericUpDown.Maximum;
            }
            else
            {
                numericUpDown.Value = value;
            }
        }

        private void AssSetBackground_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                if (!string.IsNullOrEmpty(labelFileName.Text))
                {
                    ReadDrawingFile(labelFileName.Text);
                }

                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#assa_bg_box");
                e.SuppressKeyPress = true;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            buttonOK.Enabled = false;
            UpdatedSubtitle = new Subtitle(_subtitleWithNewHeader, false);
            AddBgBoxStyles(UpdatedSubtitle);
            var positionsAndSizes = CalcAllPositionsAndSizes();
            foreach (var posAndSize in positionsAndSizes)
            {
                // build box + gen preview via mpv
                var x = posAndSize.Left - (int)numericUpDownPaddingLeft.Value;
                var right = posAndSize.Right + (int)numericUpDownPaddingRight.Value;
                if (checkBoxFillWidth.Checked)
                {
                    x = (int)numericUpDownFillWidthMarginLeft.Value;
                    right = _videoInfo.Width - (int)numericUpDownFillWidthMarginRight.Value;
                }

                _assaBox = GenerateBackgroundBox(new PositionAndSize
                {
                    Id = posAndSize.Id,
                    Top = posAndSize.Top - (int)numericUpDownPaddingTop.Value,
                    Bottom = posAndSize.Bottom + (int)numericUpDownPaddingBottom.Value,
                    Left = x,
                    Right = right,
                });

                var p = UpdatedSubtitle.GetParagraphOrDefaultById(posAndSize.Id);
                if (p.IsComment)
                {
                    continue;
                }

                var p2 = new Paragraph(_assaBox ?? string.Empty, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds)
                {
                    Layer = Configuration.Settings.Tools.AssaBgBoxLayer,
                    Extra = "SE-box-bg"
                };

                if (!checkBoxOnlyDrawing.Checked)
                {
                    UpdatedSubtitle.InsertParagraphInCorrectTimeOrder(p2);
                }

                AddDrawing(x, right, posAndSize.Top - (int)numericUpDownPaddingTop.Value, posAndSize.Bottom + (int)numericUpDownPaddingBottom.Value, p, UpdatedSubtitle);
            }

            UpdatedSubtitle.Renumber();
            DialogResult = DialogResult.OK;
        }

        private void AddBgBoxStyles(Subtitle subtitle)
        {
            if (string.IsNullOrWhiteSpace(subtitle.Header))
            {
                subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
            }

            if (_drawing != null)
            {
                var styleBoxDrawing = new SsaStyle
                {
                    Alignment = "7",
                    Name = "SE-box-drawing",
                    MarginLeft = 0,
                    MarginRight = 0,
                    MarginVertical = 0,
                    ShadowWidth = 0,
                    OutlineWidth = 0,
                };
                subtitle.Header = AdvancedSubStationAlpha.UpdateOrAddStyle(subtitle.Header, styleBoxDrawing);
            }

            if (checkBoxOnlyDrawing.Checked)
            {
                return;
            }

            var styleBoxBg = new SsaStyle
            {
                Alignment = "7",
                Name = "SE-box-bg",
                MarginLeft = 0,
                MarginRight = 0,
                MarginVertical = 0,
                Primary = _boxColor,
                Secondary = _boxShadowColor,
                Tertiary = _boxShadowColor,
                Background = _boxShadowColor,
                Outline = _boxOutlineColor,
                ShadowWidth = numericUpDownShadowDistance.Value,
                OutlineWidth = numericUpDownOutlineWidth.Value,
            };
            subtitle.Header = AdvancedSubStationAlpha.UpdateOrAddStyle(subtitle.Header, styleBoxBg);
        }

        private int[] GetIndices()
        {
            return _selectedIndices;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void GeneratePreviewViaMpv()
        {
            var fileName = _videoFileName;
            if (!File.Exists(fileName))
            {
                using (var p = GetFFmpegProcess(fileName))
                {
                    p.Start();
                    p.WaitForExit();
                }
            }

            if (!LibMpvDynamic.IsInstalled)
            {
                if (MessageBox.Show("Download and use \"mpv\" as video player?", "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    using (var form = new SettingsMpv(!LibMpvDynamic.IsInstalled))
                    {
                        if (form.ShowDialog(this) != DialogResult.OK)
                        {
                            return;
                        }

                        Configuration.Settings.General.VideoPlayer = "MPV";
                    }
                }
            }

            if (!LibMpvDynamic.IsInstalled)
            {
                return;
            }

            if (_mpv == null)
            {
                _mpv = new LibMpvDynamic();
                _mpv.Initialize(pictureBoxPreview, fileName, VideoLoaded, null);
            }
            else
            {
                VideoLoaded(null, null);
            }
        }

        public static Process GetFFmpegProcess(string outputFileName)
        {
            var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
            {
                ffmpegLocation = "ffmpeg";
            }

            return new Process
            {
                StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-t 1 -f lavfi -i color=c=blue:s=720x480 -c:v libx264 -tune stillimage -pix_fmt yuv420p \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }

        private void VideoLoaded(object sender, EventArgs e)
        {
            var format = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle(_subtitleWithNewHeader);
            subtitle.Paragraphs.Clear();
            var indices = GetIndices();
            var styleToApply = string.Empty;

            if (_videoPositionSeconds < 0.001)
            {
                _videoPositionSeconds = 0.05;
            }

            var p = _subtitleWithNewHeader.Paragraphs.FirstOrDefault(l => !l.IsComment &&
                                                                             _videoPositionSeconds > l.StartTime.TotalSeconds &&
                                                                             _videoPositionSeconds < l.EndTime.TotalSeconds &&
                                                                             indices.Contains(_subtitleWithNewHeader.GetIndex(l)));
            if (p == null)
            {
                p = indices.Length > 0 ?
                    new Paragraph(_subtitleWithNewHeader.Paragraphs[indices[0]]) :
                    new Paragraph(Configuration.Settings.General.PreviewAssaText, 0, 1000);
            }

            // remove fade tags 
            p.Text = Regex.Replace(p.Text, @"{\\fad\([\d\.,]*\)}", string.Empty);
            p.Text = Regex.Replace(p.Text, @"\\fad\([\d\.,]*\)", string.Empty);
            p.Text = Regex.Replace(p.Text, @"{\\fade\([\d\.,]*\)}", string.Empty);
            p.Text = Regex.Replace(p.Text, @"\\fade\([\d\.,]*\)", string.Empty);

            p.Text = styleToApply + p.Text;
            subtitle.Paragraphs.Add(p);

            // build box + gen preview via mpv
            var x = _left - (int)numericUpDownPaddingLeft.Value;
            var right = _right + (int)numericUpDownPaddingRight.Value;
            if (checkBoxFillWidth.Checked)
            {
                x = (int)numericUpDownFillWidthMarginLeft.Value;
                right = _videoInfo.Width - (int)numericUpDownFillWidthMarginRight.Value;
            }

            _assaBox = GenerateBackgroundBox(new PositionAndSize
            {
                Id = p.Id,
                Top = _top - (int)numericUpDownPaddingTop.Value,
                Bottom = _bottom + (int)numericUpDownPaddingBottom.Value,
                Left = x,
                Right = right,
            });

            var p2 = new Paragraph(_assaBox ?? string.Empty, 0, 1000)
            {
                StartTime = { TotalMilliseconds = p.StartTime.TotalMilliseconds },
                EndTime = { TotalMilliseconds = p.EndTime.TotalMilliseconds },
                Layer = Configuration.Settings.Tools.AssaBgBoxLayer,
                Extra = "SE-box-bg"
            };

            if (!checkBoxOnlyDrawing.Checked)
            {
                subtitle.Paragraphs.Add(p2);
            }

            AddBgBoxStyles(subtitle);
            AddDrawing(x, right, _top - (int)numericUpDownPaddingTop.Value, _bottom + (int)numericUpDownPaddingBottom.Value, p, subtitle);

            var text = subtitle.ToText(format);
            _mpvTextFileName = FileUtil.GetTempFileName(format.Extension);
            File.WriteAllText(_mpvTextFileName, text);
            _mpv.LoadSubtitle(_mpvTextFileName);

            if (!_videoLoaded)
            {
                Application.DoEvents();
                _mpv.Pause();
                if (_videoPositionSeconds > p.StartTime.TotalSeconds && _videoPositionSeconds < p.EndTime.TotalSeconds)
                {
                    _mpv.CurrentPosition = _videoPositionSeconds;
                }
                else
                {
                    _mpv.CurrentPosition = p.StartTime.TotalSeconds + 0.05;
                }

                Application.DoEvents();
                _videoLoaded = true;
                timerPreview.Start();

                labelProgress.Text = $"{Path.GetFileName(_videoFileName)} - {string.Format(LanguageSettings.Current.Main.LineNumberX, _subtitleWithNewHeader.GetIndex(p) + 1)} - {p.StartTime}";
                if (p.StartTime.TotalSeconds > _videoInfo.TotalSeconds)
                {
                    labelProgress.Text = "Video position is after end of video - NO PREVIEW!";
                }

                labelProgress.Visible = true;
            }
        }

        private void AddDrawing(int x, int right, int top, int bottom, Paragraph p, Subtitle subtitle)
        {
            if (_drawing != null)
            {
                var marginH = (int)numericUpDownDrawingMarginH.Value;
                var marginV = (int)numericUpDownDrawingMarginV.Value;
                var height = bottom - top;
                var width = right - x;
                var pos = string.Empty;
                if (radioButtonTopLeft.Checked)
                {
                    pos = $"{{\\pos({x + marginH},{top + marginV})}}";
                }
                else if (radioButtonTopCenter.Checked)
                {
                    pos = $"{{\\pos({x + (width / 2) + marginH},{top + marginV})}}";
                }
                else if (radioButtonTopRight.Checked)
                {
                    pos = $"{{\\pos({right - marginH},{top + marginV})}}";
                }
                else if (radioButtonMiddleLeft.Checked)
                {
                    pos = $"{{\\pos({x + marginH},{top + (height / 2) + marginV})}}";
                }
                else if (radioButtonMiddleCenter.Checked)
                {
                    pos = $"{{\\pos({x + (width / 2) + marginH},{top + (height / 2) + marginV})}}";
                }
                else if (radioButtonMiddleRight.Checked)
                {
                    pos = $"{{\\pos({right - marginH},{top + (height / 2) + marginV})}}";
                }
                else if (radioButtonBottomLeft.Checked)
                {
                    pos = $"{{\\pos({x + marginH},{bottom - marginV})}}";
                }
                else if (radioButtonBottomCenter.Checked)
                {
                    pos = $"{{\\pos({x + (width / 2) + marginH},{bottom - marginV})}}";
                }
                else if (radioButtonBottomRight.Checked)
                {
                    pos = $"{{\\pos({right - marginH},{bottom - marginV})}}";
                }

                var layerAdd = 1;
                foreach (var dp in _drawing.Paragraphs.OrderBy(pa => pa.Layer))
                {
                    var pDrawing = new Paragraph(dp.Text, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds);
                    pDrawing.Text = pos + TranslateTemplate(pDrawing.Text, x, top, right, bottom);
                    pDrawing.Layer = Configuration.Settings.Tools.AssaBgBoxLayer + layerAdd;
                    pDrawing.Extra = "SE-box-drawing";
                    subtitle.InsertParagraphInCorrectTimeOrder(pDrawing);
                    layerAdd++;
                }
            }
        }

        private string TranslateTemplate(string input, int left, int top, int right, int bottom)
        {
            var text = input;
            var width = right - left;
            var height = bottom - top;

            try
            {
                var startIdx = text.IndexOf('[');
                while (startIdx >= 0)
                {
                    var endIdx = text.IndexOf(']', startIdx);
                    if (endIdx < 0)
                    {
                        return input;
                    }

                    var expr = text.Substring(startIdx + 1, endIdx - startIdx - 1);
                    expr = expr.ToUpperInvariant()
                        .Replace("LEFT", left.ToString(CultureInfo.InvariantCulture))
                        .Replace("TOP", top.ToString(CultureInfo.InvariantCulture))
                        .Replace("RIGHT", right.ToString(CultureInfo.InvariantCulture))
                        .Replace("BOTTOM", bottom.ToString(CultureInfo.InvariantCulture))
                        .Replace("WIDTH", width.ToString(CultureInfo.InvariantCulture))
                        .Replace("HEIGHT", height.ToString(CultureInfo.InvariantCulture));

                    var calcExpression = new Expression(expr);
                    var calcObj = calcExpression.Evaluate();
                    var calcResult = calcExpression.Evaluate().ToString();
                    if (calcObj is double numberDouble)
                    {
                        calcResult = numberDouble.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (calcObj is float numberFloat)
                    {
                        calcResult = numberFloat.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (calcObj is decimal numberDecimal)
                    {
                        calcResult = numberDecimal.ToString(CultureInfo.InvariantCulture);
                    }

                    text = text
                        .Remove(startIdx, endIdx - startIdx + 1)
                        .Insert(startIdx, calcResult);

                    startIdx = text.IndexOf('[', startIdx);
                }

                return text;
            }
            catch
            {
                return input;
            }
        }

        private void ApplyCustomStyles_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerPreview.Stop();
            timerFileChange.Stop();
            _drawingFileWatcher?.Dispose();
            _closing = true;
            _mpv?.Dispose();

            if (comboBoxBoxStyle.SelectedIndex == 2)
            {
                Configuration.Settings.Tools.AssaBgBoxStyle = "spikes";
            }
            else if (comboBoxBoxStyle.SelectedIndex == 1)
            {
                Configuration.Settings.Tools.AssaBgBoxStyle = "rounded";
            }
            else
            {
                Configuration.Settings.Tools.AssaBgBoxStyle = "square";
            }

            Configuration.Settings.Tools.AssaBgBoxStyleRadius = (int)numericUpDownRadius.Value;
            Configuration.Settings.Tools.AssaBgBoxOutlineWidth = (int)numericUpDownOutlineWidth.Value;
            Configuration.Settings.Tools.AssaBgBoxPaddingLeft = (int)numericUpDownPaddingLeft.Value;
            Configuration.Settings.Tools.AssaBgBoxPaddingRight = (int)numericUpDownPaddingRight.Value;
            Configuration.Settings.Tools.AssaBgBoxPaddingTop = (int)numericUpDownPaddingTop.Value;
            Configuration.Settings.Tools.AssaBgBoxPaddingBottom = (int)numericUpDownPaddingBottom.Value;
            Configuration.Settings.Tools.AssaBgBoxColor = panelPrimaryColor.BackColor;
            Configuration.Settings.Tools.AssaBgBoxOutlineColor = panelOutlineColor.BackColor;
            Configuration.Settings.Tools.AssaBgBoxShadowColor = panelShadowColor.BackColor;
            Configuration.Settings.Tools.AssaBgBoxDrawing = labelFileName.Text;
            Configuration.Settings.Tools.AssaBgBoxDrawingMarginV = (int)numericUpDownDrawingMarginV.Value;
            Configuration.Settings.Tools.AssaBgBoxDrawingMarginH = (int)numericUpDownDrawingMarginH.Value;
            Configuration.Settings.Tools.AssaBgBoxDrawingOnly = checkBoxOnlyDrawing.Checked;

            if (radioButtonBottomLeft.Checked)
            {
                Configuration.Settings.Tools.AssaBgBoxDrawingAlignment = "2";
            }
            else if (radioButtonBottomRight.Checked)
            {
                Configuration.Settings.Tools.AssaBgBoxDrawingAlignment = "3";
            }
            else if (radioButtonMiddleLeft.Checked)
            {
                Configuration.Settings.Tools.AssaBgBoxDrawingAlignment = "4";
            }
            else if (radioButtonMiddleCenter.Checked)
            {
                Configuration.Settings.Tools.AssaBgBoxDrawingAlignment = "5";
            }
            else if (radioButtonMiddleRight.Checked)
            {
                Configuration.Settings.Tools.AssaBgBoxDrawingAlignment = "6";
            }
            else if (radioButtonTopLeft.Checked)
            {
                Configuration.Settings.Tools.AssaBgBoxDrawingAlignment = "7";
            }
            else if (radioButtonTopCenter.Checked)
            {
                Configuration.Settings.Tools.AssaBgBoxDrawingAlignment = "8";
            }
            else if (radioButtonTopRight.Checked)
            {
                Configuration.Settings.Tools.AssaBgBoxDrawingAlignment = "9";
            }
            else
            {
                Configuration.Settings.Tools.AssaBgBoxDrawingAlignment = "1";
            }
        }

        private void timerPreview_Tick(object sender, EventArgs e)
        {
            if (_mpv == null || _closing || !_updatePreview)
            {
                return;
            }

            VideoLoaded(null, null);
            _updatePreview = false;
        }

        private void AssSetBackground_Shown(object sender, EventArgs e)
        {
            var playResX = AdvancedSubStationAlpha.GetTagFromHeader("PlayResX", "[Script Info]", _subtitleWithNewHeader.Header);
            var playResY = AdvancedSubStationAlpha.GetTagFromHeader("PlayResY", "[Script Info]", _subtitleWithNewHeader.Header);
            if (string.IsNullOrEmpty(playResX) || string.IsNullOrEmpty(playResY))
            {
                var dialogResult = MessageBox.Show(LanguageSettings.Current.AssaSetPosition.ResolutionMissing, "Subtitle Edit", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes)
                {
                    _subtitleWithNewHeader.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + _videoInfo.Width.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitleWithNewHeader.Header);
                    _subtitleWithNewHeader.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + _videoInfo.Height.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitleWithNewHeader.Header);
                }
            }

            CalcPositionAndSize();
            Application.DoEvents();
            GeneratePreviewViaMpv();
            _loading = false;
            timerFileChange.Start();
        }

        private void pictureBoxPreview_Click(object sender, EventArgs e)
        {
            VideoLoaded(null, null);
        }

        private void SetPosition_ResizeEnd(object sender, EventArgs e)
        {
            var aspectRatio = (double)_videoInfo.Width / _videoInfo.Height;
            var newWidth = pictureBoxPreview.Height * aspectRatio;
            Width += (int)(newWidth - pictureBoxPreview.Width);
        }

        private void numericUpDownRotateX_ValueChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            _updatePreview = true;
            VideoLoaded(null, null);
        }

        private void CalcPositionAndSize()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // generate blank video
                var tempVideoFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mkv");
                var process = VideoPreviewGenerator.GenerateVideoFile(
                               tempVideoFileName,
                               2,
                               _videoInfo.Width,
                               _videoInfo.Height,
                               Configuration.Settings.Tools.AssaBgBoxTransparentColor,
                               false,
                               25,
                               null);
                process.Start();
                while (!process.HasExited)
                {
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();
                }

                // make temp assa file with font
                var assaTempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ass");
                var sub = new Subtitle();
                sub.Header = _subtitleWithNewHeader.Header;
                sub.Paragraphs.Add(new Paragraph(GetPreviewParagraph()));
                File.WriteAllText(assaTempFileName, new AdvancedSubStationAlpha().ToText(sub, string.Empty));

                // hard code subtitle
                var outputVideoFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
                process = GetFfmpegProcess(tempVideoFileName, outputVideoFileName, assaTempFileName);
                process.Start();
                while (!process.HasExited)
                {
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();
                }

                Cursor = Cursors.Default;
                var bmpFileName = VideoPreviewGenerator.GetScreenShot(outputVideoFileName, "00:00:01");
                using (var bmp = new Bitmap(bmpFileName))
                {
                    var nBmp = new NikseBitmap(bmp);
                    _top = nBmp.CalcTopCropping(Configuration.Settings.Tools.AssaBgBoxTransparentColor);
                    _bottom = nBmp.Height - nBmp.CalcBottomCropping(Configuration.Settings.Tools.AssaBgBoxTransparentColor);
                    _left = nBmp.CalcLeftCropping(Configuration.Settings.Tools.AssaBgBoxTransparentColor);
                    _right = nBmp.Width - nBmp.CalcRightCropping(Configuration.Settings.Tools.AssaBgBoxTransparentColor);
                    _updatePreview = true;
                }

                try
                {
                    File.Delete(tempVideoFileName);
                    File.Delete(assaTempFileName);
                    File.Delete(outputVideoFileName);
                    File.Delete(bmpFileName);
                }
                catch
                {
                    // ignore
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private List<PositionAndSize> CalcAllPositionsAndSizes()
        {
            var third = (_selectedIndices.Length + 2) * 25;
            _totalFrames = third * 3;
            if (_totalFrames > 0 && _selectedIndices.Length > 5)
            {
                progressBar1.Visible = true;
                progressBar1.Style = ProgressBarStyle.Marquee;
                labelProgress.Text = LanguageSettings.Current.General.PleaseWait;
            }

            var list = new List<PositionAndSize>();
            try
            {
                Cursor = Cursors.WaitCursor;

                // generate blank video
                var tempVideoFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mkv");
                var process = VideoPreviewGenerator.GenerateVideoFile(
                               tempVideoFileName,
                               _selectedIndices.Length + 2,
                               _videoInfo.Width,
                               _videoInfo.Height,
                               Configuration.Settings.Tools.AssaBgBoxTransparentColor,
                               false,
                               1,
                               null);
                process.Start();
                while (!process.HasExited)
                {
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();
                }

                // make temp assa file with font
                var assaTempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ass");
                var sub = new Subtitle { Header = _subtitleWithNewHeader.Header };
                for (var i = 0; i < _selectedIndices.Length; i++)
                {
                    var idx = _selectedIndices[i];
                    var p = _subtitleWithNewHeader.Paragraphs[idx];

                    // remove fade tags 
                    var text = p.Text;
                    text = Regex.Replace(text, @"{\\fad\([\d\.,]*\)}", string.Empty);
                    text = Regex.Replace(text, @"\\fad\([\d\.,]*\)", string.Empty);
                    text = Regex.Replace(text, @"{\\fade\([\d\.,]*\)}", string.Empty);
                    text = Regex.Replace(text, @"\\fade\([\d\.,]*\)", string.Empty);

                    sub.Paragraphs.Add(new Paragraph(text, i * 1000 - 500, i * 1000 + 500)
                    {
                        Extra = p.Extra,
                    });
                }

                File.WriteAllText(assaTempFileName, new AdvancedSubStationAlpha().ToText(sub, string.Empty));

                // hard code subtitle
                var outputVideoFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
                process = GetFfmpegProcess(tempVideoFileName, outputVideoFileName, assaTempFileName);
                process.Start();
                while (!process.HasExited)
                {
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();
                }

                var tempImageFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                var images = VideoPreviewGenerator.GetScreenShotsForEachFrame(outputVideoFileName, tempImageFolder);
                Parallel.For(0, images.Length, index =>
                {
                    if (index < _selectedIndices.Length)
                    {
                        var idx = _selectedIndices[index];
                        var bmpFileName = images[index];
                        using (var bmp = new Bitmap(bmpFileName))
                        {
                            var nBmp = new NikseBitmap(bmp);
                            list.Add(new PositionAndSize
                            {
                                Top = nBmp.CalcTopCropping(Configuration.Settings.Tools.AssaBgBoxTransparentColor),
                                Bottom = nBmp.Height - nBmp.CalcBottomCropping(Configuration.Settings.Tools.AssaBgBoxTransparentColor),
                                Left = nBmp.CalcLeftCropping(Configuration.Settings.Tools.AssaBgBoxTransparentColor),
                                Right = nBmp.Width - nBmp.CalcRightCropping(Configuration.Settings.Tools.AssaBgBoxTransparentColor),
                                Id = _subtitleWithNewHeader.Paragraphs[idx].Id,
                            });
                        }
                        try
                        {
                            File.Delete(bmpFileName);
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                });

                Cursor = Cursors.Default;
                try
                {
                    File.Delete(tempVideoFileName);
                    File.Delete(assaTempFileName);
                    File.Delete(outputVideoFileName);
                    Directory.Delete(tempImageFolder);
                }
                catch
                {
                    // ignore
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            progressBar1.Visible = false;
            return list;
        }

        private string GenerateBackgroundBox(PositionAndSize posAndSize)
        {
            var x = posAndSize.Left;
            var y = posAndSize.Top;
            var bottom = posAndSize.Bottom;
            var right = posAndSize.Right;

            if (comboBoxBoxStyle.SelectedIndex == 2) // spikes
            {
                var sb = new StringBuilder();
                sb.Append($"{{\\p1}}m {x} {y} l ");
                var step = (int)numericUpDownSpikesStep.Value;
                var maxSpike = (int)numericUpDownSpikesMax.Value;
                var minSpike = Math.Min(10, maxSpike - 1);
                for (var i = x; i <= right; i += step)
                {
                    sb.Append($"{i + (step / 2)} {y - _random.Next(minSpike, maxSpike)} ");
                    sb.Append($"{i + step} {y} ");
                }

                for (var i = y; i <= bottom; i += step)
                {
                    sb.Append($"{right + _random.Next(minSpike, maxSpike)} {i + (step / 2)} ");
                    sb.Append($"{right} {i + step} ");
                }

                for (var i = right; i >= x; i -= step)
                {
                    sb.Append($"{i - (step / 2)} {bottom + _random.Next(minSpike, maxSpike)} ");
                    sb.Append($"{i - step} {bottom} ");
                }

                for (var i = bottom; i >= y; i -= step)
                {
                    sb.Append($"{x - _random.Next(minSpike, maxSpike)} {i - (step / 2)} ");
                    sb.Append($"{x} {i - step} ");
                }
                sb.Append("{\\p0}");
                return sb.ToString();
            }

            if (comboBoxBoxStyle.SelectedIndex == 1) // rounded corners
            {
                var radius = (int)numericUpDownRadius.Value;
                radius = Math.Min((bottom - y) / 2, radius); // do not allow overflow
                x += radius;
                right -= radius;
                return $@"{{\p1}}m {x} {y} b {x - radius} {y} {x - radius} {y} {x - radius} {y + radius} l {x - radius} {bottom - radius} b {x - radius} {bottom} {x - radius} {bottom}  {x} {bottom} l {right} {bottom} b {right + radius} {bottom} {right + radius} {bottom} {right + radius} {bottom - radius} l {right + radius} {bottom - radius} {right + radius} {y + radius} b {right + radius} {y} {right + radius} {y} {right} {y} l  {x} {y}{{\p0}}";
                //                 top left  left top corner                                                                                left bottom corner                                                             lower right corner                                                                                                                                          upper right corner
            }

            return $"{{\\p1}}m {x} {y} l {right} {y} {right} {bottom} {x} {bottom}{{\\p0}}";
        }

        private Paragraph GetPreviewParagraph()
        {
            var idx = _selectedIndices.Min();
            var first = _subtitleWithNewHeader.Paragraphs[idx];
            if (string.IsNullOrWhiteSpace(first.Text))
            {
                return new Paragraph("Example text", 0, 10000);
            }

            return new Paragraph(first) { StartTime = new TimeCode(0), EndTime = new TimeCode(10000) };
        }

        private Process GetFfmpegProcess(string inputVideoFileName, string outputVideoFileName, string assaTempFileName, int? passNumber = null, string twoPassBitRate = null)
        {
            var pass = string.Empty;
            if (passNumber.HasValue)
            {
                pass = passNumber.Value.ToString(CultureInfo.InvariantCulture);
            }

            return VideoPreviewGenerator.GenerateHardcodedVideoFile(
                inputVideoFileName,
                assaTempFileName,
                outputVideoFileName,
                _videoInfo.Width,
                _videoInfo.Height,
                "libx264",
                null,
                null,
                null,
                false,
                null,
                null,
                null,
                pass,
                twoPassBitRate);
        }

        private void PreviewValueChanged(object sender, EventArgs e)
        {
            _updatePreview = true;
        }

        private void buttonPrimaryColor_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorChooser();
            colorDialog.Color = panelPrimaryColor.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _boxColor = colorDialog.Color;
                _updatePreview = true;
                panelPrimaryColor.BackColor = colorDialog.Color;
            }
        }

        private void panelPrimaryColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonPrimaryColor_Click(null, null);
        }

        private void buttonShadowColor_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorChooser();
            colorDialog.Color = panelShadowColor.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _boxShadowColor = colorDialog.Color;
                _updatePreview = true;
                panelShadowColor.BackColor = colorDialog.Color;
            }
        }

        private void panelShadowColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonShadowColor_Click(null, null);
        }

        private void buttonOutlineColor_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorChooser();
            colorDialog.Color = panelOutlineColor.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _boxOutlineColor = colorDialog.Color;
                _updatePreview = true;
                panelOutlineColor.BackColor = colorDialog.Color;
            }
        }

        private void panelOutlineColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonOutlineColor_Click(null, null);
        }

        private void checkBoxFillWidth_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownPaddingLeft.Enabled = !checkBoxFillWidth.Checked;
            numericUpDownPaddingRight.Enabled = !checkBoxFillWidth.Checked;
            _updatePreview = true;
        }

        private void comboBoxBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (comboBoxBoxStyle.SelectedIndex == 1) // rounded
            {
                panelStyleSpikes.Visible = false;
                panelStyleRounded.Top = comboBoxBoxStyle.Top + comboBoxBoxStyle.Height + 9;
                panelStyleRounded.Visible = true;
                panelStyleRounded.BringToFront();
            }
            else if (comboBoxBoxStyle.SelectedIndex == 2) // spikes
            {
                panelStyleRounded.Visible = false;
                panelStyleSpikes.Top = comboBoxBoxStyle.Top + comboBoxBoxStyle.Height + 9;
                panelStyleSpikes.Visible = true;
                panelStyleSpikes.BringToFront();
            }
            else // square
            {
                panelStyleRounded.Visible = false;
                panelStyleSpikes.Visible = false;
            }

            groupBoxPreview.BringToFront();
            _updatePreview = true;
        }

        private void buttonChooseDrawing_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = "ASSA/AssaDraw files|*.ass;*.assadraw";
                openFileDialog1.FileName = string.Empty;
                if (string.IsNullOrEmpty(openFileDialog1.InitialDirectory) && !string.IsNullOrEmpty(_subtitleWithNewHeader.FileName))
                {
                    openFileDialog1.InitialDirectory = Path.GetDirectoryName(_subtitleWithNewHeader.FileName);
                }

                if (!string.IsNullOrEmpty(labelFileName.Text))
                {
                    openFileDialog1.InitialDirectory = Path.GetDirectoryName(labelFileName.Text);
                    openFileDialog1.FileName = labelFileName.Text;
                }

                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                ReadDrawingFile(openFileDialog1.FileName);
                _updatePreview = true;
            }
        }

        private void ReadDrawingFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            {
                _drawing = null;
                labelFileName.Text = string.Empty;
                return;
            }

            _drawingFileWatcher?.Dispose();
            _drawing = Subtitle.Parse(fileName, new AdvancedSubStationAlpha());
            if (_drawing == null)
            {
                Application.DoEvents();
                _drawing = Subtitle.Parse(fileName, new AdvancedSubStationAlpha());
            }

            if (_drawing != null && _drawing.Paragraphs.Count > 0 && _drawing.Paragraphs.Count < 200)
            {
                labelFileName.Text = fileName;

                if (Configuration.Settings.Tools.AssaBgBoxDrawingFileWatch)
                {
                    _drawingFileWatcher = new FileSystemWatcher();
                    _drawingFileWatcher.Path = Path.GetDirectoryName(fileName);
                    _drawingFileWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
                    _drawingFileWatcher.Filter = Path.GetFileName(fileName);
                    _drawingFileWatcher.Changed += OnChanged;
                    _drawingFileWatcher.Created += OnChanged;
                    _drawingFileWatcher.EnableRaisingEvents = true;
                }
            }
            else
            {
                _drawing = null;
                labelFileName.Text = string.Empty;
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            _updateDrawingFile = true;
        }

        private void buttonDrawingClear_Click(object sender, EventArgs e)
        {
            _drawingFileWatcher?.Dispose();
            _drawing = null;
            labelFileName.Text = string.Empty;
            _updatePreview = true;
        }

        private void buttonAssaDraw_Click(object sender, EventArgs e)
        {
            var assaDrawPluginFileName = Path.Combine(Configuration.PluginsDirectory, "AssaDraw.dll");
            var pluginObject = Main.GetPropertiesAndDoAction(assaDrawPluginFileName, out var name, out var text, out var version, out var description, out var actionType, out var shortcut, out var mi);
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(actionType) && mi != null)
            {
                mi.Invoke(pluginObject,
                    new object[]
                    {
                        this,
                        "standalone",
                        Configuration.Settings.General.CurrentFrameRate,
                        Configuration.Settings.General.ListViewLineSeparatorString,
                        _drawing != null ? _drawing.FileName : string.Empty,
                        string.Empty,
                        string.Empty,
                    });
            }
        }

        private void checkBoxNoBox_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxStyle.Enabled = !checkBoxOnlyDrawing.Checked;
            buttonPrimaryColor.Enabled = !checkBoxOnlyDrawing.Checked;
            buttonOutlineColor.Enabled = !checkBoxOnlyDrawing.Checked;
            buttonShadowColor.Enabled = !checkBoxOnlyDrawing.Checked;
            numericUpDownShadowDistance.Enabled = !checkBoxOnlyDrawing.Checked;
            numericUpDownOutlineWidth.Enabled = !checkBoxOnlyDrawing.Checked;

            _updatePreview = true;
        }

        private void timerFileChange_Tick(object sender, EventArgs e)
        {
            if (_updateDrawingFile)
            {
                _updateDrawingFile = false;
                ReadDrawingFile(labelFileName.Text);
                _updatePreview = true;
            }
        }

        private void AssSetBackground_Load(object sender, EventArgs e)
        {

        }
    }
}

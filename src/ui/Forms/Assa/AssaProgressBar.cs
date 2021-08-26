using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public sealed partial class AssaProgressBar : Form
    {
        private readonly List<AssaAttachmentFont> _fontAttachments;
        private readonly Subtitle _subtitle;
        private Subtitle _progessBarSubtitle;
        private readonly string _videoFileName;
        private readonly VideoInfo _videoInfo;
        private readonly Timer _timerRender;
        private int _oldHashValue = -1;
        private readonly Subtitle _chapters;
        private readonly Controls.VideoPlayerContainer _videoPlayerContainer;

        public AssaProgressBar(Subtitle subtitle, string videoFileName, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitle = subtitle;
            _videoFileName = videoFileName;
            _videoInfo = videoInfo;
            _chapters = new Subtitle();

            var l = LanguageSettings.Current.AssaProgressBarGenerator;
            Text = l.Title;
            groupBoxStyle.Text = l.Progressbar;
            labelPosition.Text = l.Position;
            radioButtonPosBottom.Text = l.Bottom;
            radioButtonPosTop.Text = l.Top;
            labelHeight.Text = LanguageSettings.Current.General.Height;
            buttonForeColor.Text = LanguageSettings.Current.Settings.WaveformColor;
            buttonSecondaryColor.Text = LanguageSettings.Current.Settings.SubtitleBackgroundColor;
            labelEdgeStyle.Text = LanguageSettings.Current.General.Style;
            groupBoxChapters.Text = l.Chapters;
            labelSplitterWidth.Text = l.SplitterWidth;
            labelSplitterHeight.Text = l.SplitterHeight;
            labelFontName.Text = LanguageSettings.Current.ExportPngXml.FontFamily;
            labelFontSize.Text = LanguageSettings.Current.ExportPngXml.FontSize;
            labelXAdjust.Text = l.XAdjustment;
            labelYAdjust.Text = l.YAdjustment;
            labelTextHorizontalAlignment.Text = l.TextAlignment;
            labelStartTime.Text = LanguageSettings.Current.General.StartTime;
            labelText.Text = LanguageSettings.Current.General.Text;
            buttonAdd.Text = LanguageSettings.Current.SubStationAlphaStyles.New;
            buttonRemove.Text = LanguageSettings.Current.SubStationAlphaStyles.Remove;
            buttonRemoveAll.Text = LanguageSettings.Current.SubStationAlphaStyles.RemoveAll;
            buttonTextColor.Text = LanguageSettings.Current.Settings.WaveformTextColor;
            columnHeaderName.Text = LanguageSettings.Current.General.Text;
            columnHeaderStart.Text = LanguageSettings.Current.General.StartTime;

            comboBoxTextHorizontalAlignment.Items.Clear();
            comboBoxTextHorizontalAlignment.Items.Add(LanguageSettings.Current.ExportPngXml.Left);
            comboBoxTextHorizontalAlignment.Items.Add(LanguageSettings.Current.ExportPngXml.Center);
            comboBoxTextHorizontalAlignment.Items.Add(LanguageSettings.Current.ExportPngXml.Right);

            comboBoxProgressBarEdge.Items.Clear();
            comboBoxProgressBarEdge.Items.Add(l.SquareCorners);
            comboBoxProgressBarEdge.Items.Add(l.RoundedCorners);

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            comboBoxFontName.Items.Clear();
            foreach (var font in FontFamily.Families)
            {
                comboBoxFontName.Items.Add(font.Name);
                if (font.Name == "Arial")
                {
                    comboBoxFontName.SelectedIndex = comboBoxFontName.Items.Count - 1;
                }
            }
            if (comboBoxFontName.SelectedIndex == -1 && comboBoxFontName.Items.Count > 0)
            {
                comboBoxFontName.SelectedIndex = 1;
            }

            InitializeFromSettings();

            _videoPlayerContainer = new Controls.VideoPlayerContainer();
            Controls.Add(_videoPlayerContainer);
            _videoPlayerContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _videoPlayerContainer.Location = new Point(401, 12);
            _videoPlayerContainer.Name = "_videoPlayerContainer";
            _videoPlayerContainer.Size = new Size(923, buttonOK.Top - 18);

            _fontAttachments = new List<AssaAttachmentFont>();
            if (subtitle.Footer != null)
            {
                GetFonts(subtitle.Footer.SplitToLines());
            }
            buttonPickAttachmentFont.Visible = _fontAttachments.Count > 0;

            listViewChapters_SelectedIndexChanged(null, null);

            var left = labelPosition.Left + Math.Max(labelPosition.Width, labelHeight.Width) + 10;
            radioButtonPosBottom.Left = left;
            radioButtonPosTop.Left = left + radioButtonPosBottom.Width + 10;
            numericUpDownHeight.Left = left;
            buttonForeColor.Left = left;
            panelPrimaryColor.Left = left + buttonForeColor.Width + 4;
            buttonSecondaryColor.Left = left;
            panelSecondaryColor.Left = left + buttonSecondaryColor.Width + 4;
            comboBoxProgressBarEdge.Left = left;
            comboBoxProgressBarEdge.SelectedIndex = 0;

            left = Math.Max(Math.Max(labelSplitterHeight.Width, labelTextHorizontalAlignment.Width), Math.Max(labelFontName.Width, labelFontSize.Width)) + 12;
            numericUpDownSplitterWidth.Left = left;
            labelSplitterHeight.Left = numericUpDownSplitterWidth.Left + numericUpDownSplitterWidth.Width + 15;
            numericUpDownSplitterHeight.Left = labelSplitterHeight.Left + labelSplitterHeight.Width + 4;
            comboBoxFontName.Left = left;
            buttonPickAttachmentFont.Left = left + comboBoxFontName.Width + 4;
            numericUpDownFontSize.Left = left;
            buttonTextColor.Left = left;
            panelTextColor.Left = left + buttonTextColor.Width + 4;
            numericUpDownXAdjust.Left = left;
            labelYAdjust.Left = numericUpDownXAdjust.Left + numericUpDownXAdjust.Width + 10;
            numericUpDownYAdjust.Left = labelYAdjust.Left + labelYAdjust.Width + 4;
            comboBoxTextHorizontalAlignment.Left = left;
            buttonTakePosFromVideo.Left = timeUpDownStartTime.Left + timeUpDownStartTime.Width + 10;

            LoadExistingProgressBarSettings();

            _timerRender = new Timer { Interval = 100 };
            _timerRender.Tick += TimerRenderTick;
        }

        private void InitializeFromSettings()
        {
            panelPrimaryColor.BackColor = Configuration.Settings.Tools.AssaProgressBarForeColor;
            panelSecondaryColor.BackColor = Configuration.Settings.Tools.AssaProgressBarBackColor;
            panelTextColor.BackColor = Configuration.Settings.Tools.AssaProgressBarTextColor;
            if (Configuration.Settings.Tools.AssaProgressBarHeight >= numericUpDownHeight.Minimum &&
                Configuration.Settings.Tools.AssaProgressBarHeight <= numericUpDownHeight.Maximum)
            {
                numericUpDownHeight.Value = Configuration.Settings.Tools.AssaProgressBarHeight;
            }

            if (Configuration.Settings.Tools.AssaProgressBarSplitterWidth >= numericUpDownSplitterWidth.Minimum &&
                Configuration.Settings.Tools.AssaProgressBarSplitterWidth <= numericUpDownSplitterWidth.Maximum)
            {
                numericUpDownSplitterWidth.Value = Configuration.Settings.Tools.AssaProgressBarSplitterWidth;
            }

            if (Configuration.Settings.Tools.AssaProgressBarSplitterHeight >= numericUpDownSplitterHeight.Minimum &&
                Configuration.Settings.Tools.AssaProgressBarSplitterHeight <= numericUpDownSplitterHeight.Maximum)
            {
                numericUpDownSplitterHeight.Value = Configuration.Settings.Tools.AssaProgressBarSplitterHeight;
            }

            if (Configuration.Settings.Tools.AssaProgressBarFontSize >= numericUpDownFontSize.Minimum &&
                Configuration.Settings.Tools.AssaProgressBarFontSize <= numericUpDownFontSize.Maximum)
            {
                numericUpDownFontSize.Value = Configuration.Settings.Tools.AssaProgressBarFontSize;
            }

            if (Configuration.Settings.Tools.AssaProgressBarTopAlign)
            {
                radioButtonPosTop.Checked = true;
            }
            else
            {
                radioButtonPosBottom.Checked = true;
            }

            if (Configuration.Settings.Tools.AssaProgressBarTextAlign == "center")
            {
                comboBoxTextHorizontalAlignment.SelectedIndex = 1;
            }
            else if (Configuration.Settings.Tools.AssaProgressBarTextAlign == "right")
            {
                comboBoxTextHorizontalAlignment.SelectedIndex = 2;
            }
            else
            {
                comboBoxTextHorizontalAlignment.SelectedIndex = 0;
            }
        }

        private void LoadExistingProgressBarSettings()
        {
            //TODO: splitter height + width
            foreach (var p in _subtitle.Paragraphs)
            {
                if (p.Extra == "SE-progress-bar-text")
                {
                    var newParagraph = new Paragraph(p)
                    {
                        Text = Utilities.RemoveSsaTags(p.Text)
                    };

                    if (double.TryParse(p.Actor, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
                    {
                        newParagraph.StartTime.TotalMilliseconds = number;
                    }

                    _chapters.Paragraphs.Add(newParagraph);
                }
            }

            foreach (var style in AdvancedSubStationAlpha.GetSsaStylesFromHeader(_subtitle.Header))
            {
                if (style.Name == "SE-progress-bar-text")
                {
                    comboBoxFontName.Text = style.FontName;
                    numericUpDownFontSize.Value = (decimal)style.FontSize;
                    panelTextColor.BackColor = style.Primary;
                }
                else if (style.Name == "SE-progress-bar-bg")
                {
                    panelPrimaryColor.BackColor = style.Primary;
                    panelSecondaryColor.BackColor = style.Secondary;

                    if (style.Alignment != "7")
                    {
                        radioButtonPosBottom.Checked = true;
                    }
                    else
                    {
                        radioButtonPosTop.Checked = true;
                    }

                }
            }

            RefreshListView();
        }

        private void GetFonts(List<string> lines)
        {
            bool attachmentOn = false;
            var attachmentContent = new StringBuilder();
            var attachmentFileName = string.Empty;
            var category = string.Empty;
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (attachmentOn)
                {
                    if (s == "[V4+ Styles]" || s == "[Events]")
                    {
                        SaveFontNames(attachmentFileName, attachmentContent.ToString(), category);
                        attachmentOn = false;
                        attachmentContent = new StringBuilder();
                        attachmentFileName = string.Empty;
                    }
                    else if (s.Length == 0)
                    {
                        SaveFontNames(attachmentFileName, attachmentContent.ToString(), category);
                        attachmentContent = new StringBuilder();
                        attachmentFileName = string.Empty;
                    }
                    else if (s.StartsWith("filename:", StringComparison.Ordinal) || s.StartsWith("fontname:", StringComparison.Ordinal))
                    {
                        SaveFontNames(attachmentFileName, attachmentContent.ToString(), category);
                        attachmentContent = new StringBuilder();
                        attachmentFileName = s.Remove(0, 9).Trim();
                    }
                    else
                    {
                        attachmentContent.AppendLine(s);
                    }
                }
                else if (s == "[Fonts]" || s == "[Graphics]")
                {
                    category = s;
                    attachmentOn = true;
                    attachmentContent = new StringBuilder();
                    attachmentFileName = string.Empty;
                }
            }

            SaveFontNames(attachmentFileName, attachmentContent.ToString(), category);
        }

        private void SaveFontNames(string attachmentFileName, string attachmentContent, string category)
        {
            var content = attachmentContent.Trim();
            if (string.IsNullOrEmpty(attachmentFileName) || content.Length == 0 || !attachmentFileName.EndsWith(".ttf", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var bytes = UUEncoding.UUDecode(content);
            foreach (var fontName in GetFontNames(bytes))
            {
                _fontAttachments.Add(new AssaAttachmentFont { FileName = attachmentFileName, FontName = fontName, Bytes = bytes, Category = category, Content = content });
            }
        }

        private List<string> GetFontNames(byte[] fontBytes)
        {
            var privateFontCollection = new PrivateFontCollection();
            var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
            var pointer = handle.AddrOfPinnedObject();
            try
            {
                privateFontCollection.AddMemoryFont(pointer, fontBytes.Length);
            }
            finally
            {
                handle.Free();
            }

            var resultList = new List<string>();
            foreach (var font in privateFontCollection.Families)
            {
                resultList.Add(font.Name);
            }

            privateFontCollection.Dispose();
            return resultList;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_subtitle.Header))
            {
                _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
            }

            var newStyles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(_progessBarSubtitle.Header);
            var ignoreStyleNames = new List<string> { "SE-progress-bar-text", "SE-progress-bar-splitter", "SE-progress-bar-bg" };
            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(_subtitle.Header).Where(p => !ignoreStyleNames.Contains(p.Name)).ToList();
            styles.AddRange(newStyles);
            _subtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(_subtitle.Header, styles);

            for (int i = _subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                if (ignoreStyleNames.Contains(_subtitle.Paragraphs[i].Extra))
                {
                    _subtitle.Paragraphs.RemoveAt(i);
                }
            }

            _subtitle.Paragraphs.AddRange(_progessBarSubtitle.Paragraphs);

            if (_videoInfo != null && _videoInfo.Width > 0 && _videoInfo.Height > 0)
            {
                _subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + _videoInfo.Width.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitle.Header);
                _subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + _videoInfo.Height.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitle.Header);
            }


            Configuration.Settings.Tools.AssaProgressBarForeColor = panelPrimaryColor.BackColor;
            Configuration.Settings.Tools.AssaProgressBarBackColor = panelSecondaryColor.BackColor;
            Configuration.Settings.Tools.AssaProgressBarTextColor = panelTextColor.BackColor;
            Configuration.Settings.Tools.AssaProgressBarHeight = (int)numericUpDownHeight.Value;
            Configuration.Settings.Tools.AssaProgressBarSplitterWidth = (int)numericUpDownSplitterWidth.Value;
            Configuration.Settings.Tools.AssaProgressBarSplitterHeight = (int)numericUpDownSplitterHeight.Value;
            Configuration.Settings.Tools.AssaProgressBarFontName = comboBoxFontName.Text;
            Configuration.Settings.Tools.AssaProgressBarFontSize = (int)numericUpDownFontSize.Value;
            if (comboBoxTextHorizontalAlignment.SelectedIndex == 1)
            {
                Configuration.Settings.Tools.AssaProgressBarTextAlign = "center";
            }
            else if (comboBoxTextHorizontalAlignment.SelectedIndex == 2)
            {
                Configuration.Settings.Tools.AssaProgressBarTextAlign = "right";
            }
            else
            {
                Configuration.Settings.Tools.AssaProgressBarTextAlign = "left";
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonPickAttachmentFont_Click(object sender, EventArgs e)
        {
            using (var form = new ChooseAssaFontName(_fontAttachments))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    comboBoxFontName.Text = form.FontName;
                }
            }
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            _chapters.Paragraphs.Clear();
            RefreshListView();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listViewChapters.SelectedItems.Count == 0)
            {
                return;
            }

            foreach (ListViewItem selectedItem in listViewChapters.SelectedItems)
            {
                int index = selectedItem.Index;
                _chapters.Paragraphs.RemoveAt(index);
            }

            RefreshListView();
        }

        private void buttonForeColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelPrimaryColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelPrimaryColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonSecondaryColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelSecondaryColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelSecondaryColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void panelPrimaryColor_Click(object sender, EventArgs e)
        {
            buttonForeColor_Click(null, null);
        }

        private void panelSecondaryColor_Click(object sender, EventArgs e)
        {
            buttonSecondaryColor_Click(null, null);
        }

        private void OpenVideo(string fileName)
        {
            if (File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length < 1000)
                {
                    return;
                }

                if (_videoPlayerContainer.VideoPlayer != null)
                {
                    _videoPlayerContainer.Pause();
                    _videoPlayerContainer.VideoPlayer.DisposeVideoPlayer();
                }

                VideoInfo videoInfo = UiUtil.GetVideoInfo(fileName);
                UiUtil.InitializeVideoPlayerAndContainer(fileName, videoInfo, _videoPlayerContainer, VideoStartLoaded, VideoStartEnded);
            }
        }

        private void VideoStartEnded(object sender, EventArgs e)
        {
            _videoPlayerContainer.Pause();
        }

        private void VideoStartLoaded(object sender, EventArgs e)
        {
            _videoPlayerContainer.Pause();
            _timerRender.Start();
        }

        private void AssaProgressBar_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timerRender.Stop();
            Application.DoEvents();
            _videoPlayerContainer?.Pause();
            _videoPlayerContainer?.VideoPlayer?.DisposeVideoPlayer();
        }

        private void AssaProgressBar_Shown(object sender, EventArgs e)
        {
            OpenVideo(_videoFileName);
            timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBox_TextChanged;
            textBoxChapterText.TextChanged += TextBoxChapterText_TextChanged;
        }

        private void TextBoxChapterText_TextChanged(object sender, EventArgs e)
        {
            if (listViewChapters.SelectedItems.Count == 0)
            {
                return;
            }

            var p = (Paragraph)listViewChapters.SelectedItems[0].Tag;
            p.Text = textBoxChapterText.Text;
            listViewChapters.SelectedItems[0].Text = p.Text;
        }

        private void MaskedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (listViewChapters.SelectedItems.Count == 0)
            {
                return;
            }

            var p = (Paragraph)listViewChapters.SelectedItems[0].Tag;
            p.StartTime.TotalMilliseconds = timeUpDownStartTime.TimeCode.TotalMilliseconds;
            listViewChapters.SelectedItems[0].SubItems[1].Text = p.StartTime.ToDisplayString();
        }

        private void TimerRenderTick(object sender, EventArgs e)
        {
            _progessBarSubtitle = new Subtitle();
            var script = @"[Script Info]
; This is an Advanced Sub Station Alpha v4+script.
ScriptType: v4.00+
Collisions: Normal
PlayResX: [VIDEO_WIDTH]
PlayResY: [VIDEO_HEIGHT]

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: SE-progress-bar-splitter,Arial,20,[TEXT_COLOR],&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,0,0,7,0,0,0,1
Style: SE-progress-bar-bg,Arial,20,[FORE_COLOR],[BACK_COLOR],&H00FFFFFF,&H00000000,0,0,0,0,100,100,0,0,1,0,0,[ALIGNMENT],0,0,0,1
Style: SE-progress-bar-text,[FONT_NAME],[FONT_SIZE],[TEXT_COLOR],&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,0,0,[TEXT_ALIGNMENT],0,0,0,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: -255,0:00:00.00,0:43:00.00,SE-progress-bar-bg,,0,0,0,,[PB_DRAWING]";

            script = script.Replace("[VIDEO_WIDTH]", _videoInfo.Width.ToString(CultureInfo.InvariantCulture));
            script = script.Replace("[VIDEO_HEIGHT]", _videoInfo.Height.ToString(CultureInfo.InvariantCulture));

            script = script.Replace("[FORE_COLOR]", AdvancedSubStationAlpha.GetSsaColorString(panelPrimaryColor.BackColor));
            script = script.Replace("[BACK_COLOR]", AdvancedSubStationAlpha.GetSsaColorString(panelSecondaryColor.BackColor));
            script = script.Replace("[TEXT_COLOR]", AdvancedSubStationAlpha.GetSsaColorString(panelTextColor.BackColor));

            script = script.Replace("[FONT_SIZE]", numericUpDownFontSize.Value.ToString(CultureInfo.InvariantCulture));
            script = script.Replace("[FONT_NAME]", comboBoxFontName.Text);

            var duration = (int)Math.Round(_videoInfo.TotalMilliseconds / 10.0);
            script = script.Replace("[PB_DRAWING]", GenerateProgressBar(_videoInfo, (int)numericUpDownHeight.Value, duration));

            if (radioButtonPosTop.Checked)
            {
                script = script.Replace("[ALIGNMENT]", "7");
            }
            else
            {
                script = script.Replace("[ALIGNMENT]", "1");
            }

            if (comboBoxTextHorizontalAlignment.SelectedIndex == 1)
            {
                script = script.Replace("[TEXT_ALIGNMENT]", "5");
            }
            else if (comboBoxTextHorizontalAlignment.SelectedIndex == 2)
            {
                script = script.Replace("[TEXT_ALIGNMENT]", "6");
            }
            else
            {
                script = script.Replace("[TEXT_ALIGNMENT]", "4");
            }

            new AdvancedSubStationAlpha().LoadSubtitle(_progessBarSubtitle, script.SplitToLines(), string.Empty);

            _progessBarSubtitle.Paragraphs[0].EndTime.TotalMilliseconds = _videoInfo.TotalMilliseconds;

            if (_chapters?.Paragraphs.Count > 0)
            {
                var layer = -254;
                using (var graphics = CreateGraphics())
                {
                    for (int i = 0; i < _chapters.Paragraphs.Count; i++)
                    {
                        Paragraph p = _chapters.Paragraphs[i];
                        var percentOfDuration = p.StartTime.TotalMilliseconds * 100.0 / _videoInfo.TotalMilliseconds;
                        var position = (int)Math.Round(_videoInfo.Width * percentOfDuration / 100.0);
                        var textPosition = GetTextPosition(position, p, i, percentOfDuration);
                        string posTag;
                        string splitterText;
                        SizeF chapterSize;
                        try
                        {
                            using (var font = new Font(comboBoxFontName.Text, (float)(numericUpDownFontSize.Value * 0.7m), FontStyle.Regular))
                            {
                                chapterSize = graphics.MeasureString(p.Text, font);
                            }
                        }
                        catch
                        {
                            using (var font = new Font(Font.Name, (float)(numericUpDownFontSize.Value * 0.7m), FontStyle.Regular))
                            {
                                chapterSize = graphics.MeasureString(p.Text, font);
                            }
                        }

                        if (radioButtonPosTop.Checked)
                        {
                            var top = (int)Math.Round((numericUpDownHeight.Value / 2.0m) + numericUpDownYAdjust.Value);
                            posTag = $"{{\\pos({textPosition}, {top})}}";

                            var splitterTop = 0;
                            if (numericUpDownSplitterHeight.Value < numericUpDownHeight.Value)
                            {
                                splitterTop += (int)Math.Round((numericUpDownHeight.Value - numericUpDownSplitterHeight.Value) / 2);
                            }
                            splitterText = $"{{\\p1}}m {position} {splitterTop} l " + // top left point
                                $"{position + numericUpDownSplitterWidth.Value} {splitterTop} " +  // top right point
                                $"{position + numericUpDownSplitterWidth.Value} {splitterTop + numericUpDownSplitterHeight.Value} " + // bottom right point
                                $"{position} {splitterTop + numericUpDownSplitterHeight.Value} " + // bottom left point
                                $"{position} {splitterTop}" + // top left point
                                "{{\\p0}}";

                        }
                        else
                        {
                            posTag = $"{{\\pos({textPosition}, {(int)Math.Round(_videoInfo.Height - (numericUpDownHeight.Value / 2.0m) + numericUpDownYAdjust.Value)})}}";

                            var splitterTop = _videoInfo.Height - numericUpDownHeight.Value;
                            if (numericUpDownSplitterHeight.Value < numericUpDownHeight.Value)
                            {
                                splitterTop += (int)Math.Round((numericUpDownHeight.Value - numericUpDownSplitterHeight.Value) / 2);
                            }
                            splitterText = $"{{\\p1}}m {position} {splitterTop} l " + // top left point
                                $"{position + numericUpDownSplitterWidth.Value} {splitterTop} " +  // top right point
                                $"{position + numericUpDownSplitterWidth.Value} {splitterTop + numericUpDownSplitterHeight.Value} " + // bottom right point
                                $"{position} {splitterTop + numericUpDownSplitterHeight.Value} " + // bottom left point
                                $"{position} {splitterTop}" + // top left point
                                "{{\\p0}}";
                        }

                        if (i > 0)
                        {
                            var splitter = new Paragraph(splitterText, 0, _videoInfo.TotalMilliseconds)
                            {
                                Extra = "SE-progress-bar-splitter",
                                Layer = layer,
                            };
                            layer++;
                            _progessBarSubtitle.Paragraphs.Add(splitter);
                        }

                        var chapterInfo = new Paragraph(posTag + p.Text, 0, _videoInfo.TotalMilliseconds)
                        {
                            Extra = "SE-progress-bar-text",
                            Layer = -1,
                            Actor = p.StartTime.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                        };
                        _progessBarSubtitle.Paragraphs.Add(chapterInfo);
                    }
                }
            }

            textBoxSource.Text = new AdvancedSubStationAlpha().ToText(_progessBarSubtitle, string.Empty);


            var hashValue = _progessBarSubtitle.GetFastHashCode(string.Empty);
            if (hashValue != _oldHashValue)
            {
                _videoPlayerContainer.SetSubtitleText(string.Empty, new Paragraph(), _progessBarSubtitle);
                _oldHashValue = hashValue;
            }

            _videoPlayerContainer.RefreshProgressBar();
        }

        private string GenerateProgressBar(VideoInfo videoInfo, int height, int duration)
        {
            if (comboBoxProgressBarEdge.SelectedIndex == 1) // rounded corners
            {
                var barEnd = videoInfo.Width - height;
                var w = videoInfo.Width;
                return $@"{{\K{duration}\p1}}m {height} 0 b 0 0 0 {height} {height} {height} l {barEnd} {height} b {w} {height} {w} 0 {barEnd} 0 l  {barEnd} 0 {height} 0{{\p0}}";
            }

            return $"{{\\K{duration}\\p1}}m 0 0 l {videoInfo.Width} 0 {videoInfo.Width} {height} 0 {height}{{\\p0}}";
        }

        private int GetTextPosition(int position, Paragraph p, int i, double percentOfDuration)
        {
            if (comboBoxTextHorizontalAlignment.SelectedIndex == 1) // center
            {
                var positionNextX = _videoInfo.Width * 100.0 / 100.0;
                var next = _chapters.GetParagraphOrDefault(i + 1);
                if (next != null)
                {
                    var percentOfDurationNext = next.StartTime.TotalMilliseconds * 100.0 / _videoInfo.TotalMilliseconds;
                    positionNextX = (int)Math.Round(_videoInfo.Width * percentOfDurationNext / 100.0);
                }
                return (int)Math.Round((position + positionNextX) / 2.0 + (double)numericUpDownXAdjust.Value);
            }
            else if (comboBoxTextHorizontalAlignment.SelectedIndex == 2) // right
            {
                var positionNextX = _videoInfo.Width * 100.0 / 100.0;
                var next = _chapters.GetParagraphOrDefault(i + 1);
                if (next != null)
                {
                    var percentOfDurationNext = next.StartTime.TotalMilliseconds * 100.0 / _videoInfo.TotalMilliseconds;
                    positionNextX = (int)Math.Round(_videoInfo.Width * percentOfDurationNext / 100.0);
                }

                if (comboBoxProgressBarEdge.SelectedIndex == 1 && i == _chapters.Paragraphs.Count - 1 && comboBoxTextHorizontalAlignment.SelectedIndex == 2) // Rounded corners + last paragraph + align text right
                {
                    return (int)Math.Round(positionNextX - (double)numericUpDownHeight.Value + (double)numericUpDownXAdjust.Value);
                }

                return (int)Math.Round(positionNextX - 10.0 + (double)numericUpDownXAdjust.Value);
            }

            if (comboBoxProgressBarEdge.SelectedIndex == 1 && i == 0 && comboBoxTextHorizontalAlignment.SelectedIndex == 0) // Rounded corners + first paragraph + align text left
            {
                return (int)Math.Round(position + numericUpDownHeight.Value + numericUpDownSplitterWidth.Value + numericUpDownXAdjust.Value);
            }            

            return (int)Math.Round(position + 8 + numericUpDownSplitterWidth.Value + numericUpDownXAdjust.Value);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (_chapters.Paragraphs.Count > 0)
            {
                var startTimeMs = _chapters.Paragraphs.Last().StartTime.TotalMilliseconds + 60000 * 5;
                var videoMs = _videoPlayerContainer.CurrentPosition * 1000.0;
                if (videoMs - 999 > _chapters.Paragraphs.Last().StartTime.TotalMilliseconds)
                {
                    startTimeMs = videoMs;
                }

                _chapters.Paragraphs.Add(new Paragraph("Text", startTimeMs, startTimeMs));
            }
            else
            {
                _chapters.Paragraphs.Add(new Paragraph("Text", 0, 0));
            }

            RefreshListView();
            listViewChapters.SelectedItems.Clear();
            listViewChapters.Items[listViewChapters.Items.Count - 1].Selected = true;
            listViewChapters.Items[listViewChapters.Items.Count - 1].Focused = true;
        }

        private void RefreshListView()
        {
            listViewChapters.BeginUpdate();
            listViewChapters.Items.Clear();
            foreach (var p in _chapters.Paragraphs)
            {

                var item = new ListViewItem(p.Text) { Tag = p };
                item.SubItems.Add(p.StartTime.ToDisplayString());
                listViewChapters.Items.Add(item);
            }

            listViewChapters.EndUpdate();
        }

        private void AssaProgressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.F2)
            {
                e.SuppressKeyPress = true;
                if (textBoxSource.Visible)
                {
                    textBoxSource.Visible = false;
                }
                else
                {
                    textBoxSource.Visible = true;
                    textBoxSource.BringToFront();
                }
            }
        }

        private void buttonTextColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelTextColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelTextColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void panelTextColor_Click(object sender, EventArgs e)
        {
            buttonTextColor_Click(null, null);
        }

        private void listViewChapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewChapters.SelectedItems.Count == 0)
            {
                labelStartTime.Enabled = false;
                labelText.Enabled = false;
                timeUpDownStartTime.Enabled = false;
                timeUpDownStartTime.MaskedTextBox.Enabled = false;
                textBoxChapterText.Enabled = false;
                textBoxChapterText.Text = string.Empty;
                buttonTakePosFromVideo.Enabled = false;
                return;
            }

            labelStartTime.Enabled = true;
            labelText.Enabled = true;
            timeUpDownStartTime.Enabled = true;
            timeUpDownStartTime.MaskedTextBox.Enabled = true;
            textBoxChapterText.Enabled = true;
            buttonTakePosFromVideo.Enabled = true;

            var p = (Paragraph)listViewChapters.SelectedItems[0].Tag;
            timeUpDownStartTime.TimeCode = new TimeCode(p.StartTime.TotalMilliseconds);
            textBoxChapterText.Text = p.Text;
        }

        private void listViewChapters_DoubleClick(object sender, EventArgs e)
        {
            if (listViewChapters.SelectedItems.Count == 1)
            {

                var p = (Paragraph)listViewChapters.SelectedItems[0].Tag;
                _videoPlayerContainer.CurrentPosition = p.StartTime.TotalSeconds;
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            var defaultToolsSettings = new ToolsSettings();
            Configuration.Settings.Tools.AssaProgressBarForeColor = defaultToolsSettings.AssaProgressBarForeColor;
            Configuration.Settings.Tools.AssaProgressBarBackColor = defaultToolsSettings.AssaProgressBarBackColor;
            Configuration.Settings.Tools.AssaProgressBarTextColor = defaultToolsSettings.AssaProgressBarTextColor;
            Configuration.Settings.Tools.AssaProgressBarHeight = defaultToolsSettings.AssaProgressBarHeight;
            Configuration.Settings.Tools.AssaProgressBarSplitterWidth = defaultToolsSettings.AssaProgressBarSplitterWidth;
            Configuration.Settings.Tools.AssaProgressBarSplitterHeight = defaultToolsSettings.AssaProgressBarSplitterHeight;
            Configuration.Settings.Tools.AssaProgressBarFontName = defaultToolsSettings.AssaProgressBarFontName;
            Configuration.Settings.Tools.AssaProgressBarFontSize = defaultToolsSettings.AssaProgressBarFontSize;
            Configuration.Settings.Tools.AssaProgressBarTopAlign = defaultToolsSettings.AssaProgressBarTopAlign;
            numericUpDownYAdjust.Value = 0;
            InitializeFromSettings();
        }

        private void listViewChapters_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                buttonRemove_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.OpenUrl("https://www.nikse.dk/SubtitleEdit/AssaOverrideTags#pos");
                e.SuppressKeyPress = true;
            }
        }

        private void buttonTakePosFromVideo_Click(object sender, EventArgs e)
        {
            if (listViewChapters.SelectedItems.Count != 1)
            {
                return;
            }

            var p = (Paragraph)listViewChapters.SelectedItems[0].Tag;
            p.StartTime.TotalMilliseconds = _videoPlayerContainer.CurrentPosition * 1000.0;
            listViewChapters.SelectedItems[0].SubItems[1].Text = p.StartTime.ToDisplayString();
        }

        private void numericUpDownHeight_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownHeight.Value < numericUpDownSplitterHeight.Value)
            {
                numericUpDownSplitterHeight.Value = numericUpDownHeight.Value;
            }
        }

        private void numericUpDownSplitterHeight_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownHeight.Value < numericUpDownSplitterHeight.Value)
            {
                numericUpDownSplitterHeight.Value = numericUpDownHeight.Value;
            }
        }
    }
}

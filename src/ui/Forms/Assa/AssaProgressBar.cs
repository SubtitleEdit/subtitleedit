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
    public partial class AssaProgressBar : Form
    {
        private readonly List<AssaAttachmentFont> _fontAttachments;
        private Subtitle _subtitle;
        private Subtitle _progessBarSubtitle;
        private string _videoFileName;
        private VideoInfo _videoInfo;
        private Timer _timer1;
        private int _oldHashValue = -1;
        private Subtitle _chapters;
        private Controls.VideoPlayerContainer _videoPlayerContainer;

        public AssaProgressBar(Subtitle subtitle, string videoFileName, VideoInfo videoInfo)
        {
            InitializeComponent();

            _subtitle = subtitle;
            _videoFileName = videoFileName;
            _videoInfo = videoInfo;
            _chapters = new Subtitle();

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
            _videoPlayerContainer.Size = new Size(923, 601);

            _fontAttachments = new List<AssaAttachmentFont>();
            if (subtitle.Footer != null)
            {
                GetFonts(subtitle.Footer.SplitToLines());
            }
            buttonPickAttachmentFont.Visible = _fontAttachments.Count > 0;

            listViewChapters_SelectedIndexChanged(null, null);

            var left = labelStorageCategory.Left + Math.Max(labelStorageCategory.Width, labelHeight.Width) + 10;
            radioButtonPosBottom.Left = left;
            radioButtonPosTop.Left = left + radioButtonPosBottom.Width + 10;
            numericUpDownHeight.Left = left;
            buttonForeColor.Left = left;
            panelPrimaryColor.Left = left + buttonForeColor.Width + 5;
            buttonSecondaryColor.Left = left;
            panelSecondaryColor.Left = left + buttonSecondaryColor.Width + 5;

            left = Math.Max(Math.Max(Math.Max(labelSplitterWidth.Width, labelSplitter.Width), Math.Max(labelFontName.Width, labelRotateX.Width)), labelYAdjust.Width) + 12;
            numericUpDownSplitterWidth.Left = left;
            numericUpDownSplitterHeight.Left = left;
            comboBoxFontName.Left = left;
            buttonPickAttachmentFont.Left = left + comboBoxFontName.Width + 5;
            numericUpDownFontSize.Left = left;
            buttonTextColor.Left = left;
            panelTextColor.Left = left + buttonTextColor.Width + 5;
            numericUpDownyAdjust.Left = left;

            LoadExistingProgressBarSettings();

            _timer1 = new Timer();
            _timer1.Interval = 100;
            _timer1.Tick += _timer1_Tick;
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

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonPickAttachmentFont_Click(object sender, System.EventArgs e)
        {
            using (var form = new Forms.Styles.ChooseFontName(_fontAttachments))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    comboBoxFontName.Text = form.FontName;
                }
            }
        }

        private void buttonRemoveAll_Click(object sender, System.EventArgs e)
        {
            _chapters.Paragraphs.Clear();
            RefreshListView();
        }

        private void buttonRemove_Click(object sender, System.EventArgs e)
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

        private void buttonForeColor_Click(object sender, System.EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelPrimaryColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelPrimaryColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonSecondaryColor_Click(object sender, System.EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelSecondaryColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelSecondaryColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void panelPrimaryColor_Click(object sender, System.EventArgs e)
        {
            buttonForeColor_Click(null, null);
        }

        private void panelSecondaryColor_Click(object sender, System.EventArgs e)
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
            _timer1.Start();
        }

        private void AssaProgressBar_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timer1.Stop();
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

        private void _timer1_Tick(object sender, EventArgs e)
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
Style: SE-progress-bar-text,[FONT_NAME],[FONT_SIZE],[TEXT_COLOR],&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,0,0,7,0,0,0,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: -255,0:00:00.00,0:43:00.00,SE-progress-bar-bg,,0,0,0,,{\K[DURATION]\p1}m 0 0 l [VIDEO_WIDTH] 0 [VIDEO_WIDTH] [PROGRESS_BAR_HEIGHT] 0 [PROGRESS_BAR_HEIGHT]{\p0}";

            script = script.Replace("[VIDEO_WIDTH]", _videoInfo.Width.ToString(CultureInfo.InvariantCulture));
            script = script.Replace("[VIDEO_HEIGHT]", _videoInfo.Height.ToString(CultureInfo.InvariantCulture));

            script = script.Replace("[FORE_COLOR]", AdvancedSubStationAlpha.GetSsaColorString(panelPrimaryColor.BackColor));
            script = script.Replace("[BACK_COLOR]", AdvancedSubStationAlpha.GetSsaColorString(panelSecondaryColor.BackColor));
            script = script.Replace("[TEXT_COLOR]", AdvancedSubStationAlpha.GetSsaColorString(panelTextColor.BackColor));

            script = script.Replace("[FONT_SIZE]", numericUpDownFontSize.Value.ToString(CultureInfo.InvariantCulture));
            script = script.Replace("[FONT_NAME]", comboBoxFontName.Text);

            var duration = (int)(_videoInfo.TotalMilliseconds / 10);
            script = script.Replace("[DURATION]", duration.ToString(CultureInfo.InvariantCulture));

            script = script.Replace("[PROGRESS_BAR_HEIGHT]", numericUpDownHeight.Value.ToString(CultureInfo.InvariantCulture));

            if (radioButtonPosTop.Checked)
            {
                script = script.Replace("[ALIGNMENT]", "7");
            }
            else
            {
                script = script.Replace("[ALIGNMENT]", "1");
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
                        var percentOfDuration = p.StartTime.TotalMilliseconds * 100 / _videoInfo.TotalMilliseconds;
                        var position = (int)Math.Round(_videoInfo.Width * percentOfDuration / 100.0);
                        string posTag;
                        string splitterText;
                        SizeF chapterSize;


                        try
                        {
                            //TODO: use font from attachment (if relevant)
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
                            var top = numericUpDownyAdjust.Value;
                            if ((decimal)chapterSize.Height < numericUpDownHeight.Value)
                            {
                                var textHeight = (decimal)chapterSize.Height;
                                top = (int)Math.Round((numericUpDownHeight.Value - textHeight) / 2.0m + numericUpDownyAdjust.Value);
                            }
                            posTag = $"{{\\pos({position + 10}, {top})}}";

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
                                $"{{\\p0}}";
                        }
                        else
                        {
                            posTag = $"{{\\pos({position + 10}, {_videoInfo.Height - numericUpDownHeight.Value + numericUpDownyAdjust.Value})}}";

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
                                $"{{\\p0}}";
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

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (_chapters.Paragraphs.Count > 0)
            {
                var startTimeMs = _chapters.Paragraphs.Last().StartTime.TotalMilliseconds + 60000 * 5;
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
                return;
            }

            labelStartTime.Enabled = true;
            labelText.Enabled = true;
            timeUpDownStartTime.Enabled = true;
            timeUpDownStartTime.MaskedTextBox.Enabled = true;
            textBoxChapterText.Enabled = true;

            var p = (Paragraph)listViewChapters.SelectedItems[0].Tag;
            timeUpDownStartTime.TimeCode = new TimeCode(p.StartTime.TotalMilliseconds);
            textBoxChapterText.Text = p.Text;
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                }
            }
        }

        private void buttonGoToPosition_Click(object sender, EventArgs e)
        {
            _videoPlayerContainer.CurrentPosition = timeUpDownStartTime.TimeCode.TotalSeconds;
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

            InitializeFromSettings();
        }
    }
}

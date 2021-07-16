using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Globalization;
using System.IO;
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

        public AssaProgressBar(Subtitle subtitle, string videoFileName, VideoInfo videoInfo)
        {
            InitializeComponent();

            _subtitle = subtitle;
            _videoFileName = videoFileName;
            _videoInfo = videoInfo;
            _chapters = new Subtitle();

            panelPrimaryColor.BackColor = Configuration.Settings.Tools.AssaProgressBarForeColor;
            panelSecondaryColor.BackColor = Configuration.Settings.Tools.AssaProgressBarBackColor;
            panelTextColor.BackColor = Configuration.Settings.Tools.AssaProgressBarTextColor;

            comboBoxFontName.Items.Clear();
            foreach (var font in FontFamily.Families)
            {
                comboBoxFontName.Items.Add(font.Name);
            }

            _timer1 = new Timer();
            _timer1.Interval = 100;
            _timer1.Tick += _timer1_Tick;
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
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

                if (videoPlayerContainer1.VideoPlayer != null)
                {
                    videoPlayerContainer1.Pause();
                    videoPlayerContainer1.VideoPlayer.DisposeVideoPlayer();
                }

                VideoInfo videoInfo = UiUtil.GetVideoInfo(fileName);
                UiUtil.InitializeVideoPlayerAndContainer(fileName, videoInfo, videoPlayerContainer1, VideoStartLoaded, VideoStartEnded);
            }
        }

        private void VideoStartEnded(object sender, EventArgs e)
        {
            videoPlayerContainer1.Pause();
        }

        private void VideoStartLoaded(object sender, EventArgs e)
        {
            videoPlayerContainer1.Pause();
            _timer1.Start();
        }

        private void AssaProgressBar_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timer1.Stop();
            Application.DoEvents();
            videoPlayerContainer1?.Pause();
            videoPlayerContainer1?.VideoPlayer?.DisposeVideoPlayer();
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
            listViewChapters.SelectedItems[0].SubItems[0].Text = p.StartTime.ToDisplayString();
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
Style: Default,Arial,40,&H00FFFFFF,&H0000FFFB,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,1,1,2,0,0,50,1
Style: sepbar_splitter,Arial,20,[TEXT_COLOR],&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,0,0,7,0,0,0,1
Style: sepbar_bg,Arial,20,[FORE_COLOR],[BACK_COLOR],&H00FFFFFF,&H00000000,0,0,0,0,100,100,0,0,1,0,0,[ALIGNMENT],0,0,0,1
Style: sepbar_title,Arial,[FONT_SIZE],[TEXT_COLOR],&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,0,0,7,0,0,0,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: -255,0:00:00.00,0:43:00.00,sepbar_bg,,0,0,0,,{\K[DURATION]\p1}m 0 0 l [VIDEO_WIDTH] 0 [VIDEO_WIDTH] [PROGRESS_BAR_HEIGHT] 0 [PROGRESS_BAR_HEIGHT]{\p0}";

            script = script.Replace("[VIDEO_WIDTH]", _videoInfo.Width.ToString(CultureInfo.InvariantCulture));
            script = script.Replace("[VIDEO_HEIGHT]", _videoInfo.Height.ToString(CultureInfo.InvariantCulture));

            script = script.Replace("[FORE_COLOR]", AdvancedSubStationAlpha.GetSsaColorString(panelPrimaryColor.BackColor));
            script = script.Replace("[BACK_COLOR]", AdvancedSubStationAlpha.GetSsaColorString(panelSecondaryColor.BackColor));
            script = script.Replace("[TEXT_COLOR]", AdvancedSubStationAlpha.GetSsaColorString(panelTextColor.BackColor));

            script = script.Replace("[FONT_SIZE]", numericUpDownFontSize.Value.ToString(CultureInfo.InvariantCulture));

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

                        //TODO: use real font
                        using (var font = new Font("Arial", (float)(numericUpDownFontSize.Value * 0.7m), FontStyle.Regular))
                        {
                            chapterSize = graphics.MeasureString(p.Text, font);
                        }


                        if (radioButtonPosTop.Checked)
                        {
                            var top = 0;
                            if ((decimal)chapterSize.Height < numericUpDownHeight.Value)
                            {
                                var textHeight = (decimal)chapterSize.Height;
                                top = (int)Math.Round((numericUpDownHeight.Value - textHeight) / 2.0m);
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
                            posTag = $"{{\\pos({position + 10}, {_videoInfo.Height - numericUpDownHeight.Value})}}";

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
                                Extra = "sepbar_splitter",
                                Layer = layer,
                            };
                            layer++;
                            _progessBarSubtitle.Paragraphs.Add(splitter);
                        }

                        var chapterInfo = new Paragraph(posTag + p.Text, 0, _videoInfo.TotalMilliseconds)
                        {
                            Extra = "sepbar_title",
                            Layer = -1,
                        };
                        _progessBarSubtitle.Paragraphs.Add(chapterInfo);
                    }
                }
            }

            textBoxSource.Text = new AdvancedSubStationAlpha().ToText(_progessBarSubtitle, string.Empty);


            var hashValue = _progessBarSubtitle.GetFastHashCode(string.Empty);
            if (hashValue != _oldHashValue)
            {
                videoPlayerContainer1.SetSubtitleText(string.Empty, new Paragraph(), _progessBarSubtitle);
                _oldHashValue = hashValue;
            }

            videoPlayerContainer1.RefreshProgressBar();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0)
            {
                var startTimeMs = _subtitle.Paragraphs.Last().StartTime.TotalMilliseconds + 2000;
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
                timeUpDownStartTime.Enabled = false;
                timeUpDownStartTime.MaskedTextBox.Enabled = false;
                textBoxChapterText.Enabled = false;
                textBoxChapterText.Text = string.Empty;
                return;
            }

            labelStartTime.Enabled = Enabled;
            timeUpDownStartTime.Enabled = Enabled;
            timeUpDownStartTime.MaskedTextBox.Enabled = true;
            textBoxChapterText.Enabled = true;

            var p = (Paragraph)listViewChapters.SelectedItems[0].Tag;
            timeUpDownStartTime.TimeCode = new TimeCode(p.StartTime.TotalMilliseconds);
            textBoxChapterText.Text = p.Text;
        }
    }
}

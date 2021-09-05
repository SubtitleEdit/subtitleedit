using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Controls
{
    public sealed class VideoPlayerContainer : Panel
    {
        public class RichTextBoxViewOnly : RichTextBox
        {
            public RichTextBoxViewOnly()
            {
                ReadOnly = true;
                BorderStyle = BorderStyle.None;
                TabStop = false;
                SetStyle(ControlStyles.Selectable, false);
                SetStyle(ControlStyles.UserMouse, true);
                MouseEnter += delegate { Cursor = Cursors.Default; };
                ScrollBars = RichTextBoxScrollBars.None;
                Margin = new Padding(0);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x204)
                {
                    return; // WM_RBUTTONDOWN
                }

                if (m.Msg == 0x205)
                {
                    return; // WM_RBUTTONUP
                }

                base.WndProc(ref m);
            }
        }

        public event EventHandler OnButtonClicked;
        public event EventHandler OnEmptyPlayerClicked;

        public Panel PanelPlayer { get; private set; }
        private Panel _panelSubtitle;
        private string _subtitleText = string.Empty;
        private VideoPlayer _videoPlayer;

        public float FontSizeFactor { get; set; }

        public VideoPlayer VideoPlayer
        {
            get => _videoPlayer;
            set
            {
                _videoPlayer = value;
                if (_videoPlayer != null)
                {
                    SetPlayerName(_videoPlayer.PlayerName);
                }

                if (_videoPlayer is LibMpvDynamic && Configuration.Settings.General.MpvHandlesPreviewText)
                {
                    _subtitlesHeight = 0;
                }
                else
                {
                    _subtitlesHeight = 57;
                }
                DeleteTempMpvFileName();
                VideoPlayerContainerResize(this, null);
            }
        }

        public RichTextBoxViewOnly TextBox { get; private set; }

        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }

        private bool _isMuted;
        private double? _muteOldVolume;
        private readonly System.ComponentModel.ComponentResourceManager _resources;
        public int ControlsHeight = 47;
        private const int OriginalSubtitlesHeight = 57;
        private int _subtitlesHeight = OriginalSubtitlesHeight;
        private readonly Color _backgroundColor = Color.FromArgb(18, 18, 18);
        private Panel _panelControls;

        private PictureBox _pictureBoxBackground;
        private PictureBox _pictureBoxReverse;
        private PictureBox _pictureBoxReverseOver;
        private PictureBox _pictureBoxReverseDown;
        private PictureBox _pictureBoxFastForward;
        private PictureBox _pictureBoxFastForwardOver;
        private PictureBox _pictureBoxFastForwardDown;
        private PictureBox _pictureBoxPlay;
        private PictureBox _pictureBoxPlayOver;
        private PictureBox _pictureBoxPlayDown;
        private readonly PictureBox _pictureBoxPause = new PictureBox();
        private readonly PictureBox _pictureBoxPauseOver = new PictureBox();
        private readonly PictureBox _pictureBoxPauseDown = new PictureBox();
        private readonly PictureBox _pictureBoxStop = new PictureBox();
        private readonly PictureBox _pictureBoxStopOver = new PictureBox();
        private readonly PictureBox _pictureBoxStopDown = new PictureBox();
        private readonly PictureBox _pictureBoxFullscreen = new PictureBox();
        private readonly PictureBox _pictureBoxFullscreenOver = new PictureBox();
        private readonly PictureBox _pictureBoxFullscreenDown = new PictureBox();
        private readonly PictureBox _pictureBoxMute = new PictureBox();
        private readonly PictureBox _pictureBoxMuteOver = new PictureBox();
        private readonly PictureBox _pictureBoxMuteDown = new PictureBox();
        private readonly PictureBox _pictureBoxProgressbarBackground = new PictureBox();
        private readonly PictureBox _pictureBoxProgressBar = new PictureBox();
        private readonly PictureBox _pictureBoxVolumeBarBackground = new PictureBox();
        private readonly PictureBox _pictureBoxVolumeBar = new PictureBox();
        private readonly Label _labelTimeCode = new Label();
        private readonly Label _labelVideoPlayerName = new Label();
        private readonly Label _labelVolume = new Label();
        private readonly ToolTip _currentPositionToolTip = new ToolTip();
        private int _lastCurrentPositionToolTipX;
        private int _lastCurrentPositionToolTipY;
        private List<MatroskaChapter> _chapters = new List<MatroskaChapter>();

        public List<MatroskaChapter> Chapters
        {
            get => _chapters;
            set
            {
                _chapters = value;
                Invalidate(true);
            }
        }

        public RightToLeft TextRightToLeft
        {
            get => TextBox.RightToLeft;
            set
            {
                TextBox.RightToLeft = value;
                TextBox.SelectAll();
                TextBox.SelectionAlignment = HorizontalAlignment.Center;
            }
        }

        public bool ShowStopButton
        {
            get => _pictureBoxStop.Visible || _pictureBoxStopOver.Visible || _pictureBoxStopDown.Visible;
            set
            {
                if (value)
                {
                    _pictureBoxStop.Visible = true;
                    _pictureBoxStop.BringToFront();
                }
                else
                {
                    HideAllStopImages();
                }
            }
        }

        public bool ShowMuteButton
        {
            get => _pictureBoxMute.Visible || _pictureBoxMuteOver.Visible || _pictureBoxMuteDown.Visible;
            set
            {
                if (value)
                {
                    _pictureBoxMute.Visible = true;
                    _pictureBoxMute.BringToFront();
                }
                else
                {
                    HideAllMuteImages();
                }
            }
        }

        public bool ShowFullscreenButton
        {
            get => _pictureBoxFullscreen.Visible || _pictureBoxFullscreenOver.Visible || _pictureBoxFullscreenDown.Visible;
            set
            {
                if (value)
                {
                    _pictureBoxFullscreen.Visible = true;
                    _pictureBoxFullscreen.BringToFront();
                }
                else
                {
                    HideAllFullscreenImages();
                }
            }
        }

        public VideoPlayerContainer()
        {
            _chapters = new List<MatroskaChapter>();
            SmpteMode = false;
            FontSizeFactor = 1.0F;
            BorderStyle = BorderStyle.None;
            _resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoPlayerContainer));
            _labelVolume.Text = Configuration.Settings.General.VideoPlayerDefaultVolume + "%";
            BackColor = _backgroundColor;
            Controls.Add(MakePlayerPanel());
            Controls.Add(MakeSubtitlesPanel());
            Controls.Add(MakeControlsPanel());
            _panelControls.BringToFront();
            _pictureBoxProgressBar.Width = 0;

            ShowAllControls();
            if (Configuration.IsRunningOnLinux)
            {
                System.Threading.SynchronizationContext.Current.Post(TimeSpan.FromMilliseconds(1500), () =>
                {
                    if (string.IsNullOrEmpty(_labelVideoPlayerName.Text))
                    {
                        _labelVideoPlayerName.Text = "...";
                    }
                    FontSizeFactor = 1.0F;
                    SetSubtitleFont();
                    _labelTimeCode.Text = $"{new TimeCode().ToDisplayString()} / ?";
                    ShowAllControls();
                    VideoPlayerContainerResize(this, null);
                    ShowAllControls();
                    Invalidate();
                    Refresh();
                });
            }

            VideoPlayerContainerResize(this, null);
            Resize += VideoPlayerContainerResize;
            PanelPlayer.MouseDown += PanelPlayerMouseDown;

            PictureBoxFastForwardMouseEnter(null, null);
            PictureBoxFastForwardOverMouseLeave(null, null);
        }

        private void ShowAllControls()
        {
            HideAllPlayImages();
            HideAllPauseImages();
            _pictureBoxPlay.Visible = true;
            _pictureBoxPlay.BringToFront();

            HideAllStopImages();
            _pictureBoxStop.Visible = true;
            _pictureBoxStop.BringToFront();

            HideAllStopImages();
            _pictureBoxStop.Visible = true;
            _pictureBoxStop.BringToFront();

            HideAllFullscreenImages();
            _pictureBoxFullscreen.Visible = true;
            _pictureBoxFullscreen.BringToFront();

            HideAllMuteImages();
            _pictureBoxMute.Visible = true;
            _pictureBoxMute.BringToFront();

            HideAllReverseImages();
            _pictureBoxReverse.Visible = true;
            _pictureBoxReverse.BringToFront();

            HideAllFastForwardImages();
            _pictureBoxFastForward.Visible = true;
            _pictureBoxFastForward.BringToFront();

            _pictureBoxProgressbarBackground.Visible = true;
            _pictureBoxProgressbarBackground.BringToFront();
            _pictureBoxProgressBar.Visible = true;
            _pictureBoxProgressBar.BringToFront();

            _labelTimeCode.Visible = true;
            _labelTimeCode.BringToFront();
        }

        public void EnableMouseWheelStep()
        {
            AddMouseWheelEvent(this);
        }

        public void SetPlayerName(string s)
        {
            _labelVideoPlayerName.Text = s;
            _labelVideoPlayerName.Left = Width - _labelVideoPlayerName.Width - 3;
        }

        public void UpdatePlayerName()
        {
            if (_videoPlayer != null)
            {
                SetPlayerName(_videoPlayer.PlayerName);
            }
        }

        public void ResetTimeLabel()
        {
            _labelTimeCode.Text = string.Empty;
        }

        private void AddMouseWheelEvent(Control control)
        {
            control.MouseWheel += ControlMouseWheel;
            foreach (Control ctrl in control.Controls)
            {
                AddMouseWheelEvent(ctrl);
            }
        }

        private void ControlMouseWheel(object sender, MouseEventArgs e)
        {
            int delta = e.Delta;
            double newPosition = CurrentPosition - delta / 256.0;
            if (newPosition < 0)
            {
                newPosition = 0;
            }
            else if (newPosition > Duration)
            {
                newPosition = Duration;
            }

            CurrentPosition = newPosition;
        }

        private Control MakeSubtitlesPanel()
        {
            _panelSubtitle = new Panel { BackColor = _backgroundColor, Left = 0, Top = 0, Height = _subtitlesHeight + 1 };
            TextBox = new RichTextBoxViewOnly();
            _panelSubtitle.Controls.Add(TextBox);
            TextBox.BackColor = _backgroundColor;
            TextBox.ForeColor = Color.White;
            TextBox.Dock = DockStyle.Fill;
            SetSubtitleFont();
            TextBox.MouseClick += SubtitleTextBoxMouseClick;
            return _panelSubtitle;
        }

        public void SetSubtitleFont()
        {
            var gs = Configuration.Settings.General;
            if (string.IsNullOrEmpty(gs.SubtitleFontName))
            {
                gs.SubtitleFontName = "Tahoma";
            }

            if (gs.VideoPlayerPreviewFontBold)
            {
                TextBox.Font = new Font(gs.VideoPlayerPreviewFontName, gs.VideoPlayerPreviewFontSize * FontSizeFactor, FontStyle.Bold);
            }
            else
            {
                TextBox.Font = new Font(gs.VideoPlayerPreviewFontName, gs.VideoPlayerPreviewFontSize * FontSizeFactor, FontStyle.Regular);
            }

            SubtitleText = _subtitleText;
        }

        private void SubtitleTextBoxMouseClick(object sender, MouseEventArgs e)
        {
            TogglePlayPause();
        }

        public Paragraph LastParagraph { get; set; }

        public void SetSubtitleText(string text, Paragraph p, Subtitle subtitle)
        {
            var mpv = VideoPlayer as LibMpvDynamic;
            LastParagraph = p;
            if (mpv != null && Configuration.Settings.General.MpvHandlesPreviewText && VideoHeight > 0 && VideoWidth > 0)
            {
                if (_subtitlesHeight > 0)
                {
                    _subtitlesHeight = 0;
                    VideoPlayerContainerResize(null, null);
                }
                _subtitleText = text;
                RefreshMpv(mpv, subtitle);
                if (TextBox.Text.Length > 0)
                {
                    TextBox.Text = string.Empty;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_mpvTextFileName) || _subtitlesHeight == 0)
                {
                    mpv?.RemoveSubtitle();
                    _subtitlesHeight = 57;
                    VideoPlayerContainerResize(null, null);
                    DeleteTempMpvFileName();
                }
                SubtitleText = text;
            }
        }

        public void UpdateMpvStyle()
        {
            var gs = Configuration.Settings.General;
            var mpvStyle = new SsaStyle
            {
                Name = "Default",
                FontName = gs.VideoPlayerPreviewFontName,
                FontSize = gs.VideoPlayerPreviewFontSize,
                Bold = gs.VideoPlayerPreviewFontBold,
                Primary = gs.MpvPreviewTextPrimaryColor,
                OutlineWidth = gs.MpvPreviewTextOutlineWidth,
                ShadowWidth = gs.MpvPreviewTextShadowWidth,
                BorderStyle = gs.MpvPreviewTextOpaqueBox ? "3" : "1",
                Alignment = gs.MpvPreviewTextAlignment,
                MarginVertical = gs.MpvPreviewTextMarginVertical
            };

            MpvPreviewStyleHeader = string.Format(AdvancedSubStationAlpha.HeaderNoStyles, "MPV preview file", mpvStyle.ToRawAss(SsaStyle.DefaultAssStyleFormat));
        }

        private string _mpvPreviewStyleHeader;
        private string MpvPreviewStyleHeader
        {
            get
            {
                if (_mpvPreviewStyleHeader is null)
                {
                    UpdateMpvStyle();
                }

                return _mpvPreviewStyleHeader;
            }
            set => _mpvPreviewStyleHeader = value;
        }

        private Subtitle _subtitlePrev;
        private string _mpvTextOld = string.Empty;
        private int _mpvSubOldHash = -1;
        private string _mpvTextFileName;
        private int _retryCount = 3;
        private void RefreshMpv(LibMpvDynamic mpv, Subtitle subtitle)
        {
            if (subtitle == null)
            {
                return;
            }

            try
            {
                if (SmpteMode)
                {
                    subtitle = new Subtitle(subtitle, false);
                    foreach (var paragraph in subtitle.Paragraphs)
                    {
                        paragraph.StartTime.TotalMilliseconds *= 1.001;
                        paragraph.EndTime.TotalMilliseconds *= 1.001;
                    }
                }

                var format = new AdvancedSubStationAlpha();
                string text;

                if (subtitle.Header != null && subtitle.Header.Contains("lang=\"ja\"", StringComparison.Ordinal) && subtitle.Header.Contains("bouten-", StringComparison.Ordinal))
                {
                    text = NetflixImsc11JapaneseToAss.Convert(subtitle, 1280, 720);
                }
                else
                {
                    if (subtitle.Header == null || !subtitle.Header.Contains("[V4+ Styles]"))
                    {
                        if (subtitle.Header != null && subtitle.Header.Contains("[V4 Styles]"))
                        {
                            subtitle = new Subtitle(subtitle, false);
                            subtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromSubStationAlpha(subtitle.Header);
                        }

                        var oldSub = subtitle;
                        subtitle = new Subtitle(subtitle);
                        if (TextBox.RightToLeft == RightToLeft.Yes && LanguageAutoDetect.CouldBeRightToLeftLanguage(subtitle))
                        {
                            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
                            {
                                var paragraph = subtitle.Paragraphs[index];
                                if (LanguageAutoDetect.ContainsRightToLeftLetter(paragraph.Text))
                                {
                                    paragraph.Text = Utilities.FixRtlViaUnicodeChars(paragraph.Text);
                                }
                            }
                        }

                        subtitle.Header = MpvPreviewStyleHeader;

                        if (oldSub.Header != null && oldSub.Header.Length > 20 && oldSub.Header.Substring(3, 3) == "STL")
                        {
                            subtitle.Header = subtitle.Header.Replace("Style: Default,", "Style: Box,arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,3,2,0,2,10,10,10,1" +
                                                                       Environment.NewLine + "Style: Default,");
                            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
                            {
                                var p = subtitle.Paragraphs[index];
                                if (p.Text.Contains("<box>"))
                                {
                                    p.Extra = "Box";
                                    p.Text = p.Text.Replace("<box>", string.Empty).Replace("</box>", string.Empty);
                                }
                            }
                        }
                    }

                    var hash = subtitle.GetFastHashCode(null);
                    if (hash != _mpvSubOldHash || string.IsNullOrEmpty(_mpvTextOld))
                    {
                        text = subtitle.ToText(format);
                        _mpvSubOldHash = hash;
                    }
                    else
                    {
                        text = _mpvTextOld;
                    }
                }


                if (text != _mpvTextOld || _mpvTextFileName == null || _retryCount > 0)
                {
                    if (_retryCount >= 0 || string.IsNullOrEmpty(_mpvTextFileName) || _subtitlePrev == null || _subtitlePrev.FileName != subtitle.FileName || !_mpvTextFileName.EndsWith(format.Extension, StringComparison.Ordinal))
                    {
                        mpv.RemoveSubtitle();
                        DeleteTempMpvFileName();
                        _mpvTextFileName = FileUtil.GetTempFileName(format.Extension);
                        File.WriteAllText(_mpvTextFileName, text);
                        mpv.LoadSubtitle(_mpvTextFileName);
                        _retryCount--;
                    }
                    else
                    {
                        File.WriteAllText(_mpvTextFileName, text);
                        mpv.ReloadSubtitle();
                    }
                    _mpvTextOld = text;
                }
                _subtitlePrev = subtitle;
            }
            catch
            {
                // ignored
            }
        }

        private void DeleteTempMpvFileName()
        {
            try
            {
                if (File.Exists(_mpvTextFileName))
                {
                    File.Delete(_mpvTextFileName);
                    _mpvTextFileName = null;
                }
            }
            catch
            {
                // ignored
            }
        }

        public string SubtitleText
        {
            get => _subtitleText;
            set
            {
                _subtitleText = value;

                bool alignLeft = _subtitleText.StartsWith("{\\a1}", StringComparison.Ordinal) || _subtitleText.StartsWith("{\\a5}", StringComparison.Ordinal) || _subtitleText.StartsWith("{\\a9}", StringComparison.Ordinal) || // sub station alpha
                                 _subtitleText.StartsWith("{\\an1}", StringComparison.Ordinal) || _subtitleText.StartsWith("{\\an4}", StringComparison.Ordinal) || _subtitleText.StartsWith("{\\an7}", StringComparison.Ordinal); // advanced sub station alpha

                bool alignRight = _subtitleText.StartsWith("{\\a3}", StringComparison.Ordinal) || _subtitleText.StartsWith("{\\a7}", StringComparison.Ordinal) || _subtitleText.StartsWith("{\\a11}", StringComparison.Ordinal) || // sub station alpha
                                  _subtitleText.StartsWith("{\\an3}", StringComparison.Ordinal) || _subtitleText.StartsWith("{\\an6}", StringComparison.Ordinal) || _subtitleText.StartsWith("{\\an9}", StringComparison.Ordinal); // advanced sub station alpha

                // remove styles for display text (except italic)
                string text = Utilities.RemoveSsaTags(_subtitleText);
                text = text.Replace("<b></b>", string.Empty);
                text = text.Replace("<i></i>", string.Empty);
                text = text.Replace("<u></u>", string.Empty);

                // display italic
                var sb = new StringBuilder();
                int i = 0;
                bool isItalic = false;
                bool isBold = false;
                bool isUnderline = false;
                bool isFontColor = false;
                int fontColorBegin = 0;
                TextBox.Text = string.Empty;
                int letterCount = 0;
                var fontColorLookups = new Dictionary<Point, Color>();
                var styleLookups = new Dictionary<int, FontStyle>(text.Length);
                for (int j = 0; j < text.Length; j++)
                {
                    styleLookups.Add(j, Configuration.Settings.General.VideoPlayerPreviewFontBold ? FontStyle.Bold : FontStyle.Regular);
                }

                Color fontColor = Color.White;
                while (i < text.Length)
                {
                    if (text.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                    {
                        TextBox.AppendText(sb.ToString());
                        sb.Clear();
                        isItalic = true;
                        i += 2;
                    }
                    else if (isItalic && text.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase))
                    {
                        TextBox.AppendText(sb.ToString());
                        sb.Clear();
                        isItalic = false;
                        i += 3;
                    }
                    else if (text.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!Configuration.Settings.General.VideoPlayerPreviewFontBold)
                        {
                            TextBox.AppendText(sb.ToString());
                            sb.Clear();
                            isBold = true;
                        }
                        i += 2;
                    }
                    else if (isBold && text.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!Configuration.Settings.General.VideoPlayerPreviewFontBold)
                        {
                            TextBox.AppendText(sb.ToString());
                            sb.Clear();
                            isBold = false;
                        }
                        i += 3;
                    }
                    else if (Configuration.Settings.General.VideoPlayerPreviewFontBold && text.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase))
                    {
                        i += 3;
                    }
                    else if (text.Substring(i).StartsWith("<u>", StringComparison.OrdinalIgnoreCase))
                    {
                        TextBox.AppendText(sb.ToString());
                        sb.Clear();
                        isUnderline = true;
                        i += 2;
                    }
                    else if (isUnderline && text.Substring(i).StartsWith("</u>", StringComparison.OrdinalIgnoreCase))
                    {
                        TextBox.AppendText(sb.ToString());
                        sb.Clear();
                        isUnderline = false;
                        i += 3;
                    }
                    else if (text.Substring(i).StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                    {
                        string s = text.Substring(i);
                        bool fontFound = false;
                        int end = s.IndexOf('>');
                        if (end > 0)
                        {
                            string f = s.Substring(0, end);
                            int colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);

                            if (colorStart > 0)
                            {
                                int colorEnd = colorStart + " color=".Length + 1;
                                if (colorEnd < f.Length)
                                {
                                    colorEnd = f.IndexOf('"', colorEnd);
                                    if (colorEnd > 0 || colorEnd == -1)
                                    {
                                        if (colorEnd == -1)
                                        {
                                            s = f.Substring(colorStart);
                                        }
                                        else
                                        {
                                            s = f.Substring(colorStart, colorEnd - colorStart);
                                        }

                                        s = s.Remove(0, " color=".Length);
                                        s = s.Trim('"');
                                        s = s.Trim('\'');
                                        try
                                        {
                                            fontColor = ColorTranslator.FromHtml(s);
                                            fontFound = true;
                                        }
                                        catch
                                        {
                                            fontFound = false;
                                            if (s.Length > 0)
                                            {
                                                try
                                                {
                                                    fontColor = ColorTranslator.FromHtml("#" + s);
                                                    fontFound = true;
                                                }
                                                catch
                                                {
                                                    fontFound = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            i += end;
                        }
                        if (fontFound)
                        {
                            TextBox.AppendText(sb.ToString());
                            sb.Clear();
                            isFontColor = true;
                            fontColorBegin = letterCount;
                        }
                    }
                    else if (isFontColor && text.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                    {
                        fontColorLookups.Add(new Point(fontColorBegin, TextBox.Text.Length + sb.ToString().Length - fontColorBegin), fontColor);
                        TextBox.AppendText(sb.ToString());
                        sb.Clear();
                        isFontColor = false;
                        i += 6;
                    }
                    else if (text.Substring(i).StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                    {
                        i += 6;
                    }
                    else if (text[i] == '\n') // RichTextBox only count NewLine as one character!
                    {
                        sb.Append(text[i]);
                    }
                    else if (text[i] == '\r')
                    {
                        // skip carriage return (0xd / 13)
                    }
                    else
                    {
                        var idx = TextBox.TextLength + sb.Length;
                        if (isBold)
                        {
                            styleLookups[idx] |= FontStyle.Bold;
                        }

                        if (isItalic)
                        {
                            styleLookups[idx] |= FontStyle.Italic;
                        }

                        if (isUnderline)
                        {
                            styleLookups[idx] |= FontStyle.Underline;
                        }

                        sb.Append(text[i]);
                        letterCount++;
                    }
                    i++;
                }
                TextBox.Text += sb.ToString();
                TextBox.SelectAll();

                if (alignLeft)
                {
                    TextBox.SelectionAlignment = HorizontalAlignment.Left;
                }
                else if (alignRight)
                {
                    TextBox.SelectionAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    TextBox.SelectionAlignment = HorizontalAlignment.Center;
                }

                TextBox.DeselectAll();

                Font currentFont = TextBox.SelectionFont;
                for (int k = 0; k < TextBox.TextLength; k++)
                {
                    TextBox.SelectionStart = k;
                    TextBox.SelectionLength = 1;
                    TextBox.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, styleLookups[k]);
                    TextBox.DeselectAll();
                }

                foreach (var entry in fontColorLookups)
                {
                    TextBox.SelectionStart = entry.Key.X;
                    TextBox.SelectionLength = entry.Key.Y;
                    TextBox.SelectionColor = entry.Value;
                    TextBox.DeselectAll();
                }
            }
        }

        private void PanelPlayerMouseDown(object sender, MouseEventArgs e)
        {
            if (VideoPlayer == null)
            {
                OnEmptyPlayerClicked?.Invoke(sender, e);
            }

            TogglePlayPause();
        }

        public void InitializeVolume(double defaultVolume)
        {
            int maxVolume = _pictureBoxVolumeBarBackground.Width - 18;
            _pictureBoxVolumeBar.Width = (int)(maxVolume * defaultVolume / 100.0);
        }

        private Control MakePlayerPanel()
        {
            PanelPlayer = new Panel { BackColor = _backgroundColor, Left = 0, Top = 0 };
            return PanelPlayer;
        }

        public void HideControls(bool hideCursor)
        {
            if (_panelControls.Visible)
            {
                _panelSubtitle.Height += ControlsHeight;
                _panelControls.Visible = false;
            }
            if (hideCursor)
            {
                HideCursor();
            }
        }

        public void ShowControls()
        {
            if (!_panelControls.Visible)
            {
                _panelControls.Visible = true;
                _panelControls.BringToFront();
                _panelSubtitle.Height -= ControlsHeight;
            }
            ShowCursor();
        }

        public void HideCursor()
        {
            if (_cursorStatus < 0)
            {
                return;
            }

            _cursorStatus--;
            if (VideoPlayer != null)
            {
                var mpv = VideoPlayer as LibMpvDynamic;
                mpv?.HideCursor();
            }
            Cursor.Hide();
        }

        private int _cursorStatus;

        public void ShowCursor()
        {
            if (_cursorStatus >= 0)
            {
                return;
            }

            _cursorStatus++;
            if (VideoPlayer != null)
            {
                var mpv = VideoPlayer as LibMpvDynamic;
                mpv?.ShowCursor();
            }
            Cursor.Show();
        }

        private Control MakeControlsPanel()
        {
            _panelControls = new Panel { Left = 0, Height = ControlsHeight };

            _pictureBoxBackground = new PictureBox
            {
                Image = (Image)_resources.GetObject("pictureBoxBar.Image"),
                Location = new Point(0, 0),
                Name = "_pictureBoxBackground",
                Size = new Size(200, 45),
                SizeMode = PictureBoxSizeMode.StretchImage,
                TabStop = false
            };
            _panelControls.Controls.Add(_pictureBoxBackground);

            _pictureBoxPlay = new PictureBox
            {
                Image = (Image)_resources.GetObject("pictureBoxPlay.Image"),
                Location = new Point(22, 126 - 113),
                Name = "_pictureBoxPlay",
                Size = new Size(29, 29),
                SizeMode = PictureBoxSizeMode.AutoSize,
                TabStop = false
            };
            _pictureBoxPlay.MouseEnter += PictureBoxPlayMouseEnter;
            _panelControls.Controls.Add(_pictureBoxPlay);

            _pictureBoxPlayDown = new PictureBox
            {
                Image = (Image)_resources.GetObject("pictureBoxPlayDown.Image"),
                Location = new Point(22, 127 - 113),
                Name = "_pictureBoxPlayDown",
                Size = new Size(29, 29),
                SizeMode = PictureBoxSizeMode.AutoSize,
                TabStop = false
            };
            _panelControls.Controls.Add(_pictureBoxPlayDown);

            _pictureBoxPlayOver = new PictureBox
            {
                Image = (Image)_resources.GetObject("pictureBoxPlayOver.Image"),
                Location = new Point(23, 126 - 113),
                Name = "_pictureBoxPlayOver",
                Size = new Size(29, 29),
                SizeMode = PictureBoxSizeMode.AutoSize,
                TabStop = false
            };
            _pictureBoxPlayOver.MouseLeave += PictureBoxPlayOverMouseLeave;
            _pictureBoxPlayOver.MouseDown += PictureBoxPlayOverMouseDown;
            _pictureBoxPlayOver.MouseUp += PictureBoxPlayOverMouseUp;
            _panelControls.Controls.Add(_pictureBoxPlayOver);

            _pictureBoxPause.Image = (Image)_resources.GetObject("pictureBoxPause.Image");
            _pictureBoxPause.Location = new Point(23, 126 - 113);
            _pictureBoxPause.Name = "_pictureBoxPause";
            _pictureBoxPause.Size = new Size(29, 29);
            _pictureBoxPause.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxPause.TabStop = false;
            _pictureBoxPause.MouseEnter += PictureBoxPauseMouseEnter;
            _panelControls.Controls.Add(_pictureBoxPause);

            _pictureBoxPauseDown.Image = (Image)_resources.GetObject("pictureBoxPauseDown.Image");
            _pictureBoxPauseDown.Location = new Point(22, 127 - 113);
            _pictureBoxPauseDown.Name = "_pictureBoxPauseDown";
            _pictureBoxPauseDown.Size = new Size(29, 29);
            _pictureBoxPauseDown.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxPauseDown.TabStop = false;
            _panelControls.Controls.Add(_pictureBoxPauseDown);

            _pictureBoxPauseOver.Image = (Image)_resources.GetObject("pictureBoxPauseOver.Image");
            _pictureBoxPauseOver.Location = new Point(22, 127 - 113);
            _pictureBoxPauseOver.Name = "_pictureBoxPauseOver";
            _pictureBoxPauseOver.Size = new Size(29, 29);
            _pictureBoxPauseOver.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxPauseOver.TabStop = false;
            _pictureBoxPauseOver.MouseLeave += PictureBoxPauseOverMouseLeave;
            _pictureBoxPauseOver.MouseDown += PictureBoxPauseOverMouseDown;
            _pictureBoxPauseOver.MouseUp += PictureBoxPauseOverMouseUp;
            _panelControls.Controls.Add(_pictureBoxPauseOver);

            _pictureBoxStop.Image = (Image)_resources.GetObject("pictureBoxStop.Image");
            _pictureBoxStop.Location = new Point(52, 130 - 113);
            _pictureBoxStop.Name = "_pictureBoxStop";
            _pictureBoxStop.Size = new Size(20, 20);
            _pictureBoxStop.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxStop.TabStop = false;
            _pictureBoxStop.MouseEnter += PictureBoxStopMouseEnter;
            _panelControls.Controls.Add(_pictureBoxStop);

            _pictureBoxStopDown.Image = (Image)_resources.GetObject("pictureBoxStopDown.Image");
            _pictureBoxStopDown.Location = new Point(52, 130 - 113);
            _pictureBoxStopDown.Name = "_pictureBoxStopDown";
            _pictureBoxStopDown.Size = new Size(20, 20);
            _pictureBoxStopDown.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxStopDown.TabStop = false;
            _panelControls.Controls.Add(_pictureBoxStopDown);

            _pictureBoxStopOver.Image = (Image)_resources.GetObject("pictureBoxStopOver.Image");
            _pictureBoxStopOver.Location = new Point(52, 130 - 113);
            _pictureBoxStopOver.Name = "_pictureBoxStopOver";
            _pictureBoxStopOver.Size = new Size(20, 20);
            _pictureBoxStopOver.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxStopOver.TabStop = false;
            _pictureBoxStopOver.MouseLeave += PictureBoxStopOverMouseLeave;
            _pictureBoxStopOver.MouseDown += PictureBoxStopOverMouseDown;
            _pictureBoxStopOver.MouseUp += PictureBoxStopOverMouseUp;
            _panelControls.Controls.Add(_pictureBoxStopOver);

            _pictureBoxFullscreen.Image = (Image)_resources.GetObject("pictureBoxFS.Image");
            _pictureBoxFullscreen.Location = new Point(95, 130 - 113);
            _pictureBoxFullscreen.Name = "_pictureBoxFullscreen";
            _pictureBoxFullscreen.Size = new Size(20, 20);
            _pictureBoxFullscreen.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxFullscreen.TabStop = false;
            _pictureBoxFullscreen.MouseEnter += PictureBoxFullscreenMouseEnter;
            _panelControls.Controls.Add(_pictureBoxFullscreen);

            _pictureBoxFullscreenDown.Image = (Image)_resources.GetObject("pictureBoxFSDown.Image");
            _pictureBoxFullscreenDown.Location = new Point(95, 130 - 113);
            _pictureBoxFullscreenDown.Name = "_pictureBoxFullscreenDown";
            _pictureBoxFullscreenDown.Size = new Size(20, 20);
            _pictureBoxFullscreenDown.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxFullscreenDown.TabStop = false;
            _panelControls.Controls.Add(_pictureBoxFullscreenDown);

            _pictureBoxFullscreenOver.Image = (Image)_resources.GetObject("pictureBoxFSOver.Image");
            _pictureBoxFullscreenOver.Location = new Point(95, 130 - 113);
            _pictureBoxFullscreenOver.Name = "_pictureBoxFullscreenOver";
            _pictureBoxFullscreenOver.Size = new Size(20, 20);
            _pictureBoxFullscreenOver.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxFullscreenOver.TabStop = false;
            _pictureBoxFullscreenOver.MouseLeave += PictureBoxFullscreenOverMouseLeave;
            _pictureBoxFullscreenOver.MouseDown += PictureBoxFullscreenOverMouseDown;
            _pictureBoxFullscreenOver.MouseUp += PictureBoxFullscreenOverMouseUp;
            _panelControls.Controls.Add(_pictureBoxFullscreenOver);

            _pictureBoxProgressbarBackground.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _pictureBoxProgressbarBackground.BackColor = Color.Transparent;
            _pictureBoxProgressbarBackground.Image = (Image)_resources.GetObject("pictureBoxProgressbarBackground.Image");
            _pictureBoxProgressbarBackground.Location = new Point(43, 114 - 113);
            _pictureBoxProgressbarBackground.Margin = new Padding(0);
            _pictureBoxProgressbarBackground.Name = "_pictureBoxProgressbarBackground";
            _pictureBoxProgressbarBackground.Size = new Size(531, 12);
            _pictureBoxProgressbarBackground.SizeMode = PictureBoxSizeMode.StretchImage;
            _pictureBoxProgressbarBackground.TabStop = false;
            _pictureBoxProgressbarBackground.Paint += PictureBoxProgressbarBackgroundPaint;
            _pictureBoxProgressbarBackground.MouseDown += PictureBoxProgressbarBackgroundMouseDown;
            _pictureBoxProgressbarBackground.MouseLeave += PictureBoxProgressbarBackgroundMouseLeave;
            _pictureBoxProgressbarBackground.MouseMove += PictureBoxProgressbarBackgroundMouseMove;
            _panelControls.Controls.Add(_pictureBoxProgressbarBackground);

            _pictureBoxProgressBar.Image = (Image)_resources.GetObject("pictureBoxProgressBar.Image");
            _pictureBoxProgressBar.Location = new Point(47, 118 - 113);
            _pictureBoxProgressBar.Name = "_pictureBoxProgressBar";
            _pictureBoxProgressBar.Size = new Size(318, 4);
            _pictureBoxProgressBar.SizeMode = PictureBoxSizeMode.StretchImage;
            _pictureBoxProgressBar.TabStop = false;
            _pictureBoxProgressBar.Paint += PictureBoxProgressBarPaint;
            _pictureBoxProgressBar.MouseDown += PictureBoxProgressBarMouseDown;
            _pictureBoxProgressBar.MouseLeave += PictureBoxProgressBarMouseLeave;
            _pictureBoxProgressBar.MouseMove += PictureBoxProgressBarMouseMove;
            _panelControls.Controls.Add(_pictureBoxProgressBar);
            _pictureBoxProgressBar.BringToFront();

            _pictureBoxMute.Image = (Image)_resources.GetObject("pictureBoxMute.Image");
            _pictureBoxMute.Location = new Point(75, 131 - 113);
            _pictureBoxMute.Name = "_pictureBoxMute";
            _pictureBoxMute.Size = new Size(19, 19);
            _pictureBoxMute.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxMute.TabStop = false;
            _pictureBoxMute.MouseEnter += PictureBoxMuteMouseEnter;
            _panelControls.Controls.Add(_pictureBoxMute);

            _pictureBoxMuteDown.Image = (Image)_resources.GetObject("pictureBoxMuteDown.Image");
            _pictureBoxMuteDown.Location = new Point(75, 131 - 113);
            _pictureBoxMuteDown.Name = "_pictureBoxMuteDown";
            _pictureBoxMuteDown.Size = new Size(19, 19);
            _pictureBoxMuteDown.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxMuteDown.TabStop = false;
            _pictureBoxMuteDown.Click += PictureBoxMuteDownClick;
            _panelControls.Controls.Add(_pictureBoxMuteDown);

            _pictureBoxMuteOver.Image = (Image)_resources.GetObject("pictureBoxMuteOver.Image");
            _pictureBoxMuteOver.Location = new Point(75, 131 - 113);
            _pictureBoxMuteOver.Name = "_pictureBoxMuteOver";
            _pictureBoxMuteOver.Size = new Size(19, 19);
            _pictureBoxMuteOver.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxMuteOver.TabStop = false;
            _pictureBoxMuteOver.MouseLeave += PictureBoxMuteOverMouseLeave;
            _pictureBoxMuteOver.MouseDown += PictureBoxMuteOverMouseDown;
            _pictureBoxMuteOver.MouseUp += PictureBoxMuteOverMouseUp;
            _panelControls.Controls.Add(_pictureBoxMuteOver);

            _pictureBoxVolumeBarBackground.Image = (Image)_resources.GetObject("pictureBoxVolumeBarBackground.Image");
            _pictureBoxVolumeBarBackground.Location = new Point(111, 135 - 113);
            _pictureBoxVolumeBarBackground.Name = "_pictureBoxVolumeBarBackground";
            _pictureBoxVolumeBarBackground.Size = new Size(82, 13);
            _pictureBoxVolumeBarBackground.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxVolumeBarBackground.TabStop = false;
            _pictureBoxVolumeBarBackground.MouseDown += PictureBoxVolumeBarBackgroundMouseDown;
            _panelControls.Controls.Add(_pictureBoxVolumeBarBackground);

            _pictureBoxVolumeBar.Image = (Image)_resources.GetObject("pictureBoxVolumeBar.Image");
            _pictureBoxVolumeBar.Location = new Point(120, 139 - 113);
            _pictureBoxVolumeBar.Name = "_pictureBoxVolumeBar";
            _pictureBoxVolumeBar.Size = new Size(48, 4);
            _pictureBoxVolumeBar.SizeMode = PictureBoxSizeMode.StretchImage;
            _pictureBoxVolumeBar.TabStop = false;
            _pictureBoxVolumeBar.MouseDown += PictureBoxVolumeBarMouseDown;
            _panelControls.Controls.Add(_pictureBoxVolumeBar);
            _pictureBoxVolumeBar.BringToFront();

            _pictureBoxReverse = new PictureBox
            {
                Image = (Image)_resources.GetObject("pictureBoxReverse.Image"),
                Location = new Point(28, 3),
                Name = "_pictureBoxReverse",
                Size = new Size(16, 8),
                SizeMode = PictureBoxSizeMode.AutoSize,
                TabStop = false
            };
            _panelControls.Controls.Add(_pictureBoxReverse);
            _pictureBoxReverse.MouseEnter += PictureBoxReverseMouseEnter;

            _pictureBoxReverseOver = new PictureBox
            {
                Image = (Image)_resources.GetObject("pictureBoxReverseMouseOver.Image"),
                Location = _pictureBoxReverse.Location,
                Name = "_pictureBoxReverseOver",
                Size = _pictureBoxReverse.Size,
                SizeMode = PictureBoxSizeMode.AutoSize,
                TabStop = false
            };
            _panelControls.Controls.Add(_pictureBoxReverseOver);
            _pictureBoxReverseOver.MouseLeave += PictureBoxReverseOverMouseLeave;
            _pictureBoxReverseOver.MouseDown += PictureBoxReverseOverMouseDown;
            _pictureBoxReverseOver.MouseUp += PictureBoxReverseOverMouseUp;

            _pictureBoxReverseDown = new PictureBox
            {
                Image = (Image)_resources.GetObject("pictureBoxReverseMouseDown.Image"),
                Location = _pictureBoxReverse.Location,
                Name = "_pictureBoxReverseOver",
                Size = _pictureBoxReverse.Size,
                SizeMode = PictureBoxSizeMode.AutoSize,
                TabStop = false
            };
            _panelControls.Controls.Add(_pictureBoxReverseDown);

            _pictureBoxFastForward = new PictureBox
            {
                Image = (Image)_resources.GetObject("pictureBoxFastForward.Image"),
                Location = new Point(571, 1),
                Name = "_pictureBoxFastForward",
                Size = new Size(17, 13),
                SizeMode = PictureBoxSizeMode.AutoSize,
                TabStop = false
            };
            _panelControls.Controls.Add(_pictureBoxFastForward);
            _pictureBoxFastForward.MouseEnter += PictureBoxFastForwardMouseEnter;

            _pictureBoxFastForwardOver = new PictureBox
            {
                Image = (Image)_resources.GetObject("pictureBoxFastForwardMouseOver.Image"),
                Location = _pictureBoxFastForward.Location,
                Name = "_pictureBoxFastForwardOver",
                Size = _pictureBoxFastForward.Size,
                SizeMode = PictureBoxSizeMode.AutoSize,
                TabStop = false
            };
            _panelControls.Controls.Add(_pictureBoxFastForwardOver);
            _pictureBoxFastForwardOver.MouseLeave += PictureBoxFastForwardOverMouseLeave;
            _pictureBoxFastForwardOver.MouseDown += PictureBoxFastForwardOverMouseDown;
            _pictureBoxFastForwardOver.MouseUp += PictureBoxFastForwardOverMouseUp;

            _pictureBoxFastForwardDown = new PictureBox
            {
                Image = (Image)_resources.GetObject("pictureBoxFastForwardMouseDown.Image"),
                Location = _pictureBoxFastForward.Location,
                Name = "_pictureBoxFastForwardDown",
                Size = _pictureBoxFastForward.Size,
                SizeMode = PictureBoxSizeMode.AutoSize,
                TabStop = false
            };
            _panelControls.Controls.Add(_pictureBoxFastForwardDown);

            _labelVolume.Location = new Point(120, 17);
            _labelVolume.ForeColor = Color.WhiteSmoke;
            _labelVolume.BackColor = Color.FromArgb(67, 75, 93);
            _labelVolume.AutoSize = true;
            _labelVolume.Font = new Font(_labelTimeCode.Font.FontFamily, 6);
            _panelControls.Controls.Add(_labelVolume);

            _labelTimeCode.Location = new Point(280, 28);
            _labelTimeCode.ForeColor = Color.WhiteSmoke;
            _labelTimeCode.Font = new Font(_labelTimeCode.Font.FontFamily, 8, FontStyle.Bold);
            _labelTimeCode.AutoSize = true;
            _panelControls.Controls.Add(_labelTimeCode);

            _labelVideoPlayerName.Location = new Point(282, 17);
            _labelVideoPlayerName.ForeColor = Color.WhiteSmoke;
            _labelVideoPlayerName.BackColor = Color.FromArgb(67, 75, 93);
            _labelVideoPlayerName.AutoSize = true;
            _labelVideoPlayerName.Font = new Font(_labelTimeCode.Font.FontFamily, 6);
            _panelControls.Controls.Add(_labelVideoPlayerName);

            if (Configuration.Settings.General.UseDarkTheme)
            {
                _labelVolume.ForeColor = Color.Gray;
                _labelTimeCode.ForeColor = Color.Gray;
                _labelVideoPlayerName.ForeColor = Color.Gray;
            }


            _pictureBoxBackground.SendToBack();
            _pictureBoxFastForwardDown.BringToFront();
            _pictureBoxFastForwardOver.BringToFront();
            _pictureBoxFastForward.BringToFront();
            _pictureBoxPlay.BringToFront();

            _panelControls.BackColor = _backgroundColor;
            _pictureBoxPlayDown.BringToFront();
            _pictureBoxPlayOver.BringToFront();
            _pictureBoxPlay.BringToFront();
            _labelTimeCode.BringToFront();
            _labelVolume.BringToFront();
            return _panelControls;
        }

        public void VideoPlayerContainerResize(object sender, EventArgs e)
        {
            ControlsHeight = _pictureBoxBackground.Height;
            PanelPlayer.Height = Height - (ControlsHeight + _subtitlesHeight);
            PanelPlayer.Width = Width;
            _videoPlayer?.Resize(PanelPlayer.Width, PanelPlayer.Height);

            _panelSubtitle.Top = Height - (ControlsHeight + _subtitlesHeight);
            _panelSubtitle.Width = Width;

            _panelControls.Top = Height - ControlsHeight + 2;
            _panelControls.Width = Width;
            _pictureBoxBackground.Width = Width;
            _pictureBoxProgressbarBackground.Width = Width - (_pictureBoxProgressbarBackground.Left * 2);
            _pictureBoxFastForward.Left = Width - 48;
            _pictureBoxFastForwardDown.Left = _pictureBoxFastForward.Left;
            _pictureBoxFastForwardOver.Left = _pictureBoxFastForward.Left;

            ResizeTimeCode();

            _labelVideoPlayerName.Left = Width - _labelVideoPlayerName.Width - 3;
            DeleteTempMpvFileName();
        }

        private void ResizeTimeCode()
        {
            if (string.IsNullOrEmpty(_labelTimeCode.Text))
            {
                var span = TimeCode.FromSeconds(1);
                _labelTimeCode.Text = $"{span.ToDisplayString()} / {span.ToDisplayString()}{(SmpteMode ? " SMPTE" : string.Empty)}";
                _labelTimeCode.Left = Width - _labelTimeCode.Width - 9;
                if (_labelTimeCode.Top + _labelTimeCode.Height >= _panelControls.Height - 4)
                {
                    _labelTimeCode.Font = new Font(_labelTimeCode.Font.Name, _labelTimeCode.Font.Size - 1);
                }

                _labelTimeCode.Text = string.Empty;
            }
            else
            {
                _labelTimeCode.Left = Width - _labelTimeCode.Width - 9;
                if (_labelTimeCode.Top + _labelTimeCode.Height >= _panelControls.Height - 4)
                {
                    _labelTimeCode.Font = new Font(_labelTimeCode.Font.Name, _labelTimeCode.Font.Size - 1);
                }
            }
        }

        #region PlayPauseButtons

        public void RefreshPlayPauseButtons()
        {
            if (VideoPlayer != null)
            {
                if (VideoPlayer.IsPlaying)
                {
                    if (!_pictureBoxPause.Visible && !_pictureBoxPauseDown.Visible && !_pictureBoxPauseOver.Visible)
                    {
                        HideAllPauseImages();
                        HideAllPlayImages();
                        _pictureBoxPause.Visible = true;
                        _pictureBoxPause.BringToFront();
                    }
                }
                else
                {
                    if (!_pictureBoxPlay.Visible && !_pictureBoxPlayOver.Visible && !_pictureBoxPlayDown.Visible)
                    {
                        HideAllPauseImages();
                        HideAllPlayImages();
                        _pictureBoxPlay.Visible = true;
                        _pictureBoxPlay.BringToFront();
                    }
                }
            }
        }

        private void HideAllPlayImages()
        {
            _pictureBoxPlayOver.Visible = false;
            _pictureBoxPlayDown.Visible = false;
            _pictureBoxPlay.Visible = false;
        }

        private void PictureBoxPlayMouseEnter(object sender, EventArgs e)
        {
            if (_pictureBoxPlay.Visible)
            {
                HideAllPlayImages();
                _pictureBoxPlayOver.Visible = true;
                _pictureBoxPlayOver.BringToFront();
            }
        }

        private void PictureBoxPlayOverMouseLeave(object sender, EventArgs e)
        {
            if (_pictureBoxPlayOver.Visible)
            {
                HideAllPlayImages();
                _pictureBoxPlay.Visible = true;
                _pictureBoxPlay.BringToFront();
            }
        }

        private void PictureBoxPlayOverMouseDown(object sender, MouseEventArgs e)
        {
            HideAllPlayImages();
            _pictureBoxPlayDown.Visible = true;
            _pictureBoxPlayDown.BringToFront();
            OnButtonClicked?.Invoke(sender, e);
        }

        private void PictureBoxPlayOverMouseUp(object sender, MouseEventArgs e)
        {
            if (IsMouseOverControl((PictureBox)sender, e.Location))
            {
                HideAllPlayImages();
                _pictureBoxPause.Visible = true;
                _pictureBoxPause.BringToFront();
                Play();
            }
            else
            {
                HideAllPlayImages();
                _pictureBoxPlay.Visible = true;
                _pictureBoxPause.BringToFront();
            }
        }

        private void HideAllPauseImages()
        {
            _pictureBoxPauseOver.Visible = false;
            _pictureBoxPauseDown.Visible = false;
            _pictureBoxPause.Visible = false;
        }

        private void PictureBoxPauseMouseEnter(object sender, EventArgs e)
        {
            if (_pictureBoxPause.Visible)
            {
                HideAllPauseImages();
                _pictureBoxPauseOver.Visible = true;
                _pictureBoxPauseOver.BringToFront();
            }
        }

        private void PictureBoxPauseOverMouseLeave(object sender, EventArgs e)
        {
            if (_pictureBoxPauseOver.Visible)
            {
                HideAllPauseImages();
                _pictureBoxPause.Visible = true;
                _pictureBoxPause.BringToFront();
            }
        }

        private void PictureBoxPauseOverMouseDown(object sender, MouseEventArgs e)
        {
            if (_pictureBoxPauseOver.Visible)
            {
                HideAllPauseImages();
                _pictureBoxPauseDown.Visible = true;
                _pictureBoxPauseDown.BringToFront();
            }
            OnButtonClicked?.Invoke(sender, e);
        }

        private void PictureBoxPauseOverMouseUp(object sender, MouseEventArgs e)
        {
            if (IsMouseOverControl((PictureBox)sender, e.Location))
            {
                HideAllPauseImages();
                _pictureBoxPlay.Visible = true;
                _pictureBoxPlay.BringToFront();
                Pause();
            }
            else
            {
                HideAllPauseImages();
                _pictureBoxPause.Visible = true;
                _pictureBoxPause.BringToFront();
            }
        }

        #endregion PlayPauseButtons

        #region StopButtons

        private void HideAllStopImages()
        {
            _pictureBoxStopOver.Visible = false;
            _pictureBoxStopDown.Visible = false;
            _pictureBoxStop.Visible = false;
        }

        private void PictureBoxStopMouseEnter(object sender, EventArgs e)
        {
            HideAllStopImages();
            _pictureBoxStopOver.Visible = true;
            _pictureBoxStopOver.BringToFront();
        }

        private void PictureBoxStopOverMouseLeave(object sender, EventArgs e)
        {
            if (_pictureBoxStopOver.Visible)
            {
                HideAllStopImages();
                _pictureBoxStop.Visible = true;
                _pictureBoxStop.BringToFront();
            }
        }

        private void PictureBoxStopOverMouseDown(object sender, MouseEventArgs e)
        {
            if (_pictureBoxStopOver.Visible)
            {
                HideAllStopImages();
                _pictureBoxStopDown.Visible = true;
                _pictureBoxStopDown.BringToFront();
            }
            OnButtonClicked?.Invoke(sender, e);
        }

        private void PictureBoxStopOverMouseUp(object sender, MouseEventArgs e)
        {
            if (IsMouseOverControl((PictureBox)sender, e.Location))
            {
                HideAllStopImages();
                _pictureBoxStop.Visible = true;
                _pictureBoxStop.BringToFront();
                Stop();
            }
            else
            {
                HideAllStopImages();
                _pictureBoxStop.Visible = true;
                _pictureBoxStop.BringToFront();
            }
        }

        #endregion StopButtons

        #region FullscreenButtons

        private void HideAllFullscreenImages()
        {
            _pictureBoxFullscreenOver.Visible = false;
            _pictureBoxFullscreenDown.Visible = false;
            _pictureBoxFullscreen.Visible = false;
        }

        public void ShowFullScreenControls()
        {
            _pictureBoxFullscreen.Image = (Image)_resources.GetObject("pictureBoxNoFS.Image");
            _pictureBoxFullscreenDown.Image = (Image)_resources.GetObject("pictureBoxNoFSDown.Image");
            _pictureBoxFullscreenOver.Image = (Image)_resources.GetObject("pictureBoxNoFSOver.Image");
        }

        public void ShowNonFullScreenControls()
        {
            _pictureBoxFullscreen.Image = (Image)_resources.GetObject("pictureBoxFS.Image");
            _pictureBoxFullscreenDown.Image = (Image)_resources.GetObject("pictureBoxFSDown.Image");
            _pictureBoxFullscreenOver.Image = (Image)_resources.GetObject("pictureBoxFSOver.Image");
        }

        private void PictureBoxFullscreenMouseEnter(object sender, EventArgs e)
        {
            HideAllFullscreenImages();
            _pictureBoxFullscreenOver.Visible = true;
            _pictureBoxFullscreenOver.BringToFront();
        }

        private void PictureBoxFullscreenOverMouseLeave(object sender, EventArgs e)
        {
            if (_pictureBoxFullscreenOver.Visible)
            {
                HideAllFullscreenImages();
                _pictureBoxFullscreen.Visible = true;
                _pictureBoxFullscreen.BringToFront();
            }
        }

        private void PictureBoxFullscreenOverMouseDown(object sender, MouseEventArgs e)
        {
            HideAllFullscreenImages();
            OnButtonClicked?.Invoke(sender, e);
        }

        private void PictureBoxFullscreenOverMouseUp(object sender, MouseEventArgs e)
        {
            HideAllFullscreenImages();
            _pictureBoxFullscreen.Visible = true;
            _pictureBoxFullscreen.BringToFront();
        }

        #endregion FullscreenButtons

        #region Mute buttons

        private void HideAllMuteImages()
        {
            _pictureBoxMuteOver.Visible = false;
            _pictureBoxMuteDown.Visible = false;
            _pictureBoxMute.Visible = false;
        }

        private void PictureBoxMuteMouseEnter(object sender, EventArgs e)
        {
            HideAllMuteImages();
            if (Mute)
            {
                _pictureBoxMuteDown.Visible = true;
                _pictureBoxMuteDown.BringToFront();
            }
            else
            {
                _pictureBoxMuteOver.Visible = true;
                _pictureBoxMuteOver.BringToFront();
            }
        }

        private void PictureBoxMuteOverMouseLeave(object sender, EventArgs e)
        {
            if (_pictureBoxMuteOver.Visible)
            {
                HideAllMuteImages();
                _pictureBoxMute.Visible = true;
                _pictureBoxMute.BringToFront();
            }
        }

        private void PictureBoxMuteOverMouseDown(object sender, MouseEventArgs e)
        {
            if (_pictureBoxMuteOver.Visible)
            {
                HideAllMuteImages();
                _pictureBoxMuteDown.Visible = true;
                _pictureBoxMuteDown.BringToFront();
            }
            OnButtonClicked?.Invoke(sender, e);
        }

        private void PictureBoxMuteOverMouseUp(object sender, MouseEventArgs e)
        {
            if (IsMouseOverControl((PictureBox)sender, e.Location))
            {
                HideAllMuteImages();
                _pictureBoxMuteDown.Visible = true;
                _pictureBoxMuteDown.BringToFront();
                Mute = true;
            }
            else
            {
                HideAllMuteImages();
                _pictureBoxMute.Visible = true;
                _pictureBoxMute.BringToFront();
            }
        }

        private void PictureBoxMuteDownClick(object sender, EventArgs e)
        {
            Mute = false;
            HideAllMuteImages();
            _pictureBoxMute.Visible = true;
            _pictureBoxMute.BringToFront();
            OnButtonClicked?.Invoke(sender, e);
        }

        #endregion Mute buttons

        #region Reverse buttons

        private void HideAllReverseImages()
        {
            _pictureBoxReverseOver.Visible = false;
            _pictureBoxReverseDown.Visible = false;
            _pictureBoxReverse.Visible = false;
        }

        private void PictureBoxReverseMouseEnter(object sender, EventArgs e)
        {
            HideAllReverseImages();
            _pictureBoxReverseOver.Visible = true;
            _pictureBoxReverseOver.BringToFront();
        }

        private void PictureBoxReverseOverMouseLeave(object sender, EventArgs e)
        {
            HideAllReverseImages();
            _pictureBoxReverse.Visible = true;
            _pictureBoxReverse.BringToFront();
        }

        private void PictureBoxReverseOverMouseDown(object sender, MouseEventArgs e)
        {
            HideAllReverseImages();
            _pictureBoxReverseDown.Visible = true;
            _pictureBoxReverseDown.BringToFront();
            if (VideoPlayer != null)
            {
                var newPosition = CurrentPosition - 3.0;
                if (newPosition < 0)
                {
                    newPosition = 0;
                }

                CurrentPosition = newPosition;
            }
        }

        private void PictureBoxReverseOverMouseUp(object sender, MouseEventArgs e)
        {
            HideAllReverseImages();
            _pictureBoxReverse.Visible = true;
            _pictureBoxReverse.BringToFront();
        }

        #endregion Reverse buttons

        #region Fast forward buttons

        private void HideAllFastForwardImages()
        {
            _pictureBoxFastForwardOver.Visible = false;
            _pictureBoxFastForwardDown.Visible = false;
            _pictureBoxFastForward.Visible = false;
        }

        private void PictureBoxFastForwardMouseEnter(object sender, EventArgs e)
        {
            HideAllFastForwardImages();
            _pictureBoxFastForwardOver.Visible = true;
            _pictureBoxFastForwardOver.BringToFront();
        }

        private void PictureBoxFastForwardOverMouseLeave(object sender, EventArgs e)
        {
            HideAllFastForwardImages();
            _pictureBoxFastForward.Visible = true;
            _pictureBoxFastForward.BringToFront();
        }

        private void PictureBoxFastForwardOverMouseDown(object sender, MouseEventArgs e)
        {
            HideAllFastForwardImages();
            _pictureBoxFastForwardDown.Visible = true;
            _pictureBoxFastForwardDown.BringToFront();

            if (VideoPlayer != null)
            {
                var newPosition = CurrentPosition + 3.0;
                if (newPosition < 0)
                {
                    newPosition = 0;
                }

                CurrentPosition = newPosition;
            }
        }

        private void PictureBoxFastForwardOverMouseUp(object sender, MouseEventArgs e)
        {
            HideAllFastForwardImages();
            _pictureBoxFastForward.Visible = true;
            _pictureBoxFastForward.BringToFront();
        }

        #endregion Fast forward buttons

        #region Progress bars

        private void SetProgressBarPosition(int mouseX)
        {
            int max = _pictureBoxProgressbarBackground.Width - 9;
            if (mouseX > max)
            {
                mouseX = max;
            }
            else if (mouseX < 0)
            {
                mouseX = 0;
            }

            double percent = mouseX * 100.0 / max;
            _pictureBoxProgressBar.Width = (int)(max * percent / 100.0);

            CurrentPosition = percent * Duration / 100.0;
        }

        private int SecondsToXPosition(double seconds)
        {
            int max = _pictureBoxProgressbarBackground.Width - 9;
            return (int)Math.Round(seconds * max / Duration);
        }

        private double CursorVideoPosition(double mouseX)
        {
            int max = _pictureBoxProgressbarBackground.Width - 9;
            if (mouseX > max)
            {
                mouseX = max;
            }
            else if (mouseX < 0)
            {
                mouseX = 0;
            }

            double videoPosition = mouseX * Duration / max;

            return videoPosition;
        }

        private string CurrentPositionToolTipText(double mouseX)
        {
            double cursorVideoPosition = CursorVideoPosition(mouseX);
            string toolTiptext = TimeCode.FromSeconds(cursorVideoPosition + Configuration.Settings.General.CurrentVideoOffsetInMs / TimeCode.BaseUnit).ToDisplayString();

            if (_chapters?.Count > 0)
            {
                toolTiptext += " - ";

                for (int index = 0; index < _chapters.Count; index++)
                {
                    var chapterTime = _chapters[index].StartTime;
                    var nextChapterTime = index + 1 < _chapters.Count ? _chapters[index + 1].StartTime : Duration;

                    if (cursorVideoPosition >= chapterTime && cursorVideoPosition < nextChapterTime)
                    {
                        if (_chapters[index].Nested)
                        {
                            toolTiptext += "+ ";
                        }

                        toolTiptext += _chapters[index].Name;
                        break;
                    }
                }
            }

            return toolTiptext;
        }

        private void DrawChapters(Graphics graphics, int mergin, int startPoint, int endPoint)
        {
            try
            {
                int max = _pictureBoxProgressbarBackground.Width - 9;
                int index = 0;
                while (index < _chapters.Count)
                {
                    int pos;
                    try
                    {
                        double time = _chapters[index++].StartTime;
                        pos = SecondsToXPosition(time) + mergin;
                    }
                    catch
                    {
                        pos = -1;
                    }
                    if (pos > 0 && pos < max)
                    {
                        using (var p = new Pen(Color.LightGray))
                        {
                            graphics.DrawLine(p, pos, startPoint, pos, endPoint);
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        private void PictureBoxProgressbarBackgroundMouseDown(object sender, MouseEventArgs e)
        {
            SetProgressBarPosition(e.X - 4);
            OnButtonClicked?.Invoke(sender, e);
        }

        private void PictureBoxProgressBarMouseDown(object sender, MouseEventArgs e)
        {
            SetProgressBarPosition(e.X + 2);
            OnButtonClicked?.Invoke(sender, e);
        }

        private void PictureBoxProgressbarBackgroundPaint(object sender, PaintEventArgs e)
        {
            if (_chapters?.Count > 0)
            {
                DrawChapters(e.Graphics, 3, _pictureBoxProgressBar.Location.Y, _pictureBoxProgressBar.Location.Y + 3);
            }
        }

        private void PictureBoxProgressBarPaint(object sender, PaintEventArgs e)
        {
            if (_chapters?.Count > 0)
            {
                DrawChapters(e.Graphics, -1, 1, _pictureBoxProgressBar.Height);
            }
        }

        private void PictureBoxProgressbarBackgroundMouseMove(object sender, MouseEventArgs e)
        {
            if (VideoPlayer != null)
            {
                string toolTiptext = CurrentPositionToolTipText(e.X - 4);
                if (e.X != _lastCurrentPositionToolTipX || e.Y != _lastCurrentPositionToolTipY)
                {
                    _currentPositionToolTip.Show(toolTiptext, _pictureBoxProgressbarBackground, e.X - 10, e.Y - 25);
                    _lastCurrentPositionToolTipX = e.X;
                    _lastCurrentPositionToolTipY = e.Y;
                }
            }
        }

        private void PictureBoxProgressbarBackgroundMouseLeave(object sender, EventArgs e)
        {
            if (VideoPlayer != null)
            {
                _currentPositionToolTip.Hide(_pictureBoxProgressbarBackground);
            }
        }

        private void PictureBoxProgressBarMouseMove(object sender, MouseEventArgs e)
        {
            if (VideoPlayer != null)
            {
                string toolTiptext = CurrentPositionToolTipText(e.X + 2);
                _currentPositionToolTip.Show(toolTiptext, _pictureBoxProgressBar, e.X - 10, e.Y - 25);
            }
        }

        private void PictureBoxProgressBarMouseLeave(object sender, EventArgs e)
        {
            if (VideoPlayer != null)
            {
                _currentPositionToolTip.Hide(_pictureBoxProgressBar);
            }
        }


        /// <summary>
        /// Use SMPTE time (drop frame mode)
        /// See https://blog.frame.io/2017/07/17/timecode-and-frame-rates/ and
        ///     https://backlothelp.netflix.com/hc/en-us/articles/215131928-How-do-I-know-whether-to-select-SMPTE-or-MEDIA-for-a-timing-reference-
        /// </summary>
        public bool SmpteMode { get; set; }

        public void RefreshProgressBar()
        {
            if (VideoPlayer == null)
            {
                _pictureBoxProgressBar.Width = 0;
                _labelTimeCode.Text = string.Empty;
            }
            else
            {
                int max = _pictureBoxProgressbarBackground.Width - 9;
                double percent = (VideoPlayer.CurrentPosition * 100.0) / VideoPlayer.Duration;
                _pictureBoxProgressBar.Width = (int)(max * percent / 100.0);

                if (Convert.ToInt64(Duration) == 0)
                {
                    return;
                }

                var pos = CurrentPosition;
                if (pos > 1000000)
                {
                    pos = 0;
                }

                var dur = TimeCode.FromSeconds(Duration + Configuration.Settings.General.CurrentVideoOffsetInMs / TimeCode.BaseUnit);
                if (SmpteMode)
                {
                    var span = TimeCode.FromSeconds(pos + 0.017 + Configuration.Settings.General.CurrentVideoOffsetInMs / TimeCode.BaseUnit);
                    _labelTimeCode.Text = $"{span.ToDisplayString()} / {dur.ToDisplayString()} SMPTE";
                }
                else
                {
                    var span = TimeCode.FromSeconds(pos + Configuration.Settings.General.CurrentVideoOffsetInMs / TimeCode.BaseUnit);
                    _labelTimeCode.Text = $"{span.ToDisplayString()} / {dur.ToDisplayString()}";
                }
                ResizeTimeCode();

                RefreshPlayPauseButtons();
            }
        }

        private void SetVolumeBarPosition(int mouseX)
        {
            int max = _pictureBoxVolumeBarBackground.Width - 18;
            if (mouseX > max)
            {
                mouseX = max;
            }

            double percent = (mouseX * 100.0) / max;
            _pictureBoxVolumeBar.Width = (int)(max * percent / 100.0);
            if (_videoPlayer != null)
            {
                _videoPlayer.Volume = (int)percent;
            }

            _labelVolume.Text = (int)percent + "%";
            Configuration.Settings.General.VideoPlayerDefaultVolume = (int)percent;
        }

        private void PictureBoxVolumeBarBackgroundMouseDown(object sender, MouseEventArgs e)
        {
            SetVolumeBarPosition(e.X - 6);
            OnButtonClicked?.Invoke(sender, e);
        }

        private void PictureBoxVolumeBarMouseDown(object sender, MouseEventArgs e)
        {
            SetVolumeBarPosition(e.X + 2);
            OnButtonClicked?.Invoke(sender, e);
        }

        private void RefreshVolumeBar()
        {
            if (VideoPlayer == null)
            {
                _pictureBoxVolumeBar.Width = 0;
            }
            else
            {
                int max = _pictureBoxVolumeBarBackground.Width - 18;
                _pictureBoxVolumeBar.Width = (int)(max * Volume / 100.0);
            }
        }

        #endregion Progress bars

        #region VideoPlayer functions

        public void Play()
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.Play();
                HideAllPlayImages();
                _pictureBoxPause.Visible = true;
                _pictureBoxPause.BringToFront();
                RefreshProgressBar();
            }
            OnButtonClicked?.Invoke(null, null);
        }

        public void Stop()
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.Pause();
                VideoPlayer.CurrentPosition = 0;
                HideAllPauseImages();
                _pictureBoxPlay.Visible = true;
                _pictureBoxPlay.BringToFront();
                RefreshProgressBar();
            }
            OnButtonClicked?.Invoke(null, null);
        }

        public void Pause()
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.Pause();
                HideAllPauseImages();
                _pictureBoxPlay.Visible = true;
                _pictureBoxPlay.BringToFront();
                RefreshProgressBar();
            }
        }

        public void TogglePlayPause()
        {
            if (VideoPlayer != null)
            {
                if (VideoPlayer.IsPaused)
                {
                    Play();
                }
                else
                {
                    Pause();
                }
            }
        }

        public bool IsPaused => VideoPlayer?.IsPaused == true;

        public double Volume
        {
            get
            {
                if (VideoPlayer != null)
                {
                    return VideoPlayer.Volume;
                }

                return 0;
            }
            set
            {
                if (VideoPlayer != null)
                {
                    if (value > 0)
                    {
                        _muteOldVolume = null;
                    }

                    if (value > 100)
                    {
                        VideoPlayer.Volume = 100;
                    }
                    else if (value < 0)
                    {
                        VideoPlayer.Volume = 0;
                    }
                    else
                    {
                        VideoPlayer.Volume = (int)value;
                    }

                    RefreshVolumeBar();
                }
            }
        }

        /// <summary>
        /// Current position in seconds
        /// </summary>
        public double CurrentPosition
        {
            get
            {
                if (VideoPlayer != null)
                {
                    if (SmpteMode)
                    {
                        return VideoPlayer.CurrentPosition / 1.001;
                    }

                    return VideoPlayer.CurrentPosition;
                }
                return 0;
            }
            set
            {
                if (VideoPlayer != null)
                {
                    if (SmpteMode)
                    {
                        VideoPlayer.CurrentPosition = value * 1.001;
                    }
                    else
                    {
                        VideoPlayer.CurrentPosition = value;
                    }
                }
                else
                {
                    RefreshProgressBar();
                }
            }
        }

        /// <summary>
        /// Total duration in seconds
        /// </summary>
        public double Duration
        {
            get
            {
                if (VideoPlayer != null)
                {
                    return VideoPlayer.Duration;
                }

                return 0;
            }
        }

        private bool Mute
        {
            get
            {
                if (VideoPlayer != null)
                {
                    return _isMuted;
                }
                return false;
            }
            set
            {
                if (VideoPlayer != null)
                {
                    if (!value && _muteOldVolume != null)
                    {
                        Volume = _muteOldVolume.Value;
                        _labelVolume.Visible = true;
                    }
                    else if (value)
                    {
                        _muteOldVolume = Volume;
                        Volume = 0;
                        _labelVolume.Visible = false;
                    }
                    _isMuted = value;
                }
            }
        }

        #endregion VideoPlayer functions

        protected override void Dispose(bool disposing)
        {
            DeleteTempMpvFileName();
            base.Dispose(disposing);
            _retryCount = 3;
            SmpteMode = false;
        }

        public void PauseAndDisposePlayer()
        {
            PanelPlayer.Hide();
            Pause();
            SubtitleText = string.Empty;
            Chapters = new List<MatroskaChapter>();
            MpvPreviewStyleHeader = null;
            var temp = VideoPlayer;
            VideoPlayer = null;
            Application.DoEvents();
            temp.DisposeVideoPlayer();

            // to avoid not showing video with libmpv, a new PanelPlayer is made...
            PanelPlayer.MouseDown -= PanelPlayerMouseDown;
            Controls.Add(MakePlayerPanel());
            PanelPlayer.BringToFront();
            PanelPlayer.MouseDown += PanelPlayerMouseDown;
            VideoPlayerContainerResize(this, null);

            DeleteTempMpvFileName();
            _retryCount = 3;
            SmpteMode = false;
            RefreshProgressBar();
        }

        private bool IsMouseOverControl(Control control, Point cursorLocation)
        {
            if (control.ClientRectangle.Contains(cursorLocation))
            {
                return true;
            }

            return false;
        }
    }
}

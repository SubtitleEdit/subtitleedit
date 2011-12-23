using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic.VideoPlayers;

namespace Nikse.SubtitleEdit.Controls
{
    public sealed class VideoPlayerContainer : Panel
    {

        class RichTextBoxViewOnly : System.Windows.Forms.RichTextBox
        {
            public RichTextBoxViewOnly()
            {
                base.ReadOnly = true;
                base.BorderStyle = BorderStyle.None;
                base.TabStop = false;
                base.SetStyle(ControlStyles.Selectable, false);
                base.SetStyle(ControlStyles.UserMouse, true);
                base.MouseEnter += delegate(object sender, EventArgs e) { this.Cursor = Cursors.Default; };
                base.ScrollBars = RichTextBoxScrollBars.None;
                base.Margin = new System.Windows.Forms.Padding(0);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x204) return; // WM_RBUTTONDOWN
                if (m.Msg == 0x205) return; // WM_RBUTTONUP
                base.WndProc(ref m);
            }
        }

        public event EventHandler OnButtonClicked;
        public Panel PanelPlayer { get; private set; }
        private Panel _panelSubtitle;
        private RichTextBoxViewOnly _subtitleTextBox;
        private string _subtitleText = string.Empty;
        private VideoPlayer _videoPlayer;
        public float FontSizeFactor { get; set; }
        public VideoPlayer VideoPlayer
        {
            get { return _videoPlayer; }
            set { _videoPlayer = value; }
        }

        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }

        private bool _isMuted;
        private double? _muteOldVolume;
        private readonly System.ComponentModel.ComponentResourceManager _resources;
        private const int ControlsHeight = 47;
        private const int SubtitlesHeight = 57;
        private readonly Color _backgroundColor = Color.FromArgb(18, 18, 18);
        private Panel _panelcontrols;

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
        private PictureBox _pictureBoxPause = new PictureBox();
        private PictureBox _pictureBoxPauseOver = new PictureBox();
        private PictureBox _pictureBoxPauseDown = new PictureBox();
        private PictureBox _pictureBoxStop = new PictureBox();
        private PictureBox _pictureBoxStopOver = new PictureBox();
        private PictureBox _pictureBoxStopDown = new PictureBox();
        private PictureBox _pictureBoxMute = new PictureBox();
        private PictureBox _pictureBoxMuteOver = new PictureBox();
        private PictureBox _pictureBoxMuteDown = new PictureBox();
        private PictureBox _pictureBoxProgressbarBackground = new PictureBox();
        private PictureBox _pictureBoxProgressBar = new PictureBox();
        private PictureBox _pictureBoxVolumeBarBackground = new PictureBox();
        private PictureBox _pictureBoxVolumeBar = new PictureBox();
        private Label _labelTimeCode = new Label();

        public RightToLeft TextRightToLeft
        {
            get
            {
                return _subtitleTextBox.RightToLeft;
            }
            set
            {
                _subtitleTextBox.RightToLeft = value;
                _subtitleTextBox.SelectAll();
                _subtitleTextBox.SelectionAlignment = HorizontalAlignment.Center;
            }
        }

        public bool ShowStopButton
        {
            get
            {
                return _pictureBoxStop.Visible || _pictureBoxStopOver.Visible || _pictureBoxStopDown.Visible;
            }
            set
            {
                if (value)
                {
                    _pictureBoxStop.Visible = true;
                }
                else
                {
                    HideAllStopImages();
                }
            }
        }

        public VideoPlayerContainer()
        {
            FontSizeFactor = 1.0F;
            BorderStyle = System.Windows.Forms.BorderStyle.None;
            _resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoPlayerContainer));
            BackColor = _backgroundColor;
            Controls.Add(MakePlayerPanel());
            Controls.Add(MakeSubtitlesPanel());
            Controls.Add(MakeControlsPanel());
            _panelcontrols.BringToFront();

            HideAllPlayImages();
            HideAllPauseImages();
            _pictureBoxPlay.Visible = true;
            _pictureBoxPlay.BringToFront();

            HideAllStopImages();
            _pictureBoxStop.Visible = true;

            HideAllMuteImages();
            _pictureBoxMute.Visible = true;

            HideAllReverseImages();
            _pictureBoxReverse.Visible = true;

            HideAllFastForwardImages();
            _pictureBoxFastForward.Visible = true;

            VideoPlayerContainerResize(this, null);
            Resize += VideoPlayerContainerResize;

            _pictureBoxProgressBar.Width = 0;

            PanelPlayer.MouseDown += PanelPlayer_MouseDown;
        }

        public void EnableMouseWheelStep()
        {
            AddMouseWheelEvent(this);
        }

        private void AddMouseWheelEvent(Control control)
        {
            control.MouseWheel += Control_MouseWheel;
            foreach (Control ctrl in control.Controls)
                AddMouseWheelEvent(ctrl);
        }

        void Control_MouseWheel(object sender, MouseEventArgs e)
        {
            int delta = e.Delta;
            double newPosition = CurrentPosition - (delta / 256.0);
            if (newPosition < 0)
                newPosition = 0;
            else if (newPosition > Duration)
                newPosition = Duration;
            CurrentPosition = newPosition;
        }

        private Control MakeSubtitlesPanel()
        {
            _panelSubtitle = new Panel { BackColor = _backgroundColor, Left = 0, Top = 0 };
            _panelSubtitle.Height = SubtitlesHeight + 1;
            _subtitleTextBox = new RichTextBoxViewOnly();
            _panelSubtitle.Controls.Add(_subtitleTextBox);
            _subtitleTextBox.BackColor = _backgroundColor;
            _subtitleTextBox.ForeColor = Color.White;
            _subtitleTextBox.Dock = DockStyle.Fill;
            SetSubtitleFont();
            _subtitleTextBox.MouseClick += SubtitleTextBox_MouseClick;
            return _panelSubtitle;
        }

        public void SetSubtitleFont()
        {
            var gs = Nikse.SubtitleEdit.Logic.Configuration.Settings.General;
            if (string.IsNullOrEmpty(gs.SubtitleFontName))
                gs.SubtitleFontName = "Tahoma";
            _subtitleTextBox.Font = new Font(gs.SubtitleFontName, gs.VideoPlayerPreviewFontSize * FontSizeFactor, FontStyle.Bold);
        }

        void SubtitleTextBox_MouseClick(object sender, MouseEventArgs e)
        {
            TogglePlayPause();
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

        public string SubtitleText
        {
            get
            {
                return _subtitleText;
            }
            set
            {
                _subtitleText = value;

                // remove styles for display text (except italic)
                string text = RemoveSubStationAlphaFormatting(_subtitleText);
                text = text.Replace("<b>", string.Empty);
                text = text.Replace("</b>", string.Empty);
                text = text.Replace("<B>", string.Empty);
                text = text.Replace("</B>", string.Empty);
                text = text.Replace("<u>", string.Empty);
                text = text.Replace("</u>", string.Empty);
                text = text.Replace("<U>", string.Empty);
                text = text.Replace("</U>", string.Empty);
                text = Logic.Utilities.RemoveHtmlFontTag(text);

                // display italic
                StringBuilder sb = new StringBuilder();
                int i = 0;
                bool isItalic = false;
                int italicBegin = 0;
                _subtitleTextBox.Text = string.Empty;
                int letterCount = 0;
                System.Collections.Generic.Dictionary<int, int> italicLookups = new System.Collections.Generic.Dictionary<int, int>();
                while  (i < text.Length)
                {
                    if (text.Substring(i).ToLower().StartsWith("<i>"))
                    {
                        _subtitleTextBox.AppendText(sb.ToString());
                        sb = new StringBuilder();
                        isItalic = true;
                        italicBegin = letterCount;
                        i+=2;
                    }
                    else if (text.Substring(i).ToLower().StartsWith("</i>") && isItalic)
                    {
                        italicLookups.Add(italicBegin, sb.Length);
                        _subtitleTextBox.AppendText(sb.ToString());
                        sb = new StringBuilder();
                        isItalic = false;
                        i += 3;
                    }
                    else if (text.Substring(i, 1) == "\n") // RichTextBox only count NewLine as one character!
                    {
                        sb.Append(text.Substring(i, 1));
                    }
                    else
                    {
                        sb.Append(text.Substring(i, 1));
                        letterCount++;
                    }
                    i++;
                }
                _subtitleTextBox.Text += sb.ToString();
                _subtitleTextBox.SelectAll();
                _subtitleTextBox.SelectionAlignment = HorizontalAlignment.Center;
                _subtitleTextBox.DeselectAll();
                foreach (var entry in italicLookups)
                {
                    System.Drawing.Font currentFont = _subtitleTextBox.SelectionFont;
                    System.Drawing.FontStyle newFontStyle = FontStyle.Italic;
//                    if (Nikse.SubtitleEdit.Logic.Configuration.Settings.General.SubtitleFontBold)
                        newFontStyle = FontStyle.Italic | FontStyle.Bold;
                    _subtitleTextBox.SelectionStart = entry.Key;
                    _subtitleTextBox.SelectionLength = entry.Value;
                    _subtitleTextBox.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newFontStyle);
                    _subtitleTextBox.DeselectAll();
                }
            }
        }

        void PanelPlayer_MouseDown(object sender, MouseEventArgs e)
        {
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

        public void HideControls()
        {
            if (_panelcontrols.Visible)
            {
                _panelSubtitle.Height = _panelSubtitle.Height + ControlsHeight;
                _panelcontrols.Visible = false;
            }
        }

        public void ShowControls()
        {
            if (!_panelcontrols.Visible)
            {
                _panelcontrols.Visible = true;
                _panelSubtitle.Height = _panelSubtitle.Height - ControlsHeight;
            }
        }

        private Control MakeControlsPanel()
        {
            _panelcontrols = new Panel {Left = 0, Height = ControlsHeight};

            _pictureBoxBackground = new PictureBox
                                        {
                                            Image = ((Image) (_resources.GetObject("pictureBoxBar.Image"))),
                                            Location = new Point(0, 0),
                                            Name = "_pictureBoxBackground",
                                            Size = new Size(200, 45),
                                            SizeMode = PictureBoxSizeMode.StretchImage,
                                            TabStop = false
                                        };
            _panelcontrols.Controls.Add(_pictureBoxBackground);

            _pictureBoxPlay = new PictureBox
                                  {
                                      Image = ((Image) (_resources.GetObject("pictureBoxPlay.Image"))),
                                      Location = new Point(22, 126 - 113),
                                      Name = "_pictureBoxPlay",
                                      Size = new Size(29, 29),
                                      SizeMode = PictureBoxSizeMode.AutoSize,
                                      TabStop = false
                                  };
            _pictureBoxPlay.MouseEnter += PictureBoxPlayMouseEnter;
            _panelcontrols.Controls.Add(_pictureBoxPlay);

            _pictureBoxPlayDown = new PictureBox
                                      {
                                          Image = ((Image) (_resources.GetObject("pictureBoxPlayDown.Image"))),
                                          Location = new Point(22, 127 - 113),
                                          Name = "_pictureBoxPlayDown",
                                          Size = new Size(29, 29),
                                          SizeMode = PictureBoxSizeMode.AutoSize,
                                          TabStop = false
                                      };
            _panelcontrols.Controls.Add(_pictureBoxPlayDown);

            _pictureBoxPlayOver = new PictureBox
                                      {
                                          Image = ((Image) (_resources.GetObject("pictureBoxPlayOver.Image"))),
                                          Location = new Point(23, 126 - 113),
                                          Name = "_pictureBoxPlayOver",
                                          Size = new Size(29, 29),
                                          SizeMode = PictureBoxSizeMode.AutoSize,
                                          TabStop = false
                                      };
            _pictureBoxPlayOver.MouseLeave += PictureBoxPlayOverMouseLeave;
            _pictureBoxPlayOver.MouseDown += PictureBoxPlayOverMouseDown;
            _pictureBoxPlayOver.MouseUp += PictureBoxPlayOverMouseUp;
            _panelcontrols.Controls.Add(_pictureBoxPlayOver);

            _pictureBoxPause.Image = ((Image)(_resources.GetObject("pictureBoxPause.Image")));
            _pictureBoxPause.Location = new Point(23, 126 - 113);
            _pictureBoxPause.Name = "_pictureBoxPause";
            _pictureBoxPause.Size = new Size(29, 29);
            _pictureBoxPause.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxPause.TabStop = false;
            _pictureBoxPause.MouseEnter += PictureBoxPauseMouseEnter;
            _panelcontrols.Controls.Add(_pictureBoxPause);

            _pictureBoxPauseDown.Image = ((Image)(_resources.GetObject("pictureBoxPauseDown.Image")));
            _pictureBoxPauseDown.Location = new Point(22, 127 - 113);
            _pictureBoxPauseDown.Name = "_pictureBoxPauseDown";
            _pictureBoxPauseDown.Size = new Size(29, 29);
            _pictureBoxPauseDown.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxPauseDown.TabStop = false;
            _panelcontrols.Controls.Add(_pictureBoxPauseDown);

            _pictureBoxPauseOver.Image = ((Image)(_resources.GetObject("pictureBoxPauseOver.Image")));
            _pictureBoxPauseOver.Location = new Point(22, 127 - 113);
            _pictureBoxPauseOver.Name = "_pictureBoxPauseOver";
            _pictureBoxPauseOver.Size = new Size(29, 29);
            _pictureBoxPauseOver.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxPauseOver.TabStop = false;
            _pictureBoxPauseOver.MouseLeave += PictureBoxPauseOverMouseLeave;
            _pictureBoxPauseOver.MouseDown += PictureBoxPauseOverMouseDown;
            _pictureBoxPauseOver.MouseUp += PictureBoxPauseOverMouseUp;
            _panelcontrols.Controls.Add(_pictureBoxPauseOver);

            _pictureBoxStop.Image = ((Image)(_resources.GetObject("pictureBoxStop.Image")));
            _pictureBoxStop.Location = new Point(60, 130 - 113);
            _pictureBoxStop.Name = "_pictureBoxStop";
            _pictureBoxStop.Size = new Size(20, 20);
            _pictureBoxStop.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxStop.TabStop = false;
            _pictureBoxStop.MouseEnter += PictureBoxStopMouseEnter;
            _panelcontrols.Controls.Add(_pictureBoxStop);

            _pictureBoxStopDown.Image = ((Image)(_resources.GetObject("pictureBoxStopDown.Image")));
            _pictureBoxStopDown.Location = new Point(60, 130 - 113);
            _pictureBoxStopDown.Name = "_pictureBoxStopDown";
            _pictureBoxStopDown.Size = new Size(20, 20);
            _pictureBoxStopDown.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxStopDown.TabStop = false;
            _panelcontrols.Controls.Add(_pictureBoxStopDown);

            _pictureBoxStopOver.Image = ((Image)(_resources.GetObject("pictureBoxStopOver.Image")));
            _pictureBoxStopOver.Location = new Point(60, 130 - 113);
            _pictureBoxStopOver.Name = "_pictureBoxStopOver";
            _pictureBoxStopOver.Size = new Size(20, 20);
            _pictureBoxStopOver.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxStopOver.TabStop = false;
            _pictureBoxStopOver.MouseLeave += PictureBoxStopOverMouseLeave;
            _pictureBoxStopOver.MouseDown += PictureBoxStopOverMouseDown;
            _pictureBoxStopOver.MouseUp += PictureBoxStopOverMouseUp;
            _panelcontrols.Controls.Add(_pictureBoxStopOver);

            _pictureBoxProgressbarBackground.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _pictureBoxProgressbarBackground.BackColor = Color.Transparent;
            _pictureBoxProgressbarBackground.Image = (Image)_resources.GetObject("pictureBoxProgressbarBackground.Image");
            _pictureBoxProgressbarBackground.Location = new Point(43, 114 - 113);
            _pictureBoxProgressbarBackground.Margin = new Padding(0);
            _pictureBoxProgressbarBackground.Name = "_pictureBoxProgressbarBackground";
            _pictureBoxProgressbarBackground.Size = new Size(531, 12);
            _pictureBoxProgressbarBackground.SizeMode = PictureBoxSizeMode.StretchImage;
            _pictureBoxProgressbarBackground.TabStop = false;
            _pictureBoxProgressbarBackground.MouseDown += PictureBoxProgressbarBackgroundMouseDown;
            _panelcontrols.Controls.Add(_pictureBoxProgressbarBackground);

            _pictureBoxProgressBar.Image = (Image)(_resources.GetObject("pictureBoxProgressBar.Image"));
            _pictureBoxProgressBar.Location = new Point(47, 118 - 113);
            _pictureBoxProgressBar.Name = "_pictureBoxProgressBar";
            _pictureBoxProgressBar.Size = new Size(318, 4);
            _pictureBoxProgressBar.SizeMode = PictureBoxSizeMode.StretchImage;
            _pictureBoxProgressBar.TabStop = false;
            _pictureBoxProgressBar.MouseDown += PictureBoxProgressBarMouseDown;
            _panelcontrols.Controls.Add(_pictureBoxProgressBar);
            _pictureBoxProgressBar.BringToFront();

            _pictureBoxMute.Image = ((Image)(_resources.GetObject("pictureBoxMute.Image")));
            _pictureBoxMute.Location = new Point(91, 131 - 113);
            _pictureBoxMute.Name = "_pictureBoxMute";
            _pictureBoxMute.Size = new Size(19, 19);
            _pictureBoxMute.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxMute.TabStop = false;
            _pictureBoxMute.MouseEnter += PictureBoxMuteMouseEnter;
            _panelcontrols.Controls.Add(_pictureBoxMute);

            _pictureBoxMuteDown.Image = ((Image)(_resources.GetObject("pictureBoxMuteDown.Image")));
            _pictureBoxMuteDown.Location = new Point(91, 131 - 113);
            _pictureBoxMuteDown.Name = "_pictureBoxMuteDown";
            _pictureBoxMuteDown.Size = new Size(19, 19);
            _pictureBoxMuteDown.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxMuteDown.TabStop = false;
            _pictureBoxMuteDown.Click += PictureBoxMuteDownClick;
            _panelcontrols.Controls.Add(_pictureBoxMuteDown);

            _pictureBoxMuteOver.Image = ((Image)(_resources.GetObject("pictureBoxMuteOver.Image")));
            _pictureBoxMuteOver.Location = new Point(91, 131 - 113);
            _pictureBoxMuteOver.Name = "_pictureBoxMuteOver";
            _pictureBoxMuteOver.Size = new Size(19, 19);
            _pictureBoxMuteOver.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxMuteOver.TabStop = false;
            _pictureBoxMuteOver.MouseLeave += PictureBoxMuteOverMouseLeave;
            _pictureBoxMuteOver.MouseDown += PictureBoxMuteOverMouseDown;
            _pictureBoxMuteOver.MouseUp += PictureBoxMuteOverMouseUp;
            _panelcontrols.Controls.Add(_pictureBoxMuteOver);

            _pictureBoxVolumeBarBackground.Image = ((Image)(_resources.GetObject("pictureBoxVolumeBarBackground.Image")));
            _pictureBoxVolumeBarBackground.Location = new Point(111, 135 - 113);
            _pictureBoxVolumeBarBackground.Name = "_pictureBoxVolumeBarBackground";
            _pictureBoxVolumeBarBackground.Size = new Size(82, 13);
            _pictureBoxVolumeBarBackground.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxVolumeBarBackground.TabStop = false;
            _pictureBoxVolumeBarBackground.MouseDown += PictureBoxVolumeBarBackgroundMouseDown;
            _panelcontrols.Controls.Add(_pictureBoxVolumeBarBackground);

            _pictureBoxVolumeBar.Image = ((Image)(_resources.GetObject("pictureBoxVolumeBar.Image")));
            _pictureBoxVolumeBar.Location = new Point(120, 139 - 113);
            _pictureBoxVolumeBar.Name = "_pictureBoxVolumeBar";
            _pictureBoxVolumeBar.Size = new Size(48, 4);
            _pictureBoxVolumeBar.SizeMode = PictureBoxSizeMode.StretchImage;
            _pictureBoxVolumeBar.TabStop = false;
            _pictureBoxVolumeBar.MouseDown += PictureBoxVolumeBarMouseDown;
            _panelcontrols.Controls.Add(_pictureBoxVolumeBar);
            _pictureBoxVolumeBar.BringToFront();

            _pictureBoxReverse = new PictureBox();
            _pictureBoxReverse.Image = ((Image)(_resources.GetObject("pictureBoxReverse.Image")));
            _pictureBoxReverse.Location = new Point(28, 3);
            _pictureBoxReverse.Name = "_pictureBoxReverse";
            _pictureBoxReverse.Size = new Size(16, 8);
            _pictureBoxReverse.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxReverse.TabStop = false;
            _panelcontrols.Controls.Add(_pictureBoxReverse);
            _pictureBoxReverse.MouseEnter += PictureBoxReverseMouseEnter;

            _pictureBoxReverseOver = new PictureBox();
            _pictureBoxReverseOver.Image = ((Image)(_resources.GetObject("pictureBoxReverseMouseOver.Image")));
            _pictureBoxReverseOver.Location = _pictureBoxReverse.Location;
            _pictureBoxReverseOver.Name = "_pictureBoxReverseOver";
            _pictureBoxReverseOver.Size = _pictureBoxReverse.Size;
            _pictureBoxReverseOver.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxReverseOver.TabStop = false;
            _panelcontrols.Controls.Add(_pictureBoxReverseOver);
            _pictureBoxReverseOver.MouseLeave += PictureBoxReverseOverMouseLeave;
            _pictureBoxReverseOver.MouseDown += PictureBoxReverseOverMouseDown;
            _pictureBoxReverseOver.MouseUp += PictureBoxReverseOverMouseUp;

            _pictureBoxReverseDown = new PictureBox();
            _pictureBoxReverseDown.Image = ((Image)(_resources.GetObject("pictureBoxReverseMouseDown.Image")));
            _pictureBoxReverseDown.Location = _pictureBoxReverse.Location;
            _pictureBoxReverseDown.Name = "_pictureBoxReverseOver";
            _pictureBoxReverseDown.Size = _pictureBoxReverse.Size;
            _pictureBoxReverseDown.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxReverseDown.TabStop = false;
            _panelcontrols.Controls.Add(_pictureBoxReverseDown);

            _pictureBoxFastForward = new PictureBox();
            _pictureBoxFastForward.Image = ((Image)(_resources.GetObject("pictureBoxFastForward.Image")));
            _pictureBoxFastForward.Location = new Point(571, 1);
            _pictureBoxFastForward.Name = "_pictureBoxFastForward";
            _pictureBoxFastForward.Size = new Size(17, 13);
            _pictureBoxFastForward.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxFastForward.TabStop = false;
            _panelcontrols.Controls.Add(_pictureBoxFastForward);
            _pictureBoxFastForward.MouseEnter += PictureBoxFastForwardMouseEnter;

            _pictureBoxFastForwardOver = new PictureBox();
            _pictureBoxFastForwardOver.Image = ((Image)(_resources.GetObject("pictureBoxFastForwardMouseOver.Image")));
            _pictureBoxFastForwardOver.Location = _pictureBoxFastForward.Location;
            _pictureBoxFastForwardOver.Name = "_pictureBoxFastForwardOver";
            _pictureBoxFastForwardOver.Size = _pictureBoxFastForward.Size;
            _pictureBoxFastForwardOver.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxFastForwardOver.TabStop = false;
            _panelcontrols.Controls.Add(_pictureBoxFastForwardOver);
            _pictureBoxFastForwardOver.MouseLeave += PictureBoxFastForwardOverMouseLeave;
            _pictureBoxFastForwardOver.MouseDown += PictureBoxFastForwardOverMouseDown;
            _pictureBoxFastForwardOver.MouseUp += PictureBoxFastForwardOverMouseUp;

            _pictureBoxFastForwardDown = new PictureBox();
            _pictureBoxFastForwardDown.Image = ((Image)(_resources.GetObject("pictureBoxFastForwardMouseDown.Image")));
            _pictureBoxFastForwardDown.Location = _pictureBoxFastForward.Location;
            _pictureBoxFastForwardDown.Name = "_pictureBoxFastForwardDown";
            _pictureBoxFastForwardDown.Size = _pictureBoxFastForward.Size;
            _pictureBoxFastForwardDown.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBoxFastForwardDown.TabStop = false;
            _panelcontrols.Controls.Add(_pictureBoxFastForwardDown);

            _labelTimeCode.Location = new Point(280, 28);
            _labelTimeCode.ForeColor = Color.Gray;
            _labelTimeCode.Font = new Font(_labelTimeCode.Font.FontFamily, 7);
            _labelTimeCode.AutoSize = true;
            _panelcontrols.Controls.Add(_labelTimeCode);

            _pictureBoxBackground.SendToBack();
            _pictureBoxFastForward.BringToFront();
            _pictureBoxFastForwardDown.BringToFront();
            _pictureBoxFastForwardOver.BringToFront();
            _pictureBoxPlay.BringToFront();

            _panelcontrols.BackColor = _backgroundColor;
            _pictureBoxPlay.BringToFront();
            _pictureBoxPlayDown.BringToFront();
            _pictureBoxPlayOver.BringToFront();
            _labelTimeCode.BringToFront();
            return _panelcontrols;
        }

        public void VideoPlayerContainerResize(object sender, EventArgs e)
        {
            PanelPlayer.Height = Height - (ControlsHeight + SubtitlesHeight);
            PanelPlayer.Width = Width;

            _panelSubtitle.Top = Height - (ControlsHeight + SubtitlesHeight);
            _panelSubtitle.Width = Width;

            _panelcontrols.Top = Height - ControlsHeight + 2;
            _panelcontrols.Width = Width;
            _pictureBoxBackground.Width = Width;
            _pictureBoxProgressbarBackground.Width = Width - (_pictureBoxProgressbarBackground.Left * 2);
            _pictureBoxFastForward.Left = Width - 48;
            _pictureBoxFastForwardDown.Left = _pictureBoxFastForward.Left;
            _pictureBoxFastForwardOver.Left = _pictureBoxFastForward.Left;

            _labelTimeCode.Left = Width - 170;
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
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke(sender, e);
        }

        private void PictureBoxPlayOverMouseUp(object sender, MouseEventArgs e)
        {
            HideAllPlayImages();
            _pictureBoxPause.Visible = true;
            _pictureBoxPause.BringToFront();
            Play();
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

            }
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke(sender, e);
        }

        private void PictureBoxPauseOverMouseUp(object sender, MouseEventArgs e)
        {
            HideAllPauseImages();
            _pictureBoxPlay.Visible = true;
            Pause();
        }
        #endregion

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
        }

        private void PictureBoxStopOverMouseLeave(object sender, EventArgs e)
        {
            if (_pictureBoxStopOver.Visible)
            {
                HideAllStopImages();
                _pictureBoxStop.Visible = true;
            }
        }

        private void PictureBoxStopOverMouseDown(object sender, MouseEventArgs e)
        {
            if (_pictureBoxStopOver.Visible)
            {
                HideAllStopImages();
                _pictureBoxStopDown.Visible = true;
            }
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke(sender, e);
        }

        private void PictureBoxStopOverMouseUp(object sender, MouseEventArgs e)
        {
            HideAllStopImages();
            _pictureBoxStop.Visible = true;
            Stop();
        }
        #endregion

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
                _pictureBoxMuteDown.Visible = true;
            else
                _pictureBoxMuteOver.Visible = true;
        }

        private void PictureBoxMuteOverMouseLeave(object sender, EventArgs e)
        {
            if (_pictureBoxMuteOver.Visible)
            {
                HideAllMuteImages();
                _pictureBoxMute.Visible = true;
            }
        }

        private void PictureBoxMuteOverMouseDown(object sender, MouseEventArgs e)
        {
            if (_pictureBoxMuteOver.Visible)
            {
                HideAllMuteImages();
                _pictureBoxMuteDown.Visible = true;
            }
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke(sender, e);
        }

        private void PictureBoxMuteOverMouseUp(object sender, MouseEventArgs e)
        {
            HideAllMuteImages();
            Mute = true;
            _pictureBoxMuteDown.Visible = true;
        }

        private void PictureBoxMuteDownClick(object sender, EventArgs e)
        {
            Mute = false;
            HideAllMuteImages();
            _pictureBoxMute.Visible = true;
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke(sender, e);
        }

        #endregion

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
        }

        private void PictureBoxReverseOverMouseLeave(object sender, EventArgs e)
        {
            HideAllReverseImages();
            _pictureBoxReverse.Visible = true;
        }

        private void PictureBoxReverseOverMouseDown(object sender, MouseEventArgs e)
        {
            HideAllReverseImages();
            _pictureBoxReverseDown.Visible = true;
            if (VideoPlayer != null)
            {
                var newPosition = CurrentPosition - 3.0;
                if (newPosition < 0)
                    newPosition = 0;
                CurrentPosition = newPosition;
            }
        }

        private void PictureBoxReverseOverMouseUp(object sender, MouseEventArgs e)
        {
            HideAllReverseImages();
            _pictureBoxReverse.Visible = true;
        }

        #endregion

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
        }

        private void PictureBoxFastForwardOverMouseLeave(object sender, EventArgs e)
        {
            HideAllFastForwardImages();
            _pictureBoxFastForward.Visible = true;
        }

        private void PictureBoxFastForwardOverMouseDown(object sender, MouseEventArgs e)
        {
            HideAllFastForwardImages();
            _pictureBoxFastForwardDown.Visible = true;

            if (VideoPlayer != null)
            {
                var newPosition = CurrentPosition + 3.0;
                if (newPosition < 0)
                    newPosition = 0;
                CurrentPosition = newPosition;
            }
        }

        private void PictureBoxFastForwardOverMouseUp(object sender, MouseEventArgs e)
        {
            HideAllFastForwardImages();
            _pictureBoxFastForward.Visible = true;
        }

        #endregion

        #region Progress bars
        private void SetProgressBarPosition(int mouseX)
        {
            int max = _pictureBoxProgressbarBackground.Width - 9;
            if (mouseX > max)
                mouseX = max;

            double percent = (mouseX * 100.0) / max;
            _pictureBoxProgressBar.Width = (int)(max * percent / 100.0);

            double pos = percent * Duration / 100;
            CurrentPosition = pos;
        }

        private void PictureBoxProgressbarBackgroundMouseDown(object sender, MouseEventArgs e)
        {
            SetProgressBarPosition(e.X - 4);
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke(sender, e);
        }

        private void PictureBoxProgressBarMouseDown(object sender, MouseEventArgs e)
        {
            SetProgressBarPosition(e.X + 2);
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke(sender, e);
        }

        public void RefreshProgressBar()
        {
            if (VideoPlayer == null)
            {
                _pictureBoxProgressBar.Width = 0;
            }
            else
            {
                int max = _pictureBoxProgressbarBackground.Width - 9;
                double percent = (VideoPlayer.CurrentPosition * 100.0) / VideoPlayer.Duration;
                _pictureBoxProgressBar.Width = (int)(max * percent / 100.0);

                if (VideoPlayer.Duration == 0)
                    return;

                var pos = CurrentPosition;
                if (pos > 1000000)
                    pos = 0;
                var span = TimeSpan.FromSeconds(pos);
                var dur = TimeSpan.FromSeconds(Duration);
                _labelTimeCode.Text = string.Format("{0:00}:{1:00}:{2:00},{3:000} / {4:00}:{5:00}:{6:00},{7:000}", span.Hours, span.Minutes, span.Seconds, span.Milliseconds,
                                                     dur.Hours, dur.Minutes, dur.Seconds, dur.Milliseconds);

                RefreshPlayPauseButtons();
            }
        }

        private void SetVolumeBarPosition(int mouseX)
        {
            int max = _pictureBoxVolumeBarBackground.Width - 18;
            if (mouseX > max)
                mouseX = max;

            double percent = (mouseX * 100.0) / max;
            _pictureBoxVolumeBar.Width = (int)(max * percent / 100.0);
            if (_videoPlayer != null)
                _videoPlayer.Volume = (int)percent;

        }

        private void PictureBoxVolumeBarBackgroundMouseDown(object sender, MouseEventArgs e)
        {
            SetVolumeBarPosition(e.X - 6);
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke(sender, e);
        }

        private void PictureBoxVolumeBarMouseDown(object sender, MouseEventArgs e)
        {
            SetVolumeBarPosition(e.X + 2);
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke(sender, e);
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

        #endregion

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
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke(null, null);
        }

        public void Stop()
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.Pause();
                CurrentPosition = 0;
                HideAllPauseImages();
                _pictureBoxPlay.Visible = true;
                RefreshProgressBar();
            }
            if (OnButtonClicked != null)
                OnButtonClicked.Invoke(null, null);
        }

        public void Pause()
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.Pause();
                HideAllPauseImages();
                _pictureBoxPlay.Visible = true;
                RefreshProgressBar();
            }
        }

        public void TogglePlayPause()
        {
            if (VideoPlayer != null)
            {
                if (VideoPlayer.IsPaused)
                    Play();
                else
                    Pause();
            }
        }

        public bool IsPaused
        {
            get
            {
                if (VideoPlayer != null)
                {
                    return VideoPlayer.IsPaused;
                }
                return false;
            }
        }

        public double Volume
        {
            get
            {
                if (VideoPlayer != null)
                    return VideoPlayer.Volume;
                return 0;
            }
            set
            {
                if (VideoPlayer != null)
                {
                    if (value > 0)
                        _muteOldVolume = null;

                    if (value > 100)
                        VideoPlayer.Volume = 100;
                    else if (value < 0)
                        VideoPlayer.Volume = 0;
                    else
                        VideoPlayer.Volume = (int)value;

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
                    return VideoPlayer.CurrentPosition;
                return 0;
            }
            set
            {
                if (VideoPlayer != null)
                {
                    VideoPlayer.CurrentPosition = value;
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
                    return VideoPlayer.Duration;
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
                    if (value == false && _muteOldVolume != null)
                    {
                        Volume = _muteOldVolume.Value;
                    }
                    else if (value)
                    {
                        _muteOldVolume = Volume;
                        Volume = 0;
                    }
                    _isMuted = value;
                }
            }
        }
        #endregion

    }
}

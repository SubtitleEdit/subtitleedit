using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Forms.Options;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VideoError : Form
    {
        public bool VideoPlayerInstalled { get; set; }

        public VideoError()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonCancel);
        }

        public void Initialize(string fileName)
        {
            Text += fileName;
            var sb = new StringBuilder();
            sb.Append("SE was unable to play the video/audio file (or file is not a valid video/audio file).");

            var currentVideoPlayer = Configuration.Settings.General.VideoPlayer;
            if (string.IsNullOrEmpty(currentVideoPlayer))
            {
                Text = "Please install video player";
                sb.Clear();
                sb.Append("Subtitle Edit needs a video player.");
                currentVideoPlayer = "DirectShow";
            }

            var isLibMpvInstalled = LibMpvDynamic.IsInstalled;
            if (currentVideoPlayer == "MPV" && !isLibMpvInstalled)
            {
                currentVideoPlayer = "DirectShow";
            }

            if (currentVideoPlayer == "VLC" && !LibVlcDynamic.IsInstalled)
            {
                currentVideoPlayer = "DirectShow";
            }

            if (Configuration.IsRunningOnLinux)
            {
                sb.AppendLine();
                sb.AppendLine("Try installing latest version of libmpv and libvlc!");
                sb.Append("Read more about Subtitle Edit on Linux here: https://nikse.dk/SubtitleEdit/Help#linux");
            }
            else if (currentVideoPlayer == "MPV" && Configuration.IsRunningOnWindows)
            {
                sb.AppendLine();
                sb.AppendLine("You can re-download mpv or change video player via Options -> Settings -> Video Player");
                labelMpvInfo.Visible = false;
                buttonMpvSettings.Visible = false;
            }

            labelError.Text = sb.ToString();

            if (!Configuration.IsRunningOnWindows || currentVideoPlayer == "MPV")
            {
                buttonMpvSettings.Visible = false;
                labelMpvInfo.Visible = false;
            }
            else if (currentVideoPlayer != "MPV")
            {
                labelMpvInfo.Text = "To use the recommended video player \"mpv\" click on the button below.";
                if (isLibMpvInstalled)
                {
                    buttonMpvSettings.Text = "Use \"mpv\" as video player";
                }
            }
        }

        private void VideoError_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#settings_video");
            }
        }

        private void buttonMpvSettings_Click(object sender, EventArgs e)
        {
            if (LibMpvDynamic.IsInstalled)
            {
                Configuration.Settings.General.VideoPlayer = "MPV";
                DialogResult = DialogResult.OK;
                return;
            }

            using (var form = new SettingsMpv(true))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    VideoPlayerInstalled = true;
                    Configuration.Settings.General.VideoPlayer = "MPV";
                    DialogResult = DialogResult.OK;
                }
            }
        }
    }
}

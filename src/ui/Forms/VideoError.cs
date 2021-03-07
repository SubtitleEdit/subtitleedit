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
        public VideoError()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonCancel);
        }

        public void Initialize(string fileName, Exception exception)
        {
            var sb = new StringBuilder();
            sb.AppendLine("There seems to be missing a codec (or file is not a valid video/audio file)!");
            sb.AppendLine();

            var currentVideoPlayer = Configuration.Settings.General.VideoPlayer;
            if (string.IsNullOrEmpty(currentVideoPlayer))
            {
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
                sb.AppendLine("Try installing latest version of libmpv and libvlc!");
                sb.Append("Read more about Subtitle Edit on Linux here: https://nikse.dk/SubtitleEdit/Help#linux");
            }
            else if (currentVideoPlayer == "DirectShow")
            {
                sb.AppendLine("Try installing/updating \"LAV Filters - DirectShow Media Splitter and Decoders\": https://github.com/Nevcairiel/LAVFilters/releases");
                sb.Append("Note that Subtitle Edit is a " + IntPtr.Size * 8 + "-bit program, and hence requires " + IntPtr.Size * 8 + "-bit codecs!");
                sb.AppendLine();
            }
            else if (currentVideoPlayer == "VLC")
            {
                sb.AppendLine("VLC media player was unable to play this file (perhaps it's not a valid video/audio file) - you can change video player via Options -> Settings -> Video Player");
                sb.AppendLine("Latest version of VLC is available here: http://www.videolan.org/vlc/  (get the " + IntPtr.Size * 8 + "-bit version!)");
                sb.AppendLine();
            }
            else if (currentVideoPlayer == "MPV" && Configuration.IsRunningOnWindows)
            {
                sb.AppendLine("You can re-download mpv or change video player via Options -> Settings -> Video Player");
                sb.AppendLine();
            }
            richTextBoxMessage.Text = sb.ToString();

            if (!Configuration.IsRunningOnWindows || currentVideoPlayer == "MPV")
            {
                buttonMpvSettings.Visible = false;
                labelMpvInfo.Visible = false;
            }
            else if (currentVideoPlayer != "MPV")
            {
                labelMpvInfo.Text = $"You could also switch video player from \"{currentVideoPlayer}\" to \"mpv\".";
                if (isLibMpvInstalled)
                {
                    buttonMpvSettings.Text = "Use \"mpv\" as video player";
                }
            }

            if (exception != null)
            {
                var source = string.Empty;
                if (!string.IsNullOrEmpty(exception.Source))
                {
                    source = "Source: " + exception.Source.Trim() + Environment.NewLine + Environment.NewLine;
                }

                textBoxError.Text = "Message: " + exception.Message.Trim() + Environment.NewLine +
                                    source +
                                    "Stack Trace: " + Environment.NewLine +
                                    exception.StackTrace.Trim();
            }
            Text += fileName;
        }

        private void VideoError_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void richTextBoxMessage_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(e.LinkText);
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
                    Configuration.Settings.General.VideoPlayer = "MPV";
                    DialogResult = DialogResult.OK;
                }
            }
        }
    }
}

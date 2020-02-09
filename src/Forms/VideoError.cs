using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Diagnostics;
using System.IO;
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

        public void Initialize(string fileName, VideoInfo videoInfo, Exception exception)
        {
            var sb = new StringBuilder();
            sb.AppendLine("There seems to be missing a codec (or file is not a valid video/audio file)!");
            sb.AppendLine();
            if (Configuration.Settings.General.VideoPlayer != "VLC")
            {
                sb.AppendLine("You need to install/update LAV Filters - DirectShow Media Splitter and Decoders: https://github.com/Nevcairiel/LAVFilters/releases");
                sb.AppendLine();
                sb.AppendLine("NOTE: Subtitle Edit can also use mpv, VLC or MPC-HC as built-in video player. See Options -> Settings -> Video Player");
                sb.AppendLine("http://www.videolan.org/vlc/");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("VLC media player was unable to play this file (perhaps it's not a valid video/audio file) - you can change video player via Options -> Settings -> Video Player");
                sb.AppendLine("Latest version of VLC is available here: http://www.videolan.org/vlc/");
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(videoInfo?.VideoCodec))
            {
                sb.AppendLine($"The file {Path.GetFileName(fileName)} is encoded with {videoInfo.VideoCodec.Replace('\0', ' ')}!");
                sb.AppendLine("");
            }

            sb.AppendLine("");

            sb.AppendLine("You can find a few useful tools below:");
            sb.AppendLine(" - http://mediaarea.net/MediaInfo");
            sb.AppendLine(" - http://www.free-codecs.com/download/Codec_Tweak_Tool.htm");

            sb.AppendLine("");
            sb.Append("Note that Subtitle Edit is a " + (IntPtr.Size * 8) + "-bit program, and hence requires " + (IntPtr.Size * 8) + "-bit codecs!");

            richTextBoxMessage.Text = sb.ToString();

            if (exception != null)
            {
                textBoxError.Text = "Message: " + exception.Message + Environment.NewLine +
                                    "Source: " + exception.Source + Environment.NewLine +
                                    Environment.NewLine +
                                    "Stack Trace: " + Environment.NewLine +
                                    exception.StackTrace;
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
            UiUtil.OpenURL(e.LinkText);
        }

    }
}

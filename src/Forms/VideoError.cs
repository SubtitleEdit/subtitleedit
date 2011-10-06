using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VideoError : Form
    {
        public VideoError()
        {
            InitializeComponent();
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, this.Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public static bool Is64BitOS
        {
            get { return false; }
            //get { return (Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null); }
        }

        public void Initialize(string fileName, VideoInfo videInfo, Exception exception)
        {
            bool matraskaSplitterSuggested = false;
            bool mp4SplitterSuggested = false;
            bool isWindowsXpOrVista = Environment.OSVersion.Version.Major < 7;

            var sb = new StringBuilder();
            sb.AppendLine("There seems to be missing a codec (or file is not a valid video file)!");
            sb.AppendLine();
            if (Configuration.Settings.General.VideoPlayer != "VLC")
            {
                sb.AppendLine("NOTE: Subtitle Edit can also use VLC media player as built-in video player. See Options -> Settings -> Video Player");
                sb.AppendLine("http://www.videolan.org/vlc/");
                sb.AppendLine();
            }


            if (!string.IsNullOrEmpty(fileName) && fileName.ToLower().EndsWith(".mkv"))
            {
                sb.AppendLine("This file uses the matroska container file format.");
                if (Is64BitOS)
                {
                    sb.AppendLine("You need a matroska splitter like 'Matroska Splitter' under 'DirectShow Filters (64-bit)': http://sourceforge.net/projects/guliverkli2/files/");
                    sb.AppendLine("(Unzip the .ax file to " + Environment.SystemDirectory + ", then register MatroskaSplitter from the commandline: regsvr32 MatroskaSplitter.ax)");
                }
                else
                {
                    sb.AppendLine("You need a matroska splitter like 'MatroskaSplitter': http://haali.cs.msu.ru/mkv/");
                }
                sb.AppendLine("");
                matraskaSplitterSuggested = true;
            }
            else if (!string.IsNullOrEmpty(fileName) && fileName.ToLower().EndsWith(".mp4") && isWindowsXpOrVista)
            {
                sb.AppendLine("This file uses the mp4 container file format.");
                if (Is64BitOS)
                {
                    sb.AppendLine("You need an mp4 splitter like 'MP4 Splitter' under 'DirectShow Filters (64-bit)': http://sourceforge.net/projects/guliverkli2/files/");
                    sb.AppendLine("(Unzip the .ax file to " + Environment.SystemDirectory + ", then register MP4Splitter from the commandline: regsvr32 MP4Splitter.ax)");
                }
                else
                {
                    sb.AppendLine("You need an mp4 splitter like 'MatroskaSplitter': http://haali.cs.msu.ru/mkv/");
                }
                sb.AppendLine("");
                mp4SplitterSuggested = true;
            }

            if (videInfo != null && !string.IsNullOrEmpty(videInfo.VideoCodec))
            {
                sb.AppendLine(string.Format("The file {0} is encoded with {1}!", Path.GetFileName(fileName), videInfo.VideoCodec.Replace('\0', ' ')));
                sb.AppendLine("");
            }

            if (matraskaSplitterSuggested || mp4SplitterSuggested)
                sb.AppendLine("You might also need to install:");
            else
                sb.AppendLine("You might need to install:");


            if (Is64BitOS)
            {
                sb.AppendLine(" - ffdshow (64-bit builds): http://ffdshow-tryout.sourceforge.net/");
                if (!matraskaSplitterSuggested)
                    sb.AppendLine(" - 'Matroska Splitter' under 'DirectShow Filters (64-bit)': http://sourceforge.net/projects/guliverkli2/files/");
                if (!mp4SplitterSuggested && isWindowsXpOrVista)
                    sb.AppendLine(" - 'MP4 Splitter' under 'DirectShow Filters (64-bit)': http://sourceforge.net/projects/guliverkli2/files/");
            }
            else
            {
                sb.AppendLine(" - ffdshow: http://ffdshow-tryout.sourceforge.net/");
                if (!matraskaSplitterSuggested && !mp4SplitterSuggested)
                    sb.AppendLine(" - MatroskaSplitter: http://haali.cs.msu.ru/mkv/");
            }

            sb.AppendLine("");

            sb.AppendLine("You can read more about codecs and media formats here:");
            sb.AppendLine(" - http://www.inmatrix.com/zplayer/formats/");
            sb.AppendLine(" - http://www.free-codecs.com/guides/What_Codecs_Should_I_Use.htm");
            sb.AppendLine("");

            sb.AppendLine("Other useful utilities:");
            sb.AppendLine(" - http://mediainfo.sourceforge.net/");
            sb.AppendLine(" - http://www.free-codecs.com/download/Codec_Tweak_Tool.htm");
            sb.AppendLine(" - http://www.free-codecs.com/download/DirectShow_Filter_Manager.htm");
            sb.AppendLine(" - http://www.headbands.com/gspot/");

            sb.AppendLine("");
            sb.Append("Note that Subtitle Edit is a 32-bit program, and hence requires 32-bit codecs!");


            richTextBoxMessage.Text = sb.ToString();

            if (exception != null)
            {
                textBoxError.Text = "Message: " + exception.Message + Environment.NewLine +
                                    "Source: " + exception.Source + Environment.NewLine +
                                    Environment.NewLine +
                                    "StackTrace: " + Environment.NewLine +
                                    exception.StackTrace;
            }
            Text += fileName;
        }

        private void VideoError_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void richTextBoxMessage_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

    }
}

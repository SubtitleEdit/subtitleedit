using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    internal static class UiUtil
    {
        public static VideoInfo GetVideoInfo(string fileName)
        {
            VideoInfo info = Utilities.TryReadVideoInfoViaAviHeader(fileName);
            if (info.Success)
                return info;

            info = Utilities.TryReadVideoInfoViaMatroskaHeader(fileName);
            if (info.Success)
                return info;

            info = TryReadVideoInfoViaDirectShow(fileName);
            if (info.Success)
                return info;

            info = Utilities.TryReadVideoInfoViaMp4(fileName);
            if (info.Success)
                return info;

            return new VideoInfo { VideoCodec = "Unknown" };
        }

        private static VideoInfo TryReadVideoInfoViaDirectShow(string fileName)
        {
            return QuartsPlayer.GetVideoInfo(fileName);
        }

        public static int GetSubtitleIndex(List<Paragraph> paragraphs, VideoPlayerContainer videoPlayerContainer)
        {
            if (videoPlayerContainer.VideoPlayer != null)
            {
                double positionInMilliseconds = (videoPlayerContainer.VideoPlayer.CurrentPosition * TimeCode.BaseUnit) + 5;
                for (int i = 0; i < paragraphs.Count; i++)
                {
                    var p = paragraphs[i];
                    if (p.StartTime.TotalMilliseconds <= positionInMilliseconds && p.EndTime.TotalMilliseconds > positionInMilliseconds)
                    {
                        bool isInfo = p == paragraphs[0] && (p.StartTime.TotalMilliseconds == 0 && p.Duration.TotalMilliseconds == 0 || p.StartTime.TotalMilliseconds == Pac.PacNullTime.TotalMilliseconds);
                        if (!isInfo)
                            return i;
                    }
                }
                if (!string.IsNullOrEmpty(videoPlayerContainer.SubtitleText))
                    videoPlayerContainer.SetSubtitleText(string.Empty, null);
            }
            return -1;
        }

        public static int ShowSubtitle(List<Paragraph> paragraphs, VideoPlayerContainer videoPlayerContainer)
        {
            if (videoPlayerContainer.VideoPlayer != null)
            {
                double positionInMilliseconds = (videoPlayerContainer.CurrentPosition * TimeCode.BaseUnit) + 5;
                for (int i = 0; i < paragraphs.Count; i++)
                {
                    var p = paragraphs[i];
                    if (p.StartTime.TotalMilliseconds <= positionInMilliseconds &&
                        p.EndTime.TotalMilliseconds > positionInMilliseconds)
                    {
                        string text = p.Text.Replace("|", Environment.NewLine);
                        bool isInfo = p == paragraphs[0] && (p.StartTime.TotalMilliseconds == 0 && p.Duration.TotalMilliseconds == 0 || p.StartTime.TotalMilliseconds == Pac.PacNullTime.TotalMilliseconds);
                        if (!isInfo)
                        {
                            if (videoPlayerContainer.LastParagraph != p)
                                videoPlayerContainer.SetSubtitleText(text, p);
                            else if (videoPlayerContainer.SubtitleText != text)
                                videoPlayerContainer.SetSubtitleText(text, p);
                            return i;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(videoPlayerContainer.SubtitleText))
                    videoPlayerContainer.SetSubtitleText(string.Empty, null);
            }
            return -1;
        }

        public static int ShowSubtitle(List<Paragraph> paragraphs, Subtitle original, VideoPlayerContainer videoPlayerContainer)
        {
            if (videoPlayerContainer.VideoPlayer != null)
            {
                double positionInMilliseconds = (videoPlayerContainer.VideoPlayer.CurrentPosition * TimeCode.BaseUnit) + 15;
                for (int i = 0; i < paragraphs.Count; i++)
                {
                    var p = paragraphs[i];
                    if (p.StartTime.TotalMilliseconds <= positionInMilliseconds &&
                        p.EndTime.TotalMilliseconds > positionInMilliseconds)
                    {
                        var op = Utilities.GetOriginalParagraph(0, p, original.Paragraphs);

                        string text = p.Text.Replace("|", Environment.NewLine);
                        if (op != null)
                            text = text + Environment.NewLine + Environment.NewLine + op.Text.Replace("|", Environment.NewLine);

                        bool isInfo = p == paragraphs[0] && p.StartTime.TotalMilliseconds == 0 && positionInMilliseconds > 3000;
                        if (!isInfo)
                        {
                            if (videoPlayerContainer.LastParagraph != p)
                                videoPlayerContainer.SetSubtitleText(text, p);
                            else if (videoPlayerContainer.SubtitleText != text)
                                videoPlayerContainer.SetSubtitleText(text, p);
                            return i;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(videoPlayerContainer.SubtitleText))
                videoPlayerContainer.SetSubtitleText(string.Empty, null);
            return -1;
        }

        public static bool IsQuartsDllInstalled
        {
            get
            {
                if (Utilities.IsRunningOnMono())
                    return false;

                string quartzFileName = Environment.GetFolderPath(Environment.SpecialFolder.System).TrimEnd('\\') + @"\quartz.dll";
                return File.Exists(quartzFileName);
            }
        }

        public static bool IsMPlayerAvailable
        {
            get
            {
                if (Configuration.IsRunningOnLinux() || Utilities.IsRunningOnMono())
                    return File.Exists(Path.Combine(Configuration.BaseDirectory, "mplayer"));

                return MPlayer.GetMPlayerFileName != null;
            }
        }

        public static bool IsMpcHcInstalled
        {
            get
            {
                if (Utilities.IsRunningOnMono())
                    return false;

                try
                {
                    return VideoPlayers.MpcHC.MpcHc.GetMpcHcFileName() != null;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
            }
        }

        public static VideoPlayer GetVideoPlayer()
        {
            GeneralSettings gs = Configuration.Settings.General;

            if (Configuration.IsRunningOnLinux())
                return new MPlayer();

            // Mono on OS X is 32 bit and thus requires 32 bit VLC. Place VLC in the same
            // folder as Subtitle Edit and add this to the app.config inside the
            // "configuration" element:
            // <dllmap dll="libvlc" target="VLC.app/Contents/MacOS/lib/libvlc.dylib" />
            if (Configuration.IsRunningOnMac())
                return new LibVlcMono();

            if (gs.VideoPlayer == "VLC" && LibVlcDynamic.IsInstalled)
                return new LibVlcDynamic();

            if (gs.VideoPlayer == "MPV" && LibMpvDynamic.IsInstalled)
                return new LibMpvDynamic();

            //if (gs.VideoPlayer == "WindowsMediaPlayer" && IsWmpAvailable)
            //    return new WmpPlayer();

            //            if (gs.VideoPlayer == "MPlayer" && MPlayer.IsInstalled)
            //                return new MPlayer();

            if (gs.VideoPlayer == "MPC-HC" && VideoPlayers.MpcHC.MpcHc.IsInstalled)
                return new VideoPlayers.MpcHC.MpcHc();

            if (IsQuartsDllInstalled)
                return new QuartsPlayer();
            //if (IsWmpAvailable)
            //    return new WmpPlayer();

            throw new NotSupportedException("You need DirectX, VLC media player 1.1.x, or MPlayer2 installed as well as Subtitle Edit dll files in order to use the video player!");
        }

        public static void InitializeVideoPlayerAndContainer(string fileName, VideoInfo videoInfo, VideoPlayerContainer videoPlayerContainer, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            try
            {
                videoPlayerContainer.VideoPlayer = GetVideoPlayer();
                videoPlayerContainer.VideoPlayer.Initialize(videoPlayerContainer.PanelPlayer, fileName, onVideoLoaded, onVideoEnded);
                videoPlayerContainer.ShowStopButton = Configuration.Settings.General.VideoPlayerShowStopButton;
                videoPlayerContainer.ShowFullscreenButton = false;
                videoPlayerContainer.ShowMuteButton = Configuration.Settings.General.VideoPlayerShowMuteButton;
                videoPlayerContainer.Volume = Configuration.Settings.General.VideoPlayerDefaultVolume;
                videoPlayerContainer.EnableMouseWheelStep();
                videoPlayerContainer.VideoWidth = videoInfo.Width;
                videoPlayerContainer.VideoHeight = videoInfo.Height;
                videoPlayerContainer.VideoPlayer.Resize(videoPlayerContainer.PanelPlayer.Width, videoPlayerContainer.PanelPlayer.Height);
            }
            catch (Exception exception)
            {
                videoPlayerContainer.VideoPlayer = null;
                var videoError = new VideoError();
                videoError.Initialize(fileName, videoInfo, exception);
                videoError.ShowDialog();
            }
        }

        public static void CheckAutoWrap(TextBox textBox, KeyEventArgs e, int numberOfNewLines)
        {
            // Do not autobreak lines more than 1 line.
            if (numberOfNewLines != 1 || !Configuration.Settings.General.AutoWrapLineWhileTyping)
                return;

            int length = HtmlUtil.RemoveHtmlTags(textBox.Text, true).Length;
            if (e.Modifiers == Keys.None && e.KeyCode != Keys.Enter && length > Configuration.Settings.General.SubtitleLineMaximumLength)
            {
                string newText;
                if (length > Configuration.Settings.General.SubtitleLineMaximumLength + 30)
                {
                    newText = Utilities.AutoBreakLine(textBox.Text);
                }
                else
                {
                    int lastSpace = textBox.Text.LastIndexOf(' ');
                    if (lastSpace > 0)
                        newText = textBox.Text.Remove(lastSpace, 1).Insert(lastSpace, Environment.NewLine);
                    else
                        newText = textBox.Text;
                }

                int autobreakIndex = newText.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                if (autobreakIndex > 0)
                {
                    int selectionStart = textBox.SelectionStart;
                    textBox.Text = newText;
                    if (selectionStart > autobreakIndex)
                        selectionStart += Environment.NewLine.Length - 1;
                    if (selectionStart >= 0)
                        textBox.SelectionStart = selectionStart;
                }
            }
        }

        private static readonly Dictionary<string, Keys> AllKeys = new Dictionary<string, Keys>();
        public static Keys GetKeys(string keysInString)
        {
            if (string.IsNullOrEmpty(keysInString))
                return Keys.None;

            if (AllKeys.Count == 0)
            {
                foreach (Keys val in Enum.GetValues(typeof(Keys)))
                {
                    string k = val.ToString().ToLower();
                    if (!AllKeys.ContainsKey(k))
                        AllKeys.Add(k, val);
                }
                if (!AllKeys.ContainsKey("pagedown"))
                    AllKeys.Add("pagedown", Keys.RButton | Keys.Space);
                if (!AllKeys.ContainsKey("home"))
                    AllKeys.Add("home", Keys.MButton | Keys.Space);
                if (!AllKeys.ContainsKey("capslock"))
                    AllKeys.Add("capslock", Keys.CapsLock);
            }

            string[] parts = keysInString.ToLower().Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            var resultKeys = Keys.None;
            foreach (string k in parts)
            {
                if (AllKeys.ContainsKey(k))
                    resultKeys = resultKeys | AllKeys[k];
            }
            return resultKeys;
        }

        public static void SetButtonHeight(Control control, int newHeight, int level)
        {
            if (level > 6)
                return;

            if (control.HasChildren)
            {
                foreach (Control subControl in control.Controls)
                {
                    if (subControl.HasChildren)
                        SetButtonHeight(subControl, newHeight, level + 1);
                    else if (subControl is Button)
                        subControl.Height = newHeight;
                }
            }
            else if (control is Button)
                control.Height = newHeight;
        }

        public static void InitializeSubtitleFont(Control control)
        {
            var gs = Configuration.Settings.General;

            if (string.IsNullOrEmpty(gs.SubtitleFontName))
                gs.SubtitleFontName = "Tahoma";

            try
            {
                if (gs.SubtitleFontBold)
                    control.Font = new Font(gs.SubtitleFontName, gs.SubtitleFontSize, FontStyle.Bold);
                else
                    control.Font = new Font(gs.SubtitleFontName, gs.SubtitleFontSize);

                control.BackColor = gs.SubtitleBackgroundColor;
                control.ForeColor = gs.SubtitleFontColor;
            }
            catch
            {
            }
        }

        public static void FixLargeFonts(Control mainCtrl, Control ctrl)
        {
            using (Graphics graphics = mainCtrl.CreateGraphics())
            {
                SizeF textSize = graphics.MeasureString(ctrl.Text, ctrl.Font);
                if (textSize.Height > ctrl.Height - 4)
                {
                    SetButtonHeight(mainCtrl, (int)Math.Round(textSize.Height + 7.5), 1);
                }
            }
        }

        public static void SetSaveDialogFilter(SaveFileDialog saveFileDialog, SubtitleFormat currentFormat)
        {
            var sb = new StringBuilder();
            int index = 0;
            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
            {
                sb.Append(format.Name + "|*" + format.Extension + "|");
                if (currentFormat.Name == format.Name)
                    saveFileDialog.FilterIndex = index + 1;
                index++;
            }
            saveFileDialog.Filter = sb.ToString().TrimEnd('|');
        }

        public static void GetLineLengths(Label label, string text)
        {
            label.ForeColor = Color.Black;
            var lines = HtmlUtil.RemoveHtmlTags(text, true).SplitToLines();

            const int max = 3;

            var sb = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (i > 0)
                {
                    sb.Append('/');
                }

                if (i > max)
                {
                    label.ForeColor = Color.Red;
                    sb.Append("...");
                    label.Text = sb.ToString();
                    return;
                }

                sb.Append(line.Length);
                if (line.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                    label.ForeColor = Color.Red;
            }
            label.Text = sb.ToString();
        }

    }
}

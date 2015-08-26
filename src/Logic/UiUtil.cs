using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System.IO;

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

        public static bool IsManagedDirectXInstalled
        {
            get
            {
                if (Utilities.IsRunningOnMono())
                    return false;

                try
                {
                    //Check if this folder exists: C:\WINDOWS\Microsoft.NET\DirectX for Managed Code
                    string folderName = Environment.SystemDirectory.TrimEnd('\\');
                    folderName = folderName.Substring(0, folderName.LastIndexOf('\\'));
                    folderName = folderName + @"\\Microsoft.NET\DirectX for Managed Code";
                    return Directory.Exists(folderName);
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
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

            if (Configuration.IsRunningOnLinux() || Configuration.IsRunningOnMac())
                return new MPlayer();

            //if (Utilities.IsRunningOnMac())
            //    return new LibVlcMono();

            if (gs.VideoPlayer == "VLC" && LibVlcDynamic.IsInstalled)
                return new LibVlcDynamic();

            //if (gs.VideoPlayer == "WindowsMediaPlayer" && IsWmpAvailable)
            //    return new WmpPlayer();
            //if (gs.VideoPlayer == "ManagedDirectX" && IsManagedDirectXInstalled)
            //    return new ManagedDirectXPlayer();

            if (gs.VideoPlayer == "MPlayer" && MPlayer.IsInstalled)
                return new MPlayer();

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


    }
}

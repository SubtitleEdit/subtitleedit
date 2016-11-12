using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public interface IVideoPlayer : IDisposable
    {
        event EventHandler OnVideoLoaded;
        event EventHandler OnVideoEnded;
        string PlayerName { get; }
        string VideoFileName { get; }
        int Volume { get; set; }
        double Duration { get; }
        double CurrentPosition { get; set; }

        /// <summary>
        /// 1.0 is normal playback speed, 0.5 is half speed, and 2.0 is twice speed.
        /// </summary>
        double PlayRate { get; set; }

        void Play();
        void Pause();
        void Stop();
        bool IsPaused { get; }
        bool IsPlaying { get; }
        void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded);
    }
}

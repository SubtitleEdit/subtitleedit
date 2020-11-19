using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public abstract class VideoPlayer
    {
        public abstract string PlayerName { get; }
        public string VideoFileName { get; protected set; }
        public abstract int Volume { get; set; }
        public abstract double Duration { get; }
        public abstract double CurrentPosition { get; set; }

        /// <summary>
        /// 1.0 is normal playback speed, 0.5 is half speed, and 2.0 is twice speed.
        /// </summary>
        public virtual double PlayRate { get; set; }

        public abstract void Play();
        public abstract void Pause();
        public abstract void Stop();
        public abstract bool IsPaused { get; }
        public abstract bool IsPlaying { get; }
        public abstract void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded);
        public abstract void DisposeVideoPlayer();
        public abstract event EventHandler OnVideoLoaded;
        public abstract event EventHandler OnVideoEnded;
        public virtual void Resize(int width, int height)
        {
        }
    }
}

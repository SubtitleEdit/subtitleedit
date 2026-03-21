//using Avalonia;
//using Avalonia.Layout;
//using LibVLCSharp.Avalonia;
//using LibVLCSharp.Shared;
//using System;
//using System.Threading.Tasks;

//namespace Nikse.SubtitleEdit.Controls.VideoPlayer;

//public class VideoPlayerInstanceVlc : IVideoPlayerInstance
//{
//    public string Name => "vlc";

//    private string _fileName = string.Empty;
//    public string FileName => _fileName;

//    public bool IsPlaying => MediaPlayerVlc?.IsPlaying ?? false;

//    public bool IsPaused => !IsPlaying;

//    public double Position
//    {
//        get
//        {
//            var ms = MediaPlayerVlc?.Time ?? +0;
//            if (ms < 0.0001)
//            {
//                return 0;
//            }

//            return ms / 1000.0;
//        }
//        set
//        {
//            if (MediaPlayerVlc == null || value < 0.0001)
//            {
//                return;
//            }

//            var positionMs = value * 1000.0;
//            MediaPlayerVlc!.Time = (long)(positionMs);
//        }
//    }

//    public double Duration
//    {
//        get => MediaPlayerVlc?.Length / 1000.0 ?? 0;
//    }

//    public int VolumeMaximum => 100;

//    public double Volume
//    {
//        get => MediaPlayerVlc?.Volume ?? 0;
//        set
//        {
//            if (MediaPlayerVlc == null)
//            {
//                return;
//            }

//            MediaPlayerVlc.Volume = (int)value;
//        }
//    }

//    public VideoView VideoViewVlc { get; internal set; }
//    public MediaPlayer? MediaPlayerVlc { get; set; }
//    public LibVLC LibVLC { get; internal set; }

//    public VideoPlayerInstanceVlc()
//    {
//        MediaPlayerVlc?.Dispose();
//        LibVLC?.Dispose();
//        LibVLC = new LibVLC();
//        MediaPlayerVlc = new MediaPlayer(LibVLC);
//        VideoViewVlc = new VideoView
//        {
//            Margin = new Thickness(0),
//            MediaPlayer = MediaPlayerVlc,
//            HorizontalAlignment = HorizontalAlignment.Stretch,
//            VerticalAlignment = VerticalAlignment.Stretch,
//        };
//    }

//    public void Close()
//    {
//        _fileName = string.Empty;

//        if (MediaPlayerVlc == null)
//        {
//            return;
//        }

//        MediaPlayerVlc.Media = null;
//    }

//    public Task Open(string fileName)
//    {
//        if (MediaPlayerVlc == null)
//        {
//            return Task.CompletedTask;
//        }

//        _fileName = fileName;
//        var media = new Media(LibVLC, new Uri(fileName));

//        // add option to not close media
//        media.AddOption(":play-and-pause");

//        MediaPlayerVlc.Play(media);
//        return Task.CompletedTask;
//    }

//    public void Pause()
//    {
//        MediaPlayerVlc?.Pause();
//    }

//    public void Play()
//    {
//        MediaPlayerVlc?.Play();

//    }

//    public void PlayOrPause()
//    {
//        if (MediaPlayerVlc == null)
//        {
//            return;
//        }

//        if (MediaPlayerVlc.IsPlaying)
//        {
//            MediaPlayerVlc?.Pause();
//        }
//        else
//        {
//            MediaPlayerVlc?.Play();
//        }
//    }


//    public void Stop()
//    {
//        if (MediaPlayerVlc == null)
//        {
//            return;
//        }

//        MediaPlayerVlc.Pause();
//        MediaPlayerVlc.Position = 0;
//    }
//}

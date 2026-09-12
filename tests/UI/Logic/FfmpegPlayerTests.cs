using Avalonia;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;
using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg.Audio;
using System.IO.Compression;

namespace UITests.Logic;

/// <summary>
/// The pure parts of the FFmpeg video player: frame queue/pool bookkeeping, letterboxing, the
/// silent (clock-only) audio sink, and the library extraction filter. The decode pipeline itself
/// needs the FFmpeg shared libraries and is exercised through the app.
/// </summary>
public class FfmpegPlayerTests
{
    [Fact]
    public void FitRect_WiderControl_LetterboxesHorizontally()
    {
        var rect = FfmpegSoftwareControl.FitRect(new Rect(0, 0, 1000, 500), 16 / 9.0);

        Assert.Equal(500, rect.Height, 3);
        Assert.Equal(500 * 16 / 9.0, rect.Width, 3);
        Assert.Equal((1000 - rect.Width) / 2, rect.X, 3);
        Assert.Equal(0, rect.Y, 3);
    }

    [Fact]
    public void FitRect_TallerControl_LetterboxesVertically()
    {
        var rect = FfmpegSoftwareControl.FitRect(new Rect(0, 0, 400, 1000), 4 / 3.0);

        Assert.Equal(400, rect.Width, 3);
        Assert.Equal(300, rect.Height, 3);
        Assert.Equal(350, rect.Y, 3);
    }

    [Fact]
    public void FitRect_UnknownAspect_FillsBounds()
    {
        var bounds = new Rect(0, 0, 640, 480);
        Assert.Equal(bounds, FfmpegSoftwareControl.FitRect(bounds, 0));
    }

    [Fact]
    public void VideoFrameQueue_ReturnsBuffersToPoolAndReusesThem()
    {
        var queue = new VideoFrameQueue(2);
        var serial = 1;

        var a = queue.Rent(16, 8, 1, ref serial);
        Assert.NotNull(a);
        queue.Push(a!);
        Assert.Equal(1, queue.Count);

        var popped = queue.Pop();
        Assert.Same(a, popped);
        queue.Return(popped);

        var b = queue.Rent(16, 8, 1, ref serial);
        Assert.Same(a, b); // pooled buffer, no new allocation

        queue.Close();
    }

    [Fact]
    public void VideoFrameQueue_Rent_GivesUpWhenSerialMoves()
    {
        var queue = new VideoFrameQueue(1);
        var serial = 1;
        queue.Push(queue.Rent(4, 4, 1, ref serial)!); // full

        var stale = 1;
        serial = 2; // a seek happened
        var frame = queue.Rent(4, 4, stale, ref serial);

        Assert.Null(frame);
        queue.Close();
    }

    [Fact]
    public void VideoFrameQueue_PeekSecond_SeesTheFrameBehindTheHead()
    {
        var queue = new VideoFrameQueue(3);
        var serial = 1;
        var first = queue.Rent(4, 4, 1, ref serial)!;
        var second = queue.Rent(4, 4, 1, ref serial)!;
        queue.Push(first);
        Assert.Null(queue.PeekSecond());
        queue.Push(second);

        Assert.Same(first, queue.Peek());
        Assert.Same(second, queue.PeekSecond());
        queue.Close();
    }

    [Fact]
    public void VideoFrameQueue_SizeChange_DropsOldPool()
    {
        var queue = new VideoFrameQueue(2);
        var serial = 1;
        var small = queue.Rent(4, 4, 1, ref serial)!;
        queue.Return(small);

        var big = queue.Rent(8, 8, 1, ref serial)!;
        Assert.NotSame(small, big);
        Assert.Equal(8, big.Width);

        queue.Return(small); // wrong size now - must be disposed, not pooled
        Assert.Equal(System.IntPtr.Zero, small.Data);
        queue.Close();
    }

    [Fact]
    public void SilentAudioSink_PlayedSecondsNeverExceedsWrittenAudio()
    {
        using var sink = new SilentAudioSink();
        sink.Open(48000, 2);
        sink.Resume();

        Assert.True(sink.Write(new byte[48000 * 4 / 10])); // 100 ms
        Thread.Sleep(250);

        Assert.InRange(sink.PlayedSeconds, 0.09, 0.101);
    }

    [Fact]
    public void SilentAudioSink_Reset_AbortsBlockedWrite()
    {
        using var sink = new SilentAudioSink();
        sink.Open(48000, 2);
        sink.Pause(); // clock stopped: nothing drains, so the second write must block
        Assert.True(sink.Write(new byte[48000 * 4 / 10])); // 100 ms fills the lead

        var result = true;
        var writer = new Thread(() => { result = sink.Write(new byte[48000 * 4]); });
        writer.Start();
        Thread.Sleep(100);
        Assert.True(writer.IsAlive);

        sink.Reset();
        Assert.True(writer.Join(2000));
        Assert.False(result);
    }

    [Fact]
    public void ExtractLibraries_TakesOnlyBinDlls_Flattened()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-ffmpeg-libs-" + Guid.NewGuid().ToString("N"));
        var zip = folder + ".zip";
        try
        {
            using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
            {
                AddEntry(archive, "ffmpeg-n9.0-latest-win64-lgpl-shared-9.0/bin/avcodec-62.dll");
                AddEntry(archive, "ffmpeg-n9.0-latest-win64-lgpl-shared-9.0/bin/ffmpeg.exe");
                AddEntry(archive, "ffmpeg-n9.0-latest-win64-lgpl-shared-9.0/lib/avcodec.lib");
                AddEntry(archive, "ffmpeg-n9.0-latest-win64-lgpl-shared-9.0/include/libavcodec/avcodec.h");
            }

            DownloadFfmpegLibsViewModel.ExtractLibraries(zip, folder, CancellationToken.None);

            var files = Directory.GetFiles(folder).Select(Path.GetFileName).ToArray();
            Assert.Single(files);
            Assert.Equal("avcodec-62.dll", files[0]);
        }
        finally
        {
            File.Delete(zip);
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
    }

    [Fact]
    public void ExtractLibraries_NoDlls_Throws()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-ffmpeg-libs-" + Guid.NewGuid().ToString("N"));
        var zip = folder + ".zip";
        try
        {
            using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
            {
                AddEntry(archive, "x/bin/ffmpeg.exe");
            }

            Assert.Throws<InvalidOperationException>(() => DownloadFfmpegLibsViewModel.ExtractLibraries(zip, folder, CancellationToken.None));
        }
        finally
        {
            File.Delete(zip);
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
    }

    [Fact]
    public void AvCodecFileName_CarriesTheBindingsMajorVersion()
    {
        Assert.Contains(FfmpegLibraries.AvCodecMajor.ToString(), FfmpegLibraries.AvCodecFileName);
    }

    private static void AddEntry(ZipArchive archive, string name)
    {
        using var stream = archive.CreateEntry(name).Open();
        stream.Write(new byte[] { 1, 2, 3 });
    }
}

using System;
using System.IO;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.RemuxVideo;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

public class RemuxVideoViewModelTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static RemuxVideoViewModel BuildViewModel()
    {
        return new RemuxVideoViewModel(
            new FileHelper(),
            new FolderHelper(),
            new WindowService(new NullServiceProvider()));
    }

    private static string WriteTempFile()
    {
        var fileName = Path.Combine(Path.GetTempPath(), $"remux-test-{Guid.NewGuid():N}.mkv");
        File.WriteAllBytes(fileName, new byte[] { 1, 2, 3 });
        return fileName;
    }

    /// <summary>
    /// ffmpeg splits "-metadata key=value" at the first "=" only and does not unescape the
    /// value, so a backslash-escaped "=" ended up in the track title verbatim.
    /// </summary>
    [Theory]
    [InlineData("Track 1=EN", "Track 1=EN")]
    [InlineData("a\"b", "a'b")]
    [InlineData("a\\b", "a_b")]
    [InlineData("Plain title", "Plain title")]
    [InlineData("", "")]
    public void EscapeFfmpegMetadata_KeepsEqualsAndNeutralisesQuotesAndBackslashes(string input, string expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.EscapeFfmpegMetadata(input));
    }

    [AvaloniaFact]
    public void OnClosing_WhileRemuxing_DeletesThePartialOutputWithoutThrowing()
    {
        var vm = BuildViewModel();
        var outputFileName = WriteTempFile();
        try
        {
            vm.OutputFileName = outputFileName;
            vm.IsRemuxing = true; // as if ffmpeg were running; no process object exists here

            vm.OnClosing();

            Assert.False(File.Exists(outputFileName));
        }
        finally
        {
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }
        }
    }

    [AvaloniaFact]
    public void OnClosing_WhenIdle_KeepsTheFinishedOutput()
    {
        var vm = BuildViewModel();
        var outputFileName = WriteTempFile();
        try
        {
            vm.OutputFileName = outputFileName;
            vm.IsRemuxing = false;

            vm.OnClosing();

            Assert.True(File.Exists(outputFileName));
        }
        finally
        {
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }
        }
    }

    [Theory]
    [InlineData(45, "00:45")]
    [InlineData(754, "12:34")]
    [InlineData(11645, "3:14:05")]
    public void FormatDuration_FormatsHoursAndMinutesCorrectly(int totalSeconds, string expected)
    {
        Assert.Equal(expected, RemuxFileItem.FormatDuration(TimeSpan.FromSeconds(totalSeconds)));
    }

    [AvaloniaFact]
    public void RemuxFileItem_SetDuration_UpdatesDetails()
    {
        var tempFile = WriteTempFile();
        try
        {
            var item = new RemuxFileItem(tempFile);
            item.SetDuration(TimeSpan.FromMinutes(5));
            Assert.Equal("05:00", item.DurationDisplay);
            Assert.StartsWith("05:00  -  ", item.Details);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Theory]
    [InlineData(10, 50, "00:10 elapsed · ~00:10 left")]
    [InlineData(9, 90, "00:09 elapsed · ~00:01 left")]
    [InlineData(60, 25, "01:00 elapsed · ~03:00 left")]
    [InlineData(3723, 50, "1:02:03 elapsed · ~1:02:03 left")]
    public void FormatProgressTime_WithValidPercent_IncludesElapsedAndRemaining(int elapsedSeconds, double percent, string expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.FormatProgressTime(TimeSpan.FromSeconds(elapsedSeconds), percent));
    }

    [Theory]
    [InlineData(10, 0, "00:10 elapsed")]
    [InlineData(10, 100, "00:10 elapsed")]
    [InlineData(0, 50, "00:00 elapsed")]
    public void FormatProgressTime_BoundaryPercent_ReturnsElapsedOnly(int elapsedSeconds, double percent, string expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.FormatProgressTime(TimeSpan.FromSeconds(elapsedSeconds), percent));
    }

    /// <summary>
    /// The dialog is modeless and remuxes any video, so closing it may only hand the output to
    /// the main window after "Done" on a finished remux of the video the main window still has
    /// loaded (or when it has none) - never over a video the user opened in the meantime.
    /// </summary>
    [AvaloniaTheory]
    [InlineData(true, true, "same", true)]
    [InlineData(true, true, "SAME-CASE", true)]
    [InlineData(true, true, "none", true)]
    [InlineData(true, true, "other", false)]
    [InlineData(false, true, "same", false)] // closed via Cancel / Escape / title-bar X
    [InlineData(true, false, "same", false)] // nothing was remuxed (or the inputs changed since)
    public void ShouldLoadOutputOnClose_OnlyTakesOverTheRemuxedVideo(bool donePressed, bool completed, string mainVideo, bool expected)
    {
        var vm = BuildViewModel();
        var outputFileName = WriteTempFile();
        try
        {
            var input = Path.Combine(Path.GetTempPath(), "remux-test-input-does-not-exist.mkv");
            vm.VideoFileName = input; // resets IsCompleted, so set it first
            vm.OutputFileName = outputFileName;
            vm.IsCompleted = completed;
            if (donePressed)
            {
                vm.DoneCommand.Execute(null);
            }

            var current = mainVideo switch
            {
                "same" => input,
                "SAME-CASE" => input.ToUpperInvariant(),
                "none" => null,
                _ => Path.Combine(Path.GetTempPath(), "another-video.mkv"),
            };

            Assert.Equal(expected, vm.ShouldLoadOutputOnClose(current));
        }
        finally
        {
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }
        }
    }

    [AvaloniaFact]
    public void ShouldLoadOutputOnClose_MissingOutputFile_ReturnsFalse()
    {
        var vm = BuildViewModel();
        var input = Path.Combine(Path.GetTempPath(), "remux-test-input-does-not-exist.mkv");
        vm.VideoFileName = input;
        vm.OutputFileName = Path.Combine(Path.GetTempPath(), $"remux-test-missing-{Guid.NewGuid():N}.mkv");
        vm.IsCompleted = true;
        vm.DoneCommand.Execute(null);

        Assert.False(vm.ShouldLoadOutputOnClose(input));
    }

    private static (RemuxVideoViewModel Vm, RemuxFileItem VideoAudio, RemuxFileItem Narration) BuildMixViewModel(bool mix)
    {
        var vm = BuildViewModel();
        var video = Path.Combine(Path.GetTempPath(), "remux-mix-video-does-not-exist.mp4");
        vm.VideoFileName = video;
        vm.OutputFileName = Path.Combine(Path.GetTempPath(), "remux-mix-out.mp4");
        vm.MixAudio = mix;
        var videoAudio = new RemuxFileItem(video);
        var narration = new RemuxFileItem(Path.Combine(Path.GetTempPath(), "remux-mix-narration-does-not-exist.mp3"));
        vm.AudioFiles.Add(videoAudio);
        vm.AudioFiles.Add(narration);
        return (vm, videoAudio, narration);
    }

    [AvaloniaFact]
    public void BuildFfmpegArguments_Mix_MixesEverySourceAtItsVolumeIntoOneTrack()
    {
        var (vm, videoAudio, narration) = BuildMixViewModel(true);
        videoAudio.VolumePercent = 15;
        narration.VolumePercent = 120;

        var args = vm.BuildFfmpegArguments([.. vm.AudioFiles], []);

        Assert.Contains("-filter_complex \"[0:a:0]volume=0.15[a0];[1:a:0]volume=1.20[a1];[a0][a1]amix=inputs=2:duration=longest:normalize=0[aout]\"", args);
        Assert.Contains("-map 0:v:0 -map \"[aout]\" ", args);
        Assert.DoesNotContain("-map 0:a:", args);
        Assert.Contains("-c:a aac", args);
        Assert.Contains("-metadata:s:a:0 title=", args);
        Assert.DoesNotContain("-metadata:s:a:1", args);
    }

    [AvaloniaFact]
    public void BuildFfmpegArguments_NoMix_KeepsOneCopiedTrackPerFile()
    {
        var (vm, videoAudio, _) = BuildMixViewModel(false);
        videoAudio.VolumePercent = 15; // ignored without mixing

        var args = vm.BuildFfmpegArguments([.. vm.AudioFiles], []);

        Assert.DoesNotContain("-filter_complex", args);
        Assert.Contains("-map 0:v:0 -map 0:a:0 -map 1:a:0 ", args);
        Assert.Contains("-c:a copy", args);
    }

    // An .avi/.ts/.webm input defaults to .mp4 output, and "-c:a copy" failed for audio .mp4 cannot
    // hold (PCM, Vorbis, ...) - such audio is re-encoded to AAC.
    [AvaloniaTheory]
    [InlineData(".mp4", "pcm_s16le, 48000 Hz, stereo, s16, 1536 kb/s", true)]
    [InlineData(".mp4", "vorbis, 44100 Hz, stereo, fltp", true)]
    [InlineData(".mp4", "opus, 48000 Hz, stereo, fltp", false)]
    [InlineData(".mp4", "aac (LC), 48000 Hz, stereo, fltp", false)]
    [InlineData(".mov", "pcm_s16le, 48000 Hz, stereo, s16, 1536 kb/s", false)]
    [InlineData(".mov", "opus, 48000 Hz, stereo, fltp", true)]
    public void BuildFfmpegArguments_AudioTheContainerCannotHold_IsReencodedToAac(string outputFormat, string details, bool reencode)
    {
        var (vm, videoAudio, narration) = BuildMixViewModel(false);
        vm.SelectedOutputFormat = outputFormat;
        videoAudio.SetTracks([new AudioTrackOption { Index = 0, Details = details }], null);
        narration.SetTracks([new AudioTrackOption { Index = 0, Details = "mp3 (mp3float), 44100 Hz, stereo, fltp, 128 kb/s" }], null);

        var args = vm.BuildFfmpegArguments([.. vm.AudioFiles], []);

        Assert.Equal(reencode, args.Contains("-c:a aac -b:a 192k"));
        Assert.Equal(!reencode, args.Contains("-c:a copy"));
    }

    [Theory]
    [InlineData("ac3, 48000 Hz, 5.1(side), fltp, 448 kb/s", ".mp4", true)]
    [InlineData("eac3, 48000 Hz, 5.1(side), fltp, 640 kb/s", ".mov", true)]
    [InlineData("flac, 48000 Hz, stereo, s16", ".mp4", true)]
    [InlineData("flac, 48000 Hz, stereo, s16", ".mov", false)]
    [InlineData("pcm_s24le, 48000 Hz, stereo, s32", ".mp4", false)]
    [InlineData("dts (DTS), 48000 Hz, 5.1(side), fltp, 1536 kb/s", ".mp4", true)]
    [InlineData("truehd, 48000 Hz, 7.1, s32 (24 bit)", ".mp4", false)]
    [InlineData(null, ".mp4", true)]
    public void CanCopyAudioToMovFamily_FollowsTheContainer(string? details, string outputExtension, bool expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.CanCopyAudioToMovFamily(details, outputExtension));
    }

    [Theory]
    [InlineData("vp8, yuv420p(progressive), 640x360, 30 fps", ".mp4", false)]
    [InlineData("vp8, yuv420p(progressive), 640x360, 30 fps", ".mov", false)]
    [InlineData("vp8, yuv420p(progressive), 640x360, 30 fps", ".mkv", true)]
    [InlineData("theora, yuv420p, 352x288, 25 fps", ".mp4", false)]
    [InlineData("theora, yuv420p, 352x288, 25 fps", ".mov", true)]
    [InlineData("vp9 (Profile 0), yuv420p(tv), 640x360, 30 fps", ".mp4", true)]
    [InlineData("h264 (High), yuv420p(progressive), 1920x1080, 23.98 fps", ".mp4", true)]
    [InlineData(null, ".mp4", true)]
    public void CanCopyVideoTo_FollowsTheContainer(string? details, string outputExtension, bool expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.CanCopyVideoTo(details, outputExtension));
    }

    // A .webm input defaulted to .mp4 output, and "-c:v copy" of its VP8 failed with
    // "Could not find tag for codec vp8" - the output switches to .mkv.
    [AvaloniaTheory]
    [InlineData("vp8, yuv420p(progressive), 640x360, 30 fps", ".mkv")]
    [InlineData("vp9 (Profile 0), yuv420p(tv), 640x360, 30 fps", ".mp4")]
    [InlineData(null, ".mp4")]
    public void UseOutputFormatForVideoCodec_SwitchesToMkvForVideoMp4CannotHold(string? details, string expected)
    {
        var vm = BuildViewModel();
        vm.SelectedOutputFormat = ".mp4";

        vm.UseOutputFormatForVideoCodec(details);

        Assert.Equal(expected, vm.SelectedOutputFormat);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void BuildFfmpegArguments_FastStart_FollowsCheckBox(bool fastStart)
    {
        var (vm, _, _) = BuildMixViewModel(true);
        var saved = Nikse.SubtitleEdit.Logic.Config.Se.Settings.Video.RemuxFastStart;
        try
        {
            vm.FastStart = fastStart;

            var args = vm.BuildFfmpegArguments([.. vm.AudioFiles], []);

            Assert.Equal(fastStart, args.Contains("-movflags +faststart"));
            Assert.Equal(fastStart, Nikse.SubtitleEdit.Logic.Config.Se.Settings.Video.RemuxFastStart);
        }
        finally
        {
            Nikse.SubtitleEdit.Logic.Config.Se.Settings.Video.RemuxFastStart = saved;
        }
    }

    [AvaloniaFact]
    public void MixAudio_TwoFilesStayInMp4_UncheckingSwitchesToMkv()
    {
        var (vm, videoAudio, _) = BuildMixViewModel(true);
        Assert.Equal(".mp4", vm.SelectedOutputFormat);
        Assert.True(vm.IsMixAudioVisible);
        Assert.True(videoAudio.ShowVolume);

        vm.MixAudio = false;

        Assert.Equal(".mkv", vm.SelectedOutputFormat);
        Assert.False(videoAudio.ShowVolume);
    }

    [AvaloniaFact]
    public void MixAudio_VolumeIsEnabledOnlyWithASelectedFile()
    {
        var (vm, videoAudio, _) = BuildMixViewModel(true);
        vm.SelectedAudioFile = null;
        Assert.False(vm.IsVolumeEnabled);

        vm.SelectedAudioFile = videoAudio;
        Assert.True(vm.IsVolumeEnabled);

        vm.AudioFiles.RemoveAt(1); // one file left - nothing to mix
        Assert.False(vm.IsMixAudioVisible);
        Assert.False(vm.IsVolumeEnabled);
    }

    [AvaloniaFact]
    public void Scc_SwitchesToMovAndCopiesTheCaptionsAsCea608()
    {
        var (vm, _, _) = BuildMixViewModel(false);
        var srt = new RemuxFileItem(Path.Combine(Path.GetTempPath(), "remux-srt-does-not-exist.srt"));
        var scc = new RemuxFileItem(Path.Combine(Path.GetTempPath(), "remux-scc-does-not-exist.scc"));
        vm.SubtitleFiles.Add(srt);
        vm.SubtitleFiles.Add(scc);

        Assert.Equal(".mov", vm.SelectedOutputFormat);

        var args = vm.BuildFfmpegArguments([.. vm.AudioFiles], [.. vm.SubtitleFiles]);

        Assert.Contains("-map 0:v:0 -map 0:a:0 -map 1:a:0 -map 2:s:0 -map 3:s:0 ", args);
        Assert.Contains("-c:s mov_text -c:s:1 copy", args);
        Assert.Contains("-c:a copy", args);
    }

    /// <summary>
    /// An .mpg gets every subtitle as A/53 closed captions in the video afterwards (#15405), so
    /// neither .scc (MOV) nor .ass (MKV) switches the container, and ffmpeg gets no subtitle
    /// streams - it writes an MPEG-2 program stream to the given (temporary) file.
    /// </summary>
    [AvaloniaFact]
    public void Mpg_KeepsSubtitlesOutOfFfmpegAndStaysMpg()
    {
        var (vm, _, _) = BuildMixViewModel(false);
        vm.SelectedOutputFormat = ".mpg";
        vm.SubtitleFiles.Add(new RemuxFileItem(Path.Combine(Path.GetTempPath(), "remux-scc-does-not-exist.en.scc")));
        vm.SubtitleFiles.Add(new RemuxFileItem(Path.Combine(Path.GetTempPath(), "remux-ass-does-not-exist.ass")));

        Assert.Equal(".mpg", vm.SelectedOutputFormat);

        var temp = Path.Combine(Path.GetTempPath(), "remux-mpg-temp.mpg");
        var args = vm.BuildFfmpegArguments([.. vm.AudioFiles], [.. vm.SubtitleFiles], temp);

        Assert.Contains("-map 0:v:0 -map 0:a:0 -map 1:a:0 -c:v copy -c:a ac3 -b:a 192k ", args);
        Assert.DoesNotContain("-map 2:", args);
        Assert.DoesNotContain("-c:s", args);
        Assert.DoesNotContain("-metadata:s:s", args);
        Assert.EndsWith($"-f vob \"{temp}\"", args);

        vm.ReencodeVideoToMpeg2 = true;
        Assert.Contains("-c:v mpeg2video -q:v 2 ", vm.BuildFfmpegArguments([.. vm.AudioFiles], [.. vm.SubtitleFiles], temp));
    }

    /// <summary>
    /// MacCaption (.mcc) can only be embedded in .mpg - with .scc too, .mpg wins over .mov.
    /// </summary>
    [AvaloniaFact]
    public void Mcc_SwitchesToMpg()
    {
        var (vm, _, _) = BuildMixViewModel(false);
        vm.SubtitleFiles.Add(new RemuxFileItem(Path.Combine(Path.GetTempPath(), "remux-scc-does-not-exist.scc")));
        Assert.Equal(".mov", vm.SelectedOutputFormat);

        vm.SubtitleFiles.Add(new RemuxFileItem(Path.Combine(Path.GetTempPath(), "remux-mcc-does-not-exist.mcc")));

        Assert.Equal(".mpg", vm.SelectedOutputFormat);
    }

    /// <summary>
    /// The first file's caption data, with the second file's field 1 as field 2 (CC3).
    /// </summary>
    [Fact]
    public void GetClosedCaptionBytes_SecondFileIsField2()
    {
        var first = Path.Combine(Path.GetTempPath(), "remux-cc-first-" + System.Guid.NewGuid() + ".scc");
        var second = Path.Combine(Path.GetTempPath(), "remux-cc-second-" + System.Guid.NewGuid() + ".scc");
        try
        {
            File.WriteAllText(first, "Scenarist_SCC V1.0\n\n00:00:01:00\t9420 9420\n");
            File.WriteAllText(second, "Scenarist_SCC V1.0\n\n00:00:02:00\t942c 942c 942f\n");

            var captions = RemuxVideoViewModel.GetClosedCaptionBytes([new RemuxFileItem(first), new RemuxFileItem(second)]);

            Assert.Equal(new long[] { 30, 31 }, captions.Field1.Select(p => p.Slot));
            Assert.Equal(new long[] { 60, 61, 62 }, captions.Field2.Select(p => p.Slot));
            Assert.False(captions.HasCea708);
        }
        finally
        {
            File.Delete(first);
            File.Delete(second);
        }
    }

    [Theory]
    [InlineData("ac3, 48000 Hz, stereo, fltp, 192 kb/s", true)]
    [InlineData("mp2, 48000 Hz, stereo, s16p, 224 kb/s", true)]
    [InlineData("mp3 (mp3float), 44100 Hz, stereo", true)]
    [InlineData("aac (LC) (mp4a / 0x6134706D), 48000 Hz, stereo", false)]
    [InlineData("opus, 48000 Hz, stereo", false)]
    [InlineData(null, false)]
    public void IsMpegProgramStreamAudio_OnlyMpegAudioAndAc3(string? details, bool expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.IsMpegProgramStreamAudio(details));
    }

    [Theory]
    [InlineData("mpeg2video (Main), yuv420p(tv, progressive), 720x480", "mpeg2video")]
    [InlineData("h264 (High) (avc1 / 0x31637661), yuv420p", "h264")]
    [InlineData("", "")]
    public void GetCodecName_IsTheFirstWord(string details, string expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.GetCodecName(details));
    }

    [AvaloniaFact]
    public void Subtitles_GetLanguageTagFromFileName()
    {
        var (vm, _, _) = BuildMixViewModel(false);
        vm.SubtitleFiles.Add(new RemuxFileItem(Path.Combine(Path.GetTempPath(), "remux-does-not-exist.en.scc")));
        vm.SubtitleFiles.Add(new RemuxFileItem(Path.Combine(Path.GetTempPath(), "remux-does-not-exist.srt")));
        vm.SubtitleFiles.Add(new RemuxFileItem(Path.Combine(Path.GetTempPath(), "remux-does-not-exist.de.srt")));

        var args = vm.BuildFfmpegArguments([.. vm.AudioFiles], [.. vm.SubtitleFiles]);

        Assert.Contains("-metadata:s:s:0 language=eng ", args);
        Assert.DoesNotContain("-metadata:s:s:1 language=", args);
        Assert.Contains("-metadata:s:s:2 language=ger ", args);
    }

    [Theory]
    [InlineData("movie.en.scc", "eng")]
    [InlineData("movie.spa.srt", "spa")]
    [InlineData("movie.fr.forced.srt", "fre")]
    [InlineData("movie_track3_[dut].srt", "dut")]
    [InlineData("movie.scc", null)]
    [InlineData("Dr.No.srt", null)]
    public void GetSubtitleLanguageFromFileName_ReturnsBibliographicCode(string fileName, string? expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.GetSubtitleLanguageFromFileName(fileName));
    }

    [AvaloniaFact]
    public void Mov_KeepsMultipleTracks_ButAssStillSwitchesToMkv()
    {
        var (vm, _, _) = BuildMixViewModel(false);
        vm.SelectedOutputFormat = ".mov";
        Assert.Equal(".mov", vm.SelectedOutputFormat);

        vm.SubtitleFiles.Add(new RemuxFileItem(Path.Combine(Path.GetTempPath(), "remux-ass-does-not-exist.ass")));

        Assert.Equal(".mkv", vm.SelectedOutputFormat);
    }

    [Theory]
    [InlineData(100, "1.00")]
    [InlineData(15, "0.15")]
    [InlineData(250, "2.00")]
    [InlineData(-5, "0.00")]
    public void FormatVolumeFactor_ClampsToZeroTo200Percent(int percent, string expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.FormatVolumeFactor(percent));
    }
}

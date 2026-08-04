using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
using Nikse.SubtitleEdit.Features.Shared.PickMp4Track;
using Nikse.SubtitleEdit.Features.Shared.PickTsTrack;
using Nikse.SubtitleEdit.Features.Shared.PickVobSubLanguage;
using SkiaSharp;

namespace UITests.Features.Shared;

/// <summary>
/// The track/language pickers only open when a file holds more than one track, so the initial
/// selection has to reach the view model on its own. The tracks grid is created with
/// SelectionMode.AlwaysSelected, which puts it on row 0 before the SelectedItem binding is wired
/// up - so re-assigning index 0 afterwards raised no SelectionChanged and the view model's
/// selection stayed null: the preview pane opened empty and OK picked nothing, until the user
/// clicked another row and back again.
/// </summary>
public class PickTrackWindowSelectionTests
{
    private static PickMatroskaTrackViewModel MakeMatroskaViewModel(int trackCount)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<PickMatroskaTrackViewModel>();

        var tracks = new List<MatroskaTrackInfo>();
        for (var i = 0; i < trackCount; i++)
        {
            tracks.Add(new MatroskaTrackInfo
            {
                TrackNumber = i + 1,
                IsSubtitle = true,
                CodecId = "S_TEXT/UTF8",
                Language = "eng",
                Name = $"Track {i + 1}",
            });
        }

        // No MatroskaFile: the preview build bails out early, which keeps the test off the disk
        // while still exercising the selection plumbing.
        vm.Initialize(null!, tracks, "test.mkv");
        return vm;
    }

    [AvaloniaFact]
    public void PickMatroskaTrackWindow_SelectsFirstTrackInViewModelOnOpen()
    {
        var vm = MakeMatroskaViewModel(2);
        var window = new PickMatroskaTrackWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Assert.Same(vm.Tracks[0], vm.SelectedTrack);
            Assert.Equal(0, vm.TracksGrid.SelectedIndex);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void PickMatroskaTrackWindow_OkUsesTheInitiallySelectedTrackWithoutAClick()
    {
        var vm = MakeMatroskaViewModel(2);
        var window = new PickMatroskaTrackWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.OkPressed);
            Assert.NotNull(vm.SelectedMatroskaTrack);
            Assert.Equal(1, vm.SelectedMatroskaTrack!.TrackNumber);
        }
        finally
        {
            window.Close();
        }
    }

    private static PickMp4TrackViewModel MakeMp4ViewModel(int trackCount)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<PickMp4TrackViewModel>();

        for (var i = 0; i < trackCount; i++)
        {
            // No Trak: the preview build bails out early, which keeps the test off the disk while
            // still exercising the selection plumbing.
            vm.Tracks.Add(new Mp4TrackInfoDisplay { HandlerType = "sbtl", Track = null });
        }

        return vm;
    }

    [AvaloniaFact]
    public void PickMp4TrackWindow_SelectsFirstTrackInViewModelOnOpen()
    {
        var vm = MakeMp4ViewModel(2);
        var window = new PickMp4TrackWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Assert.Same(vm.Tracks[0], vm.SelectedTrack);
            Assert.Equal(0, vm.TracksGrid.SelectedIndex);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void PickMp4TrackWindow_OkUsesTheInitiallySelectedTrackWithoutAClick()
    {
        var vm = MakeMp4ViewModel(2);
        var window = new PickMp4TrackWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.OkPressed);
            Assert.Same(vm.Tracks[0], vm.SelectedMatroskaTrack);
        }
        finally
        {
            window.Close();
        }
    }

    // Fragmented (DASH/CMAF) MP4s have no moov Trak objects; the picker is fed
    // Mp4FragmentedSubtitleTrack instead, and both the cue preview and OK must work
    // off the fragmented subtitle.
    [AvaloniaFact]
    public void PickMp4TrackWindow_FragmentedTracks_PreviewAndOkWork()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<PickMp4TrackViewModel>();

        var english = new Subtitle();
        english.Paragraphs.Add(new Paragraph("Hello", 1000, 2000));
        english.Paragraphs.Add(new Paragraph("World", 3000, 4000));
        var danish = new Subtitle();
        danish.Paragraphs.Add(new Paragraph("Hej", 1000, 2000));
        var fragmentedTracks = new List<Mp4FragmentedSubtitleTrack>
        {
            new() { TrackId = 1, Language = "eng", Codec = "wvtt", Subtitle = english },
            new() { TrackId = 2, Language = "dan", Codec = "stpp", Subtitle = danish },
        };

        vm.Initialize(fragmentedTracks, "test.mp4");

        var window = new PickMp4TrackWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(2, vm.Tracks.Count);
            Assert.Equal("wvtt", vm.Tracks[0].HandlerType);
            Assert.Equal("stpp", vm.Tracks[1].HandlerType);
            Assert.Same(vm.Tracks[0], vm.SelectedTrack);

            // preview is built from the fragmented cues (no Trak/stbl involved)
            Assert.Equal(2, vm.Rows.Count);
            Assert.Equal("Hello", vm.Rows[0].Text);

            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.OkPressed);
            Assert.Same(fragmentedTracks[0], vm.SelectedMatroskaTrack!.FragmentedTrack);
        }
        finally
        {
            window.Close();
        }
    }

    private static PickTsTrackViewModel MakeTsViewModel(int trackCount)
    {
        // A stream with no sync bytes parses to zero tracks, which is all Initialize needs here -
        // it is called for the window title, and the tracks are added the way it would add them.
        var tsParser = new TransportStreamParser();
        tsParser.Parse(new MemoryStream(new byte[4096]), null!);

        var vm = new PickTsTrackViewModel();
        vm.Initialize(tsParser, @"C:\video\movie.ts");

        for (var i = 0; i < trackCount; i++)
        {
            vm.Tracks.Add(new TsTrackInfoDisplay
            {
                TrackNumber = 4096 + i,
                Language = "eng",
                Codec = "DVB",
            });
        }

        return vm;
    }

    [AvaloniaFact]
    public void PickTsTrackWindow_SelectsFirstTrackInViewModelOnOpen()
    {
        var vm = MakeTsViewModel(2);
        var window = new PickTsTrackWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Assert.Same(vm.Tracks[0], vm.SelectedTrack);
            Assert.Equal(0, vm.TracksGrid.SelectedIndex);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void PickTsTrackWindow_HasATitle()
    {
        var vm = MakeTsViewModel(2);
        var window = new PickTsTrackWindow(vm);
        try
        {
            // The view model left WindowTitle empty, so the window opened with no title at all.
            Assert.Contains("movie.ts", window.Title);
        }
        finally
        {
            window.Close();
        }
    }

    private static PickVobSubLanguageViewModel MakeVobSubViewModel(int languageCount)
    {
        var streamIdDictionary = new Dictionary<int, List<VobSubMergedPack>>();
        var idxLanguages = new List<string>();
        for (var i = 0; i < languageCount; i++)
        {
            var streamId = 0x20 + i;

            // No packs: the preview stays empty, which keeps the test off image decoding.
            streamIdDictionary.Add(streamId, new List<VobSubMergedPack>());
            idxLanguages.Add($"English (0x{streamId:x})");
        }

        var vm = new PickVobSubLanguageViewModel();
        vm.Initialize(streamIdDictionary, new List<SKColor>(), idxLanguages, @"C:\video\movie.idx");
        return vm;
    }

    [AvaloniaFact]
    public void PickVobSubLanguageWindow_SelectsFirstLanguageInViewModelOnOpen()
    {
        var vm = MakeVobSubViewModel(2);
        var window = new PickVobSubLanguageWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Assert.Same(vm.Languages[0], vm.SelectedLanguage);
            Assert.Equal(0, vm.LanguagesGrid.SelectedIndex);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void PickVobSubLanguageWindow_OkUsesTheInitiallySelectedLanguageWithoutAClick()
    {
        var vm = MakeVobSubViewModel(2);
        var window = new PickVobSubLanguageWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            vm.OkCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.OkPressed);
            Assert.Equal(0x20, vm.SelectedStreamId);
        }
        finally
        {
            window.Close();
        }
    }
}

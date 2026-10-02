using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace UITests.Features.Files;

/// <summary>
/// A Blu-ray sup's cue times are snapped to the export frame rate's grid, so with a video open
/// the dialog uses the video's frame rate instead of the profile's (25 by default) - without
/// writing it into the profile unless the user picks it.
/// </summary>
public class ExportImageBasedVideoFrameRateTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static ExportImageBasedViewModel BuildViewModel(SeExportImagesProfile profile)
    {
        var vm = new ExportImageBasedViewModel(
            new FileHelper(),
            new FolderHelper(),
            new WindowService(new NullServiceProvider()));

        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new SubtitleLineViewModel
            {
                Number = 1,
                Text = "Hello world",
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(3),
            },
        };

        vm.Initialize(new ExportHandlerBluRaySup(), subtitles, null, null);
        vm.Profiles.Add(profile);
        vm.SelectedProfile = profile;
        vm.ProfileChanged(null, new SelectionChangedEventArgs(SelectingItemsControl.SelectionChangedEvent, Array.Empty<object>(), new object[] { profile }));
        return vm;
    }

    private static void SwitchAwayFrom(ExportImageBasedViewModel vm, SeExportImagesProfile profile)
    {
        vm.ProfileChanged(null, new SelectionChangedEventArgs(SelectingItemsControl.SelectionChangedEvent, new object[] { profile }, Array.Empty<object>()));
    }

    [AvaloniaFact]
    public void VideoFrameRate_IsSelected_ButNotSavedToProfile()
    {
        var profile = new SeExportImagesProfile { ProfileName = "test", FramesPerSecond = 25 };
        var vm = BuildViewModel(profile);
        Assert.Equal(25, vm.SelectedFrameRate);

        vm.UseVideoFrameRate(24000.0 / 1001);

        Assert.Equal(23.976, vm.SelectedFrameRate);
        SwitchAwayFrom(vm, profile);
        Assert.Equal(25, profile.FramesPerSecond);
    }

    [AvaloniaFact]
    public void FrameRateChangedByUser_IsSavedToProfile()
    {
        var profile = new SeExportImagesProfile { ProfileName = "test", FramesPerSecond = 25 };
        var vm = BuildViewModel(profile);

        vm.UseVideoFrameRate(24000.0 / 1001);
        vm.SelectedFrameRate = 29.97;

        SwitchAwayFrom(vm, profile);
        Assert.Equal(29.97, profile.FramesPerSecond);
    }

    [AvaloniaFact]
    public void VideoFrameRateNotInList_KeepsProfileFrameRate()
    {
        var profile = new SeExportImagesProfile { ProfileName = "test", FramesPerSecond = 25 };
        var vm = BuildViewModel(profile);

        vm.UseVideoFrameRate(12.5);

        Assert.Equal(25, vm.SelectedFrameRate);
    }
}

using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Reflection;
using System.Text.Json;

namespace UITests.Features.Main;

public class SubtitleEndPreviewTests
{
    [Theory]
    [InlineData(12, 15, 16, 500, 14.5, 15)]
    [InlineData(12, 15, 16, 750, 14.25, 15)]
    [InlineData(14.9, 15, 16, 500, 14.9, 15)]
    [InlineData(-1, 0.3, 16, 500, 0, 0.3)]
    [InlineData(12, 15, 14.8, 500, 14.5, 14.8)]
    [InlineData(12, 15, 16, 0, 14.5, 15)]
    [InlineData(0, 15, 16, int.MaxValue, 5, 15)]
    public void ClipsTheRequestedTailToTheSubtitleAndMedia(
        double start, double end, double mediaDuration, int ms, double expectedStart, double expectedEnd)
    {
        Assert.Equal((expectedStart, expectedEnd), MainViewModel.GetSubtitleEndPreviewRange(start, end, mediaDuration, ms));
    }

    [Theory]
    [InlineData(3, 3, 16)]
    [InlineData(4, 3, 16)]
    [InlineData(-3, -2, 16)]
    [InlineData(16, 17, 16)]
    [InlineData(12, 15, 14)]
    [InlineData(12, 15, double.NaN)]
    [InlineData(12, 15, 0)]
    public void IgnoresAnEmptyRange(double start, double end, double mediaDuration)
    {
        Assert.Null(MainViewModel.GetSubtitleEndPreviewRange(start, end, mediaDuration, 500));
    }

    [Fact]
    public void OlderSettingsDefaultTo500MillisecondsAndSaveACustomDuration()
    {
        var settings = JsonSerializer.Deserialize("""{"Waveform":{}}""", SeJsonContext.Default.Se)!;
        Assert.Equal(500, settings.Waveform.SubtitleEndPreviewMs);

        settings.Waveform.SubtitleEndPreviewMs = 750;
        var json = JsonSerializer.Serialize(settings, SeJsonContext.Default.Se);
        Assert.Equal(750, JsonSerializer.Deserialize(json, SeJsonContext.Default.Se)!.Waveform.SubtitleEndPreviewMs);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ShortcutWaitsForTheSeekAndStopsWithoutChangingTheSubtitle(bool smpte)
    {
        using var services = MakeServices();
        var (vm, player, control) = MakeViewModel(services, smpte);
        var originalFrameRate = Se.Settings.General.CurrentFrameRate;
        var originalDuration = Se.Settings.Waveform.SubtitleEndPreviewMs;
        var binding = new SeShortCut(nameof(MainViewModel.PlaySubtitleEndCommand), ["D"], ShortcutCategory.Waveform);
        try
        {
            Se.Settings.General.CurrentFrameRate = 25;
            Se.Settings.Waveform.SubtitleEndPreviewMs = 750;
            var selected = vm.SelectedSubtitle!;
            var clockFactor = smpte ? 1001.0 / 1000.0 : 1.0;

            var shortcut = Assert.Single(ShortcutsMain.GetAllShortcuts(vm),
                s => s.Name == nameof(MainViewModel.PlaySubtitleEndCommand));
            Assert.Equal(ShortcutCategory.Waveform, shortcut.Category);
            Assert.Empty(shortcut.Keys);
            Se.Settings.Shortcuts.Add(binding);
            var shortcutManager = new ShortcutManager();
            shortcutManager.RegisterShortcut(Assert.Single(ShortcutsMain.GetUsedShortcuts(vm),
                s => s.Name == shortcut.Name));
            var key = new KeyEventArgs { Key = Key.D, PhysicalKey = PhysicalKey.D };
            shortcutManager.OnKeyPressed(null, key);
            Assert.Null(shortcutManager.CheckShortcuts(key, ShortcutCategory.SubtitleGridAndTextBox.ToString()));
            var command = shortcutManager.CheckShortcuts(key, ShortcutCategory.Waveform.ToString());
            Assert.Same(vm.PlaySubtitleEndCommand, command);
            command!.Execute(null);
            Assert.Equal(14.25 * clockFactor, player.Position, 8);

            player.Position = 20; // stale position from before the seek
            vm.UpdateSubtitleEndPreview(control);
            Assert.True(player.IsPlaying);

            player.SeekLanded = true;
            player.Position = 14.9 * clockFactor;
            vm.UpdateSubtitleEndPreview(control);
            Assert.True(player.IsPlaying);
            player.Position = 15 * clockFactor;
            vm.UpdateSubtitleEndPreview(control);

            Assert.False(player.IsPlaying);
            Assert.Equal(14.96 * clockFactor, player.Position, 8);
            Assert.Same(selected, vm.SelectedSubtitle);
            Assert.Equal(TimeSpan.FromSeconds(12), selected.StartTime);
            Assert.Equal(TimeSpan.FromSeconds(15), selected.EndTime);
            Assert.Equal("Synthetic subtitle", selected.Text);
        }
        finally
        {
            Se.Settings.General.CurrentFrameRate = originalFrameRate;
            Se.Settings.Waveform.SubtitleEndPreviewMs = originalDuration;
            Se.Settings.Shortcuts.Remove(binding);
            vm.VideoPlayerControl = null;
        }
    }

    [AvaloniaFact]
    public void APositionChangeCancelsThePreviewBeforeNormalPlayback()
    {
        using var services = MakeServices();
        var (vm, player, control) = MakeViewModel(services, false);
        try
        {
            vm.PlaySubtitleEndCommand.Execute(null);
            Assert.True(player.IsPlaying);

            vm.OnVideoPlayerPositionSet(20);
            Assert.False(player.IsPlaying);

            player.Position = 20;
            player.SeekLanded = true;
            vm.TogglePlayPauseCommand.Execute(null);
            vm.UpdateSubtitleEndPreview(control);
            Assert.True(player.IsPlaying);
            Assert.Equal(20, player.Position);
        }
        finally
        {
            vm.VideoPlayerControl = null;
        }
    }

    private static ServiceProvider MakeServices()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        return services.BuildServiceProvider();
    }

    private static (MainViewModel Vm, TestPlayer Player, VideoPlayerControl Control) MakeViewModel(
        ServiceProvider services, bool smpte)
    {
        var vm = services.GetRequiredService<MainViewModel>();
        var player = new TestPlayer();
        var control = new VideoPlayerControl(player) { Duration = player.Duration, IsSmpteTimingEnabled = smpte };
        vm.VideoPlayerControl = control;
        typeof(MainViewModel).GetField("_videoFileName", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(vm, player.FileName);
        vm.SelectedSubtitle = new SubtitleLineViewModel
        {
            StartTime = TimeSpan.FromSeconds(12),
            EndTime = TimeSpan.FromSeconds(15),
            Text = "Synthetic subtitle",
        };
        return (vm, player, control);
    }

    private sealed class TestPlayer : IVideoPlayer
    {
        public string Name => "fake";
        public string FileName => "test.wav";
        public bool CanLoad() => true;
        public Task LoadFile(string fileName, double startPositionSeconds = 0) => Task.CompletedTask;
        public void CloseFile() { }
        public void Play() => IsPlaying = true;
        public void PlayOrPause() => IsPlaying = !IsPlaying;
        public void Pause() => IsPlaying = false;
        public void Stop() => IsPlaying = false;
        public AudioTrackInfo? ToggleAudioTrack() => null;
        public bool IsPlaying { get; private set; }
        public bool IsPaused => !IsPlaying;
        public double Position { get; set; }
        public double Duration => 30;
        public int VolumeMaximum => 100;
        public double Volume { get; set; }
        public double Speed { get; set; } = 1;
        public bool SeekLanded { get; set; }
        public bool SupportsPlaybackRestartEvents => true;
        public bool HasPlaybackRestartedSince(long stopwatchTimestamp) => SeekLanded;
    }
}

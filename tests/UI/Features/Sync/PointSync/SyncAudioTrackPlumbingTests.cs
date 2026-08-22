using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSync;
using Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;
using Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System.Reflection;

namespace UITests.Features.Sync.PointSync;

/// <summary>
/// Every sync window opens its own mpv instance, and a new instance starts on the file's default
/// audio track - so the track picked in the main window's Video > Audio tracks has to be carried in
/// and re-applied, or a dubbed track plays while the user syncs against the original.
///
/// Visual Sync learned this in #11952; Point sync, Point sync via other subtitle and the Set sync
/// point dialog they both open did not, which is issue #13995. These tests pin the plumbing: the
/// track has to survive every hop from the main window down to the window that owns the player.
/// Whether mpv then honours it is covered by the same await-then-set order Visual Sync uses.
/// </summary>
public class SyncAudioTrackPlumbingTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static SetSyncPointViewModel MakeSetSyncPointViewModel()
        => new(new WindowService(new NullServiceProvider()), new FileHelper(), new VideoPreviewSubtitle(new MpvReloader(), new VlcReloader()));

    private static List<SubtitleLineViewModel> OneLine()
        => new() { new() { Text = "one", StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2) } };

    private static int AudioTrackIdOf(object viewModel) =>
        (int)viewModel.GetType()
            .GetField("_audioTrackId", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(viewModel)!;

    private static ParameterInfo AudioTrackParameterOf(Type type)
    {
        var initialize = type.GetMethods()
            .Single(m => m.Name == "Initialize" && m.GetParameters().Any(p => p.Name == "audioTrackId"));
        return initialize.GetParameters().Single(p => p.Name == "audioTrackId");
    }

    [Theory]
    [InlineData(typeof(SetSyncPointViewModel))]
    [InlineData(typeof(PointSyncViewModel))]
    [InlineData(typeof(PointSyncViaOtherViewModel))]
    [InlineData(typeof(VisualSyncViewModel))]
    public void EverySyncWindow_TakesAnAudioTrack(Type viewModelType)
    {
        var parameter = AudioTrackParameterOf(viewModelType);

        Assert.Equal(typeof(int), parameter.ParameterType);
    }

    // Optional and defaulting to -1 ("no track picked"), so the existing callers that have no
    // track - and the tests that construct these view models directly - keep compiling and mean
    // "leave mpv on the file's default".
    [Theory]
    [InlineData(typeof(SetSyncPointViewModel))]
    [InlineData(typeof(PointSyncViewModel))]
    [InlineData(typeof(PointSyncViaOtherViewModel))]
    [InlineData(typeof(VisualSyncViewModel))]
    public void TheAudioTrackParameter_IsOptionalAndDefaultsToNoTrack(Type viewModelType)
    {
        var parameter = AudioTrackParameterOf(viewModelType);

        Assert.True(parameter.IsOptional);
        Assert.Equal(-1, parameter.DefaultValue);
    }

    // The dialog that actually owns the player: the id has to reach its field, not just its
    // signature.
    [AvaloniaFact]
    public void SetSyncPoint_KeepsTheAudioTrackItWasGiven()
    {
        var lines = OneLine();
        var vm = MakeSetSyncPointViewModel();

        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null,
            previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null, audioTrackId: 3);

        Assert.Equal(3, AudioTrackIdOf(vm));
    }

    [AvaloniaFact]
    public void SetSyncPoint_WithoutATrack_MeansTheFilesDefault()
    {
        var lines = OneLine();
        var vm = MakeSetSyncPointViewModel();

        vm.Initialize(lines, lines[0], videoFileName: null, subtitleFileName: null,
            previewContext: VideoPreviewSubtitleContext.Default, audioVisualizer: null);

        Assert.Equal(-1, AudioTrackIdOf(vm));
    }
}

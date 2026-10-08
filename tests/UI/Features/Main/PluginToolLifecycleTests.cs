using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Plugins;
using Nikse.SubtitleEdit.Logic.UndoRedo;

namespace UITests.Features.Main;

public sealed class PluginToolLifecycleTests : IDisposable
{
    private readonly SettingsScope _settings = new("General.UseFrameMode", "General.UseFrameNumbersPersisted", "General.CurrentFrameRate");
    private Window? _window;
    private MainViewModel? _vm;
    private IUndoRedoManager? _undo;
    private static readonly string Header = new Ebu.EbuGeneralSubtitleInformation().ToString();

    [AvaloniaFact]
    public async Task StructuredResponse_SurvivesChangeTickAndUndoRedo()
    {
        var vm = Create();
        var response = Response(newHeader: "850" + Header[3..]);
        response.Subtitle!.Paragraphs = new()
        {
            new() { StartMs = 36_000_000, EndMs = 36_000_200, Text = "", MarginV = "22" },
            new() { StartMs = 36_001_000, EndMs = 36_002_000, Text = "Corrected text", MarginV = "20" },
        };

        Assert.True(await Apply(vm, response));
        _undo!.CheckForChanges(null); // The actual path that previously snapped 200 back to 209.
        Assert.Equal(36_000_200, vm.Subtitles[0].EndTime.TotalMilliseconds);
        Assert.Equal(response.Subtitle.Header, vm.GetUpdateSubtitle().Header);
        Assert.Equal("Plugin correction", _undo.UndoList.Last().Description);
        Assert.Equal(2, _undo.UndoCount);

        vm.UndoCommand.Execute(null);
        Assert.Equal(36_000_209, vm.Subtitles[0].EndTime.TotalMilliseconds);
        Assert.Single(vm.Subtitles);
        Assert.Equal("Before", vm.Subtitles[0].Text);
        Assert.Equal(Header, vm.GetUpdateSubtitle().Header);

        vm.RedoCommand.Execute(null);
        _undo.CheckForChanges(null);
        Assert.Equal(36_000_200, vm.Subtitles[0].EndTime.TotalMilliseconds);
        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Equal("Corrected text", vm.Subtitles[1].Text);
        Assert.Equal(36_001_000, vm.Subtitles[1].StartTime.TotalMilliseconds);
        Assert.Equal(36_002_000, vm.Subtitles[1].EndTime.TotalMilliseconds);
        Assert.Equal("20", vm.GetUpdateSubtitle().Paragraphs[1].MarginV);
        Assert.Equal(response.Subtitle.Header, vm.GetUpdateSubtitle().Header);

        var request = (PluginRequest)typeof(MainViewModel).GetMethod("BuildPluginRequest", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object[] { Plugin(), new List<int>() })!;
        Assert.Equal(36_000_200, request.Subtitle.Paragraphs![0].EndMs);
        Assert.Equal(24000.0 / 1001.0, Configuration.Settings.General.CurrentFrameRate);
    }

    [AvaloniaFact]
    public async Task StructuredResponse_25FpsTimesSurviveChangeTickAt30Fps()
    {
        // ArteCheck returns 25 fps time codes (40 ms grid) for an STL30 file; the change tick
        // snapped every returned line to 30 fps frames, so the next check found them all again.
        var vm = Create();
        Se.Settings.General.CurrentFrameRate = 30;
        Configuration.Settings.General.CurrentFrameRate = 30;
        var response = Response();
        response.Subtitle!.Paragraphs = new()
        {
            new() { StartMs = 36_000_000, EndMs = 36_000_200, Text = "", MarginV = "22" },
            new() { StartMs = 36_000_520, EndMs = 36_002_080, Text = "One", MarginV = "22" },
            new() { StartMs = 36_002_280, EndMs = 36_004_040, Text = "Two", MarginV = "22" },
        };

        Assert.True(await Apply(vm, response));
        _undo!.CheckForChanges(null);

        var request = (PluginRequest)typeof(MainViewModel).GetMethod("BuildPluginRequest", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object[] { Plugin(), new List<int>() })!;
        Assert.Equal(
            response.Subtitle.Paragraphs.Select(p => (p.StartMs, p.EndMs)),
            request.Subtitle.Paragraphs!.Select(p => (p.StartMs, p.EndMs)));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StructuredResponse_Stl25Header_FollowsFrameRateUnlessVideo(bool videoLoaded)
    {
        // ArteCheck converts an STL30 file to 25 fps and sets the disk format code to STL25.01.
        var vm = Create();
        var stl30 = Header[..3] + "STL30.01" + Header[11..];
        var subtitle = (Subtitle)typeof(MainViewModel).GetField("_subtitle", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(vm)!;
        subtitle.Header = stl30;
        vm.SetSelectedFrameRate(30);
        Se.Settings.General.CurrentFrameRate = 30;
        Configuration.Settings.General.CurrentFrameRate = 30;
        if (videoLoaded)
        {
            typeof(MainViewModel).GetField("_mediaInfo", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(vm, System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(FfmpegMediaInfo2)));
        }

        Assert.True(await Apply(vm, Response(newHeader: Header)));

        var expected = videoLoaded ? 30 : 25;
        Assert.Equal(expected, Se.Settings.General.CurrentFrameRate);
        Assert.Equal(expected, Configuration.Settings.General.CurrentFrameRate);
        Assert.Equal(expected, double.Parse(vm.SelectedFrameRate!, System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(Header, vm.GetUpdateSubtitle().Header);
        typeof(MainViewModel).GetField("_mediaInfo", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(vm, null);
    }

    [AvaloniaFact]
    public async Task StructuredResponse_ThenManualEdit_StillSnaps()
    {
        var vm = Create();
        var response = Response();
        response.Subtitle!.Paragraphs![0].EndMs = 36_000_200;
        Assert.True(await Apply(vm, response));
        _undo!.CheckForChanges(null);
        Assert.Equal(36_000_200, vm.Subtitles[0].EndTime.TotalMilliseconds);

        vm.Subtitles[0].EndTime = TimeSpan.FromMilliseconds(36_000_201);
        _undo.CheckForChanges(null);
        Assert.Equal(36_000_209, vm.Subtitles[0].EndTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public async Task StructuredResponse_NoChange_DoesNotAddUndoOrReplaceRows()
    {
        var vm = Create();
        var row = vm.Subtitles[0];
        Assert.True(await Apply(vm, Response()));
        _undo!.CheckForChanges(null);
        Assert.Equal(1, _undo.UndoCount);
        Assert.Same(row, vm.Subtitles[0]);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StructuredResponse_TextOrHeaderOnly_IsUndoable(bool headerOnly)
    {
        var vm = Create();
        var response = Response(newHeader: headerOnly ? "850" + Header[3..] : null);
        if (!headerOnly) response.Subtitle!.Paragraphs![0].Text = "After";
        Assert.True(await Apply(vm, response));
        _undo!.CheckForChanges(null);
        Assert.Equal(2, _undo.UndoCount);
        vm.UndoCommand.Execute(null);
        Assert.Equal("Before", vm.Subtitles[0].Text);
        Assert.Equal(Header, vm.GetUpdateSubtitle().Header);
        vm.RedoCommand.Execute(null);
        Assert.Equal(headerOnly ? "Before" : "After", vm.Subtitles[0].Text);
        Assert.Equal(headerOnly ? response.Subtitle!.Header : Header, vm.GetUpdateSubtitle().Header);
    }

    [AvaloniaFact]
    public async Task StructuredResponse_RemovesParagraphs_UndoRestoresThem()
    {
        var vm = Create();
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Removed", 36_002_000, 36_003_000), null!));
        _undo!.Do(vm.MakeUndoRedoObject("Before removal"));
        Assert.True(await Apply(vm, Response()));
        _undo.CheckForChanges(null);
        Assert.Single(vm.Subtitles);
        vm.UndoCommand.Execute(null);
        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Equal("Removed", vm.Subtitles[1].Text);
        vm.RedoCommand.Execute(null);
        Assert.Single(vm.Subtitles);
    }

    [AvaloniaFact]
    public async Task StructuredResponse_MetadataOnly_IsUndoable()
    {
        var vm = Create();
        var response = Response();
        response.Subtitle!.Paragraphs![0].MarginV = "20";
        Assert.True(await Apply(vm, response));
        _undo!.CheckForChanges(null);
        Assert.Equal(2, _undo.UndoCount);
        Assert.Equal("20", vm.GetUpdateSubtitle().Paragraphs[0].MarginV);
        vm.UndoCommand.Execute(null);
        Assert.Equal("22", vm.GetUpdateSubtitle().Paragraphs[0].MarginV);
        vm.RedoCommand.Execute(null);
        Assert.Equal("20", vm.GetUpdateSubtitle().Paragraphs[0].MarginV);
    }

    private MainViewModel Create()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();
        _window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = _window;
        var view = new MainView();
        _window.Content = view;
        _window.Show();
        Dispatcher.UIThread.RunJobs();
        _vm = (MainViewModel)view.DataContext!;
        _vm.SelectedSubtitleFormat = _vm.SubtitleFormats.First(f => f is Ebu);
        _vm.Subtitles.Clear();
        _vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Before", 36_000_000, 36_000_209) { MarginV = "22" }, null!) { Number = 1 });
        var subtitle = (Subtitle)typeof(MainViewModel).GetField("_subtitle", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(_vm)!;
        subtitle.Header = Header;
        Se.Settings.General.UseFrameMode = true;
        Se.Settings.General.UseFrameNumbersPersisted = false;
        Se.Settings.General.CurrentFrameRate = 24000.0 / 1001.0;
        Configuration.Settings.General.CurrentFrameRate = 24000.0 / 1001.0;
        _undo = (IUndoRedoManager)typeof(MainViewModel).GetField("_undoRedoManager", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(_vm)!;
        _undo.Reset();
        _undo.SetupChangeDetection(_vm, TimeSpan.FromHours(1));
        _undo.Do(_vm.MakeUndoRedoObject("Before plugin"));
        _undo.StartChangeDetection();
        return _vm;
    }

    private static PluginResponse Response(string? newHeader = null) => new()
    {
        Status = PluginConstants.StatusOk,
        UndoDescription = "Plugin correction",
        Subtitle = new PluginSubtitle
        {
            Header = newHeader,
            Paragraphs = new() { new() { StartMs = 36_000_000, EndMs = 36_000_209, Text = "Before", MarginV = "22" } },
        },
    };

    private static InstalledPlugin Plugin() => new()
    {
        Manifest = new PluginManifest { Name = "Test plugin" }, FolderPath = "", ManifestPath = "",
    };

    private static Task<bool> Apply(MainViewModel vm, PluginResponse response) =>
        (Task<bool>)typeof(MainViewModel).GetMethod("ApplyPluginSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object[] { Plugin(), response })!;

    public void Dispose()
    {
        _undo?.Dispose();
        if (_window != null && _vm != null)
        {
            _window.Closing -= _vm.OnClosing;
            foreach (var owned in _window.OwnedWindows.ToArray()) owned.Close();
            _window.Close();
        }
        _settings.Dispose();
    }
}

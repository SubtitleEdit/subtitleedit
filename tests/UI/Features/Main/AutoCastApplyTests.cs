using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.AutoCast;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// "Find voices in video and clone" ends by putting the detected speakers into the subtitle.
/// These tests drive <see cref="MainViewModel.ApplyAutoCastToSubtitle"/> exactly the way
/// ShowVideoAutoCastFromVideo does - labels already moved into the actor field by
/// <see cref="SpeakerLabelParser.MoveLabelsToActors"/> - and check the three promises the flow
/// makes: the lines get their actors, the format becomes ASSA (the format that keeps them), and
/// the actor column becomes visible.
/// </summary>
public class AutoCastApplyTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;

    public AutoCastApplyTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SubtitleEdit.UITests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();

        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    private (Window Window, MainViewModel Vm) ShowEmptyMainWindow()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        _windows.Add(window);
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        return (window, vm);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private string WriteSrt(string name)
    {
        var fileName = Path.Combine(_tempDirectory, name);
        File.WriteAllText(fileName,
            "1" + Environment.NewLine +
            "00:00:01,000 --> 00:00:02,000" + Environment.NewLine +
            "Hello there." + Environment.NewLine +
            Environment.NewLine +
            "2" + Environment.NewLine +
            "00:00:03,000 --> 00:00:04,000" + Environment.NewLine +
            "Hi yourself." + Environment.NewLine);
        return fileName;
    }

    /// <summary>
    /// A diarized transcription the way the flow holds it right before applying: the engine's
    /// "(Speaker N)" labels have already been moved out of the text and into the actor field.
    /// </summary>
    private static Subtitle MakeDiarizedTranscription()
    {
        var transcription = new Subtitle();
        transcription.Paragraphs.Add(new Paragraph("(Speaker 1) Hello there.", 900, 2100));
        transcription.Paragraphs.Add(new Paragraph("(Speaker 2) Hi yourself.", 2900, 4100));
        SpeakerLabelParser.MoveLabelsToActors(transcription);
        return transcription;
    }

    private static readonly Dictionary<string, string> RenamedSpeakers = new()
    {
        { "Speaker 1", "Alice" },
        { "Speaker 2", "Bob" },
    };

    [AvaloniaFact]
    public async Task OpenSubtitleGainsActorsAssaFormatAndActorColumn()
    {
        using var _ = new SettingsScope("General.ShowColumnActor");
        Nikse.SubtitleEdit.Logic.Config.Se.Settings.General.ShowColumnActor = false;

        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(WriteSrt("autocast-open.srt"), skipLoadVideo: true);
        Settle(window);
        Assert.False(vm.SelectedSubtitleFormat is AdvancedSubStationAlpha);

        vm.ApplyAutoCastToSubtitle(MakeDiarizedTranscription(), RenamedSpeakers);
        Settle(window);

        Assert.IsType<AdvancedSubStationAlpha>(vm.SelectedSubtitleFormat);
        Assert.True(vm.ShowColumnActor);
        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Equal("Alice", vm.Subtitles[0].Actor);
        Assert.Equal("Bob", vm.Subtitles[1].Actor);
    }

    [AvaloniaFact]
    public void TranscriptionBecomesSubtitleWithActorsWhenNothingIsOpen()
    {
        using var _ = new SettingsScope("General.ShowColumnActor");
        Nikse.SubtitleEdit.Logic.Config.Se.Settings.General.ShowColumnActor = false;

        var (window, vm) = ShowEmptyMainWindow();

        vm.ApplyAutoCastToSubtitle(MakeDiarizedTranscription(), RenamedSpeakers);
        Settle(window);

        Assert.IsType<AdvancedSubStationAlpha>(vm.SelectedSubtitleFormat);
        Assert.True(vm.ShowColumnActor);
        Assert.Equal(2, vm.Subtitles.Count);
        Assert.Equal("Alice", vm.Subtitles[0].Actor);
        Assert.Equal("Bob", vm.Subtitles[1].Actor);
        Assert.Equal("Hello there.", vm.Subtitles[0].Text);
    }
}

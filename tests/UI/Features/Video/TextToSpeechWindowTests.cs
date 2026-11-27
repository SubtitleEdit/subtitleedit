using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// Construction smoke test for the text-to-speech window: its layout is built entirely in code,
/// so a bad grid index or binding only surfaces when the window is instantiated - which no other
/// test (and no plain app-start smoke run) does.
/// </summary>
public class TextToSpeechWindowTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    // WindowService only touches the provider when it creates a child window, which this
    // construction test never does.
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private TextToSpeechWindow BuildWindow()
    {
        var vm = new TextToSpeechViewModel(
            new TtsDownloadService(new HttpClient()),
            new WindowService(new NullServiceProvider()),
            new FileHelper(),
            new AceStepAudioCppDownloadService(new HttpClient()));
        var window = new TextToSpeechWindow(vm);
        _windows.Add(window);
        return window;
    }

    private static IEnumerable<Button> AllButtons(TextToSpeechWindow window)
    {
        return window.GetLogicalDescendants().OfType<Button>();
    }

    [AvaloniaFact]
    public void Window_Constructs()
    {
        var window = BuildWindow();

        Assert.NotNull(window.Content);
    }

    [AvaloniaFact]
    public void SpokenTextEditorBindsToTheSelectedCueWithoutChangingTheSubtitle()
    {
        var first = new SpokenTextWindow.Row(new Paragraph("御茶ノ水", 0, 3000), null);
        var second = new SpokenTextWindow.Row(new Paragraph("烏丸通り", 3000, 5000), "からすま通り");
        var window = new SpokenTextWindow(new[] { first, second });
        _windows.Add(window);
        window.Show();
        var list = Assert.Single(window.GetLogicalDescendants().OfType<ListBox>());
        var fields = window.GetLogicalDescendants().OfType<TextBox>().ToArray();
        var subtitle = Assert.Single(fields, field => field.IsReadOnly);
        var spoken = Assert.Single(fields, field => !field.IsReadOnly);

        Assert.Equal("御茶ノ水", subtitle.Text);
        spoken.SetCurrentValue(TextBox.TextProperty, "👂おちゃのみず");
        Assert.Equal("👂おちゃのみず", first.SpokenText);
        Assert.Equal("御茶ノ水", first.Paragraph.Text);

        list.SelectedIndex = 1;
        Assert.Equal("からすま通り", spoken.Text);
        Assert.Equal("烏丸通り", subtitle.Text);
    }

    [AvaloniaFact]
    public void GenerateButton_IsTheOnlyPrimaryAction()
    {
        var window = BuildWindow();

        var primaryButtons = AllButtons(window).Where(b => b.Classes.Contains("primary")).ToList();

        Assert.Single(primaryButtons);
    }
}

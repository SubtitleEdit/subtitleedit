using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// Issue #13653: switching the main window's format dropdown to Advanced Sub Station Alpha (or
/// SubStation Alpha) must adopt the user's default storage styles - the whole default category,
/// like SE 4 - instead of forcing the hard-coded Arial default. Driven through the real toolbar
/// ComboBox so the SelectionChanged conversion path is what is under test.
/// </summary>
public class FormatDropdownDefaultStyleTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();
    private readonly string _tempDirectory;
    private readonly List<SeAssaStyle> _savedAssaStyles;
    private readonly List<SeAssaStyle> _savedSsaStyles;

    public FormatDropdownDefaultStyleTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SubtitleEdit.UITests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _savedAssaStyles = Se.Settings.Assa.StoredStyles.ToList();
        _savedSsaStyles = Se.Settings.Ssa.StoredStyles.ToList();
        Se.Settings.Assa.StoredStyles.Clear();
        Se.Settings.Ssa.StoredStyles.Clear();
    }

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();

        Se.Settings.Assa.StoredStyles.Clear();
        Se.Settings.Assa.StoredStyles.AddRange(_savedAssaStyles);
        Se.Settings.Ssa.StoredStyles.Clear();
        Se.Settings.Ssa.StoredStyles.AddRange(_savedSsaStyles);

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
            "Line one" + Environment.NewLine +
            Environment.NewLine +
            "2" + Environment.NewLine +
            "00:00:03,000 --> 00:00:04,000" + Environment.NewLine +
            "Line two" + Environment.NewLine);
        return fileName;
    }

    private static SeAssaStyle MakeStoredStyle(string name, string fontName, bool isDefault)
    {
        return new SeAssaStyle
        {
            Name = name,
            FontName = fontName,
            FontSize = 42,
            IsDefault = isDefault,
            ColorPrimary = "#FFFFFF",
            ColorSecondary = "#FFFFFF",
            ColorOutline = "#000000",
            ColorShadow = "#000000",
            Alignment = "2",
        };
    }

    private static ComboBox FindFormatComboBox(Window window, MainViewModel vm)
    {
        var combo = window.GetVisualDescendants().OfType<ComboBox>()
            .FirstOrDefault(c => ReferenceEquals(c.ItemsSource, vm.SubtitleFormats));
        Assert.NotNull(combo);
        return combo!;
    }

    [AvaloniaFact]
    public async Task SwitchDropdownToAssa_AppliesWholeDefaultStorageCategory()
    {
        // Two styles in the default category, "Speech" flagged - both must reach the file,
        // with the flagged one on the lines.
        Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Sign", "Verdana", isDefault: false));
        Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Speech", "Georgia", isDefault: true));

        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(WriteSrt("issue13653.srt"), skipLoadVideo: true);
        Settle(window);

        var combo = FindFormatComboBox(window, vm);
        combo.SelectedItem = vm.SubtitleFormats.First(f => f is AdvancedSubStationAlpha);
        Settle(window);

        var subtitle = vm.GetUpdateSubtitle();
        Assert.Contains("[V4+ Styles]", subtitle.Header);
        Assert.Equal(new[] { "Speech", "Sign" }, AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header));
        Assert.Contains("Georgia", subtitle.Header);
        Assert.Contains("Verdana", subtitle.Header);
        Assert.DoesNotContain("Arial", subtitle.Header);
        Assert.All(vm.Subtitles, s => Assert.Equal("Speech", s.Style));
    }

    [AvaloniaFact]
    public async Task SwitchDropdownToSsa_AppliesSsaDefaultStorageStyle()
    {
        // The SSA storage is separate from the ASSA one - and its default must be used for SSA.
        Se.Settings.Ssa.StoredStyles.Add(MakeStoredStyle("MySsaStyle", "Georgia", isDefault: true));
        Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("WrongStorage", "Impact", isDefault: true));

        var (window, vm) = ShowEmptyMainWindow();
        await vm.SubtitleOpen(WriteSrt("issue13653-ssa.srt"), skipLoadVideo: true);
        Settle(window);

        var combo = FindFormatComboBox(window, vm);
        combo.SelectedItem = vm.SubtitleFormats.First(f => f is SubStationAlpha);
        Settle(window);

        var subtitle = vm.GetUpdateSubtitle();
        Assert.Contains("[V4 Styles]", subtitle.Header);
        Assert.DoesNotContain("[V4+ Styles]", subtitle.Header);
        Assert.Contains("Georgia", subtitle.Header);
        Assert.DoesNotContain("Impact", subtitle.Header);
        Assert.All(vm.Subtitles, s => Assert.Equal("MySsaStyle", s.Style));
    }
}

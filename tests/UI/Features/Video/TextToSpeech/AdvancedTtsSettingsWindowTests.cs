using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.AdvancedTtsSettings;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.TextToSpeech;

/// <summary>
/// Construction smoke test for the advanced text-to-speech settings window: its layout is built
/// entirely in code, so a bad binding or a missing language string only surfaces when the window
/// is instantiated.
/// </summary>
public class AdvancedTtsSettingsWindowTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private AdvancedTtsSettingsWindow BuildWindow()
    {
        var window = new AdvancedTtsSettingsWindow(new AdvancedTtsSettingsViewModel(new FolderHelper()));
        _windows.Add(window);
        return window;
    }

    [AvaloniaFact]
    public void Window_Constructs()
    {
        var window = BuildWindow();

        Assert.NotNull(window.Content);
    }

    [AvaloniaFact]
    public void GenerationFolderSection_ShowsTheConfiguredFolderAndTheSweepCheckBox()
    {
        using var _ = new SettingsScope(
            "Video.TextToSpeech.GenerationFolder",
            "Video.TextToSpeech.DeleteTempFiles");
        Se.Settings.Video.TextToSpeech.GenerationFolder = Path.Combine(Path.GetTempPath(), "se-generation-folder-test");
        Se.Settings.Video.TextToSpeech.DeleteTempFiles = false;

        var window = BuildWindow();

        Assert.Contains(
            window.GetLogicalDescendants().OfType<TextBox>(),
            t => t.Text == Se.Settings.Video.TextToSpeech.GenerationFolder);

        var sweepCheckBox = Assert.Single(
            window.GetLogicalDescendants().OfType<CheckBox>(),
            c => Equals(c.Content, Se.Language.Video.TextToSpeech.DeleteTempFiles));
        Assert.False(sweepCheckBox.IsChecked);
    }
}

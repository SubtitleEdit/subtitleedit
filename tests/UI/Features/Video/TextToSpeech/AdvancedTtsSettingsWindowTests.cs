using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
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
            c => IsCheckBoxFor(c, Se.Language.Video.TextToSpeech.DeleteTempFiles));
        Assert.False(sweepCheckBox.IsChecked);
    }

    /// <summary>
    /// The option descriptions used to sit under every option as wrapped paragraphs, which made the
    /// window taller than a laptop screen. They belong in the hover hints now (#14331).
    /// </summary>
    [AvaloniaFact]
    public void OptionDescriptions_AreHintsNotInlineParagraphs()
    {
        using var _ = new SettingsScope("Appearance.ShowHints");
        Se.Settings.Appearance.ShowHints = true;
        var l = Se.Language.Video.TextToSpeech;

        var window = BuildWindow();

        Assert.DoesNotContain(
            window.GetLogicalDescendants().OfType<TextBlock>(),
            t => t.Text == l.VadSilenceCompressionDescription || t.Text == l.HighQualityTimeStretchDescription);

        var hints = window.GetLogicalDescendants()
            .OfType<Control>()
            .Select(c => ToolTip.GetTip(c) as string)
            .Where(t => t != null)
            .ToList();
        Assert.Contains(l.VadSilenceCompressionDescription, hints);
        Assert.Contains(l.HighQualityTimeStretchDescription, hints);
    }

    /// <summary>
    /// Hints are a user setting - with them off the icon must go too, rather than sit there doing
    /// nothing on hover.
    /// </summary>
    [AvaloniaFact]
    public void HintIcons_AreHidden_WhenHintsAreTurnedOff()
    {
        using var _ = new SettingsScope("Appearance.ShowHints");
        Se.Settings.Appearance.ShowHints = false;

        var window = BuildWindow();

        Assert.DoesNotContain(
            window.GetLogicalDescendants().OfType<Control>(),
            c => ToolTip.GetTip(c) is string);
    }

    /// <summary>
    /// The window is clamped to the working area on small screens; without the scroll viewer the
    /// OK/Cancel buttons ended up below the bottom edge and the dialog could not be used (#14331).
    /// </summary>
    [AvaloniaFact]
    public void OkAndCancel_StayInsideTheWindow_WhenItIsClampedToASmallScreen()
    {
        var window = BuildWindow();
        var vm = (AdvancedTtsSettingsViewModel)window.DataContext!;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        window.SizeToContent = SizeToContent.Manual;
        window.Height = 300;
        Dispatcher.UIThread.RunJobs();

        var buttonOk = Assert.Single(
            window.GetLogicalDescendants().OfType<Button>(),
            b => ReferenceEquals(b.Command, vm.OkCommand));
        var bottom = buttonOk.TranslatePoint(new Point(0, buttonOk.Bounds.Height), window);

        Assert.NotNull(bottom);
        Assert.True(bottom!.Value.Y <= window.Bounds.Height,
            $"OK button bottom {bottom.Value.Y} is below the window height {window.Bounds.Height}");

        var scrollViewer = Assert.Single(window.GetVisualDescendants().OfType<ScrollViewer>(),
            s => s.Extent.Height > s.Viewport.Height);
        Assert.True(scrollViewer.Offset.Y >= 0);
    }

    /// <summary>
    /// A value that only applies while its option is on should not look editable while it is off.
    /// </summary>
    [AvaloniaFact]
    public void DependentValue_IsDisabled_WhileItsOptionIsOff()
    {
        var window = BuildWindow();
        var vm = (AdvancedTtsSettingsViewModel)window.DataContext!;
        vm.DoAudioDucking = false;
        Dispatcher.UIThread.RunJobs();

        var volume = Assert.Single(FindNumericUpDownsAfter(window, Se.Language.Video.TextToSpeech.OriginalVolumePercent));
        Assert.False(volume.IsEffectivelyEnabled);

        vm.DoAudioDucking = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(volume.IsEffectivelyEnabled);
    }

    [AvaloniaFact]
    public void EdgeTtsSection_IsOnlyShownForTheEdgeTtsEngine()
    {
        var window = BuildWindow();
        var vm = (AdvancedTtsSettingsViewModel)window.DataContext!;

        var edgeRate = Assert.Single(
            window.GetLogicalDescendants().OfType<Label>(),
            c => Equals(c.Content, Se.Language.Video.TextToSpeech.EdgeTtsRate));

        vm.IsEdgeTtsEngine = false;
        Dispatcher.UIThread.RunJobs();
        Assert.False(edgeRate.IsVisible && IsAncestorChainVisible(edgeRate));

        vm.IsEdgeTtsEngine = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(IsAncestorChainVisible(edgeRate));
    }

    // Walks up to (but not into) the window - an unshown window is itself IsVisible == false.
    private static bool IsAncestorChainVisible(Control control)
    {
        for (StyledElement? c = control; c != null && c is not Window; c = c.Parent)
        {
            if (c is Control { IsVisible: false })
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<NumericUpDown> FindNumericUpDownsAfter(Window window, string label)
    {
        return window.GetLogicalDescendants()
            .OfType<StackPanel>()
            .Where(p => p.Children.OfType<Label>().Any(c => Equals(c.Content, label)))
            .SelectMany(p => p.Children.OfType<NumericUpDown>());
    }

    private static bool IsCheckBoxFor(CheckBox checkBox, string text)
    {
        return Equals(checkBox.Content, text) || (checkBox.Content is TextBlock tb && tb.Text == text);
    }
}

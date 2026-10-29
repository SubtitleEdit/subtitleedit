using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// Construction + layout tests for the burn-in window (built entirely in code): the style column
/// scrolls on its own and the preview gives up height first, so the progress bar and the button
/// bar always stay reachable - even on a short screen (issues #13904, #14360).
/// </summary>
public class BurnInWindowTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.Close();
        }

        _windows.Clear();
    }

    // WindowService only touches the provider when it creates a child window, which these
    // tests never do.
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private BurnInWindow BuildWindow()
    {
        var vm = new BurnInViewModel(
            new FolderHelper(),
            new FileHelper(),
            new WindowService(new NullServiceProvider()));
        var window = new BurnInWindow(vm);
        _windows.Add(window);
        return window;
    }

    private static Grid RootGrid(BurnInWindow window) => Assert.IsType<Grid>(window.Content);

    private static Control RootRow(BurnInWindow window, int row) =>
        RootGrid(window).Children.OfType<Control>().Single(c => Grid.GetRow(c) == row);

    private static double Bottom(Visual visual, Visual relativeTo) =>
        visual.TranslatePoint(new Point(0, visual.Bounds.Height), relativeTo)?.Y ?? double.MaxValue;

    [AvaloniaFact]
    public void Window_Constructs()
    {
        var window = BuildWindow();

        Assert.NotNull(window.Content);
    }

    [AvaloniaFact]
    public void ProgressAndButtonBar_StayReachable_OnAShortScreen()
    {
        var window = BuildWindow();
        var vm = Assert.IsType<BurnInViewModel>(window.DataContext);
        vm.IsGenerating = true; // the progress bar only shows while generating
        window.Show();

        // A maximized window on a 1366x768 laptop (taskbar + title bar) leaves about 700 DIPs of
        // client height; the window may not get more than that (#14360). Go below even that.
        window.Height = window.MinHeight;
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var progress = RootRow(window, 1);
        var buttons = RootRow(window, 2);
        Assert.True(progress.IsVisible);
        Assert.True(Bottom(buttons, window) <= window.ClientSize.Height + 1.5,
            $"Button bar bottom ({Bottom(buttons, window):0.#}) is clipped by the window ({window.ClientSize.Height:0.#}).");
        Assert.True(Bottom(progress, window) <= buttons.TranslatePoint(new Point(0, 0), window)!.Value.Y + 1.5,
            "The progress bar overlaps the button bar.");

        // The style column gives way by scrolling, not by being clipped
        var scroller = window.GetVisualDescendants().OfType<ScrollViewer>().First(s => s.Content is StackPanel);
        Assert.True(Bottom(scroller, window) <= progress.TranslatePoint(new Point(0, 0), window)!.Value.Y + 1.5,
            "The style column paints over the progress bar.");
    }

    [AvaloniaFact]
    public void ProgressBar_ShowsOnlyWhileGenerating()
    {
        var window = BuildWindow();
        var vm = Assert.IsType<BurnInViewModel>(window.DataContext);
        window.Show();

        Assert.False(RootRow(window, 1).IsVisible);
        vm.IsGenerating = true;
        Assert.True(RootRow(window, 1).IsVisible);
        vm.IsGenerating = false;
        Assert.False(RootRow(window, 1).IsVisible);
    }

    [AvaloniaFact]
    public void BatchMode_ShowsTheJobListInPlaceOfThePreview()
    {
        var window = BuildWindow();
        var vm = Assert.IsType<BurnInViewModel>(window.DataContext);
        window.Show();
        var preview = Assert.IsAssignableFrom<Control>(vm.VideoPlayerControl!.Parent);
        var jobList = Assert.IsAssignableFrom<Control>(vm.BatchGrid);

        Assert.True(preview.IsEffectivelyVisible);
        Assert.False(jobList.IsEffectivelyVisible);

        vm.BatchModeCommand.Execute(null);

        Assert.False(preview.IsEffectivelyVisible);
        Assert.True(jobList.IsEffectivelyVisible);
    }

    [AvaloniaFact]
    public void AlignmentGrid_SetsTheAlignment()
    {
        var window = BuildWindow();
        var vm = Assert.IsType<BurnInViewModel>(window.DataContext);
        window.Show();
        var anchors = window.GetLogicalDescendants().OfType<RadioButton>().Where(r => r.Classes.Contains("anchor")).ToList();
        Assert.Equal(9, anchors.Count);

        anchors[0].IsChecked = true; // top left
        Assert.Equal("7", vm.SelectedFontAlignment.Code);

        vm.SelectedFontAlignment = vm.FontAlignments.First(a => a.Code == "3");
        Assert.True(anchors[8].IsChecked);
        Assert.False(anchors[0].IsChecked);
    }

    // The extension list is only correct if both combo boxes are wired up: the encoder box
    // rebuilds the container list, and the container box rebuilds the audio encoder list.
    [AvaloniaFact]
    public void ContainerAndAudioLists_FollowTheSelectedEncoder()
    {
        var window = BuildWindow();
        var vm = window.DataContext as BurnInViewModel;
        Assert.NotNull(vm);
        window.Show();

        var encoderComboBox = window.GetLogicalDescendants().OfType<ComboBox>()
            .First(c => ReferenceEquals(c.ItemsSource, vm.VideoEncodings));
        var extensionComboBox = window.GetLogicalDescendants().OfType<ComboBox>()
            .First(c => ReferenceEquals(c.ItemsSource, vm.VideoExtensions));

        encoderComboBox.SelectedItem = vm.VideoEncodings.First(p => p.Codec == "libvpx-vp9");
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.Contains(".webm", vm.VideoExtensions);
        Assert.DoesNotContain(".ts", vm.VideoExtensions);

        extensionComboBox.SelectedItem = ".webm";
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // WebM holds Opus/Vorbis only, so "copy" (the default) cannot survive the switch.
        Assert.Equal(".webm", vm.SelectedVideoExtension);
        Assert.DoesNotContain("copy", vm.AudioEncodings);
        Assert.DoesNotContain("aac", vm.AudioEncodings);
        Assert.Contains(vm.SelectedAudioEncoding, vm.AudioEncodings);

        encoderComboBox.SelectedItem = vm.VideoEncodings.First(p => p.Codec == "libx264");
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        // H.264 cannot go into WebM, so the container - and with it the audio list - moves back.
        Assert.Contains(".ts", vm.VideoExtensions);
        Assert.DoesNotContain(".webm", vm.VideoExtensions);
        Assert.Contains(vm.SelectedVideoExtension, vm.VideoExtensions);
        Assert.Contains(vm.SelectedAudioEncoding, vm.AudioEncodings);
    }

    /// <summary>
    /// An image subtitle (a Blu-ray sup from the image-based editor) is burned in as it is, so the
    /// font/color/box/effect settings mean nothing and go away - the logo still applies.
    /// </summary>
    [AvaloniaFact]
    public void ImageSubtitle_HidesTheTextSettingsAndKeepsTheLogo()
    {
        var window = BuildWindow();
        var vm = Assert.IsType<BurnInViewModel>(window.DataContext);
        var textSettings = window.GetLogicalDescendants().OfType<Grid>().Single(p => p.Name == BurnInWindow.TextSettingsName);
        var logoButton = window.GetLogicalDescendants().OfType<Button>().Single(p => ReferenceEquals(p.Command, vm.ShowLogoCommand));
        Assert.True(textSettings.IsVisible);

        vm.InitializeImageSubtitle(string.Empty, "subs.sup");
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        Assert.False(textSettings.IsVisible);
        Assert.True(logoButton.IsVisible);
        Assert.True(vm.IsImageSubtitle);
    }
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;

namespace UITests.Features.Assa;

/// <summary>
/// Background behind the drawing: image files (and video frames) shown with or without the
/// libass preview, removable, with a remembered opacity.
/// </summary>
public class AssaDrawBackgroundTests
{
    private static string MakePng(int width, int height)
    {
        var fileName = Path.Combine(Path.GetTempPath(), $"assadraw-bg-{Guid.NewGuid():N}.png");
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.SteelBlue);
        }

        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(fileName, data.ToArray());
        return fileName;
    }

    private static (AssaDrawWindow Window, AssaDrawViewModel Vm) Open()
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, [], 1920, 1080);
        var window = new AssaDrawWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, vm);
    }

    [AvaloniaFact]
    public void ImageBackground_ShowsWithoutPreview_AndCanBeRemoved()
    {
        var png = MakePng(800, 600);
        var (window, vm) = Open();
        try
        {
            Assert.False(vm.ShowPreview);

            Assert.True(vm.LoadImageBackground(png));

            Assert.True(vm.HasBackground);
            Assert.NotNull(vm.Canvas!.BackgroundImage);
            Assert.Equal(800, vm.Canvas.BackgroundImage!.Size.Width);

            vm.RemoveBackgroundCommand.Execute(null);

            Assert.False(vm.HasBackground);
            Assert.Null(vm.Canvas.BackgroundImage);
        }
        finally
        {
            window.Close();
            File.Delete(png);
        }
    }

    [AvaloniaFact]
    public void NotAnImage_IsRejected()
    {
        var fileName = Path.Combine(Path.GetTempPath(), $"assadraw-bg-{Guid.NewGuid():N}.png");
        File.WriteAllText(fileName, "not an image");
        var (window, vm) = Open();
        try
        {
            Assert.False(vm.LoadImageBackground(fileName));
            Assert.False(vm.HasBackground);
        }
        finally
        {
            window.Close();
            File.Delete(fileName);
        }
    }

    [AvaloniaFact]
    public void OpacityAndStretch_AreRemembered_AndReachTheCanvas()
    {
        var oldOpacity = Se.Settings.Assa.DrawBackgroundOpacity;
        var oldStretch = Se.Settings.Assa.DrawBackgroundStretch;
        var (window, vm) = Open();
        try
        {
            vm.BackgroundOpacity = 0.4;
            vm.ToggleBackgroundStretchCommand.Execute(null);

            Assert.Equal(0.4, Se.Settings.Assa.DrawBackgroundOpacity);
            Assert.Equal(!oldStretch, Se.Settings.Assa.DrawBackgroundStretch);
            Assert.Equal(0.4, vm.Canvas!.BackgroundOpacity);
            Assert.Equal(!oldStretch, vm.Canvas.BackgroundStretch);
        }
        finally
        {
            window.Close();
            Se.Settings.Assa.DrawBackgroundOpacity = oldOpacity;
            Se.Settings.Assa.DrawBackgroundStretch = oldStretch;
        }
    }

    private sealed class PickFileHelper(string fileName) : StubFileHelper
    {
        public override Task<string> PickOpenFile(Visual sender, string title, string extensionTitle, string extension, string extensionTitle2 = "", string extension2 = "", string? suggestedStartFolder = null)
            => Task.FromResult(fileName);
    }

    /// <summary>
    /// The toolbar button used a MenuFlyout filled in its Opening event, which opened empty.
    /// </summary>
    [AvaloniaFact]
    public void BackgroundButton_OpensMenu_AndImageItemSetsBackground()
    {
        var png = MakePng(640, 360);
        var vm = new AssaDrawViewModel(new PickFileHelper(png), new StubWindowService());
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, [], 1920, 1080);
        var window = new AssaDrawWindow(vm) { Width = 1400, Height = 860 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var button = window.GetVisualDescendants().OfType<Button>().First(b => AutomationProperties.GetName(b) == Se.Language.Assa.DrawBackground);
            var p = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
            window.MouseMove(p);
            window.MouseDown(p, MouseButton.Left);
            window.MouseUp(p, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            // Third item: "Image file..." (rows are 28 px, the menu opens below the button)
            var item = new Point(p.X + 80, p.Y + button.Bounds.Height / 2 + 4 + 28 * 2 + 14);
            window.MouseMove(item);
            window.MouseDown(item, MouseButton.Left);
            window.MouseUp(item, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.HasBackground);
            Assert.Equal(640, vm.Canvas!.BackgroundImage!.Size.Width);
        }
        finally
        {
            window.Close();
            File.Delete(png);
        }
    }

    [Fact]
    public void WithoutVideo_VideoFrameCommandsAreDisabled()
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, [], 1920, 1080);

        Assert.False(vm.BackgroundFromVideoCommand.CanExecute(null));
        Assert.False(vm.BackgroundFromVideoAtCommand.CanExecute(null));
        Assert.True(vm.BackgroundFromImageCommand.CanExecute(null));
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace UITests.Features.Files;

/// <summary>
/// The "full frame image" option: a frame-sized png with the subtitle in its place, which SE4
/// offered for Final Cut Pro and Blu-ray sup. The other image formats place the subtitle from the
/// bitmap size themselves, so the option stays hidden for them.
/// </summary>
public class ExportImageBasedFullFrameTests : IDisposable
{
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

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private ExportImageBasedViewModel BuildViewModel(IExportHandler handler)
    {
        var vm = new ExportImageBasedViewModel(
            new FileHelper(),
            new FolderHelper(),
            new WindowService(new NullServiceProvider()));

        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new SubtitleLineViewModel
            {
                Number = 1,
                Text = "Hello world",
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(3),
            },
        };

        vm.Initialize(handler, subtitles, null, null);
        return vm;
    }

    private ExportImageBasedWindow BuildWindow(IExportHandler handler)
    {
        var window = new ExportImageBasedWindow(BuildViewModel(handler));
        _windows.Add(window);
        return window;
    }

    private static CheckBox FindFullFrameCheckBox(ExportImageBasedWindow window)
    {
        var checkBox = window.GetLogicalDescendants()
            .OfType<CheckBox>()
            .FirstOrDefault(c => Equals(c.Content, Se.Language.File.Export.FullFrameImage));
        Assert.NotNull(checkBox);
        return checkBox;
    }

    [AvaloniaFact]
    public void FullFrameOption_IsShownForFinalCutPro()
    {
        var window = BuildWindow(new ExportHandlerFcp());
        window.Show();

        var checkBox = FindFullFrameCheckBox(window);
        Assert.True(checkBox.IsEffectivelyVisible);
    }

    [AvaloniaFact]
    public void FullFrameOption_IsShownForBluRaySup()
    {
        var window = BuildWindow(new ExportHandlerBluRaySup());
        window.Show();

        var checkBox = FindFullFrameCheckBox(window);
        Assert.True(checkBox.IsEffectivelyVisible);
    }

    [AvaloniaFact]
    public void FullFrameOption_IsHiddenForTheOtherFormats()
    {
        var window = BuildWindow(new ExportHandlerVobSub());
        window.Show();

        var checkBox = FindFullFrameCheckBox(window);
        Assert.False(checkBox.IsEffectivelyVisible);
    }

    /// <summary>
    /// A profile is shared by every image export, so a checked box saved from a Final Cut Pro
    /// export must not turn frame-sized images on for a format that never offered the option.
    /// </summary>
    [AvaloniaFact]
    public void FullFrame_IsNotAppliedWhenTheOptionIsHidden()
    {
        var vm = BuildViewModel(new ExportHandlerVobSub());
        vm.IsFullFrame = true;

        Assert.False(vm.GetImageParameter(0).IsFullFrame);
    }

    [AvaloniaFact]
    public void FullFrame_IsAppliedWithItsOwnBackgroundColor()
    {
        var vm = BuildViewModel(new ExportHandlerFcp());
        vm.IsFullFrame = true;
        vm.FullFrameBackgroundColor = Colors.Blue;
        vm.BoxColor = Colors.Red;

        var parameter = vm.GetImageParameter(0);

        Assert.True(parameter.IsFullFrame);
        Assert.Equal(SkiaSharp.SKColors.Blue, parameter.FullFrameBackgroundColor);

        // The box behind the text keeps its own colour.
        Assert.Equal(SkiaSharp.SKColors.Red, parameter.BackgroundColor);
    }
}

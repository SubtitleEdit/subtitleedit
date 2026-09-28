using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace UITests.Features.Files.Compare;

/// <summary>
/// The file names in Compare's headers use the space the card has (#15384): a name is no longer
/// cut to 40 characters, a name too long for the card is trimmed instead of clipped, and the
/// "Current"/"Reference" caption stays right after the name.
/// </summary>
public class CompareHeaderFileNameTests : IDisposable
{
    private readonly SeCompare _savedSettings = Se.Settings.File.Compare;
    private Window? _window;

    public CompareHeaderFileNameTests()
    {
        Se.Settings.File.Compare = new SeCompare();
    }

    public void Dispose()
    {
        _window?.Close();
        Se.Settings.File.Compare = _savedSettings;
    }

    [AvaloniaFact]
    public void LongName_ThatFits_IsShownInFull()
    {
        const string name = "The.Long.Series.Name.S01E02.The.Episode.Title.1080p.WEB.en.srt";
        var (vm, window) = Show(name, width: 1600);

        Assert.Equal(name, vm.LeftFileNameDisplay);
        var label = FindName(window, name);
        Assert.False(IsTrimmed(label));
    }

    [AvaloniaFact]
    public void NameTooLongForTheCard_IsTrimmed_AndTheCaptionStaysInside()
    {
        var name = string.Concat(Enumerable.Repeat("Very.Long.File.Name.", 12)) + "en.srt";
        var (_, window) = Show(name, width: 900);

        var label = FindName(window, name);
        Assert.True(IsTrimmed(label));

        var caption = FindText(window, Se.Language.File.CompareCurrent);
        var card = caption.FindAncestorOfType<Border>()!;
        var captionRight = caption.TranslatePoint(new Point(caption.Bounds.Width, 0), card)!.Value.X;
        Assert.InRange(captionRight, 0, card.Bounds.Width);
    }

    [AvaloniaFact]
    public void ShortName_KeepsTheCaptionBesideIt()
    {
        var (_, window) = Show("a.srt", width: 1600);

        var label = FindName(window, "a.srt");
        var caption = FindText(window, Se.Language.File.CompareCurrent);
        var labelRight = label.TranslatePoint(new Point(label.Bounds.Width, 0), window)!.Value.X;
        var captionLeft = caption.TranslatePoint(new Point(0, 0), window)!.Value.X;
        Assert.InRange(captionLeft - labelRight, 0, 20);
    }

    private (CompareViewModel vm, Window window) Show(string leftName, double width)
    {
        var vm = new CompareViewModel(new FileHelper(), new FolderHelper());
        var lines = new ObservableCollection<SubtitleLineViewModel>
        {
            new(new Paragraph("One", 0, 1500), null!) { Number = 1 },
        };
        vm.Initialize(lines, "/subs/" + leftName, new ObservableCollection<SubtitleLineViewModel>(), string.Empty, false);

        var window = new CompareWindow(vm) { Width = width, Height = 600 };
        _window = window;
        window.Show();
        for (var pump = 0; pump < 12; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.Width = width; // restoring a saved window position must not change the width under test
            window.UpdateLayout();
        }

        return (vm, window);
    }

    private static TextBlock FindName(Window window, string name)
    {
        return window.GetVisualDescendants().OfType<TextBlock>().First(p => p.Text == name && p.TextTrimming != TextTrimming.None);
    }

    private static TextBlock FindText(Window window, string text)
    {
        return window.GetVisualDescendants().OfType<TextBlock>().First(p => p.Text == text && p.IsEffectivelyVisible);
    }

    private static bool IsTrimmed(TextBlock textBlock)
    {
        return textBlock.TextLayout.TextLines.Any(p => p.HasCollapsed);
    }
}

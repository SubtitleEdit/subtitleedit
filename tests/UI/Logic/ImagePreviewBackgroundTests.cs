using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace UITests.Logic;

/// <summary>
/// Issue #14328: the binary (Blu-ray sup) edit grid drew subtitle thumbnails on the mid-gray
/// checkerboard, whose light squares leave light subtitles unreadable. The OCR grid had already
/// rejected the checkerboard for the same reason (#12692) and used a flat near-black.
///
/// Both now share one configurable colour and one context-menu entry. "Shared" is the whole
/// point, so these pin it: one brush instance, one menu item, both windows.
/// </summary>
public class ImagePreviewBackgroundTests
{
    [Fact]
    public void DefaultColour_IsTheOcrWindowsOriginalBackdrop()
    {
        Assert.Equal(Color.FromRgb(0x2D, 0x2D, 0x30), ImagePreviewBackground.DefaultColor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a colour")]
    [InlineData("#12")]
    public void UnreadableSetting_FallsBackToTheDefault(string? hex)
    {
        Assert.Equal(ImagePreviewBackground.DefaultColor, ImagePreviewBackground.ParseOrDefault(hex));
    }

    [Theory]
    [InlineData("#FF102030", 0xFF, 0x10, 0x20, 0x30)]
    [InlineData("#102030", 0xFF, 0x10, 0x20, 0x30)]
    public void SavedHex_IsRead(string hex, byte a, byte r, byte g, byte b)
    {
        Assert.Equal(Color.FromArgb(a, r, g, b), ImagePreviewBackground.ParseOrDefault(hex));
    }

    // A brush is an AvaloniaObject with dispatcher affinity, so each window gets its own rather
    // than sharing one static instance across application lifetimes. Every cell in a window still
    // shares that window's brush, which is what makes a colour pick repaint them all at once.
    [AvaloniaFact]
    public void CreateBrush_ReturnsAFreshInstanceEachTime()
    {
        var first = ImagePreviewBackground.CreateBrush();
        var second = ImagePreviewBackground.CreateBrush();

        Assert.NotSame(first, second);
        Assert.Equal(first.Color, second.Color);
    }

    // "The same way in both windows" is a property of two separate files, so it can only drift.
    [Theory]
    [InlineData("src/ui/Features/Ocr/OcrWindow.cs")]
    [InlineData("src/ui/Features/Shared/BinaryEdit/BinaryEditWindow.cs")]
    public void BothWindows_UseTheSharedBrushAndOfferTheSharedMenuItem(string relativePath)
    {
        var source = File.ReadAllText(Path.Combine(FindRepoRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar)));

        Assert.Contains("ImagePreviewBackground.CreateBrush()", source, StringComparison.Ordinal);
        Assert.Contains("Background = previewBackground", source, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"ImagePreviewBackground\.MakeMenuItem\("), source);

        // The checkerboard is deliberately gone from the thumbnail grids; it stays only in the
        // large single-image previews, where the tiling does not fight the glyphs.
        Assert.DoesNotContain("GetCheckerboardBrush", source, StringComparison.Ordinal);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src", "ui")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Could not find repo root");
    }
}

using Avalonia;
using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Assa.FontCollector;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;

namespace UITests.Features.Assa;

/// <summary>
/// End-to-end check of the embedded-font matching: a real system font is UU-encoded
/// into an ASSA [Fonts] footer and must be matched to the style that uses it.
/// </summary>
public class FontCollectorEmbeddedMatchTests
{
    private sealed class FakeFolderHelper : IFolderHelper
    {
        public Task<string> PickFolderAsync(Window window, string title) => Task.FromResult(string.Empty);
        public Task OpenFolder(Window window, string folder) => Task.CompletedTask;
        public Task OpenFolderWithFileSelected(Window window, string selectedFile) => Task.CompletedTask;
    }

    private sealed class FakeFileHelper : IFileHelper
    {
        // Only the members the collector could touch matter; the rest throw if reached.
        public Task<string> PickOpenFile(Visual sender, string title, string extensionTitle, string extension, string extensionTitle2 = "", string extension2 = "", string? suggestedStartFolder = null) => Task.FromResult(string.Empty);
        public Task<string[]> PickOpenFiles(Visual sender, string title, string extensionTitle, List<string> extensions, string extensionTitle2, List<string> extensions2) => Task.FromResult(Array.Empty<string>());
        public Task<string> PickOpenSubtitleFile(Visual sender, string title, bool includeVideoFiles = true, string? lastOpenedFilePath = null) => throw new NotSupportedException();
        public Task<string[]> PickOpenSubtitleFiles(Visual sender, string title, bool includeVideoFiles = true, string? lastOpenedFilePath = null) => throw new NotSupportedException();
        public Task<string> PickSaveSubtitleFile(Visual sender, Nikse.SubtitleEdit.Core.SubtitleFormats.SubtitleFormat currentFormat, string suggestedFileName, string title) => throw new NotSupportedException();
        public Task<FileHelperSubtitleSavePickerResult?> PickSaveSubtitleFileAs(Visual sender, Nikse.SubtitleEdit.Core.SubtitleFormats.SubtitleFormat currentFormat, string suggestedFileName, string title) => throw new NotSupportedException();
        public Task<string> PickSaveSubtitleFile(Visual sender, string extension, string suggestedFileName, string title) => throw new NotSupportedException();
        public Task<string> PickSaveFile(Visual sender, string extension, string suggestedFileName, string title) => throw new NotSupportedException();
        public Task<string> PickSaveFile(Visual sender, string extension, string extensionTitle, string suggestedFileName, string title) => throw new NotSupportedException();
        public Task<string> PickSaveFile(Visual sender, IReadOnlyList<(string Name, string Extension)> fileTypes, string suggestedFileName, string title) => throw new NotSupportedException();
        public Task<string> PickOpenVideoFile(Visual sender, string title) => throw new NotSupportedException();
        public Task<string[]> PickOpenVideoFiles(Visual sender, string title) => throw new NotSupportedException();
        public Task<string> PickOpenImageFile(Visual sender, string title) => throw new NotSupportedException();
    }

    private static (string FamilyName, byte[] Bytes)? GetAnyRealFontFileBytes()
    {
        var candidates = new List<string>();
        if (OperatingSystem.IsMacOS())
        {
            candidates.Add("/System/Library/Fonts/Supplemental/Arial.ttf");
            candidates.Add("/System/Library/Fonts/Supplemental/Courier New.ttf");
        }
        else if (OperatingSystem.IsWindows())
        {
            var winFonts = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
            candidates.Add(Path.Combine(winFonts, "arial.ttf"));
        }
        else
        {
            candidates.Add("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf");
            candidates.Add("/usr/share/fonts/TTF/DejaVuSans.ttf");
        }

        foreach (var path in candidates.Where(File.Exists))
        {
            var bytes = File.ReadAllBytes(path);
            using var typeface = SKTypeface.FromData(SKData.CreateCopy(bytes));
            if (typeface != null)
            {
                return (typeface.FamilyName, bytes);
            }
        }

        return null;
    }

    [Fact]
    public void EmbeddedFontRoundTrip_DecodesToLoadableTypeface()
    {
        var font = GetAnyRealFontFileBytes();
        if (font == null)
        {
            return;
        }

        var (familyName, bytes) = font.Value;
        var footer = "[Fonts]\r\nfontname: embedded_0.ttf\r\n" + UUEncoding.UUEncode(bytes) + "\r\n";

        var embedded = Nikse.SubtitleEdit.Logic.AssaFontEmbedder.GetEmbeddedFonts(footer);

        Assert.Single(embedded);
        Assert.True(embedded[0].Bytes.Length >= bytes.Length, $"decoded {embedded[0].Bytes.Length} < original {bytes.Length}");
        Assert.Equal(bytes, embedded[0].Bytes.Take(bytes.Length));

        using var typeface = SKTypeface.FromData(SKData.CreateCopy(embedded[0].Bytes));
        Assert.NotNull(typeface);
        Assert.Equal(familyName, typeface.FamilyName);
    }

    [Fact]
    public void MatchEmbeddedFonts_MarksStyleFontCarriedAsAttachment()
    {
        var font = GetAnyRealFontFileBytes();
        if (font == null)
        {
            return; // no known system font on this machine - nothing to test against
        }

        var (familyName, bytes) = font.Value;

        var header =
            "[Script Info]\r\nScriptType: v4.00+\r\n\r\n[V4+ Styles]\r\n" +
            "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\r\n" +
            $"Style: Default,{familyName},20,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1\r\n";
        var subtitle = new Subtitle { Header = header };
        subtitle.Paragraphs.Add(new Paragraph("Hello", 0, 1000) { Extra = "Default" });
        subtitle.Footer = "[Fonts]\r\nfontname: embedded_0.ttf\r\n" + UUEncoding.UUEncode(bytes) + "\r\n";

        var vm = new FontCollectorViewModel(new FakeFolderHelper(), new FakeFileHelper());
        vm.CollectFontNames(subtitle);
        vm.MatchEmbeddedFonts(subtitle.Footer);

        var item = Assert.Single(vm.FontItems, i => i.FontName.Equals(familyName, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(item.EmbeddedFontBytes);
        Assert.Equal("embedded_0.ttf", item.EmbeddedFileName);
    }
}

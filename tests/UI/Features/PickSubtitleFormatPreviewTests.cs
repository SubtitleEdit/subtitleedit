using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;

namespace UITests.Features;

/// <summary>
/// The format preview renders in a <see cref="SyntaxHighlightingTextBox"/> (it used to be an
/// AvaloniaEdit editor); formats without syntax rules fall back to a plain text box.
/// </summary>
public class PickSubtitleFormatPreviewTests
{
    private static TextBox ShowPreview(SubtitleFormat format)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello, World!", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("This is a sample subtitle.", 3500, 6000));

        var vm = new PickSubtitleFormatViewModel();
        vm.Initialize(format, subtitle);

        return Assert.IsAssignableFrom<TextBox>(vm.PreviewContainer.Child);
    }

    [AvaloniaFact]
    public void KnownFormatIsPreviewedWithSyntaxHighlighting()
    {
        var textBox = ShowPreview(new SubRip());

        var syntaxTextBox = Assert.IsType<SyntaxHighlightingTextBox>(textBox);
        Assert.IsType<Nikse.SubtitleEdit.Logic.SubRipSourceSyntaxHighlighting>(syntaxTextBox.SourceHighlighter);
        Assert.True(syntaxTextBox.IsReadOnly);
        Assert.Contains("00:00:01,000 --> 00:00:03,000", syntaxTextBox.Text);
    }

    [AvaloniaFact]
    public void FormatWithoutSyntaxRulesUsesAPlainTextBox()
    {
        var textBox = ShowPreview(new UnknownSubtitle1());

        Assert.IsNotType<SyntaxHighlightingTextBox>(textBox);
        Assert.True(textBox.IsReadOnly);
        Assert.False(string.IsNullOrEmpty(textBox.Text));
    }
}

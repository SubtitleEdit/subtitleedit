using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features;

/// <summary>
/// The format preview renders in the virtualizing source editor, with line numbers and the syntax
/// rules of the selected format.
/// </summary>
public class PickSubtitleFormatPreviewTests
{
    private static SyntaxTextEditor ShowPreview(SubtitleFormat format)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello, World!", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("This is a sample subtitle.", 3500, 6000));

        var vm = new PickSubtitleFormatViewModel();
        vm.Initialize(format, subtitle);

        return Assert.IsType<SyntaxTextEditor>(vm.PreviewContainer.Child);
    }

    [AvaloniaFact]
    public void KnownFormatIsPreviewedWithSyntaxHighlighting()
    {
        var editor = ShowPreview(new SubRip());

        Assert.IsType<SubRipSourceSyntaxHighlighting>(editor.SourceHighlighter);
        Assert.True(editor.IsReadOnly);
        Assert.True(editor.ShowLineNumbers);
        Assert.Contains("00:00:01,000 --> 00:00:03,000", editor.Text);
    }

    [AvaloniaFact]
    public void FormatWithoutSyntaxRulesIsShownWithoutColoring()
    {
        var editor = ShowPreview(new UnknownSubtitle1());

        Assert.Null(editor.SourceHighlighter);
        Assert.True(editor.IsReadOnly);
        Assert.False(string.IsNullOrEmpty(editor.Text));
    }
}

using Avalonia.Media;
using AvaloniaEdit.Rendering;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Stores text alignment preference for subtitle text editor.
/// The actual alignment is applied through padding in the TextEditorWrapper.
/// </summary>
public class SubtitleTextAlignmentTransformer : IVisualLineTransformer
{
    private TextAlignment _alignment = TextAlignment.Left;

    public TextAlignment Alignment
    {
        get => _alignment;
        set => _alignment = value;
    }

    public void Transform(ITextRunConstructionContext context, System.Collections.Generic.IList<VisualLineElement> elements)
    {
        // Alignment is handled by padding in the TextEditorWrapper
    }
}






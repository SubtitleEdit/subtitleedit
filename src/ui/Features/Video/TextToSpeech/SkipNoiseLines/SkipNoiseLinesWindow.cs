using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SkipNoiseLines;

public class SkipNoiseLinesWindow : Window
{
    public SkipNoiseLinesWindow(SkipNoiseLinesViewModel vm)
    {
        // Select all / invert selection are in the list header, added by the builder
        var rowsView = SelectLinesWindowBuilder.MakeRowsView(vm,
            Se.Language.Video.TextToSpeech.SkipNoiseLinesColumnSkip);

        SelectLinesWindowBuilder.Initialize(this, vm,
            Se.Language.Video.TextToSpeech.SkipNoiseLinesTitle, width: 800, null, rowsView);
    }
}

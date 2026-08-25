using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SkipNoiseLines;

public class SkipNoiseLinesWindow : Window
{
    public SkipNoiseLinesWindow(SkipNoiseLinesViewModel vm)
    {
        var panelSelection = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.SelectAll, vm.SelectAllCommand),
            UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.InverseSelectionCommand));

        var rowsView = SelectLinesWindowBuilder.MakeRowsView(vm,
            Se.Language.Video.TextToSpeech.SkipNoiseLinesColumnSkip);

        SelectLinesWindowBuilder.Initialize(this, vm,
            Se.Language.Video.TextToSpeech.SkipNoiseLinesTitle, width: 800, panelSelection, rowsView);
    }
}

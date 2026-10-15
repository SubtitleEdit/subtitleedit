using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DetectSpeakers;

public class DetectSpeakersWindow : Window
{
    public DetectSpeakersWindow(DetectSpeakersViewModel vm)
    {
        // Sticky speakers sits with the selection actions in the list header
        var switchSticky = TtsMainEditorLook.SwitchRow(nameof(vm.StickySpeakers), Se.Language.Video.TextToSpeech.DetectSpeakersSticky);
        switchSticky.DataContext = vm;
        switchSticky.Margin = new Avalonia.Thickness(0, 0, 4, 0);

        var rowsView = SelectLinesWindowBuilder.MakeRowsView(vm,
            Se.Language.Video.TextToSpeech.DetectSpeakersColumnUse,
            new SeTableViewColumn
            {
                Header = Se.Language.General.Actor,
                Binding = new Binding(nameof(DetectSpeakersRow.Speaker)),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Width = new GridLength(160),
            });

        SelectLinesWindowBuilder.Initialize(this, vm,
            Se.Language.Video.TextToSpeech.DetectSpeakersTitle, width: 900, switchSticky, rowsView);
    }
}

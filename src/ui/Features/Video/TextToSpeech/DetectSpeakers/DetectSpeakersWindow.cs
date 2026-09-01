using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DetectSpeakers;

public class DetectSpeakersWindow : Window
{
    public DetectSpeakersWindow(DetectSpeakersViewModel vm)
    {
        var panelOptions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
        };
        panelOptions.Children.Add(UiUtil.MakeCheckBox(
            Se.Language.Video.TextToSpeech.DetectSpeakersSticky, vm, nameof(vm.StickySpeakers)));
        panelOptions.Children.Add(UiUtil.MakeButton(Se.Language.General.SelectAll, vm.SelectAllCommand));
        panelOptions.Children.Add(UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.InverseSelectionCommand));

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
            Se.Language.Video.TextToSpeech.DetectSpeakersTitle, width: 900, panelOptions, rowsView);
    }
}

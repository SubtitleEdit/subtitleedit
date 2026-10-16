using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewSortBy
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var comboSortBy = FunctionViewChrome.Compact(UiUtil.MakeComboBox(vm.SortByOptions, vm, nameof(vm.SelectedSortByOption)), Se.Language.Tools.SortBy.SortOrder);

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.SortBy.Title,
            FunctionViewChrome.MakeRow(Se.Language.Tools.SortBy.SortOrder, comboSortBy),
            EditorChrome.MakeSwitchRow(nameof(vm.SortByDescending), Se.Language.General.Descending));
    }
}

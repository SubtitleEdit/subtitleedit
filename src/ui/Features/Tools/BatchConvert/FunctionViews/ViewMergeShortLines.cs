using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewMergeShortLines
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.MergeShortLines.Title,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelMaxChars = UiUtil.MakeLabel(Se.Language.Tools.MergeShortLines.MaxCharacters);
        var numericMaxChars = UiUtil.MakeNumericUpDownInt(10, 500, vm.MergeShortLinesMaxCharacters, 130, vm, nameof(vm.MergeShortLinesMaxCharacters));

        var labelMaxMs = UiUtil.MakeLabel(Se.Language.Tools.MergeShortLines.MaxMillisecondsBetweenLines);
        var numericMaxMs = UiUtil.MakeNumericUpDownInt(0, 10000, vm.MergeShortLinesMaxMillisecondsBetweenLines, 130, vm, nameof(vm.MergeShortLinesMaxMillisecondsBetweenLines));

        var checkBoxOnlyContinuation = UiUtil.MakeCheckBox(Se.Language.Tools.MergeShortLines.OnlyContinuationLines, vm, nameof(vm.MergeShortLinesOnlyContinuationLines));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Avalonia.Thickness(10),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        grid.Add(labelHeader, 0, 0, 1, 2);
        grid.Add(labelMaxChars, 1);
        grid.Add(numericMaxChars, 1, 1);
        grid.Add(labelMaxMs, 2);
        grid.Add(numericMaxMs, 2, 1);
        grid.Add(checkBoxOnlyContinuation, 3, 0, 1, 2);

        return grid;
    }
}

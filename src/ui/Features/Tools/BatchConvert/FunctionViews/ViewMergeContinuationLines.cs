using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewMergeContinuationLines
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.MergeContinuationLines.Title,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelMaxMs = UiUtil.MakeLabel(Se.Language.Tools.MergeContinuationLines.MaxMillisecondsBetweenLines);
        var numericMaxMs = UiUtil.MakeNumericUpDownInt(0, 10000, vm.MergeContinuationLinesMaxMillisecondsBetweenLines, 130, vm, nameof(vm.MergeContinuationLinesMaxMillisecondsBetweenLines));

        var labelMaxChars = UiUtil.MakeLabel(Se.Language.Tools.MergeContinuationLines.MaxCharacters);
        var numericMaxChars = UiUtil.MakeNumericUpDownInt(20, 1000, vm.MergeContinuationLinesMaxCharacters, 130, vm, nameof(vm.MergeContinuationLinesMaxCharacters));

        var grid = new Grid
        {
            RowDefinitions =
            {
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
        grid.Add(labelMaxMs, 1);
        grid.Add(numericMaxMs, 1, 1);
        grid.Add(labelMaxChars, 2);
        grid.Add(numericMaxChars, 2, 1);

        return grid;
    }
}

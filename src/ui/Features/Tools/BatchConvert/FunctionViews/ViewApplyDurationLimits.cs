using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewApplyDurationLimits
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.ApplyDurationLimits.Title,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var checkBoxFixMin = UiUtil.MakeCheckBox(Se.Language.Tools.ApplyDurationLimits.FixMinDurationMs, vm, nameof(vm.ApplyDurationLimitsFixMin));
        var numericMin = UiUtil.MakeNumericUpDownInt(100, 60000, vm.ApplyDurationLimitsMinDurationMs, 130, vm, nameof(vm.ApplyDurationLimitsMinDurationMs));

        var checkBoxFixMax = UiUtil.MakeCheckBox(Se.Language.Tools.ApplyDurationLimits.FixMaxDurationMs, vm, nameof(vm.ApplyDurationLimitsFixMax));
        var numericMax = UiUtil.MakeNumericUpDownInt(100, 600000, vm.ApplyDurationLimitsMaxDurationMs, 130, vm, nameof(vm.ApplyDurationLimitsMaxDurationMs));

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
        grid.Add(checkBoxFixMin, 1);
        grid.Add(numericMin, 1, 1);
        grid.Add(checkBoxFixMax, 2);
        grid.Add(numericMax, 2, 1);

        return grid;
    }
}

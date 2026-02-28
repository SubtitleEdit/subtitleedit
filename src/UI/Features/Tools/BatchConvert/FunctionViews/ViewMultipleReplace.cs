using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewMultipleReplace
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = UiUtil.MakeLabel(Se.Language.General.MultipleReplace).WithBold();
        var buttonSettings = UiUtil.MakeButton(vm.ShowMultipleReplaceCommand, IconNames.Settings);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        grid.Add(labelHeader, 0, 0);
        grid.Add(buttonSettings, 1, 0);

        return grid;
    }
}

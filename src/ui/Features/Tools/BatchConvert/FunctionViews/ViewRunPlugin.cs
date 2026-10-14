using Avalonia.Controls;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRunPlugin
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Plugins.RunPlugin,
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Avalonia.Thickness(10),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        grid.Add(labelHeader, 0, 0, 1, 3);

        if (!vm.HasRunPlugins)
        {
            grid.Add(new TextBlock
            {
                Text = Se.Language.Plugins.NoBatchPluginsInstalled,
                TextWrapping = TextWrapping.Wrap,
            }, 1, 0, 1, 3);
            return grid;
        }

        var labelPlugin = UiUtil.MakeLabel(Se.Language.Plugins.Plugin);
        var comboPlugin = UiUtil.MakeComboBox(vm.RunPluginNames, vm, nameof(vm.SelectedRunPluginName), null)
            .WithMinWidth(220);
        var buttonConfigure = UiUtil.MakeButton(Se.Language.Plugins.ConfigurePlugin, vm.ConfigureRunPluginCommand);
        var textInfo = new TextBlock
        {
            Text = Se.Language.Plugins.BatchPluginInfo,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.7,
        };

        grid.Add(labelPlugin, 1, 0);
        grid.Add(comboPlugin, 1, 1);
        grid.Add(buttonConfigure, 1, 2);
        grid.Add(textInfo, 2, 0, 1, 3);

        return grid;
    }
}

using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRunPlugin
{
    public static Control Make(BatchConvertViewModel vm)
    {
        if (!vm.HasRunPlugins)
        {
            return FunctionViewChrome.MakePanel(vm, Se.Language.Plugins.RunPlugin,
                EditorChrome.MakeHint(Se.Language.Plugins.NoBatchPluginsInstalled));
        }

        var comboPlugin = FunctionViewChrome.Compact(UiUtil.MakeComboBox(vm.RunPluginNames, vm, nameof(vm.SelectedRunPluginName), null), Se.Language.Plugins.Plugin, 220);
        var buttonConfigure = FunctionViewChrome.MakeButton("fa-solid fa-sliders", Se.Language.Plugins.ConfigurePlugin, vm.ConfigureRunPluginCommand);

        var panelPlugin = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8,
            Children = { comboPlugin, buttonConfigure },
        };

        return FunctionViewChrome.MakePanel(vm, Se.Language.Plugins.RunPlugin,
            FunctionViewChrome.MakeRow(Se.Language.Plugins.Plugin, panelPlugin),
            EditorChrome.MakeHint(Se.Language.Plugins.BatchPluginInfo));
    }
}

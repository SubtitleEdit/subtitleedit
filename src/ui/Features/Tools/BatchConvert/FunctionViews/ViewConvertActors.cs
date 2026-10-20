using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewConvertActors
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var comboBoxFrom = FunctionViewChrome.Compact(UiUtil.MakeComboBox(vm.ConvertActorsFromTypes, vm, nameof(vm.SelectedConvertActorsFromType)), Se.Language.Tools.ConvertActors.ConvertActorFrom);
        var comboBoxTo = FunctionViewChrome.Compact(UiUtil.MakeComboBox(vm.ConvertActorsToTypes, vm, nameof(vm.SelectedConvertActorsToType)), Se.Language.Tools.ConvertActors.ConvertActorTo);

        var colorPicker = UiUtil.MakeColorPickerButton(vm, nameof(vm.ConvertActorsColor));
        colorPicker.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ConvertActorsSetColor)) { Source = vm, Mode = BindingMode.OneWay });
        AutomationProperties.SetName(colorPicker, Se.Language.Tools.ConvertActors.SetColor);

        var comboBoxCasing = FunctionViewChrome.Compact(new ComboBox
        {
            ItemsSource = vm.ConvertActorsCasingOptions,
            DataContext = vm,
        }, Se.Language.General.ChangeCasing, 160);
        comboBoxCasing.Bind(ComboBox.SelectedIndexProperty, new Binding(nameof(vm.ConvertActorsCasingIndex)) { Mode = BindingMode.TwoWay });
        comboBoxCasing.Bind(ComboBox.IsVisibleProperty, new Binding(nameof(vm.ConvertActorsChangeCasing)) { Source = vm, Mode = BindingMode.OneWay });

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.ConvertActors.Title,
            FunctionViewChrome.MakeRow(Se.Language.Tools.ConvertActors.ConvertActorFrom, comboBoxFrom),
            FunctionViewChrome.MakeRow(Se.Language.Tools.ConvertActors.ConvertActorTo, comboBoxTo),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.ConvertActorsSetColor), Se.Language.Tools.ConvertActors.SetColor, colorPicker),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.ConvertActorsChangeCasing), Se.Language.General.ChangeCasing, comboBoxCasing),
            EditorChrome.MakeSwitchRow(nameof(vm.ConvertActorsOnlyNames), Se.Language.Tools.ConvertActors.OnlyNames));
    }
}

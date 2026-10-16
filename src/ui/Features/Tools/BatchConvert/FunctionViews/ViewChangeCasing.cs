using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewChangeCasing
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var radioNormalCasing = MakeRadio(Se.Language.General.NormalCasing, nameof(vm.NormalCasing));

        // Options of "Normal casing", in under it
        var switchNormalCasingFixNames = FunctionViewChrome.Indented(
            EditorChrome.MakeSwitchRow(nameof(vm.NormalCasingFixNames), Se.Language.Tools.ChangeCasing.FixNames));
        var switchNormalCasingOnlyUpper = FunctionViewChrome.Indented(
            EditorChrome.MakeSwitchRow(nameof(vm.NormalCasingOnlyUpper), Se.Language.Tools.ChangeCasing.OnlyFixUppercaseLines));

        var radioFixNamesOnly = MakeRadio(Se.Language.Tools.ChangeCasing.FixNamesOnly, nameof(vm.FixNamesOnly));
        var radioAllUppercase = MakeRadio(Se.Language.Tools.ChangeCasing.AllUppercase, nameof(vm.AllUppercase));
        var radioAllLowercase = MakeRadio(Se.Language.Tools.ChangeCasing.AllLowercase, nameof(vm.AllLowercase));

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.ChangeCasing,
            radioNormalCasing,
            switchNormalCasingFixNames,
            switchNormalCasingOnlyUpper,
            radioFixNamesOnly,
            radioAllUppercase,
            radioAllLowercase);
    }

    private static RadioButton MakeRadio(string text, string property)
    {
        return FunctionViewChrome.Compact(new RadioButton
        {
            Content = text,
            [!RadioButton.IsCheckedProperty] = new Binding(property) { Mode = BindingMode.TwoWay },
            GroupName = "ChangeCasingType",
        });
    }
}

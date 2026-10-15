using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewChangeFrameRate
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var comboFromFrameRate = FunctionViewChrome.Compact(new ComboBox
        {
            ItemsSource = vm.FromFrameRates,
            SelectedValue = vm.SelectedFromFrameRate,
        }.WithFrameRateDisplay()
         .WithBindSelected(nameof(vm.SelectedFromFrameRate)), Se.Language.Sync.FromFrameRate, 110);

        //var buttonFromFrameRate = UiUtil.MakeButtonBrowse(vm.BrowseFromFrameRateCommand);

        //var buttonSwitch = UiUtil.MakeButton(vm.SwitchFrameRatesCommand, IconNames.MdiSwapVertical);

        var comboToFrameRate = FunctionViewChrome.Compact(new ComboBox
        {
            ItemsSource = vm.ToFrameRates,
            SelectedValue = vm.SelectedToFrameRate,
        }.WithFrameRateDisplay()
         .WithBindSelected(nameof(vm.SelectedToFrameRate)), Se.Language.Sync.ToFrameRate, 110);

        //var buttonToFrameRate = UiUtil.MakeButtonBrowse(vm.BrowseToFrameRateCommand);

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.ChangeFrameRate,
            FunctionViewChrome.MakeRow(Se.Language.Sync.FromFrameRate, comboFromFrameRate),
            FunctionViewChrome.MakeRow(Se.Language.Sync.ToFrameRate, comboToFrameRate));
    }
}

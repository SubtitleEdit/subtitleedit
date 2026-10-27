using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewBeautifyTimeCodes
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var radioFrameRateFromVideo = FunctionViewChrome.Compact(UiUtil.MakeRadioButton(Se.Language.Tools.BeautifyTimeCodes.BatchFrameRateFromVideo, vm, nameof(vm.BeautifyTimeCodesUseVideoFrameRate), "BeautifyTimeCodesFrameRateSource"));
        var radioFrameRateFixed = FunctionViewChrome.Compact(UiUtil.MakeRadioButton(Se.Language.Tools.BeautifyTimeCodes.BatchFrameRateFixed, vm, nameof(vm.BeautifyTimeCodesUseFixedFrameRate), "BeautifyTimeCodesFrameRateSource"));

        var comboFrameRate = FunctionViewChrome.Compact(new ComboBox
        {
            ItemsSource = vm.BeautifyTimeCodesFrameRates,
            SelectedValue = vm.SelectedBeautifyTimeCodesFrameRate,
        }.WithFrameRateDisplay()
         .WithBindSelected(nameof(vm.SelectedBeautifyTimeCodesFrameRate))
         .WithBindEnabled(nameof(vm.BeautifyTimeCodesUseFixedFrameRate)), Se.Language.Tools.BeautifyTimeCodes.BatchFrameRateFixed, 110);

        var panelFrameRateFixed = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Children =
            {
                radioFrameRateFixed,
                comboFrameRate,
            }
        };

        var buttonEditProfile = FunctionViewChrome.MakeButton("fa-solid fa-sliders", Se.Language.Tools.BeautifyTimeCodes.EditProfile, vm.ShowBeautifyTimeCodesProfileCommand);

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.BeautifyTimeCodes.Title,
            EditorChrome.MakeSwitchRow(nameof(vm.BeautifyTimeCodesSnapToShotChanges), Se.Language.Tools.BeautifyTimeCodes.BatchSnapToShotChanges),
            radioFrameRateFromVideo,
            panelFrameRateFixed,
            buttonEditProfile,
            EditorChrome.MakeHint(Se.Language.Tools.BeautifyTimeCodes.BatchInfo));
    }
}

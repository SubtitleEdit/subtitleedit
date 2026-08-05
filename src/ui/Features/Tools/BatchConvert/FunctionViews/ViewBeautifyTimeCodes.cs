using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewBeautifyTimeCodes
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.BeautifyTimeCodes.Title,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
        };

        var checkBoxSnapToShotChanges = UiUtil.MakeCheckBox(Se.Language.Tools.BeautifyTimeCodes.BatchSnapToShotChanges, vm, nameof(vm.BeautifyTimeCodesSnapToShotChanges));

        var buttonEditProfile = UiUtil.MakeButton(vm.ShowBeautifyTimeCodesProfileCommand, IconNames.Settings, Se.Language.Tools.BeautifyTimeCodes.EditProfile);

        var labelInfo = new TextBlock
        {
            Text = Se.Language.Tools.BeautifyTimeCodes.BatchInfo,
            Opacity = 0.7,
            FontStyle = FontStyle.Italic,
            TextWrapping = TextWrapping.Wrap,
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Avalonia.Thickness(10),
            Spacing = 10,
            Children =
            {
                labelHeader,
                checkBoxSnapToShotChanges,
                buttonEditProfile,
                labelInfo,
            }
        };
    }
}

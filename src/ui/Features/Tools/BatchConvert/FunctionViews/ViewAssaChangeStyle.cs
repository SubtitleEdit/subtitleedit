using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAssaChangeStyle
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.BatchConvert.AssaChangeStyleTitle,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelOnlyAssa = new Label
        {
            Content = Se.Language.Tools.BatchConvert.AssaChangeResolutionOnlyAppliesToAssa,
            FontStyle = Avalonia.Media.FontStyle.Italic,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelFrom = UiUtil.MakeLabel(Se.Language.Tools.BatchConvert.AssaChangeStyleFromStyle);
        var textBoxFrom = UiUtil.MakeTextBox(130, vm, nameof(vm.AssaChangeStyleFromStyle));
        var buttonBrowseFrom = UiUtil.MakeButtonBrowse(vm.AssaChangeStyleBrowseFromStyleCommand, accessibleName: Se.Language.Tools.BatchConvert.AssaChangeStyleFromStyle);

        var labelTo = UiUtil.MakeLabel(Se.Language.Tools.BatchConvert.AssaChangeStyleToStyle);
        var textBoxTo = UiUtil.MakeTextBox(130, vm, nameof(vm.AssaChangeStyleToStyle));
        var buttonBrowseTo = UiUtil.MakeButtonBrowse(vm.AssaChangeStyleBrowseToStyleCommand, accessibleName: Se.Language.Tools.BatchConvert.AssaChangeStyleToStyle);

        var panelFromTo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                labelFrom,
                textBoxFrom,
                buttonBrowseFrom,
                labelTo,
                textBoxTo,
                buttonBrowseTo,
            },
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var buttonImport = UiUtil.MakeButton(Se.Language.Tools.BatchConvert.AssaChangeStyleImportStyle, vm.AssaChangeStyleImportStyleCommand);
        var labelImported = UiUtil.MakeLabel(string.Empty);
        labelImported.Bind(Label.ContentProperty, new Binding
        {
            Path = nameof(vm.AssaChangeStyleImportFileName),
            Mode = BindingMode.OneWay,
            Source = vm,
        });

        var panelImport = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = { buttonImport, labelImported },
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var checkBoxTrim = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.AssaChangeStyleTrimUnusedStyles, vm, nameof(vm.AssaChangeStyleTrimUnusedStyles));

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                labelHeader,
                labelOnlyAssa,
                panelImport,
                panelFromTo,
                checkBoxTrim,
            },
            Margin = new Avalonia.Thickness(10),
        };

        return panel;
    }
}

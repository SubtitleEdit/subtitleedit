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
        var textBoxFrom = EditorChrome.MakeTextField(nameof(vm.AssaChangeStyleFromStyle), Se.Language.Tools.BatchConvert.AssaChangeStyleFromStyle);
        var buttonBrowseFrom = UiUtil.MakeButtonBrowse(vm.AssaChangeStyleBrowseFromStyleCommand, accessibleName: Se.Language.Tools.BatchConvert.AssaChangeStyleFromStyle);

        var textBoxTo = EditorChrome.MakeTextField(nameof(vm.AssaChangeStyleToStyle), Se.Language.Tools.BatchConvert.AssaChangeStyleToStyle);
        var buttonBrowseTo = UiUtil.MakeButtonBrowse(vm.AssaChangeStyleBrowseToStyleCommand, accessibleName: Se.Language.Tools.BatchConvert.AssaChangeStyleToStyle);

        var buttonImport = FunctionViewChrome.MakeButton("fa-solid fa-file-import", Se.Language.Tools.BatchConvert.AssaChangeStyleImportStyle, vm.AssaChangeStyleImportStyleCommand);
        var labelImported = EditorChrome.MakeBoundHint(nameof(vm.AssaChangeStyleImportFileName));
        labelImported.Bind(TextBlock.TextProperty, new Binding
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
        };

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.BatchConvert.AssaChangeStyleTitle,
            EditorChrome.MakeHint(Se.Language.Tools.BatchConvert.AssaChangeResolutionOnlyAppliesToAssa),
            panelImport,
            FunctionViewChrome.MakeRow(Se.Language.Tools.BatchConvert.AssaChangeStyleFromStyle, WithButton(textBoxFrom, buttonBrowseFrom)),
            FunctionViewChrome.MakeRow(Se.Language.Tools.BatchConvert.AssaChangeStyleToStyle, WithButton(textBoxTo, buttonBrowseTo)),
            EditorChrome.MakeSwitchRow(nameof(vm.AssaChangeStyleTrimUnusedStyles), Se.Language.Tools.BatchConvert.AssaChangeStyleTrimUnusedStyles));
    }

    /// <summary>
    /// The text box takes the width the browse button leaves.
    /// </summary>
    private static DockPanel WithButton(TextBox textBox, Button button)
    {
        button.Margin = new Avalonia.Thickness(6, 0, 0, 0);
        DockPanel.SetDock(button, Dock.Right);
        return new DockPanel { Children = { button, textBox } };
    }
}

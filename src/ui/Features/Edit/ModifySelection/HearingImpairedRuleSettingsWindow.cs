using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public class HearingImpairedRuleSettingsWindow : Window
{
    private readonly HearingImpairedRuleSettingsViewModel _vm;

    public HearingImpairedRuleSettingsWindow(HearingImpairedRuleSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Edit.ModifySelection.HearingImpaired;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Edit.ModifySelection;

        var labelBetween = UiUtil.MakeLabel(Se.Language.Tools.RemoveTextForHearingImpaired.RemoveTextBetween);
        var checkBoxBrackets = UiUtil.MakeCheckBox(l.HearingImpairedBrackets, vm, nameof(vm.IsBracketsOn));
        var checkBoxCurlyBrackets = UiUtil.MakeCheckBox(l.HearingImpairedCurlyBrackets, vm, nameof(vm.IsCurlyBracketsOn));
        var checkBoxParentheses = UiUtil.MakeCheckBox(l.HearingImpairedParentheses, vm, nameof(vm.IsParenthesesOn));
        var checkBoxCustom = UiUtil.MakeCheckBox(l.HearingImpairedCustom, vm, nameof(vm.IsCustomOn));
        var labelCustom = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.CustomText)).WithMarginLeft(5);
        var panelCustom = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                checkBoxCustom,
                labelCustom,
            }
        };

        var betweenPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                labelBetween,
                checkBoxBrackets,
                checkBoxCurlyBrackets,
                checkBoxParentheses,
                panelCustom,
            }
        };

        var otherPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                UiUtil.MakeCheckBox(l.HearingImpairedTextBeforeColon, vm, nameof(vm.IsTextBeforeColonOn)),
                UiUtil.MakeCheckBox(l.HearingImpairedUppercaseLine, vm, nameof(vm.IsUppercaseLineOn)),
                UiUtil.MakeCheckBox(l.HearingImpairedLineContains, vm, nameof(vm.IsLineContainsOn)),
                UiUtil.MakeCheckBox(l.HearingImpairedMusicSymbols, vm, nameof(vm.IsMusicSymbolsOn)),
                UiUtil.MakeCheckBox(l.HearingImpairedInterjections, vm, nameof(vm.IsInterjectionsOn)),
            }
        };

        var labelHint = UiUtil.MakeLabel(l.HearingImpairedSettingsHint).WithMarginTop(5);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 5,
            MinWidth = 320,
        };

        grid.Add(UiUtil.MakeBorderForControl(betweenPanel), 0);
        grid.Add(UiUtil.MakeBorderForControl(otherPanel), 1);
        grid.Add(labelHint, 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { checkBoxBrackets.Focus(); }; // not an action button - a focused button clicks on bare Space
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

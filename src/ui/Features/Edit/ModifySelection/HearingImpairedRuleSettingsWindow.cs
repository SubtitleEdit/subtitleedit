using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

/// <summary>
/// Settings of the "Hearing impaired" modify-selection rule, in the editor look (see
/// <see cref="EditorChrome"/>): which brackets and which other kinds of lines count, Cancel and OK
/// at the bottom.
/// </summary>
public class HearingImpairedRuleSettingsWindow : Window
{
    private readonly HearingImpairedRuleSettingsViewModel _vm;

    public HearingImpairedRuleSettingsWindow(HearingImpairedRuleSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Edit.ModifySelection.HearingImpaired;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Edit.ModifySelection;

        var rowBrackets = EditorChrome.MakeToggleRow(nameof(vm.IsBracketsOn), l.HearingImpairedBrackets);
        var rowCustom = EditorChrome.MakeToggleRow(nameof(vm.IsCustomOn), l.HearingImpairedCustom);
        var labelCustom = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0)).WithBindText(vm, nameof(vm.CustomText));
        rowCustom.Children.Add(labelCustom);

        var betweenPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                rowBrackets,
                EditorChrome.MakeToggleRow(nameof(vm.IsCurlyBracketsOn), l.HearingImpairedCurlyBrackets),
                EditorChrome.MakeToggleRow(nameof(vm.IsParenthesesOn), l.HearingImpairedParentheses),
                rowCustom,
            }
        };

        var otherPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                EditorChrome.MakeToggleRow(nameof(vm.IsTextBeforeColonOn), l.HearingImpairedTextBeforeColon),
                EditorChrome.MakeToggleRow(nameof(vm.IsUppercaseLineOn), l.HearingImpairedUppercaseLine),
                EditorChrome.MakeToggleRow(nameof(vm.IsLineContainsOn), l.HearingImpairedLineContains),
                EditorChrome.MakeToggleRow(nameof(vm.IsMusicSymbolsOn), l.HearingImpairedMusicSymbols),
                EditorChrome.MakeToggleRow(nameof(vm.IsInterjectionsOn), Se.Language.General.Interjections),
            }
        };

        var labelHint = new TextBlock
        {
            Text = l.HearingImpairedSettingsHint,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.FaintTextBrush,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 340,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        var hintSection = new Border
        {
            Child = labelHint,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 10, 14, 12),
        };

        var sections = new StackPanel { MinWidth = 320 };
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.Tools.RemoveTextForHearingImpaired.RemoveTextBetween, betweenPanel, first: true));
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Other, otherPanel));
        sections.Children.Add(hintSection);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        // not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, rowBrackets.Children.OfType<ToggleSwitch>().First());
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

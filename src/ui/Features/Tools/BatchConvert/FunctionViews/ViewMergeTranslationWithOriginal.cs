using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

/// <summary>
/// The two styles of "Merge translation with original" - the same settings as Tools -> Merge two
/// subtitles. Style 1 is used for the original text, style 2 for the translation.
/// </summary>
public static class ViewMergeTranslationWithOriginal
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var requirements = EditorChrome.MakeHint(string.Format(
            Se.Language.Tools.BatchConvert.MergeTranslationWithOriginalRequirements,
            Se.Language.General.AutoTranslate,
            Se.Language.General.TargetFormat,
            AdvancedSubStationAlpha.NameOfFormat));
        requirements.ClearValue(TextBlock.ForegroundProperty);
        requirements.FontWeight = FontWeight.Bold;

        var rows = new List<Control>
        {
            requirements,
            EditorChrome.MakeHint(Se.Language.Tools.BatchConvert.MergeTranslationWithOriginalInfo),
        };

        rows.Add(MakeDivider());
        rows.AddRange(MakeStyleRows(vm, Se.Language.Tools.BatchConvert.MergeTranslationWithOriginalStyleOriginal,
            nameof(vm.MergeTranslationWithOriginalFontName1), nameof(vm.MergeTranslationWithOriginalFontSize1),
            nameof(vm.MergeTranslationWithOriginalBold1), nameof(vm.MergeTranslationWithOriginalItalic1),
            nameof(vm.MergeTranslationWithOriginalPrimaryColor1), nameof(vm.MergeTranslationWithOriginalOutlineColor1),
            nameof(vm.MergeTranslationWithOriginalOutlineWidth1), nameof(vm.MergeTranslationWithOriginalShadowWidth1),
            nameof(vm.MergeTranslationWithOriginalAlignTop1), "mergeTranslationWithOriginalAlign1"));

        rows.Add(MakeDivider());
        rows.AddRange(MakeStyleRows(vm, Se.Language.Tools.BatchConvert.MergeTranslationWithOriginalStyleTranslation,
            nameof(vm.MergeTranslationWithOriginalFontName2), nameof(vm.MergeTranslationWithOriginalFontSize2),
            nameof(vm.MergeTranslationWithOriginalBold2), nameof(vm.MergeTranslationWithOriginalItalic2),
            nameof(vm.MergeTranslationWithOriginalPrimaryColor2), nameof(vm.MergeTranslationWithOriginalOutlineColor2),
            nameof(vm.MergeTranslationWithOriginalOutlineWidth2), nameof(vm.MergeTranslationWithOriginalShadowWidth2),
            nameof(vm.MergeTranslationWithOriginalAlignTop2), "mergeTranslationWithOriginalAlign2"));

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.BatchConvert.MergeTranslationWithOriginalTitle, rows.ToArray());
    }

    private static Border MakeDivider() => new()
    {
        Height = 1,
        Background = EditorChrome.LineBrush,
        Margin = new Thickness(0, 8, 0, 2),
    };

    private static Control[] MakeStyleRows(BatchConvertViewModel vm,
        string title,
        string fontNamePath, string fontSizePath,
        string boldPath, string italicPath,
        string primaryColorPath, string outlineColorPath,
        string outlineWidthPath, string shadowWidthPath,
        string alignTopPath, string alignGroupName)
    {
        var comboFontName = FunctionViewChrome.Compact(UiUtil.MakeComboBox(vm.MergeTranslationWithOriginalFontNames, vm, fontNamePath), $"{title} - {Se.Language.General.FontName}");
        var numFontSize = FunctionViewChrome.MakeIntField(vm, fontSizePath, 6, 200, 20, $"{title} - {Se.Language.General.FontSize}");

        var primaryPicker = UiUtil.MakeColorPickerButton(vm, primaryColorPath, false)
            .WithAccessibleName($"{title} - {Se.Language.General.TextColor}");
        var outlinePicker = UiUtil.MakeColorPickerButton(vm, outlineColorPath, false)
            .WithAccessibleName($"{title} - {Se.Language.General.OutlineColor}");

        var numOutlineWidth = FunctionViewChrome.Compact(UiUtil.MakeNumericUpDownOneDecimal(0, 20, FunctionViewChrome.FieldWidth, vm, outlineWidthPath), $"{title} - {Se.Language.General.OutlineWidth}");
        var numShadowWidth = FunctionViewChrome.Compact(UiUtil.MakeNumericUpDownOneDecimal(0, 20, FunctionViewChrome.FieldWidth, vm, shadowWidthPath), $"{title} - {Se.Language.General.ShadowWidth}");

        var radioTop = FunctionViewChrome.Compact(UiUtil.MakeRadioButton(Se.Language.General.Top, vm, alignTopPath, alignGroupName))
            .WithAccessibleName($"{title} - {Se.Language.General.Top}");
        var radioBottom = FunctionViewChrome.Compact(new RadioButton
        {
            Content = Se.Language.General.Bottom,
            GroupName = alignGroupName,
            DataContext = vm,
        }).WithAccessibleName($"{title} - {Se.Language.General.Bottom}");
        radioBottom.Bind(RadioButton.IsCheckedProperty, new Binding(alignTopPath)
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
            Converter = InverseBooleanConverter.Instance,
        });
        var alignPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children = { radioTop, radioBottom },
        };

        return new Control[]
        {
            EditorChrome.MakeDimLabel(title, new Thickness(0, 6, 0, 0)),
            FunctionViewChrome.MakeRow(Se.Language.General.FontName, comboFontName),
            FunctionViewChrome.MakeRow(Se.Language.General.FontSize, numFontSize),
            EditorChrome.MakeSwitchRow(EditorChrome.MakeSwitch(boldPath, $"{title} - {Se.Language.General.Bold}"), Se.Language.General.Bold),
            EditorChrome.MakeSwitchRow(EditorChrome.MakeSwitch(italicPath, $"{title} - {Se.Language.General.Italic}"), Se.Language.General.Italic),
            FunctionViewChrome.MakeRow(Se.Language.General.TextColor, primaryPicker),
            FunctionViewChrome.MakeRow(Se.Language.General.OutlineColor, outlinePicker),
            FunctionViewChrome.MakeRow(Se.Language.General.OutlineWidth, numOutlineWidth),
            FunctionViewChrome.MakeRow(Se.Language.General.ShadowWidth, numShadowWidth),
            FunctionViewChrome.MakeRow(Se.Language.General.Alignment, alignPanel),
        };
    }
}

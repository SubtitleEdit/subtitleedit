using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.TmpegEncXmlProperties;

/// <summary>
/// TMPGEnc XML properties in the editor look (see <see cref="FormatPropertiesChrome"/>).
/// </summary>
public class TmpegEncXmlPropertiesWindow : Window
{
    public TmpegEncXmlPropertiesWindow(TmpegEncXmlProperties.TmpegEncXmlPropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.File.XProperties, new TmpegEncXml().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Form);
        vm.Window = this;
        DataContext = vm;

        var comboBoxFontName = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.FontNames, vm, nameof(vm.SelectedFontName)), Se.Language.General.FontName, 220);

        var numericUpDownFontHeight = FormatPropertiesChrome.Number(UiUtil.MakeNumericUpDownThreeDecimals(0.04m, 1.0m, 120, vm, nameof(vm.FontHeight)), Se.Language.General.FontHeight, 120);
        numericUpDownFontHeight.Increment = 0.001m;

        var numericUpDownOffsetX = FormatPropertiesChrome.Number(UiUtil.MakeNumericUpDownThreeDecimals(-100.0m, 100.0m, 120, vm, nameof(vm.OffsetX)), Se.Language.General.OffsetX, 120);
        numericUpDownOffsetX.Increment = 0.001m;

        var numericUpDownOffsetY = FormatPropertiesChrome.Number(UiUtil.MakeNumericUpDownThreeDecimals(-100.0m, 100.0m, 120, vm, nameof(vm.OffsetY)), Se.Language.General.OffsetY, 120);
        numericUpDownOffsetY.Increment = 0.001m;

        var font = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(Se.Language.General.FontName, comboBoxFontName),
            FormatPropertiesChrome.Row(Se.Language.General.FontHeight, numericUpDownFontHeight),
            FormatPropertiesChrome.Row(string.Empty, EditorChrome.MakeSwitchRow(nameof(vm.IsFontBold), Se.Language.General.Bold)));

        var position = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(Se.Language.General.OffsetX, numericUpDownOffsetX),
            FormatPropertiesChrome.Row(Se.Language.General.OffsetY, numericUpDownOffsetY));

        var buttonBar = EditorChrome.MakeDockedCancelOkBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), UiUtil.MakeButtonOk(vm.OkCommand));

        Content = FormatPropertiesChrome.MakeRoot(
            buttonBar,
            EditorChrome.MakeEditorSection(Se.Language.General.Font, font, first: true),
            EditorChrome.MakeEditorSection(Se.Language.General.Position, position));

        UiUtil.FocusOnFirstActivation(this, comboBoxFontName); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

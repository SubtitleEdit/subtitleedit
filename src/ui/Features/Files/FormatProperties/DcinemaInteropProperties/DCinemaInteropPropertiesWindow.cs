using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.DCinemaInteropProperties;

/// <summary>
/// D-Cinema Interop properties in the editor look (see <see cref="FormatPropertiesChrome"/>):
/// the subtitle header and the font, Import/Export in the button bar.
/// </summary>
public class DCinemaInteropPropertiesWindow : Window
{
    public DCinemaInteropPropertiesWindow(DCinemaInteropPropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.WindowTitle))
        {
            Source = vm,
            Mode = BindingMode.OneWay,
        });
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 560;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Form);
        vm.Window = this;
        DataContext = vm;

        var lang = Se.Language.File.PropertiesDCinema;

        // General section
        var switchGenerateIdAuto = EditorChrome.MakeSwitchRow(nameof(vm.GenerateIdAuto), lang.GenerateIdAuto, out var toggleGenerateIdAuto);
        var textBoxSubtitleId = EditorChrome.AsTextField(UiUtil.MakeTextBox(340, vm, nameof(vm.SubtitleId)), lang.SubtitleId);
        var buttonGenerateId = FormatPropertiesChrome.MakeAction("fa-solid fa-rotate", lang.GenerateId, vm.GenerateSubtitleIdCommand);
        var textBoxMovieTitle = EditorChrome.AsTextField(UiUtil.MakeTextBox(340, vm, nameof(vm.MovieTitle)), lang.MovieTitle);
        var numericUpDownReelNumber = FormatPropertiesChrome.Number(UiUtil.MakeNumericUpDownInt(1, 250, 1, 90, vm, nameof(vm.ReelNumber)), lang.ReelNumber);
        var comboBoxLanguage = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage)), Se.Language.General.Language, 150);

        var general = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(string.Empty, switchGenerateIdAuto),
            FormatPropertiesChrome.Row(lang.SubtitleId, textBoxSubtitleId, buttonGenerateId),
            FormatPropertiesChrome.Row(lang.MovieTitle, textBoxMovieTitle),
            FormatPropertiesChrome.Row(lang.ReelNumber, numericUpDownReelNumber, FormatPropertiesChrome.InlineLabel(Se.Language.General.Language), comboBoxLanguage));

        // Font section
        var textBoxFontId = EditorChrome.AsTextField(UiUtil.MakeTextBox(200, vm, nameof(vm.FontId)), lang.FontId);
        var textBoxFontUri = EditorChrome.AsTextField(UiUtil.MakeTextBox(340, vm, nameof(vm.FontUri)), lang.FontUri);
        var buttonFontColor = FormatPropertiesChrome.ColorButton(Se.Language.General.ChooseColor, Se.Language.General.FontColor, vm.ChooseFontColorCommand, nameof(vm.FontColor));
        var comboBoxFontEffect = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox<string>(vm.FontEffects, vm, nameof(vm.SelectedFontEffect)), lang.FontEffect, 140);
        var buttonFontEffectColor = FormatPropertiesChrome.ColorButton(Se.Language.General.ChooseColor, lang.EffectColor, vm.ChooseFontEffectColorCommand, nameof(vm.FontEffectColor));
        var numericUpDownFontSize = FormatPropertiesChrome.Number(UiUtil.MakeNumericUpDownInt(0, 250, 42, 90, vm, nameof(vm.FontSize)), Se.Language.General.FontSize);
        var numericUpDownTopBottomMargin = FormatPropertiesChrome.Number(UiUtil.MakeNumericUpDownInt(1, 50, 8, 90, vm, nameof(vm.TopBottomMargin)), Se.Language.General.TopBottomMargin);
        var numericUpDownZPosition = FormatPropertiesChrome.Number(UiUtil.MakeNumericUpDownTwoDecimals(-10, 10, 90, vm, nameof(vm.ZPosition)), lang.ZPosition);
        var labelZPositionHelp = FormatPropertiesChrome.Hint(lang.ZPositionHelp, 300);
        var numericUpDownFadeUpTime = FormatPropertiesChrome.Number(UiUtil.MakeNumericUpDownInt(0, 50, 0, 90, vm, nameof(vm.FadeUpTime)), lang.FadeUpTime);
        var numericUpDownFadeDownTime = FormatPropertiesChrome.Number(UiUtil.MakeNumericUpDownInt(0, 50, 0, 90, vm, nameof(vm.FadeDownTime)), lang.FadeDownTime);

        var font = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(lang.FontId, textBoxFontId),
            FormatPropertiesChrome.Row(lang.FontUri, textBoxFontUri),
            FormatPropertiesChrome.Row(Se.Language.General.FontColor, buttonFontColor),
            FormatPropertiesChrome.Row(lang.FontEffect, comboBoxFontEffect, FormatPropertiesChrome.InlineLabel(lang.EffectColor), buttonFontEffectColor),
            FormatPropertiesChrome.Row(Se.Language.General.FontSize, numericUpDownFontSize, FormatPropertiesChrome.InlineLabel(Se.Language.General.TopBottomMargin), numericUpDownTopBottomMargin),
            FormatPropertiesChrome.Row(lang.ZPosition, numericUpDownZPosition, labelZPositionHelp),
            FormatPropertiesChrome.Row(lang.FadeUpTime, numericUpDownFadeUpTime, FormatPropertiesChrome.Unit(Se.Language.General.Frames),
                FormatPropertiesChrome.InlineLabel(lang.FadeDownTime), numericUpDownFadeDownTime, FormatPropertiesChrome.Unit(Se.Language.General.Frames)));

        var buttonImport = FormatPropertiesChrome.MakeAction("fa-solid fa-file-import", Se.Language.General.ImportDotDotDot, vm.ImportCommand);
        var buttonExport = FormatPropertiesChrome.MakeAction("fa-solid fa-file-export", Se.Language.General.ExportDotDotDot, vm.ExportCommand);
        var buttonBar = EditorChrome.MakeDockedCancelOkBar(FormatPropertiesChrome.MakeActions(buttonImport, buttonExport), UiUtil.MakeButtonCancel(vm.CancelCommand), UiUtil.MakeButtonOk(vm.OkCommand));

        Content = FormatPropertiesChrome.MakeRoot(
            buttonBar,
            EditorChrome.MakeEditorSection(Se.Language.General.General, general, first: true),
            EditorChrome.MakeEditorSection(Se.Language.General.Font, font));

        UiUtil.FocusOnFirstActivation(this, toggleGenerateIdAuto); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

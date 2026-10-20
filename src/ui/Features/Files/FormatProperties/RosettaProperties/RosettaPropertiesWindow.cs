using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.RosettaProperties;

/// <summary>
/// Rosetta properties in the editor look (see <see cref="FormatPropertiesChrome"/>), Import in the button bar.
/// </summary>
public class RosettaPropertiesWindow : Window
{
    public RosettaPropertiesWindow(RosettaPropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.RosettaProperties;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 420;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Form);
        vm.Window = this;
        DataContext = vm;

        var comboBoxLanguage = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox<string>(vm.Languages, vm, nameof(vm.SelectedLanguage)), Se.Language.General.Language, 160);
        var textBoxLineHeight = EditorChrome.AsTextField(UiUtil.MakeTextBox(120, vm, nameof(vm.SelectedLineHeight)), Se.Language.General.LineHeight);
        var numericUpDownFontSize = FormatPropertiesChrome.Number(UiUtil.MakeNumericUpDownOneDecimal(1, 100, 120, vm, nameof(vm.SelectedFontSize)), Se.Language.File.RosettaFontSize, 120);

        var rows = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(Se.Language.General.Language, comboBoxLanguage),
            FormatPropertiesChrome.Row(Se.Language.General.LineHeight, textBoxLineHeight),
            FormatPropertiesChrome.Row(Se.Language.File.RosettaFontSize, numericUpDownFontSize));

        var buttonImport = FormatPropertiesChrome.MakeAction("fa-solid fa-file-import", Se.Language.General.ImportDotDotDot, vm.ImportCommand);
        var buttonBar = EditorChrome.MakeDockedCancelOkBar(buttonImport, UiUtil.MakeButtonCancel(vm.CancelCommand), UiUtil.MakeButtonOk(vm.OkCommand));

        Content = FormatPropertiesChrome.MakeRoot(
            buttonBar,
            EditorChrome.MakeEditorSection(Se.Language.General.Settings, rows, first: true));

        UiUtil.FocusOnFirstActivation(this, comboBoxLanguage); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

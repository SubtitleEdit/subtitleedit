using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Files.FormatProperties;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportDvbTeletext;

/// <summary>
/// DVB Teletext export settings in the editor look (see <see cref="FormatPropertiesChrome"/>).
/// </summary>
public class ExportDvbTeletextWindow : Window
{
    public ExportDvbTeletextWindow(ExportDvbTeletextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.TitleExportDvbTeletext;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Form);
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.File.Export;
        var numericPageNumber = FormatPropertiesChrome.Number(UiUtil.MakeNumericUpDownInt(100, 899, 888, 120, vm, nameof(vm.PageNumber)), l.ExportDvbTeletextPageNumber);
        var textBoxLanguage = EditorChrome.AsTextField(UiUtil.MakeTextBox(90, vm, nameof(vm.LanguageCode)), Se.Language.General.Language);
        var comboBoxSubtitleType = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.SubtitleTypes, vm, nameof(vm.SelectedSubtitleType)), l.ExportDvbTeletextSubtitleType, 180);

        var rows = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(l.ExportDvbTeletextPageNumber, numericPageNumber),
            FormatPropertiesChrome.Row(Se.Language.General.Language, textBoxLanguage),
            FormatPropertiesChrome.Row(l.ExportDvbTeletextSubtitleType, comboBoxSubtitleType));

        var buttonBar = EditorChrome.MakeDockedCancelOkBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), UiUtil.MakeButtonOk(vm.OkCommand));

        Content = FormatPropertiesChrome.MakeRoot(
            buttonBar,
            EditorChrome.MakeEditorSection(Se.Language.General.Settings, rows, first: true));

        UiUtil.FocusOnFirstActivation(this, numericPageNumber); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.OnKeyDown;
    }
}

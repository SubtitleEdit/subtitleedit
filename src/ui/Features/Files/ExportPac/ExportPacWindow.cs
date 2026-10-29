using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Files.FormatProperties;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportPac;

/// <summary>
/// PAC code pages in the editor look (see <see cref="FormatPropertiesChrome"/>).
/// </summary>
public class ExportPacWindow : Window
{
    public ExportPacWindow(ExportPacViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Shortcuts.FileExportPac;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Form);
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.File.Export;
        var comboBoxPacFormats = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.PacCodePages, vm, nameof(vm.SelectedPacCodePage)), l.ExportPacCodePage, 220);

        // lines in another script (e.g. Russian lines in a Hebrew subtitle) go in this code page
        var comboBoxSecondary = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.SecondaryPacCodePages, vm, nameof(vm.SelectedSecondaryPacCodePage)), l.ExportPacSecondaryCodePage, 220);

        var rows = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(l.ExportPacCodePage, comboBoxPacFormats),
            FormatPropertiesChrome.Row(l.ExportPacSecondaryCodePage, comboBoxSecondary));

        var buttonBar = EditorChrome.MakeDockedCancelOkBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), UiUtil.MakeButtonOk(vm.OkCommand));

        Content = FormatPropertiesChrome.MakeRoot(
            buttonBar,
            EditorChrome.MakeEditorSection(Se.Language.General.Encoding, rows, first: true));

        UiUtil.FocusOnFirstActivation(this, comboBoxPacFormats); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

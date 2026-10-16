using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Files.FormatProperties;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportCavena890;

/// <summary>
/// Cavena 890 export header in the editor look (see <see cref="FormatPropertiesChrome"/>).
/// </summary>
public class ExportCavena890Window : Window
{
    private const double FieldWidth = 300;

    public ExportCavena890Window(ExportCavena890ViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.TitleExportCavena890;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Form);
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.File.Export;
        var textBoxTranslatedTitle = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.TranslatedTitle)), l.ExportCavenaTranslatedTitle);
        var textBoxOriginalTitle = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.OriginalTitle)), l.ExportCavenaOriginalTitle);
        var textBoxTranslator = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.Translator)), l.ExportCavenaTranslator);
        var textBoxComment = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.Comment)), l.ExportCavenaComment);
        var textBoxLanguage = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.Language)), Se.Language.General.Language);
        var textBoxStartOfProgramme = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.StartOfProgramme)), l.ExportCavenaStartOfProgramme);

        var rows = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(l.ExportCavenaTranslatedTitle, textBoxTranslatedTitle),
            FormatPropertiesChrome.Row(l.ExportCavenaOriginalTitle, textBoxOriginalTitle),
            FormatPropertiesChrome.Row(l.ExportCavenaTranslator, textBoxTranslator),
            FormatPropertiesChrome.Row(l.ExportCavenaComment, textBoxComment),
            FormatPropertiesChrome.Row(Se.Language.General.Language, textBoxLanguage),
            FormatPropertiesChrome.Row(l.ExportCavenaStartOfProgramme, textBoxStartOfProgramme));

        var buttonBar = EditorChrome.MakeDockedCancelOkBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), UiUtil.MakeButtonOk(vm.OkCommand));

        Content = FormatPropertiesChrome.MakeRoot(
            buttonBar,
            EditorChrome.MakeEditorSection(Se.Language.General.Header, rows, first: true));

        UiUtil.FocusOnFirstActivation(this, textBoxTranslatedTitle); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.OnKeyDown;
    }
}

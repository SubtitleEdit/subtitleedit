using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.ItunesTimedTextProperties;

/// <summary>
/// iTunes Timed Text properties in the editor look (see <see cref="FormatPropertiesChrome"/>).
/// </summary>
public class ItunesTimedTextPropertiesWindow : Window
{
    private const double FieldWidth = 260;

    public ItunesTimedTextPropertiesWindow(ItunesTimedTextPropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.File.XProperties, "iTunes Timed Text");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 480;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Form);
        vm.Window = this;
        DataContext = vm;

        var textBoxTitle = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.Title)), Se.Language.General.Title);
        var textBoxDescription = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.Description)), Se.Language.General.Description);
        var textBoxLanguage = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.SelectedLanguage)), Se.Language.General.Language);
        var general = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(Se.Language.General.Title, textBoxTitle),
            FormatPropertiesChrome.Row(Se.Language.General.Description, textBoxDescription),
            FormatPropertiesChrome.Row(Se.Language.General.Language, textBoxLanguage));

        var comboBoxTimeBase = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.TimeBases, vm, nameof(vm.SelectedTimeBase)), Se.Language.File.PropertyTimeBase, FieldWidth);
        var textBoxFrameRate = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.SelectedFrameRate)), Se.Language.General.FrameRate);
        var textBoxFrameRateMultiplier = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.SelectedFrameRateMultiplier)), Se.Language.File.PropertyFrameRateMultiplier);
        var comboBoxDropMode = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.DropModes, vm, nameof(vm.SelectedDropMode)), Se.Language.File.PropertyDropMode, FieldWidth);
        var comboBoxTimeCodeFormat = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.TimeCodeFormats, vm, nameof(vm.SelectedTimeCodeFormat)), Se.Language.General.TimeCodeFormat, FieldWidth);
        var timing = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(Se.Language.File.PropertyTimeBase, comboBoxTimeBase),
            FormatPropertiesChrome.Row(Se.Language.General.FrameRate, textBoxFrameRate),
            FormatPropertiesChrome.Row(Se.Language.File.PropertyFrameRateMultiplier, textBoxFrameRateMultiplier),
            FormatPropertiesChrome.Row(Se.Language.File.PropertyDropMode, comboBoxDropMode),
            FormatPropertiesChrome.Row(Se.Language.General.TimeCodeFormat, comboBoxTimeCodeFormat));

        var comboBoxDefaultStyle = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox<string>(vm.DefaultStyles, vm, nameof(vm.SelectedDefaultStyle)), Se.Language.File.PropertyDefaultStyle, FieldWidth);
        var comboBoxDefaultRegion = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.DefaultRegions, vm, nameof(vm.SelectedDefaultRegion)), Se.Language.File.PropertyDefaultRegion, FieldWidth);
        var comboBoxStyleAttribute = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.StyleAttributes, vm, nameof(vm.SelectedStyleAttribute)), Se.Language.File.PropertyStyleAttributeName, FieldWidth);
        var style = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(Se.Language.File.PropertyDefaultStyle, comboBoxDefaultStyle),
            FormatPropertiesChrome.Row(Se.Language.File.PropertyDefaultRegion, comboBoxDefaultRegion),
            FormatPropertiesChrome.Row(Se.Language.File.PropertyStyleAttributeName, comboBoxStyleAttribute));

        // Alignment section: where the top and bottom regions are
        var textBoxTopOrigin = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.TopOrigin)), Se.Language.File.PropertyTopOrigin);
        var textBoxTopExtent = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.TopExtent)), Se.Language.File.PropertyTopExtent);
        var textBoxBottomOrigin = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.BottomOrigin)), Se.Language.File.PropertyBottomOrigin);
        var textBoxBottomExtent = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.BottomExtent)), Se.Language.File.PropertyBottomExtent);
        var alignment = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(Se.Language.File.PropertyTopOrigin, textBoxTopOrigin),
            FormatPropertiesChrome.Row(Se.Language.File.PropertyTopExtent, textBoxTopExtent),
            FormatPropertiesChrome.Row(Se.Language.File.PropertyBottomOrigin, textBoxBottomOrigin),
            FormatPropertiesChrome.Row(Se.Language.File.PropertyBottomExtent, textBoxBottomExtent));

        var buttonBar = EditorChrome.MakeDockedCancelOkBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), UiUtil.MakeButtonOk(vm.OkCommand));

        Content = FormatPropertiesChrome.MakeRoot(
            buttonBar,
            EditorChrome.MakeEditorSection(Se.Language.General.General, general, first: true),
            EditorChrome.MakeEditorSection(Se.Language.General.Timing, timing),
            EditorChrome.MakeEditorSection(Se.Language.General.Style, style),
            EditorChrome.MakeEditorSection(Se.Language.General.Alignment, alignment));

        UiUtil.FocusOnFirstActivation(this, textBoxTitle); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

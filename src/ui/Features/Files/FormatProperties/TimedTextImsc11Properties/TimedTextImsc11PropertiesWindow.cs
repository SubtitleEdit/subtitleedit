using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.TimedTextImsc11Properties;

/// <summary>
/// Timed Text IMSC 1.1 properties in the editor look (see <see cref="FormatPropertiesChrome"/>).
/// </summary>
public class TimedTextImsc11PropertiesWindow : Window
{
    private const double FieldWidth = 260;

    public TimedTextImsc11PropertiesWindow(TimedTextImsc11PropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.File.XProperties, "Timed Text IMSC 1.1");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 450;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Form);
        vm.Window = this;
        DataContext = vm;

        var textBoxTitle = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, nameof(vm.Title)), Se.Language.General.Title);
        var general = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(Se.Language.General.Title, textBoxTitle));

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
        var comboBoxFileExtension = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.FileExtensions, vm, nameof(vm.SelectedFileExtension)), Se.Language.General.FileExtension, FieldWidth);
        var style = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(Se.Language.File.PropertyDefaultStyle, comboBoxDefaultStyle),
            FormatPropertiesChrome.Row(Se.Language.File.PropertyDefaultRegion, comboBoxDefaultRegion),
            FormatPropertiesChrome.Row(Se.Language.General.FileExtension, comboBoxFileExtension));

        var buttonBar = EditorChrome.MakeDockedCancelOkBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), UiUtil.MakeButtonOk(vm.OkCommand));

        Content = FormatPropertiesChrome.MakeRoot(
            buttonBar,
            EditorChrome.MakeEditorSection(Se.Language.General.General, general, first: true),
            EditorChrome.MakeEditorSection(Se.Language.General.Timing, timing),
            EditorChrome.MakeEditorSection(Se.Language.General.Style, style));

        UiUtil.FocusOnFirstActivation(this, textBoxTitle); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

/// <summary>
/// The gear-button dialog for the export-to-images text effect. Not much lives here - the
/// view model writes every change into the export dialog's view model, which regenerates
/// the preview, so tuning is live while this window is open.
/// </summary>
public class TextEffectWindow : Window
{
    private readonly TextEffectViewModel _vm;

    public TextEffectWindow(TextEffectViewModel vm)
    {
        _vm = vm;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.TextEffectSettingsTitle;
        CanResize = false;
        Width = 440;
        // Height comes from the content - a fixed height clipped the button bar below the
        // visible area. Only the width is fixed (SizeToContent.WidthAndHeight measures too
        // wide on macOS).
        SizeToContent = SizeToContent.Height;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var comboPreset = UiUtil.MakeComboBox(vm.Presets, vm, nameof(vm.SelectedPreset));
        comboPreset.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboPreset, Se.Language.General.Preset);

        var sliders = new StackPanel { Spacing = 4 };
        sliders.Children.Add(MakeSliderRow(Se.Language.File.Export.TextEffectStrength, nameof(vm.Strength), 25, 300, "{0}%"));
        sliders.Children.Add(MakeSliderRow(Se.Language.File.Export.TextEffectLetterSpacing, nameof(vm.LetterSpacing), 0, 100, "{0}"));
        sliders.Children.Add(MakeSliderRow(Se.Language.File.Export.TextEffectArcBend, nameof(vm.ArcBend), -100, 100, "{0}"));
        sliders.Children.Add(MakeSliderRow(Se.Language.File.Export.TextEffectWave, nameof(vm.Wave), 0, 100, "{0}"));

        var sections = new StackPanel();
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Preset, comboPreset, first: true));
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Settings, sliders));

        var buttonReset = EditorChrome.MakeTextToolButton("fa-solid fa-rotate-left", Se.Language.General.Reset, vm.ResetCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(buttonReset, buttonCancel, buttonOk));

        KeyDown += (_, e) => vm.OnKeyDown(e);
        UiUtil.FocusOnFirstActivation(this, comboPreset);
    }

    // The settings are pushed into the export dialog live, so a close that is not an OK -
    // including the title bar X and Alt+F4, which never go through the Cancel command - must
    // behave exactly like Cancel and put the original values back.
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnWindowClosing();
    }

    private static Grid MakeSliderRow(string label, string propertyName, double min, double max, string valueFormat)
    {
        var labelControl = EditorChrome.MakeDimLabel(label, new Thickness(0));
        labelControl.TextTrimming = TextTrimming.CharacterEllipsis;

        var slider = new Slider
        {
            Minimum = min,
            Maximum = max,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!Slider.ValueProperty] = new Binding(propertyName),
        };
        AutomationProperties.SetName(slider, label);

        var valueLabel = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Right,
            MinWidth = 45,
            [!TextBlock.TextProperty] = new Binding(propertyName) { StringFormat = valueFormat },
        };

        var grid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(110)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        grid.Add(labelControl, 0, 0);
        grid.Add(slider, 0, 1);
        grid.Add(valueLabel, 0, 2);
        return grid;
    }
}

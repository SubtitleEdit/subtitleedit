using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
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
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        var labelPreset = UiUtil.MakeLabel(Se.Language.File.Export.TextEffectPreset);
        var comboPreset = UiUtil.MakeComboBox(vm.Presets, vm, nameof(vm.SelectedPreset));
        grid.Add(labelPreset, 0, 0);
        grid.Add(comboPreset, 0, 1);

        AddSliderRow(grid, 1, Se.Language.File.Export.TextEffectStrength, nameof(vm.Strength), 25, 300, "{0}%");
        AddSliderRow(grid, 2, Se.Language.File.Export.TextEffectLetterSpacing, nameof(vm.LetterSpacing), 0, 100, "{0}");
        AddSliderRow(grid, 3, Se.Language.File.Export.TextEffectArcBend, nameof(vm.ArcBend), -100, 100, "{0}");
        AddSliderRow(grid, 4, Se.Language.File.Export.TextEffectWave, nameof(vm.Wave), 0, 100, "{0}");

        var buttonReset = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonReset, buttonOk, buttonCancel);
        grid.Add(panelButtons, 5, 0, 1, 3);

        Content = grid;

        KeyDown += (_, e) => vm.OnKeyDown(e);
        UiUtil.FocusOnFirstActivation(this, buttonOk);
    }

    // The settings are pushed into the export dialog live, so a close that is not an OK -
    // including the title bar X and Alt+F4, which never go through the Cancel command - must
    // behave exactly like Cancel and put the original values back.
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnWindowClosing();
    }

    private static void AddSliderRow(Grid grid, int row, string label, string propertyName, double min, double max, string valueFormat)
    {
        var labelControl = UiUtil.MakeLabel(label);
        labelControl.VerticalAlignment = VerticalAlignment.Center;

        var slider = new Slider
        {
            Minimum = min,
            Maximum = max,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!Slider.ValueProperty] = new Binding(propertyName),
        };

        // A real TextBlock - UiUtil.MakeLabel returns a Label, and a TextBlock.Text binding
        // set on a Label never renders.
        var valueLabel = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 45,
            [!TextBlock.TextProperty] = new Binding(propertyName) { StringFormat = valueFormat },
        };

        grid.Add(labelControl, row, 0);
        grid.Add(slider, row, 1);
        grid.Add(valueLabel, row, 2);
    }
}

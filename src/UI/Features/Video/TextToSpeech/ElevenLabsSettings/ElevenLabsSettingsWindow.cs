using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;

public class ElevenLabsSettingsWindow : Window
{
    private readonly ElevenLabsSettingsViewModel _vm;

    public ElevenLabsSettingsWindow(ElevenLabsSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.ElevenLabsSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelStability = UiUtil.MakeLabel(Se.Language.Video.TextToSpeech.Stability);
        var sliderStability = new Slider
        {
            Minimum = 0,
            Maximum = 1,
            Value = vm.Stability,
            Width = 200,
            Margin = new Thickness(5, 0, 0, 0),
            [!Slider.ValueProperty] = new Binding(nameof(ElevenLabsSettingsViewModel.Stability)),
        };
        var labelStabilityValue = UiUtil.MakeLabel().WithBindText(vm, nameof(ElevenLabsSettingsViewModel.Stability), new DoubleToTwoDecimalConverter());
        var buttonStability = UiUtil.MakeButton(vm.ShowStabilityHelpCommand, IconNames.Help);

        var labelSimilarity = UiUtil.MakeLabel(Se.Language.Video.TextToSpeech.Similarity);
        var sliderSimilarity = new Slider
        {
            Minimum = 0,
            Maximum = 1,
            Value = vm.Similarity,
            Width = 200,
            Margin = new Thickness(5, 0, 0, 0),
            [!Slider.ValueProperty] = new Binding(nameof(ElevenLabsSettingsViewModel.Similarity)),
        };
        var labelSimilarityValue = UiUtil.MakeLabel().WithBindText(vm, nameof(ElevenLabsSettingsViewModel.Similarity), new DoubleToTwoDecimalConverter());
        var buttonSimilarity = UiUtil.MakeButton(vm.ShowSimilarityHelpCommand, IconNames.Help);

        var labelSpeakerBoost = UiUtil.MakeLabel(Se.Language.Video.TextToSpeech.SpeakerBoost);
        var sliderSpeakerBoost = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Value = vm.SpeakerBoost,
            Width = 200,
            Margin = new Thickness(5, 0, 0, 0),
            [!Slider.ValueProperty] = new Binding(nameof(ElevenLabsSettingsViewModel.SpeakerBoost)),
        };
        var labelSpeakerBoostValue = UiUtil.MakeLabel().WithBindText(vm, nameof(ElevenLabsSettingsViewModel.SpeakerBoost), new DoubleToTwoDecimalConverter());
        var buttonSpeakerBoost = UiUtil.MakeButton(vm.ShowSpeakerBoostHelpCommand, IconNames.Help);

        var labelSpeed = UiUtil.MakeLabel(Se.Language.General.Speed);
        var sliderSpeed = new Slider
        {
            Minimum = 0.7,
            Maximum = 1.2,
            Value = vm.Speed,
            Width = 200,
            Margin = new Thickness(5, 0, 0, 0),
            [!Slider.ValueProperty] = new Binding(nameof(ElevenLabsSettingsViewModel.Speed)),
        };
        var labelSpeedValue = UiUtil.MakeLabel().WithBindText(vm, nameof(ElevenLabsSettingsViewModel.Speed), new DoubleToTwoDecimalConverter());
        var buttonSpeed = UiUtil.MakeButton(vm.ShowSpeedHelpCommand, IconNames.Help);

        var labelStyleExaggeration = UiUtil.MakeLabel(Se.Language.General.StyleExaggeration);
        var sliderStyleExaggeration = new Slider
        {
            Minimum = 0.0,
            Maximum = 1.0,
            Value = vm.StyleExaggeration,
            Width = 200,
            Margin = new Thickness(5, 0, 0, 0),
            [!Slider.ValueProperty] = new Binding(nameof(ElevenLabsSettingsViewModel.StyleExaggeration)),
        };
        var labelStyleExaggerationValue = UiUtil.MakeLabel().WithBindText(vm, nameof(ElevenLabsSettingsViewModel.StyleExaggeration), new DoubleToTwoDecimalConverter());
        var buttonStyleExaggeration = UiUtil.MakeButton(vm.ShowStyleExaggerationHelpCommand, IconNames.Help);

        var buttonWeb = UiUtil.MakeButton(Se.Language.General.MoreInfo, vm.ShowMoreOnWebCommand).WithIconLeft(IconNames.Web);
        var buttonReset = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetCommand).WithIconLeft(IconNames.Repeat);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonWeb, buttonReset, buttonOk, buttonCancel);

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelStability, 0, 0);
        grid.Add(sliderStability, 0, 1);
        grid.Add(labelStabilityValue, 0, 2);
        grid.Add(buttonStability, 0, 3);

        grid.Add(labelSimilarity, 1, 0);
        grid.Add(sliderSimilarity, 1, 1);
        grid.Add(labelSimilarityValue, 1, 2);
        grid.Add(buttonSimilarity, 1, 3);

        grid.Add(labelSpeakerBoost, 2, 0);
        grid.Add(sliderSpeakerBoost, 2, 1);
        grid.Add(labelSpeakerBoostValue, 2, 2);
        grid.Add(buttonSpeakerBoost, 2, 3);

        grid.Add(labelSpeed, 3, 0);
        grid.Add(sliderSpeed, 3, 1);
        grid.Add(labelSpeedValue, 3, 2);
        grid.Add(buttonSpeed, 3, 3);

        grid.Add(labelStyleExaggeration, 4, 0);
        grid.Add(sliderStyleExaggeration, 4, 1);
        grid.Add(labelStyleExaggerationValue, 4, 2);
        grid.Add(buttonStyleExaggeration, 4, 3);

        grid.Add(panelButtons, 5, 0, 1, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.OpenAiCompatibleSettings;

public class OpenAiCompatibleSettingsWindow : Window
{
    private readonly OpenAiCompatibleSettingsViewModel _vm;

    public OpenAiCompatibleSettingsWindow(OpenAiCompatibleSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.OpenAiCompatibleSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Video.TextToSpeech;
        const double inputWidth = 400;

        var textBoxUrl = UiUtil.MakeTextBox(inputWidth, vm, nameof(vm.CustomUrl));
        var textBoxModels = UiUtil.MakeTextBox(inputWidth, vm, nameof(vm.CustomModels));
        var textBoxVoices = UiUtil.MakeTextBox(inputWidth, vm, nameof(vm.CustomVoices));
        var textBoxInstructions = UiUtil.MakeTextBox(inputWidth, vm, nameof(vm.Instructions));
        textBoxInstructions.AcceptsReturn = true;
        textBoxInstructions.TextWrapping = TextWrapping.Wrap;
        textBoxInstructions.Height = 70;
        var numericSpeed = UiUtil.MakeNumericUpDownTwoDecimals(0.25m, 4.0m, 120, vm, nameof(vm.Speed), defaultValue: 1.0m);
        var comboBoxResponseFormat = UiUtil.MakeComboBox(vm.ResponseFormats, vm, nameof(vm.SelectedResponseFormat));
        comboBoxResponseFormat.MinWidth = 120;

        var buttonWeb = UiUtil.MakeButton(Se.Language.General.MoreInfo, vm.ShowMoreOnWebCommand).WithIconLeft(IconNames.Web);
        var buttonReset = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetCommand).WithIconLeft(IconNames.Repeat);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonWeb, buttonReset, buttonOk, buttonCancel);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 8,
            RowSpacing = 10,
        };

        var row = 0;
        AddRow(grid, ref row, l.CustomServerUrl, textBoxUrl, l.CustomServerUrlHint);
        AddRow(grid, ref row, l.CustomModels, textBoxModels, l.CustomModelsHint);
        AddRow(grid, ref row, l.CustomVoices, textBoxVoices, l.CustomVoicesHint);
        AddRow(grid, ref row, l.Instructions, textBoxInstructions, l.InstructionsHint);
        AddRow(grid, ref row, Se.Language.General.Speed, numericSpeed, l.OpenAiSpeedHint);
        AddRow(grid, ref row, l.OutputFormat, comboBoxResponseFormat, l.OutputFormatHint);

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.Add(panelButtons, row, 0, 1, 3);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxUrl); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    private static void AddRow(Grid grid, ref int row, string label, Control input, string hint)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var labelControl = UiUtil.MakeLabel(label);
        labelControl.VerticalAlignment = input is TextBox { AcceptsReturn: true } ? VerticalAlignment.Top : VerticalAlignment.Center;
        grid.Add(labelControl, row, 0);
        grid.Add(input, row, 1);
        var hintIcon = UiUtil.MakeHintIcon(hint, input);
        hintIcon.VerticalAlignment = labelControl.VerticalAlignment;
        grid.Add(hintIcon, row, 2);
        row++;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

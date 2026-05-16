using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class LlamaCppOcrSettingsWindow : Window
{
    public LlamaCppOcrSettingsWindow(LlamaCppOcrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.LlamaCppOcrSettingsTitle;
        Width = 640;
        Height = 320;
        MinWidth = 480;
        MinHeight = 240;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        vm.Window = this;
        DataContext = vm;

        var labelUrl = UiUtil.MakeTextBlock(Se.Language.General.Url);
        var textBoxUrl = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            DataContext = vm,
        };
        textBoxUrl.Bind(TextBox.TextProperty, new Binding(nameof(vm.Url))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        var labelTimeout = UiUtil.MakeTextBlock(Se.Language.Ocr.LlamaCppOcrTimeoutMinutes);
        var numericTimeout = UiUtil.MakeNumericUpDownInt(1, 120, 5, 100, vm, nameof(vm.TimeoutMinutes));

        var labelPrompt = UiUtil.MakeTextBlock(Se.Language.General.OpenAiCompatibleSttPrompt);
        var textBoxPrompt = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = false,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            DataContext = vm,
        };
        textBoxPrompt.Bind(TextBox.TextProperty, new Binding(nameof(vm.Prompt))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        var hintPrompt = UiUtil.MakeTextBlock(Se.Language.Ocr.LlamaCppOcrPromptHint);
        hintPrompt.Opacity = 0.7;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 6,
        };

        grid.Add(labelUrl, 0, 0);
        grid.Add(textBoxUrl, 1, 0);
        grid.Add(labelTimeout, 2, 0);
        grid.Add(numericTimeout, 3, 0);
        grid.Add(labelPrompt, 4, 0);
        grid.Add(textBoxPrompt, 5, 0);
        grid.Add(hintPrompt, 6, 0);
        grid.Add(buttonBar, 7, 0);

        Content = grid;

        Activated += delegate { textBoxUrl.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

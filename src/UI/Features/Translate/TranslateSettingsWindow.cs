using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate;

public class TranslateSettingsWindow : Window
{
    private readonly TranslateSettingsViewModel _vm;

    public TranslateSettingsWindow(TranslateSettingsViewModel vm)
    {
        _vm = vm;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Settings;
        Width = 750;
        MinWidth = 600;
        Height = 400;
        MinHeight = 350;
        DataContext = vm;
        vm.Window = this;

        if (!vm.PromptIsVisible)
        {
            Width = MinWidth = 400;
            Height = MinHeight = 220;
            CanResize = false;
        }

        var labelMerge = UiUtil.MakeTextBlock("Line merge");
        var comboMerge = UiUtil.MakeComboBox(vm.MergeOptions, vm, nameof(vm.SelectedMergeOptions));

        var labelDelay = UiUtil.MakeTextBlock("Delay in seconds between requests");
        var delayNumericUpDown = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 60,
            Value = vm.ServerDelaySeconds,
            Width = 150,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Increment = 1,
            FormatString = "#,###,##0",
        };
        delayNumericUpDown.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.ServerDelaySeconds),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });

        var labelMaxBytes = UiUtil.MakeTextBlock("Max bytes per request");
        var maxBytesNumericUpDown = new NumericUpDown //TODO: UiUtil.MakeNumericUpDown
        {
            Minimum = 0,
            Maximum = 100_000,
            Value = 1000,
            Width = 150,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Increment = 100,
            FormatString = "#,###,##0",
        };
        maxBytesNumericUpDown.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.MaxBytesRequest),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });

        var labelPrompt = UiUtil.MakeTextBlock("Prompt text", vm, null, nameof(vm.PromptIsVisible));
        var promptTextBox = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = true,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        }.BindIsVisible(vm, nameof(vm.PromptIsVisible))
            .BindText(vm, nameof(vm.PromptText));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            Margin = new Thickness(UiUtil.WindowMarginWidth),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        var row = 0;
        grid.Children.Add(labelMerge);
        Grid.SetRow(labelMerge, row);
        Grid.SetColumn(labelMerge, 0);
        grid.Children.Add(comboMerge);
        Grid.SetRow(comboMerge, row);
        Grid.SetColumn(comboMerge, 1);
        row++;

        grid.Children.Add(labelDelay);
        Grid.SetRow(labelDelay, row);
        Grid.SetColumn(labelDelay, 0);
        grid.Children.Add(delayNumericUpDown);
        Grid.SetRow(delayNumericUpDown, row);
        Grid.SetColumn(delayNumericUpDown, 1);
        row++;

        grid.Children.Add(labelMaxBytes);
        Grid.SetRow(labelMaxBytes, row);
        Grid.SetColumn(labelMaxBytes, 0);
        grid.Children.Add(maxBytesNumericUpDown);
        Grid.SetRow(maxBytesNumericUpDown, row);
        Grid.SetColumn(maxBytesNumericUpDown, 1);
        row++;

        grid.Children.Add(labelPrompt);
        Grid.SetRow(labelPrompt, row);
        Grid.SetColumn(labelPrompt, 0);
        Grid.SetColumnSpan(labelPrompt, 2);
        row++;

        grid.Children.Add(promptTextBox);
        Grid.SetRow(promptTextBox, row);
        Grid.SetColumn(promptTextBox, 0);
        Grid.SetColumnSpan(promptTextBox, 2);
        row++;

        grid.Children.Add(buttonBar);
        Grid.SetRow(buttonBar, row);
        Grid.SetColumn(buttonBar, 0);
        Grid.SetColumnSpan(buttonBar, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
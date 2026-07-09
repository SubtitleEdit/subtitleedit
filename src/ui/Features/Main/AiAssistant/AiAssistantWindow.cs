using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.AiAssistant;

public class AiAssistantWindow : Window
{
    public AiAssistantWindow(AiAssistantViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        var l = Se.Language.Tools.AiAssistant;
        Title = l.Title;
        Width = 720;
        Height = 620;
        MinWidth = 520;
        MinHeight = 460;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            Margin = new Thickness(15),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,*,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };

        var currentLabel = UiUtil.MakeTextBlock(l.CurrentLine);
        var currentBox = MakeReadonlyMultiline(vm, nameof(vm.CurrentText), 60);

        var contextLabel = UiUtil.MakeTextBlock(l.ContextLines);
        var contextBox = MakeReadonlyMultiline(vm, nameof(vm.ContextText), 70);

        var actionPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 8, 0, 0),
        };
        actionPanel.Children.Add(MakeActionButton(l.FixErrors, vm.FixErrorsCommand));
        actionPanel.Children.Add(MakeActionButton(l.FitReadingSpeed, vm.FitReadingSpeedCommand));
        actionPanel.Children.Add(MakeActionButton(l.MakeFormal, vm.MakeFormalCommand));
        actionPanel.Children.Add(MakeActionButton(l.MakeInformal, vm.MakeInformalCommand));

        var questionBox = UiUtil.MakeTextBox(420, vm, nameof(vm.QuestionText));
        questionBox.Watermark = l.AskPlaceholder;
        var askButton = UiUtil.MakeButton(l.Ask, vm.AskCommand).Compact();
        var questionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(0, 8, 0, 0),
            Children = { questionBox, askButton },
        };

        var resultLabel = UiUtil.MakeTextBlock(l.Result);
        resultLabel.Margin = new Thickness(0, 8, 0, 0);
        var resultBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            MinHeight = 120,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        resultBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.ResultText)) { Mode = BindingMode.TwoWay });

        var statusText = UiUtil.MakeTextBlock(string.Empty);
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusText)));
        statusText.Margin = new Thickness(0, 6, 0, 0);

        var applyButton = UiUtil.MakeButton(l.Apply, vm.ApplyCommand);
        var closeButton = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Margin = new Thickness(0, 12, 0, 0),
            Children = { applyButton, closeButton },
        };

        grid.Add(currentLabel, 0, 0);
        grid.Add(currentBox, 1, 0);
        grid.Add(contextLabel, 2, 0);
        grid.Add(contextBox, 3, 0);
        grid.Add(actionPanel, 4, 0);
        grid.Add(questionPanel, 5, 0);
        grid.Add(resultLabel, 6, 0);
        grid.Add(resultBox, 8, 0);
        grid.Add(statusText, 9, 0);
        grid.Add(buttonPanel, 10, 0);

        Content = grid;
    }

    private static TextBox MakeReadonlyMultiline(AiAssistantViewModel vm, string path, double minHeight)
    {
        var box = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            MinHeight = minHeight,
            Margin = new Thickness(0, 2, 0, 0),
        };
        box.Bind(TextBox.TextProperty, new Binding(path));
        return box;
    }

    private static Button MakeActionButton(string text, CommunityToolkit.Mvvm.Input.IRelayCommand command)
    {
        var button = UiUtil.MakeButton(text, command).Compact();
        button.Margin = new Thickness(0, 0, 6, 6);
        return button;
    }
}

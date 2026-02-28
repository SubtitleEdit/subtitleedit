using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class PromptUnknownWordWindow : Window
{
    public PromptUnknownWordWindow(PromptUnknownWordViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.SpellCheck;
        CanResize = true;
        Width = 800;
        Height = 600;
        MinWidth = 700;
        MinHeight = 500;
        vm.Window = this;
        DataContext = vm;

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Abort, vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // image
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // whole text
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // word + suggestions
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons (abort)
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MaxHeight = 200,
            DataContext = vm,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.Bitmap)));

        grid.Add(image, 0, 0, 1, 2);
        grid.Add(MakeWholeTextView(vm), 1, 0, 1, 2);
        grid.Add(MakeWordView(vm), 2);
        grid.Add(MakeWordSuggestionsView(vm), 2, 1);
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { vm.TextBoxWord.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
    }

    private static Grid MakeWholeTextView(PromptUnknownWordViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
        };

        vm.TextBoxWholeText.Bind(TextBox.TextProperty, new Binding(nameof(vm.WholeText)) { Mode = BindingMode.TwoWay });
        vm.TextBoxWholeText
            .WithHorizontalAlignmentStretch()
            .WithHeight(88)
            .WithBindIsVisible(nameof(vm.DoEditWholeText));

        vm.PanelWholeText.Width = double.NaN;
        vm.PanelWholeText.HorizontalAlignment = HorizontalAlignment.Left;
        vm.PanelWholeText.VerticalAlignment = VerticalAlignment.Top;
        vm.PanelWholeText.Orientation = Orientation.Horizontal;
        vm.PanelWholeText.Margin = new Thickness(10, 6, 10, 6);
        var scrollViewerWholeText = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = vm.PanelWholeText,
            Height = 85,
        };
        var scrollViewerWholeTextBorder = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderBrush(),
            Child = scrollViewerWholeText,
        };
        scrollViewerWholeText.Bind(Border.IsVisibleProperty, new Binding(nameof(vm.DoEditWholeText))
        {
            Converter = new InverseBooleanConverter(),
        });

        var buttonEditWholeText = new ToggleButton
        {
            Content = Se.Language.Ocr.EditWholeText,
            DataContext = vm,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.DoEditWholeText)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(3, 0, 0, 0),
        };
        buttonEditWholeText.Click += (_, _) => vm.OnEditWholeTextClicked();

        var panelButtons = UiUtil.MakeVerticalPanel(buttonEditWholeText);

        grid.Add(scrollViewerWholeTextBorder, 0);
        grid.Add(vm.TextBoxWholeText, 0);
        grid.Add(panelButtons, 0, 1);

        return grid;
    }

    private static Grid MakeWordView(PromptUnknownWordViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            RowSpacing = 5,
            ColumnSpacing = 5,
        };

        vm.TextBoxWord
            .WithHorizontalAlignmentStretch()
            .WithBindEnabled(nameof(vm.DoEditWholeText), new InverseBooleanConverter())
            .Bind(TextBox.TextProperty, new Binding(nameof(vm.Word)) { Mode = BindingMode.TwoWay });
        var buttonChangeAll = UiUtil.MakeButton(Se.Language.General.ChangeAll, vm.ChangeAllCommand)
            .WithHorizontalAlignmentStretch()
            .WithBindEnabled(nameof(vm.DoEditWholeText), new InverseBooleanConverter());
        var buttonChangeOnce = UiUtil.MakeButton(Se.Language.General.ChangeOnce, vm.ChangeOnceCommand)
            .WithHorizontalAlignmentStretch();
        var buttonSkipOne = UiUtil.MakeButton(Se.Language.General.SkipOnce, vm.SkipOnceCommand)
            .WithHorizontalAlignmentStretch()
            .WithBindEnabled(nameof(vm.DoEditWholeText), new InverseBooleanConverter());
        var buttonGoogleIt = UiUtil.MakeButton(Se.Language.General.GoogleIt, vm.GoogleItCommand)
            .WithHorizontalAlignmentStretch()
            .WithBindEnabled(nameof(vm.DoEditWholeText), new InverseBooleanConverter());
        var buttonSkipAll = UiUtil.MakeButton(Se.Language.General.SkipAll, vm.SkipAllCommand)
            .WithHorizontalAlignmentStretch()
            .WithBindEnabled(nameof(vm.DoEditWholeText), new InverseBooleanConverter());
        var buttonAddToNameList = UiUtil.MakeButton(Se.Language.General.AddToNamesListCaseSensitive, vm.AddToNamesListCommand)
            .WithHorizontalAlignmentStretch()
            .WithBindEnabled(nameof(vm.DoEditWholeText), new InverseBooleanConverter());
        var buttonAddToUserDictionary = UiUtil.MakeButton(Se.Language.General.AddToUserDictionary, vm.AddToUserDictionaryCommand)
            .WithHorizontalAlignmentStretch()
            .WithBindEnabled(nameof(vm.DoEditWholeText), new InverseBooleanConverter());

        grid.Add(vm.TextBoxWord, 0, 0, 1, 2);
        grid.Add(buttonChangeAll, 1, 0, 1, 2);
        grid.Add(buttonChangeOnce, 2, 0);
        grid.Add(buttonSkipOne, 2, 1);
        grid.Add(buttonGoogleIt, 3, 0);
        grid.Add(buttonSkipAll, 3, 1);
        grid.Add(buttonAddToNameList, 4, 0, 1, 2);
        grid.Add(buttonAddToUserDictionary, 5, 0, 1, 2);

        return grid;
    }

    private static Grid MakeWordSuggestionsView(PromptUnknownWordViewModel vm)
    {
        var labelSuggestions = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            Content = Se.Language.General.Suggestions,
            Margin = new Thickness(0, 10, 0, 0),
        };

        var buttonUseOnce = new Button
        {
            Content = Se.Language.General.UseOnce,
            Command = vm.SuggestionUseCommand,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        }.WithBindEnabled(nameof(vm.AreSuggestionsEnabled));

        var buttonUseAlways = new Button
        {
            Content = Se.Language.General.UseAlways,
            Command = vm.SuggestionUseAlwaysCommand,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        }.WithBindEnabled(nameof(vm.AreSuggestionsEnabled));

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 10),
            Spacing = 5,
            Children =
            {
                buttonUseOnce,
                buttonUseAlways,
            }
        };

        var listBoxSuggestions = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(SpellCheckViewModel.Suggestions)) { Source = vm, Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(SpellCheckViewModel.SelectedSuggestion)) { Source = vm, Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Top,
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = new SolidColorBrush(Colors.Transparent),
        };
        listBoxSuggestions.DoubleTapped += vm.ListBoxSuggestionsDoubleTapped;
        listBoxSuggestions.SelectionChanged += vm.ListBoxSuggestionsSelectionChanged;

        var scrollViewSuggestions = new ScrollViewer
        {
            Content = listBoxSuggestions,
            Height = 160,
        };

        var borderSuggestions = UiUtil.MakeBorderForControl(scrollViewSuggestions);


        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
        };

        grid.Add(labelSuggestions, 0, 0);
        grid.Add(borderSuggestions, 1, 0);
        grid.Add(panelButtons, 2, 0);

        return grid;
    }
}

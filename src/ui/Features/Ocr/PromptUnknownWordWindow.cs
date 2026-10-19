using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Ocr;

/// <summary>
/// Unknown word found during OCR, in the editor look of spell check (see <see cref="EditorChrome"/>):
/// the image and the line on top, the word with its actions on the left, the suggestions on the
/// right, Abort in the button bar.
/// </summary>
public class PromptUnknownWordWindow : Window
{
    public PromptUnknownWordWindow(PromptUnknownWordViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.SpellCheck;
        CanResize = true;
        Width = 800;
        Height = 640;
        MinWidth = 700;
        MinHeight = 560;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            MaxHeight = 200,
            DataContext = vm,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.Bitmap)));

        // subtitle bitmaps are light or dark text on transparent pixels, so without a backdrop they
        // vanish into the window - white text on the light theme (#15111)
        var imageBackdrop = BinaryOcrChrome.MakeImageFrame(image, ImagePreviewBackground.CreateBrush());
        imageBackdrop.HorizontalAlignment = HorizontalAlignment.Center;
        imageBackdrop.VerticalAlignment = VerticalAlignment.Center;
        var imageSection = new Border
        {
            Child = imageBackdrop,
            Padding = new Thickness(14, 14, 14, 0),
        };

        var columns = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        columns.Add(MakeWordView(vm), 0, 0);
        var suggestions = MakeWordSuggestionsView(vm);
        suggestions.BorderThickness = new Thickness(1, 1, 0, 0);
        columns.Add(suggestions, 0, 1);

        // The image takes what is left, so the word and its actions always fit
        var body = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // image
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // whole text
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // word + suggestions
            },
        };
        body.Add(imageSection, 0, 0);
        body.Add(MakeWholeTextView(vm), 1, 0);
        body.Add(columns, 2, 0);

        var buttonCancel = EditorChrome.WithoutMargin(UiUtil.MakeButton(Se.Language.General.Abort, vm.CancelCommand));
        Content = EditorChrome.MakeRoot(body, EditorChrome.MakeDockedButtonBar(null, buttonCancel));

        UiUtil.FocusOnFirstActivation(this, vm.TextBoxWord); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
    }

    private static Border MakeWholeTextView(PromptUnknownWordViewModel vm)
    {
        vm.TextBoxWholeText.Bind(TextBox.TextProperty, new Binding(nameof(vm.WholeText)) { Mode = BindingMode.TwoWay });
        vm.TextBoxWholeText.WithAccessibleName(Se.Language.General.EditWholeText);
        vm.TextBoxWholeText.Classes.Add("text");
        vm.TextBoxWholeText
            .WithHorizontalAlignmentStretch()
            .WithHeight(88)
            .WithBindIsVisible(nameof(vm.DoEditWholeText));
        vm.TextBoxWholeText.VerticalContentAlignment = VerticalAlignment.Top; // 88 high, the text starts at the top
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            vm.TextBoxWholeText.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        vm.PanelWholeText.Width = double.NaN;
        vm.PanelWholeText.HorizontalAlignment = HorizontalAlignment.Left;
        vm.PanelWholeText.VerticalAlignment = VerticalAlignment.Top;
        vm.PanelWholeText.Orientation = Orientation.Horizontal;
        vm.PanelWholeText.Margin = new Thickness(10, 8, 10, 8);
        var scrollViewerWholeText = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = vm.PanelWholeText,
            Height = 86,
        };
        var scrollViewerWholeTextBorder = EditorChrome.MakeListFrame(scrollViewerWholeText, clearListBackground: true);
        scrollViewerWholeTextBorder.Bind(Border.IsVisibleProperty, new Binding(nameof(vm.DoEditWholeText))
        {
            Converter = InverseBooleanConverter.Instance,
        });

        // Toggles between the line with the word highlighted and a text box with the whole line
        var buttonEditWholeText = new ToggleButton
        {
            DataContext = vm,
            [!ContentControl.ContentProperty] = new Binding(nameof(vm.EditButtonLabel)),
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.DoEditWholeText)) { Mode = BindingMode.TwoWay },
            [!AutomationProperties.NameProperty] = new Binding(nameof(vm.EditButtonLabel)),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12.5,
            MinHeight = 28,
            Padding = new Thickness(10, 4),
            CornerRadius = new CornerRadius(6),
        };
        buttonEditWholeText.Click += (_, _) => vm.OnEditWholeTextClicked();

        var panel = new Panel();
        panel.Children.Add(scrollViewerWholeTextBorder);
        panel.Children.Add(vm.TextBoxWholeText);

        var section = EditorChrome.MakeEditorSection(Se.Language.General.Text, panel, buttonEditWholeText);
        section.BorderThickness = new Thickness(0);
        return section;
    }

    private static Border MakeWordView(PromptUnknownWordViewModel vm)
    {
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            RowSpacing = 6,
            ColumnSpacing = 6,
        };

        // The word box gets the first focus: the window keys (Esc, F1) work from the start, and a
        // bare Space types a space instead of clicking a focused action button (#15028)
        vm.TextBoxWord.Classes.Add("text");
        vm.TextBoxWord
            .WithHorizontalAlignmentStretch()
            .WithBindEnabled(nameof(vm.DoEditWholeText), InverseBooleanConverter.Instance)
            .Bind(TextBox.TextProperty, new Binding(nameof(vm.Word)) { Mode = BindingMode.TwoWay });
        vm.TextBoxWord.WithAccessibleName(Se.Language.General.Text);
        vm.TextBoxWord.Margin = new Thickness(0, 0, 0, 4);
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            vm.TextBoxWord.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }
        var buttonChangeAll = MakeActionButton(Se.Language.General.ChangeAll, IconNames.FileReplaceOutline, vm.ChangeAllCommand)
            .WithBindIsVisible(nameof(vm.DoEditWholeText), InverseBooleanConverter.Instance);
        var buttonChangeOnce = MakeActionButton(Se.Language.General.ChangeOnce, IconNames.FindReplace, vm.ChangeOnceCommand);
        var buttonSkipOne = MakeActionButton(Se.Language.General.SkipOnce, IconNames.SkipNext, vm.SkipOnceCommand);
        var buttonGoogleIt = MakeActionButton(Se.Language.General.GoogleIt.Trim(), IconNames.Web, vm.GoogleItCommand)
            .WithBindIsVisible(nameof(vm.DoEditWholeText), InverseBooleanConverter.Instance);
        var buttonSkipAll = MakeActionButton(Se.Language.General.SkipAll, IconNames.ChevronDoubleRight, vm.SkipAllCommand)
            .WithBindIsVisible(nameof(vm.DoEditWholeText), InverseBooleanConverter.Instance);
        var buttonAddToNameList = MakeActionButton(Se.Language.General.AddToNamesListCaseSensitive, IconNames.Account, vm.AddToNamesListCommand)
            .WithBindIsVisible(nameof(vm.DoEditWholeText), InverseBooleanConverter.Instance);
        var buttonAddToUserDictionary = MakeActionButton(Se.Language.General.AddToUserDictionary, IconNames.BookAlphabet, vm.AddToUserDictionaryCommand)
            .WithBindIsVisible(nameof(vm.DoEditWholeText), InverseBooleanConverter.Instance);

        grid.Add(vm.TextBoxWord, 0, 0, 1, 2);
        grid.Add(buttonChangeAll, 1, 0, 1, 2);
        grid.Add(buttonChangeOnce, 2, 0);
        grid.Add(buttonSkipOne, 2, 1);
        grid.Add(buttonGoogleIt, 3, 0);
        grid.Add(buttonSkipAll, 3, 1);
        grid.Add(buttonAddToNameList, 4, 0, 1, 2);
        grid.Add(buttonAddToUserDictionary, 5, 0, 1, 2);

        return EditorChrome.MakeEditorSection(Se.Language.SpellCheck.WordNotFound, grid);
    }

    private static Button MakeActionButton(string text, string icon, System.Windows.Input.ICommand? command)
    {
        var button = new Button
        {
            Content = text,
            Command = command,
        }.WithIconLeft(icon);
        return EditorChrome.StyleActionColumnButton(button);
    }

    private static Border MakeWordSuggestionsView(PromptUnknownWordViewModel vm)
    {
        var buttonUseOnce = MakeActionButton(Se.Language.General.UseOnce, IconNames.Check, vm.SuggestionUseCommand)
            .WithBindEnabled(nameof(vm.AreSuggestionsEnabled));
        var buttonUseAlways = MakeActionButton(Se.Language.General.UseAlways, IconNames.CheckBold, vm.SuggestionUseAlwaysCommand)
            .WithBindEnabled(nameof(vm.AreSuggestionsEnabled));

        // Side by side when both fit, otherwise stacked - a horizontal StackPanel kept each button at
        // its natural width, so longer translations (e.g. Russian, Bulgarian) ran past the window edge (#15901).
        var panelButtons = new ButtonPairPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
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
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 170,
            Background = Brushes.Transparent,
        }.WithAccessibleName(Se.Language.General.Suggestions);
        listBoxSuggestions.Styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(TemplatedControl.PaddingProperty, new Thickness(10, 6)),
                new Setter(TemplatedControl.FontSizeProperty, 13.0),
                new Setter(Layoutable.MinHeightProperty, 0.0),
            },
        });
        listBoxSuggestions.DoubleTapped += vm.ListBoxSuggestionsDoubleTapped;
        listBoxSuggestions.SelectionChanged += vm.ListBoxSuggestionsSelectionChanged;

        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(EditorChrome.MakeListFrame(listBoxSuggestions, clearListBackground: true));
        panel.Children.Add(panelButtons);
        return EditorChrome.MakeEditorSection(Se.Language.General.Suggestions, panel);
    }
}

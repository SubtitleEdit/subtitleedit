using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

/// <summary>
/// Spell check, in the editor look (see <see cref="EditorChrome"/>): the line with the word
/// highlighted across the window, under it the word not found with its actions and the suggestions,
/// the dictionary, the last action's status and Done in the button bar.
/// </summary>
public class SpellCheckWindow : Window
{
    public SpellCheckWindow(SpellCheckViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.SpellCheck.SpellCheck);
        SizeToContent = SizeToContent.Height;
        Width = 780;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var textBoxWord = MakeWordTextBox(vm);

        var columns = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*") };
        columns.Add(MakeWordNotFoundSection(vm, textBoxWord), 0, 0);
        columns.Add(new Border
        {
            Child = MakeSuggestionsSection(vm),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var body = new StackPanel();
        body.Children.Add(MakeLineSection(vm));
        body.Children.Add(columns);
        body.Children.Add(MakeSourceImageSection(vm));

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(new Border { Background = EditorChrome.PanelBrush, Child = body }, 0, 0);
        root.Add(MakeButtonBar(vm), 1, 0);

        Content = root;

        // The word box, which the view model also focuses after each change/skip, so the keys
        // (Esc, F1, play) work from the start - a focused action button would be clicked by a
        // bare Space instead.
        UiUtil.FocusOnFirstActivation(this, textBoxWord); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);

        // Remember where the user parked the window, e.g. off to the side of the text (#9762).
        // The size is not restored: the window is not resizable and sizes to its content.
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static TextBox MakeWordTextBox(SpellCheckViewModel vm)
    {
        var textBoxWord = new TextBox
        {
            [!TextBox.TextProperty] = new Binding(nameof(SpellCheckViewModel.CurrentWord)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = EditorChrome.ScaledFont(15),
            MinHeight = 36,
            Width = double.NaN,
        }.WithAccessibleName(Se.Language.SpellCheck.WordNotFound);
        textBoxWord.Classes.Add("text");
        vm.TextBoxWordNotFound = textBoxWord;
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            textBoxWord.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        return textBoxWord;
    }

    /// <summary>
    /// "Line x of y" with how far the check has come, play and edit whole text, and the line with
    /// the word highlighted, wrapped, so a long line is not cut off.
    /// </summary>
    private static Border MakeLineSection(SpellCheckViewModel vm)
    {
        var labelLine = new TextBlock
        {
            FontSize = EditorChrome.ScaledFont(11),
            FontWeight = FontWeight.SemiBold,
            LetterSpacing = 0.6,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(SpellCheckViewModel.LineText)) { Mode = BindingMode.OneWay },
        };

        var progress = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Height = 3,
            MinHeight = 3,
            MinWidth = 0,
            CornerRadius = new CornerRadius(2),
            VerticalAlignment = VerticalAlignment.Center,
            [!RangeBase.ValueProperty] = new Binding(nameof(SpellCheckViewModel.LineProgress)) { Mode = BindingMode.OneWay },
        }.WithLabeledBy(labelLine);

        // Plays the line the current word is in, in the main window's video player, and pauses at
        // its end - hidden when no video is loaded (the view model then gets no play hook). Mainly
        // for STT output, where only the audio says what an unknown word should be (#14145).
        var buttonPlay = EditorChrome.MakeTextToolButton("fa-solid fa-play", Se.Language.General.PlayCurrent, vm.PlayCurrentLineCommand);
        buttonPlay.Bind(Visual.IsVisibleProperty, new Binding(nameof(SpellCheckViewModel.IsPlayVisible)));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonPlay, Se.Language.SpellCheck.PlayCurrentLineHint);
        }

        var buttonEditWholeText = EditorChrome.CreateToolButton(IconNames.Pencil, Se.Language.General.EditWholeText, vm.EditWholeTextCommand, 28);

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"),
            ColumnSpacing = 10,
            MinHeight = 28,
        };
        header.Add(labelLine, 0, 0);
        header.Add(progress, 0, 1);
        header.Add(buttonPlay, 0, 2);
        header.Add(buttonEditWholeText, 0, 3);

        var panelWholeText = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(16, 12),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        vm.PanelWholeText = panelWholeText;
        var scrollViewerWholeText = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = panelWholeText,
            Height = 85,
        };

        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(header);
        panel.Children.Add(MakeFrame(scrollViewerWholeText));
        return new Border
        {
            Child = panel,
            Padding = new Thickness(14, 8, 14, 14),
        };
    }

    /// <summary>
    /// The word, the common action in the accent, and the actions that are less common and change
    /// the word lists as flat buttons.
    /// </summary>
    private static Border MakeWordNotFoundSection(SpellCheckViewModel vm, TextBox textBoxWord)
    {
        var buttonGoogleSearch = EditorChrome.CreateToolButton(IconNames.Web, Se.Language.General.GoogleIt.Trim(), vm.GoogleItCommand, 36);
        var wordRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 6,
        };
        wordRow.Add(textBoxWord, 0, 0);
        wordRow.Add(buttonGoogleSearch, 0, 1);

        var buttonChange = MakeActionButton(Se.Language.General.Change, nameof(SpellCheckViewModel.ChangeWordCommand), IconNames.FindReplace);
        buttonChange.Classes.Add("primary");
        var buttonChangeAll = MakeActionButton(Se.Language.General.ChangeAll, nameof(SpellCheckViewModel.ChangeWordAllCommand), IconNames.FileReplaceOutline);
        var buttonSkipOne = MakeActionButton(Se.Language.General.SkipOnce, nameof(SpellCheckViewModel.SkipWordCommand), IconNames.SkipNext);
        var buttonSkipAll = MakeActionButton(Se.Language.General.SkipAll, nameof(SpellCheckViewModel.SkipWordAllCommand), IconNames.ChevronDoubleRight);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            ColumnSpacing = 6,
            RowSpacing = 6,
        };
        grid.Add(buttonChange, 0, 0);
        grid.Add(buttonChangeAll, 0, 1);
        grid.Add(buttonSkipOne, 1, 0);
        grid.Add(buttonSkipAll, 1, 1);

        // The text is the action to undo ("Undo: change x to y"), so it is bound, icon in front
        var buttonUndo = EditorChrome.MakeTextToolButton(IconNames.Undo, string.Empty, vm.UndoCommand);
        buttonUndo.Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                new Optris.Icons.Avalonia.Icon { Value = IconNames.Undo, FontSize = 12, VerticalAlignment = VerticalAlignment.Center },
                new TextBlock
                {
                    FontSize = EditorChrome.FieldFontSize,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    [!TextBlock.TextProperty] = new Binding(nameof(SpellCheckViewModel.UndoText)) { Mode = BindingMode.OneWay },
                },
            },
        };
        buttonUndo.Bind(Visual.IsVisibleProperty, new Binding(nameof(SpellCheckViewModel.IsUndoVisible)));
        buttonUndo.Bind(AutomationProperties.NameProperty, new Binding(nameof(SpellCheckViewModel.UndoText)));

        var panelWordLists = new StackPanel
        {
            Margin = new Thickness(-8, 2, 0, 0),
            Children =
            {
                EditorChrome.MakeTextToolButton(IconNames.BookAlphabet, Se.Language.General.AddToUserDictionary, vm.AddToUserDictionaryCommand),
                EditorChrome.MakeTextToolButton(IconNames.Account, Se.Language.General.AddToNamesListCaseSensitive, vm.AddToNamesListCommand),
                buttonUndo,
            },
        };

        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(wordRow);
        panel.Children.Add(grid);
        panel.Children.Add(panelWordLists);
        return EditorChrome.MakeEditorSection(Se.Language.SpellCheck.WordNotFound, panel);
    }

    private static Button MakeActionButton(string text, string commandProperty, string icon)
    {
        var button = new Button
        {
            Content = text,
            [!Button.CommandProperty] = new Binding(commandProperty) { Mode = BindingMode.OneWay },
        }.WithIconLeft(icon);
        EditorChrome.StyleActionColumnButton(button);
        button.HorizontalContentAlignment = HorizontalAlignment.Center;
        return button;
    }

    private static Border MakeSuggestionsSection(SpellCheckViewModel vm)
    {
        var listBoxSuggestions = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(SpellCheckViewModel.Suggestions)) { Mode = BindingMode.OneWay },
            [!ListBox.SelectedItemProperty] = new Binding(nameof(SpellCheckViewModel.SelectedSuggestion)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 176,
            Background = Brushes.Transparent,
        }.WithAccessibleName(Se.Language.General.Suggestions);
        listBoxSuggestions.Styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(TemplatedControl.PaddingProperty, new Thickness(10, 5)),
                new Setter(TemplatedControl.FontSizeProperty, 13.0),
                new Setter(Layoutable.MinHeightProperty, 0.0),
            },
        });
        listBoxSuggestions.DoubleTapped += vm.ListBoxSuggestionsDoubleTapped;

        var buttonUseOnce = MakeActionButton(Se.Language.General.UseOnce, nameof(SpellCheckViewModel.SuggestionUseOnceCommand), IconNames.Check);
        var buttonUseAlways = MakeActionButton(Se.Language.General.UseAlways, nameof(SpellCheckViewModel.SuggestionUseAlwaysCommand), IconNames.CheckBold);

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

        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(MakeFrame(listBoxSuggestions));
        panel.Children.Add(panelButtons);
        return EditorChrome.MakeEditorSection(Se.Language.General.Suggestions, panel);
    }

    /// <summary>
    /// Source image (e.g. Blu-ray .sup) of the current line, shown when the user has attached it
    /// (#11719) via the image button (left of Done) or by right-clicking the image.
    /// </summary>
    private static Border MakeSourceImageSection(SpellCheckViewModel vm)
    {
        var sourceImage = new Image
        {
            Stretch = Stretch.Uniform,
            MaxHeight = 140,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(8),
            [!Image.SourceProperty] = new Binding(nameof(SpellCheckViewModel.SourceImage)),
        };
        var sourceImageBorder = MakeFrame(sourceImage);
        sourceImageBorder.ContextMenu = new ContextMenu
        {
            ItemsSource = new[]
            {
                new MenuItem
                {
                    Header = Se.Language.SpellCheck.LoadSourceImage,
                    Command = vm.LoadSourceImageCommand,
                },
            },
        };

        var section = new Border
        {
            Child = sourceImageBorder,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
        section.Bind(Visual.IsVisibleProperty, new Binding(nameof(SpellCheckViewModel.HasSourceImage)));
        return section;
    }

    /// <summary>
    /// The dictionary with its two tools and what the last action did on the left, the source image
    /// button and Done on the right.
    /// </summary>
    private static Border MakeButtonBar(SpellCheckViewModel vm)
    {
        var comboBoxDictionary = new ComboBox
        {
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(SpellCheckViewModel.Dictionaries)) { Mode = BindingMode.OneWay },
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(SpellCheckViewModel.SelectedDictionary)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Center,
            Width = 190,
        }.WithAccessibleName(Se.Language.General.Dictionary);
        comboBoxDictionary.SelectionChanged += (_, _) => vm.OnDictionaryChanged();

        var buttonDictionaryBrowse = EditorChrome.CreateToolButton(IconNames.Download, Se.Language.SpellCheck.GetDictionariesTitle, vm.BrowseDictionaryCommand, 28);
        var buttonUseAlwaysList = EditorChrome.CreateToolButton(IconNames.FormatListChecks, Se.Language.SpellCheck.EditUseAlwaysList, vm.EditUseAlwaysListCommand, 28);

        // What the last action did ("Change x to y"), cleared again after a few seconds
        var labelStatus = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(10, 0, 12, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(SpellCheckViewModel.StatusText)) { Mode = BindingMode.OneWay },
        };

        var panelLeft = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,*"),
            ColumnSpacing = 4,
        };
        panelLeft.Add(comboBoxDictionary, 0, 0);
        panelLeft.Add(buttonDictionaryBrowse, 0, 1);
        panelLeft.Add(buttonUseAlwaysList, 0, 2);
        panelLeft.Add(labelStatus, 0, 3);

        // Icon button left of "Done" to attach/show the source image(s) — a context menu
        // alone isn't reliable on macOS (panels without a background aren't hit-tested).
        var buttonLoadSourceImage = EditorChrome.CreateToolButton(IconNames.Image, Se.Language.SpellCheck.LoadSourceImage, vm.LoadSourceImageCommand);
        var buttonDone = UiUtil.MakeButtonDone(vm.OkCommand);
        buttonDone.Margin = new Thickness(0);
        return EditorChrome.MakeDockedButtonBar(panelLeft, buttonLoadSourceImage, buttonDone);
    }

    /// <summary>
    /// A rounded frame on the field color, like the editor's text box and lists.
    /// </summary>
    private static Border MakeFrame(Control child)
    {
        return new Border
        {
            Child = child,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
        };
    }
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

/// <summary>
/// Train an nOCR database from installed fonts in the editor look (see <see cref="EditorChrome"/>):
/// fonts, preview and options as sections with icons, characters below, the progress strip and the
/// status with Done + Start training in the button bar.
/// </summary>
public class NOcrTrainWindow : Window
{
    private static readonly Color PreviewBackground = Color.FromRgb(0x1E, 0x23, 0x2B);
    private static readonly Color LearnedColor = Color.FromRgb(0x3F, 0xB9, 0x50);
    private static readonly Color SkippedColor = Color.FromRgb(0x8B, 0x94, 0x9E);

    public NOcrTrainWindow(NOcrTrainViewModel vm)
    {
        Title = Se.Language.Ocr.TrainNOcrDatabase.TrimEnd('.');
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Width = 960;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.TextToolToggles | EditorWindowStyles.CompactSplitButtons);
        DataContext = vm;

        var buttonTrain = new Button
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new Icon
                    {
                        FontSize = 14,
                        VerticalAlignment = VerticalAlignment.Center,
                        [!Optris.Icons.Avalonia.Icon.ValueProperty] = new Binding(nameof(vm.TrainButtonIcon)),
                    },
                    new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        [!TextBlock.TextProperty] = new Binding(nameof(vm.TrainButtonText)),
                    },
                },
            },
            Command = vm.StartOrAbortTrainingCommand,
            Padding = new Thickness(14, 6),
        };
        buttonTrain.Classes.Add("primary");
        AutomationProperties.SetName(buttonTrain, Se.Language.Ocr.StartTraining);
        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand);

        var columns = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        columns.Add(MakeSection(IconNames.FormatFont, Se.Language.General.Fonts, BuildFontsSection(vm), BuildFontsCount(vm)), 0, 0);

        // The settings section stretches so both columns end level.
        var right = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
            },
        };
        right.Add(MakeSection(IconNames.Eye, Se.Language.General.Preview, BuildPreviewSection(vm), BuildPreviewFontName(vm)), 0, 0);
        right.Add(MakeSection(IconNames.Tune, Se.Language.Ocr.TrainingOptions, BuildSettingsSection(vm), top: true), 1, 0);
        columns.Add(new Border
        {
            Child = right,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                BuildHeader(),
                columns,
                MakeSection(IconNames.AlphabeticalVariant, Se.Language.Ocr.CharactersToTrain, BuildCharactersSection(vm), top: true),
            },
        };

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };

        // Sections scroll on short screens, so the progress and the buttons stay reachable
        root.Add(new ScrollViewer
        {
            Content = new Border { Background = EditorChrome.PanelBrush, Child = stack },
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
        }, 0, 0);
        root.Add(BuildProgressStrip(vm), 1, 0);
        root.Add(EditorChrome.MakeDockedButtonBar(BuildStatus(vm), buttonDone, buttonTrain), 2, 0);
        Content = root;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            buttonTrain.Focus(); // hack to make OnKeyDown work
        });
        KeyDown += (_, e) => vm.KeyDown(e);
        Closing += (_, _) => vm.OnClosing();
    }

    private static Control BuildHeader()
    {
        var badge = new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(10),
            Background = EditorChrome.AccentSoftBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new Icon
            {
                Value = IconNames.School,
                FontSize = 22,
                Foreground = EditorChrome.AccentBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

        var texts = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new TextBlock
                {
                    Text = Se.Language.Ocr.TrainNOcrDatabase.TrimEnd('.'),
                    FontSize = UiUtil.ScaledFontSize(17),
                    FontWeight = FontWeight.SemiBold,
                },
                new TextBlock
                {
                    Text = Se.Language.Ocr.TrainNOcrSubtitle,
                    FontSize = EditorChrome.FieldFontSize,
                    Foreground = EditorChrome.DimTextBrush,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 0),
                },
            },
        };

        return new Border
        {
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Children = { badge, texts },
            },
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(14, 12),
        };
    }

    private static Control BuildFontsCount(NOcrTrainViewModel vm)
    {
        return new Border
        {
            Padding = new Thickness(9, 2),
            CornerRadius = new CornerRadius(10),
            Background = EditorChrome.AccentSoftBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                FontSize = EditorChrome.ScaledFont(11),
                FontWeight = FontWeight.SemiBold,
                Foreground = EditorChrome.StrongTextBrush,
                [!TextBlock.TextProperty] = new Binding(nameof(vm.SelectedFontsText)),
            },
        };
    }

    private static Control BuildPreviewFontName(NOcrTrainViewModel vm)
    {
        return new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 220,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PreviewFontName)),
        };
    }

    private static Control BuildFontsSection(NOcrTrainViewModel vm)
    {
        var search = new TextBox
        {
            PlaceholderText = Se.Language.General.Search,
            [!TextBox.TextProperty] = new Binding(nameof(vm.FontSearchText)) { Mode = BindingMode.TwoWay },
        }.WithSearchAndClearIcons();
        search.Classes.Add("text");
        AutomationProperties.SetName(search, Se.Language.General.Search);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(-8, 0, 0, 0),
            Children =
            {
                EditorChrome.MakeTextToolButton(IconNames.AutoFix, Se.Language.Ocr.SubtitleFonts, vm.SelectSubtitleFontsCommand),
                EditorChrome.MakeTextToolButton(IconNames.Close, Se.Language.General.Clear, vm.ClearFontsCommand),
            },
        };

        // The template must bind everything (no use of the build parameter): the virtualized
        // ListBox recycles containers with a null item, and reuses them for other items where
        // static values would go stale. Uniform row height keeps scroll extent estimation sane
        // despite wildly varying font line heights.
        var fontNameToFontFamily = new FuncValueConverter<string?, FontFamily>(name =>
            string.IsNullOrWhiteSpace(name) ? FontFamily.Default : FontFamilyHelper.Make(name));
        var fontsListBox = new ListBox
        {
            Height = 336,
            ItemsSource = vm.FilteredFonts,
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.HighlightedFont)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<NOcrTrainFontItem>((_, _) => new CheckBox
            {
                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(NOcrTrainFontItem.IsSelected)),
                [!ContentControl.ContentProperty] = new Binding(nameof(NOcrTrainFontItem.Name)),
                [!TemplatedControl.FontFamilyProperty] = new Binding(nameof(NOcrTrainFontItem.Name))
                {
                    Converter = fontNameToFontFamily,
                },
                FontSize = UiUtil.ScaledFontSize(15),
                Height = 26,
                VerticalContentAlignment = VerticalAlignment.Center,
                ClipToBounds = true,
            }, supportsRecycling: true),
        };
        AutomationProperties.SetName(fontsListBox, Se.Language.General.Fonts);

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                search,
                buttons,
                EditorChrome.MakeListFrame(fontsListBox, clearListBackground: true),
            },
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotTraining)),
        };
    }

    private static Control BuildPreviewSection(NOcrTrainViewModel vm)
    {
        var image = new Image
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            MaxHeight = 150,
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewImage)),
        };

        // The preview renders white text, so it keeps its dark backdrop in both themes
        return new Border
        {
            Child = image,
            Padding = new Thickness(6),
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.FromRgb(0x2A, 0x33, 0x40), 0),
                    new GradientStop(PreviewBackground, 1),
                },
            },
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
            MinHeight = 64,
        };
    }

    private static Control BuildSettingsSection(NOcrTrainViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            ColumnSpacing = 12,
            RowSpacing = 10,
        };
        for (var i = 0; i < 5; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        var nameBox = EditorChrome.AsTextField(UiUtil.MakeTextBox(220, vm, nameof(vm.DatabaseName)), Se.Language.General.Name);
        nameBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        nameBox.Width = double.NaN;
        nameBox.Margin = new Thickness(0);
        AddRow(grid, 0, Se.Language.General.Name, nameBox);

        var baseCombo = UiUtil.MakeComboBox(vm.BaseDatabases, vm, nameof(vm.SelectedBaseDatabase));
        baseCombo.HorizontalAlignment = HorizontalAlignment.Stretch;
        baseCombo.Margin = new Thickness(0);
        AutomationProperties.SetName(baseCombo, Se.Language.Ocr.StartFromDatabase);
        SetHint(baseCombo, Se.Language.Ocr.StartFromDatabaseHint);
        AddRow(grid, 1, Se.Language.Ocr.StartFromDatabase, baseCombo);

        var sizesBox = EditorChrome.AsTextField(UiUtil.MakeTextBox(220, vm, nameof(vm.FontSizes)), Se.Language.Ocr.FontSizes);
        sizesBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        sizesBox.Width = double.NaN;
        sizesBox.Margin = new Thickness(0);
        sizesBox.InnerRightContent = new TextBlock
        {
            Text = "px",
            Foreground = EditorChrome.FaintTextBrush,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        SetHint(sizesBox, Se.Language.Ocr.FontSizesHint);
        AddRow(grid, 2, Se.Language.Ocr.FontSizes, sizesBox);

        var styles = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                NOcrChrome.MakeToggle(Se.Language.General.Bold, nameof(vm.TrainBold), vm, IconNames.Bold),
                NOcrChrome.MakeToggle(Se.Language.General.Italic, nameof(vm.TrainItalic), vm, IconNames.Italic),
            },
        };
        AddRow(grid, 3, Se.Language.General.Styles, styles);

        var segments = EditorChrome.MakeField(nameof(vm.NumberOfSegments), 10, 500, 130);
        segments.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(segments, Se.Language.Ocr.NumberOfLineSegments);
        SetHint(segments, Se.Language.Ocr.NumberOfLineSegmentsHint);
        AddRow(grid, 4, Se.Language.Ocr.NumberOfLineSegments, segments);

        grid[!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotTraining));
        return grid;
    }

    private static Control BuildCharactersSection(NOcrTrainViewModel vm)
    {
        var charactersTextBox = EditorChrome.AsTextField(UiUtil.MakeTextBox(200, vm, nameof(vm.CharactersToTrain)), Se.Language.Ocr.CharactersToTrain);
        charactersTextBox.Width = double.NaN;
        charactersTextBox.Margin = new Thickness(0);
        charactersTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        charactersTextBox.TextWrapping = TextWrapping.Wrap;
        charactersTextBox.AcceptsReturn = false;
        charactersTextBox.Height = 56;
        charactersTextBox.VerticalContentAlignment = VerticalAlignment.Top;
        charactersTextBox.FontSize = UiUtil.ScaledFontSize(15);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(-8, 0, 0, 0),
            Children =
            {
                EditorChrome.MakeTextToolButton(IconNames.Restore, Se.Language.General.Reset, vm.ResetCharactersCommand),
                EditorChrome.MakeTextToolButton(IconNames.Import, Se.Language.Ocr.ImportCharactersFromSubtitleFile, vm.ImportCharactersFromFileCommand),
            },
        };

        var mergedTextBox = EditorChrome.AsTextField(UiUtil.MakeTextBox(200, vm, nameof(vm.MergedLetters)), Se.Language.Ocr.LetterCombinationsToTrain);
        mergedTextBox.Width = double.NaN;
        mergedTextBox.Margin = new Thickness(0);
        mergedTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        mergedTextBox.PlaceholderText = "fi ff fl rn";

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                charactersTextBox,
                buttons,
                EditorChrome.MakeDimLabel(Se.Language.Ocr.LetterCombinationsToTrain, new Thickness(0, 4, 0, 0)),
                mergedTextBox,
            },
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotTraining)),
        };
    }

    private static Control BuildProgressStrip(NOcrTrainViewModel vm)
    {
        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Height = 6,
            MinHeight = 6,
            CornerRadius = new CornerRadius(3),
            VerticalAlignment = VerticalAlignment.Center,
            [!RangeBase.ValueProperty] = new Binding(nameof(vm.ProgressValue)),
        };

        return new Border
        {
            Child = progressBar,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsProgressVisible)),
        };
    }

    /// <summary>The learned/skipped badges and the status text, on the left of the button bar.</summary>
    private static Control BuildStatus(NOcrTrainViewModel vm)
    {
        var status = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
        };
        var statusRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            ColumnSpacing = 8,
            Margin = new Thickness(0, 0, 12, 0),
        };
        statusRow.Add(MakeBadge(IconNames.CheckCircle, nameof(vm.LearnedText), LearnedColor), 0, 0);
        statusRow.Add(MakeBadge(IconNames.SkipNext, nameof(vm.SkippedText), SkippedColor), 0, 1);
        statusRow.Add(status, 0, 2);
        return statusRow;
    }

    private static Control MakeBadge(string iconName, string textPath, Color color)
    {
        var brush = new SolidColorBrush(color);
        return new Border
        {
            Padding = new Thickness(8, 2),
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(color, 0.14),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                Children =
                {
                    new Icon { Value = iconName, FontSize = 12, Foreground = brush, VerticalAlignment = VerticalAlignment.Center },
                    new TextBlock
                    {
                        FontSize = EditorChrome.ScaledFont(11.5),
                        FontWeight = FontWeight.SemiBold,
                        Foreground = brush,
                        VerticalAlignment = VerticalAlignment.Center,
                        [!TextBlock.TextProperty] = new Binding(textPath),
                    },
                },
            },
            [!Visual.IsVisibleProperty] = new Binding(textPath) { Converter = StringConverters.IsNotNullOrEmpty },
        };
    }

    private static void AddRow(Grid grid, int row, string label, Control control)
    {
        grid.Add(EditorChrome.MakeDimLabel(label, new Thickness(0)), row, 0);
        grid.Add(control, row, 1);
    }

    /// <summary>
    /// A section with an accent icon before the small caps header (the trainer's cards, in the
    /// editor look), optional text on the right of the header, and a line on top.
    /// </summary>
    private static Border MakeSection(string iconName, string title, Control content, Control? headerRight = null, bool top = false)
    {
        var headerTitle = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Icon
                {
                    Value = iconName,
                    FontSize = 14,
                    Foreground = EditorChrome.AccentBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                EditorChrome.MakeSectionHeader(title),
            },
        };

        var header = new DockPanel { MinHeight = 22 };
        if (headerRight != null)
        {
            DockPanel.SetDock(headerRight, Dock.Right);
            header.Children.Add(headerRight);
        }

        header.Children.Add(headerTitle);
        return new Border
        {
            Child = new StackPanel { Spacing = 10, Children = { header, content } },
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, top ? 1 : 0, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    private static void SetHint(Control control, string hint)
    {
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(control, hint);
        }
    }
}

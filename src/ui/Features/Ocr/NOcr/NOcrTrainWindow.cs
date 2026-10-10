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
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

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
        DataContext = vm;

        var accent = GetAccentColor();

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
        buttonTrain.Classes.Add("accent");
        AutomationProperties.SetName(buttonTrain, Se.Language.Ocr.StartTraining);
        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand);

        var columns = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 12,
        };
        columns.Add(MakeCard(BuildFontsSection(vm, accent)), 0, 0);

        // The settings card stretches so both columns end level.
        var right = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
            },
            RowSpacing = 12,
        };
        right.Add(MakeCard(BuildPreviewSection(vm, accent)), 0, 0);
        right.Add(MakeCard(BuildSettingsSection(vm, accent)), 1, 0);
        columns.Add(right, 0, 1);

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                BuildHeader(accent),
                columns,
                MakeCard(BuildCharactersSection(vm, accent)),
                BuildProgressSection(vm),
                UiUtil.MakeButtonBar(buttonTrain, buttonDone),
            },
        };

        Content = new Border
        {
            Child = stack,
            Padding = UiUtil.MakeWindowMargin(),
        };

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            buttonTrain.Focus(); // hack to make OnKeyDown work
        });
        KeyDown += (_, e) => vm.KeyDown(e);
        Closing += (_, _) => vm.OnClosing();
    }

    private static Color GetAccentColor()
    {
        return Application.Current != null &&
               Application.Current.TryGetResource("SystemAccentColor", Application.Current.ActualThemeVariant, out var value) &&
               value is Color color
            ? color
            : Color.FromRgb(0x00, 0x78, 0xD4);
    }

    private static Control BuildHeader(Color accent)
    {
        var badge = new Border
        {
            Width = 44,
            Height = 44,
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(accent, 0.18),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new Icon
            {
                Value = IconNames.School,
                FontSize = 24,
                Foreground = new SolidColorBrush(accent),
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
                    FontSize = UiUtil.ScaledFontSize(18),
                    FontWeight = FontWeight.SemiBold,
                },
                new TextBlock
                {
                    Text = Se.Language.Ocr.TrainNOcrSubtitle,
                    FontSize = UiUtil.ScaledFontSize(12),
                    Opacity = 0.75,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 0),
                },
            },
        };

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children = { badge, texts },
        };
    }

    private static Control BuildFontsSection(NOcrTrainViewModel vm, Color accent)
    {
        var count = new Border
        {
            Padding = new Thickness(8, 1),
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(accent, 0.18),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Child = new TextBlock
            {
                FontSize = UiUtil.ScaledFontSize(12),
                Foreground = new SolidColorBrush(accent),
                FontWeight = FontWeight.SemiBold,
                [!TextBlock.TextProperty] = new Binding(nameof(vm.SelectedFontsText)),
            },
        };
        var titleRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
        };
        titleRow.Add(MakeSectionTitle(IconNames.FormatFont, Se.Language.General.Fonts, accent), 0, 0);
        titleRow.Add(count, 0, 1);

        var search = new TextBox
        {
            PlaceholderText = Se.Language.General.Search,
            [!TextBox.TextProperty] = new Binding(nameof(vm.FontSearchText)) { Mode = BindingMode.TwoWay },
        }.WithSearchAndClearIcons();
        AutomationProperties.SetName(search, Se.Language.General.Search);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                MakeIconTextButton(IconNames.AutoFix, Se.Language.Ocr.SubtitleFonts, vm.SelectSubtitleFontsCommand),
                MakeIconTextButton(IconNames.Close, Se.Language.General.Clear, vm.ClearFontsCommand),
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
            Background = Brushes.Transparent,
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

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                titleRow,
                search,
                buttons,
                UiUtil.MakeBorderForControlNoPadding(fontsListBox),
            },
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotTraining)),
        };
    }

    private static Control BuildPreviewSection(NOcrTrainViewModel vm, Color accent)
    {
        var fontName = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(12),
            Opacity = 0.7,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 220,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PreviewFontName)),
        };
        var titleRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
        };
        titleRow.Add(MakeSectionTitle(IconNames.Eye, Se.Language.General.Preview, accent), 0, 0);
        titleRow.Add(fontName, 0, 1);

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            MaxHeight = 150,
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewImage)),
        };
        var imageBorder = new Border
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

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children = { titleRow, imageBorder },
        };
    }

    private static Control BuildSettingsSection(NOcrTrainViewModel vm, Color accent)
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

        var nameBox = UiUtil.MakeTextBox(220, vm, nameof(vm.DatabaseName));
        nameBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        nameBox.Width = double.NaN;
        AddRow(grid, 0, Se.Language.General.Name, nameBox);

        var baseCombo = UiUtil.MakeComboBox(vm.BaseDatabases, vm, nameof(vm.SelectedBaseDatabase));
        baseCombo.HorizontalAlignment = HorizontalAlignment.Stretch;
        SetHint(baseCombo, Se.Language.Ocr.StartFromDatabaseHint);
        AddRow(grid, 1, Se.Language.Ocr.StartFromDatabase, baseCombo);

        var sizesBox = UiUtil.MakeTextBox(220, vm, nameof(vm.FontSizes));
        sizesBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        sizesBox.Width = double.NaN;
        sizesBox.InnerRightContent = new TextBlock
        {
            Text = "px",
            Opacity = 0.5,
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
                MakeStyleToggle(IconNames.Bold, Se.Language.General.Bold, nameof(vm.TrainBold)),
                MakeStyleToggle(IconNames.Italic, Se.Language.General.Italic, nameof(vm.TrainItalic)),
            },
        };
        AddRow(grid, 3, Se.Language.General.Styles, styles);

        var segments = UiUtil.MakeNumericUpDownInt(10, 500, 100, 130, vm, nameof(vm.NumberOfSegments));
        SetHint(segments, Se.Language.Ocr.NumberOfLineSegmentsHint);
        AddRow(grid, 4, Se.Language.Ocr.NumberOfLineSegments, segments);

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children = { MakeSectionTitle(IconNames.Tune, Se.Language.Ocr.TrainingOptions, accent), grid },
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotTraining)),
        };
    }

    /// <summary>A toggle button with an icon and a label - a bolder look than a check box.</summary>
    private static ToggleButton MakeStyleToggle(string iconName, string text, string isCheckedPath)
    {
        var toggle = new ToggleButton
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new Icon { Value = iconName, FontSize = 16, VerticalAlignment = VerticalAlignment.Center },
                    new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center },
                },
            },
            Padding = new Thickness(10, 4),
            [!ToggleButton.IsCheckedProperty] = new Binding(isCheckedPath) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, text);
        return toggle;
    }

    private static Control BuildCharactersSection(NOcrTrainViewModel vm, Color accent)
    {
        var charactersTextBox = UiUtil.MakeTextBox(200, vm, nameof(vm.CharactersToTrain));
        charactersTextBox.Width = double.NaN;
        charactersTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        charactersTextBox.TextWrapping = TextWrapping.Wrap;
        charactersTextBox.AcceptsReturn = false;
        charactersTextBox.Height = 56;
        charactersTextBox.FontSize = UiUtil.ScaledFontSize(15);
        AutomationProperties.SetName(charactersTextBox, Se.Language.Ocr.CharactersToTrain);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                MakeIconTextButton(IconNames.Restore, Se.Language.General.Reset, vm.ResetCharactersCommand),
                MakeIconTextButton(IconNames.Import, Se.Language.Ocr.ImportCharactersFromSubtitleFile, vm.ImportCharactersFromFileCommand),
            },
        };

        var mergedTextBox = UiUtil.MakeTextBox(200, vm, nameof(vm.MergedLetters));
        mergedTextBox.Width = double.NaN;
        mergedTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        mergedTextBox.PlaceholderText = "fi ff fl rn";
        AutomationProperties.SetName(mergedTextBox, Se.Language.Ocr.LetterCombinationsToTrain);

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                MakeSectionTitle(IconNames.AlphabeticalVariant, Se.Language.Ocr.CharactersToTrain, accent),
                charactersTextBox,
                buttons,
                MakeSmallLabel(Se.Language.Ocr.LetterCombinationsToTrain).WithMarginTop(4),
                mergedTextBox,
            },
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotTraining)),
        };
    }

    private static Control BuildProgressSection(NOcrTrainViewModel vm)
    {
        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Height = 6,
            MinHeight = 6,
            CornerRadius = new CornerRadius(3),
            [!RangeBase.ValueProperty] = new Binding(nameof(vm.ProgressValue)),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsProgressVisible)),
        };

        var status = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Opacity = 0.85,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
        };
        var statusRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            },
            ColumnSpacing = 8,
        };
        statusRow.Add(status, 0, 0);
        statusRow.Add(MakeBadge(IconNames.CheckCircle, nameof(vm.LearnedText), LearnedColor), 0, 1);
        statusRow.Add(MakeBadge(IconNames.SkipNext, nameof(vm.SkippedText), SkippedColor), 0, 2);

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children = { progressBar, statusRow },
        };
    }

    private static Control MakeBadge(string iconName, string textPath, Color color)
    {
        var brush = new SolidColorBrush(color);
        return new Border
        {
            Padding = new Thickness(8, 2),
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(color, 0.12),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(color, 0.5),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                Children =
                {
                    new Icon { Value = iconName, FontSize = 12, Foreground = brush, VerticalAlignment = VerticalAlignment.Center },
                    new TextBlock
                    {
                        FontSize = UiUtil.ScaledFontSize(12),
                        Foreground = brush,
                        VerticalAlignment = VerticalAlignment.Center,
                        [!TextBlock.TextProperty] = new Binding(textPath),
                    },
                },
            },
            [!Visual.IsVisibleProperty] = new Binding(textPath) { Converter = StringConverters.IsNotNullOrEmpty },
        };
    }

    private static Button MakeIconTextButton(string iconName, string text, IRelayCommand command)
    {
        var button = new Button
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new Icon { Value = iconName, FontSize = 14, VerticalAlignment = VerticalAlignment.Center },
                    new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center },
                },
            },
            Command = command,
            Padding = new Thickness(10, 4),
        };
        AutomationProperties.SetName(button, text);
        return button;
    }

    private static void AddRow(Grid grid, int row, string label, Control control)
    {
        var textBlock = MakeSmallLabel(label);
        textBlock.VerticalAlignment = VerticalAlignment.Center;
        grid.Add(textBlock, row, 0);
        grid.Add(control, row, 1);
    }

    private static Control MakeSectionTitle(string iconName, string text, Color accent)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Icon
                {
                    Value = iconName,
                    FontSize = 16,
                    Foreground = new SolidColorBrush(accent),
                    VerticalAlignment = VerticalAlignment.Center,
                },
                new TextBlock
                {
                    Text = text,
                    FontSize = UiUtil.ScaledFontSize(14),
                    FontWeight = FontWeight.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };
    }

    private static TextBlock MakeSmallLabel(string text)
    {
        return new TextBlock
        {
            Text = text,
            Opacity = 0.85,
        };
    }

    private static void SetHint(Control control, string hint)
    {
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(control, hint);
        }
    }

    private static Border MakeCard(Control child)
    {
        return new Border
        {
            Child = child,
            Padding = new Thickness(16),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)),
            Background = new SolidColorBrush(Color.FromArgb(0x0A, 0x80, 0x80, 0x80)),
        };
    }
}

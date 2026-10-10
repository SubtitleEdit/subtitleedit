using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrTrainWindow : Window
{
    public NOcrTrainWindow(NOcrTrainViewModel vm)
    {
        Title = Se.Language.Ocr.TrainNOcrDatabase.TrimEnd('.');
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Width = 940;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var buttonTrain = UiUtil.MakeButton(Se.Language.Ocr.StartTraining, vm.StartOrAbortTrainingCommand);
        buttonTrain[!ContentControl.ContentProperty] = new Binding(nameof(vm.TrainButtonText));
        buttonTrain.Classes.Add("accent");
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
        columns.Add(MakeCard(BuildFontsSection(vm)), 0, 0);
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
        right.Add(MakeCard(BuildPreviewSection(vm)), 0, 0);
        right.Add(MakeCard(BuildSettingsSection(vm)), 1, 0);
        columns.Add(right, 0, 1);

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Children =
            {
                BuildHeader(),
                columns,
                MakeCard(BuildCharactersSection(vm)),
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

    private static Control BuildHeader()
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
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
    }

    private static Control BuildFontsSection(NOcrTrainViewModel vm)
    {
        var title = MakeSectionTitle(Se.Language.General.Fonts);
        var count = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(12),
            Opacity = 0.75,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SelectedFontsText)),
        };
        var titleRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
        };
        titleRow.Add(title, 0, 0);
        titleRow.Add(count, 0, 1);

        var search = new TextBox
        {
            PlaceholderText = Se.Language.General.Search,
            [!TextBox.TextProperty] = new Binding(nameof(vm.FontSearchText)) { Mode = BindingMode.TwoWay },
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotTraining)),
        };

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                UiUtil.MakeButton(Se.Language.Ocr.SubtitleFonts, vm.SelectSubtitleFontsCommand).WithMargin(0),
                UiUtil.MakeButton(Se.Language.General.Clear, vm.ClearFontsCommand).WithMargin(0),
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
            Height = 330,
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
                Height = 26,
                VerticalContentAlignment = VerticalAlignment.Center,
                ClipToBounds = true,
            }, supportsRecycling: true),
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                titleRow,
                search,
                buttons,
                UiUtil.MakeBorderForControlNoPadding(fontsListBox),
            },
        };
    }

    private static Control BuildPreviewSection(NOcrTrainViewModel vm)
    {
        var fontName = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(12),
            Opacity = 0.75,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PreviewFontName)),
        };
        var titleRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
        };
        titleRow.Add(MakeSectionTitle(Se.Language.General.Preview), 0, 0);
        titleRow.Add(fontName, 0, 1);

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            HorizontalAlignment = HorizontalAlignment.Left,
            MaxHeight = 150,
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewImage)),
        };
        var imageBorder = new Border
        {
            Child = image,
            Background = new SolidColorBrush(Color.FromRgb(0x26, 0x2B, 0x33)),
            CornerRadius = new CornerRadius(4),
            ClipToBounds = true,
            MinHeight = 60,
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children = { titleRow, imageBorder },
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
            RowSpacing = 8,
        };
        for (var i = 0; i < 6; i++)
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
        SetHint(sizesBox, Se.Language.Ocr.FontSizesHint);
        AddRow(grid, 2, Se.Language.Ocr.FontSizes, sizesBox);

        var styles = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children =
            {
                UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.TrainBold)),
                UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.TrainItalic)),
            },
        };
        AddRow(grid, 3, Se.Language.General.Styles, styles);

        var segments = UiUtil.MakeNumericUpDownInt(10, 500, 100, 130, vm, nameof(vm.NumberOfSegments));
        SetHint(segments, Se.Language.Ocr.NumberOfLineSegmentsHint);
        AddRow(grid, 4, Se.Language.Ocr.NumberOfLineSegments, segments);

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children = { MakeSectionTitle(Se.Language.Ocr.TrainingOptions), grid },
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotTraining)),
        };
    }

    private static Control BuildCharactersSection(NOcrTrainViewModel vm)
    {
        var charactersTextBox = UiUtil.MakeTextBox(200, vm, nameof(vm.CharactersToTrain));
        charactersTextBox.Width = double.NaN;
        charactersTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        charactersTextBox.TextWrapping = TextWrapping.Wrap;
        charactersTextBox.AcceptsReturn = false;
        charactersTextBox.Height = 56;

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetCharactersCommand).WithMargin(0),
                UiUtil.MakeButton(Se.Language.Ocr.ImportCharactersFromSubtitleFile, vm.ImportCharactersFromFileCommand).WithMargin(0),
            },
        };

        var mergedTextBox = UiUtil.MakeTextBox(200, vm, nameof(vm.MergedLetters));
        mergedTextBox.Width = double.NaN;
        mergedTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        mergedTextBox.PlaceholderText = "fi ff fl rn";

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                MakeSectionTitle(Se.Language.Ocr.CharactersToTrain),
                charactersTextBox,
                buttons,
                MakeSmallLabel(Se.Language.Ocr.LetterCombinationsToTrain),
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
            [!RangeBase.ValueProperty] = new Binding(nameof(vm.ProgressValue)),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsProgressVisible)),
        };

        var status = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
        };
        var learned = MakeBadge(nameof(vm.LearnedText), Color.FromRgb(0x3F, 0xB9, 0x50));
        var skipped = MakeBadge(nameof(vm.SkippedText), Color.FromRgb(0x8B, 0x94, 0x9E));
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
        statusRow.Add(learned, 0, 1);
        statusRow.Add(skipped, 0, 2);

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children = { progressBar, statusRow },
        };
    }

    private static Control MakeBadge(string textPath, Color color)
    {
        var text = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(12),
            Foreground = new SolidColorBrush(color),
            [!TextBlock.TextProperty] = new Binding(textPath),
        };
        return new Border
        {
            Child = text,
            Padding = new Thickness(8, 2),
            CornerRadius = new CornerRadius(10),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x80, color.R, color.G, color.B)),
            [!Visual.IsVisibleProperty] = new Binding(textPath) { Converter = StringConverters.IsNotNullOrEmpty },
        };
    }

    private static void AddRow(Grid grid, int row, string label, Control control)
    {
        var textBlock = MakeSmallLabel(label);
        textBlock.VerticalAlignment = VerticalAlignment.Center;
        grid.Add(textBlock, row, 0);
        grid.Add(control, row, 1);
    }

    private static TextBlock MakeSectionTitle(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = UiUtil.ScaledFontSize(14),
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
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
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)),
        };
    }
}

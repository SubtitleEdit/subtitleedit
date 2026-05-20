using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;

public class GetDictionariesWindow : Window
{
    private const int LabelWidth = 90;
    private const int ContentWidth = 380;

    public GetDictionariesWindow(GetDictionariesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.GetDictionariesTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 520;
        vm.Window = this;
        DataContext = vm;

        var downloadButton = UiUtil.MakeButton(string.Empty, vm.DownloadCommand)
            .WithIconLeft(IconNames.Download)
            .WithBindEnabled(nameof(vm.IsDownloadEnabled));
        downloadButton.Bind(ContentControl.ContentProperty, new Binding(nameof(vm.DownloadButtonText)));

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children =
            {
                BuildHeader(),
                BuildDetails(vm),
                BuildProgress(vm),
                BuildActions(vm, downloadButton),
            },
        };

        var outerGrid = new Grid { Margin = UiUtil.MakeWindowMargin() };
        outerGrid.Children.Add(stack);

        Content = new Border
        {
            Child = outerGrid,
            Padding = new Thickness(4),
        };

        Activated += delegate { downloadButton.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static TextBlock BuildHeader() => new()
    {
        Text = Se.Language.SpellCheck.GetDictionaryInstructions,
        TextWrapping = TextWrapping.Wrap,
        Opacity = 0.85,
        MaxWidth = LabelWidth + ContentWidth + 12,
    };

    private static Border BuildDetails(GetDictionariesViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(LabelWidth, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnSpacing = 12,
            RowSpacing = 12,
        };

        var combo = new ComboBox
        {
            ItemsSource = vm.Dictionaries,
            ItemTemplate = BuildDictionaryItemTemplate(),
            MinWidth = ContentWidth,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedDictionary)),
            [!ComboBox.IsEnabledProperty] = new Binding(nameof(vm.IsDownloadEnabled)),
        };

        var description = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            MinHeight = 32,
            MaxWidth = ContentWidth,
            Opacity = 0.85,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Description)),
        };

        var folder = new TextBox
        {
            IsReadOnly = true,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 12,
            MaxWidth = ContentWidth,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TextBox.TextProperty] = new Binding(nameof(vm.DictionariesFolder)),
        };

        grid.Add(MakeLabel(Se.Language.General.Dictionary), 0, 0);
        grid.Add(combo, 0, 1);

        grid.Add(MakeLabel(Se.Language.General.Status), 1, 0);
        grid.Add(MakeStatusPanel(nameof(vm.SelectedStatusBrush), nameof(vm.SelectedStatusText)), 1, 1);

        grid.Add(MakeLabel(Se.Language.General.Description), 2, 0);
        grid.Add(description, 2, 1);

        grid.Add(MakeLabel(Se.Language.General.InstallFolder), 3, 0);
        grid.Add(folder, 3, 1);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)),
        };
    }

    private static StackPanel BuildProgress(GetDictionariesViewModel vm)
    {
        var bar = UiUtil.MakeProgressBar();
        bar[!ProgressBar.ValueProperty] = new Binding(nameof(vm.Progress));
        bar[!Visual.OpacityProperty] = new Binding(nameof(vm.ProgressOpacity));

        var status = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { bar, status },
        };
    }

    private static Grid BuildActions(GetDictionariesViewModel vm, Button downloadButton)
    {
        var openFolder = UiUtil.MakeButton(Se.Language.General.OpenDictionaryFolder, vm.OpenFolderCommand)
            .WithIconLeft(IconNames.FolderOpen);

        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { downloadButton, openFolder },
        };

        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand)
            .WithBindIsVisible(nameof(vm.IsDownloadEnabled));
        var cancel = UiUtil.MakeButtonCancel(vm.CancelCommand)
            .WithBindIsVisible(nameof(vm.IsProgressVisible));

        var rightPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { close, cancel },
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        grid.Add(leftPanel, 0, 0);
        grid.Add(rightPanel, 0, 2);
        return grid;
    }

    private static FuncDataTemplate<GetSpellCheckDictionaryDisplay> BuildDictionaryItemTemplate()
    {
        return new FuncDataTemplate<GetSpellCheckDictionaryDisplay>((_, _) =>
        {
            var dot = new Ellipse
            {
                Width = 10,
                Height = 10,
                VerticalAlignment = VerticalAlignment.Center,
                [!Shape.FillProperty] = new Binding(nameof(GetSpellCheckDictionaryDisplay.StatusBrush)),
            };

            var text = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
                [!TextBlock.TextProperty] = new Binding(nameof(GetSpellCheckDictionaryDisplay.DisplayName)),
            };

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { dot, text },
            };
        }, true);
    }

    private static StackPanel MakeStatusPanel(string brushBindingPath, string textBindingPath)
    {
        var dot = new Ellipse
        {
            Width = 10,
            Height = 10,
            VerticalAlignment = VerticalAlignment.Center,
            [!Shape.FillProperty] = new Binding(brushBindingPath),
        };

        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(8, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding(textBindingPath),
        };

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { dot, text },
        };
    }

    private static TextBlock MakeLabel(string text) => new()
    {
        Text = text,
        Opacity = 0.7,
        VerticalAlignment = VerticalAlignment.Center,
    };
}

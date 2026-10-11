using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrErrorFinderWindow : Window
{
    private static readonly IBrush GlyphBackground = new SolidColorBrush(Color.FromRgb(0x1E, 0x23, 0x2B));

    public NOcrErrorFinderWindow(NOcrErrorFinderViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Ocr.FindErrorsInNOcrDatabase.TrimEnd('.'));
        CanResize = true;
        Width = 900;
        Height = 650;
        MinWidth = 700;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var labelInfo = new TextBlock
        {
            Text = Se.Language.Ocr.FindErrorsInNOcrDatabaseInfo,
            TextWrapping = TextWrapping.Wrap,
        };

        var engines = new ItemsControl
        {
            ItemsSource = vm.Engines,
            ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel { Orientation = Orientation.Horizontal, Spacing = 15 }),
            ItemTemplate = new FuncDataTemplate<OcrCompareEngine>((_, _) => new CheckBox
            {
                [!ContentControl.ContentProperty] = new Binding(nameof(OcrCompareEngine.Name)),
                [!CheckBox.IsCheckedProperty] = new Binding(nameof(OcrCompareEngine.IsChecked)) { Mode = BindingMode.TwoWay },
                [!IsEnabledProperty] = new Binding(nameof(OcrCompareEngine.IsEnabled)),
            }),
            VerticalAlignment = VerticalAlignment.Center,
        };

        var buttonStart = UiUtil.MakeButton(Se.Language.General.Start, vm.StartCommand)
            .WithBindIsVisible(nameof(vm.IsRunning), InverseBooleanConverter.Instance)
            .WithBindIsEnabled(nameof(vm.CanStart));
        var buttonStop = UiUtil.MakeButton(Se.Language.General.Stop, vm.StopCommand)
            .WithBindIsVisible(nameof(vm.IsRunning));

        var panelEngines = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = Se.Language.Ocr.CompareWith, VerticalAlignment = VerticalAlignment.Center },
                engines,
                buttonStart,
                buttonStop,
            },
        };

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Width = double.NaN;
        progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(IsVisibleProperty, new Binding(nameof(vm.IsRunning)));

        var statusText = new TextBlock { TextWrapping = TextWrapping.Wrap };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusText)));

        var buttonChangeText = UiUtil.MakeButton(string.Empty, vm.ChangeTextCommand).WithBindIsEnabled(nameof(vm.CanChangeText));
        buttonChangeText.Bind(ContentControl.ContentProperty, new Binding(nameof(vm.ChangeTextText)));
        var buttonDelete = UiUtil.MakeButton(Se.Language.General.Delete, vm.DeleteCommand).WithBindIsEnabled(nameof(vm.HasSelection));
        var buttonInspect = UiUtil.MakeButton(Se.Language.Ocr.InspectLine, vm.InspectCommand).WithBindIsEnabled(nameof(vm.HasSelection));
        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonChangeText, buttonDelete, buttonInspect, buttonDone);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
        };

        grid.Add(labelInfo, 0);
        grid.Add(panelEngines, 1);
        grid.Add(MakeTableView(vm), 2);
        grid.Add(progressBar, 3);
        grid.Add(statusText, 4);
        grid.Add(panelButtons, 5);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, buttonDone);
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Closing += delegate
        {
            vm.OnClosing();
            UiUtil.SaveWindowPosition(this);
        };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static Border MakeTableView(NOcrErrorFinderViewModel vm)
    {
        var tableView = TableViewExtras.MakeTableView();
        tableView.Height = double.NaN;
        tableView.Margin = new Thickness(2);
        tableView.ItemsSource = vm.Items;

        tableView.Columns.Add(new TableViewColumn
        {
            Header = Se.Language.General.Image,
            Width = new GridLength(110),
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<NOcrErrorFinderItem>((item, _) => new Border
            {
                Background = GlyphBackground,
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(4),
                Margin = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new Image
                {
                    Source = item?.Image,
                    Stretch = Stretch.Uniform,
                    MaxHeight = 48,
                    MaxWidth = 96,
                },
            }),
        });
        AddTextColumn(tableView, Se.Language.General.Text, nameof(NOcrErrorFinderItem.EntryText), new GridLength(1, GridUnitType.Star));
        AddTextColumn(tableView, Se.Language.Ocr.OthersRead, nameof(NOcrErrorFinderItem.OthersRead), new GridLength(1.5, GridUnitType.Star));
        AddTextColumn(tableView, Se.Language.Ocr.NOcrEntryErrors, nameof(NOcrErrorFinderItem.Errors), new GridLength(100));
        AddTextColumn(tableView, Se.Language.Ocr.NOcrEntryUsed, nameof(NOcrErrorFinderItem.Used), new GridLength(70));
        AddTextColumn(tableView, Se.Language.Ocr.NOcrEntryLines, nameof(NOcrErrorFinderItem.Lines), new GridLength(1.5, GridUnitType.Star));

        TableViewExtras.BindSelectedItem(tableView, vm, nameof(vm.SelectedItem));
        tableView.DoubleTapped += async (_, _) => await vm.InspectCommand.ExecuteAsync(null);

        return UiUtil.MakeBorderForControlNoPadding(tableView);
    }

    private static void AddTextColumn(TableView tableView, string header, string bindingPath, GridLength width)
    {
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = header,
            Binding = new Binding(bindingPath),
            Width = width,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
    }
}

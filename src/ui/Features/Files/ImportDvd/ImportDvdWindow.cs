using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Files.ImportDvd;

public class ImportDvdWindow : Window
{
    public ImportDvdWindow(ImportDvdViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Import.TitleImportDvd;
        CanResize = true;
        Width = 820;
        Height = 620;
        MinWidth = 640;
        MinHeight = 480;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // disc: IFO + title + info
                new RowDefinition { Height = GridLength.Auto }, // VOB files label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // VOB files
                new RowDefinition { Height = GridLength.Auto }, // PAL/NTSC
                new RowDefinition { Height = GridLength.Auto }, // progress
                new RowDefinition { Height = GridLength.Auto }, // buttons
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 8,
        };

        grid.Add(MakeDiscView(vm), 0);
        grid.Add(MakeHeader(IconNames.FileVideoOutline, Se.Language.File.Import.DvdVobFiles).WithMarginTop(4), 1);
        grid.Add(MakeVobFilesView(vm), 2);
        grid.Add(MakeVideoStandardView(vm), 3);
        grid.Add(MakeProgressView(vm), 4);
        grid.Add(MakeButtons(vm), 5);

        Content = grid;

        // drop an IFO or VOB files anywhere on the window
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, vm.OnDragOver, RoutingStrategies.Bubble);
        AddHandler(DragDrop.DropEvent, vm.OnDrop, RoutingStrategies.Bubble);
        AddHandler(KeyDownEvent, vm.OnKeyDown, RoutingStrategies.Tunnel);
    }

    private static Border MakeDiscView(ImportDvdViewModel vm)
    {
        var discIcon = new ContentControl
        {
            FontSize = 44,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(4, 2, 14, 0),
            Opacity = 0.85,
        };
        Attached.SetIcon(discIcon, IconNames.Disc);

        var labelIfo = UiUtil.MakeLabel(Se.Language.File.Import.DvdIfoFile).WithBold();
        var textBoxIfo = new TextBox
        {
            IsReadOnly = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.IfoFileName)),
            PlaceholderText = "VIDEO_TS\\VTS_01_0.IFO",
        };
        AutomationProperties.SetName(textBoxIfo, Se.Language.File.Import.DvdIfoFile);
        var buttonBrowse = UiUtil.MakeButton(vm.BrowseIfoCommand, IconNames.FolderOpen, Se.Language.File.Import.DvdOpenIfoFile)
            .BindIsEnabled(vm, nameof(vm.IsNotRipping));

        var labelTitle = UiUtil.MakeLabel(Se.Language.General.Title).WithBold();
        var comboBoxTitles = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemsSource = vm.Titles,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedTitle)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<DvdTitleDisplay>((item, _) => MakeTitleItem(), true),
        };
        comboBoxTitles.BindIsEnabled(vm, nameof(vm.IsNotRipping));
        AutomationProperties.SetName(comboBoxTitles, Se.Language.General.Title);
        labelTitle.BindIsVisible(vm, nameof(vm.HasTitles));
        comboBoxTitles.BindIsVisible(vm, nameof(vm.HasTitles));

        var infoIcon = new ContentControl { Opacity = 0.6, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) };
        Attached.SetIcon(infoIcon, IconNames.Information);
        var infoText = new TextBlock
        {
            Opacity = 0.75,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Info)),
        };
        var infoPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { infoIcon, infoText },
        };
        infoPanel.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.Info)) { Converter = StringConverters.IsNotNullOrEmpty });

        var fields = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnSpacing = 8,
            RowSpacing = 8,
        };
        fields.Add(labelIfo, 0, 0);
        fields.Add(textBoxIfo, 0, 1);
        fields.Add(buttonBrowse, 0, 2);
        fields.Add(labelTitle, 1, 0);
        fields.Add(comboBoxTitles, 1, 1, 1, 2);
        fields.Add(infoPanel, 2, 1, 1, 2);

        var panel = new DockPanel();
        DockPanel.SetDock(discIcon, Dock.Left);
        panel.Children.Add(discIcon);
        panel.Children.Add(fields);

        var border = UiUtil.MakeBorderForControl(panel);
        border.Padding = new Thickness(10);
        return border;
    }

    private static Control MakeTitleItem()
    {
        var name = new TextBlock { FontWeight = FontWeight.SemiBold, MinWidth = 36, VerticalAlignment = VerticalAlignment.Center };
        name.Bind(TextBlock.TextProperty, new Binding(nameof(DvdTitleDisplay.Name)));

        var clockIcon = new ContentControl { Opacity = 0.6, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 4, 0) };
        Attached.SetIcon(clockIcon, IconNames.ClockOutline);
        var duration = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
        duration.Bind(TextBlock.TextProperty, new Binding(nameof(DvdTitleDisplay.Duration)) { Converter = new TimeSpanToDisplayShortConverter() });

        var chapters = new TextBlock { Opacity = 0.7, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
        chapters.Bind(TextBlock.TextProperty, new Binding(nameof(DvdTitleDisplay.Chapters)) { StringFormat = "{0} " + Se.Language.File.Chapters.ToLowerInvariant() });

        var status = new TextBlock { Foreground = Brushes.OrangeRed, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
        status.Bind(TextBlock.TextProperty, new Binding(nameof(DvdTitleDisplay.Status)));

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { name, clockIcon, duration, chapters, status },
        };
    }

    private static StackPanel MakeHeader(string iconName, string text)
    {
        var icon = new ContentControl { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) };
        Attached.SetIcon(icon, iconName);
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { icon, UiUtil.MakeLabel(text).WithBold() },
        };
    }

    private static Grid MakeVobFilesView(ImportDvdViewModel vm)
    {
        var fileSizeConverter = new FileSizeConverter();
        var listBox = new ListBox
        {
            ItemsSource = vm.VobFiles,
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedVobFile)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<DvdVobFileItem>((item, _) =>
            {
                var icon = new ContentControl { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), Opacity = 0.8 };
                Attached.SetIcon(icon, IconNames.MovieOpenOutline);
                var name = new TextBlock { FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center };
                name.Bind(TextBlock.TextProperty, new Binding(nameof(DvdVobFileItem.Name)));
                var folder = new TextBlock { Opacity = 0.55, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis };
                folder.Bind(TextBlock.TextProperty, new Binding(nameof(DvdVobFileItem.Folder)));
                var size = new TextBlock { Opacity = 0.75, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 4, 0) };
                size.Bind(TextBlock.TextProperty, new Binding(nameof(DvdVobFileItem.Size)) { Converter = fileSizeConverter });

                var row = new DockPanel { LastChildFill = true };
                DockPanel.SetDock(icon, Dock.Left);
                DockPanel.SetDock(name, Dock.Left);
                DockPanel.SetDock(size, Dock.Right);
                row.Children.Add(icon);
                row.Children.Add(name);
                row.Children.Add(size);
                row.Children.Add(folder);
                return row;
            }, true),
        };
        listBox.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsNotRipping)));
        AutomationProperties.SetName(listBox, Se.Language.File.Import.DvdVobFiles);

        // empty list: say what to do
        var hint = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8,
            IsHitTestVisible = false,
        };
        var hintIcon = new ContentControl { FontSize = 40, Opacity = 0.35, HorizontalAlignment = HorizontalAlignment.Center };
        Attached.SetIcon(hintIcon, IconNames.Disc);
        hint.Children.Add(hintIcon);
        hint.Children.Add(new TextBlock
        {
            Text = Se.Language.File.Import.DvdDropHint,
            Opacity = 0.55,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            MaxWidth = 380,
        });
        hint.BindIsVisible(vm, nameof(vm.IsVobListEmpty));

        var listHost = new Grid();
        listHost.Children.Add(listBox);
        listHost.Children.Add(hint);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Margin = new Thickness(8, 0, 0, 0),
            Children =
            {
                UiUtil.MakeButton(vm.AddVobCommand, IconNames.Plus, Se.Language.File.Import.DvdAddVobFiles).BindIsEnabled(vm, nameof(vm.IsNotRipping)),
                UiUtil.MakeButton(vm.RemoveVobCommand, IconNames.Minus, Se.Language.General.Remove).BindIsEnabled(vm, nameof(vm.IsNotRipping)),
                UiUtil.MakeButton(vm.MoveVobUpCommand, IconNames.ArrowUpThin, Se.Language.General.MoveUp).BindIsEnabled(vm, nameof(vm.IsNotRipping)),
                UiUtil.MakeButton(vm.MoveVobDownCommand, IconNames.ArrowDownThin, Se.Language.General.MoveDown).BindIsEnabled(vm, nameof(vm.IsNotRipping)),
                UiUtil.MakeButton(vm.ClearVobsCommand, IconNames.Trash, Se.Language.General.Clear).BindIsEnabled(vm, nameof(vm.IsNotRipping)),
            },
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        grid.Add(UiUtil.MakeBorderForControlNoPadding(listHost), 0, 0);
        grid.Add(buttons, 0, 1);
        return grid;
    }

    private static StackPanel MakeVideoStandardView(ImportDvdViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.File.Import.DvdVideoStandard).WithBold(),
                new RadioButton
                {
                    Content = "PAL (25 fps)",
                    GroupName = "DvdVideoStandard",
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.IsPal)) { Mode = BindingMode.TwoWay },
                    [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotRipping)),
                },
                new RadioButton
                {
                    Content = "NTSC (29.97 fps)",
                    GroupName = "DvdVideoStandard",
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.IsNtsc)) { Mode = BindingMode.TwoWay },
                    [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotRipping)),
                },
            },
        };
        return panel;
    }

    private static Grid MakeProgressView(ImportDvdViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar(8);
        progressBar.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.BindIsVisible(vm, nameof(vm.IsRipping));

        var status = new TextBlock
        {
            Opacity = 0.8,
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            RowSpacing = 4,
        };
        grid.Add(progressBar, 0);
        grid.Add(status, 1);
        return grid;
    }

    private static Grid MakeButtons(ImportDvdViewModel vm)
    {
        var buttonStart = UiUtil.MakeButton(Se.Language.File.Import.DvdStartRipping, vm.StartRippingCommand)
            .WithIconLeft(IconNames.PlayCircle)
            .WithBold()
            .BindIsEnabled(vm, nameof(vm.CanRip))
            .WithBindIsVisible(vm, nameof(vm.IsNotRipping));
        var buttonAbort = UiUtil.MakeButton(Se.Language.General.Abort, vm.AbortCommand)
            .WithIconLeft(IconNames.StopCircle)
            .WithBindIsVisible(vm, nameof(vm.IsRipping));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        grid.Add(UiUtil.MakeButtonBar(buttonStart, buttonAbort, buttonCancel), 0, 1);
        return grid;
    }
}

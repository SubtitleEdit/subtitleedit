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

/// <summary>
/// Subtitles from a DVD (IFO/VOB), in the editor look (see <see cref="EditorChrome"/>): the IFO
/// file and title, the VOB files to rip, the video standard; progress and Start ripping at the bottom.
/// </summary>
public class ImportDvdWindow : Window
{
    private const double LabelWidth = 70;

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

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var vobFiles = MakeVobFilesView(vm);
        vobFiles.Margin = new Thickness(14, 12, 14, 12);
        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // disc: IFO + title + info
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // VOB files
                new RowDefinition { Height = GridLength.Auto }, // PAL/NTSC
            },
        };
        sections.Add(MakeDiscView(vm), 0);
        sections.Add(new Border
        {
            Child = vobFiles,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        }, 1);
        sections.Add(MakeVideoStandardView(vm), 2);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto }, // progress, while ripping
                new RowDefinition { Height = GridLength.Auto }, // status + buttons
            },
        };
        root.Add(new Border { Background = EditorChrome.PanelBrush, Child = sections }, 0);
        root.Add(MakeProgressView(vm), 1);
        root.Add(MakeButtons(vm), 2);

        Content = root;

        // drop an IFO or VOB files anywhere on the window
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, vm.OnDragOver, RoutingStrategies.Bubble);
        AddHandler(DragDrop.DropEvent, vm.OnDrop, RoutingStrategies.Bubble);
        AddHandler(KeyDownEvent, vm.OnKeyDown, RoutingStrategies.Tunnel);
    }

    private static Border MakeDiscView(ImportDvdViewModel vm)
    {
        var textBoxIfo = new TextBox
        {
            IsReadOnly = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.IfoFileName)),
            PlaceholderText = "VIDEO_TS\\VTS_01_0.IFO",
        };
        textBoxIfo.Classes.Add("text");
        AutomationProperties.SetName(textBoxIfo, Se.Language.File.Import.DvdIfoFile);
        var buttonBrowse = EditorChrome.CreateToolButton("fa-solid fa-folder-open", Se.Language.File.Import.DvdOpenIfoFile, vm.BrowseIfoCommand, 28)
            .BindIsEnabled(vm, nameof(vm.IsNotRipping));

        var labelTitle = EditorChrome.MakeDimLabel(Se.Language.General.Title, new Thickness(0));
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

        var infoIcon = new Optris.Icons.Avalonia.Icon { Value = "fa-solid fa-circle-info", FontSize = 12, Foreground = EditorChrome.FaintTextBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) };
        var infoText = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
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
                new ColumnDefinition { Width = new GridLength(LabelWidth) },
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
        fields.Add(textBoxIfo, 0, 0, 1, 2); // the section header names it
        fields.Add(buttonBrowse, 0, 2);
        fields.Add(labelTitle, 1, 0);
        fields.Add(comboBoxTitles, 1, 1, 1, 2);
        fields.Add(infoPanel, 2, 0, 1, 3);

        return EditorChrome.MakeEditorSection(Se.Language.File.Import.DvdIfoFile, fields, first: true);
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
        chapters.Bind(TextBlock.TextProperty, new Binding(nameof(DvdTitleDisplay.Chapters)) { StringFormat = "{0} " + Se.Language.General.Chapters.ToLowerInvariant() });

        var status = new TextBlock { Foreground = Brushes.OrangeRed, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
        status.Bind(TextBlock.TextProperty, new Binding(nameof(DvdTitleDisplay.Status)));

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { name, clockIcon, duration, chapters, status },
        };
    }

    private static DockPanel MakeVobFilesView(ImportDvdViewModel vm)
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
        var hintIcon = new ContentControl { FontSize = EditorChrome.ScaledFont(40), Foreground = EditorChrome.FaintTextBrush, HorizontalAlignment = HorizontalAlignment.Center };
        Attached.SetIcon(hintIcon, IconNames.Disc);
        hint.Children.Add(hintIcon);
        hint.Children.Add(new TextBlock
        {
            Text = Se.Language.File.Import.DvdDropHint,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = EditorChrome.FieldFontSize,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            MaxWidth = 380,
        });
        hint.BindIsVisible(vm, nameof(vm.IsVobListEmpty));

        var listHost = new Grid();
        listHost.Children.Add(listBox);
        listHost.Children.Add(hint);

        Button Tool(string icon, string name, System.Windows.Input.ICommand command) =>
            EditorChrome.CreateToolButton(icon, name, command, 28).BindIsEnabled(vm, nameof(vm.IsNotRipping));

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                Tool("fa-solid fa-plus", Se.Language.File.Import.DvdAddVobFiles, vm.AddVobCommand),
                Tool("fa-solid fa-minus", Se.Language.General.Remove, vm.RemoveVobCommand),
                Tool("fa-solid fa-arrow-up", Se.Language.General.MoveUp, vm.MoveVobUpCommand),
                Tool("fa-solid fa-arrow-down", Se.Language.General.MoveDown, vm.MoveVobDownCommand),
                Tool("fa-solid fa-trash", Se.Language.General.Clear, vm.ClearVobsCommand),
            },
        };

        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8), MinHeight = 28 };
        DockPanel.SetDock(buttons, Dock.Right);
        header.Children.Add(buttons);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.File.Import.DvdVobFiles));

        var panel = new DockPanel();
        DockPanel.SetDock(header, Dock.Top);
        panel.Children.Add(header);
        panel.Children.Add(EditorChrome.MakeListFrame(listHost, clearListBackground: true));
        return panel;
    }

    private static Border MakeVideoStandardView(ImportDvdViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            Children =
            {
                new RadioButton
                {
                    Content = "PAL (25 fps)",
                    FontSize = EditorChrome.FieldFontSize,
                    GroupName = "DvdVideoStandard",
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.IsPal)) { Mode = BindingMode.TwoWay },
                    [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotRipping)),
                },
                new RadioButton
                {
                    Content = "NTSC (29.97 fps)",
                    FontSize = EditorChrome.FieldFontSize,
                    GroupName = "DvdVideoStandard",
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.IsNtsc)) { Mode = BindingMode.TwoWay },
                    [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsNotRipping)),
                },
            },
        };
        return EditorChrome.MakeEditorSection(Se.Language.File.Import.DvdVideoStandard, panel);
    }

    private static Border MakeProgressView(ImportDvdViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar(8);
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        progressBar.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var status = MakeStatusText(vm);
        status.MaxWidth = 360;

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(progressBar, 0, 0);
        grid.Add(status, 0, 1);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 8),
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.IsRipping)),
        };
    }

    private static TextBlock MakeStatusText(ImportDvdViewModel vm) => new()
    {
        FontSize = EditorChrome.FieldFontSize,
        Foreground = EditorChrome.DimTextBrush,
        TextWrapping = TextWrapping.Wrap,
        VerticalAlignment = VerticalAlignment.Center,
        [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
    };

    private static Border MakeButtons(ImportDvdViewModel vm)
    {
        var buttonStart = UiUtil.MakeButton(Se.Language.File.Import.DvdStartRipping, vm.StartRippingCommand)
            .WithIconLeft(IconNames.PlayCircle)
            .BindIsEnabled(vm, nameof(vm.CanRip))
            .WithBindIsVisible(vm, nameof(vm.IsNotRipping));
        buttonStart.Classes.Add("primary");
        var buttonAbort = UiUtil.MakeButton(Se.Language.General.Abort, vm.AbortCommand)
            .WithIconLeft(IconNames.StopCircle)
            .WithBindIsVisible(vm, nameof(vm.IsRipping));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        // The result (or what went wrong) once ripping is done; while ripping it sits beside the progress bar
        var status = MakeStatusText(vm);
        status.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsNotRipping)));
        return EditorChrome.MakeDockedButtonBar(status, buttonCancel, buttonAbort, buttonStart);
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.PromptFilesSaved;

/// <summary>
/// Multi-file "done" dialog: a PromptFileSaved-style header (status ball, headline, summary chips)
/// over a scrolling list of file cards, each with its own play / show in folder / copy path.
/// </summary>
public class PromptFilesSavedWindow : Window
{
    private readonly PromptFilesSavedViewModel _vm;
    // Follow the app theme, also when it changes while the dialog is open
    private static readonly IBrush OkForeground = EditorChrome.Themed(Color.FromRgb(76, 194, 122), Color.FromRgb(46, 158, 91));
    private static readonly IBrush OkBackground = EditorChrome.Themed(Color.FromArgb(60, 46, 158, 91), Color.FromArgb(36, 46, 158, 91));
    private static readonly IBrush WarnForeground = EditorChrome.Themed(Color.FromRgb(240, 180, 70), Color.FromRgb(196, 128, 16));
    private static readonly IBrush WarnBackground = EditorChrome.Themed(Color.FromArgb(60, 220, 150, 30), Color.FromArgb(36, 220, 150, 30));
    private static IBrush ChipBackground => EditorChrome.HoverBrush;

    public PromptFilesSavedWindow(PromptFilesSavedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        // Header: same language as PromptFileSaved - a green check (amber alert when some jobs
        // produced nothing) in a soft circle, headline, where the files went and summary chips.
        var headerBall = MakeStatusBall(vm.AllSucceeded, 44, 22);

        var labelHeadline = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(14.5),
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 2, 0, 4),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Headline)) { Mode = BindingMode.OneWay },
        };

        var labelFolderSummary = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(11.5),
            Foreground = EditorChrome.DimTextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 460,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.FolderSummary)) { Mode = BindingMode.OneWay },
        };

        var panelSummaryChips = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(0, 8, 0, 0),
            Children =
            {
                MakeChip(nameof(vm.TotalSizeChip), nameof(vm.HasTotalSizeChip), emphasized: true),
                MakeChip(nameof(vm.ElapsedChip), nameof(vm.HasElapsedChip), emphasized: false),
            },
        };

        var panelHeader = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            Margin = new Thickness(4, 0, 4, 0),
            Children =
            {
                headerBall,
                new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Children = { labelHeadline, labelFolderSummary, panelSummaryChips },
                },
            },
        };

        var fileList = new ItemsControl
        {
            ItemsSource = vm.Files,
            ItemTemplate = new FuncDataTemplate<SavedFileItem>((item, _) => MakeFileCard(item), true),
            ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel { Spacing = 6 }),
        };

        var scrollViewer = new ScrollViewer
        {
            Content = fileList,
            MaxHeight = 380,
            Width = 580,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Padding = new Thickness(0, 0, 4, 0),
        };

        var buttonOpenFolder = EditorChrome.MakeTextToolButton("fa-solid fa-folder-open", Se.Language.General.OpenContainingFolder, vm.OpenOutputFolderCommand)
            .WithBindIsVisible(nameof(vm.HasOutputFolder));
        buttonOpenFolder.Margin = new Thickness(-8, 0, 0, 0); // the flat button's text lines up with the content
        var buttonDone = EditorChrome.AsPrimary(UiUtil.MakeButtonDone(vm.OkCommand));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(16, 18, 16, 16),
            RowSpacing = 14,
        };

        grid.Add(panelHeader, 0);
        grid.Add(scrollViewer, 1);

        Content = EditorChrome.MakeRoot(grid, EditorChrome.MakeButtonBar(buttonOpenFolder, buttonDone));

        UiUtil.FocusOnFirstActivation(this, buttonDone); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private Border MakeStatusBall(bool isSuccess, double size, double iconFontSize)
    {
        var icon = new ContentControl
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = isSuccess ? OkForeground : WarnForeground,
            FontSize = iconFontSize,
        };
        Attached.SetIcon(icon, isSuccess ? IconNames.Check : IconNames.Alert);

        return new Border
        {
            Width = size,
            Height = size,
            CornerRadius = new CornerRadius(size / 2),
            Background = isSuccess ? OkBackground : WarnBackground,
            VerticalAlignment = VerticalAlignment.Center,
            Child = icon,
        };
    }

    private Control MakeFileCard(SavedFileItem item)
    {
        var ball = MakeStatusBall(item.IsSuccess, 28, 15);

        var labelFileName = new TextBlock
        {
            Text = item.FileNameDisplay,
            FontWeight = FontWeight.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        ToolTip.SetTip(labelFileName, item.FileName);

        var labelFolder = new TextBlock
        {
            Text = item.FolderDisplay,
            FontSize = UiUtil.ScaledFontSize(11),
            Foreground = EditorChrome.DimTextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 1, 0, 0),
            IsVisible = item.ShowFolder,
        };

        var panelChips = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Margin = new Thickness(0, 5, 0, 0),
        };
        if (item.IsSuccess)
        {
            if (item.HasExtensionChip)
            {
                panelChips.Children.Add(MakeChip(item.ExtensionChip, emphasized: true));
            }

            panelChips.Children.Add(MakeChip(nameof(item.FileSizeChip), nameof(item.HasFileSizeChip), emphasized: false));
            panelChips.Children.Add(MakeChip(nameof(item.DurationChip), nameof(item.HasDurationChip), emphasized: false));
        }
        else
        {
            var statusChip = MakeChip(item.StatusText, emphasized: true);
            statusChip.Background = WarnBackground;
            ((TextBlock)statusChip.Child!).Foreground = WarnForeground;
            panelChips.Children.Add(statusChip);
        }

        var panelText = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { labelFileName, labelFolder, panelChips },
        };

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = item.IsSuccess,
        };
        if (PromptFilesSavedViewModel.IsMediaFile(item.FileName))
        {
            panelButtons.Children.Add(MakeRowButton(_vm.PlayFileCommand, IconNames.Play, Se.Language.General.Play, item));
        }

        panelButtons.Children.Add(MakeRowButton(_vm.ShowFileInFolderCommand, IconNames.FolderOpen, Se.Language.General.OpenContainingFolder, item));

        var buttonCopy = MakeRowButton(_vm.CopyFilePathCommand, IconNames.Copy, Se.Language.General.Copy, item);
        buttonCopy.Click += async (_, _) =>
        {
            // Same copied-feedback as PromptFileSaved: flip to a check for a moment.
            Attached.SetIcon(buttonCopy, IconNames.Check);
            await Task.Delay(1500);
            Attached.SetIcon(buttonCopy, IconNames.Copy);
        };
        panelButtons.Children.Add(buttonCopy);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 12,
        };
        grid.Add(ball, 0, 0);
        grid.Add(panelText, 0, 1);
        grid.Add(panelButtons, 0, 2);

        return new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10, 8),
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            Child = grid,
        };
    }

    private static Button MakeRowButton(CommunityToolkit.Mvvm.Input.IRelayCommand command, string iconName, string hint, SavedFileItem item)
    {
        var button = UiUtil.MakeButton(command, iconName, hint);
        button.CommandParameter = item;
        button.Classes.Add("tool");
        button.Width = 28;
        button.Height = 28;
        return button;
    }

    private static Border MakeChip(string text, bool emphasized)
    {
        return new Border
        {
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(9, 1, 9, 2),
            VerticalAlignment = VerticalAlignment.Center,
            Background = ChipBackground,
            Child = new TextBlock
            {
                Text = text,
                FontSize = UiUtil.ScaledFontSize(11),
                FontWeight = emphasized ? FontWeight.SemiBold : FontWeight.Normal,
                Opacity = emphasized ? 0.9 : 0.75,
            },
        };
    }

    private static Border MakeChip(string textPropertyPath, string visiblePropertyPath, bool emphasized)
    {
        return new Border
        {
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(9, 1, 9, 2),
            VerticalAlignment = VerticalAlignment.Center,
            Background = ChipBackground,
            Child = new TextBlock
            {
                FontSize = UiUtil.ScaledFontSize(11),
                FontWeight = emphasized ? FontWeight.SemiBold : FontWeight.Normal,
                Opacity = emphasized ? 0.9 : 0.75,
                [!TextBlock.TextProperty] = new Binding(textPropertyPath) { Mode = BindingMode.OneWay },
            },
            [!Border.IsVisibleProperty] = new Binding(visiblePropertyPath) { Mode = BindingMode.OneWay },
        };
    }
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;

/// <summary>
/// Spell check dictionary download, in the editor look (see <see cref="EditorChrome"/>): the
/// dictionary picker and its details in a section, a progress strip while downloading, and
/// Close/Cancel + Download in the button bar with the download status on the left.
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var downloadButton = UiUtil.MakeButton(string.Empty, vm.DownloadCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.DownloadButtonText))
            .WithBindEnabled(nameof(vm.IsDownloadEnabled));
        downloadButton.Classes.Add("primary");
        AutomationProperties.SetName(downloadButton, Se.Language.General.Download);

        var uninstallButton = UiUtil.MakeButton(Se.Language.General.Uninstall, vm.UninstallCommand)
            .WithIconLeft(IconNames.Trash)
            .WithBindIsVisible(nameof(vm.IsUninstallVisible));
        AutomationProperties.SetName(uninstallButton, Se.Language.General.Uninstall);

        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand)
            .WithBindIsVisible(nameof(vm.IsDownloadEnabled));
        var cancel = UiUtil.MakeButtonCancel(vm.CancelCommand)
            .WithBindIsVisible(nameof(vm.IsProgressVisible));

        var sections = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.Dictionary, BuildDetails(vm), first: true),
            },
        };

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(new Border { Background = EditorChrome.PanelBrush, Child = sections }, 0, 0);
        root.Add(BuildProgress(vm), 1, 0);
        root.Add(EditorChrome.MakeDockedButtonBar(BuildStatus(vm), close, cancel, uninstallButton, downloadButton), 2, 0);
        Content = root;

        UiUtil.FocusOnFirstActivation(this, downloadButton); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static StackPanel BuildDetails(GetDictionariesViewModel vm)
    {
        var instructions = EditorChrome.MakeWrappingDimLabel(Se.Language.SpellCheck.GetDictionaryInstructions);
        instructions.FontSize = 12.5;
        instructions.MaxWidth = LabelWidth + ContentWidth + 12;
        instructions.HorizontalAlignment = HorizontalAlignment.Left;

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                // Auto (not a fixed width) so longer translations of the labels - e.g. Italian
                // "Cartella installazione" for "Install folder" - are not clipped (#12886).
                new ColumnDefinition { Width = GridLength.Auto, MinWidth = LabelWidth },
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
            RowSpacing = 10,
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
        AutomationProperties.SetName(combo, Se.Language.General.Dictionary);

        var description = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Top,
            MinHeight = 32,
            MaxWidth = ContentWidth,
            FontSize = 12.5,
            [!TextBox.DataContextProperty] = new Binding(nameof(vm.SelectedDictionary)),
            [!TextBlock.TextProperty] = new Binding(nameof(GetSpellCheckDictionaryDisplay.Description)),
        };

        var folder = new TextBox
        {
            IsReadOnly = true,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            MinHeight = 0,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            MaxWidth = ContentWidth - 36,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TextBox.TextProperty] = new Binding(nameof(vm.DictionariesFolder)),
        };
        AutomationProperties.SetName(folder, Se.Language.General.InstallFolder);

        var openFolder = EditorChrome.CreateToolButton(IconNames.FolderOpen, Se.Language.General.OpenDictionaryFolder, vm.OpenFolderCommand, 28);
        var folderRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { folder, openFolder },
        };

        grid.Add(MakeLabel(Se.Language.General.Dictionary), 0, 0);
        grid.Add(combo, 0, 1);

        grid.Add(MakeLabel(Se.Language.General.Status), 1, 0);
        grid.Add(MakeStatusPanel(nameof(vm.SelectedStatusText)), 1, 1);

        var labelDescription = MakeLabel(Se.Language.General.Description);
        labelDescription.VerticalAlignment = VerticalAlignment.Top; // lines up with the first line of a wrapped description
        grid.Add(labelDescription, 2, 0);
        grid.Add(description, 2, 1);

        grid.Add(MakeLabel(Se.Language.General.InstallFolder), 3, 0);
        grid.Add(folderRow, 3, 1);

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children = { instructions, grid },
        };
    }

    private static Border BuildProgress(GetDictionariesViewModel vm)
    {
        var bar = UiUtil.MakeProgressBar();
        bar.VerticalAlignment = VerticalAlignment.Center;
        bar[!ProgressBar.ValueProperty] = new Binding(nameof(vm.Progress));
        bar[!Visual.OpacityProperty] = new Binding(nameof(vm.ProgressOpacity));

        return new Border
        {
            Child = bar,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsProgressVisible)),
        };
    }

    /// <summary>
    /// In the button bar rather than the progress strip: "Download failed" stays after the strip
    /// is gone.
    /// </summary>
    private static TextBlock BuildStatus(GetDictionariesViewModel vm) => new()
    {
        FontSize = 12,
        Foreground = EditorChrome.DimTextBrush,
        VerticalAlignment = VerticalAlignment.Center,
        TextTrimming = TextTrimming.CharacterEllipsis,
        [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
    };

    private static FuncDataTemplate<GetSpellCheckDictionaryDisplay> BuildDictionaryItemTemplate()
    {
        return new FuncDataTemplate<GetSpellCheckDictionaryDisplay>((_, _) =>
        {
            var dot = new Ellipse
            {
                Width = 8,
                Height = 8,
                VerticalAlignment = VerticalAlignment.Center,
                [!Shape.FillProperty] = new Binding(nameof(GetSpellCheckDictionaryDisplay.IsInstalled))
                {
                    Converter = new BooleanToBrushConverter(),
                },
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

    private static StackPanel MakeStatusPanel(string textBindingPath)
    {
        var dot = new Ellipse
        {
            Width = 8,
            Height = 8,
            VerticalAlignment = VerticalAlignment.Center,
            [!Shape.FillProperty] = new Binding(nameof(GetDictionariesViewModel.SelectedDictionary) + "." +
                nameof(GetSpellCheckDictionaryDisplay.IsInstalled))
            {
                Converter = new BooleanToBrushConverter(),
            },
        };

        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            FontSize = 12.5,
            Margin = new Thickness(8, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding(textBindingPath),
        };

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { dot, text },
        };
    }

    private static TextBlock MakeLabel(string text) => EditorChrome.MakeDimLabel(text, new Thickness(0));
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.CrispEmbedSettings;

public class CrispEmbedSettingsWindow : Window
{
    private const int LabelWidth = 110;

    public CrispEmbedSettingsWindow(CrispEmbedSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.CrispEmbedSettingsTitle;

        // Explicit width plus height-only auto-sizing: SizeToContent.WidthAndHeight makes this
        // window far too wide on macOS.
        Width = 800;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        vm.Window = this;
        DataContext = vm;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.EngineDialog);

        var buttonClose = EditorChrome.AsPrimary(UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand));

        var sections = new StackPanel();
        sections.Children.Add(EditorChrome.MakeDialogHeader(CrispEmbedEngine.StaticName, Se.Language.Ocr.CrispEmbedDescription));
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Engine, BuildEngineSection(vm)));
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Models, BuildModelsSection(vm)));

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeButtonBar(null, buttonClose), stretch: false);

        UiUtil.FocusOnFirstActivation(this, buttonClose);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static StackPanel BuildEngineSection(CrispEmbedSettingsViewModel vm)
    {
        var engineButton = EditorChrome.AsFlatAction(UiUtil.MakeButton(string.Empty, vm.RedownloadEngineCommand)
            .WithIconLeftBindText(IconNames.CloudDownload, nameof(vm.EngineDownloadButtonText)));

        var enginePanel = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 8,
        };
        enginePanel.Add(EditorChrome.MakeStatusDot(nameof(vm.EngineBrush), nameof(vm.EngineLabel)), 0, 0);
        enginePanel.Add(engineButton, 0, 1);

        var folderText = new TextBox
        {
            IsReadOnly = true,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = EditorChrome.FieldFontSize,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.InstallFolder)),
        };
        AutomationProperties.SetName(folderText, Se.Language.General.InstallFolder);

        var folderPanel = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 8,
        };
        folderPanel.Add(folderText, 0, 0);
        folderPanel.Add(EditorChrome.AsFlatAction(UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenInstallFolderCommand)
            .WithIconLeft(IconNames.FolderOpen)), 0, 1);

        return new StackPanel
        {
            Spacing = 6,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.Status, enginePanel, LabelWidth, spacing: 10),
                EditorChrome.MakeLabeledRow(Se.Language.General.InstallFolder, folderPanel, LabelWidth, spacing: 10),
            },
        };
    }

    private static Border BuildModelsSection(CrispEmbedSettingsViewModel vm)
    {
        // Scrolls when the screen is too short for every model row
        return EditorChrome.MakeListFrame(new ScrollViewer
        {
            MaxHeight = 420,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = MakeModelList(vm),
        });
    }

    /// <summary>
    /// One row per downloadable model across all backends: backend, file name, size, install
    /// state, and its own download button. Everything is bound rather than read from the item
    /// passed to the template, as the template is reused across rows.
    /// </summary>
    private static ItemsControl MakeModelList(CrispEmbedSettingsViewModel vm)
    {
        return new ItemsControl
        {
            ItemsSource = vm.Models,
            Padding = new Thickness(0, 4),
            ItemTemplate = new FuncDataTemplate<CrispEmbedModelStatusViewModel>((_, _) =>
            {
                var backend = new TextBlock
                {
                    Width = 120,
                    FontSize = EditorChrome.FieldFontSize,
                    Foreground = EditorChrome.DimTextBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    [!TextBlock.TextProperty] = new Binding(nameof(CrispEmbedModelStatusViewModel.BackendName)),
                };
                var name = new TextBlock
                {
                    Width = 260,
                    FontSize = EditorChrome.FieldFontSize,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    [!TextBlock.TextProperty] = new Binding(nameof(CrispEmbedModelStatusViewModel.ModelName)),
                };
                var size = new TextBlock
                {
                    Width = 70,
                    FontSize = EditorChrome.FieldFontSize,
                    Foreground = EditorChrome.FaintTextBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(CrispEmbedModelStatusViewModel.SizeText)),
                };
                var status = EditorChrome.MakeStatusDot(nameof(CrispEmbedModelStatusViewModel.StatusBrush), nameof(CrispEmbedModelStatusViewModel.StatusLabel));
                status.Width = 110;

                // Fixed width so the buttons line up down the column - "Download" and
                // "Re-download" are different lengths and the rows mix both.
                // The accessible name carries the model, as the rows are a column of
                // identical "Download" buttons to a screen reader (#12087).
                var button = EditorChrome.AsFlatAction(UiUtil.MakeButton(string.Empty)
                    .WithIconLeftBindText(IconNames.Download,
                        nameof(CrispEmbedModelStatusViewModel.DownloadButtonText),
                        nameof(CrispEmbedModelStatusViewModel.DownloadButtonAccessibleName)));
                button.Width = 140;
                button.Height = 28;
                button.Bind(Button.CommandProperty,
                    new Binding(nameof(CrispEmbedModelStatusViewModel.DownloadCommand)));

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Margin = new Thickness(12, 1, 6, 1),
                    Children = { backend, name, size, status, button },
                };
            }, true),
        };
    }
}

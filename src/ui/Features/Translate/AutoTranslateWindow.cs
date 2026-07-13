using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using System.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Translate;

public class AutoTranslateWindow : Window
{
    private readonly AutoTranslateViewModel _vm;
    private Button? _buttonTranslate;
    private Button? _buttonOk;

    public AutoTranslateWindow(AutoTranslateViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.AutoTranslate;
        Width = 1050;
        MinWidth = 800;
        Height = 780;
        MinHeight = 480;

        DataContext = vm;
        vm.Window = this;
        _vm = vm;

        var dataGridCard = BuildDataGridCard(vm);
        var controlsCard = BuildControlsCard(vm);
        var apiConfigCard = BuildApiConfigCard(vm);
        var footer = BuildFooter(vm);

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto,Auto,Auto"),
            RowSpacing = 10,
            Margin = UiUtil.MakeWindowMargin(),
        };

        var row = 0;
        grid.Children.Add(dataGridCard);
        Grid.SetRow(dataGridCard, row++);

        grid.Children.Add(controlsCard);
        Grid.SetRow(controlsCard, row++);

        grid.Children.Add(apiConfigCard);
        Grid.SetRow(apiConfigCard, row++);

        grid.Children.Add(footer);
        Grid.SetRow(footer, row++);

        Content = grid;

        ApplyButtonAccentStates(vm);
        vm.PropertyChanged += OnViewModelPropertyChanged;

        Loaded += (s, e) => UiUtil.RestoreWindowPosition(this);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not AutoTranslateViewModel vm)
        {
            return;
        }

        if (e.PropertyName is nameof(AutoTranslateViewModel.IsTranslatePrimary)
            or nameof(AutoTranslateViewModel.IsOkPrimary))
        {
            ApplyButtonAccentStates(vm);
        }
    }

    private void ApplyButtonAccentStates(AutoTranslateViewModel vm)
    {
        SetAccent(_buttonTranslate, vm.IsTranslatePrimary);
        SetAccent(_buttonOk, vm.IsOkPrimary);
    }

    private static void SetAccent(Button? button, bool accent)
    {
        if (button == null)
        {
            return;
        }

        if (accent)
        {
            if (!button.Classes.Contains("accent"))
            {
                button.Classes.Add("accent");
            }
        }
        else
        {
            button.Classes.Remove("accent");
        }
    }

    private static Border BuildControlsCard(AutoTranslateViewModel vm)
    {
        var engineLabel = UiUtil.MakeTextBlock(Se.Language.General.Engine);
        engineLabel.VerticalAlignment = VerticalAlignment.Center;
        engineLabel.Margin = new Thickness(0, 0, 8, 0);

        var engineCombo = UiUtil.MakeComboBox(vm.AutoTranslators, vm, nameof(vm.SelectedAutoTranslator));
        engineCombo.MinWidth = 220;
        engineCombo.WithLabeledBy(engineLabel);
        engineCombo.ItemTemplate = BuildTranslatorItemTemplate();
        engineCombo.SelectionChanged += (s, e) =>
        {
            vm.AutoTranslatorChanged(engineCombo);
        };

        // Re-evaluate the engine combo's install-status dots after a download finishes - the
        // FuncDataTemplate caches each row's dot when first realised, so a fresh template is
        // the simplest way to refresh. The model combos already rebuild via PopulateModels.
        vm.RefreshDownloadDots = () => engineCombo.ItemTemplate = BuildTranslatorItemTemplate();

        // Appears only when the selected SE-managed engine (CrispASR/MADLAD or llama.cpp) is installed
        // but outdated - i.e. it shows the amber "update available" dot - giving the user a way to act on it.
        var updateEngineButton = UiUtil.MakeButton(Se.Language.General.Update, vm.UpdateEngineCommand)
            .WithIconLeft(IconNames.Download)
            .WithMarginLeft(8);
        updateEngineButton.VerticalAlignment = VerticalAlignment.Center;
        updateEngineButton.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.EngineUpdateButtonIsVisible)));
        ToolTip.SetTip(updateEngineButton, Se.Language.General.UpdateAvailable);

        var fromLabel = UiUtil.MakeTextBlock(Se.Language.General.From);
        fromLabel.VerticalAlignment = VerticalAlignment.Center;
        fromLabel.Margin = new Thickness(16, 0, 8, 0);

        var sourceCombo = UiUtil.MakeComboBox(vm.SourceLanguages!, vm, nameof(vm.SelectedSourceLanguage));
        sourceCombo.MinWidth = 180;
        sourceCombo.WithLabeledBy(fromLabel);

        var swapButton = UiUtil.MakeButton(vm.SwapLanguagesCommand, IconNames.SwapVertical, Se.Language.Translate.SwapLanguages);
        swapButton.Margin = new Thickness(8, 0);
        swapButton.VerticalAlignment = VerticalAlignment.Center;

        var toLabel = UiUtil.MakeTextBlock(Se.Language.General.To);
        toLabel.VerticalAlignment = VerticalAlignment.Center;
        toLabel.Margin = new Thickness(0, 0, 8, 0);

        var targetCombo = UiUtil.MakeComboBox(vm.TargetLanguages!, vm, nameof(vm.SelectedTargetLanguage));
        targetCombo.MinWidth = 180;
        targetCombo.WithLabeledBy(toLabel);

        var controlsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                engineLabel,
                engineCombo,
                updateEngineButton,
                fromLabel,
                sourceCombo,
                swapButton,
                toLabel,
                targetCombo,
            },
        };

        var poweredByLabel = UiUtil.MakeTextBlock(Se.Language.General.PoweredBy);
        poweredByLabel.Foreground = UiUtil.GetTextColor(0.65);
        poweredByLabel.FontSize = 11;
        poweredByLabel.VerticalAlignment = VerticalAlignment.Center;

        var poweredByLink = UiUtil.MakeLink("Google Translate V1", vm.GoToAutoTranslatorUriCommand, vm, nameof(vm.AutoTranslatorLinkText));
        poweredByLink.FontSize = 11;
        poweredByLink.VerticalAlignment = VerticalAlignment.Center;

        var poweredByPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 0, 6),
            Children = { poweredByLabel, poweredByLink },
        };

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { poweredByPanel, controlsPanel },
        };

        return MakeCard(stack);
    }

    private static FuncDataTemplate<IAutoTranslator> BuildTranslatorItemTemplate()
    {
        return StatusDots.ComboItemTemplate<IAutoTranslator>(
            translator => translator.Name,
            _ => null,
            GetTranslatorDotStatus);
    }

    // Install-status dot for the auto-translate engine combo. Only the two engines that Subtitle
    // Edit downloads itself - llama.cpp and CrispASR/MADLAD - get a dot; cloud/API translators
    // (Google, DeepL, ChatGPT, ...) and externally-hosted servers have nothing to install.
    private static DownloadDotStatus GetTranslatorDotStatus(IAutoTranslator translator)
    {
        switch (translator)
        {
            case LlamaCppTranslate:
                return StatusDots.From(
                    LlamaCppServerManager.IsEngineInstalled(),
                    LlamaCppUpdateStatus.GetEngineUpdateStatus());
            case CrispAsrMadladTranslate:
                var crispAsr = new CrispAsrMadlad();
                if (!crispAsr.IsEngineInstalled())
                {
                    return DownloadDotStatus.NotInstalled;
                }

                return StatusDots.From(true, DownloadHashManager.GetSidecarStatus(crispAsr.GetAndCreateWhisperFolder()));
            default:
                return DownloadDotStatus.None;
        }
    }

    // A custom *.gguf the user dropped into the models folder has no Url - it is already on disk,
    // so it shows a green dot and a "custom" size tag rather than a download size.
    private static string? GetLlamaCppModelSize(LlamaCppModelDisplay model)
    {
        if (string.IsNullOrEmpty(model.Model.Url))
        {
            var custom = Se.Language.General.Custom;
            return string.IsNullOrEmpty(model.Model.Size) ? custom : $"{custom}, {model.Model.Size}";
        }

        return string.IsNullOrEmpty(model.Model.Size) ? null : model.Model.Size;
    }

    private static DownloadDotStatus GetLlamaCppModelDotStatus(LlamaCppModelDisplay model)
    {
        if (string.IsNullOrEmpty(model.Model.Url) || LlamaCppServerManager.IsModelInstalled(model.Model))
        {
            return DownloadDotStatus.UpToDate;
        }

        return DownloadDotStatus.NotInstalled;
    }

    private static Border BuildApiConfigCard(AutoTranslateViewModel vm)
    {
        var buttonDownloadCrispAsr = UiUtil.MakeButton(string.Empty, vm.DownloadCrispAsrCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.CrispAsrDownloadButtonText))
            .WithMarginLeft(5);
        buttonDownloadCrispAsr.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.ButtonDownloadIsVisible)));

        var crispAsrModelCombo = UiUtil.MakeComboBox(vm.CrispAsrModels, vm, nameof(vm.SelectedCrispAsrModel), nameof(vm.CrispAsrModelComboIsVisible));
        crispAsrModelCombo.ItemTemplate = StatusDots.ComboItemTemplate<SpeechToTextModelDisplay>(
            model => model.Model.Name,
            model => string.IsNullOrEmpty(model.Model.Size) ? null : model.Model.Size,
            model => model.Engine.IsModelInstalled(model.Model)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled);
        crispAsrModelCombo.WithAccessibleName(Se.Language.General.Model);

        var llamaCppModelCombo = UiUtil.MakeComboBox(vm.LlamaCppModels, vm, nameof(vm.SelectedLlamaCppModel), nameof(vm.LlamaCppModelComboIsVisible)).WithWidth(220);
        llamaCppModelCombo.ItemTemplate = StatusDots.ComboItemTemplate<LlamaCppModelDisplay>(
            model => model.Model.DisplayName,
            GetLlamaCppModelSize,
            GetLlamaCppModelDotStatus);
        llamaCppModelCombo.WithAccessibleName(Se.Language.General.Model);

        var buttonDownloadLlamaCpp = UiUtil.MakeButton(string.Empty, vm.DownloadLlamaCppCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.LlamaCppDownloadButtonText))
            .WithMarginLeft(5);
        buttonDownloadLlamaCpp.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppButtonsAreVisible)));

        var buttonLlamaCppServer = UiUtil.MakeButton(string.Empty, vm.ToggleLlamaCppServerCommand).WithMarginLeft(5);
        buttonLlamaCppServer.Bind(Button.ContentProperty, new Binding(nameof(vm.LlamaCppServerButtonText)));
        buttonLlamaCppServer.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppButtonsAreVisible)));

        var buttonLlamaCppOpenFolder = UiUtil.MakeButton(vm.OpenLlamaCppModelsFolderCommand, IconNames.FolderOpen, Se.Language.General.OpenContainingFolder)
            .WithMarginLeft(5);
        buttonLlamaCppOpenFolder.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppButtonsAreVisible)));

        var settingsPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
        };

        settingsPanel.Children.Add(UiUtil.MakeTextBlock(Se.Language.General.Id, vm, null, nameof(vm.ApiIdIsVisible)).WithMarginRight(5));
        settingsPanel.Children.Add(UiUtil.MakeTextBox(150, vm, nameof(vm.ApiIdText), nameof(vm.ApiIdIsVisible)).WithMarginRight(15).WithAccessibleName(Se.Language.General.Id));

        settingsPanel.Children.Add(UiUtil.MakeTextBlock(Se.Language.General.ApiSecret, vm, null, nameof(vm.ApiSecretIsVisible)).WithMarginRight(5));
        settingsPanel.Children.Add(UiUtil.MakeTextBox(150, vm, nameof(vm.ApiSecretText), nameof(vm.ApiSecretIsVisible)).WithMarginRight(15).WithAccessibleName(Se.Language.General.ApiSecret));

        settingsPanel.Children.Add(UiUtil.MakeTextBlock(Se.Language.General.ApiKey, vm, null, nameof(vm.ApiKeyIsVisible)).WithMarginRight(5));
        var panelApiKey = UiUtil.MakeApiKeyTextBox(150, vm, nameof(vm.ApiKeyText), nameof(vm.ApiKeyIsVisible));
        panelApiKey.Margin = new Thickness(0, 0, 15, 0);
        settingsPanel.Children.Add(panelApiKey);

        settingsPanel.Children.Add(UiUtil.MakeTextBlock(Se.Language.General.Formality, vm, null, nameof(vm.FormalityIsVisible)).WithMarginRight(5));
        settingsPanel.Children.Add(UiUtil.MakeComboBox(vm.Formalities, vm, nameof(vm.SelectedFormality), nameof(vm.FormalityIsVisible)).WithWidth(220).WithMarginRight(15).WithAccessibleName(Se.Language.General.Formality));

        var checkBoxLlamaCppRemote = UiUtil.MakeCheckBox(Se.Language.General.LlamaCppUseRemoteServer, vm, nameof(vm.LlamaCppUseRemoteServer)).WithMarginRight(15);
        checkBoxLlamaCppRemote.Bind(CheckBox.IsVisibleProperty, new Binding(nameof(vm.LlamaCppRemoteToggleIsVisible)));
        settingsPanel.Children.Add(checkBoxLlamaCppRemote);

        settingsPanel.Children.Add(UiUtil.MakeTextBlock(Se.Language.General.Url, vm, null, nameof(vm.ApiUrlIsVisible)).WithMarginRight(5));
        settingsPanel.Children.Add(UiUtil.MakeTextBox(200, vm, nameof(vm.ApiUrlText), nameof(vm.ApiUrlIsVisible)).WithMarginRight(15).WithAccessibleName(Se.Language.General.Url));

        settingsPanel.Children.Add(UiUtil.MakeTextBlock(Se.Language.General.Model, vm, null, nameof(vm.ModelIsVisible)).WithMarginRight(5));
        settingsPanel.Children.Add(UiUtil.MakeTextBox(150, vm, nameof(vm.ModelText), nameof(vm.ModelIsVisible)).WithAccessibleName(Se.Language.General.Model));
        settingsPanel.Children.Add(UiUtil.MakeButtonBrowse(vm.BrowseModelCommand, nameof(vm.ModelBrowseIsVisible), Se.Language.General.Model).WithMarginLeft(5));

        settingsPanel.Children.Add(UiUtil.MakeTextBlock(Se.Language.General.Model, vm, null, nameof(vm.CrispAsrModelComboIsVisible)).WithMarginRight(5));
        settingsPanel.Children.Add(crispAsrModelCombo);
        settingsPanel.Children.Add(buttonDownloadCrispAsr);

        settingsPanel.Children.Add(UiUtil.MakeTextBlock(Se.Language.General.Model, vm, null, nameof(vm.LlamaCppModelComboIsVisible)).WithMarginRight(5));
        settingsPanel.Children.Add(llamaCppModelCombo);
        settingsPanel.Children.Add(buttonDownloadLlamaCpp);
        settingsPanel.Children.Add(buttonLlamaCppServer);
        settingsPanel.Children.Add(buttonLlamaCppOpenFolder);

        var settingsButton = UiUtil.MakeButton(vm.OpenSettingsCommand, IconNames.Settings, Se.Language.General.Settings);
        settingsButton.HorizontalAlignment = HorizontalAlignment.Right;
        settingsButton.VerticalAlignment = VerticalAlignment.Center;
        settingsButton.Margin = new Thickness(8, 0, 0, 0);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
        };
        grid.Children.Add(settingsPanel);
        Grid.SetColumn(settingsPanel, 0);
        grid.Children.Add(settingsButton);
        Grid.SetColumn(settingsButton, 1);

        return MakeCard(grid);
    }

    private Border BuildDataGridCard(AutoTranslateViewModel vm)
    {
        var contextMenu = new MenuFlyout
        {
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.General.TranslateRow,
                    Command = vm.TranslateRowCommand,
                },
            }
        };

        var dataGrid = new DataGrid
        {
            Height = double.NaN,
            CanUserSortColumns = false,
            ContextFlyout = contextMenu,
            DataContext = vm,
            IsReadOnly = true,
            AutoGenerateColumns = false,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    Binding = new Binding(nameof(TranslateRow.Number)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    Binding = new Binding(nameof(TranslateRow.Show)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Duration,
                    Binding = new Binding(nameof(TranslateRow.Duration)),
                    Width = new DataGridLength(80),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Text,
                    Binding = new Binding(nameof(TranslateRow.Text)),
                    Width = new DataGridLength(200, DataGridLengthUnitType.Star),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Translation,
                    Binding = new Binding(nameof(TranslateRow.TranslatedText)),
                    Width = new DataGridLength(200, DataGridLengthUnitType.Star),
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                }
            }
        };

        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Rows)));
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedTranslateRow)));
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);
        dataGrid.WithAccessibleName(Se.Language.General.Lines);
        vm.RowGrid = dataGrid;

        var dataGridBorder = UiUtil.MakeBorderForControlNoPadding(dataGrid);
        return MakeCard(dataGridBorder, new Thickness(1));
    }

    private Control BuildFooter(AutoTranslateViewModel vm)
    {
        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Height = 6,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(3),
        };
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsProgressEnabled)));
        progressBar.WithAccessibleName(Se.Language.General.Translation);

        var progressLabel = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10, 0, 0, 0),
            MinWidth = 50,
            Foreground = UiUtil.GetTextColor(0.75),
        };
        progressLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));
        progressLabel.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsProgressEnabled)));

        var progressGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(0, 0, 0, 10),
            MinHeight = 8,
        };
        progressGrid.Children.Add(progressBar);
        Grid.SetColumn(progressBar, 0);
        progressGrid.Children.Add(progressLabel);
        Grid.SetColumn(progressLabel, 1);

        _buttonTranslate = UiUtil.MakeButton(Se.Language.General.Translate, vm.TranslateCommand);
        _buttonTranslate.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsTranslateEnabled)));

        _buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        _buttonOk.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsOkEnabled)));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var buttonBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Children =
            {
                _buttonTranslate,
                buttonCancel,
                _buttonOk,
            }
        };

        var footerGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto"),
        };
        footerGrid.Children.Add(progressGrid);
        Grid.SetRow(progressGrid, 0);
        footerGrid.Children.Add(buttonBar);
        Grid.SetRow(buttonBar, 1);

        return footerGrid;
    }

    private static Border MakeCard(Control child, Thickness? padding = null)
    {
        return new Border
        {
            Child = child,
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetTextColor(0.18),
            Background = UiUtil.GetTextColor(0.04),
            Padding = padding ?? new Thickness(14, 12),
            CornerRadius = new CornerRadius(8),
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.KeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (DataContext is AutoTranslateViewModel vm)
        {
            vm.SaveSettings();
            vm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        UiUtil.SaveWindowPosition(this);
    }
}

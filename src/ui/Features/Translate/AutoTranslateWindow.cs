using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Platform.Progress;
using System.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// Auto-translate in the editor look (see <see cref="EditorChrome"/>): engine and languages in a
/// toolbar on top, the selected engine's settings (API key, URL, model, llama.cpp server) in a
/// strip under it, the lines in the middle, then the progress strip and the button bar.
/// </summary>
public class AutoTranslateWindow : Window
{
    private readonly AutoTranslateViewModel _vm;
    private Button? _buttonTranslate;
    private Button? _buttonOk;

    public AutoTranslateWindow(AutoTranslateViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.AutoTranslate);
        Width = 1050;
        MinWidth = 800;
        Height = 780;
        MinHeight = 480;
        // The Edit dialogs' styles: compact combos and text fields that keep the field color while focused
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        // The editable model combo's text box: as compact as the other fields, text centered
        Styles.Add(new Style(x => x.OfType<ComboBox>().Template().OfType<TextBox>().Name("PART_EditableTextBox"))
        {
            Setters =
            {
                new Setter(MinHeightProperty, 0.0),
                new Setter(TextBox.PaddingProperty, new Thickness(0)),
                new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center),
            },
        });

        DataContext = vm;
        vm.Window = this;
        _vm = vm;

        var toolbar = BuildToolbar(vm);
        var engineSettings = BuildEngineSettingsStrip(vm);
        var rows = BuildRowGrid(vm);
        var progress = BuildProgressStrip(vm);
        var buttonBar = BuildButtonBar(vm);

        // The lines give up height first, so the progress strip and the buttons stay reachable on
        // a short screen.
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*,Auto,Auto"),
        };
        grid.Add(toolbar, 0, 0);
        grid.Add(engineSettings, 1, 0);
        grid.Add(rows, 2, 0);
        grid.Add(progress, 3, 0);
        grid.Add(buttonBar, 4, 0);

        Content = grid;

        ApplyButtonAccentStates(vm);
        vm.PropertyChanged += OnViewModelPropertyChanged;

        AddHandler(KeyDownEvent, (_, e) => _vm.PreviewKeyDown(e), RoutingStrategies.Tunnel, handledEventsToo: false);

        Loaded += (s, e) => UiUtil.RestoreWindowPosition(this);

        // Start out on the accented button so it is selected, not just coloured like it - Enter
        // then starts the translation right away. First activation only: coming back from a
        // dialog must not pull focus away from where the user left it.
        UiUtil.FocusOnFirstActivation(this, () => _buttonTranslate?.Focus());
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
        // The accent marks the next step: Translate until something is translated, then OK.
        _buttonTranslate?.Classes.Set("primary", vm.IsTranslatePrimary);
        _buttonOk?.Classes.Set("primary", vm.IsOkPrimary);

        // A focused button answers Enter itself, so let the focus follow the accent when OK takes
        // over as the default button after a translation - otherwise Enter would keep translating.
        // Posted: this runs from the first of the property changes that flip the two buttons, and
        // OK is still disabled (hence unfocusable) until its own IsEnabled binding has caught up.
        if (vm.IsOkPrimary && _buttonTranslate?.IsFocused == true)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_vm.IsOkPrimary && _buttonTranslate?.IsFocused == true)
                {
                    _buttonOk?.Focus();
                }
            });
        }
    }

    /// <summary>
    /// Engine (with its update button), source language, swap, target language, and the
    /// translate settings on the right.
    /// </summary>
    private static Border BuildToolbar(AutoTranslateViewModel vm)
    {
        var engineLabel = EditorChrome.MakeDimLabel(Se.Language.General.Engine, new Thickness(4, 0, 0, 0));

        var engineCombo = UiUtil.MakeComboBox(vm.AutoTranslators, vm, nameof(vm.SelectedAutoTranslator));
        engineCombo.MinWidth = 210;
        engineCombo.WithLabeledBy(engineLabel);
        engineCombo.WithAccessibleName(Se.Language.General.Engine);
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
        var updateEngineButton = Flat(UiUtil.MakeButton(Se.Language.General.Update, vm.UpdateEngineCommand)
            .WithIconLeft(IconNames.Download));
        updateEngineButton.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.EngineUpdateButtonIsVisible)));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(updateEngineButton, Se.Language.General.UpdateAvailable);
        }

        var fromLabel = EditorChrome.MakeDimLabel(Se.Language.General.From, new Thickness(0));
        var sourceCombo = UiUtil.MakeComboBox(vm.SourceLanguages!, vm, nameof(vm.SelectedSourceLanguage));
        sourceCombo.MinWidth = 160;
        sourceCombo.WithLabeledBy(fromLabel);
        sourceCombo.WithAccessibleName(Se.Language.General.From);

        var swapButton = EditorChrome.CreateToolButton("fa-solid fa-right-left", Se.Language.Translate.SwapLanguages, vm.SwapLanguagesCommand, 28);

        var toLabel = EditorChrome.MakeDimLabel(Se.Language.General.To, new Thickness(0));
        var targetCombo = UiUtil.MakeComboBox(vm.TargetLanguages!, vm, nameof(vm.SelectedTargetLanguage));
        targetCombo.MinWidth = 160;
        targetCombo.WithLabeledBy(toLabel);
        targetCombo.WithAccessibleName(Se.Language.General.To);

        var settingsButton = EditorChrome.CreateToolButton(IconNames.Settings, Se.Language.General.Settings, vm.OpenSettingsCommand, 30);

        var toolbarGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,Auto,*,Auto"),
            ColumnSpacing = 8,
        };
        toolbarGrid.Add(engineLabel, 0, 0);
        toolbarGrid.Add(engineCombo, 0, 1);
        toolbarGrid.Add(updateEngineButton, 0, 2);
        toolbarGrid.Add(EditorChrome.MakeToolbarSeparator(), 0, 3);
        toolbarGrid.Add(fromLabel, 0, 4);
        toolbarGrid.Add(sourceCombo, 0, 5);
        toolbarGrid.Add(swapButton, 0, 6);
        toolbarGrid.Add(toLabel, 0, 7);
        toolbarGrid.Add(targetCombo, 0, 8);
        toolbarGrid.Add(settingsButton, 0, 10);

        var toolbar = EditorChrome.MakeToolbar(toolbarGrid);
        toolbar.Padding = new Thickness(12, 8);
        return toolbar;
    }

    // Shared with batch convert's auto-translate view - see AutoTranslateCombos.
    private static FuncDataTemplate<IAutoTranslator> BuildTranslatorItemTemplate()
    {
        return AutoTranslateCombos.EngineItemTemplate();
    }

    /// <summary>
    /// The selected engine's own settings - API id/secret/key, formality, URL, model, and the
    /// llama.cpp model/server buttons. Each piece shows only for the engines that use it, and the
    /// whole strip goes away for engines without settings (Google Translate V1, ...).
    /// </summary>
    private static Border BuildEngineSettingsStrip(AutoTranslateViewModel vm)
    {
        var buttonDownloadCrispAsr = Flat(UiUtil.MakeButton(string.Empty, vm.DownloadCrispAsrCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.CrispAsrDownloadButtonText)));
        buttonDownloadCrispAsr.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.ButtonDownloadIsVisible)));

        var crispAsrModelCombo = UiUtil.MakeComboBox(vm.CrispAsrModels, vm, nameof(vm.SelectedCrispAsrModel), nameof(vm.CrispAsrModelComboIsVisible));
        crispAsrModelCombo.ItemTemplate = AutoTranslateCombos.CrispAsrModelItemTemplate();
        crispAsrModelCombo.WithAccessibleName(Se.Language.General.Model);

        var llamaCppModelCombo = UiUtil.MakeComboBox(vm.LlamaCppModels, vm, nameof(vm.SelectedLlamaCppModel), nameof(vm.LlamaCppModelComboIsVisible)).WithWidth(260);
        llamaCppModelCombo.ItemTemplate = AutoTranslateCombos.LlamaCppModelItemTemplate();
        llamaCppModelCombo.WithAccessibleName(Se.Language.General.Model);

        var buttonDownloadLlamaCpp = Flat(UiUtil.MakeButton(string.Empty, vm.DownloadLlamaCppCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.LlamaCppDownloadButtonText)));
        buttonDownloadLlamaCpp.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppButtonsAreVisible)));

        var buttonLlamaCppServer = Compact(UiUtil.MakeButton(string.Empty, vm.ToggleLlamaCppServerCommand));
        buttonLlamaCppServer.Bind(Button.ContentProperty, new Binding(nameof(vm.LlamaCppServerButtonText)));
        buttonLlamaCppServer.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppButtonsAreVisible)));
        if (Se.Settings.Appearance.ShowHints)
        {
            // The local server's live endpoint (random port) - see LlamaCppServerUrlInfo.
            buttonLlamaCppServer.Bind(ToolTip.TipProperty, new Binding(nameof(vm.LlamaCppServerUrlInfo)));
        }

        var buttonLlamaCppOpenFolder = EditorChrome.CreateToolButton(IconNames.FolderOpen, Se.Language.General.OpenContainingFolder,
            vm.OpenLlamaCppModelsFolderCommand, 28);
        buttonLlamaCppOpenFolder.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppButtonsAreVisible)));

        var buttonLlamaCppEngineSettings = EditorChrome.CreateToolButton(IconNames.Settings, Se.Language.General.LlamaCppEngineSettings,
            vm.ShowLlamaCppEngineSettingsCommand, 28);
        buttonLlamaCppEngineSettings.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppButtonsAreVisible)));

        var buttonLlamaCppAdvancedSettings = Flat(UiUtil.MakeButton(Se.Language.General.AdvancedDotDotDot, vm.ShowLlamaCppAdvancedSettingsCommand)
            .WithAccessibleName(Se.Language.General.AdvancedSettings));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonLlamaCppAdvancedSettings, Se.Language.General.AdvancedSettings);
        }
        buttonLlamaCppAdvancedSettings.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppAdvancedButtonIsVisible)));

        // Groups of label + input(s), so a label never wraps away from its field.
        var settingsPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            ItemSpacing = 16,
            LineSpacing = 8,
        };

        settingsPanel.Children.Add(Group(nameof(vm.ApiIdIsVisible), Label(Se.Language.General.Id),
            Text(UiUtil.MakeTextBox(150, vm, nameof(vm.ApiIdText)).WithAccessibleName(Se.Language.General.Id))));

        settingsPanel.Children.Add(Group(nameof(vm.ApiSecretIsVisible), Label(Se.Language.General.ApiSecret),
            Text(UiUtil.MakeTextBox(150, vm, nameof(vm.ApiSecretText)).WithAccessibleName(Se.Language.General.ApiSecret))));

        var panelApiKey = UiUtil.MakeApiKeyTextBox(150, vm, nameof(vm.ApiKeyText));
        foreach (var child in panelApiKey.Children)
        {
            if (child is TextBox textBox)
            {
                Text(textBox);
            }
            else if (child is Button button)
            {
                button.Classes.Add("tool");
                button.Width = 28;
                button.Height = 28;
            }
        }
        settingsPanel.Children.Add(Group(nameof(vm.ApiKeyIsVisible), Label(Se.Language.General.ApiKey), panelApiKey));

        settingsPanel.Children.Add(Group(nameof(vm.FormalityIsVisible), Label(Se.Language.General.Formality),
            UiUtil.MakeComboBox(vm.Formalities, vm, nameof(vm.SelectedFormality)).WithWidth(200).WithAccessibleName(Se.Language.General.Formality)));

        var switchLlamaCppRemote = EditorChrome.MakeSwitch(nameof(vm.LlamaCppUseRemoteServer), Se.Language.General.LlamaCppUseRemoteServer);
        var rowLlamaCppRemote = EditorChrome.MakeSwitchRow(switchLlamaCppRemote, Se.Language.General.LlamaCppUseRemoteServer);
        rowLlamaCppRemote.VerticalAlignment = VerticalAlignment.Center;
        rowLlamaCppRemote.Bind(IsVisibleProperty, new Binding(nameof(vm.LlamaCppRemoteToggleIsVisible)));
        settingsPanel.Children.Add(rowLlamaCppRemote);

        // Wide enough for typical ".../v1/chat/completions" endpoints, with the full value in a
        // tooltip - a clipped URL hid a broken endpoint in #12907.
        var textBoxApiUrl = Text(UiUtil.MakeTextBox(300, vm, nameof(vm.ApiUrlText)).WithAccessibleName(Se.Language.General.Url));
        if (Se.Settings.Appearance.ShowHints)
        {
            textBoxApiUrl.Bind(ToolTip.TipProperty, new Binding(nameof(vm.ApiUrlText)));
        }
        settingsPanel.Children.Add(Group(nameof(vm.ApiUrlIsVisible), Label(Se.Language.General.Url), textBoxApiUrl));

        var textBoxModel = Text(UiUtil.MakeTextBox(150, vm, nameof(vm.ModelText), nameof(vm.ModelTextBoxIsVisible)).WithAccessibleName(Se.Language.General.Model));

        // The engines that know their models offer them in a drop-down; any other name can still be typed.
        var modelCombo = UiUtil.MakeEditableComboBox(220, System.Array.Empty<string>(), vm, nameof(vm.ModelText)).WithAccessibleName(Se.Language.General.Model);
        modelCombo.Bind(ComboBox.ItemsSourceProperty, new Binding(nameof(vm.ModelPresets)));
        modelCombo.Bind(ComboBox.IsVisibleProperty, new Binding(nameof(vm.ModelComboIsVisible)));
        var buttonBrowseModel = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.Model, vm.BrowseModelCommand, 28);
        buttonBrowseModel.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.ModelBrowseIsVisible)));
        var labelModel = Label(Se.Language.General.Model);
        labelModel.Bind(IsVisibleProperty, new Binding(nameof(vm.ModelIsVisible)));
        var groupModel = Group(nameof(vm.ModelIsVisible), labelModel, textBoxModel, modelCombo, buttonBrowseModel);
        // The browse button has its own visibility, so the group shows for either
        groupModel.Bind(IsVisibleProperty, new MultiBinding
        {
            Converter = BoolConverters.Or,
            Bindings = { new Binding(nameof(vm.ModelIsVisible)), new Binding(nameof(vm.ModelBrowseIsVisible)) },
        });
        settingsPanel.Children.Add(groupModel);

        settingsPanel.Children.Add(Group(nameof(vm.CrispAsrModelComboIsVisible), Label(Se.Language.General.Model), crispAsrModelCombo));
        settingsPanel.Children.Add(buttonDownloadCrispAsr);

        settingsPanel.Children.Add(Group(nameof(vm.LlamaCppModelComboIsVisible), Label(Se.Language.General.Model), llamaCppModelCombo));
        var llamaCppButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                buttonDownloadLlamaCpp,
                buttonLlamaCppServer,
                buttonLlamaCppOpenFolder,
                buttonLlamaCppEngineSettings,
                buttonLlamaCppAdvancedSettings,
            },
        };
        settingsPanel.Children.Add(llamaCppButtons);

        var strip = new Border
        {
            Child = settingsPanel,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(16, 8),
        };

        // An empty strip would just be a thin band - hide it when the engine shows no settings.
        var anySetting = new MultiBinding { Converter = BoolConverters.Or };
        foreach (var property in new[]
                 {
                     nameof(vm.ApiIdIsVisible), nameof(vm.ApiSecretIsVisible), nameof(vm.ApiKeyIsVisible),
                     nameof(vm.FormalityIsVisible), nameof(vm.LlamaCppRemoteToggleIsVisible), nameof(vm.ApiUrlIsVisible),
                     nameof(vm.ModelIsVisible), nameof(vm.ModelBrowseIsVisible), nameof(vm.CrispAsrModelComboIsVisible), nameof(vm.ButtonDownloadIsVisible),
                     nameof(vm.LlamaCppModelComboIsVisible), nameof(vm.LlamaCppButtonsAreVisible), nameof(vm.LlamaCppAdvancedButtonIsVisible),
                 })
        {
            anySetting.Bindings.Add(new Binding(property));
        }

        strip.Bind(IsVisibleProperty, anySetting);
        return strip;
    }

    private Border BuildRowGrid(AutoTranslateViewModel vm)
    {
        var menuItemTranslateRow = new MenuItem
        {
            Header = Se.Language.General.TranslateRow,
            Command = vm.TranslateRowCommand,
        };
        var menuItemTranslateRows = new MenuItem
        {
            Header = Se.Language.General.TranslateRows,
            Command = vm.TranslateRowsCommand,
        };
        var contextMenu = new MenuFlyout
        {
            Items =
            {
                menuItemTranslateRow,
                menuItemTranslateRows,
            }
        };

        var tableView = TableViewExtras.MakeTableView();
        contextMenu.Opening += (_, _) =>
        {
            // "Translate rows" only when more than one row is selected - one row keeps "Translate row".
            var multipleSelected = (tableView.SelectedItems?.Count ?? 0) > 1;
            menuItemTranslateRow.IsVisible = !multipleSelected;
            menuItemTranslateRows.IsVisible = multipleSelected;
        };
        tableView.Height = double.NaN;
        tableView.ContextFlyout = contextMenu;
        tableView.DataContext = vm;

        // The DataGrid sized the number and show columns to content (Auto); TableView
        // treats Auto as star, so they get fixed widths instead.
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(TranslateRow.Number)),
            Width = new GridLength(60),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            Binding = new Binding(nameof(TranslateRow.Show)),
            Width = new GridLength(110),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            Binding = new Binding(nameof(TranslateRow.Duration)),
            Width = new GridLength(80),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(TranslateRow.Text)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Translation,
            // Editable in place: a click on the selected row's translation opens a TextBox, so a
            // slip in the machine translation is fixed here instead of after closing the window.
            // The display stays a binding, so rows keep updating while a translation runs; editing
            // is gated to when no translation is running, as the engine writes TranslatedText then.
            CellTemplate = new FuncDataTemplate<TranslateRow>((row, _nameScope) =>
            {
                if (row == null)
                {
                    return new Border();
                }

                var cell = new Border { Background = Brushes.Transparent };
                _ = new TableViewInlineTextEditor(cell, tableView,
                    () => row.TranslatedText,
                    text =>
                    {
                        row.TranslatedText = text;
                        vm.HasTranslatedSomething = true; // an edited translation is something to keep - enables OK
                    },
                    () => TableViewExtras.MakeTextCellTemplate(nameof(TranslateRow.TranslatedText)).Build(row)!,
                    canEdit: () => vm.IsTranslateEnabled,
                    hint: Se.Language.Translate.EditTranslationHint);
                return cell;
            }),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        tableView.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Rows)));
        tableView.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedTranslateRow)));
        UiUtil.AttachMacContextFlyoutHandler(tableView);
        tableView.WithAccessibleName(Se.Language.General.Lines);
        vm.RowGrid = tableView;

        var frame = EditorChrome.MakeListFrame(tableView);
        frame.Margin = new Thickness(14, 12);
        return EditorChrome.MakePanel(frame);
    }

    /// <summary>
    /// Status, progress bar and percentage on the status color, while translating.
    /// </summary>
    private Border BuildProgressStrip(AutoTranslateViewModel vm)
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
        progressBar.WithPlatformProgress(this, nameof(vm.IsProgressEnabled));
        progressBar.WithAccessibleName(Se.Language.General.Translation);

        var statusText = EditorChrome.MakeBoundHint(nameof(vm.StatusText));
        statusText.FontSize = EditorChrome.FieldFontSize;
        statusText.MaxWidth = 300;

        var progressLabel = EditorChrome.MakeBoundHint(nameof(vm.ProgressText));
        progressLabel.FontSize = EditorChrome.FieldFontSize;
        progressLabel.MinWidth = 40;
        progressLabel.TextAlignment = TextAlignment.Right;

        var progressRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*,Auto"),
            ColumnSpacing = 10,
        };
        progressRow.Add(new Optris.Icons.Avalonia.Icon
        {
            Value = "mdi-translate",
            FontSize = 14,
            Foreground = EditorChrome.IconBrush,
            VerticalAlignment = VerticalAlignment.Center,
        }, 0, 0);
        progressRow.Add(statusText, 0, 1);
        progressRow.Add(progressBar, 0, 2);
        progressRow.Add(progressLabel, 0, 3);

        return new Border
        {
            Child = progressRow,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 7),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsProgressEnabled)),
        };
    }

    /// <summary>
    /// "Powered by" and the translate-in-place switch on the left; Translate, Cancel and OK on the
    /// right, the accent on the next step.
    /// </summary>
    private Border BuildButtonBar(AutoTranslateViewModel vm)
    {
        var poweredByLabel = EditorChrome.MakeDimLabel(Se.Language.General.PoweredBy, new Thickness(0));
        poweredByLabel.FontSize = EditorChrome.ScaledFont(11.5);

        var poweredByLink = UiUtil.MakeLink("Google Translate V1", vm.GoToAutoTranslatorUriCommand, vm, nameof(vm.AutoTranslatorLinkText));
        poweredByLink.FontSize = EditorChrome.ScaledFont(11.5);
        poweredByLink.VerticalAlignment = VerticalAlignment.Center;

        var poweredByPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { poweredByLabel, poweredByLink },
        };

        var switchTranslateInPlace = EditorChrome.MakeSwitch(nameof(vm.TranslateInPlace), Se.Language.Translate.TranslateInPlaceNoOriginal);
        var rowTranslateInPlace = EditorChrome.MakeSwitchRow(switchTranslateInPlace, Se.Language.Translate.TranslateInPlaceNoOriginal);
        rowTranslateInPlace.VerticalAlignment = VerticalAlignment.Center;
        rowTranslateInPlace.Bind(IsVisibleProperty, new Binding(nameof(vm.TranslateInPlaceIsVisible)));

        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20,
            Children = { poweredByPanel, rowTranslateInPlace },
        };

        _buttonTranslate = UiUtil.MakeButton(Se.Language.General.Translate, vm.TranslateCommand);
        _buttonTranslate.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsTranslateEnabled)));

        _buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        _buttonOk.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsOkEnabled)));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return EditorChrome.MakeButtonBar(leftPanel, _buttonTranslate, buttonCancel, _buttonOk);
    }

    private static Grid Group(string isVisibleProperty, params Control[] controls)
    {
        var group = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
        };
        foreach (var control in controls)
        {
            control.Margin = new Thickness(0);
            group.Children.Add(control);
        }

        // A Grid wrapper keeps the IsVisible binding off the children, which bind their own
        var wrapper = new Grid { VerticalAlignment = VerticalAlignment.Center };
        wrapper.Children.Add(group);
        wrapper.Bind(IsVisibleProperty, new Binding(isVisibleProperty));
        return wrapper;
    }

    private static TextBlock Label(string text) => EditorChrome.MakeDimLabel(text, new Thickness(0));

    private static TextBox Text(TextBox textBox)
    {
        textBox.Classes.Add("text");
        textBox.Margin = new Thickness(0);
        return textBox;
    }

    private static Button Flat(Button button)
    {
        button.Classes.Add("tool");
        button.Height = EditorChrome.ScaledSize(28);
        button.MinWidth = 0;
        button.Padding = new Thickness(10, 0);
        button.FontSize = EditorChrome.FieldFontSize;
        button.Margin = new Thickness(0);
        button.VerticalAlignment = VerticalAlignment.Center;
        return button;
    }

    private static Button Compact(Button button)
    {
        button.MinHeight = 28;
        button.Height = EditorChrome.ScaledSize(28);
        button.MinWidth = 0;
        button.Padding = new Thickness(10, 0);
        button.FontSize = EditorChrome.FieldFontSize;
        button.Margin = new Thickness(0);
        button.VerticalContentAlignment = VerticalAlignment.Center;
        return button;
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
            vm.OnClosing();
            vm.SaveSettings();
            vm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        UiUtil.SaveWindowPosition(this);
    }
}

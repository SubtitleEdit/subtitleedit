using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System.Linq;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Platform.Progress;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

/// <summary>
/// AI review in the editor look (see <see cref="EditorChrome"/>): engine and model in a toolbar
/// on top, the category filter chips and the suggestions list in the middle, the reason and the
/// neighbouring lines of the selected suggestion under it, then the status/progress strip and
/// the button bar.
/// </summary>
public class AiReviewWindow : Window
{
    public AiReviewWindow(AiReviewViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.AiReview.Title);
        Width = 1200;
        Height = 720;
        MinWidth = 900;
        MinHeight = 500;
        CanResize = true;
        // The Edit dialogs' styles: compact fields that keep the field color while focused
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        AddChipStyles(Styles);
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Tools.AiReview;

        // ---------- toolbar ----------
        var comboEngine = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine))
            .WithAccessibleName(Se.Language.General.Engine);
        comboEngine.ItemTemplate = AiEngineCombo.ItemTemplate();

        // Ollama may run on another machine or port, so the URL is editable here like in
        // auto-translate - it was only reachable by hand-editing Settings.json before.
        var textBoxOllamaUrl = Field(UiUtil.MakeTextBox(250, vm, nameof(vm.OllamaUrl))
            .WithAccessibleName(Se.Language.General.Url));
        textBoxOllamaUrl.PlaceholderText = Se.Language.General.Url;
        var textBoxOllamaModel = Field(UiUtil.MakeTextBox(220, vm, nameof(vm.OllamaModel))
            .WithAccessibleName(Se.Language.General.Model));
        var buttonPickOllamaModel = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.PickOllamaModel, vm.PickOllamaModelCommand, 28);
        var panelOllama = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { textBoxOllamaUrl, textBoxOllamaModel, buttonPickOllamaModel },
        };
        panelOllama.Bind(IsVisibleProperty, new Binding(nameof(vm.IsOllamaVisible)));

        var textBoxOpenAiUrl = Field(UiUtil.MakeTextBox(250, vm, nameof(vm.OpenAiCompatibleUrl))
            .WithAccessibleName(Se.Language.General.Url));
        var textBoxOpenAiModel = Field(UiUtil.MakeTextBox(150, vm, nameof(vm.OpenAiCompatibleModel))
            .WithAccessibleName(Se.Language.General.Model));
        textBoxOpenAiModel.PlaceholderText = Se.Language.General.Model;
        var panelOpenAiApiKey = UiUtil.MakeApiKeyTextBox(130, vm, nameof(vm.OpenAiCompatibleApiKey),
            placeholderText: Se.Language.General.ApiKey);
        foreach (var textBox in panelOpenAiApiKey.Children.OfType<TextBox>())
        {
            Field(textBox);
        }

        foreach (var button in panelOpenAiApiKey.Children.OfType<Button>())
        {
            button.Classes.Add("tool");
            button.Width = 28;
            button.Height = 28;
        }

        // Free cloud tiers rate limit hard, so the delay between requests lives next to the API key
        // rather than in a settings dialog. The label would not fit the toolbar - a timer icon plus
        // tooltip carries the meaning, and the accessible name keeps it readable for screen readers.
        var numericDelay = UiUtil.MakeNumericUpDownInt(0, 600, 0, 56, vm, nameof(vm.RequestDelaySeconds))
            .WithAccessibleName(Se.Language.Translate.DelayInSecondsBetweenRequests);
        numericDelay.Classes.Add("field");
        ToolTip.SetTip(numericDelay, Se.Language.Translate.DelayInSecondsBetweenRequests);
        var iconDelay = new Optris.Icons.Avalonia.Icon
        {
            Value = "mdi-timer-outline",
            FontSize = 16,
            Foreground = EditorChrome.IconBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTip.SetTip(iconDelay, Se.Language.Translate.DelayInSecondsBetweenRequests);

        var panelOpenAiCompatible = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { textBoxOpenAiUrl, textBoxOpenAiModel, panelOpenAiApiKey, iconDelay, numericDelay },
        };
        panelOpenAiCompatible.Bind(IsVisibleProperty, new Binding(nameof(vm.IsOpenAiCompatibleVisible)));

        var comboLlamaCppModel = UiUtil.MakeComboBox(vm.LlamaCppModels, vm, nameof(vm.SelectedLlamaCppModel))
            .WithAccessibleName(Se.Language.General.Model);
        // Shared dot template, so the install colours here match auto-translate and the engine
        // settings dialog rather than this window's former bespoke green/grey pair.
        comboLlamaCppModel.ItemTemplate = Features.Translate.AutoTranslateCombos.LlamaCppModelItemTemplate();

        // Start/Stop server, as in auto-translate/OCR: without it an idle llama-server (and the
        // model's VRAM) could only be released by closing Subtitle Edit (#13969).
        var buttonLlamaCppServer = UiUtil.MakeButton(string.Empty, vm.ToggleLlamaCppServerCommand);
        buttonLlamaCppServer.Bind(Button.ContentProperty, new Binding(nameof(vm.LlamaCppServerButtonText)));
        buttonLlamaCppServer.Bind(IsEnabledProperty, new Binding("!" + nameof(vm.IsReviewing)));
        CompactButton(buttonLlamaCppServer);

        var panelLlamaCpp = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { comboLlamaCppModel, buttonLlamaCppServer },
        };
        panelLlamaCpp.Bind(IsVisibleProperty, new Binding(nameof(vm.IsLlamaCppVisible)));

        var buttonLlamaCppEngineSettings = EditorChrome.CreateToolButton("fa-solid fa-gear", Se.Language.General.LlamaCppEngineSettings,
            vm.ShowLlamaCppEngineSettingsCommand, 28);
        buttonLlamaCppEngineSettings.Bind(IsVisibleProperty, new Binding(nameof(vm.IsLlamaCppVisible)));

        // The engine settings button sits directly after the engine combo, not with the model combo:
        // it configures the engine itself (backend build, release, install state), not the model.
        var panelEngine = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { comboEngine, buttonLlamaCppEngineSettings },
        };

        var languageText = MakeBoundTextBlock(nameof(vm.LanguageDisplay));
        languageText.FontSize = 12.5;
        var languageChip = new Border
        {
            Background = EditorChrome.AccentSoftBrush,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10, 4),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new Optris.Icons.Avalonia.Icon { Value = "mdi-web", FontSize = 14, Foreground = EditorChrome.IconBrush, VerticalAlignment = VerticalAlignment.Center },
                    languageText,
                },
            },
        };

        var buttonEditPrompt = EditorChrome.MakeActionButton("fa-solid fa-pen", l.EditPrompt, vm.EditPromptCommand);

        var toolbarGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,*,Auto"),
            ColumnSpacing = 8,
        };
        toolbarGrid.Add(EditorChrome.MakeDimLabel(Se.Language.General.Engine, new Thickness(4, 0, 2, 0)), 0, 0);
        toolbarGrid.Add(panelEngine, 0, 1);
        toolbarGrid.Add(panelOllama, 0, 2);
        toolbarGrid.Add(panelLlamaCpp, 0, 3);
        toolbarGrid.Add(panelOpenAiCompatible, 0, 4);
        toolbarGrid.Add(languageChip, 0, 5);
        toolbarGrid.Add(buttonEditPrompt, 0, 7);
        var toolbar = EditorChrome.MakeToolbar(toolbarGrid);
        toolbar.Padding = new Thickness(12, 8);

        // ---------- filter chips ----------
        var chipsItems = new ItemsControl
        {
            ItemsSource = vm.FilterChips,
            VerticalAlignment = VerticalAlignment.Center,
            ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 }),
            ItemTemplate = new FuncDataTemplate<ReviewFilterChip>((chip, _) =>
            {
                var content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                if (chip?.Category != null)
                {
                    content.Children.Add(new Avalonia.Controls.Shapes.Ellipse
                    {
                        Width = 7,
                        Height = 7,
                        VerticalAlignment = VerticalAlignment.Center,
                        Fill = GetCategoryBrush(chip.Category.Value),
                    });
                }

                var chipText = new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontSize = 12.5 };
                chipText.Bind(TextBlock.TextProperty, new Binding(nameof(ReviewFilterChip.Display)));
                content.Children.Add(chipText);

                var toggle = new ToggleButton
                {
                    Padding = new Thickness(10, 3),
                    MinHeight = 26,
                    CornerRadius = new CornerRadius(13),
                    Command = vm.SetFilterCommand,
                    CommandParameter = chip,
                    Content = content,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(ReviewFilterChip.IsActive)),
                };
                toggle.Classes.Add("chip");
                AutomationProperties.SetName(toggle, chip?.Label ?? string.Empty);
                return toggle;
            }),
        };

        var warningBrush = ReviewSuggestionItem.GetWarningBrush();
        var warningText = MakeBoundTextBlock(nameof(vm.WarningNoteText));
        warningText.Foreground = warningBrush;
        warningText.FontSize = 12.5;
        var warningNote = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            Children =
            {
                new Optris.Icons.Avalonia.Icon { Value = "mdi-alert", FontSize = 14, Foreground = warningBrush, VerticalAlignment = VerticalAlignment.Center },
                warningText,
            },
        };
        warningNote.Bind(IsVisibleProperty, new Binding(nameof(vm.HasWarningNote)));

        // Tick all / untick all / invert, as icons on the list header like Multiple replace's preview.
        var listActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                EditorChrome.CreateToolButton("fa-solid fa-square-check", Se.Language.General.SelectAll, vm.SelectAllCommand, 26),
                EditorChrome.CreateToolButton("fa-regular fa-square", Se.Language.General.SelectNone, vm.SelectNoneCommand, 26),
                EditorChrome.CreateToolButton("fa-solid fa-right-left", Se.Language.General.InvertSelection, vm.InvertSelectionCommand, 26),
            },
        };

        var chipsBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"),
            ColumnSpacing = 10,
            Margin = new Thickness(0, 0, 0, 8),
        };
        chipsBar.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Suggestions), 0, 0);
        chipsBar.Add(chipsItems, 0, 1);
        chipsBar.Add(warningNote, 0, 2);
        chipsBar.Add(listActions, 0, 3);

        // ---------- suggestions grid ----------
        // No header-click sorting (the DataGrid's CanUserSortColumns is not carried
        // over): suggestions are fix previews in subtitle order. The DataGrid-era
        // DataGridCheckboxMultiSelect is replaced by native extended selection plus
        // TableViewExtras.AddSpaceToggle for the Space-toggles-checkbox piece.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Suggestions;
        dataGrid.Columns.AddRange(new[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<ReviewSuggestionItem>(nameof(ReviewSuggestionItem.IsSelected), nameof(ReviewSuggestionItem.CategoryDisplay)),
                    Width = new GridLength(80), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(ReviewSuggestionItem.Number)),
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.Tools.FixCommonErrors.Action,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = new FuncDataTemplate<ReviewSuggestionItem>((item, _) =>
                    {
                        var panel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 6,
                            VerticalAlignment = VerticalAlignment.Center,
                        };
                        if (item != null)
                        {
                            panel.Children.Add(new Border
                            {
                                Background = item.CategoryBackgroundBrush,
                                CornerRadius = new CornerRadius(5),
                                Padding = new Thickness(7, 2),
                                VerticalAlignment = VerticalAlignment.Center,
                                Child = new StackPanel
                                {
                                    Orientation = Orientation.Horizontal,
                                    Spacing = 5,
                                    Children =
                                    {
                                        new Optris.Icons.Avalonia.Icon
                                        {
                                            Value = item.CategoryIconName,
                                            FontSize = 13,
                                            Foreground = item.CategoryBrush,
                                            VerticalAlignment = VerticalAlignment.Center,
                                        },
                                        new TextBlock
                                        {
                                            Text = item.CategoryDisplay,
                                            FontSize = UiUtil.ScaledFontSize(12),
                                            Foreground = item.CategoryBrush,
                                            VerticalAlignment = VerticalAlignment.Center,
                                        },
                                    },
                                },
                            });
                            if (item.IsWarning)
                            {
                                panel.Children.Add(new Optris.Icons.Avalonia.Icon
                                {
                                    Value = "mdi-alert",
                                    FontSize = 13,
                                    Foreground = item.WarningBrush,
                                    VerticalAlignment = VerticalAlignment.Center,
                                });
                            }
                        }

                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = panel,
                        };
                    }),
                    Width = new GridLength(150), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = new FuncDataTemplate<ReviewSuggestionItem>((item, _) =>
                        item == null ? new Border() : MakeDiffCell(item, isAfter: false, dataGrid)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = new FuncDataTemplate<ReviewSuggestionItem>((item, _) =>
                        item == null ? new Border() : MakeDiffCell(item, isAfter: true, dataGrid)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
        });
        AutomationProperties.SetName(dataGrid, l.Title);
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSuggestion)));
        TableViewExtras.AddSpaceToggle<ReviewSuggestionItem>(dataGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);
        dataGrid.DoubleTapped += (_, _) => vm.OnSuggestionsGridDoubleTapped();

        var borderGrid = EditorChrome.MakeListFrame(dataGrid);

        // ---------- reason strip ----------
        var reasonTextBlock = MakeBoundTextBlock(nameof(vm.ReasonText));
        reasonTextBlock.FontSize = 12.5;
        reasonTextBlock.TextWrapping = TextWrapping.Wrap;
        var reasonText = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            ColumnSpacing = 7,
            MinHeight = 20,
        };
        reasonText.Add(new Optris.Icons.Avalonia.Icon
        {
            Value = "mdi-message-text-outline",
            FontSize = 14,
            Foreground = EditorChrome.IconBrush,
            VerticalAlignment = VerticalAlignment.Center,
        }, 0, 0);
        reasonText.Add(reasonTextBlock, 0, 1);
        reasonText.Bind(IsVisibleProperty, new Binding(nameof(vm.HasReason)));

        // ---------- context strip ----------
        // The lines before and after the selected suggestion, so a casing/punctuation fix can be
        // judged against its neighbors without leaving the window (issue #14619).
        var contextGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            ColumnSpacing = 7,
            RowSpacing = 2,
        };
        contextGrid.Add(new Optris.Icons.Avalonia.Icon
        {
            Value = "mdi-arrow-up-thin",
            FontSize = 14,
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        }, 0, 0);
        contextGrid.Add(new Optris.Icons.Avalonia.Icon
        {
            Value = "mdi-arrow-down-thin",
            FontSize = 14,
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        }, 1, 0);
        var contextPreviousLabel = MakeBoundTextBlock(nameof(vm.ContextPreviousLabel));
        contextPreviousLabel.Foreground = EditorChrome.FaintTextBrush;
        contextPreviousLabel.FontSize = 12;
        var contextNextLabel = MakeBoundTextBlock(nameof(vm.ContextNextLabel));
        contextNextLabel.Foreground = EditorChrome.FaintTextBrush;
        contextNextLabel.FontSize = 12;
        var contextPreviousText = MakeBoundTextBlock(nameof(vm.ContextPreviousText));
        contextPreviousText.Foreground = EditorChrome.DimTextBrush;
        contextPreviousText.FontSize = 12.5;
        contextPreviousText.TextWrapping = TextWrapping.Wrap;
        var contextNextText = MakeBoundTextBlock(nameof(vm.ContextNextText));
        contextNextText.Foreground = EditorChrome.DimTextBrush;
        contextNextText.FontSize = 12.5;
        contextNextText.TextWrapping = TextWrapping.Wrap;
        contextGrid.Add(contextPreviousLabel, 0, 1);
        contextGrid.Add(contextPreviousText, 0, 2);
        contextGrid.Add(contextNextLabel, 1, 1);
        contextGrid.Add(contextNextText, 1, 2);
        contextGrid.Bind(IsVisibleProperty, new Binding(nameof(vm.HasContext)));

        var detailPanel = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(0, 10, 0, 0),
            Children = { reasonText, contextGrid },
        };

        var listGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            Margin = new Thickness(14, 12, 14, 12),
        };
        listGrid.Add(chipsBar, 0, 0);
        listGrid.Add(borderGrid, 1, 0);
        listGrid.Add(detailPanel, 2, 0);

        // ---------- progress ----------
        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Height = 6,
            VerticalAlignment = VerticalAlignment.Center,
            [!RangeBase.ValueProperty] = new Binding(nameof(vm.ProgressValue)),
            // Only meaningful while a review is running - a full bar sitting under a
            // finished review just looks stuck.
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsReviewing)),
        };
        progressBar.WithPlatformProgress(this, nameof(vm.IsReviewing));
        var statusText = MakeBoundTextBlock(nameof(vm.StatusText));
        statusText.FontSize = 12;
        statusText.Foreground = EditorChrome.DimTextBrush;
        statusText.TextTrimming = TextTrimming.CharacterEllipsis;

        var progressIcon = new Optris.Icons.Avalonia.Icon
        {
            Value = "mdi-robot-outline",
            FontSize = 15,
            Foreground = EditorChrome.IconBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var progressRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
            ColumnSpacing = 10,
        };
        progressRow.Add(progressIcon, 0, 0);
        progressRow.Add(statusText, 0, 1);
        progressRow.Add(progressBar, 0, 2);

        // The status stays after a review ("Review done - ...", "Fixed N lines"), so the strip
        // is there whenever there is something to say, not only while reviewing.
        var progressStrip = new Border
        {
            Child = progressRow,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 7),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.StatusText)) { Converter = StringConverters.IsNotNullOrEmpty },
        };

        // ---------- bottom bar ----------
        var summaryText = EditorChrome.MakeBoundHint(nameof(vm.SummaryText));
        summaryText.FontSize = 12;

        // Plays the selected suggestion's line in the main window's video player - hidden when no
        // video is loaded (the view model gets no play hook then).
        var buttonPlay = EditorChrome.MakeActionButton("fa-solid fa-play", Se.Language.General.PlayCurrent, vm.PlayCurrentLineCommand);
        buttonPlay.Bind(IsVisibleProperty, new Binding(nameof(vm.IsPlayVisible)));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonPlay, l.PlayCurrentLineHint);
        }

        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = { buttonPlay, summaryText },
        };

        var buttonReview = UiUtil.MakeButton(l.Review, vm.ReviewCommand)
            .WithIconLeft("fa-solid fa-robot");
        buttonReview.Bind(IsVisibleProperty, new Binding("!" + nameof(vm.IsReviewing)));

        var buttonStop = UiUtil.MakeButton(Se.Language.General.Stop, vm.StopReviewCommand)
            .WithIconLeft("fa-solid fa-stop");
        buttonStop.Bind(IsVisibleProperty, new Binding(nameof(vm.IsReviewing)));

        // Apply/Ok/Cancel, the same shape as Multiple replace: Apply writes the checked fixes and
        // keeps the window open so the review can be worked through in passes (issue #13807), Ok
        // writes them and closes, Cancel closes and leaves the unapplied ones behind. Apply is
        // hidden for callers without a live target - they have nowhere to receive a pass.
        // WithIconLeft assigns Content, so setting it after binding ContentProperty replaced the
        // bound text with the bare icon (and binding after WithIconLeft would drop the icon).
        // WithIconLeftBindText builds the icon+bound-text panel once, which is what was wanted.
        var buttonApply = UiUtil.MakeButton(string.Empty, vm.ApplyCommand)
            .WithBindIsVisible(nameof(vm.IsApplyVisible))
            .WithIconLeftBindText("fa-solid fa-check", nameof(vm.ApplyButtonText));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        // The accent marks the next step: Review until there are suggestions, then OK.
        void UpdateAccent()
        {
            var hasSuggestions = vm.Suggestions.Count > 0;
            buttonReview.Classes.Set("primary", !hasSuggestions);
            buttonOk.Classes.Set("primary", hasSuggestions);
        }

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(vm.SummaryText) or nameof(vm.IsReviewing) or nameof(vm.Suggestions))
            {
                UpdateAccent();
            }
        };
        UpdateAccent();

        var bottomBar = EditorChrome.MakeButtonBar(leftPanel, buttonReview, buttonStop, buttonApply, buttonCancel, buttonOk);

        // ---------- layout ----------
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto,Auto"),
        };
        grid.Add(toolbar, 0, 0);
        grid.Add(EditorChrome.MakePanel(listGrid), 1, 0);
        grid.Add(progressStrip, 2, 0);
        grid.Add(bottomBar, 3, 0);

        Content = grid;

        Loaded += delegate
        {
            buttonReview.Focus();
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += delegate { vm.OnClosing(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// The filter chips: outlined pills, the active one in the soft accent.
    /// </summary>
    private static void AddChipStyles(Styles styles)
    {
        static Style Presenter(System.Func<Selector?, Selector> toggle, params (AvaloniaProperty Property, object Value)[] setters)
        {
            var style = new Style(x => toggle(x).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            foreach (var (property, value) in setters)
            {
                style.Setters.Add(new Setter(property, value));
            }

            return style;
        }

        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("chip"),
            (ContentPresenter.BackgroundProperty, Brushes.Transparent),
            (ContentPresenter.BorderBrushProperty, EditorChrome.FieldBorderBrush),
            (ContentPresenter.ForegroundProperty, EditorChrome.DimTextBrush)));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("chip").Class(":pointerover"),
            (ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush),
            (ContentPresenter.BorderBrushProperty, EditorChrome.FieldBorderBrush),
            (ContentPresenter.ForegroundProperty, EditorChrome.StrongTextBrush)));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("chip").Class(":pressed"),
            (ContentPresenter.BackgroundProperty, EditorChrome.PressedBrush)));
        foreach (var state in new[] { ":checked", ":checked:pointerover" })
        {
            styles.Add(Presenter(x => state == ":checked"
                    ? x.OfType<ToggleButton>().Class("chip").Class(":checked")
                    : x.OfType<ToggleButton>().Class("chip").Class(":checked").Class(":pointerover"),
                (ContentPresenter.BackgroundProperty, EditorChrome.AccentSoftBrush),
                (ContentPresenter.BorderBrushProperty, EditorChrome.AccentBrush),
                (ContentPresenter.ForegroundProperty, EditorChrome.StrongTextBrush)));
        }
    }

    private static TextBox Field(TextBox textBox)
    {
        textBox.Classes.Add("text");
        return textBox;
    }

    private static void CompactButton(Button button)
    {
        button.MinHeight = 28;
        button.Height = 28;
        button.Padding = new Thickness(10, 0);
        button.FontSize = 12.5;
        button.VerticalContentAlignment = VerticalAlignment.Center;
    }

    /// <summary>
    /// A Before/After cell showing the word-level diff of the suggestion. Both cells re-render
    /// when <see cref="ReviewSuggestionItem.After"/> changes; the After cell can also be edited
    /// in place (<see cref="TableViewInlineTextEditor"/>), so a nearly-right fix is corrected
    /// here instead of being declined and retyped in the main window.
    /// </summary>
    private static Border MakeDiffCell(ReviewSuggestionItem item, bool isAfter, TableView grid)
    {
        var cell = new Border
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(4),
        };

        Control MakeDisplay()
        {
            var (beforeBlock, afterBlock) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
            return isAfter ? afterBlock : beforeBlock;
        }

        TableViewInlineTextEditor? editor = null;
        if (isAfter)
        {
            editor = new TableViewInlineTextEditor(cell, grid, () => item.After, text => item.After = text, MakeDisplay,
                hint: Se.Language.Tools.AiReview.EditAfterHint);
        }
        else
        {
            cell.Child = MakeDisplay();
        }

        void OnItemChanged(object? _, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ReviewSuggestionItem.After))
            {
                return;
            }

            if (editor != null)
            {
                editor.Refresh();
            }
            else
            {
                cell.Child = MakeDisplay();
            }
        }

        // The template is rebuilt per row, so the subscription must not outlive the cell.
        cell.AttachedToVisualTree += (_, _) => item.PropertyChanged += OnItemChanged;
        cell.DetachedFromVisualTree += (_, _) => item.PropertyChanged -= OnItemChanged;
        return cell;
    }

    private static IBrush GetCategoryBrush(ReviewCategory category)
    {
        return ReviewSuggestionItem.GetBrushForCategory(category);
    }

    private static TextBlock MakeBoundTextBlock(string textPropertyPath)
    {
        var textBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
        };
        textBlock.Bind(TextBlock.TextProperty, new Binding(textPropertyPath));
        return textBlock;
    }
}

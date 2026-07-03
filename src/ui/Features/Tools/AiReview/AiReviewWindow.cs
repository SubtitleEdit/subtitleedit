using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

public class AiReviewWindow : Window
{
    public AiReviewWindow(AiReviewViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.AiReview.Title;
        Width = 1024;
        Height = 720;
        MinWidth = 800;
        MinHeight = 500;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Tools.AiReview;

        // ---------- toolbar ----------
        var comboEngine = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine))
            .WithAccessibleName(Se.Language.General.Engine);

        var textBoxOllamaModel = UiUtil.MakeTextBox(220, vm, nameof(vm.OllamaModel))
            .WithAccessibleName(Se.Language.General.Model);
        var buttonPickOllamaModel = UiUtil.MakeButton("...", vm.PickOllamaModelCommand)
            .Compact()
            .WithAccessibleName(Se.Language.General.PickOllamaModel);
        var panelOllama = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { textBoxOllamaModel, buttonPickOllamaModel },
        };
        panelOllama.Bind(IsVisibleProperty, new Binding(nameof(vm.IsOllamaVisible)));

        var textBoxOpenAiUrl = UiUtil.MakeTextBox(250, vm, nameof(vm.OpenAiCompatibleUrl))
            .WithAccessibleName(Se.Language.General.Url);
        var textBoxOpenAiModel = UiUtil.MakeTextBox(150, vm, nameof(vm.OpenAiCompatibleModel))
            .WithAccessibleName(Se.Language.General.Model);
        textBoxOpenAiModel.PlaceholderText = Se.Language.General.Model;
        var textBoxOpenAiApiKey = UiUtil.MakeTextBox(130, vm, nameof(vm.OpenAiCompatibleApiKey))
            .WithAccessibleName(Se.Language.General.ApiKey);
        textBoxOpenAiApiKey.PlaceholderText = Se.Language.General.ApiKey;
        textBoxOpenAiApiKey.PasswordChar = '\u25cf';
        var panelOpenAiCompatible = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { textBoxOpenAiUrl, textBoxOpenAiModel, textBoxOpenAiApiKey },
        };
        panelOpenAiCompatible.Bind(IsVisibleProperty, new Binding(nameof(vm.IsOpenAiCompatibleVisible)));

        var comboLlamaCppModel = UiUtil.MakeComboBox(vm.LlamaCppModels, vm, nameof(vm.SelectedLlamaCppModel))
            .WithAccessibleName(Se.Language.General.Model);
        comboLlamaCppModel.Bind(IsVisibleProperty, new Binding(nameof(vm.IsLlamaCppVisible)));
        comboLlamaCppModel.ItemTemplate = new FuncDataTemplate<Features.Translate.LlamaCppModelDisplay>((item, _) =>
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 7,
                VerticalAlignment = VerticalAlignment.Center,
            };
            if (item != null)
            {
                panel.Children.Add(new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 8,
                    Height = 8,
                    VerticalAlignment = VerticalAlignment.Center,
                    Fill = item.IsInstalled
                        ? new SolidColorBrush(Color.FromRgb(0x5c, 0xb8, 0x5c))
                        : new SolidColorBrush(Color.FromArgb(0x50, 0x80, 0x88, 0x90)),
                });
                panel.Children.Add(new TextBlock
                {
                    Text = item.DisplayText,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }

            return panel;
        });

        var languageChip = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x24, 0x4c, 0x9c, 0xe8)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x59, 0x4c, 0x9c, 0xe8)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10, 3),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    new Optris.Icons.Avalonia.Icon { Value = "mdi-web", FontSize = 14, VerticalAlignment = VerticalAlignment.Center },
                    MakeBoundTextBlock(nameof(vm.LanguageDisplay)),
                },
            },
        };

        var buttonEditPrompt = UiUtil.MakeButton(l.EditPrompt, vm.EditPromptCommand)
            .WithIconLeft("fa-solid fa-pen");

        var toolbar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,Auto,Auto,Auto,*,Auto"),
            ColumnSpacing = 8,
        };
        var labelEngine = UiUtil.MakeTextBlock(Se.Language.General.Engine).WithMarginRight(2);
        labelEngine.VerticalAlignment = VerticalAlignment.Center;
        toolbar.Add(labelEngine, 0, 0);
        toolbar.Add(comboEngine, 0, 1);
        toolbar.Add(panelOllama, 0, 2);
        toolbar.Add(comboLlamaCppModel, 0, 3);
        toolbar.Add(panelOpenAiCompatible, 0, 4);
        toolbar.Add(languageChip, 0, 5);
        toolbar.Add(buttonEditPrompt, 0, 7);

        // ---------- filter chips ----------
        var chipsItems = new ItemsControl
        {
            ItemsSource = vm.FilterChips,
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

                var chipText = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
                chipText.Bind(TextBlock.TextProperty, new Binding(nameof(ReviewFilterChip.Display)));
                content.Children.Add(chipText);

                var toggle = new ToggleButton
                {
                    Padding = new Thickness(10, 3),
                    CornerRadius = new CornerRadius(12),
                    Command = vm.SetFilterCommand,
                    CommandParameter = chip,
                    Content = content,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(ReviewFilterChip.IsActive)),
                };
                AutomationProperties.SetName(toggle, chip?.Label ?? string.Empty);
                return toggle;
            }),
        };

        var warningBrush = new SolidColorBrush(Color.FromRgb(0xf0, 0xa6, 0x3c));
        var warningText = MakeBoundTextBlock(nameof(vm.WarningNoteText));
        warningText.Foreground = warningBrush;
        var warningNote = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Optris.Icons.Avalonia.Icon { Value = "mdi-alert", FontSize = 14, Foreground = warningBrush, VerticalAlignment = VerticalAlignment.Center },
                warningText,
            },
        };
        warningNote.Bind(IsVisibleProperty, new Binding(nameof(vm.HasWarningNote)));

        var chipsBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
        };
        chipsBar.Add(chipsItems, 0, 0);
        chipsBar.Add(warningNote, 0, 1);

        // ---------- suggestions grid ----------
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.Suggestions,
            IsReadOnly = false,
            Columns =
            {
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<ReviewSuggestionItem>((item, _) => new Border
                    {
                        Background = Brushes.Transparent,
                        Padding = new Thickness(4),
                        Child = new CheckBox
                        {
                            Focusable = false,
                            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(ReviewSuggestionItem.IsSelected)),
                            [!AutomationProperties.NameProperty] = new Binding(nameof(ReviewSuggestionItem.CategoryDisplay)),
                            HorizontalAlignment = HorizontalAlignment.Center,
                        },
                    }),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(ReviewSuggestionItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.Tools.FixCommonErrors.Action,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
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
                                            FontSize = 12,
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
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<ReviewSuggestionItem>((item, _) =>
                    {
                        if (item == null)
                        {
                            return new Border();
                        }

                        var (beforeBlock, _) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = beforeBlock,
                        };
                    }),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    CellTemplate = new FuncDataTemplate<ReviewSuggestionItem>((item, _) =>
                    {
                        if (item == null)
                        {
                            return new Border();
                        }

                        var (_, afterBlock) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = afterBlock,
                        };
                    }),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };
        AutomationProperties.SetName(dataGrid, l.Title);
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedSuggestion)));
        _ = new DataGridCheckboxMultiSelect<ReviewSuggestionItem>(dataGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);

        var borderGrid = UiUtil.MakeBorderForControlNoPadding(dataGrid);

        // ---------- progress ----------
        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Height = 6,
            VerticalAlignment = VerticalAlignment.Center,
            [!RangeBase.ValueProperty] = new Binding(nameof(vm.ProgressValue)),
        };
        var statusText = MakeBoundTextBlock(nameof(vm.StatusText));
        statusText.VerticalAlignment = VerticalAlignment.Center;
        statusText.Opacity = 0.8;

        var progressIcon = new Optris.Icons.Avalonia.Icon
        {
            Value = "mdi-robot-outline",
            FontSize = 15,
            Opacity = 0.8,
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

        // ---------- reason strip ----------
        var reasonTextBlock = MakeBoundTextBlock(nameof(vm.ReasonText));
        reasonTextBlock.Opacity = 0.8;
        reasonTextBlock.TextWrapping = TextWrapping.Wrap;
        var reasonText = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            MinHeight = 20,
            Children =
            {
                new Optris.Icons.Avalonia.Icon
                {
                    Value = "mdi-message-text-outline",
                    FontSize = 14,
                    Opacity = 0.7,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                reasonTextBlock,
            },
        };
        reasonText.Bind(IsVisibleProperty, new Binding(nameof(vm.HasReason)));

        // ---------- bottom bar ----------
        var summaryText = MakeBoundTextBlock(nameof(vm.SummaryText));
        summaryText.VerticalAlignment = VerticalAlignment.Center;
        summaryText.Opacity = 0.8;

        var leftButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                summaryText.WithMarginRight(10),
                UiUtil.MakeButton(Se.Language.General.SelectAll, vm.SelectAllCommand),
                UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.InvertSelectionCommand),
            },
        };

        var buttonReview = UiUtil.MakeButton(l.Review, vm.ReviewCommand)
            .WithIconLeft("fa-solid fa-robot");
        buttonReview.Bind(IsVisibleProperty, new Binding(nameof(vm.IsNotReviewing)));

        var buttonStop = UiUtil.MakeButton(l.Stop, vm.StopReviewCommand)
            .WithIconLeft("fa-solid fa-stop");
        buttonStop.Bind(IsVisibleProperty, new Binding(nameof(vm.IsReviewing)));

        var buttonApply = UiUtil.MakeButton(string.Empty, vm.OkCommand);
        buttonApply.Bind(ContentControl.ContentProperty, new Binding(nameof(vm.ApplyButtonText)));
        buttonApply.WithIconLeft("fa-solid fa-check");

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var bottomBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
        };
        bottomBar.Add(leftButtons, 0, 0);
        bottomBar.Add(UiUtil.MakeButtonBar(buttonReview, buttonStop, buttonApply, buttonCancel), 0, 2);

        // ---------- layout ----------
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(toolbar, 0, 0);
        grid.Add(chipsBar, 1, 0);
        grid.Add(borderGrid, 2, 0);
        grid.Add(progressRow, 3, 0);
        grid.Add(reasonText, 4, 0);
        grid.Add(bottomBar, 5, 0);

        Content = grid;

        Loaded += delegate
        {
            buttonReview.Focus();
            UiUtil.RestoreWindowPosition(this);
        };
        Closing += delegate { vm.OnClosing(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
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

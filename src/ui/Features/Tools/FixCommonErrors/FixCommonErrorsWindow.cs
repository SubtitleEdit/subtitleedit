using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public class FixCommonErrorsWindow : Window
{
    private readonly FixCommonErrorsViewModel _vm;
    private Button? _buttonApplySelectedFixes;

    public FixCommonErrorsWindow(FixCommonErrorsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.FixCommonErrors;
        Width = 1024;
        Height = 720;
        MinWidth = 800;
        MinHeight = 600;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelStep1 = new Label
        {
            Content = Se.Language.Tools.FixCommonErrors.FixCommonOcrErrorsStep1,
            VerticalAlignment = VerticalAlignment.Center,
        };
        labelStep1.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));

        var labelStep2 = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
        };
        labelStep2.Bind(Label.ContentProperty, new Binding(nameof(vm.Step2Title)));
        labelStep2.Bind(IsVisibleProperty, new Binding(nameof(vm.Step2IsVisible)));

        var textBoxSearch = UiUtil.MakeTextBox(250, vm, nameof(vm.SearchText)).WithMarginRight(25)
            .WithAccessibleName(Se.Language.Tools.FixCommonErrors.SearchRulesDotDotDot);
        textBoxSearch.PlaceholderText = Se.Language.Tools.FixCommonErrors.SearchRulesDotDotDot;
        textBoxSearch.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));
        textBoxSearch.TextChanged += vm.TextBoxSearch_TextChanged;
        // Off by default (#12441) - keep it reachable here, next to the language it depends on,
        // instead of only in the OCR window where a Fix-common-errors user would never look.
        var checkBoxGuessUnknownWords = UiUtil.MakeCheckBox(Se.Language.Ocr.TryToGuessUnknownWords, vm, nameof(vm.TryToGuessUnknownWords))
            .WithMarginRight(25);
        checkBoxGuessUnknownWords.Bind(IsVisibleProperty, new Binding(nameof(vm.Step2IsVisible)));

        var panelTopRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                checkBoxGuessUnknownWords,
                UiUtil.MakeTextBlock(Se.Language.General.Language).WithMarginRight(5),
                UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage))
                    .WithAccessibleName(Se.Language.General.Language),
            },
        };

        var rulesGrid = TableViewExtras.MakeTableView();
        rulesGrid[!TableView.ItemsSourceProperty] = new Binding($"{nameof(vm.SelectedProfile)}.{nameof(ProfileDisplayItem.FixRules)}");

        var rulesEnabledColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<FixRuleDisplayItem>((item, _) =>
            {
                return new Border
                {
                    Background = Brushes.Transparent, // Prevents highlighting
                    Padding = new Thickness(4),
                    Child = new CheckBox
                    {
                        Focusable = false,
                        [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixRuleDisplayItem.IsSelected)),
                        // The checkbox is unfocusable, so name it after the rule it toggles so a
                        // screen reader can tell which rule's enabled state it is on (#11745).
                        [!AutomationProperties.NameProperty] = new Binding(nameof(FixRuleDisplayItem.Name)),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                };
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(80),
        };
        var rulesNameColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(FixRuleDisplayItem.Name)),
            Width = new GridLength(320),
        };
        var rulesExampleColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Example,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(FixRuleDisplayItem.Example)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        rulesGrid.Columns.Add(rulesEnabledColumn);
        rulesGrid.Columns.Add(rulesNameColumn);
        rulesGrid.Columns.Add(rulesExampleColumn);
        rulesGrid.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));
        AutomationProperties.SetName(rulesGrid, Se.Language.General.Rules);
        // Extended selection is native ListBox behavior on TableView; Space toggling every
        // selected row's checkbox is the piece the old DataGridCheckboxMultiSelect provided.
        TableViewExtras.AddSpaceToggle<FixRuleDisplayItem>(rulesGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);
        // Header sorting reorders the profile's FixRules collection in place. That is safe
        // here because rule *execution* order is canonical - ApplyFixes runs the selected
        // rules in the order they were defined, not in display order - and profiles persist
        // rule names, not row order (kept sortable per #12431).
        new TableViewHeaderSorter(rulesGrid)
            .AddSortable<FixRuleDisplayItem, bool>(rulesEnabledColumn, x => x.IsSelected)
            .AddSortable<FixRuleDisplayItem, string>(rulesNameColumn, x => x.Name)
            .AddSortable<FixRuleDisplayItem, string>(rulesExampleColumn, x => x.Example);

        var step2Grid = MakeStep2Grid();
        step2Grid.Bind(IsVisibleProperty, new Binding(nameof(_vm.Step2IsVisible)));
        var comboProfile = UiUtil.MakeComboBox(vm.Profiles, vm, nameof(vm.SelectedProfile))
            .WithAccessibleName(Se.Language.General.Profile);
        var buttonPanelRules = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.SelectAll, vm.RulesSelectAllCommand),
            UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.RulesInverseSelectedCommand),
            UiUtil.MakeTextBlock(Se.Language.General.Profile).WithMarginLeft(25).WithMarginRight(10),
            comboProfile,
            UiUtil.MakeButton("...", vm.ShowProfileCommand).Compact().WithAccessibleName(Se.Language.General.Profiles)
        );
        buttonPanelRules.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));

        var buttonToApplyFixes = UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.GoToApplyFixes, vm.ToApplyFixesCommand)
            .WithIconRight("fa-solid fa-arrow-right")
            .BindIsVisible(vm, nameof(vm.Step1IsVisible));

        var buttonBackToFixList = UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.BackToFixList, vm.BackToFixListCommand)
            .WithIconLeft("fa-solid fa-arrow-left")
            .BindIsVisible(vm, nameof(vm.Step2IsVisible));

        var buttonDone = UiUtil.MakeButton(Se.Language.General.Done, vm.OkCommand)
            .BindIsVisible(vm, nameof(vm.Step2IsVisible));

        var buttonPanelRight = UiUtil.MakeButtonBar(
            buttonBackToFixList,
            buttonToApplyFixes,
            buttonDone,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Children.Add(labelStep1);
        Grid.SetRow(labelStep1, 0);
        Grid.SetColumn(labelStep1, 0);
        grid.Children.Add(labelStep2);
        Grid.SetRow(labelStep2, 0);
        Grid.SetColumn(labelStep2, 0);

        grid.Children.Add(panelTopRight);
        Grid.SetRow(panelTopRight, 0);
        Grid.SetColumn(panelTopRight, 0);
        Grid.SetColumnSpan(panelTopRight, 2);

        grid.Children.Add(rulesGrid);
        Grid.SetRow(rulesGrid, 1);
        Grid.SetColumn(rulesGrid, 0);
        Grid.SetColumnSpan(rulesGrid, 2);

        grid.Children.Add(step2Grid);
        Grid.SetRow(step2Grid, 1);
        Grid.SetColumn(step2Grid, 0);
        Grid.SetColumnSpan(step2Grid, 2);

        grid.Children.Add(buttonPanelRules);
        Grid.SetRow(buttonPanelRules, 2);
        Grid.SetColumn(buttonPanelRules, 0);

        var labelFixesApplied = UiUtil.MakeTextBlock(string.Empty);
        labelFixesApplied.Bind(TextBlock.TextProperty, new Binding(nameof(vm.FixesAppliedText)));
        labelFixesApplied.Bind(IsVisibleProperty, new Binding(nameof(vm.FixesAppliedText)) { Converter = StringConverters.IsNotNullOrEmpty });
        labelFixesApplied.VerticalAlignment = VerticalAlignment.Center;

        // Green check + "Nothing to fix" so an empty re-scan gives visible feedback; the counters
        // stay as they were, which on its own reads as "the button did nothing" (#12849).
        var nothingToFixBrush = new SolidColorBrush(UiTheme.IsDarkThemeEnabled()
            ? Color.FromRgb(0x6e, 0xcb, 0x87)
            : Color.FromRgb(0x1e, 0x7e, 0x34));
        var nothingToFixText = UiUtil.MakeTextBlock(Se.Language.Tools.FixCommonErrors.NothingToFix);
        nothingToFixText.Foreground = nothingToFixBrush;
        nothingToFixText.VerticalAlignment = VerticalAlignment.Center;
        var nothingToFixPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Optris.Icons.Avalonia.Icon
                {
                    Value = IconNames.CheckCircle,
                    FontSize = 16,
                    Foreground = nothingToFixBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                nothingToFixText,
            },
        };
        nothingToFixPanel.Bind(IsVisibleProperty, new Binding(nameof(vm.NothingToFixIsVisible)));

        // "Analyzing..." while a re-scan runs, so a scan that ends in the same state as it started
        // still shows that it ran - this is what SE4 does on every re-scan (#12849).
        var analysingText = UiUtil.MakeTextBlock(Se.Language.Tools.FixCommonErrors.Analysing);
        analysingText.VerticalAlignment = VerticalAlignment.Center;
        analysingText.Opacity = 0.75;
        analysingText.Bind(IsVisibleProperty, new Binding(nameof(vm.AnalysingIsVisible)));

        var panelStep2Status = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { labelFixesApplied, nothingToFixPanel, analysingText },
        };
        panelStep2Status.Bind(IsVisibleProperty, new Binding(nameof(vm.Step2IsVisible)));
        grid.Children.Add(panelStep2Status);
        Grid.SetRow(panelStep2Status, 2);
        Grid.SetColumn(panelStep2Status, 0);

        grid.Children.Add(buttonPanelRight);
        Grid.SetRow(buttonPanelRight, 2);
        Grid.SetColumn(buttonPanelRight, 1);

        Content = grid;

        // Make Enter trigger - and put focus on - the current step's primary button, so the keyboard
        // flow works without a manual click: step 1 -> "Go to apply fixes", step 2 -> "Apply selected
        // fixes" (the repeated apply+re-scan action, not "Done"/close). Focusing a button (instead of
        // the window) still lets the window's OnKeyDown fire, as key events bubble up. (#12029)
        void FocusStepButton()
        {
            var step2 = vm.Step2IsVisible;
            buttonToApplyFixes.IsDefault = !step2;
            buttonDone.IsDefault = false;
            if (_buttonApplySelectedFixes != null)
            {
                _buttonApplySelectedFixes.IsDefault = step2;
            }

            Control? target = step2 ? _buttonApplySelectedFixes : buttonToApplyFixes;
            if (target != null)
            {
                Dispatcher.UIThread.Post(() => target.Focus());
            }
        }

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(vm.Step1IsVisible) or nameof(vm.Step2IsVisible))
            {
                FocusStepButton();
            }
            else if (e.PropertyName == nameof(vm.FixesAppliedText) && vm.Step2IsVisible && !string.IsNullOrEmpty(vm.FixesAppliedText))
            {
                // After "Apply selected fixes": if nothing is left to fix, move focus/default to
                // "Done" so the next Return finishes; otherwise keep it on "Apply selected fixes"
                // for another round (matches SE4). (#12029)
                var done = vm.Fixes.Count == 0;
                buttonDone.IsDefault = done;
                if (_buttonApplySelectedFixes != null)
                {
                    _buttonApplySelectedFixes.IsDefault = !done;
                }

                Control? target = done ? buttonDone : _buttonApplySelectedFixes;
                if (target != null)
                {
                    Dispatcher.UIThread.Post(() => target.Focus());
                }
            }
        };

        Activated += delegate { FocusStepButton(); };

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Grid MakeStep2Grid()
    {
        // top
        var gridFixes = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 0,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        var dataGridFixes = TableViewExtras.MakeTableView();
        dataGridFixes.DataContext = _vm;
        dataGridFixes.ItemsSource = _vm.VisibleFixes;

        var fixesApplyColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Apply,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<FixDisplayItem>((item, _) =>
            {
                return new Border
                {
                    Background = Brushes.Transparent, // Prevents highlighting
                    Padding = new Thickness(4),
                    Child = new CheckBox
                    {
                        Focusable = false,
                        [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixDisplayItem.IsSelected)),
                        // Unfocusable checkbox - name it after the fix it applies so a screen
                        // reader can tell which fix's apply state it is on (#11745).
                        [!AutomationProperties.NameProperty] = new Binding(nameof(FixDisplayItem.ActionDisplay)),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                };
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(80),
        };
        var fixesNumberColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(FixDisplayItem.Number)),
            Width = new GridLength(60),
        };
        var fixesActionColumn = new SeTableViewColumn
        {
            Header = Se.Language.Tools.FixCommonErrors.Action,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<FixDisplayItem>((item, _) =>
            {
                if (item == null)
                {
                    return new Border();
                }

                return new Border
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(4),
                    Child = new Border
                    {
                        Background = _vm.GetActionBackgroundBrush(item.ActionDisplay),
                        CornerRadius = new CornerRadius(5),
                        Padding = new Thickness(7, 2),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Child = new TextBlock
                        {
                            Text = item.ActionDisplay,
                            FontSize = 12,
                            Foreground = _vm.GetActionBrush(item.ActionDisplay),
                            VerticalAlignment = VerticalAlignment.Center,
                        },
                    },
                };
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(220),
        };
        var fixesBeforeColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Before,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<FixDisplayItem>((item, _) =>
            {
                var (beforeBlock, _) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                return new Border
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(4),
                    Child = beforeBlock,
                };
            }),
            Width = new GridLength(1, GridUnitType.Star),
        };
        var fixesAfterColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.After,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<FixDisplayItem>((item, _) =>
            {
                var (_, afterBlock) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                return new Border
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(4),
                    Child = afterBlock,
                };
            }),
            Width = new GridLength(1, GridUnitType.Star),
        };
        dataGridFixes.Columns.Add(fixesApplyColumn);
        dataGridFixes.Columns.Add(fixesNumberColumn);
        dataGridFixes.Columns.Add(fixesActionColumn);
        dataGridFixes.Columns.Add(fixesBeforeColumn);
        dataGridFixes.Columns.Add(fixesAfterColumn);
        AutomationProperties.SetName(dataGridFixes, Se.Language.Tools.FixCommonErrors.Fixes);
        dataGridFixes.Bind(TableView.SelectedItemProperty, new Binding(nameof(_vm.SelectedFix)));
        // Header sorting reorders VisibleFixes in place, which is presentation-only here:
        // applying fixes matches by (paragraph id, action) via AllowFix's lookup set, never
        // by row order, and a chip-filter change rebuilds the list anyway (kept sortable
        // per #12431; a re-scan or filter switch resets the sort order).
        new TableViewHeaderSorter(dataGridFixes)
            .AddSortable<FixDisplayItem, bool>(fixesApplyColumn, x => x.IsSelected)
            .AddSortable<FixDisplayItem, int>(fixesNumberColumn, x => x.Number)
            .AddSortable<FixDisplayItem, string>(fixesActionColumn, x => x.ActionDisplay)
            .AddSortable<FixDisplayItem, string>(fixesBeforeColumn, x => x.Before)
            .AddSortable<FixDisplayItem, string>(fixesAfterColumn, x => x.After);
        dataGridFixes.ContextMenu = new ContextMenu
        {
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.Tools.FixCommonErrors.RuleDetailsDotDotDot,
                    Command = _vm.ShowRuleDetailsCommand,
                },
                new MenuItem
                {
                    Header = Se.Language.Tools.FixCommonErrors.ShowOnlyThisRule,
                    Command = _vm.FilterBySelectedFixRuleCommand,
                },
            },
        };
        // Extended selection is native ListBox behavior on TableView; Space toggling every
        // selected row's checkbox is the piece the old DataGridCheckboxMultiSelect provided.
        TableViewExtras.AddSpaceToggle<FixDisplayItem>(dataGridFixes,
            item => item.IsSelected, (item, v) => item.IsSelected = v);
        // Keep the subtitle preview following the focused fix (the old helper's
        // onFocusedItemChanged callback).
        dataGridFixes.SelectionChanged += (_, _) =>
        {
            if (dataGridFixes.SelectedItem is FixDisplayItem item)
            {
                _vm.SelectAndScrollTo(item);
            }
        };

        var leftButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 0,
        };
        var buttonSelectAll = UiUtil.MakeButton(Se.Language.General.SelectAll, _vm.FixesSelectAllCommand);
        // Caption toggles between "Select all" and "Select none" as the current category fills up.
        buttonSelectAll.Bind(Button.ContentProperty, new Binding(nameof(_vm.FixesSelectAllText)) { Source = _vm });
        leftButtons.Children.Add(buttonSelectAll);

        var rightButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 0,
        };
        rightButtons.Children.Add(UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.RefreshFixes, _vm.DoRefreshFixesCommand).WithIconLeft("fa-solid fa-rotate"));
        // Caption carries the live count of fixes the button will apply, e.g. "Apply selected fixes (706)".
        _buttonApplySelectedFixes = UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.ApplySelectedFixes, _vm.DoApplyFixesCommand)
            .WithIconLeftBindText("fa-solid fa-check", nameof(_vm.ApplySelectedFixesText));
        AutomationProperties.SetName(_buttonApplySelectedFixes, Se.Language.Tools.FixCommonErrors.ApplySelectedFixes);
        rightButtons.Children.Add(_buttonApplySelectedFixes);

        var buttonBarFixes = new Grid
        {
            Margin = new Thickness(10, 10, 10, 10),
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        buttonBarFixes.Add(leftButtons, 0, 0);
        buttonBarFixes.Add(rightButtons, 0, 1);

        var chipsItems = new ItemsControl
        {
            Margin = new Thickness(10, 8, 10, 4),
            ItemsSource = _vm.FixChips,
            ItemsPanel = new FuncTemplate<Panel?>(() => new WrapPanel { Orientation = Orientation.Horizontal, ItemSpacing = 6, LineSpacing = 6 }),
            ItemTemplate = new FuncDataTemplate<FixFilterChip>((chip, _) =>
            {
                var content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                if (chip?.Action != null)
                {
                    content.Children.Add(new Avalonia.Controls.Shapes.Ellipse
                    {
                        Width = 7,
                        Height = 7,
                        VerticalAlignment = VerticalAlignment.Center,
                        Fill = _vm.GetActionBrush(chip.Action),
                    });
                }

                var chipText = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
                chipText.Bind(TextBlock.TextProperty, new Binding(nameof(FixFilterChip.Display)));
                content.Children.Add(chipText);

                var toggle = new ToggleButton
                {
                    Padding = new Thickness(10, 3),
                    CornerRadius = new CornerRadius(12),
                    Command = _vm.SetFixFilterCommand,
                    CommandParameter = chip,
                    Content = content,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixFilterChip.IsActive)),
                };
                AutomationProperties.SetName(toggle, chip?.Label ?? string.Empty);
                return toggle;
            }),
        };

        gridFixes.Children.Add(chipsItems);
        Grid.SetRow(chipsItems, 0);
        Grid.SetColumn(chipsItems, 0);

        gridFixes.Children.Add(dataGridFixes);
        Grid.SetRow(dataGridFixes, 1);
        Grid.SetColumn(dataGridFixes, 0);

        gridFixes.Children.Add(buttonBarFixes);
        Grid.SetRow(buttonBarFixes, 2);
        Grid.SetColumn(buttonBarFixes, 0);

        var borderFixes = UiUtil.MakeBorderForControlNoPadding(gridFixes);

        // bottom
        var gridSubtitles = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var syntaxHighlightingConverter = new TextWithSubtitleSyntaxHighlightingConverter();
        var textToFlowDirectionConverter = new TextToFlowDirectionConverter();
        var dataGridSubtitles = TableViewExtras.MakeTableView(multiSelect: false);
        dataGridSubtitles.DataContext = _vm;
        dataGridSubtitles.ItemsSource = _vm.Paragraphs;
        dataGridSubtitles.FontSize = Se.Settings.Appearance.SubtitleGridFontSize;
        dataGridSubtitles.Margin = new Thickness(Se.Settings.Appearance.GridCompactMode ? 0 : 2);
        dataGridSubtitles.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(60),
        });
        dataGridSubtitles.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(120),
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.StartTimeBackgroundBrush)),
                };
                border.Child = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                };
                return border;
            }),
        });
        dataGridSubtitles.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Hide,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(120),
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.EndTimeBackgroundBrush)),
                };
                border.Child = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter },
                };
                return border;
            }),
        });
        dataGridSubtitles.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(90),
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
                new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.DurationBackgroundBrush)),
                    Child = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        [!TextBlock.TextProperty] = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
                    },
                }),
        });
        dataGridSubtitles.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star),
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((_, _) =>
            {
                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.NoWrap,
                    [!TextBlock.InlinesProperty] = new Binding(nameof(SubtitleLineViewModel.Text)) { Converter = syntaxHighlightingConverter },

                    // Right-to-left text needs a right-to-left cell, or Avalonia splits the
                    // line at every zero width non-joiner and reverses the word order (#13160).
                    [!TextBlock.FlowDirectionProperty] = new Binding(nameof(SubtitleLineViewModel.Text)) { Converter = textToFlowDirectionConverter },
                };
                if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
                {
                    textBlock.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
                }

                return new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(SubtitleLineViewModel.TextBackgroundBrush)),
                    Child = textBlock,
                };
            }),
        });
        dataGridSubtitles.Bind(TableView.SelectedItemProperty, new Binding(nameof(_vm.SelectedParagraph)));
        AutomationProperties.SetName(dataGridSubtitles, Se.Language.General.Preview);
        _vm.GridSubtitles = dataGridSubtitles;
        // Home/End jump to the first/last row even when focus is on the grid itself
        // (this replaces the hand-rolled tunnel handler the DataGrid needed).
        TableViewExtras.AttachListNavigation(dataGridSubtitles);

        var gridCurrentSubtbtitle = MakeStep2EditPanel();

        gridSubtitles.Children.Add(dataGridSubtitles);
        Grid.SetRow(dataGridSubtitles, 0);
        Grid.SetColumn(dataGridSubtitles, 0);

        gridSubtitles.Children.Add(gridCurrentSubtbtitle);
        Grid.SetRow(gridCurrentSubtbtitle, 1);
        Grid.SetColumn(gridCurrentSubtbtitle, 0);

        var borderSubtitles = UiUtil.MakeBorderForControlNoPadding(gridSubtitles);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 150 },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 150 },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 0,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var splitter = new GridSplitter
        {
            Height = UiUtil.SplitterWidthOrHeight,
            ResizeDirection = GridResizeDirection.Rows,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 2),
        };

        grid.Children.Add(borderFixes);
        Grid.SetRow(borderFixes, 0);
        Grid.SetColumn(borderFixes, 0);

        grid.Children.Add(splitter);
        Grid.SetRow(splitter, 1);
        Grid.SetColumn(splitter, 0);

        grid.Children.Add(borderSubtitles);
        Grid.SetRow(borderSubtitles, 2);
        Grid.SetColumn(borderSubtitles, 0);

        return grid;
    }

    private Grid MakeStep2EditPanel()
    {
        var textEditGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(10),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var textBox = new TextBox
        {
            DataContext = _vm,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 60,
            FontSize = Se.Settings.Appearance.SubtitleTextBoxFontSize,
            FontWeight = Se.Settings.Appearance.SubtitleTextBoxFontBold ? FontWeight.Bold : FontWeight.Normal,
            [!TextBox.TextProperty] = new Binding($"{nameof(_vm.SelectedParagraph)}.{nameof(SubtitleLineViewModel.Text)}")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            },
        };
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            textBox.FontFamily = new FontFamily(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        AutomationProperties.SetName(textBox, Se.Language.General.Text);

        textEditGrid.Add(textBox, 0, 0);

        var panelSingleLineLengths = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Orientation = Orientation.Horizontal,
        };
        _vm.PanelSingleLineLengths = panelSingleLineLengths;
        textEditGrid.Add(panelSingleLineLengths, 1, 0);

        var totalLengthLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            FontSize = 12,
            Padding = new Thickness(2),
        };
        totalLengthLabel.Bind(TextBlock.TextProperty, new Binding(nameof(_vm.EditTextTotalLength)) { Source = _vm });
        totalLengthLabel.Bind(TextBlock.BackgroundProperty, new Binding(nameof(_vm.EditTextTotalLengthBackground)) { Source = _vm });
        textEditGrid.Add(totalLengthLabel, 1, 0);

        return textEditGrid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
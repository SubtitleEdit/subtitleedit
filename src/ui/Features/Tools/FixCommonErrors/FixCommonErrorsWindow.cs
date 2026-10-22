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
using Avalonia.Controls.Presenters;
using Avalonia.Styling;
using System;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public class FixCommonErrorsWindow : Window
{
    // Follow the app theme, also when it changes while the window is open
    private static readonly IBrush NothingToFixBrush = EditorChrome.Themed(Color.FromRgb(0x6e, 0xcb, 0x87), Color.FromRgb(0x1e, 0x7e, 0x34));
    private static readonly IBrush ErrorsFoundBrush = EditorChrome.Themed(Color.FromRgb(0xff, 0x8a, 0x80), Color.FromRgb(0xc4, 0x28, 0x28));

    private readonly FixCommonErrorsViewModel _vm;
    private Button? _buttonApplySelectedFixes;

    public FixCommonErrorsWindow(FixCommonErrorsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.FixCommonErrors);
        Width = 1024;
        Height = 720;
        MinWidth = 800;
        MinHeight = 600;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        AddChipStyles(Styles);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelStep1 = new TextBlock
        {
            Text = Se.Language.Tools.FixCommonErrors.FixCommonOcrErrorsStep1,
            FontSize = EditorChrome.FieldFontSize,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(4, 0, 12, 0),
        };
        labelStep1.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));

        var labelStep2 = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(4, 0, 12, 0),
        };
        labelStep2.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Step2Title)));
        labelStep2.Bind(IsVisibleProperty, new Binding(nameof(vm.Step2IsVisible)));

        var textBoxSearch = UiUtil.MakeTextBox(250, vm, nameof(vm.SearchText)).WithMarginRight(16)
            .WithAccessibleName(Se.Language.Tools.FixCommonErrors.SearchRulesDotDotDot)
            .WithSearchAndClearIcons();
        textBoxSearch.Classes.Add("text");
        textBoxSearch.VerticalAlignment = VerticalAlignment.Center;
        textBoxSearch.PlaceholderText = Se.Language.Tools.FixCommonErrors.SearchRulesDotDotDot;
        textBoxSearch.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));
        // Off by default (#12441) - keep it reachable here, next to the language it depends on,
        // instead of only in the OCR window where a Fix-common-errors user would never look.
        var switchGuessUnknownWords = EditorChrome.MakeSwitch(nameof(vm.TryToGuessUnknownWords), Se.Language.Ocr.TryToGuessUnknownWords);
        var panelGuessUnknownWords = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 16, 0),
            Children =
            {
                switchGuessUnknownWords,
                new TextBlock { Text = Se.Language.Ocr.TryToGuessUnknownWords, FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center },
            },
        };
        panelGuessUnknownWords.Bind(IsVisibleProperty, new Binding(nameof(vm.Step2IsVisible)));

        var comboLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage))
            .WithAccessibleName(Se.Language.General.Language);
        comboLanguage.VerticalAlignment = VerticalAlignment.Center;
        var panelTopRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                // Filters the step 1 rule list by name (#14893); step 2 shows the switch instead.
                textBoxSearch,
                panelGuessUnknownWords,
                EditorChrome.MakeDimLabel(Se.Language.General.Language, new Thickness(0, 0, 8, 0)),
                comboLanguage,
            },
        };

        var toolbarContent = new DockPanel { MinHeight = 32 };
        DockPanel.SetDock(panelTopRight, Dock.Right);
        toolbarContent.Children.Add(panelTopRight);
        toolbarContent.Children.Add(new Panel { Children = { labelStep1, labelStep2 }, VerticalAlignment = VerticalAlignment.Center });
        var toolbar = EditorChrome.MakeToolbar(toolbarContent);
        toolbar.Padding = new Thickness(12, 8);

        var rulesGrid = TableViewExtras.MakeTableView();
        rulesGrid[!TableView.ItemsSourceProperty] = new Binding($"{nameof(vm.SelectedProfile)}.{nameof(ProfileDisplayItem.FixRules)}");

        var rulesEnabledColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<FixRuleDisplayItem>(nameof(FixRuleDisplayItem.IsSelected), nameof(FixRuleDisplayItem.Name)),
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

        // Rules header: select all / invert, then the profile picker and its editor.
        var comboProfile = UiUtil.MakeComboBox(vm.Profiles, vm, nameof(vm.SelectedProfile))
            .WithAccessibleName(Se.Language.General.Profile);
        comboProfile.VerticalAlignment = VerticalAlignment.Center;
        var rulesActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                EditorChrome.CreateToolButton("fa-solid fa-square-check", Se.Language.General.SelectAll, vm.RulesSelectAllCommand, 26),
                EditorChrome.CreateToolButton("fa-solid fa-right-left", Se.Language.General.InvertSelection, vm.RulesInverseSelectedCommand, 26),
                EditorChrome.MakeToolbarSeparator(),
                EditorChrome.MakeDimLabel(Se.Language.General.Profile, new Thickness(0, 0, 8, 0)),
                comboProfile,
                EditorChrome.CreateToolButton("fa-solid fa-sliders", Se.Language.General.Profiles, vm.ShowProfileCommand, 28),
            },
        };
        var rulesHeader = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(rulesActions, Dock.Right);
        rulesHeader.Children.Add(rulesActions);
        rulesHeader.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Rules));

        var rulesPanel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        rulesPanel.Add(rulesHeader, 0);
        rulesPanel.Add(EditorChrome.MakeListFrame(rulesGrid, clearListBackground: true), 1);
        var step1View = EditorChrome.MakePanel(rulesPanel);
        step1View.Bind(IsVisibleProperty, new Binding(nameof(vm.Step1IsVisible)));

        var step2Grid = MakeStep2Grid();
        step2Grid.Bind(IsVisibleProperty, new Binding(nameof(_vm.Step2IsVisible)));

        var buttonToApplyFixes = UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.GoToApplyFixes, vm.ToApplyFixesCommand)
            .WithIconRight("fa-solid fa-arrow-right")
            .BindIsVisible(vm, nameof(vm.Step1IsVisible));
        buttonToApplyFixes.Classes.Add("primary");

        var buttonBackToFixList = UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.BackToFixList, vm.BackToFixListCommand)
            .WithIconLeft("fa-solid fa-arrow-left")
            .BindIsVisible(vm, nameof(vm.Step2IsVisible));

        var buttonDone = UiUtil.MakeButton(Se.Language.General.Done, vm.OkCommand)
            .BindIsVisible(vm, nameof(vm.Step2IsVisible));
        buttonDone.Classes.Add("primary");

        var labelFixesApplied = EditorChrome.MakeBoundHint(nameof(vm.FixesAppliedText));
        labelFixesApplied.FontSize = EditorChrome.FieldFontSize;
        labelFixesApplied.Bind(IsVisibleProperty, new Binding(nameof(vm.FixesAppliedText)) { Converter = StringConverters.IsNotNullOrEmpty });

        // Green check + "Nothing to fix" so an empty re-scan gives visible feedback; the counters
        // stay as they were, which on its own reads as "the button did nothing" (#12849).
        var nothingToFixBrush = NothingToFixBrush;
        var nothingToFixText = new TextBlock
        {
            Text = Se.Language.Tools.FixCommonErrors.NothingToFix,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = nothingToFixBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
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
                    FontSize = 14,
                    Foreground = nothingToFixBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                nothingToFixText,
            },
        };
        nothingToFixPanel.Bind(IsVisibleProperty, new Binding(nameof(vm.NothingToFixIsVisible)));

        // Errors the rules found but could not fix (e.g. a display time that is too short with no
        // room to extend it). Without this they are silent, and a subtitle full of them still shows
        // the green "Nothing to fix" - click the warning to see one log line per error (#13645).
        var errorsFoundBrush = ErrorsFoundBrush;
        var errorsFoundIcon = new Optris.Icons.Avalonia.Icon
        {
            Value = IconNames.Alert,
            FontSize = 14,
            Foreground = errorsFoundBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        errorsFoundIcon.PointerPressed += (_, _) => vm.ShowLogCommand.Execute(null);
        var errorsFoundText = UiUtil.MakeLink(string.Empty, vm.ShowLogCommand);
        errorsFoundText.FontSize = EditorChrome.FieldFontSize;
        errorsFoundText.Foreground = errorsFoundBrush;
        errorsFoundText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ErrorsFoundText)));
        var errorsFoundPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { errorsFoundIcon, errorsFoundText },
        };
        errorsFoundPanel.Bind(IsVisibleProperty, new Binding(nameof(vm.ErrorsFoundIsVisible)));
        AutomationProperties.SetName(errorsFoundPanel, Se.Language.Tools.FixCommonErrors.Log);
        if (Se.Settings.Appearance.ShowHints)
        {
            UiUtil.AttachHoverTooltip(errorsFoundPanel, Se.Language.Tools.FixCommonErrors.Log);
        }

        // The log also holds what the applies changed, so it stays reachable when a subtitle has
        // no errors left - the warning above links to the same window when it does.
        var logLink = UiUtil.MakeLink(Se.Language.Tools.FixCommonErrors.Log, vm.ShowLogCommand);
        logLink.FontSize = EditorChrome.FieldFontSize;
        logLink.Bind(IsVisibleProperty, new Binding(nameof(vm.LogIsVisible)));

        // "Analyzing..." while a re-scan runs, so a scan that ends in the same state as it started
        // still shows that it ran - this is what SE4 does on every re-scan (#12849).
        var analysingText = new TextBlock
        {
            Text = Se.Language.Tools.FixCommonErrors.Analysing,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        analysingText.Bind(IsVisibleProperty, new Binding(nameof(vm.AnalysingIsVisible)));

        var panelStep2Status = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { labelFixesApplied, nothingToFixPanel, errorsFoundPanel, logLink, analysingText },
        };
        panelStep2Status.Bind(IsVisibleProperty, new Binding(nameof(vm.Step2IsVisible)));

        var buttonBar = EditorChrome.MakeButtonBar(panelStep2Status, buttonBackToFixList, UiUtil.MakeButtonCancel(vm.CancelCommand), buttonToApplyFixes, buttonDone);

        var body = new Panel { Children = { step1View, step2Grid } };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Add(toolbar, 0);
        grid.Add(body, 1);
        grid.Add(buttonBar, 2);

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

        UiUtil.FocusOnFirstActivation(this, () => { FocusStepButton(); });

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Grid MakeStep2Grid()
    {
        var dataGridFixes = TableViewExtras.MakeTableView();
        dataGridFixes.DataContext = _vm;
        dataGridFixes.ItemsSource = _vm.VisibleFixes;

        var fixesApplyColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Apply,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<FixDisplayItem>(nameof(FixDisplayItem.IsSelected), nameof(FixDisplayItem.ActionDisplay)),
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
                            FontSize = UiUtil.ScaledFontSize(12),
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
            item => item.IsSelected, _vm.SetFixesSelected);
        // Keep the subtitle preview following the focused fix (the old helper's
        // onFocusedItemChanged callback).
        dataGridFixes.SelectionChanged += (_, _) =>
        {
            if (dataGridFixes.SelectedItem is FixDisplayItem item)
            {
                _vm.SelectAndScrollTo(item);
            }
        };

        // Fixes header: select all/none, re-scan and apply - the repeated step 2 action.
        var buttonSelectAll = EditorChrome.MakeTextToolButton("fa-solid fa-square-check", Se.Language.General.SelectAll, _vm.FixesSelectAllCommand);
        // Caption toggles between "Select all" and "Select none" as the current category fills up.
        if (buttonSelectAll.Content is StackPanel { Children: [_, TextBlock selectAllText] })
        {
            selectAllText.Bind(TextBlock.TextProperty, new Binding(nameof(_vm.FixesSelectAllText)) { Source = _vm });
        }

        buttonSelectAll.Bind(AutomationProperties.NameProperty, new Binding(nameof(_vm.FixesSelectAllText)) { Source = _vm });
        var buttonRefreshFixes = EditorChrome.MakeTextToolButton("fa-solid fa-rotate", Se.Language.Tools.FixCommonErrors.RefreshFixes, _vm.DoRefreshFixesCommand);

        // Caption carries the live count of fixes the button will apply, e.g. "Apply selected fixes (706)".
        _buttonApplySelectedFixes = UiUtil.MakeButton(Se.Language.Tools.FixCommonErrors.ApplySelectedFixes, _vm.DoApplyFixesCommand)
            .WithIconLeftBindText("fa-solid fa-check", nameof(_vm.ApplySelectedFixesText));
        AutomationProperties.SetName(_buttonApplySelectedFixes, Se.Language.Tools.FixCommonErrors.ApplySelectedFixes);
        _buttonApplySelectedFixes.Margin = new Thickness(6, 0, 0, 0);
        _buttonApplySelectedFixes.MinHeight = 28;
        _buttonApplySelectedFixes.Padding = new Thickness(10, 4);
        _buttonApplySelectedFixes.FontSize = EditorChrome.FieldFontSize;
        _buttonApplySelectedFixes.VerticalAlignment = VerticalAlignment.Center;

        var fixesActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonSelectAll, EditorChrome.MakeToolbarSeparator(), buttonRefreshFixes, _buttonApplySelectedFixes },
        };
        var fixesHeader = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
        DockPanel.SetDock(fixesActions, Dock.Right);
        fixesHeader.Children.Add(fixesActions);
        fixesHeader.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.Tools.FixCommonErrors.Fixes));

        var chipsItems = new ItemsControl
        {
            Margin = new Thickness(0, 0, 0, 8),
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
                    Classes = { "chip" },
                    FontSize = EditorChrome.FieldFontSize,
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

        var gridFixes = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(14, 12, 14, 12),
        };
        gridFixes.Add(fixesHeader, 0);
        gridFixes.Add(chipsItems, 1);
        gridFixes.Add(EditorChrome.MakeListFrame(dataGridFixes, clearListBackground: true), 2);

        var borderFixes = EditorChrome.MakePanel(gridFixes);

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
                    textBlock.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
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

        var gridSubtitles = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = new Thickness(14, 10, 14, 12),
        };
        var subtitlesHeader = EditorChrome.MakeSectionHeader(Se.Language.General.Preview);
        subtitlesHeader.Margin = new Thickness(0, 0, 0, 8);
        gridSubtitles.Add(subtitlesHeader, 0);
        gridSubtitles.Add(EditorChrome.MakeListFrame(dataGridSubtitles, clearListBackground: true), 1);
        gridSubtitles.Add(gridCurrentSubtbtitle, 2);

        var borderSubtitles = EditorChrome.MakePanel(gridSubtitles);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 150 },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 150 },
            },
        };

        // The splitter doubles as the line between the fixes and the subtitle preview.
        var splitter = new GridSplitter
        {
            Height = 5,
            Background = EditorChrome.LineBrush,
            ResizeDirection = GridResizeDirection.Rows,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(borderFixes, 0);
        grid.Add(splitter, 1);
        grid.Add(borderSubtitles, 2);

        return grid;
    }

    private Grid MakeStep2EditPanel()
    {
        var textEditGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(0, 10, 0, 0),
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
            textBox.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        AutomationProperties.SetName(textBox, Se.Language.General.Text);
        textBox.Classes.Add("text");

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
            FontSize = UiUtil.ScaledFontSize(12),
            Padding = new Thickness(2),
        };
        totalLengthLabel.Bind(TextBlock.TextProperty, new Binding(nameof(_vm.EditTextTotalLength)) { Source = _vm });
        totalLengthLabel.Bind(TextBlock.BackgroundProperty, new Binding(nameof(_vm.EditTextTotalLengthBackground)) { Source = _vm });
        textEditGrid.Add(totalLengthLabel, 1, 0);

        return textEditGrid;
    }

    /// <summary>
    /// The filter chips above the fixes: a field-colored pill, the soft accent while active.
    /// </summary>
    private static void AddChipStyles(Styles styles)
    {
        styles.Add(new Style(x => x.OfType<ToggleButton>().Class("chip"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)),
                new Setter(TemplatedControl.MinHeightProperty, 0.0),
            },
        });

        static Style Presenter(Func<Selector?, Selector> selector, IBrush background, IBrush border, IBrush foreground) =>
            new(x => selector(x).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, background),
                    new Setter(ContentPresenter.BorderBrushProperty, border),
                    new Setter(ContentPresenter.ForegroundProperty, foreground),
                },
            };

        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("chip").Class(":pointerover"),
            EditorChrome.HoverBrush, EditorChrome.SeparatorBrush, EditorChrome.StrongTextBrush));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("chip").Class(":checked"),
            EditorChrome.AccentSoftBrush, EditorChrome.AccentBrush, EditorChrome.StrongTextBrush));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("chip").Class(":checked").Class(":pointerover"),
            EditorChrome.AccentSoftBrush, EditorChrome.AccentHoverBrush, EditorChrome.StrongTextBrush));
        styles.Add(Presenter(x => x.OfType<ToggleButton>().Class("chip").Class(":pressed"),
            EditorChrome.PressedBrush, EditorChrome.SeparatorBrush, EditorChrome.StrongTextBrush));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
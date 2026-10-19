using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

/// <summary>
/// Remove text for hearing impaired, in the editor look (see <see cref="EditorChrome"/>): the
/// options in sections on the left (scrolling on short screens), the fixes preview on the right,
/// the number of lines found and the buttons in the bar at the bottom.
/// </summary>
public class RemoveTextForHearingImpairedWindow : Window
{
    // Same presets the SE 4 combo boxes offered - awkward characters to type, and they cover
    // nearly every real use of these fields. All three fields stay free text.
    private static readonly string[] SymbolPresets = ["¶", "♪", "♫"];
    private static readonly string[] ContainsPresets = ["¶", "♪", "♫", "♪,♫"];

    private readonly RemoveTextForHearingImpairedViewModel _vm;

    public RemoveTextForHearingImpairedWindow(RemoveTextForHearingImpairedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.RemoveTextForHearingImpaired);
        Width = 960;
        Height = 700;
        MinWidth = 820;
        MinHeight = 480;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        // The text of the editable (symbol) combo boxes in the middle of the compact box
        Styles.Add(new Style(x => x.OfType<ComboBox>().Template().OfType<TextBox>().Name("PART_EditableTextBox"))
        {
            Setters =
            {
                new Setter(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                new Setter(TextBox.PaddingProperty, new Thickness(0)),
                new Setter(TextBox.MinHeightProperty, 0.0),
            },
        });

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var settingsView = MakeSettingsView(vm);
        var fixesView = MakeFixesView(vm);

        // Settings-only callers (BatchConvert) have no live target and commit once with Ok/Cancel;
        // edit callers (menu, selected lines) apply live and repeatedly with Apply, then Done closes.
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand)
            .WithBindIsVisible(nameof(vm.IsSettingsMode));
        buttonOk.Classes.Add("primary");
        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand)
            .WithBindIsVisible(nameof(vm.IsApplyVisible));
        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand)
            .WithBindIsVisible(nameof(vm.IsApplyVisible));
        buttonDone.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand)
            .WithBindIsVisible(nameof(vm.IsSettingsMode));

        var labelLinesFound = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0));
        labelLinesFound.Bind(TextBlock.TextProperty, new Binding($"{nameof(vm.Fixes)}.{nameof(vm.Fixes.Count)}")
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            StringFormat = Se.Language.Tools.RemoveTextForHearingImpaired.LinesFoundX,
        });

        var sections = EditorChrome.MakeSideBySide(settingsView, fixesView);
        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(labelLinesFound, buttonCancel, buttonApply, buttonOk, buttonDone));

        UiUtil.FocusOnFirstActivation(this, () => { (vm.IsApplyVisible ? buttonDone : buttonOk).Focus(); }); // hack to make OnKeyDown work

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private static ScrollViewer MakeSettingsView(RemoveTextForHearingImpairedViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Width = 340,
            Children =
            {
                MakeRemoveBetweenView(vm),
                MakeOtherView(vm),
                MakeInterjectionsView(vm),
            }
        };

        // Many options: they scroll on short screens, so the buttons stay reachable.
        return new ScrollViewer
        {
            Content = panel,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
    }

    private static Border MakeRemoveBetweenView(RemoveTextForHearingImpairedViewModel vm)
    {
        var l = Se.Language.Tools.RemoveTextForHearingImpaired;

        var toggleCustom = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(nameof(vm.IsRemoveCustomOn)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggleCustom, Se.Language.Edit.ModifySelection.HearingImpairedCustom);
        var textBoxCustomStart = UiUtil.MakeEditableComboBox(80, SymbolPresets, vm, nameof(vm.CustomStart))
            .WithAccessibleName(l.RemoveTextBetween);
        var labelAnd = EditorChrome.MakeDimLabel(l.And, new Thickness(0));
        var textBoxCustomEnd = UiUtil.MakeEditableComboBox(80, SymbolPresets, vm, nameof(vm.CustomEnd))
            .WithAccessibleName(l.And);
        var panelCustom = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                toggleCustom,
                textBoxCustomStart,
                labelAnd,
                textBoxCustomEnd,
            }
        };

        var panel = new StackPanel
        {
            Spacing = 6,
            Children =
            {
                EditorChrome.MakeToggleRow(nameof(vm.IsRemoveBracketsOn), l.Brackets),
                EditorChrome.MakeToggleRow(nameof(vm.IsRemoveCurlyBracketsOn), l.CurlyBrackets),
                EditorChrome.MakeToggleRow(nameof(vm.IsRemoveParenthesesOn), l.Parentheses),
                panelCustom,
                EditorChrome.MakeToggleRow(nameof(vm.IsOnlySeparateLine), l.OnlySeparateLines).WithMarginTop(4),
            }
        };

        return EditorChrome.MakeEditorSection(l.RemoveTextBetween, panel, first: true);
    }

    private static Border MakeOtherView(RemoveTextForHearingImpairedViewModel vm)
    {
        var l = Se.Language.Tools.RemoveTextForHearingImpaired;

        var textBoxWhitelist = UiUtil.MakeTextBox(150, vm, nameof(vm.UppercaseWhitelist))
            .WithAccessibleName(l.KeepUppercaseWords);
        textBoxWhitelist.Classes.Add("text");
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(textBoxWhitelist, l.KeepUppercaseWords);
        }

        var textBoxContains = UiUtil.MakeEditableComboBox(150, ContainsPresets, vm, nameof(vm.TextContains))
            .WithAccessibleName(l.IfLineContains);

        var panel = new StackPanel
        {
            Spacing = 6,
            Children =
            {
                EditorChrome.MakeToggleRow(nameof(vm.IsRemoveTextBeforeColonOn), l.RemoveTextBeforeColon),
                EditorChrome.MakeToggleRow(nameof(vm.IsRemoveTextBeforeColonUppercaseOn), l.OnlyIfTextIsUppercase).WithMarginLeft(20),
                EditorChrome.MakeToggleRow(nameof(vm.IsRemoveTextBeforeColonSeparateLineOn), l.OnlyOnSeparateLine).WithMarginLeft(20),
                MakeToggleWithField(nameof(vm.IsRemoveTextUppercaseLineOn), l.IfLineIsUppercase, textBoxWhitelist).WithMarginTop(4),
                MakeToggleWithField(nameof(vm.IsRemoveTextContainsOn), l.IfLineContains, textBoxContains),
                EditorChrome.MakeToggleRow(nameof(vm.IsRemoveOnlyMusicSymbolsOn), l.IfLineOnlyContainsMusicSymbols),
            }
        };

        return EditorChrome.MakeEditorSection(Se.Language.General.Other, panel);
    }

    private static Border MakeInterjectionsView(RemoveTextForHearingImpairedViewModel vm)
    {
        var l = Se.Language.Tools.RemoveTextForHearingImpaired;

        var buttonEdit = EditorChrome.MakeTextToolButton("fa-solid fa-pen-to-square", Se.Language.General.Edit, vm.EditInterjectionsCommand);

        var comboBoxLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage))
            .WithAccessibleName(Se.Language.General.Language);
        comboBoxLanguage.HorizontalAlignment = HorizontalAlignment.Stretch;

        var panel = new StackPanel
        {
            Spacing = 6,
            Children =
            {
                EditorChrome.MakeToggleRow(nameof(vm.IsRemoveInterjectionsOn), l.RemoveInterjections),
                EditorChrome.MakeLabeledRow(Se.Language.General.Language, comboBoxLanguage, 80).WithMarginTop(4),
                EditorChrome.MakeToggleRow(nameof(vm.IsInterjectionsSeparateLineOn), l.OnlySeparateLines).WithMarginTop(4),
            }
        };

        return EditorChrome.MakeEditorSection(Se.Language.General.Interjections, panel, buttonEdit);
    }

    /// <summary>
    /// A switch with its label, then a field for its text (whitelist, "contains" symbols).
    /// </summary>
    private static StackPanel MakeToggleWithField(string property, string text, Control field)
    {
        field.Margin = new Thickness(48, 0, 0, 0); // under the label, past the switch
        field.HorizontalAlignment = HorizontalAlignment.Stretch;
        field.Width = double.NaN;
        return new StackPanel
        {
            Spacing = 4,
            Children = { EditorChrome.MakeToggleRow(property, text), field },
        };
    }

    private Grid MakeFixesView(RemoveTextForHearingImpairedViewModel vm)
    {
        // Sorting dropped in the DataGrid -> TableView conversion: the grid previews
        // fixes in subtitle order.
        var dataGrid = TableViewExtras.MakeTableView().WithAccessibleName(Se.Language.General.Preview);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = _vm;
        dataGrid.ItemsSource = _vm.Fixes;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<RemoveItem>(nameof(RemoveItem.Apply)),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(70)
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(RemoveItem.IndexDisplay)),
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    // Content-sized (Auto) on the DataGrid; Before holds subtitle text,
                    // so it shares the star width with After.
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(RemoveItem.Before)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(RemoveItem.After)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(_vm.SelectedFix)));

        // Extended selection is native ListBox behavior on TableView; only the
        // Space-toggles-checkbox piece of the old CheckboxMultiSelect needs wiring.
        TableViewExtras.AddSpaceToggle<RemoveItem>(dataGrid,
            item => item.Apply, (item, v) => item.Apply = v);

        dataGrid.ContextMenu = MakeFixesContextMenu(vm);

        // The context menu gestures must mean the same with the grid focused, so take them
        // before the TableView (a ListBox) turns Ctrl+A into "select all rows" (#13496).
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (vm.HandleFixesSelectionKey(e))
            {
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        // The list fills the rest of the window, so the section header and list are laid out
        // in a grid rather than in a section's stack panel.
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 14),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        grid.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Preview), 0, 0);
        grid.Add(EditorChrome.MakeListFrame(dataGrid, clearListBackground: true), 1, 0);

        return grid;
    }

    // Mass-ticking the "Apply" column by hand is not an option on a long list (1259 fixes in
    // the report on #13496), so offer tick all / untick all / invert with SE's usual gestures.
    private static ContextMenu MakeFixesContextMenu(RemoveTextForHearingImpairedViewModel vm)
    {
        var commandModifier = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;

        return new ContextMenu
        {
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.General.SelectAll,
                    DataContext = vm,
                    Command = vm.SelectAllFixesCommand,
                    InputGesture = new KeyGesture(Key.A, commandModifier),
                },
                new MenuItem
                {
                    Header = Se.Language.General.SelectNone,
                    DataContext = vm,
                    Command = vm.SelectNoFixesCommand,
                    InputGesture = new KeyGesture(Key.D, commandModifier),
                },
                new MenuItem
                {
                    Header = Se.Language.General.InvertSelection,
                    DataContext = vm,
                    Command = vm.InvertFixesSelectionCommand,
                    InputGesture = new KeyGesture(Key.I, commandModifier | KeyModifiers.Shift),
                },
            },
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded(e);
    }
}

using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;
using System;
using System.Linq;

/// <summary>
/// Compare as a merge view: the current subtitle on the left, the reference on the right, one
/// aligned pair per row, and a gutter between them that pulls a line across. Only the current
/// side is ever edited - it is the editor's own subtitle, so Apply hands the result straight
/// back, while the reference stays the fixed thing it is compared against (#14358).
/// </summary>
public class CompareWindow : Window
{
    private const double GutterWidth = 52;
    private const string EditButtonClass = "compareEdit";

    private readonly CompareViewModel _vm;
    private readonly CompareOverviewRuler _ruler = new();
    private ScrollViewer? _scrollViewer;

    public CompareWindow(CompareViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Compare;
        Width = 1300;
        Height = 800;
        MinWidth = 900;
        MinHeight = 500;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;
        _vm = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto), // filter tabs + options + navigation
                new RowDefinition(GridLength.Auto), // the two file headers
                new RowDefinition(GridLength.Star), // the aligned rows
                new RowDefinition(GridLength.Auto), // status text + legend
                new RowDefinition(GridLength.Auto), // pending changes + buttons
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
        };

        grid.Add(MakeToolbar(vm), 0);
        grid.Add(MakeHeaders(vm), 1);
        grid.Add(MakeRowsArea(vm), 2);
        grid.Add(MakeStatusBar(vm), 3);
        grid.Add(MakeBottomBar(vm), 4);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            Dispatcher.UIThread.Post(() => vm.RowsView?.Focus()); // an input, not an action button - a focused button clicks on bare Space
        });

        // Tunnel, so Ctrl+Enter and Escape reach the edit commands before the line's text box takes them.
        AddHandler(KeyDownEvent, vm.KeyDown, RoutingStrategies.Tunnel);

        vm.RowsRebuilt += (_, _) => _ruler.SetRows(vm.Rows);
        _ruler.ScrollRequested += (_, fraction) => ScrollToFraction(fraction);

        Closing += vm.WindowClosing;
        Closed += delegate
        {
            UiUtil.SaveWindowPosition(this);
            vm.SaveSettings(); // the compare options are remembered between sessions (#14299)
        };
        Loaded += delegate
        {
            UiUtil.RestoreWindowPosition(this);
            HookScrollViewer();
        };
    }

    private Control MakeToolbar(CompareViewModel vm)
    {
        var tabs = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Children =
            {
                MakeTab(Se.Language.General.All, nameof(vm.AllCount), CompareVisualType.All, vm.ShowAllCommand),
                MakeTab(Se.Language.File.CompareDifferences, nameof(vm.DifferenceCount), CompareVisualType.ShowOnlyDifferences, vm.ShowDifferencesCommand),
                MakeTab(Se.Language.File.CompareTextDifferences, nameof(vm.TextDifferenceCount), CompareVisualType.ShowOnlyDifferencesInText, vm.ShowTextDifferencesCommand),
            },
        };

        var segmented = new Border
        {
            Background = UiUtil.GetTextColor(0.06),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(2),
            VerticalAlignment = VerticalAlignment.Center,
            Child = tabs,
        };

        var options = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16, 0, 0, 0),
            Children =
            {
                MakeOption(vm, Se.Language.File.IgnoreWhitespace, Se.Language.File.IgnoreWhitespaceHint, nameof(vm.IgnoreWhiteSpace)),
                MakeOption(vm, Se.Language.File.IgnoreFormatting, Se.Language.File.IgnoreFormattingHint, nameof(vm.IgnoreFormatting)),
                MakeOption(vm, Se.Language.File.IgnoreNumbering, Se.Language.File.IgnoreNumberingHint, nameof(vm.IgnoreNumbering)),
            },
        };

        var buttonPrevious = UiUtil.MakeButton(vm.PreviousDifferenceCommand, IconNames.ChevronLeft, Se.Language.File.PreviousDifference + " (Shift+F8)");
        var buttonNext = UiUtil.MakeButton(vm.NextDifferenceCommand, IconNames.ChevronRight, Se.Language.File.NextDifference + " (F8)");
        var navigation = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonPrevious, buttonNext },
        };

        var bar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        bar.Add(segmented, 0);
        bar.Add(options, 0, 1);
        bar.Add(navigation, 0, 2);
        return bar;
    }

    private ToggleButton MakeTab(string label, string countPath, CompareVisualType type, System.Windows.Input.ICommand command)
    {
        var count = new Border
        {
            Background = UiUtil.GetTextColor(0.12),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(6, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                FontSize = UiUtil.ScaledFontSize(11),
                [!TextBlock.TextProperty] = new Binding(countPath),
            },
        };

        var tab = new ToggleButton
        {
            Command = command,
            Padding = new Thickness(12, 4),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(0),
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 7,
                Children = { new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center }, count },
            },
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(CompareViewModel.SelectedCompareVisual))
            {
                Mode = BindingMode.OneWay,
                Converter = new FuncValueConverter<CompareVisual?, bool>(v => v?.Type == type),
            },
        };

        AutomationProperties.SetName(tab, label);
        return tab;
    }

    private static CheckBox MakeOption(CompareViewModel vm, string text, string hint, string propertyPath)
    {
        var checkBox = UiUtil.MakeCheckBox(text, vm, propertyPath);
        checkBox.IsCheckedChanged += vm.CheckBoxChanged;
        AddHint(checkBox, hint);
        return checkBox;
    }

    private Control MakeHeaders(CompareViewModel vm)
    {
        // Left: the current subtitle, editable while it is still the editor's own.
        var labelLeftFileName = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.LeftFileNameDisplay)),
            [!ToolTip.TipProperty] = new Binding(nameof(vm.LeftFileName)),
        };
        var labelLeftHasChanges = UiUtil.MakeLabel("*").WithBindVisible(vm, nameof(vm.LeftFileNameHasChanges));
        var pillLeft = MakePill(IconNames.Pencil, nameof(vm.LeftSideLabel), highlighted: true)
            .WithBindIsVisible(nameof(vm.IsLeftEditable));
        AddHint(pillLeft, Se.Language.File.CompareEditableHint);
        var pillLeftReadOnly = MakePill(IconNames.Lock, nameof(vm.LeftSideLabel), highlighted: false)
            .WithBindIsVisible(nameof(vm.IsLeftEditable), new FuncValueConverter<bool, bool>(v => !v));
        var buttonLeftBrowse = UiUtil.MakeButtonBrowse(vm.PickLeftSubtitleFileCommand, accessibleName: Se.Language.General.OpenOriginalSubtitleFileTitle);

        var left = MakeHeaderCard(
            MakeIcon(IconNames.FileOutline, 0.9),
            labelLeftFileName,
            labelLeftHasChanges,
            MakeCaption(Se.Language.File.CompareCurrent),
            pillLeft,
            pillLeftReadOnly,
            buttonLeftBrowse);

        // Right: the reference, which is only ever read.
        var labelRightFileName = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.RightFileNameDisplay)),
            [!ToolTip.TipProperty] = new Binding(nameof(vm.RightFileName)),
        };
        var pillRight = MakePill(IconNames.Lock, string.Empty, highlighted: false, fixedText: Se.Language.File.CompareReadOnly);
        var buttonRightReload = UiUtil.MakeButton(string.Format(Se.Language.File.LoadXFromFile, System.IO.Path.GetFileName(vm.LeftFileName)), vm.ReloadRightFromFileCommand)
            .WithIconLeft(IconNames.Refresh)
            .WithBindIsVisible(nameof(vm.IsReloadFromFileVisible));
        var buttonRightBrowse = UiUtil.MakeButtonBrowse(vm.PickRightSubtitleFileCommand, accessibleName: Se.Language.General.OpenSubtitleFileTitle);

        var right = MakeHeaderCard(
            MakeIcon(IconNames.FileOutline, 0.6),
            labelRightFileName,
            new Control(),
            MakeCaption(Se.Language.File.CompareReference),
            pillRight,
            buttonRightReload,
            buttonRightBrowse);

        var headers = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GutterWidth, GridUnitType.Pixel),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(_ruler.Width + 6, GridUnitType.Pixel),
            },
        };
        headers.Add(MakeDropHost(left, vm.FileGridOnDropLeft), 0);
        headers.Add(MakeDropHost(right, vm.FileGridOnDropRight), 0, 2);
        return headers;
    }

    private static Border MakeHeaderCard(Control icon, TextBlock fileName, Control hasChanges, Control caption, params Control[] trailing)
    {
        var namePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { icon, fileName, hasChanges, caption },
        };

        var trailingPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
        };
        foreach (var control in trailing)
        {
            trailingPanel.Children.Add(control);
        }

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            MinHeight = 32,
        };
        grid.Add(namePanel, 0);
        grid.Add(trailingPanel, 0, 1);

        return new Border
        {
            Background = UiUtil.GetTextColor(0.04),
            BorderBrush = UiUtil.GetTextColor(0.1),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10, 5),
            Child = grid,
        };
    }

    private static Border MakePill(string iconName, string textPath, bool highlighted, string? fixedText = null)
    {
        var text = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(11),
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
        };
        if (fixedText != null)
        {
            text.Text = fixedText;
        }
        else
        {
            text.Bind(TextBlock.TextProperty, new Binding(textPath));
        }

        var icon = MakeIcon(iconName, 0.85);
        icon.FontSize = UiUtil.ScaledFontSize(11);

        var accent = UiUtil.GetAccentBrush();
        return new Border
        {
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(8, 2),
            VerticalAlignment = VerticalAlignment.Center,
            BorderThickness = new Thickness(1),
            BorderBrush = highlighted ? accent : UiUtil.GetTextColor(0.2),
            Background = highlighted ? new SolidColorBrush(((ISolidColorBrush)accent).Color, 0.15) : UiUtil.GetTextColor(0.05),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                Children = { icon, text },
            },
        };
    }

    private Control MakeRowsArea(CompareViewModel vm)
    {
        var listBox = new ListBox
        {
            SelectionMode = SelectionMode.Single,
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            ItemTemplate = new FuncDataTemplate<CompareRow>((_, _) => MakeRow(vm), supportsRecycling: true),
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Rows)),
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedRow)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(listBox, Se.Language.File.Compare);
        ScrollViewer.SetHorizontalScrollBarVisibility(listBox, ScrollBarVisibility.Disabled);

        listBox.Styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(ListBoxItem.PaddingProperty, new Thickness(0)),
                new Setter(ListBoxItem.MinHeightProperty, 0d),
                new Setter(ListBoxItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch),
            },
        });

        // The pencil only shows on the selected row, so a long list does not turn into a column of buttons.
        listBox.Styles.Add(new Style(x => x.OfType<Button>().Class(EditButtonClass))
        {
            Setters =
            {
                new Setter(OpacityProperty, 0d),
                new Setter(IsHitTestVisibleProperty, false),
            },
        });
        listBox.Styles.Add(new Style(x => x.OfType<ListBoxItem>().Class(":selected").Descendant().OfType<Button>().Class(EditButtonClass))
        {
            Setters =
            {
                new Setter(OpacityProperty, 1d),
                new Setter(IsHitTestVisibleProperty, true),
            },
        });

        listBox.DoubleTapped += (_, e) =>
        {
            if (e.Source is Visual source && source.FindAncestorOfType<TextBox>(includeSelf: true) != null)
            {
                return;
            }

            if (vm.SelectedRow is { IsEditing: false } row)
            {
                vm.BeginEditCommand.Execute(row);
            }
        };

        vm.RowsView = listBox;

        var listBorder = new Border
        {
            BorderBrush = UiUtil.GetTextColor(0.15),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
            Child = listBox,
        };

        var area = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            ColumnSpacing = 6,
        };
        area.Add(MakeDropHost(listBorder, null), 0);
        area.Add(_ruler, 0, 1);
        return area;
    }

    /// <summary>One row: the current card, the gutter with the action between them, the reference card.</summary>
    private static Control MakeRow(CompareViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GutterWidth, GridUnitType.Pixel),
                new ColumnDefinition(GridLength.Star),
            },
        };

        grid.Add(MakeLeftCard(vm), 0);
        grid.Add(MakeGutter(vm), 0, 1);
        grid.Add(MakeRightCard(), 0, 2);

        return new Border
        {
            BorderBrush = UiUtil.GetTextColor(0.08),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = grid,
        };
    }

    private static Control MakeLeftCard(CompareViewModel vm)
    {
        var editButton = new Button
        {
            Padding = new Thickness(6, 2),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Center,
            Command = vm.BeginEditCommand,
            [!Button.CommandParameterProperty] = new Binding("."),
            [!IsVisibleProperty] = new Binding(nameof(CompareRow.CanEdit)),
        };
        editButton.Classes.Add(EditButtonClass);
        Attached.SetIcon(editButton, IconNames.Pencil);
        AutomationProperties.SetName(editButton, Se.Language.File.CompareEditLine);
        AddHint(editButton, Se.Language.File.CompareEditLine + " (F2)");

        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        header.Add(MakeMeta(nameof(CompareRow.Left), nameof(CompareRow.LeftDurationDisplay), showEdited: true), 0);
        header.Add(editButton, 0, 1);

        var text = MakeTextHost(nameof(CompareRow.Left));
        text.Bind(IsVisibleProperty, new Binding(nameof(CompareRow.IsEditing)) { Converter = new FuncValueConverter<bool, bool>(v => !v) });

        var content = new StackPanel
        {
            Spacing = 2,
            Children = { header, text, MakeEditor(vm) },
        };

        return new Border
        {
            Padding = new Thickness(10, 5, 6, 7),
            BorderThickness = new Thickness(3, 0, 0, 0),
            [!Border.BackgroundProperty] = new Binding(nameof(CompareRow.LeftCardBrush)),
            [!Border.BorderBrushProperty] = new Binding(nameof(CompareRow.EditedBrush)),
            Child = new Panel
            {
                Children =
                {
                    MakeMissingMarker(nameof(CompareRow.IsMissingLeft)),
                    new Panel { Children = { content }, [!IsVisibleProperty] = new Binding(nameof(CompareRow.HasLeft)) },
                },
            },
        };
    }

    private static Control MakeRightCard()
    {
        var content = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                MakeMeta(nameof(CompareRow.Right), nameof(CompareRow.RightDurationDisplay), showEdited: false),
                MakeTextHost(nameof(CompareRow.Right)),
            },
            [!IsVisibleProperty] = new Binding(nameof(CompareRow.HasRight)),
        };

        return new Border
        {
            Padding = new Thickness(10, 5, 10, 7),
            [!Border.BackgroundProperty] = new Binding(nameof(CompareRow.RightCardBrush)),
            Child = new Panel { Children = { MakeMissingMarker(nameof(CompareRow.IsMissingRight)), content } },
        };
    }

    /// <summary>Where one side has no line, a thin line in the "only in one file" color marks the gap.</summary>
    private static Control MakeMissingMarker(string isVisiblePath)
    {
        return new Border
        {
            Height = 2,
            CornerRadius = new CornerRadius(1),
            Margin = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Background = CompareColors.OnlyInOneFileRow,
            [!IsVisibleProperty] = new Binding(isVisiblePath),
        };
    }

    /// <summary>"12   00:00:01,250 → 00:00:03,480   2.23s", with the cells that differ marked.</summary>
    private static Control MakeMeta(string side, string durationPath, bool showEdited)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                MakeMetaCell($"{side}.{nameof(CompareItem.NumberDisplay)}", $"{side}.{nameof(CompareItem.NumberBackgroundBrush)}", FontWeight.SemiBold, 22),
                MakeMetaCell($"{side}.{nameof(CompareItem.StartTimeDisplay)}", $"{side}.{nameof(CompareItem.StartTimeBackgroundBrush)}"),
                new TextBlock { Text = "→", Opacity = 0.45, FontSize = UiUtil.ScaledFontSize(11), VerticalAlignment = VerticalAlignment.Center },
                MakeMetaCell($"{side}.{nameof(CompareItem.EndTimeDisplay)}", $"{side}.{nameof(CompareItem.EndTimeBackgroundBrush)}"),
                new TextBlock
                {
                    Opacity = 0.45,
                    FontSize = UiUtil.ScaledFontSize(11),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4, 0, 0, 0),
                    [!TextBlock.TextProperty] = new Binding(durationPath),
                },
            },
        };

        if (showEdited)
        {
            panel.Children.Add(new TextBlock
            {
                Text = "● " + Se.Language.File.CompareEdited,
                Foreground = CompareColors.Edited,
                FontSize = UiUtil.ScaledFontSize(11),
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 0, 0),
                [!IsVisibleProperty] = new Binding(nameof(CompareRow.IsEdited)),
            });
        }

        return panel;
    }

    private static Border MakeMetaCell(string textPath, string backgroundPath, FontWeight? weight = null, double minWidth = 0)
    {
        return new Border
        {
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(3, 0),
            MinWidth = minWidth,
            VerticalAlignment = VerticalAlignment.Center,
            [!Border.BackgroundProperty] = new Binding(backgroundPath),
            Child = new TextBlock
            {
                Opacity = 0.7,
                FontSize = UiUtil.ScaledFontSize(11),
                FontWeight = weight ?? FontWeight.Normal,
                [!TextBlock.TextProperty] = new Binding(textPath),
            },
        };
    }

    /// <summary>
    /// Hosts the item's diff-highlighted text panel. The panel is a control owned by the item, so
    /// a recycled row has to let go of the previous one before it can show the next.
    /// </summary>
    private static ContentControl MakeTextHost(string side)
    {
        var host = new ContentControl
        {
            FontSize = UiUtil.ScaledFontSize(14),
            Margin = new Thickness(3, 0, 0, 0),
        };

        host.DataContextChanged += (_, _) =>
        {
            host.Content = null;
            if (host.DataContext is CompareRow row)
            {
                var panel = side == nameof(CompareRow.Left) ? row.Left.TextPanel : row.Right.TextPanel;
                if (panel.Parent is ContentControl oldHost)
                {
                    oldHost.Content = null;
                }
                else if (panel.Parent is Panel oldParent)
                {
                    oldParent.Children.Remove(panel);
                }

                host.Content = panel;
            }
        };

        return host;
    }

    /// <summary>The inline editor for the current line: timing, text, and the two take-from-reference shortcuts.</summary>
    private static Control MakeEditor(CompareViewModel vm)
    {
        var start = new TimeCodeUpDown
        {
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(CompareRow.EditStart)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(start, Se.Language.General.Show);
        var end = new TimeCodeUpDown
        {
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(CompareRow.EditEnd)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(end, Se.Language.General.Hide);

        var times = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                MakeCaption(Se.Language.General.Show), start,
                MakeCaption(Se.Language.General.Hide), end,
            },
        };

        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 56,
            FontSize = UiUtil.ScaledFontSize(14),
            [!TextBox.TextProperty] = new Binding(nameof(CompareRow.EditText)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(textBox, Se.Language.General.Text);

        var buttonSave = UiUtil.MakeButton(Se.Language.General.Ok, vm.CommitEditCommand).WithIconLeft(IconNames.Check);
        buttonSave.Bind(Button.CommandParameterProperty, new Binding("."));
        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelEditCommand);
        buttonCancel.Bind(Button.CommandParameterProperty, new Binding("."));
        var buttonTakeText = UiUtil.MakeButton(Se.Language.File.CompareTakeText, vm.TakeReferenceTextCommand).WithIconLeft(IconNames.ArrowLeft);
        buttonTakeText.Bind(Button.CommandParameterProperty, new Binding("."));
        buttonTakeText.Bind(IsVisibleProperty, new Binding(nameof(CompareRow.CanTakeFromPair)));
        var buttonTakeTiming = UiUtil.MakeButton(Se.Language.File.CompareTakeTiming, vm.TakeReferenceTimingCommand).WithIconLeft(IconNames.ArrowLeft);
        buttonTakeTiming.Bind(Button.CommandParameterProperty, new Binding("."));
        buttonTakeTiming.Bind(IsVisibleProperty, new Binding(nameof(CompareRow.CanTakeFromPair)));

        var hint = MakeCaption(Se.Language.File.CompareEditHint);
        hint.Margin = new Thickness(6, 0, 0, 0);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { buttonSave, buttonCancel, buttonTakeText, buttonTakeTiming, hint },
        };

        var editor = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(0, 4, 0, 2),
            Children = { times, textBox, buttons },
            [!IsVisibleProperty] = new Binding(nameof(CompareRow.IsEditing)),
        };

        // Straight into the text when the editor opens, caret at the end.
        editor.PropertyChanged += (_, e) =>
        {
            if (e.Property == IsVisibleProperty && editor.IsVisible)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    textBox.Focus();
                    textBox.CaretIndex = textBox.Text?.Length ?? 0;
                });
            }
        };

        return editor;
    }

    /// <summary>The band between the cards, carrying the one action that fits the pair.</summary>
    private static Control MakeGutter(CompareViewModel vm)
    {
        var take = MakeGutterButton(vm.TakeReferenceCommand, IconNames.ArrowLeft, nameof(CompareRow.CanTakeReference));
        AutomationProperties.SetName(take, Se.Language.File.CompareTakeFromReference);
        if (Se.Settings.Appearance.ShowHints)
        {
            take.Bind(ToolTip.TipProperty, new Binding(nameof(CompareRow.TakeReferenceHint)));
        }

        var delete = MakeGutterButton(vm.DeleteCurrentLineCommand, IconNames.Trash, nameof(CompareRow.CanDeleteCurrent));
        AutomationProperties.SetName(delete, Se.Language.File.CompareDeleteFromCurrent);
        AddHint(delete, Se.Language.File.CompareDeleteFromCurrent);

        return new Border
        {
            [!Border.BackgroundProperty] = new Binding(nameof(CompareRow.GutterBrush)),
            Child = new Panel { Children = { take, delete } },
        };
    }

    private static Button MakeGutterButton(System.Windows.Input.ICommand command, string iconName, string isVisiblePath)
    {
        var button = new Button
        {
            Width = 28,
            Height = 28,
            Padding = new Thickness(0),
            CornerRadius = new CornerRadius(14),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Command = command,
            [!Button.CommandParameterProperty] = new Binding("."),
            [!IsVisibleProperty] = new Binding(isVisiblePath),
        };
        Attached.SetIcon(button, iconName);
        return button;
    }

    private Control MakeStatusBar(CompareViewModel vm)
    {
        var statusText = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.StatusText));
        statusText.VerticalAlignment = VerticalAlignment.Center;

        var legend = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                MakeLegendSwatch(CompareColors.OnlyInOneFile, Se.Language.File.CompareOnlyInOneFile),
                MakeLegendSwatch(CompareColors.TextOrTimeDifference, Se.Language.File.CompareTextOrTimeDifference),
                MakeLegendSwatch(CompareColors.NumberDifference, Se.Language.File.CompareNumberDifference),
                MakeLegendSwatch(((ISolidColorBrush)CompareColors.Edited).Color, Se.Language.File.CompareEdited),
            },
        };

        return MakeTwoColumnBar(statusText, legend);
    }

    private Control MakeBottomBar(CompareViewModel vm)
    {
        // Pending changes: how many, the latest one, and a way back.
        var badge = new Border
        {
            Background = CompareColors.Edited,
            CornerRadius = new CornerRadius(10),
            MinWidth = 20,
            Padding = new Thickness(6, 1),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold,
                FontSize = UiUtil.ScaledFontSize(11),
                HorizontalAlignment = HorizontalAlignment.Center,
                [!TextBlock.TextProperty] = new Binding(nameof(vm.PendingChangeCount)),
            },
        };
        var pendingText = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PendingChangesText)),
        };
        var lastChange = new TextBlock
        {
            Opacity = 0.65,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 360,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.LastChangeText)),
        };
        var buttonUndo = UiUtil.MakeButton(Se.Language.General.Undo, vm.UndoCommand).WithIconLeft(IconNames.Undo);
        AddHint(buttonUndo, Se.Language.General.Undo + " (Ctrl+Z)");

        var tray = new Border
        {
            Background = UiUtil.GetTextColor(0.05),
            BorderBrush = UiUtil.GetTextColor(0.1),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10, 4, 4, 4),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children = { badge, pendingText, lastChange, buttonUndo },
            },
            [!IsVisibleProperty] = new Binding(nameof(vm.HasPendingChanges)),
        };

        // Bound like before: with only one side loaded the lists are never padded to equal length,
        // and Export indexes the right-hand list by the left-hand count.
        var buttonExport = UiUtil.MakeButton(Se.Language.General.Export, vm.ExportCommand)
            .WithIconLeft(IconNames.Export)
            .WithBindIsVisible(nameof(vm.IsExportVisible));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindContent(nameof(vm.OkButtonText));
        var panelButtons = UiUtil.MakeButtonBar(buttonExport, buttonCancel, buttonOk);

        return MakeTwoColumnBar(tray, panelButtons);
    }

    private void HookScrollViewer()
    {
        _scrollViewer = _vm.RowsView?.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (_scrollViewer == null)
        {
            return;
        }

        _scrollViewer.ScrollChanged += (_, _) => UpdateRulerViewport();
        _scrollViewer.PropertyChanged += (_, e) =>
        {
            if (e.Property == ScrollViewer.ExtentProperty || e.Property == ScrollViewer.ViewportProperty)
            {
                UpdateRulerViewport();
            }
        };
        _ruler.SetRows(_vm.Rows);
        UpdateRulerViewport();
    }

    private void UpdateRulerViewport()
    {
        if (_scrollViewer == null || _scrollViewer.Extent.Height <= 0)
        {
            return;
        }

        var extent = _scrollViewer.Extent.Height;
        _ruler.SetViewport(_scrollViewer.Offset.Y / extent, _scrollViewer.Viewport.Height / extent);
    }

    private void ScrollToFraction(double fraction)
    {
        if (_scrollViewer == null)
        {
            return;
        }

        var extent = _scrollViewer.Extent.Height;
        var viewport = _scrollViewer.Viewport.Height;
        var y = Math.Clamp(fraction * extent - viewport / 2, 0, Math.Max(0, extent - viewport));
        _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, y);
    }

    /// <summary>Takes a dropped subtitle file: a header takes it for its own side, the list for the side it was dropped on.</summary>
    private Border MakeDropHost(Control child, EventHandler<DragEventArgs>? onDrop)
    {
        var host = new Border
        {
            Background = Brushes.Transparent,
            Child = child,
        };
        DragDrop.SetAllowDrop(host, true);
        host.AddHandler(DragDrop.DragOverEvent, _vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        host.AddHandler(DragDrop.DropEvent, (object? sender, DragEventArgs e) =>
        {
            if (onDrop != null)
            {
                onDrop(sender, e);
            }
            else if (e.GetPosition(host).X < host.Bounds.Width / 2)
            {
                _vm.FileGridOnDropLeft(sender, e);
            }
            else
            {
                _vm.FileGridOnDropRight(sender, e);
            }
        }, RoutingStrategies.Bubble);
        return host;
    }

    /// <summary>Left group and right group in one row, so a wide left group cannot overlap the buttons.</summary>
    private static Grid MakeTwoColumnBar(Control left, Control right)
    {
        var bar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
        };

        bar.Add(left, 0);
        bar.Add(right, 0, 1);

        return bar;
    }

    private static ContentControl MakeIcon(string iconName, double opacity = 0.75)
    {
        var icon = new ContentControl
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = opacity,
        };

        Attached.SetIcon(icon, iconName);

        return icon;
    }

    private static TextBlock MakeCaption(string text)
    {
        return new TextBlock
        {
            Text = text,
            Opacity = 0.6,
            FontSize = UiUtil.ScaledFontSize(12),
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    private static void AddHint(Control control, string hint)
    {
        if (Se.Settings.Appearance.ShowHints)
        {
            UiUtil.AttachHoverTooltip(control, hint);
        }
    }

    private static Control MakeLegendSwatch(Color color, string label)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(14, 0, 0, 0),
            Children =
            {
                new Border
                {
                    Width = 12,
                    Height = 12,
                    CornerRadius = new CornerRadius(3),
                    Background = new SolidColorBrush(color),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(0x60, 0x60, 0x60, 0x60)),
                    BorderThickness = new Thickness(1),
                    VerticalAlignment = VerticalAlignment.Center,
                },
                new TextBlock
                {
                    Text = label,
                    Opacity = 0.8,
                    FontSize = UiUtil.ScaledFontSize(12),
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };
    }
}

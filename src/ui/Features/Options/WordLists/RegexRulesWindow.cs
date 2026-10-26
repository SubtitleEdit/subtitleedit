using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.WordLists;

/// <summary>
/// The OCR fix regular expressions of one language: the rules on the left (each with an on/off
/// check box), and on the right an editor with insert helpers and a live test of the pattern.
/// </summary>
public class RegexRulesWindow : Window
{
    private static readonly FontFamily MonoFont = new("Cascadia Mono,Consolas,Menlo,Monaco,monospace");
    private static readonly IBrush ValidBrush = EditorChrome.Themed(Color.Parse("#4ec98a"), Color.Parse("#1f8a52"));
    private static readonly IBrush InvalidBrush = EditorChrome.Themed(Color.Parse("#f07a82"), Color.Parse("#c8323d"));
    private static readonly IBrush UserBadgeBrush = EditorChrome.Themed(Color.Parse("#7fa8f0"), Color.Parse("#2f5fb8"));
    private static readonly IValueConverter EnabledToOpacity = new FuncValueConverter<bool, double>(enabled => enabled ? 1.0 : 0.5);

    private readonly RegexRulesViewModel _vm;

    private sealed record InsertHelper(string Display, string Before, string After, string Description);

    public RegexRulesWindow(RegexRulesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.Title;
        CanResize = true;
        Width = 1100;
        Height = 780;
        MinWidth = 820;
        MinHeight = 540;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Options.WordLists;
        var header = new Border
        {
            Child = EditorChrome.MakeDialogHeader(vm.Title, l.RegexRulesDescription, 760),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
        };

        var listPane = MakeRulesPane(vm, out var listBox);
        var editorPane = MakeEditorPane(vm);

        var body = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(11, GridUnitType.Star)) { MinWidth = 380 },
                new ColumnDefinition(new GridLength(9, GridUnitType.Star)) { MinWidth = 380 },
            },
        };
        body.Add(listPane, 0, 0);
        body.Add(new Border
        {
            Child = editorPane,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        sections.Add(header, 0, 0);
        sections.Add(body, 1, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var status = EditorChrome.MakeStatusText(nameof(vm.EnabledCountText));

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(status, buttonOk));

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.SelectedRule) && vm.SelectedRule != null)
            {
                listBox.ScrollIntoView(vm.SelectedRule);
            }
        };

        UiUtil.FocusOnFirstActivation(this, listBox);
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
    }

    private static Grid MakeRulesPane(RegexRulesViewModel vm, out ListBox listBox)
    {
        var l = Se.Language.Options.WordLists;

        var search = EditorChrome.MakeSearchBox(vm, nameof(vm.SearchText), Se.Language.General.Search, double.NaN);
        search.HorizontalAlignment = HorizontalAlignment.Stretch;

        var comboShow = new ComboBox
        {
            ItemsSource = vm.ShowOptions,
            MinWidth = 130,
            VerticalAlignment = VerticalAlignment.Center,
            [!SelectingItemsControl.SelectedIndexProperty] = new Binding(nameof(vm.SelectedShowIndex)) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(comboShow, Se.Language.General.Filter);

        var filterRow = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(GridLength.Auto) },
        };
        filterRow.Add(search, 0, 0);
        filterRow.Add(comboShow, 0, 1);

        listBox = new ListBox
        {
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Rules)) { Mode = BindingMode.OneWay },
            [!SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedRule)) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ItemTemplate = MakeRuleTemplate(),
        };
        AutomationProperties.SetName(listBox, l.RegexRules);
        listBox.KeyDown += (_, e) => vm.RuleListKeyDown(e);

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem { Header = l.EnableRule + " / " + l.DisableRule, Command = vm.ToggleSelectedCommand, InputGesture = new KeyGesture(Key.Space) });
        flyout.Items.Add(new MenuItem { Header = l.CopyPattern, Command = vm.CopyPatternCommand });
        flyout.Items.Add(new Separator());
        var deleteItem = new MenuItem { Header = l.DeleteRule, Command = vm.DeleteRuleCommand, InputGesture = new KeyGesture(Key.Delete) };
        deleteItem.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.CanDelete)));
        flyout.Items.Add(deleteItem);
        listBox.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(listBox);

        var buttonEnable = EditorChrome.MakeTextToolButton(IconNames.CheckCircle, l.EnableShown, vm.EnableShownCommand);
        var buttonDisable = EditorChrome.MakeTextToolButton(IconNames.CardRemoveOutline, l.DisableShown, vm.DisableShownCommand);
        var bulkRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Children = { buttonEnable, buttonDisable },
        };

        var countBadge = EditorChrome.MakeCountBadge(
            new Binding(nameof(vm.Rules) + ".Count") { Mode = BindingMode.OneWay },
            EditorChrome.AccentBrush, EditorChrome.AccentSoftBrush);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 14),
        };
        grid.Add(EditorChrome.MakeSectionHeaderRow(l.RegexRules, countBadge), 0, 0);
        grid.Add(filterRow, 1, 0);
        grid.Add(EditorChrome.MakeCompactListFrame(listBox), 2, 0);
        grid.Add(bulkRow, 3, 0);
        return grid;
    }

    private static FuncDataTemplate<RegexRuleItem> MakeRuleTemplate()
    {
        return new FuncDataTemplate<RegexRuleItem>((item, _) =>
        {
            var checkBox = new CheckBox
            {
                MinWidth = 0,
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                [!ToggleButton.IsCheckedProperty] = new Binding(nameof(RegexRuleItem.IsEnabled)) { Mode = BindingMode.TwoWay },
                [!AutomationProperties.NameProperty] = new Binding(nameof(RegexRuleItem.Find)),
            };

            var find = MakeMonoText(nameof(RegexRuleItem.Find));
            var arrow = new Optris.Icons.Avalonia.Icon
            {
                Value = IconNames.ArrowRightThick,
                FontSize = 11,
                Foreground = EditorChrome.DimTextBrush,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var replace = MakeMonoText(nameof(RegexRuleItem.ReplaceWith));
            replace.Foreground = EditorChrome.DimTextBrush;

            var badge = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(6, 1),
                Background = EditorChrome.AccentSoftBrush,
                VerticalAlignment = VerticalAlignment.Center,
                IsVisible = item?.IsUserRule == true,
                Child = new TextBlock
                {
                    Text = Se.Language.Options.WordLists.MyRule,
                    FontSize = UiUtil.ScaledFontSize(10.5),
                    FontWeight = FontWeight.SemiBold,
                    Foreground = UserBadgeBrush,
                },
            };

            var row = new Grid
            {
                ColumnSpacing = 8,
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(new GridLength(3, GridUnitType.Star)),
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(new GridLength(2, GridUnitType.Star)),
                    new ColumnDefinition(GridLength.Auto),
                },
                [!Visual.OpacityProperty] = new Binding(nameof(RegexRuleItem.IsEnabled)) { Converter = EnabledToOpacity },
            };
            row.Add(checkBox, 0, 0);
            row.Add(find, 0, 1);
            row.Add(arrow, 0, 2);
            row.Add(replace, 0, 3);
            row.Add(badge, 0, 4);
            ToolTip.SetTip(row, item?.ToString());
            return row;
        }, supportsRecycling: false);
    }

    private static TextBlock MakeMonoText(string property)
    {
        return new TextBlock
        {
            FontFamily = MonoFont,
            FontSize = UiUtil.ScaledFontSize(12),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(property),
        };
    }

    private Grid MakeEditorPane(RegexRulesViewModel vm)
    {
        var l = Se.Language.Options.WordLists;
        var c = Se.Language.Edit.RegularExpressionContextMenu;

        var textBoxFind = MakeMonoTextBox(nameof(vm.EditFind), l.Pattern, 56);
        RegexContextFlyout.Attach(textBoxFind, vm, () => true);
        var textBoxReplace = MakeMonoTextBox(nameof(vm.EditReplace), Se.Language.General.ReplaceWith, 34);

        var findHelpers = MakeHelperChips(textBoxFind, new[]
        {
            new InsertHelper("^", "^", string.Empty, l.StartOfLine),
            new InsertHelper("$", "$", string.Empty, l.EndOfLine),
            new InsertHelper("\\b", "\\b", string.Empty, c.WordBoundary),
            new InsertHelper("\\s", "\\s", string.Empty, c.AnyWhitespace),
            new InsertHelper("\\S", "\\S", string.Empty, c.NonSpaceCharacter),
            new InsertHelper("\\w", "\\w", string.Empty, l.WordCharacter),
            new InsertHelper("\\d", "\\d", string.Empty, c.AnyDigit),
            new InsertHelper(".", ".", string.Empty, c.AnyCharacter),
            new InsertHelper("*", "*", string.Empty, c.ZeroOrMore),
            new InsertHelper("+", "+", string.Empty, c.OneOrMore),
            new InsertHelper("?", "?", string.Empty, l.Optional),
            new InsertHelper("|", "|", string.Empty, l.Either),
            new InsertHelper("( )", "(", ")", l.CaptureGroup),
            new InsertHelper("[ ]", "[", "]", c.InCharacterGroup),
            new InsertHelper("[^ ]", "[^", "]", c.NotInCharacterGroup),
            new InsertHelper("\\x22", "\\x22", string.Empty, l.StraightQuote),
        });

        var replaceHelpers = MakeHelperChips(textBoxReplace, new[]
        {
            new InsertHelper("$1", "$1", string.Empty, l.CaptureGroupReference),
            new InsertHelper("$2", "$2", string.Empty, l.CaptureGroupReference.Replace("1", "2")),
            new InsertHelper("$&", "$&", string.Empty, l.WholeMatch),
        });

        var statusIcon = new Optris.Icons.Avalonia.Icon { FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
        var statusText = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(11.5),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PatternStatus)),
        };
        var statusRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { statusIcon, statusText },
        };

        void UpdateStatusLook()
        {
            var brush = vm.IsPatternValid ? ValidBrush : InvalidBrush;
            statusIcon.Value = vm.IsPatternValid ? IconNames.CheckCircle : IconNames.Alert;
            statusIcon.Foreground = brush;
            statusText.Foreground = brush;
            statusRow.IsVisible = !string.IsNullOrEmpty(vm.PatternStatus);
        }

        var hint = EditorChrome.MakeHint(null);
        hint.Bind(TextBlock.TextProperty, new Binding(nameof(vm.EditorHint)));
        hint.Bind(IsVisibleProperty, new Binding(nameof(vm.EditorHint)) { Converter = StringConverters.IsNotNullOrEmpty });

        var buttonNew = EditorChrome.MakeTextToolButton(IconNames.New, l.NewRule, vm.NewRuleCommand);
        var buttonAdd = EditorChrome.MakeActionButton(IconNames.Plus, l.AddAsNewRule, vm.AddRuleCommand).EnabledBy(vm, nameof(vm.CanAdd));
        buttonAdd.Classes.Add("primary");
        var buttonUpdate = EditorChrome.MakeActionButton(IconNames.ContentSave, l.UpdateRule, vm.UpdateRuleCommand).EnabledBy(vm, nameof(vm.CanUpdate));
        var buttonDelete = EditorChrome.MakeActionButton(IconNames.Trash, l.DeleteRule, vm.DeleteRuleCommand).EnabledBy(vm, nameof(vm.CanDelete));
        var buttonRow = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            ItemSpacing = 6,
            LineSpacing = 6,
            Children = { buttonNew, buttonAdd, buttonUpdate, buttonDelete },
        };

        var rulePanel = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeDimLabel(l.Pattern, new Thickness(0)),
                textBoxFind,
                statusRow,
                findHelpers,
                EditorChrome.MakeDimLabel(Se.Language.General.ReplaceWith, new Thickness(0, 4, 0, 0)),
                textBoxReplace,
                replaceHelpers,
                hint,
                buttonRow,
            },
        };
        var ruleSection = EditorChrome.MakeEditorSection(l.Rule, rulePanel, first: true);

        var textBoxTest = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = MonoFont,
            MinHeight = 64,
            MaxHeight = 120,
            PlaceholderText = l.TestTextPlaceholder,
            [!TextBox.TextProperty] = new Binding(nameof(vm.TestText)) { Mode = BindingMode.TwoWay },
        }.AsTextArea();
        AutomationProperties.SetName(textBoxTest, l.TestText);

        var switchAll = EditorChrome.MakeSwitchRow(nameof(vm.ApplyAllEnabled), l.ApplyAllEnabledRules);

        var matchesView = MakePreviewBlock();
        var resultView = MakePreviewBlock();

        void RenderSegments(SelectableTextBlock target, IReadOnlyList<RegexTestSegment> segments)
        {
            target.Inlines ??= new InlineCollection();
            target.Inlines.Clear();
            foreach (var segment in segments)
            {
                var run = new Run(segment.Text);
                if (segment.IsMatch)
                {
                    run.Background = EditorChrome.AccentSoftBrush;
                    run.FontWeight = FontWeight.SemiBold;
                }

                target.Inlines.Add(run);
            }

            AutomationProperties.SetName(target, RegexRulesViewModel.Describe(segments));
        }

        var matchInfo = EditorChrome.MakeCountBadge(new Binding(nameof(vm.MatchInfo)), EditorChrome.AccentBrush, EditorChrome.AccentSoftBrush);
        matchInfo.Bind(IsVisibleProperty, new Binding(nameof(vm.MatchInfo)) { Converter = StringConverters.IsNotNullOrEmpty });

        var matchesLabel = EditorChrome.MakeDimLabel(l.Matches, new Thickness(0, 2, 0, 0));
        var matchesFrame = MakePreviewFrame(matchesView);
        var notApplyAll = new Binding(nameof(vm.ApplyAllEnabled)) { Converter = BoolConverters.Not };
        matchesLabel.Bind(IsVisibleProperty, notApplyAll);
        matchesFrame.Bind(IsVisibleProperty, notApplyAll);

        var testPanel = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                textBoxTest,
                switchAll,
                matchesLabel,
                matchesFrame,
                EditorChrome.MakeDimLabel(l.Result, new Thickness(0, 2, 0, 0)),
                MakePreviewFrame(resultView),
            },
        };
        var testSection = EditorChrome.MakeEditorSection(l.TestText, testPanel, matchInfo);

        void Refresh()
        {
            UpdateStatusLook();
            RenderSegments(matchesView, vm.InputSegments);
            RenderSegments(resultView, vm.OutputSegments);
        }

        vm.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(vm.IsPatternValid):
                case nameof(vm.PatternStatus):
                    UpdateStatusLook();
                    break;
                case nameof(vm.InputSegments):
                    RenderSegments(matchesView, vm.InputSegments);
                    break;
                case nameof(vm.OutputSegments):
                    RenderSegments(resultView, vm.OutputSegments);
                    break;
            }
        };
        Refresh();

        var scroll = new ScrollViewer
        {
            Content = new StackPanel { Children = { ruleSection, testSection } },
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
        };

        var grid = new Grid();
        grid.Children.Add(scroll);
        return grid;
    }

    private static TextBox MakeMonoTextBox(string property, string accessibleName, double minHeight)
    {
        var textBox = new TextBox
        {
            AcceptsReturn = false,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = MonoFont,
            MinHeight = minHeight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            [!TextBox.TextProperty] = new Binding(property) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        }.AsTextField(accessibleName);
        return textBox;
    }

    private static WrapPanel MakeHelperChips(TextBox target, IEnumerable<InsertHelper> helpers)
    {
        var panel = new WrapPanel { Orientation = Orientation.Horizontal, ItemSpacing = 4, LineSpacing = 4 };
        foreach (var helper in helpers)
        {
            var button = new Button
            {
                Content = new TextBlock
                {
                    Text = helper.Display,
                    FontFamily = MonoFont,
                    FontSize = UiUtil.ScaledFontSize(11.5),
                },
                Padding = new Thickness(7, 2),
                MinWidth = 30,
                Height = 24,
                Background = EditorChrome.FieldBrush,
                BorderBrush = EditorChrome.FieldBorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Focusable = false,
            };
            ToolTip.SetTip(button, Se.Language.Options.WordLists.Insert + ": " + helper.Description);
            AutomationProperties.SetName(button, helper.Description);
            button.Click += (_, _) => Insert(target, helper);
            panel.Children.Add(button);
        }

        return panel;
    }

    // Inserts at the caret, or wraps the selection when the helper has an "after" part ("( )").
    private static void Insert(TextBox textBox, InsertHelper helper)
    {
        var text = textBox.Text ?? string.Empty;
        var start = Math.Clamp(Math.Min(textBox.SelectionStart, textBox.SelectionEnd), 0, text.Length);
        var end = Math.Clamp(Math.Max(textBox.SelectionStart, textBox.SelectionEnd), 0, text.Length);
        var selected = text.Substring(start, end - start);
        var inserted = helper.Before + selected + helper.After;
        textBox.Text = text.Substring(0, start) + inserted + text.Substring(end);
        var caret = selected.Length == 0 ? start + helper.Before.Length : start + inserted.Length;
        textBox.Focus();
        textBox.SelectionStart = caret;
        textBox.SelectionEnd = caret;
        textBox.CaretIndex = caret;
    }

    private static SelectableTextBlock MakePreviewBlock()
    {
        return new SelectableTextBlock
        {
            FontFamily = MonoFont,
            FontSize = UiUtil.ScaledFontSize(12.5),
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 20,
        };
    }

    private static Border MakePreviewFrame(Control content)
    {
        return new Border
        {
            Child = content,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 6),
            MinHeight = 34,
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public partial class MultipleReplaceViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MultipleReplaceFix> _fixes;
    [ObservableProperty] private ObservableCollection<MultipleReplaceTypeItem> _replaceTypes;
    [ObservableProperty] private MultipleReplaceFix? _selectedFix;
    [ObservableProperty] private RuleTreeNode? _selectedNode;
    [ObservableProperty] private bool _isEditPanelVisible;
    [ObservableProperty] private bool _isFixDetailPanelVisible;
    [ObservableProperty] private bool _isMultipleReplaceDotDotDotButtonsVisible;
    [ObservableProperty] private string _fixesInfo;
    [ObservableProperty] private ObservableCollection<ReplaceExpression> _selectedFixHits;
    public ObservableCollection<MultipleReplaceTypeItem> RuleTypes { get; }

    // The rule whose row shows text boxes in the rules list, and its texts from before the edit
    // so Escape can put them back.
    private RuleTreeNode? _inlineEditNode;
    private (string Find, string ReplaceWith, string Description) _inlineEditOriginal;

    public ObservableCollection<RuleTreeNode> Nodes { get; }

    // The category list on the left and the rules of the selected category on the right - a flat,
    // scrollable list like Subtitle Edit 4 had, instead of a tree that opened a category of
    // hundreds of rules in full every time a rule was added to it (#16215).
    [ObservableProperty] private RuleTreeNode? _selectedCategory;
    [ObservableProperty] private string _selectedCategoryTitle;
    [ObservableProperty] private string _rulesFilter;
    [ObservableProperty] private bool _newestFirst;
    [ObservableProperty] private bool _canReorderRules;
    public ObservableCollection<RuleTreeNode> VisibleRules { get; }

    // The type-in row above the rules: Enter adds the rule to the end of the category and leaves
    // the cursor in "Find what" for the next one (#16215).
    [ObservableProperty] private string _newFind;
    [ObservableProperty] private string _newReplaceWith;
    [ObservableProperty] private MultipleReplaceTypeItem? _newRuleType;
    [ObservableProperty] private bool _newWholeWord;
    [ObservableProperty] private bool _canUseNewWholeWord;

    public ListBox CategoriesListBox { get; internal set; }
    public TableView RulesTable { get; internal set; }
    public TextBox? NewFindTextBox { get; internal set; }
    private bool _refreshingRules;
    private ObservableCollection<RuleTreeNode>? _watchedRules;
    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public Subtitle FixedSubtitle { get; private set; }

    // Invoked by the re-usable "Apply" button: applies the checked replacements to the document
    // (subtitle, number applied) without closing the window, so several rounds can be run (#12029).
    public Action<Subtitle, int>? OnApply { get; set; }
    public int TotalReplaced { get; private set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private Subtitle _subtitle;
    private readonly ConcurrentDictionary<string, Regex> _compiledRegExList;

    // Pattern -> why it cannot run, or null when it is fine. Every regular expression rule is
    // checked so a broken one is marked even in an unticked category, so this must not be the
    // compiled cache: see GetRegexError.
    private readonly ConcurrentDictionary<string, string?> _regExErrors;
    private readonly ConcurrentDictionary<(string FindWhat, bool IgnoreCase), Regex> _wholeWordRegexes = new();
    private readonly Timer _timerReplace;

    /// <summary>
    /// The preview debounce (250 ms). Internal so the headless tests, which can only observe the
    /// preview by waiting for this timer, can shorten it instead of sleeping through it.
    /// </summary>
    internal double PreviewIntervalMs
    {
        get => _timerReplace.Interval;
        set => _timerReplace.Interval = value;
    }
    private readonly object _previewLock = new();
    private volatile bool _dirty;
    private volatile bool _closed;

    // Subtitle Edit 4 used Ctrl+Up/Down/Home/End for the four move commands on both the rules
    // and the groups list (#13523). Ctrl+Up/Down is Mission Control on macOS, so show Cmd there.
    // The gestures are what the context menus advertise; OnKeyDown accepts Ctrl and Cmd alike.
    private static readonly KeyModifiers CommandModifier = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
    private static readonly KeyGesture MoveUpGesture = new(Key.Up, CommandModifier);
    private static readonly KeyGesture MoveDownGesture = new(Key.Down, CommandModifier);
    private static readonly KeyGesture MoveToTopGesture = new(Key.Home, CommandModifier);
    private static readonly KeyGesture MoveToBottomGesture = new(Key.End, CommandModifier);
    private static readonly KeyGesture AddRulesGesture = new(Key.N, CommandModifier | KeyModifiers.Shift);
    private static readonly KeyGesture DuplicateGesture = new(Key.D, CommandModifier);
    private static readonly KeyGesture EditGesture = new(Key.F2);
    private static readonly KeyGesture DeleteGesture = new(Key.Delete);

    public MultipleReplaceViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;

        Fixes = new ObservableCollection<MultipleReplaceFix>();
        SelectedFixHits = new ObservableCollection<ReplaceExpression>();
        FixesInfo = string.Empty;
        Nodes = new ObservableCollection<RuleTreeNode>(GetNodes());
        CategoriesListBox = new ListBox();
        RulesTable = new TableView();
        VisibleRules = new ObservableCollection<RuleTreeNode>();
        SelectedCategoryTitle = string.Empty;
        _rulesFilter = string.Empty;
        _newFind = string.Empty;
        _newReplaceWith = string.Empty;
        _newestFirst = Se.Settings.Edit.MultipleReplace.NewestFirst;
        IsMultipleReplaceDotDotDotButtonsVisible = Se.Settings.Tools.MultipleReplaceShowDotDotDotButtons;

        _compiledRegExList = new ConcurrentDictionary<string, Regex>();
        _regExErrors = new ConcurrentDictionary<string, string?>();

        _timerReplace = new Timer();
        _timerReplace.Interval = 250;
        _timerReplace.Elapsed += TimerReplaceElapsed;
        _timerReplace.Start();

        _subtitle = new Subtitle();
        FixedSubtitle = new Subtitle();

        ReplaceTypes =
        [
            new MultipleReplaceTypeItem(Se.Language.General.CaseInsensitive, MultipleReplaceType.CaseInsensitive),
            new MultipleReplaceTypeItem(Se.Language.General.CaseSensitive, MultipleReplaceType.CaseSensitive),
            new MultipleReplaceTypeItem(Se.Language.General.RegularExpression, MultipleReplaceType.RegularExpression)
        ];
        RuleTypes = ReplaceTypes; // bridge for ItemsSource expected by view
        _fileHelper = fileHelper;

        var lastCategory = Se.Settings.Edit.MultipleReplace.LastCategory;
        SelectedCategory = Nodes.FirstOrDefault(p => p.CategoryName == lastCategory) ?? Nodes.FirstOrDefault();
    }

    partial void OnSelectedCategoryChanged(RuleTreeNode? value)
    {
        if (_watchedRules != null)
        {
            _watchedRules.CollectionChanged -= WatchedRulesChanged;
        }

        _watchedRules = value?.SubNodes;
        if (_watchedRules != null)
        {
            _watchedRules.CollectionChanged += WatchedRulesChanged;
        }

        if (SelectedNode != null && SelectedNode.Parent != value)
        {
            SelectedNode = null;
        }

        // A list of words usually shares type and whole word with the rules already there.
        var last = value?.SubNodes?.LastOrDefault();
        NewRuleType = ReplaceTypes.FirstOrDefault(t => t.Type == (last?.Type ?? MultipleReplaceType.CaseInsensitive));
        NewWholeWord = last?.WholeWord ?? false;

        RefreshVisibleRules();
    }

    partial void OnNewRuleTypeChanged(MultipleReplaceTypeItem? value)
    {
        CanUseNewWholeWord = value?.Type != MultipleReplaceType.RegularExpression;
    }

    partial void OnRulesFilterChanged(string value) => RefreshVisibleRules();

    partial void OnNewestFirstChanged(bool value) => RefreshVisibleRules();

    private void WatchedRulesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => RefreshVisibleRules();

    /// <summary>
    /// Rebuilds the rules list from the selected category: numbered in run order, filtered on find,
    /// replace and description, and last rule first with "Newest first". The selected rule stays
    /// selected while it is still in the list.
    /// </summary>
    internal void RefreshVisibleRules()
    {
        var keep = SelectedNode;
        var category = SelectedCategory;
        var rules = category?.SubNodes ?? new ObservableCollection<RuleTreeNode>();
        var filter = RulesFilter?.Trim() ?? string.Empty;

        _refreshingRules = true;
        try
        {
            VisibleRules.Clear();
            for (var i = 0; i < rules.Count; i++)
            {
                rules[i].Number = i + 1;
            }

            IEnumerable<RuleTreeNode> visible = rules;
            if (filter.Length > 0)
            {
                visible = visible.Where(p =>
                    p.Find.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    p.ReplaceWith.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(filter, StringComparison.OrdinalIgnoreCase));
            }

            if (NewestFirst)
            {
                visible = visible.Reverse();
            }

            foreach (var rule in visible.ToList())
            {
                VisibleRules.Add(rule);
            }
        }
        finally
        {
            _refreshingRules = false;
        }

        UpdateSelectedCategoryTitle();

        // Reordering inside a filtered list would move rules past ones that are not shown.
        CanReorderRules = filter.Length == 0;

        if (keep != null && VisibleRules.Contains(keep))
        {
            RulesTable.SelectedItem = keep;
        }
        else
        {
            SelectedNode = null;
        }
    }

    /// <summary>
    /// The rules list's selection, pushed here by the view - ignored while the list is rebuilt.
    /// </summary>
    internal void RulesTableSelectionChanged()
    {
        if (_refreshingRules)
        {
            return;
        }

        SelectedNode = RulesTable.SelectedItem as RuleTreeNode;
    }

    private void UpdateSelectedCategoryTitle()
    {
        SelectedCategoryTitle = SelectedCategory == null
            ? Se.Language.General.Rules
            : string.Format(Se.Language.Edit.MultipleReplace.RulesInX, SelectedCategory.CategoryName);
    }

    /// <summary>
    /// The type-in row: adds the rule to the end of the selected category, shows it, and empties
    /// the fields for the next one - type and whole word stay as they are (#16215).
    /// </summary>
    [RelayCommand]
    private void AddNewRule()
    {
        var category = SelectedCategory ?? Nodes.FirstOrDefault();
        if (category?.SubNodes == null || string.IsNullOrEmpty(NewFind))
        {
            NewFindTextBox?.Focus();
            return;
        }

        var rule = new RuleTreeNode(category, new MultipleReplaceRule
        {
            Active = true,
            Find = NewFind,
            ReplaceWith = NewReplaceWith ?? string.Empty,
            Description = string.Empty,
            Type = NewRuleType?.Type ?? MultipleReplaceType.CaseInsensitive,
            // the check box is only disabled for regular expressions, so a tick from before stays
            WholeWord = NewWholeWord && NewRuleType?.Type != MultipleReplaceType.RegularExpression,
        });

        if (SelectedCategory != category)
        {
            SelectedCategory = category;
        }

        RulesFilter = string.Empty; // the new rule must be in the list
        category.SubNodes.Add(rule);
        _dirty = true;

        NewFind = string.Empty;
        NewReplaceWith = string.Empty;
        NavigateToRule(rule, focus: false);
        NewFindTextBox?.Focus();
    }

    internal void NewRuleTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        // Enter in the type-in row adds the rule - without this it reached the window and meant OK.
        // With nothing typed it still means OK, as the row has the initial focus (#14586).
        if (e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.None && !string.IsNullOrEmpty(NewFind))
        {
            e.Handled = true;
            AddNewRule();
        }
    }

    partial void OnSelectedNodeChanged(RuleTreeNode? value)
    {
        IsEditPanelVisible = value is { IsCategory: false };
        if (_inlineEditNode != null && _inlineEditNode != value)
        {
            EndInlineEdit(commit: true, focusRow: false);
        }

        if (value == null || value.IsCategory)
        {
            if (value == null && !_refreshingRules && RulesTable.SelectedItem != null)
            {
                RulesTable.SelectedItem = null;
            }

            return;
        }

        if (value.Parent != null && value.Parent != SelectedCategory)
        {
            SelectedCategory = value.Parent;
        }

        if (!VisibleRules.Contains(value) && !string.IsNullOrEmpty(RulesFilter))
        {
            RulesFilter = string.Empty;
        }

        if (RulesTable.SelectedItem != value && VisibleRules.Contains(value))
        {
            RulesTable.SelectedItem = value;
        }
    }

    partial void OnSelectedFixChanged(MultipleReplaceFix? value)
    {
        SelectedFixHits.Clear();
        IsFixDetailPanelVisible = value?.Hits.Count > 0;
        if (value != null)
        {
            foreach (var hit in value.Hits)
            {
                SelectedFixHits.Add(hit);
            }
        }
    }

    private void TimerReplaceElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!_dirty || _closed)
        {
            return;
        }

        _timerReplace.Stop();
        _dirty = false;
        try
        {
            GeneratePreview();
        }
        catch (Exception exception)
        {
            // The timer is stopped while generating, so an escaping exception used to leave it
            // stopped for good - the preview then silently froze for the rest of the session and
            // no rule or category tick ever changed it again (#13534). Retry on the next tick.
            _dirty = true;
            SeLogger.Error(exception, "Multiple replace: unable to generate preview");
        }
        finally
        {
            if (!_closed)
            {
                _timerReplace.Start();
            }
        }
    }

    public void Initialize(Subtitle subtitle)
    {
        _subtitle = subtitle;
        _dirty = true;
    }

    private static List<RuleTreeNode> GetNodes()
    {
        var nodes = new List<RuleTreeNode>();

        foreach (var category in Se.Settings.Edit.MultipleReplace.Categories)
        {
            var categoryNode = new RuleTreeNode(null, category.Name, new ObservableCollection<RuleTreeNode>(),
                category.IsActive)
            {
                IsCategory = true,
                IsExpanded = category.IsExpanded,
            };
            nodes.Add(categoryNode);

            foreach (var rule in category.Rules)
            {
                var node = new RuleTreeNode(categoryNode, rule);
                categoryNode.SubNodes?.Add(node);
            }
        }

        AddDefaultCategoryIfNone(nodes);

        return nodes;
    }

    private void SaveSettings()
    {
        Se.Settings.Edit.MultipleReplace.LastCategory = SelectedCategory?.CategoryName ?? string.Empty;
        Se.Settings.Edit.MultipleReplace.NewestFirst = NewestFirst;
        Se.Settings.Edit.MultipleReplace.Categories.Clear();
        foreach (var category in Nodes)
        {
            var c = new SeEditMultipleReplace.MultipleReplaceCategory
            {
                Name = category.CategoryName,
                IsActive = category.IsActive,
                IsExpanded = category.IsExpanded,
            };
            Se.Settings.Edit.MultipleReplace.Categories.Add(c);

            foreach (var rule in category.SubNodes ?? [])
            {
                c.Rules.Add(new MultipleReplaceRule
                {
                    Active = rule.IsActive,
                    Description = rule.Description,
                    Find = rule.Find,
                    ReplaceWith = rule.ReplaceWith,
                    Type = rule.Type,
                    WholeWord = rule.WholeWord,
                });
            }
        }
    }

    private static void AddDefaultCategoryIfNone(List<RuleTreeNode> nodes)
    {
        if (nodes.Count == 0)
        {
            var defaultCategory = new RuleTreeNode(null, Se.Language.General.Default,
                new ObservableCollection<RuleTreeNode>(), true)
            {
                IsCategory = true,
            };
            nodes.Add(defaultCategory);
        }
    }

    private void AddDefaultCategoryIfNone()
    {
        if (Nodes.Count == 0)
        {
            var defaultCategory = new RuleTreeNode(null, Se.Language.General.Default,
                new ObservableCollection<RuleTreeNode>(), true)
            {
                IsCategory = true,
            };
            Nodes.Add(defaultCategory);
            SelectedCategory = defaultCategory;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        GeneratePreview();
        UndoUnchecked();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Apply()
    {
        GeneratePreview();
        UndoUnchecked();

        // Apply the currently checked replacements to the document, then make the result the new
        // working subtitle so the next round operates on the already-fixed text - without closing
        // the window (Subtitle Edit 4 had this re-usable "Apply" button - #12029).
        // generateNewId: false - the paragraph ids are how the main window finds the grid row each
        // line came from, so they must survive every Apply round (#14053).
        OnApply?.Invoke(new Subtitle(FixedSubtitle, false), Fixes.Count(f => f.Apply));
        _subtitle = new Subtitle(FixedSubtitle, false);
        _dirty = true;
        GeneratePreview();
    }

    private void UndoUnchecked()
    {
        foreach (var fix in Fixes)
        {
            if (!fix.Apply)
            {
                FixedSubtitle.Paragraphs[fix.Number - 1].Text = _subtitle.Paragraphs[fix.Number - 1].Text;
            }
        }
    }

    [RelayCommand]
    private void SelectAllFixes()
    {
        foreach (var fix in Fixes)
        {
            fix.Apply = true;
        }
    }

    [RelayCommand]
    private void SelectNoFixes()
    {
        foreach (var fix in Fixes)
        {
            fix.Apply = false;
        }
    }

    [RelayCommand]
    private void InvertFixesSelection()
    {
        foreach (var fix in Fixes)
        {
            fix.Apply = !fix.Apply;
        }
    }

    /// <summary>
    /// The gestures advertised by the fixes grid context menu (#13502): tick all, untick all and
    /// invert the "Apply" column, the same set Remove text for hearing impaired and the rule
    /// category picker use. Called from a tunneling handler on the grid, which has to run before
    /// the TableView turns Ctrl+A into "select all rows" - and before the window's own key
    /// handler, where Ctrl+D means "duplicate rule".
    /// </summary>
    internal bool HandleFixesSelectionKey(KeyEventArgs e)
    {
        var isCommand = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);
        if (!isCommand || e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            return false;
        }

        var isShift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        if (e.Key == Key.A && !isShift)
        {
            SelectAllFixes();
        }
        else if (e.Key == Key.D && !isShift)
        {
            SelectNoFixes();
        }
        else if (e.Key == Key.I && isShift)
        {
            InvertFixesSelection();
        }
        else
        {
            return false;
        }

        return true;
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void NodeCategoryOpenContextMenu(RuleTreeNode? node)
    {
        if (node is not { IsCategory: true })
        {
            return;
        }

        var contextMenu = new ContextMenu
        {
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.General.EditDotDotDot,
                    Command = CategoryEditCommand,
                    CommandParameter = node,
                    InputGesture = EditGesture,
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.Edit.MultipleReplace.NewCategory,
                    Command = CategoryAddCategoryCommand,
                    CommandParameter = node
                },
                new MenuItem
                {
                    Header = Se.Language.Edit.MultipleReplace.AddRulesDotDotDot,
                    Command = CategoryAddRulesCommand,
                    CommandParameter = node,
                    InputGesture = AddRulesGesture,
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.General.MoveUp,
                    Command = CategoryMoveUpCommand,
                    CommandParameter = node,
                    InputGesture = MoveUpGesture,
                },
                new MenuItem
                {
                    Header = Se.Language.General.MoveDown,
                    Command = CategoryMoveDownCommand,
                    CommandParameter = node,
                    InputGesture = MoveDownGesture,
                },
                new MenuItem
                {
                    Header = Se.Language.General.MoveToTop,
                    Command = CategoryMoveToTopCommand,
                    CommandParameter = node,
                    InputGesture = MoveToTopGesture,
                },
                new MenuItem
                {
                    Header = Se.Language.General.MoveToBottom,
                    Command = CategoryMoveToBottomCommand,
                    CommandParameter = node,
                    InputGesture = MoveToBottomGesture,
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.General.Delete,
                    Command = CategoryDeleteCommand,
                    CommandParameter = node,
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.General.ImportDotDotDot,
                    Command = CategoryImportCommand,
                    CommandParameter = node,
                },
                new MenuItem
                {
                    Header = Se.Language.General.ExportDotDotDot,
                    Command = CategoryExportCommand,
                    CommandParameter = node,
                },
            }
        };

        OpenContextMenu(CategoriesListBox, contextMenu);
    }

    private static void OpenContextMenu(Control host, ContextMenu contextMenu)
    {
        host.ContextMenu = contextMenu;
        contextMenu.Closing += (_, _) =>
        {
            host.ContextMenu = null;
        };
        contextMenu.Open(host);
    }

    [RelayCommand]
    private void TreeOpenContextMenu()
    {
        var contextMenu = new ContextMenu
        {
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.Edit.MultipleReplace.NewCategory,
                    Command = CategoryAddCategoryCommand,
                    CommandParameter = null,
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.General.ImportDotDotDot,
                    Command = CategoryImportCommand,
                    CommandParameter = null,
                },
            }
        };

        OpenContextMenu(CategoriesListBox, contextMenu);
    }

    [RelayCommand]
    private async Task CategoryEdit(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditCategoryWindow, EditCategoryViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.Edit.MultipleReplace.EditCategory, node); });

        if (result.OkPressed)
        {
            node.CategoryName = result.CategoryName;
            _dirty = true;
            if (node == SelectedCategory)
            {
                UpdateSelectedCategoryTitle(); // "Rules in <name>"
            }
        }
    }

    [RelayCommand]
    private async Task CategoryAddCategory(RuleTreeNode? node)
    {
        // Categories are always top-level (they are added to, removed from and reordered inside
        // Nodes), so the new one has no parent - passing the node whose context menu was used
        // left a root category claiming another category as its Parent.
        var category = new RuleTreeNode(null, string.Empty, new ObservableCollection<RuleTreeNode>(), true);
        var result = await _windowService.ShowDialogAsync<EditCategoryWindow, EditCategoryViewModel>(Window!,
            vm =>
            {
                vm.Initialize(Se.Language.Edit.MultipleReplace.NewCategory, category);
            });

        if (result.OkPressed)
        {
            category.CategoryName = result.CategoryName;
            Nodes.Add(category);
            SelectedCategory = category;
            _dirty = true;
        }
    }

    /// <summary>
    /// Ctrl+N: the cursor to "Find what" in the type-in row above the rules.
    /// </summary>
    [RelayCommand]
    private void AddRule()
    {
        NewFindTextBox?.Focus();
        NewFindTextBox?.SelectAll();
    }

    /// <summary>
    /// "Add rules...": one window for a whole list of find/replace pairs, instead of a "New rule"
    /// dialog per word (#16215). With a category the rules go at its end, with a rule right after
    /// it; without a node (the toolbar button, Ctrl+Shift+N) the selection decides.
    /// </summary>
    [RelayCommand]
    private async Task CategoryAddRules(RuleTreeNode? node)
    {
        node ??= SelectedNode ?? SelectedCategory ?? Nodes.FirstOrDefault();
        if (node == null || Window == null)
        {
            return;
        }

        var category = node.IsCategory ? node : node.Parent;
        if (category?.SubNodes == null)
        {
            return;
        }

        // A list of words usually shares type and whole-word with the rules around it.
        var neighbour = node.IsCategory ? category.SubNodes.LastOrDefault() : node;
        var result = await _windowService.ShowDialogAsync<AddRulesWindow, AddRulesViewModel>(Window, vm =>
        {
            vm.Initialize(category.CategoryName, neighbour?.Type ?? MultipleReplaceType.CaseInsensitive, neighbour?.IsWholeWordActive ?? false);
        });

        if (result.OkPressed)
        {
            InsertRules(node, result.Rules.ToList());
        }
    }

    /// <summary>
    /// Appends rules to a category, or inserts them right after a rule, in the given order, and
    /// shows the last one.
    /// </summary>
    internal void InsertRules(RuleTreeNode anchor, IReadOnlyList<MultipleReplaceRule> rules)
    {
        var category = anchor.IsCategory ? anchor : anchor.Parent;
        if (category?.SubNodes == null || rules.Count == 0)
        {
            return;
        }

        var index = anchor.IsCategory ? category.SubNodes.Count : category.SubNodes.IndexOf(anchor) + 1;
        if (index <= 0 && !anchor.IsCategory)
        {
            index = category.SubNodes.Count;
        }

        RuleTreeNode? last = null;
        foreach (var rule in rules)
        {
            last = new RuleTreeNode(category, rule);
            category.SubNodes.Insert(index++, last);
        }

        _dirty = true;
        NavigateToRule(last);
    }

    [RelayCommand]
    private async Task CategoryDeleteAsync(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        if (Se.Settings.General.PromptBeforeDelete)
        {
            var answer = await MessageBox.Show(
                Window!,
                Se.Language.General.Delete,
                string.Format(Se.Language.Edit.MultipleReplace.DeleteCategoryConfirm, node.CategoryName),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        var index = Nodes.IndexOf(node);
        Nodes.Remove(node);
        AddDefaultCategoryIfNone();
        if (SelectedCategory == node || SelectedCategory == null)
        {
            SelectedCategory = Nodes[Math.Clamp(index, 0, Nodes.Count - 1)];
        }

        _dirty = true;
    }

    [RelayCommand]
    private async Task CategoryImport(RuleTreeNode? node)
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Options.Settings.OpenRuleFile, "Replace rules", ".template;.xml", "CSV (comma separated)", ".csv");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        // import from json, SE4 xml (exported rules or a full Settings.xml) or csv
        List<RuleTreeNode>? imported = null;
        try
        {
            var content = await System.IO.File.ReadAllTextAsync(fileName);

            CategoryImportExportItem? temp;

            if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                temp = CsvImporter.Import(content);
            }
            else if (content.Contains("<MultipleSearchAndReplaceList>"))
            {
                temp = Se4XmlImporter.ImportFromXml(content);
            }
            else if (content.Contains("<MultipleSearchAndReplaceGroups>"))
            {
                temp = Se4SettingsXmlReplaceImporter.ImportFromXmlAsImportExport(content);
            }
            else
            {
                temp = JsonSerializer.Deserialize<CategoryImportExportItem>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            if (temp == null)
            {
                imported = new List<RuleTreeNode>();
            }
            else
            {
                imported = temp.RuleTreeNodeList();
            }
        }
        catch (Exception exception)
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                "Unable to import replace rules: " + exception.Message,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        if (imported == null || imported.Count == 0)
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                "No replace rules found in file",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        // Let the user pick which of the categories in the file to bring in (#13529). A file with a
        // single category has nothing to pick, so it goes straight in as before.
        var toImport = imported;
        if (imported.Count > 1)
        {
            var picked = await _windowService
                .ShowDialogAsync<CategoryPickerWindow, CategoryPickerViewModel>(Window, vm =>
                {
                    vm.InitializeForImport(imported);
                });

            if (!picked.OkPressed)
            {
                return;
            }

            toImport = picked.Rules.Where(p => p.IsSelected).ToList();
            if (toImport.Count == 0)
            {
                return;
            }
        }

        foreach (var profile in toImport)
        {
            Nodes.Add(profile);
        }

        _dirty = true;

        await MessageBox.Show(
            Window!,
            Se.Language.General.Information,
            string.Format(Se.Language.Options.Settings.RuleProfilesImportedX, toImport.Count),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }


    [RelayCommand]
    private async Task CategoryExport(RuleTreeNode? node)
    {
        if (node == null || node.SubNodes == null || Window == null)
        {
            return;
        }

        var result = await _windowService
        .ShowDialogAsync<CategoryPickerWindow, CategoryPickerViewModel>(Window, vm =>
        {
            vm.InitializeForExport(Nodes.ToList(), node);
        });

        if (!result.OkPressed)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(
            Window,
            new[] { ("Replace rules", ".template"), ("CSV (comma separated)", ".csv") },
            "SE_Replace_Rules",
            Se.Language.Options.Settings.SaveRuleProfilesFile);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var toExport = result.Rules.Where(p => p.IsSelected).ToList();
        if (toExport.Count == 0)
        {
            return;
        }

        var export = new CategoryImportExportItem(toExport);
        if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            // UTF-8 with BOM so Excel opens non-ASCII rules correctly.
            await System.IO.File.WriteAllTextAsync(fileName, CsvExporter.Export(export), new System.Text.UTF8Encoding(true));
        }
        else
        {
            var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await System.IO.File.WriteAllTextAsync(fileName, json);
        }

        _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window,
            vm =>
            {
                vm.Initialize(Se.Language.General.FileSaved,
                    string.Format(Se.Language.Options.Settings.RuleProfilesExportedX, toExport.Count), fileName, true, true);
            });
    }

    [RelayCommand]
    private void CategoryMoveUp(RuleTreeNode? node) => MoveCategory(node, ListMoveDirection.Up);

    [RelayCommand]
    private void CategoryMoveDown(RuleTreeNode? node) => MoveCategory(node, ListMoveDirection.Down);

    [RelayCommand]
    private void CategoryMoveToTop(RuleTreeNode? node) => MoveCategory(node, ListMoveDirection.Top);

    [RelayCommand]
    private void CategoryMoveToBottom(RuleTreeNode? node) => MoveCategory(node, ListMoveDirection.Bottom);

    private void MoveCategory(RuleTreeNode? node, ListMoveDirection direction)
    {
        if (node == null || !node.IsCategory)
        {
            return;
        }

        MoveNodeIn(Nodes, node, direction);
    }

    [RelayCommand]
    private void NodeOpenContextMenu(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var contextMenu = new ContextMenu
        {
            Items =
            {
                new MenuItem
                {
                    Header = Se.Language.Edit.MultipleReplace.EditRule,
                    Command = NodeEditCommand,
                    CommandParameter = node,
                    InputGesture = EditGesture,
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.General.Duplicate,
                    Command = NodeDuplicateCommand,
                    CommandParameter = node,
                    InputGesture = DuplicateGesture,
                },
                new MenuItem
                {
                    Header = Se.Language.General.InsertBefore,
                    Command = NodeInsertBeforeCommand,
                    CommandParameter = node
                },
                new MenuItem
                {
                    Header = Se.Language.General.InsertAfter,
                    Command = NodeInsertAfterCommand,
                    CommandParameter = node,
                },
                new MenuItem
                {
                    Header = Se.Language.Edit.MultipleReplace.AddRulesDotDotDot,
                    Command = CategoryAddRulesCommand,
                    CommandParameter = node,
                    InputGesture = AddRulesGesture,
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.General.MoveUp,
                    Command = NodeMoveUpCommand,
                    CommandParameter = node,
                    InputGesture = MoveUpGesture,
                },
                new MenuItem
                {
                    Header = Se.Language.General.MoveDown,
                    Command = NodeMoveDownCommand,
                    CommandParameter = node,
                    InputGesture = MoveDownGesture,
                },
                new MenuItem
                {
                    Header = Se.Language.General.MoveToTop,
                    Command = NodeMoveToTopCommand,
                    CommandParameter = node,
                    InputGesture = MoveToTopGesture,
                },
                new MenuItem
                {
                    Header = Se.Language.General.MoveToBottom,
                    Command = NodeMoveToBottomCommand,
                    CommandParameter = node,
                    InputGesture = MoveToBottomGesture,
                },
                MakeMoveToCategoryMenuItem(node),
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.General.Delete,
                    Command = NodeDeleteCommand,
                    CommandParameter = node,
                    InputGesture = DeleteGesture,
                },
            }
        };

        OpenContextMenu(RulesTable, contextMenu);
    }

    [RelayCommand]
    private async Task NodeEdit(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditRuleWindow, EditRuleViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.Edit.MultipleReplace.EditRule, node); });

        if (result.OkPressed)
        {
            node.Find = result.FindWhat;
            node.ReplaceWith = result.ReplaceWith;
            node.Description = result.Description;
            if (result.IsRegularExpression)
            {
                node.Type = MultipleReplaceType.RegularExpression;
            }
            else if (result.IsCaseSensitive)
            {
                node.Type = MultipleReplaceType.CaseSensitive;
            }
            else
            {
                node.Type = MultipleReplaceType.CaseInsensitive;
            }

            node.WholeWord = result.IsWholeWord;
            _dirty = true;
        }
    }

    [RelayCommand]
    private void NodeDuplicate(RuleTreeNode? node)
    {
        if (node == null || node.Parent == null || node.Parent.SubNodes == null)
        {
            return;
        }

        var nodes = node.Parent.SubNodes;
        var index = nodes.IndexOf(node);
        if (index >= 0)
        {
            nodes.Insert(index, new RuleTreeNode(node.Parent, new MultipleReplaceRule
            {
                Active = node.IsActive,
                Description = node.Description,
                Find = node.Find,
                ReplaceWith = node.ReplaceWith,
                Type = node.Type,
                WholeWord = node.WholeWord,
            }));
            _dirty = true;
        }
    }

    [RelayCommand]
    private async Task NodeInsertBefore(RuleTreeNode? node)
    {
        if (node == null || node.Parent == null || node.Parent.SubNodes == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditRuleWindow, EditRuleViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.Edit.MultipleReplace.NewRule, node); });

        if (result.OkPressed)
        {
            var nodes = node.Parent.SubNodes;
            var index = nodes.IndexOf(node);
            var rule = MakeRuleTreeNode(node, result);
            nodes.Insert(index, rule);
            _dirty = true;
            NavigateToRule(rule);
        }
    }

    [RelayCommand]
    private async Task NodeInsertAfter(RuleTreeNode? node)
    {
        if (node == null || node.Parent == null || node.Parent.SubNodes == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditRuleWindow, EditRuleViewModel>(Window!,
            vm => { vm.Initialize(Se.Language.Edit.MultipleReplace.NewRule, node); });

        if (result.OkPressed)
        {
            var nodes = node.Parent.SubNodes;
            var index = nodes.IndexOf(node);
            var rule = MakeRuleTreeNode(node, result);
            nodes.Insert(index + 1, rule);
            _dirty = true;
            NavigateToRule(rule);
        }
    }

    [RelayCommand]
    private async Task NodeDeleteAsync(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        if (Se.Settings.General.PromptBeforeDelete)
        {
            var answer = await MessageBox.Show(
                Window!,
                Se.Language.General.Delete,
                string.Format(Se.Language.Edit.MultipleReplace.DeleteRuleConfirm, node.Find),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        if (node.Parent != null && node.Parent.SubNodes != null)
        {
            node.Parent.SubNodes.Remove(node);
        }

        _dirty = true;
    }

    [RelayCommand]
    private void NodeMoveUp(RuleTreeNode? node) => MoveRule(node, ListMoveDirection.Up);

    [RelayCommand]
    private void NodeMoveDown(RuleTreeNode? node) => MoveRule(node, ListMoveDirection.Down);

    [RelayCommand]
    private void NodeMoveToTop(RuleTreeNode? node) => MoveRule(node, ListMoveDirection.Top);

    [RelayCommand]
    private void NodeMoveToBottom(RuleTreeNode? node) => MoveRule(node, ListMoveDirection.Bottom);

    private void MoveRule(RuleTreeNode? node, ListMoveDirection direction)
    {
        if (node == null || node.IsCategory || node.Parent?.SubNodes == null || !CanReorderRules)
        {
            return;
        }

        // Newest first shows the rules bottom up, so up on screen is later in the run order.
        if (NewestFirst)
        {
            direction = direction switch
            {
                ListMoveDirection.Up => ListMoveDirection.Down,
                ListMoveDirection.Down => ListMoveDirection.Up,
                ListMoveDirection.Top => ListMoveDirection.Bottom,
                _ => ListMoveDirection.Top,
            };
        }

        MoveNodeIn(node.Parent.SubNodes, node, direction);
    }

    /// <summary>
    /// "Move to category" submenu with every category except the rule's own - before this a rule
    /// could only change category by recreating it there and deleting the original (#15374).
    /// </summary>
    private MenuItem MakeMoveToCategoryMenuItem(RuleTreeNode node)
    {
        var menuItem = new MenuItem { Header = Se.Language.Edit.MultipleReplace.MoveToCategory };
        foreach (var item in MakeMoveToCategoryItems(node))
        {
            menuItem.Items.Add(item);
        }

        menuItem.IsEnabled = menuItem.Items.Count > 0;
        return menuItem;
    }

    internal List<MenuItem> MakeMoveToCategoryItems(RuleTreeNode node)
    {
        return Nodes
            .Where(p => p.IsCategory && p != node.Parent)
            .Select(category => new MenuItem
            {
                Header = category.CategoryName,
                Command = new RelayCommand(() => MoveRuleToCategory(node, category)),
            })
            .ToList();
    }

    /// <summary>
    /// The "Move to category" button under the rules: the categories as a menu on the button.
    /// </summary>
    internal void OpenMoveToCategoryMenu(Control button)
    {
        var node = SelectedNode;
        if (node == null)
        {
            return;
        }

        var contextMenu = new ContextMenu();
        foreach (var item in MakeMoveToCategoryItems(node))
        {
            contextMenu.Items.Add(item);
        }

        if (contextMenu.Items.Count > 0)
        {
            OpenContextMenu(button, contextMenu);
        }
    }

    /// <summary>
    /// Moves a rule to the end of another category, then expands that category and keeps the
    /// rule selected so it can be walked into place with Ctrl+Up/Down.
    /// </summary>
    internal void MoveRuleToCategory(RuleTreeNode? node, RuleTreeNode? category)
    {
        if (node == null || node.IsCategory || node.Parent?.SubNodes == null ||
            category?.SubNodes == null || !category.IsCategory || category == node.Parent)
        {
            return;
        }

        node.Parent.SubNodes.Remove(node);
        node.Parent = category;
        category.SubNodes.Add(node);
        _dirty = true;
        NavigateToRule(node);
    }

    /// <summary>
    /// Reorders a single node inside the collection it lives in and keeps it selected and
    /// focused afterwards - <see cref="ObservableCollection{T}.Move"/> rebuilds the tree
    /// container, which otherwise drops both.
    /// </summary>
    private void MoveNodeIn(ObservableCollection<RuleTreeNode> nodes, RuleTreeNode node, ListMoveDirection direction)
    {
        var index = nodes.IndexOf(node);
        if (index < 0)
        {
            return;
        }

        ListReorder.Move(nodes, new[] { index }, direction);

        if (nodes.IndexOf(node) == index)
        {
            return; // already at the edge - nothing moved, so the rules are unchanged
        }

        _dirty = true;
        SelectedNode = node;
        Dispatcher.UIThread.Post(() => FocusNode(node), DispatcherPriority.Input);
    }

    /// <summary>
    /// Selects a category in the category list, or a rule in the rules list, scrolls it into view
    /// and - with <paramref name="focus"/> - puts keyboard focus on its row, so the next
    /// Ctrl+Up/Down keeps walking the same node (#14136).
    /// </summary>
    private void FocusNode(RuleTreeNode node, bool focus = true)
    {
        if (node.IsCategory)
        {
            SelectedCategory = node;
            CategoriesListBox.ScrollIntoView(node);
            if (focus && CategoriesListBox.ContainerFromItem(node) is { } categoryContainer)
            {
                categoryContainer.Focus(NavigationMethod.Directional);
            }

            return;
        }

        SelectedNode = node;
        if (!VisibleRules.Contains(node))
        {
            return;
        }

        RulesTable.SelectedItem = node;
        RulesTable.ScrollIntoView(node);
        if (focus)
        {
            TableViewExtras.FocusRow(RulesTable);
        }
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.None)
        {
            // Initial focus is on the empty type-in row, not the OK button (a focused button clicks
            // on bare Space), so Enter has to reach OK from the window - the type-in row lets Enter
            // through while it is empty, the rules filter keeps it, and the rules list and preview
            // grid do not use Enter themselves. A focused Cancel/Apply button consumes Enter before
            // it bubbles here, so those keep their own meaning (#14586).
            e.Handled = true;
            Ok();
        }
        else if (e.Key == Key.N && IsCommand(e))
        {
            e.Handled = true;
            AddRule();
        }
        else if (e.Key == Key.N && IsCommand(e, KeyModifiers.Shift))
        {
            e.Handled = true;
            _ = CategoryAddRules(null);
        }
        else if (e.Key == Key.F2 && e.KeyModifiers == KeyModifiers.None)
        {
            // Edit the selected rule in its row, or rename the selected category - the usual "rename" key.
            var node = CategoriesListBox.IsKeyboardFocusWithin ? SelectedCategory : SelectedNode ?? SelectedCategory;
            if (node != null)
            {
                e.Handled = true;
                if (node.IsCategory)
                {
                    _ = CategoryEdit(node);
                }
                else
                {
                    BeginInlineEdit(node, nameof(RuleTreeNode.Find));
                }
            }
        }
        else if (e.Key == Key.D && IsCommand(e))
        {
            e.Handled = true;
            var node = SelectedNode;
            if (node != null && !node.IsCategory)
            {
                NodeDuplicate(node);
            }
        }
        else if (e.Key == Key.F && IsCommand(e))
        {
            e.Handled = true;
            _ = FindRule();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/edit", "multiple-replace");
        }
    }

    /// <summary>
    /// Ctrl on Windows/Linux, Cmd on macOS (Ctrl is accepted there too), with exactly
    /// <paramref name="extra"/> held as well - so Ctrl+N is not also Ctrl+Shift+N.
    /// </summary>
    private static bool IsCommand(KeyEventArgs e, KeyModifiers extra = KeyModifiers.None)
    {
        return e.KeyModifiers == (KeyModifiers.Control | extra) || e.KeyModifiers == (KeyModifiers.Meta | extra);
    }

    private async Task FindRule()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<FindRuleWindow, FindRuleViewModel>(Window,
            vm => { vm.Initialize(Nodes); });

        if (!result.OkPressed || result.SelectedRule == null)
        {
            return;
        }

        NavigateToRule(result.SelectedRule);
    }

    internal void NavigateToRule(RuleTreeNode? rule, bool focus = true)
    {
        if (rule == null)
        {
            return;
        }

        // Shows the rule's category and selects the rule now; scrolling to it has to wait for the
        // layout pass that realizes the new rows.
        SelectedNode = rule;
        Dispatcher.UIThread.Post(() => FocusNode(rule, focus), DispatcherPriority.Input);
    }

    /// <summary>
    /// Ctrl/Cmd + Up/Down/Home/End reorders the selected rule or category. This has to tunnel:
    /// the list box inside the tree view handles Ctrl+Arrow itself (move focus, keep selection),
    /// so a bubbling handler never sees it.
    /// </summary>
    internal void RulesTable_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Source is not TextBox) // Ctrl+Home/End in an inline edit box moves the caret
        {
            MoveKeyDown(e, SelectedNode);
        }
    }

    internal void CategoriesListBox_PreviewKeyDown(object? sender, KeyEventArgs e) => MoveKeyDown(e, SelectedCategory);

    private void MoveKeyDown(KeyEventArgs e, RuleTreeNode? node)
    {
        if (e.KeyModifiers is not (KeyModifiers.Control or KeyModifiers.Meta))
        {
            return;
        }

        var direction = e.Key switch
        {
            Key.Up => (ListMoveDirection?)ListMoveDirection.Up,
            Key.Down => ListMoveDirection.Down,
            Key.Home => ListMoveDirection.Top,
            Key.End => ListMoveDirection.Bottom,
            _ => null,
        };

        if (direction == null || node == null)
        {
            return;
        }

        e.Handled = true;
        if (node.IsCategory)
        {
            MoveCategory(node, direction.Value);
        }
        else
        {
            MoveRule(node, direction.Value);
        }
    }

    internal void RulesTable_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Source is TextBox)
        {
            return; // typing in an inline edit box
        }

        if (e.Key == Key.Delete && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            _ = RemoveSelectedRule();
        }
        else if (e.Key == Key.Space && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            var node = SelectedNode;
            if (node != null)
            {
                node.IsActive = !node.IsActive;
                _dirty = true;
            }
        }
        else if (e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.None && SelectedNode != null)
        {
            e.Handled = true;
            BeginInlineEdit(SelectedNode, nameof(RuleTreeNode.Find));
        }
    }

    /// <summary>
    /// Shows text boxes for find, replace with and description in the rule's own row, the caret
    /// in <paramref name="field"/>. The preview follows while typing; Enter keeps the change,
    /// Escape puts the texts back, and leaving the row keeps it.
    /// </summary>
    internal void BeginInlineEdit(RuleTreeNode? node, string field)
    {
        if (node == null || node.IsCategory)
        {
            return;
        }

        if (_inlineEditNode != null && _inlineEditNode != node)
        {
            EndInlineEdit(commit: true, focusRow: false);
        }

        node.EditField = field;
        if (_inlineEditNode == node)
        {
            node.RequestEditFocus();
            return;
        }

        _inlineEditOriginal = (node.Find, node.ReplaceWith, node.Description);
        _inlineEditNode = node;
        node.IsEditing = true;
    }

    internal void EndInlineEdit(bool commit, bool focusRow = true)
    {
        var node = _inlineEditNode;
        if (node == null)
        {
            return;
        }

        _inlineEditNode = null;
        if (!commit)
        {
            node.Find = _inlineEditOriginal.Find;
            node.ReplaceWith = _inlineEditOriginal.ReplaceWith;
            node.Description = _inlineEditOriginal.Description;
        }

        node.IsEditing = false;
        _dirty = true;
        if (focusRow)
        {
            Dispatcher.UIThread.Post(() => FocusNode(node), DispatcherPriority.Input);
        }
    }

    internal void InlineEditTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        // Handled here, so neither reaches the window - where Enter means OK and Escape closes.
        if (e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            EndInlineEdit(commit: true);
        }
        else if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            EndInlineEdit(commit: false);
        }
    }

    /// <summary>
    /// Focus left the rules list while a row was being edited - keep the change.
    /// </summary>
    internal void RulesTableLostFocus()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_inlineEditNode != null && !RulesTable.IsKeyboardFocusWithin)
            {
                EndInlineEdit(commit: true, focusRow: false);
            }
        }, DispatcherPriority.Input);
    }

    /// <summary>
    /// The rule type picker behind the type icon in front of "Find what".
    /// </summary>
    internal void OpenRuleTypeMenu(Control button, RuleTreeNode? node)
    {
        if (node == null || node.IsCategory)
        {
            return;
        }

        var contextMenu = new ContextMenu();
        foreach (var typeItem in ReplaceTypes)
        {
            var menuItem = new MenuItem
            {
                Header = typeItem.Name,
                Icon = new Optris.Icons.Avalonia.Icon { Value = RuleTreeNode.GetIconName(typeItem.Type) },
                ToggleType = MenuItemToggleType.Radio,
                IsChecked = node.Type == typeItem.Type,
            };
            menuItem.Click += (_, _) =>
            {
                if (node.Type != typeItem.Type)
                {
                    node.Type = typeItem.Type;
                    _dirty = true;
                }
            };
            contextMenu.Items.Add(menuItem);
        }

        OpenContextMenu(button, contextMenu);
    }

    internal void CategoriesListBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            _ = CategoryDeleteAsync(SelectedCategory);
        }
        else if (e.Key == Key.Space && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            var category = SelectedCategory;
            if (category != null)
            {
                category.IsActive = !category.IsActive;
                _dirty = true;
            }
        }
    }

    /// <summary>
    /// Deletes the selected rule and selects the one that took its place in the list.
    /// </summary>
    [RelayCommand]
    private async Task RemoveSelectedRule()
    {
        var node = SelectedNode;
        if (node == null)
        {
            return;
        }

        var index = VisibleRules.IndexOf(node);
        await NodeDeleteAsync(node);
        if (node.Parent?.SubNodes?.Contains(node) == true || VisibleRules.Count == 0 || index < 0)
        {
            return; // not deleted, or nothing left to select
        }

        var next = VisibleRules[Math.Min(index, VisibleRules.Count - 1)];
        Dispatcher.UIThread.Post(() => FocusNode(next), DispatcherPriority.Input);
    }

    [RelayCommand]
    private Task EditSelectedRule() => SelectedNode == null ? Task.CompletedTask : NodeEdit(SelectedNode);

    [RelayCommand]
    private void MoveSelectedRuleUp() => MoveRule(SelectedNode, ListMoveDirection.Up);

    [RelayCommand]
    private void MoveSelectedRuleDown() => MoveRule(SelectedNode, ListMoveDirection.Down);

    [RelayCommand]
    private Task EditSelectedCategory() => CategoryEdit(SelectedCategory);

    [RelayCommand]
    private Task DeleteSelectedCategory() => CategoryDeleteAsync(SelectedCategory);

    [RelayCommand]
    private void OpenSelectedCategoryMenu() => NodeCategoryOpenContextMenu(SelectedCategory);

    private static RuleTreeNode MakeRuleTreeNode(RuleTreeNode node, EditRuleViewModel result)
    {
        return new RuleTreeNode(node.Parent, new MultipleReplaceRule
        {
            // "node" is only the neighbour used to find the insert position - a rule the user just
            // typed must start active, as the type-in row does. Inheriting the neighbour's state
            // meant inserting next to an unticked rule silently created an unticked one that never
            // ran, with nothing in the dialog to explain why.
            Active = true,
            Description = result.Description,
            Find = result.FindWhat,
            ReplaceWith = result.ReplaceWith,
            Type = result.IsRegularExpression ? MultipleReplaceType.RegularExpression :
                result.IsCaseSensitive ? MultipleReplaceType.CaseSensitive :
                MultipleReplaceType.CaseInsensitive,
            WholeWord = result.IsWholeWord,
        });
    }

    private void GeneratePreview()
    {
        // Snapshot outside the lock: it hops to the UI thread, and "Ok"/"Apply" call this from
        // there while the timer thread may already hold the lock.
        var replaceExpressions = BuildReplaceExpressions();

        lock (_previewLock)
        {
            GeneratePreview(replaceExpressions);
        }
    }

    // "Ok" and "Apply" generate on the UI thread while the preview timer generates on its own
    // thread; both write FixedSubtitle and TotalReplaced, so only one may run at a time.
    private void GeneratePreview(List<ReplaceExpression> replaceExpressions)
    {
        FixedSubtitle = new Subtitle(_subtitle, false);
        TotalReplaced = 0;
        var fixes = new List<MultipleReplaceFix>();

        // Patterns the match timeout stopped part way through this pass - see the catch below.
        HashSet<string>? retiredThisPass = null;

        for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
        {
            var p = _subtitle.Paragraphs[i];
            var hit = false;
            var newText = p.Text;
            var ruleInfo = string.Empty;
            var ruleHits = new List<ReplaceExpression>();
            foreach (var item in replaceExpressions)
            {
                if (item.WholeWordRegex != null)
                {
                    if (item.WholeWordRegex.IsMatch(newText))
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                        ruleHits.Add(item);

                        // An evaluator so the replacement is literal text - a "$" in it is not a group reference.
                        newText = item.WholeWordRegex.Replace(newText, _ => item.ReplaceWith);
                    }
                }
                else if (item.SearchType == ReplaceExpression.SearchCaseSensitive)
                {
                    if (newText.Contains(item.FindWhat))
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                        ruleHits.Add(item);
                        newText = newText.Replace(item.FindWhat, item.ReplaceWith);
                    }
                }
                else if (item.SearchType == ReplaceExpression.SearchRegEx)
                {
                    // retiredThisPass is null until something times out, so this costs a null
                    // check per rule per line in the normal case. It is needed because the
                    // expression list was built before the timeout, and without it every
                    // remaining line would pay the five seconds again.
                    if (retiredThisPass?.Contains(item.FindWhat) == true ||
                        !TryGetRunnableRegex(item.FindWhat, out var r))
                    {
                        continue;
                    }

                    try
                    {
                        // Match against line-feed-normalized text so a pattern's \n line break matches even
                        // when the paragraph text uses \r\n (the pattern is FixNewLine'd to \n) (#11956).
                        // Text without a \r or \u2028 is already in that form - and this runs per
                        // regex rule, per line, so skip the split and join for it.
                        var lineFeedText = newText.AsSpan().IndexOfAny('\r', '\u2028') < 0
                            ? newText
                            : string.Join("\n", newText.SplitToLines());
                        if (r.IsMatch(lineFeedText))
                        {
                            var replaced = RegexUtils.ReplaceNewLineSafe(r, newText, item.ReplaceWith);
                            hit = true;
                            ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                            ruleHits.Add(item);
                            newText = replaced;
                        }
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        // Five seconds on one line for a pattern that backtracks catastrophically
                        // is already too much, so retire the rule rather than carrying on with it.
                        (retiredThisPass ??= new HashSet<string>(StringComparer.Ordinal)).Add(item.FindWhat);
                        RetireTimedOutRegex(item.FindWhat);
                    }
                }
                else
                {
                    var index = newText.IndexOf(item.FindWhat, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                        ruleHits.Add(item);
                        do
                        {
                            newText = newText.Remove(index, item.FindWhat.Length).Insert(index, item.ReplaceWith);
                            index = newText.IndexOf(item.FindWhat, index + item.ReplaceWith.Length,
                                StringComparison.OrdinalIgnoreCase);
                        } while (index >= 0);
                    }
                }
            }

            if (hit && newText != p.Text)
            {
                TotalReplaced++;

                var fix = new MultipleReplaceFix
                {
                    Apply = true,
                    Number = i + 1,
                    Before = p.Text,
                    After = newText,
                    Hits = ruleHits,
                };
                fixes.Add(fix);
                FixedSubtitle.Paragraphs[i].Text = newText;
            }
        }

        Dispatcher.UIThread.Post(() =>
        {
            Fixes.Clear();
            Fixes.AddRange(fixes);
            FixesInfo = string.Format(Se.Language.Edit.MultipleReplace.XLinesAffected, TotalReplaced);
        });
    }

    private List<ReplaceExpression> BuildReplaceExpressions()
    {
        var replaceExpressions = new List<ReplaceExpression>();
        var errors = new List<(RuleTreeNode Rule, string? Message)>();

        foreach (var group in SnapshotRules())
        {
            var rules = group.Rules;
            for (var ruleNumber = 1; ruleNumber <= rules.Count; ruleNumber++)
            {
                var rule = rules[ruleNumber - 1];
                var isRegex = rule.SearchType == ReplaceExpression.SearchTypeRegularExpression;
                var findWhat = rule.Find;
                if (!string.IsNullOrEmpty(findWhat) && isRegex) // allow space or spaces
                {
                    findWhat = RegexUtils.FixNewLine(findWhat);
                }

                // Every regular expression is checked, whether or not it runs, so that a rule
                // sitting in an unticked category is still flagged as broken in the tree.
                var error = isRegex && !string.IsNullOrEmpty(findWhat) ? GetRegexError(findWhat) : null;
                errors.Add((rule, error));

                if (error != null || !group.IsActive || !rule.IsActive || string.IsNullOrEmpty(findWhat))
                {
                    continue;
                }

                var replaceWith = isRegex ? RegexUtils.FixNewLine(rule.ReplaceWith) : rule.ReplaceWith;
                var ruleInfo = string.IsNullOrEmpty(rule.Description)
                    ? $"Group name: {group.CategoryName} - Rule number: {ruleNumber}"
                    : $"Group name: {group.CategoryName} - Rule number: {ruleNumber}. {rule.Description}";
                var mpi = new ReplaceExpression(findWhat, replaceWith, rule.SearchType, ruleInfo);
                mpi.RuleTreeNode = rule;
                if (rule.IsWholeWordActive)
                {
                    mpi.WholeWordRegex = GetWholeWordRegex(findWhat, mpi.SearchType != ReplaceExpression.SearchCaseSensitive);
                }

                replaceExpressions.Add(mpi);
            }
        }

        ReportRuleErrors(errors);

        return replaceExpressions;
    }

    /// <summary>
    /// Null when the pattern is usable. A pattern that will not compile - or that has already been
    /// stopped by the match timeout - is skipped rather than allowed to take the whole preview down
    /// with it (#13534); the rule is marked in the tree instead.
    /// </summary>
    private string? GetRegexError(string findWhat)
    {
        // Deliberately NOT RegexOptions.Compiled: every regular expression rule is validated,
        // including the ones in unticked categories that never run, and compiling emits IL that
        // is never reclaimed. The runnable regex is built lazily in TryGetRunnableRegex instead.
        return _regExErrors.GetOrAdd(findWhat, static pattern =>
        {
            try
            {
                _ = new Regex(pattern, RegexOptions.Multiline, RegexUtils.UserPatternMatchTimeout);
                return null;
            }
            catch (ArgumentException exception)
            {
                return string.Format(Se.Language.General.InvalidRegularExpressionX, exception.Message);
            }
        });
    }

    /// <summary>
    /// The regex a rule actually runs with, built on first use. Carries the match timeout: without
    /// it a pattern with catastrophic backtracking holds <see cref="_previewLock"/> forever - and
    /// with it the UI thread, which waits on the same lock in "Ok" and "Apply".
    /// </summary>
    private bool TryGetRunnableRegex(string findWhat, out Regex regex)
    {
        if (_compiledRegExList.TryGetValue(findWhat, out regex!))
        {
            return true;
        }

        try
        {
            regex = new Regex(findWhat, RegexOptions.Compiled | RegexOptions.Multiline, RegexUtils.UserPatternMatchTimeout);
            _compiledRegExList[findWhat] = regex;
            return true;
        }
        catch (ArgumentException)
        {
            regex = null!;
            return false;
        }
    }

    /// <summary>
    /// The regex a "Whole word" rule runs with: the escaped find text between word boundaries, so
    /// "Zeyn" no longer matches inside "Zeynep" (#15510). Cached per find text and case setting.
    /// </summary>
    private Regex GetWholeWordRegex(string findWhat, bool ignoreCase)
    {
        return _wholeWordRegexes.GetOrAdd((findWhat, ignoreCase), static key =>
            ReplaceExpression.CreateWholeWordRegex(key.FindWhat, key.IgnoreCase));
    }

    /// <summary>
    /// Retires a pattern the match timeout stopped. Recording it as an error means the next pass
    /// leaves the rule out and marks it in the tree, so a rule that is too slow to run says so
    /// rather than looking like a rule that simply matches nothing.
    /// </summary>
    private void RetireTimedOutRegex(string findWhat)
    {
        _regExErrors[findWhat] = string.Format(
            Se.Language.Edit.MultipleReplace.RegularExpressionTooSlowX,
            RegexUtils.UserPatternMatchTimeout.TotalSeconds);
        _dirty = true;
    }

    private void ReportRuleErrors(List<(RuleTreeNode Rule, string? Message)> errors)
    {
        if (errors.All(e => e.Message == e.Rule.ErrorMessage))
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var (rule, message) in errors)
            {
                rule.ErrorMessage = message;
            }
        });
    }

    /// <summary>
    /// The rule tree is owned by the UI thread but the preview runs on a timer thread, so copy the
    /// groups and their rules over there - iterating the live collections could throw "collection
    /// was modified" mid-edit, which took the whole preview down with it (#13534).
    /// </summary>
    private List<(string CategoryName, bool IsActive, List<RuleTreeNode> Rules)> SnapshotRules()
    {
        return Dispatcher.UIThread.Invoke(() => Nodes
            .Where(p => p.SubNodes != null)
            .Select(p => (p.CategoryName, p.IsActive, Rules: p.SubNodes!.ToList()))
            .ToList());
    }

    public void OnActiveChanged(object? sender, RoutedEventArgs e)
    {
        _dirty = true;
    }

    public void RuleTextChanged(object? sender, TextChangedEventArgs e)
    {
        _dirty = true;
    }

    internal void OnClosing()
    {
        // The view model is transient, so without this every visit to the dialog leaves another
        // preview timer ticking for the rest of the process lifetime.
        _closed = true;
        _timerReplace.Stop();
        _timerReplace.Elapsed -= TimerReplaceElapsed;
        _timerReplace.Dispose();

        SaveSettings();
        UiUtil.SaveWindowPosition(Window);
    }

    internal void OnLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);
    }

    internal void RulesTableDoubleTapped(string field)
    {
        BeginInlineEdit(SelectedNode, field);
    }

    internal void CategoriesListBoxDoubleTapped()
    {
        if (SelectedCategory is { } category)
        {
            _ = CategoryEdit(category);
        }
    }
}
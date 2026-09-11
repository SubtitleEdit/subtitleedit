using Avalonia;
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
    [ObservableProperty] private MultipleReplaceTypeItem? _selectedRuleType;

    public ObservableCollection<RuleTreeNode> Nodes { get; }
    public TreeView RulesTreeView { get; internal set; }
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
    private static readonly KeyModifiers MoveModifier = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
    private static readonly KeyGesture MoveUpGesture = new(Key.Up, MoveModifier);
    private static readonly KeyGesture MoveDownGesture = new(Key.Down, MoveModifier);
    private static readonly KeyGesture MoveToTopGesture = new(Key.Home, MoveModifier);
    private static readonly KeyGesture MoveToBottomGesture = new(Key.End, MoveModifier);

    public MultipleReplaceViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;

        Fixes = new ObservableCollection<MultipleReplaceFix>();
        SelectedFixHits = new ObservableCollection<ReplaceExpression>();
        FixesInfo = string.Empty;
        Nodes = new ObservableCollection<RuleTreeNode>(GetNodes());
        RulesTreeView = new TreeView();
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
    }

    // Keep SelectedRuleType in sync with SelectedNode
    partial void OnSelectedNodeChanged(RuleTreeNode? value)
    {
        if (value == null || value.IsCategory)
        {
            SelectedRuleType = null;
            return;
        }

        var item = ReplaceTypes.FirstOrDefault(t => t.Type == value.Type);
        SelectedRuleType = item;
    }

    partial void OnSelectedRuleTypeChanged(MultipleReplaceTypeItem? value)
    {
        if (value == null)
        {
            return;
        }

        var node = SelectedNode;
        if (node == null || node.IsCategory)
        {
            return;
        }

        if (node.Type != value.Type)
        {
            node.Type = value.Type;
            _dirty = true;
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

        // Expanded/collapsed is restored by the tree item container theme binding to
        // RuleTreeNode.IsExpanded - pushing it onto the containers from here could not work, as
        // the view model is configured before the window is even constructed (#13526).
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
            SelectedNode = defaultCategory;
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
                    CommandParameter = node
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
                    Header = Se.Language.Edit.MultipleReplace.NewRule,
                    Command = CategoryAddRuleCommand,
                    CommandParameter = node,
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

        RulesTreeView.ContextMenu = contextMenu;
        contextMenu.Closing += (sender, args) =>
        {
            RulesTreeView.ContextMenu = null;
        };
        contextMenu.Open();
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

        RulesTreeView.ContextMenu = contextMenu;
        contextMenu.Closing += (_, _) =>
        {
            RulesTreeView.ContextMenu = null;
        };
        contextMenu.Open();
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
            SelectedNode = category;
            _dirty = true;
        }
    }

    [RelayCommand]
    private async Task CategoryAddRule(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditRuleWindow, EditRuleViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Edit.MultipleReplace.NewRule, node);
        });

        if (result.OkPressed)
        {
            var rule = new RuleTreeNode(node, new MultipleReplaceRule
            {
                Active = true,
                Description = result.Description,
                Find = result.FindWhat,
                ReplaceWith = result.ReplaceWith,
                Type = result.IsRegularExpression ? MultipleReplaceType.RegularExpression :
                    result.IsCaseSensitive ? MultipleReplaceType.CaseSensitive :
                    MultipleReplaceType.CaseInsensitive,
            });
            node.SubNodes?.Add(rule);
            node.IsExpanded = true;

            SelectedNode = rule;
            _dirty = true;
        }
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

        Nodes.Remove(node);
        AddDefaultCategoryIfNone();
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
            var content = System.IO.File.ReadAllText(fileName);

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
            System.IO.File.WriteAllText(fileName, CsvExporter.Export(export), new System.Text.UTF8Encoding(true));
        }
        else
        {
            var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            System.IO.File.WriteAllText(fileName, json);
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
                    CommandParameter = node
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.General.Duplicate,
                    Command = NodeDuplicateCommand,
                    CommandParameter = node,
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
                    CommandParameter = node
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
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.General.Delete,
                    Command = NodeDeleteCommand,
                    CommandParameter = node
                },
            }
        };

        RulesTreeView.ContextMenu = contextMenu;
        contextMenu.Closing += (sender, args) =>
        {
            RulesTreeView.ContextMenu = null;
        };
        contextMenu.Open();
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
            SelectedNode = rule;
            _dirty = true;
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
            SelectedNode = rule;
            _dirty = true;
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
        if (node == null || node.IsCategory || node.Parent?.SubNodes == null)
        {
            return;
        }

        MoveNodeIn(node.Parent.SubNodes, node, direction);
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
    /// Selects a node and puts keyboard focus back on its row, so the next Ctrl+Up/Down keeps
    /// walking the same node. <see cref="ItemsControl.ContainerFromItem"/> only ever sees the
    /// top-level categories, so a rule - which lives one level down - never got its container
    /// back: focus was left nowhere, the tree handed it to the category above on the next key
    /// press, and that stole the selection (#14136).
    /// </summary>
    private void FocusNode(RuleTreeNode node)
    {
        SelectedNode = node;
        if (RulesTreeView.TreeContainerFromItem(node) is TreeViewItem container)
        {
            container.BringIntoView();
            container.Focus(NavigationMethod.Directional);
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
            // Initial focus is on the rules tree, not the OK button (a focused button clicks on bare
            // Space), so Enter has to reach OK from the window - the rules tree and preview grid do
            // not use Enter themselves. A focused Cancel/Apply button consumes Enter before it bubbles
            // here, so those keep their own meaning (#14586).
            e.Handled = true;
            Ok();
        }
        else if (e.Key == Key.N && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            var node = SelectedNode;
            if (node != null)
            {
                if (node.IsCategory)
                {
                    _ = CategoryAddRule(node);
                }
                else
                {
                    _ = NodeInsertAfter(node);
                }
            }
        }
        else if (e.Key == Key.D && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            var node = SelectedNode;
            if (node != null && !node.IsCategory)
            {
                NodeDuplicate(node);
            }
        }
        else if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            _ = FindRule();
        }
        else if ((e.Key == Key.OemMinus || e.Key == Key.Subtract) && e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift))
        {
            e.Handled = true;
            CollapseAll();
        }
        else if ((e.Key == Key.OemPlus || e.Key == Key.Add) && e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift))
        {
            e.Handled = true;
            ExpandAll();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/edit");
        }
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

    internal void NavigateToRule(RuleTreeNode? rule)
    {
        if (rule == null)
        {
            return;
        }

        if (rule.Parent != null)
        {
            rule.Parent.IsExpanded = true;
        }

        // The rule's own container only exists once the category above it has expanded, so
        // selecting and scrolling to it has to wait for that layout pass.
        Dispatcher.UIThread.Post(() => FocusNode(rule), DispatcherPriority.Input);
    }

    /// <summary>
    /// Ctrl/Cmd + Up/Down/Home/End reorders the selected rule or category. This has to tunnel:
    /// the list box inside the tree view handles Ctrl+Arrow itself (move focus, keep selection),
    /// so a bubbling handler never sees it.
    /// </summary>
    internal void RulesTreeView_PreviewKeyDown(object? sender, KeyEventArgs e)
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

        if (direction == null)
        {
            return;
        }

        var node = SelectedNode;
        if (node == null)
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

    internal async void RulesTreeView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            e.Handled = true;
            var node = SelectedNode;
            if (node != null && !node.IsCategory)
            {
                var idx = -1;
                var parent = node.Parent;
                if (parent != null && parent.SubNodes != null)
                {
                    idx = parent.SubNodes.IndexOf(node);
                }

                await NodeDeleteAsync(node);

                if (parent != null && parent.SubNodes != null && idx >= 0)
                {
                    RuleTreeNode? selectedNode = null;
                    if (idx < parent.SubNodes.Count)
                    {
                        selectedNode = parent.SubNodes[idx];
                    }
                    else if (parent.SubNodes.Count > 0)
                    {
                        selectedNode = parent.SubNodes[parent.SubNodes.Count - 1];
                    }

                    if (selectedNode != null)
                    {
                        Dispatcher.UIThread.Post(() => FocusNode(selectedNode), DispatcherPriority.Input);
                    }
                }
            }
        }
        else if (e.Key == Key.Space)
        {
            e.Handled = true;
            var node = SelectedNode;
            if (node != null)
            {
                node.IsActive = !node.IsActive;
                _dirty = true;
            }
        }
    }

    [RelayCommand]
    public void ExpandAll()
    {
        SetAllExpanded(true);
    }

    [RelayCommand]
    public void CollapseAll()
    {
        SetAllExpanded(false);
    }

    private void SetAllExpanded(bool isExpanded)
    {
        foreach (var node in Nodes.Where(p => p.IsCategory))
        {
            node.IsExpanded = isExpanded;
        }
    }

    private static RuleTreeNode MakeRuleTreeNode(RuleTreeNode node, EditRuleViewModel result)
    {
        return new RuleTreeNode(node.Parent, new MultipleReplaceRule
        {
            // "node" is only the neighbour used to find the insert position - a rule the user just
            // typed must start active, as CategoryAddRule does. Inheriting the neighbour's state
            // meant inserting next to an unticked rule silently created an unticked one that never
            // ran, with nothing in the dialog to explain why.
            Active = true,
            Description = result.Description,
            Find = result.FindWhat,
            ReplaceWith = result.ReplaceWith,
            Type = result.IsRegularExpression ? MultipleReplaceType.RegularExpression :
                result.IsCaseSensitive ? MultipleReplaceType.CaseSensitive :
                MultipleReplaceType.CaseInsensitive,
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
                if (item.SearchType == ReplaceExpression.SearchCaseSensitive)
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
                        if (r.IsMatch(string.Join("\n", newText.SplitToLines())))
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
                return string.Format(Se.Language.Edit.MultipleReplace.InvalidRegularExpressionX, exception.Message);
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

    public void RulesTreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var node = e.AddedItems.OfType<RuleTreeNode>().FirstOrDefault();
        IsEditPanelVisible = node is { IsCategory: false };
    }

    public void OnActiveChanged(object? sender, RoutedEventArgs e)
    {
        _dirty = true;
    }

    public void RuleTextChanged(object? sender, TextChangedEventArgs e)
    {
        _dirty = true;
    }

    public void RuleComboBoxChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not MultipleReplaceTypeItem newType)
        {
            return;
        }

        var node = SelectedNode;
        if (node is null)
        {
            return;
        }

        node.Type = newType.Type;

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

    internal void TreeViewDoubleTapped(TappedEventArgs e)
    {
        var node = SelectedNode;
        if (node is null)
        {
            return;
        }

        if (!node.IsCategory)
        {
            _ = NodeEdit(node);
        }
    }
}
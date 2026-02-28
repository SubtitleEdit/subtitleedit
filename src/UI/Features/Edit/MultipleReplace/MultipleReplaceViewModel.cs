using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
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

    // Expose a dedicated list property for the ComboBox ItemsSource
    public ObservableCollection<MultipleReplaceTypeItem> RuleTypes { get; }

    // Selected rule type for the ComboBox (two-way bound)
    [ObservableProperty] private MultipleReplaceTypeItem? _selectedRuleType;

    public ObservableCollection<RuleTreeNode> Nodes { get; }
    public TreeView RulesTreeView { get; internal set; }
    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public Subtitle FixedSubtitle { get; private set; }
    public int TotalReplaced { get; private set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private Subtitle _subtitle;
    private readonly Dictionary<string, Regex> _compiledRegExList;
    private readonly Timer _timerReplace;
    private bool _dirty;

    public MultipleReplaceViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;

        Fixes = new ObservableCollection<MultipleReplaceFix>();
        Nodes = new ObservableCollection<RuleTreeNode>(GetNodes());
        RulesTreeView = new TreeView();

        _compiledRegExList = new Dictionary<string, Regex>();

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

    private void TimerReplaceElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!_dirty)
        {
            return;
        }

        _timerReplace.Stop();
        _dirty = false;
        GeneratePreview();
        _timerReplace.Start();
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
                IsActive = category.IsActive
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
                    CommandParameter = node
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.General.MoveUp,
                    Command = CategoryMoveUpCommand,
                    CommandParameter = node
                },
                new MenuItem
                {
                    Header = Se.Language.General.MoveDown,
                    Command = CategoryMoveDownCommand,
                    CommandParameter = node
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
        if (node == null)
        {
            return;
        }

        var category = new RuleTreeNode(node, string.Empty, new ObservableCollection<RuleTreeNode>(), true);
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
            var rule = MakeRuleTreeNode(node, result);
            node.SubNodes?.Add(rule);

            Dispatcher.UIThread.Post(() =>
            {
                var allTreeViewItems = FindAllTreeViewItems(RulesTreeView);
                foreach (var item in allTreeViewItems)
                {
                    if (item.DataContext == node)
                    {
                        item.IsExpanded = true;
                        break;
                    }
                }
            }, DispatcherPriority.Background);

            SelectedNode = rule;
            _dirty = true;
        }
    }

    [RelayCommand]
    private void CategoryDelete(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        Nodes.Remove(node);
        AddDefaultCategoryIfNone();
        _dirty = true;
    }

    [RelayCommand]
    private async Task CategoryImport(RuleTreeNode? node)
    {
        if (node == null || Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Options.Settings.OpenRuleFile, "Replace rules", ".template");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        // import from json
        List<RuleTreeNode>? imported = null;
        try
        {
            var jsonOrXml = System.IO.File.ReadAllText(fileName);

            CategoryImportExportItem? temp = null;

            if (jsonOrXml.Contains("<MultipleSearchAndReplaceList>"))
            {
                temp = Se4XmlImporter.ImportFromXml(jsonOrXml);
            }
            else
            {
                temp = JsonSerializer.Deserialize<CategoryImportExportItem>(jsonOrXml, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        foreach (var profile in imported)
        {
            Nodes.Add(profile);
        }

        await MessageBox.Show(
            Window!,
            Se.Language.General.Information,
            string.Format(Se.Language.Options.Settings.RuleProfilesImportedX, imported.Count),
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
        .ShowDialogAsync<CategoryExportWindow, CategoryExportViewModel>(Window, vm =>
        {
            vm.Initialize(Nodes.ToList(), node);
        });

        if (!result.OkPressed)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".template", "SE_Replace_Rules", Se.Language.Options.Settings.SaveRuleProfilesFile);
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
        var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        System.IO.File.WriteAllText(fileName, json);

        _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window,
            vm =>
            {
                vm.Initialize(Se.Language.General.FileSaved,
                    string.Format(Se.Language.Options.Settings.RuleProfilesExportedX, toExport.Count), fileName, true, true);
            });
    }

    [RelayCommand]
    private void CategoryMoveUp(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var index = Nodes.IndexOf(node);
        if (index > 0)
        {
            Nodes.Move(index, index - 1);
            _dirty = true;
        }
    }

    [RelayCommand]
    private void CategoryMoveDown(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var index = Nodes.IndexOf(node);
        if (index < Nodes.Count - 1)
        {
            Nodes.Move(index, index + 1);
        }

        _dirty = true;
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
                    CommandParameter = node
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
                    CommandParameter = node
                },
                new MenuItem
                {
                    Header = Se.Language.General.MoveDown,
                    Command = NodeMoveDownCommand,
                    CommandParameter = node
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
    private void NodeDelete(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        if (node.Parent != null && node.Parent.SubNodes != null)
        {
            node.Parent.SubNodes.Remove(node);
        }

        _dirty = true;
    }

    [RelayCommand]
    private void NodeMoveUp(RuleTreeNode? node)
    {
        if (node == null || node.Parent == null || node.Parent.SubNodes == null)
        {
            return;
        }

        var nodes = node.Parent.SubNodes;
        var index = nodes.IndexOf(node);
        if (index > 0)
        {
            nodes.Move(index, index - 1);
            _dirty = true;
        }
    }

    [RelayCommand]
    private void NodeMoveDown(RuleTreeNode? node)
    {
        if (node == null || node.Parent == null || node.Parent.SubNodes == null)
        {
            return;
        }

        var nodes = node.Parent.SubNodes;
        var index = nodes.IndexOf(node);
        if (index < nodes.Count - 1)
        {
            nodes.Move(index, index + 1);
            _dirty = true;
        }
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }


    internal void RulesTreeView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            e.Handled = true;
            var node = SelectedNode;
            if (node != null && !node.IsCategory)
            {
                NodeDelete(node);
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

    public void ExpandAll()
    {
        if (RulesTreeView.ItemCount > 30)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var allTreeViewItems = FindAllTreeViewItems(RulesTreeView);
            foreach (var item in allTreeViewItems)
            {
                item.IsExpanded = true;
            }
        }, DispatcherPriority.Background);
    }

    private static IEnumerable<TreeViewItem> FindAllTreeViewItems(Control parent)
    {
        var result = new List<TreeViewItem>();
        if (parent is TreeViewItem tvi)
        {
            result.Add(tvi);
        }

        foreach (var child in parent.GetLogicalDescendants())
        {
            if (child is TreeViewItem treeViewItem)
            {
                result.Add(treeViewItem);
            }
        }

        return result;
    }

    private static RuleTreeNode MakeRuleTreeNode(RuleTreeNode node, EditRuleViewModel result)
    {
        return new RuleTreeNode(node.Parent, new MultipleReplaceRule
        {
            Active = node.IsActive,
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
        FixedSubtitle = new Subtitle(_subtitle, false);
        TotalReplaced = 0;
        var replaceExpressions = BuildReplaceExpressions();
        var fixes = new List<MultipleReplaceFix>();
        for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
        {
            var p = _subtitle.Paragraphs[i];
            var hit = false;
            var newText = p.Text;
            var ruleInfo = string.Empty;
            var ruleHits = new List<MultipleReplaceRule>();
            foreach (var item in replaceExpressions)
            {
                if (item.SearchType == ReplaceExpression.SearchCaseSensitive)
                {
                    if (newText.Contains(item.FindWhat))
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                        newText = newText.Replace(item.FindWhat, item.ReplaceWith);
                    }
                }
                else if (item.SearchType == ReplaceExpression.SearchRegEx)
                {
                    var r = _compiledRegExList[item.FindWhat];
                    if (r.IsMatch(newText))
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                        newText = RegexUtils.ReplaceNewLineSafe(r, newText, item.ReplaceWith);
                    }
                }
                else
                {
                    var index = newText.IndexOf(item.FindWhat, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
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
                };
                fixes.Add(fix);
                FixedSubtitle.Paragraphs[i].Text = newText;
            }
        }

        Dispatcher.UIThread.Post(() =>
        {
            Fixes.Clear();
            Fixes.AddRange(fixes);
        });

        //groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.MultipleReplace.LinesFoundX, fixedLines);
    }

    private HashSet<ReplaceExpression> BuildReplaceExpressions()
    {
        var replaceExpressions = new HashSet<ReplaceExpression>();
        foreach (var group in Nodes.Where(p => p.IsActive && p.SubNodes != null))
        {
            foreach (var rule in group.SubNodes!.Where(p => p.IsActive))
            {
                var findWhat = rule.Find;
                if (!string.IsNullOrEmpty(findWhat)) // allow space or spaces
                {
                    var replaceWith = RegexUtils.FixNewLine(rule.ReplaceWith);
                    findWhat = RegexUtils.FixNewLine(findWhat);
                    if (group.SubNodes != null)
                    {
                        var ruleInfo = string.IsNullOrEmpty(rule.Description)
                            ? $"Group name: {group.CategoryName} - Rule number: {group.SubNodes.IndexOf(rule) + 1}"
                            : $"Group name: {group.CategoryName} - Rule number: {group.SubNodes.IndexOf(rule) + 1}. {rule.Description}";
                        var mpi = new ReplaceExpression(findWhat, replaceWith, rule.SearchType, ruleInfo);
                        replaceExpressions.Add(mpi);
                        if (mpi.SearchType == ReplaceExpression.SearchRegEx && !_compiledRegExList.ContainsKey(findWhat))
                        {
                            _compiledRegExList.Add(findWhat,
                                new Regex(findWhat, RegexOptions.Compiled | RegexOptions.Multiline));
                        }
                    }
                }
            }
        }

        return replaceExpressions;
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
        SaveSettings();
        UiUtil.SaveWindowPosition(Window);
    }

    internal void OnLoaded()
    {
        ExpandAll();
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
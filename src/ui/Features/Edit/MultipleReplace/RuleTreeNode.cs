using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public partial class RuleTreeNode : ObservableObject
{
    [ObservableProperty] private string _categoryName;
    public ObservableCollection<RuleTreeNode>? SubNodes { get; }
    [ObservableProperty] private string _find;
    [ObservableProperty] private string _replaceWith;
    [ObservableProperty] private string _description;
    [ObservableProperty] private bool _isActive = false;
    [ObservableProperty] private bool _isCategory = false;
    [ObservableProperty] private string _iconName;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private bool _wholeWord;

    /// <summary>
    /// True while the rule's row in the rules list shows text boxes instead of text.
    /// </summary>
    [ObservableProperty] private bool _isEditing;

    /// <summary>
    /// The text box the row editor puts the caret in: Find, ReplaceWith or Description.
    /// </summary>
    public string EditField { get; set; } = nameof(Find);

    /// <summary>
    /// Asks the row editor's text box for <see cref="EditField"/> to take focus.
    /// </summary>
    public event System.Action? EditFocusRequested;

    public void RequestEditFocus() => EditFocusRequested?.Invoke();

    partial void OnIsEditingChanged(bool value)
    {
        if (value)
        {
            RequestEditFocus();
        }
    }

    /// <summary>
    /// The rule's place in its category, 1-based - the order the rules run in.
    /// </summary>
    [ObservableProperty] private int _number;

    /// <summary>
    /// How many rules a category holds, for the category list.
    /// </summary>
    public int RuleCount => SubNodes?.Count ?? 0;

    /// <summary>
    /// Why this rule cannot run - currently only a regular expression that will not compile.
    /// Set by the preview, shown as a warning marker on the row.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    private MultipleReplaceType _type;
    public MultipleReplaceType Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                UpdateIconName();
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(SearchType));
                OnPropertyChanged(nameof(CanUseWholeWord));
            }
        }
    }

    public RuleTreeNode? Parent { get; set; }

    // What a screen reader announces for a row of the category list (#12087).
    public override string ToString() => IsCategory ? CategoryName : Find;

    /// <summary>
    /// "Whole word" applies to the plain text types only - a regular expression uses \b itself.
    /// </summary>
    public bool CanUseWholeWord => Type != MultipleReplaceType.RegularExpression;

    /// <summary>
    /// True when the rule should only match whole words when it runs.
    /// </summary>
    public bool IsWholeWordActive => WholeWord && CanUseWholeWord;
    
    public string SearchType 
    {
        get
        {
            return Type switch
            {
                MultipleReplaceType.RegularExpression => "RegularExpression",
                MultipleReplaceType.CaseInsensitive => "Normal",
                MultipleReplaceType.CaseSensitive => "CaseSensitive",
                _ => "Unknown"
            };
        }
    }

    public RuleTreeNode(bool isCategory)
    {
        CategoryName = string.Empty;
        Find = string.Empty;
        ReplaceWith = string.Empty;
        Description = string.Empty;
        IsActive = true;
        IsCategory = isCategory;
        IconName = string.Empty;        
        if (isCategory)
        {
            SubNodes = new ObservableCollection<RuleTreeNode>();
            SubNodes.CollectionChanged += (_, _) => OnPropertyChanged(nameof(RuleCount));
        }
    }

    public RuleTreeNode(RuleTreeNode? parent, MultipleReplaceRule rule)
    {
        CategoryName = string.Empty;
        Find = rule.Find;
        ReplaceWith = rule.ReplaceWith;
        Description = rule.Description;
        IsActive = rule.Active;
        IsCategory = false;
        _type = rule.Type;
        _wholeWord = rule.WholeWord;
        IconName = string.Empty;
        Parent = parent;
        UpdateIconName();
    }

    private void UpdateIconName()
    {
        IconName = GetIconName(Type);
    }

    /// <summary>
    /// The icon the tree shows for a rule type - also used by lists that show plain rules.
    /// </summary>
    internal static string GetIconName(MultipleReplaceType type)
    {
        return type switch
        {
            MultipleReplaceType.RegularExpression => IconNames.Regex,
            MultipleReplaceType.CaseSensitive => IconNames.CaseSensitiveAlt,
            _ => IconNames.FindReplace,
        };
    }

    public RuleTreeNode(RuleTreeNode? parent, string categoryName, ObservableCollection<RuleTreeNode> subNodes, bool active)
    {
        CategoryName = categoryName;
        SubNodes = subNodes;
        SubNodes.CollectionChanged += (_, _) => OnPropertyChanged(nameof(RuleCount));
        Find = string.Empty;
        ReplaceWith = string.Empty;
        Description = string.Empty;
        IconName = string.Empty;
        IsActive = active;
        IsCategory = true;
        Parent = parent;
    }
}
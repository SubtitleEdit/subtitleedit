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
            }
        }
    }

    public RuleTreeNode? Parent { get; set; }
    
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
        IconName = string.Empty;
        Parent = parent;
        UpdateIconName();
    }

    private void UpdateIconName()
    {
        if (Type == MultipleReplaceType.RegularExpression)
        {
            IconName = IconNames.Regex;
        }
        else if (Type == MultipleReplaceType.CaseInsensitive)
        {
            IconName = IconNames.FindReplace;
        }
        else if (Type == MultipleReplaceType.CaseSensitive)
        {
            IconName =  IconNames.CaseSensitiveAlt;
        }
    }

    public RuleTreeNode(RuleTreeNode? parent, string categoryName, ObservableCollection<RuleTreeNode> subNodes, bool active)
    {
        CategoryName = categoryName;
        SubNodes = subNodes;
        Find = string.Empty;
        ReplaceWith = string.Empty;
        Description = string.Empty;
        IconName = string.Empty;
        IsActive = active;
        IsCategory = true;
        Parent = parent;
    }
}
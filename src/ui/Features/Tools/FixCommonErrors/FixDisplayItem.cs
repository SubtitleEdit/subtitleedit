using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class FixDisplayItem : ObservableObject
{
    // Internal identifier used to preserve selection/apply behavior for fixes that share the
    // same visible label but must remain distinct entries.
    [ObservableProperty] private string _action;

    [ObservableProperty] private string _actionDisplay;

    [ObservableProperty] private string _before;

    [ObservableProperty] private string _after;

    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private int _number;
    public Paragraph Paragraph { get; set; }

    /// <summary>Name of the rule (step 1 display item) that produced this fix.</summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>The rule's example text from step 1, shown in the rule-details popup.</summary>
    public string RuleExample { get; set; } = string.Empty;

    public FixDisplayItem(Paragraph p, int number, string action, string before, string after, bool isChecked)
    {
        Paragraph = p;
        Number = number;
        Action = action;
        ActionDisplay = FixActionKey.GetDisplay(action);
        Before = before;
        After = after;
        IsSelected = isChecked;
    }
}

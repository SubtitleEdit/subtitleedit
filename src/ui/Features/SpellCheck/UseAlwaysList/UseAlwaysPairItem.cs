using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.SpellCheck.UseAlwaysList;

public partial class UseAlwaysPairItem : ObservableObject
{
    [ObservableProperty] private string _from;
    [ObservableProperty] private string _to;

    // The "from" word is spelled correctly (or is a name / user word), so spell check never
    // flags it and the pair can never be applied. (#15767)
    [ObservableProperty] private bool _isUnused;

    public UseAlwaysPairItem(string from, string to)
    {
        _from = from;
        _to = to;
    }

    public override string ToString() => $"{From} -> {To}";
}

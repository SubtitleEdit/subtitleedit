using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public partial class EditRuleViewModel : ObservableObject
{
    [ObservableProperty] private string _findWhat;
    [ObservableProperty] private string _replaceWith;
    [ObservableProperty] private string _description;
    [ObservableProperty] private bool _isRegularExpression;
    [ObservableProperty] private bool _isCaseSensitive;
    [ObservableProperty] private bool _isCaseInsensitive;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public string? Title { get; internal set; }

    public EditRuleViewModel()
    {
        FindWhat = string.Empty;
        ReplaceWith = string.Empty;
        Description = string.Empty;
        IsRegularExpression = false;
        IsCaseSensitive = false;
        IsCaseInsensitive = false;
    }

    public void Initialize(string title, RuleTreeNode node)
    {
        Title = title;
        FindWhat = node.Find;
        ReplaceWith = node.ReplaceWith;
        Description = node.Description;
        if (node.Type == MultipleReplaceType.RegularExpression)
        {
            IsRegularExpression = true;
        }
        else if (node.Type == MultipleReplaceType.CaseSensitive)
        {
            IsCaseSensitive = true;
        }
        else
        {
            IsCaseInsensitive = true;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            e.Handled = true;
            Ok();
        }
    }
}
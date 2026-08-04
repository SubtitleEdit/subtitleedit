using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public partial class HearingImpairedRuleSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _isBracketsOn;
    [ObservableProperty] private bool _isCurlyBracketsOn;
    [ObservableProperty] private bool _isParenthesesOn;
    [ObservableProperty] private bool _isCustomOn;
    [ObservableProperty] private string _customText;
    [ObservableProperty] private bool _isTextBeforeColonOn;
    [ObservableProperty] private bool _isUppercaseLineOn;
    [ObservableProperty] private bool _isLineContainsOn;
    [ObservableProperty] private bool _isMusicSymbolsOn;
    [ObservableProperty] private bool _isInterjectionsOn;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public HearingImpairedRuleSettingsViewModel()
    {
        CustomText = string.Empty;
    }

    public void Initialize(HearingImpairedRuleOptions options, string customStart, string customEnd)
    {
        IsBracketsOn = options.Brackets;
        IsCurlyBracketsOn = options.CurlyBrackets;
        IsParenthesesOn = options.Parentheses;
        IsCustomOn = options.Custom;
        IsTextBeforeColonOn = options.TextBeforeColon;
        IsUppercaseLineOn = options.UppercaseLine;
        IsLineContainsOn = options.LineContains;
        IsMusicSymbolsOn = options.MusicSymbols;
        IsInterjectionsOn = options.Interjections;

        // The delimiters themselves belong to "Remove text for hearing impaired" - show them so it
        // is clear what the custom option will match here.
        CustomText = $"{customStart}...{customEnd}";
    }

    public HearingImpairedRuleOptions GetOptions()
    {
        return new HearingImpairedRuleOptions
        {
            Brackets = IsBracketsOn,
            CurlyBrackets = IsCurlyBracketsOn,
            Parentheses = IsParenthesesOn,
            Custom = IsCustomOn,
            TextBeforeColon = IsTextBeforeColonOn,
            UppercaseLine = IsUppercaseLineOn,
            LineContains = IsLineContainsOn,
            MusicSymbols = IsMusicSymbolsOn,
            Interjections = IsInterjectionsOn,
        };
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/modify-selection");
        }
    }
}

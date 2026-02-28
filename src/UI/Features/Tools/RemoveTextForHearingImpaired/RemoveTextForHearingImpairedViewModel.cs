using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

public partial class RemoveTextForHearingImpairedViewModel : ObservableObject
{
    public class LanguageItem
    {
        public string Code { get; }
        public string Name { get; }

        public LanguageItem(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public static List<LanguageItem> GetAll()
        {
            return Iso639Dash2LanguageCode.List
                .Select(p => new LanguageItem(p.TwoLetterCode, p.EnglishName))
                .OrderBy(p => p.Name)
                .ToList();
        }
    }

    [ObservableProperty] private bool _isRemoveBracketsOn;
    [ObservableProperty] private bool _isRemoveCurlyBracketsOn;
    [ObservableProperty] private bool _isRemoveParenthesesOn;
    [ObservableProperty] private bool _isRemoveCustomOn;
    [ObservableProperty] private string _customStart;
    [ObservableProperty] private string _customEnd;
    [ObservableProperty] private bool _isOnlySeparateLine;
    [ObservableProperty] private bool _isRemoveTextBeforeColonOn;
    [ObservableProperty] private bool _isRemoveTextBeforeColonUppercaseOn;
    [ObservableProperty] private bool _isRemoveTextBeforeColonSeparateLineOn;
    [ObservableProperty] private bool _isRemoveTextUppercaseLineOn;
    [ObservableProperty] private bool _isRemoveTextContainsOn;
    [ObservableProperty] private string _textContains;
    [ObservableProperty] private bool _isRemoveOnlyMusicSymbolsOn;
    [ObservableProperty] private bool _isRemoveInterjectionsOn;
    [ObservableProperty] private bool _isInterjectionsSeparateLineOn;
    [ObservableProperty] private DisplayFile? _selectedFile;
    [ObservableProperty] private ObservableCollection<LanguageItem> _languages;
    [ObservableProperty] private LanguageItem? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<RemoveItem> _fixes;
    [ObservableProperty] private RemoveItem? _selectedFix;
    [ObservableProperty] private string _fixText;
    [ObservableProperty] private bool _fixTextEnabled;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Subtitle FixedSubtitle { get; private set; }

    private Subtitle _subtitle;
    private RemoveTextForHI? _removeTextForHiLib;
    private readonly Timer _timer;
    private readonly List<Paragraph> _edited;

    private readonly IWindowService _windowService;

    public RemoveTextForHearingImpairedViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        CustomStart = "?";
        CustomEnd = "?";
        TextContains = string.Empty;
        Languages = new ObservableCollection<LanguageItem>(LanguageItem.GetAll());
        Fixes = new ObservableCollection<RemoveItem>();
        FixText = string.Empty;
        _edited = new List<Paragraph>();
        _timer = new Timer(500);
        _timer.Elapsed += TimerElapsed;
        FixedSubtitle = new Subtitle();
        _subtitle = new Subtitle();
    }

    public void Initialize(Subtitle subtitle)
    {
        _subtitle = subtitle;
        LoadSettings();
        _removeTextForHiLib = new RemoveTextForHI(GetSettings(_subtitle));
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.Tools.RemoveTextForHi;

        IsRemoveBracketsOn = settings.IsRemoveBracketsOn;
        IsRemoveCurlyBracketsOn = settings.IsRemoveCurlyBracketsOn;
        IsRemoveParenthesesOn = settings.IsRemoveParenthesesOn;
        IsRemoveCustomOn = settings.IsRemoveCustomOn;
        CustomStart = settings.CustomStart;
        CustomEnd = settings.CustomEnd;
        IsOnlySeparateLine = settings.IsOnlySeparateLine;

        IsRemoveTextBeforeColonOn = settings.IsRemoveTextBeforeColonOn;
        IsRemoveTextBeforeColonUppercaseOn = settings.IsRemoveTextBeforeColonUppercaseOn;
        IsRemoveTextBeforeColonSeparateLineOn = settings.IsRemoveTextBeforeColonSeparateLineOn;

        IsRemoveTextUppercaseLineOn = settings.IsRemoveTextUppercaseLineOn;

        IsRemoveTextContainsOn = settings.IsRemoveTextContainsOn;
        TextContains = settings.TextContains;

        IsRemoveOnlyMusicSymbolsOn = settings.IsRemoveOnlyMusicSymbolsOn;

        IsRemoveInterjectionsOn = settings.IsRemoveInterjectionsOn;
        IsInterjectionsSeparateLineOn = settings.IsInterjectionsSeparateLineOn;
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Tools.RemoveTextForHi;

        settings.IsRemoveBracketsOn = IsRemoveBracketsOn;
        settings.IsRemoveCurlyBracketsOn = IsRemoveCurlyBracketsOn;
        settings.IsRemoveParenthesesOn = IsRemoveParenthesesOn;
        settings.IsRemoveCustomOn = IsRemoveCustomOn;
        settings.CustomStart = CustomStart;
        settings.CustomEnd = CustomEnd;
        settings.IsOnlySeparateLine = IsOnlySeparateLine;

        settings.IsRemoveTextBeforeColonOn = IsRemoveTextBeforeColonOn;
        settings.IsRemoveTextBeforeColonUppercaseOn = IsRemoveTextBeforeColonUppercaseOn;
        settings.IsRemoveTextBeforeColonSeparateLineOn = IsRemoveTextBeforeColonSeparateLineOn;

        settings.IsRemoveTextUppercaseLineOn = IsRemoveTextUppercaseLineOn;

        settings.IsRemoveTextContainsOn = IsRemoveTextContainsOn;
        settings.TextContains = TextContains;

        settings.IsRemoveOnlyMusicSymbolsOn = IsRemoveOnlyMusicSymbolsOn;

        settings.IsRemoveInterjectionsOn = IsRemoveInterjectionsOn;
        settings.IsInterjectionsSeparateLineOn = IsInterjectionsSeparateLineOn;

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        OkPressed = true;
        FixedSubtitle = new Subtitle(_subtitle, false);
        FixedSubtitle.Paragraphs.Clear();
        for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
        {
            var p = _subtitle.Paragraphs[index];
            var fixedParagraph = Fixes.FirstOrDefault(ri => ri.Index == index);
            if (fixedParagraph is { Apply: true })
            {
                p.Text = fixedParagraph.After;
            }

            FixedSubtitle.Paragraphs.Add(p);
        }

        FixedSubtitle.RemoveEmptyLines();

        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task EditInterjections()
    {
        await _windowService.ShowDialogAsync<InterjectionsWindow, InterjectionsViewModel>(Window!,
            vm => { vm.Initialize(SelectedLanguage); });
    }

    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _timer.Stop();

        try
        {
            Dispatcher.UIThread.Invoke(GeneratePreview);
        }
        catch
        {
            return;
        }

        _timer.Start();
    }

    private void GeneratePreview()
    {
        if (_removeTextForHiLib == null)
        {
            return;
        }

        _removeTextForHiLib.Settings = GetSettings(_subtitle);
        _removeTextForHiLib.Warnings = [];

        _removeTextForHiLib.ReloadInterjection(SelectedLanguage?.Code ?? "en");

        var count = 0;
        var newFixes = new List<RemoveItem>();
        for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
        {
            var p = _subtitle.Paragraphs[index];
            _removeTextForHiLib.WarningIndex = index - 1;
            if (_edited.Contains(p))
            {
                var editedParagraph = _edited.First(x => x.Id == p.Id);
                var newText = editedParagraph.Text;

                var apply = true;
                var oldItem = Fixes.FirstOrDefault(f => f.Index == index);
                if (oldItem != null)
                {
                    apply = oldItem.Apply;
                }

                var item = new RemoveItem(apply, index, p.Text, newText, p);
                newFixes.Add(item);
                count++;
            }
            else
            {
                var newText = _removeTextForHiLib.RemoveTextFromHearImpaired(p.Text, _subtitle, index,
                    SelectedLanguage == null ? "en" : SelectedLanguage.Code);
                if (p.Text.RemoveChar(' ') != newText.RemoveChar(' '))
                {
                    var apply = true;
                    var oldItem = Fixes.FirstOrDefault(f => f.Index == index);
                    if (oldItem != null)
                    {
                        apply = oldItem.Apply;
                    }

                    var item = new RemoveItem(apply, index, p.Text, newText, p);
                    newFixes.Add(item);
                    count++;
                }
            }
        }

        if (newFixes.Count == Fixes.Count)
        {
            var same = true;
            for (var i = 0; i < newFixes.Count; i++)
            {
                if (newFixes[i].Index != Fixes[i].Index ||
                    newFixes[i].Before != Fixes[i].Before ||
                    newFixes[i].After != Fixes[i].After)
                {
                    same = false;
                    break;
                }
            }

            if (same)
            {
                return; // no changes
            }
        }

        Fixes.Clear();
        Fixes.AddRange(newFixes);

        //groupBoxLinesFound.Text = string.Format(_language.LinesFoundX, count);
    }

    public RemoveTextForHISettings GetSettings(Subtitle subtitle)
    {
        var textContainsList = TextContains.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim()).ToList();

        var settings = new RemoveTextForHISettings(subtitle)
        {
            OnlyIfInSeparateLine = IsOnlySeparateLine,
            RemoveIfAllUppercase = IsRemoveTextUppercaseLineOn,
            RemoveTextBeforeColon = IsRemoveTextBeforeColonOn,
            RemoveTextBeforeColonOnlyUppercase = IsRemoveTextBeforeColonUppercaseOn,
            ColonSeparateLine = IsRemoveTextBeforeColonSeparateLineOn,
            RemoveWhereContains = IsRemoveTextContainsOn,
            RemoveIfTextContains = textContainsList,
            RemoveTextBetweenCustomTags = IsRemoveCustomOn,
            RemoveInterjections = IsRemoveInterjectionsOn,
            RemoveInterjectionsOnlySeparateLine = IsRemoveInterjectionsOn && IsInterjectionsSeparateLineOn,
            RemoveTextBetweenSquares = IsRemoveBracketsOn,
            RemoveTextBetweenBrackets = IsRemoveCurlyBracketsOn,
            RemoveTextBetweenQuestionMarks = false,
            RemoveTextBetweenParentheses = IsRemoveParenthesesOn,
            RemoveIfOnlyMusicSymbols = IsRemoveOnlyMusicSymbolsOn,
            CustomStart = CustomStart,
            CustomEnd = CustomEnd,
        };

        foreach (var item in TextContains.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            settings.RemoveIfTextContains.Add(item.Trim());
        }

        return settings;
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
            UiUtil.ShowHelp("features/remove-text-hi");
        }
    }

    public void OnLoaded(RoutedEventArgs routedEventArgs)
    {
        var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(_subtitle) ?? "en";
        SelectedLanguage = Languages.FirstOrDefault(l => l.Code == languageCode) ??
            Languages.FirstOrDefault(l => l.Code == "en");
        
        _timer.Start();
    }
}
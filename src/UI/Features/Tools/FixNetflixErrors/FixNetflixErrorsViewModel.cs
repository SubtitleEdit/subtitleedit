using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.NetflixQualityCheck;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;

public partial class FixNetflixErrorsViewModel : ObservableObject
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

    [ObservableProperty] private DisplayFile? _selectedFile;
    [ObservableProperty] private ObservableCollection<LanguageItem> _languages;
    [ObservableProperty] private LanguageItem? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<FixNetflixErrorsItem> _fixes;
    [ObservableProperty] private FixNetflixErrorsItem? _selectedFix;
    [ObservableProperty] private string _fixText;
    [ObservableProperty] private bool _fixTextEnabled;

    // New: selectable list of Netflix checks
    [ObservableProperty] private ObservableCollection<NetflixCheckDisplayItem> _checks = new();

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Subtitle FixedSubtitle { get; private set; }

    private Subtitle _subtitle;
    private string _videoFileName;
    private readonly Timer _timer;
    private bool _dirty;
    private readonly List<Paragraph> _edited;

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private FfmpegMediaInfo2? _mediaInfo;

    public FixNetflixErrorsViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        Languages = new ObservableCollection<LanguageItem>(LanguageItem.GetAll());
        Fixes = new ObservableCollection<FixNetflixErrorsItem>();
        FixText = string.Empty;
        _edited = new List<Paragraph>();
        _timer = new Timer(500);
        _timer.Elapsed += TimerElapsed;
        FixedSubtitle = new Subtitle();
        _subtitle = new Subtitle();
        _videoFileName = string.Empty;
    }

    public void Initialize(Subtitle subtitle, string videoFileName)
    {
        _subtitle = subtitle;
        _videoFileName = videoFileName;

        _ = Task.Run(() =>
        {
            _mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
        });

        LoadSettings();
        LoadChecks();
        SetDirty();
    }

    private void LoadChecks()
    {
        Checks.Clear();
        foreach (var checker in NetflixQualityController.GetAllCheckers())
        {
            var name = GetFriendlyCheckerName(checker.GetType().Name);
            Checks.Add(new NetflixCheckDisplayItem(checker, name, true));
        }
    }

    private static string GetFriendlyCheckerName(string typeName)
    {
        // Remove common prefix
        if (typeName.StartsWith("NetflixCheck", StringComparison.Ordinal))
        {
            typeName = typeName.Substring("NetflixCheck".Length);
        }

        // Insert spaces before capitals
        var chars = new List<char>();
        for (int i = 0; i < typeName.Length; i++)
        {
            var c = typeName[i];
            if (i > 0 && char.IsUpper(c) && (char.IsLower(typeName[i - 1]) || (i + 1 < typeName.Length && char.IsLower(typeName[i + 1]))))
            {
                chars.Add(' ');
            }
            chars.Add(c);
        }
        return new string(chars.ToArray());
    }

    private void LoadSettings()
    {
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task GenerateReport()
    {
        if (Window == null || SelectedLanguage == null)
        {
            return;
        }

        if (Fixes.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Tools.NetflixCheckAndFix.NothingToReport, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var csvBuilder = new StringBuilder();

        // Header
        csvBuilder.AppendLine("LineNumber,TimeCode,Context,Comment");

        // Rows
        foreach (var fix in Fixes)
        {
            csvBuilder.AppendLine(fix.Record.ToCsvRow());
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".csv", "netflix_report.csv", Se.Language.Tools.NetflixCheckAndFix.SaveNetflixQualityReport);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        System.IO.File.WriteAllText(fileName, csvBuilder.ToString());

        _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.Tools.NetflixCheckAndFix.NetflixReportSaved, string.Format(Se.Language.Tools.NetflixCheckAndFix.NetFlixQualityReportSavedToX,  fileName), fileName, true, true);
        });
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
            if (fixedParagraph != null)
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
    private void ChecksSelectAll()
    {
        foreach (var c in Checks)
        {
            c.IsSelected = true;
        }
        SetDirty();
    }

    [RelayCommand]
    private void ChecksInverseSelection()
    {
        foreach (var c in Checks)
        {
            c.IsSelected = !c.IsSelected;
        }
        SetDirty();
    }

    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _timer.Stop();

        try
        {
            if (_dirty)
            {
                _dirty = false;
                Dispatcher.UIThread.Invoke(GeneratePreview);
            }
        }
        catch
        {
            return;
        }

        _timer.Start();
    }

    private void GeneratePreview()
    {
        // Build selected checks list
        var selectedChecks = Checks.Where(c => c.IsSelected).Select(c => c.Checker).ToList();
        Fixes.Clear();

        if (_subtitle.Paragraphs.Count == 0 || selectedChecks.Count == 0)
        {
            return;
        }

        var controller = new NetflixQualityController
        {
            Language = SelectedLanguage?.Code ?? "en",
            FrameRate = (double)(_mediaInfo?.FramesRate ?? (decimal)Configuration.Settings.General.CurrentFrameRate),
        };

        controller.RunChecks(_subtitle, selectedChecks);

        // Map paragraph to proposed text changes (ignore pure timing-only changes for now)
        var fixMap = new Dictionary<int, (string Before, string After, Paragraph P, NetflixQualityController.Record)>();
        foreach (var r in controller.Records)
        {
            if (r.OriginalParagraph == null)
            {
                continue;
            }

            var idx = _subtitle.Paragraphs.IndexOf(r.OriginalParagraph);
            if (idx < 0)
            {
                continue;
            }

            var before = r.OriginalParagraph.Text;
            var after = r.FixedParagraph?.Text;
            if (!string.IsNullOrEmpty(after) && !string.Equals(before, after, StringComparison.Ordinal))
            {
                // If multiple fixes affect the same paragraph, keep last suggestion
                fixMap[idx] = (before, after, r.OriginalParagraph, r);
            }
        }

        if (fixMap.Count == 0)
        {
            return;
        }

        foreach (var kvp in fixMap.OrderBy(k => k.Key))
        {
            var index = kvp.Key;
            var (before, after, p, r) = kvp.Value;
            var item = new FixNetflixErrorsItem(r.CanBeFixed, index, before, after, p, r);
            Fixes.Add(item);
        }
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        SetDirty();
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
            UiUtil.ShowHelp("features/fix-common-errors");
        }
    }

    public void OnLoaded(RoutedEventArgs routedEventArgs)
    {
        var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(_subtitle) ?? "en";
        SelectedLanguage = Languages.FirstOrDefault(l => l.Code == languageCode) ??
            Languages.FirstOrDefault(l => l.Code == "en");

        _timer.Start();

        if (Checks.Count == 0)
        {
            LoadChecks();
        }

        SetDirty();
    }

    public void SetDirty()
    {
        _dirty = true;
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class ProfileDisplay : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private int? _singleLineMaxLength;
    [ObservableProperty] private double? _optimalCharsPerSec;
    [ObservableProperty] private double? _maxCharsPerSec;
    [ObservableProperty] private double? _maxWordsPerMin;
    [ObservableProperty] private int? _minDurationMs;
    [ObservableProperty] private int? _maxDurationMs;
    [ObservableProperty] private int? _minGapMs;
    [ObservableProperty] private int? _maxLines;
    [ObservableProperty] private int? _unbreakLinesShorterThan;
    [ObservableProperty] private DialogStyleDisplay _dialogStyle;
    [ObservableProperty] private ContinuationStyleDisplay _continuationStyle;
    [ObservableProperty] private CpsLineLengthStrategyDisplay _cpsLineLengthStrategy;
    [ObservableProperty] private bool _isSelected;

    public ProfileDisplay()
    {
        Name = string.Empty;
        SingleLineMaxLength = null;
        OptimalCharsPerSec = null;
        MaxCharsPerSec = null;
        MaxWordsPerMin = null;
        MinDurationMs = null;
        MaxDurationMs = null;
        MinGapMs = null;
        MaxLines = null;
        UnbreakLinesShorterThan = null;
        DialogStyle = DialogStyleDisplay.List().First();
        ContinuationStyle = ContinuationStyleDisplay.List().First();
        CpsLineLengthStrategy = CpsLineLengthStrategyDisplay.List().First();
    }

    public ProfileDisplay(ProfileDisplay other)
    {
        Name = other.Name;
        SingleLineMaxLength = other.SingleLineMaxLength;
        OptimalCharsPerSec = other.OptimalCharsPerSec;
        MaxCharsPerSec = other.MaxCharsPerSec;
        MaxWordsPerMin = other.MaxWordsPerMin;
        MinDurationMs = other.MinDurationMs;
        MaxDurationMs = other.MaxDurationMs;
        MinGapMs = other.MinGapMs;
        MaxLines = other.MaxLines;
        UnbreakLinesShorterThan = other.UnbreakLinesShorterThan;
        DialogStyle = new DialogStyleDisplay(other.DialogStyle ?? DialogStyleDisplay.List().First());
        ContinuationStyle = new ContinuationStyleDisplay(other.ContinuationStyle ?? ContinuationStyleDisplay.List().First());
        CpsLineLengthStrategy = new CpsLineLengthStrategyDisplay(other.CpsLineLengthStrategy ?? CpsLineLengthStrategyDisplay.List().First());
    }

    public ProfileDisplay(
        RulesProfile other, 
        List<DialogStyleDisplay> dialogStyles,
        List<ContinuationStyleDisplay> continuationStyles,
        List<CpsLineLengthStrategyDisplay> cpsLineLengthStrategies)
    {
        Name = other.Name;
        SingleLineMaxLength = other.SubtitleLineMaximumLength;
        OptimalCharsPerSec = (double)other.SubtitleOptimalCharactersPerSeconds;
        MaxCharsPerSec = (double)other.SubtitleMaximumCharactersPerSeconds;
        MaxWordsPerMin = (double) other.SubtitleMaximumWordsPerMinute;
        MinDurationMs = other.SubtitleMinimumDisplayMilliseconds;
        MaxDurationMs = other.SubtitleMaximumDisplayMilliseconds;
        MinGapMs = other.MinimumMillisecondsBetweenLines;
        MaxLines = other.MaxNumberOfLines;
        UnbreakLinesShorterThan = other.MergeLinesShorterThan;
        DialogStyle = dialogStyles.FirstOrDefault(p=>p.Code == other.DialogStyle.ToString()) ?? dialogStyles.First();
        ContinuationStyle = continuationStyles.FirstOrDefault(p=>p.Code == other.ContinuationStyle.ToString()) ?? continuationStyles.First();
        CpsLineLengthStrategy = cpsLineLengthStrategies.FirstOrDefault(p=>p.Code == other.CpsLineLengthStrategy.ToString()) ?? cpsLineLengthStrategies.First();
    }

    public RulesProfile ToRulesProfile()
    {
        var s = new SeGeneral();
        return new RulesProfile
        {
            Name = Name,
            SubtitleLineMaximumLength = SingleLineMaxLength ?? s.SubtitleLineMaximumLength,
            SubtitleOptimalCharactersPerSeconds = (decimal)(OptimalCharsPerSec ?? s.SubtitleOptimalCharactersPerSeconds),
            SubtitleMaximumCharactersPerSeconds = (decimal)(MaxCharsPerSec ?? s.SubtitleMaximumCharactersPerSeconds),
            SubtitleMaximumWordsPerMinute = (decimal)(MaxWordsPerMin ?? s.SubtitleMaximumWordsPerMinute),
            SubtitleMinimumDisplayMilliseconds = MinDurationMs ?? s.SubtitleMinimumDisplayMilliseconds,
            SubtitleMaximumDisplayMilliseconds = MaxDurationMs ?? s.SubtitleMaximumDisplayMilliseconds,
            MinimumMillisecondsBetweenLines = MinGapMs ?? s.MinimumMillisecondsBetweenLines,
            MaxNumberOfLines = MaxLines ?? s.MaxNumberOfLines,
            MergeLinesShorterThan = UnbreakLinesShorterThan ?? s.UnbreakLinesShorterThan,
            DialogStyle = Enum.Parse<DialogType>(DialogStyle.Code),
            ContinuationStyle = Enum.Parse<ContinuationStyle>(ContinuationStyle.Code),
            CpsLineLengthStrategy = CpsLineLengthStrategy.Code,
        };
    }

    override public string ToString()
    {
        return Name;
    }
}

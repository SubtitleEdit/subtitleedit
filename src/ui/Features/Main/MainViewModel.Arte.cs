using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;
using System.Linq;
using System.Threading.Tasks;

using Nikse.SubtitleEdit.Logic.Config;
namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel
{
    // Working ARTE/Teletext values for the current editor session.
    // The EBU STL hard row limit remains 40 cells.
    private int _workingTeletextMaxCells = 37;

    public int WorkingTeletextMaxCells
    {
        get => _workingTeletextMaxCells;
        set => SetWorkingTeletextMaxCells(value);
    }
    public double WorkingReadingDurationTolerancePercent { get; private set; } = 15.0;
    public bool WorkingAcceptShortDurations { get; private set; }
    public int WorkingShortMinimumFrames { get; private set; } = 18;

    private void SetWorkingTeletextMaxCells(int value)
    {
        value = System.Math.Max(1, value);
        if (_workingTeletextMaxCells == value)
        {
            return;
        }

        _workingTeletextMaxCells = value;

        // Keep the normal subtitle-grid Teletext validation on the same
        // working limit as Flow and the ARTE checker.
        SubtitleLineViewModel.TeletextMaxCharacters = value;

        OnPropertyChanged(nameof(WorkingTeletextMaxCells));
    }

    public int WorkingMinimumGapFrames =>
        Se.Settings.General.MinimumBetweenLines.Frames;

    public int ArteRunChecksGeneration { get; private set; }

    public void RequestFlowForArteRunChecks()
    {
        ArteRunChecksGeneration++;
        OnPropertyChanged(nameof(ArteRunChecksGeneration));
    }

    public bool IsWorkingArtePresetActive =>
        WorkingTeletextMaxCells == 37 &&
        WorkingMinimumGapFrames == 5 &&
        System.Math.Abs(WorkingReadingDurationTolerancePercent - 15.0) < 0.001 &&
        !WorkingAcceptShortDurations &&
        WorkingShortMinimumFrames == 18;

    public void ApplyWorkingArtePreset()
    {
        WorkingTeletextMaxCells = 37;

        var minimumGap = Se.Settings.General.MinimumBetweenLines;
        minimumGap.Frames = 5;
        minimumGap.Milliseconds = 200;
        Se.SaveSettings();

        WorkingReadingDurationTolerancePercent = 15.0;
        WorkingAcceptShortDurations = false;
        WorkingShortMinimumFrames = 18;

        OnPropertyChanged(nameof(WorkingMinimumGapFrames));
        OnPropertyChanged(nameof(IsWorkingArtePresetActive));
    }

    public void GoToSubtitleNumber(int number)
    {
        if (number < 1)
        {
            return;
        }

        var row = Subtitles.FirstOrDefault(
            p => !p.IsReferenceOnly && p.Number == number);

        var index = row != null
            ? Subtitles.IndexOf(row)
            : number - 1;

        if (index < 0 || index >= Subtitles.Count)
        {
            return;
        }

        SelectAndScrollToRow(index);
    }

    [RelayCommand]
    private async Task ShowToolsCheckArteErrors()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var selectedIndex = SelectedSubtitleIndex ?? 0;

        // Analysis is detached; each correction/undo publishes a complete snapshot while
        // the modal review window stays open. Closing must not apply the same changes twice.
        var subtitle = GetUpdateSubtitleWithRowMap(out _);
        var viewModel = new CheckArteErrorsViewModel(_windowService, _fileHelper);

        viewModel.InitializeWorkingSettings(
            WorkingTeletextMaxCells,
            WorkingReadingDurationTolerancePercent,
            WorkingAcceptShortDurations,
            WorkingShortMinimumFrames);

        viewModel.RunChecksStarted = RequestFlowForArteRunChecks;

        viewModel.WorkingSettingsChanged = (
            maxCells,
            tolerancePercent,
            acceptShort,
            shortMinimumFrames) =>
        {
            SetWorkingTeletextMaxCells(maxCells);
            WorkingReadingDurationTolerancePercent = tolerancePercent;
            WorkingAcceptShortDurations = acceptShort;
            WorkingShortMinimumFrames = shortMinimumFrames;
            OnPropertyChanged(nameof(IsWorkingArtePresetActive));
        };

        viewModel.Initialize(subtitle);
        viewModel.ApplyToMainSubtitle = corrected =>
        {
            GetUpdateSubtitleWithRowMap(out var currentRows);
            _subtitle.Header = corrected.Header;
            var targetFormat = Ebu.IsStlHeader(corrected.Header)
                ? SubtitleFormats.FirstOrDefault(format => format is Ebu) ?? new Ebu()
                : SelectedSubtitleFormat;
            if (targetFormat is Ebu)
            {
                SetSubtitleFormat(targetFormat);
                _subtitle.OriginalFormat = targetFormat;
            }
            ApplyFixedSubtitle(corrected, currentRows, selectedIndex, targetFormat);
        };
        viewModel.OpenEbuOptionsDialog = async (reportEntries, reportErrorCount) =>
        {
            if (!await ShowEbuOptionsDialog(reportEntries, reportErrorCount))
            {
                return null;
            }

            return new Subtitle(GetUpdateSubtitleWithRowMap(out _), generateNewId: false);
        };

        var window = new CheckArteErrorsWindow(viewModel);
        await window.ShowDialog(Window);
        viewModel.ApplyToMainSubtitle = null;
        viewModel.OpenEbuOptionsDialog = null;

        if (viewModel.OkPressed)
        {
            ShowStatus(string.Format(Se.Language.Tools.CheckArteErrors.CorrectionsAppliedX, viewModel.AppliedFixCount));
        }
    }
}

using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryMoveCaptions;

public enum MoveCaptionsMode
{
    /// <summary>Move captions into the letterbox bars (above/below the picture).</summary>
    IntoBars,

    /// <summary>Move captions out of the letterbox bars, inside the active picture.</summary>
    IntoPicture,
}

/// <summary>
/// BDSup2Sub's "move all captions": for letterboxed (cinemascope) video, push every caption
/// either into the black bars or back inside the active picture, keeping a fixed offset from
/// the bar edge. A caption whose centre is in the upper half of the screen goes to the top
/// edge, the rest go to the bottom edge - the same rule BDSup2Sub uses.
/// </summary>
public partial class BinaryMoveCaptionsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<LetterboxRatioItem> _letterboxRatios;
    [ObservableProperty] private LetterboxRatioItem _selectedLetterboxRatio;
    [ObservableProperty] private int _barHeight;
    [ObservableProperty] private bool _isBarHeightEditable;
    [ObservableProperty] private bool _moveIntoBars;
    [ObservableProperty] private bool _moveIntoPicture;
    [ObservableProperty] private int _offset;
    [ObservableProperty] private string _infoText;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private List<BinarySubtitleItem> _subtitles = new();
    private int _screenWidth;
    private int _screenHeight;
    private bool _isSyncingRatio;

    public BinaryMoveCaptionsViewModel()
    {
        var lang = Se.Language.Tools.ImageBasedEdit;
        _letterboxRatios = new ObservableCollection<LetterboxRatioItem>
        {
            new("1.66:1", 1.66, false, "1.66"),
            new("1.85:1", 1.85, false, "1.85"),
            new("2.00:1", 2.00, false, "2.00"),
            new("2.20:1", 2.20, false, "2.20"),
            new("2.35:1", 2.35, false, "2.35"),
            new("2.39:1", 2.39, false, "2.39"),
            new("2.40:1", 2.40, false, "2.40"),
            new(lang.LetterboxCustom, null, true, "custom"),
        };
        _selectedLetterboxRatio = _letterboxRatios[5];
        _moveIntoBars = true;
        _offset = 10;
        _infoText = string.Empty;
    }

    /// <summary>
    /// <paramref name="currentRatioKey"/> and <paramref name="currentBarHeight"/> come from the
    /// main window's position monitor so the dialog opens on the letterbox the user already set up.
    /// </summary>
    public void Initialize(List<BinarySubtitleItem> subtitles, int screenWidth, int screenHeight, string? currentRatioKey, int currentBarHeight)
    {
        _subtitles = subtitles;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        var preset = LetterboxRatios.FirstOrDefault(r => r.SettingsKey == currentRatioKey);
        if (preset != null)
        {
            SelectedLetterboxRatio = preset;
            if (preset.IsCustom)
            {
                BarHeight = Math.Max(0, currentBarHeight);
            }
        }
        else
        {
            // Screen size was unknown in the constructor - recompute the preset's bar height now.
            OnSelectedLetterboxRatioChanged(SelectedLetterboxRatio);
        }

        UpdateInfo();
    }

    partial void OnSelectedLetterboxRatioChanged(LetterboxRatioItem value)
    {
        if (value == null)
        {
            return;
        }

        IsBarHeightEditable = value.IsCustom;
        if (!value.IsCustom && value.Ratio.HasValue)
        {
            _isSyncingRatio = true;
            BarHeight = PositionMonitorLogic.CalculateBarHeight(_screenWidth, _screenHeight, value.Ratio.Value);
            _isSyncingRatio = false;
        }

        UpdateInfo();
    }

    partial void OnBarHeightChanged(int value)
    {
        if (!_isSyncingRatio)
        {
            UpdateInfo();
        }
    }

    partial void OnOffsetChanged(int value) => UpdateInfo();
    partial void OnMoveIntoBarsChanged(bool value) => UpdateInfo();
    partial void OnMoveIntoPictureChanged(bool value) => UpdateInfo();

    private MoveCaptionsMode Mode => MoveIntoPicture ? MoveCaptionsMode.IntoPicture : MoveCaptionsMode.IntoBars;

    private void UpdateInfo()
    {
        var lang = Se.Language.Tools.ImageBasedEdit;
        var moving = _subtitles.Count(s => GetNewY(s, _screenHeight, BarHeight, Offset, Mode) != s.Y);
        InfoText = string.Format(lang.LetterboxBarHeightXPxYOfZCaptionsWillMove, BarHeight, moving, _subtitles.Count);
    }

    /// <summary>
    /// Computes the new Y for one caption. Captions centred in the upper half go to the top
    /// edge, the rest to the bottom edge. The result is clamped so the image stays on screen.
    /// </summary>
    public static int GetNewY(BinarySubtitleItem item, int screenHeight, int barHeight, int offset, MoveCaptionsMode mode)
    {
        var height = item.Bitmap?.PixelSize.Height ?? 0;
        if (screenHeight <= 0 || height <= 0)
        {
            return item.Y;
        }

        var isTop = item.Y + height / 2.0 < screenHeight / 2.0;
        int y;
        if (mode == MoveCaptionsMode.IntoBars)
        {
            // Sit against the outer screen edge, "offset" pixels in.
            y = isTop ? offset : screenHeight - height - offset;
        }
        else
        {
            // Sit against the picture edge, "offset" pixels inside the active picture.
            y = isTop ? barHeight + offset : screenHeight - barHeight - height - offset;
        }

        return Math.Clamp(y, 0, Math.Max(0, screenHeight - height));
    }

    public static void Apply(IEnumerable<BinarySubtitleItem> subtitles, int screenHeight, int barHeight, int offset, MoveCaptionsMode mode)
    {
        foreach (var item in subtitles)
        {
            item.Y = GetNewY(item, screenHeight, barHeight, offset, mode);
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (Window == null)
        {
            return;
        }

        if (BarHeight < 0 || BarHeight * 2 >= _screenHeight)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(Se.Language.General.PleaseEnterAValidValueForX, Se.Language.Tools.ImageBasedEdit.BarHeightPx),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Apply(_subtitles, _screenHeight, BarHeight, Offset, Mode);
        OkPressed = true;
        Window.Close();
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
    }
}

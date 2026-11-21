using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr;
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

public enum MoveCaptionsAlignment
{
    /// <summary>
    /// Each caption is placed on its own, by the baseline of its last text line at the bottom
    /// edge (by the image edge at the top), so descenders don't lift the text (#16360).
    /// </summary>
    Baseline,

    /// <summary>All captions of one edge move by the same offset, so their relative positions are kept.</summary>
    KeepRelativePositions,
}

/// <summary>
/// BDSup2Sub's "move all captions": for letterboxed (cinemascope) video, push every caption
/// either into the black bars or back inside the active picture, keeping a fixed offset from
/// the bar edge. A caption whose centre is in the upper half of the screen goes to the top
/// edge, the rest go to the bottom edge - the same rule BDSup2Sub uses. Unlike BDSup2Sub, bottom
/// captions are placed by their text baseline, or optionally all captions of one edge move by the
/// same offset (#16360).
/// </summary>
public partial class BinaryMoveCaptionsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<LetterboxRatioItem> _letterboxRatios;
    [ObservableProperty] private LetterboxRatioItem _selectedLetterboxRatio;
    [ObservableProperty] private int _barHeight;
    [ObservableProperty] private bool _isBarHeightEditable;
    [ObservableProperty] private bool _moveIntoBars;
    [ObservableProperty] private bool _moveIntoPicture;
    [ObservableProperty] private bool _alignBaseline;
    [ObservableProperty] private bool _alignKeepRelativePositions;
    [ObservableProperty] private int _offset;
    [ObservableProperty] private string _infoText;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private List<BinarySubtitleItem> _subtitles = new();
    private int _screenWidth;
    private int _screenHeight;
    private bool _isSyncingRatio;
    private int[]? _descents;

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
        _alignKeepRelativePositions = Se.Settings.Tools.BinEditMoveCaptionsKeepRelativePositions;
        _alignBaseline = !_alignKeepRelativePositions;
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
        }

        if (SelectedLetterboxRatio.IsCustom)
        {
            BarHeight = Math.Max(0, currentBarHeight);
        }
        else
        {
            // Screen size was unknown in the constructor, so the bar height computed there is 0.
            // Recompute it explicitly: the property setter above is a no-op (and never raises the
            // change callback) when the preset is the constructor's default 2.39:1 item.
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
    partial void OnAlignBaselineChanged(bool value) => UpdateInfo();
    partial void OnAlignKeepRelativePositionsChanged(bool value) => UpdateInfo();

    private MoveCaptionsMode Mode => MoveIntoPicture ? MoveCaptionsMode.IntoPicture : MoveCaptionsMode.IntoBars;

    private MoveCaptionsAlignment Alignment => AlignKeepRelativePositions ? MoveCaptionsAlignment.KeepRelativePositions : MoveCaptionsAlignment.Baseline;

    /// <summary>The descents are only needed for baseline alignment, and finding them runs the letter splitter on every image, so they are found once, on first use.</summary>
    private int[]? Descents => Alignment == MoveCaptionsAlignment.Baseline ? _descents ??= GetDescents(_subtitles, _screenHeight) : null;

    /// <summary>
    /// How far the last text line of each image hangs below its baseline (0 without a bitmap, a
    /// descender, or when it can't be told), as used by "Bottom align (baseline)".
    /// </summary>
    public static int[] GetDescents(IReadOnlyList<BinarySubtitleItem> subtitles, int screenHeight)
    {
        var minLineHeight = screenHeight >= 1080 ? 25 : 12;
        var descents = new int[subtitles.Count];
        for (var i = 0; i < subtitles.Count; i++)
        {
            var bitmap = subtitles[i].Bitmap;
            if (bitmap == null || bitmap.PixelSize.Height <= 0)
            {
                continue;
            }

            using var skBitmap = bitmap.ToSkBitmap();
            descents[i] = ImageBaselineFinder.GetDescent(skBitmap, minLineHeight);
        }

        return descents;
    }

    private void UpdateInfo()
    {
        var lang = Se.Language.Tools.ImageBasedEdit;
        var newYs = GetNewYs(_subtitles, _screenHeight, BarHeight, Offset, Mode, Alignment, Descents);
        var moving = _subtitles.Where((s, i) => newYs[i] != s.Y).Count();
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

        var isTop = IsTop(item, screenHeight);
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

    /// <summary>
    /// Computes the new Y of every caption. Bitmaps are cropped to the ink, so their heights differ
    /// with the text (descenders); placing each one by its own image edge would make the text
    /// baseline jump between captions (#16360). So:
    /// <list type="bullet">
    /// <item><see cref="MoveCaptionsAlignment.Baseline"/>: each caption is placed on its own, and a
    /// bottom caption is moved down by its <paramref name="descents"/> so its last baseline, not
    /// its image edge, sits at the edge (as "Bottom align (baseline)" does).</item>
    /// <item><see cref="MoveCaptionsAlignment.KeepRelativePositions"/>: all captions of one edge
    /// move by the same offset, taken from the caption that sticks out furthest towards that edge,
    /// so captions that shared a Y keep sharing it.</item>
    /// </list>
    /// </summary>
    public static int[] GetNewYs(IReadOnlyList<BinarySubtitleItem> subtitles, int screenHeight, int barHeight, int offset, MoveCaptionsMode mode, MoveCaptionsAlignment alignment, IReadOnlyList<int>? descents)
    {
        var newYs = subtitles.Select(s => s.Y).ToArray();
        if (screenHeight <= 0)
        {
            return newYs;
        }

        var movable = Enumerable.Range(0, subtitles.Count)
            .Where(i => (subtitles[i].Bitmap?.PixelSize.Height ?? 0) > 0)
            .ToList();

        if (alignment == MoveCaptionsAlignment.Baseline)
        {
            foreach (var i in movable)
            {
                var item = subtitles[i];
                var y = GetNewY(item, screenHeight, barHeight, offset, mode);
                if (descents != null && !IsTop(item, screenHeight))
                {
                    var height = item.Bitmap!.PixelSize.Height;
                    y = Math.Clamp(y + descents[i], 0, Math.Max(0, screenHeight - height));
                }

                newYs[i] = y;
            }

            return newYs;
        }

        var top = movable.Where(i => IsTop(subtitles[i], screenHeight)).ToList();
        var bottom = movable.Except(top).ToList();

        // Top edge: the caption closest to the top. Bottom edge: the caption whose bottom is lowest.
        MoveZone(top, i => subtitles[i].Y);
        MoveZone(bottom, i => -(subtitles[i].Y + subtitles[i].Bitmap!.PixelSize.Height));
        return newYs;

        void MoveZone(List<int> zone, Func<int, int> protrusion)
        {
            if (zone.Count == 0)
            {
                return;
            }

            var anchor = subtitles[zone.MinBy(protrusion)];
            var delta = GetNewY(anchor, screenHeight, barHeight, offset, mode) - anchor.Y;
            foreach (var i in zone)
            {
                var height = subtitles[i].Bitmap!.PixelSize.Height;
                newYs[i] = Math.Clamp(subtitles[i].Y + delta, 0, Math.Max(0, screenHeight - height));
            }
        }
    }

    private static bool IsTop(BinarySubtitleItem item, int screenHeight)
    {
        var height = item.Bitmap?.PixelSize.Height ?? 0;
        return item.Y + height / 2.0 < screenHeight / 2.0;
    }

    public static void Apply(IReadOnlyList<BinarySubtitleItem> subtitles, int screenHeight, int barHeight, int offset, MoveCaptionsMode mode, MoveCaptionsAlignment alignment, IReadOnlyList<int>? descents)
    {
        var newYs = GetNewYs(subtitles, screenHeight, barHeight, offset, mode, alignment, descents);
        for (var i = 0; i < subtitles.Count; i++)
        {
            subtitles[i].Y = newYs[i];
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

        Apply(_subtitles, _screenHeight, BarHeight, Offset, Mode, Alignment, Descents);
        Se.Settings.Tools.BinEditMoveCaptionsKeepRelativePositions = AlignKeepRelativePositions;
        OkPressed = true;
        Window.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e) => UiUtil.CloseOnEscapeOrShowHelp(e, Window, "features/binary-edit", "tools");
}

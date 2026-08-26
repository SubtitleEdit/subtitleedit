using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.UiLogic.Export;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

/// <summary>
/// Settings for the export-to-images text effect. Every change is pushed straight into the
/// owning <see cref="ExportImageBasedViewModel"/>, so the preview in the export dialog follows
/// the sliders live; Cancel puts the original values back.
/// </summary>
public partial class TextEffectViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<TextEffectDisplayItem> _presets;
    [ObservableProperty] private TextEffectDisplayItem? _selectedPreset;
    [ObservableProperty] private int _strength = 100;
    [ObservableProperty] private int _letterSpacing;
    [ObservableProperty] private int _arcBend;
    [ObservableProperty] private int _wave;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private ExportImageBasedViewModel? _parent;
    private TextEffectPreset _originalPreset;
    private int _originalStrength;
    private int _originalLetterSpacing;
    private int _originalArcBend;
    private int _originalWave;
    private bool _initialized;
    private bool _restored;

    public TextEffectViewModel()
    {
        Presets = new ObservableCollection<TextEffectDisplayItem>(TextEffectDisplayItem.GetItems());
    }

    public void Initialize(ExportImageBasedViewModel parent)
    {
        _parent = parent;
        _originalPreset = parent.SelectedTextEffect?.Preset ?? TextEffectPreset.SoftShadow;
        _originalStrength = parent.TextEffectStrength;
        _originalLetterSpacing = parent.TextEffectLetterSpacing;
        _originalArcBend = parent.TextEffectArcBend;
        _originalWave = parent.TextEffectWave;

        SelectedPreset = Presets.FirstOrDefault(p => p.Preset == _originalPreset) ?? Presets[0];
        Strength = _originalStrength;
        LetterSpacing = _originalLetterSpacing;
        ArcBend = _originalArcBend;
        Wave = _originalWave;

        _initialized = true;
    }

    partial void OnSelectedPresetChanged(TextEffectDisplayItem? value) => Push();
    partial void OnStrengthChanged(int value) => Push();
    partial void OnLetterSpacingChanged(int value) => Push();
    partial void OnArcBendChanged(int value) => Push();
    partial void OnWaveChanged(int value) => Push();

    private void Push()
    {
        if (!_initialized || _parent == null)
        {
            return;
        }

        if (SelectedPreset != null)
        {
            _parent.SelectedTextEffect =
                _parent.TextEffectItems.FirstOrDefault(t => t.Preset == SelectedPreset.Preset)
                ?? _parent.SelectedTextEffect;
        }

        _parent.TextEffectStrength = Strength;
        _parent.TextEffectLetterSpacing = LetterSpacing;
        _parent.TextEffectArcBend = ArcBend;
        _parent.TextEffectWave = Wave;
    }

    /// <summary>Back to the neutral tuning values; the preset choice stays.</summary>
    [RelayCommand]
    private void Reset()
    {
        Strength = 100;
        LetterSpacing = 0;
        ArcBend = 0;
        Wave = 0;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Push();
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        RestoreOriginalValues();
        Window?.Close();
    }

    /// <summary>
    /// Puts the owner's values back to what they were when the dialog opened. Guarded so it
    /// runs at most once: Cancel restores and then closes, and that close raises
    /// <see cref="OnWindowClosing"/>, which would otherwise restore a second time.
    /// </summary>
    private void RestoreOriginalValues()
    {
        if (_restored || _parent == null)
        {
            return;
        }

        _restored = true;
        _parent.SelectedTextEffect =
            _parent.TextEffectItems.FirstOrDefault(t => t.Preset == _originalPreset)
            ?? _parent.SelectedTextEffect;
        _parent.TextEffectStrength = _originalStrength;
        _parent.TextEffectLetterSpacing = _originalLetterSpacing;
        _parent.TextEffectArcBend = _originalArcBend;
        _parent.TextEffectWave = _originalWave;
    }

    /// <summary>
    /// The window is closing. The values were pushed into the owner live, so any close that
    /// is not an OK - the Cancel button, Escape, the title bar X, Alt+F4 - must put the
    /// original values back.
    /// </summary>
    internal void OnWindowClosing()
    {
        if (!OkPressed)
        {
            RestoreOriginalValues();
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
    }
}

using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;

public partial class EncodingSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<EncodingDisplayItem> _encodings;
    [ObservableProperty] private EncodingDisplayItem? _selectedEncoding;
    [ObservableProperty] private bool _isStereo;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public EncodingSettingsViewModel()
    {
        Encodings = new ObservableCollection<EncodingDisplayItem>(EncodingDisplayItem.GetDefaultEncodings());
        LoadSettings();
    }

    private void LoadSettings()
    {
        var lastUsedEncoding = Se.Settings.Video.TextToSpeech.CustomAudioEncoding;
        var encoding = Encodings.FirstOrDefault(e => e.Name.Equals(lastUsedEncoding, StringComparison.OrdinalIgnoreCase));
        if (encoding != null)
        {
            SelectedEncoding = encoding;
        }
        else
        {
            SelectedEncoding = Encodings.FirstOrDefault(e => e.Name.Equals("Default", StringComparison.OrdinalIgnoreCase));
        }

        IsStereo = Se.Settings.Video.TextToSpeech.CustomAudioStereo;
    }

    [RelayCommand]
    private void Ok()
    {
        var encoding = SelectedEncoding;
        if (encoding != null)
        {
            Se.Settings.Video.TextToSpeech.CustomAudioEncoding = encoding.Code;
            // The user's checkbox, not the codec's IsStereoEnabled *capability* flag - saving the
            // capability made the checkbox have no effect (always stereo for AAC/MP3, never for
            // Default). Gated on the capability so a codec without the option can't force -ac 2.
            Se.Settings.Video.TextToSpeech.CustomAudioStereo = IsStereo && encoding.IsStereoEnabled;
        }

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
    }
}
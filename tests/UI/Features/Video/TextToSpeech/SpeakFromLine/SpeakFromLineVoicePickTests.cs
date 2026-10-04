using Nikse.SubtitleEdit.Features.Video.TextToSpeech.SpeakFromLine;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using System.Collections.ObjectModel;

namespace UITests.Features.Video.TextToSpeech.SpeakFromLine;

/// <summary>
/// After a first-use engine download the voice list is reloaded; the voice the user picked
/// before clicking OK must survive that reload instead of falling back to the saved/English one.
/// </summary>
public class SpeakFromLineVoicePickTests
{
    private static ObservableCollection<Voice> Voices(params string[] names) =>
        new(names.Select(n => new Voice(n)));

    [Fact]
    public void PickVoice_NamedVoice_WinsOverEnglish()
    {
        var voices = Voices("en_US-amy", "de_DE-thorsten", "fr_FR-siwis");

        Assert.Equal("de_DE-thorsten", SpeakFromLineViewModel.PickVoice(voices, "de_DE-thorsten")?.Name);
    }

    [Fact]
    public void PickVoice_UnknownName_FallsBackToEnglish()
    {
        var voices = Voices("de_DE-thorsten", "en_US-amy");

        Assert.Equal("en_US-amy", SpeakFromLineViewModel.PickVoice(voices, "missing")?.Name);
    }

    [Fact]
    public void PickVoice_NoEnglish_FallsBackToFirst()
    {
        var voices = Voices("de_DE-thorsten", "fr_FR-siwis");

        Assert.Equal("de_DE-thorsten", SpeakFromLineViewModel.PickVoice(voices, null)?.Name);
        Assert.Null(SpeakFromLineViewModel.PickVoice(Voices(), "x"));
    }
}

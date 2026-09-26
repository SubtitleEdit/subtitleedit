using System;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.SpeechToText;

public class SpeechToTextDetectSpeakersTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static SpeechToTextViewModel MakeViewModel() =>
        new(new WindowService(new NullServiceProvider()), new FileHelper(), new FolderHelper());

    [AvaloniaFact]
    public void AutoCast_StartsWithDetectSpeakersOn_WhateverTheUserLastChose()
    {
        var old = Se.Settings.Tools.AudioToText.CrispAsrDetectSpeakers;
        try
        {
            Se.Settings.Tools.AudioToText.CrispAsrDetectSpeakers = false;
            var vm = MakeViewModel();

            vm.Initialize(null, -1, detectSpeakers: true);

            Assert.True(vm.DoDetectSpeakers);
        }
        finally
        {
            Se.Settings.Tools.AudioToText.CrispAsrDetectSpeakers = old;
        }
    }

    [AvaloniaFact]
    public void PlainSpeechToText_KeepsTheUsersChoice()
    {
        var old = Se.Settings.Tools.AudioToText.CrispAsrDetectSpeakers;
        try
        {
            Se.Settings.Tools.AudioToText.CrispAsrDetectSpeakers = false;
            var vm = MakeViewModel();

            vm.Initialize(null, -1);

            Assert.False(vm.DoDetectSpeakers);
        }
        finally
        {
            Se.Settings.Tools.AudioToText.CrispAsrDetectSpeakers = old;
        }
    }
}

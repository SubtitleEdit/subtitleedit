using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace UITests.Features.Main;

/// <summary>
/// The subtitle grid's WebVTT Style and Voice columns read their values off the cue text
/// (WebVTT has no style/actor field of its own), and both values are memoized - so they must
/// follow a text edit and raise a change notification for the cells to repaint.
/// </summary>
public class SubtitleLineViewModelWebVttTests
{
    [AvaloniaFact]
    public void WebVttStyle_ListsTheCueClasses()
    {
        var vm = new SubtitleLineViewModel { Text = "<c.loud.red>Hello</c>" };

        Assert.Equal("loud, red", vm.WebVttStyle);
    }

    [AvaloniaFact]
    public void WebVttStyle_IsEmptyWithoutCueClasses()
    {
        var vm = new SubtitleLineViewModel { Text = "<i>Hello</i>" };

        Assert.Equal(string.Empty, vm.WebVttStyle);
    }

    [AvaloniaFact]
    public void WebVttVoice_ReadsTheVoiceTag()
    {
        var vm = new SubtitleLineViewModel { Text = "<v Joe Smith>Hello" };

        Assert.Equal("Joe Smith", vm.WebVttVoice);
    }

    [AvaloniaFact]
    public void WebVttStyleAndVoice_FollowATextChange()
    {
        var vm = new SubtitleLineViewModel { Text = "<v Joe><c.red>Hello</c>" };
        Assert.Equal("red", vm.WebVttStyle);
        Assert.Equal("Joe", vm.WebVttVoice);

        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Text = "<v Ann><c.loud>Hi</c>";

        Assert.Equal("loud", vm.WebVttStyle);
        Assert.Equal("Ann", vm.WebVttVoice);
        Assert.Contains(nameof(SubtitleLineViewModel.WebVttStyle), changed);
        Assert.Contains(nameof(SubtitleLineViewModel.WebVttVoice), changed);
    }
}

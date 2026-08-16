using System.Collections.ObjectModel;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// Regression tests for issue #13677: a line inserted into an ASSA file adopted whichever style
/// sat first in the header instead of keeping the style of the line next to it, so files whose
/// "Default" style was not listed first got new lines in a foreign style.
/// </summary>
public class InsertServiceAssaStyleTests
{
    private static string MakeHeaderWithStyles(params string[] styleNames)
    {
        var styles = styleNames.Select(n => new SsaStyle { Name = n }).ToList();
        return AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(AdvancedSubStationAlpha.DefaultHeader, styles);
    }

    private static SubtitleLineViewModel MakeLine(string text, double startMs, double endMs, string style)
    {
        return new SubtitleLineViewModel
        {
            Text = text,
            StartTime = System.TimeSpan.FromMilliseconds(startMs),
            EndTime = System.TimeSpan.FromMilliseconds(endMs),
            Style = style,
        };
    }

    [Fact]
    public void InsertAfter_Assa_KeepsTheStyleOfTheLineItIsInsertedAfter()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();

            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Jacob Andrews", "Both", "Default") };
            var subtitles = new ObservableCollection<SubtitleLineViewModel>
            {
                MakeLine("Yammers, hello.", 1000, 3000, "Default"),
                MakeLine("It's your boy,", 4000, 6000, "Jacob Andrews"),
            };

            new InsertService().InsertAfter(new AdvancedSubStationAlpha(), subtitle, subtitles, 1, string.Empty);

            Assert.Equal(3, subtitles.Count);
            Assert.Equal("Jacob Andrews", subtitles[2].Style);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void InsertBefore_Assa_KeepsTheStyleOfTheLineItIsInsertedBefore()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();

            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Jacob Andrews", "Both", "Default") };
            var subtitles = new ObservableCollection<SubtitleLineViewModel>
            {
                MakeLine("Yammers, hello.", 4000, 6000, "Default"),
            };

            new InsertService().InsertBefore(new AdvancedSubStationAlpha(), subtitle, subtitles, 0, string.Empty);

            Assert.Equal(2, subtitles.Count);

            // Without the fix this was "Julia Lepetit" - the first style in the header.
            Assert.Equal("Default", subtitles[0].Style);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void InsertBefore_Assa_EmptyFile_PrefersTheStyleNamedDefaultOverTheFirstHeaderStyle()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();

            // No line to inherit from, and no storage default configured.
            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Jacob Andrews", "Both", "Default") };
            var subtitles = new ObservableCollection<SubtitleLineViewModel>();

            new InsertService().InsertBefore(new AdvancedSubStationAlpha(), subtitle, subtitles, 0, string.Empty);

            Assert.Single(subtitles);
            Assert.Equal("Default", subtitles[0].Style);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void InsertAfter_Assa_NeighborWithoutStyle_FallsBackToTheStyleNamedDefault()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();

            // A line that was itself inserted before the fix carries no style at all; the new
            // line must not inherit that emptiness.
            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Jacob Andrews", "Both", "Default") };
            var subtitles = new ObservableCollection<SubtitleLineViewModel>
            {
                MakeLine("Yammers, hello.", 1000, 3000, string.Empty),
            };

            new InsertService().InsertAfter(new AdvancedSubStationAlpha(), subtitle, subtitles, 0, string.Empty);

            Assert.Equal(2, subtitles.Count);
            Assert.Equal("Default", subtitles[1].Style);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void InsertAfter_Assa_WrittenFileReferencesTheInheritedStyle()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();

            var format = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Jacob Andrews", "Both", "Default") };
            var subtitles = new ObservableCollection<SubtitleLineViewModel>
            {
                MakeLine("Yammers, hello.", 1000, 3000, "Both"),
            };

            new InsertService().InsertAfter(format, subtitle, subtitles, 0, "This is a new line");

            // Mirror the save path: the grid rebuilds the paragraphs, ToParagraph maps Style to Extra.
            subtitle.Paragraphs.Clear();
            subtitle.Paragraphs.AddRange(subtitles.Select(s => s.ToParagraph(format)));

            var text = format.ToText(subtitle, string.Empty);
            var dialogueLines = text.SplitToLines().Where(l => l.StartsWith("Dialogue:")).ToList();

            Assert.Equal(2, dialogueLines.Count);
            Assert.All(dialogueLines, l => Assert.Equal("Both", l.Split(',')[3]));
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void InsertAfter_EmptySubtitle_AppendsInsteadOfThrowing()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();

            // "Insert after" on an empty subtitle asks for index 1 - it used to throw.
            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Default") };
            var subtitles = new ObservableCollection<SubtitleLineViewModel>();

            new InsertService().InsertAfter(new AdvancedSubStationAlpha(), subtitle, subtitles, null, "Hello");

            var line = Assert.Single(subtitles);
            Assert.Equal("Hello", line.Text);
            Assert.Equal("Default", line.Style);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void InsertAfter_IndexPastTheEnd_AppendsInsteadOfThrowing()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();

            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Default") };
            var subtitles = new ObservableCollection<SubtitleLineViewModel>
            {
                MakeLine("Yammers, hello.", 1000, 3000, "Julia Lepetit"),
            };

            new InsertService().InsertAfter(new AdvancedSubStationAlpha(), subtitle, subtitles, 7, "Hello");

            Assert.Equal(2, subtitles.Count);
            Assert.Equal("Hello", subtitles[1].Text);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void InsertBefore_IndexPastTheEnd_AppendsInsteadOfThrowing()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();

            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Default") };
            var subtitles = new ObservableCollection<SubtitleLineViewModel>
            {
                MakeLine("Yammers, hello.", 1000, 3000, "Julia Lepetit"),
            };

            new InsertService().InsertBefore(new AdvancedSubStationAlpha(), subtitle, subtitles, 7, "Hello");

            Assert.Equal(2, subtitles.Count);
            Assert.Equal("Hello", subtitles[1].Text);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void InsertAfter_SubRip_LeavesStyleAlone()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();

            var subtitles = new ObservableCollection<SubtitleLineViewModel>
            {
                MakeLine("Hello", 1000, 3000, string.Empty),
            };

            new InsertService().InsertAfter(new SubRip(), new Subtitle(), subtitles, 0, string.Empty);

            Assert.Equal(2, subtitles.Count);
            Assert.Equal(string.Empty, subtitles[1].Style);
        }
        finally
        {
            Se.Settings = saved;
        }
    }
}

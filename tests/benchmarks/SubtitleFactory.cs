using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Benchmarks;

internal static class SubtitleFactory
{
    private static readonly string[] Sentences =
    {
        "It was the best of times, it was the worst of times.",
        "<i>Are you coming with us?</i>",
        "- No.\r\n- Then stay here and wait.",
        "I told you already, this is not going to work out the way you think it will.",
        "Hello.",
        "{\\an8}Somewhere in Denmark",
    };

    /// <summary>Builds a realistic, start-time sorted list of subtitle lines.</summary>
    public static List<SubtitleLineViewModel> Make(int count)
    {
        var list = new List<SubtitleLineViewModel>(count);
        var startMs = 1000.0;
        for (var i = 0; i < count; i++)
        {
            var text = Sentences[i % Sentences.Length];
            var durationMs = 1200 + i % 7 * 350;
            list.Add(new SubtitleLineViewModel
            {
                Number = i + 1,
                Text = text,
                StartTime = TimeSpan.FromMilliseconds(startMs),
                EndTime = TimeSpan.FromMilliseconds(startMs + durationMs),
            });
            startMs += durationMs + 120;
        }

        return list;
    }
}

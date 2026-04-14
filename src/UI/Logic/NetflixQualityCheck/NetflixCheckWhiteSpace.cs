using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

public class NetflixCheckWhiteSpace : INetflixQualityChecker
{
    private static readonly Regex LineEndingSpaceBefore = new Regex(@"^( |\n|\r\n)[^\s]", RegexOptions.Compiled);
    private static readonly Regex LineEndingSpaceAfter = new Regex(@"[^\s]( |\n|\r\n)$", RegexOptions.Compiled);
    private static readonly Regex SpacesBeforePunctuation = new Regex(@"[^\s]( |\n|\r\n)[!?).,؟،…]", RegexOptions.Compiled);
    private static readonly Regex TwoPlusConsequentSpaces = new Regex(@"( |\n|\r\n){2,}", RegexOptions.Compiled);

    public string Name { get; set; }

    public NetflixCheckWhiteSpace(string name)
    {
        Name = name;
    }

    private static void AddWhiteSpaceWarning(Paragraph p, NetflixQualityController report, string issue, int pos)
    {
        string timeCode = p.StartTime.ToHHMMSSFF();
        string context = NetflixQualityController.StringContext(p.Text, pos, 6);
        string comment = string.Format(Se.Language.Tools.NetflixCheckAndFix.WhiteSpaceCheckForXReport, issue, pos);

        report.AddRecord(p, timeCode, context, comment);
    }

    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        foreach (Paragraph p in subtitle.Paragraphs)
        {
            // Line endings
            if (LineEndingSpaceBefore.IsMatch(p.Text))
            {
                AddWhiteSpaceWarning(p, controller, Se.Language.Tools.NetflixCheckAndFix.WhiteSpaceLineEnding, 1);
            }

            if (LineEndingSpaceAfter.IsMatch(p.Text))
            {
                AddWhiteSpaceWarning(p, controller, Se.Language.Tools.NetflixCheckAndFix.WhiteSpaceLineEnding, p.Text.Length);
            }

            // Spaces before punctuation
            foreach (Match m in SpacesBeforePunctuation.Matches(p.Text))
            {
                AddWhiteSpaceWarning(p, controller, Se.Language.Tools.NetflixCheckAndFix.WhiteSpaceBeforePunctuation, m.Index + 1);
            }


            // 2+ consecutive spaces
            foreach (Match m in TwoPlusConsequentSpaces.Matches(p.Text))
            {
                AddWhiteSpaceWarning(p, controller, Se.Language.Tools.NetflixCheckAndFix.WhiteSpaceCheckConsecutive, m.Index);
            }
        }
    }
}

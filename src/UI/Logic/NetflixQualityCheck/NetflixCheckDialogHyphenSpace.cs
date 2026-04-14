using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

/// <summary>
/// Check speaker style depending on the language.
/// </summary>
public class NetflixCheckDialogHyphenSpace : INetflixQualityChecker
{
    public string Name { get; set; }

    public NetflixCheckDialogHyphenSpace(string name)
    {
        Name = name;        
    }


    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        if (controller.Language == "ja")
        {
            return;
        }

        var dialogHelper = new DialogSplitMerge { DialogStyle = controller.SpeakerStyle };
        string comment = Se.Language.Tools.NetflixCheckAndFix.DualSpeakersHyphenWithoutSpace;
        if (controller.SpeakerStyle == DialogType.DashBothLinesWithSpace)
        {
            comment = Se.Language.Tools.NetflixCheckAndFix.DualSpeakersHyphenWithSpace;
        }
        else if (controller.SpeakerStyle == DialogType.DashSecondLineWithSpace)
        {
            comment = Se.Language.Tools.NetflixCheckAndFix.DualSpeakersHyphenWithSpaceSecondOnly;
        }
        else if (controller.SpeakerStyle == DialogType.DashSecondLineWithoutSpace)
        {
            comment = Se.Language.Tools.NetflixCheckAndFix.DualSpeakersHyphenWithoutSpaceSecondOnly;
        }

        for (int i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var p = subtitle.Paragraphs[i];
            string oldText = p.Text;
            string newText = dialogHelper.FixDashesAndSpaces(p.Text, p, subtitle.GetParagraphOrDefault(i - 1));
            if (newText != oldText)
            {
                var fixedParagraph = new Paragraph(p, false) { Text = newText };
                controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
            }
        }
    }
}

using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    /// <summary>
    /// Check speaker style depending on the language.
    /// </summary>
    public class NetflixCheckDialogHyphenSpace : INetflixQualityChecker
    {
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            if (controller.Language == "ja")
            {
                return;
            }

            var dialogHelper = new DialogSplitMerge { DialogStyle = controller.SpeakerStyle };
            string comment = "Dual Speakers: Use a hyphen without a space";
            if (controller.SpeakerStyle == DialogType.DashBothLinesWithSpace)
            {
                comment = "Dual Speakers: Use a hyphen with a space";
            }
            else if (controller.SpeakerStyle == DialogType.DashSecondLineWithSpace)
            {
                comment = "Dual Speakers: Use a hyphen with a space to denote the second speaker only";
            }
            else if (controller.SpeakerStyle == DialogType.DashSecondLineWithoutSpace)
            {
                comment = "Dual Speakers: Use a hyphen without a space to denote the second speaker only";
            }

            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                string oldText = p.Text;
                string newText = dialogHelper.FixDashesAndSpaces(p.Text, p, subtitle.GetParagraphOrDefault(i-1));
                if (newText != oldText)
                {
                    var fixedParagraph = new Paragraph(p, false) { Text = newText };
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }
    }
}
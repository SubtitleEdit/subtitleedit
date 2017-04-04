using System;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckDialogeHyphenNoSpace : INetflixQualityChecker
    {

        /// <summary>
        /// Use a hyphen without a space to indicate two speakers in one subtitle
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            if (!controller.DualSpeakersHasHypenAndNoSplace)
            {
                return;
            }

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var arr = p.Text.SplitToLines();
                if (arr.Length == 2 && p.Text.Contains("-"))
                {
                    string newText = p.Text;
                    if (arr[0].StartsWith("- ") && arr[1].StartsWith("- "))
                    {
                        newText = "-" + arr[0].Remove(0, 2) + Environment.NewLine + "-" + arr[1].Remove(0, 2);
                    }
                    else if (arr[0].StartsWith("<i>- ") && arr[1].StartsWith("<i>- "))
                    {
                        newText = "<i>-" + arr[0].Remove(0, 5) + Environment.NewLine + "<i>-" + arr[1].Remove(0, 5);
                    }
                    else if (arr[0].StartsWith("<i>- ") && arr[1].StartsWith("- "))
                    {
                        newText = "<i>-" + arr[0].Remove(0, 5) + Environment.NewLine + "-" + arr[1].Remove(0, 2);
                    }
                    else if (arr[0].StartsWith("- ") && arr[1].StartsWith("<i>- "))
                    {
                        newText = "-" + arr[0].Remove(0, 2) + Environment.NewLine + "<i>-" + arr[1].Remove(0, 5);
                    }
                    else if ((arr[0].StartsWith("-") || arr[0].StartsWith("<i>-")) && arr[1].StartsWith("- "))
                    {
                        newText = "-" + arr[0] + Environment.NewLine + "-" + arr[1].Remove(0, 2);
                    }
                    else if (arr[0].StartsWith("- ") && (arr[1].StartsWith("-") || arr[1].StartsWith("<i>-")))
                    {
                        newText = "-" + arr[0].Remove(0, 2) + Environment.NewLine + "-" + arr[1];
                    }

                    if (newText != p.Text)
                    {
                        var fixedParagraph = new Paragraph(p, false) { Text = newText };
                        string comment = "Dual Speakers: Use a hyphen without a space";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }
            }
        }

    }
}
using System;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckDialogHyphenSpace : INetflixQualityChecker
    {

        /// <summary>
        /// Use a hyphen with or without a space to indicate two speakers in one subtitle
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            if (controller.DualSpeakersHasHyphenAndNoSpace)
            {
                RemoveSpaceAfterHyphenInDialogues(subtitle, controller);
            }
            else
            {
                AddSpaceAfterHyphenInDialogues(subtitle, controller);
            }
        }

        private static void RemoveSpaceAfterHyphenInDialogues(Subtitle subtitle, NetflixQualityController controller)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var arr = p.Text.SplitToLines();
                if (arr.Count == 2 && p.Text.Contains("-"))
                {
                    string newText = p.Text;
                    if (arr[0].StartsWith("- ", StringComparison.Ordinal) && arr[1].StartsWith("- ", StringComparison.Ordinal))
                    {
                        newText = "-" + arr[0].Remove(0, 2) + Environment.NewLine + "-" + arr[1].Remove(0, 2);
                    }
                    else if (arr[0].StartsWith("<i>- ", StringComparison.Ordinal) && arr[1].StartsWith("<i>- ", StringComparison.Ordinal))
                    {
                        newText = "<i>-" + arr[0].Remove(0, 5) + Environment.NewLine + "<i>-" + arr[1].Remove(0, 5);
                    }
                    else if (arr[0].StartsWith("<i>- ", StringComparison.Ordinal) && arr[1].StartsWith("- ", StringComparison.Ordinal))
                    {
                        newText = "<i>-" + arr[0].Remove(0, 5) + Environment.NewLine + "-" + arr[1].Remove(0, 2);
                    }
                    else if (arr[0].StartsWith("- ", StringComparison.Ordinal) && arr[1].StartsWith("<i>- ", StringComparison.Ordinal))
                    {
                        newText = "-" + arr[0].Remove(0, 2) + Environment.NewLine + "<i>-" + arr[1].Remove(0, 5);
                    }
                    else if ((arr[0].StartsWith("-", StringComparison.Ordinal) || arr[0].StartsWith("<i>-", StringComparison.Ordinal)) && arr[1].StartsWith("- ", StringComparison.Ordinal))
                    {
                        newText = "-" + arr[0] + Environment.NewLine + "-" + arr[1].Remove(0, 2);
                    }
                    else if (arr[0].StartsWith("- ", StringComparison.Ordinal) && (arr[1].StartsWith("-", StringComparison.Ordinal) || arr[1].StartsWith("<i>-", StringComparison.Ordinal)))
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

        private static void AddSpaceAfterHyphenInDialogues(Subtitle subtitle, NetflixQualityController controller)
        {
            var sub = new Subtitle(subtitle);
            for (int i = 0; i < sub.Paragraphs.Count; i++)
            {
                Paragraph p = new Paragraph(sub.Paragraphs[i]);
                var arr = p.Text.SplitToLines();
                if (arr.Count == 2 && p.Text.Contains("-") && arr[0].Length > 3 && arr[1].Length > 3)
                {
                    string newText = p.Text;
                    if (arr[0][0] == '-' && char.IsLetter(arr[0][1]) && (arr[1].StartsWith("-", StringComparison.Ordinal) || arr[1].StartsWith("<i>-", StringComparison.Ordinal)))
                    {
                        newText = arr[0].Insert(1, " ") + Environment.NewLine + arr[1];
                        arr = newText.SplitToLines();
                    }
                    else if (arr[0].StartsWith("<i>-", StringComparison.Ordinal) && arr[0].Length > 5 && char.IsLetter(arr[0][4]) && (arr[1].StartsWith("-", StringComparison.Ordinal) || arr[1].StartsWith("<i>-", StringComparison.Ordinal)))
                    {
                        newText = arr[0].Insert(4, " ") + Environment.NewLine + arr[1];
                        arr = newText.SplitToLines();
                    }

                    if (arr[1][0] == '-' && char.IsLetter(arr[1][1]) && (arr[0].StartsWith("-", StringComparison.Ordinal) || arr[0].StartsWith("<i>-", StringComparison.Ordinal)))
                    {
                        newText = arr[0] + Environment.NewLine + arr[1].Insert(1, " ");
                    }
                    else if (arr[1].StartsWith("<i>-", StringComparison.Ordinal) && arr[1].Length > 5 && char.IsLetter(arr[1][4]) && (arr[0].StartsWith("-", StringComparison.Ordinal) || arr[0].StartsWith("<i>-", StringComparison.Ordinal)))
                    {
                        newText = arr[0] + Environment.NewLine + arr[1].Insert(4, " ");
                    }

                    if (newText != p.Text)
                    {
                        var fixedParagraph = new Paragraph(p, false) { Text = newText };
                        string comment = "Dual Speakers: Use a space after hyphen";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }
            }
        }

    }
}
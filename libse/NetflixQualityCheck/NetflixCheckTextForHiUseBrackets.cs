using System;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckTextForHiUseBrackets : INetflixQualityChecker
    {

        /// <summary>
        /// Use brackets[] to enclose speaker IDs or sound effects
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            if (controller.Language == "jp")
            {
                return;
            }

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string newText = p.Text;
                var arr = p.Text.SplitToLines();
                if (newText.StartsWith("(", StringComparison.Ordinal) && newText.EndsWith(")", StringComparison.Ordinal))
                {
                    newText = "[" + newText.Substring(1, newText.Length - 2) + "]";
                }
                else if (newText.StartsWith("{", StringComparison.Ordinal) && newText.EndsWith("}", StringComparison.Ordinal))
                {
                    newText = "[" + newText.Substring(1, newText.Length - 2) + "]";
                }
                else if (arr.Count == 2 && arr[0].StartsWith("-", StringComparison.Ordinal) && arr[1].StartsWith("-", StringComparison.Ordinal))
                {
                    if ((arr[0].StartsWith("-(", StringComparison.Ordinal) && arr[0].EndsWith(")", StringComparison.Ordinal)) || (arr[0].StartsWith("-{", StringComparison.Ordinal) && arr[0].EndsWith("}", StringComparison.Ordinal)))
                    {
                        arr[0] = "-[" + newText.Substring(2, newText.Length - 3) + "]";
                    }
                    if ((arr[1].StartsWith("-(", StringComparison.Ordinal) && arr[1].EndsWith(")", StringComparison.Ordinal)) || (arr[1].StartsWith("-{", StringComparison.Ordinal) && arr[1].EndsWith("}", StringComparison.Ordinal)))
                    {
                        arr[1] = "-[" + arr[1].Substring(2, arr[1].Length - 3) + "]";
                    }
                    newText = arr[0] + Environment.NewLine + arr[1];
                }

                if (newText != p.Text)
                {
                    var fixedParagraph = new Paragraph(p, false) { Text = newText };
                    string comment = "Use brackets [ ] to enclose speaker IDs or sound effects";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }

    }
}

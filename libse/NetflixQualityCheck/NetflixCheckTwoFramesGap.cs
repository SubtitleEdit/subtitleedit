namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckTwoFramesGap : INetflixQualityChecker
    {

        /// <summary>
        /// Two frames gap minimum
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            if (controller.Language == "ja")
            {
                return;
            }

            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = subtitle.Paragraphs[index];
                var next = subtitle.GetParagraphOrDefault(index + 1);
                double twoFramesGap = 1000.0 / controller.FrameRate * 2.0;
                if (next != null && p.EndTime.TotalMilliseconds + twoFramesGap > next.StartTime.TotalMilliseconds)
                {
                    var fixedParagraph = new Paragraph(p, false) { EndTime = { TotalMilliseconds = next.StartTime.TotalMilliseconds - twoFramesGap } };
                    string comment = "Minimum two frames gap";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }
    }
}

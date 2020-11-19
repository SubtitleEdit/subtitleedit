using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public class UiGetYouTubeAnnotationStyles : YouTubeAnnotations.IGetYouTubeAnnotationStyles
    {
        public List<string> GetYouTubeAnnotationStyles(Dictionary<string, int> stylesWithCount)
        {
            if (stylesWithCount == null)
            {
                return new List<string>();
            }

            using (YouTubeAnnotationsImport import = new YouTubeAnnotationsImport(stylesWithCount))
            {
                if (import.ShowDialog() == DialogResult.OK)
                {
                    return import.SelectedStyles;
                }
                List<string> styles = new List<string>();
                foreach (string k in stylesWithCount.Keys)
                {
                    styles.Add(k);
                }

                return styles;
            }
        }
    }
}

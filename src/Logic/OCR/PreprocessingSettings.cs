using System.Drawing;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class PreprocessingSettings
    {
        public Color ColorToWhite { get; set; }
        public Color ColorToRemove { get; set; }
        public bool InvertColors { get; set; }
        public bool YellowToWhite { get; set; }

        public bool CropTransparentColors { get; set; }
        public int CropLeftRight { get; set; }
        public int CropTopBottom { get; set; }

        public int BinaryImageCompareThresshold { get; set; }

        public PreprocessingSettings()
        {
            ColorToWhite = Color.Transparent;
            ColorToRemove = Color.Transparent;
        }

        public bool Active => ColorToWhite != Color.Transparent ||
                              ColorToRemove != Color.Transparent ||
                              InvertColors ||
                              YellowToWhite ||
                              CropTransparentColors ||
                              CropLeftRight > 0 ||
                              CropTopBottom > 0;
    }
}

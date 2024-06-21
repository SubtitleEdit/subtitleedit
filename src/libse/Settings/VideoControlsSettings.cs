using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class VideoControlsSettings
    {
        public string CustomSearchText1 { get; set; }
        public string CustomSearchText2 { get; set; }
        public string CustomSearchText3 { get; set; }
        public string CustomSearchText4 { get; set; }
        public string CustomSearchText5 { get; set; }
        public string CustomSearchUrl1 { get; set; }
        public string CustomSearchUrl2 { get; set; }
        public string CustomSearchUrl3 { get; set; }
        public string CustomSearchUrl4 { get; set; }
        public string CustomSearchUrl5 { get; set; }
        public string LastActiveTab { get; set; }
        public bool WaveformDrawGrid { get; set; }
        public bool WaveformDrawCps { get; set; }
        public bool WaveformDrawWpm { get; set; }
        public bool WaveformAllowOverlap { get; set; }
        public bool WaveformFocusOnMouseEnter { get; set; }
        public bool WaveformListViewFocusOnMouseEnter { get; set; }
        public bool WaveformSetVideoPositionOnMoveStartEnd { get; set; }
        public bool WaveformSingleClickSelect { get; set; }
        public bool WaveformSnapToShotChanges { get; set; }
        public int WaveformShotChangeStartTimeBeforeMs { get; set; }
        public int WaveformShotChangeStartTimeAfterMs { get; set; }
        public int WaveformShotChangeEndTimeBeforeMs { get; set; }
        public int WaveformShotChangeEndTimeAfterMs { get; set; }
        public int WaveformBorderHitMs { get; set; }
        public Color WaveformGridColor { get; set; }
        public Color WaveformColor { get; set; }
        public Color WaveformSelectedColor { get; set; }
        public Color WaveformBackgroundColor { get; set; }
        public Color WaveformTextColor { get; set; }
        public Color WaveformCursorColor { get; set; }
        public Color WaveformChaptersColor { get; set; }
        public int WaveformTextSize { get; set; }
        public bool WaveformTextBold { get; set; }
        public string WaveformDoubleClickOnNonParagraphAction { get; set; }
        public string WaveformRightClickOnNonParagraphAction { get; set; }
        public bool WaveformMouseWheelScrollUpIsForward { get; set; }
        public bool WaveformLabelShowCodec { get; set; }
        public bool GenerateSpectrogram { get; set; }
        public string SpectrogramAppearance { get; set; }
        public int WaveformMinimumSampleRate { get; set; }
        public double WaveformSeeksSilenceDurationSeconds { get; set; }
        public double WaveformSeeksSilenceMaxVolume { get; set; }
        public bool WaveformUnwrapText { get; set; }
        public bool WaveformHideWpmCpsLabels { get; set; }


        public VideoControlsSettings()
        {
            CustomSearchText1 = "The Free Dictionary";
            CustomSearchUrl1 = "https://www.thefreedictionary.com/{0}";
            CustomSearchText2 = "Wikipedia";
            CustomSearchUrl2 = "https://en.wikipedia.org/wiki?search={0}";
            CustomSearchText3 = "DuckDuckGo";
            CustomSearchUrl3 = "https://duckduckgo.com/?q={0}";

            LastActiveTab = "Translate";
            WaveformDrawGrid = true;
            WaveformAllowOverlap = false;
            WaveformBorderHitMs = 15;
            WaveformGridColor = Color.FromArgb(255, 20, 20, 18);
            WaveformColor = Color.FromArgb(255, 160, 240, 30);
            WaveformSelectedColor = Color.FromArgb(255, 230, 0, 0);
            WaveformBackgroundColor = Color.Black;
            WaveformTextColor = Color.Gray;
            WaveformCursorColor = Color.Turquoise;
            WaveformChaptersColor = Color.FromArgb(255, 104, 33, 122);
            WaveformTextSize = 9;
            WaveformTextBold = true;
            WaveformDoubleClickOnNonParagraphAction = "PlayPause";
            WaveformDoubleClickOnNonParagraphAction = string.Empty;
            WaveformMouseWheelScrollUpIsForward = true;
            WaveformLabelShowCodec = true;
            SpectrogramAppearance = "OneColorGradient";
            WaveformMinimumSampleRate = 126;
            WaveformSeeksSilenceDurationSeconds = 0.3;
            WaveformSeeksSilenceMaxVolume = 0.1;
            WaveformSnapToShotChanges = true;
        }
    }
}
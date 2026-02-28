using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;

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
        public SKColor WaveformGridColor { get; set; }
        public SKColor WaveformColor { get; set; }
        public SKColor WaveformSelectedColor { get; set; }
        public SKColor WaveformBackgroundColor { get; set; }
        public SKColor WaveformTextColor { get; set; }
        public SKColor WaveformCursorColor { get; set; }
        public SKColor WaveformChaptersColor { get; set; }
        public int WaveformTextSize { get; set; }
        public bool WaveformTextBold { get; set; }
        public string WaveformDoubleClickOnNonParagraphAction { get; set; }
        public string WaveformRightClickOnNonParagraphAction { get; set; }
        public bool WaveformMouseWheelScrollUpIsForward { get; set; }
        public bool WaveformLabelShowCodec { get; set; }
        public bool GenerateSpectrogram { get; set; }
        public string SpectrogramAppearance { get; set; }

        public int SpectrogramWaveformOpacity { get; set; }
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
            WaveformGridColor = ColorUtils.FromArgb(255, 20, 20, 18);
            WaveformColor = ColorUtils.FromArgb(255, 160, 240, 30);
            WaveformSelectedColor = ColorUtils.FromArgb(255, 230, 0, 0);
            WaveformBackgroundColor = SKColors.Black;
            WaveformTextColor = SKColors.Gray;
            WaveformCursorColor = SKColors.Turquoise;
            WaveformChaptersColor = ColorUtils.FromArgb(255, 104, 33, 122);
            WaveformTextSize = 9;
            WaveformTextBold = true;
            WaveformDoubleClickOnNonParagraphAction = "PlayPause";
            WaveformDoubleClickOnNonParagraphAction = string.Empty;
            WaveformMouseWheelScrollUpIsForward = true;
            WaveformLabelShowCodec = true;
            SpectrogramAppearance = "OneColorGradient";
            SpectrogramWaveformOpacity = 256;
            WaveformMinimumSampleRate = 126;
            WaveformSeeksSilenceDurationSeconds = 0.3;
            WaveformSeeksSilenceMaxVolume = 0.1;
            WaveformSnapToShotChanges = true;
        }
    }
}
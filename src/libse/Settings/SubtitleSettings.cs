using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class SubtitleSettings
    {
        public List<AssaStorageCategory> AssaStyleStorageCategories { get; set; }
        public List<string> AssaOverrideTagHistory { get; set; }
        public bool AssaResolutionAutoNew { get; set; }
        public bool AssaResolutionPromptChange { get; set; }
        public bool AssaShowScaledBorderAndShadow { get; set; }
        public bool AssaShowPlayDepth { get; set; }
        public string DCinemaFontFile { get; set; }
        public string DCinemaLoadFontResource { get; set; }
        public int DCinemaFontSize { get; set; }
        public int DCinemaBottomMargin { get; set; }
        public double DCinemaZPosition { get; set; }
        public int DCinemaFadeUpTime { get; set; }
        public int DCinemaFadeDownTime { get; set; }
        public bool DCinemaAutoGenerateSubtitleId { get; set; }

        public string CurrentDCinemaSubtitleId { get; set; }
        public string CurrentDCinemaMovieTitle { get; set; }
        public string CurrentDCinemaReelNumber { get; set; }
        public string CurrentDCinemaIssueDate { get; set; }
        public string CurrentDCinemaLanguage { get; set; }
        public string CurrentDCinemaEditRate { get; set; }
        public string CurrentDCinemaTimeCodeRate { get; set; }
        public string CurrentDCinemaStartTime { get; set; }
        public string CurrentDCinemaFontId { get; set; }
        public string CurrentDCinemaFontUri { get; set; }
        public SKColor CurrentDCinemaFontColor { get; set; }
        public string CurrentDCinemaFontEffect { get; set; }
        public SKColor CurrentDCinemaFontEffectColor { get; set; }
        public int CurrentDCinemaFontSize { get; set; }

        public int CurrentCavena890LanguageIdLine1 { get; set; }
        public int CurrentCavena890LanguageIdLine2 { get; set; }
        public string CurrentCavena89Title { get; set; }
        public string CurrentCavena890riginalTitle { get; set; }
        public string CurrentCavena890Translator { get; set; }
        public string CurrentCavena89Comment { get; set; }
        public int CurrentCavena89LanguageId { get; set; }
        public string Cavena890StartOfMessage { get; set; }

        public bool EbuStlTeletextUseBox { get; set; }
        public bool EbuStlTeletextUseDoubleHeight { get; set; }
        public int EbuStlMarginTop { get; set; }
        public int EbuStlMarginBottom { get; set; }
        public int EbuStlNewLineRows { get; set; }
        public bool EbuStlRemoveEmptyLines { get; set; }
        public int PacVerticalTop { get; set; }
        public int PacVerticalCenter { get; set; }
        public int PacVerticalBottom { get; set; }

        public string DvdStudioProHeader { get; set; }

        public string TmpegEncXmlFontName { get; set; }
        public string TmpegEncXmlFontHeight { get; set; }
        public string TmpegEncXmlPosition { get; set; }

        public bool CheetahCaptionAlwayWriteEndTime { get; set; }

        public bool SamiDisplayTwoClassesAsTwoSubtitles { get; set; }
        public int SamiHtmlEncodeMode { get; set; }

        public string TimedText10TimeCodeFormat { get; set; }
        public string TimedText10TimeCodeFormatSource { get; set; }
        public bool TimedText10ShowStyleAndLanguage { get; set; }
        public string TimedText10FileExtension { get; set; }
        public string TimedTextItunesTopOrigin { get; set; }
        public string TimedTextItunesTopExtent { get; set; }
        public string TimedTextItunesBottomOrigin { get; set; }
        public string TimedTextItunesBottomExtent { get; set; }
        public string TimedTextItunesTimeCodeFormat { get; set; }
        public string TimedTextItunesStyleAttribute { get; set; }
        public string TimedTextItunesLanguage { get; set; }
        public string TimedTextImsc11TimeCodeFormat { get; set; }
        public string TimedTextImsc11FileExtension { get; set; }


        public int FcpFontSize { get; set; }
        public string FcpFontName { get; set; }

        public string NuendoCharacterListFile { get; set; }

        public bool WebVttUseXTimestampMap { get; set; }
        public bool WebVttUseMultipleXTimestampMap { get; set; }
        public bool WebVttMergeLinesWithSameText { get; set; }
        public bool WebVttDoNoMergeTags { get; set; }
        public long WebVttTimescale { get; set; }
        public string WebVttCueAn1 { get; set; }
        public string WebVttCueAn2 { get; set; }
        public string WebVttCueAn3 { get; set; }
        public string WebVttCueAn4 { get; set; }
        public string WebVttCueAn5 { get; set; }
        public string WebVttCueAn6 { get; set; }
        public string WebVttCueAn7 { get; set; }
        public string WebVttCueAn8 { get; set; }
        public string WebVttCueAn9 { get; set; }
        public string MPlayer2Extension { get; set; }
        public bool TeletextItalicFix { get; set; }
        public bool MccDebug { get; set; }
        public bool BluRaySupSkipMerge { get; set; }
        public bool BluRaySupForceMergeAll { get; set; }

        public SubtitleSettings()
        {
            AssaStyleStorageCategories = new List<AssaStorageCategory>();
            AssaOverrideTagHistory = new List<string>();
            AssaResolutionAutoNew = true;
            AssaResolutionPromptChange = true;
            AssaShowScaledBorderAndShadow = true;
            AssaShowPlayDepth = true;

            DCinemaFontFile = "Arial.ttf";
            DCinemaLoadFontResource = "urn:uuid:3dec6dc0-39d0-498d-97d0-928d2eb78391";
            DCinemaFontSize = 42;
            DCinemaBottomMargin = 8;
            DCinemaZPosition = 0;
            DCinemaFadeUpTime = 0;
            DCinemaFadeDownTime = 0;
            DCinemaAutoGenerateSubtitleId = true;

            EbuStlTeletextUseBox = true;
            EbuStlTeletextUseDoubleHeight = true;
            EbuStlMarginTop = 0;
            EbuStlMarginBottom = 2;
            EbuStlNewLineRows = 2;

            PacVerticalTop = 1;
            PacVerticalCenter = 6;
            PacVerticalBottom = 11;

            DvdStudioProHeader = @"$VertAlign          =   Bottom
$Bold               =   FALSE
$Underlined         =   FALSE
$Italic             =   FALSE
$XOffset                =   0
$YOffset                =   -5
$TextContrast           =   15
$Outline1Contrast           =   15
$Outline2Contrast           =   13
$BackgroundContrast     =   0
$ForceDisplay           =   FALSE
$FadeIn             =   0
$FadeOut                =   0
$HorzAlign          =   Center
";

            TmpegEncXmlFontName = "Tahoma";
            TmpegEncXmlFontHeight = "0.069";
            TmpegEncXmlPosition = "23";

            SamiDisplayTwoClassesAsTwoSubtitles = true;
            SamiHtmlEncodeMode = 0;

            TimedText10TimeCodeFormat = "Source";
            TimedText10ShowStyleAndLanguage = true;
            TimedText10FileExtension = ".xml";

            TimedTextItunesTopOrigin = "0% 0%";
            TimedTextItunesTopExtent = "100% 15%";
            TimedTextItunesBottomOrigin = "0% 85%";
            TimedTextItunesBottomExtent = "100% 15%";
            TimedTextItunesTimeCodeFormat = "Frames";
            TimedTextItunesStyleAttribute = "tts:fontStyle";
            TimedTextImsc11TimeCodeFormat = "hh:mm:ss.ms";
            TimedTextImsc11FileExtension = ".xml";

            FcpFontSize = 18;
            FcpFontName = "Lucida Grande";

            Cavena890StartOfMessage = "10:00:00:00";

            WebVttTimescale = 90000;
            WebVttUseXTimestampMap = true;
            WebVttUseMultipleXTimestampMap = true;
            WebVttCueAn1 = "position:20%";
            WebVttCueAn2 = "";
            WebVttCueAn3 = "position:80%";
            WebVttCueAn4 = "position:20% line:50%";
            WebVttCueAn5 = "line:50%";
            WebVttCueAn6 = "position:80% line:50%";
            WebVttCueAn7 = "position:20% line:20%";
            WebVttCueAn8 = "line:20%";
            WebVttCueAn9 = "position:80% line:20%";

            MPlayer2Extension = ".txt";

            TeletextItalicFix = true;
        }

        public void InitializeDCinemaSettings(bool smpte)
        {
            if (smpte)
            {
                CurrentDCinemaSubtitleId = "urn:uuid:" + Guid.NewGuid();
                CurrentDCinemaLanguage = "en";
                CurrentDCinemaFontUri = DCinemaLoadFontResource;
                CurrentDCinemaFontId = "theFontId";
            }
            else
            {
                string hex = Guid.NewGuid().ToString().RemoveChar('-').ToLowerInvariant();
                hex = hex.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
                CurrentDCinemaSubtitleId = hex;
                CurrentDCinemaLanguage = "English";
                CurrentDCinemaFontUri = DCinemaFontFile;
                CurrentDCinemaFontId = "Arial";
            }
            CurrentDCinemaIssueDate = DateTime.Now.ToString("s");
            CurrentDCinemaMovieTitle = "title";
            CurrentDCinemaReelNumber = "1";
            CurrentDCinemaFontColor = SKColors.White;
            CurrentDCinemaFontEffect = "border";
            CurrentDCinemaFontEffectColor = SKColors.Black;
            CurrentDCinemaFontSize = DCinemaFontSize;
            CurrentCavena890LanguageIdLine1 = -1;
            CurrentCavena890LanguageIdLine2 = -1;
        }
    }
}
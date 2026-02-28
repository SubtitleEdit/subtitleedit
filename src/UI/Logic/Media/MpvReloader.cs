using Avalonia.Skia;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public class MpvReloader : IMpvReloader
{
    public bool SmpteMode { get; set; }
    public int VideoWidth { get; set; } = 1280;
    public int VideoHeight { get; set; } = 720;

    private readonly AdvancedSubStationAlpha _assFormat = new();
    private Subtitle? _subtitlePrev;
    private string _mpvTextOld = string.Empty;
    private int _mpvSubOldHash = -1;
    private string? _mpvTextFileName;
    private string? _mpvTextFileExtension;
    private int _retryCount = 3;
    private string? _mpvPreviewStyleHeader;

    public async Task RefreshMpv(LibMpvDynamicPlayer mpvContext, Subtitle subtitle, SubtitleFormat uiFormat)
    {
        if (subtitle.Paragraphs.Count == 0)
        {
            return;
        }

        try
        {
            var uiFormatType = uiFormat.GetType();
            subtitle = new Subtitle(subtitle, false);

            if (SmpteMode)
            {
                foreach (var paragraph in subtitle.Paragraphs)
                {
                    paragraph.StartTime.TotalMilliseconds *= 1.001;
                    paragraph.EndTime.TotalMilliseconds *= 1.001;
                }
            }

            SubtitleFormat format = _assFormat;
            string text;
            if (uiFormatType == typeof(WebVTT) || uiFormatType == typeof(WebVTTFileWithLineNumber))
            {
                var defaultStyle = GetMpvPreviewStyle(Se.Settings.Video);
                defaultStyle.BorderStyle = "3";
                subtitle = new Subtitle(subtitle);
                subtitle = WebVttToAssa.Convert(subtitle, defaultStyle, VideoWidth, VideoHeight);
                text = subtitle.ToText(_assFormat);
            }
            else
            {
                if (subtitle.Header == null || !subtitle.Header.Contains("[V4+ Styles]") || uiFormatType != typeof(AdvancedSubStationAlpha))
                {
                    if (string.IsNullOrEmpty(subtitle.Header) && uiFormatType == typeof(SubStationAlpha))
                    {
                        subtitle.Header = SubStationAlpha.DefaultHeader;
                    }

                    if (subtitle.Header != null && subtitle.Header.Contains("[V4 Styles]", StringComparison.Ordinal))
                    {
                        subtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromSubStationAlpha(subtitle.Header);
                    }

                    var oldSub = subtitle;
                    subtitle = new Subtitle(subtitle);
                    if (Se.Settings.Appearance.RightToLeft)
                    {
                        for (var index = 0; index < subtitle.Paragraphs.Count; index++)
                        {
                            var paragraph = subtitle.Paragraphs[index];
                            if (LanguageAutoDetect.ContainsRightToLeftLetter(paragraph.Text))
                            {
                                paragraph.Text = Utilities.FixRtlViaUnicodeChars(paragraph.Text);
                            }
                        }
                    }

                    if (subtitle.Header == null || !(subtitle.Header.Contains("[V4+ Styles]") && uiFormatType == typeof(SubStationAlpha)))
                    {
                        subtitle.Header = MpvPreviewStyleHeader;
                    }

                    if (oldSub.Header != null && oldSub.Header.Length > 20 && oldSub.Header.AsSpan(3, 3).SequenceEqual("STL"))
                    {
                        var boldValue = Configuration.Settings.General.VideoPlayerPreviewFontBold ? "-1" : "0";
                        var boxStyle = $"Style: Box,{Configuration.Settings.General.VideoPlayerPreviewFontName},{Configuration.Settings.General.VideoPlayerPreviewFontSize},&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,{boldValue},0,0,0,100,100,0,0,3,2,0,2,10,10,10,1{Environment.NewLine}Style: Default,";
                        subtitle.Header = subtitle.Header.Replace("Style: Default,", boxStyle, StringComparison.Ordinal);

                        var useBox = false;
                        if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox)
                        {
                            try
                            {
                                var encoding = Ebu.GetEncoding(oldSub.Header[..3]);
                                var buffer = encoding.GetBytes(oldSub.Header);
                                var header = Ebu.ReadHeader(buffer);
                                if (header.DisplayStandardCode != "0")
                                {
                                    useBox = true;
                                }
                            }
                            catch
                            {
                                // ignore
                            }
                        }

                        for (var index = 0; index < subtitle.Paragraphs.Count; index++)
                        {
                            var p = subtitle.Paragraphs[index];

                            p.Extra = useBox ? "Box" : "Default";

                            if (p.Text.Contains("<box>", StringComparison.Ordinal))
                            {
                                p.Extra = "Box";
                                p.Text = p.Text.Replace("<box>", string.Empty).Replace("</box>", string.Empty);
                            }
                        }
                    }
                }

                var hash = subtitle.GetFastHashCode(null);
                if (hash != _mpvSubOldHash || string.IsNullOrEmpty(_mpvTextOld))
                {
                    text = subtitle.ToText(_assFormat);
                    _mpvSubOldHash = hash;
                }
                else
                {
                    text = _mpvTextOld;
                }
            }


            if (text != _mpvTextOld || _mpvTextFileName == null || _retryCount > 0)
            {
                if (_retryCount >= 0 || string.IsNullOrEmpty(_mpvTextFileName) || _subtitlePrev == null || _subtitlePrev.FileName != subtitle.FileName || _mpvTextFileExtension != format.Extension)
                {
                    DeleteTempMpvFileName();
                    _mpvTextFileName = FileUtil.GetTempFileName(format.Extension);
                    _mpvTextFileExtension = format.Extension;
                    await File.WriteAllTextAsync(_mpvTextFileName, text);
                    mpvContext.SubRemove();
                    mpvContext.SubAdd(_mpvTextFileName);
                    _retryCount--;
                }
                else
                {
                    await File.WriteAllTextAsync(_mpvTextFileName, text);
                    mpvContext.SubReload();
                }
                _mpvTextOld = text;
            }
            _subtitlePrev = subtitle;
        }
        catch (Exception exception)
        {
            Se.LogError(exception);
        }
    }

    private string MpvPreviewStyleHeader
    {
        get
        {
            if (string.IsNullOrEmpty(_mpvPreviewStyleHeader))
            {
                UpdateMpvStyle();
            }

            return _mpvPreviewStyleHeader ?? string.Empty;
        }
        set => _mpvPreviewStyleHeader = value;
    }

    public void UpdateMpvStyle()
    {
        var mpvStyle = GetMpvPreviewStyle(Se.Settings.Video);
        MpvPreviewStyleHeader = string.Format(AdvancedSubStationAlpha.HeaderNoStyles, "MPV preview file", mpvStyle.ToRawAss(SsaStyle.DefaultAssStyleFormat));
    }

    private static SsaStyle GetMpvPreviewStyle(SeVideo gs)
    {
        return new SsaStyle
        {
            Name = "Default",
            FontName = gs.MpvPreviewFontName,
            FontSize = gs.MpvPreviewFontSize,
            Bold = gs.MpvPreviewFontBold,
            Primary = gs.MpvPreviewColorPrimary.FromHexToColor().ToSKColor(),
            Outline = gs.MpvPreviewColorOutline.FromHexToColor().ToSKColor(),
            Background = gs.MpvPreviewColorShadow.FromHexToColor().ToSKColor(),
            OutlineWidth = gs.MpvPreviewOutlineWidth,
            ShadowWidth = gs.MpvPreviewShadowWidth,
            BorderStyle = gs.MpvPreviewBorderType.ToString(),
            Alignment = "2",  // bottom center
            MarginVertical = 10,
        };
    }

    private void DeleteTempMpvFileName()
    {
        try
        {
            if (File.Exists(_mpvTextFileName))
            {
                File.Delete(_mpvTextFileName);
                _mpvTextFileName = null;
            }
        }
        catch
        {
            // ignored
        }
    }

    public void Reset()
    {
        _mpvTextFileName = null;
        _mpvTextFileExtension = null;
        _mpvTextOld = string.Empty;
        _mpvPreviewStyleHeader = null;
        _retryCount = 3;
        _mpvSubOldHash = -1;
    }
}

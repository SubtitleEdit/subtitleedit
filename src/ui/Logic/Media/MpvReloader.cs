using Avalonia.Skia;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

public class MpvReloader : IMpvReloader
{
    public bool SmpteMode { get; set; }
    public bool SubtitlesVisible { get; set; } = true;
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

    // Bumped whenever newer state supersedes an in-flight background serialize (a newer
    // refresh, a reset, or a subtitle removal), so the stale result is discarded instead
    // of pushing old text over new. Only touched on the UI thread.
    private int _refreshVersion;

    public async Task RefreshMpv(LibMpvDynamicPlayer mpvContext, Subtitle subtitle, Subtitle? subtitleSecondary, SubtitleFormat uiFormat)
    {
        if (subtitle.Paragraphs.Count == 0 && subtitleSecondary == null)
        {
            _refreshVersion++; // an in-flight serialize would re-add the removed subtitle

            // All subtitles were deleted - remove any previously loaded subtitle from mpv,
            // otherwise the last shown lines stay visible on the video.
            if (!string.IsNullOrEmpty(_mpvTextFileName) || !string.IsNullOrEmpty(_mpvTextOld))
            {
                try
                {
                    mpvContext.SubRemove();
                }
                catch (Exception exception)
                {
                    Se.LogError(exception);
                }

                DeleteTempMpvFileName();
                _mpvTextFileName = null;
                _mpvTextOld = string.Empty;
                _mpvSubOldHash = -1;
                _subtitlePrev = null;
            }

            return;
        }

        try
        {
            var uiFormatType = uiFormat.GetType();

            // Deep copy on the calling (UI) thread: the caller usually passes the live
            // GetUpdateSubtitle() instance, which other UI code clears and repopulates at
            // will, so it must not be touched from the background serialize below.
            subtitle = new Subtitle(subtitle, false);

            // Prime the lazy style-header memo while still on the UI thread, so the
            // background thread only ever reads the field (UpdateMpvStyle writes it
            // from the settings dialog on the UI thread).
            _ = MpvPreviewStyleHeader;

            var version = ++_refreshVersion;
            var oldHash = _mpvSubOldHash;
            var oldText = _mpvTextOld;

            // Serialize off the UI thread: the SMPTE/RTL/header transforms + hash + ToText
            // are the expensive part of a preview refresh, and running them on the UI thread
            // is why the preview used to wait out a long typing pause (issue #13234).
            // "subtitle" is a thread-confined copy from here on; the secondary subtitle is
            // only read (its paragraphs are never mutated by the transforms).
            var built = await Task.Run(() => BuildPreviewText(subtitle, subtitleSecondary, uiFormatType, oldHash, oldText));

            if (version != _refreshVersion)
            {
                // Superseded while serializing (newer refresh, reset, or removal) -
                // applying this result would push stale text over newer state.
                return;
            }

            subtitle = built.Subtitle;
            var text = built.Text;
            if (built.HashValid)
            {
                _mpvSubOldHash = built.Hash;
            }

            var format = _assFormat;
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

            // Re-assert visibility so a hidden preview stays hidden on a freshly
            // created player (fullscreen/undock create a new mpv instance).
            mpvContext.SetSubtitleVisibility(SubtitlesVisible);
            _subtitlePrev = subtitle;
        }
        catch (Exception exception)
        {
            Se.LogError(exception);
        }
    }

    // Runs on a background thread (see RefreshMpv). Must only touch the thread-confined
    // subtitle copy, read-only state and libse statics - never the memo fields.
    private (Subtitle Subtitle, string Text, int Hash, bool HashValid) BuildPreviewText(Subtitle subtitle, Subtitle? subtitleSecondary, Type uiFormatType, int oldHash, string oldText)
    {
        if (SmpteMode)
        {
            foreach (var paragraph in subtitle.Paragraphs)
            {
                paragraph.StartTime.TotalMilliseconds *= 1.001;
                paragraph.EndTime.TotalMilliseconds *= 1.001;
            }
        }

        if (uiFormatType == typeof(WebVTT) || uiFormatType == typeof(WebVTTFileWithLineNumber))
        {
            var defaultStyle = GetMpvPreviewStyle(Se.Settings.Video);
            defaultStyle.BorderStyle = "3";
            // No extra copy here: "subtitle" is already RefreshMpv's private copy, and
            // Convert deep-copies its input again internally without mutating it.
            subtitle = WebVttToAssa.Convert(subtitle, defaultStyle, VideoWidth, VideoHeight);
            AddSecondarySubtitle(subtitle, subtitleSecondary);
            // The WebVTT path never used the hash memo, so keep it untouched (HashValid false).
            return (subtitle, subtitle.ToText(_assFormat), 0, false);
        }

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

            // "subtitle" is RefreshMpv's private copy, so mutating it in place is safe -
            // the second full deep copy that used to live here cost one Paragraph + two
            // TimeCode allocations per line on the UI thread for every preview refresh.
            // Only the pre-preview-style header is needed for the STL checks below.
            var oldHeader = subtitle.Header;
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

            if (oldHeader != null && oldHeader.Length > 20 && oldHeader.AsSpan(3, 3).SequenceEqual("STL"))
            {
                var previewFontName = Configuration.IsRunningOnLinux ? Configuration.DefaultLinuxFontName : "Tahoma";
                var boxStyle = $"Style: Box,{previewFontName},12,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,-1,0,0,0,100,100,0,0,3,2,0,2,10,10,10,1{Environment.NewLine}Style: Default,";
                subtitle.Header = subtitle.Header.Replace("Style: Default,", boxStyle, StringComparison.Ordinal);

                var useBox = false;
                if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox)
                {
                    try
                    {
                        var encoding = Ebu.GetEncoding(oldHeader[..3]);
                        var buffer = encoding.GetBytes(oldHeader);
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

        AddSecondarySubtitle(subtitle, subtitleSecondary);
        var hash = subtitle.GetFastHashCode(null);
        if (hash != oldHash || string.IsNullOrEmpty(oldText))
        {
            return (subtitle, subtitle.ToText(_assFormat), hash, true);
        }

        return (subtitle, oldText, hash, true);
    }

    private static void AddSecondarySubtitle(Subtitle subtitle, Subtitle? subtitleSecondary)
    {
        if (subtitleSecondary == null)
        {
            return;
        }


        var styleName = subtitleSecondary.Paragraphs.FirstOrDefault()?.Extra ?? "Secondary";
        var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, subtitleSecondary.Header);
        subtitle.Header = AdvancedSubStationAlpha.AddSsaStyle(style, subtitle.Header);
        foreach (var p in subtitleSecondary.Paragraphs)
        {
            subtitle.Paragraphs.Add(p);
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
            Alignment = gs.MpvPreviewAlignment,
            MarginVertical = gs.MpvPreviewMargin,
            MarginLeft =  gs.MpvPreviewMargin,
            MarginRight =  gs.MpvPreviewMargin,
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
        _refreshVersion++; // discard any in-flight background serialize
        DeleteTempMpvFileName();
        _mpvTextFileName = null;
        _mpvTextFileExtension = null;
        _mpvTextOld = string.Empty;
        _mpvPreviewStyleHeader = null;
        _retryCount = 3;
        _mpvSubOldHash = -1;
    }
}

using Nikse.SubtitleEdit.Core.Settings;

namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// JSON-serializable mirror of <see cref="BeautifyTimeCodesSettings"/> (libse). Lives in
/// SE5's <c>Settings.json</c> so profile edits survive restart. The libse engine still
/// reads <see cref="Configuration.Settings"/>, so we sync this onto libse at load time
/// and copy back from libse after the user clicks OK in the profile editor.
/// </summary>
public class SeBeautifyTimeCodes
{
    public bool AlignTimeCodes { get; set; }
    public bool ExtractExactTimeCodes { get; set; }
    public bool SnapToShotChanges { get; set; }
    public int OverlapThreshold { get; set; }

    // General
    public int Gap { get; set; }

    // In cues
    public int InCuesGap { get; set; }
    public int InCuesLeftGreenZone { get; set; }
    public int InCuesLeftRedZone { get; set; }
    public int InCuesRightRedZone { get; set; }
    public int InCuesRightGreenZone { get; set; }

    // Out cues
    public int OutCuesGap { get; set; }
    public int OutCuesLeftGreenZone { get; set; }
    public int OutCuesLeftRedZone { get; set; }
    public int OutCuesRightRedZone { get; set; }
    public int OutCuesRightGreenZone { get; set; }

    // Connected subtitles
    public int ConnectedSubtitlesInCueClosestLeftGap { get; set; }
    public int ConnectedSubtitlesInCueClosestRightGap { get; set; }
    public int ConnectedSubtitlesOutCueClosestLeftGap { get; set; }
    public int ConnectedSubtitlesOutCueClosestRightGap { get; set; }
    public int ConnectedSubtitlesLeftGreenZone { get; set; }
    public int ConnectedSubtitlesLeftRedZone { get; set; }
    public int ConnectedSubtitlesRightRedZone { get; set; }
    public int ConnectedSubtitlesRightGreenZone { get; set; }
    public int ConnectedSubtitlesTreatConnected { get; set; }

    // Chaining
    public bool ChainingGeneralUseZones { get; set; }
    public int ChainingGeneralMaxGap { get; set; }
    public int ChainingGeneralLeftGreenZone { get; set; }
    public int ChainingGeneralLeftRedZone { get; set; }
    public int ChainingGeneralShotChangeBehavior { get; set; }

    public bool ChainingInCueOnShotUseZones { get; set; }
    public int ChainingInCueOnShotMaxGap { get; set; }
    public int ChainingInCueOnShotLeftGreenZone { get; set; }
    public int ChainingInCueOnShotLeftRedZone { get; set; }
    public int ChainingInCueOnShotShotChangeBehavior { get; set; }
    public bool ChainingInCueOnShotCheckGeneral { get; set; }

    public bool ChainingOutCueOnShotUseZones { get; set; }
    public int ChainingOutCueOnShotMaxGap { get; set; }
    public int ChainingOutCueOnShotRightRedZone { get; set; }
    public int ChainingOutCueOnShotRightGreenZone { get; set; }
    public int ChainingOutCueOnShotShotChangeBehavior { get; set; }
    public bool ChainingOutCueOnShotCheckGeneral { get; set; }

    /// <summary>Are we still at the all-zeros default (fresh install, never edited)?</summary>
    public bool IsEmpty()
    {
        // Gap is always >=3 in any built-in preset; if it's 0 we treat the config as
        // never-saved and let the libse defaults stand.
        return Gap == 0 && OutCuesGap == 0 && InCuesLeftGreenZone == 0;
    }

    public void CopyFrom(BeautifyTimeCodesSettings source)
    {
        AlignTimeCodes = source.AlignTimeCodes;
        ExtractExactTimeCodes = source.ExtractExactTimeCodes;
        SnapToShotChanges = source.SnapToShotChanges;
        OverlapThreshold = source.OverlapThreshold;

        var p = source.Profile;
        Gap = p.Gap;
        InCuesGap = p.InCuesGap;
        InCuesLeftGreenZone = p.InCuesLeftGreenZone;
        InCuesLeftRedZone = p.InCuesLeftRedZone;
        InCuesRightRedZone = p.InCuesRightRedZone;
        InCuesRightGreenZone = p.InCuesRightGreenZone;
        OutCuesGap = p.OutCuesGap;
        OutCuesLeftGreenZone = p.OutCuesLeftGreenZone;
        OutCuesLeftRedZone = p.OutCuesLeftRedZone;
        OutCuesRightRedZone = p.OutCuesRightRedZone;
        OutCuesRightGreenZone = p.OutCuesRightGreenZone;
        ConnectedSubtitlesInCueClosestLeftGap = p.ConnectedSubtitlesInCueClosestLeftGap;
        ConnectedSubtitlesInCueClosestRightGap = p.ConnectedSubtitlesInCueClosestRightGap;
        ConnectedSubtitlesOutCueClosestLeftGap = p.ConnectedSubtitlesOutCueClosestLeftGap;
        ConnectedSubtitlesOutCueClosestRightGap = p.ConnectedSubtitlesOutCueClosestRightGap;
        ConnectedSubtitlesLeftGreenZone = p.ConnectedSubtitlesLeftGreenZone;
        ConnectedSubtitlesLeftRedZone = p.ConnectedSubtitlesLeftRedZone;
        ConnectedSubtitlesRightRedZone = p.ConnectedSubtitlesRightRedZone;
        ConnectedSubtitlesRightGreenZone = p.ConnectedSubtitlesRightGreenZone;
        ConnectedSubtitlesTreatConnected = p.ConnectedSubtitlesTreatConnected;
        ChainingGeneralUseZones = p.ChainingGeneralUseZones;
        ChainingGeneralMaxGap = p.ChainingGeneralMaxGap;
        ChainingGeneralLeftGreenZone = p.ChainingGeneralLeftGreenZone;
        ChainingGeneralLeftRedZone = p.ChainingGeneralLeftRedZone;
        ChainingGeneralShotChangeBehavior = (int)p.ChainingGeneralShotChangeBehavior;
        ChainingInCueOnShotUseZones = p.ChainingInCueOnShotUseZones;
        ChainingInCueOnShotMaxGap = p.ChainingInCueOnShotMaxGap;
        ChainingInCueOnShotLeftGreenZone = p.ChainingInCueOnShotLeftGreenZone;
        ChainingInCueOnShotLeftRedZone = p.ChainingInCueOnShotLeftRedZone;
        ChainingInCueOnShotShotChangeBehavior = (int)p.ChainingInCueOnShotShotChangeBehavior;
        ChainingInCueOnShotCheckGeneral = p.ChainingInCueOnShotCheckGeneral;
        ChainingOutCueOnShotUseZones = p.ChainingOutCueOnShotUseZones;
        ChainingOutCueOnShotMaxGap = p.ChainingOutCueOnShotMaxGap;
        ChainingOutCueOnShotRightRedZone = p.ChainingOutCueOnShotRightRedZone;
        ChainingOutCueOnShotRightGreenZone = p.ChainingOutCueOnShotRightGreenZone;
        ChainingOutCueOnShotShotChangeBehavior = (int)p.ChainingOutCueOnShotShotChangeBehavior;
        ChainingOutCueOnShotCheckGeneral = p.ChainingOutCueOnShotCheckGeneral;
    }

    public void ApplyTo(BeautifyTimeCodesSettings target)
    {
        target.AlignTimeCodes = AlignTimeCodes;
        target.ExtractExactTimeCodes = ExtractExactTimeCodes;
        target.SnapToShotChanges = SnapToShotChanges;
        target.OverlapThreshold = OverlapThreshold;

        var p = target.Profile;
        p.Gap = Gap;
        p.InCuesGap = InCuesGap;
        p.InCuesLeftGreenZone = InCuesLeftGreenZone;
        p.InCuesLeftRedZone = InCuesLeftRedZone;
        p.InCuesRightRedZone = InCuesRightRedZone;
        p.InCuesRightGreenZone = InCuesRightGreenZone;
        p.OutCuesGap = OutCuesGap;
        p.OutCuesLeftGreenZone = OutCuesLeftGreenZone;
        p.OutCuesLeftRedZone = OutCuesLeftRedZone;
        p.OutCuesRightRedZone = OutCuesRightRedZone;
        p.OutCuesRightGreenZone = OutCuesRightGreenZone;
        p.ConnectedSubtitlesInCueClosestLeftGap = ConnectedSubtitlesInCueClosestLeftGap;
        p.ConnectedSubtitlesInCueClosestRightGap = ConnectedSubtitlesInCueClosestRightGap;
        p.ConnectedSubtitlesOutCueClosestLeftGap = ConnectedSubtitlesOutCueClosestLeftGap;
        p.ConnectedSubtitlesOutCueClosestRightGap = ConnectedSubtitlesOutCueClosestRightGap;
        p.ConnectedSubtitlesLeftGreenZone = ConnectedSubtitlesLeftGreenZone;
        p.ConnectedSubtitlesLeftRedZone = ConnectedSubtitlesLeftRedZone;
        p.ConnectedSubtitlesRightRedZone = ConnectedSubtitlesRightRedZone;
        p.ConnectedSubtitlesRightGreenZone = ConnectedSubtitlesRightGreenZone;
        p.ConnectedSubtitlesTreatConnected = ConnectedSubtitlesTreatConnected;
        p.ChainingGeneralUseZones = ChainingGeneralUseZones;
        p.ChainingGeneralMaxGap = ChainingGeneralMaxGap;
        p.ChainingGeneralLeftGreenZone = ChainingGeneralLeftGreenZone;
        p.ChainingGeneralLeftRedZone = ChainingGeneralLeftRedZone;
        p.ChainingGeneralShotChangeBehavior = (BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingShotChangeBehaviorEnum)ChainingGeneralShotChangeBehavior;
        p.ChainingInCueOnShotUseZones = ChainingInCueOnShotUseZones;
        p.ChainingInCueOnShotMaxGap = ChainingInCueOnShotMaxGap;
        p.ChainingInCueOnShotLeftGreenZone = ChainingInCueOnShotLeftGreenZone;
        p.ChainingInCueOnShotLeftRedZone = ChainingInCueOnShotLeftRedZone;
        p.ChainingInCueOnShotShotChangeBehavior = (BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingShotChangeBehaviorEnum)ChainingInCueOnShotShotChangeBehavior;
        p.ChainingInCueOnShotCheckGeneral = ChainingInCueOnShotCheckGeneral;
        p.ChainingOutCueOnShotUseZones = ChainingOutCueOnShotUseZones;
        p.ChainingOutCueOnShotMaxGap = ChainingOutCueOnShotMaxGap;
        p.ChainingOutCueOnShotRightRedZone = ChainingOutCueOnShotRightRedZone;
        p.ChainingOutCueOnShotRightGreenZone = ChainingOutCueOnShotRightGreenZone;
        p.ChainingOutCueOnShotShotChangeBehavior = (BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingShotChangeBehaviorEnum)ChainingOutCueOnShotShotChangeBehavior;
        p.ChainingOutCueOnShotCheckGeneral = ChainingOutCueOnShotCheckGeneral;
    }
}

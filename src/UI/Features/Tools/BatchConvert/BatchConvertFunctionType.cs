namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public enum BatchConvertFunctionType
{
    AddFormatting,
    RemoveFormatting,
    OffsetTimeCodes,
    AdjustDisplayDuration,
    DeleteLines,
    ChangeFrameRate,
    ChangeSpeed,
    ChangeCasing,
    FixCommonErrors,
    MultipleReplace,
    AutoTranslate,
    RemoveTextForHearingImpaired,
    MergeLinesWithSameTimeCodes,
    MergeLinesWithSameText,
    FixRightToLeft,
    BridgeGaps,
    ApplyMinGap,
    SplitBreakLongLines,
}
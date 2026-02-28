using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertFunction
{
    public BatchConvertFunctionType Type { get; set; }
    public string Name { get; set; }
    public bool IsSelected { get; set; }
    public Control View { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public BatchConvertFunction(BatchConvertFunctionType type, string name, bool isSelected, Control view)
    {
        Type = type;
        Name = name;
        IsSelected = isSelected;
        View = view;
    }

    public static BatchConvertFunction[] List(BatchConvertViewModel vm)
    {
        var activeFunctions = Se.Settings.Tools.BatchConvert.ActiveFunctions;
        return new List<BatchConvertFunction>()
        {
            MakeFunction(BatchConvertFunctionType.DeleteLines, Se.Language.General.DeleteLines, ViewDeleteLines.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.RemoveTextForHearingImpaired, Se.Language.General.RemoveTextForHearingImpaired, ViewRemoveTextForHearingImpaired.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.RemoveFormatting, Se.Language.General.RemoveFormatting, ViewRemoveFormatting.Make(vm) , activeFunctions),
            MakeFunction(BatchConvertFunctionType.AddFormatting, Se.Language.Tools.BatchConvert.AddFormatting, ViewAddFormatting.Make(vm) , activeFunctions),
            MakeFunction(BatchConvertFunctionType.SplitBreakLongLines,  Se.Language.Tools.SplitBreakLongLines.Title, ViewSplitBreakLongLines.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.OffsetTimeCodes,  Se.Language.General.OffsetTimeCodes, ViewOffsetTimeCodes.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.AdjustDisplayDuration, Se.Language.General.AdjustDisplayDuration, ViewAdjustDuration.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.ChangeFrameRate, Se.Language.General.ChangeFrameRate, ViewChangeFrameRate.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.ChangeSpeed, Se.Language.General.ChangeSpeed, ViewChangeSpeed.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.ChangeCasing, Se.Language.General.ChangeCasing, ViewChangeCasing.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.BridgeGaps, Se.Language.General.BridgeGaps, ViewBridgeGaps.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.ApplyMinGap, Se.Language.Tools.ApplyMinGaps.Title, ViewApplyMinGap.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.FixCommonErrors, Se.Language.General.FixCommonErrors, ViewFixCommonErrors.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.MultipleReplace, Se.Language.General.MultipleReplace, ViewMultipleReplace.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.MergeLinesWithSameText, Se.Language.General.MergeLinesWithSameText, ViewMergeLinesWithSameText.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.MergeLinesWithSameTimeCodes, Se.Language.General.MergeLinesWithSameTimeCodes, ViewMergeLinesWithSameTimeCodes.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.FixRightToLeft, Se.Language.General.FixRightToLeft, ViewFixRightToLeft.Make(vm), activeFunctions),
            MakeFunction(BatchConvertFunctionType.AutoTranslate, Se.Language.General.AutoTranslate, ViewAutoTranslate.Make(vm), activeFunctions),
        }.ToArray();
    }

    private static BatchConvertFunction MakeFunction(BatchConvertFunctionType functionType, string name, Control view, string[] activeFunctions)
    {
        var isActive = activeFunctions.Contains(functionType.ToString());
        return new BatchConvertFunction(functionType, name, isActive, view);
    }
}
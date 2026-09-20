using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class ToolbarSettingItem : ObservableObject
{
    public string Name { get; }
    public string? ImageName { get; }

    private bool _isVisible;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private readonly Func<SeAppearance, bool> _read;
    private readonly Action<SeAppearance, bool> _write;

    private ToolbarSettingItem(string name, string? imageName, Func<SeAppearance, bool> read,
        Action<SeAppearance, bool> write, string? formats = null)
    {
        Name = name.Replace("_", string.Empty).Trim().TrimEnd('.', '…').Trim();
        if (formats != null) Name += " (" + formats + ")";

        ImageName = imageName;
        _read = read;
        _write = write;
    }

    public void Load(SeAppearance appearance) => IsVisible = _read(appearance);

    public void Save(SeAppearance appearance) => _write(appearance, IsVisible);

    public static IReadOnlyList<ToolbarSettingItem> CreateItems()
    {
        var menu = Se.Language.Main.Menu;
        return
        [
            new(menu.New, "New", a => a.ToolbarShowFileNew, (a, value) => a.ToolbarShowFileNew = value),
            new(menu.Open, "Open", a => a.ToolbarShowFileOpen, (a, value) => a.ToolbarShowFileOpen = value),
            new(menu.OpenVideo, "OpenVideo", a => a.ToolbarShowVideoFileOpen, (a, value) => a.ToolbarShowVideoFileOpen = value),
            new(menu.Save, "Save", a => a.ToolbarShowSave, (a, value) => a.ToolbarShowSave = value),
            new(menu.SaveAs, "SaveAs", a => a.ToolbarShowSaveAs, (a, value) => a.ToolbarShowSaveAs = value),
            new(menu.Find, "Find", a => a.ToolbarShowFind, (a, value) => a.ToolbarShowFind = value),
            new(menu.Replace, "Replace", a => a.ToolbarShowReplace, (a, value) => a.ToolbarShowReplace = value),
            new(menu.MultipleReplace, "MultipleReplace", a => a.ToolbarShowMultipleReplace, (a, value) => a.ToolbarShowMultipleReplace = value),
            new(menu.SpellCheck, "SpellCheck", a => a.ToolbarShowSpellCheck, (a, value) => a.ToolbarShowSpellCheck = value),
            new(menu.FixCommonErrors, "FixCommonErrors", a => a.ToolbarShowFixCommonErrors, (a, value) => a.ToolbarShowFixCommonErrors = value),
            new(menu.RemoveTextForHearingImpaired, "RemoveTextForHi", a => a.ToolbarShowRemoveTextForHi, (a, value) => a.ToolbarShowRemoveTextForHi = value),
            new(menu.VisualSync, "VisualSync", a => a.ToolbarShowVisualSync, (a, value) => a.ToolbarShowVisualSync = value),
            new(menu.PointSyncViaOther, "PointSync", a => a.ToolbarShowPointSync, (a, value) => a.ToolbarShowPointSync = value),
            new(menu.BeautifyTimeCodes, "BeautifyTimeCodes", a => a.ToolbarShowBeautifyTimeCodes, (a, value) => a.ToolbarShowBeautifyTimeCodes = value),
            new(menu.GenerateBurnIn, "BurnIn", a => a.ToolbarShowBurnIn, (a, value) => a.ToolbarShowBurnIn = value),
            new(menu.AutoTranslate, "AutoTranslate", a => a.ToolbarShowAutoTranslate, (a, value) => a.ToolbarShowAutoTranslate = value),
            new(menu.SpeechToText, "SpeechToText", a => a.ToolbarShowSpeechToText, (a, value) => a.ToolbarShowSpeechToText = value),
            new(menu.Settings, "Settings", a => a.ToolbarShowSettings, (a, value) => a.ToolbarShowSettings = value),
            new(Se.Language.Options.Shortcuts.GeneralChooseLayout, "Layout", a => a.ToolbarShowLayout, (a, value) => a.ToolbarShowLayout = value),
            new(Se.Language.Options.Shortcuts.SourceView, "SourceView", a => a.ToolbarShowSourceView, (a, value) => a.ToolbarShowSourceView = value),
            new(menu.Help, "Help", a => a.ToolbarShowHelp, (a, value) => a.ToolbarShowHelp = value),
            new(Se.Language.General.Encoding, null, a => a.ToolbarShowEncoding, (a, value) => a.ToolbarShowEncoding = value),
            new(Se.Language.General.FrameRate, null, a => a.ToolbarShowFrameRate, (a, value) => a.ToolbarShowFrameRate = value),
            new(Se.Language.General.Styles, "AssaStyle", a => a.ToolbarShowStyleManager, (a, value) => a.ToolbarShowStyleManager = value, formats: "ASSA/SSA/WebVTT"),
            new(menu.AssaProperties, "AssaProperties", a => a.ToolbarShowProperties, (a, value) => a.ToolbarShowProperties = value, formats: "ASSA/SSA"),
            new(menu.AssaAttachments, "AssaAttachments", a => a.ToolbarShowAttachments, (a, value) => a.ToolbarShowAttachments = value, formats: "ASSA/SSA"),
            new(menu.AssaDraw, "AssaDraw", a => a.ToolbarShowAssaDraw, (a, value) => a.ToolbarShowAssaDraw = value, formats: "ASSA"),
        ];
    }
}

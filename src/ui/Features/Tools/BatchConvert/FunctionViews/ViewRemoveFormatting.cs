using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRemoveFormatting
{
    public static Control Make(BatchConvertViewModel vm)
    {
        return FunctionViewChrome.MakePanel(vm, Se.Language.General.RemoveFormatting,
            EditorChrome.MakeSwitchRow(nameof(vm.FormattingRemoveAll), Se.Language.General.RemoveAllFormatting),
            EditorChrome.MakeSwitchRow(nameof(vm.FormattingRemoveItalic), Se.Language.General.RemoveItalic),
            EditorChrome.MakeSwitchRow(nameof(vm.FormattingRemoveBold), Se.Language.General.RemoveBold),
            EditorChrome.MakeSwitchRow(nameof(vm.FormattingRemoveUnderline), Se.Language.General.RemoveUnderline),
            EditorChrome.MakeSwitchRow(nameof(vm.FormattingRemoveFontTags), Se.Language.General.RemoveFontName),
            EditorChrome.MakeSwitchRow(nameof(vm.FormattingRemoveAlignmentTags), Se.Language.General.RemoveAlignment),
            EditorChrome.MakeSwitchRow(nameof(vm.FormattingRemoveColors), Se.Language.General.RemoveColor));
    }
}

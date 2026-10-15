using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRemoveFormatting
{
    public static Control Make(BatchConvertViewModel vm)
    {
        return FunctionViewChrome.MakePanel(vm, Se.Language.General.RemoveFormatting,
            FunctionViewChrome.MakeSwitchRow(nameof(vm.FormattingRemoveAll), Se.Language.General.RemoveAllFormatting),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.FormattingRemoveItalic), Se.Language.General.RemoveItalic),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.FormattingRemoveBold), Se.Language.General.RemoveBold),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.FormattingRemoveUnderline), Se.Language.General.RemoveUnderline),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.FormattingRemoveFontTags), Se.Language.General.RemoveFontName),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.FormattingRemoveAlignmentTags), Se.Language.General.RemoveAlignment),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.FormattingRemoveColors), Se.Language.General.RemoveColor));
    }
}

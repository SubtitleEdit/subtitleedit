using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAssaEmbedFonts
{
    public static Control Make(BatchConvertViewModel vm)
    {
        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.BatchConvert.AssaEmbedFontsTitle,
            EditorChrome.MakeHint(Se.Language.Tools.BatchConvert.AssaEmbedFontsInfo),
            EditorChrome.MakeSwitchRow(nameof(vm.AssaEmbedFontsTrim), Se.Language.Assa.FontCollectorTrimFonts));
    }
}

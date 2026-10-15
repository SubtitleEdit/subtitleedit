using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAssaEmbedFonts
{
    public static Control Make(BatchConvertViewModel vm)
    {
        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.BatchConvert.AssaEmbedFontsTitle,
            FunctionViewChrome.MakeHint(Se.Language.Tools.BatchConvert.AssaEmbedFontsInfo),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.AssaEmbedFontsTrim), Se.Language.Assa.FontCollectorTrimFonts));
    }
}

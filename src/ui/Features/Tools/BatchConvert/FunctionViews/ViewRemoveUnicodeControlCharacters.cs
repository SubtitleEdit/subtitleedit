using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRemoveUnicodeControlCharacters
{
    public static Control Make(BatchConvertViewModel vm)
    {
        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.BatchConvert.RemoveUnicodeControlCharactersTitle,
            FunctionViewChrome.MakeHint(Se.Language.Tools.BatchConvert.RemoveUnicodeControlCharactersInfo));
    }
}

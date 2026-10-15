using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRenumber
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var numericStartNumber = FunctionViewChrome.MakeIntField(vm, nameof(vm.RenumberStartNumber), 0, 99999, vm.RenumberStartNumber, Se.Language.Tools.Renumber.StartFromNumber);

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.Renumber.Title,
            FunctionViewChrome.MakeRow(Se.Language.Tools.Renumber.StartFromNumber, numericStartNumber));
    }
}

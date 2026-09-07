using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel
{
    [RelayCommand]
    private async Task ShowToolsCheckArteErrors()
    {
        if (Window == null)
        {
            return;
        }

        if (IsEmpty)
        {
            ShowSubtitleNotLoadedMessage();
            return;
        }

        var selectedIndex = SelectedSubtitleIndex ?? 0;

        // Analysis is detached; each correction/undo publishes a complete snapshot while
        // the modal review window stays open. Closing must not apply the same changes twice.
        var subtitle = GetUpdateSubtitleWithRowMap(out _);
        var viewModel = new CheckArteErrorsViewModel();
        viewModel.Initialize(subtitle);
        viewModel.ApplyToMainSubtitle = corrected =>
        {
            GetUpdateSubtitleWithRowMap(out var currentRows);
            _subtitle.Header = corrected.Header;
            var targetFormat = Ebu.IsStlHeader(corrected.Header)
                ? SubtitleFormats.FirstOrDefault(format => format is Ebu) ?? new Ebu()
                : SelectedSubtitleFormat;
            if (targetFormat is Ebu)
            {
                SetSubtitleFormat(targetFormat);
                _subtitle.OriginalFormat = targetFormat;
            }
            ApplyFixedSubtitle(corrected, currentRows, selectedIndex, targetFormat);
        };
        viewModel.OpenEbuOptionsDialog = async () =>
        {
            if (!await ShowEbuOptionsDialog())
            {
                return null;
            }

            return new Subtitle(GetUpdateSubtitleWithRowMap(out _), generateNewId: false);
        };

        var window = new CheckArteErrorsWindow(viewModel);
        await window.ShowDialog(Window);
        viewModel.ApplyToMainSubtitle = null;
        viewModel.OpenEbuOptionsDialog = null;

        if (viewModel.OkPressed)
        {
            ShowStatus($"{viewModel.AppliedFixCount} ARTE fix(es) applied.");
        }
    }
}

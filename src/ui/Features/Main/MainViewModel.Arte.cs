using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;
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

        // Work on a detached Subtitle snapshot. ARTE analysis must never mutate the
        // live subtitle grid or trigger Flow/timing dialogs. Only OK applies the
        // explicitly selected fixes back to the real subtitle.
        var subtitle = GetUpdateSubtitleWithRowMap(out var rowByParagraphId);

        var viewModel = new CheckArteErrorsViewModel();
        viewModel.Initialize(subtitle);

        var window = new CheckArteErrorsWindow(viewModel);
        await window.ShowDialog(Window);

        if (!viewModel.OkPressed || viewModel.FixedSubtitle == null)
        {
            return;
        }

        ApplyFixedSubtitle(
            viewModel.FixedSubtitle,
            rowByParagraphId,
            selectedIndex,
            SelectedSubtitleFormat);

        ShowStatus($"{viewModel.AppliedFixCount} ARTE fix(es) applied.");
    }
}

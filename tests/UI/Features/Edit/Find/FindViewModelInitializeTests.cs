using System.Collections.Generic;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Edit.Find;

/// <summary>
/// Reopening the find window restores the previous search pattern verbatim:
/// edge spaces are significant in a search pattern and must not be trimmed
/// away by InitializeFindData (issue #13489).
/// </summary>
public class FindViewModelInitializeTests
{
    private sealed class StubFindResult : IFindResult
    {
        public void RequestFindData()
        {
        }

        public Task HandleFindResult(FindViewModel result)
        {
            return Task.CompletedTask;
        }

        public Task HandleReplaceResult(ReplaceViewModel result)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void InitializeFindData_KeepsEdgeSpacesInSearchText()
    {
        var vm = new FindViewModel();

        vm.InitializeFindData(new FindService(), new List<string> { "some text here" }, " some text ", new StubFindResult());

        Assert.Equal(" some text ", vm.SearchText);
    }
}

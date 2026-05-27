using Nikse.SubtitleEdit.Features.Edit.Replace;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Edit.Find;

public interface IFindResult
{
    public void RequestFindData();
    public Task HandleFindResult(FindViewModel result);
    public Task HandleReplaceResult(ReplaceViewModel result);
}
